﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
            Debug.Assert(Verify());
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given parameters.
        /// </summary>
        /// <remarks>
        /// Assumes caller handles overflow
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
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given array.
        /// </summary>
        /// <remarks>
        /// Assumes caller handles overflow
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
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given pointer.
        /// </summary>
        /// <remarks>
        /// Assumes there is no overflow
        /// </remarks>
        /// <param name="pt">Pointer of the array containing 8 items (256 bits)</param>
        public unsafe Scalar8x32(uint* pt)
        {
            b0 = pt[0]; b1 = pt[1]; b2 = pt[2]; b3 = pt[3];
            b4 = pt[4]; b5 = pt[5]; b6 = pt[6]; b7 = pt[7];
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
        /// <param name="hPt">SHA512 hashState pointer</param>
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
            uint* r = stackalloc uint[8];
            overflow = SetB32(pt, r);
            b0 = r[0]; b1 = r[1]; b2 = r[2]; b3 = r[3];
            b4 = r[4]; b5 = r[5]; b6 = r[6]; b7 = r[7];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar8x32"/> using the given big-endian array
        /// and reduces the result modulo curve order (n).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="data">Array to use</param>
        /// <param name="overflow">Returns true if value was bigger than or equal to curve order; otherwise false</param>
        public unsafe Scalar8x32(ReadOnlySpan<byte> data, out bool overflow)
        {
            if (data.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(data));

            uint* r = stackalloc uint[8];
            fixed (byte* pt = &data[0])
            {
                overflow = SetB32(pt, r);
                b0 = r[0]; b1 = r[1]; b2 = r[2]; b3 = r[3];
                b4 = r[4]; b5 = r[5]; b6 = r[6]; b7 = r[7];
            }
        }

        private static unsafe bool SetB32(byte* pt, uint* r)
        {
            r[0] = pt[31] | ((uint)pt[30] << 8) | ((uint)pt[29] << 16) | ((uint)pt[28] << 24);
            r[1] = pt[27] | ((uint)pt[26] << 8) | ((uint)pt[25] << 16) | ((uint)pt[24] << 24);
            r[2] = pt[23] | ((uint)pt[22] << 8) | ((uint)pt[21] << 16) | ((uint)pt[20] << 24);
            r[3] = pt[19] | ((uint)pt[18] << 8) | ((uint)pt[17] << 16) | ((uint)pt[16] << 24);
            r[4] = pt[15] | ((uint)pt[14] << 8) | ((uint)pt[13] << 16) | ((uint)pt[12] << 24);
            r[5] = pt[11] | ((uint)pt[10] << 8) | ((uint)pt[09] << 16) | ((uint)pt[08] << 24);
            r[6] = pt[07] | ((uint)pt[06] << 8) | ((uint)pt[05] << 16) | ((uint)pt[04] << 24);
            r[7] = pt[03] | ((uint)pt[02] << 8) | ((uint)pt[01] << 16) | ((uint)pt[00] << 24);

            uint of = GetOverflow(r);
            Debug.Assert(of == 0 || of == 1);
            Reduce(r, of);
            return of != 0;
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
        private static readonly Scalar8x32 _one = new Scalar8x32(1);
        /// <summary>
        /// Zero
        /// </summary>
        public static ref readonly Scalar8x32 Zero => ref _zero;
        /// <summary>
        /// One
        /// </summary>
        public static ref readonly Scalar8x32 One => ref _one;

        private static readonly Scalar8x32 _mb1 = new Scalar8x32(0x0ABFE4C3U, 0x6F547FA9U, 0x010E8828U, 0xE4437ED6U,
                                                                 0x00000000U, 0x00000000U, 0x00000000U, 0x00000000U);
        private static readonly Scalar8x32 _mb2 = new Scalar8x32(0x3DB1562CU, 0xD765CDA8U, 0x0774346DU, 0x8A280AC5U,
                                                                 0xFFFFFFFEU, 0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU);
        private static readonly Scalar8x32 _g1 = new Scalar8x32(0x45DBB031U, 0xE893209AU, 0x71E8CA7FU, 0x3DAA8A14U,
                                                                0x9284EB15U, 0xE86C90E4U, 0xA7D46BCDU, 0x3086D221U);
        private static readonly Scalar8x32 _g2 = new Scalar8x32(0x8AC47F71U, 0x1571B4AEU, 0x9DF506C6U, 0x221208ACU,
                                                                0x0ABFE4C4U, 0x6F547FA9U, 0x010E8828U, 0xE4437ED6U);
        private static readonly Scalar8x32 _lambda = new Scalar8x32(0x1B23BD72U, 0xDF02967CU, 0x20816678U, 0x122E22EAU,
                                                                    0x8812645AU, 0xA5261C02U, 0xC05C30E0U, 0x5363AD4CU);
        internal static ref readonly Scalar8x32 Minus_b1 => ref _mb1;
        internal static ref readonly Scalar8x32 Minus_b2 => ref _mb2;
        internal static ref readonly Scalar8x32 G1 => ref _g1;
        internal static ref readonly Scalar8x32 G2 => ref _g2;
        /// <summary>
        /// The Secp256k1 curve has an endomorphism, where lambda* (x, y) = (beta* x, y), where lambda is:
        /// </summary>
        public static ref readonly Scalar8x32 Lambda => ref _lambda;

        /// <summary>
        /// Returns if the value is equal to zero
        /// </summary>
        public bool IsZero
        {
            get
            {
                Debug.Assert(GetOverflow(this) == 0);
                return (b0 | b1 | b2 | b3 | b4 | b5 | b6 | b7) == 0;
            }
        }

        /// <summary>
        /// Returns if the value is equal to one
        /// </summary>
        public bool IsOne
        {
            get
            {
                Debug.Assert(GetOverflow(this) == 0);
                return ((b0 ^ 1) | b1 | b2 | b3 | b4 | b5 | b6 | b7) == 0;
            }
        }

        /// <summary>
        /// Returns if the value is even
        /// </summary>
        public bool IsEven
        {
            get
            {
                Debug.Assert(GetOverflow(this) == 0);
                return (b0 & 1) == 0;
            }
        }

        /// <summary>
        /// Returns if this scalar is higher than the group order divided by 2
        /// </summary>
        public bool IsHigh
        {
            get
            {
                Debug.Assert(GetOverflow(this) == 0);

                int yes = 0;
                int no = 0;
                no |= (b7 < NH7 ? 1 : 0);
                yes |= (b7 > NH7 ? 1 : 0) & ~no;
                no |= (b6 < NH6 ? 1 : 0) & ~yes; // No need for a > check
                no |= (b5 < NH5 ? 1 : 0) & ~yes; // No need for a > check
                no |= (b4 < NH4 ? 1 : 0) & ~yes; // No need for a > check
                no |= (b3 < NH3 ? 1 : 0) & ~yes;
                yes |= (b3 > NH3 ? 1 : 0) & ~no;
                no |= (b2 < NH2 ? 1 : 0) & ~yes;
                yes |= (b2 > NH2 ? 1 : 0) & ~no;
                no |= (b1 < NH1 ? 1 : 0) & ~yes;
                yes |= (b1 > NH1 ? 1 : 0) & ~no;
                yes |= (b0 > NH0 ? 1 : 0) & ~no;
                return yes != 0;
            }
        }


        /// <summary>
        /// Creates a new instance of <see cref="Scalar8x32"/> using the given big-endian array
        /// and reduces the result modulo curve order (n). 
        /// Return value indicates validity of the result as a private key.
        /// </summary>
        /// <param name="data">Array to use</param>
        /// <param name="res">Scalar</param>
        /// <returns>True if value was non-zero and smaller than curve order; otherwise false</returns>
        public static bool TrySetPrivateKey(ReadOnlySpan<byte> data, out Scalar8x32 res)
        {
            // secp256k1_scalar_set_b32_seckey
            res = new Scalar8x32(data, out bool overflow);
            Debug.Assert(res.Verify());
            return !overflow && !res.IsZero;
        }


        /// <summary>
        /// Multiply a scalar with the multiplicative inverse of 2
        /// </summary>
        /// <returns></returns>
        public Scalar8x32 Half()
        {
            // Writing `/` for field division and `//` for integer division, we compute
            //
            //   a/2 = (a - (a&1))/2 + (a&1)/2
            //       = (a >> 1) + (a&1 ?    1/2 : 0)
            //       = (a >> 1) + (a&1 ? n//2+1 : 0),
            //
            // where n is the group order and in the last equality we have used 1/2 = n//2+1 (mod n).
            // For n//2, we have the constants SECP256K1_N_H_0, ...
            //
            // This sum does not overflow. The most extreme case is a = -2, the largest odd scalar.
            // Here:
            // - the left summand is:  a >> 1 = (a - a&1)/2 = (n-2-1)//2           = (n-3)//2
            // - the right summand is: a&1 ? n//2+1 : 0 = n//2+1 = (n-1)//2 + 2//2 = (n+1)//2
            // Together they sum to (n-3)//2 + (n+1)//2 = (2n-2)//2 = n - 1, which is less than n.

            // uint32_t mask = -(uint32_t)(a->d[0] & 1U);
            uint mask = (b0 & 1U) * 0xFFFFFFFFU;
            ulong t = (b0 >> 1) | (b1 << 31);
            Debug.Assert(GetOverflow(this) == 0);

            t += (NH0 + 1U) & mask;
            uint r0 = (uint)t; t >>= 32;
            t += (b1 >> 1) | (b2 << 31);
            t += NH1 & mask;
            uint r1 = (uint)t; t >>= 32;
            t += (b2 >> 1) | (b3 << 31);
            t += NH2 & mask;
            uint r2 = (uint)t; t >>= 32;
            t += (b3 >> 1) | (b4 << 31);
            t += NH3 & mask;
            uint r3 = (uint)t; t >>= 32;
            t += (b4 >> 1) | (b5 << 31);
            t += NH4 & mask;
            uint r4 = (uint)t; t >>= 32;
            t += (b5 >> 1) | (b6 << 31);
            t += NH5 & mask;
            uint r5 = (uint)t; t >>= 32;
            t += (b6 >> 1) | (b7 << 31);
            t += NH6 & mask;
            uint r6 = (uint)t; t >>= 32;
            uint r7 = (uint)t + (b7 >> 1) + (NH7 & mask);

            // The line above only computed the bottom 32 bits of r->d[7]. Redo the computation
            // in full 64 bits to make sure the top 32 bits are indeed zero.
            Debug.Assert((t + (b7 >> 1) + (NH7 & mask)) >> 32 == 0);

            Scalar8x32 result = new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
            Debug.Assert(result.Verify());
            return result;
        }


        public bool Verify()
        {
            return CheckOverflow() == 0;
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

        internal static uint GetOverflow(in Scalar8x32 sc)
        {
            uint yes = 0;
            uint no = 0;
            no |= (sc.b7 < N7 ? 1U : 0);
            no |= (sc.b6 < N6 ? 1U : 0);
            no |= (sc.b5 < N5 ? 1U : 0);
            no |= (sc.b4 < N4 ? 1U : 0);
            yes |= (sc.b4 > N4 ? 1U : 0) & ~no;
            no |= (sc.b3 < N3 ? 1U : 0) & ~yes;
            yes |= (sc.b3 > N3 ? 1U : 0) & ~no;
            no |= (sc.b2 < N2 ? 1U : 0) & ~yes;
            yes |= (sc.b2 > N2 ? 1U : 0) & ~no;
            no |= (sc.b1 < N1 ? 1U : 0) & ~yes;
            yes |= (sc.b1 > N1 ? 1U : 0) & ~no;
            yes |= (sc.b0 >= N0 ? 1U : 0) & ~no;
            return yes;
        }


        private static unsafe uint GetOverflow(uint* r)
        {
            uint yes = 0;
            uint no = 0;
            no |= (r[7] < N7 ? 1U : 0);
            no |= (r[6] < N6 ? 1U : 0);
            no |= (r[5] < N5 ? 1U : 0);
            no |= (r[4] < N4 ? 1U : 0);
            yes |= (r[4] > N4 ? 1U : 0) & ~no;
            no |= (r[3] < N3 ? 1U : 0) & ~yes;
            yes |= (r[3] > N3 ? 1U : 0) & ~no;
            no |= (r[2] < N2 ? 1U : 0) & ~yes;
            yes |= (r[2] > N2 ? 1U : 0) & ~no;
            no |= (r[1] < N1 ? 1U : 0) & ~yes;
            yes |= (r[1] > N1 ? 1U : 0) & ~no;
            yes |= (r[0] >= N0 ? 1U : 0) & ~no;
            return yes;
        }

        private static unsafe void Reduce(uint* r, uint overflow)
        {
            Debug.Assert(overflow <= 1);

            ulong t = (ulong)r[0] + (overflow * NC0);
            r[0] = (uint)t; t >>= 32;
            t += (ulong)r[1] + (overflow * NC1);
            r[1] = (uint)t; t >>= 32;
            t += (ulong)r[2] + (overflow * NC2);
            r[2] = (uint)t; t >>= 32;
            t += (ulong)r[3] + (overflow * NC3);
            r[3] = (uint)t; t >>= 32;
            t += (ulong)r[4] + (overflow * NC4);
            r[4] = (uint)t; t >>= 32;
            t += r[5];
            r[5] = (uint)t; t >>= 32;
            t += r[6];
            r[6] = (uint)t; t >>= 32;
            t += r[7];
            r[7] = (uint)t;

            Debug.Assert(GetOverflow(r) == 0);
        }

        /// <summary>
        /// Adds the two scalars together modulo the group order.
        /// </summary>
        /// <param name="other">Other value</param>
        /// <param name="overflow">Returns whether it overflowed</param>
        /// <returns>Result</returns>
        public unsafe Scalar8x32 Add(in Scalar8x32 other, out bool overflow)
        {
            Debug.Assert(GetOverflow(this) == 0);
            Debug.Assert(GetOverflow(other) == 0);

            uint* r = stackalloc uint[8];

            ulong t = (ulong)b0 + other.b0;
            r[0] = (uint)t; t >>= 32;
            t += (ulong)b1 + other.b1;
            r[1] = (uint)t; t >>= 32;
            t += (ulong)b2 + other.b2;
            r[2] = (uint)t; t >>= 32;
            t += (ulong)b3 + other.b3;
            r[3] = (uint)t; t >>= 32;
            t += (ulong)b4 + other.b4;
            r[4] = (uint)t; t >>= 32;
            t += (ulong)b5 + other.b5;
            r[5] = (uint)t; t >>= 32;
            t += (ulong)b6 + other.b6;
            r[6] = (uint)t; t >>= 32;
            t += (ulong)b7 + other.b7;
            r[7] = (uint)t; t >>= 32;

            uint of = GetOverflow(r) + (uint)t;
            overflow = of != 0;

            Debug.Assert(of == 0 || of == 1);
            Reduce(r, of);

            return new Scalar8x32(r);
        }


        /// <summary>
        /// Conditionally add a power of two to this scalar. The result is not allowed to overflow.
        /// </summary>
        /// <param name="bit"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public Scalar8x32 CAddBit(uint bit, uint flag)
        {
            Debug.Assert(GetOverflow(this) == 0);
            Debug.Assert(bit < 256);

            bit += (flag - 1) & 0x100;  // forcing (bit >> 5) > 7 makes this a noop
            ulong t = (ulong)b0 + (((bit >> 5) == 0 ? 1U : 0) << ((int)bit & 0x1F));
            uint r0 = (uint)t; t >>= 32;
            t += (ulong)b1 + (((bit >> 5) == 1 ? 1U : 0) << ((int)bit & 0x1F));
            uint r1 = (uint)t; t >>= 32;
            t += (ulong)b2 + (((bit >> 5) == 2 ? 1U : 0) << ((int)bit & 0x1F));
            uint r2 = (uint)t; t >>= 32;
            t += (ulong)b3 + (((bit >> 5) == 3 ? 1U : 0) << ((int)bit & 0x1F));
            uint r3 = (uint)t; t >>= 32;
            t += (ulong)b4 + (((bit >> 5) == 4 ? 1U : 0) << ((int)bit & 0x1F));
            uint r4 = (uint)t; t >>= 32;
            t += (ulong)b5 + (((bit >> 5) == 5 ? 1U : 0) << ((int)bit & 0x1F));
            uint r5 = (uint)t; t >>= 32;
            t += (ulong)b6 + (((bit >> 5) == 6 ? 1U : 0) << ((int)bit & 0x1F));
            uint r6 = (uint)t; t >>= 32;
            t += (ulong)b7 + (((bit >> 5) == 7 ? 1U : 0) << ((int)bit & 0x1F));
            uint r7 = (uint)t;

            Debug.Assert((t >> 32) == 0);

            Scalar8x32 result = new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
            Debug.Assert(result.Verify());
            return result;
        }


        /// <summary>
        /// Access bits (1 &#60; <paramref name="count"/> &#60;= 32) from a scalar.
        /// All requested bits must belong to the same 32-bit limb.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static unsafe uint GetBits(uint* pt, int offset, int count)
        {
            Debug.Assert(GetOverflow(pt) == 0);
            Debug.Assert(count > 0 && count <= 32);
            Debug.Assert((offset + count - 1) >> 5 == offset >> 5);

            return (pt[offset >> 5] >> (offset & 0x1F)) & (0xFFFFFFFF >> (32 - count));
        }

        /// <summary>
        /// Access bits (1 &#60; <paramref name="count"/> &#60;= 32) from a scalar.
        /// <paramref name="offset"/> + <paramref name="count"/> must be &#60; 256.
        /// Not constant time in offset and count.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static unsafe uint GetBitsVar(uint* pt, int offset, int count)
        {
            Debug.Assert(GetOverflow(pt) == 0);
            Debug.Assert(count > 0 && count <= 32);
            Debug.Assert(offset + count <= 256);

            if ((offset + count - 1) >> 5 == offset >> 5)
            {
                return GetBits(pt, offset, count);
            }
            else
            {
                Debug.Assert((offset >> 5) + 1 < 8);
                return ((pt[offset >> 5] >> (offset & 0x1F)) | (pt[(offset >> 5) + 1] << (32 - (offset & 0x1F)))) &
                       (0xFFFFFFFF >> (32 - count));
            }
        }


        /// <summary>
        /// Returns the inverse of this scalar modulo the group order.
        /// </summary>
        /// <returns>Inverse</returns>
        public Scalar8x32 Inverse()
        {
#if DEBUG
            Debug.Assert(Verify());
            bool zero_in = IsZero;
#endif
            ModInv32Signed30 s = new ModInv32Signed30(this);
            // secp256k1_modinv32(&s, &secp256k1_const_modinfo_scalar);
            ModInv32.Compute(ref s, ModInv32ModInfo.ScalarConstant);
            Scalar8x32 r = s.ToScalar8x32();
#if DEBUG
            Debug.Assert(r.Verify());
            Debug.Assert(r.IsZero == zero_in);
#endif
            return r;
        }

        /// <summary>
        /// Returns the inverse of this scalar modulo the group order, without constant-time guarantee.
        /// </summary>
        /// <returns>Inverse</returns>
        public Scalar8x32 InverseVar()
        {
#if DEBUG
            Debug.Assert(Verify());
            bool zero_in = IsZero;
#endif
            ModInv32Signed30 s = new ModInv32Signed30(this);
            ModInv32.ComputeVar(ref s, ModInv32ModInfo.ScalarConstant);
            Scalar8x32 r = s.ToScalar8x32();

#if DEBUG
            Debug.Assert(r.Verify());
            Debug.Assert(r.IsZero == zero_in);
#endif
            return r;
        }

        /// <summary>
        /// Use Inverse() instead
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use Inverse() instead.")]
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

        /// <summary>
        /// Use InverseVar() instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use InverseVar() instead.")]
        public Scalar8x32 InverseVar_old()
        {
            Debug.Assert(GetOverflow(this) == 0);

            return Inverse_old();
        }


        /// <summary>
        /// Multiply two scalars modulo the group order.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public unsafe Scalar8x32 Multiply(in Scalar8x32 b)
        {
            Debug.Assert(GetOverflow(this) == 0);
            Debug.Assert(GetOverflow(b) == 0);

            uint* l = stackalloc uint[16];
            Mult512(l, this, b);
            return Reduce512(l);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Muladd(uint a, uint b, ref uint c0, ref uint c1, ref uint c2)
        {
            ulong t = (ulong)a * b;
            uint th = (uint)(t >> 32);
            uint tl = (uint)t;

            c0 += tl;                    // overflow is handled on the next line
            th += (c0 < tl) ? 1U : 0U;   // at most 0xFFFFFFFF
            c1 += th;                    // overflow is handled on the next line
            c2 += (c1 < th) ? 1U : 0U;   // never overflows by contract (verified in the next line)

            Debug.Assert((c1 >= th) || (c2 != 0));
        }

        // Add a*b to the number defined by (c0,c1). c1 must never overflow.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MuladdFast(uint a, uint b, ref uint c0, ref uint c1)
        {
            ulong t = (ulong)a * b;
            uint th = (uint)(t >> 32);   // at most 0xFFFFFFFE
            uint tl = (uint)t;

            c0 += tl;                    // overflow is handled on the next line
            th += (c0 < tl) ? 1U : 0U;   // at most 0xFFFFFFFF
            c1 += th;                    // never overflows by contract (verified in the next line)

            Debug.Assert(c1 >= th);
        }

        // Add a to the number defined by (c0,c1,c2). c2 must never overflow.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SumAdd(uint a, ref uint c0, ref uint c1, ref uint c2)
        {
            c0 += a;                        // overflow is handled on the next line
            uint over = (c0 < a) ? 1U : 0U;
            c1 += over;                     // overflow is handled on the next line
            c2 += (c1 < over) ? 1U : 0U;    // never overflows by contract

            c1 &= uint.MaxValue;
        }

        // Add a to the number defined by (c0,c1). c1 must never overflow, c2 must be zero.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SumaddFast(uint a, ref uint c0, ref uint c1, ref uint c2)
        {
            c0 += a;                        // overflow is handled on the next line
            c1 += (c0 < a) ? 1U : 0U;       // never overflows by contract (verified the next line)

            Debug.Assert((c1 != 0) | (c0 >= a));
            Debug.Assert(c2 == 0);
        }

        // Extract the lowest 32 bits of (c0,c1,c2) into n, and left shift the number 32 bits.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Extract(ref uint n, ref uint c0, ref uint c1, ref uint c2)
        {
            n = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
        }

        // Extract the lowest 32 bits of (c0,c1,c2) into n, and left shift the number 32 bits. c2 is required to be zero.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExtractFast(ref uint n, ref uint c0, ref uint c1, ref uint c2)
        {
            n = c0;
            c0 = c1;
            c1 = 0;

            Debug.Assert(c2 == 0);
        }

        private static unsafe void Mult512(uint* l, in Scalar8x32 a, in Scalar8x32 b)
        {
            // 96 bit accumulator
            uint c0 = 0, c1 = 0, c2 = 0;

            // l[0..15] = a[0..7] * b[0..7]
            MuladdFast(a.b0, b.b0, ref c0, ref c1);
            ExtractFast(ref l[0], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l[1], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l[2], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l[3], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b4, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b4, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l[4], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b5, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b4, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b4, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b5, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l[5], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b6, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b5, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b4, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b4, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b5, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b6, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l[6], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b7, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b6, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b5, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b4, ref c0, ref c1, ref c2);
            Muladd(a.b4, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b5, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b6, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b7, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l[7], ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b7, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b6, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b5, ref c0, ref c1, ref c2);
            Muladd(a.b4, b.b4, ref c0, ref c1, ref c2);
            Muladd(a.b5, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b6, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b7, b.b1, ref c0, ref c1, ref c2);
            Extract(ref l[8], ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b7, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b6, ref c0, ref c1, ref c2);
            Muladd(a.b4, b.b5, ref c0, ref c1, ref c2);
            Muladd(a.b5, b.b4, ref c0, ref c1, ref c2);
            Muladd(a.b6, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b7, b.b2, ref c0, ref c1, ref c2);
            Extract(ref l[9], ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b7, ref c0, ref c1, ref c2);
            Muladd(a.b4, b.b6, ref c0, ref c1, ref c2);
            Muladd(a.b5, b.b5, ref c0, ref c1, ref c2);
            Muladd(a.b6, b.b4, ref c0, ref c1, ref c2);
            Muladd(a.b7, b.b3, ref c0, ref c1, ref c2);
            Extract(ref l[10], ref c0, ref c1, ref c2);
            Muladd(a.b4, b.b7, ref c0, ref c1, ref c2);
            Muladd(a.b5, b.b6, ref c0, ref c1, ref c2);
            Muladd(a.b6, b.b5, ref c0, ref c1, ref c2);
            Muladd(a.b7, b.b4, ref c0, ref c1, ref c2);
            Extract(ref l[11], ref c0, ref c1, ref c2);
            Muladd(a.b5, b.b7, ref c0, ref c1, ref c2);
            Muladd(a.b6, b.b6, ref c0, ref c1, ref c2);
            Muladd(a.b7, b.b5, ref c0, ref c1, ref c2);
            Extract(ref l[12], ref c0, ref c1, ref c2);
            Muladd(a.b6, b.b7, ref c0, ref c1, ref c2);
            Muladd(a.b7, b.b6, ref c0, ref c1, ref c2);
            Extract(ref l[13], ref c0, ref c1, ref c2);
            MuladdFast(a.b7, b.b7, ref c0, ref c1);
            ExtractFast(ref l[14], ref c0, ref c1, ref c2);
            Debug.Assert(c1 == 0);
            l[15] = c0;
        }

        private static unsafe Scalar8x32 Reduce512(uint* l)
        {
            ulong c;
            uint n0 = l[8], n1 = l[9], n2 = l[10], n3 = l[11], n4 = l[12], n5 = l[13], n6 = l[14], n7 = l[15];
            uint m0 = 0, m1 = 0, m2 = 0, m3 = 0, m4 = 0, m5 = 0, m6 = 0, m7 = 0, m8 = 0, m9 = 0, m10 = 0, m11 = 0, m12 = 0;
            uint p0 = 0, p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0, p6 = 0, p7 = 0, p8 = 0;

            // 96 bit accumulator
            uint c0, c1, c2;

            // Reduce 512 bits into 385
            // m[0..12] = l[0..7] + n[0..7] * NC
            c0 = l[0]; c1 = 0; c2 = 0;
            MuladdFast(n0, NC0, ref c0, ref c1);
            ExtractFast(ref m0, ref c0, ref c1, ref c2);
            SumaddFast(l[1], ref c0, ref c1, ref c2);
            Muladd(n1, NC0, ref c0, ref c1, ref c2);
            Muladd(n0, NC1, ref c0, ref c1, ref c2);
            Extract(ref m1, ref c0, ref c1, ref c2);
            SumAdd(l[2], ref c0, ref c1, ref c2);
            Muladd(n2, NC0, ref c0, ref c1, ref c2);
            Muladd(n1, NC1, ref c0, ref c1, ref c2);
            Muladd(n0, NC2, ref c0, ref c1, ref c2);
            Extract(ref m2, ref c0, ref c1, ref c2);
            SumAdd(l[3], ref c0, ref c1, ref c2);
            Muladd(n3, NC0, ref c0, ref c1, ref c2);
            Muladd(n2, NC1, ref c0, ref c1, ref c2);
            Muladd(n1, NC2, ref c0, ref c1, ref c2);
            Muladd(n0, NC3, ref c0, ref c1, ref c2);
            Extract(ref m3, ref c0, ref c1, ref c2);
            SumAdd(l[4], ref c0, ref c1, ref c2);
            Muladd(n4, NC0, ref c0, ref c1, ref c2);
            Muladd(n3, NC1, ref c0, ref c1, ref c2);
            Muladd(n2, NC2, ref c0, ref c1, ref c2);
            Muladd(n1, NC3, ref c0, ref c1, ref c2);
            SumAdd(n0, ref c0, ref c1, ref c2);
            Extract(ref m4, ref c0, ref c1, ref c2);
            SumAdd(l[5], ref c0, ref c1, ref c2);
            Muladd(n5, NC0, ref c0, ref c1, ref c2);
            Muladd(n4, NC1, ref c0, ref c1, ref c2);
            Muladd(n3, NC2, ref c0, ref c1, ref c2);
            Muladd(n2, NC3, ref c0, ref c1, ref c2);
            SumAdd(n1, ref c0, ref c1, ref c2);
            Extract(ref m5, ref c0, ref c1, ref c2);
            SumAdd(l[6], ref c0, ref c1, ref c2);
            Muladd(n6, NC0, ref c0, ref c1, ref c2);
            Muladd(n5, NC1, ref c0, ref c1, ref c2);
            Muladd(n4, NC2, ref c0, ref c1, ref c2);
            Muladd(n3, NC3, ref c0, ref c1, ref c2);
            SumAdd(n2, ref c0, ref c1, ref c2);
            Extract(ref m6, ref c0, ref c1, ref c2);
            SumAdd(l[7], ref c0, ref c1, ref c2);
            Muladd(n7, NC0, ref c0, ref c1, ref c2);
            Muladd(n6, NC1, ref c0, ref c1, ref c2);
            Muladd(n5, NC2, ref c0, ref c1, ref c2);
            Muladd(n4, NC3, ref c0, ref c1, ref c2);
            SumAdd(n3, ref c0, ref c1, ref c2);
            Extract(ref m7, ref c0, ref c1, ref c2);
            Muladd(n7, NC1, ref c0, ref c1, ref c2);
            Muladd(n6, NC2, ref c0, ref c1, ref c2);
            Muladd(n5, NC3, ref c0, ref c1, ref c2);
            SumAdd(n4, ref c0, ref c1, ref c2);
            Extract(ref m8, ref c0, ref c1, ref c2);
            Muladd(n7, NC2, ref c0, ref c1, ref c2);
            Muladd(n6, NC3, ref c0, ref c1, ref c2);
            SumAdd(n5, ref c0, ref c1, ref c2);
            Extract(ref m9, ref c0, ref c1, ref c2);
            Muladd(n7, NC3, ref c0, ref c1, ref c2);
            SumAdd(n6, ref c0, ref c1, ref c2);
            Extract(ref m10, ref c0, ref c1, ref c2);
            SumaddFast(n7, ref c0, ref c1, ref c2);
            ExtractFast(ref m11, ref c0, ref c1, ref c2);
            Debug.Assert(c0 <= 1);
            m12 = c0;

            // Reduce 385 bits into 258
            // p[0..8] = m[0..7] + m[8..12] * NC
            c0 = m0; c1 = 0; c2 = 0;
            MuladdFast(m8, NC0, ref c0, ref c1);
            ExtractFast(ref p0, ref c0, ref c1, ref c2);
            SumaddFast(m1, ref c0, ref c1, ref c2);
            Muladd(m9, NC0, ref c0, ref c1, ref c2);
            Muladd(m8, NC1, ref c0, ref c1, ref c2);
            Extract(ref p1, ref c0, ref c1, ref c2);
            SumAdd(m2, ref c0, ref c1, ref c2);
            Muladd(m10, NC0, ref c0, ref c1, ref c2);
            Muladd(m9, NC1, ref c0, ref c1, ref c2);
            Muladd(m8, NC2, ref c0, ref c1, ref c2);
            Extract(ref p2, ref c0, ref c1, ref c2);
            SumAdd(m3, ref c0, ref c1, ref c2);
            Muladd(m11, NC0, ref c0, ref c1, ref c2);
            Muladd(m10, NC1, ref c0, ref c1, ref c2);
            Muladd(m9, NC2, ref c0, ref c1, ref c2);
            Muladd(m8, NC3, ref c0, ref c1, ref c2);
            Extract(ref p3, ref c0, ref c1, ref c2);
            SumAdd(m4, ref c0, ref c1, ref c2);
            Muladd(m12, NC0, ref c0, ref c1, ref c2);
            Muladd(m11, NC1, ref c0, ref c1, ref c2);
            Muladd(m10, NC2, ref c0, ref c1, ref c2);
            Muladd(m9, NC3, ref c0, ref c1, ref c2);
            SumAdd(m8, ref c0, ref c1, ref c2);
            Extract(ref p4, ref c0, ref c1, ref c2);
            SumAdd(m5, ref c0, ref c1, ref c2);
            Muladd(m12, NC1, ref c0, ref c1, ref c2);
            Muladd(m11, NC2, ref c0, ref c1, ref c2);
            Muladd(m10, NC3, ref c0, ref c1, ref c2);
            SumAdd(m9, ref c0, ref c1, ref c2);
            Extract(ref p5, ref c0, ref c1, ref c2);
            SumAdd(m6, ref c0, ref c1, ref c2);
            Muladd(m12, NC2, ref c0, ref c1, ref c2);
            Muladd(m11, NC3, ref c0, ref c1, ref c2);
            SumAdd(m10, ref c0, ref c1, ref c2);
            Extract(ref p6, ref c0, ref c1, ref c2);
            SumaddFast(m7, ref c0, ref c1, ref c2);
            MuladdFast(m12, NC3, ref c0, ref c1);
            SumaddFast(m11, ref c0, ref c1, ref c2);
            ExtractFast(ref p7, ref c0, ref c1, ref c2);
            p8 = c0 + m12;
            Debug.Assert(p8 <= 2);

            // Reduce 258 bits into 256
            // r[0..7] = p[0..7] + p[8] * NC
            c = p0 + (ulong)NC0 * p8;
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
            c += p7;
            p7 = (uint)c; c >>= 32;

            Scalar8x32 r = new Scalar8x32(p0, p1, p2, p3, p4, p5, p6, p7);

            // Final reduction of r
            return Reduce(r, (uint)c + GetOverflow(r));
        }

        private static Scalar8x32 Reduce(in Scalar8x32 r, uint overflow)
        {
            Debug.Assert(overflow <= 1);

            ulong t = (ulong)r.b0 + (overflow * NC0);
            uint r0 = (uint)t; t >>= 32;
            t += (ulong)r.b1 + (overflow * NC1);
            uint r1 = (uint)t; t >>= 32;
            t += (ulong)r.b2 + (overflow * NC2);
            uint r2 = (uint)t; t >>= 32;
            t += (ulong)r.b3 + (overflow * NC3);
            uint r3 = (uint)t; t >>= 32;
            t += (ulong)r.b4 + (overflow * NC4);
            uint r4 = (uint)t; t >>= 32;
            t += r.b5;
            uint r5 = (uint)t; t >>= 32;
            t += r.b6;
            uint r6 = (uint)t; t >>= 32;
            t += r.b7;
            uint r7 = (uint)t;

            Scalar8x32 result = new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
            Debug.Assert(result.Verify());
            return result;
        }


        /// <summary>
        /// Multiply a and b (without taking the modulus!), divide by 2**shift, and round to the nearest integer.
        /// Shift must be at least 256
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <param name="shift">Shift must be at least 256</param>
        /// <returns>Result</returns>
        public static unsafe Scalar8x32 MulShiftVar(in Scalar8x32 a, in Scalar8x32 b, int shift)
        {
            Debug.Assert(GetOverflow(a) == 0);
            Debug.Assert(GetOverflow(b) == 0);
            Debug.Assert(shift >= 256);

            uint* l = stackalloc uint[16];
            Mult512(l, a, b);

            int shLimbs = shift >> 5;
            int shiftlow = shift & 0x1F;
            int shifthigh = 32 - shiftlow;
            bool sb = shiftlow != 0;

            uint r0 = shift < 512 ? (l[0 + shLimbs] >> shiftlow | (shift < 480 && sb ? (l[1 + shLimbs] << shifthigh) : 0)) : 0;
            uint r1 = shift < 480 ? (l[1 + shLimbs] >> shiftlow | (shift < 448 && sb ? (l[2 + shLimbs] << shifthigh) : 0)) : 0;
            uint r2 = shift < 448 ? (l[2 + shLimbs] >> shiftlow | (shift < 416 && sb ? (l[3 + shLimbs] << shifthigh) : 0)) : 0;
            uint r3 = shift < 416 ? (l[3 + shLimbs] >> shiftlow | (shift < 384 && sb ? (l[4 + shLimbs] << shifthigh) : 0)) : 0;
            uint r4 = shift < 384 ? (l[4 + shLimbs] >> shiftlow | (shift < 352 && sb ? (l[5 + shLimbs] << shifthigh) : 0)) : 0;
            uint r5 = shift < 352 ? (l[5 + shLimbs] >> shiftlow | (shift < 320 && sb ? (l[6 + shLimbs] << shifthigh) : 0)) : 0;
            uint r6 = shift < 320 ? (l[6 + shLimbs] >> shiftlow | (shift < 288 && sb ? (l[7 + shLimbs] << shifthigh) : 0)) : 0;
            uint r7 = shift < 288 ? (l[7 + shLimbs] >> shiftlow) : 0;

            Scalar8x32 r = new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
            Debug.Assert(r.Verify());
            return r.CAddBit(0, (l[(shift - 1) >> 5] >> ((shift - 1) & 0x1f)) & 1);
        }

        /// <summary>
        /// Returns the complement of this scalar modulo the group order.
        /// </summary>
        /// <returns></returns>
        public Scalar8x32 Negate()
        {
            Debug.Assert(GetOverflow(this) == 0);

            // uint32_t nonzero = 0xFFFFFFFFUL * (secp256k1_scalar_is_zero(a) == 0);
            // Instead of a branch to get 1/0 then multiply it by the constant we use branch to get the resulting constant directly
            // ie. we skip multiplication (optimization effect is minuscule though!)
            uint nonzero = IsZero ? 0 : 0xFFFFFFFFU;
            ulong t = (ulong)(~b0) + (N0 + 1);
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

            Scalar8x32 result = new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
            Debug.Assert(result.Verify());
            return result;
        }

        /// <summary>
        /// Returns the conditional complement of this scalar modulo the group order.
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="result"></param>
        /// <returns>-1 if the number was negated; otherwise 1.</returns>
        public int NegateConditional(int flag, out Scalar8x32 result)
        {
            Debug.Assert(GetOverflow(this) == 0);

            // If flag = 0 then mask = 00...00 so this is a no-op
            // if flag = 1 then mask = 11...11 so this is identical Negate()
            uint mask = (uint)-flag;
            // nonzero is computed same as above
            uint nonzero = IsZero ? 0 : 0xFFFFFFFFU;
            ulong t = (ulong)(b0 ^ mask) + ((N0 + 1) & mask);
            uint r0 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(b1 ^ mask) + (N1 & mask);
            uint r1 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(b2 ^ mask) + (N2 & mask);
            uint r2 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(b3 ^ mask) + (N3 & mask);
            uint r3 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(b4 ^ mask) + (N4 & mask);
            uint r4 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(b5 ^ mask) + (N5 & mask);
            uint r5 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(b6 ^ mask) + (N6 & mask);
            uint r6 = (uint)(t & nonzero); t >>= 32;
            t += (ulong)(b7 ^ mask) + (N7 & mask);
            uint r7 = (uint)(t & nonzero);

            result = new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
            Debug.Assert(result.Verify());
            // return 2 * (mask == 0) - 1;
            return mask == 0 ? 1 : -1;
        }


        /// <summary>
        /// Conditional move. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is true (=1).
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="flag">Zero or one. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is one.</param>
        /// <returns></returns>
        public static Scalar8x32 CMov(in Scalar8x32 r, in Scalar8x32 a, uint flag)
        {
            Debug.Assert(GetOverflow(r) == 0);
            Debug.Assert(GetOverflow(a) == 0);

            uint mask0 = flag + ~0U;
            uint mask1 = ~mask0;
            uint r0 = (r.b0 & mask0) | (a.b0 & mask1);
            uint r1 = (r.b1 & mask0) | (a.b1 & mask1);
            uint r2 = (r.b2 & mask0) | (a.b2 & mask1);
            uint r3 = (r.b3 & mask0) | (a.b3 & mask1);
            uint r4 = (r.b4 & mask0) | (a.b4 & mask1);
            uint r5 = (r.b5 & mask0) | (a.b5 & mask1);
            uint r6 = (r.b6 & mask0) | (a.b6 & mask1);
            uint r7 = (r.b7 & mask0) | (a.b7 & mask1);

            Scalar8x32 result = new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
            Debug.Assert(result.Verify());
            return result;
        }


        /// <summary>
        /// Find r1 and r2 such that r1+r2*2^128 = k
        /// </summary>
        /// <param name="k"></param>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        public static void Split128(in Scalar8x32 k, out Scalar8x32 r1, out Scalar8x32 r2)
        {
            Debug.Assert(GetOverflow(k) == 0);

            r1 = new Scalar8x32(k.b0, k.b1, k.b2, k.b3, 0, 0, 0, 0);
            r2 = new Scalar8x32(k.b4, k.b5, k.b6, k.b7, 0, 0, 0, 0);

            Debug.Assert(r1.Verify());
            Debug.Assert(r2.Verify());
        }

        /// <summary>
        /// Find r1 and r2 such that r1+r2*lambda = k, where r1 and r2 or their negations are
        /// maximum 128 bits long (see <see cref="Point.MulLambda"/>).
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="k"></param>
        internal static void SplitLambda(out Scalar8x32 r1, out Scalar8x32 r2, in Scalar8x32 k)
        {
            Debug.Assert(GetOverflow(k) == 0);

            // these *Var calls are constant time since the shift amount is constant
            Scalar8x32 c1 = MulShiftVar(k, G1, 384);
            Scalar8x32 c2 = MulShiftVar(k, G2, 384);
            c1 = c1.Multiply(Minus_b1);
            c2 = c2.Multiply(Minus_b2);
            r2 = c1.Add(c2, out _);
            r1 = r2.Multiply(Lambda);
            r1 = r1.Negate();
            r1 = r1.Add(k, out _);

            Debug.Assert(GetOverflow(r1) == 0);
            Debug.Assert(GetOverflow(r2) == 0);
#if DEBUG
            SplitLambdaVerify(r1, r2, k);
#endif
        }

#if DEBUG
        private static void SplitLambdaVerify(in Scalar8x32 r1, in Scalar8x32 r2, in Scalar8x32 k)
        {
            // (a1 + a2 + 1)/2 is 0xa2a8918ca85bafe22016d0b917e4dd77
            Span<byte> k1_bound = new byte[32]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0xa2, 0xa8, 0x91, 0x8c, 0xa8, 0x5b, 0xaf, 0xe2, 0x20, 0x16, 0xd0, 0xb9, 0x17, 0xe4, 0xdd, 0x77
            };

            // (-b1 + b2)/2 + 1 is 0x8a65287bd47179fb2be08846cea267ed
            Span<byte> k2_bound = new byte[32]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x8a, 0x65, 0x28, 0x7b, 0xd4, 0x71, 0x79, 0xfb, 0x2b, 0xe0, 0x88, 0x46, 0xce, 0xa2, 0x67, 0xed
            };

            Scalar8x32 s = Lambda.Multiply(r2);
            s = s.Add(r1, out _);

            Debug.Assert(s.Equals(k));

            s = r1.Negate();
            byte[] buf1 = r1.ToByteArray();
            byte[] buf2 = s.ToByteArray();

            Debug.Assert(MemCmpVar(buf1, k1_bound, 32) < 0 || MemCmpVar(buf2, k1_bound, 32) < 0);

            s = r2.Negate();
            buf1 = r2.ToByteArray();
            buf2 = s.ToByteArray();

            Debug.Assert(MemCmpVar(buf1, k2_bound, 32) < 0 || MemCmpVar(buf2, k2_bound, 32) < 0);
        }

        // https://github.com/bitcoin-core/secp256k1/blob/b314cf28334a91db2fe144d04f86077e2bfd7a25/src/util.h#L212-L228
        private static int MemCmpVar(ReadOnlySpan<byte> s1, ReadOnlySpan<byte> s2, int n)
        {
            for (int i = 0; i < n; i++)
            {
                int diff = s1[i] - s2[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return 0;
        }
#endif // DEBUG


        /// <summary>
        /// Returns byte array representation of this instance
        /// </summary>
        /// <returns>32 bytes</returns>
        public byte[] ToByteArray()
        {
            Debug.Assert(GetOverflow(this) == 0);

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

            Debug.Assert(GetOverflow(this) == 0);

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
        public static bool operator ==(in Scalar8x32 left, in Scalar8x32 right)
        {
            Debug.Assert(GetOverflow(left) == 0);
            Debug.Assert(GetOverflow(right) == 0);

            return ((left.b0 ^ right.b0) | (left.b1 ^ right.b1) | (left.b2 ^ right.b2) | (left.b3 ^ right.b3) |
                    (left.b4 ^ right.b4) | (left.b5 ^ right.b5) | (left.b6 ^ right.b6) | (left.b7 ^ right.b7)) == 0;
        }

        /// <summary>
        /// Returns if the two scalars are not equal to each other
        /// </summary>
        /// <param name="left">First scalar</param>
        /// <param name="right">Second scalar</param>
        /// <returns>True if the two scalars are not equal; otherwise false.</returns>
        public static bool operator !=(in Scalar8x32 left, in Scalar8x32 right) => !(left == right);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(b0, b1, b2, b3, b4, b5, b6, b7);

        /// <inheritdoc/>
        public override string ToString() => $"0x{ToByteArray().ToBase16()}";
    }
}
