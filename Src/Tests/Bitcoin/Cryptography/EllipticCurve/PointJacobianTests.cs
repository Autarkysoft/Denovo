// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class PointJacobianTests
    {
        internal static void AssertEquality(in PointJacobian expected, in PointJacobian actual)
        {
            UInt256_10x26Tests.AssertEquality(expected.x.Normalize(), actual.x.Normalize());
            UInt256_10x26Tests.AssertEquality(expected.y.Normalize(), actual.y.Normalize());
            UInt256_10x26Tests.AssertEquality(expected.z.Normalize(), actual.z.Normalize());
        }

        internal static void AssertEquality(in Point expected, in Point actual)
        {
            UInt256_10x26Tests.AssertEquality(expected.x.Normalize(), actual.x.Normalize());
            UInt256_10x26Tests.AssertEquality(expected.y.Normalize(), actual.y.Normalize());
        }

        private static Point CreateRandom()
        {
            for (int i = 0; i < 20; i++)
            {
                byte[] ba = Helper.CreateRandomBytes(32);
                UInt256_10x26 x = new(ba, out bool isValid);
                if (isValid && Point.TryCreateVar(x, true, out Point result))
                {
                    return result;
                }
            }
            throw new Exception("Something is wrong.");
        }

        private static UInt256_10x26 CreateRandomUint()
        {
            for (int i = 0; i < 20; i++)
            {
                byte[] ba = Helper.CreateRandomBytes(32);
                UInt256_10x26 x = new(ba, out bool isValid);
                if (isValid && !x.IsZero)
                {
                    return x;
                }
            }
            throw new Exception("Something is wrong.");
        }
        [Fact]
        public void Add_RandomTest()
        {
            Point pt1 = CreateRandom();
            Point pt2 = CreateRandom();
            PointJacobian pt1j = pt1.ToPointJacobian();
            PointJacobian pt2j = pt2.ToPointJacobian();

            PointJacobian rescaled = pt2j.Rescale(CreateRandomUint());

            Assert.True(pt2j.EqualsVar(rescaled));

            PointJacobian res1 = pt1j.Add(pt2);
            PointJacobian res2 = pt1j.AddVar(pt2, out UInt256_10x26 rzrA);
            PointJacobian res3 = pt1j.AddVar(pt2j, out UInt256_10x26 rzrB);
            PointJacobian res4 = pt1j.AddVar(rescaled, out _);

            AssertEquality(res1.ToPoint(), res2.ToPoint());
            AssertEquality(res1.ToPoint(), res3.ToPoint());
            AssertEquality(res1.ToPoint(), res4.ToPoint());
            Assert.True(res1.EqualsVar(res4));

            UInt256_10x26Tests.AssertEquality(rzrA.Normalize(), rzrB.Normalize());
        }


        // https://github.com/bitcoin-core/secp256k1/blob/70f149b9a1bf4ed3266f97774d0ae9577534bf40/src/group.h#L35
        internal static PointJacobian SECP256K1_GEJ_CONST(uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h,
                                                          uint i, uint j, uint k, uint l, uint m, uint n, uint o, uint p)
        {
            return new PointJacobian(
                UInt256_10x26Tests.SECP256K1_FE_CONST(a, b, c, d, e, f, g, h),
                UInt256_10x26Tests.SECP256K1_FE_CONST(i, j, k, l, m, n, o, p),
                UInt256_10x26Tests.SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 1),
                false);
        }
    }
}
