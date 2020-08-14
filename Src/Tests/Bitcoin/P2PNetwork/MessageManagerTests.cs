// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MessageManagerTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var cs = new MockClientSettings() { _netType = NetworkType.MainNet, _buffLen = 10 };

            MessageManager man = new MessageManager(cs, new MockReplyManager(), new NodeStatus());

            Assert.True(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);
            Assert.Null(man.DataToSend);
        }

        [Fact]
        public void DataToSendTest()
        {
            var cs = new MockClientSettings() { _netType = NetworkType.MainNet, _buffLen = 10 };
            MessageManager man = new MessageManager(cs, new MockReplyManager(), new NodeStatus())
            {
                DataToSend = null
            };

            Assert.True(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);

            man.DataToSend = new byte[0];
            Assert.True(man.IsReceiveCompleted);
            Assert.True(man.HasDataToSend);

            man.DataToSend = new byte[3] { 1, 2, 3 };
            Assert.True(man.IsReceiveCompleted);
            Assert.True(man.HasDataToSend);
        }

        public static IEnumerable<object[]> GetSetBufferCases()
        {
            yield return new object[]
            {
                // Smaller than buffer length
                10,
                new byte[3] { 1, 2, 3 },
                new byte[10] { 1, 2, 3, 0, 0, 0, 0, 0, 0, 0 },
                null,
                3,
                0
            };
            yield return new object[]
            {
                // Equal to buffer length
                10,
                new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                null,
                10,
                0
            };
            yield return new object[]
            {
                // Bigger than buffer length (needs only 2 calls)
                10,
                new byte[12] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                // From 3rd byte is from previous buffer but since count is 3 they are ignored and SAEA won't send them
                new byte[10] { 11, 12, 3, 4, 5, 6, 7, 8, 9, 10 },
                10,
                2
            };
        }
        [Theory]
        [MemberData(nameof(GetSetBufferCases))]
        public void SetSendBufferTest(int buffLen, byte[] toSend, byte[] expecBuffer1, byte[] expecBuffer2,
                                      int sendLen1, int sendLen2)
        {
            using SocketAsyncEventArgs sarg = new SocketAsyncEventArgs();
            sarg.SetBuffer(new byte[buffLen], 0, buffLen);

            var cs = new MockClientSettings() { _buffLen = buffLen, _netType = NetworkType.MainNet };
            MessageManager man = new MessageManager(cs, new MockReplyManager(), new NodeStatus())
            {
                DataToSend = toSend
            };

            Assert.True(man.HasDataToSend);

            man.SetSendBuffer(sarg);

            Assert.Equal(sendLen1, sarg.Count);
            Assert.Equal(0, sarg.Offset);
            Assert.Equal(expecBuffer1, sarg.Buffer);

            if (expecBuffer2 == null)
            {
                Assert.False(man.HasDataToSend);
            }
            else
            {
                Assert.True(man.HasDataToSend);

                man.SetSendBuffer(sarg);

                Assert.False(man.HasDataToSend);
                Assert.Equal(sendLen2, sarg.Count);
                Assert.Equal(0, sarg.Offset);
                Assert.Equal(expecBuffer2, sarg.Buffer);
            }
        }

        public static IEnumerable<object[]> GetSetBufferMsgCases()
        {
            yield return new object[]
            {
                // Smaller than buffer length
                30, // < 24+3
                new Message(new MockSerializableMessagePayload(PayloadType.Addr, new byte[3] { 1, 2, 3 }), NetworkType.MainNet),
                new byte[30]
                {
                    0xf9, 0xbe, 0xb4, 0xd9,
                    (byte)'a', (byte)'d', (byte)'d', (byte)'r', 0, 0, 0, 0, 0, 0, 0, 0,
                    3, 0, 0, 0,
                    0x19, 0xc6, 0x19, 0x7e,
                    1, 2, 3,
                    0, 0, 0
                },
                null,
                24+3,
                0
            };
            yield return new object[]
            {
                // Bigger than buffer length
                20,
                new Message(new MockSerializableMessagePayload(PayloadType.Addr, new byte[3] { 1, 2, 3 }), NetworkType.MainNet),
                new byte[20]
                {
                    0xf9, 0xbe, 0xb4, 0xd9,
                    (byte)'a', (byte)'d', (byte)'d', (byte)'r', 0, 0, 0, 0, 0, 0, 0, 0,
                    3, 0, 0, 0
                },
                new byte[20]
                {
                    0x19, 0xc6, 0x19, 0x7e,
                    1, 2, 3,
                    // Leftover from previous buffer set
                    (byte)'r', 0, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0
                },
                20,
                7
            };

        }
        [Theory]
        [MemberData(nameof(GetSetBufferMsgCases))]
        public void SetSendBuffer_WithMsgTest(int buffLen, Message toSend, byte[] expecBuffer1, byte[] expecBuffer2,
                                              int sendLen1, int sendLen2)
        {
            using SocketAsyncEventArgs sarg = new SocketAsyncEventArgs();
            sarg.SetBuffer(new byte[buffLen], 0, buffLen);

            var cs = new MockClientSettings() { _buffLen = buffLen, _netType = NetworkType.MainNet };
            MessageManager man = new MessageManager(cs, new MockReplyManager(), new NodeStatus());

            Assert.False(man.HasDataToSend);

            man.SetSendBuffer(sarg, toSend);

            Assert.Equal(sendLen1, sarg.Count);
            Assert.Equal(0, sarg.Offset);
            Assert.Equal(expecBuffer1, sarg.Buffer);

            if (expecBuffer2 == null)
            {
                Assert.False(man.HasDataToSend);
            }
            else
            {
                Assert.True(man.HasDataToSend);

                man.SetSendBuffer(sarg);

                Assert.False(man.HasDataToSend);
                Assert.Equal(sendLen2, sarg.Count);
                Assert.Equal(0, sarg.Offset);
                Assert.Equal(expecBuffer2, sarg.Buffer);
            }
        }

        [Fact]
        public void StartHandShakeTest()
        {
            var pl = new MockSerializableMessagePayload(PayloadType.Version, new byte[3] { 1, 2, 3 });
            Message msg = new Message(pl, NetworkType.MainNet);
            byte[] msgSer = Helper.HexToBytes("f9beb4d976657273696f6e00000000000300000019c6197e010203");
            var repMan = new MockReplyManager() { verMessage = msg };
            var cs = new MockClientSettings() { _netType = NetworkType.MainNet, _buffLen = 30 };
            MessageManager man = new MessageManager(cs, repMan, new NodeStatus());
            using SocketAsyncEventArgs sarg = new SocketAsyncEventArgs();
            sarg.SetBuffer(new byte[30], 0, 30);

            byte[] expBuffer = new byte[30];
            Buffer.BlockCopy(msgSer, 0, expBuffer, 0, msgSer.Length);

            man.StartHandShake(sarg);

            Assert.Equal(expBuffer, sarg.Buffer);
            Assert.Equal(msgSer.Length, sarg.Count);
            Assert.Equal(0, sarg.Offset);
            Assert.Null(man.DataToSend); // StartHandShake sets the SAEA buffer and sets the field to null
            Assert.False(man.HasDataToSend);
            Assert.True(man.IsReceiveCompleted);
        }

        public static IEnumerable<object[]> GetReadBytesCases()
        {
            var mockPlt = (PayloadType)10000;
            var mockMsg = new Message(new MockSerializableMessagePayload(mockPlt, new byte[] { 1, 2, 3 }), NetworkType.MainNet);

            yield return new object[]
            {
                new MockReplyManager() { toReceive = new PayloadType[] { PayloadType.Verack } },
                Helper.HexToBytes("f9beb4d976657261636b000000000000000000005df6e0e2"),
                0,
                24,
                false,
                false
            };
            yield return new object[]
            {
                new MockReplyManager() { toReceive = new PayloadType[] { PayloadType.Verack } },
                Helper.HexToBytes("010203f9beb4d976657261636b000000000000000000005df6e0e2"),
                3,
                24,
                false,
                false
            };
            yield return new object[]
            {
                new MockReplyManager() { toReceive = new PayloadType[] { PayloadType.Verack } },
                Helper.HexToBytes("0102030405f9beb4d976657261636b000000000000000000005df6e0e2"),
                3,
                26, // 2 extra bytes at the beginning
                false,
                false
            };
            yield return new object[]
            {
                new MockReplyManager() { toReceive = new PayloadType[] { PayloadType.Verack } },
                Helper.HexToBytes("f9beb4d976657261636b000000000000000000005df6e0e1"),
                0,
                24,
                false,
                true // Invalid checksum
            };
            yield return new object[]
            {
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Version },
                    toReply = new Message[][] { new Message[] { mockMsg } }
                },
                Helper.HexToBytes("f9beb4d976657273696f6e0000000000650000005f1a69d2" +
                "721101000100000000000000bc8f5e5400000000010000000000000000000000000000000000ffffc61b6409208d" +
                "010000000000000000000000000000000000ffffcb0071c0208d128035cbc97953f80f2f5361746f7368693a302e" +
                "392e332fcf05050001"),
                0,
                125,
                true,
                false
            };
        }
        [Theory]
        [MemberData(nameof(GetReadBytesCases))]
        public void ReadBytesTest(IReplyManager repMan, byte[] buffer, int offset, int recvLen, bool hasSend, bool violation)
        {
            var cs = new MockClientSettings() { _netType = NetworkType.MainNet, _buffLen = 200 };
            MessageManager man = new MessageManager(cs, repMan, new MockNodeStatus() { smallViolation = violation });
            man.ReadBytes(buffer, offset, recvLen);
            Assert.Equal(hasSend, man.HasDataToSend);
        }

        [Fact]
        public void ReadBytes_SmallBufferTest()
        {
            var mockMsg = new Message(new MockSerializableMessagePayload((PayloadType)10000, new byte[] { 1, 2, 3 }), NetworkType.MainNet);
            var repMan = new MockReplyManager()
            {
                toReceive = new PayloadType[] { PayloadType.Version },
                toReply = new Message[][] { new Message[] { mockMsg } }
            };
            var cs = new MockClientSettings() { _netType = NetworkType.MainNet, _buffLen = 10 };
            MessageManager man = new MessageManager(cs, repMan, new NodeStatus());

            man.ReadBytes(null, 0, 0);
            Assert.True(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);

            man.ReadBytes(new byte[100], 0, 0);
            Assert.True(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);

            // f9beb4d976657273696f6e0000000000650000005f1a69d2
            man.ReadBytes(new byte[] { 0xf9, 0xbe }, 0, 2);
            Assert.False(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);

            man.ReadBytes(new byte[] { 0xb4, 0xd9, 0x76 }, 0, 3);
            Assert.False(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);
            Helper.ComparePrivateField(man, "rcvHolder", new byte[] { 0xf9, 0xbe, 0xb4, 0xd9, 0x76 });

            man.ReadBytes(new byte[18] { 0x65, 0x72, 0x73, 0x69, 0x6f, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x65, 0x00, 0x00, 0x00, 0x5f, 0x1a, 0x69 }, 0, 18);
            Assert.False(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);
            Helper.ComparePrivateField(man, "rcvHolder", new byte[] { 0xf9, 0xbe, 0xb4, 0xd9, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6f, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x65, 0x00, 0x00, 0x00, 0x5f, 0x1a, 0x69 });

            // Header will be read completely but this header needs a payload which is not yet present
            man.ReadBytes(new byte[] { 0xd2 }, 0, 1);
            Assert.False(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);
            Helper.ComparePrivateField(man, "rcvHolder", new byte[] { 0xf9, 0xbe, 0xb4, 0xd9, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6f, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x65, 0x00, 0x00, 0x00, 0x5f, 0x1a, 0x69, 0xd2 });

            man.ReadBytes(new byte[5] { 0x72, 0x11, 0x01, 0x00, 0x01 }, 0, 5);
            Assert.False(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);
            Helper.ComparePrivateField(man, "rcvHolder", Helper.HexToBytes("f9beb4d976657273696f6e0000000000650000005f1a69d27211010001"));

            man.ReadBytes(Helper.HexToBytes("00000000000000bc8f5e5400000000010000000000000000000000000000000000ffffc61b6409208d010000000000000000000000000000000000ffffcb0071c0208d128035cbc97953f80f2f5361746f7368693a302e392e332fcf05050001"), 0, 96);
            Assert.True(man.IsReceiveCompleted);
            Assert.True(man.HasDataToSend);
        }
    }
}
