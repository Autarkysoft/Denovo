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
    public class PointJacobianTests
    {
        // https://github.com/bitcoin-core/secp256k1/blob/46db787112beabdb5e17e0dc35680716f1057e7b/src/group.h#L35
        internal static PointJacobian SECP256K1_GEJ_CONST(uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h,
                                                          uint i, uint j, uint k, uint l, uint m, uint n, uint o, uint p)
        {
            return new PointJacobian(
                UInt256_5x52Tests.SECP256K1_FE_CONST(a, b, c, d, e, f, g, h),
                UInt256_5x52Tests.SECP256K1_FE_CONST(i, j, k, l, m, n, o, p),
                UInt256_5x52Tests.SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 1),
                false);
        }

        internal static void AssertEqual(in PointJacobian expected, in PointJacobian actual)
        {
            UInt256_5x52Tests.AssertEqual(expected.x.Normalize(), actual.x.Normalize());
            UInt256_5x52Tests.AssertEqual(expected.y.Normalize(), actual.y.Normalize());
            UInt256_5x52Tests.AssertEqual(expected.z.Normalize(), actual.z.Normalize());
        }

        internal static void AssertEqual(in Point expected, in Point actual)
        {
            UInt256_5x52Tests.AssertEqual(expected.x.Normalize(), actual.x.Normalize());
            UInt256_5x52Tests.AssertEqual(expected.y.Normalize(), actual.y.Normalize());
        }


        public static IEnumerable<TheoryDataRow<ulong[], ulong[], ulong[]>> GetCtorCases()
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
                },
                new ulong[4]
                {
                    0x9ec7353b4ffc37c1, 0x27c6bcb5f35e29c8, 0x78a61937135c3850, 0xae10a8db11c88bac
                }
            );
        }
        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void Constructor_FromUInt256_5x52Test(ulong[] xArr, ulong[] yArr, ulong[] zArr)
        {
            UInt256_5x52 x = new(xArr[0], xArr[1], xArr[2], xArr[3]);
            UInt256_5x52 y = new(yArr[0], yArr[1], yArr[2], yArr[3]);
            UInt256_5x52 z = new(zArr[0], zArr[1], zArr[2], zArr[3]);

            PointJacobian pt = new(x, y, z);

            UInt256_5x52Tests.AssertEqual(x, pt.x);
            UInt256_5x52Tests.AssertEqual(y, pt.y);
            UInt256_5x52Tests.AssertEqual(z, pt.z);
            Assert.False(pt.isInfinity);

            pt = new(x, y, z, true);
            UInt256_5x52Tests.AssertEqual(x, pt.x);
            UInt256_5x52Tests.AssertEqual(y, pt.y);
            UInt256_5x52Tests.AssertEqual(z, pt.z);
            Assert.True(pt.isInfinity); // This ctor sets the isInfinity field
        }

        [Fact]
        public void StaticMemberTest()
        {
            Assert.True(PointJacobian.Infinity.x.IsZero);
            Assert.True(PointJacobian.Infinity.y.IsZero);
            Assert.True(PointJacobian.Infinity.z.IsZero);
            Assert.True(PointJacobian.Infinity.isInfinity);
        }

        private static Point CreateRandom()
        {
            for (int i = 0; i < 20; i++)
            {
                byte[] ba = Helper.CreateRandomBytes(32);
                UInt256_5x52 x = new(ba, out bool isValid);
                if (isValid && Point.TryCreateVar(x, true, out Point result))
                {
                    return result;
                }
            }
            throw new Exception("Something is wrong.");
        }

        private static UInt256_5x52 CreateRandomUint()
        {
            for (int i = 0; i < 20; i++)
            {
                byte[] ba = Helper.CreateRandomBytes(32);
                UInt256_5x52 x = new(ba, out bool isValid);
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
            PointJacobian res2 = pt1j.AddVar(pt2, out UInt256_5x52 rzrA);
            PointJacobian res3 = pt1j.AddVar(pt2j, out UInt256_5x52 rzrB);
            PointJacobian res4 = pt1j.AddVar(rescaled, out _);

            AssertEqual(res1.ToPoint(), res2.ToPoint());
            AssertEqual(res1.ToPoint(), res3.ToPoint());
            AssertEqual(res1.ToPoint(), res4.ToPoint());
            Assert.True(res1.EqualsVar(res4));

            UInt256_5x52Tests.AssertEqual(rzrA.Normalize(), rzrB.Normalize());
        }
    }
}
