// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.ModInv;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    /// <summary>
    /// 256-bit unsigned integer used as field elements, implemented using radix-2^52 representation (instead of 2^64)
    /// in little-endian order.
    /// <para/>integer = sum(i=0..4, b[i]*2^(i*52)) % p [where p = 2^256 - 0x1000003D1]
    /// <para/>integer value can exceed P unless it's normalized
    /// </summary>
    /// <remarks>
    /// This implements a UInt256 using 5x UInt64 limbs (total of 320 bits).
    /// When normalized, each limb stores 52 bits except the last one that stores 48 bits.
    /// <para/>The arithmetic here is all modulo secp256k1 prime
    /// </remarks>
    [DebuggerDisplay("{ToSpan().ToArray().ToBase16()}")]
    public readonly struct UInt256_5x52
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_5x52"/> using the given unsigned 32-bit integer.
        /// </summary>
        /// <param name="a">32-bit integer</param>
        public UInt256_5x52(uint a)
        {
            b0 = a;
            b1 = 0; b2 = 0; b3 = 0; b4 = 0;
#if DEBUG
            magnitude = (a == 0) ? 0 : 1;
            isNormalized = true;
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_5x52"/> using the given parameters.
        /// </summary>
        /// <param name="u0">1st 64 bits (least significant)</param>
        /// <param name="u1">2nd 64 bits</param>
        /// <param name="u2">3rd 64 bits</param>
        /// <param name="u3">4th 64 bits (most significant)</param>
        public UInt256_5x52(ulong u0, ulong u1, ulong u2, ulong u3)
        {
            // Total=52: 52 bits ulong_0 -> remaining=12(=64-52)
            b0 = u0 & 0x000FFFFFFFFFFFFFUL;
            // Total=52: 12 bits ulong_0 + 40 bits ulong_1 -> rem=24(=64-40)
            b1 = u0 >> 52 | ((u1 << 12) & 0x000FFFFFFFFFFFFFUL);
            // Total=52: 24 bits ulong_1 + 28 bits ulong_2 -> rem=36(=64-28)
            b2 = u1 >> 40 | ((u2 << 24) & 0x000FFFFFFFFFFFFFUL);
            // Total=52: 36 bits ulong_2 + 16 bits ulong_3 -> rem=48(=64-16)
            b3 = u2 >> 28 | ((u3 << 36) & 0x000FFFFFFFFFFFFFUL);
            // Total=48: 48 bits ulong_3
            b4 = u3 >> 16;
#if DEBUG
            magnitude = ((b0 | b1 | b2 | b3 | b4) == 0) ? 0 : 1;
            isNormalized = !((b4 == 0x0FFFFFFFFFFFFUL) & ((b3 & b2 & b1) == 0xFFFFFFFFFFFFFUL) & (b0 >= 0xFFFFEFFFFFC2FUL));
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_5x52"/> using the given parameters.
        /// </summary>
        /// <param name="u0">1st 52 bits (least significant)</param>
        /// <param name="u1">2nd 52 bits</param>
        /// <param name="u2">3rd 52 bits</param>
        /// <param name="u3">4th 52 bits</param>
        /// <param name="u4">5th 48 bits (most significant)</param>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="normalized">Is normalized</param>
        public UInt256_5x52(ulong u0, ulong u1, ulong u2, ulong u3, ulong u4
#if DEBUG
            , int magnitude, bool normalized
#endif
            )
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3; b4 = u4;
#if DEBUG
            this.magnitude = magnitude;
            isNormalized = normalized;
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_5x52"/> using the given array containing 5 limbs.
        /// </summary>
        /// <param name="arr5">Array containing 5 limbs.</param>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="normalized">Is normalized</param>
        public UInt256_5x52(ReadOnlySpan<ulong> arr5
#if DEBUG
            , int magnitude, bool normalized
#endif
            )
        {
            Debug.Assert(arr5.Length == 5);

            b0 = arr5[0]; b1 = arr5[1]; b2 = arr5[2]; b3 = arr5[3]; b4 = arr5[4];
#if DEBUG
            this.magnitude = magnitude;
            isNormalized = normalized;
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_5x52"/> using the given 32-byte big-endian array,
        /// checking for overflow.
        /// </summary>
        /// <remarks>
        /// If <paramref name="ba32"/> is &lt; P, the instance is normalized with magnitude 1;
        /// otherwise if <paramref name="ba32"/> is &gt;= P, the instance will be made invalid
        /// (and must not be used without overwriting)
        /// </remarks>
        /// <param name="ba32">32-byte array</param>
        /// <param name="isValid">Returns if it didn't overflow and if the instance is valid (can be used)</param>
        public UInt256_5x52(ReadOnlySpan<byte> ba32, out bool isValid)
        {
            // This is the same as secp256k1_fe_impl_set_b32_limit
            Debug.Assert(ba32.Length == 32);

            // 8 + 8 + 8 + 8 + 8 + 8 + 4
            b0 = (ulong)ba32[31] |
                ((ulong)ba32[30] << 8) |
                ((ulong)ba32[29] << 16) |
                ((ulong)ba32[28] << 24) |
                ((ulong)ba32[27] << 32) |
                ((ulong)ba32[26] << 40) |
                ((ulong)(ba32[25] & 0b00001111) << 48);
            // 4 + 8 + 8 + 8 + 8 + 8 + 8
            b1 = (ulong)ba32[25] >> 4 |
                ((ulong)ba32[24] << 4) |
                ((ulong)ba32[23] << 12) |
                ((ulong)ba32[22] << 20) |
                ((ulong)ba32[21] << 28) |
                ((ulong)ba32[20] << 36) |
                ((ulong)ba32[19] << 44);
            b2 = (ulong)ba32[18] |
                ((ulong)ba32[17] << 8) |
                ((ulong)ba32[16] << 16) |
                ((ulong)ba32[15] << 24) |
                ((ulong)ba32[14] << 32) |
                ((ulong)ba32[13] << 40) |
                ((ulong)(ba32[12] & 0b00001111) << 48);
            b3 = (ulong)ba32[12] >> 4 |
                ((ulong)ba32[11] << 4) |
                ((ulong)ba32[10] << 12) |
                ((ulong)ba32[9] << 20) |
                ((ulong)ba32[8] << 28) |
                ((ulong)ba32[7] << 36) |
                ((ulong)ba32[6] << 44);
            // 8 + 8 + 8 + 8 + 8 + 8
            b4 = (ulong)ba32[5] |
                ((ulong)ba32[4] << 8) |
                ((ulong)ba32[3] << 16) |
                ((ulong)ba32[2] << 24) |
                ((ulong)ba32[1] << 32) |
                ((ulong)ba32[0] << 40);

            isValid = !((b4 == 0x0FFFFFFFFFFFFUL) & ((b3 & b2 & b1) == 0xFFFFFFFFFFFFFUL) & (b0 >= 0xFFFFEFFFFFC2FUL));
#if DEBUG
            isNormalized = isValid;
            if (isValid)
            {
                magnitude = 1;
                Verify();
            }
            else
            {
                // Mark the output field element as invalid.
                magnitude = -1;
            }
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_5x52"/> using the given 32-byte big-endian array
        /// interpreted modulo p.
        /// </summary>
        /// <remarks>
        /// The instance will be <paramref name="ba32"/> (mod p). It will have magnitude 1, and not be normalized.
        /// </remarks>
        /// <param name="ba32">32-byte array</param>
        public UInt256_5x52(ReadOnlySpan<byte> ba32)
        {
            // This is the same as secp256k1_fe_impl_set_b32_mod
            Debug.Assert(ba32.Length == 32);

            // 8 + 8 + 8 + 8 + 8 + 8 + 4
            b0 = (ulong)ba32[31] |
                ((ulong)ba32[30] << 8) |
                ((ulong)ba32[29] << 16) |
                ((ulong)ba32[28] << 24) |
                ((ulong)ba32[27] << 32) |
                ((ulong)ba32[26] << 40) |
                ((ulong)(ba32[25] & 0b00001111) << 48);
            // 4 + 8 + 8 + 8 + 8 + 8 + 8
            b1 = (ulong)ba32[25] >> 4 |
                ((ulong)ba32[24] << 4) |
                ((ulong)ba32[23] << 12) |
                ((ulong)ba32[22] << 20) |
                ((ulong)ba32[21] << 28) |
                ((ulong)ba32[20] << 36) |
                ((ulong)ba32[19] << 44);
            b2 = (ulong)ba32[18] |
                ((ulong)ba32[17] << 8) |
                ((ulong)ba32[16] << 16) |
                ((ulong)ba32[15] << 24) |
                ((ulong)ba32[14] << 32) |
                ((ulong)ba32[13] << 40) |
                ((ulong)(ba32[12] & 0b00001111) << 48);
            b3 = (ulong)ba32[12] >> 4 |
                ((ulong)ba32[11] << 4) |
                ((ulong)ba32[10] << 12) |
                ((ulong)ba32[9] << 20) |
                ((ulong)ba32[8] << 28) |
                ((ulong)ba32[7] << 36) |
                ((ulong)ba32[6] << 44);
            // 8 + 8 + 8 + 8 + 8 + 8
            b4 = (ulong)ba32[5] |
                ((ulong)ba32[4] << 8) |
                ((ulong)ba32[3] << 16) |
                ((ulong)ba32[2] << 24) |
                ((ulong)ba32[1] << 32) |
                ((ulong)ba32[0] << 40);

#if DEBUG
            magnitude = 1;
            isNormalized = false;
            Verify();
#endif
        }

        private UInt256_5x52(uint d0, uint d1, uint d2, uint d3, uint d4, uint d5, uint d6, uint d7)
        {
            b0 = d0 | (((ulong)d1 & 0xFFFFFU) << 32);
            b1 = ((ulong)d1 >> 20) | (((ulong)d2) << 12) | (((ulong)d3 & 0xFFU) << 44);
            b2 = ((ulong)d3 >> 8) | (((ulong)d4 & 0xFFFFFFFU) << 24);
            b3 = ((ulong)d4 >> 28) | (((ulong)d5) << 4) | (((ulong)d6 & 0xFFFFU) << 36);
            b4 = ((ulong)d6 >> 16) | (((ulong)d7) << 16);
#if DEBUG
            magnitude = ((b0 | b1 | b2 | b3 | b4) == 0) ? 0 : 1;
            isNormalized = !((b4 == 0x0FFFFFFFFFFFFUL) & ((b3 & b2 & b1) == 0xFFFFFFFFFFFFFUL) & (b0 >= 0xFFFFEFFFFFC2FUL));
            Verify();
#endif
        }


        /// <summary>
        /// Bit chunks
        /// </summary>
        public readonly ulong b0, b1, b2, b3, b4;


#if DEBUG
        /// <summary>Only works in DEBUG</summary>
        /// <remarks>
        /// An integer in [0,32]
        /// <para/>Magnitude means:
        /// <para/>n[i] &lt;= 2 * m * (2^52 - 1) for i=0..3
        /// <para/>n[4] &lt;= 2 * m * (2^48 - 1)
        /// </remarks>
        public readonly int magnitude;
        /// <summary>Only works in DEBUG</summary>
        /// <remarks>
        /// The value is normalized if reduced modulo the order of the field.
        /// It will also have a magnitude of 0 or 1.
        /// <para/>Normalize requires:
        /// <para/>n[i] &lt;= (2^52 - 1) for i=0..3
        /// <para/>sum(i=0..4, n[i] &lt;&lt; (i*52)) &lt; p
        /// <para/>(together these imply n[4] &lt;= 2^48 - 1)
        /// </remarks>
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
            Debug.Assert(b0 <= 0xFFFFFFFFFFFFFUL * (ulong)m);
            Debug.Assert(b1 <= 0xFFFFFFFFFFFFFUL * (ulong)m);
            Debug.Assert(b2 <= 0xFFFFFFFFFFFFFUL * (ulong)m);
            Debug.Assert(b3 <= 0xFFFFFFFFFFFFFUL * (ulong)m);
            Debug.Assert(b4 <= 0x0FFFFFFFFFFFFUL * (ulong)m);

            if (isNormalized)
            {
                if ((b4 == 0x0FFFFFFFFFFFFUL) && ((b3 & b2 & b1) == 0xFFFFFFFFFFFFFUL))
                {
                    Debug.Assert(b0 < 0xFFFFEFFFFFC2FUL);
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


        private static readonly UInt256_5x52 _zero = new UInt256_5x52(0, 0, 0, 0, 0
#if DEBUG
            , 0, true
#endif
            );
        private static readonly UInt256_5x52 _one = new UInt256_5x52(1, 0, 0, 0, 0
#if DEBUG
            , 1, true
#endif
            );

        private static readonly UInt256_5x52 _n = new UInt256_5x52(0xD0364141U, 0xBFD25E8CU, 0xAF48A03BU, 0xBAAEDCE6U,
                                                                   0xFFFFFFFEU, 0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU);
        private static readonly UInt256_5x52 _pn = new UInt256_5x52(0x2FC9BAEEU, 0x402DA172U, 0x50B75FC4U, 0x45512319U,
                                                                    1, 0, 0, 0);
        private static readonly UInt256_5x52 _beta = new UInt256_5x52(0x719501eeU, 0xc1396c28U, 0x12f58995U, 0x9cf04975U,
                                                                      0xac3434e9U, 0x6e64479eU, 0x657c0710U, 0x7ae96a2bU);

        /// <summary>
        /// Secp256k1 order (0xfffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141)
        /// </summary>
        public static ref readonly UInt256_5x52 N => ref _n;
        /// <summary>
        /// Difference between secp256k1 prime and order (p-n=0x014551231950b75fc4402da1722fc9baee)
        /// </summary>
        public static ref readonly UInt256_5x52 PMinusN => ref _pn;
        /// <summary>
        /// secp256k1_const_beta
        /// </summary>
        public static ref readonly UInt256_5x52 Beta => ref _beta;
        /// <summary>
        /// Zero
        /// </summary>
        public static ref readonly UInt256_5x52 Zero => ref _zero;
        /// <summary>
        /// One
        /// </summary>
        public static ref readonly UInt256_5x52 One => ref _one;

        /// <summary>
        /// Returns if this instance is odd (instance must be normalized)
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
        /// Returns if this instance is zero (instance must be normalized)
        /// </summary>
        public bool IsZero
        {
            get
            {
#if DEBUG
                Debug.Assert(isNormalized);
                Verify();
#endif
                return (b0 | b1 | b2 | b3 | b4) == 0;
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
            // secp256k1_fe_impl_normalizes_to_zero
#if DEBUG
            Verify();
#endif
            ulong t0 = b0, t1 = b1, t2 = b2, t3 = b3, t4 = b4;

            // z0 tracks a possible raw value of 0, z1 tracks a possible raw value of P
            ulong z0, z1;

            // Reduce t4 at the start so there will be at most a single carry from the first pass
            ulong x = t4 >> 48; t4 &= 0x0FFFFFFFFFFFFUL;

            // The first pass ensures the magnitude is 1, ...
            t0 += x * 0x1000003D1UL;
            t1 += (t0 >> 52); t0 &= 0xFFFFFFFFFFFFFUL; z0 = t0; z1 = t0 ^ 0x1000003D0UL;
            t2 += (t1 >> 52); t1 &= 0xFFFFFFFFFFFFFUL; z0 |= t1; z1 &= t1;
            t3 += (t2 >> 52); t2 &= 0xFFFFFFFFFFFFFUL; z0 |= t2; z1 &= t2;
            t4 += (t3 >> 52); t3 &= 0xFFFFFFFFFFFFFUL; z0 |= t3; z1 &= t3;
            z0 |= t4; z1 &= t4 ^ 0xF000000000000UL;

            // ... except for a possible carry at bit 48 of t4 (i.e. bit 256 of the field element)
            Debug.Assert(t4 >> 49 == 0);

            return (z0 == 0) | (z1 == 0xFFFFFFFFFFFFFUL);
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
            // secp256k1_fe_impl_normalizes_to_zero_var
#if DEBUG
            Verify();
#endif
            ulong t0, t1, t2, t3, t4;
            ulong z0, z1;
            ulong x;

            t0 = b0;
            t4 = b4;

            // Reduce t4 at the start so there will be at most a single carry from the first pass
            x = t4 >> 48;

            // The first pass ensures the magnitude is 1, ...
            t0 += x * 0x1000003D1UL;

            // z0 tracks a possible raw value of 0, z1 tracks a possible raw value of P */
            z0 = t0 & 0xFFFFFFFFFFFFFUL;
            z1 = z0 ^ 0x1000003D0UL;

            // Fast return path should catch the majority of cases
            if ((z0 != 0UL) & (z1 != 0xFFFFFFFFFFFFFUL))
            {
                return false;
            }

            t1 = b1;
            t2 = b2;
            t3 = b3;

            t4 &= 0x0FFFFFFFFFFFFUL;

            t1 += (t0 >> 52);
            t2 += (t1 >> 52); t1 &= 0xFFFFFFFFFFFFFUL; z0 |= t1; z1 &= t1;
            t3 += (t2 >> 52); t2 &= 0xFFFFFFFFFFFFFUL; z0 |= t2; z1 &= t2;
            t4 += (t3 >> 52); t3 &= 0xFFFFFFFFFFFFFUL; z0 |= t3; z1 &= t3;
            z0 |= t4; z1 &= t4 ^ 0xF000000000000UL;

            // ... except for a possible carry at bit 48 of t4 (i.e. bit 256 of the field element)
            Debug.Assert(t4 >> 49 == 0);

            return (z0 == 0) | (z1 == 0xFFFFFFFFFFFFFUL);
        }

        /// <summary>
        /// Returns the normalized version of this instance by reducing it modulo P and
        /// bringing the elements to canonical representations.
        /// </summary>
        /// <remarks>
        /// Result's magnitude will be 1
        /// </remarks>
        /// <returns>Normalized result</returns>
        public UInt256_5x52 Normalize()
        {
#if DEBUG
            Verify();
#endif
            ulong t0 = b0, t1 = b1, t2 = b2, t3 = b3, t4 = b4;

            // Reduce t4 at the start so there will be at most a single carry from the first pass
            ulong m;
            ulong x = t4 >> 48; t4 &= 0x0FFFFFFFFFFFFUL;

            // The first pass ensures the magnitude is 1, ...
            t0 += x * 0x1000003D1UL;
            t1 += (t0 >> 52); t0 &= 0xFFFFFFFFFFFFFUL;
            t2 += (t1 >> 52); t1 &= 0xFFFFFFFFFFFFFUL; m = t1;
            t3 += (t2 >> 52); t2 &= 0xFFFFFFFFFFFFFUL; m &= t2;
            t4 += (t3 >> 52); t3 &= 0xFFFFFFFFFFFFFUL; m &= t3;

            // ... except for a possible carry at bit 48 of t4 (i.e. bit 256 of the field element)
            Debug.Assert(t4 >> 49 == 0);

            // At most a single final reduction is needed; check if the value is >= the field characteristic
            x = (t4 >> 48) | ((t4 == 0x0FFFFFFFFFFFFUL ? 1UL : 0UL) & (m == 0xFFFFFFFFFFFFFUL ? 1UL : 0UL)
                & (t0 >= 0xFFFFEFFFFFC2FUL ? 1UL : 0UL));

            // Apply the final reduction (for constant-time behaviour, we do it always)
            t0 += x * 0x1000003D1UL;
            t1 += (t0 >> 52); t0 &= 0xFFFFFFFFFFFFFUL;
            t2 += (t1 >> 52); t1 &= 0xFFFFFFFFFFFFFUL;
            t3 += (t2 >> 52); t2 &= 0xFFFFFFFFFFFFFUL;
            t4 += (t3 >> 52); t3 &= 0xFFFFFFFFFFFFFUL;

            // If t4 didn't carry to bit 48 already, then it should have after any final reduction
            Debug.Assert(t4 >> 48 == x);

            // Mask off the possible multiple of 2^256 from the final reduction
            t4 &= 0x0FFFFFFFFFFFFUL;

            return new UInt256_5x52(t0, t1, t2, t3, t4
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
        public UInt256_5x52 NormalizeWeak()
        {
#if DEBUG
            Verify();
#endif
            ulong t0 = b0, t1 = b1, t2 = b2, t3 = b3, t4 = b4;

            // Reduce t4 at the start so there will be at most a single carry from the first pass
            ulong x = t4 >> 48; t4 &= 0x0FFFFFFFFFFFFUL;

            // The first pass ensures the magnitude is 1, ...
            t0 += x * 0x1000003D1UL;
            t1 += (t0 >> 52); t0 &= 0xFFFFFFFFFFFFFUL;
            t2 += (t1 >> 52); t1 &= 0xFFFFFFFFFFFFFUL;
            t3 += (t2 >> 52); t2 &= 0xFFFFFFFFFFFFFUL;
            t4 += (t3 >> 52); t3 &= 0xFFFFFFFFFFFFFUL;

            // ... except for a possible carry at bit 48 of t4 (i.e. bit 256 of the field element)
            Debug.Assert(t4 >> 49 == 0);

            return new UInt256_5x52(t0, t1, t2, t3, t4
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
        public UInt256_5x52 NormalizeVar()
        {
#if DEBUG
            Verify();
#endif
            ulong t0 = b0, t1 = b1, t2 = b2, t3 = b3, t4 = b4;

            // Reduce t4 at the start so there will be at most a single carry from the first pass
            ulong m;
            ulong x = t4 >> 48; t4 &= 0x0FFFFFFFFFFFFUL;

            // The first pass ensures the magnitude is 1, ...
            t0 += x * 0x1000003D1UL;
            t1 += (t0 >> 52); t0 &= 0xFFFFFFFFFFFFFUL;
            t2 += (t1 >> 52); t1 &= 0xFFFFFFFFFFFFFUL; m = t1;
            t3 += (t2 >> 52); t2 &= 0xFFFFFFFFFFFFFUL; m &= t2;
            t4 += (t3 >> 52); t3 &= 0xFFFFFFFFFFFFFUL; m &= t3;

            // ... except for a possible carry at bit 48 of t4 (i.e. bit 256 of the field element)
            Debug.Assert(t4 >> 49 == 0);

            // At most a single final reduction is needed; check if the value is >= the field characteristic
            x = (t4 >> 48) | ((t4 == 0x0FFFFFFFFFFFFUL ? 1UL : 0UL) & (m == 0xFFFFFFFFFFFFFUL ? 1UL : 0UL)
                & (t0 >= 0xFFFFEFFFFFC2FUL ? 1UL : 0UL));

            if (x != 0)
            {
                t0 += 0x1000003D1UL;
                t1 += (t0 >> 52); t0 &= 0xFFFFFFFFFFFFFUL;
                t2 += (t1 >> 52); t1 &= 0xFFFFFFFFFFFFFUL;
                t3 += (t2 >> 52); t2 &= 0xFFFFFFFFFFFFFUL;
                t4 += (t3 >> 52); t3 &= 0xFFFFFFFFFFFFFUL;

                // If t4 didn't carry to bit 48 already, then it should have after any final reduction
                Debug.Assert(t4 >> 48 == x);

                // Mask off the possible multiple of 2^256 from the final reduction
                t4 &= 0x0FFFFFFFFFFFFUL;
            }

            return new UInt256_5x52(t0, t1, t2, t3, t4
#if DEBUG
                , 1, true
#endif
                );
        }


        /// <summary>
        /// Return a field element with magnitude m, normalized if (and only if) m==0.
        /// The value is chosen so that it is likely to trigger edge cases related to
        /// internal overflows.
        /// </summary>
        public static UInt256_5x52 GetBounds(int m)
        {
            Debug.Assert(m >= 0);
            Debug.Assert(m <= 32);

            UInt256_5x52 r = new UInt256_5x52(
                0xFFFFFFFFFFFFFUL * 2UL * (ulong)m,
                0xFFFFFFFFFFFFFUL * 2UL * (ulong)m,
                0xFFFFFFFFFFFFFUL * 2UL * (ulong)m,
                0xFFFFFFFFFFFFFUL * 2UL * (ulong)m,
                0x0FFFFFFFFFFFFUL * 2UL * (ulong)m
#if DEBUG
                , m, m == 0
#endif
                );
#if DEBUG
            r.Verify();
#endif
            return r;
        }


        /// <summary>
        /// Adds the given value to this instance
        /// </summary>
        /// <remarks>
        /// Result's magnitude is the sum of the two magnitudes
        /// </remarks>
        /// <param name="u">The value to add</param>
        /// <returns>Result of the addition</returns>
        public UInt256_5x52 Add(uint u) => this + u;

        /// <summary>
        /// Adds two <see cref="UInt256_5x52"/> values
        /// </summary>
        /// <remarks>
        /// Result's magnitude is the sum of the two magnitudes
        /// </remarks>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <returns>Result of the addition</returns>
        public static UInt256_5x52 operator +(in UInt256_5x52 a, uint b)
        {
#if DEBUG
            Debug.Assert(0 <= b && b <= 0x7FFF);
            a.Verify();
#endif
            return new UInt256_5x52(
                a.b0 + b,
                a.b1,
                a.b2,
                a.b3,
                a.b4
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
        public UInt256_5x52 Add(in UInt256_5x52 other) => this + other;

        /// <summary>
        /// Adds two <see cref="UInt256_5x52"/> values
        /// </summary>
        /// <remarks>
        /// Result's magnitude is the sum of the two magnitudes
        /// </remarks>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <returns>Result of the addition</returns>
        public static UInt256_5x52 operator +(in UInt256_5x52 a, in UInt256_5x52 b)
        {
#if DEBUG
            a.Verify();
            b.Verify();
            Debug.Assert(a.magnitude + b.magnitude <= 32);
#endif
            return new UInt256_5x52(
                a.b0 + b.b0,
                a.b1 + b.b1,
                a.b2 + b.b2,
                a.b3 + b.b3,
                a.b4 + b.b4
#if DEBUG
                , a.magnitude + b.magnitude,
                false
#endif
                );
        }

        /// <summary>
        /// Adds three <see cref="UInt256_5x52"/> values
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <param name="c">Third value</param>
        /// <returns>Result</returns>
        public static UInt256_5x52 Add(in UInt256_5x52 a, in UInt256_5x52 b, in UInt256_5x52 c)
        {
#if DEBUG
            a.Verify();
            b.Verify();
            c.Verify();
            Debug.Assert(a.magnitude + b.magnitude + c.magnitude <= 32);
#endif
            return new UInt256_5x52(
                a.b0 + b.b0 + c.b0,
                a.b1 + b.b1 + c.b1,
                a.b2 + b.b2 + c.b2,
                a.b3 + b.b3 + c.b3,
                a.b4 + b.b4 + c.b4
#if DEBUG
                , a.magnitude + b.magnitude + c.magnitude,
                false
#endif
                );
        }

        /// <summary>
        /// Adds four <see cref="UInt256_5x52"/> values
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <param name="c">Third value</param>
        /// <param name="d">Fourth value</param>
        /// <returns>Result</returns>
        public static UInt256_5x52 Add(in UInt256_5x52 a, in UInt256_5x52 b, in UInt256_5x52 c, in UInt256_5x52 d)
        {
#if DEBUG
            a.Verify();
            b.Verify();
            c.Verify();
            d.Verify();
            Debug.Assert(a.magnitude + b.magnitude + c.magnitude + d.magnitude <= 32);
#endif
            return new UInt256_5x52(
                a.b0 + b.b0 + c.b0 + d.b0,
                a.b1 + b.b1 + c.b1 + d.b1,
                a.b2 + b.b2 + c.b2 + d.b2,
                a.b3 + b.b3 + c.b3 + d.b3,
                a.b4 + b.b4 + c.b4 + d.b4
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
        /// <returns>Result</returns>
        public UInt256_5x52 Half()
        {
#if DEBUG
            Verify();
            VerifyMagnitude(magnitude, 31);
#endif
            ulong t0 = b0, t1 = b1, t2 = b2, t3 = b3, t4 = b4;
            const ulong one = 1UL;
            ulong mask = (ulong)-(long)(t0 & one) >> 12;

            // Bounds analysis (over the rationals).
            //
            // Let m = r->magnitude
            //     C = 0xFFFFFFFFFFFFFULL * 2
            //     D = 0x0FFFFFFFFFFFFULL * 2
            //
            // Initial bounds: t0..t3 <= C * m
            //                     t4 <= D * m

            t0 += 0xFFFFEFFFFFC2FUL & mask;
            t1 += mask;
            t2 += mask;
            t3 += mask;
            t4 += mask >> 4;

            Debug.Assert((t0 & one) == 0);

            // t0..t3: added <= C/2
            //     t4: added <= D/2
            //
            // Current bounds: t0..t3 <= C * (m + 1/2)
            //                     t4 <= D * (m + 1/2)

            t0 = (t0 >> 1) + ((t1 & one) << 51);
            t1 = (t1 >> 1) + ((t2 & one) << 51);
            t2 = (t2 >> 1) + ((t3 & one) << 51);
            t3 = (t3 >> 1) + ((t4 & one) << 51);
            t4 = (t4 >> 1);

            // t0..t3: shifted right and added <= C/4 + 1/2
            //     t4: shifted right
            //
            // Current bounds: t0..t3 <= C * (m/2 + 1/2)
            //                     t4 <= D * (m/2 + 1/4)
            //
            // Therefore the output magnitude (M) has to be set such that:
            //     t0..t3: C * M >= C * (m/2 + 1/2)
            //         t4: D * M >= D * (m/2 + 1/4)
            //
            // It suffices for all limbs that, for any input magnitude m:
            //     M >= m/2 + 1/2
            //
            // and since we want the smallest such integer value for M:
            //     M == floor(m/2) + 1

            return new UInt256_5x52(t0, t1, t2, t3, t4
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
        public UInt256_5x52 Negate(int m)
        {
#if DEBUG
            Verify();
            Debug.Assert(m >= 0 && m <= 31);
            VerifyMagnitude(magnitude, m);

            // For all legal values of m (0..31), the following properties hold:
            Debug.Assert(0xFFFFEFFFFFC2FUL * 2 * (ulong)(m + 1) >= 0xFFFFFFFFFFFFFUL * 2 * (ulong)m);
            Debug.Assert(0xFFFFFFFFFFFFFUL * 2 * (ulong)(m + 1) >= 0xFFFFFFFFFFFFFUL * 2 * (ulong)m);
            Debug.Assert(0x0FFFFFFFFFFFFUL * 2 * (ulong)(m + 1) >= 0x0FFFFFFFFFFFFUL * 2 * (ulong)m);
#endif
            // Due to the properties above, the left hand in the subtractions below is never less than the right hand.
            return new UInt256_5x52(
                0xFFFFEFFFFFC2FUL * 2 * (ulong)(m + 1) - b0,
                0xFFFFFFFFFFFFFUL * 2 * (ulong)(m + 1) - b1,
                0xFFFFFFFFFFFFFUL * 2 * (ulong)(m + 1) - b2,
                0xFFFFFFFFFFFFFUL * 2 * (ulong)(m + 1) - b3,
                0x0FFFFFFFFFFFFUL * 2 * (ulong)(m + 1) - b4
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
        public UInt256_5x52 Multiply(uint a) => this * a;

        /// <summary>
        /// Multiplies the <see cref="UInt256_5x52"/> with the given unsigned 32-bit integer.
        /// </summary>
        /// <remarks>
        /// Result's magnitude is <see cref="UInt256_5x52"/>'s magnitude multiplied by <paramref name="b"/>.
        /// </remarks>
        /// <param name="a">Multiplicand</param>
        /// <param name="b">Multiplier in [0,32]</param>
        /// <returns>Result (is not normalized)</returns>
        public static UInt256_5x52 operator *(in UInt256_5x52 a, uint b)
        {
#if DEBUG
            a.Verify();
            Debug.Assert(b >= 0 && b <= 32);
            Debug.Assert(b * a.magnitude <= 32);
#endif
            return new UInt256_5x52(
                a.b0 * b,
                a.b1 * b,
                a.b2 * b,
                a.b3 * b,
                a.b4 * b
#if DEBUG
                , a.magnitude * (int)b,
                false
#endif
                );
        }

        /// <summary>
        /// Multiplies this instance with the other <see cref="UInt256_5x52"/> value.
        /// </summary>
        /// <remarks>
        /// Magnitude of each value must be below 8.
        /// Result's magnitude is 1 but is not normalized.
        /// </remarks>
        /// <param name="other">Other value</param>
        /// <returns>Multiplication result</returns>
        public UInt256_5x52 Multiply(in UInt256_5x52 other) => this * other;

        /// <summary>
        /// Multiplies the two <see cref="UInt256_5x52"/> values.
        /// </summary>
        /// <remarks>
        /// Magnitude of each value must be below 8.
        /// Result's magnitude is 1 but is not normalized.
        /// </remarks>
        /// <param name="a">First</param>
        /// <param name="b">Second</param>
        /// <returns>Multiplication result</returns>
        public static UInt256_5x52 operator *(in UInt256_5x52 a, in UInt256_5x52 b)
        {
#if DEBUG
            a.Verify();
            b.Verify();
            VerifyMagnitude(a.magnitude, 8);
            VerifyMagnitude(b.magnitude, 8);
#endif
            UInt128 c, d;
            ulong t3, t4, tx, u0;
            ulong a0 = a.b0, a1 = a.b1, a2 = a.b2, a3 = a.b3, a4 = a.b4;
            const ulong M = 0xFFFFFFFFFFFFFUL, R = 0x1000003D10UL;

            Debug.Assert(a.b0 >> 56 == 0);
            Debug.Assert(a.b1 >> 56 == 0);
            Debug.Assert(a.b2 >> 56 == 0);
            Debug.Assert(a.b3 >> 56 == 0);
            Debug.Assert(a.b4 >> 52 == 0);

            Debug.Assert(b.b0 >> 56 == 0);
            Debug.Assert(b.b1 >> 56 == 0);
            Debug.Assert(b.b2 >> 56 == 0);
            Debug.Assert(b.b3 >> 56 == 0);
            Debug.Assert(b.b4 >> 52 == 0);

            // [... a b c] is a shorthand for ... + a<<104 + b<<52 + c<<0 mod n.
            // for 0 <= x <= 4, px is a shorthand for sum(a[i]*b[x-i], i=0..x).
            // for 4 <= x <= 8, px is a shorthand for sum(a[i]*b[x-i], i=(x-4)..4)
            // Note that [x 0 0 0 0 0] = [x*R].

            d = (UInt128)a0 * b.b3 +
                (UInt128)a1 * b.b2 +
                (UInt128)a2 * b.b1 +
                (UInt128)a3 * b.b0;
            Debug.Assert(d >> 114 == 0);
            // [d 0 0 0] = [p3 0 0 0]
            c = (UInt128)a4 * b.b4;
            Debug.Assert(c >> 112 == 0);
            // [c 0 0 0 0 d 0 0 0] = [p8 0 0 0 0 p3 0 0 0]
            d += (UInt128)R * (ulong)c;
            c >>= 64;
            Debug.Assert(d >> 115 == 0);
            Debug.Assert(c >> 48 == 0);
            // [(c<<12) 0 0 0 0 0 d 0 0 0] = [p8 0 0 0 0 p3 0 0 0]
            t3 = (ulong)d & M; d >>= 52;
            Debug.Assert(t3 >> 52 == 0);
            Debug.Assert(d >> 63 == 0);
            // [(c<<12) 0 0 0 0 d t3 0 0 0] = [p8 0 0 0 0 p3 0 0 0]

            d += (UInt128)a0 * b.b4 +
                 (UInt128)a1 * b.b3 +
                 (UInt128)a2 * b.b2 +
                 (UInt128)a3 * b.b1 +
                 (UInt128)a4 * b.b0;
            Debug.Assert(d >> 115 == 0);
            // [(c<<12) 0 0 0 0 d t3 0 0 0] = [p8 0 0 0 p4 p3 0 0 0]
            d += (UInt128)(R << 12) * (ulong)c;
            Debug.Assert(d >> 116 == 0);
            // [d t3 0 0 0] = [p8 0 0 0 p4 p3 0 0 0]
            t4 = (ulong)d & M; d >>= 52;
            Debug.Assert(t4 >> 52 == 0);
            Debug.Assert(d >> 64 == 0);
            // [d t4 t3 0 0 0] = [p8 0 0 0 p4 p3 0 0 0]
            tx = (t4 >> 48); t4 &= (M >> 4);
            Debug.Assert(tx >> 4 == 0);
            Debug.Assert(t4 >> 48 == 0);
            // [d t4+(tx<<48) t3 0 0 0] = [p8 0 0 0 p4 p3 0 0 0]

            c = (UInt128)a0 * b.b0;
            Debug.Assert(c >> 112 == 0);
            // [d t4+(tx<<48) t3 0 0 c] = [p8 0 0 0 p4 p3 0 0 p0]
            d += (UInt128)a1 * b.b4 +
                 (UInt128)a2 * b.b3 +
                 (UInt128)a3 * b.b2 +
                 (UInt128)a4 * b.b1;
            Debug.Assert(d >> 114 == 0);
            // [d t4+(tx<<48) t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
            u0 = (ulong)d & M; d >>= 52;
            Debug.Assert(u0 >> 52 == 0);
            Debug.Assert(d >> 62 == 0);
            // [d u0 t4+(tx<<48) t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
            // [d 0 t4+(tx<<48)+(u0<<52) t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
            u0 = (u0 << 4) | tx;
            Debug.Assert(u0 >> 56 == 0);
            // [d 0 t4+(u0<<48) t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
            c += (UInt128)u0 * R >> 4;
            Debug.Assert(c >> 113 == 0);
            // [d 0 t4 t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
            ulong r0 = (ulong)c & M; c >>= 52;
            Debug.Assert(r0 >> 52 == 0);
            Debug.Assert(c >> 61 == 0);
            // [d 0 t4 t3 0 c r0] = [p8 0 0 p5 p4 p3 0 0 p0]
            c += (UInt128)a0 * b.b1 +
                 (UInt128)a1 * b.b0;
            Debug.Assert(c >> 114 == 0);
            // [d 0 t4 t3 0 c r0] = [p8 0 0 p5 p4 p3 0 p1 p0]
            d += (UInt128)a2 * b.b4 +
                 (UInt128)a3 * b.b3 +
                 (UInt128)a4 * b.b2;
            Debug.Assert(d >> 114 == 0);
            // [d 0 t4 t3 0 c r0] = [p8 0 p6 p5 p4 p3 0 p1 p0]
            c += ((ulong)d & M) * (UInt128)R; d >>= 52;
            Debug.Assert(c >> 115 == 0);
            Debug.Assert(d >> 62 == 0);
            // [d 0 0 t4 t3 0 c r0] = [p8 0 p6 p5 p4 p3 0 p1 p0]
            ulong r1 = (ulong)c & M; c >>= 52;
            Debug.Assert(r1 >> 52 == 0);
            Debug.Assert(c >> 63 == 0);
            // [d 0 0 t4 t3 c r1 r0] = [p8 0 p6 p5 p4 p3 0 p1 p0]

            c += (UInt128)a0 * b.b2 +
                 (UInt128)a1 * b.b1 +
                 (UInt128)a2 * b.b0;
            Debug.Assert(c >> 114 == 0);
            // [d 0 0 t4 t3 c r1 r0] = [p8 0 p6 p5 p4 p3 p2 p1 p0]
            d += (UInt128)a3 * b.b4 +
                 (UInt128)a4 * b.b3;
            Debug.Assert(d >> 114 == 0);
            // [d 0 0 t4 t3 c t1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]
            c += (UInt128)R * (ulong)d; d >>= 64;
            Debug.Assert(c >> 115 == 0);
            Debug.Assert(d >> 50 == 0);
            // [(d<<12) 0 0 0 t4 t3 c r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]

            ulong r2 = (ulong)c & M; c >>= 52;
            Debug.Assert(r2 >> 52 == 0);
            Debug.Assert(c >> 63 == 0);
            // [(d<<12) 0 0 0 t4 t3+c r2 r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]
            c += (UInt128)(R << 12) * (ulong)d + t3;
            Debug.Assert(c >> 100 == 0);
            // [t4 c r2 r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]
            ulong r3 = (ulong)c & M; c >>= 52;
            Debug.Assert(r3 >> 52 == 0);
            Debug.Assert(c >> 48 == 0);
            // [t4+c r3 r2 r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]
            ulong r4 = (ulong)c + t4;
            Debug.Assert(r4 >> 49 == 0);
            // [r4 r3 r2 r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]

            return new UInt256_5x52(r0, r1, r2, r3, r4
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
            UInt256_5x52 tmp = NormalizeVar();
            // secp256k1_jacobi64_maybe_var cannot deal with input 0.
            if (tmp.IsZero)
            {
                ret = true;
            }

            ModInv64Signed62 s = new ModInv64Signed62(tmp);
            int jac = ModInv64.Jacobi64MaybeVar(s, ModInv64ModInfo.FeConstant);
            if (jac == 0)
            {
                // secp256k1_jacobi64_maybe_var failed to compute the Jacobi symbol. Fall back
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
        public UInt256_5x52 Sqr() => Sqr(1);

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
        public UInt256_5x52 Sqr(int times)
        {
#if DEBUG
            Verify();
            VerifyMagnitude(magnitude, 8);
#endif
            ulong t3, t4, tx, u0;
            const ulong M = 0xFFFFFFFFFFFFFUL, R = 0x1000003D10UL;
            ulong r0 = b0, r1 = b1, r2 = b2, r3 = b3, r4 = b4;

            for (int i = 0; i < times; i++)
            {
                Debug.Assert(r0 >> 56 == 0);
                Debug.Assert(r1 >> 56 == 0);
                Debug.Assert(r2 >> 56 == 0);
                Debug.Assert(r3 >> 56 == 0);
                Debug.Assert(r4 >> 52 == 0);

                // [... a b c] is a shorthand for ... + a<<104 + b<<52 + c<<0 mod n.
                // px is a shorthand for sum(a[i]*a[x-i], i=0..x).
                // Note that [x 0 0 0 0 0] = [x*R].

                UInt128 d = (UInt128)(r0 * 2) * r3 +
                            (UInt128)(r1 * 2) * r2;
                Debug.Assert(d >> 114 == 0);
                // [d 0 0 0] = [p3 0 0 0] */
                UInt128 c = (UInt128)r4 * r4;
                Debug.Assert(c >> 112 == 0);
                // [c 0 0 0 0 d 0 0 0] = [p8 0 0 0 0 p3 0 0 0]
                d += (UInt128)R * (ulong)c; c >>= 64;
                Debug.Assert(d >> 115 == 0);
                Debug.Assert(c >> 48 == 0);
                // [(c<<12) 0 0 0 0 0 d 0 0 0] = [p8 0 0 0 0 p3 0 0 0]
                t3 = (ulong)d & M; d >>= 52;
                Debug.Assert(t3 >> 52 == 0);
                Debug.Assert(d >> 63 == 0);
                // [(c<<12) 0 0 0 0 d t3 0 0 0] = [p8 0 0 0 0 p3 0 0 0]

                r4 *= 2;
                d += (UInt128)r0 * r4 +
                     (UInt128)(r1 * 2) * r3 +
                     (UInt128)r2 * r2;
                Debug.Assert(d >> 115 == 0);
                // [(c<<12) 0 0 0 0 d t3 0 0 0] = [p8 0 0 0 p4 p3 0 0 0]
                d += (UInt128)(R << 12) * (ulong)c;
                Debug.Assert(d >> 116 == 0);
                // [d t3 0 0 0] = [p8 0 0 0 p4 p3 0 0 0]
                t4 = (ulong)d & M; d >>= 52;
                Debug.Assert(t4 >> 52 == 0);
                Debug.Assert(d >> 64 == 0);
                // [d t4 t3 0 0 0] = [p8 0 0 0 p4 p3 0 0 0]
                tx = (t4 >> 48); t4 &= (M >> 4);
                Debug.Assert(tx >> 4 == 0);
                Debug.Assert(t4 >> 48 == 0);
                // [d t4+(tx<<48) t3 0 0 0] = [p8 0 0 0 p4 p3 0 0 0]

                c = (UInt128)r0 * r0;
                Debug.Assert(c >> 112 == 0);
                // [d t4+(tx<<48) t3 0 0 c] = [p8 0 0 0 p4 p3 0 0 p0]
                d += (UInt128)r1 * r4 +
                     (UInt128)(r2 * 2) * r3;
                Debug.Assert(d >> 114 == 0);
                // [d t4+(tx<<48) t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
                u0 = (ulong)d & M; d >>= 52;
                Debug.Assert(u0 >> 52 == 0);
                Debug.Assert(d >> 62 == 0);
                // [d u0 t4+(tx<<48) t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
                // [d 0 t4+(tx<<48)+(u0<<52) t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
                u0 = (u0 << 4) | tx;
                Debug.Assert(u0 >> 56 == 0);
                // [d 0 t4+(u0<<48) t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
                c += (UInt128)u0 * (R >> 4);
                Debug.Assert(c >> 113 == 0);
                // [d 0 t4 t3 0 0 c] = [p8 0 0 p5 p4 p3 0 0 p0]
                ulong a0 = r0;
                r0 = (ulong)c & M; c >>= 52;
                Debug.Assert(r0 >> 52 == 0);
                Debug.Assert(c >> 61 == 0);
                // [d 0 t4 t3 0 c r0] = [p8 0 0 p5 p4 p3 0 0 p0]

                a0 *= 2;
                c += (UInt128)a0 * r1;
                Debug.Assert(c >> 114 == 0);
                // [d 0 t4 t3 0 c r0] = [p8 0 0 p5 p4 p3 0 p1 p0]
                d += (UInt128)r2 * r4 +
                     (UInt128)r3 * r3;
                Debug.Assert(d >> 114 == 0);
                // [d 0 t4 t3 0 c r0] = [p8 0 p6 p5 p4 p3 0 p1 p0]
                c += (UInt128)((ulong)d & M) * R; d >>= 52;
                Debug.Assert(c >> 115 == 0);
                Debug.Assert(d >> 62 == 0);
                // [d 0 0 t4 t3 0 c r0] = [p8 0 p6 p5 p4 p3 0 p1 p0]
                ulong a1 = r1;
                r1 = (ulong)c & M; c >>= 52;
                Debug.Assert(r1 >> 52 == 0);
                Debug.Assert(c >> 63 == 0);
                // [d 0 0 t4 t3 c r1 r0] = [p8 0 p6 p5 p4 p3 0 p1 p0]

                c += (UInt128)a0 * r2 +
                     (UInt128)a1 * a1;
                Debug.Assert(c >> 114 == 0);
                // [d 0 0 t4 t3 c r1 r0] = [p8 0 p6 p5 p4 p3 p2 p1 p0]
                d += (UInt128)r3 * r4;
                Debug.Assert(d >> 114 == 0);
                // [d 0 0 t4 t3 c r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]
                c += (UInt128)R * (ulong)d; d >>= 64;
                Debug.Assert(c >> 115 == 0);
                Debug.Assert(d >> 50 == 0);
                // [(d<<12) 0 0 0 t4 t3 c r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]
                r2 = (ulong)c & M; c >>= 52;
                Debug.Assert(r2 >> 52 == 0);
                Debug.Assert(c >> 63 == 0);
                // [(d<<12) 0 0 0 t4 t3+c r2 r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]

                c += (UInt128)(R << 12) * (ulong)d;
                c += t3;
                Debug.Assert(c >> 100 == 0);
                // [t4 c r2 r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]
                r3 = (ulong)c & M; c >>= 52;
                Debug.Assert(r3 >> 52 == 0);
                Debug.Assert(c >> 48 == 0);
                // [t4+c r3 r2 r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0]
                r4 = (ulong)c + t4;
                Debug.Assert(r4 >> 49 == 0);
                // [r4 r3 r2 r1 r0] = [p8 p7 p6 p5 p4 p3 p2 p1 p0] 
            }

            return new UInt256_5x52(r0, r1, r2, r3, r4
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
        public bool Sqrt(out UInt256_5x52 result)
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

            UInt256_5x52 x2, x3, x6, x9, x11, x22, x44, x88, x176, x220, x223, t1;
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
        public UInt256_5x52 Inverse()
        {
#if DEBUG
            bool input_is_zero = IsZeroNormalized();
            Verify();
#endif
            UInt256_5x52 tmp = Normalize();
            ModInv64Signed62 s = new ModInv64Signed62(tmp);
            ModInv64.Compute(ref s, ModInv64ModInfo.FeConstant);
            UInt256_5x52 r = s.ToUInt256_5x52();
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
        public UInt256_5x52 InverseVar()
        {
#if DEBUG
            bool input_is_zero = IsZeroNormalized();
            Verify();
#endif

            UInt256_5x52 tmp = NormalizeVar();
            ModInv64Signed62 s = new ModInv64Signed62(tmp);
            ModInv64.ComputeVar(ref s, ModInv64ModInfo.FeConstant);
            UInt256_5x52 r = s.ToUInt256_5x52();

#if DEBUG
            Debug.Assert(r.IsZeroNormalized() == input_is_zero);
            r.Verify();
#endif
            return r;
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
        public static UInt256_5x52 CMov(in UInt256_5x52 r, in UInt256_5x52 a, uint flag)
        {
#if DEBUG
            r.Verify();
            a.Verify();
            Debug.Assert(flag == 0 || flag == 1);
#endif
            ulong mask0 = flag + ~0UL;
            ulong mask1 = ~mask0;
            return new UInt256_5x52(
                (r.b0 & mask0) | (a.b0 & mask1),
                (r.b1 & mask0) | (a.b1 & mask1),
                (r.b2 & mask0) | (a.b2 & mask1),
                (r.b3 & mask0) | (a.b3 & mask1),
                (r.b4 & mask0) | (a.b4 & mask1)
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
        public UInt256_4x64 ToUInt256_4x64()
        {
#if DEBUG
            Verify();
            Debug.Assert(isNormalized);
#endif
            return new UInt256_4x64(this);
        }

        /// <summary>
        /// Converts this instance to a 32-byte array in big-endian order.
        /// </summary>
        /// <returns>Big-endian byte array</returns>
        public Span<byte> ToSpan()
        {
            Span<byte> res = new byte[32];
            WriteToSpan(res);
            return res;
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
            // Note: Last item is 48 bits, the rest are 52 bits
            // Read comments from bottom to make sense, array is set in reverse for optimization
            ba[31] = (byte)b0; // 8(0)
            ba[30] = (byte)(b0 >> 8); // 8(8)
            ba[29] = (byte)(b0 >> 16); // 8(16)
            ba[28] = (byte)(b0 >> 24); // 8(24)
            ba[27] = (byte)(b0 >> 32); // 8(32)
            ba[26] = (byte)(b0 >> 40); // 8(40)
            ba[25] = (byte)((b1 << 4) | (b0 >> 48)); // 4(0)+4(48)
            ba[24] = (byte)(b1 >> 4); // 8(4)
            ba[23] = (byte)(b1 >> 12); // 8(12)
            ba[22] = (byte)(b1 >> 20); // 8(20)
            ba[21] = (byte)(b1 >> 28); // 8(28)
            ba[20] = (byte)(b1 >> 36); // 8(36)
            ba[19] = (byte)(b1 >> 44); // 8(44)
            ba[18] = (byte)b2; // 8(0)
            ba[17] = (byte)(b2 >> 8); // 8(16-8=8)
            ba[16] = (byte)(b2 >> 16); // 8(24-8=16)
            ba[15] = (byte)(b2 >> 24); // 8(32-8=24)
            ba[14] = (byte)(b2 >> 32); // 8(40-8=32)
            ba[13] = (byte)(b2 >> 40); // 8(48-8=40)
            ba[12] = (byte)((b3 << 4) | (b2 >> 48)); // 4(0)+4(52-4=48)
            ba[11] = (byte)(b3 >> 4); // 8(12-8=4)
            ba[10] = (byte)(b3 >> 12); // 8(20-8=12)
            ba[9] = (byte)(b3 >> 20); // 8(28-8=20)
            ba[8] = (byte)(b3 >> 28); // 8(36-8=28)
            ba[7] = (byte)(b3 >> 36); // 8(44-8=36)
            ba[6] = (byte)(b3 >> 44); // 8(52-8=44)
            ba[5] = (byte)b4; // 8(8-8=0)
            ba[4] = (byte)(b4 >> 8); // 8(16-8)
            ba[3] = (byte)(b4 >> 16); // 8(24-8=16)
            ba[2] = (byte)(b4 >> 24); // 8(32-8=24)
            ba[1] = (byte)(b4 >> 32); // 8(40-8=32)
            ba[0] = (byte)(b4 >> 40); // Take 8 bits (rem=48-8=40)
        }

        /// <summary>
        /// Compare 2 <see cref="UInt256_5x52"/> values.
        /// Assumes both values are normalized.
        /// </summary>
        /// <remarks>
        /// This method is not constant time.
        /// </remarks>
        /// <param name="b">Other value to compare to</param>
        /// <returns>1 if this is bigger than <paramref name="b"/>, -1 if smaller and 0 if equal.</returns>
        public int CompareToVar(in UInt256_5x52 b)
        {
#if DEBUG
            Debug.Assert(isNormalized);
            Debug.Assert(b.isNormalized);
            Verify();
            b.Verify();
#endif
            if (b4 > b.b4) return 1; else if (b4 < b.b4) return -1;
            if (b3 > b.b3) return 1; else if (b3 < b.b3) return -1;
            if (b2 > b.b2) return 1; else if (b2 < b.b2) return -1;
            if (b1 > b.b1) return 1; else if (b1 < b.b1) return -1;
            if (b0 > b.b0) return 1; else if (b0 < b.b0) return -1;

            return 0;
        }


        /// <summary>
        /// Returns if the given <see cref="UInt256_5x52"/> is equal to this instance.
        /// </summary>
        /// <remarks>
        /// This method is constant time.
        /// Magnitude of this instance should be at most 1.
        /// Magnitude of b should be at most 30.
        /// </remarks>
        /// <param name="b">Other <see cref="UInt256_5x52"/> to compare to (magnitude of at most 31)</param>
        /// <returns>True if the two instances are equal; otherwise false.</returns>
        public bool Equals(in UInt256_5x52 b)
        {
#if DEBUG
            Verify();
            b.Verify();
            VerifyMagnitude(magnitude, 1);
            VerifyMagnitude(b.magnitude, 30);
#endif
            UInt256_5x52 na = Negate(1);
            na += b;
            return na.IsZeroNormalized();
        }
    }
}
