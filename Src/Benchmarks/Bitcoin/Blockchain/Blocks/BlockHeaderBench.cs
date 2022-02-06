// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using BenchmarkDotNet.Attributes;
using System;

namespace Benchmarks.Bitcoin.Blockchain.Blocks
{
    [InProcess]
    [RankColumn]
    [MemoryDiagnoser]
    public class BlockHeaderBench
    {
        public BlockHeaderBench()
        {
            header = new BlockHeader()
            {
                Version = 0x3fffe000,
                PreviousBlockHeaderHash = Base16.Decode("97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000"),
                MerkleRootHash = Base16.Decode("afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7"),
                BlockTime = 0x5e71b1c6,
                NBits = 0x17110119,
                Nonce = 0x2a436a69
            };

            ReadOnlySpan<byte> expected = Base16.DecodeReverse("0000000000000000000d558fdcdde616702d1f91d6c8567a89be99ff9869012d");
            byte[] actual1 = header.GetHash(true);
            byte[] actual2 = GetHashStatic();

            if (!expected.SequenceEqual(actual1) || !expected.SequenceEqual(actual2))
            {
                throw new ArgumentException("Hashes are not equal");
            }

        }

        private readonly BlockHeader header;


        [Benchmark(Baseline = true)]
        public byte[] GetHash_Normal()
        {
            byte[] bytesToHash = header.Serialize();
            using Sha256 hashFunc = new();
            return hashFunc.ComputeHashTwice(bytesToHash);
        }

        [Benchmark]
        public byte[] GetHash_Optimized() => header.GetHash(true);

        [Benchmark]
        public byte[] GetHash_StaticSha()
        {
            return GetHashStatic();
        }

        private unsafe byte[] GetHashStatic()
        {
            uint* pt = stackalloc uint[StaticSha256.UBufferSize];
            uint* wPt = pt + StaticSha256.HashStateSize;
            fixed (byte* prvBlkH = &header.PreviousBlockHeaderHash[0], mrkl = &header.MerkleRootHash[0])
            {
                StaticSha256.Init(pt);
                wPt[0] = (uint)header.Version.SwapEndian();
                // 32 byte previous block header hash
                wPt[1] = (uint)(prvBlkH[0] << 24 | prvBlkH[1] << 16 | prvBlkH[2] << 8 | prvBlkH[3]);
                wPt[2] = (uint)(prvBlkH[4] << 24 | prvBlkH[5] << 16 | prvBlkH[6] << 8 | prvBlkH[7]);
                wPt[3] = (uint)(prvBlkH[8] << 24 | prvBlkH[9] << 16 | prvBlkH[10] << 8 | prvBlkH[11]);
                wPt[4] = (uint)(prvBlkH[12] << 24 | prvBlkH[13] << 16 | prvBlkH[14] << 8 | prvBlkH[15]);
                wPt[5] = (uint)(prvBlkH[16] << 24 | prvBlkH[17] << 16 | prvBlkH[18] << 8 | prvBlkH[19]);
                wPt[6] = (uint)(prvBlkH[20] << 24 | prvBlkH[21] << 16 | prvBlkH[22] << 8 | prvBlkH[23]);
                wPt[7] = (uint)(prvBlkH[24] << 24 | prvBlkH[25] << 16 | prvBlkH[26] << 8 | prvBlkH[27]);
                wPt[8] = (uint)(prvBlkH[28] << 24 | prvBlkH[29] << 16 | prvBlkH[30] << 8 | prvBlkH[31]);
                // 28 (of 32) byte MerkleRoot hash
                wPt[9] = (uint)(mrkl[0] << 24 | mrkl[1] << 16 | mrkl[2] << 8 | mrkl[3]);
                wPt[10] = (uint)(mrkl[4] << 24 | mrkl[5] << 16 | mrkl[6] << 8 | mrkl[7]);
                wPt[11] = (uint)(mrkl[8] << 24 | mrkl[9] << 16 | mrkl[10] << 8 | mrkl[11]);
                wPt[12] = (uint)(mrkl[12] << 24 | mrkl[13] << 16 | mrkl[14] << 8 | mrkl[15]);
                wPt[13] = (uint)(mrkl[16] << 24 | mrkl[17] << 16 | mrkl[18] << 8 | mrkl[19]);
                wPt[14] = (uint)(mrkl[20] << 24 | mrkl[21] << 16 | mrkl[22] << 8 | mrkl[23]);
                wPt[15] = (uint)(mrkl[24] << 24 | mrkl[25] << 16 | mrkl[26] << 8 | mrkl[27]);
                StaticSha256.SetW(wPt);
                StaticSha256.CompressBlockWithWSet(pt);

                // 4 (of 32) byte MerkleRoot hash
                wPt[0] = (uint)(mrkl[28] << 24 | mrkl[29] << 16 | mrkl[30] << 8 | mrkl[31]);
                wPt[1] = header.BlockTime.SwapEndian();
                wPt[2] = ((uint)header.NBits).SwapEndian();
                wPt[3] = header.Nonce.SwapEndian();
                wPt[4] = 0b10000000_00000000_00000000_00000000U;
                wPt[5] = 0;
                wPt[6] = 0;
                wPt[7] = 0;
                wPt[8] = 0;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                wPt[14] = 0;
                wPt[15] = 640;
                StaticSha256.SetW(wPt);
                StaticSha256.CompressBlockWithWSet(pt);

                StaticSha256.DoSecondHash(pt);
                return StaticSha256.GetBytes(pt);
            }
        }
    }
}
