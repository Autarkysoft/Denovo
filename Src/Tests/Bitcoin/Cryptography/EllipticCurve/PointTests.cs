// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using System;
using System.Collections.Generic;
using Tests.Bitcoin.Cryptography.EllipticCurve.Primitives;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class PointTests
    {
        // https://github.com/bitcoin-core/secp256k1/blob/46db787112beabdb5e17e0dc35680716f1057e7b/src/group.h#L22
        internal static Point SECP256K1_GE_CONST(uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h,
                                                 uint i, uint j, uint k, uint l, uint m, uint n, uint o, uint p)
        {
            return new Point(
                UInt256_5x52Tests.SECP256K1_FE_CONST(a, b, c, d, e, f, g, h),
                UInt256_5x52Tests.SECP256K1_FE_CONST(i, j, k, l, m, n, o, p));
        }



        public static IEnumerable<TheoryDataRow<ulong[], ulong[]>> GetCtorCases()
        {
            yield return new
            (
                new ulong[4]
                {
                    0xb658d37c4bf1be70, 0x337a56a0be5c7acf, 0x2443518bb373dc52, 0xd40a8c1343c6b32e
                },
                new ulong[4]
                {
                    0x8b84ada143f66cb4, 0x7801e1899fed5efd, 0x9617f47bf4821107, 0x8375c450a05fc9a3
                }
            );
        }
        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void Constructor_FromUInt256_5x52Test(ulong[] xArr, ulong[] yArr)
        {
            UInt256_5x52 x = new(xArr[0], xArr[1], xArr[2], xArr[3]);
            UInt256_5x52 y = new(yArr[0], yArr[1], yArr[2], yArr[3]);

            Point pt = new(x, y);

            UInt256_5x52Tests.AssertEqual(x, pt.x);
            UInt256_5x52Tests.AssertEqual(y, pt.y);
            Assert.False(pt.isInfinity);

            pt = new(x, y, true);
            UInt256_5x52Tests.AssertEqual(x, pt.x);
            UInt256_5x52Tests.AssertEqual(y, pt.y);
            Assert.True(pt.isInfinity); // This ctor sets the isInfinity field
        }

        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void Constructor_FromULongsTest(ulong[] xArr, ulong[] yArr)
        {
            UInt256_5x52 x = new(xArr[0], xArr[1], xArr[2], xArr[3]);
            UInt256_5x52 y = new(yArr[0], yArr[1], yArr[2], yArr[3]);

            Point pt = new(xArr[0], xArr[1], xArr[2], xArr[3],
                           yArr[0], yArr[1], yArr[2], yArr[3]);

            UInt256_5x52Tests.AssertEqual(x, pt.x);
            UInt256_5x52Tests.AssertEqual(y, pt.y);
            Assert.False(pt.isInfinity);
        }

        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void Constructor_FromBytesTest(ulong[] xArr, ulong[] yArr)
        {
            UInt256_5x52 x = new(xArr[0], xArr[1], xArr[2], xArr[3]);
            UInt256_5x52 y = new(yArr[0], yArr[1], yArr[2], yArr[3]);
            Span<byte> buffer = new byte[64];
            x.WriteToSpan(buffer);
            y.WriteToSpan(buffer.Slice(32));

            Point pt = new(buffer);

            UInt256_5x52Tests.AssertEqual(x, pt.x);
            UInt256_5x52Tests.AssertEqual(y, pt.y);
            Assert.False(pt.isInfinity);
        }

        [Fact]
        public void Constructor_FromBytes_ZeroTest()
        {
            Span<byte> buffer = new byte[64];
            Point pt = new(buffer);

            Assert.True(pt.x.IsZero);
            Assert.True(pt.y.IsZero);
            Assert.True(pt.isInfinity);
        }

        [Fact]
        public void Constructor_FromBytes_ExceptionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Point(Array.Empty<byte>()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Point(new byte[32]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Point(new byte[63]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Point(new byte[65]));
        }

        [Fact]
        public void StaticMemberTest()
        {
            Assert.True(Point.Infinity.x.IsZero);
            Assert.True(Point.Infinity.y.IsZero);
            Assert.True(Point.Infinity.isInfinity);

            // https://github.com/bitcoin-core/secp256k1/blob/46db787112beabdb5e17e0dc35680716f1057e7b/src/group_impl.h#L38-L43
            Point expected = SECP256K1_GE_CONST(
                0x79be667e, 0xf9dcbbac, 0x55a06295, 0xce870b07,
                0x029bfcdb, 0x2dce28d9, 0x59f2815b, 0x16f81798,
                0x483ada77, 0x26a3c465, 0x5da4fbfc, 0x0e1108a8,
                0xfd17b448, 0xa6855419, 0x9c47d08f, 0xfb10d4b8);

            UInt256_5x52Tests.AssertEqual(expected.x, Point.G.x);
            UInt256_5x52Tests.AssertEqual(expected.y, Point.G.y);
            Assert.False(Point.G.isInfinity);
        }



        #region https://github.com/bitcoin-core/secp256k1/blob/46db787112beabdb5e17e0dc35680716f1057e7b/src/tests.c#L3943-L4469

        // This covers both Point and PointJacobian tests (ge+gej)

        private const int COUNT = 16;

        // These values are hard-coded and has to be the same as constants in Point and PointJacobian files
        private const int SECP256K1_GE_X_MAGNITUDE_MAX = 4;
        private const int SECP256K1_GE_Y_MAGNITUDE_MAX = 3;
        private const int SECP256K1_GEJ_X_MAGNITUDE_MAX = 4;
        private const int SECP256K1_GEJ_Y_MAGNITUDE_MAX = 4;
        private const int SECP256K1_GEJ_Z_MAGNITUDE_MAX = 1;

        /// <summary>
        /// random_ge_x_magnitude
        /// </summary>
        private static void Libsecp256k1_RandomXMagnitude(ref Point ge, TestRNG rng)
        {
            UInt256_5x52 x = ge.x;
            UInt256_5x52Tests.RandomFEMagnitude(ref x, SECP256K1_GE_X_MAGNITUDE_MAX, rng);
            ge = new(x, ge.y, ge.isInfinity);
        }

        /// <summary>
        /// random_ge_y_magnitude
        /// </summary>
        private static void Libsecp256k1_RandomYMagnitude(ref Point ge, TestRNG rng)
        {
            UInt256_5x52 y = ge.y;
            UInt256_5x52Tests.RandomFEMagnitude(ref y, SECP256K1_GE_Y_MAGNITUDE_MAX, rng);
            ge = new(ge.x, y, ge.isInfinity);
        }

        /// <summary>
        /// random_gej_x_magnitude
        /// </summary>
        private static void Libsecp256k1_RandomXMagnitude(ref PointJacobian gej, TestRNG rng)
        {
            UInt256_5x52 x = gej.x;
            UInt256_5x52Tests.RandomFEMagnitude(ref x, SECP256K1_GEJ_X_MAGNITUDE_MAX, rng);
            gej = new(x, gej.y, gej.z, gej.isInfinity);
        }

        /// <summary>
        /// random_gej_y_magnitude
        /// </summary>
        private static void Libsecp256k1_RandomYMagnitude(ref PointJacobian gej, TestRNG rng)
        {
            UInt256_5x52 y = gej.y;
            UInt256_5x52Tests.RandomFEMagnitude(ref y, SECP256K1_GEJ_Y_MAGNITUDE_MAX, rng);
            gej = new(gej.x, y, gej.z, gej.isInfinity);
        }

        /// <summary>
        /// random_gej_z_magnitude
        /// </summary>
        private static void Libsecp256k1_RandomZMagnitude(ref PointJacobian gej, TestRNG rng)
        {
            UInt256_5x52 z = gej.z;
            UInt256_5x52Tests.RandomFEMagnitude(ref z, SECP256K1_GEJ_Z_MAGNITUDE_MAX, rng);
            gej = new(gej.x, gej.y, z, gej.isInfinity);
        }

        /// <summary>
        /// testutil_random_fe_non_zero_test
        /// </summary>
        private static UInt256_5x52 Libsecp256k1_RandomFENonZeroTest(TestRNG rng)
        {
            UInt256_5x52 fe;
            do
            {
                fe = UInt256_5x52Tests.RandomFETest(rng);
            } while (fe.IsZero);
            return fe;
        }

        /// <summary>
        /// testutil_random_ge_test
        /// </summary>
        internal static Point Libsecp256k1_RandomGET(TestRNG rng)
        {
            UInt256_5x52 fe;
            Point ge;
            do
            {
                fe = UInt256_5x52Tests.RandomFETest(rng);
                bool odd = rng.RandBits(1) != 0;
                if (Point.TryCreateVar(fe, odd, out ge))
                {
                    ge = new Point(ge.x, ge.y.Normalize(), ge.isInfinity);
                    break;
                }
            } while (true);

            return ge;
        }

        /// <summary>
        /// testutil_random_ge_jacobian_test
        /// </summary>
        private static PointJacobian Libsecp256k1_RandGEJacobianT(in Point ge, TestRNG rng)
        {
            UInt256_5x52 z2, z3;
            UInt256_5x52 gejz = Libsecp256k1_RandomFENonZeroTest(rng);
            z2 = gejz.Sqr();
            z3 = z2.Multiply(gejz);
            UInt256_5x52 gejx = ge.x.Multiply(z2);
            UInt256_5x52 gejy = ge.y.Multiply(z3);
            return new PointJacobian(gejx, gejy, gejz, ge.isInfinity);
        }

        /// <summary>
        /// testutil_random_gej_test
        /// </summary>
        private static PointJacobian Libsecp256k1_RandomGejTest(TestRNG rng)
        {
            Point ge = Libsecp256k1_RandomGET(rng);
            PointJacobian gej = Libsecp256k1_RandGEJacobianT(ge, rng);
            return gej;
        }


        // This compares jacobian points including their Z, not just their geometric meaning.
        /// <summary>
        /// gej_xyz_equals_gej
        /// </summary>
        private static int Libsecp256k1_Gej_XYZ_EqualsGej(in PointJacobian a, in PointJacobian b)
        {
            PointJacobian a2;
            PointJacobian b2;
            int ret = 1;
            ret &= a.isInfinity == b.isInfinity ? 1 : 0;
            if (ret != 0 && !a.isInfinity)
            {
                a2 = a;
                b2 = b;
                UInt256_5x52 a2x = a2.x.Normalize();
                UInt256_5x52 a2y = a2.y.Normalize();
                UInt256_5x52 a2z = a2.z.Normalize();
                UInt256_5x52 b2x = b2.x.Normalize();
                UInt256_5x52 b2y = b2.y.Normalize();
                UInt256_5x52 b2z = b2.z.Normalize();
                ret &= a2x.CompareToVar(b2x) == 0 ? 1 : 0;
                ret &= a2y.CompareToVar(b2y) == 0 ? 1 : 0;
                ret &= a2z.CompareToVar(b2z) == 0 ? 1 : 0;
            }
            return ret;
        }

        /// <summary>
        /// test_ge
        /// </summary>
        private static void Libsecp256k1_TestGE(TestRNG rng)
        {
            int runs = 6;
            // 25 points are used:
            // - infinity
            // - for each of four random points p1 p2 p3 p4, we add the point, its
            //   negation, and then those two again but with randomized Z coordinate.
            // - The same is then done for lambda*p1 and lambda^2*p1.

            //secp256k1_ge* ge = (secp256k1_ge*)checked_malloc(&CTX->error_callback, sizeof(secp256k1_ge) * (1 + 4 * runs));
            //secp256k1_gej* gej = (secp256k1_gej*)checked_malloc(&CTX->error_callback, sizeof(secp256k1_gej) * (1 + 4 * runs));
            Span<Point> ge = new Point[1 + 4 * runs];
            Span<PointJacobian> gej = new PointJacobian[1 + 4 * runs];

            UInt256_5x52 zf, r;
            UInt256_5x52 zfi2, zfi3;

            gej[0] = PointJacobian.Infinity;
            ge[0] = Point.Infinity;
            for (int i = 0; i < runs; i++)
            {
                Point g = Libsecp256k1_RandomGET(rng);
                if (i >= runs - 2)
                {
                    g = ge[1].MulLambda();
                    Assert.False(g.EqualsVar(ge[1]));
                }
                if (i >= runs - 1)
                {
                    g = g.MulLambda();
                }
                ge[1 + 4 * i] = g;
                ge[2 + 4 * i] = g;
                ge[3 + 4 * i] = g.Negate();
                ge[4 + 4 * i] = g.Negate();
                gej[1 + 4 * i] = ge[1 + 4 * i].ToPointJacobian();
                gej[2 + 4 * i] = Libsecp256k1_RandGEJacobianT(ge[2 + 4 * i], rng);
                gej[3 + 4 * i] = ge[3 + 4 * i].ToPointJacobian();
                gej[4 + 4 * i] = Libsecp256k1_RandGEJacobianT(ge[4 + 4 * i], rng);
                for (int j = 0; j < 4; j++)
                {
                    Libsecp256k1_RandomXMagnitude(ref ge[1 + j + 4 * i], rng);
                    Libsecp256k1_RandomYMagnitude(ref ge[1 + j + 4 * i], rng);
                    Libsecp256k1_RandomXMagnitude(ref gej[1 + j + 4 * i], rng);
                    Libsecp256k1_RandomYMagnitude(ref gej[1 + j + 4 * i], rng);
                    Libsecp256k1_RandomZMagnitude(ref gej[1 + j + 4 * i], rng);
                }

                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        bool expect_equal = (j >> 1) == (k >> 1);
                        Assert.True(ge[1 + j + 4 * i].EqualsVar(ge[1 + k + 4 * i]) == expect_equal);
                        Assert.True(gej[1 + j + 4 * i].EqualsVar(gej[1 + k + 4 * i]) == expect_equal);
                        Assert.True(gej[1 + j + 4 * i].EqualsVar(ge[1 + k + 4 * i]) == expect_equal);
                        Assert.True(gej[1 + k + 4 * i].EqualsVar(ge[1 + j + 4 * i]) == expect_equal);
                    }
                }
            }

            // Generate random zf, and zfi2 = 1/zf^2, zfi3 = 1/zf^3
            zf = Libsecp256k1_RandomFENonZeroTest(rng);
            UInt256_5x52Tests.RandomFEMagnitude(ref zf, 8, rng);
            zfi3 = zf.InverseVar();
            zfi2 = zfi3.Sqr();
            zfi3 = zfi3.Multiply(zfi2);

            // Generate random r
            r = Libsecp256k1_RandomFENonZeroTest(rng);

            for (int i1 = 0; i1 < 1 + 4 * runs; i1++)
            {
                for (int i2 = 0; i2 < 1 + 4 * runs; i2++)
                {
                    // Compute reference result using gej + gej (var).
                    PointJacobian refj, resj;
                    refj = gej[i1].AddVar(gej[i2], out UInt256_5x52 zr);
                    // Check Z ratio.
                    if (!gej[i1].isInfinity && !refj.isInfinity)
                    {
                        UInt256_5x52 zrz = zr.Multiply(gej[i1].z);
                        Assert.True(zrz.Equals(refj.z));
                    }
                    Point _ref = refj.ToPointVar();

                    // Test gej + ge with Z ratio result (var).
                    resj = gej[i1].AddVar(ge[i2], out zr);
                    Assert.True(resj.EqualsVar(_ref));
                    if (!gej[i1].isInfinity && !resj.isInfinity)
                    {
                        UInt256_5x52 zrz = zr.Multiply(gej[i1].z);
                        Assert.True(zrz.Equals(resj.z));
                    }

                    // Test gej + ge (var, with additional Z factor).
                    {
                        Point ge2_zfi = ge[i2]; // the second term with x and y rescaled for z = 1/zf
                        UInt256_5x52 tempx = ge2_zfi.x.Multiply(zfi2);
                        UInt256_5x52 tempy = ge2_zfi.y.Multiply(zfi3);
                        ge2_zfi = new(tempx, tempy, ge2_zfi.isInfinity);

                        Libsecp256k1_RandomXMagnitude(ref ge2_zfi, rng);
                        Libsecp256k1_RandomYMagnitude(ref ge2_zfi, rng);
                        resj = gej[i1].AddZInvVar(ge2_zfi, zf);
                        Assert.True(resj.EqualsVar(_ref));
                    }

                    // Test gej + ge (const).
                    if (i2 != 0)
                    {
                        // secp256k1_gej_add_ge does not support its second argument being infinity.
                        resj = gej[i1].Add(ge[i2]);
                        Assert.True(resj.EqualsVar(_ref));
                    }

                    // Test doubling (var).
                    if ((i1 == 0 && i2 == 0) || ((i1 + 3) / 4 == (i2 + 3) / 4 && ((i1 + 3) % 4) / 2 == ((i2 + 3) % 4) / 2))
                    {
                        // Normal doubling with Z ratio result.
                        resj = gej[i1].DoubleVar(out UInt256_5x52 zr2);
                        Assert.True(resj.EqualsVar(_ref));
                        // Check Z ratio.
                        zr2 = zr2.Multiply(gej[i1].z);
                        Assert.True(zr2.Equals(resj.z));
                        // Normal doubling.
                        resj = gej[i2].DoubleVar(out _);
                        Assert.True(resj.EqualsVar(_ref));
                        // Constant-time doubling.
                        resj = gej[i2].Double();
                        Assert.True(resj.EqualsVar(_ref));
                    }

                    // Test adding opposites.
                    if ((i1 == 0 && i2 == 0) || ((i1 + 3) / 4 == (i2 + 3) / 4 && ((i1 + 3) % 4) / 2 != ((i2 + 3) % 4) / 2))
                    {
                        Assert.True(_ref.isInfinity);
                    }

                    // Test adding infinity.
                    if (i1 == 0)
                    {
                        Assert.True(ge[i1].isInfinity);
                        Assert.True(gej[i1].isInfinity);
                        Assert.True(gej[i2].EqualsVar(_ref));
                    }
                    if (i2 == 0)
                    {
                        Assert.True(ge[i2].isInfinity);
                        Assert.True(gej[i2].isInfinity);
                        Assert.True(gej[i1].EqualsVar(_ref));
                    }
                }
            }

            // Test adding all points together in random order equals infinity.
            {
                PointJacobian sum = PointJacobian.Infinity;
                //secp256k1_gej* gej_shuffled = (secp256k1_gej*)checked_malloc(&CTX->error_callback, (4 * runs + 1) * sizeof(secp256k1_gej));
                Span<PointJacobian> gej_shuffled = new PointJacobian[4 * runs + 1];
                for (int i = 0; i < 4 * runs + 1; i++)
                {
                    gej_shuffled[i] = gej[i];
                }
                for (int i = 0; i < 4 * runs + 1; i++)
                {
                    int swap = (int)(i + rng.RandInt((uint)(4 * runs + 1 - i)));
                    if (swap != i)
                    {
                        (gej_shuffled[swap], gej_shuffled[i]) = (gej_shuffled[i], gej_shuffled[swap]);
                    }
                }
                for (int i = 0; i < 4 * runs + 1; i++)
                {
                    sum = sum.AddVar(gej_shuffled[i], out _);
                }
                Assert.True(sum.isInfinity);
            }

            // Test batch gej -> ge conversion without known z ratios.
            {
                //secp256k1_ge *ge_set_all_var = (secp256k1_ge *)checked_malloc(&CTX->error_callback, (4 * runs + 1) * sizeof(secp256k1_ge));
                //secp256k1_ge* ge_set_all = (secp256k1_ge *)checked_malloc(&CTX->error_callback, (4 * runs + 1) * sizeof(secp256k1_ge));
                Span<Point> ge_set_all = new Point[4 * runs + 1];
                Span<Point> ge_set_all_var = new Point[4 * runs + 1];
                Point.SetAllPointsToJacobianVar(ge_set_all_var, gej);
                for (int i = 0; i < 4 * runs + 1; i++)
                {
                    UInt256_5x52 s = UInt256_5x52Tests.RandomFENonZero(rng);
                    gej[i] = gej[i].Rescale(s);
                    Assert.True(gej[i].EqualsVar(ge_set_all_var[i]));
                }

                // Skip infinity at &gej[0].
                Point.SetAllPointsToJacobian(ge_set_all.Slice(1), gej.Slice(1));
                for (int i = 1; i < 4 * runs + 1; i++)
                {
                    UInt256_5x52 s = UInt256_5x52Tests.RandomFENonZero(rng);
                    gej[i] = gej[i].Rescale(s);
                    Assert.True(gej[i].EqualsVar(ge_set_all[i]));
                    Assert.True(ge_set_all_var[i].EqualsVar(ge_set_all[i]));
                }

                // Test with an array of length 1.
                Point.SetAllPointsToJacobianVar(ge_set_all_var.Slice(1, 1), gej.Slice(1, 1));
                Point.SetAllPointsToJacobian(ge_set_all.Slice(1, 1), gej.Slice(1, 1));
                Assert.True(gej[1].EqualsVar(ge_set_all_var[1]));
                Assert.True(gej[1].EqualsVar(ge_set_all[1]));
                Assert.True(ge_set_all_var[1].EqualsVar(ge_set_all[1]));

                // Test with an array of length 0.
                Point.SetAllPointsToJacobianVar(ge_set_all_var.Slice(1, 0), gej.Slice(1, 0));
                Point.SetAllPointsToJacobian(ge_set_all.Slice(1, 0), gej.Slice(1, 0));
            }

            // Test that all elements have X coordinates on the curve.
            for (int i = 1; i < 4 * runs + 1; i++)
            {
                UInt256_5x52 n;
                Assert.True(Point.IsOnCurveVar(ge[i].x));
                // And the same holds after random rescaling.
                n = zf.Multiply(ge[i].x);
                Assert.True(Point.IsFracOnCurveVar(n, zf));
            }

            // Test correspondence of secp256k1_ge_x{,_frac}_on_curve_var with ge_set_xo.
            {
                UInt256_5x52 n = zf.Multiply(r);
                bool ret_on_curve = Point.IsOnCurveVar(r);
                bool ret_frac_on_curve = Point.IsFracOnCurveVar(n, zf);
                bool ret_set_xo = Point.TryCreateVar(r, false, out Point q);
                Assert.True(ret_on_curve == ret_frac_on_curve);
                Assert.True(ret_on_curve == ret_set_xo);
                if (ret_set_xo)
                {
                    Assert.True(r.Equals(q.x));
                }
            }

            // Test batch gej -> ge conversion with many infinities.
            for (int i = 0; i < 4 * runs + 1; i++)
            {
                ge[i] = Libsecp256k1_RandomGET(rng);
                bool odd = ge[i].x.IsOdd;
                // randomly set half the points to infinity
                if (odd == (i % 2 == 1)) // odd == i % 2
                {
                    ge[i] = Point.Infinity;
                }
                gej[i] = ge[i].ToPointJacobian();
            }
            // batch convert
            Point.SetAllPointsToJacobianVar(ge, gej);
            // check result
            for (int i = 0; i < 4 * runs + 1; i++)
            {
                Assert.True(gej[i].EqualsVar(ge[i]));
            }

            // Test batch gej -> ge conversion with all infinities.
            for (int i = 0; i < 4 * runs + 1; i++)
            {
                gej[i] = PointJacobian.Infinity;
            }
            // batch convert
            Point.SetAllPointsToJacobianVar(ge, gej);
            // check result
            for (int i = 0; i < 4 * runs + 1; i++)
            {
                Assert.True(ge[i].isInfinity);
            }
        }

        /// <summary>
        /// test_intialized_inf
        /// </summary>
        private static void Libsecp256k1_TestIntializedInf(TestRNG rng)
        {
            Point p;
            PointJacobian pj, npj, infj1, infj2, infj3;
            UInt256_5x52 zinv;

            // Test that adding P+(-P) results in a fully initialized infinity
            p = Libsecp256k1_RandomGET(rng);
            pj = p.ToPointJacobian();
            npj = pj.Negate();

            infj1 = pj.AddVar(npj, out _);
            Assert.True(infj1.isInfinity);
            Assert.True(infj1.x.IsZero);
            Assert.True(infj1.y.IsZero);
            Assert.True(infj1.z.IsZero);

            infj2 = npj.AddVar(p, out _);
            Assert.True(infj2.isInfinity);
            Assert.True(infj2.x.IsZero);
            Assert.True(infj2.y.IsZero);
            Assert.True(infj2.z.IsZero);

            zinv = new UInt256_5x52(1);
            infj3 = npj.AddZInvVar(p, zinv);
            Assert.True(infj3.isInfinity);
            Assert.True(infj3.x.IsZero);
            Assert.True(infj3.y.IsZero);
            Assert.True(infj3.z.IsZero);
        }

        /// <summary>
        /// test_add_neg_y_diff_x
        /// </summary>
        private static void Libsecp256k1_TestAddNegYDiffX()
        {
            /* The point of this test is to check that we can add two points
             * whose y-coordinates are negatives of each other but whose x
             * coordinates differ. If the x-coordinates were the same, these
             * points would be negatives of each other and their sum is
             * infinity. This is cool because it "covers up" any degeneracy
             * in the addition algorithm that would cause the xy coordinates
             * of the sum to be wrong (since infinity has no xy coordinates).
             * HOWEVER, if the x-coordinates are different, infinity is the
             * wrong answer, and such degeneracies are exposed. This is the
             * root of https://github.com/bitcoin-core/secp256k1/issues/257
             * which this test is a regression test for.
             *
             * These points were generated in sage as
             *
             * load("secp256k1_params.sage")
             *
             * # random "bad pair"
             * P = C.random_element()
             * Q = -int(LAMBDA) * P
             * print("    P: %x %x" % P.xy())
             * print("    Q: %x %x" % Q.xy())
             * print("P + Q: %x %x" % (P + Q).xy())
             */
            PointJacobian aj = PointJacobianTests.SECP256K1_GEJ_CONST(
                0x8d24cd95, 0x0a355af1, 0x3c543505, 0x44238d30,
                0x0643d79f, 0x05a59614, 0x2f8ec030, 0xd58977cb,
                0x001e337a, 0x38093dcd, 0x6c0f386d, 0x0b1293a8,
                0x4d72c879, 0xd7681924, 0x44e6d2f3, 0x9190117d);
            PointJacobian bj = PointJacobianTests.SECP256K1_GEJ_CONST(
                0xc7b74206, 0x1f788cd9, 0xabd0937d, 0x164a0d86,
                0x95f6ff75, 0xf19a4ce9, 0xd013bd7b, 0xbf92d2a7,
                0xffe1cc85, 0xc7f6c232, 0x93f0c792, 0xf4ed6c57,
                0xb28d3786, 0x2897e6db, 0xbb192d0b, 0x6e6feab2);
            PointJacobian sumj = PointJacobianTests.SECP256K1_GEJ_CONST(
                0x671a63c0, 0x3efdad4c, 0x389a7798, 0x24356027,
                0xb3d69010, 0x278625c3, 0x5c86d390, 0x184a8f7a,
                0x5f6409c2, 0x2ce01f2b, 0x511fd375, 0x25071d08,
                0xda651801, 0x70e95caf, 0x8f0d893c, 0xbed8fbbe);

            Point b;
            PointJacobian resj;
            Point res;
            b = bj.ToPoint();

            resj = aj.AddVar(bj, out _);
            res = resj.ToPoint();
            Assert.True(sumj.EqualsVar(res));

            resj = aj.Add(b);
            res = resj.ToPoint();
            Assert.True(sumj.EqualsVar(res));

            resj = aj.AddVar(b, out _);
            res = resj.ToPoint();
            Assert.True(sumj.EqualsVar(res));
        }

        /// <summary>
        /// test_ge_bytes
        /// </summary>
        private static void Libsecp256k1_TestGeBytes(TestRNG rng)
        {
            for (int i = 0; i < COUNT + 1; i++)
            {
                Span<byte> buf = new byte[64];
                Point p, q;

                if (i == 0)
                {
                    p = Point.Infinity;
                }
                else
                {
                    p = Libsecp256k1_RandomGET(rng);
                }

                // Note that unlike libsecp256k1 we don't have 2 methods to convert to bytes
                // secp256k1_ge_to/from_bytes and secp256k1_ge_to/from_bytes_ext are the same
                // since we handle infinity differently
                p.ToByteArray(buf);
                q = new Point(buf);
                Assert.True(p.EqualsVar(q));
            }
        }

        /// <summary>
        /// run_ge
        /// </summary>
        [Fact]
        public void Libsecp256k1_GETest()
        {
            TestRNG rng = new();
            rng.RunXoshiro256ppTests();
            rng.Init(null);

            for (int i = 0; i < COUNT * 32; i++)
            {
                Libsecp256k1_TestGE(rng);
            }
            Libsecp256k1_TestAddNegYDiffX();
            Libsecp256k1_TestIntializedInf(rng);
            Libsecp256k1_TestGeBytes(rng);
        }

        /// <summary>
        /// test_gej_cmov
        /// </summary>
        static void Libsecp256k1_TestGejCmov(in PointJacobian a, in PointJacobian b)
        {
            PointJacobian t = a;
            t = PointJacobian.CMov(t, b, 0);
            Assert.Equal(1, Libsecp256k1_Gej_XYZ_EqualsGej(t, a));
            t = PointJacobian.CMov(t, b, 1);
            Assert.Equal(1, Libsecp256k1_Gej_XYZ_EqualsGej(t, b));
        }

        /// <summary>
        /// run_gej
        /// </summary>
        [Fact]
        public void Libsecp256k1_GejTest()
        {
            TestRNG rng = new();
            rng.RunXoshiro256ppTests();
            rng.Init(null);

            PointJacobian a, b;

            // Tests for secp256k1_gej_cmov
            for (int i = 0; i < COUNT; i++)
            {
                a = PointJacobian.Infinity;
                b = PointJacobian.Infinity;
                Libsecp256k1_TestGejCmov(a, b);

                a = Libsecp256k1_RandomGejTest(rng);
                Libsecp256k1_TestGejCmov(a, b);
                Libsecp256k1_TestGejCmov(b, a);

                b = a;
                Libsecp256k1_TestGejCmov(a, b);

                b = Libsecp256k1_RandomGejTest(rng);
                Libsecp256k1_TestGejCmov(a, b);
                Libsecp256k1_TestGejCmov(b, a);
            }

            // Tests for secp256k1_gej_eq_var
            for (int i = 0; i < COUNT; i++)
            {
                UInt256_5x52 fe;
                a = Libsecp256k1_RandomGejTest(rng);
                b = Libsecp256k1_RandomGejTest(rng);
                Assert.False(a.EqualsVar(b));

                b = a;
                fe = Libsecp256k1_RandomFENonZeroTest(rng);
                a = a.Rescale(fe);
                Assert.True(a.EqualsVar(b));
            }
        }

        /// <summary>
        /// test_group_decompress
        /// </summary>
        private static void Libsecp256k1_TestGroupDecompress(in UInt256_5x52 x)
        {
            // The input itself, normalized.
            UInt256_5x52 fex = x;
            fex = fex.NormalizeVar();

            bool res_even = Point.TryCreateVar(fex, false, out Point ge_even);
            bool res_odd = Point.TryCreateVar(fex, true, out Point ge_odd);

            Assert.True(res_even == res_odd);

            if (res_even)
            {
                UInt256_5x52 normXOdd = ge_odd.x.NormalizeVar();
                UInt256_5x52 normXEven = ge_even.x.NormalizeVar();
                UInt256_5x52 normYOdd = ge_odd.y.NormalizeVar();
                UInt256_5x52 normYEven = ge_even.y.NormalizeVar();

                ge_odd = new(normXOdd, normYOdd, ge_odd.isInfinity);
                ge_even = new(normXEven, normYEven, ge_odd.isInfinity);

                // No infinity allowed.
                Assert.False(ge_even.isInfinity);
                Assert.False(ge_odd.isInfinity);

                // Check that the x coordinates check out.
                Assert.True(ge_even.x.Equals(x));
                Assert.True(ge_odd.x.Equals(x));

                // Check odd/even Y in ge_odd, ge_even.
                Assert.True(ge_odd.y.IsOdd);
                Assert.False(ge_even.y.IsOdd);
            }
        }

        /// <summary>
        /// run_group_decompress
        /// </summary>
        [Fact]
        public void Libsecp256k1_GroupDecompress()
        {
            TestRNG rng = new();
            rng.RunXoshiro256ppTests();
            rng.Init(null);

            for (int i = 0; i < COUNT * 4; i++)
            {
                UInt256_5x52 fe = UInt256_5x52Tests.RandomFETest(rng);
                Libsecp256k1_TestGroupDecompress(fe);
            }
        }

        #endregion
    }
}
