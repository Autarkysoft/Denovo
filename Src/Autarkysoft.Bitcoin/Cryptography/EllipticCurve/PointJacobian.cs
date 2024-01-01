// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// Elliptic curve point in Jacobian coordinates
    /// </summary>
    public readonly struct PointJacobian
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PointJacobian"/> using the given parameters.
        /// </summary>
        /// <param name="x26">x coordinate</param>
        /// <param name="y26">y coordinate</param>
        /// <param name="z26">z coordinate</param>
        public PointJacobian(in UInt256_10x26 x26, in UInt256_10x26 y26, in UInt256_10x26 z26) : this(x26, y26, z26, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PointJacobian"/> using the given parameters.
        /// </summary>
        /// <param name="x26">x coordinate</param>
        /// <param name="y26">y coordinate</param>
        /// <param name="z26">z coordinate</param>
        /// <param name="infinity">Is point at infinity</param>
        public PointJacobian(in UInt256_10x26 x26, in UInt256_10x26 y26, in UInt256_10x26 z26, bool infinity)
        {
            x = x26;
            y = y26;
            z = z26;
            isInfinity = infinity;
        }


        /// <summary>
        /// x coordinate (actual X=x/z^2)
        /// </summary>
        public readonly UInt256_10x26 x;
        /// <summary>
        /// y coordinate (actual Y=y/z^3)
        /// </summary>
        public readonly UInt256_10x26 y;
        /// <summary>
        /// z coordinate
        /// </summary>
        public readonly UInt256_10x26 z;
        /// <summary>
        /// True if this is the point at infinity; otherwise false.
        /// </summary>
        public readonly bool isInfinity;

#if DEBUG
        // Maximum allowed magnitudes for group element coordinates
        // SECP256K1_GEJ_{X/y/z}_MAGNITUDE_MAX
        // Any changes to these values should be reflected in the same hard-coded values in tests
        private const int MaxXMagnitude = 4;
        private const int MaxYMagnitude = 4;
        private const int MaxZMagnitude = 1;

        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        internal void Verify()
        {
            x.Verify();
            y.Verify();
            z.Verify();
            UInt256_10x26.VerifyMagnitude(x.magnitude, MaxXMagnitude);
            UInt256_10x26.VerifyMagnitude(y.magnitude, MaxYMagnitude);
            UInt256_10x26.VerifyMagnitude(z.magnitude, MaxZMagnitude);
        }
#endif

        private static readonly PointJacobian _infinity = new PointJacobian(UInt256_10x26.Zero, UInt256_10x26.Zero, UInt256_10x26.Zero, true);
        /// <summary>
        /// Point at infinity
        /// </summary>
        public static ref readonly PointJacobian Infinity => ref _infinity;


        /// <summary>
        /// Adds the given point in jacobian coordinate to this instance.
        /// </summary>
        /// <param name="b">Point in jacobian coordinate</param>
        /// <param name="rzr">sets rzr such that r.z == a.z * rzr (a cannot be infinity in that case)</param>
        /// <returns>Sum of two points</returns>
        public PointJacobian AddVar(in PointJacobian b, out UInt256_10x26 rzr)
        {
            // secp256k1_gej_add_var
#if DEBUG
            Verify();
            b.Verify();
#endif
            if (isInfinity)
            {
                rzr = UInt256_10x26.Zero;
                return b;
            }
            if (b.isInfinity)
            {
                rzr = UInt256_10x26.One;
                return this;
            }

            // 12 mul, 4 sqr, 11 add/negate/normalizes_to_zero (ignoring special cases)
            UInt256_10x26 z22 = b.z.Sqr();
            UInt256_10x26 z12 = z.Sqr();
            UInt256_10x26 u1 = x * z22;
            UInt256_10x26 u2 = b.x * z12;
            UInt256_10x26 s1 = y * z22 * b.z;
            UInt256_10x26 s2 = b.y * z12 * z;
            UInt256_10x26 h = u1.Negate(1) + u2;
            UInt256_10x26 i = s2.Negate(1) + s1;

            if (h.IsZeroNormalizedVar())
            {
                if (i.IsZeroNormalizedVar())
                {
                    return DoubleVar(out rzr);
                }
                else
                {
                    rzr = UInt256_10x26.Zero;
                    return Infinity;
                }
            }

            UInt256_10x26 t = h * b.z;
            rzr = t;

            UInt256_10x26 rz = z * t;

            UInt256_10x26 h2 = h.Sqr().Negate(1);
            UInt256_10x26 h3 = h2 * h;
            t = u1 * h2;

            UInt256_10x26 rx = UInt256_10x26.Add(i.Sqr(), h3, t, t);

            t += rx;
            UInt256_10x26 ry = (t * i) + (h3 * s1);

            PointJacobian res = new PointJacobian(rx, ry, rz, false);
#if DEBUG
            res.Verify();
#endif
            return res;
        }


        /// <summary>
        /// Computes sum of the two given points
        /// </summary>
        /// <remarks>
        /// This method is constant-time
        /// </remarks>
        /// <param name="b">Point in affine coordinate (not infinity)</param>
        /// <returns></returns>
        public PointJacobian Add(in Point b)
        {
            // secp256k1_gej_add_ge
#if DEBUG
            Verify();
            b.Verify();
            Debug.Assert(!b.isInfinity);
#endif
            /* In:
             *    Eric Brier and Marc Joye, Weierstrass Elliptic Curves and Side-Channel Attacks.
             *    In D. Naccache and P. Paillier, Eds., Public Key Cryptography, vol. 2274 of Lecture Notes in Computer Science,
             *    pages 335-345. Springer-Verlag, 2002.
             *  we find as solution for a unified addition/doubling formula:
             *    lambda = ((x1 + x2)^2 - x1 * x2 + a) / (y1 + y2), with a = 0 for secp256k1's curve equation.
             *    x3 = lambda^2 - (x1 + x2)
             *    2*y3 = lambda * (x1 + x2 - 2 * x3) - (y1 + y2).
             *
             *  Substituting x_i = Xi / Zi^2 and yi = Yi / Zi^3, for i=1,2,3, gives:
             *    U1 = X1*Z2^2, U2 = X2*Z1^2
             *    S1 = Y1*Z2^3, S2 = Y2*Z1^3
             *    Z = Z1*Z2
             *    T = U1+U2
             *    M = S1+S2
             *    Q = -T*M^2
             *    R = T^2-U1*U2
             *    X3 = R^2+Q
             *    Y3 = -(R*(2*X3+Q)+M^4)/2
             *    Z3 = M*Z
             *  (Note that the paper uses xi = Xi / Zi and yi = Yi / Zi instead.)
             *
             *  This formula has the benefit of being the same for both addition
             *  of distinct points and doubling. However, it breaks down in the
             *  case that either point is infinity, or that y1 = -y2. We handle
             *  these cases in the following ways:
             *
             *    - If b is infinity we simply bail by means of a VERIFY_CHECK.
             *
             *    - If a is infinity, we detect this, and at the end of the
             *      computation replace the result (which will be meaningless,
             *      but we compute to be constant-time) with b.x : b.y : 1.
             *
             *    - If a = -b, we have y1 = -y2, which is a degenerate case.
             *      But here the answer is infinity, so we simply set the
             *      infinity flag of the result, overriding the computed values
             *      without even needing to cmov.
             *
             *    - If y1 = -y2 but x1 != x2, which does occur thanks to certain
             *      properties of our curve (specifically, 1 has nontrivial cube
             *      roots in our field, and the curve equation has no x coefficient)
             *      then the answer is not infinity but also not given by the above
             *      equation. In this case, we cmov in place an alternate expression
             *      for lambda. Specifically (y1 - y2)/(x1 - x2). Where both these
             *      expressions for lambda are defined, they are equal, and can be
             *      obtained from each other by multiplication by (y1 + y2)/(y1 + y2)
             *      then substitution of x^3 + 7 for y^2 (using the curve equation).
             *      For all pairs of nonzero points (a, b) at least one is defined,
             *      so this covers everything.
             */

            // Operations: 7 mul, 5 sqr, 24 add/cmov/half/mul_int/negate/normalize_weak/normalizes_to_zero
            UInt256_10x26 zz, u1, u2, s1, s2, t, tt, m, n, q, rr;
            UInt256_10x26 m_alt, rr_alt;

            zz = z.Sqr();               // z = Z1^2
            u1 = x.NormalizeWeak();     // u1 = U1 = X1*Z2^2 (1)
            u2 = b.x * zz;              // u2 = U2 = X2*Z1^2 (1)
            s1 = y.NormalizeWeak();     // s1 = S1 = Y1*Z2^3 (1)
            s2 = b.y * zz;              // s2 = Y2*Z1^2 (1)
            s2 *= z;                    // s2 = S2 = Y2*Z1^3 (1)
            t = u1 + u2;                // t = T = U1+U2 (2)
            m = s1 + s2;                // m = M = S1+S2 (2)
            rr = t.Sqr();               // rr = T^2 (1)
            m_alt = u2.Negate(1);       // Malt = -X2*Z1^2
            tt = u1 * m_alt;            // tt = -U1*U2 (2)
            rr += tt;                   // rr = R = T^2-U1*U2 (3)
            // If lambda = R/M = 0/0 we have a problem (except in the "trivial"
            // case that Z = z1z2 = 0, and this is special-cased later on).
            uint degenerate = m.IsZeroNormalized() & rr.IsZeroNormalized() ? 1U : 0U;
            // This only occurs when y1 == -y2 and x1^3 == x2^3, but x1 != x2.
            // This means either x1 == beta*x2 or beta*x1 == x2, where beta is
            // a nontrivial cube root of one. In either case, an alternate
            // non-indeterminate expression for lambda is (y1 - y2)/(x1 - x2),
            // so we set R/M equal to this.
            rr_alt = s1 * 2U;               // rr = Y1*Z2^3 - Y2*Z1^3 (2)
            m_alt += u1;                    // Malt = X1*Z2^2 - X2*Z1^2

            uint flag = ~degenerate & 1;
            Debug.Assert(flag == (degenerate != 0 ? 0 : 1));
            rr_alt = UInt256_10x26.CMov(rr_alt, rr, flag);
            m_alt = UInt256_10x26.CMov(m_alt, m, flag);
            // Now Ralt / Malt = lambda and is guaranteed not to be 0/0.
            // From here on out Ralt and Malt represent the numerator
            // and denominator of lambda; R and M represent the explicit
            // expressions x1^2 + x2^2 + x1x2 and y1 + y2.
            n = m_alt.Sqr();                // n = Malt^2 (1)
            q = t.Negate(2);                // q = -T (3)
            q *= n;                         // q = Q = T*Malt^2 (1)

            // These two lines use the observation that either M == Malt or M == 0,
            // so M^3 * Malt is either Malt^4 (which is computed by squaring), or
            // zero (which is "computed" by cmov). So the cost is one squaring
            // versus two multiplications.
            n = n.Sqr();
            n = UInt256_10x26.CMov(n, m, degenerate); // n = M^3 * Malt (2)
            t = rr_alt.Sqr();                         // t = Ralt^2 (1)
            UInt256_10x26 rz = z * m_alt;             // r->z = Z3 = Malt*Z (1)
            bool infinity = rz.IsZeroNormalized() & !isInfinity;

            t += q;                                   // t = Ralt^2 + Q (2)
            UInt256_10x26 rx = t;                     // r->x = X3 = Ralt^2 + Q (2)
            t *= 2U;                                  // t = 2*X3 (4)
            t += q;                                   // t = 2*X3 + Q (5)
            t *= rr_alt;                              // t = Ralt*(2*X3 + Q) (1)
            t += n;                                   // t = Ralt*(2*X3 + Q) + M^3*Malt (3)
            UInt256_10x26 ry = t.Negate(3);           // r->y = -(Ralt*(2*X3 + Q) + M^3*Malt) (4)
            ry = ry.Half();                           // r->y = Y3 = -(Ralt*(2*X3 + Q) + M^3*Malt)/2 (3)

            // In case a.infinity == 1, replace r with (b.x, b.y, 1).
            flag = isInfinity ? 1U : 0U;
            rx = UInt256_10x26.CMov(rx, b.x, flag);
            ry = UInt256_10x26.CMov(ry, b.y, flag);
            rz = UInt256_10x26.CMov(rz, UInt256_10x26.One, flag);

            PointJacobian res = new PointJacobian(rx, ry, rz, infinity);
#if DEBUG
            res.Verify();
#endif
            return res;
        }


        /// <summary>
        /// Computes sum of the two given points
        /// </summary>
        /// <remarks>
        /// This method is not constant-time.
        /// </remarks>
        /// <param name="b">Point in affine coordinate</param>
        /// <param name="rzr"></param>
        /// <returns>Sum of two points</returns>
        public PointJacobian AddVar(in Point b, out UInt256_10x26 rzr)
        {
            // secp256k1_gej_add_ge_var
#if DEBUG
            Verify();
            b.Verify();
#endif
            if (isInfinity)
            {
                rzr = UInt256_10x26.Zero;
                return b.ToPointJacobian();
            }
            if (b.isInfinity)
            {
                rzr = UInt256_10x26.One;
                return this;
            }

            // 8 mul, 3 sqr, 13 add/negate/normalize_weak/normalizes_to_zero (ignoring special cases)
            UInt256_10x26 z12, u1, u2, s1, s2, h, i, h2, h3, t;

            z12 = z.Sqr();
            u1 = x.NormalizeWeak();
            u2 = b.x * z12;
            s1 = y.NormalizeWeak();
            s2 = b.y * z12;
            s2 *= z;
            h = u1.Negate(1);
            h += u2;
            i = s2.Negate(1);
            i += s1;
            if (h.IsZeroNormalizedVar())
            {
                if (i.IsZeroNormalizedVar())
                {
                    return DoubleVar(out rzr);
                }
                else
                {
                    rzr = UInt256_10x26.Zero;
                    return Infinity;
                }
            }

            rzr = h;

            UInt256_10x26 rz = z * h;
            h2 = h.Sqr();
            h2 = h2.Negate(1);
            h3 = h2 * h;
            t = u1 * h2;

            UInt256_10x26 rx = UInt256_10x26.Add(i.Sqr(), h3, t, t);

            t += rx;
            UInt256_10x26 ry = t * i;
            h3 *= s1;
            ry += h3;

            PointJacobian res = new PointJacobian(rx, ry, rz, false);

#if DEBUG
            res.Verify();
            rzr.Verify();
#endif
            return res;
        }


        /// <summary>
        /// Computes sum of the two given points
        /// </summary>
        /// <remarks>
        /// This method is not constant-time.
        /// </remarks>
        /// <param name="b">Point in jacobian coordinate</param>
        /// <param name="bzinv">inverse of b's Z coordinate</param>
        /// <returns></returns>
        public PointJacobian AddZInvVar(in Point b, in UInt256_10x26 bzinv)
        {
            // secp256k1_gej_add_zinv_var
#if DEBUG
            Verify();
            b.Verify();
            bzinv.Verify();
#endif
            UInt256_10x26 rx, ry;
            if (isInfinity)
            {
                UInt256_10x26 bzinv2 = bzinv.Sqr();
                UInt256_10x26 bzinv3 = bzinv2 * bzinv;
                rx = b.x * bzinv2;
                ry = b.y * bzinv3;
                PointJacobian r = new PointJacobian(rx, ry, UInt256_10x26.One, b.isInfinity);
#if DEBUG
                r.Verify();
#endif
                return r;
            }
            if (b.isInfinity)
            {
                return this;
            }

            /* We need to calculate (rx,ry,rz) = (ax,ay,az) + (bx,by,1/bzinv). Due to
             *  secp256k1's isomorphism we can multiply the Z coordinates on both sides
             *  by bzinv, and get: (rx,ry,rz*bzinv) = (ax,ay,az*bzinv) + (bx,by,1).
             *  This means that (rx,ry,rz) can be calculated as
             *  (ax,ay,az*bzinv) + (bx,by,1), when not applying the bzinv factor to rz.
             *  The variable az below holds the modified Z coordinate for a, which is used
             *  for the computation of rx and ry, but not for rz.
             */

            // 9 mul, 3 sqr, 13 add/negate/normalize_weak/normalizes_to_zero (ignoring special cases)
            UInt256_10x26 az, z12, u1, u2, s1, s2, h, i, h2, h3, t;

            az = z * bzinv;

            z12 = az.Sqr();
            u1 = x.NormalizeWeak();
            u2 = b.x * z12;
            s1 = y.NormalizeWeak();
            s2 = b.y * z12 * az;
            h = u1.Negate(1) + u2;
            i = s2.Negate(1) + s1;
            if (h.IsZeroNormalizedVar())
            {
                if (i.IsZeroNormalizedVar())
                {
                    return DoubleVar(out _);
                }
                else
                {
                    return Infinity;
                }
            }

            UInt256_10x26 rz = z * h;

            h2 = h.Sqr();
            h2 = h2.Negate(1);
            h3 = h2 * h;
            t = u1 * h2;

            rx = UInt256_10x26.Add(i.Sqr(), h3, t, t);

            t += rx;
            ry = (t * i) + (h3 * s1);

            PointJacobian res = new PointJacobian(rx, ry, rz, false);
#if DEBUG
            res.Verify();
#endif
            return res;
        }


        /// <summary>
        /// Returns double of this instance
        /// </summary>
        /// <returns>Double result</returns>
        public PointJacobian Double()
        {
#if DEBUG
            Verify();
#endif
            // Formula used:
            //   L = (3/2) * X1^2
            //   S = Y1^2
            //   T = -X1*S
            //   X3 = L^2 + 2*T
            //   Y3 = -(L*(X3 + T) + S^2)
            //   Z3 = Y1*Z1

            // Operations: 3 mul, 4 sqr, 8 add/half/mul_int/negate
            UInt256_10x26 rz = z * y;     // Z3 = Y1*Z1 (1)
            UInt256_10x26 s = y.Sqr();    // S = Y1^2 (1)
            UInt256_10x26 l = x.Sqr();    // L = X1^2 (1)
            l *= 3;                       // L = 3*X1^2 (3)
            l = l.Half();                 // L = 3/2*X1^2 (2)
            UInt256_10x26 t = s.Negate(1);// T = -S (2)
            t *= x;                       // T = -X1*S (1)
            UInt256_10x26 rx = l.Sqr();   // X3 = L^2 (1)
            //rx += t;                      // X3 = L^2 + T (2)
            //rx += t;                      // X3 = L^2 + 2*T (3)
            rx = UInt256_10x26.Add(rx, t, t);
            s = s.Sqr();                  // S' = S^2 (1)
            t += rx;                      // T' = X3 + T (4)
            UInt256_10x26 ry = t * l;     // Y3 = L*(X3 + T) (1)
            ry += s;                      // Y3 = L*(X3 + T) + S^2 (2)
            ry = ry.Negate(2);            // Y3 = -(L*(X3 + T) + S^2) (3)

            PointJacobian result = new PointJacobian(rx, ry, rz, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Returns double of this instance
        /// </summary>
        /// <param name="rzr">sets this such that r.z == a.z * rzr (where infinity means an implicit z = 0)</param>
        /// <returns>Double result</returns>
        public PointJacobian DoubleVar(out UInt256_10x26 rzr)
        {
#if DEBUG
            Verify();
#endif
            // For secp256k1, 2Q is infinity if and only if Q is infinity. This is because if 2Q = infinity,
            // Q must equal -Q, or that Q.y == -(Q.y), or Q.y is 0. For a point on y^2 = x^3 + 7 to have
            // y=0, x^3 must be -7 mod p. However, -7 has no cube root mod p.
            //
            // Having said this, if this function receives a point on a sextic twist, e.g. by
            // a fault attack, it is possible for y to be 0. This happens for y^2 = x^3 + 6,
            // since -6 does have a cube root mod p. For this point, this function will not set
            // the infinity flag even though the point doubles to infinity, and the result
            // point will be gibberish (z = 0 but infinity = 0).
            if (isInfinity)
            {
                rzr = UInt256_10x26.One;
                return Infinity;
            }

            rzr = y.NormalizeWeak();
            return Double();
        }


        /// <summary>
        /// Conditional move. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is true (=1).
        /// Constant-time
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="flag">Zero or one. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is one.</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointJacobian CMov(in PointJacobian r, in PointJacobian a, uint flag)
        {
#if DEBUG
            r.Verify();
            a.Verify();
#endif
            UInt256_10x26 rx = UInt256_10x26.CMov(r.x, a.x, flag);
            UInt256_10x26 ry = UInt256_10x26.CMov(r.y, a.y, flag);
            UInt256_10x26 rz = UInt256_10x26.CMov(r.z, a.z, flag);
            // TODO: can the following be simplified?
            bool inf = r.isInfinity ^ (r.isInfinity ^ a.isInfinity) & (flag == 1);

            PointJacobian result = new PointJacobian(rx, ry, rz, inf);
#if DEBUG
            result.Verify();
#endif
            return result;
        }

        /// <summary>
        /// Rescale this jacobian point by <paramref name="s"/> which must be non-zero. Constant-time
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public PointJacobian Rescale(in UInt256_10x26 s)
        {
#if DEBUG
            Verify();
            s.Verify();
            Debug.Assert(!s.IsZeroNormalizedVar());
#endif
            // Operations: 4 mul, 1 sqr
            UInt256_10x26 zz = s.Sqr();
            UInt256_10x26 rx = x.Multiply(zz);  // r->x *= s^2
            UInt256_10x26 ry = y.Multiply(zz);
            ry = y.Multiply(s);                 // r->y *= s^3
            UInt256_10x26 rz = z.Multiply(s);   // r->z *= s

            PointJacobian result = new PointJacobian(rx, ry, rz, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Returns inverse of this instance by mirroring it around X-axis: -P=(x,-y)
        /// </summary>
        /// <returns>-P</returns>
        public PointJacobian Negate()
        {
#if DEBUG
            Verify();
#endif
            UInt256_10x26 yNorm = y.NormalizeWeak();
            UInt256_10x26 yNeg = yNorm.Negate(1);
            PointJacobian result = new PointJacobian(x, yNeg, z, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Converts this instance to a <see cref="Point"/>.
        /// </summary>
        /// <remarks>
        /// This method is constant-time
        /// </remarks>
        /// <returns>Result</returns>
        public Point ToPoint()
        {
#if DEBUG
            Verify();
#endif
            UInt256_10x26 rz = z.Inverse();
            UInt256_10x26 z2 = rz.Sqr();
            UInt256_10x26 z3 = rz * z2;
            UInt256_10x26 rx = x * z2;
            UInt256_10x26 ry = y * z3;
            Point result = new Point(rx, ry, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }

        /// <summary>
        /// Converts this instance to a <see cref="Point"/> without constant-time guarantee.
        /// </summary>
        /// <remarks>
        /// This method is not constant-time
        /// </remarks>
        /// <returns>Result</returns>
        public Point ToPointVar()
        {
#if DEBUG
            Verify();
#endif
            if (isInfinity)
            {
                return Point.Infinity;
            }

            UInt256_10x26 rz = z.InverseVar();
            UInt256_10x26 z2 = rz.Sqr();
            UInt256_10x26 z3 = rz * z2;
            UInt256_10x26 rx = x * z2;
            UInt256_10x26 ry = y * z3;
            Point result = new Point(rx, ry, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        internal Point ToPointZInv(in UInt256_10x26 zi)
        {
#if DEBUG
            Verify();
            zi.Verify();
            Debug.Assert(!isInfinity);
#endif
            UInt256_10x26 zi2 = zi.Sqr();
            UInt256_10x26 zi3 = zi2 * zi;
            UInt256_10x26 rx = x * zi2;
            UInt256_10x26 ry = y * zi3;
            Point result = new Point(rx, ry, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Returns if the two points in jacobina coordinates are equal.
        /// </summary>
        /// <param name="other">Other point to compare</param>
        /// <returns>True if the two points are equal; otherwise false.</returns>
        public bool EqualsVar(in PointJacobian other)
        {
#if DEBUG
            Verify();
            other.Verify();
#endif
            PointJacobian tmp = Negate();
            tmp = tmp.AddVar(other, out _);
            return tmp.isInfinity;
        }

        /// <summary>
        /// Returns if this instance is equal to the given point in affine coordinates.
        /// </summary>
        /// <param name="other">Other point to compare</param>
        /// <returns>True if the two points are equal; otherwise false.</returns>
        public bool EqualsVar(in Point other)
        {
#if DEBUG
            Verify();
            other.Verify();
#endif
            PointJacobian tmp = Negate();
            tmp = tmp.AddVar(other, out _);
            return tmp.isInfinity;
        }

        /// <summary>
        /// Returns if the x coordinate of this instance is equal to the given x value.
        /// </summary>
        /// <remarks>
        /// This method is not constant-time
        /// </remarks>
        /// <param name="x">x coordinate (magnitude must not exceed 31)</param>
        /// <returns>True if x coordinates were equal; otherwise false.</returns>
        public bool EqualsVar(in UInt256_10x26 x)
        {
#if DEBUG
            Verify();
            x.Verify();
            Debug.Assert(!isInfinity);
#endif
            UInt256_10x26 r = z.Sqr() * x;
            return r.Equals(this.x);
        }
    }
}
