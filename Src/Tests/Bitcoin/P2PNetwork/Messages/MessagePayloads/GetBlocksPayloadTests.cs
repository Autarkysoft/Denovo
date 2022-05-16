// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class GetBlocksPayloadTests
    {
        private const int MaxCount = 101;

        [Fact]
        public void Constructor_OutOfRangeExceptionTest()
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GetBlocksPayload(-1, new Digest256[1], Digest256.Zero));
            Assert.Contains("Version can not be negative.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GetBlocksPayload(1, new Digest256[MaxCount + 1], Digest256.Zero));
            Assert.Contains($"Only a maximum of {MaxCount} hashes are allowed.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => new GetBlocksPayload(1, null, Digest256.Zero));
            Assert.Contains("Hash list can not be null.", ex.Message);
        }

        private const int Version = 70001;
        private const string Header1 = "00000000000000001bd3146aa1555e10b23b63e6d484987237b575778a609fd3";
        private const string Header2 = "00000000000000000aea3be27cda4b71011c2b60fb8a2e0a113708d403643e5c";
        private const string PayloadHex = "71110100" + "02" + Header1 + Header2 +
            "0000000000000000000000000000000000000000000000000000000000000000";

        [Fact]
        public void SerializeTest()
        {
            Digest256 hd1 = new(Helper.HexToBytes(Header1));
            Digest256 hd2 = new(Helper.HexToBytes(Header2));
            GetBlocksPayload pl = new(Version, new Digest256[] { hd1, hd2 }, Digest256.Zero);
            FastStream stream = new(4 + 32 + 32 + 32);
            pl.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes(PayloadHex);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            GetBlocksPayload pl = new();
            FastStreamReader stream = new(Helper.HexToBytes(PayloadHex));
            bool b = pl.TryDeserialize(stream, out Errors error);

            Digest256 hd1 = new(Helper.HexToBytes(Header1));
            Digest256 hd2 = new(Helper.HexToBytes(Header2));

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(Version, pl.Version);
            Assert.Equal(new Digest256[] { hd1, hd2 }, pl.Hashes);
            Assert.Equal(Digest256.Zero, pl.StopHash);
            Assert.Equal(PayloadType.GetBlocks, pl.PayloadType);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(new byte[1]), Errors.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[4] { 255, 255, 255, 255 }), Errors.InvalidBlocksPayloadVersion
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[5] { 1, 0, 0, 0, 253 }),
                Errors.InvalidCompactInt
            };
            yield return new object[]
            {
                // Valid CompactInt but invalid when using FastStreamReader.TryReadSmallCompactInt() (it is too big)
                new FastStreamReader(new byte[9] { 1, 0, 0, 0, 0xfe, 0x00, 0x00, 0x01, 0x00 }),
                Errors.InvalidCompactInt
            };
            yield return new object[]
            {
                // count = 102
                new FastStreamReader(new byte[5] { 1, 0, 0, 0, 0x66 }),
                Errors.MsgBlocksHashCountOverflow
            };
            yield return new object[]
            {
                // Count = 501
                new FastStreamReader(new byte[7] { 1, 0, 0, 0, 0xfd, 0xf5, 0x01 }),
                Errors.MsgBlocksHashCountOverflow
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[5] { 1, 0, 0, 0, 1 }),
                Errors.EndOfStream
            };
            byte[] temp = new byte[38];
            temp[0] = 1;
            temp[4] = 1;
            yield return new object[]
            {
                new FastStreamReader(temp),
                Errors.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, Errors expErr)
        {
            GetBlocksPayload pl = new();

            bool b = pl.TryDeserialize(stream, out Errors error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
