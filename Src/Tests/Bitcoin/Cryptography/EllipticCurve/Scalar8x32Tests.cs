// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Collections.Generic;

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


        public static IEnumerable<object[]> GetMultCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("4166cab9fb3b70e2a5608df3f41224cbf79a5389705b0b3a0fe14d4d4ae62019"),
                Helper.HexToBytes("a7b54ffabf58f1afb0b37b86521c2079d24bc4262a28b9e55a90aa19ee2298bf"),
                new uint[8]
                {
                    0x7c3a39d9, 0xa2bcc5b0, 0x96e1dc44, 0x78d2caef, 0x3cb5a1ef, 0x3507462b, 0xf24f2995, 0x9aeb40ee
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("e774610894f0910499c1ad17523e18750c9f06fba8aed18c99e55a6c42d5eb21"),
                Helper.HexToBytes("0ca42316dcbb4475797c89672af0808bd13f82f1112a6f38a3d46eae9524c2c4"),
                new uint[8]
                {
                    0x679ce50a, 0x1caa21b4, 0x2f3f3d6d, 0xd88dd68f, 0x1d364db6, 0x20cdd6a2, 0x7c86946a, 0x304e130c
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetMultCases))]
        public void MultiplyTest(byte[] arr1, byte[] arr2, uint[] expected)
        {
            Scalar8x32 a = new(arr1, out bool overflow);
            Assert.False(overflow);
            Scalar8x32 b = new(arr2, out overflow);
            Assert.False(overflow);

            Scalar8x32 actual = a.Multiply(b);
            Assert.Equal(expected[0], actual.b0);
            Assert.Equal(expected[1], actual.b1);
            Assert.Equal(expected[2], actual.b2);
            Assert.Equal(expected[3], actual.b3);
            Assert.Equal(expected[4], actual.b4);
            Assert.Equal(expected[5], actual.b5);
            Assert.Equal(expected[6], actual.b6);
            Assert.Equal(expected[7], actual.b7);
        }


        public static IEnumerable<object[]> GetMulShiftVarCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("70c26dee31df21a9862a7eb4691c82bbcd6fa25aca54a019e0630bf05f345670"),
                Helper.HexToBytes("248010c56bda6de7c632372c51db7c3f1b8fb265a5f6a37770d3891e0ae9df21"),
                384,
                new uint[8]
                {
                    0x24e77e63, 0x09ab6674, 0x921b45a4, 0x1013c00f, 0x00000000, 0x00000000, 0x00000000, 0x00000000
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("8e70dc336b8916b33850187997eca5592883a22f6e531fbb9505661681c4f5fb"),
                Helper.HexToBytes("c797d53f87435d2222a067442ea895534940a7ec6f75a6de1c1fcd896ac1cb60"),
                257,
                new uint[8]
                {
                    0x1640f495, 0x8b4f558d, 0xdf179684, 0xdafddd13, 0x6573c474, 0x7c43641b, 0x8d95fb8e, 0x37871b32
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("4f226a073400f62f18ebfa57b3622fe746032922998376fbbbe82af86b24bc34"),
                Helper.HexToBytes("96d9e6142ebd47d62e0e616b9c75b02164ef1aced8714fc07308efbca7cc587e"),
                272,
                new uint[8]
                {
                    0x36ca316f, 0x8fe847ce, 0x3be6a72e, 0x029610b2, 0xc438e6cb, 0x180386da, 0x856b40f9, 0x00002ea1
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetMulShiftVarCases))]
        public void MulShiftVarTest(byte[] arr1, byte[] arr2, int shift, uint[] expected)
        {
            Scalar8x32 a = new(arr1, out bool overflow);
            Assert.False(overflow);
            Scalar8x32 b = new(arr2, out overflow);
            Assert.False(overflow);

            Scalar8x32 actual = Scalar8x32.MulShiftVar(a, b, shift);
            Assert.Equal(expected[0], actual.b0);
            Assert.Equal(expected[1], actual.b1);
            Assert.Equal(expected[2], actual.b2);
            Assert.Equal(expected[3], actual.b3);
            Assert.Equal(expected[4], actual.b4);
            Assert.Equal(expected[5], actual.b5);
            Assert.Equal(expected[6], actual.b6);
            Assert.Equal(expected[7], actual.b7);
        }


        public static IEnumerable<object[]> EqualCases()
        {
            Random rng = new();
            Span<byte> buffer = new byte[64];
            rng.NextBytes(buffer);
            byte[] ba1 = buffer.Slice(0, 32).ToArray();
            byte[] ba2 = buffer.Slice(32, 32).ToArray();

            yield return new object[] { new Scalar8x32(0, 0, 0, 0, 0, 0, 0, 0), new Scalar8x32(0, 0, 0, 0, 0, 0, 0, 0), true };
            yield return new object[] { new Scalar8x32(0, 0, 0, 0, 0, 0, 0, 0), new Scalar8x32(0, 0, 0, 0, 0, 0, 0, 1), false };
            yield return new object[] { new Scalar8x32(ba1, out _), new Scalar8x32(ba2, out _), false };
        }
        [Theory]
        [MemberData(nameof(EqualCases))]
        public void EqualityTest(in Scalar8x32 first, in Scalar8x32 second, bool expected)
        {
            Assert.Equal(expected, first == second);
            Assert.Equal(expected, second == first);
            Assert.Equal(!expected, first != second);
            Assert.Equal(!expected, second != first);
            Assert.Equal(expected, first.Equals(second));
            Assert.Equal(expected, second.Equals(first));
            Assert.Equal(expected, first.Equals((object)second));
            Assert.Equal(expected, second.Equals((object)first));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            int h1 = new Scalar8x32(1).GetHashCode();
            int h2 = new Scalar8x32(2).GetHashCode();
            Assert.NotEqual(h1, h2);
        }



        #region https://github.com/bitcoin-core/secp256k1/blob/77af1da9f631fa622fb5b5895fd27be431432368/src/tests.c#L2126-L2946

        private const int Count = 64;

        // random_scalar_order_test
        private static Scalar8x32 CreateRandom(Rand rng)
        {
            byte[] b32 = new byte[32];
            do
            {
                rng.secp256k1_testrand256_test(b32);
                Scalar8x32 result = new(b32, out bool overflow);
                if (!overflow && !result.IsZero)
                {
                    return result;
                }
            } while (true);
        }

        // scalar_test(void)
        private static unsafe void ScalarTest(Rand rng)
        {
            // Set 's' to a random scalar, with value 'snum'.
            Scalar8x32 s = CreateRandom(rng);

            // Set 's1' to a random scalar, with value 's1num'.
            Scalar8x32 s1 = CreateRandom(rng);

            // Set 's2' to a random scalar, with value 'snum2', and byte array representation 'c'.
            Scalar8x32 s2 = CreateRandom(rng);
            byte[] c = s2.ToByteArray();

            uint* pt = stackalloc uint[8] { s.b0, s.b1, s.b2, s.b3, s.b4, s.b5, s.b6, s.b7 };

            {
                // Test that fetching groups of 4 bits from a scalar
                // and recursing n(i)=16*n(i-1)+p(i) reconstructs it.
                Scalar8x32 n = new(0);
                for (int i = 0; i < 256; i += 4)
                {
                    Scalar8x32 t = new(Scalar8x32.GetBits(pt, 256 - 4 - i, 4));
                    for (int j = 0; j < 4; j++)
                    {
                        n = n.Add(n, out _);
                    }
                    n = n.Add(t, out _);
                }
                Assert.Equal(n, s);
            }

            {
                // Test that fetching groups of randomly-sized bits from a scalar
                // and recursing n(i)=b*n(i-1)+p(i) reconstructs it
                Scalar8x32 n = new(0);
                int i = 0;
                while (i < 256)
                {
                    int now = (int)(rng.secp256k1_testrand_int(15) + 1);
                    if (now + i > 256)
                    {
                        now = 256 - i;
                    }
                    Scalar8x32 t = new(Scalar8x32.GetBitsVar(pt, 256 - now - i, now));
                    for (int j = 0; j < now; j++)
                    {
                        n = n.Add(n, out _);
                    }
                    n = n.Add(t, out _);
                    i += now;
                }
                Assert.Equal(n, s);
            }

            {
                // Test commutativity of add
                Scalar8x32 r1 = s1.Add(s2, out _);
                Scalar8x32 r2 = s2.Add(s1, out _);
                Assert.Equal(r1, r2);
            }

            {
                // Test add_bit
                uint bit = (uint)rng.secp256k1_testrand_bits(8);
                Scalar8x32 b = new(1);
                Assert.True(b.IsOne);
                for (int i = 0; i < bit; i++)
                {
                    b = b.Add(b, out _);
                }
                Scalar8x32 r1 = s1;
                Scalar8x32 r2 = s1;
                r1 = r1.Add(b, out bool overflow);
                if (!overflow)
                {
                    // No overflow happened
                    r2 = r2.CAddBit(bit, 1);
                    Assert.Equal(r1, r2);

                    // cadd is a noop when flag is zero
                    r2 = r2.CAddBit(bit, 0);
                    Assert.Equal(r1, r2);
                }
            }

            {
                // Test commutativity of mul
                Scalar8x32 r1 = s1.Multiply(s2);
                Scalar8x32 r2 = s2.Multiply(s1);
                Assert.Equal(r1, r2);
            }

            {
                // Test associativity of add
                // s1 + s2 + s == s2 + s + s1
                Scalar8x32 r1 = s1.Add(s2, out _);
                r1 = r1.Add(s, out _);
                Scalar8x32 r2 = s2.Add(s, out _);
                r2 = s1.Add(r2, out _);
                Assert.Equal(r1, r2);
            }

            {
                // Test associativity of mul
                // s1 * s2 * s == s2 * s * s1
                Scalar8x32 r1 = s1.Multiply(s2);
                r1 = r1.Multiply(s);
                Scalar8x32 r2 = s2.Multiply(s);
                r2 = s1.Multiply(r2);
                Assert.Equal(r1, r2);
            }

            {
                // Test distributitivity of mul over add
                // (s1 + s2) * s == (s1 * s) + (s2 * s)
                Scalar8x32 r1 = s1.Add(s2, out _);
                r1 = r1.Multiply(s);
                Scalar8x32 r2 = s1.Multiply(s);
                Scalar8x32 t = s2.Multiply(s);
                r2 = r2.Add(t, out _);
                Assert.Equal(r1, r2);
            }

            {
                // Test multiplicative identity
                Scalar8x32 r1 = s1.Multiply(Scalar8x32.One);
                Assert.Equal(r1, s1);
            }

            {
                // Test additive identity
                Scalar8x32 r1 = s1.Add(Scalar8x32.Zero, out _);
                Assert.Equal(r1, s1);
            }

            {
                // Test zero product property
                Scalar8x32 r1 = s1.Multiply(Scalar8x32.Zero);
                Assert.Equal(r1, Scalar8x32.Zero);
            }

            {
                // Test halving
                Scalar8x32 r = s.Add(s, out _);
                r = r.Half();
                Assert.Equal(r, s);
            }
        }

        [Fact]
        public void Libsecp256k1Tests() // run_scalar_tests
        {
            Rand rng = new();
            rng.Init(null);

            for (int i = 0; i < 128 * Count; i++)
            {
                ScalarTest(rng);
            }
        }

        #endregion
    }
}
