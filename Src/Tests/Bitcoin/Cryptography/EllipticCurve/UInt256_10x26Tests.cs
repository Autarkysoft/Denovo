// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class UInt256_10x26Tests
    {
        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(100, 1)]
        public void Constructor_FromUintTest(uint u, int expMagnitude)
        {
            UInt256_10x26 val = new(u);
            Assert.Equal(u, val.b0);
            Assert.Equal(0U, val.b1);
            Assert.Equal(0U, val.b2);
            Assert.Equal(0U, val.b3);
            Assert.Equal(0U, val.b4);
            Assert.Equal(0U, val.b5);
            Assert.Equal(0U, val.b6);
            Assert.Equal(0U, val.b7);
            Assert.Equal(0U, val.b8);
            Assert.Equal(0U, val.b9);
#if DEBUG
            Assert.True(val.isNormalized);
            Assert.Equal(expMagnitude, val.magnitude);
#endif
        }

        public static IEnumerable<object[]> GetCtor10Cases()
        {
            yield return new object[] { new uint[10], 0, true };
            yield return new object[]
            {
                new uint[10]
                {
                    0x006ce0e6, 0x02daf473, 0x02d6b793, 0x00a83843, 0x01343382,
                    0x0364d800, 0x0158067c, 0x03832e75, 0x00633f78, 0x003af47e
                },
                1, true
            };
        }
        [Theory]
        [MemberData(nameof(GetCtor10Cases))]
        public void Constructor_From10UintsTest(uint[] arr, int expMagnitude, bool expNormalize)
        {
            UInt256_10x26 val1 = new(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7], arr[8], arr[9]
#if DEBUG
                , expMagnitude, expNormalize
#endif
                );
            Assert.Equal(arr[0], val1.b0);
            Assert.Equal(arr[1], val1.b1);
            Assert.Equal(arr[2], val1.b2);
            Assert.Equal(arr[3], val1.b3);
            Assert.Equal(arr[4], val1.b4);
            Assert.Equal(arr[5], val1.b5);
            Assert.Equal(arr[6], val1.b6);
            Assert.Equal(arr[7], val1.b7);
            Assert.Equal(arr[8], val1.b8);
            Assert.Equal(arr[9], val1.b9);
#if DEBUG
            Assert.Equal(expNormalize, val1.isNormalized);
            Assert.Equal(expMagnitude, val1.magnitude);
#endif
        }

        [Theory]
        [MemberData(nameof(GetCtor10Cases))]
        public void Constructor_FromUintArrayTest(uint[] arr, int expMagnitude, bool expNormalize)
        {
            UInt256_10x26 val1 = new(arr
#if DEBUG
                , expMagnitude, expNormalize
#endif
                );
            Assert.Equal(arr[0], val1.b0);
            Assert.Equal(arr[1], val1.b1);
            Assert.Equal(arr[2], val1.b2);
            Assert.Equal(arr[3], val1.b3);
            Assert.Equal(arr[4], val1.b4);
            Assert.Equal(arr[5], val1.b5);
            Assert.Equal(arr[6], val1.b6);
            Assert.Equal(arr[7], val1.b7);
            Assert.Equal(arr[8], val1.b8);
            Assert.Equal(arr[9], val1.b9);
#if DEBUG
            Assert.Equal(expNormalize, val1.isNormalized);
            Assert.Equal(expMagnitude, val1.magnitude);
#endif
        }

        public static IEnumerable<object[]> GetCtor8Cases()
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
        [MemberData(nameof(GetCtor8Cases))]
        public void Constructor_From8UintsTest(uint[] arr, uint[] exp)
        {
            UInt256_10x26 val1 = new(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6], arr[7]);
            Assert.Equal(exp[0], val1.b0);
            Assert.Equal(exp[1], val1.b1);
            Assert.Equal(exp[2], val1.b2);
            Assert.Equal(exp[3], val1.b3);
            Assert.Equal(exp[4], val1.b4);
            Assert.Equal(exp[5], val1.b5);
            Assert.Equal(exp[6], val1.b6);
            Assert.Equal(exp[7], val1.b7);
            Assert.Equal(exp[8], val1.b8);
            Assert.Equal(exp[9], val1.b9);
#if DEBUG
            Assert.True(val1.isNormalized);
            Assert.Equal(1, val1.magnitude);
#endif
        }

        public static IEnumerable<object[]> GetCtorBytesCases()
        {
            yield return new object[] { new byte[32], new uint[10], true };
            yield return new object[]
            {
                new byte[32]
                {
                    0x85, 0x33, 0x8a, 0xb7, 0xd7, 0x3f, 0x69, 0xfe, 0x32, 0xb5, 0xfe, 0xf6, 0xa5, 0xdf, 0x65, 0x13,
                    0x74, 0xd4, 0x1d, 0xad, 0xd7, 0xb7, 0x9f, 0xf6, 0xd5, 0xc1, 0x91, 0x9e, 0x50, 0x02, 0x59, 0x1a
                },
                new uint[10]
                {
                    0x0002591a, 0x00646794, 0x01ff6d5c, 0x02b75ede, 0x0374d41d,
                    0x0177d944, 0x035fef6a, 0x01a7f8ca, 0x02b7d73f, 0x00214ce2
                },
                true
            };
            yield return new object[]
            {
                new byte[32]
                {
                    0xfc, 0x72, 0x4a, 0xca, 0x68, 0x39, 0x95, 0x68, 0xff, 0x54, 0xd2, 0xf8, 0x5e, 0x3e, 0xdd, 0x85,
                    0xca, 0xbb, 0x0d, 0x3d, 0x34, 0xd7, 0x8e, 0xc3, 0xf9, 0x11, 0xfd, 0xe9, 0xce, 0xac, 0x71, 0x21
                },
                new uint[10]
                {
                    0x02ac7121, 0x007f7a73, 0x00ec3f91, 0x00f4d35e, 0x01cabb0d,
                    0x038fb761, 0x014d2f85, 0x0255a3fd, 0x02ca6839, 0x003f1c92
                },
                true
            };
            yield return new object[] // P-1
            {
                new byte[32]
                {
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xff, 0xff, 0xfc, 0x2e
                },
                new uint[10]
                {
                    0x03fffc2e, 0x03ffffbf, 0x03ffffff, 0x03ffffff, 0x03ffffff,
                    0x03ffffff, 0x03ffffff, 0x03ffffff, 0x03ffffff, 0x003fffff
                },
                true
            };
            yield return new object[] // P
            {
                new byte[32]
                {
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xff, 0xff, 0xfc, 0x2f
                },
                new uint[10]
                {
                    0x03fffc2f, 0x03ffffbf, 0x03ffffff, 0x03ffffff, 0x03ffffff,
                    0x03ffffff, 0x03ffffff, 0x03ffffff, 0x03ffffff, 0x003fffff
                },
                false
            };
            yield return new object[] // UInt256.Max
            {
                new byte[32]
                {
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
                },
                new uint[10]
                {
                    0x03ffffff, 0x03ffffff, 0x03ffffff, 0x03ffffff, 0x03ffffff,
                    0x03ffffff, 0x03ffffff, 0x03ffffff, 0x03ffffff, 0x003fffff
                },
                false
            };
        }
        [Theory]
        [MemberData(nameof(GetCtorBytesCases))]
        public void Constructor_FromBytesTest(byte[] arr, uint[] exp, bool expB)
        {
            UInt256_10x26 val1 = new(arr, out bool isValid);
            Assert.Equal(exp[0], val1.b0);
            Assert.Equal(exp[1], val1.b1);
            Assert.Equal(exp[2], val1.b2);
            Assert.Equal(exp[3], val1.b3);
            Assert.Equal(exp[4], val1.b4);
            Assert.Equal(exp[5], val1.b5);
            Assert.Equal(exp[6], val1.b6);
            Assert.Equal(exp[7], val1.b7);
            Assert.Equal(exp[8], val1.b8);
            Assert.Equal(exp[9], val1.b9);
#if DEBUG
            Assert.Equal(expB, val1.isNormalized);
            Assert.Equal(1, val1.magnitude);
#endif
        }


        public static IEnumerable<object[]> GetStaticCases()
        {
            yield return new object[] { UInt256_10x26.Zero, 0 };
            yield return new object[] { UInt256_10x26.One, 1 };
            yield return new object[] { UInt256_10x26.Seven, 7 };
        }
        [Theory]
        [MemberData(nameof(GetStaticCases))]
        public void StaticMemberTest(UInt256_10x26 value, uint expected)
        {
            Assert.Equal(expected, value.b0);
            Assert.Equal(0U, value.b1);
            Assert.Equal(0U, value.b2);
            Assert.Equal(0U, value.b3);
            Assert.Equal(0U, value.b4);
            Assert.Equal(0U, value.b5);
            Assert.Equal(0U, value.b6);
            Assert.Equal(0U, value.b7);
            Assert.Equal(0U, value.b8);
            Assert.Equal(0U, value.b9);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(3, true)]
        public void IsOddTest(uint u, bool expected)
        {
            UInt256_10x26 value = new(u);
            Assert.Equal(expected, value.IsOdd);
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, true)]
        [InlineData(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 1, 0, 0, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 1, 0, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 1, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 1, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 1, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 1, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 1, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 1, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, 1, 1, false)]
        public void IsZeroTest(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7, uint u8, uint u9,
                               bool expected)
        {
            UInt256_10x26 value = new(u0, u1, u2, u3, u4, u5, u6, u7, u8, u9
#if DEBUG
                , 1, true
#endif
                );
            Assert.Equal(expected, value.IsZero);
        }
    }
}
