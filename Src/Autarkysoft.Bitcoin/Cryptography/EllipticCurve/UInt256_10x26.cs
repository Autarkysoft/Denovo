// Autarkysoft.Bitcoin
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
    /// </summary>
    /// <remarks>
    /// This implements a UInt256 using 10x UInt32 parts (total of 320 bits).
    /// When normalized, each limb stores 26 bits except the last one that stores 22 bits.
    /// <para/>The arithmetic here is all modulo secp256k1 prime
    /// </remarks>
    public readonly struct UInt256_10x26
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given unsigned 32-bit integer.
        /// </summary>
        /// <param name="a">Value to use (must be smaller than 0x7FFF</param>
        public UInt256_10x26(uint a)
        {
            // TODO: libsecp256k1 enforces <= 0x7FFF
            Debug.Assert((a >> 26) == 0);

            b0 = a;
            b1 = 0; b2 = 0; b3 = 0; b4 = 0;
            b5 = 0; b6 = 0; b7 = 0; b8 = 0; b9 = 0;
#if DEBUG
            magnitude = (a == 0) ? 0 : 1;
            isNormalized = true;
            Debug.Assert(Verify());
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given parameters.
        /// </summary>
        /// <param name="u0">1st 32 bits</param>
        /// <param name="u1">2nd 32 bits</param>
        /// <param name="u2">3rd 32 bits</param>
        /// <param name="u3">4th 32 bits</param>
        /// <param name="u4">5th 32 bits</param>
        /// <param name="u5">6th 32 bits</param>
        /// <param name="u6">7th 32 bits</param>
        /// <param name="u7">8th 32 bits</param>
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
            magnitude = 1;
            isNormalized = true;
            Debug.Assert(Verify());
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given parameters.
        /// </summary>
        /// <param name="u0">1st 26 bits</param>
        /// <param name="u1">2nd 26 bits</param>
        /// <param name="u2">3rd 26 bits</param>
        /// <param name="u3">4th 26 bits</param>
        /// <param name="u4">5th 26 bits</param>
        /// <param name="u5">6th 26 bits</param>
        /// <param name="u6">7th 26 bits</param>
        /// <param name="u7">8th 26 bits</param>
        /// <param name="u8">9th 26 bits</param>
        /// <param name="u9">10th 22 bits</param>
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
            Debug.Assert(Verify());
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given array.
        /// </summary>
        /// <param name="arr9">Array</param>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="normalized">Is normalized</param>
        public UInt256_10x26(ReadOnlySpan<uint> arr9
#if DEBUG
            , int magnitude, bool normalized
#endif
            )
        {
            Debug.Assert(arr9.Length == 10);

            b0 = arr9[0]; b1 = arr9[1]; b2 = arr9[2]; b3 = arr9[3]; b4 = arr9[4];
            b5 = arr9[5]; b6 = arr9[6]; b7 = arr9[7]; b8 = arr9[8]; b9 = arr9[9];
#if DEBUG
            this.magnitude = magnitude;
            isNormalized = normalized;
            Debug.Assert(Verify());
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_10x26"/> using the given 32-byte array.
        /// </summary>
        /// <param name="ba32">32-byte array</param>
        /// <param name="isValid"></param>
        public UInt256_10x26(ReadOnlySpan<byte> ba32, out bool isValid)
        {
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

            isValid = !((b9 == 0x3FFFFFU) & ((b8 & b7 & b6 & b5 & b4 & b3 & b2) == 0x3FFFFFFU) &
                        ((b1 + 0x40U + ((b0 + 0x3D1U) >> 26)) > 0x3FFFFFFU));
#if DEBUG
            magnitude = 1;
            isNormalized = isValid;
            Debug.Assert(Verify());
#endif
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

        private bool Verify()
        {
            int m = isNormalized ? 1 : 2 * magnitude;
            int r = 1;
            r &= (b0 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b1 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b2 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b3 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b4 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b5 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b6 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b7 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b8 <= 0x3FFFFFFU * m) ? 1 : 0;
            r &= (b9 <= 0x03FFFFFU * m) ? 1 : 0;
            r &= (magnitude >= 0 ? 1 : 0);
            r &= (magnitude <= 32 ? 1 : 0);
            if (isNormalized)
            {
                r &= (magnitude <= 1 ? 1 : 0);
                if (r != 0 && (b9 == 0x03FFFFFU))
                {
                    uint mid = b8 & b7 & b6 & b5 & b4 & b3 & b2;
                    if (mid == 0x3FFFFFFU)
                    {
                        r &= ((b1 + 0x40U + ((b0 + 0x3D1U) >> 26)) <= 0x3FFFFFFU) ? 1 : 0;
                    }
                }
            }
            return r == 1;
        }
#endif

        private static readonly UInt256_10x26 _zero = new UInt256_10x26(0, 0, 0, 0, 0, 0, 0, 0, 0, 0
#if DEBUG
            , 0, true
#endif
            );
        private static readonly UInt256_10x26 _one = new UInt256_10x26(1, 0, 0, 0, 0, 0, 0, 0, 0, 0
#if DEBUG
            , 0, true
#endif
            );
        private static readonly UInt256_10x26 _seven = new UInt256_10x26(7, 0, 0, 0, 0, 0, 0, 0, 0, 0
#if DEBUG
            , 0, true
#endif
            );
        /// <summary>
        /// Zero
        /// </summary>
        public static ref readonly UInt256_10x26 Zero => ref _zero;
        /// <summary>
        /// One
        /// </summary>
        public static ref readonly UInt256_10x26 One => ref _one;
        /// <summary>
        /// Seven (secp256k1 b value)
        /// </summary>
        public static ref readonly UInt256_10x26 Seven => ref _seven;

        /// <summary>
        /// Returns if this instance is odd (needs to be normalized)
        /// </summary>
        public readonly bool IsOdd
        {
            get
            {
#if DEBUG
                Debug.Assert(isNormalized);
                Debug.Assert(Verify());
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
                Debug.Assert(Verify());
#endif
                return (b0 | b1 | b2 | b3 | b4 | b5 | b6 | b7 | b8 | b9) == 0;
            }
        }

        /// <summary>
        /// Returns if this instance is zero when normalized
        /// </summary>
        /// <remarks>
        /// This method is constant time
        /// </remarks>
        /// <returns>True if normalizes to zero</returns>
        public bool IsZeroNormalized()
        {
            // z0 tracks a possible raw value of 0, z1 tracks a possible raw value of P
            uint z0, z1;

            // Reduce b9 at the start so there will be at most a single carry from the first pass
            uint x = b9 >> 22;
            uint t9 = b9 & 0b00000000_00111111_11111111_11111111;

            // The first pass ensures the magnitude is 1, ...
            uint t0 = b0 + (x * 0x3D1U); uint t1 = b1 + (x << 6);
            t1 += (t0 >> 26); t0 &= 0x03FFFFFFU; z0 = t0; z1 = t0 ^ 0x3D0U;
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

            return (z0 == 0) | (z1 == 0x3FFFFFFU);
        }

        /// <summary>
        /// Returns if this instance is zero when normalized without constant-time guarantee
        /// </summary>
        /// <remarks>
        /// This method is not constant time
        /// </remarks>
        /// <returns>True if normalizes to zero</returns>
        public bool IsZeroNormalizedVar()
        {
            // Reduce t9 at the start so there will be at most a single carry from the first pass
            uint x = b9 >> 22;

            // The first pass ensures the magnitude is 1, ...
            uint t0 = b0 + (x * 0x3D1U);

            // z0 tracks a possible raw value of 0, z1 tracks a possible raw value of P
            uint z0 = t0 & 0x3FFFFFFU;
            uint z1 = z0 ^ 0x3D0U;

            // Fast return path should catch the majority of cases
            if ((z0 != 0U) & (z1 != 0x3FFFFFFU))
            {
                return false;
            }

            uint t9 = b9 & 0x03FFFFFU;
            uint t1 = b1 + (x << 6);

            t1 += (t0 >> 26);
            uint t2 = b2 + (t1 >> 26); t1 &= 0x3FFFFFFU; z0 |= t1; z1 &= t1 ^ 0x40U;
            uint t3 = b3 + (t2 >> 26); t2 &= 0x3FFFFFFU; z0 |= t2; z1 &= t2;
            uint t4 = b4 + (t3 >> 26); t3 &= 0x3FFFFFFU; z0 |= t3; z1 &= t3;
            uint t5 = b5 + (t4 >> 26); t4 &= 0x3FFFFFFU; z0 |= t4; z1 &= t4;
            uint t6 = b6 + (t5 >> 26); t5 &= 0x3FFFFFFU; z0 |= t5; z1 &= t5;
            uint t7 = b7 + (t6 >> 26); t6 &= 0x3FFFFFFU; z0 |= t6; z1 &= t6;
            uint t8 = b8 + (t7 >> 26); t7 &= 0x3FFFFFFU; z0 |= t7; z1 &= t7;
            t9 += (t8 >> 26); t8 &= 0x3FFFFFFU; z0 |= t8; z1 &= t8;
            z0 |= t9; z1 &= t9 ^ 0x3C00000U;

            // ... except for a possible carry at bit 22 of t9 (i.e. bit 256 of the field element)
            Debug.Assert(t9 >> 23 == 0);

            return (z0 == 0) | (z1 == 0x3FFFFFFU);
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

            // ... except for a possible carry at bit 22 of t[9] (i.e. bit 256 of the field element)
            Debug.Assert(t9 >> 23 == 0);

            // At most a single final reduction is needed; check if the value is >= the field characteristic
            x = (t9 >> 22) | ((t9 == 0x003FFFFFU ? 1u : 0) & (m == 0x03FFFFFFU ? 1u : 0)
                & ((t1 + 0x40U + ((t0 + 0x03D1U) >> 26)) > 0x03FFFFFFU ? 1u : 0));

            // Apply the final reduction (always do it for constant-time behaviour)
            t0 += x * 0x3D1U; t1 += (x << 6);
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
        /// <returns></returns>
        public UInt256_10x26 NormalizeWeak()
        {
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
            // Reduce t9 at the start so there will be at most a single carry from the first pass
            uint m;
            uint x = b9 >> 22;
            uint t9 = b9 & 0x03FFFFFU;

            // The first pass ensures the magnitude is 1, ...
            uint t0 = b0 + (x * 0x03D1U); uint t1 = b1 + (x << 6);
            t1 += (t0 >> 26); t0 &= 0x03FFFFFFU;
            uint t2 = b2 + (t1 >> 26); t1 &= 0x03FFFFFFU;
            uint t3 = b3 + (t2 >> 26); t2 &= 0x03FFFFFFU; m = t2;
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
                t1 += (t0 >> 26); t0 &= 0x3FFFFFFU;
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
                t9 &= 0x03FFFFFU;
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
        /// <param name="other">The value to add</param>
        /// <returns>Result of the addition</returns>
        public readonly UInt256_10x26 Add(in UInt256_10x26 other)
        {
            return new UInt256_10x26(
                b0 + other.b0,
                b1 + other.b1,
                b2 + other.b2,
                b3 + other.b3,
                b4 + other.b4,
                b5 + other.b5,
                b6 + other.b6,
                b7 + other.b7,
                b8 + other.b8,
                b9 + other.b9
#if DEBUG
                , magnitude + other.magnitude,
                false
#endif
                );
        }


        /// <summary>
        /// Returns the additive inverse of this instance. Takes a maximum magnitude of the input as an argument.
        /// </summary>
        /// <param name="m">Magnitude</param>
        /// <returns>Additive inverse of this instance with a magnitude that is one higher than <paramref name="m"/></returns>
        public UInt256_10x26 Negate(int m)
        {
#if DEBUG
            Debug.Assert(magnitude <= m);
#endif
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
        /// Converts this instance to a 32-byte array in big-endian order and writes it to the given array.
        /// </summary>
        /// <remarks>
        /// Assumes this instance is already normalized.
        /// </remarks>
        /// <param name="ba">Array to use</param>
        public void WriteToSpan(Span<byte> ba)
        {
#if DEBUG
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
    }
}
