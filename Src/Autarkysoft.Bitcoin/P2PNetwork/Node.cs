// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Net.Sockets;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Node handles communications between each bitcoin node on peer to peer network.
    /// </summary>
    public class Node : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Node"/> for <see cref="NetworkType.MainNet"/> using the default parameters.
        /// </summary>
        /// <param name="blockchain">The blockchain (database) manager</param>
        internal Node(IBlockchain blockchain) :
            this(Constants.P2PProtocolVersion, NodeServiceFlags.All, blockchain, true, NetworkType.MainNet)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Node"/> using the given parameters.
        /// </summary>
        /// <param name="protocolVersion">P2P protocol version</param>
        /// <param name="servs">Services that this node wants to support</param>
        /// <param name="blockchain">The blockchain (database) manager</param>
        /// <param name="relay">True if the node wants to relay new transactions and blocks to other peers</param>
        /// <param name="netType">Network type</param>
        internal Node(int protocolVersion, NodeServiceFlags servs, IBlockchain blockchain, bool relay, NetworkType netType)
        {
            NodeStatus = new NodeStatus();
            repMan = new ReplyManager(NodeStatus, netType);

            // TODO: the following values are for testing, they should be set by the caller
            // they need more checks for correct and optimal values
            buffLen = 200;
            int bytesPerSaea = buffLen;
            int sendReceiveSaeaCount = 10;
            int totalBytes = bytesPerSaea * sendReceiveSaeaCount;

            var verPl = new VersionPayload(protocolVersion, servs, blockchain.Height, relay);
            Message verMsg = new Message(verPl, netType);

            buffMan = new BufferManager(totalBytes, bytesPerSaea);

            sendReceivePool = new SocketAsyncEventArgsPool(sendReceiveSaeaCount);
            for (int i = 0; i < sendReceiveSaeaCount; i++)
            {
                SocketAsyncEventArgs sArg = new SocketAsyncEventArgs();

                buffMan.SetBuffer(sArg);
                sArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                sArg.UserToken = new MessageManager(bytesPerSaea, verMsg, repMan, NodeStatus, netType);

                sendReceivePool.Push(sArg);
            }
        }



        private readonly int buffLen;
        internal SocketAsyncEventArgsPool sendReceivePool;
        private readonly BufferManager buffMan;
        private readonly IReplyManager repMan;

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


        internal void StartReceive(SocketAsyncEventArgs recEventArgs)
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
                MessageManager msgMan = recEventArgs.UserToken as MessageManager;
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
                MessageManager msgMan = sendEventArgs.UserToken as MessageManager;
                if (msgMan.HasDataToSend)
                {
                    msgMan.SetSendBuffer(sendEventArgs);
                    StartSend(sendEventArgs);
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

            MessageManager msgMan = srEventArgs.UserToken as MessageManager;
            msgMan.Init();

            sendReceivePool.Push(srEventArgs);
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
                    if (!(sendReceivePool is null))
                        sendReceivePool.Dispose();
                    sendReceivePool = null;
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
