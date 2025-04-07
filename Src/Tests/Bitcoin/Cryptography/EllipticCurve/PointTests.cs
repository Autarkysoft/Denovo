// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Collections.Generic;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class PointTests
    {
        // https://github.com/bitcoin-core/secp256k1/blob/70f149b9a1bf4ed3266f97774d0ae9577534bf40/src/group.h#L22
        internal static Point SECP256K1_GE_CONST(uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h,
                                                 uint i, uint j, uint k, uint l, uint m, uint n, uint o, uint p)
        {
            return new Point(
                UInt256_10x26Tests.SECP256K1_FE_CONST(a, b, c, d, e, f, g, h),
                UInt256_10x26Tests.SECP256K1_FE_CONST(i, j, k, l, m, n, o, p));
        }

        [Fact]
        public void ConstTest()
        {
            // https://github.com/bitcoin-core/secp256k1/blob/70f149b9a1bf4ed3266f97774d0ae9577534bf40/src/group_impl.h#L38-L43
            Point actual = SECP256K1_GE_CONST(
                0x79be667e, 0xf9dcbbac, 0x55a06295, 0xce870b07,
                0x029bfcdb, 0x2dce28d9, 0x59f2815b, 0x16f81798,
                0x483ada77, 0x26a3c465, 0x5da4fbfc, 0x0e1108a8,
                0xfd17b448, 0xa6855419, 0x9c47d08f, 0xfb10d4b8);

            Assert.True(Point.G.Equals(actual));
        }


        public static IEnumerable<object[]> GetCtorCases()
        {
            yield return new object[]
            {
                new uint[8]
                {
                    0xb658d37c, 0x4bf1be70, 0x337a56a0, 0xbe5c7acf, 0x2443518b, 0xb373dc52, 0xd40a8c13, 0x43c6b32e
                },
                new uint[8]
                {
                    0x8b84ada1, 0x43f66cb4, 0x7801e189, 0x9fed5efd, 0x9617f47b, 0xf4821107, 0x8375c450, 0xa05fc9a3
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void ConstructorTest(uint[] xArr, uint[] yArr)
        {
            UInt256_10x26 x = new(xArr[0], xArr[1], xArr[2], xArr[3], xArr[4], xArr[5], xArr[6], xArr[7]);
            UInt256_10x26 y = new(yArr[0], yArr[1], yArr[2], yArr[3], yArr[4], yArr[5], yArr[6], yArr[7]);

            Point pt = new(x, y);

            UInt256_10x26Tests.AssertEquality(x, pt.x);
            UInt256_10x26Tests.AssertEquality(y, pt.y);
        }

        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void Constructor_FromUintsTest(uint[] xArr, uint[] yArr)
        {
            UInt256_10x26 x = new(xArr[0], xArr[1], xArr[2], xArr[3], xArr[4], xArr[5], xArr[6], xArr[7]);
            UInt256_10x26 y = new(yArr[0], yArr[1], yArr[2], yArr[3], yArr[4], yArr[5], yArr[6], yArr[7]);

            Point pt = new(xArr[0], xArr[1], xArr[2], xArr[3], xArr[4], xArr[5], xArr[6], xArr[7],
                           yArr[0], yArr[1], yArr[2], yArr[3], yArr[4], yArr[5], yArr[6], yArr[7]);

            UInt256_10x26Tests.AssertEquality(x, pt.x);
            UInt256_10x26Tests.AssertEquality(y, pt.y);
        }

        [Fact]
        public void StaticMemberTest()
        {
            Assert.True(Point.Infinity.isInfinity);
            Assert.True(Point.Infinity.x.IsZero);
            Assert.True(Point.Infinity.y.IsZero);
        }



        #region https://github.com/bitcoin-core/secp256k1/blob/70f149b9a1bf4ed3266f97774d0ae9577534bf40/src/tests.c#L3626-L4079 + L4113-L4155

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
        private static void RandomXMagnitude(ref Point ge, TestRNG rng)
        {
            UInt256_10x26 x = ge.x;
            UInt256_10x26Tests.RandomFEMagnitude(ref x, SECP256K1_GE_X_MAGNITUDE_MAX, rng);
            ge = new(x, ge.y, ge.isInfinity);
        }

        /// <summary>
        /// random_ge_y_magnitude
        /// </summary>
        private static void RandomYMagnitude(ref Point ge, TestRNG rng)
        {
            UInt256_10x26 y = ge.y;
            UInt256_10x26Tests.RandomFEMagnitude(ref y, SECP256K1_GE_Y_MAGNITUDE_MAX, rng);
            ge = new(ge.x, y, ge.isInfinity);
        }

        /// <summary>
        /// random_gej_x_magnitude
        /// </summary>
        private static void RandomXMagnitude(ref PointJacobian gej, TestRNG rng)
        {
            UInt256_10x26 x = gej.x;
            UInt256_10x26Tests.RandomFEMagnitude(ref x, SECP256K1_GEJ_X_MAGNITUDE_MAX, rng);
            gej = new(x, gej.y, gej.z, gej.isInfinity);
        }

        /// <summary>
        /// random_gej_y_magnitude
        /// </summary>
        private static void RandomYMagnitude(ref PointJacobian gej, TestRNG rng)
        {
            UInt256_10x26 y = gej.y;
            UInt256_10x26Tests.RandomFEMagnitude(ref y, SECP256K1_GEJ_Y_MAGNITUDE_MAX, rng);
            gej = new(gej.x, y, gej.z, gej.isInfinity);
        }

        /// <summary>
        /// random_gej_z_magnitude
        /// </summary>
        private static void RandomZMagnitude(ref PointJacobian gej, TestRNG rng)
        {
            UInt256_10x26 z = gej.z;
            UInt256_10x26Tests.RandomFEMagnitude(ref z, SECP256K1_GEJ_Z_MAGNITUDE_MAX, rng);
            gej = new(gej.x, gej.y, z, gej.isInfinity);
        }

        /// <summary>
        /// testutil_random_fe_non_zero_test
        /// </summary>
        private static UInt256_10x26 RandomFENonZeroTest(TestRNG rng)
        {
            UInt256_10x26 fe;
            do
            {
                fe = UInt256_10x26Tests.RandomFETest(rng);
            } while (fe.IsZero);
            return fe;
        }

        /// <summary>
        /// testutil_random_ge_test
        /// </summary>
        internal static Point RandomGroupElementTest(TestRNG rng)
        {
            UInt256_10x26 fe;
            Point ge;
            do
            {
                fe = UInt256_10x26Tests.RandomFETest(rng);
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
        private static PointJacobian RandomGroupElementJacobianTest(in Point ge, TestRNG rng)
        {
            UInt256_10x26 z2, z3;
            UInt256_10x26 gejz = RandomFENonZeroTest(rng);
            z2 = gejz.Sqr();
            z3 = z2.Multiply(gejz);
            UInt256_10x26 gejx = ge.x.Multiply(z2);
            UInt256_10x26 gejy = ge.y.Multiply(z3);
            return new PointJacobian(gejx, gejy, gejz, ge.isInfinity);
        }

        /// <summary>
        /// testutil_random_gej_test
        /// </summary>
        private static PointJacobian RandomGejTest(TestRNG rng)
        {
            Point ge = RandomGroupElementTest(rng);
            PointJacobian gej = RandomGroupElementJacobianTest(ge, rng);
            return gej;
        }


        // This compares jacobian points including their Z, not just their geometric meaning.
        // gej_xyz_equals_gej
        private static int Gej_XYZ_EqualsGej(in PointJacobian a, in PointJacobian b)
        {
            PointJacobian a2;
            PointJacobian b2;
            int ret = 1;
            ret &= a.isInfinity == b.isInfinity ? 1 : 0;
            if (ret != 0 && !a.isInfinity)
            {
                a2 = a;
                b2 = b;
                UInt256_10x26 a2x = a2.x.Normalize();
                UInt256_10x26 a2y = a2.y.Normalize();
                UInt256_10x26 a2z = a2.z.Normalize();
                UInt256_10x26 b2x = b2.x.Normalize();
                UInt256_10x26 b2y = b2.y.Normalize();
                UInt256_10x26 b2z = b2.z.Normalize();
                ret &= a2x.CompareToVar(b2x) == 0 ? 1 : 0;
                ret &= a2y.CompareToVar(b2y) == 0 ? 1 : 0;
                ret &= a2z.CompareToVar(b2z) == 0 ? 1 : 0;
            }
            return ret;
        }

        // test_ge
        private static void TestGE(TestRNG rng)
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

            UInt256_10x26 zf, r;
            UInt256_10x26 zfi2, zfi3;

            gej[0] = PointJacobian.Infinity;
            ge[0] = Point.Infinity;
            for (int i = 0; i < runs; i++)
            {
                Point g = RandomGroupElementTest(rng);
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
                gej[2 + 4 * i] = RandomGroupElementJacobianTest(ge[2 + 4 * i], rng);
                gej[3 + 4 * i] = ge[3 + 4 * i].ToPointJacobian();
                gej[4 + 4 * i] = RandomGroupElementJacobianTest(ge[4 + 4 * i], rng);
                for (int j = 0; j < 4; j++)
                {
                    RandomXMagnitude(ref ge[1 + j + 4 * i], rng);
                    RandomYMagnitude(ref ge[1 + j + 4 * i], rng);
                    RandomXMagnitude(ref gej[1 + j + 4 * i], rng);
                    RandomYMagnitude(ref gej[1 + j + 4 * i], rng);
                    RandomZMagnitude(ref gej[1 + j + 4 * i], rng);
                }

                for (int j = 0; j < 4; ++j)
                {
                    for (int k = 0; k < 4; ++k)
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
            zf = RandomFENonZeroTest(rng);
            UInt256_10x26Tests.RandomFEMagnitude(ref zf, 8, rng);
            zfi3 = zf.InverseVar();
            zfi2 = zfi3.Sqr();
            zfi3 = zfi3.Multiply(zfi2);

            // Generate random r
            r = RandomFENonZeroTest(rng);

            for (int i1 = 0; i1 < 1 + 4 * runs; i1++)
            {
                for (int i2 = 0; i2 < 1 + 4 * runs; i2++)
                {
                    // Compute reference result using gej + gej (var).
                    PointJacobian refj, resj;
                    refj = gej[i1].AddVar(gej[i2], out UInt256_10x26 zr);
                    // Check Z ratio.
                    if (!gej[i1].isInfinity && !refj.isInfinity)
                    {
                        UInt256_10x26 zrz = zr.Multiply(gej[i1].z);
                        Assert.True(zrz.Equals(refj.z));
                    }
                    Point _ref = refj.ToPointVar();

                    // Test gej + ge with Z ratio result (var).
                    resj = gej[i1].AddVar(ge[i2], out zr);
                    Assert.True(resj.EqualsVar(_ref));
                    if (!gej[i1].isInfinity && !resj.isInfinity)
                    {
                        UInt256_10x26 zrz = zr.Multiply(gej[i1].z);
                        Assert.True(zrz.Equals(resj.z));
                    }

                    // Test gej + ge (var, with additional Z factor).
                    {
                        Point ge2_zfi = ge[i2]; // the second term with x and y rescaled for z = 1/zf
                        UInt256_10x26 tempx = ge2_zfi.x.Multiply(zfi2);
                        UInt256_10x26 tempy = ge2_zfi.y.Multiply(zfi3);
                        ge2_zfi = new(tempx, tempy, ge2_zfi.isInfinity);

                        RandomXMagnitude(ref ge2_zfi, rng);
                        RandomYMagnitude(ref ge2_zfi, rng);
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
                        resj = gej[i1].DoubleVar(out UInt256_10x26 zr2);
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
                    UInt256_10x26 s = UInt256_10x26Tests.RandomFENonZero(rng);
                    gej[i] = gej[i].Rescale(s);
                    Assert.True(gej[i].EqualsVar(ge_set_all_var[i]));
                }

                // Skip infinity at &gej[0].
                Point.SetAllPointsToJacobian(ge_set_all.Slice(1), gej.Slice(1));
                for (int i = 1; i < 4 * runs + 1; i++)
                {
                    UInt256_10x26 s = UInt256_10x26Tests.RandomFENonZero(rng);
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
                UInt256_10x26 n;
                Assert.True(Point.IsOnCurveVar(ge[i].x));
                // And the same holds after random rescaling.
                n = zf.Multiply(ge[i].x);
                Assert.True(Point.IsFracOnCurveVar(n, zf));
            }

            // Test correspondence of secp256k1_ge_x{,_frac}_on_curve_var with ge_set_xo.
            {
                UInt256_10x26 n = zf.Multiply(r);
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
                ge[i] = RandomGroupElementTest(rng);
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

        // test_intialized_inf
        private static void TestIntializedInf(TestRNG rng)
        {
            Point p;
            PointJacobian pj, npj, infj1, infj2, infj3;
            UInt256_10x26 zinv;

            // Test that adding P+(-P) results in a fully initialized infinity
            p = RandomGroupElementTest(rng);
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

            zinv = new UInt256_10x26(1);
            infj3 = npj.AddZInvVar(p, zinv);
            Assert.True(infj3.isInfinity);
            Assert.True(infj3.x.IsZero);
            Assert.True(infj3.y.IsZero);
            Assert.True(infj3.z.IsZero);
        }

        // test_add_neg_y_diff_x
        private static void TestAddNegYDiffX()
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
            PointJacobian aj = new(
                new UInt256_10x26(0xd58977cb, 0x2f8ec030, 0x05a59614, 0x0643d79f, 0x44238d30, 0x3c543505, 0x0a355af1, 0x8d24cd95),
                new UInt256_10x26(0x9190117d, 0x44e6d2f3, 0xd7681924, 0x4d72c879, 0x0b1293a8, 0x6c0f386d, 0x38093dcd, 0x001e337a),
                UInt256_10x26.One);
            PointJacobian bj = new(
                new UInt256_10x26(0xbf92d2a7, 0xd013bd7b, 0xf19a4ce9, 0x95f6ff75, 0x164a0d86, 0xabd0937d, 0x1f788cd9, 0xc7b74206),
                new UInt256_10x26(0x6e6feab2, 0xbb192d0b, 0x2897e6db, 0xb28d3786, 0xf4ed6c57, 0x93f0c792, 0xc7f6c232, 0xffe1cc85),
                UInt256_10x26.One);
            PointJacobian sumj = new(
                new UInt256_10x26(0x184a8f7a, 0x5c86d390, 0x278625c3, 0xb3d69010, 0x24356027, 0x389a7798, 0x3efdad4c, 0x671a63c0),
                new UInt256_10x26(0xbed8fbbe, 0x8f0d893c, 0x70e95caf, 0xda651801, 0x25071d08, 0x511fd375, 0x2ce01f2b, 0x5f6409c2),
                UInt256_10x26.One);

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

        private static void TestGeBytes(TestRNG rng)
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
                    p = RandomGroupElementTest(rng);
                }

                // Note that unlike libsecp256k1 we don't have 2 methods to convert to bytes
                // secp256k1_ge_to/from_bytes and secp256k1_ge_to/from_bytes_ext are the same
                // since we handle infinity differently
                p.ToByteArray(buf);
                q = new Point(buf);
                Assert.True(p.EqualsVar(q));
            }
        }

        [Fact]
        public void Libsecp256k1_GETest()
        {
            // run_ge

            TestRNG rng = new();
            rng.Init(null);

            for (int i = 0; i < COUNT * 32; i++)
            {
                TestGE(rng);
            }
            TestAddNegYDiffX();
            TestIntializedInf(rng);
            TestGeBytes(rng);
        }

        // test_gej_cmov
        static void TestGejCmov(in PointJacobian a, in PointJacobian b)
        {
            PointJacobian t = a;
            t = PointJacobian.CMov(t, b, 0);
            Assert.Equal(1, Gej_XYZ_EqualsGej(t, a));
            t = PointJacobian.CMov(t, b, 1);
            Assert.Equal(1, Gej_XYZ_EqualsGej(t, b));
        }

        [Fact]
        public void Libsecp256k1_GejTest()
        {
            // run_gej

            TestRNG rng = new();
            rng.Init(null);

            PointJacobian a, b;

            // Tests for secp256k1_gej_cmov
            for (int i = 0; i < COUNT; i++)
            {
                a = PointJacobian.Infinity;
                b = PointJacobian.Infinity;
                TestGejCmov(a, b);

                a = RandomGejTest(rng);
                TestGejCmov(a, b);
                TestGejCmov(b, a);

                b = a;
                TestGejCmov(a, b);

                b = RandomGejTest(rng);
                TestGejCmov(a, b);
                TestGejCmov(b, a);
            }

            // Tests for secp256k1_gej_eq_var
            for (int i = 0; i < COUNT; i++)
            {
                UInt256_10x26 fe;
                a = RandomGejTest(rng);
                b = RandomGejTest(rng);
                Assert.False(a.EqualsVar(b));

                b = a;
                fe = RandomFENonZeroTest(rng);
                a = a.Rescale(fe);
                Assert.True(a.EqualsVar(b));
            }
        }

        // test_group_decompress
        private static void TestGroupDecompress(in UInt256_10x26 x)
        {
            // The input itself, normalized.
            UInt256_10x26 fex = x;
            fex = fex.NormalizeVar();

            bool res_even = Point.TryCreateVar(fex, false, out Point ge_even);
            bool res_odd = Point.TryCreateVar(fex, true, out Point ge_odd);

            Assert.True(res_even == res_odd);

            if (res_even)
            {
                UInt256_10x26 normXOdd = ge_odd.x.NormalizeVar();
                UInt256_10x26 normXEven = ge_even.x.NormalizeVar();
                UInt256_10x26 normYOdd = ge_odd.y.NormalizeVar();
                UInt256_10x26 normYEven = ge_even.y.NormalizeVar();

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

        [Fact]
        public void Libsecp256k1_GroupDecompress()
        {
            // run_group_decompress
            TestRNG rng = new();
            rng.Init(null);
            for (int i = 0; i < COUNT * 4; i++)
            {
                UInt256_10x26 fe = UInt256_10x26Tests.RandomFETest(rng);
                TestGroupDecompress(fe);
            }
        }

        #endregion
    }
}
