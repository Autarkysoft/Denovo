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
