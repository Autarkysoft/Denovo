// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines a P2P message manager used for handling send/receive P2P messages for nodes.
    /// </summary>
    public class MessageManager
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MessageManager"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="cs">Client settings</param>
        /// <param name="repMan">A reply manager to create appropriate response to a given message</param>
        /// <param name="ns">Node status</param>
        public MessageManager(IClientSettings cs, IReplyManager repMan, INodeStatus ns)
        {
            if (cs is null)
                throw new ArgumentNullException(nameof(cs), "Client settings can not be null.");
            if (repMan is null)
                throw new ArgumentNullException(nameof(repMan), "Reply manager can not be null.");
            if (ns is null)
                throw new ArgumentNullException(nameof(ns), "Node status can not be null.");

            netType = cs.Network;
            magicBytes = netType switch
            {
                NetworkType.MainNet => Base16.Decode(Constants.MainNetMagic),
                NetworkType.TestNet => Base16.Decode(Constants.TestNetMagic),
                NetworkType.RegTest => Base16.Decode(Constants.RegTestMagic),
                _ => throw new ArgumentException(Err.InvalidNetwork)
            };

            replyManager = repMan;
            NodeStatus = ns;

            buffLen = cs.BufferLength;
            toSendQueue = new Queue<Message>();
            IsReceiveCompleted = true;
        }


        private readonly int buffLen;
        private readonly NetworkType netType;
        private readonly byte[] magicBytes;
        private readonly IReplyManager replyManager;
        private readonly Queue<Message> toSendQueue;
        private byte[] rcvHolder;

        /// <summary>
        /// Gets or sets the data to be sent
        /// </summary>
        public byte[] DataToSend { get; set; }


        /// <summary>
        /// Indicates whether the previous message was fully received
        /// </summary>
        public bool IsReceiveCompleted { get; private set; }
        /// <summary>
        /// Indicates whether there is any data to send
        /// </summary>
        public bool HasDataToSend => DataToSend != null || toSendQueue.Count > 0;

        /// <summary>
        /// Contains the node's current state
        /// </summary>
        public INodeStatus NodeStatus { get; set; }


        /// <summary>
        /// Sets <see cref="SocketAsyncEventArgs"/>'s buffer for a send operation 
        /// (caller must check <see cref="HasDataToSend"/> before calling this)
        /// </summary>
        /// <param name="sendEventArgs">Socket arg to use</param>
        public void SetSendBuffer(SocketAsyncEventArgs sendEventArgs)
        {
            if (DataToSend == null)
            {
                if (!toSendQueue.TryDequeue(out Message msg))
                {
                    sendEventArgs.SetBuffer(sendEventArgs.Offset, 0);
                    return;
                }

                FastStream stream = new FastStream(Constants.MessageHeaderSize + msg.PayloadData.Length);
                msg.Serialize(stream);
                DataToSend = stream.ToByteArray();
            }

            if (DataToSend.Length <= buffLen)
            {
                Buffer.BlockCopy(DataToSend, 0, sendEventArgs.Buffer, sendEventArgs.Offset, DataToSend.Length);
                sendEventArgs.SetBuffer(sendEventArgs.Offset, DataToSend.Length);

                DataToSend = null;
            }
            else // (DataToSend.Length > buffLen)
            {
                Buffer.BlockCopy(DataToSend, 0, sendEventArgs.Buffer, sendEventArgs.Offset, buffLen);
                sendEventArgs.SetBuffer(sendEventArgs.Offset, buffLen);

                byte[] rem = new byte[DataToSend.Length - buffLen];
                Buffer.BlockCopy(DataToSend, buffLen, rem, 0, rem.Length);
                DataToSend = rem;
            }
        }

        /// <summary>
        /// Sets <see cref="SocketAsyncEventArgs"/>'s buffer for a send operation using the given <see cref="Message"/>.
        /// </summary>
        /// <param name="sendSAEA">Socket arg to use</param>
        /// <param name="msg">Message to send</param>
        public void SetSendBuffer(SocketAsyncEventArgs sendSAEA, Message msg)
        {
            var stream = new FastStream();
            msg.Serialize(stream);
            DataToSend = stream.ToByteArray();
            SetSendBuffer(sendSAEA);
        }


        /// <summary>
        /// Starts the handshake process by enqueueing a version message to be sent
        /// </summary>
        /// <param name="srEventArgs">Socket arg to use</param>
        public void StartHandShake(SocketAsyncEventArgs srEventArgs)
        {
            NodeStatus.HandShake = HandShakeState.Sent;
            SetSendBuffer(srEventArgs, replyManager.GetVersionMsg());
        }


        /// <summary>
        /// Reads and processes the given bytes from the given buffer and provided length.
        /// </summary>
        /// <param name="buffer">Buffer containing the received bytes</param>
        /// <param name="offset">Offset inside <paramref name="buffer"/> parameter where the data begins</param>
        /// <param name="rcvLen">Number of bytes received</param>
        public void ReadBytes(byte[] buffer, int offset, int rcvLen)
        {
            if (rcvLen > 0 && buffer != null)
            {
                if (IsReceiveCompleted && rcvLen >= Constants.MessageHeaderSize)
                {
                    FastStreamReader stream = new FastStreamReader(buffer, offset, rcvLen);
                    if (stream.FindAndSkip(magicBytes))
                    {
                        int rem = stream.GetRemainingBytesCount();
                        if (rem >= Constants.MessageHeaderSize)
                        {
                            var msg = new Message(netType);
                            Message.ReadResult res = msg.Read(stream);
                            if (res == Message.ReadResult.Success)
                            {
                                IsReceiveCompleted = true;

                                Message[] reply = replyManager.GetReply(msg);
                                if (reply != null)
                                {
                                    foreach (var item in reply)
                                    {
                                        toSendQueue.Enqueue(item);
                                    }
                                }

                                // TODO: handle received message here. eg. write received block to disk,...

                                // Handle remaining data
                                rem = stream.GetRemainingBytesCount();
                                if (rem > 0)
                                {
                                    rcvHolder = stream.ReadByteArrayChecked(rem);
                                    if (rem >= Constants.MessageHeaderSize)
                                    {
                                        IsReceiveCompleted = true;
                                        ReadBytes(rcvHolder, 0, rcvHolder.Length);
                                    }
                                    else
                                    {
                                        IsReceiveCompleted = false;
                                    }
                                }
                                else
                                {
                                    rcvHolder = null;
                                }
                            }
                            else if (res == Message.ReadResult.NotEnoughBytes)
                            {
                                IsReceiveCompleted = false;
                                rcvHolder = new byte[rcvLen];
                                Buffer.BlockCopy(buffer, offset, rcvHolder, 0, rcvLen);
                            }
                            else
                            {
                                // Invalid message was received (checksum, network or payload size overflow)
                                NodeStatus.AddSmallViolation();
                                rcvHolder = null;
                            }
                        }
                        else if (rem != 0)
                        {
                            rcvHolder = stream.ReadByteArrayChecked(rem);
                            IsReceiveCompleted = false;
                        }
                    }
                    else
                    {
                        // There are always 3 bytes remaining in stream that failed to find magic and has to be stored in holder
                        rcvHolder = stream.ReadByteArrayChecked(stream.GetRemainingBytesCount());
                        IsReceiveCompleted = false;
                        // There were at least 21 bytes of garbage in this buffer
                        NodeStatus.AddSmallViolation();
                    }
                }
                else
                {
                    if (rcvHolder == null)
                    {
                        rcvHolder = new byte[rcvLen];
                        Buffer.BlockCopy(buffer, offset, rcvHolder, 0, rcvLen);
                    }
                    else
                    {
                        byte[] temp = new byte[rcvHolder.Length + rcvLen];
                        Buffer.BlockCopy(rcvHolder, 0, temp, 0, rcvHolder.Length);
                        Buffer.BlockCopy(buffer, offset, temp, rcvHolder.Length, rcvLen);
                        rcvHolder = temp;
                    }

                    if (rcvHolder.Length >= Constants.MessageHeaderSize)
                    {
                        IsReceiveCompleted = true;
                        ReadBytes(rcvHolder, 0, rcvHolder.Length);
                    }
                    else
                    {
                        IsReceiveCompleted = false;
                    }
                }
            }
        }

        /// <summary>
        /// Reads and processes the bytes that were received on this <see cref="SocketAsyncEventArgs"/>.
        /// </summary>
        /// <param name="recEventArgs"><see cref="SocketAsyncEventArgs"/> to use</param>
        public void ReadBytes(SocketAsyncEventArgs recEventArgs)
            => ReadBytes(recEventArgs.Buffer, recEventArgs.Offset, recEventArgs.BytesTransferred);
    }
}
