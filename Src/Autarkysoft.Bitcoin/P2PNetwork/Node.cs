// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Node handles communications between each bitcoin node on peer to peer network.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Node"/> for <see cref="NetworkType.MainNet"/> using the default parameters.
        /// </summary>
        /// <param name="blockchain">The blockchain (database) manager</param>
        public Node(IBlockchain blockchain) :
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
        public Node(int protocolVersion, NodeServiceFlags servs, IBlockchain blockchain, bool relay, NetworkType netType)
        {
            // TODO: the following values are for testing, they should be set by the caller
            // they need more checks for correct and optimal values
            int MaxConnections = 3;
            backlog = 3;
            int MaxAcceptOps = 10;
            int MaxConnectOps = 10;
            buffLen = 200;
            int bytesPerSaea = buffLen;
            int sendReceiveSaeaCount = 10;
            int totalBytes = bytesPerSaea * sendReceiveSaeaCount;

            Message verMsg = new Message(netType)
            {
                Payload = new VersionPayload(protocolVersion, servs, blockchain.Height, relay)
            };


            maxConnectionEnforcer = new Semaphore(MaxConnections, MaxConnections);

            acceptPool = new SocketAsyncEventArgsPool(MaxAcceptOps);
            for (int i = 0; i < MaxAcceptOps; i++)
            {
                acceptPool.Push(CreateNewAcceptSaea());
            }

            connectPool = new SocketAsyncEventArgsPool(MaxConnectOps);
            for (int i = 0; i < MaxConnectOps; i++)
            {
                connectPool.Push(CreateNewConnectSaea());
            }

            buffMan = new BufferManager(totalBytes, bytesPerSaea);

            sendReceivePool = new SocketAsyncEventArgsPool(sendReceiveSaeaCount);
            for (int i = 0; i < sendReceiveSaeaCount; i++)
            {
                SocketAsyncEventArgs sArg = new SocketAsyncEventArgs();

                buffMan.SetBuffer(sArg);
                sArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                sArg.UserToken = new MessageManager(bytesPerSaea, verMsg, netType);

                sendReceivePool.Push(sArg);
            }
        }



        private readonly int buffLen;
        private Socket listenSocket;
        private readonly Semaphore maxConnectionEnforcer;
        private readonly int backlog;
        private readonly SocketAsyncEventArgsPool acceptPool;
        private readonly SocketAsyncEventArgsPool connectPool;
        private readonly SocketAsyncEventArgsPool sendReceivePool;
        private readonly BufferManager buffMan;



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

        private SocketAsyncEventArgs CreateNewAcceptSaea()
        {
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>((sender, e) => ProcessAccept(e));

            return acceptEventArg;
        }

        private SocketAsyncEventArgs CreateNewConnectSaea()
        {
            SocketAsyncEventArgs connectEventArg = new SocketAsyncEventArgs();
            connectEventArg.Completed += new EventHandler<SocketAsyncEventArgs>((sender, e) => ProcessConnect(e));
            return connectEventArg;
        }


        /// <summary>
        /// Starts listening for new connections on the given <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="ep"><see cref="EndPoint"/> to use</param>
        public void StartListen(EndPoint ep)
        {
            listenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(ep);
            listenSocket.Listen(backlog);
            StartAccept();
        }

        private void StartAccept()
        {
            SocketAsyncEventArgs acceptEventArg = acceptPool.Count > 0 ? acceptPool.Pop() : CreateNewAcceptSaea();
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
                SocketAsyncEventArgs srEventArgs = sendReceivePool.Pop();

                // Pass the socket from the "accept" SAEA to "send/receive" SAEA.
                srEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;
                StartReceive(srEventArgs);

                // Remove "accept" SAEA socket and put it back in pool to be used for the next accept operation.
                acceptEventArgs.AcceptSocket = null;
                acceptPool.Push(acceptEventArgs);
                StartAccept();
            }
            else
            {
                acceptEventArgs.AcceptSocket.Close();
                acceptPool.Push(acceptEventArgs);
                maxConnectionEnforcer.Release();
                StartAccept();
            }
        }


        /// <summary>
        /// Connects to a node by using the given <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="ep"><see cref="EndPoint"/> to use</param>
        public void StartConnect(EndPoint ep)
        {
            SocketAsyncEventArgs connectEventArgs = connectPool.Count > 0 ? connectPool.Pop() : CreateNewConnectSaea();
            connectEventArgs.RemoteEndPoint = ep;
            connectEventArgs.AcceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            StartConnect(connectEventArgs);
        }

        private void StartConnect(SocketAsyncEventArgs connectEventArgs)
        {
            maxConnectionEnforcer.WaitOne();
            if (!connectEventArgs.AcceptSocket.ConnectAsync(connectEventArgs))
            {
                ProcessConnect(connectEventArgs);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs connectEventArgs)
        {
            if (connectEventArgs.SocketError == SocketError.Success)
            {
                SocketAsyncEventArgs srEventArgs = sendReceivePool.Pop();

                // Pass the socket from the "connect" SAEA to "send/receive" SAEA.
                srEventArgs.AcceptSocket = connectEventArgs.AcceptSocket;

                MessageManager msgMan = srEventArgs.UserToken as MessageManager;
                msgMan.StartHandShake(srEventArgs);

                StartSend(srEventArgs);

                // Remove "connect" SAEA socket and put it back in pool to be used for the next connect operation.
                connectEventArgs.AcceptSocket = null;
                connectPool.Push(connectEventArgs);
            }
            else
            {
                connectEventArgs.AcceptSocket.Close();
                connectPool.Push(connectEventArgs);
                maxConnectionEnforcer.Release();
            }
        }


        private void StartReceive(SocketAsyncEventArgs recEventArgs)
        {
            if (!recEventArgs.AcceptSocket.ReceiveAsync(recEventArgs))
            {
                ProcessReceive(recEventArgs);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs recEventArgs)
        {
            if (recEventArgs.SocketError == SocketError.Success)
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


        private void StartSend(SocketAsyncEventArgs sendEventArgs)
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

            maxConnectionEnforcer.Release();
        }
    }
}
