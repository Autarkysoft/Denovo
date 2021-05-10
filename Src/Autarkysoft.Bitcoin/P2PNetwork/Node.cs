// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Net;
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
        /// <param name="cs">Client settings</param>
        /// <param name="socket">Socket to use</param>
        internal Node(IClientSettings cs, Socket socket)
        {
            settings = cs;
            NodeStatus = new NodeStatus();
            if (socket.RemoteEndPoint is IPEndPoint ep)
            {
                NodeStatus.IP = ep.Address;
                NodeStatus.Port = (ushort)ep.Port;
            }

            var repMan = new ReplyManager(NodeStatus, cs);

            sendReceiveSAEA = cs.SendReceivePool.Pop();
            sendReceiveSAEA.AcceptSocket = socket;
            sendReceiveSAEA.UserToken = new MessageManager(cs, repMan, NodeStatus);
            sendReceiveSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            sendSAEA = cs.SendReceivePool.Pop();
            sendSAEA.AcceptSocket = socket;
            sendSAEA.UserToken = new MessageManager(cs, repMan, NodeStatus);
            sendSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);

            secondSendLimiter = new Semaphore(1, 1);

            pingTimer = new System.Timers.Timer(TimeSpan.FromMinutes(1.1).TotalMilliseconds);
            pingTimer.Elapsed += PingTimer_Elapsed;
            pingTimer.Start();
        }


        private void PingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!isDisposed)
            {
                // Every 1 minute (~66 sec) the LastSeen is checked but only if 2 minutes has passed a Ping will be sent
                // this will make sure the connection to the other node is still alive while avoiding unnecessary Pings
                var msgMan = (MessageManager)sendSAEA.UserToken;
                if (DateTime.Now.Subtract(msgMan.NodeStatus.LastSeen) >= TimeSpan.FromMinutes(2))
                {
                    if (NodeStatus.HasTooManyUnansweredPings)
                    {
                        NodeStatus.SignalDisconnect();
                    }
                    else
                    {
                        Message ping = msgMan.GetPingMsg();
                        Send(ping);
                    }
                }
            }
        }

        private Semaphore secondSendLimiter;
        private SocketAsyncEventArgs sendReceiveSAEA;
        private SocketAsyncEventArgs sendSAEA;
        private readonly IClientSettings settings;
        private System.Timers.Timer pingTimer;


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
            ((MessageManager)sendReceiveSAEA.UserToken).StartHandShake(sendReceiveSAEA);
            StartSend(sendReceiveSAEA);
        }

        internal void StartReceiving() => StartReceive(sendReceiveSAEA);

        private void StartReceive(SocketAsyncEventArgs recEventArgs)
        {
            if (!isDisposed && !recEventArgs.AcceptSocket.ReceiveAsync(recEventArgs))
            {
                ProcessReceive(recEventArgs);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs recEventArgs)
        {
            // Zero bytes transferred means remote end has closed the connection. It needs to be closed here too.
            if (recEventArgs.SocketError == SocketError.Success && recEventArgs.BytesTransferred > 0)
            {
                var msgMan = recEventArgs.UserToken as MessageManager;
                msgMan.ReadBytes(recEventArgs);

                if (!isDisposed && !NodeStatus.IsDisconnected && !NodeStatus.HasTooManyViolations)
                {
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
            }
            else
            {
                NodeStatus.SignalDisconnect();
            }
        }

        /// <summary>
        /// Sends the given message to this connected node.
        /// </summary>
        /// <param name="msg">Message to send</param>
        public void Send(Message msg)
        {
            secondSendLimiter.WaitOne();
            ((MessageManager)sendSAEA.UserToken).SetSendBuffer(sendSAEA, msg);
            StartSend(sendSAEA);
        }

        internal void StartSend(SocketAsyncEventArgs sendEventArgs)
        {
            if (!isDisposed && !sendEventArgs.AcceptSocket.SendAsync(sendEventArgs))
            {
                ProcessSend(sendEventArgs);
            }
        }

        private void ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                var msgMan = sendEventArgs.UserToken as MessageManager;
                if (msgMan.HasDataToSend)
                {
                    msgMan.SetSendBuffer(sendEventArgs);
                    StartSend(sendEventArgs);
                }
                else if (ReferenceEquals(sendEventArgs, sendSAEA))
                {
                    secondSendLimiter.Release();
                }
                else if (!isDisposed && !NodeStatus.IsDisconnected && !NodeStatus.HasTooManyViolations)
                {
                    StartReceive(sendEventArgs);
                }
            }
            else
            {
                NodeStatus.SignalDisconnect();
            }
        }



        private void CloseClientSocket(SocketAsyncEventArgs srEventArgs)
        {
            try
            {
                srEventArgs?.AcceptSocket?.Shutdown(SocketShutdown.Both);
                srEventArgs?.AcceptSocket?.Close();
            }
            catch (Exception)
            {
            }
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
                isDisposed = true;
                if (disposing)
                {
                    // Dispose timer first to prevent it from raising its event and calling Send()
                    if (!(pingTimer is null))
                    {
                        pingTimer.Stop();
                        pingTimer.Dispose();
                        pingTimer = null;
                    }

                    // There are 2 SAEAs both using the same Socket, closing only one is enough.
                    if (!(sendReceiveSAEA is null))
                    {
                        CloseClientSocket(sendReceiveSAEA);

                        sendReceiveSAEA.AcceptSocket = null;
                        sendReceiveSAEA.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                        settings.SendReceivePool.Push(sendReceiveSAEA);
                        sendReceiveSAEA = null;
                    }

                    if (!(sendSAEA is null))
                    {
                        sendSAEA.AcceptSocket = null;
                        sendSAEA.Completed -= new EventHandler<SocketAsyncEventArgs>(IO_Completed);

                        settings.SendReceivePool.Push(sendSAEA);
                        sendSAEA = null;
                    }

                    if (!(secondSendLimiter is null))
                        secondSendLimiter.Dispose();
                    secondSendLimiter = null;

                    if (settings.Blockchain.State == Blockchain.BlockchainState.BlocksSync && NodeStatus.InvsToGet?.Count != 0)
                    {
                        settings.Blockchain.PutBackMissingBlocks(NodeStatus.InvsToGet);
                    }

                    settings.MaxConnectionEnforcer.Release();
                }
            }
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose() => Dispose(true);
    }
}
