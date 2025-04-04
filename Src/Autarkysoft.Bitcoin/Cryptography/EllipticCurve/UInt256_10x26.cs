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
    /// 256-bit unsigned integer used as field elements, implemented using radix-2^26 representation (instead of 2^32)
    /// in little-endian order.
    /// <para/>integer = sum(i=0..9, b[i]*2^(i*26)) % p [where p = 2^256 - 0x1000003D1]
    /// <para/>integer value can exceed P unless it's normalized
    /// </summary>
    /// <remarks>
    /// This implements a UInt256 using 10x UInt32 limbs (total of 320 bits).
    /// When normalized, each limb stores 26 bits except the last one that stores 22 bits.
    /// <para/>The arithmetic here is all modulo secp256k1 prime
    /// </remarks>
    [DebuggerDisplay("{ToByteArray().ToBase16()}")]
    public readonly struct UInt256_10x26
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given unsigned 32-bit integer.
        /// </summary>
        /// <param name="a"></param>
        public UInt256_10x26(uint a)
        {
            b0 = a;
            b1 = 0; b2 = 0; b3 = 0; b4 = 0;
            b5 = 0; b6 = 0; b7 = 0; b8 = 0; b9 = 0;
#if DEBUG
            magnitude = (a == 0) ? 0 : 1;
            isNormalized = true;
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given parameters.
        /// </summary>
        /// <param name="u0">1st 32 bits (least significant)</param>
        /// <param name="u1">2nd 32 bits</param>
        /// <param name="u2">3rd 32 bits</param>
        /// <param name="u3">4th 32 bits</param>
        /// <param name="u4">5th 32 bits</param>
        /// <param name="u5">6th 32 bits</param>
        /// <param name="u6">7th 32 bits</param>
        /// <param name="u7">8th 32 bits (most significant)</param>
        public UInt256_10x26(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7)
        {
            // 26 bits uint_0 -> remaining=6 (=32-26)
            b0 = u0 & 0b00000011_11111111_11111111_11111111U;
            // 6 bits uint_0 + 20 bits uint_1 (total=26-bit) -> rem=12(=32-20)
            b1 = (u0 >> 26) | ((u1 & 0b00000000_00001111_11111111_11111111U) << 6);
            // 12 bits uint_1 + 14 bits uint_2 -> rem=18
            b2 = (u1 >> 20) | ((u2 & 0b00000000_00000000_00111111_11111111U) << 12);
            // 18 bits uint_2 + 8 bits uint_3 -> rem = 24
            b3 = (u2 >> 14) | ((u3 & 0b00000000_00000000_00000000_11111111U) << 18);
            // 24 bits uint_3 + 2 bits uint_4 -> rem=30
            b4 = (u3 >> 8) | ((u4 & 0b00000000_00000000_00000000_00000011U) << 24);
            // 26 bits uint_4 -> rem=4 (from remaining 30)
            b5 = (u4 >> 2) & 0b00000011_11111111_11111111_11111111U;
            // 4 bits uint_4 + 22 bits uint_5 -> rem=10
            b6 = (u4 >> 28) | ((u5 & 0b00000000_00111111_11111111_11111111U) << 4);
            // 10 bits uint_5 + 16 bits uint_6 -> rem=16
            b7 = (u5 >> 22) | ((u6 & 0b00000000_00000000_11111111_11111111U) << 10);
            // 16 bits uint_6 + 10 bits uint_7 -> rem=22
            b8 = (u6 >> 16) | ((u7 & 0b00000000_00000000_00000011_11111111U) << 16);
            // 22 bits uint_7
            b9 = u7 >> 10;
#if DEBUG
            magnitude = ((b0 | b1 | b2 | b3 | b4 | b5 | b6 | b7 | b8 | b9) == 0) ? 0 : 1;
            isNormalized = !((b7 & b6 & b5 & b4 & b3 & b2) == 0xffffffffU &&
                             (b1 == 0xffffffffU || (b1 == 0xfffffffeU && (b0 >= 0xfffffc2fU))));
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given parameters.
        /// </summary>
        /// <param name="u0">1st 26 bits (least significant)</param>
        /// <param name="u1">2nd 26 bits</param>
        /// <param name="u2">3rd 26 bits</param>
        /// <param name="u3">4th 26 bits</param>
        /// <param name="u4">5th 26 bits</param>
        /// <param name="u5">6th 26 bits</param>
        /// <param name="u6">7th 26 bits</param>
        /// <param name="u7">8th 26 bits</param>
        /// <param name="u8">9th 26 bits</param>
        /// <param name="u9">10th 22 bits (most significant)</param>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="normalized">Is normalized</param>
        public UInt256_10x26(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7, uint u8, uint u9
#if DEBUG
            , int magnitude, bool normalized
#endif
            )
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3; b4 = u4;
            b5 = u5; b6 = u6; b7 = u7; b8 = u8; b9 = u9;
#if DEBUG
            this.magnitude = magnitude;
            isNormalized = normalized;
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given array.
        /// </summary>
        /// <param name="arr10">Array</param>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="normalized">Is normalized</param>
        public UInt256_10x26(ReadOnlySpan<uint> arr10
#if DEBUG
            , int magnitude, bool normalized
#endif
            )
        {
            Debug.Assert(arr10.Length == 10);

            b0 = arr10[0]; b1 = arr10[1]; b2 = arr10[2]; b3 = arr10[3]; b4 = arr10[4];
            b5 = arr10[5]; b6 = arr10[6]; b7 = arr10[7]; b8 = arr10[8]; b9 = arr10[9];
#if DEBUG
            this.magnitude = magnitude;
            isNormalized = normalized;
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given 32-byte big-endian array.
        /// </summary>
        /// <param name="ba32">32-byte array</param>
        /// <param name="isValid"></param>
        public UInt256_10x26(ReadOnlySpan<byte> ba32, out bool isValid)
        {
            // This is the same as secp256k1_fe_impl_set_b32_limit
            Debug.Assert(ba32.Length == 32);

            // 8 + 8 + 8 + 2
            b0 = (uint)(ba32[31] | (ba32[30] << 8) | (ba32[29] << 16) | ((ba32[28] & 0b00000011) << 24));
            // 6 + 8 + 8 + 4
            b1 = (uint)((ba32[28] >> 2) | (ba32[27] << 6) | (ba32[26] << 14) | ((ba32[25] & 0b00001111) << 22));
            // 4 + 8 + 8 + 6
            b2 = (uint)((ba32[25] >> 4) | (ba32[24] << 4) | (ba32[23] << 12) | ((ba32[22] & 0b00111111) << 20));
            // 2 + 8 + 8 + 8
            b3 = (uint)((ba32[22] >> 6) | (ba32[21] << 2) | (ba32[20] << 10) | (ba32[19] << 18));
            // 8 + 8 + 8 + 2
            b4 = (uint)(ba32[18] | (ba32[17] << 8) | (ba32[16] << 16) | ((ba32[15] & 0b00000011) << 24));
            // 6 + 8 + 8 + 4
            b5 = (uint)((ba32[15] >> 2) | (ba32[14] << 6) | (ba32[13] << 14) | ((ba32[12] & 0b00001111) << 22));
            // 4 + 8 + 8 + 6
            b6 = (uint)((ba32[12] >> 4) | (ba32[11] << 4) | (ba32[10] << 12) | ((ba32[9] & 0b00111111) << 20));
            // 2 + 8 + 8 + 8
            b7 = (uint)((ba32[9] >> 6) | (ba32[8] << 2) | (ba32[7] << 10) | (ba32[6] << 18));
            // 8 + 8 + 8 + 2
            b8 = (uint)(ba32[5] | (ba32[4] << 8) | (ba32[3] << 16) | ((ba32[2] & 0b00000011) << 24));
            // 6 + 8 + 8 (last item is only 22 bits)
            b9 = (uint)((ba32[2] >> 2) | (ba32[1] << 6) | (ba32[0] << 14));

            isValid = !((b9 == 0x3FFFFFU) & ((b8 & b7 & b6 & b5 & b4 & b3 & b2) == 0x03FFFFFFU) &
                        ((b1 + 0x40U + ((b0 + 0x03D1U) >> 26)) > 0x03FFFFFFU));
#if DEBUG
            magnitude = 1;
            isNormalized = isValid;
            if (isValid)
            {
                Verify();
            }
#endif
        }


        public UInt256_10x26(ReadOnlySpan<byte> ba32)
        {
            // This is the same as secp256k1_fe_impl_set_b32_mod
            Debug.Assert(ba32.Length == 32);

            // 8 + 8 + 8 + 2
            b0 = (uint)(ba32[31] | (ba32[30] << 8) | (ba32[29] << 16) | ((ba32[28] & 0b00000011) << 24));
            // 6 + 8 + 8 + 4
            b1 = (uint)((ba32[28] >> 2) | (ba32[27] << 6) | (ba32[26] << 14) | ((ba32[25] & 0b00001111) << 22));
            // 4 + 8 + 8 + 6
            b2 = (uint)((ba32[25] >> 4) | (ba32[24] << 4) | (ba32[23] << 12) | ((ba32[22] & 0b00111111) << 20));
            // 2 + 8 + 8 + 8
            b3 = (uint)((ba32[22] >> 6) | (ba32[21] << 2) | (ba32[20] << 10) | (ba32[19] << 18));
            // 8 + 8 + 8 + 2
            b4 = (uint)(ba32[18] | (ba32[17] << 8) | (ba32[16] << 16) | ((ba32[15] & 0b00000011) << 24));
            // 6 + 8 + 8 + 4
            b5 = (uint)((ba32[15] >> 2) | (ba32[14] << 6) | (ba32[13] << 14) | ((ba32[12] & 0b00001111) << 22));
            // 4 + 8 + 8 + 6
            b6 = (uint)((ba32[12] >> 4) | (ba32[11] << 4) | (ba32[10] << 12) | ((ba32[9] & 0b00111111) << 20));
            // 2 + 8 + 8 + 8
            b7 = (uint)((ba32[9] >> 6) | (ba32[8] << 2) | (ba32[7] << 10) | (ba32[6] << 18));
            // 8 + 8 + 8 + 2
            b8 = (uint)(ba32[5] | (ba32[4] << 8) | (ba32[3] << 16) | ((ba32[2] & 0b00000011) << 24));
            // 6 + 8 + 8 (last item is only 22 bits)
            b9 = (uint)((ba32[2] >> 2) | (ba32[1] << 6) | (ba32[0] << 14));
#if DEBUG
            magnitude = 1;
            isNormalized = false;
#endif
        }


        /// <summary>
        /// Return a field element with magnitude m, normalized if (and only if) m==0.
        /// The value is chosen so that it is likely to trigger edge cases related to
        /// internal overflows.
        /// </summary>
        public static UInt256_10x26 GetBounds(uint m)
        {
            Debug.Assert(m >= 0);
            Debug.Assert(m <= 32);

            UInt256_10x26 r = new UInt256_10x26(
                0x3FFFFFFU * 2 * m,
                0x3FFFFFFU * 2 * m,
                0x3FFFFFFU * 2 * m,
                0x3FFFFFFU * 2 * m,
                0x3FFFFFFU * 2 * m,
                0x3FFFFFFU * 2 * m,
                0x3FFFFFFU * 2 * m,
                0x3FFFFFFU * 2 * m,
                0x3FFFFFFU * 2 * m,
                0x03FFFFFU * 2 * m
#if DEBUG
                , (int)m, m == 0
#endif
                );
#if DEBUG
            r.Verify();
#endif
            return r;
        }


        /// <summary>
        /// Bit chunks
        /// </summary>
        public readonly uint b0, b1, b2, b3, b4, b5, b6, b7, b8, b9;

#if DEBUG
        /// <summary>
        /// Magnitude means:
        /// <para/>2*M*(2^22-1) is the max (inclusive) of the most significant limb
        /// <para/>2*M*(2^26-1) is the max (inclusive) of the remaining limbs
        /// </summary>
        public readonly int magnitude;
        /// <summary>
        /// The value is normalized if reduced modulo the order of the field.
        /// It will also have a magnitude of 0 or 1.
        /// </summary>
        public readonly bool isNormalized;

        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        internal void Verify()
        {
            VerifyMagnitude(magnitude, 32);
            if (isNormalized)
            {
                VerifyMagnitude(magnitude, 1);
            }

            int m = isNormalized ? 1 : 2 * magnitude;
            Debug.Assert(b0 <= 0x03FFFFFFU * m);
            Debug.Assert(b1 <= 0x03FFFFFFU * m);
            Debug.Assert(b2 <= 0x03FFFFFFU * m);
            Debug.Assert(b3 <= 0x03FFFFFFU * m);
            Debug.Assert(b4 <= 0x03FFFFFFU * m);
            Debug.Assert(b5 <= 0x03FFFFFFU * m);
            Debug.Assert(b6 <= 0x03FFFFFFU * m);
            Debug.Assert(b7 <= 0x03FFFFFFU * m);
            Debug.Assert(b8 <= 0x03FFFFFFU * m);
            Debug.Assert(b9 <= 0x003FFFFFU * m);

            if (isNormalized)
            {
                if (b9 == 0x003FFFFFU)
                {
                    uint mid = b8 & b7 & b6 & b5 & b4 & b3 & b2;
                    if (mid == 0x03FFFFFFU)
                    {
                        Debug.Assert((b1 + 0x40U + ((b0 + 0x03D1U) >> 26)) <= 0x03FFFFFFU);
                    }
                }
            }
        }

        internal static void VerifyMagnitude(int magnitude, int max)
        {
            Debug.Assert(max >= 0);
            Debug.Assert(max <= 32);
            Debug.Assert(magnitude <= max);
        }

#endif // DEBUG


        private static readonly UInt256_10x26 _zero = new UInt256_10x26(0, 0, 0, 0, 0, 0, 0, 0, 0, 0
#if DEBUG
            , 0, true
#endif
            );
        private static readonly UInt256_10x26 _one = new UInt256_10x26(1, 0, 0, 0, 0, 0, 0, 0, 0, 0
#if DEBUG
            , 1, true
#endif
            );
        private static readonly UInt256_10x26 _n = new UInt256_10x26(0xD0364141U, 0xBFD25E8CU, 0xAF48A03BU, 0xBAAEDCE6U,
                                                                     0xFFFFFFFEU, 0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU);
        private static readonly UInt256_10x26 _pn = new UInt256_10x26(0x2FC9BAEEU, 0x402DA172U, 0x50B75FC4U, 0x45512319U,
                                                                      1, 0, 0, 0);
        private static readonly UInt256_10x26 _beta = new UInt256_10x26(0x719501eeU, 0xc1396c28U, 0x12f58995U, 0x9cf04975U,
                                                                        0xac3434e9U, 0x6e64479eU, 0x657c0710U, 0x7ae96a2bU);
        /// <summary>
        /// Zero
        /// </summary>
        public static ref readonly UInt256_10x26 Zero => ref _zero;
        /// <summary>
        /// One
        /// </summary>
        public static ref readonly UInt256_10x26 One => ref _one;
        /// <summary>
        /// Secp256k1 order (0xfffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141)
        /// </summary>
        public static ref readonly UInt256_10x26 N => ref _n;
        /// <summary>
        /// Difference between secp256k1 prime and order (p-n=0x014551231950b75fc4402da1722fc9baee)
        /// </summary>
        public static ref readonly UInt256_10x26 PMinusN => ref _pn;
        /// <summary>
        /// secp256k1_const_beta
        /// </summary>
        public static ref readonly UInt256_10x26 Beta => ref _beta;

        /// <summary>
        /// Returns if this instance is odd (needs to be normalized)
        /// </summary>
        public bool IsOdd
        {
            get
            {
#if DEBUG
                Debug.Assert(isNormalized);
                Verify();
#endif
                return (b0 & 1) != 0;

            }
        }

        /// <summary>
        /// Returns if this instance is zero (needs to be normalized)
        /// </summary>
        public bool IsZero
        {
            get
            {
#if DEBUG
                Debug.Assert(isNormalized);
                Verify();
#endif
                return (b0 | b1 | b2 | b3 | b4 | b5 | b6 | b7 | b8 | b9) == 0;
            }
        }

        /// <summary>
        /// Returns if this instance is zero when normalized
        /// ie. if this ≡ 0 (mod p)
        /// </summary>
        /// <remarks>
        /// This method is constant time
        /// </remarks>
        /// <returns>True if normalizes to zero</returns>
        public bool IsZeroNormalized()
        {
#if DEBUG
            Verify();
#endif
            // z0 tracks a possible raw value of 0, z1 tracks a possible raw value of P
            uint z0, z1;

            // Reduce b9 at the start so there will be at most a single carry from the first pass
            uint x = b9 >> 22;
            uint t9 = b9 & 0b00000000_00111111_11111111_11111111;

            // The first pass ensures the magnitude is 1, ...
            uint t0 = b0 + (x * 0x03D1U); uint t1 = b1 + (x << 6);
            t1 += (t0 >> 26); t0 &= 0x03FFFFFFU; z0 = t0; z1 = t0 ^ 0x03D0U;
            uint t2 = b2 + (t1 >> 26); t1 &= 0x03FFFFFFU; z0 |= t1; z1 &= t1 ^ 0x40U;
            uint t3 = b3 + (t2 >> 26); t2 &= 0x03FFFFFFU; z0 |= t2; z1 &= t2;
            uint t4 = b4 + (t3 >> 26); t3 &= 0x03FFFFFFU; z0 |= t3; z1 &= t3;
            uint t5 = b5 + (t4 >> 26); t4 &= 0x03FFFFFFU; z0 |= t4; z1 &= t4;
            uint t6 = b6 + (t5 >> 26); t5 &= 0x03FFFFFFU; z0 |= t5; z1 &= t5;
            uint t7 = b7 + (t6 >> 26); t6 &= 0x03FFFFFFU; z0 |= t6; z1 &= t6;
            uint t8 = b8 + (t7 >> 26); t7 &= 0x03FFFFFFU; z0 |= t7; z1 &= t7;
            t9 += (t8 >> 26); t8 &= 0x03FFFFFFU; z0 |= t8; z1 &= t8;
            z0 |= t9; z1 &= t9 ^ 0x03C00000U;

            // ... except for a possible carry at bit 22 of t9 (i.e. bit 256 of the field element)
            Debug.Assert(t9 >> 23 == 0);

            return (z0 == 0) | (z1 == 0x03FFFFFFU);
        }

        /// <summary>
        /// Returns if this instance is zero when normalized without constant-time guarantee
        /// ie. if this ≡ 0 (mod p)
        /// </summary>
        /// <remarks>
        /// This method is not constant time
        /// </remarks>
        /// <returns>True if normalizes to zero</returns>
        public bool IsZeroNormalizedVar()
        {
#if DEBUG
            Verify();
#endif
            // Reduce t9 at the start so there will be at most a single carry from the first pass
            uint x = b9 >> 22;

            // The first pass ensures the magnitude is 1, ...
            uint t0 = b0 + (x * 0x03D1U);

            // z0 tracks a possible raw value of 0, z1 tracks a possible raw value of P
            uint z0 = t0 & 0x03FFFFFFU;
            uint z1 = z0 ^ 0x03D0U;

            // Fast return path should catch the majority of cases
            if ((z0 != 0U) && (z1 != 0x03FFFFFFU))
            {
                return false;
            }

            uint t9 = b9 & 0x003FFFFFU;
            uint t1 = b1 + (x << 6);

            t1 += (t0 >> 26);
            uint t2 = b2 + (t1 >> 26); t1 &= 0x03FFFFFFU; z0 |= t1; z1 &= t1 ^ 0x40U;
            uint t3 = b3 + (t2 >> 26); t2 &= 0x03FFFFFFU; z0 |= t2; z1 &= t2;
            uint t4 = b4 + (t3 >> 26); t3 &= 0x03FFFFFFU; z0 |= t3; z1 &= t3;
            uint t5 = b5 + (t4 >> 26); t4 &= 0x03FFFFFFU; z0 |= t4; z1 &= t4;
            uint t6 = b6 + (t5 >> 26); t5 &= 0x03FFFFFFU; z0 |= t5; z1 &= t5;
            uint t7 = b7 + (t6 >> 26); t6 &= 0x03FFFFFFU; z0 |= t6; z1 &= t6;
            uint t8 = b8 + (t7 >> 26); t7 &= 0x03FFFFFFU; z0 |= t7; z1 &= t7;
            t9 += (t8 >> 26); t8 &= 0x03FFFFFFU; z0 |= t8; z1 &= t8;
            z0 |= t9; z1 &= t9 ^ 0x03C00000U;

            // ... except for a possible carry at bit 22 of t9 (i.e. bit 256 of the field element)
            Debug.Assert(t9 >> 23 == 0);

            return (z0 == 0) | (z1 == 0x03FFFFFFU);
        }

        /// <summary>
        /// Returns the normalized version of this instance by reducing it modulo P and
        /// bringing the elements to canonical representations.
        /// </summary>
        /// <remarks>
        /// Result's magnitude will be 1
        /// </remarks>
        /// <returns>Normalized result</returns>
        public UInt256_10x26 Normalize()
        {
#if DEBUG
            Verify();
#endif
            // Reduce b9 at the start so there will be at most a single carry from the first pass
            uint x = b9 >> 22;
            uint t9 = b9 & 0b00000000_00111111_11111111_11111111U;

            // The first pass ensures the magnitude is 1, ...
            uint t0 = b0 + (x * 0x03D1U);
            uint t1 = b1 + (x << 6);
            t1 += t0 >> 26; t0 &= 0x03FFFFFFU;
            uint t2 = b2 + (t1 >> 26); t1 &= 0x03FFFFFFU;
            uint t3 = b3 + (t2 >> 26); t2 &= 0x03FFFFFFU; uint m = t2;
            uint t4 = b4 + (t3 >> 26); t3 &= 0x03FFFFFFU; m &= t3;
            uint t5 = b5 + (t4 >> 26); t4 &= 0x03FFFFFFU; m &= t4;
            uint t6 = b6 + (t5 >> 26); t5 &= 0x03FFFFFFU; m &= t5;
            uint t7 = b7 + (t6 >> 26); t6 &= 0x03FFFFFFU; m &= t6;
            uint t8 = b8 + (t7 >> 26); t7 &= 0x03FFFFFFU; m &= t7;
            t9 += (t8 >> 26); t8 &= 0x03FFFFFFU; m &= t8;

            // ... except for a possible carry at bit 22 of t9 (i.e. bit 256 of the field element)
            Debug.Assert(t9 >> 23 == 0);

            // At most a single final reduction is needed; check if the value is >= the field characteristic
            x = (t9 >> 22) | ((t9 == 0x003FFFFFU ? 1u : 0) & (m == 0x03FFFFFFU ? 1u : 0)
                & ((t1 + 0x40U + ((t0 + 0x03D1U) >> 26)) > 0x03FFFFFFU ? 1u : 0));

            // Apply the final reduction (always do it for constant-time behaviour)
            t0 += x * 0x03D1U; t1 += (x << 6);
            t1 += (t0 >> 26); t0 &= 0x03FFFFFFU;
            t2 += (t1 >> 26); t1 &= 0x03FFFFFFU;
            t3 += (t2 >> 26); t2 &= 0x03FFFFFFU;
            t4 += (t3 >> 26); t3 &= 0x03FFFFFFU;
            t5 += (t4 >> 26); t4 &= 0x03FFFFFFU;
            t6 += (t5 >> 26); t5 &= 0x03FFFFFFU;
            t7 += (t6 >> 26); t6 &= 0x03FFFFFFU;
            t8 += (t7 >> 26); t7 &= 0x03FFFFFFU;
            t9 += (t8 >> 26); t8 &= 0x03FFFFFFU;

            // If t9 didn't carry to bit 22 already, then it should have after any final reduction
            Debug.Assert(t9 >> 22 == x);

            // Mask off the possible multiple of 2^256 from the final reduction 
            t9 &= 0x003FFFFFU;

            return new UInt256_10x26(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9
#if DEBUG
                , 1, true
#endif
                );
        }

        /// <summary>
        /// Returns the weak normalized version of this instance.
        /// </summary>
        /// <remarks>
        /// Result's magnitude will be 1
        /// </remarks>
        /// <returns>Result with same <see cref="isNormalized"/> but magnitude of 1</returns>
        public UInt256_10x26 NormalizeWeak()
        {
#if DEBUG
            Verify();
#endif
            // Reduce t9 at the start so there will be at most a single carry from the first pass
            uint x = b9 >> 22;
            uint t9 = b9 & 0x003FFFFFU;

            // The first pass ensures the magnitude is 1, ...
            uint t0 = b0 + (x * 0x03D1U); uint t1 = b1 + (x << 6);
            t1 += (t0 >> 26); t0 &= 0x03FFFFFFU;
            uint t2 = b2 + (t1 >> 26); t1 &= 0x03FFFFFFU;
            uint t3 = b3 + (t2 >> 26); t2 &= 0x03FFFFFFU;
            uint t4 = b4 + (t3 >> 26); t3 &= 0x03FFFFFFU;
            uint t5 = b5 + (t4 >> 26); t4 &= 0x03FFFFFFU;
            uint t6 = b6 + (t5 >> 26); t5 &= 0x03FFFFFFU;
            uint t7 = b7 + (t6 >> 26); t6 &= 0x03FFFFFFU;
            uint t8 = b8 + (t7 >> 26); t7 &= 0x03FFFFFFU;
            t9 += (t8 >> 26); t8 &= 0x03FFFFFFU;

            // ... except for a possible carry at bit 22 of t9 (i.e. bit 256 of the field element)
            Debug.Assert(t9 >> 23 == 0);

            return new UInt256_10x26(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9
#if DEBUG
                , 1, isNormalized
#endif
                );
        }

        /// <summary>
        /// Returns the normalized version of this instance by reducing it modulo P and
        /// bringing the elements to canonical representations.
        /// Normalization is not constant-time.
        /// </summary>
        /// <remarks>
        /// Result's magnitude will be 1
        /// </remarks>
        /// <returns>Normalized result</returns>
        public UInt256_10x26 NormalizeVar()
        {
#if DEBUG
            Verify();
#endif
            // Reduce t9 at the start so there will be at most a single carry from the first pass
            uint x = b9 >> 22;
            uint t9 = b9 & 0x003FFFFFU;

            // The first pass ensures the magnitude is 1, ...
            uint t0 = b0 + (x * 0x03D1U); uint t1 = b1 + (x << 6);
            t1 += (t0 >> 26); t0 &= 0x03FFFFFFU;
            uint t2 = b2 + (t1 >> 26); t1 &= 0x03FFFFFFU;
            uint t3 = b3 + (t2 >> 26); t2 &= 0x03FFFFFFU; uint m = t2;
            uint t4 = b4 + (t3 >> 26); t3 &= 0x03FFFFFFU; m &= t3;
            uint t5 = b5 + (t4 >> 26); t4 &= 0x03FFFFFFU; m &= t4;
            uint t6 = b6 + (t5 >> 26); t5 &= 0x03FFFFFFU; m &= t5;
            uint t7 = b7 + (t6 >> 26); t6 &= 0x03FFFFFFU; m &= t6;
            uint t8 = b8 + (t7 >> 26); t7 &= 0x03FFFFFFU; m &= t7;
            t9 += (t8 >> 26); t8 &= 0x03FFFFFFU; m &= t8;

            // ... except for a possible carry at bit 22 of t9 (i.e. bit 256 of the field element)
            Debug.Assert(t9 >> 23 == 0);

            // At most a single final reduction is needed; check if the value is >= the field characteristic
            x = (t9 >> 22) | ((t9 == 0x003FFFFFU ? 1U : 0) & (m == 0x03FFFFFFU ? 1U : 0)
                & ((t1 + 0x40U + ((t0 + 0x03D1U) >> 26)) > 0x03FFFFFFU ? 1U : 0));

            if (x != 0)
            {
                t0 += 0x03D1U; t1 += (x << 6);
                t1 += (t0 >> 26); t0 &= 0x03FFFFFFU;
                t2 += (t1 >> 26); t1 &= 0x03FFFFFFU;
                t3 += (t2 >> 26); t2 &= 0x03FFFFFFU;
                t4 += (t3 >> 26); t3 &= 0x03FFFFFFU;
                t5 += (t4 >> 26); t4 &= 0x03FFFFFFU;
                t6 += (t5 >> 26); t5 &= 0x03FFFFFFU;
                t7 += (t6 >> 26); t6 &= 0x03FFFFFFU;
                t8 += (t7 >> 26); t7 &= 0x03FFFFFFU;
                t9 += (t8 >> 26); t8 &= 0x03FFFFFFU;

                // If t9 didn't carry to bit 22 already, then it should have after any final reduction
                Debug.Assert(t9 >> 22 == x);

                // Mask off the possible multiple of 2^256 from the final reduction
                t9 &= 0x003FFFFFU;
            }

            return new UInt256_10x26(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9
#if DEBUG
                , 1, true
#endif
                );
        }


        /// <summary>
        /// Adds the given value to this instance
        /// </summary>
        /// <remarks>
        /// Result's magnitude is the sum of the two magnitudes
        /// </remarks>
        /// <param name="u">The value to add</param>
        /// <returns>Result of the addition</returns>
        public UInt256_10x26 Add(uint u) => this + u;

        /// <summary>
        /// Adds two <see cref="UInt256_10x26"/> values
        /// </summary>
        /// <remarks>
        /// Result's magnitude is the sum of the two magnitudes
        /// </remarks>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <returns>Result of the addition</returns>
        public static UInt256_10x26 operator +(in UInt256_10x26 a, uint b)
        {
#if DEBUG
            Debug.Assert(0 <= b && b <= 0x7FFF);
            a.Verify();
#endif
            return new UInt256_10x26(
                a.b0 + b,
                a.b1,
                a.b2,
                a.b3,
                a.b4,
                a.b5,
                a.b6,
                a.b7,
                a.b8,
                a.b9
#if DEBUG
                , a.magnitude + 1,
                false
#endif
                );
        }

        /// <summary>
        /// Adds the given value to this instance
        /// </summary>
        /// <remarks>
        /// Result's magnitude is the sum of the two magnitudes
        /// </remarks>
        /// <param name="other">The value to add</param>
        /// <returns>Result of the addition</returns>
        public UInt256_10x26 Add(in UInt256_10x26 other) => this + other;

        /// <summary>
        /// Adds two <see cref="UInt256_10x26"/> values
        /// </summary>
        /// <remarks>
        /// Result's magnitude is the sum of the two magnitudes
        /// </remarks>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <returns>Result of the addition</returns>
        public static UInt256_10x26 operator +(in UInt256_10x26 a, in UInt256_10x26 b)
        {
#if DEBUG
            a.Verify();
            b.Verify();
            Debug.Assert(a.magnitude + b.magnitude <= 32);
#endif
            return new UInt256_10x26(
                a.b0 + b.b0,
                a.b1 + b.b1,
                a.b2 + b.b2,
                a.b3 + b.b3,
                a.b4 + b.b4,
                a.b5 + b.b5,
                a.b6 + b.b6,
                a.b7 + b.b7,
                a.b8 + b.b8,
                a.b9 + b.b9
#if DEBUG
                , a.magnitude + b.magnitude,
                false
#endif
                );
        }

        /// <summary>
        /// Adds three <see cref="UInt256_10x26"/> values
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <param name="c">Third value</param>
        /// <returns>Result</returns>
        public static UInt256_10x26 Add(in UInt256_10x26 a, in UInt256_10x26 b, in UInt256_10x26 c)
        {
            return new UInt256_10x26(
                a.b0 + b.b0 + c.b0,
                a.b1 + b.b1 + c.b1,
                a.b2 + b.b2 + c.b2,
                a.b3 + b.b3 + c.b3,
                a.b4 + b.b4 + c.b4,
                a.b5 + b.b5 + c.b5,
                a.b6 + b.b6 + c.b6,
                a.b7 + b.b7 + c.b7,
                a.b8 + b.b8 + c.b8,
                a.b9 + b.b9 + c.b9
#if DEBUG
                , a.magnitude + b.magnitude + c.magnitude,
                false
#endif
                );
        }

        /// <summary>
        /// Adds four <see cref="UInt256_10x26"/> values
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <param name="c">Third value</param>
        /// <param name="d">Fourth value</param>
        /// <returns>Result</returns>
        public static UInt256_10x26 Add(in UInt256_10x26 a, in UInt256_10x26 b, in UInt256_10x26 c, in UInt256_10x26 d)
        {
            return new UInt256_10x26(
                a.b0 + b.b0 + c.b0 + d.b0,
                a.b1 + b.b1 + c.b1 + d.b1,
                a.b2 + b.b2 + c.b2 + d.b2,
                a.b3 + b.b3 + c.b3 + d.b3,
                a.b4 + b.b4 + c.b4 + d.b4,
                a.b5 + b.b5 + c.b5 + d.b5,
                a.b6 + b.b6 + c.b6 + d.b6,
                a.b7 + b.b7 + c.b7 + d.b7,
                a.b8 + b.b8 + c.b8 + d.b8,
                a.b9 + b.b9 + c.b9 + d.b9
#if DEBUG
                , a.magnitude + b.magnitude + c.magnitude + d.magnitude,
                false
#endif
                );
        }


        /// <summary>
        /// Halves the value of this instance modulo the field prime.
        /// </summary>
        /// <remarks>
        /// This method is constant-time.
        /// Result's magnitude is floor(m/2) + 1.
        /// Result may not be normalized.
        /// </remarks>
        /// <returns></returns>
        public UInt256_10x26 Half()
        {
#if DEBUG
            Verify();
            VerifyMagnitude(magnitude, 31);
#endif
            const uint one = 1U;
            // uint32_t mask = -(t0 & one) >> 6;
            uint mask = (b0 & one) * 0b00000011_11111111_11111111_11111111U;

            // Bounds analysis (over the rationals).
            //
            // Let m = r->magnitude
            //     C = 0x3FFFFFFUL * 2
            //     D = 0x03FFFFFUL * 2
            //
            // Initial bounds: t0..t8 <= C * m
            //                     t9 <= D * m

            uint t0 = b0 + (0x03FFFC2FU & mask);
            uint t1 = b1 + (0x03FFFFBFU & mask);
            uint t2 = b2 + mask;
            uint t3 = b3 + mask;
            uint t4 = b4 + mask;
            uint t5 = b5 + mask;
            uint t6 = b6 + mask;
            uint t7 = b7 + mask;
            uint t8 = b8 + mask;
            uint t9 = b9 + (mask >> 4);

            Debug.Assert((t0 & one) == 0);

            // t0..t8: added <= C/2
            //     t9: added <= D/2
            //
            // Current bounds: t0..t8 <= C * (m + 1/2)
            //                     t9 <= D * (m + 1/2)

            t0 = (t0 >> 1) + ((t1 & one) << 25);
            t1 = (t1 >> 1) + ((t2 & one) << 25);
            t2 = (t2 >> 1) + ((t3 & one) << 25);
            t3 = (t3 >> 1) + ((t4 & one) << 25);
            t4 = (t4 >> 1) + ((t5 & one) << 25);
            t5 = (t5 >> 1) + ((t6 & one) << 25);
            t6 = (t6 >> 1) + ((t7 & one) << 25);
            t7 = (t7 >> 1) + ((t8 & one) << 25);
            t8 = (t8 >> 1) + ((t9 & one) << 25);
            t9 = (t9 >> 1);

            // t0..t8: shifted right and added <= C/4 + 1/2
            //     t9: shifted right
            //
            // Current bounds: t0..t8 <= C * (m/2 + 1/2)
            //                     t9 <= D * (m/2 + 1/4)
            //
            // Therefore the output magnitude (M) has to be set such that:
            //     t0..t8: C * M >= C * (m/2 + 1/2)
            //         t9: D * M >= D * (m/2 + 1/4)
            //
            // It suffices for all limbs that, for any input magnitude m:
            //     M >= m/2 + 1/2
            //
            // and since we want the smallest such integer value for M:
            //     M == floor(m/2) + 1

            return new UInt256_10x26(t0, t1, t2, t3, t4, t5, t6, t7, t8, t9
#if DEBUG
                , (magnitude >> 1) + 1,
                false
#endif
                );
        }


        /// <summary>
        /// Returns the additive inverse of this instance. Takes a maximum magnitude of the input as an argument.
        /// </summary>
        /// <param name="m">Magnitude in [0,31]</param>
        /// <returns>Additive inverse of this instance with a magnitude that is <paramref name="m"/> + 1</returns>
        public UInt256_10x26 Negate(int m)
        {
#if DEBUG
            Verify();
            Debug.Assert(m >= 0 && m <= 31);
            VerifyMagnitude(magnitude, m);

            // For all legal values of m (0..31), the following properties hold:
            Debug.Assert(0x03FFFC2FU * 2 * (m + 1) >= 0x03FFFFFFU * 2 * m);
            Debug.Assert(0x03FFFFBFU * 2 * (m + 1) >= 0x03FFFFFFU * 2 * m);
            Debug.Assert(0x03FFFFFFU * 2 * (m + 1) >= 0x03FFFFFFU * 2 * m);
            Debug.Assert(0x003FFFFFU * 2 * (m + 1) >= 0x003FFFFFU * 2 * m);
#endif
            // Due to the properties above, the left hand in the subtractions below is never less than the right hand.
            return new UInt256_10x26(
                0x03FFFC2FU * 2 * (uint)(m + 1) - b0,
                0x03FFFFBFU * 2 * (uint)(m + 1) - b1,
                0x03FFFFFFU * 2 * (uint)(m + 1) - b2,
                0x03FFFFFFU * 2 * (uint)(m + 1) - b3,
                0x03FFFFFFU * 2 * (uint)(m + 1) - b4,
                0x03FFFFFFU * 2 * (uint)(m + 1) - b5,
                0x03FFFFFFU * 2 * (uint)(m + 1) - b6,
                0x03FFFFFFU * 2 * (uint)(m + 1) - b7,
                0x03FFFFFFU * 2 * (uint)(m + 1) - b8,
                0x003FFFFFU * 2 * (uint)(m + 1) - b9
#if DEBUG
                , m + 1, false
#endif
                );
        }


        /// <summary>
        /// Multiplies this instance with the given unsigned 32-bit integer.
        /// </summary>
        /// <remarks>
        /// Result's magnitude is multiplied by <paramref name="a"/>.
        /// </remarks>
        /// <param name="a">Multiplier in [0,32]</param>
        /// <returns>Result (is not normalized)</returns>
        public UInt256_10x26 Multiply(uint a) => this * a;

        /// <summary>
        /// Multiplies the <see cref="UInt256_10x26"/> with the given unsigned 32-bit integer.
        /// </summary>
        /// <remarks>
        /// Result's magnitude is <see cref="UInt256_10x26"/>'s magnitude multiplied by <paramref name="b"/>.
        /// </remarks>
        /// <param name="a">Multiplicand</param>
        /// <param name="b">Multiplier in [0,32]</param>
        /// <returns>Result (is not normalized)</returns>
        public static UInt256_10x26 operator *(in UInt256_10x26 a, uint b)
        {
#if DEBUG
            a.Verify();
            Debug.Assert(b >= 0 && b <= 32);
            Debug.Assert(b * a.magnitude <= 32);
#endif
            return new UInt256_10x26(
                a.b0 * b,
                a.b1 * b,
                a.b2 * b,
                a.b3 * b,
                a.b4 * b,
                a.b5 * b,
                a.b6 * b,
                a.b7 * b,
                a.b8 * b,
                a.b9 * b
#if DEBUG
                , a.magnitude * (int)b,
                false
#endif
                );
        }


        /// <summary>
        /// Multiplies this instance with the other <see cref="UInt256_10x26"/> value.
        /// </summary>
        /// <remarks>
        /// Magnitude of each value must be below 8.
        /// Result's magnitude is 1 but is not normalized.
        /// </remarks>
        /// <param name="other">Other value</param>
        /// <returns>Multiplication result</returns>
        public UInt256_10x26 Multiply(in UInt256_10x26 other) => this * other;

        /// <summary>
        /// Multiplies the two <see cref="UInt256_10x26"/> values.
        /// </summary>
        /// <remarks>
        /// Magnitude of each value must be below 8.
        /// Result's magnitude is 1 but is not normalized.
        /// </remarks>
        /// <param name="a">First</param>
        /// <param name="b">Second</param>
        /// <returns>Multiplication result</returns>
        public static UInt256_10x26 operator *(in UInt256_10x26 a, in UInt256_10x26 b)
        {
#if DEBUG
            a.Verify();
            b.Verify();
            VerifyMagnitude(a.magnitude, 8);
            VerifyMagnitude(b.magnitude, 8);
#endif
            Debug.Assert(a.b0 >> 30 == 0);
            Debug.Assert(a.b1 >> 30 == 0);
            Debug.Assert(a.b2 >> 30 == 0);
            Debug.Assert(a.b3 >> 30 == 0);
            Debug.Assert(a.b4 >> 30 == 0);
            Debug.Assert(a.b5 >> 30 == 0);
            Debug.Assert(a.b6 >> 30 == 0);
            Debug.Assert(a.b7 >> 30 == 0);
            Debug.Assert(a.b8 >> 30 == 0);
            Debug.Assert(a.b9 >> 26 == 0);

            Debug.Assert(b.b0 >> 30 == 0);
            Debug.Assert(b.b1 >> 30 == 0);
            Debug.Assert(b.b2 >> 30 == 0);
            Debug.Assert(b.b3 >> 30 == 0);
            Debug.Assert(b.b4 >> 30 == 0);
            Debug.Assert(b.b5 >> 30 == 0);
            Debug.Assert(b.b6 >> 30 == 0);
            Debug.Assert(b.b7 >> 30 == 0);
            Debug.Assert(b.b8 >> 30 == 0);
            Debug.Assert(b.b9 >> 26 == 0);

            ulong u0, u1, u2, u3, u4, u5, u6, u7, u8;
            uint t9, t1, t0, t2, t3, t4, t5, t6, t7;
            const uint M = 0x03FFFFFFU, R0 = 0x3D10U, R1 = 0x0400U;

            // [... a b c] is a shorthand for ... + a<<52 + b<<26 + c<<0 mod n.
            // for 0 <= x <= 9, px is a shorthand for sum(a[i]*b[x-i], i=0..x).
            // for 9 <= x <= 18, px is a shorthand for sum(a[i]*b[x-i], i=(x-9)..9)
            // Note that [x 0 0 0 0 0 0 0 0 0 0] = [x*R1 x*R0].

            ulong d = (ulong)a.b0 * b.b9
                    + (ulong)a.b1 * b.b8
                    + (ulong)a.b2 * b.b7
                    + (ulong)a.b3 * b.b6
                    + (ulong)a.b4 * b.b5
                    + (ulong)a.b5 * b.b4
                    + (ulong)a.b6 * b.b3
                    + (ulong)a.b7 * b.b2
                    + (ulong)a.b8 * b.b1
                    + (ulong)a.b9 * b.b0;
            // [d 0 0 0 0 0 0 0 0 0] = [p9 0 0 0 0 0 0 0 0 0] 
            t9 = (uint)d & M; d >>= 26;
            Debug.Assert(t9 >> 26 == 0);
            Debug.Assert(d >> 38 == 0);
            // [d t9 0 0 0 0 0 0 0 0 0] = [p9 0 0 0 0 0 0 0 0 0] 

            ulong c = (ulong)a.b0 * b.b0;
            Debug.Assert(c >> 60 == 0);
            // [d t9 0 0 0 0 0 0 0 0 c] = [p9 0 0 0 0 0 0 0 0 p0] 
            d += (ulong)a.b1 * b.b9
               + (ulong)a.b2 * b.b8
               + (ulong)a.b3 * b.b7
               + (ulong)a.b4 * b.b6
               + (ulong)a.b5 * b.b5
               + (ulong)a.b6 * b.b4
               + (ulong)a.b7 * b.b3
               + (ulong)a.b8 * b.b2
               + (ulong)a.b9 * b.b1;
            Debug.Assert(d >> 63 == 0);
            // [d t9 0 0 0 0 0 0 0 0 c] = [p10 p9 0 0 0 0 0 0 0 0 p0] 
            u0 = d & M; d >>= 26; c += u0 * R0;
            Debug.Assert(u0 >> 26 == 0);
            Debug.Assert(d >> 37 == 0);
            Debug.Assert(c >> 61 == 0);
            // [d u0 t9 0 0 0 0 0 0 0 0 c-u0*R0] = [p10 p9 0 0 0 0 0 0 0 0 p0] 
            t0 = (uint)c & M; c >>= 26; c += u0 * R1;
            Debug.Assert(t0 >> 26 == 0);
            Debug.Assert(c >> 37 == 0);
            // [d u0 t9 0 0 0 0 0 0 0 c-u0*R1 t0-u0*R0] = [p10 p9 0 0 0 0 0 0 0 0 p0] 
            // [d 0 t9 0 0 0 0 0 0 0 c t0] = [p10 p9 0 0 0 0 0 0 0 0 p0] 

            c += (ulong)a.b0 * b.b1
               + (ulong)a.b1 * b.b0;
            Debug.Assert(c >> 62 == 0);
            // [d 0 t9 0 0 0 0 0 0 0 c t0] = [p10 p9 0 0 0 0 0 0 0 p1 p0] 
            d += (ulong)a.b2 * b.b9
               + (ulong)a.b3 * b.b8
               + (ulong)a.b4 * b.b7
               + (ulong)a.b5 * b.b6
               + (ulong)a.b6 * b.b5
               + (ulong)a.b7 * b.b4
               + (ulong)a.b8 * b.b3
               + (ulong)a.b9 * b.b2;
            Debug.Assert(d >> 63 == 0);
            // [d 0 t9 0 0 0 0 0 0 0 c t0] = [p11 p10 p9 0 0 0 0 0 0 0 p1 p0] 
            u1 = d & M; d >>= 26; c += u1 * R0;
            Debug.Assert(u1 >> 26 == 0);
            Debug.Assert(d >> 37 == 0);
            Debug.Assert(c >> 63 == 0);
            // [d u1 0 t9 0 0 0 0 0 0 0 c-u1*R0 t0] = [p11 p10 p9 0 0 0 0 0 0 0 p1 p0] 
            t1 = (uint)c & M; c >>= 26; c += u1 * R1;
            Debug.Assert(t1 >> 26 == 0);
            Debug.Assert(c >> 38 == 0);
            // [d u1 0 t9 0 0 0 0 0 0 c-u1*R1 t1-u1*R0 t0] = [p11 p10 p9 0 0 0 0 0 0 0 p1 p0] 
            // [d 0 0 t9 0 0 0 0 0 0 c t1 t0] = [p11 p10 p9 0 0 0 0 0 0 0 p1 p0] 

            c += (ulong)a.b0 * b.b2
               + (ulong)a.b1 * b.b1
               + (ulong)a.b2 * b.b0;
            Debug.Assert(c >> 62 == 0);
            // [d 0 0 t9 0 0 0 0 0 0 c t1 t0] = [p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 
            d += (ulong)a.b3 * b.b9
               + (ulong)a.b4 * b.b8
               + (ulong)a.b5 * b.b7
               + (ulong)a.b6 * b.b6
               + (ulong)a.b7 * b.b5
               + (ulong)a.b8 * b.b4
               + (ulong)a.b9 * b.b3;
            Debug.Assert(d >> 63 == 0);
            // [d 0 0 t9 0 0 0 0 0 0 c t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 
            u2 = d & M; d >>= 26; c += u2 * R0;
            Debug.Assert(u2 >> 26 == 0);
            Debug.Assert(d >> 37 == 0);
            Debug.Assert(c >> 63 == 0);
            // [d u2 0 0 t9 0 0 0 0 0 0 c-u2*R0 t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 
            t2 = (uint)c & M; c >>= 26; c += u2 * R1;
            Debug.Assert(t2 >> 26 == 0);
            Debug.Assert(c >> 38 == 0);
            // [d u2 0 0 t9 0 0 0 0 0 c-u2*R1 t2-u2*R0 t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 
            // [d 0 0 0 t9 0 0 0 0 0 c t2 t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 

            c += (ulong)a.b0 * b.b3
               + (ulong)a.b1 * b.b2
               + (ulong)a.b2 * b.b1
               + (ulong)a.b3 * b.b0;
            Debug.Assert(c >> 63 == 0);
            // [d 0 0 0 t9 0 0 0 0 0 c t2 t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 
            d += (ulong)a.b4 * b.b9
               + (ulong)a.b5 * b.b8
               + (ulong)a.b6 * b.b7
               + (ulong)a.b7 * b.b6
               + (ulong)a.b8 * b.b5
               + (ulong)a.b9 * b.b4;
            Debug.Assert(d >> 63 == 0);
            // [d 0 0 0 t9 0 0 0 0 0 c t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 
            u3 = d & M; d >>= 26; c += u3 * R0;
            Debug.Assert(u3 >> 26 == 0);
            Debug.Assert(d >> 37 == 0);
            // [d u3 0 0 0 t9 0 0 0 0 0 c-u3*R0 t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 
            t3 = (uint)c & M; c >>= 26; c += u3 * R1;
            Debug.Assert(t3 >> 26 == 0);
            Debug.Assert(c >> 39 == 0);
            // [d u3 0 0 0 t9 0 0 0 0 c-u3*R1 t3-u3*R0 t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 
            // [d 0 0 0 0 t9 0 0 0 0 c t3 t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 

            c += (ulong)a.b0 * b.b4
               + (ulong)a.b1 * b.b3
               + (ulong)a.b2 * b.b2
               + (ulong)a.b3 * b.b1
               + (ulong)a.b4 * b.b0;
            Debug.Assert(c >> 63 == 0);
            // [d 0 0 0 0 t9 0 0 0 0 c t3 t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 
            d += (ulong)a.b5 * b.b9
               + (ulong)a.b6 * b.b8
               + (ulong)a.b7 * b.b7
               + (ulong)a.b8 * b.b6
               + (ulong)a.b9 * b.b5;
            Debug.Assert(d >> 62 == 0);
            // [d 0 0 0 0 t9 0 0 0 0 c t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 
            u4 = d & M; d >>= 26; c += u4 * R0;
            Debug.Assert(u4 >> 26 == 0);
            Debug.Assert(d >> 36 == 0);
            // [d u4 0 0 0 0 t9 0 0 0 0 c-u4*R0 t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 
            t4 = (uint)c & M; c >>= 26; c += u4 * R1;
            Debug.Assert(t4 >> 26 == 0);
            Debug.Assert(c >> 39 == 0);
            // [d u4 0 0 0 0 t9 0 0 0 c-u4*R1 t4-u4*R0 t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 
            // [d 0 0 0 0 0 t9 0 0 0 c t4 t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 

            c += (ulong)a.b0 * b.b5
               + (ulong)a.b1 * b.b4
               + (ulong)a.b2 * b.b3
               + (ulong)a.b3 * b.b2
               + (ulong)a.b4 * b.b1
               + (ulong)a.b5 * b.b0;
            Debug.Assert(c >> 63 == 0);
            // [d 0 0 0 0 0 t9 0 0 0 c t4 t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 
            d += (ulong)a.b6 * b.b9
               + (ulong)a.b7 * b.b8
               + (ulong)a.b8 * b.b7
               + (ulong)a.b9 * b.b6;
            Debug.Assert(d >> 62 == 0);
            // [d 0 0 0 0 0 t9 0 0 0 c t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 
            u5 = d & M; d >>= 26; c += u5 * R0;
            Debug.Assert(u5 >> 26 == 0);
            Debug.Assert(d >> 36 == 0);
            // [d u5 0 0 0 0 0 t9 0 0 0 c-u5*R0 t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 
            t5 = (uint)c & M; c >>= 26; c += u5 * R1;
            Debug.Assert(t5 >> 26 == 0);
            Debug.Assert(c >> 39 == 0);
            // [d u5 0 0 0 0 0 t9 0 0 c-u5*R1 t5-u5*R0 t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 
            // [d 0 0 0 0 0 0 t9 0 0 c t5 t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 

            c += (ulong)a.b0 * b.b6
               + (ulong)a.b1 * b.b5
               + (ulong)a.b2 * b.b4
               + (ulong)a.b3 * b.b3
               + (ulong)a.b4 * b.b2
               + (ulong)a.b5 * b.b1
               + (ulong)a.b6 * b.b0;
            Debug.Assert(c >> 63 == 0);
            // [d 0 0 0 0 0 0 t9 0 0 c t5 t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 
            d += (ulong)a.b7 * b.b9
               + (ulong)a.b8 * b.b8
               + (ulong)a.b9 * b.b7;
            Debug.Assert(d >> 61 == 0);
            // [d 0 0 0 0 0 0 t9 0 0 c t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 
            u6 = d & M; d >>= 26; c += u6 * R0;
            Debug.Assert(u6 >> 26 == 0);
            Debug.Assert(d >> 35 == 0);
            // [d u6 0 0 0 0 0 0 t9 0 0 c-u6*R0 t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 
            t6 = (uint)c & M; c >>= 26; c += u6 * R1;
            Debug.Assert(t6 >> 26 == 0);
            Debug.Assert(c >> 39 == 0);
            // [d u6 0 0 0 0 0 0 t9 0 c-u6*R1 t6-u6*R0 t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 
            // [d 0 0 0 0 0 0 0 t9 0 c t6 t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 

            c += (ulong)a.b0 * b.b7
               + (ulong)a.b1 * b.b6
               + (ulong)a.b2 * b.b5
               + (ulong)a.b3 * b.b4
               + (ulong)a.b4 * b.b3
               + (ulong)a.b5 * b.b2
               + (ulong)a.b6 * b.b1
               + (ulong)a.b7 * b.b0;
            Debug.Assert(c <= 0x8000007C00000007UL);
            // [d 0 0 0 0 0 0 0 t9 0 c t6 t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
            d += (ulong)a.b8 * b.b9
               + (ulong)a.b9 * b.b8;
            Debug.Assert(d >> 58 == 0);
            // [d 0 0 0 0 0 0 0 t9 0 c t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
            u7 = d & M; d >>= 26; c += u7 * R0;
            Debug.Assert(u7 >> 26 == 0);
            Debug.Assert(d >> 32 == 0);
            Debug.Assert(c <= 0x800001703FFFC2F7UL);
            // [d u7 0 0 0 0 0 0 0 t9 0 c-u7*R0 t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
            t7 = (uint)c & M; c >>= 26; c += u7 * R1;
            Debug.Assert(t7 >> 26 == 0);
            Debug.Assert(c >> 38 == 0);
            // [d u7 0 0 0 0 0 0 0 t9 c-u7*R1 t7-u7*R0 t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
            // [d 0 0 0 0 0 0 0 0 t9 c t7 t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 

            c += (ulong)a.b0 * b.b8
               + (ulong)a.b1 * b.b7
               + (ulong)a.b2 * b.b6
               + (ulong)a.b3 * b.b5
               + (ulong)a.b4 * b.b4
               + (ulong)a.b5 * b.b3
               + (ulong)a.b6 * b.b2
               + (ulong)a.b7 * b.b1
               + (ulong)a.b8 * b.b0;
            Debug.Assert(c <= 0x9000007B80000008UL);
            // [d 0 0 0 0 0 0 0 0 t9 c t7 t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            d += (ulong)a.b9 * b.b9;
            Debug.Assert(d >> 57 == 0);
            // [d 0 0 0 0 0 0 0 0 t9 c t7 t6 t5 t4 t3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            u8 = d & M; d >>= 26; c += u8 * R0;
            Debug.Assert(u8 >> 26 == 0);
            Debug.Assert(d >> 31 == 0);
            Debug.Assert(c <= 0x9000016FBFFFC2F8UL);
            // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 t6 t5 t4 t3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 

            uint r3 = t3;
            Debug.Assert(r3 >> 26 == 0);
            // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 t6 t5 t4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            uint r4 = t4;
            Debug.Assert(r4 >> 26 == 0);
            // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 t6 t5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            uint r5 = t5;
            Debug.Assert(r5 >> 26 == 0);
            // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 t6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            uint r6 = t6;
            Debug.Assert(r6 >> 26 == 0);
            // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            uint r7 = t7;
            Debug.Assert(r7 >> 26 == 0);
            // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 

            uint r8 = (uint)c & M; c >>= 26; c += u8 * R1;
            Debug.Assert(r8 >> 26 == 0);
            Debug.Assert(c >> 39 == 0);
            // [d u8 0 0 0 0 0 0 0 0 t9+c-u8*R1 r8-u8*R0 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            // [d 0 0 0 0 0 0 0 0 0 t9+c r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            c += d * R0 + t9;
            Debug.Assert(c >> 45 == 0);
            // [d 0 0 0 0 0 0 0 0 0 c-d*R0 r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            uint r9 = (uint)c & (M >> 4); c >>= 22; c += d * (R1 << 4);
            Debug.Assert(r9 >> 22 == 0);
            Debug.Assert(c >> 46 == 0);
            // [d 0 0 0 0 0 0 0 0 r9+((c-d*R1<<4)<<22)-d*R0 r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            // [d 0 0 0 0 0 0 0 -d*R1 r9+(c<<22)-d*R0 r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            // [r9+(c<<22) r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 

            d = c * (R0 >> 4) + t0;
            Debug.Assert(d >> 56 == 0);
            // [r9+(c<<22) r8 r7 r6 r5 r4 r3 t2 t1 d-c*R0>>4] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            uint r0 = (uint)d & M; d >>= 26;
            Debug.Assert(r0 >> 26 == 0);
            Debug.Assert(d >> 30 == 0);
            // [r9+(c<<22) r8 r7 r6 r5 r4 r3 t2 t1+d r0-c*R0>>4] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            d += c * (R1 >> 4) + t1;
            Debug.Assert(d >> 53 == 0);
            Debug.Assert(d <= 0x10000003FFFFBFUL);
            // [r9+(c<<22) r8 r7 r6 r5 r4 r3 t2 d-c*R1>>4 r0-c*R0>>4] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            // [r9 r8 r7 r6 r5 r4 r3 t2 d r0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            uint r1 = (uint)d & M; d >>= 26;
            Debug.Assert(r1 >> 26 == 0);
            Debug.Assert(d >> 27 == 0);
            Debug.Assert(d <= 0x4000000UL);
            // [r9 r8 r7 r6 r5 r4 r3 t2+d r1 r0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            d += t2;
            Debug.Assert(d >> 27 == 0);
            // [r9 r8 r7 r6 r5 r4 r3 d r1 r0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            uint r2 = (uint)d;
            Debug.Assert(r2 >> 27 == 0);
            // [r9 r8 r7 r6 r5 r4 r3 r2 r1 r0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            return new UInt256_10x26(r0, r1, r2, r3, r4, r5, r6, r7, r8, r9
#if DEBUG
                , 1, false
#endif
                );
        }


        /// <summary>
        /// Determine whether this is a square (modulo p).
        /// </summary>
        /// <returns></returns>
        public bool IsSquareVar()
        {
#if DEBUG
            Verify();
#endif
            bool ret;
            UInt256_10x26 tmp = NormalizeVar();
            // secp256k1_jacobi32_maybe_var cannot deal with input 0.
            if (tmp.IsZero)
            {
                ret = true;
            }

            ModInv32Signed30 s = new ModInv32Signed30(tmp);
            int jac = ModInv32.Jacobi32MaybeVar(s, ModInv32ModInfo.FeConstant);
            if (jac == 0)
            {
                // secp256k1_jacobi32_maybe_var failed to compute the Jacobi symbol. Fall back
                // to computing a square root. This should be extremely rare with random
                // input (except in VERIFY mode, where a lower iteration count is used).
                ret = Sqrt(out _);
            }
            else
            {
                ret = jac >= 0;
            }
#if DEBUG
            tmp = NormalizeWeak();
            Debug.Assert(ret == tmp.Sqrt(out _));
#endif
            return ret;
        }

        /// <summary>
        /// Returns square (x^2 or x*x) of this instance.
        /// </summary>
        /// <remarks>
        /// Magnitude must be below 8.
        /// Result's magnitude is 1 but is not normalized.
        /// </remarks>
        /// <returns>Square result</returns>
        public UInt256_10x26 Sqr() => Sqr(1);

        /// <summary>
        /// Returns square (x^(2^n)) of this instance.
        /// <para/>Useful to compute squares in a for loop (ie. (((x^2)^2)2)^2)
        /// </summary>
        /// <remarks>
        /// Magnitude must be below 8.
        /// Result's magnitude is 1 but is not normalized.
        /// </remarks>
        /// <param name="times">Number of times to repeat squaring</param>
        /// <returns>Result</returns>
        public UInt256_10x26 Sqr(int times)
        {
#if DEBUG
            Verify();
            VerifyMagnitude(magnitude, 8);
#endif
            const uint M = 0x03FFFFFFU, R0 = 0x3D10U, R1 = 0x0400U;
            uint r0 = b0;
            uint r1 = b1;
            uint r2 = b2;
            uint r3 = b3;
            uint r4 = b4;
            uint r5 = b5;
            uint r6 = b6;
            uint r7 = b7;
            uint r8 = b8;
            uint r9 = b9;

            ulong c, d;
            ulong u0, u1, u2, u3, u4, u5, u6, u7, u8;
            uint t9, t0, t1, t2, t3, t4, t5, t6, t7;

            for (int i = 0; i < times; i++)
            {
                Debug.Assert(r0 >> 30 == 0);
                Debug.Assert(r1 >> 30 == 0);
                Debug.Assert(r2 >> 30 == 0);
                Debug.Assert(r3 >> 30 == 0);
                Debug.Assert(r4 >> 30 == 0);
                Debug.Assert(r5 >> 30 == 0);
                Debug.Assert(r6 >> 30 == 0);
                Debug.Assert(r7 >> 30 == 0);
                Debug.Assert(r8 >> 30 == 0);
                Debug.Assert(r9 >> 26 == 0);
                // [... a b c] is a shorthand for ... + a<<52 + b<<26 + c<<0 mod n.
                //  px is a shorthand for sum(n[i]*a[x-i], i=0..x).
                //  Note that [x 0 0 0 0 0 0 0 0 0 0] = [x*R1 x*R0].

                d = (ulong)(r0 * 2) * r9
                  + (ulong)(r1 * 2) * r8
                  + (ulong)(r2 * 2) * r7
                  + (ulong)(r3 * 2) * r6
                  + (ulong)(r4 * 2) * r5;
                // [d 0 0 0 0 0 0 0 0 0] = [p9 0 0 0 0 0 0 0 0 0] 
                t9 = (uint)d & M; d >>= 26;
                Debug.Assert(t9 >> 26 == 0);
                Debug.Assert(d >> 38 == 0);
                // [d t9 0 0 0 0 0 0 0 0 0] = [p9 0 0 0 0 0 0 0 0 0] 

                c = (ulong)r0 * r0;
                Debug.Assert(c >> 60 == 0);
                // [d t9 0 0 0 0 0 0 0 0 c] = [p9 0 0 0 0 0 0 0 0 p0] 
                d += (ulong)(r1 * 2) * r9
                   + (ulong)(r2 * 2) * r8
                   + (ulong)(r3 * 2) * r7
                   + (ulong)(r4 * 2) * r6
                   + (ulong)r5 * r5;
                Debug.Assert(d >> 63 == 0);
                // [d t9 0 0 0 0 0 0 0 0 c] = [p10 p9 0 0 0 0 0 0 0 0 p0] 
                u0 = d & M; d >>= 26; c += u0 * R0;
                Debug.Assert(u0 >> 26 == 0);
                Debug.Assert(d >> 37 == 0);
                Debug.Assert(c >> 61 == 0);
                // [d u0 t9 0 0 0 0 0 0 0 0 c-u0*R0] = [p10 p9 0 0 0 0 0 0 0 0 p0] 
                t0 = (uint)c & M; c >>= 26; c += u0 * R1;
                Debug.Assert(t0 >> 26 == 0);
                Debug.Assert(c >> 37 == 0);
                // [d u0 t9 0 0 0 0 0 0 0 c-u0*R1 t0-u0*R0] = [p10 p9 0 0 0 0 0 0 0 0 p0] 
                // [d 0 t9 0 0 0 0 0 0 0 c t0] = [p10 p9 0 0 0 0 0 0 0 0 p0] 

                c += (ulong)(r0 * 2) * r1;
                Debug.Assert(c >> 62 == 0);
                // [d 0 t9 0 0 0 0 0 0 0 c t0] = [p10 p9 0 0 0 0 0 0 0 p1 p0] 
                d += (ulong)(r2 * 2) * r9
                   + (ulong)(r3 * 2) * r8
                   + (ulong)(r4 * 2) * r7
                   + (ulong)(r5 * 2) * r6;
                Debug.Assert(d >> 63 == 0);
                // [d 0 t9 0 0 0 0 0 0 0 c t0] = [p11 p10 p9 0 0 0 0 0 0 0 p1 p0] 
                u1 = d & M; d >>= 26; c += u1 * R0;
                Debug.Assert(u1 >> 26 == 0);
                Debug.Assert(d >> 37 == 0);
                Debug.Assert(c >> 63 == 0);
                // [d u1 0 t9 0 0 0 0 0 0 0 c-u1*R0 t0] = [p11 p10 p9 0 0 0 0 0 0 0 p1 p0] 
                t1 = (uint)c & M; c >>= 26; c += u1 * R1;
                Debug.Assert(t1 >> 26 == 0);
                Debug.Assert(c >> 38 == 0);
                // [d u1 0 t9 0 0 0 0 0 0 c-u1*R1 t1-u1*R0 t0] = [p11 p10 p9 0 0 0 0 0 0 0 p1 p0] 
                // [d 0 0 t9 0 0 0 0 0 0 c t1 t0] = [p11 p10 p9 0 0 0 0 0 0 0 p1 p0] 

                c += (ulong)(r0 * 2) * r2
                   + (ulong)r1 * r1;
                Debug.Assert(c >> 62 == 0);
                // [d 0 0 t9 0 0 0 0 0 0 c t1 t0] = [p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 
                d += (ulong)(r3 * 2) * r9
                   + (ulong)(r4 * 2) * r8
                   + (ulong)(r5 * 2) * r7
                   + (ulong)r6 * r6;
                Debug.Assert(d >> 63 == 0);
                // [d 0 0 t9 0 0 0 0 0 0 c t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 
                u2 = d & M; d >>= 26; c += u2 * R0;
                Debug.Assert(u2 >> 26 == 0);
                Debug.Assert(d >> 37 == 0);
                Debug.Assert(c >> 63 == 0);
                // [d u2 0 0 t9 0 0 0 0 0 0 c-u2*R0 t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 
                t2 = (uint)c & M; c >>= 26; c += u2 * R1;
                Debug.Assert(t2 >> 26 == 0);
                Debug.Assert(c >> 38 == 0);
                // [d u2 0 0 t9 0 0 0 0 0 c-u2*R1 t2-u2*R0 t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 
                // [d 0 0 0 t9 0 0 0 0 0 c t2 t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 0 p2 p1 p0] 

                c += (ulong)(r0 * 2) * r3
                   + (ulong)(r1 * 2) * r2;
                Debug.Assert(c >> 63 == 0);
                // [d 0 0 0 t9 0 0 0 0 0 c t2 t1 t0] = [p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 
                d += (ulong)(r4 * 2) * r9
                   + (ulong)(r5 * 2) * r8
                   + (ulong)(r6 * 2) * r7;
                Debug.Assert(d >> 63 == 0);
                // [d 0 0 0 t9 0 0 0 0 0 c t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 
                u3 = d & M; d >>= 26; c += u3 * R0;
                Debug.Assert(u3 >> 26 == 0);
                Debug.Assert(d >> 37 == 0);
                // [d u3 0 0 0 t9 0 0 0 0 0 c-u3*R0 t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 
                t3 = (uint)c & M; c >>= 26; c += u3 * R1;
                Debug.Assert(t3 >> 26 == 0);
                Debug.Assert(c >> 39 == 0);
                // [d u3 0 0 0 t9 0 0 0 0 c-u3*R1 t3-u3*R0 t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 
                // [d 0 0 0 0 t9 0 0 0 0 c t3 t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 0 p3 p2 p1 p0] 

                c += (ulong)(r0 * 2) * r4
                   + (ulong)(r1 * 2) * r3
                   + (ulong)r2 * r2;
                Debug.Assert(c >> 63 == 0);
                // [d 0 0 0 0 t9 0 0 0 0 c t3 t2 t1 t0] = [p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 
                d += (ulong)(r5 * 2) * r9
                   + (ulong)(r6 * 2) * r8
                   + (ulong)r7 * r7;
                Debug.Assert(d >> 62 == 0);
                // [d 0 0 0 0 t9 0 0 0 0 c t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 
                u4 = d & M; d >>= 26; c += u4 * R0;
                Debug.Assert(u4 >> 26 == 0);
                Debug.Assert(d >> 36 == 0);
                // [d u4 0 0 0 0 t9 0 0 0 0 c-u4*R0 t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 
                t4 = (uint)c & M; c >>= 26; c += u4 * R1;
                Debug.Assert(t4 >> 26 == 0);
                Debug.Assert(c >> 39 == 0);
                // [d u4 0 0 0 0 t9 0 0 0 c-u4*R1 t4-u4*R0 t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 
                // [d 0 0 0 0 0 t9 0 0 0 c t4 t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 0 p4 p3 p2 p1 p0] 

                c += (ulong)(r0 * 2) * r5
                   + (ulong)(r1 * 2) * r4
                   + (ulong)(r2 * 2) * r3;
                Debug.Assert(c >> 63 == 0);
                // [d 0 0 0 0 0 t9 0 0 0 c t4 t3 t2 t1 t0] = [p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 
                d += (ulong)(r6 * 2) * r9
                   + (ulong)(r7 * 2) * r8;
                Debug.Assert(d >> 62 == 0);
                // [d 0 0 0 0 0 t9 0 0 0 c t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 
                u5 = d & M; d >>= 26; c += u5 * R0;
                Debug.Assert(u5 >> 26 == 0);
                Debug.Assert(d >> 36 == 0);
                // [d u5 0 0 0 0 0 t9 0 0 0 c-u5*R0 t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 
                t5 = (uint)c & M; c >>= 26; c += u5 * R1;
                Debug.Assert(t5 >> 26 == 0);
                Debug.Assert(c >> 39 == 0);
                // [d u5 0 0 0 0 0 t9 0 0 c-u5*R1 t5-u5*R0 t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 
                // [d 0 0 0 0 0 0 t9 0 0 c t5 t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 0 p5 p4 p3 p2 p1 p0] 

                c += (ulong)(r0 * 2) * r6
                   + (ulong)(r1 * 2) * r5
                   + (ulong)(r2 * 2) * r4
                   + (ulong)r3 * r3;
                Debug.Assert(c >> 63 == 0);
                // [d 0 0 0 0 0 0 t9 0 0 c t5 t4 t3 t2 t1 t0] = [p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 
                d += (ulong)(r7 * 2) * r9
                   + (ulong)r8 * r8;
                Debug.Assert(d >> 61 == 0);
                // [d 0 0 0 0 0 0 t9 0 0 c t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 
                u6 = d & M; d >>= 26; c += u6 * R0;
                Debug.Assert(u6 >> 26 == 0);
                Debug.Assert(d >> 35 == 0);
                // [d u6 0 0 0 0 0 0 t9 0 0 c-u6*R0 t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 
                t6 = (uint)c & M; c >>= 26; c += u6 * R1;
                Debug.Assert(t6 >> 26 == 0);
                Debug.Assert(c >> 39 == 0);
                // [d u6 0 0 0 0 0 0 t9 0 c-u6*R1 t6-u6*R0 t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 
                // [d 0 0 0 0 0 0 0 t9 0 c t6 t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 0 p6 p5 p4 p3 p2 p1 p0] 

                c += (ulong)(r0 * 2) * r7
                   + (ulong)(r1 * 2) * r6
                   + (ulong)(r2 * 2) * r5
                   + (ulong)(r3 * 2) * r4;
                Debug.Assert(c <= 0x8000007C00000007UL);
                // [d 0 0 0 0 0 0 0 t9 0 c t6 t5 t4 t3 t2 t1 t0] = [p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
                d += (ulong)(r8 * 2) * r9;
                Debug.Assert(d >> 58 == 0);
                // [d 0 0 0 0 0 0 0 t9 0 c t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
                u7 = d & M; d >>= 26; c += u7 * R0;
                Debug.Assert(u7 >> 26 == 0);
                Debug.Assert(d >> 32 == 0);
                Debug.Assert(c <= 0x800001703FFFC2F7UL);
                // [d u7 0 0 0 0 0 0 0 t9 0 c-u7*R0 t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
                t7 = (uint)c & M; c >>= 26; c += u7 * R1;
                Debug.Assert(t7 >> 26 == 0);
                Debug.Assert(c >> 38 == 0);
                // [d u7 0 0 0 0 0 0 0 t9 c-u7*R1 t7-u7*R0 t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
                // [d 0 0 0 0 0 0 0 0 t9 c t7 t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 0 p7 p6 p5 p4 p3 p2 p1 p0] 
                c += (ulong)(r0 * 2) * r8
                   + (ulong)(r1 * 2) * r7
                   + (ulong)(r2 * 2) * r6
                   + (ulong)(r3 * 2) * r5
                   + (ulong)r4 * r4;
                Debug.Assert(c <= 0x9000007B80000008UL);
                // [d 0 0 0 0 0 0 0 0 t9 c t7 t6 t5 t4 t3 t2 t1 t0] = [p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                d += (ulong)r9 * r9;
                Debug.Assert(d >> 57 == 0);
                // [d 0 0 0 0 0 0 0 0 t9 c t7 t6 t5 t4 t3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                u8 = d & M; d >>= 26; c += u8 * R0;
                Debug.Assert(u8 >> 26 == 0);
                Debug.Assert(d >> 31 == 0);
                Debug.Assert(c <= 0x9000016FBFFFC2F8UL);
                // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 t6 t5 t4 t3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 

                r3 = t3;
                Debug.Assert(r3 >> 26 == 0);
                // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 t6 t5 t4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                r4 = t4;
                Debug.Assert(r4 >> 26 == 0);
                // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 t6 t5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                r5 = t5;
                Debug.Assert(r5 >> 26 == 0);
                // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 t6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                r6 = t6;
                Debug.Assert(r6 >> 26 == 0);
                // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 t7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                r7 = t7;
                Debug.Assert(r7 >> 26 == 0);
                // [d u8 0 0 0 0 0 0 0 0 t9 c-u8*R0 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 

                r8 = (uint)c & M; c >>= 26; c += u8 * R1;
                Debug.Assert(r8 >> 26 == 0);
                Debug.Assert(c >> 39 == 0);
                // [d u8 0 0 0 0 0 0 0 0 t9+c-u8*R1 r8-u8*R0 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                // [d 0 0 0 0 0 0 0 0 0 t9+c r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                c += d * R0 + t9;
                Debug.Assert(c >> 45 == 0);
                // [d 0 0 0 0 0 0 0 0 0 c-d*R0 r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                r9 = (uint)c & (M >> 4); c >>= 22; c += d * (R1 << 4);
                Debug.Assert(r9 >> 22 == 0);
                Debug.Assert(c >> 46 == 0);
                // [d 0 0 0 0 0 0 0 0 r9+((c-d*R1<<4)<<22)-d*R0 r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                // [d 0 0 0 0 0 0 0 -d*R1 r9+(c<<22)-d*R0 r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                // [r9+(c<<22) r8 r7 r6 r5 r4 r3 t2 t1 t0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 

                d = c * (R0 >> 4) + t0;
                Debug.Assert(d >> 56 == 0);
                // [r9+(c<<22) r8 r7 r6 r5 r4 r3 t2 t1 d-c*R0>>4] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                r0 = (uint)d & M; d >>= 26;
                Debug.Assert(r0 >> 26 == 0);
                Debug.Assert(d >> 30 == 0);
                // [r9+(c<<22) r8 r7 r6 r5 r4 r3 t2 t1+d r0-c*R0>>4] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                d += c * (R1 >> 4) + t1;
                Debug.Assert(d >> 53 == 0);
                Debug.Assert(d <= 0x10000003FFFFBFUL);
                // [r9+(c<<22) r8 r7 r6 r5 r4 r3 t2 d-c*R1>>4 r0-c*R0>>4] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                // [r9 r8 r7 r6 r5 r4 r3 t2 d r0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                r1 = (uint)d & M; d >>= 26;
                Debug.Assert(r1 >> 26 == 0);
                Debug.Assert(d >> 27 == 0);
                Debug.Assert(d <= 0x4000000UL);
                // [r9 r8 r7 r6 r5 r4 r3 t2+d r1 r0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                d += t2;
                Debug.Assert(d >> 27 == 0);
                // [r9 r8 r7 r6 r5 r4 r3 d r1 r0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
                r2 = (uint)d;
                Debug.Assert(r2 >> 27 == 0);
                // [r9 r8 r7 r6 r5 r4 r3 r2 r1 r0] = [p18 p17 p16 p15 p14 p13 p12 p11 p10 p9 p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            }

            return new UInt256_10x26(r0, r1, r2, r3, r4, r5, r6, r7, r8, r9
#if DEBUG
                , 1, false
#endif
                );
        }


        /// <summary>
        /// Return square root of this instance if it has a square root (returns true),
        /// otherwise returns square root of its negation (returns false).
        /// </summary>
        /// <remarks>
        /// Magnitude must be below 8.
        /// Result will have a magnitude of 1 but will not be normalized.
        /// </remarks>
        /// <param name="result">Square root result</param>
        /// <returns>True if square root existed, otherwise false.</returns>
        public bool Sqrt(out UInt256_10x26 result)
        {
#if DEBUG
            Verify();
            VerifyMagnitude(magnitude, 8);
#endif
            // Given that p is congruent to 3 mod 4, we can compute the square root of
            // a mod p as the (p+1)/4'th power of a.
            //
            // As (p+1)/4 is an even number, it will have the same result for a and for
            // (-a). Only one of these two numbers actually has a square root however,
            // so we test at the end by squaring and comparing to the input.
            // Also because (p+1)/4 is an even number, the computed square root is
            // itself always a square (a ** ((p+1)/4) is the square of a ** ((p+1)/8)).

            // The binary representation of (p + 1)/4 has 3 blocks of 1s, with lengths in
            //  { 2, 22, 223 }. Use an addition chain to calculate 2^n - 1 for each block:
            //  1, [2], 3, 6, 9, 11, [22], 44, 88, 176, 220, [223]

            UInt256_10x26 x2, x3, x6, x9, x11, x22, x44, x88, x176, x220, x223, t1;
            x2 = Sqr();
            x2 *= this;

            x3 = x2.Sqr();
            x3 *= this;

            x6 = x3.Sqr(3);
            x6 *= x3;

            x9 = x6.Sqr(3);
            x9 *= x3;

            x11 = x9.Sqr(2);
            x11 *= x2;

            x22 = x11.Sqr(11);
            x22 *= x11;

            x44 = x22.Sqr(22);
            x44 *= x22;

            x88 = x44.Sqr(44);
            x88 *= x44;

            x176 = x88.Sqr(88);
            x176 *= x88;

            x220 = x176.Sqr(44);
            x220 *= x44;

            x223 = x220.Sqr(3);
            x223 *= x3;

            // The final result is then assembled using a sliding window over the blocks. 
            t1 = x223.Sqr(23);
            t1 *= x22;
            t1 = t1.Sqr(6);
            t1 *= x2;
            t1 = t1.Sqr();
            result = t1.Sqr();

            // Check that a square root was actually calculated 
            t1 = result.Sqr();
            bool b = t1.Equals(this);
#if DEBUG
            if (!b)
            {
                t1 = t1.Negate(1);
                t1 = t1.NormalizeVar();
                Debug.Assert(t1.Equals(this));
            }
#endif
            return b;
        }


        /// <summary>
        /// Compute the modular inverse of this field element.
        /// </summary>
        /// <returns>Modular inverse (normalized)</returns>
        public UInt256_10x26 Inverse()
        {
#if DEBUG
            bool input_is_zero = IsZeroNormalized();
            Verify();
#endif

            UInt256_10x26 tmp = Normalize();
            ModInv32Signed30 s = new ModInv32Signed30(tmp);
            ModInv32.Compute(ref s, ModInv32ModInfo.FeConstant);
            UInt256_10x26 r = s.ToUInt256_10x26();

#if DEBUG
            Debug.Assert(r.IsZeroNormalized() == input_is_zero);
            r.Verify();
#endif

            return r;
        }


        /// <summary>
        /// Compute the modular inverse of this field element, without constant-time guarantee.
        /// </summary>
        /// <returns>Modular inverse (normalized)</returns>
        public UInt256_10x26 InverseVar()
        {
#if DEBUG
            bool input_is_zero = IsZeroNormalized();
            Verify();
#endif

            UInt256_10x26 tmp = NormalizeVar();
            ModInv32Signed30 s = new ModInv32Signed30(tmp);
            ModInv32.ComputeVar(ref s, ModInv32ModInfo.FeConstant);
            UInt256_10x26 r = s.ToUInt256_10x26();

#if DEBUG
            Debug.Assert(r.IsZeroNormalized() == input_is_zero);
            r.Verify();
#endif

            return r;
        }


        /// <summary>
        /// Obsolete: Use InverseVar() instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use InverseVar() instead.")]
        public UInt256_10x26 InverseVariable_old()
        {
            return Inverse_old();
        }

        /// <summary>
        /// Obsolete: Use Inverse() instead.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use Inverse() instead.")]
        public UInt256_10x26 Inverse_old()
        {
            UInt256_10x26 x2, x3, x6, x9, x11, x22, x44, x88, x176, x220, x223, t1;
            // The binary representation of (p - 2) has 5 blocks of 1s, with lengths in
            //  { 1, 2, 22, 223 }. Use an addition chain to calculate 2^n - 1 for each block:
            //  [1], [2], 3, 6, 9, 11, [22], 44, 88, 176, 220, [223]

            x2 = Sqr();
            x2 *= this;

            x3 = x2.Sqr();
            x3 *= this;

            x6 = x3;
            x6 = x6.Sqr(3);
            x6 *= x3;

            x9 = x6;
            x9 = x9.Sqr(3);
            x9 *= x3;

            x11 = x9;
            x11 = x11.Sqr(2);

            x11 *= x2;

            x22 = x11;
            x22 = x22.Sqr(11);
            x22 *= x11;

            x44 = x22;
            x44 = x44.Sqr(22);
            x44 *= x22;

            x88 = x44;
            x88 = x88.Sqr(44);
            x88 *= x44;

            x176 = x88;
            x176 = x176.Sqr(88);
            x176 *= x88;

            x220 = x176;
            x220 = x220.Sqr(44);
            x220 *= x44;

            x223 = x220;
            x223 = x223.Sqr(3);
            x223 *= x3;

            // The final result is then assembled using a sliding window over the blocks. 
            t1 = x223;
            t1 = t1.Sqr(23);
            t1 *= x22;

            t1 = t1.Sqr(5);
            t1 *= this;
            t1 = t1.Sqr(3);
            t1 *= x2;
            t1 = t1.Sqr(2);
            return this * t1;
        }


        /// <summary>
        /// Conditional move. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is true (=1).
        /// </summary>
        /// <remarks>
        /// This method is constant time.
        /// </remarks>
        /// <param name="r">Destination</param>
        /// <param name="a">Source</param>
        /// <param name="flag">Zero or one. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is one.</param>
        /// <returns><paramref name="a"/> if flag was one; otherwise r.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt256_10x26 CMov(in UInt256_10x26 r, in UInt256_10x26 a, uint flag)
        {
#if DEBUG
            r.Verify();
            a.Verify();
            Debug.Assert(flag == 0 || flag == 1);
#endif
            uint mask0 = flag + ~0U;
            uint mask1 = ~mask0;
            return new UInt256_10x26(
                (r.b0 & mask0) | (a.b0 & mask1),
                (r.b1 & mask0) | (a.b1 & mask1),
                (r.b2 & mask0) | (a.b2 & mask1),
                (r.b3 & mask0) | (a.b3 & mask1),
                (r.b4 & mask0) | (a.b4 & mask1),
                (r.b5 & mask0) | (a.b5 & mask1),
                (r.b6 & mask0) | (a.b6 & mask1),
                (r.b7 & mask0) | (a.b7 & mask1),
                (r.b8 & mask0) | (a.b8 & mask1),
                (r.b9 & mask0) | (a.b9 & mask1)
#if DEBUG
                , a.magnitude > r.magnitude ? a.magnitude : r.magnitude,
                !a.isNormalized ? false : r.isNormalized
#endif
                );
        }


        /// <summary>
        /// Converts this instance to <see cref="UInt256_8x32"/>.
        /// Assumes the instance is normalized.
        /// </summary>
        /// <returns>Result</returns>
        public UInt256_8x32 ToUInt256_8x32()
        {
#if DEBUG
            Verify();
            Debug.Assert(isNormalized);
#endif
            return new UInt256_8x32(this);
        }


        /// <summary>
        /// Converts this instance to a 32-byte array in big-endian order.
        /// </summary>
        /// <returns>Big-endian byte array</returns>
        public byte[] ToByteArray()
        {
            Span<byte> res = new byte[32];
            WriteToSpan(res);
            return res.ToArray();
        }

        /// <summary>
        /// Converts this instance to a 32-byte array in big-endian order and writes it to the given array.
        /// </summary>
        /// <remarks>
        /// Assumes this instance is already normalized.
        /// </remarks>
        /// <param name="ba">Array to use</param>
        public void WriteToSpan(Span<byte> ba)
        {
#if DEBUG
            Verify();
            Debug.Assert(isNormalized);
#endif
            Debug.Assert(ba.Length >= 32);
            // Note: Last item is 22 bits, the rest are 26 bits
            // Read comments from bottom to make sense, array is set in reverse for optimization
            ba[31] = (byte)b0; // 8(0)
            ba[30] = (byte)(b0 >> 8); // 8(8)
            ba[29] = (byte)(b0 >> 16); // 8(16)
            Debug.Assert(((b0 >> 24) & 0b11111100) == 0);
            ba[28] = (byte)((b1 << 2) | (b0 >> 24)); // 6(0)+2(24)
            ba[27] = (byte)(b1 >> 6); // 8(6)
            ba[26] = (byte)(b1 >> 14); // 8(14)
            Debug.Assert(((b1 >> 22) & 0b11110000) == 0);
            ba[25] = (byte)((b2 << 4) | (b1 >> 22)); // 4(0)+4(22)
            ba[24] = (byte)(b2 >> 4); // 8(4)
            ba[23] = (byte)(b2 >> 12); // 8(12)
            Debug.Assert(((b2 >> 20) & 0b11000000) == 0);
            ba[22] = (byte)((b3 << 6) | (b2 >> 20)); // 2(0)+6(20)
            ba[21] = (byte)(b3 >> 2); // 8(2)
            ba[20] = (byte)(b3 >> 10); // 8(10)
            ba[19] = (byte)(b3 >> 18); // 8(18)
            ba[18] = (byte)b4; // 8(0)
            ba[17] = (byte)(b4 >> 8); // 8(8)
            ba[16] = (byte)(b4 >> 16); // 8(16)
            Debug.Assert(((b4 >> 24) & 0b11111100) == 0);
            ba[15] = (byte)((b5 << 2) | (b4 >> 24)); // 6(0)+2(24)
            ba[14] = (byte)(b5 >> 6); // 8(6)
            ba[13] = (byte)(b5 >> 14); // 8(14)
            Debug.Assert(((b5 >> 22) & 0b11110000) == 0);
            ba[12] = (byte)((b6 << 4) | (b5 >> 22)); // 4(0)+4(22)
            ba[11] = (byte)(b6 >> 4); // 8(4)
            ba[10] = (byte)(b6 >> 12); // 8(12)
            Debug.Assert(((b6 >> 20) & 0b11000000) == 0);
            ba[9] = (byte)((b7 << 6) | (b6 >> 20)); // 2(0)+6(20)
            ba[8] = (byte)(b7 >> 2); // 8(2)
            ba[7] = (byte)(b7 >> 10); // 8(10)
            ba[6] = (byte)(b7 >> 18); // 8(18)
            ba[5] = (byte)b8; // 8(0)
            ba[4] = (byte)(b8 >> 8); // 8(8)
            ba[3] = (byte)(b8 >> 16); // 8(16)
            Debug.Assert(((b8 >> 24) & 0b11111100) == 0);
            ba[2] = (byte)((b9 << 2) | (b8 >> 24)); // 6(0)+2(26-2=24)
            ba[1] = (byte)(b9 >> 6); // 8(14-8=6)
            ba[0] = (byte)(b9 >> 14); // Take 8 bits (rem=22-8=14)
        }


        /// <summary>
        /// Compare 2 <see cref="UInt256_10x26"/> values.
        /// Assumes both values are normalized.
        /// </summary>
        /// <remarks>
        /// This method is not constant time.
        /// </remarks>
        /// <param name="b">Other value to compare to</param>
        /// <returns>1 if this is bigger than <paramref name="b"/>, -1 if smaller and 0 if equal.</returns>
        public int CompareToVar(in UInt256_10x26 b)
        {
#if DEBUG
            Debug.Assert(isNormalized);
            Debug.Assert(b.isNormalized);
            Verify();
            b.Verify();
#endif
            if (b9 > b.b9) return 1; else if (b9 < b.b9) return -1;
            if (b8 > b.b8) return 1; else if (b8 < b.b8) return -1;
            if (b7 > b.b7) return 1; else if (b7 < b.b7) return -1;
            if (b6 > b.b6) return 1; else if (b6 < b.b6) return -1;
            if (b5 > b.b5) return 1; else if (b5 < b.b5) return -1;
            if (b4 > b.b4) return 1; else if (b4 < b.b4) return -1;
            if (b3 > b.b3) return 1; else if (b3 < b.b3) return -1;
            if (b2 > b.b2) return 1; else if (b2 < b.b2) return -1;
            if (b1 > b.b1) return 1; else if (b1 < b.b1) return -1;
            if (b0 > b.b0) return 1; else if (b0 < b.b0) return -1;

            return 0;
        }

        /// <summary>
        /// Returns if the given <see cref="UInt256_10x26"/> is equal to this instance.
        /// </summary>
        /// <remarks>
        /// This method is constant time.
        /// Magnitude of this instance should be at most 1.
        /// Magnitude of b should be at most 31.
        /// </remarks>
        /// <param name="b">Other <see cref="UInt256_10x26"/> to compare to (magnitude of at most 31)</param>
        /// <returns>True if the two instances are equal; otherwise false.</returns>
        public bool Equals(in UInt256_10x26 b)
        {
#if DEBUG
            Verify();
            b.Verify();
            VerifyMagnitude(magnitude, 1);
            VerifyMagnitude(b.magnitude, 31);
#endif
            UInt256_10x26 na = Negate(1);
            na += b;
            return na.IsZeroNormalized();
        }
    }
}
