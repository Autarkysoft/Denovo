// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    // TODO: try replacing ModInv32Signed30 with a stackalloc uint[] and just pass the pointer to all methods
    /// <summary>
    /// Implementation of fast constant-time modular inversion using 30-bit signed limbs.
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin-core/secp256k1/blob/1a81df826e2a24a1656fc28fc3076b62562216d9/src/modinv32_impl.h
    /// </remarks>
    public static class ModInv32
    {
#if DEBUG
        // Compute a*factor and return it. All but the top limb in result will be in range [0,2^30).
        private static ModInv32Signed30 Mul30(in ModInv32Signed30 a, int alen, int factor)
        {
            const int M30 = (int)(uint.MaxValue >> 2);
            long c = 0;
            int[] av = a.GetArray();
            int[] rv = new int[9];
            for (int i = 0; i < 8; i++)
            {
                if (i < alen)
                {
                    c += (long)av[i] * factor;
                }
                rv[i] = (int)c & M30;
                c >>= 30;
            }

            if (8 < alen)
            {
                c += (long)av[8] * factor;
            }

            Debug.Assert(c == (int)c);

            rv[8] = (int)c;
            return new ModInv32Signed30(rv);
        }

        // Return -1 for a<b*factor, 0 for a==b*factor, 1 for a>b*factor. A consists of alen limbs; b has 9.
        private static int MulCmp30(in ModInv32Signed30 a, int alen, in ModInv32Signed30 b, int factor)
        {
            ModInv32Signed30 am = Mul30(a, alen, 1); // Normalize all but the top limb of a.
            ModInv32Signed30 bm = Mul30(b, 9, factor);
            int[] amv = am.GetArray();
            int[] bmv = bm.GetArray();
            for (int i = 0; i < 8; i++)
            {
                // Verify that all but the top limb of a and b are normalized.
                Debug.Assert(amv[i] >> 30 == 0);
                Debug.Assert(bmv[i] >> 30 == 0);
            }
            for (int i = 8; i >= 0; i--)
            {
                if (amv[i] < bmv[i]) return -1;
                if (amv[i] > bmv[i]) return 1;
            }
            return 0;
        }
#endif


        // Take as input a signed30 number in range (-2*modulus,modulus), and add a multiple of the modulus
        // to it to bring it to range [0,modulus). If sign < 0, the input will also be negated in the
        // process. The input must have limbs in range (-2^30,2^30). The output will have limbs in range
        // [0,2^30).
        private static ModInv32Signed30 Normalize30(in ModInv32Signed30 r, int sign, in ModInv32ModInfo modinfo)
        {
            const int M30 = (int)(uint.MaxValue >> 2);
            int r0 = r.v0, r1 = r.v1, r2 = r.v2, r3 = r.v3, r4 = r.v4, r5 = r.v5, r6 = r.v6, r7 = r.v7, r8 = r.v8;
            int cond_add, cond_negate;

#if DEBUG
            // Verify that all limbs are in range (-2^30,2^30).
            int[] rv = r.GetArray();
            for (int i = 0; i < 9; i++)
            {
                Debug.Assert(rv[i] >= -M30);
                Debug.Assert(rv[i] <= M30);
            }
            Debug.Assert(MulCmp30(r, 9, modinfo.modulus, -2) > 0); // r > -2*modulus
            Debug.Assert(MulCmp30(r, 9, modinfo.modulus, 1) < 0);  // r < modulus
#endif

            // In a first step, add the modulus if the input is negative, and then negate if requested.
            // This brings r from range (-2*modulus,modulus) to range (-modulus,modulus). As all input
            // limbs are in range (-2^30,2^30), this cannot overflow an int32_t. Note that the right
            // shifts below are signed sign-extending shifts (see assumptions.h for tests that that is
            // indeed the behavior of the right shift operator).
            cond_add = r8 >> 31;
            r0 += modinfo.modulus.v0 & cond_add;
            r1 += modinfo.modulus.v1 & cond_add;
            r2 += modinfo.modulus.v2 & cond_add;
            r3 += modinfo.modulus.v3 & cond_add;
            r4 += modinfo.modulus.v4 & cond_add;
            r5 += modinfo.modulus.v5 & cond_add;
            r6 += modinfo.modulus.v6 & cond_add;
            r7 += modinfo.modulus.v7 & cond_add;
            r8 += modinfo.modulus.v8 & cond_add;
            cond_negate = sign >> 31;
            r0 = (r0 ^ cond_negate) - cond_negate;
            r1 = (r1 ^ cond_negate) - cond_negate;
            r2 = (r2 ^ cond_negate) - cond_negate;
            r3 = (r3 ^ cond_negate) - cond_negate;
            r4 = (r4 ^ cond_negate) - cond_negate;
            r5 = (r5 ^ cond_negate) - cond_negate;
            r6 = (r6 ^ cond_negate) - cond_negate;
            r7 = (r7 ^ cond_negate) - cond_negate;
            r8 = (r8 ^ cond_negate) - cond_negate;
            // Propagate the top bits, to bring limbs back to range (-2^30,2^30).
            r1 += r0 >> 30; r0 &= M30;
            r2 += r1 >> 30; r1 &= M30;
            r3 += r2 >> 30; r2 &= M30;
            r4 += r3 >> 30; r3 &= M30;
            r5 += r4 >> 30; r4 &= M30;
            r6 += r5 >> 30; r5 &= M30;
            r7 += r6 >> 30; r6 &= M30;
            r8 += r7 >> 30; r7 &= M30;

            // In a second step add the modulus again if the result is still negative,
            // bringing r to range [0,modulus).
            cond_add = r8 >> 31;
            r0 += modinfo.modulus.v0 & cond_add;
            r1 += modinfo.modulus.v1 & cond_add;
            r2 += modinfo.modulus.v2 & cond_add;
            r3 += modinfo.modulus.v3 & cond_add;
            r4 += modinfo.modulus.v4 & cond_add;
            r5 += modinfo.modulus.v5 & cond_add;
            r6 += modinfo.modulus.v6 & cond_add;
            r7 += modinfo.modulus.v7 & cond_add;
            r8 += modinfo.modulus.v8 & cond_add;
            // And propagate again.
            r1 += r0 >> 30; r0 &= M30;
            r2 += r1 >> 30; r1 &= M30;
            r3 += r2 >> 30; r2 &= M30;
            r4 += r3 >> 30; r3 &= M30;
            r5 += r4 >> 30; r4 &= M30;
            r6 += r5 >> 30; r5 &= M30;
            r7 += r6 >> 30; r6 &= M30;
            r8 += r7 >> 30; r7 &= M30;

            ModInv32Signed30 result = new ModInv32Signed30(r0, r1, r2, r3, r4, r5, r6, r7, r8);
#if DEBUG
            Debug.Assert(r0 >> 30 == 0);
            Debug.Assert(r1 >> 30 == 0);
            Debug.Assert(r2 >> 30 == 0);
            Debug.Assert(r3 >> 30 == 0);
            Debug.Assert(r4 >> 30 == 0);
            Debug.Assert(r5 >> 30 == 0);
            Debug.Assert(r6 >> 30 == 0);
            Debug.Assert(r7 >> 30 == 0);
            Debug.Assert(r8 >> 30 == 0);
            Debug.Assert(MulCmp30(result, 9, modinfo.modulus, 0) >= 0); // r >= 0
            Debug.Assert(MulCmp30(result, 9, modinfo.modulus, 1) < 0); // r < modulus
#endif
            return result;
        }


        // Compute the transition matrix and zeta for 30 divsteps.
        // 
        // Input:  zeta: initial zeta
        //         f0:   bottom limb of initial f
        //         g0:   bottom limb of initial g
        // Output: t: transition matrix
        // Return: final zeta
        // 
        // Implements the divsteps_n_matrix function from the explanation.
        private static int DivSteps30(int zeta, uint f0, uint g0, out ModInv32Trans2x2 t)
        {
            // u,v,q,r are the elements of the transformation matrix being built up,
            // starting with the identity matrix. Semantically they are signed integers
            // in range [-2^30,2^30], but here represented as unsigned mod 2^32. This
            // permits left shifting (which is UB for negative numbers). The range
            // being inside [-2^31,2^31) means that casting to signed works correctly.
            uint u = 1, v = 0, q = 0, r = 1;
            uint mask1, mask2, f = f0, g = g0, x, y, z;

            for (int i = 0; i < 30; i++)
            {
                Debug.Assert((f & 1) == 1); // f must always be odd
                Debug.Assert((u * f0 + v * g0) == f << i);
                Debug.Assert((q * f0 + r * g0) == g << i);
                // Compute conditional masks for (zeta < 0) and for (g & 1).
                mask1 = (uint)(zeta >> 31);
                mask2 = (uint)-(g & 1);
                // Compute x,y,z, conditionally negated versions of f,u,v.
                x = (f ^ mask1) - mask1;
                y = (u ^ mask1) - mask1;
                z = (v ^ mask1) - mask1;
                // Conditionally add x,y,z to g,q,r.
                g += x & mask2;
                q += y & mask2;
                r += z & mask2;
                // In what follows, mask1 is a condition mask for (zeta < 0) and (g & 1).
                mask1 &= mask2;
                // Conditionally change zeta into -zeta-2 or zeta-1.
                zeta = (int)((zeta ^ mask1) - 1);
                // Conditionally add g,q,r to f,u,v.
                f += g & mask1;
                u += q & mask1;
                v += r & mask1;
                // Shifts
                g >>= 1;
                u <<= 1;
                v <<= 1;
                // Bounds on zeta that follow from the bounds on iteration count (max 20*30 divsteps).
                Debug.Assert(zeta >= -601 && zeta <= 601);
            }
            // Return data in t and return value.
            t = new ModInv32Trans2x2(u, v, q, r);

            // The determinant of t must be a power of two. This guarantees that multiplication with t
            // does not change the gcd of f and g, apart from adding a power-of-2 factor to it (which
            // will be divided out again). As each divstep's individual matrix has determinant 2, the
            // aggregate of 30 of them will have determinant 2^30.
            Debug.Assert((long)t.u * t.r - (long)t.v * t.q == 1L << 30);
            return zeta;
        }

        // secp256k1_modinv32_inv256[i] = -(2*i+1)^-1 (mod 256)
        private static readonly byte[] Inv256 = new byte[128]
        {
            0xFF, 0x55, 0x33, 0x49, 0xC7, 0x5D, 0x3B, 0x11, 0x0F, 0xE5, 0xC3, 0x59,
            0xD7, 0xED, 0xCB, 0x21, 0x1F, 0x75, 0x53, 0x69, 0xE7, 0x7D, 0x5B, 0x31,
            0x2F, 0x05, 0xE3, 0x79, 0xF7, 0x0D, 0xEB, 0x41, 0x3F, 0x95, 0x73, 0x89,
            0x07, 0x9D, 0x7B, 0x51, 0x4F, 0x25, 0x03, 0x99, 0x17, 0x2D, 0x0B, 0x61,
            0x5F, 0xB5, 0x93, 0xA9, 0x27, 0xBD, 0x9B, 0x71, 0x6F, 0x45, 0x23, 0xB9,
            0x37, 0x4D, 0x2B, 0x81, 0x7F, 0xD5, 0xB3, 0xC9, 0x47, 0xDD, 0xBB, 0x91,
            0x8F, 0x65, 0x43, 0xD9, 0x57, 0x6D, 0x4B, 0xA1, 0x9F, 0xF5, 0xD3, 0xE9,
            0x67, 0xFD, 0xDB, 0xB1, 0xAF, 0x85, 0x63, 0xF9, 0x77, 0x8D, 0x6B, 0xC1,
            0xBF, 0x15, 0xF3, 0x09, 0x87, 0x1D, 0xFB, 0xD1, 0xCF, 0xA5, 0x83, 0x19,
            0x97, 0xAD, 0x8B, 0xE1, 0xDF, 0x35, 0x13, 0x29, 0xA7, 0x3D, 0x1B, 0xF1,
            0xEF, 0xC5, 0xA3, 0x39, 0xB7, 0xCD, 0xAB, 0x01
        };


        //https://github.com/bitcoin-core/secp256k1/blob/1a81df826e2a24a1656fc28fc3076b62562216d9/src/util.h#L305
        private static readonly byte[] DeBruijn = new byte[32]
        {
            0x00, 0x01, 0x02, 0x18, 0x03, 0x13, 0x06, 0x19, 0x16, 0x04, 0x14, 0x0A,
            0x10, 0x07, 0x0C, 0x1A, 0x1F, 0x17, 0x12, 0x05, 0x15, 0x09, 0x0F, 0x0B,
            0x1E, 0x11, 0x08, 0x0E, 0x1D, 0x0D, 0x1C, 0x1B
        };
        // Determine the number of trailing zero bits in a (non-zero) 32-bit x.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Ctz32Var(uint x)
        {
            // TODO: dotnet 3.0+ have: BitOperations.TrailingZeroCount()
            Debug.Assert(x != 0);
            // Multi-cast below mitigates the redundant conv.u8
            return DeBruijn[((x & (uint)-(int)x) * 0x04D7651FU) >> 27];
        }


        // Compute the transition matrix and eta for 30 divsteps (variable time).
        // 
        // Input:  eta: initial eta
        //         f0:  bottom limb of initial f
        //         g0:  bottom limb of initial g
        // Output: t: transition matrix
        // Return: final eta
        // 
        // Implements the divsteps_n_matrix_var function from the explanation.
        private static int DivSteps30Var(int eta, uint f0, uint g0, out ModInv32Trans2x2 t)
        {
            // Transformation matrix; see comments in DivSteps30().
            uint u = 1, v = 0, q = 0, r = 1;
            uint f = f0, g = g0, m;
            ushort w;
            int i = 30, limit, zeros;

            while (true)
            {
                // Use a sentinel bit to count zeros only up to i.
                zeros = Ctz32Var(g | (uint.MaxValue << i));
                // Perform zeros divsteps at once; they all just divide g by two.
                g >>= zeros;
                u <<= zeros;
                v <<= zeros;
                eta -= zeros;
                i -= zeros;
                // We're done once we've done 30 divsteps.
                if (i == 0)
                {
                    break;
                }

                Debug.Assert((f & 1) == 1);
                Debug.Assert((g & 1) == 1);
                Debug.Assert((u * f0 + v * g0) == f << (30 - i));
                Debug.Assert((q * f0 + r * g0) == g << (30 - i));
                // Bounds on eta that follow from the bounds on iteration count (max 25*30 divsteps).
                Debug.Assert(eta >= -751 && eta <= 751);
                // If eta is negative, negate it and replace f,g with g,-f.
                if (eta < 0)
                {
                    uint tmp;
                    eta = -eta;
                    tmp = f; f = g; g = (uint)-tmp;
                    tmp = u; u = q; q = (uint)-tmp;
                    tmp = v; v = r; r = (uint)-tmp;
                }
                // eta is now >= 0. In what follows we're going to cancel out the bottom bits of g. No more
                // than i can be cancelled out (as we'd be done before that point), and no more than eta+1
                // can be done as its sign will flip once that happens.
                limit = (eta + 1) > i ? i : (eta + 1);
                // m is a mask for the bottom min(limit, 8) bits (our table only supports 8 bits).
                Debug.Assert(limit > 0 && limit <= 30);
                m = (uint.MaxValue >> (32 - limit)) & 255U;
                // Find what multiple of f must be added to g to cancel its bottom min(limit, 8) bits.
                w = (ushort)((g * Inv256[(f >> 1) & 127]) & m);
                // Do so
                g += f * w;
                q += u * w;
                r += v * w;
                Debug.Assert((g & m) == 0);
            }
            // Return data in t and return value.
            t = new ModInv32Trans2x2(u, v, q, r);
            // The determinant of t must be a power of two. This guarantees that multiplication with t
            // does not change the gcd of f and g, apart from adding a power-of-2 factor to it (which
            // will be divided out again). As each divstep's individual matrix has determinant 2, the
            // aggregate of 30 of them will have determinant 2^30.
            Debug.Assert((long)t.u * t.r - (long)t.v * t.q == 1L << 30);
            return eta;
        }


        /// <summary>
        /// Compute the transition matrix and eta for 30 posdivsteps (variable time, eta=-delta), and keeps track
        /// of the Jacobi symbol along the way. f0 and g0 must be f and g mod 2^32 rather than 2^30, because
        /// Jacobi tracking requires knowing (f mod 8) rather than just (f mod 2).
        /// </summary>
        /// <remarks>
        /// (*jacp &#38; 1) is bitflipped if and only if the Jacobi symbol of (f | g) changes sign
        /// by applying the returned transformation matrix to it. The other bits of *jacp may
        /// change, but are meaningless.
        /// </remarks>
        /// <param name="eta">initial eta</param>
        /// <param name="f0">bottom limb of initial f</param>
        /// <param name="g0">bottom limb of initial g</param>
        /// <param name="t">transition matrix</param>
        /// <param name="jacp"></param>
        /// <returns>final eta</returns>
        private static int PosDivSteps30Var(int eta, uint f0, uint g0, out ModInv32Trans2x2 t, ref int jacp)
        {
            // Transformation matrix.
            uint u = 1, v = 0, q = 0, r = 1;
            uint f = f0, g = g0, m;
            ushort w;
            int i = 30, limit, zeros;
            int jac = jacp;

            while (true)
            {
                // Use a sentinel bit to count zeros only up to i.
                zeros = Ctz32Var(g | (uint.MaxValue << i));
                // Perform zeros divsteps at once; they all just divide g by two.
                g >>= zeros;
                u <<= zeros;
                v <<= zeros;
                eta -= zeros;
                i -= zeros;
                // Update the bottom bit of jac: when dividing g by an odd power of 2,
                // if (f mod 8) is 3 or 5, the Jacobi symbol changes sign.
                jac ^= (int)(zeros & ((f >> 1) ^ (f >> 2)));
                // We're done once we've done 30 posdivsteps.
                if (i == 0)
                {
                    break;
                }

                Debug.Assert((f & 1) == 1);
                Debug.Assert((g & 1) == 1);
                Debug.Assert((u * f0 + v * g0) == f << (30 - i));
                Debug.Assert((q * f0 + r * g0) == g << (30 - i));
                // If eta is negative, negate it and replace f,g with g,f.
                if (eta < 0)
                {
                    uint tmp;
                    eta = -eta;
                    // Update bottom bit of jac: when swapping f and g, the Jacobi symbol changes sign
                    // if both f and g are 3 mod 4.
                    jac ^= (int)((f & g) >> 1);
                    tmp = f; f = g; g = tmp;
                    tmp = u; u = q; q = tmp;
                    tmp = v; v = r; r = tmp;
                }
                // eta is now >= 0. In what follows we're going to cancel out the bottom bits of g. No more
                // than i can be cancelled out (as we'd be done before that point), and no more than eta+1
                // can be done as its sign will flip once that happens.
                limit = (eta + 1) > i ? i : (eta + 1);
                // m is a mask for the bottom min(limit, 8) bits (our table only supports 8 bits).
                Debug.Assert(limit > 0 && limit <= 30);
                m = (uint.MaxValue >> (32 - limit)) & 255U;
                // Find what multiple of f must be added to g to cancel its bottom min(limit, 8) bits.
                w = (ushort)((g * Inv256[(f >> 1) & 127]) & m);
                // Do so.
                g += f * w;
                q += u * w;
                r += v * w;
                Debug.Assert((g & m) == 0);
            }

            // Return data in t and return value.
            t = new ModInv32Trans2x2(u, v, q, r);

            // The determinant of t must be a power of two. This guarantees that multiplication with t
            // does not change the gcd of f and g, apart from adding a power-of-2 factor to it (which
            // will be divided out again). As each divstep's individual matrix has determinant 2 or -2,
            // the aggregate of 30 of them will have determinant 2^30 or -2^30.
            Debug.Assert((long)t.u * t.r - (long)t.v * t.q == ((long)1) << 30 ||
                         (long)t.u * t.r - (long)t.v * t.q == -(((long)1) << 30));
            jacp = jac;
            return eta;
        }


        // Compute (t/2^30) * [d, e] mod modulus, where t is a transition matrix for 30 divsteps.
        // 
        // On input and output, d and e are in range (-2*modulus,modulus). All output limbs will be
        // in range (-2^30,2^30).
        // 
        // This implements the update_de function from the explanation.
        private static void UpdateDE30(ref ModInv32Signed30 d, ref ModInv32Signed30 e, ModInv32Trans2x2 t, in ModInv32ModInfo modinfo)
        {
            const int M30 = (int)(uint.MaxValue >> 2);
            int di, ei, md, me, sd, se;
            long cd, ce;
#if DEBUG
            Debug.Assert(MulCmp30(d, 9, modinfo.modulus, -2) > 0); // d > -2*modulus
            Debug.Assert(MulCmp30(d, 9, modinfo.modulus, 1) < 0);  // d <    modulus
            Debug.Assert(MulCmp30(e, 9, modinfo.modulus, -2) > 0); // e > -2*modulus
            Debug.Assert(MulCmp30(e, 9, modinfo.modulus, 1) < 0);  // e <    modulus
            Debug.Assert(Math.Abs(t.u) <= (M30 + 1 - Math.Abs(t.v))); // |u|+|v| <= 2^30
            Debug.Assert(Math.Abs(t.q) <= (M30 + 1 - Math.Abs(t.r))); // |q|+|r| <= 2^30
#endif
            // [md,me] start as zero; plus [u,q] if d is negative; plus [v,r] if e is negative.
            sd = d.v8 >> 31;
            se = e.v8 >> 31;
            md = (t.u & sd) + (t.v & se);
            me = (t.q & sd) + (t.r & se);
            // Begin computing t*[d,e]
            di = d.v0;
            ei = e.v0;
            cd = (long)t.u * di + (long)t.v * ei;
            ce = (long)t.q * di + (long)t.r * ei;
            // Correct md,me so that t*[d,e]+modulus*[md,me] has 30 zero bottom bits.
            md -= (int)((modinfo.modulus_inv30 * (uint)cd + md) & M30);
            me -= (int)((modinfo.modulus_inv30 * (uint)ce + me) & M30);
            // Update the beginning of computation for t*[d,e]+modulus*[md,me] now md,me are known.
            cd += (long)modinfo.modulus.v0 * md;
            ce += (long)modinfo.modulus.v0 * me;
            // Verify that the low 30 bits of the computation are indeed zero, and then throw them away.
            Debug.Assert(((int)cd & M30) == 0); cd >>= 30;
            Debug.Assert(((int)ce & M30) == 0); ce >>= 30;
            // Now iteratively compute limb i=1..8 of t*[d,e]+modulus*[md,me], and store them in output
            // limb i-1 (shifting down by 30 bits).
            int[] dv = d.GetArray();
            int[] ev = e.GetArray();
            int[] modv = modinfo.modulus.GetArray();
            for (int i = 1; i < 9; i++)
            {
                di = dv[i];
                ei = ev[i];
                cd += (long)t.u * di + (long)t.v * ei;
                ce += (long)t.q * di + (long)t.r * ei;
                cd += (long)modv[i] * md;
                ce += (long)modv[i] * me;
                dv[i - 1] = (int)cd & M30; cd >>= 30;
                ev[i - 1] = (int)ce & M30; ce >>= 30;
            }
            // What remains is limb 9 of t*[d,e]+modulus*[md,me]; store it as output limb 8.
            dv[8] = (int)cd;
            ev[8] = (int)ce;

            d = new ModInv32Signed30(dv);
            e = new ModInv32Signed30(ev);
#if DEBUG
            Debug.Assert(MulCmp30(d, 9, modinfo.modulus, -2) > 0); // d > -2*modulus
            Debug.Assert(MulCmp30(d, 9, modinfo.modulus, 1) < 0);  // d <    modulus
            Debug.Assert(MulCmp30(e, 9, modinfo.modulus, -2) > 0); // e > -2*modulus
            Debug.Assert(MulCmp30(e, 9, modinfo.modulus, 1) < 0);  // e <    modulus
#endif
        }


        // Compute (t/2^30) * [f, g], where t is a transition matrix for 30 divsteps.
        //
        // This implements the update_fg function from the explanation.
        private static void UpdateFG30(ref ModInv32Signed30 f, ref ModInv32Signed30 g, ModInv32Trans2x2 t)
        {
            const int M30 = (int)(uint.MaxValue >> 2);
            int fi, gi;
            long cf, cg;
            // Start computing t*[f,g].
            fi = f.v0;
            gi = g.v0;
            cf = (long)t.u * fi + (long)t.v * gi;
            cg = (long)t.q * fi + (long)t.r * gi;
            // Verify that the bottom 30 bits of the result are zero, and then throw them away.
            Debug.Assert(((int)cf & M30) == 0); cf >>= 30;
            Debug.Assert(((int)cg & M30) == 0); cg >>= 30;
            // Now iteratively compute limb i=1..8 of t*[f,g], and store them in output limb i-1 (shifting
            // down by 30 bits).
            int[] fv = f.GetArray();
            int[] gv = g.GetArray();
            for (int i = 1; i < 9; i++)
            {
                fi = fv[i];
                gi = gv[i];
                cf += (long)t.u * fi + (long)t.v * gi;
                cg += (long)t.q * fi + (long)t.r * gi;
                fv[i - 1] = (int)cf & M30; cf >>= 30;
                gv[i - 1] = (int)cg & M30; cg >>= 30;
            }
            // What remains is limb 9 of t*[f,g]; store it as output limb 8.
            fv[8] = (int)cf;
            gv[8] = (int)cg;

            f = new ModInv32Signed30(fv);
            g = new ModInv32Signed30(gv);
        }

        // Compute (t/2^30) * [f, g], where t is a transition matrix for 30 divsteps.
        //
        // Version that operates on a variable number of limbs in f and g.
        //
        // This implements the update_fg function from the explanation in modinv64_impl.h.
        private static void UpdateFG30Var(int len, ref ModInv32Signed30 f, ref ModInv32Signed30 g, ModInv32Trans2x2 t)
        {
            const int M30 = (int)(uint.MaxValue >> 2);
            int fi, gi;
            long cf, cg;
            Debug.Assert(len > 0);
            // Start computing t*[f,g].
            fi = f.v0;
            gi = g.v0;
            cf = (long)t.u * fi + (long)t.v * gi;
            cg = (long)t.q * fi + (long)t.r * gi;
            // Verify that the bottom 62 bits of the result are zero, and then throw them away.
            Debug.Assert(((int)cf & M30) == 0); cf >>= 30;
            Debug.Assert(((int)cg & M30) == 0); cg >>= 30;
            // Now iteratively compute limb i=1..len of t*[f,g], and store them in output limb i-1 (shifting
            // down by 30 bits).
            int[] fv = f.GetArray();
            int[] gv = g.GetArray();
            for (int i = 1; i < len; i++)
            {
                fi = fv[i];
                gi = gv[i];
                cf += (long)t.u * fi + (long)t.v * gi;
                cg += (long)t.q * fi + (long)t.r * gi;
                fv[i - 1] = (int)cf & M30; cf >>= 30;
                gv[i - 1] = (int)cg & M30; cg >>= 30;
            }
            // What remains is limb (len) of t*[f,g]; store it as output limb (len-1).
            fv[len - 1] = (int)cf;
            gv[len - 1] = (int)cg;

            f = new ModInv32Signed30(fv);
            g = new ModInv32Signed30(gv);
        }



        /// <summary>
        /// Replace x with its modular inverse mod modinfo->modulus. x must be in range [0, modulus).
        /// If x is zero, the result will be zero as well. If not, the inverse must exist(i.e., the gcd of
        /// x and modulus must be 1). These rules are automatically satisfied if the modulus is prime.
        ///
        /// On output, all of x's limbs will be in [0, 2^30).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="modinfo"></param>
        public static void Compute(ref ModInv32Signed30 x, in ModInv32ModInfo modinfo)
        {
            // Start with d=0, e=1, f=modulus, g=x, zeta=-1
            ModInv32Signed30 d = ModInv32Signed30.Zero;
            ModInv32Signed30 e = ModInv32Signed30.One;
            ModInv32Signed30 f = modinfo.modulus;
            ModInv32Signed30 g = x;
            int zeta = -1; // zeta = -(delta+1/2); delta is initially 1/2

            // Do 20 iterations of 30 divsteps each = 600 divsteps. 590 suffices for 256-bit inputs.
            for (int i = 0; i < 20; i++)
            {
                // Compute transition matrix and new zeta after 30 divsteps.
                zeta = DivSteps30(zeta, (uint)f.v0, (uint)g.v0, out ModInv32Trans2x2 t);
                // Update d,e using that transition matrix.
                UpdateDE30(ref d, ref e, t, modinfo);
                // Update f,g using that transition matrix.
#if DEBUG
                Debug.Assert(MulCmp30(f, 9, modinfo.modulus, -1) > 0); // f > -modulus
                Debug.Assert(MulCmp30(f, 9, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp30(g, 9, modinfo.modulus, -1) > 0); // g > -modulus
                Debug.Assert(MulCmp30(g, 9, modinfo.modulus, 1) < 0);  // g <  modulus
#endif
                UpdateFG30(ref f, ref g, t);
#if DEBUG
                Debug.Assert(MulCmp30(f, 9, modinfo.modulus, -1) > 0); // f > -modulus
                Debug.Assert(MulCmp30(f, 9, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp30(g, 9, modinfo.modulus, -1) > 0); // g > -modulus
                Debug.Assert(MulCmp30(g, 9, modinfo.modulus, 1) < 0);  // g <  modulus
#endif
            }

            // At this point sufficient iterations have been performed that g must have reached 0
            // and (if g was not originally 0) f must now equal +/- GCD of the initial f, g
            // values i.e. +/- 1, and d now contains +/- the modular inverse.
#if DEBUG
            // g == 0
            Debug.Assert(MulCmp30(g, 9, ModInv32Signed30.One, 0) == 0);
            // |f| == 1, or (x == 0 and d == 0 and |f|=modulus)
            Debug.Assert(MulCmp30(f, 9, ModInv32Signed30.One, -1) == 0 ||
                         MulCmp30(f, 9, ModInv32Signed30.One, 1) == 0 ||
                         (MulCmp30(x, 9, ModInv32Signed30.One, 0) == 0 &&
                          MulCmp30(d, 9, ModInv32Signed30.One, 0) == 0 &&
                          (MulCmp30(f, 9, modinfo.modulus, 1) == 0 ||
                           MulCmp30(f, 9, modinfo.modulus, -1) == 0)));
#endif

            // Optionally negate d, normalize to [0,modulus), and return it.
            x = Normalize30(d, f.v8, modinfo);
        }


        /// <summary>
        /// Replace x with its modular inverse mod modinfo->modulus. x must be in range [0, modulus).
        /// If x is zero, the result will be zero as well. If not, the inverse must exist(i.e., the gcd of
        /// x and modulus must be 1). These rules are automatically satisfied if the modulus is prime.
        ///
        /// On output, all of x's limbs will be in [0, 2^30).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="modinfo"></param>
        public static void ComputeVar(ref ModInv32Signed30 x, ModInv32ModInfo modinfo)
        {
            // Start with d=0, e=1, f=modulus, g=x, eta=-1.
            ModInv32Signed30 d = ModInv32Signed30.Zero;
            ModInv32Signed30 e = ModInv32Signed30.One;
            ModInv32Signed30 f = modinfo.modulus;
            ModInv32Signed30 g = x;
#if DEBUG
            int i = 0;
#endif
            int j, len = 9;
            int eta = -1; // eta = -delta; delta is initially 1 (faster for the variable-time code)
            int cond, fn, gn;

            // Do iterations of 30 divsteps each until g=0.
            while (true)
            {
                // Compute transition matrix and new eta after 30 divsteps.
                eta = DivSteps30Var(eta, (uint)f.v0, (uint)g.v0, out ModInv32Trans2x2 t);
                // Update d,e using that transition matrix.
                UpdateDE30(ref d, ref e, t, modinfo);
                // Update f,g using that transition matrix.
# if DEBUG
                Debug.Assert(MulCmp30(f, len, modinfo.modulus, -1) > 0); // f > -modulus
                Debug.Assert(MulCmp30(f, len, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp30(g, len, modinfo.modulus, -1) > 0); // g > -modulus
                Debug.Assert(MulCmp30(g, len, modinfo.modulus, 1) < 0);  // g <  modulus
#endif
                UpdateFG30Var(len, ref f, ref g, t);

                // If the bottom limb of g is 0, there is a chance g=0.
                int[] fv = f.GetArray();
                int[] gv = g.GetArray();
                if (gv[0] == 0)
                {
                    cond = 0;
                    // Check if all other limbs are also 0.
                    for (j = 1; j < len; ++j)
                    {
                        cond |= gv[j];
                    }
                    // If so, we're done.
                    if (cond == 0)
                    {
                        break;
                    }
                }

                // Determine if len>1 and limb (len-1) of both f and g is 0 or -1.
                fn = fv[len - 1];
                gn = gv[len - 1];
                cond = (len - 2) >> 31;
                cond |= fn ^ (fn >> 31);
                cond |= gn ^ (gn >> 31);
                // If so, reduce length, propagating the sign of f and g's top limb into the one below.
                if (cond == 0)
                {
                    fv[len - 2] |= fn << 30;
                    gv[len - 2] |= gn << 30;
                    len--;
                }

                f = new ModInv32Signed30(fv);
                g = new ModInv32Signed30(gv);
#if DEBUG
                Debug.Assert(++i < 25); // We should never need more than 25*30 = 750 divsteps
                Debug.Assert(MulCmp30(f, len, modinfo.modulus, -1) > 0); // f > -modulus
                Debug.Assert(MulCmp30(f, len, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp30(g, len, modinfo.modulus, -1) > 0); // g > -modulus
                Debug.Assert(MulCmp30(g, len, modinfo.modulus, 1) < 0);  // g <  modulus
#endif
            }

            // At this point g is 0 and (if g was not originally 0) f must now equal +/- GCD of
            // the initial f, g values i.e. +/- 1, and d now contains +/- the modular inverse.
#if DEBUG
            // g == 0
            Debug.Assert(MulCmp30(g, len, ModInv32Signed30.One, 0) == 0);
            // |f| == 1, or (x == 0 and d == 0 and |f|=modulus)
            Debug.Assert(MulCmp30(f, len, ModInv32Signed30.One, -1) == 0 ||
                         MulCmp30(f, len, ModInv32Signed30.One, 1) == 0 ||
                         (MulCmp30(x, 9, ModInv32Signed30.One, 0) == 0 &&
                          MulCmp30(d, 9, ModInv32Signed30.One, 0) == 0 &&
                          (MulCmp30(f, len, modinfo.modulus, 1) == 0 ||
                           MulCmp30(f, len, modinfo.modulus, -1) == 0)));
#endif

            // Optionally negate d, normalize to [0,modulus), and return it.
            int[] tempArr = f.GetArray();
            x = Normalize30(d, tempArr[len - 1], modinfo);
        }


        /// <summary>
        /// Compute the Jacobi symbol for (x | modinfo->modulus). x must be coprime with modulus (and thus
        /// cannot be 0, as modulus >= 3). All limbs of x must be non-negative. Returns 0 if the result
        /// cannot be computed.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="modinfo"></param>
        /// <returns></returns>
        public static int Jacobi32MaybeVar(in ModInv32Signed30 x, in ModInv32ModInfo modinfo)
        {
            // Start with f=modulus, g=x, eta=-1.
            ModInv32Signed30 f = modinfo.modulus;
            ModInv32Signed30 g = x;
            int len = 9;
            // eta = -delta; delta is initially 1
            int eta = -1;
            int cond, fn, gn;
            int jac = 0;

            // The input limbs must all be non-negative.
            Debug.Assert(g.v0 >= 0 && g.v1 >= 0 && g.v0 >= 0 && g.v3 >= 0 &&
                         g.v4 >= 0 && g.v5 >= 0 && g.v6 >= 0 && g.v7 >= 0 && g.v8 >= 0);

            // If x > 0, then if the loop below converges, it converges to f=g=gcd(x,modulus). Since we
            // require that gcd(x,modulus)=1 and modulus>=3, x cannot be 0. Thus, we must reach f=1 (or
            // time out).
            Debug.Assert((g.v0 | g.v1 | g.v2 | g.v3 | g.v4 | g.v5 | g.v6 | g.v7 | g.v8) != 0);

            const int JACOBI32_ITERATIONS =
#if DEBUG
                25;
#else
                50;
#endif
            for (int count = 0; count < JACOBI32_ITERATIONS; count++)
            {
                // Compute transition matrix and new eta after 30 posdivsteps.
                eta = PosDivSteps30Var(eta, (uint)f.v0 | ((uint)f.v1 << 30), (uint)g.v0 | ((uint)g.v1 << 30), out ModInv32Trans2x2 t, ref jac);
#if DEBUG
                // Update f,g using that transition matrix.
                Debug.Assert(MulCmp30(f, len, modinfo.modulus, 0) > 0); // f > 0
                Debug.Assert(MulCmp30(f, len, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp30(g, len, modinfo.modulus, 0) > 0); // g > 0
                Debug.Assert(MulCmp30(g, len, modinfo.modulus, 1) < 0);  // g < modulus
#endif
                UpdateFG30Var(len, ref f, ref g, t);

                // If the bottom limb of f is 1, there is a chance that f=1.
                int[] fv = f.GetArray();
                int[] gv = g.GetArray();
                if (f.v0 == 1)
                {
                    cond = 0;
                    // Check if the other limbs are also 0.
                    for (int j = 1; j < len; j++)
                    {
                        cond |= fv[j];
                    }
                    // If so, we're done. If f=1, the Jacobi symbol (g | f)=1.
                    if (cond == 0)
                    {
                        return 1 - 2 * (jac & 1);
                    }
                }

                // Determine if len>1 and limb (len-1) of both f and g is 0.
                fn = fv[len - 1];
                gn = gv[len - 1];
                cond = (len - 2) >> 31;
                cond |= fn;
                cond |= gn;
                // If so, reduce length.
                if (cond == 0)
                {
                    len--;
                }
#if DEBUG
                Debug.Assert(MulCmp30(f, len, modinfo.modulus, 0) > 0); // f > 0
                Debug.Assert(MulCmp30(f, len, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp30(g, len, modinfo.modulus, 0) > 0); // g > 0
                Debug.Assert(MulCmp30(g, len, modinfo.modulus, 1) < 0);  // g < modulus
#endif
            }

            // The loop failed to converge to f=g after 1500 iterations. Return 0, indicating unknown result.
            return 0;
        }
    }
}
