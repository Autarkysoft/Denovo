// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System.Collections.Generic;
using Tests.Bitcoin.Blockchain;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class BlockPayloadTests
    {
        [Fact]
        public void SerializeTest()
        {
            BlockPayload pl = new BlockPayload()
            {
                BlockData = new MockSerializableBlock(new byte[] { 1, 2, 3 })
            };

            FastStream stream = new FastStream(3);
            pl.Serialize(stream);
            byte[] actual = pl.Serialize();
            byte[] expected = new byte[] { 1, 2, 3 };

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            BlockPayload pl = new BlockPayload()
            {
                BlockData = new MockDeserializableBlock(0, 3)
            };
            FastStreamReader stream = new FastStreamReader(new byte[3]);
            bool b = pl.TryDeserialize(stream, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            // Mock block has its own tests.
            Assert.Equal(PayloadType.Block, pl.PayloadType);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[]
            {
                new FastStreamReader(new byte[1]),
                new MockDeserializableBlock(0, 1, "Foo"),
                "Foo"
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, MockDeserializableBlock block, string expErr)
        {
            BlockPayload pl = new BlockPayload()
            {
                BlockData = block
            };

            bool b = pl.TryDeserialize(stream, out string error);
            Assert.False(b);
            Assert.Equal(expErr, error);
            // Mock block has its own tests.
        }
    }
}
