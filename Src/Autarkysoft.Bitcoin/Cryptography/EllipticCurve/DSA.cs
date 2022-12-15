// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    public class DSA
    {
        static DSA()
        {
            PrecomputeEcMult.BuildTables(out PreG, out PreG128);
        }


        private readonly Calc calc = new Calc();

        private static readonly PointStorage[] PreG;
        private static readonly PointStorage[] PreG128;

        private const int WindowA = 5;
        private const int ECMULT_WINDOW_SIZE = 15;
        private const int WINDOW_G = ECMULT_WINDOW_SIZE;
        // 1 << (w - 2) where w = WINDOW_A = 5
        private const int TableSizeWindowA = 8;

        // The number of entries a table with precomputed multiples needs to have.
        private static int ECMULT_TABLE_SIZE(int w) => 1 << (w - 2);

        // Convert a number to WNAF notation. The number becomes represented by sum(2^i * wnaf[i], i=0..bits),
        // with the following guarantees:
        // - each wnaf[i] is either 0, or an odd integer between -(1<<(w-1) - 1) and (1<<(w-1) - 1)
        // - two non-zero entries in wnaf are separated by at least w-1 zeroes.
        // - the number of set values in wnaf is returned. This number is at most 256, and at most one more
        //   than the number of bits in the (absolute value) of the input.
        private static unsafe int EcmultWnaf(int[] wnaf, int len, ref Scalar8x32 s, in int w)
        {
            int lastSetBit = -1;
            int bit = 0;
            int sign = 1;
            int carry = 0;

            // TODO: remove "len"?
            Debug.Assert(wnaf.Length == len);
            Debug.Assert(wnaf != null);
            Debug.Assert(0 <= len && len <= 256);
            Debug.Assert(s != null);
            Debug.Assert(2 <= w && w <= 31);

            if (((s.b7 >> 31) & 1) != 0)
            {
                s = s.Negate();
                sign = -1;
            }

            uint* pt = stackalloc uint[8] { s.b0, s.b1, s.b2, s.b3, s.b4, s.b5, s.b6, s.b7 };
            while (bit < len)
            {
                if (Scalar8x32.GetBits(pt, bit, 1) == (uint)carry)
                {
                    bit++;
                    continue;
                }

                int now = w;
                if (now > len - bit)
                {
                    now = len - bit;
                }

                int word = (int)Scalar8x32.GetBitsVar(pt, bit, now) + carry;

                carry = (word >> (w - 1)) & 1;
                word -= carry << w;

                wnaf[bit] = sign * word;
                lastSetBit = bit;

                bit += now;
            }
#if DEBUG
            Debug.Assert(carry == 0);
            while (bit < 256)
            {
                Debug.Assert(Scalar8x32.GetBits(pt, bit++, 1) == 0);
            }
#endif
            return lastSetBit + 1;
        }


        // Fill a table 'pre_a' with precomputed odd multiples of a.
        // pre_a will contain [1*a,3*a,...,(2*n-1)*a], so it needs space for n group elements.
        // zr needs space for n field elements.
        //
        // Although pre_a is an array of _ge rather than _gej, it actually represents elements
        // in Jacobian coordinates with their z coordinates omitted. The omitted z-coordinates
        // can be recovered using z and zr. Using the notation z(b) to represent the omitted
        // z coordinate of b:
        // - z(pre_a[n-1]) = 'z'
        // - z(pre_a[i-1]) = z(pre_a[i]) / zr[i] for n > i > 0
        //
        // Lastly the zr[0] value, which isn't used above, is set so that:
        // - a.z = z(pre_a[0]) / zr[0]
        private static void OddMultiplesTable(int n, Point[] pre_a, UInt256_10x26[] zr, int index, ref UInt256_10x26 z, in PointJacobian a)
        {
            Debug.Assert(!a.isInfinity);

            PointJacobian d = a.DoubleVar(out _);

            // Perform the additions using an isomorphic curve Y^2 = X^3 + 7*C^6 where C := d.z.
            // The isomorphism, phi, maps a secp256k1 point (x, y) to the point (x*C^2, y*C^3) on the other curve.
            // In Jacobian coordinates phi maps (x, y, z) to (x*C^2, y*C^3, z) or, equivalently to (x, y, z/C).
            //
            //     phi(x, y, z) = (x*C^2, y*C^3, z) = (x, y, z/C)
            //   d_ge := phi(d) = (d.x, d.y, 1)
            //     ai := phi(a) = (a.x*C^2, a.y*C^3, a.z)
            //
            // The group addition functions work correctly on these isomorphic curves.
            // In particular phi(d) is easy to represent in affine coordinates under this isomorphism.
            // This lets us use the faster secp256k1_gej_add_ge_var group addition function
            // that we wouldn't be able to use otherwise.
            Point d_ge = new Point(d.x, d.y);
            pre_a[index] = a.ToPointZInv(d.z);

            // TODO: this 2 lines can be simplified
            PointJacobian ai = pre_a[index].ToPointJacobian();
            ai = new PointJacobian(ai.x, ai.y, a.z);

            // pre_a[0] is the point (a.x*C^2, a.y*C^3, a.z*C) which is equvalent to a.
            // Set zr[0] to C, which is the ratio between the omitted z(pre_a[0]) value and a.z.
            zr[index] = d.z;

            for (int i = 1; i < n; i++)
            {
                ai = ai.AddVar(d_ge, out zr[index + i]);
                pre_a[index + i] = new Point(ai.x, ai.y);
            }

            // Multiply the last z-coordinate by C to undo the isomorphism.
            // Since the z-coordinates of the pre_a values are implied by the zr array of z-coordinate ratios,
            // undoing the isomorphism here undoes the isomorphism for all pre_a values.
            z = ai.z.Multiply(d.z);
        }

#if DEBUG
        private static void VerifyTable(int n, int w)
        {
            Debug.Assert(((n) & 1) == 1);
            Debug.Assert(n >= -((1 << (w - 1)) - 1));
            Debug.Assert(n <= ((1 << (w - 1)) - 1));
        }
#endif

        private static Point TableGetPoint(Point[] pre, int index, int n, int w)
        {
#if DEBUG
            VerifyTable(n, w);
#endif
            if (n > 0)
            {
                return pre[index + ((n - 1) / 2)];
            }
            else
            {
                Point r = pre[index + ((-n - 1) / 2)];
                return new Point(r.x, r.y.Negate(1));
            }
        }

        private static Point TableGetPointLambda(Point[] pre, UInt256_10x26[] x, int index, int n, int w)
        {
#if DEBUG
            VerifyTable(n, w);
#endif
            if (n > 0)
            {
                return new Point(x[index + ((n - 1) / 2)], pre[index + ((n - 1) / 2)].y, false);
            }
            else
            {
                return new Point(x[index + ((-n - 1) / 2)], pre[index + ((-n - 1) / 2)].y.Negate(1));
            }
        }

        private static Point TableGetPointStorage(PointStorage[] pre, int n, int w)
        {
#if DEBUG
            VerifyTable(n, w);
#endif
            if (n > 0)
            {
                return pre[(n - 1) / 2].ToPoint();
            }
            else
            {
                Point r = pre[(-n - 1) / 2].ToPoint();
                return new Point(r.x, r.y.Negate(1), r.isInfinity);
            }
        }


        private static PointJacobian StraussWnaf(StraussState state, PointJacobian[] a, Scalar8x32[] na, in Scalar8x32 ng)
        {
            Debug.Assert(a.Length > 0);
            Debug.Assert(a.Length == na.Length);

            // Split G factors
            int[] wnafNg1 = new int[129];
            int[] wnafNg128 = new int[129];
            int bitsNg1 = 0;
            int bitsNg128 = 0;
            int bits = 0;
            int no = 0;

            // NOTE: num (ie. a.Length), np and no are using size_t type which is at least 16 bits.

            UInt256_10x26 Z = UInt256_10x26.One;
            for (int np = 0; np < a.Length; np++)
            {
                if (na[np].IsZero || a[np].isInfinity)
                {
                    continue;
                }

                // Split na into na_1 and na_lam (where na = na_1 + na_lam*lambda, and na_1 and na_lam are ~128 bit)
                Scalar8x32.SplitLambda(out Scalar8x32 na1, out Scalar8x32 naLam, na[np]);

                // Build wnaf representation for na_1 and na_lam
                state.ps[no].bitsNa1 = EcmultWnaf(state.ps[no].wnafNa1, 129, ref na1, WindowA);
                state.ps[no].bitsNaLam = EcmultWnaf(state.ps[no].wnafNaLam, 129, ref naLam, WindowA);
                Debug.Assert(state.ps[no].bitsNa1 <= 129);
                Debug.Assert(state.ps[no].bitsNaLam <= 129);
                if (state.ps[no].bitsNa1 > bits)
                {
                    bits = state.ps[no].bitsNa1;
                }
                if (state.ps[no].bitsNaLam > bits)
                {
                    bits = state.ps[no].bitsNaLam;
                }

                // Calculate odd multiples of a.
                // All multiples are brought to the same Z 'denominator', which is stored
                // in Z. Due to secp256k1' isomorphism we can do all operations pretending
                // that the Z coordinate was 1, use affine addition formulae, and correct
                // the Z coordinate of the result once at the end.
                // The exception is the precomputed G table points, which are actually
                // affine. Compared to the base used for other points, they have a Z ratio
                // of 1/Z, so we can use secp256k1_gej_add_zinv_var, which uses the same
                // isomorphism to efficiently add with a known Z inverse.
                PointJacobian tmp = a[np];
                if (no != 0)
                {
#if DEBUG
                    Z = Z.NormalizeVar();
#endif
                    Z.Rescale(ref tmp);
                }

                int index = no * TableSizeWindowA;
                OddMultiplesTable(TableSizeWindowA, state.preA, state.aux, index, ref Z, tmp);
                if (no != 0)
                {
                    state.aux[index] = state.aux[index] * a[np].z;
                }

                no++;
            }

            // Bring them to the same Z denominator.
            Point.SetGlobalZ(TableSizeWindowA * no, state.preA, state.aux);

            for (int np = 0; np < no; np++)
            {
                for (int i = 0; i < TableSizeWindowA; i++)
                {
                    state.aux[np * TableSizeWindowA + i] = state.preA[np * TableSizeWindowA + i].x * UInt256_10x26.Beta;
                }
            }

            if (!ng.IsZero)
            {
                // split ng into ng_1 and ng_128 (where gn = gn_1 + gn_128*2^128, and gn_1 and gn_128 are ~128 bit)
                Scalar8x32.Split128(ng, out Scalar8x32 ng_1, out Scalar8x32 ng_128);

                // Build wnaf representation for ng_1 and ng_128
                bitsNg1 = EcmultWnaf(wnafNg1, 129, ref ng_1, WINDOW_G);
                bitsNg128 = EcmultWnaf(wnafNg128, 129, ref ng_128, WINDOW_G);
                if (bitsNg1 > bits)
                {
                    bits = bitsNg1;
                }
                if (bitsNg128 > bits)
                {
                    bits = bitsNg128;
                }
            }

            PointJacobian r = PointJacobian.Infinity;

            for (int i = bits - 1; i >= 0; i--)
            {
                int n;
                r = r.DoubleVar(out _);
                Point tmpa;
                for (int np = 0; np < no; np++)
                {
                    n = state.ps[np].wnafNa1[i];
                    if (i < state.ps[np].bitsNa1 && (n != 0))
                    {
                        tmpa = TableGetPoint(state.preA, np * TableSizeWindowA, n, WindowA);
                        r = r.AddVar(tmpa, out _);
                    }

                    n = state.ps[np].wnafNaLam[i];
                    if (i < state.ps[np].bitsNaLam && (n != 0))
                    {
                        tmpa = TableGetPointLambda(state.preA, state.aux, np * TableSizeWindowA, n, WindowA);
                        r = r.AddVar(tmpa, out _);
                    }
                }

                n = wnafNg1[i];
                if (i < bitsNg1 && (n != 0))
                {
                    tmpa = TableGetPointStorage(PreG, n, WINDOW_G);
                    r = r.AddZInvVar(tmpa, Z);
                }

                n = wnafNg128[i];
                if (i < bitsNg128 && (n != 0))
                {
                    tmpa = TableGetPointStorage(PreG128, n, WINDOW_G);
                    r = r.AddZInvVar(tmpa, Z);
                }
            }

            if (!r.isInfinity)
            {
                r = new PointJacobian(r.x, r.y, r.z * Z);
            }

            return r;
        }

        private PointJacobian ECMult(in PointJacobian a, in Scalar8x32 na, in Scalar8x32 ng)
        {
            StraussState state = new StraussState(TableSizeWindowA);
            return StraussWnaf(state, new PointJacobian[] { a }, new Scalar8x32[] { na }, ng);
        }

        private PointJacobian Multiply(in Scalar8x32 k, in Point point)
        {
            PointJacobian result = PointJacobian.Infinity;
            PointJacobian addend = point.ToPointJacobian();

            Span<uint> temp = new uint[] { k.b0, k.b1, k.b2, k.b3, k.b4, k.b5, k.b6, k.b7 };
            while (temp[^1] == 0)
            {
                temp = temp.Slice(0, temp.Length - 1);
            }
            for (int i = 0; i < temp.Length - 1; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if ((temp[i] & 1) == 1)
                    {
                        result = result.AddVar(addend, out _);
                    }
                    addend = addend.AddVar(addend, out _);
                    temp[i] >>= 1;
                }
            }

            while (temp[^1] != 0)
            {
                if ((temp[^1] & 1) == 1)
                {
                    result = result.AddVar(addend, out _);
                }
                addend = addend.AddVar(addend, out _);
                temp[^1] >>= 1;
            }

            return result;
        }

        public bool VerifySimple(Signature sig, in Point pubkey, in Scalar8x32 hash, bool lowS)
        {
            // Note that Scalar (r and s) is always < N so there is no need to check and reject r/s >= N here
            if (sig.R.IsZero || sig.S.IsZero)
            {
                return false;
            }

            if (sig.S.IsHigh)
            {
                if (lowS)
                {
                    return false;
                }
                else
                {
                    sig.S = sig.S.Negate();
                }
            }
            Debug.Assert(!sig.S.IsHigh);

            Scalar8x32 invMod = sig.S.InverseVar_old();
            Scalar8x32 u1 = hash.Multiply(invMod);
            Scalar8x32 u2 = sig.R.Multiply(invMod);

            Point Rxy = calc.MultiplyByG(u1).AddVar(Multiply(u2, pubkey), out _).ToPoint();

            if (Rxy.x.IsZeroNormalizedVar() && Rxy.y.IsZeroNormalizedVar())
            {
                return false;
            }

            UInt256_10x26 temp = new UInt256_10x26(sig.R.b0, sig.R.b1, sig.R.b2, sig.R.b3, sig.R.b4, sig.R.b5, sig.R.b6, sig.R.b7);
            return Rxy.x.EqualsVar(temp);
        }


        public bool Verify(Signature sig, in Point pubkey, in Scalar8x32 hash, bool lowS)
        {
            if (sig.R.IsZero || sig.S.IsZero)
            {
                return false;
            }

            if (sig.S.IsHigh)
            {
                if (lowS)
                {
                    return false;
                }
                else
                {
                    sig.S = sig.S.Negate();
                }
            }

            Scalar8x32 sn = sig.S.InverseVar_old();
            Scalar8x32 u1 = sn.Multiply(hash);
            Scalar8x32 u2 = sn.Multiply(sig.R);
            PointJacobian pubkeyj = pubkey.ToPointJacobian();
            PointJacobian pr = ECMult(pubkeyj, u2, u1);
            if (pr.isInfinity)
            {
                return false;
            }
            byte[] c = sig.R.ToByteArray();
            UInt256_10x26 xr = new UInt256_10x26(c, out _);

            // We now have the recomputed R point in pr, and its claimed x coordinate (modulo n)
            //  in xr. Naively, we would extract the x coordinate from pr (requiring a inversion modulo p),
            //  compute the remainder modulo n, and compare it to xr. However:
            //
            //        xr == X(pr) mod n
            //    <=> exists h. (xr + h * n < p && xr + h * n == X(pr))
            //    [Since 2 * n > p, h can only be 0 or 1]
            //    <=> (xr == X(pr)) || (xr + n < p && xr + n == X(pr))
            //    [In Jacobian coordinates, X(pr) is pr.x / pr.z^2 mod p]
            //    <=> (xr == pr.x / pr.z^2 mod p) || (xr + n < p && xr + n == pr.x / pr.z^2 mod p)
            //    [Multiplying both sides of the equations by pr.z^2 mod p]
            //    <=> (xr * pr.z^2 mod p == pr.x) || (xr + n < p && (xr + n) * pr.z^2 mod p == pr.x)
            //
            //  Thus, we can avoid the inversion, but we have to check both cases separately.
            //  secp256k1_gej_eq_x implements the (xr * pr.z^2 mod p == pr.x) test.
            if (pr.EqualsXVar(xr))
            {
                // xr * pr.z^2 mod p == pr.x, so the signature is valid.
                return true;
            }
            if (xr.CompareToVar(UInt256_10x26.PMinusN) >= 0)
            {
                // xr + n >= p, so we can skip testing the second case.
                return false;
            }

            xr += UInt256_10x26.N;
            if (pr.EqualsXVar(xr))
            {
                // (xr + n) * pr.z^2 mod p == pr.x, so the signature is valid.
                return true;
            }
            return false;
        }
    }
}
