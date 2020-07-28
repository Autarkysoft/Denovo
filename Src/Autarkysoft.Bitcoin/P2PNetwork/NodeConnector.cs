// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Handles initial connection to other peers on the network. Implements <see cref="IDisposable"/>.
    /// </summary>
    public class NodeConnector : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeConnector"/> using the given parameters.
        /// </summary>
        /// <param name="peerList">List of peers (is used to add the connected node to)</param>
        /// <param name="bc">Blockchain database to use</param>
        /// <param name="cs">Client settings</param>
        public NodeConnector(ICollection<Node> peerList, IBlockchain bc, IClientSettings cs)
        {
            peers = peerList;
            blockchain = bc;
            settings = cs;

            int MaxConnections = 3;
            maxConnectionEnforcer = new Semaphore(MaxConnections, MaxConnections);

            int MaxConnectOps = 10;
            connectPool = new SocketAsyncEventArgsPool(MaxConnectOps);
            for (int i = 0; i < MaxConnectOps; i++)
            {
                var connectEventArg = new SocketAsyncEventArgs();
                connectEventArg.Completed += new EventHandler<SocketAsyncEventArgs>((sender, e) => ProcessConnect(e));
                connectPool.Push(connectEventArg);
            }
        }


        private Semaphore maxConnectionEnforcer;
        private SocketAsyncEventArgsPool connectPool;
        private ICollection<Node> peers;
        private readonly IBlockchain blockchain;
        private readonly IClientSettings settings;


        /// <summary>
        /// Connects to a node by using the given <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="ep"><see cref="EndPoint"/> to use</param>
        public void StartConnect(EndPoint ep)
        {
            maxConnectionEnforcer.WaitOne();
            SocketAsyncEventArgs connectEventArgs = connectPool.Pop();
            connectEventArgs.RemoteEndPoint = ep;
            connectEventArgs.AcceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            StartConnect(connectEventArgs);
        }

        private void StartConnect(SocketAsyncEventArgs connectEventArgs)
        {
            if (!connectEventArgs.AcceptSocket.ConnectAsync(connectEventArgs))
            {
                ProcessConnect(connectEventArgs);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs connectEventArgs)
        {
            if (connectEventArgs.SocketError == SocketError.Success)
            {
                Node node = new Node(blockchain, settings);
                peers.Add(node);
                SocketAsyncEventArgs srEventArgs = node.sendReceivePool.Pop();

                // Pass the socket from the "connect" SAEA to "send/receive" SAEA.
                srEventArgs.AcceptSocket = connectEventArgs.AcceptSocket;

                MessageManager msgMan = srEventArgs.UserToken as MessageManager;
                msgMan.StartHandShake(srEventArgs);

                node.StartSend(srEventArgs);

                // Remove "connect" SAEA socket beforing putting it back in pool to be used for the next connect operation.
                connectEventArgs.AcceptSocket = null;
            }
            else
            {
                connectEventArgs.AcceptSocket.Close();
            }

            connectPool.Push(connectEventArgs);
            maxConnectionEnforcer.Release();
        }


        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by this instance.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (!(maxConnectionEnforcer is null))
                        maxConnectionEnforcer.Dispose();
                    maxConnectionEnforcer = null;

                    if (!(connectPool is null))
                        connectPool.Dispose();
                    connectPool = null;

                    peers = null;
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose() => Dispose(true);
    }
}
