// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class DSATests
    {
        #region https://github.com/bitcoin-core/secp256k1/blob/70f149b9a1bf4ed3266f97774d0ae9577534bf40/src/tests.c#L4157

        private const int COUNT = 16;

        /// <summary>
        /// test_pre_g_table
        /// </summary>
        private static void TestPreGTable(PointStorage[] pre_g, int n)
        {
            Assert.Equal(n, pre_g.Length);

            // Tests the pre_g / pre_g_128 tables for consistency.
            // For independent verification we take a "geometric" approach to verification.
            // We check that every entry is on-curve.
            // We check that for consecutive entries p and q, that p + gg - q = 0 by checking
            //  (1) p, gg, and -q are colinear.
            //  (2) p, gg, and -q are all distinct.
            // where gg is twice the generator, where the generator is the first table entry.
            //
            // Checking the table's generators are correct is done in run_ecmult_pre_g.
            PointJacobian g2;
            Point p, q, gg;
            UInt256_10x26 dpx, dpy, dqx, dqy;
            Assert.True(0 < n);

            p = pre_g[0].ToPoint();
            Assert.True(p.IsValidVar());

            g2 = p.ToPointJacobian();
            g2 = g2.DoubleVar(out _);
            gg = g2.ToPointVar();
            for (int i = 1; i < n; i++)
            {
                dpx = p.x.Negate(1); dpx = dpx.Add(gg.x); dpx = dpx.NormalizeWeak();
                dpy = p.y.Negate(1); dpy = dpy.Add(gg.y); dpy = dpy.NormalizeWeak();
                // Check that p is not equal to gg
                Assert.True(!dpx.IsZeroNormalizedVar() || !dpy.IsZeroNormalizedVar());

                q = pre_g[i].ToPoint();
                Assert.True(q.IsValidVar());

                dqx = q.x.Negate(1); dqx = dqx.Add(gg.x);
                dqy = q.y; dqy = dqy.Add(gg.y);
                // Check that -q is not equal to gg
                Assert.True(!dqx.IsZeroNormalizedVar() || !dqy.IsZeroNormalizedVar());

                // Check that -q is not equal to p
                Assert.True(!dpx.Equals(dqx) || !dpy.Equals(dqy));

                // Check that p, -q and gg are colinear
                dpx = dpx.Multiply(dqy);
                dpy = dpy.Multiply(dqx);
                Assert.True(dpx.Equals(dpy));

                p = q;
            }
        }

        [Fact]
        public void Libsecp256k1_EcMultPreG_Test()
        {
            // run_ecmult_pre_g

            PrecomputeEcMult.BuildTables(out PointStorage[] preG, out PointStorage[] preG128);

            PointStorage gs;
            PointJacobian gj;
            Point g;

            // Check that the pre_g and pre_g_128 tables are consistent.
            TestPreGTable(preG, PrecomputeEcMult.TableSize);
            TestPreGTable(preG128, PrecomputeEcMult.TableSize);

            // Check the first entry from the pre_g table.
            gs = Point.G.ToStorage();
            Assert.Equal(0U, PointStorageTests.Libsecp256k1_CmpVar(gs, preG[0]));

            // Check the first entry from the pre_g_128 table.
            gj = Point.G.ToPointJacobian();
            for (int i = 0; i < 128; i++)
            {
                gj = gj.DoubleVar(out _);
            }
            g = gj.ToPoint();
            gs = g.ToStorage();
            Assert.Equal(0U, PointStorageTests.Libsecp256k1_CmpVar(gs, preG128[0]));
        }


        [Fact]
        public void Libsecp256k1_EcmultChain_Test()
        {
            // run_ecmult_chain

            DSA dsa = new();

            // random starting point A (on the curve)
            PointJacobian a = PointJacobianTests.SECP256K1_GEJ_CONST(
                0x8b30bbe9, 0xae2a9906, 0x96b22f67, 0x0709dff3,
                0x727fd8bc, 0x04d3362c, 0x6c7bf458, 0xe2846004,
                0xa357ae91, 0x5c4a6528, 0x1309edf2, 0x0504740f,
                0x0eb33439, 0x90216b4f, 0x81063cb6, 0x5f2f7e0f
            );
            // two random initial factors xn and gn
            Scalar8x32 xn = Scalar8x32Tests.SECP256K1_SCALAR_CONST(
                0x84cc5452, 0xf7fde1ed, 0xb4d38a8c, 0xe9b1b84c,
                0xcef31f14, 0x6e569be9, 0x705d357a, 0x42985407
            );
            Scalar8x32 gn = Scalar8x32Tests.SECP256K1_SCALAR_CONST(
                0xa1e58d22, 0x553dcd42, 0xb2398062, 0x5d4c57a9,
                0x6e9323d4, 0x2b3152e5, 0xca2c3990, 0xedc7c9de
            );
            // two small multipliers to be applied to xn and gn in every iteration:
            Scalar8x32 xf = Scalar8x32Tests.SECP256K1_SCALAR_CONST(0, 0, 0, 0, 0, 0, 0, 0x1337);
            Scalar8x32 gf = Scalar8x32Tests.SECP256K1_SCALAR_CONST(0, 0, 0, 0, 0, 0, 0, 0x7113);
            // accumulators with the resulting coefficients to A and G
            Scalar8x32 ae = Scalar8x32.One;
            Scalar8x32 ge = Scalar8x32.Zero;
            // actual points
            PointJacobian x;
            PointJacobian x2;

            // the point being computed
            x = a;

            // Note the following for loop is to (200*COUNT == 3200) in libsecp256k1 so the vrify conditional
            // branch (i == 19999) is never entered to actually verify anything so we bumped it to (19999 + 1)
            // which will take longer to run but it will run the verify branch!
            for (int i = 0; i < 19999 + 1; i++)
            {
                // in each iteration, compute X = xn*X + gn*G;
                x = dsa.ECMult(x, xn, gn);
                // also compute ae and ge: the actual accumulated factors for A and G
                // if X was (ae*A+ge*G), xn*X + gn*G results in (xn*ae*A + (xn*ge+gn)*G)
                ae = ae.Multiply(xn);
                ge = ge.Multiply(xn);
                ge = ge.Add(gn, out _);
                // modify xn and gn
                xn = xn.Multiply(xf);
                gn = gn.Multiply(gf);

                // verify
                if (i == 19999)
                {
                    // expected result after 19999 iterations
                    PointJacobian rp = PointJacobianTests.SECP256K1_GEJ_CONST(
                        0xD6E96687, 0xF9B10D09, 0x2A6F3543, 0x9D86CEBE,
                        0xA4535D0D, 0x409F5358, 0x6440BD74, 0xB933E830,
                        0xB95CBCA2, 0xC77DA786, 0x539BE8FD, 0x53354D2D,
                        0x3B4F566A, 0xE6580454, 0x07ED6015, 0xEE1B2A88
                    );
                    Assert.True(rp.EqualsVar(x));
                }
            }
            // redo the computation, but directly with the resulting ae and ge coefficients:
            x2 = dsa.ECMult(a, ae, ge);
            Assert.True(x.EqualsVar(x2));
        }

        /// <summary>
        /// test_point_times_order
        /// </summary>
        private static void TestPointTimesOrder(PointJacobian point, DSA dsa, TestRNG rng)
        {
            // X * (point + G) + (order-X) * (pointer + G) = 0
            Scalar8x32 x;
            Scalar8x32 nx;
            PointJacobian res1, res2;
            Point res3;

            //byte[] pub = new byte[65];
            //int psize = 65;

            x = Scalar8x32Tests.CreateRandom(rng);
            nx = x.Negate();

            res1 = dsa.ECMult(point, x, x);     // calc res1 = x * point + x * G;
            res2 = dsa.ECMult(point, nx, nx);   // calc res2 = (order - x) * point + (order - x) * G;
            res1 = res1.AddVar(res2, out _);
            Assert.True(res1.isInfinity);
            res3 = res1.ToPoint();
            Assert.True(res3.isInfinity);
            Assert.False(res3.IsValidVar());

            // Note secp256k1_eckey_pubkey_serialize returns zero when point is infinity. We don't have that method.
            //Assert.True(secp256k1_eckey_pubkey_serialize(&res3, pub, &psize, 0) == 0);
            //psize = 65;
            //Assert.True(secp256k1_eckey_pubkey_serialize(&res3, pub, &psize, 1) == 0);

            // check zero/one edge cases
            res1 = dsa.ECMult(point, Scalar8x32.Zero, Scalar8x32.Zero);
            res3 = res1.ToPoint();
            Assert.True(res3.isInfinity);
            res1 = dsa.ECMult(point, Scalar8x32.One, Scalar8x32.Zero);
            res3 = res1.ToPoint();
            Assert.True(point.EqualsVar(res3));
            res1 = dsa.ECMult(point, Scalar8x32.Zero, Scalar8x32.One);
            res3 = res1.ToPoint();
            Assert.True(Point.G.EqualsVar(res3));
        }

        // These scalars reach large (in absolute value) outputs when fed to secp256k1_scalar_split_lambda.
        //
        // They are computed as:
        // - For a in [-2, -1, 0, 1, 2]:
        //   - For b in [-3, -1, 1, 3]:
        //     - Output (a*LAMBDA + (ORDER+b)/2) % ORDER
        private readonly static Scalar8x32[] scalars_near_split_bounds = new Scalar8x32[20]
        {
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0xd938a566, 0x7f479e3e, 0xb5b3c7fa, 0xefdb3749, 0x3aa0585c, 0xc5ea2367, 0xe1b660db, 0x0209e6fc),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0xd938a566, 0x7f479e3e, 0xb5b3c7fa, 0xefdb3749, 0x3aa0585c, 0xc5ea2367, 0xe1b660db, 0x0209e6fd),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0xd938a566, 0x7f479e3e, 0xb5b3c7fa, 0xefdb3749, 0x3aa0585c, 0xc5ea2367, 0xe1b660db, 0x0209e6fe),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0xd938a566, 0x7f479e3e, 0xb5b3c7fa, 0xefdb3749, 0x3aa0585c, 0xc5ea2367, 0xe1b660db, 0x0209e6ff),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x2c9c52b3, 0x3fa3cf1f, 0x5ad9e3fd, 0x77ed9ba5, 0xb294b893, 0x3722e9a5, 0x00e698ca, 0x4cf7632d),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x2c9c52b3, 0x3fa3cf1f, 0x5ad9e3fd, 0x77ed9ba5, 0xb294b893, 0x3722e9a5, 0x00e698ca, 0x4cf7632e),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x2c9c52b3, 0x3fa3cf1f, 0x5ad9e3fd, 0x77ed9ba5, 0xb294b893, 0x3722e9a5, 0x00e698ca, 0x4cf7632f),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x2c9c52b3, 0x3fa3cf1f, 0x5ad9e3fd, 0x77ed9ba5, 0xb294b893, 0x3722e9a5, 0x00e698ca, 0x4cf76330),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x7fffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xd576e735, 0x57a4501d, 0xdfe92f46, 0x681b209f),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x7fffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xd576e735, 0x57a4501d, 0xdfe92f46, 0x681b20a0),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x7fffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xd576e735, 0x57a4501d, 0xdfe92f46, 0x681b20a1),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x7fffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xd576e735, 0x57a4501d, 0xdfe92f46, 0x681b20a2),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0xd363ad4c, 0xc05c30e0, 0xa5261c02, 0x88126459, 0xf85915d7, 0x7825b696, 0xbeebc5c2, 0x833ede11),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0xd363ad4c, 0xc05c30e0, 0xa5261c02, 0x88126459, 0xf85915d7, 0x7825b696, 0xbeebc5c2, 0x833ede12),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0xd363ad4c, 0xc05c30e0, 0xa5261c02, 0x88126459, 0xf85915d7, 0x7825b696, 0xbeebc5c2, 0x833ede13),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0xd363ad4c, 0xc05c30e0, 0xa5261c02, 0x88126459, 0xf85915d7, 0x7825b696, 0xbeebc5c2, 0x833ede14),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x26c75a99, 0x80b861c1, 0x4a4c3805, 0x1024c8b4, 0x704d760e, 0xe95e7cd3, 0xde1bfdb1, 0xce2c5a42),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x26c75a99, 0x80b861c1, 0x4a4c3805, 0x1024c8b4, 0x704d760e, 0xe95e7cd3, 0xde1bfdb1, 0xce2c5a43),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x26c75a99, 0x80b861c1, 0x4a4c3805, 0x1024c8b4, 0x704d760e, 0xe95e7cd3, 0xde1bfdb1, 0xce2c5a44),
            Scalar8x32Tests.SECP256K1_SCALAR_CONST(0x26c75a99, 0x80b861c1, 0x4a4c3805, 0x1024c8b4, 0x704d760e, 0xe95e7cd3, 0xde1bfdb1, 0xce2c5a45)
        };

        /// <summary>
        /// test_ecmult_target
        /// </summary>
        private static void TestEcmultTarget(in Scalar8x32 target, int mode, DSA dsa, TestRNG rng)
        {
            // Mode: 0=ecmult_gen, 1=ecmult, 2=ecmult_const
            Scalar8x32 n1, n2;
            Point p;
            PointJacobian pj, p1j, p2j, ptj;

            // Generate random n1,n2 such that n1+n2 = -target.
            n1 = Scalar8x32Tests.CreateRandom(rng);
            n2 = n1.Add(target, out _);
            n2 = n2.Negate();

            // Generate a random input point.
            //if (mode != 0)
            {
                p = PointTests.RandomGroupElementTest(rng);
                pj = p.ToPointJacobian();
            }

            // EC multiplications
            if (mode == 0)
            {
                //secp256k1_ecmult_gen(&CTX->ecmult_gen_ctx, &p1j, &n1);
                //secp256k1_ecmult_gen(&CTX->ecmult_gen_ctx, &p2j, &n2);
                //secp256k1_ecmult_gen(&CTX->ecmult_gen_ctx, &ptj, target);
            }
            else if (mode == 1)
            {
                p1j = dsa.ECMult(pj, n1, Scalar8x32.Zero);
                p2j = dsa.ECMult(pj, n2, Scalar8x32.Zero);
                ptj = dsa.ECMult(pj, target, Scalar8x32.Zero);

                // TODO: remove following 3 lines if other 2 branches were implemented and uncomment them at the end
                ptj = ptj.AddVar(p1j, out _);
                ptj = ptj.AddVar(p2j, out _);
                Assert.True(ptj.isInfinity);
            }
            else
            {
                //secp256k1_ecmult_const(&p1j, &p, &n1);
                //secp256k1_ecmult_const(&p2j, &p, &n2);
                //secp256k1_ecmult_const(&ptj, &p, target);
            }

            // Add them all up: n1*P + n2*P + target*P = (n1+n2+target)*P = (n1+n1-n1-n2)*P = 0.
            //ptj = ptj.AddVar(p1j, out _);
            //ptj = ptj.AddVar(p2j, out _);
            //Assert.True(ptj.isInfinity);
        }

        [Fact]
        public void Libsecp256k1_EcmultNearSplitBound_Test()
        {
            // run_ecmult_near_split_bound

            DSA dsa = new();
            TestRNG rng = new();
            rng.Init(null);

            for (int i = 0; i < 4 * COUNT; ++i)
            {
                for (int j = 0; j < scalars_near_split_bounds.Length; j++)
                {
                    TestEcmultTarget(scalars_near_split_bounds[j], 0, dsa, rng);
                    TestEcmultTarget(scalars_near_split_bounds[j], 1, dsa, rng);
                    TestEcmultTarget(scalars_near_split_bounds[j], 2, dsa, rng);
                }
            }
        }

        [Fact]
        public void Libsecp256k1_PointTimesOrder_Test()
        {
            // run_point_times_order

            DSA dsa = new();
            TestRNG rng = new();
            rng.Init(null);

            UInt256_10x26 x = UInt256_10x26Tests.SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 2);
            UInt256_10x26 xr = UInt256_10x26Tests.SECP256K1_FE_CONST(
                0x7603CB59, 0xB0EF6C63, 0xFE608479, 0x2A0C378C,
                0xDB3233A8, 0x0F8A9A09, 0xA877DEAD, 0x31B38C45
            );
            for (int i = 0; i < 500; i++)
            {
                if (Point.TryCreateVar(x, true, out Point p))
                {
                    Assert.True(p.IsValidVar());
                    PointJacobian j = p.ToPointJacobian();
                    TestPointTimesOrder(j, dsa, rng);
                }
                x = x.Sqr();
            }
            x = x.NormalizeVar();
            Assert.True(x.Equals(xr));
        }

        #endregion
    }
}
