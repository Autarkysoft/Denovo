// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class PointTests
    {
        internal static void AssertEquality(in Point expected, in Point actual)
        {
            UInt256_10x26Tests.AssertEquality(expected.x, actual.x);
            UInt256_10x26Tests.AssertEquality(expected.y, actual.y);
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


        public static IEnumerable<object[]> GetCreateTests()
        {
            UInt256_10x26 x1 = new(Helper.HexToBytes("01a8897cfa429fe49a4dc45e61a49ea3a3f174e655d040233957b9c9d59403cc"), out bool b);
            Assert.True(b);
            UInt256_10x26 x2 = new(Helper.HexToBytes("7d3d7adc755c2dffc7f75cc89b370f67385132ff101e1730686643a388d60f06"), out b);
            Assert.True(b);
            UInt256_10x26 y2Odd = new(Helper.HexToBytes("07bca1494689a0d39577808c86d5ca3a9d7d45621f99743d85b4e43ef107ab35"), out b);
            Assert.True(b);
            UInt256_10x26 y2Even = new(Helper.HexToBytes("f8435eb6b9765f2c6a887f73792a35c56282ba9de0668bc27a4b1bc00ef850fa"), out b);
            Assert.True(b);

            yield return new object[] { x1, true, false, Point.Infinity };
            yield return new object[] { x1, false, false, Point.Infinity };
            yield return new object[] { x2, true, true, new Point(x2, y2Odd) };
            yield return new object[] { x2, false, true, new Point(x2, y2Even) };
        }
        [Theory]
        [MemberData(nameof(GetCreateTests))]
        public void TryCreateVarTest(in UInt256_10x26 x, bool odd, bool expB, in Point expected)
        {
            bool actB = Point.TryCreateVar(x, odd, out Point actual);
            Assert.Equal(expB, actB);
            AssertEquality(expected, actual);
        }
    }
}
