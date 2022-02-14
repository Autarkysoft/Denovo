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
            TxPayload pl = new()
            {
                Tx = new MockSerializableTx(new byte[] { 1, 2, 3 })
            };

            FastStream stream = new();
            pl.Serialize(stream);
            byte[] actual = pl.Serialize();
            byte[] expected = new byte[] { 1, 2, 3 };

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            TxPayload pl = new()
            {
                Tx = new MockDeserializableTx(0, 3)
            };
            FastStreamReader stream = new(new byte[3]);
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            // Mock tx has its own tests.
            Assert.Equal(PayloadType.Tx, pl.PayloadType);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, null, Errors.NullStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[1]),
                new MockDeserializableTx(0, 1, true),
                Errors.ForTesting
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, MockDeserializableTx tx, Errors expErr)
        {
            TxPayload pl = new()
            {
                Tx = tx
            };

            bool b = pl.TryDeserialize(stream, out Errors error);
            Assert.False(b);
            Assert.Equal(expErr, error);
            // Mock tx has its own tests.
        }
    }
}
