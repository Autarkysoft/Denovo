// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class Scalar8x32Tests
    {
        private const uint N0 = 0xD0364141U;
        private const uint N1 = 0xBFD25E8CU;
        private const uint N2 = 0xAF48A03BU;
        private const uint N3 = 0xBAAEDCE6U;
        private const uint N4 = 0xFFFFFFFEU;
        private const uint N5 = 0xFFFFFFFFU;
        private const uint N6 = 0xFFFFFFFFU;
        private const uint N7 = 0xFFFFFFFFU;

        private const ulong NN0 = 0xBFD25E8CD0364141UL;
        private const ulong NN1 = 0xBAAEDCE6AF48A03BUL;
        private const ulong NN2 = 0xFFFFFFFFFFFFFFFEUL;
        private const ulong NN3 = 0xFFFFFFFFFFFFFFFFUL;

        [Fact]
        public void Constructor_uintTest()
        {
            uint val = 0x7b54de26U;
            Scalar8x32 scalar = new(val);

            Assert.Equal(val, scalar.b0);
            Assert.Equal(0U, scalar.b1);
            Assert.Equal(0U, scalar.b2);
            Assert.Equal(0U, scalar.b3);
            Assert.Equal(0U, scalar.b4);
            Assert.Equal(0U, scalar.b5);
            Assert.Equal(0U, scalar.b6);
            Assert.Equal(0U, scalar.b7);
        }

        [Fact]
        public void Constructor_uintsTest()
        {
            uint u0 = 0x96bb33d3U; uint u1 = 0x85a1c49aU; uint u2 = 0x1d9d3d5cU; uint u3 = 0x4a47e5d4U;
            uint u4 = 0x5b237147U; uint u5 = 0x0b6d30a8U; uint u6 = 0xc6951fd6U; uint u7 = 0xe86618d0U;
            Scalar8x32 scalar = new(u0, u1, u2, u3, u4, u5, u6, u7);

            Assert.Equal(u0, scalar.b0);
            Assert.Equal(u1, scalar.b1);
            Assert.Equal(u2, scalar.b2);
            Assert.Equal(u3, scalar.b3);
            Assert.Equal(u4, scalar.b4);
            Assert.Equal(u5, scalar.b5);
            Assert.Equal(u6, scalar.b6);
            Assert.Equal(u7, scalar.b7);
        }

        [Fact]
        public void Constructor_uintArrayTest()
        {
            uint u0 = 0x96bb33d3U; uint u1 = 0x85a1c49aU; uint u2 = 0x1d9d3d5cU; uint u3 = 0x4a47e5d4U;
            uint u4 = 0x5b237147U; uint u5 = 0x0b6d30a8U; uint u6 = 0xc6951fd6U; uint u7 = 0xe86618d0U;
            Scalar8x32 scalar = new(new uint[] { u0, u1, u2, u3, u4, u5, u6, u7 });

            Assert.Equal(u0, scalar.b0);
            Assert.Equal(u1, scalar.b1);
            Assert.Equal(u2, scalar.b2);
            Assert.Equal(u3, scalar.b3);
            Assert.Equal(u4, scalar.b4);
            Assert.Equal(u5, scalar.b5);
            Assert.Equal(u6, scalar.b6);
            Assert.Equal(u7, scalar.b7);
        }

        public static IEnumerable<object[]> GetUintCases()
        {
            yield return new object[] { new uint[8], false };
            yield return new object[]
            {
                new uint[8]
                {
                    0x96bb33d3U, 0x85a1c49aU, 0x1d9d3d5cU, 0x4a47e5d4U, 0x5b237147U, 0x0b6d30a8U, 0xc6951fd6U, 0xe86618d0U
                },
                false
            };
            yield return new object[] { new uint[8] { N7, N6, N5, N4, N3, N2, N1, N0 - 1 }, false }; // N-1
            yield return new object[] { new uint[8] { N7, N6, N5, N4, N3, N2, N1, N0 }, true }; // N
            yield return new object[] { new uint[8] { N7, N6, N5, N4, N3, N2, N1, N0 + 1 }, true }; // N + 1
            yield return new object[] { new uint[8] { N7, N6, N5, N4 + 1, N3, N2, N1, N0 }, true };
            yield return new object[] { new uint[8] { N7, N6, N5, N4, N3 + 1, N2, N1, N0 }, true };
            yield return new object[] { new uint[8] { N7, N6, N5, N4, N3, N2 + 1, N1, N0 }, true };
            yield return new object[] { new uint[8] { N7, N6, N5, N4, N3, N2, N1 + 1, N0 }, true };
            yield return new object[] { new uint[8] { N7, N6, N5, N4, N3, N2, N1 + 1, N0 - 1 }, true };
            yield return new object[] { new uint[8] { N7, N6, N5, N4, N3, N2 - 1, N1 + 1, N0 }, false };
            yield return new object[]
            {
                new uint[8]
                {
                    uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue
                },
                true
            };
        }
        [Theory]
        [MemberData(nameof(GetUintCases))]
        public unsafe void Constructor_uintPointerTest(uint[] array, bool expected)
        {
            fixed (uint* pt = &array[0])
            {
                Scalar8x32 scalar = new(pt, out bool actual);

                Assert.Equal(expected, actual);
                Assert.Equal(array[7], scalar.b0);
                Assert.Equal(array[6], scalar.b1);
                Assert.Equal(array[5], scalar.b2);
                Assert.Equal(array[4], scalar.b3);
                Assert.Equal(array[3], scalar.b4);
                Assert.Equal(array[2], scalar.b5);
                Assert.Equal(array[1], scalar.b6);
                Assert.Equal(array[0], scalar.b7);
            }
        }

        public static IEnumerable<object[]> GetUlongCases()
        {
            yield return new object[] { new ulong[4], new uint[8], false };
            yield return new object[]
            {
                new ulong[4]
                {
                    0x96bb33d385a1c49aU, 0x1d9d3d5c4a47e5d4U, 0x5b2371470b6d30a8U, 0xc6951fd6e86618d0U
                },
                new uint[8]
                {
                    0x96bb33d3U, 0x85a1c49aU, 0x1d9d3d5cU, 0x4a47e5d4U, 0x5b237147U, 0x0b6d30a8U, 0xc6951fd6U, 0xe86618d0U
                },
                false
            };
            yield return new object[]
            {
                new ulong[4] { NN3, NN2, NN1, NN0 - 1 },
                new uint[8] { N7, N6, N5, N4, N3, N2, N1, N0 - 1 },
                false
            }; // N-1
            yield return new object[]
            {
                new ulong[4] { NN3, NN2, NN1, NN0 },
                new uint[8] { N7, N6, N5, N4, N3, N2, N1, N0 },
                true
            }; // N
            yield return new object[]
            {
                new ulong[4] { NN3, NN2, NN1, NN0 + 1 },
                new uint[8] { N7, N6, N5, N4, N3, N2, N1, N0 + 1 },
                true
            }; // N + 1
            yield return new object[]
            {
                new ulong[4] { NN3, NN2, NN1 + 1, NN0 },
                new uint[8] { N7, N6, N5, N4, N3, N2 + 1, N1, N0 },
                true
            };
            yield return new object[]
            {
                new ulong[4] { NN3, NN2 + 1, NN1, NN0 },
                new uint[8] { N7, N6, N5, N4 + 1, N3, N2, N1, N0 },
                true
            };
            yield return new object[]
            {
                new ulong[4] { NN3, NN2 + 1, NN1, (ulong)N1 << 32 },
                new uint[8] { N7, N6, N5, N4 + 1, N3, N2, N1, 0 },
                true
            };
            yield return new object[]
            {
                new ulong[4] { ulong.MaxValue, ulong.MaxValue, ulong.MaxValue, ulong.MaxValue },
                new uint[8]
                {
                    uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue
                },
                true
            };
        }
        [Theory]
        [MemberData(nameof(GetUlongCases))]
        public unsafe void Constructor_ulongPointerTest(ulong[] array, uint[] exp, bool b)
        {
            fixed (ulong* pt = &array[0])
            {
                Scalar8x32 scalar = new(pt, out bool actual);

                Assert.Equal(b, actual);
                Assert.Equal(exp[7], scalar.b0);
                Assert.Equal(exp[6], scalar.b1);
                Assert.Equal(exp[5], scalar.b2);
                Assert.Equal(exp[4], scalar.b3);
                Assert.Equal(exp[3], scalar.b4);
                Assert.Equal(exp[2], scalar.b5);
                Assert.Equal(exp[1], scalar.b6);
                Assert.Equal(exp[0], scalar.b7);
            }
        }

        public static IEnumerable<object[]> GetByteCases()
        {
            yield return new object[] { new byte[32], new uint[8], false };
            yield return new object[]
            {
                new byte[32]
                {
                    0x96,0xbb,0x33,0xd3, 0x85,0xa1,0xc4,0x9a, 0x1d,0x9d,0x3d,0x5c, 0x4a,0x47,0xe5,0xd4,
                    0x5b,0x23,0x71,0x47, 0x0b,0x6d,0x30,0xa8, 0xc6,0x95,0x1f,0xd6, 0xe8,0x66,0x18,0xd0
                },
                new uint[8]
                {
                    0x96bb33d3U, 0x85a1c49aU, 0x1d9d3d5cU, 0x4a47e5d4U,
                    0x5b237147U, 0x0b6d30a8U, 0xc6951fd6U, 0xe86618d0U
                },
                false
            };
            yield return new object[]
            {
                new byte[32]
                {
                    0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFE,
                    0xBA,0xAE,0xDC,0xE6, 0xAF,0x48,0xA0,0x3B, 0xBF,0xD2,0x5E,0x8C, 0xD0,0x36,0x41,0x41

                },
                new uint[8],
                true
            };
            yield return new object[]
            {
                new byte[32]
                {
                    0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFE,
                    0xBA,0xAE,0xDC,0xE6, 0xAF,0x48,0xA0,0x3B, 0xBF,0xD2,0x5E,0x8C, 0xD0,0x36,0x41,0x42

                },
                new uint[8] { 0, 0, 0, 0, 0, 0, 0, 1 },
                true
            };
            yield return new object[]
            {
                new byte[32]
                {
                    0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF,
                    0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF, 0xFF,0xFF,0xFF,0xFF

                },
                new uint[8]
                {
                    0, 0, 0, 1, ~N3, ~N2, ~N1, ~N0
                },
                true
            };
        }
        [Theory]
        [MemberData(nameof(GetByteCases))]
        public unsafe void Constructor_bytePointerTest(byte[] array, uint[] exp, bool expB)
        {
            fixed (byte* pt = &array[0])
            {
                Scalar8x32 scalar1 = new(array, out bool actual1);
                Scalar8x32 scalar2 = new(pt, out bool actual2);

                Assert.Equal(expB, actual1);
                Assert.Equal(exp[7], scalar1.b0);
                Assert.Equal(exp[6], scalar1.b1);
                Assert.Equal(exp[5], scalar1.b2);
                Assert.Equal(exp[4], scalar1.b3);
                Assert.Equal(exp[3], scalar1.b4);
                Assert.Equal(exp[2], scalar1.b5);
                Assert.Equal(exp[1], scalar1.b6);
                Assert.Equal(exp[0], scalar1.b7);

                Assert.Equal(expB, actual2);
                Assert.Equal(exp[7], scalar2.b0);
                Assert.Equal(exp[6], scalar2.b1);
                Assert.Equal(exp[5], scalar2.b2);
                Assert.Equal(exp[4], scalar2.b3);
                Assert.Equal(exp[3], scalar2.b4);
                Assert.Equal(exp[2], scalar2.b5);
                Assert.Equal(exp[1], scalar2.b6);
                Assert.Equal(exp[0], scalar2.b7);
            }
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, true)]
        [InlineData(1, 0, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 1, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 1, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 1, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 1, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 1, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 1, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 1, false)]
        public void IsZeroTest(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7, bool expected)
        {
            Scalar8x32 scalar = new(u0, u1, u2, u3, u4, u5, u6, u7);
            Assert.Equal(expected, scalar.IsZero);
        }
    }
}
