// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
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
        /// <param name="bufferLength">Size of the buffer used for each <see cref="SocketAsyncEventArgs"/> object</param>
        /// <param name="versionMessage">Version message (used for initiating handshake)</param>
        /// <param name="netType">[Default value = <see cref="NetworkType.MainNet"/>] Network type</param>
        public MessageManager(int bufferLength, Message versionMessage, NetworkType netType = NetworkType.MainNet)
        {
            if (bufferLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferLength), "Buffer length can not be negative or zero.");

            magicBytes = netType switch
            {
                NetworkType.MainNet => Base16.Decode(Constants.MainNetMagic),
                NetworkType.TestNet => Base16.Decode(Constants.TestNetMagic),
                NetworkType.RegTest => Base16.Decode(Constants.RegTestMagic),
                _ => throw new ArgumentException("Network type is not defined.", nameof(netType))
            };
            this.netType = netType;

            verMsg = versionMessage;

            buffLen = bufferLength;
            Init();
        }



        private const int HeaderLength = 24;

        private readonly int buffLen;
        private readonly NetworkType netType;
        private readonly byte[] magicBytes;
        private readonly Message verMsg;

        private byte[] _sendData;
        /// <summary>
        /// Gets or sets the data to be sent
        /// </summary>
        public byte[] DataToSend
        {
            get => _sendData;
            set
            {
                remainSend = value == null ? 0 : value.Length;
                sendOffset = 0;
                _sendData = value;
            }
        }
        private int sendOffset, remainSend;


        /// <summary>
        /// Indicates whether the previous message was fully received
        /// </summary>
        public bool IsReceiveCompleted { get; private set; }
        /// <summary>
        /// Indicates whether there is any data to send
        /// </summary>
        public bool HasDataToSend => remainSend > 0 || toSendQueue.Count > 0;
        /// <summary>
        /// Indicates whether the handshake process between two nodes is already performed and succeeded
        /// </summary>
        public bool IsHandShakeComplete { get; private set; }

        private byte[] tempHolder;

        private readonly Queue<Message> toSendQueue = new Queue<Message>();


        internal void Init()
        {
            DataToSend = null;
            toSendQueue.Clear();
            IsHandShakeComplete = false;
            IsReceiveCompleted = true;
        }

        /// <summary>
        /// Sets <see cref="SocketAsyncEventArgs"/>'s buffer for a send operation 
        /// (caller must check <see cref="HasDataToSend"/> before calling this)
        /// </summary>
        /// <param name="sendEventArgs">Socket arg to use</param>
        public void SetSendBuffer(SocketAsyncEventArgs sendEventArgs)
        {
            if (remainSend == 0)
            {
                Message msg = toSendQueue.Dequeue();
                FastStream stream = new FastStream((int)msg.payloadSize + HeaderLength);
                msg.Serialize(stream);
                DataToSend = stream.ToByteArray();
            }

            if (remainSend >= buffLen)
            {
                Buffer.BlockCopy(DataToSend, sendOffset, sendEventArgs.Buffer, 0, buffLen);
                sendEventArgs.SetBuffer(0, buffLen);

                sendOffset += buffLen;
                remainSend -= buffLen;
            }
            else if (remainSend < buffLen)
            {
                Buffer.BlockCopy(DataToSend, sendOffset, sendEventArgs.Buffer, 0, remainSend);
                sendEventArgs.SetBuffer(0, remainSend);

                sendOffset += remainSend;
                remainSend = 0;
            }
        }

        /// <summary>
        /// Starts the handshake process (called must check <see cref="IsHandShakeComplete"/> before calling this).
        /// </summary>
        /// <param name="srEventArgs">Socket arg to use</param>
        public void StartHandShake(SocketAsyncEventArgs srEventArgs)
        {
            toSendQueue.Enqueue(verMsg);
            SetSendBuffer(srEventArgs);
            IsHandShakeComplete = true;
        }


        private bool TryGetReply(Message msg, out Message reply)
        {
            reply = msg.Payload.PayloadType switch
            {
                PayloadType.Ping => new Message(new PongPayload(((PingPayload)msg.Payload).Nonce), netType),
                PayloadType.Version => new Message(new VerackPayload(), netType),
                _ => null,
            };
            return !(reply is null);
        }


        private void SetRejectMessage(PayloadType plt, string error)
        {
            RejectPayload pl = new RejectPayload()
            {
                Code = RejectCode.FailedToDecodeMessage,
                RejectedMessage = 0,
                Reason = error
            };
            Message rej = new Message(pl, netType);
            toSendQueue.Enqueue(rej);
            IsReceiveCompleted = true;
        }

        private void ProcessData(FastStreamReader stream)
        {
            Message msg = new Message(netType);
            if (msg.TryDeserializeHeader(stream, out string error))
            {
                if (stream.GetRemainingBytesCount() >= msg.payloadSize)
                {
                    if (msg.Payload.TryDeserialize(stream, out error))
                    {
                        // TODO: deseraializing this way doesn't validate the checksum
                        if (TryGetReply(msg, out Message reply))
                        {
                            toSendQueue.Enqueue(reply);
                        }

                        // TODO: handle received message here. eg. write received block to disk,...

                        // Handle remaining data
                        if (stream.GetRemainingBytesCount() > 0)
                        {
                            IsReceiveCompleted = false;
                            stream.TryReadByteArray(stream.GetRemainingBytesCount(), out tempHolder);
                        }
                        else
                        {
                            IsReceiveCompleted = true;
                            tempHolder = null;
                        }
                    }
                    else
                    {
                        IsReceiveCompleted = true;
                        tempHolder = null;
                        SetRejectMessage(msg.Payload.PayloadType, error);
                    }
                }
                else
                {
                    byte[] temp = new byte[HeaderLength + stream.GetRemainingBytesCount()];
                    Buffer.BlockCopy(msg.SerializeHeader(), 0, temp, 0, HeaderLength);
                    _ = stream.TryReadByteArray(stream.GetRemainingBytesCount(), out byte[] rem);
                    Buffer.BlockCopy(rem, 0, temp, HeaderLength, rem.Length);
                    tempHolder = temp;
                    IsReceiveCompleted = false;
                }
            }
            else
            {
                SetRejectMessage(msg.Payload?.PayloadType ?? 0, error);
            }
        }


        public void ReadBytes(byte[] buffer, int len)
        {
            if (len != 0)
            {
                if (IsReceiveCompleted)
                {
                    // Try to find magic inside the received buffer
                    FastStreamReader stream = new FastStreamReader(buffer, 0, len);
                    int index = 0;
                    while (index < len - 4 && !stream.CompareBytes(magicBytes))
                    {
                        _ = stream.TryReadByte(out _);
                    }

                    if (len - index >= HeaderLength)
                    {
                        ProcessData(stream);
                    }
                    else if (len - index != 0)
                    {
                        stream.TryReadByteArray(len - index, out tempHolder);
                        IsReceiveCompleted = false;
                    }
                }
                else
                {
                    byte[] temp = new byte[tempHolder.Length + len];
                    Buffer.BlockCopy(tempHolder, 0, temp, 0, tempHolder.Length);
                    Buffer.BlockCopy(buffer, 0, temp, tempHolder.Length, len);
                    tempHolder = temp;

                    if (tempHolder.Length >= HeaderLength)
                    {
                        FastStreamReader stream = new FastStreamReader(tempHolder);
                        ProcessData(stream);
                    }
                }
            }
        }

        public void ReadBytes(SocketAsyncEventArgs recEventArgs)
        {
            ReadBytes(recEventArgs.Buffer, recEventArgs.BytesTransferred);
        }
    }
}
