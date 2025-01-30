// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;

namespace Tests.Bitcoin.P2PNetwork.Messages
{
    public class MessageTests
    {
        [Fact]
        public void ConstructorTest()
        {
            Message msg = new(NetworkType.MainNet);
            Helper.ComparePrivateField(msg, "networkMagic", Helper.HexToBytes(Constants.MainNetMagic));

            msg = new Message(NetworkType.TestNet3);
            Helper.ComparePrivateField(msg, "networkMagic", Helper.HexToBytes(Constants.TestNet3Magic));

            msg = new Message(NetworkType.TestNet4);
            Helper.ComparePrivateField(msg, "networkMagic", Helper.HexToBytes(Constants.TestNet4Magic));

            msg = new Message(NetworkType.RegTest);
            Helper.ComparePrivateField(msg, "networkMagic", Helper.HexToBytes(Constants.RegTestMagic));

            Assert.Throws<ArgumentException>(() => new Message((NetworkType)100));
        }

        [Fact]
        public void Constructor_WithPayloadTest()
        {
            MockSerializableMessagePayload pl = new(PayloadType.Addr, new byte[] { 1, 2, 3 });
            Message msg = new(pl, NetworkType.MainNet);

            Assert.Equal(new byte[] { 1, 2, 3 }, msg.PayloadData);
            Assert.Equal(new byte[12] { 0x61, 0x64, 0x64, 0x72, 0, 0, 0, 0, 0, 0, 0, 0 }, msg.PayloadName);
        }

        [Fact]
        public void Constructor_WithPayload_ExceptionTest()
        {
            MockSerializableMessagePayload pl = new(PayloadType.Addr, new byte[Constants.MaxPayloadSize + 1]);

            Assert.Throws<ArgumentNullException>(() => new Message(null, NetworkType.MainNet));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Message(pl, NetworkType.MainNet));
        }

        [Theory]
        [InlineData("f9beb4d96e6f74666f756e6400000000000000005df6e0e2", true, PayloadType.NotFound)]
        [InlineData("f9beb4d96e6f74666f756e6300000000000000005df6e0e2", false, PayloadType.Addr)]
        public void TryGetPayloadTypeTest(string hex, bool expSuccess, PayloadType expPlt)
        {
            Message msg = new(NetworkType.MainNet);
            Assert.True(msg.TryDeserialize(new FastStreamReader(Helper.HexToBytes(hex)), out Errors error), error.Convert());
            Assert.Equal(expSuccess, msg.TryGetPayloadType(out PayloadType actualPlt));
            Assert.Equal(expPlt, actualPlt);
        }

        [Fact]
        public void SerializeTest()
        {
            MockSerializableMessagePayload pl = new(PayloadType.Addr, new byte[] { 1, 2, 3 });
            Message msg = new(pl, NetworkType.MainNet);
            FastStream stream = new(Constants.MessageHeaderSize + 3);
            msg.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes($"{Constants.MainNetMagic}6164647200000000000000000300000019c6197e010203");

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetReadCases()
        {
            yield return new object[] { Array.Empty<byte>(), Message.ReadResult.NotEnoughBytes };
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
            Message msg = new(NetworkType.MainNet);
            Message.ReadResult actual = msg.Read(new FastStreamReader(data));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryDeserializeTest(byte[] data, Message.ReadResult readResult)
        {
            Message msg = new(NetworkType.MainNet);
            bool success = msg.TryDeserialize(new FastStreamReader(data), out Errors error);

            Errors expErr = readResult switch
            {
                Message.ReadResult.Success => Errors.None,
                Message.ReadResult.NotEnoughBytes => Errors.EndOfStream,
                Message.ReadResult.PayloadOverflow => Errors.MessagePayloadOverflow,
                Message.ReadResult.InvalidNetwork => Errors.InvalidMessageNetwork,
                Message.ReadResult.InvalidChecksum => Errors.InvalidMessageChecksum,
                _ => throw new ArgumentException("Undefined message result.")
            };
            bool expSuccess = readResult == Message.ReadResult.Success;

            Assert.Equal(expSuccess, success);
            Assert.Equal(expErr, error);
        }
    }
}
