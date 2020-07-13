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
        /// <param name="bufferLength">Size of the buffer used for each <see cref="SocketAsyncEventArgs"/> object</param>
        /// <param name="repMan">A reply manager to create appropriate response to a given message</param>
        /// <param name="ns">Node status</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        public MessageManager(int bufferLength, IReplyManager repMan, INodeStatus ns,
                              NetworkType netType = NetworkType.MainNet)
        {
            if (bufferLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferLength), "Buffer length can not be negative or zero.");
            if (repMan is null)
                throw new ArgumentNullException(nameof(repMan), "Reply manager can not be null.");

            magicBytes = netType switch
            {
                NetworkType.MainNet => Base16.Decode(Constants.MainNetMagic),
                NetworkType.TestNet => Base16.Decode(Constants.TestNetMagic),
                NetworkType.RegTest => Base16.Decode(Constants.RegTestMagic),
                _ => throw new ArgumentException(Err.InvalidNetwork)
            };
            this.netType = netType;

            replyManager = repMan;
            NodeStatus = ns;

            buffLen = bufferLength;
            Init();
        }


        private readonly int buffLen;
        private readonly NetworkType netType;
        private readonly byte[] magicBytes;
        private readonly IReplyManager replyManager;

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

        private byte[] tempHolder;

        private readonly Queue<Message> toSendQueue = new Queue<Message>();


        internal void Init()
        {
            DataToSend = null;
            toSendQueue.Clear();
            NodeStatus.HandShake = HandShakeState.None;
            IsReceiveCompleted = true;
        }

        /// <summary>
        /// Sets <see cref="SocketAsyncEventArgs"/>'s buffer for a send operation 
        /// (caller must check <see cref="HasDataToSend"/> before calling this)
        /// </summary>
        /// <param name="sendEventArgs">Socket arg to use</param>
        public void SetSendBuffer(SocketAsyncEventArgs sendEventArgs)
        {
            if (DataToSend == null)
            {
                Message msg = toSendQueue.Dequeue();
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
        /// Starts the handshake process by enqueueing a version message to be sent
        /// </summary>
        /// <param name="srEventArgs">Socket arg to use</param>
        public void StartHandShake(SocketAsyncEventArgs srEventArgs)
        {
            toSendQueue.Enqueue(replyManager.GetVersionMsg());
            SetSendBuffer(srEventArgs);
            NodeStatus.HandShake = HandShakeState.Sent;
        }


        /// <summary>
        /// Reads and processes the given bytes from the given buffer and provided length.
        /// </summary>
        /// <param name="buffer">Buffer containing the received bytes</param>
        /// <param name="len">Number of bytes received</param>
        /// <param name="offert">Offset inside <paramref name="buffer"/> parameter where the data begins</param>
        public void ReadBytes(byte[] buffer, int len, int offert)
        {
            if (len > 0 && buffer != null)
            {
                if (IsReceiveCompleted && len >= Constants.MessageHeaderSize)
                {
                    FastStreamReader stream = new FastStreamReader(buffer, offert, len);
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
                                if (stream.GetRemainingBytesCount() > 0)
                                {
                                    _ = stream.TryReadByteArray(stream.GetRemainingBytesCount(), out tempHolder);
                                    ReadBytes(tempHolder, tempHolder.Length, 0);
                                }
                                else
                                {
                                    tempHolder = null;
                                }
                            }
                            else if (res == Message.ReadResult.NotEnoughBytes)
                            {
                                IsReceiveCompleted = false;
                                tempHolder = new byte[len];
                                Buffer.BlockCopy(buffer, 0, tempHolder, 0, len);
                            }
                            else // TODO: add violation (invalid message)
                            {

                            }
                        }
                        else if (rem != 0)
                        {
                            stream.TryReadByteArray(rem, out tempHolder);
                            IsReceiveCompleted = false;
                        }
                    }
                    else // TODO: add violation (No magic was found)
                    {

                    }
                    // TODO: some sort of point system is needed to give negative points to malicious nodes
                    //       eg. sending garbage bytes which is what is caught in the "else" part of the "if" above
                }
                else
                {
                    if (tempHolder == null)
                    {
                        tempHolder = new byte[len];
                        Buffer.BlockCopy(buffer, 0, tempHolder, 0, len);
                    }
                    else
                    {
                        byte[] temp = new byte[tempHolder.Length + len];
                        Buffer.BlockCopy(tempHolder, 0, temp, 0, tempHolder.Length);
                        Buffer.BlockCopy(buffer, 0, temp, tempHolder.Length, len);
                        tempHolder = temp;
                    }

                    if (tempHolder.Length >= Constants.MessageHeaderSize)
                    {
                        IsReceiveCompleted = true;
                        ReadBytes(tempHolder, tempHolder.Length, 0);
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
            => ReadBytes(recEventArgs.Buffer, recEventArgs.BytesTransferred, recEventArgs.Offset);
    }
}
