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
    public class GetBlocksPayloadTests
    {
        private const int MaxCount = 101;

        [Fact]
        public void Constructor_OutOfRangeExceptionTest()
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GetBlocksPayload(-1, new byte[1][], new byte[32]));
            Assert.Contains("Version can not be negative.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GetBlocksPayload(1, new byte[MaxCount + 1][], new byte[32]));
            Assert.Contains($"Only a maximum of {MaxCount} hashes are allowed.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GetBlocksPayload(1, new byte[1][], new byte[33]));
            Assert.Contains("Stop hash length must be 32 bytes.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => new GetBlocksPayload(1, null, new byte[32]));
            Assert.Contains("Hash list can not be null.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => new GetBlocksPayload(1, new byte[1][], null));
            Assert.Contains("Stop hash can not be null.", ex.Message);
        }

        private const int Version = 70001;
        private const string Header1 = "00000000000000001bd3146aa1555e10b23b63e6d484987237b575778a609fd3";
        private const string Header2 = "00000000000000000aea3be27cda4b71011c2b60fb8a2e0a113708d403643e5c";
        private const string PayloadHex = "71110100" + "02" + Header1 + Header2 +
            "0000000000000000000000000000000000000000000000000000000000000000";

        [Fact]
        public void SerializeTest()
        {
            byte[] hd1 = Helper.HexToBytes(Header1);
            byte[] hd2 = Helper.HexToBytes(Header2);
            GetBlocksPayload pl = new GetBlocksPayload(Version, new byte[][] { hd1, hd2 }, new byte[32]);
            FastStream stream = new FastStream(4 + 32 + 32 + 32);
            pl.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes(PayloadHex);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            GetBlocksPayload pl = new GetBlocksPayload();
            FastStreamReader stream = new FastStreamReader(Helper.HexToBytes(PayloadHex));
            bool b = pl.TryDeserialize(stream, out string error);

            byte[] hd1 = Helper.HexToBytes(Header1);
            byte[] hd2 = Helper.HexToBytes(Header2);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(Version, pl.Version);
            Assert.Equal(new byte[][] { hd1, hd2 }, pl.Hashes);
            Assert.Equal(new byte[32], pl.StopHash);
            Assert.Equal(PayloadType.GetBlocks, pl.PayloadType);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, "Stream can not be null." };
            yield return new object[] { new FastStreamReader(new byte[1]), Err.EndOfStream };
            yield return new object[] { new FastStreamReader(new byte[4] { 255, 255, 255, 255 }), "Invalid version" };
            yield return new object[]
            {
                new FastStreamReader(new byte[5] { 1, 0, 0, 0, 253 }),
                "First byte 253 needs to be followed by at least 2 byte."
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[5] { 1, 0, 0, 0, 0x66 }),
                $"Only {MaxCount} hashes are accepted."
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[7] { 1, 0, 0, 0, 0xfd, 0xf5, 0x01 }),
                $"Only {MaxCount} hashes are accepted."
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[5] { 1, 0, 0, 0, 1 }),
                Err.EndOfStream
            };
            byte[] temp = new byte[38];
            temp[0] = 1;
            temp[4] = 1;
            yield return new object[]
            {
                new FastStreamReader(temp),
                Err.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, string expErr)
        {
            GetBlocksPayload pl = new GetBlocksPayload();

            bool b = pl.TryDeserialize(stream, out string error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
