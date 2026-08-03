// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

#if !NET10_0
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
#endif
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve.ModInv
{
    public static class ModInv64
    {
#if DEBUG
        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Compute a*d - b*c from signed 64-bit values and write the result to r.
        private static Int128 Det(long a, long b, long c, long d)
        {
            Int128 ad = (Int128)a * d;
            Int128 bc = (Int128)b * c;
            Debug.Assert(0 <= bc ? Int128.MinValue + bc <= ad : ad <= Int128.MaxValue + bc);
            return ad - bc;
        }

        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Tests if r is equal to sign*2^n (sign must be 1 or -1).
        // n must be strictly less than 127.
        private static int CheckPow2(in Int128 r, uint n, int sign)
        {
            Debug.Assert(n < 127);
            Debug.Assert(sign == 1 || sign == -1);
            return (r == (Int128)((UInt128)sign << (int)n)) ? 1 : 0;
        }

        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        // Check if the determinant of t is equal to 1 << n. If abs, check if |det t| == 1 << n.
        private static int DetCheckPow2(in ModInv64Trans2x2 t, uint n, int abs)
        {
            Int128 a = Det(t.u, t.v, t.q, t.r);
            if (CheckPow2(a, n, 1) == 1)
                return 1;
            if (abs == 1 && CheckPow2(a, n, -1) == 1)
                return 1;
            return 0;
        }

        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        // Compute a*factor and return it. All but the top limb in result will be in range [0,2^62).
        private static ModInv64Signed62 Mul62(in ModInv64Signed62 a, int alen, long factor)
        {
            const ulong M62 = ulong.MaxValue >> 2;
            Int128 c = 0;
            long[] av = a.GetArray();
            long[] rv = new long[5];
            for (int i = 0; i < 4; i++)
            {
                if (i < alen)
                {
                    c += (Int128)av[i] * factor;
                }
                rv[i] = (long)((ulong)c & M62);
                c >>= 62;
            }

            if (4 < alen)
            {
                c += (Int128)av[4] * factor;
            }

            Debug.Assert(c == (long)c);
            rv[4] = (long)c;

            return new ModInv64Signed62(rv);
        }

        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        // Return -1 for a<b*factor, 0 for a==b*factor, 1 for a>b*factor. A has alen limbs; b has 5.
        private static int MulCmp62(in ModInv64Signed62 a, int alen, in ModInv64Signed62 b, long factor)
        {
            ModInv64Signed62 am = Mul62(a, alen, 1); // Normalize all but the top limb of a.
            ModInv64Signed62 bm = Mul62(b, 5, factor);
            long[] amv = am.GetArray();
            long[] bmv = bm.GetArray();
            for (int i = 0; i < 4; i++)
            {
                // Verify that all but the top limb of a and b are normalized.
                Debug.Assert(amv[i] >> 62 == 0);
                Debug.Assert(bmv[i] >> 62 == 0);
            }
            for (int i = 4; i >= 0; i--)
            {
                if (amv[i] < bmv[i]) return -1;
                if (amv[i] > bmv[i]) return 1;
            }
            return 0;
        }
#endif // DEBUG


        // Take as input a signed62 number in range (-2*modulus,modulus), and add a multiple of the modulus
        // to it to bring it to range [0,modulus). If sign < 0, the input will also be negated in the
        // process. The input must have limbs in range (-2^62,2^62). The output will have limbs in range
        // [0,2^62). */
        private static ModInv64Signed62 Normalize62(in ModInv64Signed62 r, long sign, in ModInv64ModInfo modinfo)
        {
            const long M62 = (long)(ulong.MaxValue >> 2);
            long r0 = r.v0, r1 = r.v1, r2 = r.v2, r3 = r.v3, r4 = r.v4;
            long cond_add, cond_negate;

#if DEBUG
            // Verify that all limbs are in range (-2^62,2^62).
            long[] rv = r.GetArray();
            for (int i = 0; i < 5; i++)
            {
                Debug.Assert(rv[i] >= -M62);
                Debug.Assert(rv[i] <= M62);
            }
            Debug.Assert(MulCmp62(r, 5, modinfo.modulus, -2) > 0); // r > -2*modulus
            Debug.Assert(MulCmp62(r, 5, modinfo.modulus, 1) < 0);  // r < modulus
#endif

            // In a first step, add the modulus if the input is negative, and then negate if requested.
            // This brings r from range (-2*modulus,modulus) to range (-modulus,modulus). As all input
            // limbs are in range (-2^62,2^62), this cannot overflow an int64_t. Note that the right
            // shifts below are signed sign-extending shifts (see assumptions.h for tests that that is
            // indeed the behavior of the right shift operator).
            cond_add = r4 >> 63;
            r0 += modinfo.modulus.v0 & cond_add;
            r1 += modinfo.modulus.v1 & cond_add;
            r2 += modinfo.modulus.v2 & cond_add;
            r3 += modinfo.modulus.v3 & cond_add;
            r4 += modinfo.modulus.v4 & cond_add;
            cond_negate = sign >> 63;
            r0 = (r0 ^ cond_negate) - cond_negate;
            r1 = (r1 ^ cond_negate) - cond_negate;
            r2 = (r2 ^ cond_negate) - cond_negate;
            r3 = (r3 ^ cond_negate) - cond_negate;
            r4 = (r4 ^ cond_negate) - cond_negate;
            // Propagate the top bits, to bring limbs back to range (-2^62,2^62).
            r1 += r0 >> 62; r0 &= M62;
            r2 += r1 >> 62; r1 &= M62;
            r3 += r2 >> 62; r2 &= M62;
            r4 += r3 >> 62; r3 &= M62;

            // In a second step add the modulus again if the result is still negative, bringing
            // r to range [0,modulus).
            cond_add = r4 >> 63;
            r0 += modinfo.modulus.v0 & cond_add;
            r1 += modinfo.modulus.v1 & cond_add;
            r2 += modinfo.modulus.v2 & cond_add;
            r3 += modinfo.modulus.v3 & cond_add;
            r4 += modinfo.modulus.v4 & cond_add;
            // And propagate again.
            r1 += r0 >> 62; r0 &= M62;
            r2 += r1 >> 62; r1 &= M62;
            r3 += r2 >> 62; r2 &= M62;
            r4 += r3 >> 62; r3 &= M62;

            ModInv64Signed62 result = new ModInv64Signed62(r0, r1, r2, r3, r4);
#if DEBUG
            Debug.Assert(r0 >> 62 == 0);
            Debug.Assert(r1 >> 62 == 0);
            Debug.Assert(r2 >> 62 == 0);
            Debug.Assert(r3 >> 62 == 0);
            Debug.Assert(r4 >> 62 == 0);
            Debug.Assert(MulCmp62(result, 5, modinfo.modulus, 0) >= 0); // r >= 0
            Debug.Assert(MulCmp62(result, 5, modinfo.modulus, 1) < 0); // r < modulus
#endif
            return result;
        }


        // Compute the transition matrix and eta for 59 divsteps (where zeta=-(delta+1/2)).
        // Note that the transformation matrix is scaled by 2^62 and not 2^59.
        // 
        // Input:  zeta: initial zeta
        //         f0:   bottom limb of initial f
        //         g0:   bottom limb of initial g
        // Output: t: transition matrix
        // Return: final zeta
        // 
        // Implements the divsteps_n_matrix function from the explanation.
        private static long DivSteps59(long zeta, ulong f0, ulong g0, out ModInv64Trans2x2 t)
        {
            // u,v,q,r are the elements of the transformation matrix being built up,
            // starting with the identity matrix times 8 (because the caller expects
            // a result scaled by 2^62). Semantically they are signed integers
            // in range [-2^62,2^62], but here represented as unsigned mod 2^64. This
            // permits left shifting (which is UB for negative numbers). The range
            // being inside [-2^63,2^63) means that casting to signed works correctly.
            ulong u = 8, v = 0, q = 0, r = 8;
            ulong mask1, mask2, f = f0, g = g0, x, y, z;

            for (int i = 3; i < 62; i++)
            {
                Debug.Assert((f & 1) == 1); // f must always be odd
                Debug.Assert((u * f0 + v * g0) == f << i);
                Debug.Assert((q * f0 + r * g0) == g << i);
                // Compute conditional masks for (zeta < 0) and for (g & 1).
                mask1 = (ulong)(zeta >> 63);
                mask2 = (ulong)-(long)(g & 1);

                // Compute x,y,z, conditionally negated versions of f,u,v.
                x = (f ^ mask1) - mask1;
                y = (u ^ mask1) - mask1;
                z = (v ^ mask1) - mask1;
                // Conditionally add x,y,z to g,q,r.
                g += x & mask2;
                q += y & mask2;
                r += z & mask2;
                // In what follows, c1 is a condition mask for (zeta < 0) and (g & 1).
                mask1 &= mask2;
                // Conditionally change zeta into -zeta-2 or zeta-1.
                zeta = (zeta ^ (long)mask1) - 1;
                // Conditionally add g,q,r to f,u,v.
                f += g & mask1;
                u += q & mask1;
                v += r & mask1;
                /* Shifts */
                g >>= 1;
                u <<= 1;
                v <<= 1;
                /* Bounds on zeta that follow from the bounds on iteration count (max 10*59 divsteps). */
                Debug.Assert(zeta >= -591 && zeta <= 591);
            }
            // Return data in t and return value.
            t = new ModInv64Trans2x2(u, v, q, r);

#if DEBUG
            // The determinant of t must be a power of two. This guarantees that multiplication with t
            // does not change the gcd of f and g, apart from adding a power-of-2 factor to it (which
            // will be divided out again). As each divstep's individual matrix has determinant 2, the
            // aggregate of 59 of them will have determinant 2^59. Multiplying with the initial
            // 8*identity (which has determinant 2^6) means the overall outputs has determinant
            // 2^65.
            Debug.Assert(DetCheckPow2(t, 65, 0) == 1);
#endif
            return zeta;
        }

#if !NET10_0_OR_GREATER
        // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/util.h#L382-L390
        private static readonly byte[] DeBruijn = new byte[64]
        {
            0, 1, 2, 53, 3, 7, 54, 27, 4, 38, 41, 8, 34, 55, 48, 28,
            62, 5, 39, 46, 44, 42, 22, 9, 24, 35, 59, 56, 49, 18, 29, 11,
            63, 52, 6, 26, 37, 40, 33, 47, 61, 45, 43, 21, 23, 58, 17, 10,
            51, 25, 36, 32, 60, 20, 57, 16, 50, 31, 19, 15, 30, 14, 13, 12
        };
#endif
        // Determine the number of trailing zero bits in a (non-zero) 64-bit x.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Ctz64Var(ulong x)
        {
            Debug.Assert(x != 0);
#if NET10_0_OR_GREATER
            return System.Numerics.BitOperations.TrailingZeroCount(x);
#else
            return DeBruijn[((x & (ulong)-(long)x) * 0x022FDD63CC95386DU) >> 58];
#endif
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
        private static long DivSteps62Var(long eta, ulong f0, ulong g0, out ModInv64Trans2x2 t)
        {
            // Transformation matrix; see comments in DivSteps62().
            ulong u = 1, v = 0, q = 0, r = 1;
            ulong f = f0, g = g0, m;
            uint w;
            int i = 62, limit, zeros;

            while (true)
            {
                // Use a sentinel bit to count zeros only up to i.
                zeros = Ctz64Var(g | (ulong.MaxValue << i));
                // Perform zeros divsteps at once; they all just divide g by two.
                g >>= zeros;
                u <<= zeros;
                v <<= zeros;
                eta -= zeros;
                i -= zeros;
                // We're done once we've done 62 divsteps.
                if (i == 0)
                {
                    break;
                }

                Debug.Assert((f & 1) == 1);
                Debug.Assert((g & 1) == 1);
                Debug.Assert((u * f0 + v * g0) == f << (62 - i));
                Debug.Assert((q * f0 + r * g0) == g << (62 - i));
                // Bounds on eta that follow from the bounds on iteration count (max 12*62 divsteps).
                Debug.Assert(eta >= -745 && eta <= 745);

                // If eta is negative, negate it and replace f,g with g,-f.
                if (eta < 0)
                {
                    ulong tmp;
                    eta = -eta;
                    tmp = f; f = g; g = (ulong)-(long)tmp;
                    tmp = u; u = q; q = (ulong)-(long)tmp;
                    tmp = v; v = r; r = (ulong)-(long)tmp;
                    // Use a formula to cancel out up to 6 bits of g. Also, no more than i can be cancelled
                    // out (as we'd be done before that point), and no more than eta+1 can be done as its
                    // sign will flip again once that happens.
                    limit = ((int)eta + 1) > i ? i : ((int)eta + 1);
                    Debug.Assert(limit > 0 && limit <= 62);
                    // m is a mask for the bottom min(limit, 6) bits.
                    m = (ulong.MaxValue >> (64 - limit)) & 63U;
                    // Find what multiple of f must be added to g to cancel its bottom min(limit, 6) bits.
                    w = (uint)((f * g * (f * f - 2)) & m);
                }
                else
                {
                    // In this branch, use a simpler formula that only lets us cancel up to 4 bits of g, as
                    // eta tends to be smaller here.
                    limit = ((int)eta + 1) > i ? i : ((int)eta + 1);
                    Debug.Assert(limit > 0 && limit <= 62);
                    // m is a mask for the bottom min(limit, 4) bits.
                    m = (ulong.MaxValue >> (64 - limit)) & 15U;
                    // Find what multiple of f must be added to g to cancel its bottom min(limit, 4)
                    // bits.
                    w = (uint)(f + (((f + 1) & 4) << 1));
                    w = (uint)(((ulong)-w * g) & m);
                }
                g += f * w;
                q += u * w;
                r += v * w;
                Debug.Assert((g & m) == 0);
            }
            // Return data in t and return value.
            t = new ModInv64Trans2x2(u, v, q, r);

#if DEBUG
            // The determinant of t must be a power of two. This guarantees that multiplication with t
            // does not change the gcd of f and g, apart from adding a power-of-2 factor to it (which
            // will be divided out again). As each divstep's individual matrix has determinant 2, the
            // aggregate of 62 of them will have determinant 2^62.
            Debug.Assert(DetCheckPow2(t, 62, 0) == 1);
#endif
            return eta;
        }


        /// <summary>
        /// Compute the transition matrix and eta for 62 posdivsteps (variable time, eta=-delta), and keeps track
        /// of the Jacobi symbol along the way. f0 and g0 must be f and g mod 2^64 rather than 2^62, because
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
        private static long PosDivSteps62Var(long eta, ulong f0, ulong g0, out ModInv64Trans2x2 t, ref int jacp)
        {
            // Transformation matrix. See comments in Divsteps62()
            ulong u = 1, v = 0, q = 0, r = 1;
            ulong f = f0, g = g0, m;
            uint w;
            int i = 62, limit, zeros;
            // TODO: can we skip assigning jac and use jacp?
            int jac = jacp;

            while (true)
            {
                // Use a sentinel bit to count zeros only up to i.
                zeros = Ctz64Var(g | (ulong.MaxValue << i));
                // Perform zeros divsteps at once; they all just divide g by two.
                g >>= zeros;
                u <<= zeros;
                v <<= zeros;
                eta -= zeros;
                i -= zeros;
                // Update the bottom bit of jac: when dividing g by an odd power of 2,
                // if (f mod 8) is 3 or 5, the Jacobi symbol changes sign.
                jac ^= zeros & (int)((f >> 1) ^ (f >> 2));
                // We're done once we've done 62 posdivsteps.
                if (i == 0)
                {
                    break;
                }

                Debug.Assert((f & 1) == 1);
                Debug.Assert((g & 1) == 1);
                Debug.Assert((u * f0 + v * g0) == f << (62 - i));
                Debug.Assert((q * f0 + r * g0) == g << (62 - i));
                // If eta is negative, negate it and replace f,g with g,f.
                if (eta < 0)
                {
                    ulong tmp;
                    eta = -eta;
                    tmp = f; f = g; g = tmp;
                    tmp = u; u = q; q = tmp;
                    tmp = v; v = r; r = tmp;
                    // Update bottom bit of jac: when swapping f and g, the Jacobi symbol changes sign
                    // if both f and g are 3 mod 4.
                    jac ^= (int)((f & g) >> 1);
                    // Use a formula to cancel out up to 6 bits of g. Also, no more than i can be cancelled
                    // out (as we'd be done before that point), and no more than eta+1 can be done as its
                    // sign will flip again once that happens.
                    limit = ((int)eta + 1) > i ? i : ((int)eta + 1);
                    Debug.Assert(limit > 0 && limit <= 62);
                    /* m is a mask for the bottom min(limit, 6) bits. */
                    m = (ulong.MaxValue >> (64 - limit)) & 63U;
                    /* Find what multiple of f must be added to g to cancel its bottom min(limit, 6)
                     * bits. */
                    w = (uint)((f * g * (f * f - 2)) & m);
                }
                else
                {
                    /* In this branch, use a simpler formula that only lets us cancel up to 4 bits of g, as
                     * eta tends to be smaller here. */
                    limit = ((int)eta + 1) > i ? i : ((int)eta + 1);
                    Debug.Assert(limit > 0 && limit <= 62);
                    /* m is a mask for the bottom min(limit, 4) bits. */
                    m = (ulong.MaxValue >> (64 - limit)) & 15U;
                    /* Find what multiple of f must be added to g to cancel its bottom min(limit, 4)
                     * bits. */
                    w = (uint)(f + (((f + 1) & 4) << 1));
                    w = (uint)(((ulong)-w * g) & m);
                }
                g += f * w;
                q += u * w;
                r += v * w;
                Debug.Assert((g & m) == 0);
            }

            // Return data in t and return value.
            t = new ModInv64Trans2x2(u, v, q, r);

#if DEBUG
            // The determinant of t must be a power of two. This guarantees that multiplication with t
            // does not change the gcd of f and g, apart from adding a power-of-2 factor to it (which
            // will be divided out again). As each divstep's individual matrix has determinant 2 or -2,
            // the aggregate of 62 of them will have determinant 2^62 or -2^62.
            Debug.Assert(DetCheckPow2(t, 62, 1) == 1);
#endif

            jacp = jac;
            return eta;
        }


        // Compute (t/2^62) * [d, e] mod modulus, where t is a transition matrix scaled by 2^62.
        // 
        // On input and output, d and e are in range (-2*modulus,modulus). All output limbs will be in range
        // (-2^62,2^62).
        // 
        // This implements the update_de function from the explanation.
        private static void UpdateDE62(ref ModInv64Signed62 d, ref ModInv64Signed62 e, in ModInv64Trans2x2 t, in ModInv64ModInfo modinfo)
        {
            const ulong M62 = ulong.MaxValue >> 2;
            long md, me, sd, se;
            Int128 cd, ce;
#if DEBUG
            Debug.Assert(MulCmp62(d, 5, modinfo.modulus, -2) > 0); // d > -2*modulus
            Debug.Assert(MulCmp62(d, 5, modinfo.modulus, 1) < 0);  // d <    modulus
            Debug.Assert(MulCmp62(e, 5, modinfo.modulus, -2) > 0); // e > -2*modulus
            Debug.Assert(MulCmp62(e, 5, modinfo.modulus, 1) < 0);  // e <    modulus
            Debug.Assert(Math.Abs(t.u) <= (((long)1 << 62) - Math.Abs(t.v))); // |u|+|v| <= 2^62
            Debug.Assert(Math.Abs(t.q) <= (((long)1 << 62) - Math.Abs(t.r))); // |q|+|r| <= 2^62
#endif
            // [md,me] start as zero; plus [u,q] if d is negative; plus [v,r] if e is negative.
            sd = d.v4 >> 63;
            se = e.v4 >> 63;
            md = (t.u & sd) + (t.v & se);
            me = (t.q & sd) + (t.r & se);
            // Begin computing t*[d,e]
            cd = ((Int128)t.u * d.v0) + ((Int128)t.v * e.v0);
            ce = ((Int128)t.q * d.v0) + ((Int128)t.r * e.v0);
            // Correct md,me so that t*[d,e]+modulus*[md,me] has 62 zero bottom bits.
            md -= (long)((modinfo.modulus_inv62 * (ulong)cd + (ulong)md) & M62);
            me -= (long)((modinfo.modulus_inv62 * (ulong)ce + (ulong)me) & M62);
            // Update the beginning of computation for t*[d,e]+modulus*[md,me] now md,me are known.
            cd += (Int128)modinfo.modulus.v0 * md;
            ce += (Int128)modinfo.modulus.v0 * me;
            // Verify that the low 62 bits of the computation are indeed zero, and then throw them away.
            Debug.Assert(((ulong)cd & M62) == 0); cd >>= 62;
            Debug.Assert(((ulong)ce & M62) == 0); ce >>= 62;
            // Compute limb 1 of t*[d,e]+modulus*[md,me], and store it as output limb 0 (= down shift).
            cd += ((Int128)t.u * d.v1) + ((Int128)t.v * e.v1);
            ce += ((Int128)t.q * d.v1) + ((Int128)t.r * e.v1);
            if (modinfo.modulus.v1 != 0)
            {
                // Optimize for the case where limb of modulus is zero.
                cd += (Int128)modinfo.modulus.v1 * md;
                ce += (Int128)modinfo.modulus.v1 * me;
            }
            long dv0 = (long)((ulong)cd & M62); cd >>= 62;
            long ev0 = (long)((ulong)ce & M62); ce >>= 62;
            /* Compute limb 2 of t*[d,e]+modulus*[md,me], and store it as output limb 1. */
            cd += ((Int128)t.u * d.v2) + ((Int128)t.v * e.v2);
            ce += ((Int128)t.q * d.v2) + ((Int128)t.r * e.v2);
            if (modinfo.modulus.v2 != 0)
            {
                // Optimize for the case where limb of modulus is zero.
                cd += (Int128)modinfo.modulus.v2 * md;
                ce += (Int128)modinfo.modulus.v2 * me;
            }
            long dv1 = (long)((ulong)cd & M62); cd >>= 62;
            long ev1 = (long)((ulong)ce & M62); ce >>= 62;
            // Compute limb 3 of t*[d,e]+modulus*[md,me], and store it as output limb 2.
            cd += ((Int128)t.u * d.v3) + ((Int128)t.v * e.v3);
            ce += ((Int128)t.q * d.v3) + ((Int128)t.r * e.v3);
            if (modinfo.modulus.v3 != 0)
            {
                // Optimize for the case where limb of modulus is zero.
                cd += (Int128)modinfo.modulus.v3 * md;
                ce += (Int128)modinfo.modulus.v3 * me;
            }
            long dv2 = (long)((ulong)cd & M62); cd >>= 62;
            long ev2 = (long)((ulong)ce & M62); ce >>= 62;
            // Compute limb 4 of t*[d,e]+modulus*[md,me], and store it as output limb 3.
            cd += ((Int128)t.u * d.v4) + ((Int128)t.v * e.v4);
            ce += ((Int128)t.q * d.v4) + ((Int128)t.r * e.v4);
            cd += (Int128)modinfo.modulus.v4 * md;
            ce += (Int128)modinfo.modulus.v4 * me;
            long dv3 = (long)((ulong)cd & M62); cd >>= 62;
            long ev3 = (long)((ulong)ce & M62); ce >>= 62;
            // What remains is limb 5 of t*[d,e]+modulus*[md,me]; store it as output limb 4.
            long dv4 = (long)cd;
            long ev4 = (long)ce;

            d = new ModInv64Signed62(dv0, dv1, dv2, dv3, dv4);
            e = new ModInv64Signed62(ev0, ev1, ev2, ev3, ev4);
#if DEBUG
            Debug.Assert(MulCmp62(d, 5, modinfo.modulus, -2) > 0); // d > -2*modulus
            Debug.Assert(MulCmp62(d, 5, modinfo.modulus, 1) < 0);  // d <    modulus
            Debug.Assert(MulCmp62(e, 5, modinfo.modulus, -2) > 0); // e > -2*modulus
            Debug.Assert(MulCmp62(e, 5, modinfo.modulus, 1) < 0);  // e <    modulus
#endif
        }


        // Compute (t/2^62) * [f, g], where t is a transition matrix scaled by 2^62.
        //
        // This implements the update_fg function from the explanation.
        private static void UpdateFG62(ref ModInv64Signed62 f, ref ModInv64Signed62 g, in ModInv64Trans2x2 t)
        {
            const ulong M62 = ulong.MaxValue >> 2;
            Int128 cf, cg;
            // Start computing t*[f,g].
            cf = ((Int128)t.u * f.v0) + ((Int128)t.v * g.v0);
            cg = ((Int128)t.q * f.v0) + ((Int128)t.r * g.v0);
            // Verify that the bottom 62 bits of the result are zero, and then throw them away.
            Debug.Assert(((ulong)cf & M62) == 0); cf >>= 62;
            Debug.Assert(((ulong)cg & M62) == 0); cg >>= 62;
            // Compute limb 1 of t*[f,g], and store it as output limb 0 (= down shift).
            cf += ((Int128)t.u * f.v1) + ((Int128)t.v * g.v1);
            cg += ((Int128)t.q * f.v1) + ((Int128)t.r * g.v1);
            long fv0 = (long)((ulong)cf & M62); cf >>= 62;
            long gv0 = (long)((ulong)cg & M62); cg >>= 62;
            // Compute limb 2 of t*[f,g], and store it as output limb 1.
            cf += ((Int128)t.u * f.v2) + ((Int128)t.v * g.v2);
            cg += ((Int128)t.q * f.v2) + ((Int128)t.r * g.v2);
            long fv1 = (long)((ulong)cf & M62); cf >>= 62;
            long gv1 = (long)((ulong)cg & M62); cg >>= 62;
            // Compute limb 3 of t*[f,g], and store it as output limb 2.
            cf += ((Int128)t.u * f.v3) + ((Int128)t.v * g.v3);
            cg += ((Int128)t.q * f.v3) + ((Int128)t.r * g.v3);
            long fv2 = (long)((ulong)cf & M62); cf >>= 62;
            long gv2 = (long)((ulong)cg & M62); cg >>= 62;
            /* Compute limb 4 of t*[f,g], and store it as output limb 3. */
            cf += ((Int128)t.u * f.v4) + ((Int128)t.v * g.v4);
            cg += ((Int128)t.q * f.v4) + ((Int128)t.r * g.v4);
            long fv3 = (long)((ulong)cf & M62); cf >>= 62;
            long gv3 = (long)((ulong)cg & M62); cg >>= 62;
            // What remains is limb 5 of t*[f,g]; store it as output limb 4.
            long fv4 = (long)cf;
            long gv4 = (long)cg;

            f = new ModInv64Signed62(fv0, fv1, fv2, fv3, fv4);
            g = new ModInv64Signed62(gv0, gv1, gv2, gv3, gv4);
        }

        // Compute (t/2^62) * [f, g], where t is a transition matrix for 62 divsteps.
        //
        // Version that operates on a variable number of limbs in f and g.
        //
        // This implements the update_fg function from the explanation.
        private static void UpdateFG62Var(int len, ref ModInv64Signed62 f, ref ModInv64Signed62 g, in ModInv64Trans2x2 t)
        {
            const ulong M62 = ulong.MaxValue >> 2;
            long fi, gi;
            Int128 cf, cg;
            Debug.Assert(len > 0);
            // Start computing t*[f,g].
            cf = ((Int128)t.u * f.v0) + ((Int128)t.v * g.v0);
            cg = ((Int128)t.q * f.v0) + ((Int128)t.r * g.v0);
            // Verify that the bottom 62 bits of the result are zero, and then throw them away.
            Debug.Assert(((ulong)cf & M62) == 0); cf >>= 62;
            Debug.Assert(((ulong)cg & M62) == 0); cg >>= 62;
            // Now iteratively compute limb i=1..len of t*[f,g], and store them in output limb i-1 (shifting
            // down by 62 bits).
            long[] fv = f.GetArray();
            long[] gv = g.GetArray();
            for (int i = 1; i < len; i++)
            {
                fi = fv[i];
                gi = gv[i];
                cf += ((Int128)t.u * fi) + ((Int128)t.v * gi);
                cg += ((Int128)t.q * fi) + ((Int128)t.r * gi);
                fv[i - 1] = (long)((ulong)cf & M62); cf >>= 62;
                gv[i - 1] = (long)((ulong)cg & M62); cg >>= 62;
            }
            // What remains is limb (len) of t*[f,g]; store it as output limb (len-1).
            fv[len - 1] = (long)cf;
            gv[len - 1] = (long)cg;

            f = new ModInv64Signed62(fv);
            g = new ModInv64Signed62(gv);
        }


        /// <summary>
        /// Replace x with its modular inverse mod modinfo->modulus. x must be in range [0, modulus).
        /// If x is zero, the result will be zero as well. If not, the inverse must exist(i.e., the gcd of
        /// x and modulus must be 1). These rules are automatically satisfied if the modulus is prime.
        ///
        /// On output, all of x's limbs will be in [0, 2^62).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="modinfo"></param>
        public static void Compute(ref ModInv64Signed62 x, in ModInv64ModInfo modinfo)
        {
            // Start with d=0, e=1, f=modulus, g=x, zeta=-1
            ModInv64Signed62 d = ModInv64Signed62.Zero;
            ModInv64Signed62 e = ModInv64Signed62.One;
            ModInv64Signed62 f = modinfo.modulus;
            ModInv64Signed62 g = x;
            long zeta = -1; // zeta = -(delta+1/2); delta starts at 1/2

            // Do 10 iterations of 59 divsteps each = 590 divsteps. This suffices for 256-bit inputs.
            for (int i = 0; i < 10; i++)
            {
                // Compute transition matrix and new zeta after 59 divsteps.
                zeta = DivSteps59(zeta, (ulong)f.v0, (ulong)g.v0, out ModInv64Trans2x2 t);
                // Update d,e using that transition matrix.
                UpdateDE62(ref d, ref e, t, modinfo);
                // Update f,g using that transition matrix.
#if DEBUG
                Debug.Assert(MulCmp62(f, 5, modinfo.modulus, -1) > 0); // f > -modulus
                Debug.Assert(MulCmp62(f, 5, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp62(g, 5, modinfo.modulus, -1) > 0); // g > -modulus
                Debug.Assert(MulCmp62(g, 5, modinfo.modulus, 1) < 0);  // g <  modulus
#endif
                UpdateFG62(ref f, ref g, t);
#if DEBUG
                Debug.Assert(MulCmp62(f, 5, modinfo.modulus, -1) > 0); // f > -modulus
                Debug.Assert(MulCmp62(f, 5, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp62(g, 5, modinfo.modulus, -1) > 0); // g > -modulus
                Debug.Assert(MulCmp62(g, 5, modinfo.modulus, 1) < 0);  // g <  modulus
#endif
            }

            // At this point sufficient iterations have been performed that g must have reached 0
            // and (if g was not originally 0) f must now equal +/- GCD of the initial f, g
            // values i.e. +/- 1, and d now contains +/- the modular inverse.
#if DEBUG
            // g == 0
            Debug.Assert(MulCmp62(g, 5, ModInv64Signed62.One, 0) == 0);
            // |f| == 1, or (x == 0 and d == 0 and f == modulus)
            Debug.Assert(MulCmp62(f, 5, ModInv64Signed62.One, -1) == 0 ||
                         MulCmp62(f, 5, ModInv64Signed62.One, 1) == 0 ||
                         (MulCmp62(x, 5, ModInv64Signed62.One, 0) == 0 &&
                          MulCmp62(d, 5, ModInv64Signed62.One, 0) == 0 &&
                          MulCmp62(f, 5, modinfo.modulus, 1) == 0));
#endif

            // Optionally negate d, normalize to [0,modulus), and return it.
            x = Normalize62(d, f.v4, modinfo);
        }

        /// <summary>
        /// Replace x with its modular inverse mod modinfo->modulus. x must be in range [0, modulus).
        /// If x is zero, the result will be zero as well. If not, the inverse must exist(i.e., the gcd of
        /// x and modulus must be 1). These rules are automatically satisfied if the modulus is prime.
        ///
        /// On output, all of x's limbs will be in [0, 2^62).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="modinfo"></param>
        public static void ComputeVar(ref ModInv64Signed62 x, in ModInv64ModInfo modinfo)
        {
            // Start with d=0, e=1, f=modulus, g=x, eta=-1.
            ModInv64Signed62 d = ModInv64Signed62.Zero;
            ModInv64Signed62 e = ModInv64Signed62.One;
            ModInv64Signed62 f = modinfo.modulus;
            ModInv64Signed62 g = x;
#if DEBUG
            int i = 0;
#endif
            int j, len = 5;
            long eta = -1; // eta = -delta; delta is initially 1
            long cond, fn, gn;

            // Do iterations of 62 divsteps each until g=0.
            while (true)
            {
                // Compute transition matrix and new eta after 62 divsteps.
                eta = DivSteps62Var(eta, (ulong)f.v0, (ulong)g.v0, out ModInv64Trans2x2 t);
                // Update d,e using that transition matrix.
                UpdateDE62(ref d, ref e, t, modinfo);
                // Update f,g using that transition matrix.
#if DEBUG
                Debug.Assert(MulCmp62(f, len, modinfo.modulus, -1) > 0); // f > -modulus
                Debug.Assert(MulCmp62(f, len, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp62(g, len, modinfo.modulus, -1) > 0); // g > -modulus
                Debug.Assert(MulCmp62(g, len, modinfo.modulus, 1) < 0);  // g <  modulus
#endif
                UpdateFG62Var(len, ref f, ref g, t);

                // If the bottom limb of g is zero, there is a chance that g=0.
                long[] fv = f.GetArray();
                long[] gv = g.GetArray();
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
                cond = ((long)len - 2) >> 63;
                cond |= fn ^ (fn >> 63);
                cond |= gn ^ (gn >> 63);
                // If so, reduce length, propagating the sign of f and g's top limb into the one below.
                if (cond == 0)
                {
                    fv[len - 2] |= fn << 62;
                    gv[len - 2] |= gn << 62;
                    len--;
                }

                f = new ModInv64Signed62(fv);
                g = new ModInv64Signed62(gv);
#if DEBUG
                Debug.Assert(++i < 12); // We should never need more than 12*62 = 744 divsteps
                Debug.Assert(MulCmp62(f, len, modinfo.modulus, -1) > 0); // f > -modulus
                Debug.Assert(MulCmp62(f, len, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp62(g, len, modinfo.modulus, -1) > 0); // g > -modulus
                Debug.Assert(MulCmp62(g, len, modinfo.modulus, 1) < 0);  // g <  modulus
#endif
            }

            // At this point g is 0 and (if g was not originally 0) f must now equal +/- GCD of
            // the initial f, g values i.e. +/- 1, and d now contains +/- the modular inverse.
#if DEBUG
            // g == 0
            Debug.Assert(MulCmp62(g, len, ModInv64Signed62.One, 0) == 0);
            // |f| == 1, or (x == 0 and d == 0 and f == modulus)
            Debug.Assert(MulCmp62(f, len, ModInv64Signed62.One, -1) == 0 ||
                         MulCmp62(f, len, ModInv64Signed62.One, 1) == 0 ||
                         (MulCmp62(x, 5, ModInv64Signed62.One, 0) == 0 &&
                          MulCmp62(d, 5, ModInv64Signed62.One, 0) == 0 &&
                          MulCmp62(f, len, modinfo.modulus, 1) == 0));
#endif

            // Optionally negate d, normalize to [0,modulus), and return it.
            long[] tempArr = f.GetArray();
            x = Normalize62(d, tempArr[len - 1], modinfo);
        }


        /// <summary>
        /// Compute the Jacobi symbol for (x | modinfo->modulus). x must be coprime with modulus (and thus
        /// cannot be 0, as modulus >= 3). All limbs of x must be non-negative. Returns 0 if the result
        /// cannot be computed.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="modinfo"></param>
        /// <returns></returns>
        public static int Jacobi64MaybeVar(in ModInv64Signed62 x, in ModInv64ModInfo modinfo)
        {
            // Start with f=modulus, g=x, eta=-1.
            ModInv64Signed62 f = modinfo.modulus;
            ModInv64Signed62 g = x;
            int len = 5;
            // eta = -delta; delta is initially 1
            long eta = -1;
            long cond, fn, gn;
            int jac = 0;

            // The input limbs must all be non-negative.
            Debug.Assert(g.v0 >= 0 && g.v1 >= 0 && g.v0 >= 0 && g.v3 >= 0 && g.v4 >= 0);

            // If x > 0, then if the loop below converges, it converges to f=g=gcd(x,modulus). Since we
            // require that gcd(x,modulus)=1 and modulus>=3, x cannot be 0. Thus, we must reach f=1 (or
            // time out).
            Debug.Assert((g.v0 | g.v1 | g.v2 | g.v3 | g.v4) != 0);

            const int JACOBI64_ITERATIONS =
#if DEBUG
                12;
#else
                25;
#endif
            for (int count = 0; count < JACOBI64_ITERATIONS; count++)
            {
                // Compute transition matrix and new eta after 62 posdivsteps.
                eta = PosDivSteps62Var(eta, (ulong)f.v0 | ((ulong)f.v1 << 62), (ulong)g.v0 | ((ulong)g.v1 << 62), out ModInv64Trans2x2 t, ref jac);
#if DEBUG
                // Update f,g using that transition matrix.
                Debug.Assert(MulCmp62(f, len, modinfo.modulus, 0) > 0); // f > 0
                Debug.Assert(MulCmp62(f, len, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp62(g, len, modinfo.modulus, 0) > 0); // g > 0
                Debug.Assert(MulCmp62(g, len, modinfo.modulus, 1) < 0);  // g < modulus
#endif
                UpdateFG62Var(len, ref f, ref g, t);

                // If the bottom limb of f is 1, there is a chance that f=1.
                long[] fv = f.GetArray();
                long[] gv = g.GetArray();
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
                cond = ((long)len - 2) >> 63;
                cond |= fn;
                cond |= gn;
                // If so, reduce length.
                if (cond == 0)
                {
                    len--;
                }
#if DEBUG
                Debug.Assert(MulCmp62(f, len, modinfo.modulus, 0) > 0); // f > 0
                Debug.Assert(MulCmp62(f, len, modinfo.modulus, 1) <= 0); // f <= modulus
                Debug.Assert(MulCmp62(g, len, modinfo.modulus, 0) > 0); // g > 0
                Debug.Assert(MulCmp62(g, len, modinfo.modulus, 1) < 0);  // g < modulus
#endif
            }

            // The loop failed to converge to f=g after 1500 iterations. Return 0, indicating unknown result.
            return 0;
        }
    }
}
