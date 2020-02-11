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
    public class TxPayloadTests
    {
        [Fact]
        public void SerializeTest()
        {
            TxPayload pl = new TxPayload()
            {
                Tx = new MockSerializableTx(new byte[] { 1, 2, 3 })
            };

            FastStream stream = new FastStream();
            pl.Serialize(stream);
            byte[] actual = pl.Serialize();
            byte[] expected = new byte[] { 1, 2, 3 };

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            TxPayload pl = new TxPayload()
            {
                Tx = new MockDeserializableTx(0, 3)
            };
            FastStreamReader stream = new FastStreamReader(new byte[3]);
            bool b = pl.TryDeserialize(stream, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            // Mock tx has its own tests.
            Assert.Equal(PayloadType.Tx, pl.PayloadType);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, null, "Stream can not be null." };
            yield return new object[]
            {
                new FastStreamReader(new byte[1]),
                new MockDeserializableTx(0, 1, "Foo"),
                "Foo"
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, MockDeserializableTx tx, string expErr)
        {
            TxPayload pl = new TxPayload()
            {
                Tx = tx
            };

            bool b = pl.TryDeserialize(stream, out string error);
            Assert.False(b);
            Assert.Equal(expErr, error);
            // Mock tx has its own tests.
        }
    }
}
