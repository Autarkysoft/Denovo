// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// 256-bit scalar using 8 32-bit limbs using little-endian order
    /// </summary>
    public readonly struct Scalar8x32 : IEquatable<Scalar8x32>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given unsigned 32-bit integer.
        /// </summary>
        /// <remarks>
        /// Assumes there is no overflow
        /// </remarks>
        /// <param name="u">Value to use</param>
        public Scalar8x32(uint u)
        {
            b0 = u;
            b1 = 0; b1 = 0; b2 = 0; b3 = 0; b4 = 0; b5 = 0; b6 = 0; b7 = 0;
            Debug.Assert(CheckOverflow() == 0);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given parameters.
        /// </summary>
        /// <remarks>
        /// Assumes there is no overflow
        /// </remarks>
        /// <param name="u0">1st 32 bits</param>
        /// <param name="u1">2nd 32 bits</param>
        /// <param name="u2">3rd 32 bits</param>
        /// <param name="u3">4th 32 bits</param>
        /// <param name="u4">5th 32 bits</param>
        /// <param name="u5">6th 32 bits</param>
        /// <param name="u6">7th 32 bits</param>
        /// <param name="u7">8th 32 bits</param>
        public Scalar8x32(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7)
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3;
            b4 = u4; b5 = u5; b6 = u6; b7 = u7;
            Debug.Assert(CheckOverflow() == 0);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given array.
        /// </summary>
        /// <remarks>
        /// Assumes there is no overflow
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="array">Array of unsigned 32-bit integers</param>
        public Scalar8x32(Span<uint> array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array), "Array can not be null.");
            if (array.Length != 8)
                throw new ArgumentOutOfRangeException(nameof(array), "Array must contain 8 items.");

            b0 = array[0]; b1 = array[1]; b2 = array[2]; b3 = array[3];
            b4 = array[4]; b5 = array[5]; b6 = array[6]; b7 = array[7];
            Debug.Assert(CheckOverflow() == 0);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given pointer.
        /// </summary>
        /// <param name="hPt"><see cref="Hashing.Sha256.hashState"/> pointer</param>
        /// <param name="overflow">Returns true if value is bigger than or equal to curve order; otherwise false</param>
        public unsafe Scalar8x32(uint* hPt, out bool overflow)
        {
            b7 = hPt[0]; b6 = hPt[1]; b5 = hPt[2]; b4 = hPt[3];
            b3 = hPt[4]; b2 = hPt[5]; b1 = hPt[6]; b0 = hPt[7];
            overflow = CheckOverflow() != 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given pointer.
        /// </summary>
        /// <param name="hPt"><see cref="Hashing.Sha512.hashState"/> pointer</param>
        /// <param name="overflow">Returns true if value is bigger than or equal to curve order; otherwise false</param>
        public unsafe Scalar8x32(ulong* hPt, out bool overflow)
        {
            b7 = (uint)(hPt[0] >> 32); b6 = (uint)hPt[0];
            b5 = (uint)(hPt[1] >> 32); b4 = (uint)hPt[1];
            b3 = (uint)(hPt[2] >> 32); b2 = (uint)hPt[2];
            b1 = (uint)(hPt[3] >> 32); b0 = (uint)hPt[3];

            overflow = CheckOverflow() != 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given pointer to a big-endian array
        /// and reduces the result modulo curve order (n).
        /// </summary>
        /// <param name="pt">Pointer</param>
        /// <param name="overflow">Returns true if value was bigger than or equal to curve order; otherwise false</param>
        public unsafe Scalar8x32(byte* pt, out bool overflow)
        {
            b0 = pt[31] | ((uint)pt[30] << 8) | ((uint)pt[29] << 16) | ((uint)pt[28] << 24);
            b1 = pt[27] | ((uint)pt[26] << 8) | ((uint)pt[25] << 16) | ((uint)pt[24] << 24);
            b2 = pt[23] | ((uint)pt[22] << 8) | ((uint)pt[21] << 16) | ((uint)pt[20] << 24);
            b3 = pt[19] | ((uint)pt[18] << 8) | ((uint)pt[17] << 16) | ((uint)pt[16] << 24);
            b4 = pt[15] | ((uint)pt[14] << 8) | ((uint)pt[13] << 16) | ((uint)pt[12] << 24);
            b5 = pt[11] | ((uint)pt[10] << 8) | ((uint)pt[09] << 16) | ((uint)pt[08] << 24);
            b6 = pt[07] | ((uint)pt[06] << 8) | ((uint)pt[05] << 16) | ((uint)pt[04] << 24);
            b7 = pt[03] | ((uint)pt[02] << 8) | ((uint)pt[01] << 16) | ((uint)pt[00] << 24);

            uint of = CheckOverflow();
            overflow = of != 0;

            Debug.Assert(of == 0 || of == 1);

            ulong t = (ulong)b0 + (of * NC0);
            b0 = (uint)t; t >>= 32;
            t += (ulong)b1 + (of * NC1);
            b1 = (uint)t; t >>= 32;
            t += (ulong)b2 + (of * NC2);
            b2 = (uint)t; t >>= 32;
            t += (ulong)b3 + (of * NC3);
            b3 = (uint)t; t >>= 32;
            t += (ulong)b4 + (of * NC4);
            b4 = (uint)t; t >>= 32;
            t += b5;
            b5 = (uint)t; t >>= 32;
            t += b6;
            b6 = (uint)t; t >>= 32;
            t += b7;
            b7 = (uint)t;

            Debug.Assert((of == 1 && t >> 32 == 1) || (of == 0 && t >> 32 == 0));
            Debug.Assert(CheckOverflow() == 0);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given big-endian array
        /// and reduces the result modulo curve order (n).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="data">Array to use</param>
        /// <param name="overflow">Returns true if value was bigger than or equal to curve order; otherwise false</param>
        public Scalar8x32(ReadOnlySpan<byte> data, out bool overflow)
        {
            if (data.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(data));

            b0 = data[31] | ((uint)data[30] << 8) | ((uint)data[29] << 16) | ((uint)data[28] << 24);
            b1 = data[27] | ((uint)data[26] << 8) | ((uint)data[25] << 16) | ((uint)data[24] << 24);
            b2 = data[23] | ((uint)data[22] << 8) | ((uint)data[21] << 16) | ((uint)data[20] << 24);
            b3 = data[19] | ((uint)data[18] << 8) | ((uint)data[17] << 16) | ((uint)data[16] << 24);
            b4 = data[15] | ((uint)data[14] << 8) | ((uint)data[13] << 16) | ((uint)data[12] << 24);
            b5 = data[11] | ((uint)data[10] << 8) | ((uint)data[09] << 16) | ((uint)data[08] << 24);
            b6 = data[07] | ((uint)data[06] << 8) | ((uint)data[05] << 16) | ((uint)data[04] << 24);
            b7 = data[03] | ((uint)data[02] << 8) | ((uint)data[01] << 16) | ((uint)data[00] << 24);

            uint of = CheckOverflow();
            overflow = of != 0;

            Debug.Assert(of == 0 || of == 1);

            ulong t = (ulong)b0 + (of * NC0);
            b0 = (uint)t; t >>= 32;
            t += (ulong)b1 + (of * NC1);
            b1 = (uint)t; t >>= 32;
            t += (ulong)b2 + (of * NC2);
            b2 = (uint)t; t >>= 32;
            t += (ulong)b3 + (of * NC3);
            b3 = (uint)t; t >>= 32;
            t += (ulong)b4 + (of * NC4);
            b4 = (uint)t; t >>= 32;
            t += b5;
            b5 = (uint)t; t >>= 32;
            t += b6;
            b6 = (uint)t; t >>= 32;
            t += b7;
            b7 = (uint)t;

            Debug.Assert((of == 1 && t >> 32 == 1) || (of == 0 && t >> 32 == 0));
            Debug.Assert(CheckOverflow() == 0);
        }


        /// <summary>
        /// Bit chunks
        /// </summary>
        public readonly uint b0, b1, b2, b3, b4, b5, b6, b7;

        // Secp256k1 curve order (N)
        private const uint N0 = 0xD0364141U;
        private const uint N1 = 0xBFD25E8CU;
        private const uint N2 = 0xAF48A03BU;
        private const uint N3 = 0xBAAEDCE6U;
        private const uint N4 = 0xFFFFFFFEU;
        private const uint N5 = 0xFFFFFFFFU;
        private const uint N6 = 0xFFFFFFFFU;
        private const uint N7 = 0xFFFFFFFFU;

        // 2^256 - N
        // Since overflow will be less than 2N the result of X % N is X - N
        // X - N ≡ Z (mod N) => X + (2^256 - N) ≡ Z + 2^256 (mod N)
        // 250 ≡ 9 (mod 241) => 250 - 241 ≡ 250 + 256 - 241 ≡ 265 ≡ 265 - 256 ≡ 9 (mod 241)
        //                   => 265=0x0109 256=0x0100 => 265-256: get rid of highest bit => 0x0109≡0x09
        private const uint NC0 = ~N0 + 1;
        private const uint NC1 = ~N1;
        private const uint NC2 = ~N2;
        private const uint NC3 = ~N3;
        private const uint NC4 = 1;

        // N/2
        private const uint NH0 = 0x681B20A0U;
        private const uint NH1 = 0xDFE92F46U;
        private const uint NH2 = 0x57A4501DU;
        private const uint NH3 = 0x5D576E73U;
        private const uint NH4 = 0xFFFFFFFFU;
        private const uint NH5 = 0xFFFFFFFFU;
        private const uint NH6 = 0xFFFFFFFFU;
        private const uint NH7 = 0x7FFFFFFFU;

        /// <summary>
        /// Byte size of <see cref="Scalar8x32"/>
        /// </summary>
        public const int ByteSize = 32;

        private static readonly Scalar8x32 _zero = new Scalar8x32(0);
        /// <summary>
        /// Zero
        /// </summary>
        public static ref readonly Scalar8x32 Zero => ref _zero;

        /// <summary>
        /// Returns if the value is equal to zero
        /// </summary>
        public bool IsZero => (b0 | b1 | b2 | b3 | b4 | b5 | b6 | b7) == 0;
        /// <summary>
        /// Returns if the value is even
        /// </summary>
        public bool IsEven => (b0 & 1) == 0;
        /// <summary>
        /// Returns if this scalar is higher than the group order divided by 2
        /// </summary>
        public bool IsHigh
        {
            // TODO: needs testing, (int was replaced with bool)
            get
            {
                bool yes = false;
                bool no = false;
                no |= (b7 < NH7);
                yes |= (b7 > NH7) & !no;
                no |= (b6 < NH6) & !yes; // No need for a > check.
                no |= (b5 < NH5) & !yes; // No need for a > check.
                no |= (b4 < NH4) & !yes; // No need for a > check.
                no |= (b3 < NH3) & !yes;
                yes |= (b3 > NH3) & !no;
                no |= (b2 < NH2) & !yes;
                yes |= (b2 > NH2) & !no;
                no |= (b1 < NH1) & !yes;
                yes |= (b1 > NH1) & !no;
                yes |= (b0 > NH0) & !no;
                return yes;
            }
        }

        private uint CheckOverflow()
        {
            uint yes = 0U;
            uint no = 0U;
            no |= (b7 < N7 ? 1U : 0U);
            no |= (b6 < N6 ? 1U : 0U);
            no |= (b5 < N5 ? 1U : 0U);
            no |= (b4 < N4 ? 1U : 0U);
            yes |= (b4 > N4 ? 1U : 0U) & ~no;
            no |= (b3 < N3 ? 1U : 0U) & ~yes;
            yes |= (b3 > N3 ? 1U : 0U) & ~no;
            no |= (b2 < N2 ? 1U : 0U) & ~yes;
            yes |= (b2 > N2 ? 1U : 0U) & ~no;
            no |= (b1 < N1 ? 1U : 0U) & ~yes;
            yes |= (b1 > N1 ? 1U : 0U) & ~no;
            yes |= (b0 >= N0 ? 1U : 0U) & ~no;
            return yes;
        }


        /// <summary>
        /// Adds the two scalars together modulo the group order.
        /// </summary>
        /// <param name="other">Other value</param>
        /// <param name="overflow">Returns whether it overflowed</param>
        /// <returns>Result</returns>
        public Scalar8x32 Add(in Scalar8x32 other, out bool overflow)
        {
            ulong t = (ulong)b0 + other.b0;
            uint r0 = (uint)t; t >>= 32;
            t += (ulong)b1 + other.b1;
            uint r1 = (uint)t; t >>= 32;
            t += (ulong)b2 + other.b2;
            uint r2 = (uint)t; t >>= 32;
            t += (ulong)b3 + other.b3;
            uint r3 = (uint)t; t >>= 32;
            t += (ulong)b4 + other.b4;
            uint r4 = (uint)t; t >>= 32;
            t += (ulong)b5 + other.b5;
            uint r5 = (uint)t; t >>= 32;
            t += (ulong)b6 + other.b6;
            uint r6 = (uint)t; t >>= 32;
            t += (ulong)b7 + other.b7;
            uint r7 = (uint)t; t >>= 32;

            int yes = 0;
            int no = 0;
            no |= (r7 < N7 ? 1 : 0);
            no |= (r6 < N6 ? 1 : 0);
            no |= (r5 < N5 ? 1 : 0);
            no |= (r4 < N4 ? 1 : 0);
            yes |= (r4 > N4 ? 1 : 0) & ~no;
            no |= (r3 < N3 ? 1 : 0) & ~yes;
            yes |= (r3 > N3 ? 1 : 0) & ~no;
            no |= (r2 < N2 ? 1 : 0) & ~yes;
            yes |= (r2 > N2 ? 1 : 0) & ~no;
            no |= (r1 < N1 ? 1 : 0) & ~yes;
            yes |= (r1 > N1 ? 1 : 0) & ~no;
            yes |= (r0 >= N0 ? 1 : 0) & ~no;

            uint of = (uint)yes + (uint)t;
            overflow = of != 0;

            Debug.Assert(of == 0 || of == 1);

            t = (ulong)r0 + (of * NC0);
            r0 = (uint)t; t >>= 32;
            t += (ulong)r1 + (of * NC1);
            r1 = (uint)t; t >>= 32;
            t += (ulong)r2 + (of * NC2);
            r2 = (uint)t; t >>= 32;
            t += (ulong)r3 + (of * NC3);
            r3 = (uint)t; t >>= 32;
            t += (ulong)r4 + (of * NC4);
            r4 = (uint)t; t >>= 32;
            t += r5;
            r5 = (uint)t; t >>= 32;
            t += r6;
            r6 = (uint)t; t >>= 32;
            t += r7;
            r7 = (uint)t;

            return new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
        }


        /// <summary>
        /// Conditionally add a power of two to this scalar. The result is not allowed to overflow.
        /// </summary>
        /// <param name="bit"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public Scalar8x32 CAddBit(uint bit, int flag)
        {
            Debug.Assert(bit < 256);
            bit += ((uint)flag - 1) & 0x100;  // forcing (bit >> 5) > 7 makes this a noop
            int shift = (int)bit & 0x1F;
            ulong t = (ulong)b0 + (((bit >> 5) == 0 ? 1U : 0) << shift);
            uint r0 = (uint)t; t >>= 32;
            t += (ulong)b1 + (((bit >> 5) == 1 ? 1U : 0) << shift);
            uint r1 = (uint)t; t >>= 32;
            t += (ulong)b2 + (((bit >> 5) == 2 ? 1U : 0) << shift);
            uint r2 = (uint)t; t >>= 32;
            t += (ulong)b3 + (((bit >> 5) == 3 ? 1U : 0) << shift);
            uint r3 = (uint)t; t >>= 32;
            t += (ulong)b4 + (((bit >> 5) == 4 ? 1U : 0) << shift);
            uint r4 = (uint)t; t >>= 32;
            t += (ulong)b5 + (((bit >> 5) == 5 ? 1U : 0) << shift);
            uint r5 = (uint)t; t >>= 32;
            t += (ulong)b6 + (((bit >> 5) == 6 ? 1U : 0) << shift);
            uint r6 = (uint)t; t >>= 32;
            t += (ulong)b7 + (((bit >> 5) == 7 ? 1U : 0) << shift);
            uint r7 = (uint)t;

            Debug.Assert((t >> 32) == 0);

            return new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
        }


        public static unsafe uint GetBits(uint* pt, int offset, int count)
        {
            Debug.Assert((offset + count - 1) >> 5 == offset >> 5);
            return (pt[offset >> 5] >> (offset & 0x1F)) & ((1U << count) - 1);
        }

        public static unsafe uint GetBitsVar(uint* pt, int offset, int count)
        {
            Debug.Assert(count < 32);
            Debug.Assert(offset + count <= 256);
            if ((offset + count - 1) >> 5 == offset >> 5)
            {
                return GetBits(pt, offset, count);
            }
            else
            {
                Debug.Assert((offset >> 5) + 1 < 8);
                return ((pt[offset >> 5] >> (offset & 0x1F)) | (pt[(offset >> 5) + 1] << (32 - (offset & 0x1F)))) & 
                       ((1U << count) - 1);
            }
        }

        public Scalar8x32 Inverse_old()
        {
            /* First compute xN as x ^ (2^N - 1) for some values of N,
             * and uM as x ^ M for some values of M. */
            Scalar8x32 x2, x3, x6, x8, x14, x28, x56, x112, x126;
            Scalar8x32 u2, u5, u9, u11, u13;

            u2 = this.Multiply(this);
            x2 = u2.Multiply(this);
            u5 = u2.Multiply(x2);
            x3 = u5.Multiply(u2);
            u9 = x3.Multiply(u2);
            u11 = u9.Multiply(u2);
            u13 = u11.Multiply(u2);

            x6 = u13.Multiply(u13);
            x6 = x6.Multiply(x6);
            x6 = x6.Multiply(u11);

            x8 = x6.Multiply(x6);
            x8 = x8.Multiply(x8);
            x8 = x8.Multiply(x2);

            x14 = x8.Multiply(x8);
            for (int i = 0; i < 5; i++)
            {
                x14 = x14.Multiply(x14);
            }
            x14 = x14.Multiply(x6);

            x28 = x14.Multiply(x14);
            for (int i = 0; i < 13; i++)
            {
                x28 = x28.Multiply(x28);
            }
            x28 = x28.Multiply(x14);

            x56 = x28.Multiply(x28);
            for (int i = 0; i < 27; i++)
            {
                x56 = x56.Multiply(x56);
            }
            x56 = x56.Multiply(x28);

            x112 = x56.Multiply(x56);
            for (int i = 0; i < 55; i++)
            {
                x112 = x112.Multiply(x112);
            }
            x112 = x112.Multiply(x56);

            x126 = x112.Multiply(x112);
            for (int i = 0; i < 13; i++)
            {
                x126 = x126.Multiply(x126);
            }
            x126 = x126.Multiply(x14);

            /* Then accumulate the final result (t starts at x126). */
            ref Scalar8x32 t = ref x126;
            for (int i = 0; i < 3; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u5); /* 101 */
            for (int i = 0; i < 4; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(x3); /* 111 */
            for (int i = 0; i < 4; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u5); /* 101 */
            for (int i = 0; i < 5; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u11); /* 1011 */
            for (int i = 0; i < 4; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u11); /* 1011 */
            for (int i = 0; i < 4; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(x3); /* 111 */
            for (int i = 0; i < 5; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(x3); /* 111 */
            for (int i = 0; i < 6; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u13); /* 1101 */
            for (int i = 0; i < 4; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u5); /* 101 */
            for (int i = 0; i < 3; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(x3); /* 111 */
            for (int i = 0; i < 5; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u9); /* 1001 */
            for (int i = 0; i < 6; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u5); /* 101 */
            for (int i = 0; i < 10; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(x3); /* 111 */
            for (int i = 0; i < 4; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(x3); /* 111 */
            for (int i = 0; i < 9; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(x8); /* 11111111 */
            for (int i = 0; i < 5; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u9); /* 1001 */
            for (int i = 0; i < 6; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u11); /* 1011 */
            for (int i = 0; i < 4; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u13); /* 1101 */
            for (int i = 0; i < 5; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(x2); /* 11 */
            for (int i = 0; i < 6; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u13); /* 1101 */
            for (int i = 0; i < 10; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u13); /* 1101 */
            for (int i = 0; i < 4; i++)
            {
                t = t.Multiply(t);
            }
            t = t.Multiply(u9); /* 1001 */
            /* 00000 */
            for (int i = 0; i < 6; i++)
            {
                t = t.Multiply(t);
            }

            t = t.Multiply(this); /* 1 */
            for (int i = 0; i < 8; i++)
            {
                t = t.Multiply(t);
            }
            return t.Multiply(x6); /* 111111 */
        }

        public Scalar8x32 InverseVar_old()
        {
            return Inverse_old();
        }


        /// <summary>
        /// Multiply two scalars modulo the group order.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public unsafe Scalar8x32 Multiply(in Scalar8x32 b)
        {
            // * secp256k1_scalar_mul_512
            // 96 bit accumulator
            // uint c0 = 0, c1 = 0, c2 = 0;

            // l[0..15] = a[0..7] * b[0..7].
            // muladd_fast(a->d[0], b->d[0]);
            // extract_fast(l[0]);
            ulong t = (ulong)b0 * b.b0;
            uint c0 = (uint)(t >> 32);
            uint L0 = (uint)t;
            // muladd(a->d[0], b->d[1]);
            t = (ulong)b0 * b.b1;
            uint c1 = (uint)(t >> 32);
            uint tl = (uint)t;
            c0 += tl;
            bool comp = (c0 < tl);
            c1 += *(uint*)&comp;
            // muladd(a->d[1], b->d[0]);
            t = (ulong)b1 * b.b0; uint th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            uint c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[1]);
            uint L1 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[0], b->d[2]);
            t = (ulong)b0 * b.b2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[1], b->d[1]);
            t = (ulong)b1 * b.b1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[2], b->d[0]);
            t = (ulong)b2 * b.b0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[2]);
            uint L2 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[0], b->d[3]);
            t = (ulong)b0 * b.b3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[1], b->d[2]);
            t = (ulong)b1 * b.b2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[2], b->d[1]);
            t = (ulong)b2 * b.b1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[3], b->d[0]);
            t = (ulong)b3 * b.b0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[3]);
            uint L3 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[0], b->d[4]);
            t = (ulong)b0 * b.b4; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[1], b->d[3]);
            t = (ulong)b1 * b.b3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[2], b->d[2]);
            t = (ulong)b2 * b.b2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[3], b->d[1]);
            t = (ulong)b3 * b.b1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[4], b->d[0]);
            t = (ulong)b4 * b.b0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[4]);
            uint L4 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[0], b->d[5]);
            t = (ulong)b0 * b.b5; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[1], b->d[4]);
            t = (ulong)b1 * b.b4; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[2], b->d[3]);
            t = (ulong)b2 * b.b3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[3], b->d[2]);
            t = (ulong)b3 * b.b2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[4], b->d[1]);
            t = (ulong)b4 * b.b1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[5], b->d[0]);
            t = (ulong)b5 * b.b0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[5]);
            uint L5 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[0], b->d[6]);
            t = (ulong)b0 * b.b6; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[1], b->d[5]);
            t = (ulong)b1 * b.b5; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[2], b->d[4]);
            t = (ulong)b2 * b.b4; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[3], b->d[3]);
            t = (ulong)b3 * b.b3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[4], b->d[2]);
            t = (ulong)b4 * b.b2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[5], b->d[1]);
            t = (ulong)b5 * b.b1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp; ;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[6], b->d[0]);
            t = (ulong)b6 * b.b0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[6]);
            uint L6 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[0], b->d[7]);
            t = (ulong)b0 * b.b7; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[1], b->d[6]);
            t = (ulong)b1 * b.b6; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[2], b->d[5]);
            t = (ulong)b2 * b.b5; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[3], b->d[4]);
            t = (ulong)b3 * b.b4; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[4], b->d[3]);
            t = (ulong)b4 * b.b3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[5], b->d[2]);
            t = (ulong)b5 * b.b2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[6], b->d[1]);
            t = (ulong)b6 * b.b1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[7], b->d[0]);
            t = (ulong)b7 * b.b0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[7]);
            uint L7 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[1], b->d[7]);
            t = (ulong)b1 * b.b7; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[2], b->d[6]);
            t = (ulong)b2 * b.b6; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[3], b->d[5]);
            t = (ulong)b3 * b.b5; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[4], b->d[4]);
            t = (ulong)b4 * b.b4; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[5], b->d[3]);
            t = (ulong)b5 * b.b3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[6], b->d[2]);
            t = (ulong)b6 * b.b2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[7], b->d[1]);
            t = (ulong)b7 * b.b1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[8]);
            uint L8 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[2], b->d[7]);
            t = (ulong)b2 * b.b7; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[3], b->d[6]);
            t = (ulong)b3 * b.b6; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[4], b->d[5]);
            t = (ulong)b4 * b.b5; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[5], b->d[4]);
            t = (ulong)b5 * b.b4; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[6], b->d[3]);
            t = (ulong)b6 * b.b3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[7], b->d[2]);
            t = (ulong)b7 * b.b2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[9]);
            uint L9 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[3], b->d[7]);
            t = (ulong)b3 * b.b7; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[4], b->d[6]);
            t = (ulong)b4 * b.b6; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[5], b->d[5]);
            t = (ulong)b5 * b.b5; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[6], b->d[4]);
            t = (ulong)b6 * b.b4; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[7], b->d[3]);
            t = (ulong)b7 * b.b3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[10]);
            uint L10 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[4], b->d[7]);
            t = (ulong)b4 * b.b7; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[5], b->d[6]);
            t = (ulong)b5 * b.b6; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[6], b->d[5]);
            t = (ulong)b6 * b.b5; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[7], b->d[4]);
            t = (ulong)b7 * b.b4; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[11]);
            uint L11 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[5], b->d[7]);
            t = (ulong)b5 * b.b7; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[6], b->d[6]);
            t = (ulong)b6 * b.b6;
            th = (uint)(t >> 32);
            tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[7], b->d[5]);
            t = (ulong)b7 * b.b5; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[12]);
            uint L12 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(a->d[6], b->d[7]);
            t = (ulong)b6 * b.b7; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(a->d[7], b->d[6]);
            t = (ulong)b7 * b.b6; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(l[13]);
            uint L13 = c0;
            c0 = c1;
            c1 = c2;
            // muladd_fast(a->d[7], b->d[7]);
            t = (ulong)b7 * b.b7; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            Debug.Assert(c1 >= th);
            // extract_fast(l[14]);
            uint L14 = c0;
            // c0 = c1; l[15] = c0;
            uint L15 = c1;

            // * secp256k1_scalar_reduce_512

            uint m0, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12;
            uint p0, p1, p2, p3, p4, p5, p6, p7, p8;

            // 96 bit accumulator
            // uint c0, c1, c2;

            // Reduce 512 bits into 385.
            // m[0..12] = l[0..7] + n[0..7] * SECP256K1_N_C
            c0 = L0;
            // muladd_fast(n0, SECP256K1_N_C_0); n0 = L8
            t = (ulong)L8 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 = th;
            Debug.Assert(c1 >= th);
            // extract_fast(m0);
            m0 = c0;
            c0 = c1;
            // sumadd_fast(l[1]);
            c0 += L1;
            comp = (c0 < L1);
            c1 = *(uint*)&comp;
            Debug.Assert((c1 != 0) | (c0 >= L1));
            // muladd(n1, SECP256K1_N_C_0); n1 = L9
            t = (ulong)L9 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 = *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n0, SECP256K1_N_C_1); n0 = L8
            t = (ulong)L8 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(m1);
            m1 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(l[2]);
            c0 += L2;
            comp = (c0 < L2);
            uint over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(n2, SECP256K1_N_C_0); n2 = L10
            t = (ulong)L10 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n1, SECP256K1_N_C_1); n1 = L9
            t = (ulong)L9 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n0, SECP256K1_N_C_2); n0 = L8
            t = (ulong)L8 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(m2);
            m2 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(l[3]);
            c0 += L3;
            comp = (c0 < L3);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(n3, SECP256K1_N_C_0); n3 = L11
            t = (ulong)L11 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n2, SECP256K1_N_C_1); n2 = L10
            t = (ulong)L10 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n1, SECP256K1_N_C_2); n1 = L9
            t = (ulong)L9 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n0, SECP256K1_N_C_3); n0 = L8
            t = (ulong)L8 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(m3);
            m3 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(l[4]);
            c0 += L4;
            comp = (c0 < L4);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(n4, SECP256K1_N_C_0); n4 = L12
            t = (ulong)L12 * NC0;
            th = (uint)(t >> 32);
            tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n3, SECP256K1_N_C_1); n3 = L11
            t = (ulong)L11 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n2, SECP256K1_N_C_2); n2 = L10
            t = (ulong)L10 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n1, SECP256K1_N_C_3); n1 = L9
            t = (ulong)L9 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(n0); n0 = L8
            c0 += L8;
            comp = (c0 < L8);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(m4);
            m4 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(l[5]);
            c0 += L5;
            comp = (c0 < L5);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(n5, SECP256K1_N_C_0); n5 = L13
            t = (ulong)L13 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n4, SECP256K1_N_C_1); n4 = L12
            t = (ulong)L12 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n3, SECP256K1_N_C_2); n3 = L11
            t = (ulong)L11 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n2, SECP256K1_N_C_3); n2 = L10
            t = (ulong)L10 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(n1); n1 = L9
            c0 += L9;
            comp = (c0 < L9);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(m5);
            m5 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(l[6]);
            c0 += L6;
            comp = (c0 < L6);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(n6, SECP256K1_N_C_0); n6 = L14
            t = (ulong)L14 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n5, SECP256K1_N_C_1); n5 = L13
            t = (ulong)L13 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n4, SECP256K1_N_C_2); n4 = L12
            t = (ulong)L12 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n3, SECP256K1_N_C_3); n3 = L11
            t = (ulong)L11 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(n2); n2 = L10
            c0 += L10;
            comp = (c0 < L10);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(m6);
            m6 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(l[7]);
            c0 += L7;
            comp = (c0 < L7);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(n7, SECP256K1_N_C_0); n7=L15
            t = (ulong)L15 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n6, SECP256K1_N_C_1); n6 = L14
            t = (ulong)L14 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n5, SECP256K1_N_C_2); n5 = L13
            t = (ulong)L13 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n4, SECP256K1_N_C_3); n4 = L12
            t = (ulong)L12 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(n3); n3 = L11
            c0 += L11;
            comp = (c0 < L11);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(m7);
            m7 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(n7, SECP256K1_N_C_1); n7=L15
            t = (ulong)L15 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n6, SECP256K1_N_C_2); n6 = L14
            t = (ulong)L14 * NC2;
            th = (uint)(t >> 32);
            tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n5, SECP256K1_N_C_3); n5 = L13
            t = (ulong)L13 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(n4); n4 = L12
            c0 += L12;
            comp = (c0 < L12);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(m8);
            m8 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(n7, SECP256K1_N_C_2); n7=L15
            t = (ulong)L15 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(n6, SECP256K1_N_C_3); n6 = L14
            t = (ulong)L14 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(n5); n5 = L13
            c0 += L13;
            comp = (c0 < L13);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(m9);
            m9 = c0;
            c0 = c1;
            c1 = c2;
            // muladd(n7, SECP256K1_N_C_3); n7=L15
            t = (ulong)L15 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(n6); n6 = L14
            c0 += L14;
            comp = (c0 < L14);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(m10);
            m10 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd_fast(n7); n7=L15
            c0 += L15;
            comp = (c0 < L15);
            c1 += *(uint*)&comp;
            Debug.Assert((c1 != 0) | (c0 >= L15));
            // extract_fast(m11);
            m11 = c0;
            // c0 = c1;

            Debug.Assert(c1 <= 1);
            m12 = c1;

            // Reduce 385 bits into 258
            // p[0..8] = m[0..7] + m[8..12] * SECP256K1_N_C
            c0 = m0;
            // muladd_fast(m8, SECP256K1_N_C_0);
            t = (ulong)m8 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 = th;
            Debug.Assert(c1 >= th);
            // extract_fast(p0);
            p0 = c0;
            c0 = c1;
            // sumadd_fast(m1);
            c0 += m1;
            comp = (c0 < m1);
            c1 = *(uint*)&comp;
            Debug.Assert((c1 != 0) | (c0 >= m1));
            // muladd(m9, SECP256K1_N_C_0);
            t = (ulong)m9 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m8, SECP256K1_N_C_1);
            t = (ulong)m8 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(p1);
            p1 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(m2);
            c0 += m2;
            comp = (c0 < m2);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(m10, SECP256K1_N_C_0);
            t = (ulong)m10 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m9, SECP256K1_N_C_1);
            t = (ulong)m9 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m8, SECP256K1_N_C_2);
            t = (ulong)m8 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(p2);
            p2 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(m3);
            c0 += m3;
            comp = (c0 < m3);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(m11, SECP256K1_N_C_0);
            t = (ulong)m11 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m10, SECP256K1_N_C_1);
            t = (ulong)m10 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m9, SECP256K1_N_C_2);
            t = (ulong)m9 * NC2;
            th = (uint)(t >> 32);
            tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m8, SECP256K1_N_C_3);
            t = (ulong)m8 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // extract(p3);
            p3 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(m4);
            c0 += m4;
            comp = (c0 < m4);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(m12, SECP256K1_N_C_0);
            t = (ulong)m12 * NC0; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m11, SECP256K1_N_C_1);
            t = (ulong)m11 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m10, SECP256K1_N_C_2);
            t = (ulong)m10 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m9, SECP256K1_N_C_3);
            t = (ulong)m9 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(m8);
            c0 += m8;
            comp = (c0 < m8);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(p4);
            p4 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(m5);
            c0 += m5;
            comp = (c0 < m5);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(m12, SECP256K1_N_C_1);
            t = (ulong)m12 * NC1; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m11, SECP256K1_N_C_2);
            t = (ulong)m11 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m10, SECP256K1_N_C_3);
            t = (ulong)m10 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(m9);
            c0 += m9;
            comp = (c0 < m9);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(p5);
            p5 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd(m6);
            c0 += m6;
            comp = (c0 < m6);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 = *(uint*)&comp;
            // muladd(m12, SECP256K1_N_C_2);
            t = (ulong)m12 * NC2; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // muladd(m11, SECP256K1_N_C_3);
            t = (ulong)m11 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            comp = (c1 < th);
            c2 += *(uint*)&comp;
            Debug.Assert((c1 >= th) || (c2 != 0));
            // sumadd(m10);
            c0 += m10;
            comp = (c0 < m10);
            over = *(uint*)&comp;
            c1 += over;
            comp = (c1 < over);
            c2 += *(uint*)&comp;
            // extract(p6);
            p6 = c0;
            c0 = c1;
            c1 = c2;
            // sumadd_fast(m7);
            c0 += m7;
            comp = (c0 < m7);
            c1 += *(uint*)&comp;
            Debug.Assert((c1 != 0) | (c0 >= m7));
            // muladd_fast(m12, SECP256K1_N_C_3);
            t = (ulong)m12 * NC3; th = (uint)(t >> 32); tl = (uint)t;
            c0 += tl;
            comp = (c0 < tl);
            th += *(uint*)&comp;
            c1 += th;
            Debug.Assert(c1 >= th);
            // sumadd_fast(m11);
            c0 += m11;
            comp = (c0 < m11);
            c1 += *(uint*)&comp;
            Debug.Assert((c1 != 0) | (c0 >= m11));
            // extract_fast(p7);
            //p7 = c0;

            p8 = c1 + m12;
            Debug.Assert(p8 <= 2);

            // Reduce 258 bits into 256
            // r[0..7] = p[0..7] + p[8] * SECP256K1_N_C
            ulong c = p0 + (ulong)NC0 * p8;
            p0 = (uint)c; c >>= 32;
            c += p1 + (ulong)NC1 * p8;
            p1 = (uint)c; c >>= 32;
            c += p2 + (ulong)NC2 * p8;
            p2 = (uint)c; c >>= 32;
            c += p3 + (ulong)NC3 * p8;
            p3 = (uint)c; c >>= 32;
            c += p4 + (ulong)p8;
            p4 = (uint)c; c >>= 32;
            c += p5;
            p5 = (uint)c; c >>= 32;
            c += p6;
            p6 = (uint)c; c >>= 32;
            c += c0;
            p7 = (uint)c; c >>= 32;

            // Final reduction of r
            // secp256k1_scalar_reduce(r, c + secp256k1_scalar_check_overflow(r));

            uint yes = 0;
            uint no = 0;
            no |= (p7 < N7 ? 1U : 0U); /* No need for a > check. */
            no |= (p6 < N6 ? 1U : 0U); /* No need for a > check. */
            no |= (p5 < N5 ? 1U : 0U); /* No need for a > check. */
            no |= (p4 < N4 ? 1U : 0U);
            yes |= (p4 > N4 ? 1U : 0U) & ~no;
            no |= (p3 < N3 ? 1U : 0U) & ~yes;
            yes |= (p3 > N3 ? 1U : 0U) & ~no;
            no |= (p2 < N2 ? 1U : 0U) & ~yes;
            yes |= (p2 > N2 ? 1U : 0U) & ~no;
            no |= (p1 < N1 ? 1U : 0U) & ~yes;
            yes |= (p1 > N1 ? 1U : 0U) & ~no;
            yes |= (p0 >= N0 ? 1U : 0U) & ~no;
            ulong of = c + yes;

            t = p0 + (of * NC0);
            p0 = (uint)t; t >>= 32;
            t += p1 + (of * NC1);
            p1 = (uint)t; t >>= 32;
            t += p2 + (of * NC2);
            p2 = (uint)t; t >>= 32;
            t += p3 + (of * NC3);
            p3 = (uint)t; t >>= 32;
            t += p4 + (of * NC4);
            p4 = (uint)t; t >>= 32;
            t += p5;
            p5 = (uint)t; t >>= 32;
            t += p6;
            p6 = (uint)t; t >>= 32;
            t += p7;
            p7 = (uint)t;

            return new Scalar8x32(p0, p1, p2, p3, p4, p5, p6, p7);
        }


        /// <summary>
        /// Returns the complement of this scalar modulo the group order.
        /// </summary>
        /// <returns></returns>
        public Scalar8x32 Negate()
        {
            uint nonzero = 0xFFFFFFFFU * (IsZero ? 0U : 1U); // secp256k1_scalar_is_zero(a) == 0);
            ulong t = (ulong)(~b0) + N0 + 1;
            uint r0 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(~b1) + N1;
            uint r1 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(~b2) + N2;
            uint r2 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(~b3) + N3;
            uint r3 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(~b4) + N4;
            uint r4 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(~b5) + N5;
            uint r5 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(~b6) + N6;
            uint r6 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(~b7) + N7;
            uint r7 = (uint)(t & nonzero);

            return new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
        }


        /// <summary>
        /// Returns byte array representation of this instance
        /// </summary>
        /// <returns>32 bytes</returns>
        public byte[] ToByteArray()
        {
            return new byte[32]
            {
                (byte)(b7 >> 24), (byte)(b7 >> 16), (byte)(b7 >> 8), (byte)b7,
                (byte)(b6 >> 24), (byte)(b6 >> 16), (byte)(b6 >> 8), (byte)b6,
                (byte)(b5 >> 24), (byte)(b5 >> 16), (byte)(b5 >> 8), (byte)b5,
                (byte)(b4 >> 24), (byte)(b4 >> 16), (byte)(b4 >> 8), (byte)b4,
                (byte)(b3 >> 24), (byte)(b3 >> 16), (byte)(b3 >> 8), (byte)b3,
                (byte)(b2 >> 24), (byte)(b2 >> 16), (byte)(b2 >> 8), (byte)b2,
                (byte)(b1 >> 24), (byte)(b1 >> 16), (byte)(b1 >> 8), (byte)b1,
                (byte)(b0 >> 24), (byte)(b0 >> 16), (byte)(b0 >> 8), (byte)b0,
            };
        }


        public void WriteToSpan(Span<byte> stream)
        {
            if (stream.Length < 32)
                throw new ArgumentOutOfRangeException();

            stream[0] = (byte)(b7 >> 24); stream[1] = (byte)(b7 >> 16); stream[2] = (byte)(b7 >> 8); stream[3] = (byte)b7;
            stream[4] = (byte)(b6 >> 24); stream[5] = (byte)(b6 >> 16); stream[6] = (byte)(b6 >> 8); stream[7] = (byte)b6;
            stream[8] = (byte)(b5 >> 24); stream[9] = (byte)(b5 >> 16); stream[10] = (byte)(b5 >> 8); stream[11] = (byte)b5;
            stream[12] = (byte)(b4 >> 24); stream[13] = (byte)(b4 >> 16); stream[14] = (byte)(b4 >> 8); stream[15] = (byte)b4;
            stream[16] = (byte)(b3 >> 24); stream[17] = (byte)(b3 >> 16); stream[18] = (byte)(b3 >> 8); stream[19] = (byte)b3;
            stream[20] = (byte)(b2 >> 24); stream[21] = (byte)(b2 >> 16); stream[22] = (byte)(b2 >> 8); stream[23] = (byte)b2;
            stream[24] = (byte)(b1 >> 24); stream[25] = (byte)(b1 >> 16); stream[26] = (byte)(b1 >> 8); stream[27] = (byte)b1;
            stream[28] = (byte)(b0 >> 24); stream[29] = (byte)(b0 >> 16); stream[30] = (byte)(b0 >> 8); stream[31] = (byte)b0;
        }


        /// <summary>
        /// Returns if the given scalar is equal to this instance
        /// </summary>
        /// <param name="other">Scalar to compare to</param>
        /// <returns>True if the two scalars are equal; otherwise false.</returns>
        public bool Equals(Scalar8x32 other) => this == other;
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Scalar8x32 other && this == other;

        /// <summary>
        /// Returns if the two scalars are equal to each other
        /// </summary>
        /// <param name="left">First scalar</param>
        /// <param name="right">Second scalar</param>
        /// <returns>True if the two scalars are equal; otherwise false.</returns>
        public static bool operator ==(in Scalar8x32 left, in Scalar8x32 right) =>
            ((left.b0 ^ right.b0) | (left.b1 ^ right.b1) | (left.b2 ^ right.b2) | (left.b3 ^ right.b3) |
             (left.b4 ^ right.b4) | (left.b5 ^ right.b5) | (left.b6 ^ right.b6) | (left.b7 ^ right.b7)) == 0;

        /// <summary>
        /// Returns if the two scalars are not equal to each other
        /// </summary>
        /// <param name="left">First scalar</param>
        /// <param name="right">Second scalar</param>
        /// <returns>True if the two scalars are not equal; otherwise false.</returns>
        public static bool operator !=(in Scalar8x32 left, in Scalar8x32 right) => !(left == right);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(b0, b1, b2, b3, b4, b5, b6, b7);
    }
}
