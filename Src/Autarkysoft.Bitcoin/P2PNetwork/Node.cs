// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Net.Sockets;
using System.Threading;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Node handles communications between each bitcoin node on peer to peer network.
    /// </summary>
    public class Node : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Node"/> using the given parameters.
        /// </summary>
        /// <param name="bc">The blockchain (database) manager</param>
        /// <param name="cs">Client settings</param>
        /// <param name="socket">Socket to use</param>
        internal Node(IBlockchain bc, IClientSettings cs, Socket socket)
        {
            settings = cs;
            NodeStatus = new NodeStatus();
            var repMan = new ReplyManager(NodeStatus, bc, cs);
            msgMan = new MessageManager(cs.BufferLength, repMan, NodeStatus, cs.Network);

            sendReceiveSAEA = cs.SendReceivePool.Pop();
            sendReceiveSAEA.AcceptSocket = socket;
            sendReceiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            sendSAEA = cs.SendReceivePool.Pop();
            sendSAEA.AcceptSocket = socket;
            sendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }


        private Semaphore secondSendLimiter;
        private SocketAsyncEventArgs sendReceiveSAEA;
        private SocketAsyncEventArgs sendSAEA;
        private readonly MessageManager msgMan;
        private readonly IClientSettings settings;


        /// <summary>
        /// Contains the node's current state
        /// </summary>
        public INodeStatus NodeStatus { get; set; }


        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }


        internal void StartHandShake()
        {
            msgMan.StartHandShake(sendReceiveSAEA);
            StartSend(sendReceiveSAEA);
        }

        internal void StartReceiving() => StartReceive(sendReceiveSAEA);

        private void StartReceive(SocketAsyncEventArgs recEventArgs)
        {
            if (!recEventArgs.AcceptSocket.ReceiveAsync(recEventArgs))
            {
                ProcessReceive(recEventArgs);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs recEventArgs)
        {
            // Zero bytes transferred means remote end has closed the connection. It needs to be closed here too.
            if (recEventArgs.SocketError == SocketError.Success && recEventArgs.BytesTransferred > 0)
            {
                msgMan.ReadBytes(recEventArgs);

                if (msgMan.HasDataToSend)
                {
                    msgMan.SetSendBuffer(recEventArgs);
                    StartSend(recEventArgs);
                }
                else
                {
                    StartReceive(recEventArgs);
                }
            }
            else
            {
                CloseClientSocket(recEventArgs);
            }
        }

        /// <summary>
        /// Sends the given message to this connected node.
        /// </summary>
        /// <param name="msg">Message to send</param>
        public void Send(Message msg)
        {
            secondSendLimiter.WaitOne();
            msgMan.SetSendBuffer(sendSAEA, msg);
            StartSend(sendSAEA);
        }

        internal void StartSend(SocketAsyncEventArgs sendEventArgs)
        {
            if (!sendEventArgs.AcceptSocket.SendAsync(sendEventArgs))
            {
                ProcessSend(sendEventArgs);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                if (msgMan.HasDataToSend)
                {
                    msgMan.SetSendBuffer(sendEventArgs);
                    StartSend(sendEventArgs);
                }
                else if (ReferenceEquals(sendEventArgs, sendSAEA))
                {
                    secondSendLimiter.Release();
                }
                else
                {
                    StartReceive(sendEventArgs);
                }
            }
            else
            {
                CloseClientSocket(sendEventArgs);
            }
        }



        private void CloseClientSocket(SocketAsyncEventArgs srEventArgs)
        {
            try
            {
                srEventArgs.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }

            srEventArgs.AcceptSocket.Close();
            settings.MaxConnectionEnforcer.Release();
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
                    CloseClientSocket(sendReceiveSAEA);
                    sendReceiveSAEA.AcceptSocket = null;
                    sendReceiveSAEA.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    sendSAEA.AcceptSocket = null;
                    sendSAEA.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    settings.SendReceivePool.Push(sendReceiveSAEA);
                    settings.SendReceivePool.Push(sendSAEA);
                    sendReceiveSAEA = null;
                    sendSAEA = null;

                    if (!(secondSendLimiter is null))
                        secondSendLimiter.Dispose();
                    secondSendLimiter = null;
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
