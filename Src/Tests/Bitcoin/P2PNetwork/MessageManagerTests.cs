// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Autarkysoft.Bitcoin.P2PNetwork;
using System.Net.Sockets;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MessageManagerTests
    {
        [Fact]
        public void ConstructorTest()
        {
            MessageManager man = new MessageManager(20, null, NetworkType.MainNet);

            Assert.False(man.IsHandShakeComplete);
            Assert.True(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);
            Assert.Null(man.DataToSend);
        }

        [Fact]
        public void DataToSendTest()
        {
            MessageManager man = new MessageManager(20, null, NetworkType.MainNet)
            {
                DataToSend = null
            };
            Assert.False(man.IsHandShakeComplete);
            Assert.True(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);
            Helper.ComparePrivateField(man, "sendOffset", 0);
            Helper.ComparePrivateField(man, "remainSend", 0);

            man.DataToSend = new byte[0];
            Assert.False(man.IsHandShakeComplete);
            Assert.True(man.IsReceiveCompleted);
            Assert.False(man.HasDataToSend);
            Helper.ComparePrivateField(man, "sendOffset", 0);
            Helper.ComparePrivateField(man, "remainSend", 0);

            man.DataToSend = new byte[3] { 1, 2, 3 };
            Assert.False(man.IsHandShakeComplete);
            Assert.True(man.IsReceiveCompleted);
            Assert.True(man.HasDataToSend);
            Helper.ComparePrivateField(man, "sendOffset", 0);
            Helper.ComparePrivateField(man, "remainSend", 3);
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
            MessageManager man = new MessageManager(buffLen, null, NetworkType.MainNet)
            {
                DataToSend = toSend
            };
            using SocketAsyncEventArgs sarg = new SocketAsyncEventArgs();
            sarg.SetBuffer(new byte[buffLen], 0, buffLen);

            Assert.True(man.HasDataToSend);

            man.SetSendBuffer(sarg);

            Assert.Equal(sendLen1, sarg.Count);
            Assert.Equal(0, sarg.Offset);
            Assert.Equal(expecBuffer1, sarg.Buffer);
            Helper.ComparePrivateField(man, "sendOffset", sendLen1);

            if (expecBuffer2 == null)
            {
                Assert.False(man.HasDataToSend);
                Helper.ComparePrivateField(man, "remainSend", 0);
            }
            else
            {
                Assert.True(man.HasDataToSend);
                Helper.ComparePrivateField(man, "remainSend", sendLen2);

                man.SetSendBuffer(sarg);

                Assert.False(man.HasDataToSend);
                Assert.Equal(sendLen2, sarg.Count);
                Assert.Equal(0, sarg.Offset);
                Assert.Equal(expecBuffer2, sarg.Buffer);
                Helper.ComparePrivateField(man, "sendOffset", toSend.Length);
                Helper.ComparePrivateField(man, "remainSend", 0);
            }
        }

        [Fact]
        public void StartHandShakeTest()
        {
            var pl = new MockSerializableMessagePayload(PayloadType.Version, new byte[3] { 1, 2, 3 });
            Message msg = new Message(pl, NetworkType.MainNet);
            byte[] msgSer = Helper.HexToBytes("f9beb4d976657273696f6e00000000000300000019c6197e010203");
            MessageManager man = new MessageManager(30, msg, NetworkType.MainNet);
            using SocketAsyncEventArgs sarg = new SocketAsyncEventArgs();
            sarg.SetBuffer(new byte[30], 0, 30);

            byte[] expBuffer = new byte[30];
            Buffer.BlockCopy(msgSer, 0, expBuffer, 0, msgSer.Length);

            man.StartHandShake(sarg);

            Assert.Equal(expBuffer, sarg.Buffer);
            Assert.Equal(msgSer.Length, sarg.Count);
            Assert.Equal(0, sarg.Offset);
            Assert.Equal(msgSer, man.DataToSend);
            Assert.True(man.IsHandShakeComplete);
            Assert.False(man.HasDataToSend);
            Assert.True(man.IsReceiveCompleted);
        }

        [Fact]
        public void ReadBytesTest()
        {
            MessageManager man = new MessageManager(30, null, NetworkType.MainNet);
            string hex = "f9beb4d976657273696f6e0000000000650000005f1a69d2" +
                "721101000100000000000000bc8f5e5400000000010000000000000000000000000000000000ffffc61b6409208d" +
                "010000000000000000000000000000000000ffffcb0071c0208d128035cbc97953f80f2f5361746f7368693a302e" +
                "392e332fcf05050001";
            byte[] msgBa = Helper.HexToBytes(hex);

            Assert.False(man.HasDataToSend);
            man.ReadBytes(msgBa, msgBa.Length);
            Assert.True(man.HasDataToSend);
        }
    }
}
