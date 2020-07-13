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
    /// A node handling the incoming transmissions from other peers on the network. Implements <see cref="IDisposable"/>.
    /// </summary>
    public class NodeListener : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeListener"/> using the given parameters.
        /// </summary>
        /// <param name="peerList">List of peers (is used to add the connected node to)</param>
        /// <param name="bc">Blockchain database to use</param>
        /// <param name="cs">Client settings</param>
        public NodeListener(ICollection<Node> peerList, IBlockchain bc, IClientSettings cs)
        {
            peers = peerList;
            blockchain = bc;
            settings = cs;

            int MaxConnections = 3;
            backlog = 3;
            int MaxAcceptOps = 10;

            maxConnectionEnforcer = new Semaphore(MaxConnections, MaxConnections);

            acceptPool = new SocketAsyncEventArgsPool(MaxAcceptOps);
            for (int i = 0; i < MaxAcceptOps; i++)
            {
                var acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>((sender, e) => ProcessAccept(e));
                acceptPool.Push(acceptEventArg);
            }
        }

        private Socket listenSocket;
        private Semaphore maxConnectionEnforcer;
        private readonly int backlog;
        private SocketAsyncEventArgsPool acceptPool;
        private ICollection<Node> peers;
        private readonly IBlockchain blockchain;
        private readonly IClientSettings settings;

        /// <summary>
        /// Starts listening for new connections on the given <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="ep"><see cref="EndPoint"/> to use</param>
        public void StartListen(EndPoint ep)
        {
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(ep);
            listenSocket.Listen(backlog);
            StartAccept();
        }

        private void StartAccept()
        {
            SocketAsyncEventArgs acceptEventArg = acceptPool.Pop();
            maxConnectionEnforcer.WaitOne();
            if (!listenSocket.AcceptAsync(acceptEventArg))
            {
                ProcessAccept(acceptEventArg);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError == SocketError.Success)
            {
                Node node = new Node(blockchain, settings);
                peers.Add(node);
                SocketAsyncEventArgs srEventArgs = node.sendReceivePool.Pop();

                // Pass the socket from the "accept" SAEA to "send/receive" SAEA.
                srEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
                node.StartReceive(srEventArgs);

                // Remove "accept" SAEA socket and put it back in pool to be used for the next accept operation.
                acceptEventArgs.AcceptSocket = null;
                acceptPool.Push(acceptEventArgs);
                StartAccept();
            }
            else
            {
                acceptEventArgs.AcceptSocket = null;
                acceptPool.Push(acceptEventArgs);
                maxConnectionEnforcer.Release();
                StartAccept();
            }
        }


        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by this class.
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
                    if (!(listenSocket is null))
                        listenSocket.Dispose();
                    listenSocket = null;

                    if (!(maxConnectionEnforcer is null))
                        maxConnectionEnforcer.Dispose();
                    maxConnectionEnforcer = null;

                    if (!(acceptPool is null))
                        acceptPool.Dispose();
                    acceptPool = null;

                    peers = null;
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
