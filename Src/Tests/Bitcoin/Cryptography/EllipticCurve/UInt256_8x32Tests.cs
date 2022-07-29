// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class UInt256_8x32Tests
    {
        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0)]
        [InlineData(0xc78cbb72, 0x4a372272, 0x4252b11e, 0x4f702abe, 0x0b84feb6, 0x37790825, 0x608d2c16, 0xd3477069)]
        [InlineData(uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue)]
        public void ConstructorTest(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7)
        {
            UInt256_8x32 value = new(u0, u1, u2, u3, u4, u5, u6, u7);
            Assert.Equal(u0, value.b0);
            Assert.Equal(u1, value.b1);
            Assert.Equal(u2, value.b2);
            Assert.Equal(u3, value.b3);
            Assert.Equal(u4, value.b4);
            Assert.Equal(u5, value.b5);
            Assert.Equal(u6, value.b6);
            Assert.Equal(u7, value.b7);
        }


        public static IEnumerable<object[]> GetCtorCases()
        {
            yield return new object[] { new uint[8], new uint[10] };
            yield return new object[]
            {
                new uint[8]
                {
                    0xaa29e005, 0x0e02cfe1, 0xd1dd4aa6, 0xd43e9368, 0x7b8973c2, 0x62e10154, 0xf8ce846a, 0x2786a706
                },
                new uint[10]
                {
                    0x0229e005, 0x00b3f86a, 0x00aa60e0, 0x01a34775, 0x02d43e93,
                    0x02e25cf0, 0x02101547, 0x0211a98b, 0x0306f8ce, 0x0009e1a9
                },
            };
        }
        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void ToUInt256_10x26Test(uint[] arr, uint[] exp)
        {
            UInt256_8x32 value = new(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7]);
            UInt256_10x26 actual = value.ToUInt256_10x26();
            Assert.Equal(exp[0], actual.b0);
            Assert.Equal(exp[1], actual.b1);
            Assert.Equal(exp[2], actual.b2);
            Assert.Equal(exp[3], actual.b3);
            Assert.Equal(exp[4], actual.b4);
            Assert.Equal(exp[5], actual.b5);
            Assert.Equal(exp[6], actual.b6);
            Assert.Equal(exp[7], actual.b7);
            Assert.Equal(exp[8], actual.b8);
            Assert.Equal(exp[9], actual.b9);
        }
    }
}
