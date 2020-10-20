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
        public void GetPingMsgTest()
        {
            var cs = new MockClientSettings() { _netType = NetworkType.TestNet, _buffLen = 10 };
            var expectedPing = new Message(new PingPayload(1), NetworkType.TestNet);
            var repMan = new MockReplyManager() { pingMsg = expectedPing };
            MessageManager man = new MessageManager(cs, repMan, new NodeStatus());

            var actualPing = man.GetPingMsg();

            Assert.Same(expectedPing, actualPing);
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
                new byte[1][] { new byte[10] { 1, 2, 3, 0, 0, 0, 0, 0, 0, 0 } },
                3
            };
            yield return new object[]
            {
                // Equal to buffer length
                10,
                new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                new byte[1][] { new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } },
                10
            };
            yield return new object[]
            {
                // Bigger than buffer length (needs only 2 calls)
                10,
                new byte[12] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                new byte[2][]
                {
                    new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                    // From 3rd byte is from previous buffer but since count is 3 they are ignored and SAEA won't send them
                    new byte[10] { 11, 12, 3, 4, 5, 6, 7, 8, 9, 10 }
                },
                2
            };
            yield return new object[]
            {
                // Bigger than buffer length (needs 4 calls)
                3,
                new byte[11] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
                new byte[4][]
                {
                    new byte[3] { 1, 2, 3 },
                    new byte[3] { 4, 5, 6 },
                    new byte[3] { 7, 8, 9 },
                    new byte[3] { 10, 11, 9 }, // Last byte is leftover but sendLen is 2
                },
                2
            };
            yield return new object[]
            {
                // Bigger than buffer length (needs 4 calls)
                3,
                new byte[12] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                new byte[4][]
                {
                    new byte[3] { 1, 2, 3 },
                    new byte[3] { 4, 5, 6 },
                    new byte[3] { 7, 8, 9 },
                    new byte[3] { 10, 11, 12 },
                },
                3
            };
        }
        [Theory]
        [MemberData(nameof(GetSetBufferCases))]
        public void SetSendBufferTest(int buffLen, byte[] toSend, byte[][] expecBuffers, int lastSendLen)
        {
            using SocketAsyncEventArgs sarg = new SocketAsyncEventArgs();
            // There are 5 bytes before and 5 bytes after the buffer used by socket and is set to 254 and should not change.
            byte[] buffer = new byte[buffLen + 10];
            buffer[0] = 254;
            buffer[1] = 254;
            buffer[2] = 254;
            buffer[3] = 254;
            buffer[4] = 254;
            buffer[^1] = 254;
            buffer[^2] = 254;
            buffer[^3] = 254;
            buffer[^4] = 254;
            buffer[^5] = 254;
            sarg.SetBuffer(buffer, 5, buffLen);
            // Create a copy to use as expected buffer
            byte[] expectedBuffer = buffer.CloneByteArray();

            var cs = new MockClientSettings() { _buffLen = buffLen, _netType = NetworkType.MainNet };
            MessageManager man = new MessageManager(cs, new MockReplyManager(), new NodeStatus())
            {
                DataToSend = toSend
            };

            for (int i = 0; i < expecBuffers.Length; i++)
            {
                Assert.True(man.HasDataToSend);
                man.SetSendBuffer(sarg);

                Assert.Equal(5, sarg.Offset);
                // Copy the buffer bytes to the expected buffer from offset=5
                Buffer.BlockCopy(expecBuffers[i], 0, expectedBuffer, 5, buffLen);
                Assert.Equal(expectedBuffer, sarg.Buffer);

                if (i != expecBuffers.Length - 1)
                {
                    Assert.Equal(expecBuffers[i].Length, sarg.Count);
                }
                else
                {
                    Assert.Equal(lastSendLen, sarg.Count);
                }
            }

            Assert.False(man.HasDataToSend);
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
                // Receive length is 0
                new MockReplyManager(),
                new MockNodeStatus(),
                new byte[][] { new byte[0], new byte[0] },
                0,
                new int[] { 0, 0 },
                new byte[][] { null, null },
                new bool[] { true, true },
                new bool[] { false, false }
            };
            yield return new object[]
            {
                // Receive length is 0 with a bigger buffer and at an offset
                new MockReplyManager(),
                new MockNodeStatus(),
                new byte[][] { new byte[10], new byte[10] },
                2,
                new int[] { 0, 0 },
                new byte[][] { null, null },
                new bool[] { true, true },
                new bool[] { false, false }
            };
            yield return new object[]
            {
                // 3 buffers of length 3, 1 and 2 are received and since total size is smaller than header
                // it is not processed and only moved to holder
                new MockReplyManager(),
                new MockNodeStatus(),
                new byte[][] { new byte[5] { 1, 2, 3, 4, 5 }, new byte[5] { 6, 7, 8, 9, 10 }, new byte[5] { 11, 12, 13, 14, 15 } },
                2,
                new int[] { 3, 1, 2 },
                new byte[][] { new byte[3] { 3, 4, 5 }, new byte[4] { 3, 4, 5, 8 }, new byte[6] { 3, 4, 5, 8, 13, 14 } },
                new bool[] { false, false, false },
                new bool[] { false, false, false }
            };
            yield return new object[]
            {
                // 3 buffers received with total length of equal to header size so the result is processed
                // But it doesn't contain the magic.
                // There is also 2 0xff bytes at the start and at the end that aren't read based on offset and buffer length
                new MockReplyManager(),
                new MockNodeStatus() { smallViolation = true },
                new byte[][]
                {
                    new byte[10 + 2 + 2] { 255, 255, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 255, 255  },
                    new byte[4 + 2 + 2] { 255, 255, 11, 12, 13, 14, 255, 255 },
                    new byte[10 + 2 + 2] { 255, 255, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 255, 255 }
                },
                2,
                new int[] { 10, 4, 10 },
                new byte[][]
                {
                    new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                    new byte[14] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 },
                    // Next holder is set to 24 bytes (1,2,...,24) then it is processed and only 3 last bytes are stored
                    new byte[3] { 22, 23, 24 }
                },
                new bool[] { false, false, false },
                new bool[] { false, false, false }
            };
            yield return new object[]
            {
                // Same as before but with bigger than header size total
                new MockReplyManager(),
                new MockNodeStatus() { smallViolation = true },
                new byte[][]
                {
                    new byte[10 + 2 + 2] { 255, 255, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 255, 255  },
                    new byte[4 + 2 + 2] { 255, 255, 11, 12, 13, 14, 255, 255 },
                    new byte[11 + 2 + 2] { 255, 255, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 255, 255 }
                },
                2,
                new int[] { 10, 4, 11 },
                new byte[][]
                {
                    new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                    new byte[14] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 },
                    // Next holder is set to 25 bytes (1,2,...,24,25) then it is processed and only 3 last bytes are stored
                    new byte[3] { 23, 24, 25 }
                },
                new bool[] { false, false, false },
                new bool[] { false, false, false }
            };
            yield return new object[]
            {
                // Same as before but with has another buffer to read
                new MockReplyManager(),
                new MockNodeStatus() { smallViolation = true },
                new byte[][]
                {
                    new byte[10 + 2 + 2] { 255, 255, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 255, 255  },
                    new byte[4 + 2 + 2] { 255, 255, 11, 12, 13, 14, 255, 255 },
                    new byte[11 + 2 + 2] { 255, 255, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 255, 255 },
                    new byte[3 + 2 + 2] { 255, 255, 26, 27, 28, 255, 255 }
                },
                2,
                new int[] { 10, 4, 11, 3 },
                new byte[][]
                {
                    new byte[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                    new byte[14] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 },
                    // Next holder is set to 25 bytes (1,2,...,24,25) then it is processed and only 3 last bytes are stored
                    new byte[3] { 23, 24, 25 },
                    new byte[6] { 23, 24, 25, 26, 27, 28 },
                },
                new bool[] { false, false, false, false },
                new bool[] { false, false, false, false }
            };
            yield return new object[]
            {
                // 2 buffers received and since total size exceeds header size the buffer is processed 
                // and the garbage bytes are disposed in second round
                new MockReplyManager(),
                new MockNodeStatus(),
                new byte[][]
                {
                    new byte[20 + 1] { 255, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 },
                    new byte[4 + 1] { 255, 0xf9, 0xbe, 0xb4, 0xd9 }
                },
                1,
                new int[] { 20, 4 },
                new byte[][]
                {
                    new byte[20] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 },
                    new byte[4] { 0xf9, 0xbe, 0xb4, 0xd9 }
                },
                new bool[] { false, false },
                new bool[] { false, false }
            };
            yield return new object[]
            {
                // Same as before but the message is processed
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Verack },
                    toReceiveBytes = new byte[][] { new byte[0] }
                },
                new MockNodeStatus(),
                new byte[][]
                {
                    new byte[3 + 1 + 2] { 255, 1, 2, 3, 255, 255 },
                    new byte[24 + 1 + 2] { 255, 0xf9, 0xbe, 0xb4, 0xd9, 0x76, 0x65, 0x72, 0x61, 0x63, 0x6b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5d, 0xf6, 0xe0, 0xe2, 255, 255 }
                },
                1,
                new int[] { 3, 24 },
                new byte[][]
                {
                    new byte[3] { 1, 2, 3 },
                    null
                },
                new bool[] { false, true },
                new bool[] { false, false }
            };
            yield return new object[]
            {
                // Same as before but with a reply
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Verack },
                    toReceiveBytes = new byte[][] { new byte[0] },
                    toReply = new Message[][] { new Message[] { mockMsg } }
                },
                new MockNodeStatus(),
                new byte[][]
                {
                    new byte[3 + 1 + 2] { 255, 1, 2, 3, 255, 255 },
                    new byte[24 + 1 + 2] { 255, 0xf9, 0xbe, 0xb4, 0xd9, 0x76, 0x65, 0x72, 0x61, 0x63, 0x6b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5d, 0xf6, 0xe0, 0xe2, 255, 255 }
                },
                1,
                new int[] { 3, 24 },
                new byte[][]
                {
                    new byte[3] { 1, 2, 3 },
                    null
                },
                new bool[] { false, true },
                new bool[] { false, true }
            };
            yield return new object[]
            {
                // Same as before but after processing first message there are some leftover bytes
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Verack },
                    toReceiveBytes = new byte[][] { new byte[0] },
                },
                new MockNodeStatus(),
                new byte[][]
                {
                    new byte[3 + 1 + 2] { 255, 1, 2, 3, 255, 255 },
                    new byte[27 + 1 + 2] { 255, 0xf9, 0xbe, 0xb4, 0xd9, 0x76, 0x65, 0x72, 0x61, 0x63, 0x6b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5d, 0xf6, 0xe0, 0xe2, 4, 5, 6, 255, 255 }
                },
                1,
                new int[] { 3, 27 },
                new byte[][]
                {
                    new byte[3] { 1, 2, 3 },
                    new byte[3] { 4, 5, 6 },
                },
                new bool[] { false, false },
                new bool[] { false, false }
            };
            yield return new object[]
            {
                // 2 messages are received back to back with some garbage bytes in the middle
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Verack, PayloadType.Verack },
                    toReceiveBytes = new byte[][] { new byte[0], new byte[1] { 0x23 } },
                },
                new MockNodeStatus(),
                new byte[][]
                {
                    new byte[3 + 1 + 2] { 255, 1, 2, 3, 255, 255 },
                    new byte[27 + 1 + 2] { 255, 0xf9, 0xbe, 0xb4, 0xd9, 0x76, 0x65, 0x72, 0x61, 0x63, 0x6b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5d, 0xf6, 0xe0, 0xe2, 4, 5, 6, 255, 255 },
                    new byte[28 + 1 + 2] { 255, 0xf9, 0xbe, 0xb4, 0xd9, 0x76, 0x65, 0x72, 0x61, 0x63, 0x6b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x7c, 0x8b, 0x1e, 0xd7, 0x23, 7, 8, 9, 255, 255 }
                },
                1,
                new int[] { 3, 27, 28 },
                new byte[][]
                {
                    new byte[3] { 1, 2, 3 },
                    new byte[3] { 4, 5, 6 },
                    new byte[3] { 7, 8, 9 },
                },
                new bool[] { false, false, false },
                new bool[] { false, false, false }
            };
            yield return new object[]
            {
                // 2 messages in 1 buffer
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Verack, PayloadType.Verack },
                    toReceiveBytes = new byte[][] { new byte[0], new byte[1] { 0x23 } },
                },
                new MockNodeStatus(),
                new byte[][]
                {
                    Helper.HexToBytes("ffff" +
                    "f9beb4d976657261636b000000000000000000005df6e0e2"+"f9beb4d976657261636b000000000000010000007c8b1ed723" +
                    "ff")
                },
                2,
                new int[] { 49 },
                new byte[][] { null },
                new bool[] { true },
                new bool[] { false }
            };
            yield return new object[]
            {
                // Same as before but with some garbage bytes inserted
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Verack, PayloadType.Verack },
                    toReceiveBytes = new byte[][] { new byte[0], new byte[1] { 0x23 } },
                },
                new MockNodeStatus(),
                new byte[][]
                {
                    Helper.HexToBytes("ffff" +
                    "f9beb4d976657261636b000000000000000000005df6e0e2"+"010203"+
                    "f9beb4d976657261636b000000000000010000007c8b1ed723" + "0405" +
                    "ff")
                },
                2,
                new int[] { 54 },
                new byte[][] { new byte[] { 4, 5 } },
                new bool[] { false },
                new bool[] { false }
            };
            yield return new object[]
            {
                // Buffer is bigger than header size so message is evaluated but doesn't have enough bytes 
                // so there's more to receive
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Verack },
                    toReceiveBytes = new byte[][] { new byte[] { 0x11, 0x22 } }
                },
                new MockNodeStatus(),
                new byte[][] { Helper.HexToBytes("f9beb4d976657261636b0000000000000200000037f67ee111"), new byte[1] { 0x22 } },
                0,
                new int[] { 25, 1 },
                new byte[][] { Helper.HexToBytes("f9beb4d976657261636b0000000000000200000037f67ee111"), null },
                new bool[] { false, true },
                new bool[] { false, false }
            };
            yield return new object[]
            {
                // Same as before but with invalid checksum
                new MockReplyManager()
                {
                    toReceive = new PayloadType[] { PayloadType.Verack },
                    toReceiveBytes = new byte[][] { new byte[] { 0x11, 0x22 } }
                },
                new MockNodeStatus() { smallViolation = true },
                new byte[][] { Helper.HexToBytes("f9beb4d976657261636b0000000000000200000027f67ee111"), new byte[1] { 0x22 } },
                0,
                new int[] { 25, 1 },
                new byte[][] { Helper.HexToBytes("f9beb4d976657261636b0000000000000200000027f67ee111"), null },
                new bool[] { false, true },
                new bool[] { false, false }
            };
        }
        [Theory]
        [MemberData(nameof(GetReadBytesCases))]
        public void ReadBytesTest(IReplyManager repMan, MockNodeStatus ns, byte[][] buffers, int offset, int[] recvLens,
                                  byte[][] leftovers, bool[] rcvComplete, bool[] hasSend)
        {
            var cs = new MockClientSettings() { _netType = NetworkType.MainNet, _buffLen = 10 };
            var man = new MessageManager(cs, repMan, ns);

            Assert.Equal(buffers.Length, recvLens.Length);
            Assert.Equal(buffers.Length, leftovers.Length);
            Assert.Equal(buffers.Length, hasSend.Length);
            Assert.Equal(buffers.Length, rcvComplete.Length);

            for (int i = 0; i < buffers.Length; i++)
            {
                byte[] item = buffers[i];
                man.ReadBytes(item, offset, recvLens[i]);

                if (leftovers[i] == null)
                {
                    Helper.CheckNullPrivateField(man, "rcvHolder");
                }
                else
                {
                    Helper.ComparePrivateField(man, "rcvHolder", leftovers[i]);
                }

                Assert.Equal(rcvComplete[i], man.IsReceiveCompleted);
                Assert.Equal(hasSend[i], man.HasDataToSend);
            }

            // It is either not set and is false or set and checked and changed to false
            Assert.False(ns.smallViolation);
        }
    }
}
