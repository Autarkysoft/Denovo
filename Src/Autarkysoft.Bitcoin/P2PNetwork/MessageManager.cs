// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Net.Sockets;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    class MessageManager
    {
        public MessageManager(int bufferLength)
        {
            buffLen = bufferLength;
            Init();
        }



        private const int HeaderLength = 24;
        private readonly byte[] MagicBa = new byte[] { 0xf9, 0xbe, 0xb4, 0xd9 };
        private readonly int buffLen;
        private byte[] dataToSend;
        private int sendOffset, remainSend;

        public bool HasReply { get; private set; }
        public bool IsReceiveCompleted { get; private set; }
        public bool IsSendCompleted { get; private set; }
        public bool IsHandShaking { get; private set; }



        internal void Init()
        {
            dataToSend = null;
            sendOffset = 0;
            remainSend = 0;
            IsReceiveCompleted = true;
            IsSendCompleted = true;
            HasReply = false;
        }

        private bool TryGetReply(Message msg, out Message reply)
        {
            reply = msg.Payload.PayloadType switch
            {
                PayloadType.Ping => new Message(new PongPayload(), NetworkType.MainNet),
                PayloadType.Version => new Message(new VerackPayload(), NetworkType.MainNet),
                _ => null,
            };
            return !(reply is null);
        }

        private byte[] tempHolder;
        bool isInTheMiddleOfReceivingMessage;
        internal bool IsSendFinished;

        // Data length is at least HeaderSize bytes (=24)
        private void ProcessData(byte[] data, int len)
        {
            FastStreamReader stream = new FastStreamReader(data.SubArray(0, len));
            Message msg = new Message(NetworkType.MainNet);
            if (msg.TryDeserializeHeader(stream, out _))
            {
                if (msg.payloadSize + HeaderLength <= len)
                {
                    if (msg.Payload.TryDeserialize(stream, out _))
                    {
                        if (TryGetReply(msg, out Message reply))
                        {
                            HasReply = true;
                            FastStream str = new FastStream();
                            reply.Serialize(str);
                            dataToSend = str.ToByteArray();
                            remainSend = dataToSend.Length;
                            sendOffset = 0;
                            IsSendCompleted = false;
                            IsHandShaking = true;
                        }

                        // TODO: handle received message here. eg. write received block to disk,...

                        // handle remaining data
                        int offset = stream.GetCurrentIndex();
                        if (len - offset >= HeaderLength)
                        {
                            ProcessData(data.SubArray(offset, len - offset), len - offset);
                        }
                        else
                        {
                            if (data[offset] == MagicBa[0])
                            {
                                isInTheMiddleOfReceivingMessage = true;
                                tempHolder = new byte[len - offset];
                                Buffer.BlockCopy(data, offset, tempHolder, 0, len - offset);
                            }
                        }
                    }
                    else
                    {
                        tempHolder = null;
                        isInTheMiddleOfReceivingMessage = false;
                        // TODO: set a new reject message to be sent
                    }
                }
                else
                {
                    isInTheMiddleOfReceivingMessage = true;
                    tempHolder = new byte[len];
                    Buffer.BlockCopy(data, 0, tempHolder, 0, len);
                }
            }
            else
            {
                // TODO: set a new reject message to be sent for invalid message header
            }
        }



        internal void ReadBytes(SocketAsyncEventArgs recEventArgs)
        {
            if (recEventArgs.BytesTransferred != 0)
            {
                if (isInTheMiddleOfReceivingMessage)
                {
                    byte[] temp = new byte[tempHolder.Length + recEventArgs.BytesTransferred];
                    Buffer.BlockCopy(tempHolder, 0, temp, 0, tempHolder.Length);
                    Buffer.BlockCopy(recEventArgs.Buffer, 0, temp, tempHolder.Length, recEventArgs.BytesTransferred);
                    tempHolder = temp;

                    if (tempHolder.Length >= HeaderLength)
                    {
                        ProcessData(tempHolder, tempHolder.Length);
                    }
                }
                else
                {
                    // look for magic, check if header size is met => read header, else set middleofreceiving to true and get out
                    if (recEventArgs.BytesTransferred >= HeaderLength)
                    {
                        ProcessData(recEventArgs.Buffer, recEventArgs.BytesTransferred);
                    }
                    else
                    {
                        isInTheMiddleOfReceivingMessage = true;
                        tempHolder = new byte[recEventArgs.BytesTransferred];
                        Buffer.BlockCopy(recEventArgs.Buffer, 0, tempHolder, 0, recEventArgs.BytesTransferred);
                    }
                }
            }
        }

        internal void SetSendBuffer(SocketAsyncEventArgs sendEventArgs)
        {
            if (remainSend > buffLen)
            {
                Buffer.BlockCopy(dataToSend, sendOffset, sendEventArgs.Buffer, 0, buffLen);
                sendEventArgs.SetBuffer(0, buffLen);

                IsSendFinished = false;
                remainSend -= buffLen;
                sendOffset += buffLen;
            }
            else if (remainSend == buffLen)
            {
                Buffer.BlockCopy(dataToSend, sendOffset, sendEventArgs.Buffer, 0, buffLen);
                sendEventArgs.SetBuffer(0, buffLen);

                IsSendFinished = true;
                remainSend = 0;
                sendOffset += buffLen;
            }
            else if (remainSend < buffLen)
            {
                Buffer.BlockCopy(dataToSend, sendOffset, sendEventArgs.Buffer, 0, remainSend);
                sendEventArgs.SetBuffer(0, remainSend);

                IsSendFinished = true;
                remainSend = 0;
                sendOffset += remainSend;
            }
        }

        internal void StartHandShake(SocketAsyncEventArgs srEventArgs)
        {
            Message ver = new Message(NetworkType.MainNet)
            {
                Payload = new VersionPayload(70015, NodeServiceFlags.All, 0, false)
            };

            FastStream stream = new FastStream();
            ver.Serialize(stream);
            dataToSend = stream.ToByteArray();
            remainSend = dataToSend.Length;
            sendOffset = 0;
            IsSendCompleted = false;
            IsHandShaking = true;

            SetSendBuffer(srEventArgs);
        }
    }
}
