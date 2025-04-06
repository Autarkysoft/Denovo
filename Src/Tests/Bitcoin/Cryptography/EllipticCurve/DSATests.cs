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


        #endregion
    }
}
