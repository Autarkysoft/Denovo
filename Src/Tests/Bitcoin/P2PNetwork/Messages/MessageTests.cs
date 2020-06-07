// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages
{
    public class MessageTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var msg = new Message(NetworkType.MainNet);
            Helper.ComparePrivateField(msg, "networkMagic", Helper.HexToBytes(Constants.MainNetMagic));

            msg = new Message(NetworkType.TestNet);
            Helper.ComparePrivateField(msg, "networkMagic", Helper.HexToBytes(Constants.TestNetMagic));

            msg = new Message(NetworkType.RegTest);
            Helper.ComparePrivateField(msg, "networkMagic", Helper.HexToBytes(Constants.RegTestMagic));

            Assert.Throws<ArgumentException>(() => new Message((NetworkType)100));
        }

        [Fact]
        public void Constructor_WithPayloadTest()
        {
            var pl = new MockSerializableMessagePayload(PayloadType.Addr, new byte[] { 1, 2, 3 });
            var msg = new Message(pl, NetworkType.MainNet);

            Assert.Equal(new byte[] { 1, 2, 3 }, msg.PayloadData);
            Assert.Equal(new byte[12] { 0x61, 0x64, 0x64, 0x72, 0, 0, 0, 0, 0, 0, 0, 0 }, msg.PayloadName);
        }

        [Fact]
        public void Constructor_WithPayload_ExceptionTest()
        {
            var pl = new MockSerializableMessagePayload(PayloadType.Addr, new byte[Constants.MaxPayloadSize + 1]);

            Assert.Throws<ArgumentNullException>(() => new Message(null, NetworkType.MainNet));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Message(pl, NetworkType.MainNet));
        }

        [Fact]
        public void SerializeTest()
        {
            var pl = new MockSerializableMessagePayload(PayloadType.Addr, new byte[] { 1, 2, 3 });
            var msg = new Message(pl, NetworkType.MainNet);
            var stream = new FastStream(Constants.MessageHeaderSize + 3);
            msg.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes($"{Constants.MainNetMagic}6164647200000000000000000300000019c6197e010203");

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetReadCases()
        {
            yield return new object[] { new byte[0], Message.ReadResult.NotEnoughBytes };
            yield return new object[] { new byte[Constants.MessageHeaderSize - 1], Message.ReadResult.NotEnoughBytes };
            yield return new object[]
            {
                Helper.HexToBytes("f9beb4d976657261636b000000000000000000005df6e0e2"),
                Message.ReadResult.Success
            };
            yield return new object[]
            {
                Helper.HexToBytes("f0beb4d976657261636b000000000000000000005df6e0e2"),
                Message.ReadResult.InvalidNetwork
            };
            yield return new object[]
            {
                Helper.HexToBytes("f9beb4d976657261636b000000000000010000005df6e0e2"),
                Message.ReadResult.NotEnoughBytes
            };
            yield return new object[]
            {
                Helper.HexToBytes("f9beb4d976657261636b000000000000000000005df6e0e0"),
                Message.ReadResult.InvalidChecksum
            };
            yield return new object[]
            {
                Helper.HexToBytes("f9beb4d976657261636b000000000000010000005df6e0e201"),
                Message.ReadResult.InvalidChecksum
            };
            yield return new object[]
            {
                // This is a verak message with { 0x01 } as its payload and valid checksum (Message shouldn't care)
                Helper.HexToBytes("f9beb4d976657261636b000000000000010000009c12cfdc01"),
                Message.ReadResult.Success
            };
            yield return new object[]
            {
                Helper.HexToBytes("f9beb4d976657261636b00000000000001093d005df6e0e2"),
                Message.ReadResult.PayloadOverflow
            };
        }
        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void ReadTest(byte[] data, Message.ReadResult expected)
        {
            var msg = new Message(NetworkType.MainNet);
            Message.ReadResult actual = msg.Read(new FastStreamReader(data));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryDeserializeTest(byte[] data, Message.ReadResult readResult)
        {
            var msg = new Message(NetworkType.MainNet);
            bool success = msg.TryDeserialize(new FastStreamReader(data), out string error);

            string expErr = readResult switch
            {
                Message.ReadResult.Success => null,
                Message.ReadResult.NotEnoughBytes => Err.EndOfStream,
                Message.ReadResult.PayloadOverflow => "Payload size is bigger than allowed size (4000000).",
                Message.ReadResult.InvalidNetwork => "Invalid message magic.",
                Message.ReadResult.InvalidChecksum => "Invalid checksum",
                _ => throw new ArgumentException("Undefined message result.")
            };
            bool expSuccess = expErr is null;

            Assert.Equal(expSuccess, success);
            Assert.Equal(expErr, error);
        }
    }
}
