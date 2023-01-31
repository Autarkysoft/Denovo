﻿// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Runtime.CompilerServices;

namespace Benchmarks
{
    /// <summary>
    /// Implementation of 512-bit Secure Hash Algorithm (SHA) based on RFC-6234.
    /// <para/>Implements <see cref="IDisposable"/>.
    /// <para/>https://tools.ietf.org/html/rfc6234
    /// </summary>
    public sealed class Sha512Instance : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sha512Instance"/>.
        /// </summary>
        public Sha512Instance() { }


        /// <summary>
        /// Size of the hash result in bytes (=64 bytes).
        /// </summary>
        public const int HashByteSize = 64;
        /// <summary>
        /// Size of the blocks used in each round (=128 bytes).
        /// </summary>
        public const int BlockByteSize = 128;


        internal ulong[] hashState = new ulong[8];
        internal ulong[] w = new ulong[80];

        private static readonly ulong[] Ks =
        {
            0x428a2f98d728ae22, 0x7137449123ef65cd, 0xb5c0fbcfec4d3b2f, 0xe9b5dba58189dbbc,
            0x3956c25bf348b538, 0x59f111f1b605d019, 0x923f82a4af194f9b, 0xab1c5ed5da6d8118,
            0xd807aa98a3030242, 0x12835b0145706fbe, 0x243185be4ee4b28c, 0x550c7dc3d5ffb4e2,
            0x72be5d74f27b896f, 0x80deb1fe3b1696b1, 0x9bdc06a725c71235, 0xc19bf174cf692694,
            0xe49b69c19ef14ad2, 0xefbe4786384f25e3, 0x0fc19dc68b8cd5b5, 0x240ca1cc77ac9c65,
            0x2de92c6f592b0275, 0x4a7484aa6ea6e483, 0x5cb0a9dcbd41fbd4, 0x76f988da831153b5,
            0x983e5152ee66dfab, 0xa831c66d2db43210, 0xb00327c898fb213f, 0xbf597fc7beef0ee4,
            0xc6e00bf33da88fc2, 0xd5a79147930aa725, 0x06ca6351e003826f, 0x142929670a0e6e70,
            0x27b70a8546d22ffc, 0x2e1b21385c26c926, 0x4d2c6dfc5ac42aed, 0x53380d139d95b3df,
            0x650a73548baf63de, 0x766a0abb3c77b2a8, 0x81c2c92e47edaee6, 0x92722c851482353b,
            0xa2bfe8a14cf10364, 0xa81a664bbc423001, 0xc24b8b70d0f89791, 0xc76c51a30654be30,
            0xd192e819d6ef5218, 0xd69906245565a910, 0xf40e35855771202a, 0x106aa07032bbd1b8,
            0x19a4c116b8d2d0c8, 0x1e376c085141ab53, 0x2748774cdf8eeb99, 0x34b0bcb5e19b48a8,
            0x391c0cb3c5c95a63, 0x4ed8aa4ae3418acb, 0x5b9cca4f7763e373, 0x682e6ff3d6b2b8a3,
            0x748f82ee5defb2fc, 0x78a5636f43172f60, 0x84c87814a1f0ab72, 0x8cc702081a6439ec,
            0x90befffa23631e28, 0xa4506cebde82bde9, 0xbef9a3f7b2c67915, 0xc67178f2e372532b,
            0xca273eceea26619c, 0xd186b8c721c0c207, 0xeada7dd6cde0eb1e, 0xf57d4f7fee6ed178,
            0x06f067aa72176fba, 0x0a637dc5a2c898a6, 0x113f9804bef90dae, 0x1b710b35131c471b,
            0x28db77f523047d84, 0x32caab7b40c72493, 0x3c9ebe0a15c9bebc, 0x431d67c49c100d4c,
            0x4cc5d4becb3e42b6, 0x597f299cfc657e2a, 0x5fcb6fab3ad6faec, 0x6c44198c4a475817,
        };


        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">The byte array to compute hash for</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHash(ReadOnlySpan<byte> data)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");

            fixed (byte* dPt = data)
            fixed (ulong* hPt = &hashState[0], wPt = &w[0])
            {
                Init(hPt);
                CompressData(dPt, data.Length, data.Length, hPt, wPt);

                return GetBytes(hPt);
            }
        }


        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="buffer">The byte array to compute hash for</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHash(ReadOnlySpan<byte> buffer, int offset, int count)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Instance was disposed.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Data can not be null.");
            if (offset < 0 || count < 0)
                throw new IndexOutOfRangeException("Offset or count can not be negative.");
            if (buffer.Length != 0 && offset > buffer.Length - 1 || buffer.Length == 0 && offset != 0)
                throw new IndexOutOfRangeException("Index can not be bigger than array length.");
            if (count > buffer.Length - offset)
                throw new IndexOutOfRangeException("Array is not long enough.");

            fixed (byte* dPt = &buffer[offset])
            fixed (ulong* hPt = &hashState[0], wPt = &w[0])
            {
                Init(hPt);
                CompressData(dPt, count, count, hPt, wPt);

                return GetBytes(hPt);
            }
        }


        /// <summary>
        /// Computes the hash value for the specified byte array twice (hash of hash).
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">The byte array to compute hash for</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHashTwice(ReadOnlySpan<byte> data)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");

            fixed (byte* dPt = data)
            fixed (ulong* hPt = &hashState[0], wPt = &w[0])
            {
                Init(hPt);
                CompressData(dPt, data.Length, data.Length, hPt, wPt);
                ComputeSecondHash(hPt, wPt);

                return GetBytes(hPt);
            }
        }



        internal unsafe void Init()
        {
            fixed (ulong* hPt = &hashState[0])
            {
                Init(hPt);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Init(ulong* hPt)
        {
            hPt[0] = 0x6a09e667f3bcc908;
            hPt[1] = 0xbb67ae8584caa73b;
            hPt[2] = 0x3c6ef372fe94f82b;
            hPt[3] = 0xa54ff53a5f1d36f1;
            hPt[4] = 0x510e527fade682d1;
            hPt[5] = 0x9b05688c2b3e6c1f;
            hPt[6] = 0x1f83d9abfb41bd6b;
            hPt[7] = 0x5be0cd19137e2179;
        }


        internal unsafe byte[] GetBytes()
        {
            fixed (ulong* hPt = &hashState[0])
                return GetBytes(hPt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe byte[] GetBytes(ulong* hPt)
        {
            return new byte[HashByteSize]
            {
                (byte)(hPt[0] >> 56), (byte)(hPt[0] >> 48), (byte)(hPt[0] >> 40), (byte)(hPt[0] >> 32),
                (byte)(hPt[0] >> 24), (byte)(hPt[0] >> 16), (byte)(hPt[0] >> 8), (byte)hPt[0],

                (byte)(hPt[1] >> 56), (byte)(hPt[1] >> 48), (byte)(hPt[1] >> 40), (byte)(hPt[1] >> 32),
                (byte)(hPt[1] >> 24), (byte)(hPt[1] >> 16), (byte)(hPt[1] >> 8), (byte)hPt[1],

                (byte)(hPt[2] >> 56), (byte)(hPt[2] >> 48), (byte)(hPt[2] >> 40), (byte)(hPt[2] >> 32),
                (byte)(hPt[2] >> 24), (byte)(hPt[2] >> 16), (byte)(hPt[2] >> 8), (byte)hPt[2],

                (byte)(hPt[3] >> 56), (byte)(hPt[3] >> 48), (byte)(hPt[3] >> 40), (byte)(hPt[3] >> 32),
                (byte)(hPt[3] >> 24), (byte)(hPt[3] >> 16), (byte)(hPt[3] >> 8), (byte)hPt[3],

                (byte)(hPt[4] >> 56), (byte)(hPt[4] >> 48), (byte)(hPt[4] >> 40), (byte)(hPt[4] >> 32),
                (byte)(hPt[4] >> 24), (byte)(hPt[4] >> 16), (byte)(hPt[4] >> 8), (byte)hPt[4],

                (byte)(hPt[5] >> 56), (byte)(hPt[5] >> 48), (byte)(hPt[5] >> 40), (byte)(hPt[5] >> 32),
                (byte)(hPt[5] >> 24), (byte)(hPt[5] >> 16), (byte)(hPt[5] >> 8), (byte)hPt[5],

                (byte)(hPt[6] >> 56), (byte)(hPt[6] >> 48), (byte)(hPt[6] >> 40), (byte)(hPt[6] >> 32),
                (byte)(hPt[6] >> 24), (byte)(hPt[6] >> 16), (byte)(hPt[6] >> 8), (byte)hPt[6],

                (byte)(hPt[7] >> 56), (byte)(hPt[7] >> 48), (byte)(hPt[7] >> 40), (byte)(hPt[7] >> 32),
                (byte)(hPt[7] >> 24), (byte)(hPt[7] >> 16), (byte)(hPt[7] >> 8), (byte)hPt[7]
            };
        }


        internal unsafe void CompressData(byte* dPt, int dataLen, int totalLen, ulong* hPt, ulong* wPt)
        {
            Span<byte> finalBlock = stackalloc byte[BlockByteSize];

            fixed (byte* fPt = &finalBlock[0])
            {
                int dIndex = 0;
                while (dataLen >= BlockByteSize)
                {
                    for (int i = 0; i < 16; i++, dIndex += 8)
                    {
                        wPt[i] =
                                ((ulong)dPt[dIndex] << 56) |
                                ((ulong)dPt[dIndex + 1] << 48) |
                                ((ulong)dPt[dIndex + 2] << 40) |
                                ((ulong)dPt[dIndex + 3] << 32) |
                                ((ulong)dPt[dIndex + 4] << 24) |
                                ((ulong)dPt[dIndex + 5] << 16) |
                                ((ulong)dPt[dIndex + 6] << 8) |
                                dPt[dIndex + 7];
                    }

                    CompressBlock(hPt, wPt);

                    dataLen -= BlockByteSize;
                }

                // Copy the reamaining bytes into a blockSize length buffer so that we can loop through it easily:
                Buffer.MemoryCopy(dPt + dIndex, fPt, finalBlock.Length, dataLen);

                // Append 1 bit followed by zeros. Since we only work with bytes, this is 1 whole byte
                fPt[dataLen] = 0b1000_0000;

                if (dataLen >= 112) // blockSize - pad2.Len = 128 - 16
                {
                    // This means we have an additional block to compress, which we do it here:

                    for (int i = 0, j = 0; i < 16; i++, j += 8)
                    {
                        wPt[i] =
                            ((ulong)fPt[j] << 56) |
                            ((ulong)fPt[j + 1] << 48) |
                            ((ulong)fPt[j + 2] << 40) |
                            ((ulong)fPt[j + 3] << 32) |
                            ((ulong)fPt[j + 4] << 24) |
                            ((ulong)fPt[j + 5] << 16) |
                            ((ulong)fPt[j + 6] << 8) |
                            fPt[j + 7];
                    }

                    CompressBlock(hPt, wPt);

                    // Zero out all the items in FinalBlock so it can be reused
                    finalBlock.Clear();
                }

                // Add length in bits as the last 16 bytes of final block in big-endian order
                // See MessageLengthTest in SHA256 Test project to understand what the following shifts are
                fPt[127] = (byte)(totalLen << 3);
                fPt[126] = (byte)(totalLen >> 5);
                fPt[125] = (byte)(totalLen >> 13);
                fPt[124] = (byte)(totalLen >> 21);
                fPt[123] = (byte)(totalLen >> 29);
                // The remainig 11 bytes are always zero
                // The remaining 112 bytes are already set

                for (int i = 0, j = 0; i < 16; i++, j += 8)
                {
                    wPt[i] =
                            ((ulong)fPt[j] << 56) |
                            ((ulong)fPt[j + 1] << 48) |
                            ((ulong)fPt[j + 2] << 40) |
                            ((ulong)fPt[j + 3] << 32) |
                            ((ulong)fPt[j + 4] << 24) |
                            ((ulong)fPt[j + 5] << 16) |
                            ((ulong)fPt[j + 6] << 8) |
                            fPt[j + 7];
                }

                CompressBlock(hPt, wPt);
            }
        }

        internal unsafe void ComputeSecondHash(ulong* hPt, ulong* wPt)
        {
            // Result of previous hash (hashState[]) is now our new block. So copy it here:
            wPt[0] = hPt[0];
            wPt[1] = hPt[1];
            wPt[2] = hPt[2];
            wPt[3] = hPt[3];
            wPt[4] = hPt[4];
            wPt[5] = hPt[5];
            wPt[6] = hPt[6];
            wPt[7] = hPt[7]; // 8*8 = 64 byte hash result

            wPt[8] = 0b10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000UL; // 1 followed by 0 bits: pad1
            wPt[9] = 0;
            wPt[10] = 0;
            wPt[11] = 0;
            wPt[12] = 0;
            wPt[13] = 0;

            wPt[14] = 0; // Message length for pad2, since message is the 64 byte result of previous hash, length is 512 bit
            wPt[15] = 512;

            // Now initialize hashState to compute next round
            Init(hPt);

            // We only have 1 block so there is no need for a loop.
            CompressBlock(hPt, wPt);
        }


        internal unsafe void CompressBlock(ulong* hPt, ulong* wPt)
        {
            for (int i = 16; i < w.Length; i++)
            {
                wPt[i] = SSIG1(wPt[i - 2]) + wPt[i - 7] + SSIG0(wPt[i - 15]) + wPt[i - 16];
            }

            ulong a = hPt[0];
            ulong b = hPt[1];
            ulong c = hPt[2];
            ulong d = hPt[3];
            ulong e = hPt[4];
            ulong f = hPt[5];
            ulong g = hPt[6];
            ulong h = hPt[7];

            ulong temp, aa, bb, cc, dd, ee, ff, hh, gg;

            fixed (ulong* kPt = &Ks[0])
            {
                for (int j = 0; j < 80;)
                {
                    temp = h + BSIG1(e) + CH(e, f, g) + kPt[j] + wPt[j];
                    ee = d + temp;
                    aa = temp + BSIG0(a) + MAJ(a, b, c);
                    j++;

                    temp = g + BSIG1(ee) + CH(ee, e, f) + kPt[j] + wPt[j];
                    ff = c + temp;
                    bb = temp + BSIG0(aa) + MAJ(aa, a, b);
                    j++;

                    temp = f + BSIG1(ff) + CH(ff, ee, e) + kPt[j] + wPt[j];
                    gg = b + temp;
                    cc = temp + BSIG0(bb) + MAJ(bb, aa, a);
                    j++;

                    temp = e + BSIG1(gg) + CH(gg, ff, ee) + kPt[j] + wPt[j];
                    hh = a + temp;
                    dd = temp + BSIG0(cc) + MAJ(cc, bb, aa);
                    j++;

                    temp = ee + BSIG1(hh) + CH(hh, gg, ff) + kPt[j] + wPt[j];
                    h = aa + temp;
                    d = temp + BSIG0(dd) + MAJ(dd, cc, bb);
                    j++;

                    temp = ff + BSIG1(h) + CH(h, hh, gg) + kPt[j] + wPt[j];
                    g = bb + temp;
                    c = temp + BSIG0(d) + MAJ(d, dd, cc);
                    j++;

                    temp = gg + BSIG1(g) + CH(g, h, hh) + kPt[j] + wPt[j];
                    f = cc + temp;
                    b = temp + BSIG0(c) + MAJ(c, d, dd);
                    j++;

                    temp = hh + BSIG1(f) + CH(f, g, h) + kPt[j] + wPt[j];
                    e = dd + temp;
                    a = temp + BSIG0(b) + MAJ(b, c, d);
                    j++;
                }
            }

            hPt[0] += a;
            hPt[1] += b;
            hPt[2] += c;
            hPt[3] += d;
            hPt[4] += e;
            hPt[5] += f;
            hPt[6] += g;
            hPt[7] += h;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong CH(ulong x, ulong y, ulong z)
        {
            // (x & y) ^ ((~x) & z);
            return z ^ (x & (y ^ z)); //TODO: find mathematical proof for this change
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MAJ(ulong x, ulong y, ulong z)
        {
            // (x & y) ^ (x & z) ^ (y & z);
            return (x & y) | (z & (x | y)); //TODO: find mathematical proof for this change
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong BSIG0(ulong x)
        {
            // ROTR(x, 28) ^ ROTR(x, 34) ^ ROTR(x, 39);
            return (x >> 28 | x << 36) ^ (x >> 34 | x << 30) ^ (x >> 39 | x << 25);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong BSIG1(ulong x)
        {
            // ROTR(x, 14) ^ ROTR(x, 18) ^ ROTR(x, 41);
            return (x >> 14 | x << 50) ^ (x >> 18 | x << 46) ^ (x >> 41 | x << 23);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong SSIG0(ulong x)
        {
            // ROTR(x, 1) ^ ROTR(x, 8) ^ (x >> 7);
            return (x >> 1 | x << 63) ^ (x >> 8 | x << 56) ^ (x >> 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong SSIG1(ulong x)
        {
            // ROTR(x, 19) ^ ROTR(x, 61) ^ (x >> 6);
            return (x >> 19 | x << 45) ^ (x >> 61 | x << 3) ^ (x >> 6);
        }



        private bool isDisposed = false;

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Sha512Instance"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                if (!(hashState is null))
                    Array.Clear(hashState, 0, hashState.Length);
                hashState = null;

                if (!(w is null))
                    Array.Clear(w, 0, w.Length);
                w = null;

                isDisposed = true;
            }
        }
    }
}
