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

    }
}
