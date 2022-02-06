// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Blocks
{
    public class BlockHeaderTests
    {
        [Fact]
        public void Constructor_NullExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new BlockHeader(1, null, new byte[32], 123, 0x1d00ffffU, 0));
            Assert.Throws<ArgumentNullException>(() => new BlockHeader(1, new byte[32], null, 123, 0x1d00ffffU, 0));
        }

        public static IEnumerable<object[]> GetCtorOutOfRangeCases()
        {
            yield return new object[] { new byte[31], new byte[32] };
            yield return new object[] { new byte[32], new byte[33] };
        }
        [Theory]
        [MemberData(nameof(GetCtorOutOfRangeCases))]
        public void Constructor_OutOfRangeExceptionTest(byte[] header, byte[] merkle)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BlockHeader(1, header, merkle, 123, 0x1d00ffffU, 0));
        }


        // Block #622051
        internal static BlockHeader GetSampleBlockHeader()
        {
            return new BlockHeader()
            {
                Version = 0x3fffe000,
                PreviousBlockHeaderHash = Helper.HexToBytes("97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000"),
                MerkleRootHash = Helper.HexToBytes("afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7"),
                BlockTime = 0x5e71b1c6,
                NBits = 0x17110119,
                Nonce = 0x2a436a69
            };
        }

        internal static string GetSampleBlockHex() => "0000000000000000000d558fdcdde616702d1f91d6c8567a89be99ff9869012d";
        internal static byte[] GetSampleBlockHash() => Helper.HexToBytes(GetSampleBlockHex(), true);
        internal static byte[] GetSampleBlockHeaderBytes() => Helper.HexToBytes("00e0ff3f97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7c6b1715e19011117696a432a");


        [Fact]
        public void GetHashTest()
        {
            BlockHeader hdr = GetSampleBlockHeader();
            byte[] actualHash1 = hdr.GetHash();
            Assert.Equal(GetSampleBlockHash(), actualHash1);

            // Change any property to have a different hash
            hdr.Version++;

            byte[] actualHash2 = hdr.GetHash();
            // Hash doesn't change until explicitly asked
            Assert.Equal(actualHash2, actualHash1);

            byte[] actualHash3 = hdr.GetHash(true);
            Assert.NotEqual(actualHash3, actualHash1);
        }

        [Fact]
        public void GetHash_NullFieldTest()
        {
            BlockHeader hdr = GetSampleBlockHeader();
            // Requesting re-hash while the "hash" field is null
            byte[] actualHash1 = hdr.GetHash(true);
            Assert.Equal(GetSampleBlockHash(), actualHash1);
        }

        [Fact]
        public void GetIDTest()
        {
            BlockHeader hdr = GetSampleBlockHeader();
            string actual1 = hdr.GetID(false);
            // Call again to make sure the hash field is not changed
            string actual2 = hdr.GetID(false);
            string expected = GetSampleBlockHex();

            Assert.Equal(expected, actual1);
            Assert.Equal(expected, actual2);

            hdr.Version++;
            // Hash doesn't change until explicitly asked
            string actual3 = hdr.GetID(false);
            // Hash is re-computed and is changed
            string actual4 = hdr.GetID(true);

            Assert.Equal(expected, actual3);
            Assert.NotEqual(expected, actual4);
        }

        [Fact]
        public void AddSerializedSizeTest()
        {
            BlockHeader hd = new();
            SizeCounter counter = new();
            hd.AddSerializedSize(counter);
            Assert.Equal(BlockHeader.Size, counter.Size);
        }

        [Fact]
        public void SerializeTest()
        {
            BlockHeader hd = GetSampleBlockHeader();

            FastStream stream = new();
            hd.Serialize(stream);

            byte[] expected = GetSampleBlockHeaderBytes();

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, hd.Serialize());
        }

        [Fact]
        public void TryDeserializeTest()
        {
            BlockHeader hd = new();
            bool b = hd.TryDeserialize(new FastStreamReader(GetSampleBlockHeaderBytes()), out string error);
            BlockHeader expected = GetSampleBlockHeader();

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(expected.Version, hd.Version);
            Assert.Equal(expected.PreviousBlockHeaderHash, hd.PreviousBlockHeaderHash);
            Assert.Equal(expected.MerkleRootHash, hd.MerkleRootHash);
            Assert.Equal(expected.BlockTime, hd.BlockTime);
            Assert.Equal(expected.NBits, hd.NBits);
            Assert.Equal(expected.Nonce, hd.Nonce);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[]
            {
                new byte[BlockHeader.Size -1],
                Err.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTests(byte[] data, string expErr)
        {
            BlockHeader hd = new();
            bool b = hd.TryDeserialize(new FastStreamReader(data), out string error);

            Assert.False(b, error);
            Assert.Equal(expErr, error);
        }
    }
}
