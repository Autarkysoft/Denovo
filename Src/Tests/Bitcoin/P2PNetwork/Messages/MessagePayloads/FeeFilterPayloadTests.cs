// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class FeeFilterPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            FeeFilterPayload pl = new(123);
            Assert.Equal(123UL, pl.FeeRate);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FeeFilterPayload((Constants.TotalSupply * 1000) + 1));
        }

        [Fact]
        public void FeePerByteTest()
        {
            FeeFilterPayload pl = new(26993);
            Assert.Equal(27UL, pl.FeeRatePerByte);

            pl.FeeRate = 26111;
            Assert.Equal(27UL, pl.FeeRatePerByte);
        }

        [Fact]
        public void SerializeTest()
        {
            FeeFilterPayload pl = new(48_508);
            FastStream stream = new(8);
            pl.Serialize(stream);
            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes("7cbd000000000000");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            FeeFilterPayload pl = new();
            FastStreamReader stream = new(new byte[8] { 0x7c, 0xbd, 0, 0, 0, 0, 0, 0 });
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(48_508UL, pl.FeeRate);
            Assert.Equal(PayloadType.FeeFilter, pl.PayloadType);
        }

        [Fact]
        public void TryDeserialize_EdgeTest()
        {
            FeeFilterPayload pl = new();
            FastStreamReader stream = new(new byte[8] { 0x7c, 0xbd, 0, 0, 0, 0, 0, 0 });
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(48_508UL, pl.FeeRate);
            Assert.Equal(PayloadType.FeeFilter, pl.PayloadType);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(new byte[1]), Errors.EndOfStream };
            yield return new object[]
            {
                // Fee is 21 million + 1 (no *1000)
                new FastStreamReader(new byte[8] { 0x01, 0x40, 0x07, 0x5a, 0xf0, 0x75, 0x07, 0x00 }),
                Errors.MsgFeeRateFilterOverflow
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, Errors expErr)
        {
            FeeFilterPayload pl = new();

            bool b = pl.TryDeserialize(stream, out Errors error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
