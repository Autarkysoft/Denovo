// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Runtime.CompilerServices;

namespace Benchmarks
{
    // Static implementation of SHA256 to benchmark the difference in speed
    public static class StaticSha256
    {
        /// <summary>
        /// Size of the hash result in bytes (=32 bytes).
        /// </summary>
        public const int HashByteSize = 32;
        /// <summary>
        /// Size of the blocks used in each round (=64 bytes).
        /// </summary>
        public const int BlockByteSize = 64;

        public const int HashStateSize = 8;
        public const int WorkingVectorSize = 64;
        /// <summary>
        /// Size of UInt32[] buffer = 72
        /// </summary>
        public const int UBufferSize = HashStateSize + WorkingVectorSize;


        private static readonly uint[] Ks =
        {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
            0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
            0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
            0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
            0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
            0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
            0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
            0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
            0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };

        public static unsafe byte[] ComputeHash(Span<byte> data)
        {
            uint* pt = stackalloc uint[HashStateSize + WorkingVectorSize];
            Init(pt);
            fixed (byte* dPt = data)
            {
                CompressData(dPt, data.Length, data.Length, pt);
            }
            return GetBytes(pt);
        }

        public static unsafe byte[] ComputeHashTwice(Span<byte> data)
        {
            uint* pt = stackalloc uint[HashStateSize + WorkingVectorSize];
            Init(pt);
            fixed (byte* dPt = data)
            {
                CompressData(dPt, data.Length, data.Length, pt);
                DoSecondHash(pt);
            }
            return GetBytes(pt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Init(uint* hPt)
        {
            hPt[0] = 0x6a09e667;
            hPt[1] = 0xbb67ae85;
            hPt[2] = 0x3c6ef372;
            hPt[3] = 0xa54ff53a;
            hPt[4] = 0x510e527f;
            hPt[5] = 0x9b05688c;
            hPt[6] = 0x1f83d9ab;
            hPt[7] = 0x5be0cd19;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte[] GetBytes(uint* hPt)
        {
            return new byte[32]
            {
                (byte)(hPt[0] >> 24), (byte)(hPt[0] >> 16), (byte)(hPt[0] >> 8), (byte)hPt[0],
                (byte)(hPt[1] >> 24), (byte)(hPt[1] >> 16), (byte)(hPt[1] >> 8), (byte)hPt[1],
                (byte)(hPt[2] >> 24), (byte)(hPt[2] >> 16), (byte)(hPt[2] >> 8), (byte)hPt[2],
                (byte)(hPt[3] >> 24), (byte)(hPt[3] >> 16), (byte)(hPt[3] >> 8), (byte)hPt[3],
                (byte)(hPt[4] >> 24), (byte)(hPt[4] >> 16), (byte)(hPt[4] >> 8), (byte)hPt[4],
                (byte)(hPt[5] >> 24), (byte)(hPt[5] >> 16), (byte)(hPt[5] >> 8), (byte)hPt[5],
                (byte)(hPt[6] >> 24), (byte)(hPt[6] >> 16), (byte)(hPt[6] >> 8), (byte)hPt[6],
                (byte)(hPt[7] >> 24), (byte)(hPt[7] >> 16), (byte)(hPt[7] >> 8), (byte)hPt[7]
            };
        }

        internal static unsafe void DoSecondHash(uint* pt)
        {
            // Result of previous hash (hashState[]) is now our new block. So copy it here:
            pt[8] = pt[0];
            pt[9] = pt[1];
            pt[10] = pt[2];
            pt[11] = pt[3];
            pt[12] = pt[4];
            pt[13] = pt[5];
            pt[14] = pt[6];
            pt[15] = pt[7]; // 8*4 = 32 byte hash result

            pt[16] = 0b10000000_00000000_00000000_00000000U; // 1 followed by 0 bits to fill pad1
            pt[17] = 0;
            pt[18] = 0;
            pt[19] = 0;
            pt[20] = 0;
            pt[21] = 0;

            pt[22] = 0; // Message length for pad2, since message is the 32 byte result of previous hash, length is 256 bit
            pt[23] = 256;

            // Set the rest of working vector from 16 to 64
            pt[24] = SSIG0(pt[9]) + pt[8];
            pt[25] = 10485760 + SSIG0(pt[10]) + pt[9];
            pt[26] = SSIG1(pt[24]) + SSIG0(pt[11]) + pt[10];
            pt[27] = SSIG1(pt[25]) + SSIG0(pt[12]) + pt[11];
            pt[28] = SSIG1(pt[26]) + SSIG0(pt[13]) + pt[12];
            pt[29] = SSIG1(pt[27]) + SSIG0(pt[14]) + pt[13];
            pt[30] = SSIG1(pt[28]) + 256 + SSIG0(pt[15]) + pt[14];
            pt[31] = SSIG1(pt[29]) + pt[24] + 285220864 + pt[15];
            pt[32] = SSIG1(pt[30]) + pt[25] + 0b10000000_00000000_00000000_00000000U;
            pt[33] = SSIG1(pt[31]) + pt[26];
            pt[34] = SSIG1(pt[32]) + pt[27];
            pt[35] = SSIG1(pt[33]) + pt[28];
            pt[36] = SSIG1(pt[34]) + pt[29];
            pt[37] = SSIG1(pt[35]) + pt[30];
            pt[38] = SSIG1(pt[36]) + pt[31] + 4194338;
            pt[39] = SSIG1(pt[37]) + pt[32] + SSIG0(pt[24]) + 256;
            pt[40] = SSIG1(pt[38]) + pt[33] + SSIG0(pt[25]) + pt[24];
            pt[41] = SSIG1(pt[39]) + pt[34] + SSIG0(pt[26]) + pt[25];
            pt[42] = SSIG1(pt[40]) + pt[35] + SSIG0(pt[27]) + pt[26];
            pt[43] = SSIG1(pt[41]) + pt[36] + SSIG0(pt[28]) + pt[27];
            pt[44] = SSIG1(pt[42]) + pt[37] + SSIG0(pt[29]) + pt[28];
            pt[45] = SSIG1(pt[43]) + pt[38] + SSIG0(pt[30]) + pt[29];
            pt[46] = SSIG1(pt[44]) + pt[39] + SSIG0(pt[31]) + pt[30];
            pt[47] = SSIG1(pt[45]) + pt[40] + SSIG0(pt[32]) + pt[31];
            pt[48] = SSIG1(pt[46]) + pt[41] + SSIG0(pt[33]) + pt[32];
            pt[49] = SSIG1(pt[47]) + pt[42] + SSIG0(pt[34]) + pt[33];
            pt[50] = SSIG1(pt[48]) + pt[43] + SSIG0(pt[35]) + pt[34];
            pt[51] = SSIG1(pt[49]) + pt[44] + SSIG0(pt[36]) + pt[35];
            pt[52] = SSIG1(pt[50]) + pt[45] + SSIG0(pt[37]) + pt[36];
            pt[53] = SSIG1(pt[51]) + pt[46] + SSIG0(pt[38]) + pt[37];
            pt[54] = SSIG1(pt[52]) + pt[47] + SSIG0(pt[39]) + pt[38];
            pt[55] = SSIG1(pt[53]) + pt[48] + SSIG0(pt[40]) + pt[39];
            pt[56] = SSIG1(pt[54]) + pt[49] + SSIG0(pt[41]) + pt[40];
            pt[57] = SSIG1(pt[55]) + pt[50] + SSIG0(pt[42]) + pt[41];
            pt[58] = SSIG1(pt[56]) + pt[51] + SSIG0(pt[43]) + pt[42];
            pt[59] = SSIG1(pt[57]) + pt[52] + SSIG0(pt[44]) + pt[43];
            pt[60] = SSIG1(pt[58]) + pt[53] + SSIG0(pt[45]) + pt[44];
            pt[61] = SSIG1(pt[59]) + pt[54] + SSIG0(pt[46]) + pt[45];
            pt[62] = SSIG1(pt[60]) + pt[55] + SSIG0(pt[47]) + pt[46];
            pt[63] = SSIG1(pt[61]) + pt[56] + SSIG0(pt[48]) + pt[47];
            pt[64] = SSIG1(pt[62]) + pt[57] + SSIG0(pt[49]) + pt[48];
            pt[65] = SSIG1(pt[63]) + pt[58] + SSIG0(pt[50]) + pt[49];
            pt[66] = SSIG1(pt[64]) + pt[59] + SSIG0(pt[51]) + pt[50];
            pt[67] = SSIG1(pt[65]) + pt[60] + SSIG0(pt[52]) + pt[51];
            pt[68] = SSIG1(pt[66]) + pt[61] + SSIG0(pt[53]) + pt[52];
            pt[69] = SSIG1(pt[67]) + pt[62] + SSIG0(pt[54]) + pt[53];
            pt[70] = SSIG1(pt[68]) + pt[63] + SSIG0(pt[55]) + pt[54];
            pt[71] = SSIG1(pt[69]) + pt[64] + SSIG0(pt[56]) + pt[55];

            // Now initialize hashState to compute next round, since this is a new hash
            Init(pt);

            // We only have 1 block so there is no need for a loop.
            CompressBlockWithWSet(pt);
        }

        public static unsafe void CompressData(byte* dPt, int dataLen, int totalLen, uint* pt)
        {
            Span<byte> finalBlock = new byte[64];

            fixed (byte* fPt = &finalBlock[0])
            {
                uint* wPt = pt + HashStateSize;
                int dIndex = 0;
                while (dataLen >= BlockByteSize)
                {
                    for (int i = 0; i < 16; i++, dIndex += 4)
                    {
                        wPt[i] = (uint)((dPt[dIndex] << 24) | (dPt[dIndex + 1] << 16) | (dPt[dIndex + 2] << 8) | dPt[dIndex + 3]);
                    }
                    SetW(wPt);
                    CompressBlockWithWSet(pt);

                    dataLen -= BlockByteSize;
                }

                // Copy the reamaining bytes into a blockSize length buffer so that we can loop through it easily:
                Buffer.MemoryCopy(dPt + dIndex, fPt, finalBlock.Length, dataLen);

                // Append 1 bit followed by zeros. Since we only work with bytes, this is 1 whole byte
                fPt[dataLen] = 0b1000_0000;

                if (dataLen >= 56) // blockSize - pad2.Len = 64 - 8
                {
                    // This means we have an additional block to compress, which we do it here:

                    for (int i = 0, j = 0; i < 16; i++, j += 4)
                    {
                        wPt[i] = (uint)((fPt[j] << 24) | (fPt[j + 1] << 16) | (fPt[j + 2] << 8) | fPt[j + 3]);
                    }
                    SetW(wPt);
                    CompressBlockWithWSet(pt);

                    // Zero out all the items in FinalBlock so it can be reused
                    finalBlock.Clear();
                }

                // Add length in bits as the last 8 bytes of final block in big-endian order
                // See MessageLengthTest in Test project to understand what the following shifts are
                fPt[63] = (byte)(totalLen << 3);
                fPt[62] = (byte)(totalLen >> 5);
                fPt[61] = (byte)(totalLen >> 13);
                fPt[60] = (byte)(totalLen >> 21);
                fPt[59] = (byte)(totalLen >> 29);
                // The remainig 3 bytes are always zero
                // The remaining 56 bytes are already set

                for (int i = 0, j = 0; i < 16; i++, j += 4)
                {
                    wPt[i] = (uint)((fPt[j] << 24) | (fPt[j + 1] << 16) | (fPt[j + 2] << 8) | fPt[j + 3]);
                }
                SetW(wPt);
                CompressBlockWithWSet(pt);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetW(uint* wPt, int start = 16)
        {
            for (int i = start; i < WorkingVectorSize; i++)
            {
                wPt[i] = SSIG1(wPt[i - 2]) + wPt[i - 7] + SSIG0(wPt[i - 15]) + wPt[i - 16];
            }
        }

        public static unsafe void CompressBlockWithWSet(uint* pt)
        {
            uint a = pt[0];
            uint b = pt[1];
            uint c = pt[2];
            uint d = pt[3];
            uint e = pt[4];
            uint f = pt[5];
            uint g = pt[6];
            uint h = pt[7];

            uint temp, aa, bb, cc, dd, ee, ff, hh, gg;

            fixed (uint* kPt = &Ks[0])
            {
                for (int j = 0; j < 64;)
                {
                    temp = h + BSIG1(e) + CH(e, f, g) + kPt[j] + pt[HashStateSize + j];
                    ee = d + temp;
                    aa = temp + BSIG0(a) + MAJ(a, b, c);
                    j++;

                    temp = g + BSIG1(ee) + CH(ee, e, f) + kPt[j] + pt[HashStateSize + j];
                    ff = c + temp;
                    bb = temp + BSIG0(aa) + MAJ(aa, a, b);
                    j++;

                    temp = f + BSIG1(ff) + CH(ff, ee, e) + kPt[j] + pt[HashStateSize + j];
                    gg = b + temp;
                    cc = temp + BSIG0(bb) + MAJ(bb, aa, a);
                    j++;

                    temp = e + BSIG1(gg) + CH(gg, ff, ee) + kPt[j] + pt[HashStateSize + j];
                    hh = a + temp;
                    dd = temp + BSIG0(cc) + MAJ(cc, bb, aa);
                    j++;

                    temp = ee + BSIG1(hh) + CH(hh, gg, ff) + kPt[j] + pt[HashStateSize + j];
                    h = aa + temp;
                    d = temp + BSIG0(dd) + MAJ(dd, cc, bb);
                    j++;

                    temp = ff + BSIG1(h) + CH(h, hh, gg) + kPt[j] + pt[HashStateSize + j];
                    g = bb + temp;
                    c = temp + BSIG0(d) + MAJ(d, dd, cc);
                    j++;

                    temp = gg + BSIG1(g) + CH(g, h, hh) + kPt[j] + pt[HashStateSize + j];
                    f = cc + temp;
                    b = temp + BSIG0(c) + MAJ(c, d, dd);
                    j++;

                    temp = hh + BSIG1(f) + CH(f, g, h) + kPt[j] + pt[HashStateSize + j];
                    e = dd + temp;
                    a = temp + BSIG0(b) + MAJ(b, c, d);
                    j++;
                }
            }

            pt[0] += a;
            pt[1] += b;
            pt[2] += c;
            pt[3] += d;
            pt[4] += e;
            pt[5] += f;
            pt[6] += g;
            pt[7] += h;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint CH(uint x, uint y, uint z) => z ^ (x & (y ^ z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MAJ(uint x, uint y, uint z) => (x & y) | (z & (x | y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint BSIG0(uint x) => (x >> 2 | x << 30) ^ (x >> 13 | x << 19) ^ (x >> 22 | x << 10);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint BSIG1(uint x) => (x >> 6 | x << 26) ^ (x >> 11 | x << 21) ^ (x >> 25 | x << 7);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint SSIG0(uint x) => (x >> 7 | x << 25) ^ (x >> 18 | x << 14) ^ (x >> 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint SSIG1(uint x) => (x >> 17 | x << 15) ^ (x >> 19 | x << 13) ^ (x >> 10);
    }
}
