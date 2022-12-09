// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Collections.Generic;
using Xunit;

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

        [Fact]
        public void Add_RandomTest()
        {
            Point pt1 = CreateRandom();
            Point pt2 = CreateRandom();
            PointJacobian pt1j = pt1.ToPointJacobian();
            PointJacobian pt2j = pt2.ToPointJacobian();

            PointJacobian res1 = pt1j.Add(pt2);
            PointJacobian res2 = pt1j.AddVar(pt2, out UInt256_10x26 rzrA);
            PointJacobian res3 = pt1j.AddVar(pt2j, out UInt256_10x26 rzrB);

            AssertEquality(res1.ToPoint(), res2.ToPoint());
            AssertEquality(res1.ToPoint(), res3.ToPoint());

            UInt256_10x26Tests.AssertEquality(rzrA.Normalize(), rzrB.Normalize());
        }

        public static IEnumerable<object[]> GetAddVarCases()
        {
            UInt256_10x26 x1 = new(0x1c62e3c2, 0x1acab440, 0x1df5bb35, 0x1db73f9f, 0x1cbdf064, 0x1dc09f43, 0x177e5c5d, 0x19893f43, 0x1b88acf4, 0x01d87069
#if DEBUG
                , 5, false
#endif
                );
            UInt256_10x26 y1 = new(0x1127dc4b, 0x0e194111, 0x0e3f29c8, 0x0e387961, 0x0f09545c, 0x125ceb1c, 0x0e8b73a3, 0x11947f3e, 0x0ccc2506, 0x01077fae
#if DEBUG
                , 3, false
#endif
                );
            UInt256_10x26 z1 = new(0x01e51e8e, 0x0048482b, 0x03f8bbb5, 0x01102293, 0x03ef9ff8, 0x00adbd64, 0x03611fc2, 0x039a2b4d, 0x029e7726, 0x00109fa5
#if DEBUG
                , 1, false
#endif
                );
            UInt256_10x26 x2 = new(0x02f81798, 0x00a056c5, 0x028d959f, 0x036cb738, 0x03029bfc, 0x03a1c2c1, 0x0206295c, 0x02eeb156, 0x027ef9dc, 0x001e6f99
#if DEBUG
                , 1, true
#endif
                );
            UInt256_10x26 y2 = new(0x0310d4b8, 0x01f423fe, 0x014199c4, 0x01229a15, 0x00fd17b4, 0x0384422a, 0x024fbfc0, 0x03119576, 0x027726a3, 0x00120eb6
#if DEBUG
                , 1, true
#endif
                );
            UInt256_10x26 z2 = UInt256_10x26.One;
            UInt256_10x26 x3 = new(0x19cb4e44, 0x17c1dae7, 0x15faa10e, 0x1bbd4e12, 0x1a89e48d, 0x1833f37a, 0x1c8f7b29, 0x1f47d6bd, 0x1a7871a7, 0x01ff9baf
#if DEBUG
                , 5, false
#endif
                );
            UInt256_10x26 y3 = new(0x10177d5a, 0x0f78e254, 0x12e57081, 0x0f9f5f36, 0x0df8e421, 0x1273cd44, 0x10174bbe, 0x12a9eb21, 0x10bdcbb4, 0x00e62f89
#if DEBUG
                , 3, false
#endif
                );
            UInt256_10x26 z3 = new(0x02bb8fa8, 0x02f599ec, 0x0274c0f9, 0x02019ab0, 0x002f2568, 0x00e946e8, 0x00bcfe47, 0x03368df0, 0x013be3cd, 0x00396ac0
#if DEBUG
                , 1, false
#endif
                );
            UInt256_10x26 rzr = new(0x008772a1, 0x01a66d62, 0x00a5afdd, 0x0267abfd, 0x02be1065, 0x01bb4e6e, 0x032cfa3a, 0x03f2afe8, 0x01812fdf, 0x000662f1
#if DEBUG
                , 1, false
#endif
                );

            PointJacobian a = new(x1, y1, z1);
            PointJacobian b = new(x2, y2, z2);
            PointJacobian sumAB = new(x3, y3, z3);

            yield return new object[] { a, b, sumAB, rzr };
        }
        [Theory]
        [MemberData(nameof(GetAddVarCases))]
        public void AddVarTest(in PointJacobian a, in PointJacobian b, in PointJacobian expected, in UInt256_10x26 expRZR)
        {
            PointJacobian actual = a.AddVar(b, out UInt256_10x26 rzr);
            AssertEquality(expected, actual);
            UInt256_10x26Tests.AssertEquality(expRZR, rzr);
        }


        public static IEnumerable<object[]> GetAddCases()
        {
            UInt256_10x26 x1 = new(0x08a93eac, 0x07843910, 0x01765b8c, 0x0d2b9710, 0x0137efe0, 0x0741ee00, 0x0534a948, 0x04cb9ec4, 0x0c8daf68, 0x00e87df4
#if DEBUG
                , 4, false
#endif
                );
            UInt256_10x26 y1 = new(0x01690b68, 0x0bc74e08, 0x0265aae8, 0x04f08234, 0x0c866bb4, 0x012f9d24, 0x01bf0588, 0x0e351974, 0x00eb11b4, 0x001931e4
#if DEBUG
                , 4, false
#endif
                );
            UInt256_10x26 z1 = new(0x04769af6, 0x051ac2de, 0x075a1d78, 0x07eb74aa, 0x0012b408, 0x02d24f00, 0x02ac49c4, 0x03a10bce, 0x062eace8, 0x0067134e
#if DEBUG
                , 2, false
#endif
                );
            UInt256_10x26 x2 = new(0x024f74c0, 0x00178110, 0x03a6cb75, 0x03e185b7, 0x005c2dae, 0x0322f024, 0x00f4612a, 0x03961ff8, 0x01ab68df, 0x0007a65c
#if DEBUG
                , 1, true
#endif
                );
            UInt256_10x26 y2 = new(0x008918b5, 0x01c443f0, 0x02cd0e53, 0x0351da42, 0x025fcbdd, 0x01eb9c28, 0x00a1e615, 0x02c979af, 0x01186214, 0x0013a708
#if DEBUG
                , 1, true
#endif
                );

            UInt256_10x26 x3 = new(0x085d7308, 0x0aaa966c, 0x0b897084, 0x088c8e88, 0x06cd34b4, 0x0fe04758, 0x02ddd3b0, 0x07bcb578, 0x079e451c, 0x000101f8
#if DEBUG
                , 4, false
#endif
                );
            UInt256_10x26 y3 = new(0x0adc2308, 0x0ddeb3b0, 0x0f574f2c, 0x03c3e4a8, 0x094c9a78, 0x00247fdc, 0x029dcdd8, 0x05614904, 0x003669f0, 0x00bd183c
#if DEBUG
                , 4, false
#endif
                );
            UInt256_10x26 z3 = new(0x02ec27ac, 0x06ac145a, 0x0257f2d6, 0x00467278, 0x0499f274, 0x05edf0aa, 0x060bc252, 0x03f11e9e, 0x05f168ca, 0x0010e54a
#if DEBUG
                , 2, false
#endif
                );

            PointJacobian a = new(x1, y1, z1);
            Point b = new(x2, y2);
            PointJacobian sumAB = new(x3, y3, z3);

            yield return new object[] { a, b, sumAB };
        }
        [Theory]
        [MemberData(nameof(GetAddCases))]
        public void AddTest(in PointJacobian a, in Point b, in PointJacobian expected)
        {
            PointJacobian actual1 = a + b;
            AssertEquality(expected.ToPoint(), actual1.ToPoint());

            PointJacobian actual2 = a.Add(b);
            AssertEquality(expected.ToPoint(), actual2.ToPoint());

            PointJacobian actual3 = a.AddVar(b, out _);
            AssertEquality(expected.ToPoint(), actual3.ToPoint());
        }
    }
}
