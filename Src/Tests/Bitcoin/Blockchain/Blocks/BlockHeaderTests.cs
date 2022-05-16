// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Collections.Generic;
using Tests.Bitcoin.ValueTypesTests;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Blocks
{
    public class BlockHeaderTests
    {
        [Fact]
        public void ConstructorTest()
        {
            Span<byte> ba64 = Helper.GetBytes(64);
            Digest256 expPrvHash = new(ba64.Slice(0, 32));
            Digest256 expMerkle = new(ba64.Slice(32, 32));

            BlockHeader header = new(5, expPrvHash, expMerkle, 123, TargetTests.Example, 456);

            Assert.Equal(5, header.Version);
            Assert.Equal(expPrvHash, header.PreviousBlockHeaderHash);
            Assert.Equal(expMerkle, header.MerkleRootHash);
            Assert.Equal(123U, header.BlockTime);
            Assert.Equal(TargetTests.Example, (uint)header.NBits);
            Assert.Equal(456U, header.Nonce);
        }

        [Fact]
        public void Constructor_FromConsensusTest()
        {
            MockConsensus consensus = new() { _minVer = 7 };
            Digest256 expPrvHash = new(GetSampleBlockHash());
            Digest256 expMerkle = Digest256.One;

            BlockHeader header = new(consensus, GetSampleBlockHeader(), expMerkle, TargetTests.Example);

            Assert.Equal(7, header.Version);
            Assert.Equal(expPrvHash, header.PreviousBlockHeaderHash);
            Assert.Equal(expMerkle, header.MerkleRootHash);
            Assert.True(Math.Abs(header.BlockTime - (uint)UnixTimeStamp.GetEpochUtcNow()) < 5);
            Assert.Equal(TargetTests.Example, (uint)header.NBits);
            Assert.Equal(0U, header.Nonce);
        }

        [Fact]
        public void Constructor_NullReferenceExceptionTest()
        {
            IConsensus c = null;
            Target tar = TargetTests.Example;
            Assert.Throws<NullReferenceException>(() => new BlockHeader(c, GetSampleBlockHeader(), Digest256.One, tar));
        }



        // Block #622051
        internal static BlockHeader GetSampleBlockHeader()
        {
            Digest256 prv = new(Helper.HexToBytes("97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000"));
            Digest256 mrkl = new(Helper.HexToBytes("afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7"));
            return new BlockHeader(0x3fffe000, prv, mrkl, 0x5e71b1c6, 0x17110119, 0x2a436a69);
        }

        internal static string GetSampleBlockHex() => "0000000000000000000d558fdcdde616702d1f91d6c8567a89be99ff9869012d";
        internal static byte[] GetSampleBlockHash() => Helper.HexToBytes(GetSampleBlockHex(), true);
        internal static byte[] GetSampleBlockHeaderBytes() => Helper.HexToBytes("00e0ff3f97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7c6b1715e19011117696a432a");


        [Fact]
        public void HashTest()
        {
            BlockHeader hdr = GetSampleBlockHeader();
            Assert.Equal(GetSampleBlockHash(), hdr.Hash.ToByteArray());
        }

        [Fact]
        public void GetIDTest()
        {
            BlockHeader hdr = GetSampleBlockHeader();
            string actual = hdr.GetID();
            string expected = GetSampleBlockHex();
            Assert.Equal(expected, actual);
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
            FastStreamReader stream = new(GetSampleBlockHeaderBytes());
            bool b = BlockHeader.TryDeserialize(stream, out BlockHeader hd, out Errors error);
            BlockHeader expected = GetSampleBlockHeader();

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expected.Version, hd.Version);
            Assert.Equal(expected.PreviousBlockHeaderHash, hd.PreviousBlockHeaderHash);
            Assert.Equal(expected.MerkleRootHash, hd.MerkleRootHash);
            Assert.Equal(expected.BlockTime, hd.BlockTime);
            Assert.Equal(expected.NBits, hd.NBits);
            Assert.Equal(expected.Nonce, hd.Nonce);
            Assert.Equal(GetSampleBlockHash(), hd.Hash.ToByteArray());
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[]
            {
                new byte[BlockHeader.Size -1],
                Errors.EndOfStream
            };
            yield return new object[]
            {
                Helper.HexToBytes("00e0ff3f97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7c6b1715e01008004696a432a"),
                Errors.NegativeTarget
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTests(byte[] data, Errors expErr)
        {
            bool b = BlockHeader.TryDeserialize(new FastStreamReader(data), out _, out Errors error);

            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
