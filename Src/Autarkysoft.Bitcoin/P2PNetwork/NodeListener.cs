// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Clients;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
        /// <param name="cs">Client settings</param>
        public NodeListener(IClientSettings cs)
        {
            peers = cs.AllNodes;
            settings = cs;

            backlog = 3;

            acceptPool = new SocketAsyncEventArgsPool(cs.MaxConnectionCount);
            for (int i = 0; i < cs.MaxConnectionCount; i++)
            {
                var acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>((sender, e) => ProcessAccept(e));
                acceptPool.Push(acceptEventArg);
            }
        }

        private Socket listenSocket;
        private readonly int backlog;
        private SocketAsyncEventArgsPool acceptPool;
        private ICollection<Node> peers;
        private readonly IClientSettings settings;

        /// <summary>
        /// Starts listening for new connections on the given <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="ep"><see cref="EndPoint"/> to use</param>
        public void StartListen(EndPoint ep)
        {
            listenSocket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(ep);
            listenSocket.Listen(backlog);
            StartAccept();
        }

        private void StartAccept()
        {
            settings.MaxConnectionEnforcer.WaitOne();
            SocketAsyncEventArgs acceptEventArg = acceptPool.Pop();
            if (!listenSocket.AcceptAsync(acceptEventArg))
            {
                ProcessAccept(acceptEventArg);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError == SocketError.Success)
            {
                Node node = new Node(settings, acceptEventArgs.AcceptSocket);
                peers.Add(node);
                node.StartReceiving();
            }
            else
            {
                settings.MaxConnectionEnforcer.Release();
            }

            // Remove "accept" SAEA socket and put it back in pool to be used for the next accept operation.
            acceptEventArgs.AcceptSocket = null;
            acceptPool.Push(acceptEventArgs);
            StartAccept();
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
        public void Dispose() => Dispose(true);
    }
}
