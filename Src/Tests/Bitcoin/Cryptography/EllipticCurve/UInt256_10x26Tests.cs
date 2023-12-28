// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Collections.Generic;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class UInt256_10x26Tests
    {
        private static UInt256_10x26 CreateRandom()
        {
            for (int i = 0; i < 3; i++)
            {
                byte[] ba = Helper.CreateRandomBytes(32);
                UInt256_10x26 result = new(ba, out bool isValid);
                if (isValid)
                {
                    return result;
                }
            }
            throw new Exception("Something is wrong.");
        }

        internal static void AssertEquality(in UInt256_10x26 actual, uint[] expected)
        {
            Assert.Equal(expected[0], actual.b0);
            Assert.Equal(expected[1], actual.b1);
            Assert.Equal(expected[2], actual.b2);
            Assert.Equal(expected[3], actual.b3);
            Assert.Equal(expected[4], actual.b4);
            Assert.Equal(expected[5], actual.b5);
            Assert.Equal(expected[6], actual.b6);
            Assert.Equal(expected[7], actual.b7);
            Assert.Equal(expected[8], actual.b8);
            Assert.Equal(expected[9], actual.b9);
        }

        internal static void AssertEquality(in UInt256_8x32 actual, uint[] expected)
        {
            Assert.Equal(expected[0], actual.b0);
            Assert.Equal(expected[1], actual.b1);
            Assert.Equal(expected[2], actual.b2);
            Assert.Equal(expected[3], actual.b3);
            Assert.Equal(expected[4], actual.b4);
            Assert.Equal(expected[5], actual.b5);
            Assert.Equal(expected[6], actual.b6);
            Assert.Equal(expected[7], actual.b7);
        }

        internal static void AssertEquality(in UInt256_10x26 expected, in UInt256_10x26 actual)
        {
            Assert.Equal(expected.b0, actual.b0);
            Assert.Equal(expected.b1, actual.b1);
            Assert.Equal(expected.b2, actual.b2);
            Assert.Equal(expected.b3, actual.b3);
            Assert.Equal(expected.b4, actual.b4);
            Assert.Equal(expected.b5, actual.b5);
            Assert.Equal(expected.b6, actual.b6);
            Assert.Equal(expected.b7, actual.b7);
            Assert.Equal(expected.b8, actual.b8);
            Assert.Equal(expected.b9, actual.b9);
        }



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
            yield return new object[] { new uint[8], new uint[10], 0 };
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
                1
            };
        }
        [Theory]
        [MemberData(nameof(GetCtor8Cases))]
        public void Constructor_From8UintsTest(uint[] arr, uint[] exp, int expMagnitude)
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
            Assert.Equal(expMagnitude, val1.magnitude);
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
        [InlineData(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, false)]
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


        public static IEnumerable<object[]> GetAddCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("620e158dd19a50a134f39b7a581b500de7ea1bde54ce47bb4a78027fd5418339"),
                Helper.HexToBytes("0889fce902fc06f9bc8d8e870b971354a4708c06c643e694c65a435997d1cbf3"),
                new uint[10]
                {
                    0x05134f2c, 0x0491765a, 0x02e5010c, 0x03946c48, 0x028c5aa7,
                    0x04ec98d8, 0x0412a015, 0x015e6bc5, 0x0276d496, 0x001aa604
                },
            };
        }
        [Theory]
        [MemberData(nameof(GetAddCases))]
        public void AddTest(byte[] ba1, byte[] ba2, uint[] expected)
        {
            UInt256_10x26 a = new(ba1, out bool b1);
            UInt256_10x26 b = new(ba2, out bool b2);
            Assert.True(b1);
            Assert.True(b2);

            UInt256_10x26 actual1 = a + b;
            UInt256_10x26 actual2 = b + a;
            UInt256_10x26 actual3 = a.Add(b);
            UInt256_10x26 actual4 = b.Add(a);

            AssertEquality(actual1, expected);
            AssertEquality(actual2, expected);
            AssertEquality(actual3, expected);
            AssertEquality(actual4, expected);
        }

        public static IEnumerable<object[]> GetAddUintCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("d5bd44891becfd769c518abe73a10abc42786612c00db7b8ab6603083357072e"),
                7,
                new uint[10]
                {
                    0x03570735, 0x0180c20c, 0x037b8ab6, 0x004b0036, 0x00427866,
                    0x00e842af, 0x0118abe7, 0x03f5da71, 0x00891bec, 0x00356f51
                },
            };
        }
        [Theory]
        [MemberData(nameof(GetAddUintCases))]
        public void Add_UintTest(byte[] ba1, uint u, uint[] expected)
        {
            UInt256_10x26 a = new(ba1, out bool success);
            Assert.True(success);

            UInt256_10x26 actual1 = a + u;
            UInt256_10x26 actual2 = a.Add(u);

            AssertEquality(actual1, expected);
            AssertEquality(actual2, expected);
#if DEBUG
            Assert.Equal(2, actual1.magnitude);
#endif
        }


        public static IEnumerable<object[]> GetNegateCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("8eda443fe7035fe6b7b6fa39ce984a8bda44797c9663d74f140ea4a44f117646"),
                1,
                new uint[10]
                {
                    0x0cee7a76, 0x0c56d5e9, 0x0e8b0ebc, 0x0e0da66d, 0x0c25bb83,
                    0x0c59ed5a, 0x0c905c60, 0x0e80651e, 0x0fc018f9, 0x00dc496b
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("3fb4d76b95f7aa05b863cd0f602082a20dd70f35cc73b8baa6d850eba9b2b5f3"),
                2,
                new uint[10]
                {
                    0x164d3327, 0x15ebc390, 0x1474558d, 0x1728ce2c, 0x15f228eb,
                    0x17f7df52, 0x15c32f04, 0x1557e919, 0x14946a03, 0x017012c5
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("aeda1f04a7690878a072c68d288ee1f80e6d7820e72e488e83ac1d7dd07af5af"),
                3,
                new uint[10]
                {
                    0x1f84ebc9, 0x1cf89e84, 0x1f7717be, 0x1f7c633f, 0x1ff19280,
                    0x1ddc477a, 0x1cd39726, 0x1fde1d77, 0x1cfb588f, 0x01d44971
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("7d16aae412e26a49326ea0934710f33709b04103cbbb68a725fc6dfd433fbef5"),
                4,
                new uint[10]
                {
                    0x24c01ae1, 0x24e47e26, 0x25758d97, 0x27f0d109, 0x24f64fb5,
                    0x263bc329, 0x2515f6c2, 0x2656db2d, 0x251bed14, 0x0260ba4c
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("b0aae887566e964b55e9a166b48f0c9bc3ed60f94e22ac0764f8d79e0044e3f5"),
                5,
                new uint[10]
                {
                    0x2fbaee3f, 0x2dca1574, 0x2d3f89a5, 0x2c1ac76a, 0x2c3c1294,
                    0x2edc3cce, 0x2d65e989, 0x2da6d29d, 0x2f78a986, 0x02d3d53a
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetNegateCases))]
        public void NegateTest(byte[] ba, int m, uint[] expected)
        {
            UInt256_10x26 a = new(ba, out bool success);
            Assert.True(success);

            UInt256_10x26 actual = a.Negate(m);

            AssertEquality(actual, expected);
        }


        public static IEnumerable<object[]> GetMultUintCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("9020615f3b25c39f62e4db9c8f06462033a8f87aba8a1d6ce978c4901ddb7ba3"),
                1,
                new uint[10]
                {
                    0x01db7ba3, 0x02312407, 0x01d6ce97, 0x01eaea28, 0x0033a8f8,
                    0x03c19188, 0x024db9c8, 0x030e7d8b, 0x015f3b25, 0x00240818
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("4a08ef2a9fd3f4933c932b076cb193a52fe121a30b2acc7c24b64043081c6aec"),
                2,
                new uint[10]
                {
                    0x0038d5d8, 0x03202184, 0x018f8496, 0x05185956, 0x025fc242,
                    0x0658c9d2, 0x026560ec, 0x07a499e4, 0x06553fa6, 0x00250476
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("01ae1168ab0ac63922e09d6e1a0ac1f46c99516c46d14f84671bfac29d180f9b"),
                3,
                new uint[10]
                {
                    0x03482ed1, 0x08fc11f5, 0x02e8d353, 0x051351cf, 0x0145cbf3,
                    0x07881177, 0x061d84a3, 0x094aada1, 0x043a011e, 0x0001428c
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("6852522efc533667c3db7b60db7ecb28016214560d1b311638bd281dbbdaa889"),
                4,
                new uint[10]
                {
                    0x0f6aa224, 0x0d281db8, 0x0c458e2c, 0x0560d1b0, 0x00058850,
                    0x0b7ecb28, 0x06ded834, 0x03667c3c, 0x08bbf14c, 0x00685250
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("d101bd8b5bc7abb12ab461315e8757563e3435443767fb6bd97f9abcb9de8c07"),
                5,
                new uint[10]
                {
                    0x0958bc23, 0x13816be6, 0x1291b3f3, 0x0554541b, 0x0b370509,
                    0x12292d29, 0x105e5f69, 0x0d69d752, 0x07b8cae3, 0x0105422b
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("7869f62c52526ac5ad2f7debe384d1d734aa51dbce57d2690552b2dc2ceef84b"),
                6,
                new uint[10]
                {
                    0x0599d1c2, 0x040c4a42, 0x06e761fe, 0x149b583a, 0x133bfde6,
                    0x05473abe, 0x11cf3874, 0x0a028838, 0x0d09edec, 0x00b49eee
                }
            };
            yield return new object[]
            {
                Helper.HexToBytes("7f87b4c7d9f68615976695db5a464c162ef8928964a6b1ad6b7c956693c869bd"),
                7,
                new uint[10]
                {
                    0x1a7ae42b, 0x1605737c, 0x15bbdf01, 0x0f070236, 0x0f48cbfe,
                    0x11fb0523, 0x10e18ff3, 0x0eaa5c8b, 0x0576f5ba, 0x00df2d7b
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetMultUintCases))]
        public void Multiply_ByUintTest(byte[] ba, uint b, uint[] expected)
        {
            UInt256_10x26 a = new(ba, out bool success);
            Assert.True(success);

            UInt256_10x26 actual1 = a * b;
            UInt256_10x26 actual2 = a.Multiply(b);

            AssertEquality(actual1, expected);
            AssertEquality(actual2, expected);
        }


        public static IEnumerable<object[]> GetMultCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("2b2b87691c5dce535fdf173d97c9db98d87f14249fd496f9d46d8e2c7ff98425"),
                Helper.HexToBytes("f6b9de150a988670307ec2aaf1582cc9de3b8c3b8d257a56958e94aecfa2d366"),
                new uint[10]
                {
                    0x023e7de5, 0x013b930c, 0x03348800, 0x0085a39e, 0x0303ce76,
                    0x006deeec, 0x03a51882, 0x0004838c, 0x01722db1, 0x002042b2
                },
            };
        }
        [Theory]
        [MemberData(nameof(GetMultCases))]
        public void MultiplyTest(byte[] ba1, byte[] ba2, uint[] expected)
        {
            UInt256_10x26 a = new(ba1, out bool b1);
            UInt256_10x26 b = new(ba2, out bool b2);
            Assert.True(b1);
            Assert.True(b2);

            UInt256_10x26 actual1 = a * b;
            UInt256_10x26 actual2 = b * a;
            UInt256_10x26 actual3 = a.Multiply(b);
            UInt256_10x26 actual4 = b.Multiply(a);

            AssertEquality(actual1, expected);
            AssertEquality(actual2, expected);
            AssertEquality(actual3, expected);
            AssertEquality(actual4, expected);
        }


        public static IEnumerable<object[]> GetSqrCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("5d2b70c932c610a6b3886f158527ef8e2efdaeeff09250a7d4e28e03776a6d2c"),
                1,
                new uint[10]
                {
                    0x02ba7518, 0x000af822, 0x02bce262, 0x03c32509, 0x0027870d,
                    0x01cc75b2, 0x00de3e97, 0x03bea68b, 0x0199d4da, 0x00266553
                },
            };
            yield return new object[]
            {
                Helper.HexToBytes("1b42129dd69d568f2ba70bb42a2a96f1ddf7ab055c142e2a6585756faa81cfd7"),
                3,
                new uint[10]
                {
                    0x02504926, 0x02482355, 0x031c2326, 0x037dfb03, 0x00a40ec5,
                    0x014398e3, 0x0251306b, 0x002814a9, 0x0196f967, 0x00100537
                },
            };
            yield return new object[]
            {
                Helper.HexToBytes("d733a70c1fc8207738f1eadee7d65c97e4c43da70e9076e43204bae92d6e9133"),
                23,
                new uint[10]
                {
                    0x039b1d39, 0x02e51f39, 0x014d297e, 0x03518ba6, 0x01f3387c,
                    0x02a5d197, 0x031041d3, 0x01916943, 0x007b5180, 0x0016ad4a
                },
            };
            yield return new object[]
            {
                Helper.HexToBytes("60b7456356c2704f32d4c4138806a724e57c5f8c012e895be839dd64bb775bac"),
                44,
                new uint[10]
                {
                    0x01feca07, 0x0054a0a8, 0x0023a107, 0x00dfae24, 0x026b4659,
                    0x02b8b29c, 0x001c6363, 0x03556000, 0x032e922b, 0x000c2a59
                },
            };
            yield return new object[]
            {
                Helper.HexToBytes("7918614af7215b2a95975ae4fdb4e2607b26490642cad4f476dd57df3fcbe225"),
                88,
                new uint[10]
                {
                    0x00745d45, 0x019d48eb, 0x00984c62, 0x00c5afd6, 0x0126b7bc,
                    0x01666c77, 0x005ebd08, 0x00cfb619, 0x00e9e7b4, 0x002066fe
                },
            };
        }
        [Theory]
        [MemberData(nameof(GetSqrCases))]
        public void SqrTest(byte[] ba, int times, uint[] expected)
        {
            UInt256_10x26 a = new(ba, out bool isValid);
            Assert.True(isValid);

            UInt256_10x26 actual = a.Sqr(times);

            AssertEquality(actual, expected);
        }

        [Fact]
        public void SqrMultTest()
        {
            UInt256_10x26 a = CreateRandom();

            UInt256_10x26 a_mult2 = a * a;
            UInt256_10x26 a_sqr1 = a.Sqr(1);
            AssertEquality(a_mult2, a_sqr1);

            UInt256_10x26 a_mult4 = a * a * a * a;
            UInt256_10x26 a_sqr2 = a.Sqr(2);
            AssertEquality(a_mult4, a_sqr2);

            UInt256_10x26 a_mult8 = a * a * a * a * a * a * a * a;
            UInt256_10x26 a_sqr3 = a.Sqr(3);
            AssertEquality(a_mult8, a_sqr3);
        }


        public static IEnumerable<object[]> GetSqrtCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("818c8c5d848e9c7bcdc006a0b7d178f94560f76283091a9f30bc764e132e38b0"),
                true,
                new uint[10]
                {
                    0x02434f76, 0x001768c0, 0x00722ce0, 0x0173eafa, 0x03249d46,
                    0x0254c715, 0x02257743, 0x037032f3, 0x028ee356, 0x000f2949
                },
            };
            yield return new object[]
            {
                Helper.HexToBytes("ebdc15778f467efc0b5180b9b582145cd82c73020f7064c4c94c45ef0de954d5"),
                true,
                new uint[10]
                {
                    0x035137cf, 0x015245f3, 0x02cf41e1, 0x01932c2b, 0x00f9ddfb,
                    0x03cf60bd, 0x00106e32, 0x00063338, 0x039b5f7f, 0x00271d67
                },
            };
            yield return new object[]
            {
                Helper.HexToBytes("1482c249ef89dda0b0a7995da642e393632be835b3f59fccd95923e4e4a2cd61"),
                false,
                new uint[10]
                {
                    0x032d94f2, 0x03473e11, 0x0282459f, 0x02d24c08, 0x0098d558,
                    0x02316b64, 0x030b5be0, 0x0096ea81, 0x01add21c, 0x003e5f94
                },
            };
            yield return new object[]
            {
                Helper.HexToBytes("da38612f757deb3c11b6ab0558927b5810577dc883d9a75c86ad82b2b684ca91"),
                false,
                new uint[10]
                {
                    0x015b96fe, 0x01192d8e, 0x00ba08e9, 0x01cc7890, 0x022232cc,
                    0x03e803cd, 0x0019bf01, 0x01fb7d95, 0x022521bf, 0x003c50e4
                },
            };
        }
        [Theory]
        [MemberData(nameof(GetSqrtCases))]
        public void SqrtTest(byte[] ba, bool expSuccess, uint[] expected)
        {
            UInt256_10x26 a = new(ba, out bool isValid);
            Assert.True(isValid);

            bool success = a.Sqrt(out UInt256_10x26 actual);
            Assert.Equal(expSuccess, success);
            if (expSuccess)
            {
                AssertEquality(actual, expected);
            }
        }


        public static IEnumerable<object[]> GetToUInt256_8x32Cases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("d18aef65ef13976dcb566544d95b46fccf62cdd6b7b9c2ff91802332119c14cf"),
                new uint[8]
                {
                    0x119c14cf, 0x91802332, 0xb7b9c2ff, 0xcf62cdd6, 0xd95b46fc, 0xcb566544, 0xef13976d, 0xd18aef65,
                },
            };
        }
        [Theory]
        [MemberData(nameof(GetToUInt256_8x32Cases))]
        public void ToUInt256_8x32Test(byte[] ba, uint[] expected)
        {
            UInt256_10x26 a = new(ba, out bool isValid);
            Assert.True(isValid);

            UInt256_8x32 actual = a.ToUInt256_8x32();
            AssertEquality(actual, expected);
        }



        #region https://github.com/bitcoin-core/secp256k1/blob/efe85c70a2e357e3605a8901a9662295bae1001f/src/tests.c#L2948-L3353

        private const int COUNT = 64;

        /// <summary>
        /// secp256k1_memcmp_var
        /// </summary>
        private static int Libsecp256k1_CmpVar(ReadOnlySpan<ushort> p1, ReadOnlySpan<ushort> p2, int n)
        {
            for (int i = 0; i < n; i++)
            {
                int diff = p1[i] - p2[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return 0;
        }

        /// <summary>
        /// secp256k1_memcmp_var
        /// </summary>
        private static uint Libsecp256k1_CmpVar(in UInt256_8x32 a, in UInt256_8x32 b)
        {
            ReadOnlySpan<uint> p1 = new uint[8] { a.b0, a.b1, a.b2, a.b3, a.b4, a.b5, a.b6, a.b7 };
            ReadOnlySpan<uint> p2 = new uint[8] { b.b0, b.b1, b.b2, b.b3, b.b4, b.b5, b.b6, b.b7 };
            for (int i = 0; i < p1.Length; i++)
            {
                uint diff = p1[i] - p2[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return 0;
        }

        /// <summary>
        /// random_fe_test
        /// </summary>
        private static UInt256_10x26 RandomFETest(TestRNG rng)
        {
            byte[] bin = new byte[32];
            do
            {
                rng.Rand256Test(bin);
                UInt256_10x26 x = new(bin, out bool isValid);
                if (isValid)
                {
                    return x;
                }
            } while (true);
        }

        /// <summary>
        /// random_fe
        /// <para/> https://github.com/bitcoin-core/secp256k1/blob/efe85c70a2e357e3605a8901a9662295bae1001f/src/testutil.h
        /// </summary>
        private static UInt256_10x26 RandomFE(TestRNG rng)
        {
            byte[] bin = new byte[32];
            do
            {
                rng.Rand256(bin);
                UInt256_10x26 result = new(bin, out bool isValid);
                if (isValid)
                {
                    return result;
                }
            } while (true);
        }

        /// <summary>
        /// random_field_element_magnitude
        /// </summary>
        private static void RandomFEMagnitude(ref UInt256_10x26 fe, int m, TestRNG rng)
        {
            int n = (int)rng.RandInt((uint)(m + 1));
            fe = fe.Normalize();
            if (n == 0)
            {
                return;
            }
            UInt256_10x26 zero = UInt256_10x26.Zero.Negate(0);
            zero = zero.Multiply((uint)(n - 1));
            fe = fe.Add(zero);
# if  DEBUG
            Assert.True(fe.magnitude == n);
#endif
        }

        /// <summary>
        /// random_fe_magnitude
        /// </summary>
        private static void RandomFEMagnitude(ref UInt256_10x26 fe, TestRNG rng)
        {
            RandomFEMagnitude(ref fe, 8, rng);
        }

        /// <summary>
        /// random_fe_non_zero
        /// </summary>
        private static UInt256_10x26 RandomFENonZero(TestRNG rng)
        {
            UInt256_10x26 result;
            do
            {
                result = RandomFE(rng);
            } while (result.IsZero);
            return result;
        }

        /// <summary>
        /// random_fe_non_square
        /// </summary>
        private static UInt256_10x26 RandomFENonSquare(TestRNG rng)
        {
            UInt256_10x26 ns = RandomFENonZero(rng);
            if (ns.Sqrt(out _))
            {
                ns = ns.Negate(1);
            }
            return ns;
        }

        /// <summary>
        /// check_fe_equal
        /// </summary>
        private static bool CheckFEEqual(in UInt256_10x26 a, in UInt256_10x26 b)
        {
            UInt256_10x26 an = a.NormalizeWeak();
            return an.Equals(b);
        }


        [Fact]
        public void Libsecp256k1_FieldConvertTest()
        {
            // run_field_convert
            ReadOnlySpan<byte> b32 = new byte[32]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29,
                0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40
            };
            UInt256_8x32 fes = new(0x37383940U, 0x33343536U, 0x26272829U, 0x22232425U, 0x15161718U, 0x11121314U, 0x04050607U, 0x00010203U);
            UInt256_10x26 fe = new(0x37383940U, 0x33343536U, 0x26272829U, 0x22232425U, 0x15161718U, 0x11121314U, 0x04050607U, 0x00010203U);

            // Check conversions to fe
            UInt256_10x26 fe2 = new(b32, out bool isValid);
            Assert.True(isValid);
            Assert.True(fe.Equals(fe2));
            fe2 = fes.ToUInt256_10x26();
            Assert.True(fe.Equals(fe2));
            // Check conversion from fe
            byte[] b322 = fe.ToByteArray();
            Assert.Equal(b32, b322);
            UInt256_8x32 fes2 = fe.ToUInt256_8x32();
            Assert.True(fes.Equals(fes2));
        }

        [Fact]
        public void Libsecp256k1_FieldBe32OverflowTest()
        {
            // run_field_be32_overflow
            {
                ReadOnlySpan<byte> zero_overflow = new byte[32]
                {
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFE, 0xFF, 0xFF, 0xFC, 0x2F,
                };
                ReadOnlySpan<byte> zero = new byte[32];
                UInt256_10x26 fe = new(zero_overflow, out bool isValid);
                Assert.False(isValid);
                fe = new(zero_overflow);
                Assert.True(fe.IsZeroNormalized());
                fe = fe.Normalize();
                Assert.True(fe.IsZero);
                byte[] actual = fe.ToByteArray();
                Assert.Equal(zero, actual);
            }

            {
                ReadOnlySpan<byte> one_overflow = new byte[32]
                {
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFE, 0xFF, 0xFF, 0xFC, 0x30,
                };
                ReadOnlySpan<byte> one = new byte[32]
                {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
                };

                UInt256_10x26 fe = new(one_overflow, out bool isValid);
                Assert.False(isValid);
                fe = new(one_overflow);
                fe = fe.Normalize();
                Assert.True(fe.CompareToVar(UInt256_10x26.One) == 0);
                byte[] actual = fe.ToByteArray();
                Assert.Equal(one, actual);
            }

            {
                ReadOnlySpan<byte> ff_overflow = new byte[32]
                {
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                };
                ReadOnlySpan<byte> ff = new byte[32]
                {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x03, 0xD0,
                };

                UInt256_10x26 fe_ff = new(0x000003d0, 0x01, 0, 0, 0, 0, 0, 0);
                UInt256_10x26 fe = new(ff_overflow, out bool isValid);
                Assert.False(isValid);
                fe = new(ff_overflow);
                fe = fe.Normalize();
                Assert.True(fe.CompareToVar(fe_ff) == 0);
                byte[] actual = fe.ToByteArray();
                Assert.Equal(ff, actual);
            }
        }

        /// <summary>
        /// Returns true if two field elements have the same representation.
        /// <para/>fe_identical
        /// </summary>
        private static bool FEIdentical(in UInt256_10x26 a, in UInt256_10x26 b)
        {
            return a.b0 - b.b0 == 0 && a.b1 - b.b1 == 0 && a.b2 - b.b2 == 0 && a.b3 - b.b3 == 0 && a.b4 - b.b4 == 0 &&
                   a.b5 - b.b5 == 0 && a.b6 - b.b6 == 0 && a.b7 - b.b7 == 0 && a.b8 - b.b8 == 0 && a.b9 - b.b9 == 0;
        }


        [Fact]
        public void Libsecp256k1_FieldHalfTest()
        {
            // run_field_half

            // Check magnitude 0 input
            UInt256_10x26 t = UInt256_10x26.GetBounds(0);

            t = t.Half();
# if  DEBUG
            Assert.True(t.magnitude == 1);
            Assert.False(t.isNormalized);
#endif
            Assert.True(t.IsZeroNormalized());

            // Check non-zero magnitudes in the supported range
            for (uint m = 1; m < 32; m++)
            {
                // Check max-value input
                t = UInt256_10x26.GetBounds(m);

                UInt256_10x26 u = t.Half();
#if DEBUG
                Assert.True(u.magnitude == (m >> 1) + 1);
                Assert.False(u.isNormalized);
#endif
                u = u.NormalizeWeak();
                u = u.Add(u);
                Assert.True(CheckFEEqual(t, u));

                // Check worst-case input: ensure the LSB is 1 so that P will be added,
                // which will also cause all carries to be 1, since all limbs that can
                // generate a carry are initially even and all limbs of P are odd in
                // every existing field implementation.
                t = UInt256_10x26.GetBounds(m);
                Assert.True(t.b0 > 0);
                Assert.True((t.b0 & 1) == 0);
                // --t.n[0]; our structs are immutable!
                t = new(t.b0 - 1, t.b1, t.b2, t.b3, t.b4, t.b5, t.b6, t.b7, t.b8, t.b9
#if DEBUG
                    , t.magnitude, t.isNormalized
#endif
                    );

                u = t.Half();
# if DEBUG
                Assert.True(u.magnitude == (m >> 1) + 1);
                Assert.False(u.isNormalized);
#endif
                u = u.NormalizeWeak();
                u = u.Add(u);
                Assert.True(CheckFEEqual(t, u));
            }
        }


        [Fact]
        public void Libsecp256k1_FieldMiscTest()
        {
            // run_field_misc

            TestRNG rng = new();
            rng.Init(null);

            UInt256_10x26 fe5 = new(5, 0, 0, 0, 0, 0, 0, 0);
            for (int i = 0; i < 1000 * COUNT; i++)
            {
                UInt256_10x26 x = (i & 1) != 0 ? RandomFE(rng) : RandomFETest(rng);
                UInt256_10x26 y = RandomFENonZero(rng);
                uint v = (uint)rng.RandBits(15);
                // Test that fe_add_int is equivalent to fe_set_int + fe_add.
                UInt256_10x26 q = new(v); // q = v
                UInt256_10x26 z = x; // z = x
                z = z.Add(q); // z = x+v
                q = x; // q = x
                q = q.Add(v); // q = x+v
                Assert.True(CheckFEEqual(q, z));
                // Test the fe equality and comparison operations.
                Assert.True(x.CompareToVar(x) == 0);
                Assert.True(x.Equals(x));
                z = x;
                z = z.Add(y);
                // Test fe conditional move; z is not normalized here.
                q = x;
                x = UInt256_10x26.CMov(x, z, 0);
#if DEBUG
                Assert.False(x.isNormalized);
                Assert.True((x.magnitude == q.magnitude) || (x.magnitude == z.magnitude));
                Assert.True((x.magnitude >= q.magnitude) && (x.magnitude >= z.magnitude));
#endif
                x = q;
                x = UInt256_10x26.CMov(x, x, 1);
                Assert.False(FEIdentical(x, z));
                Assert.True(FEIdentical(x, q));
                q = UInt256_10x26.CMov(q, z, 1);
# if DEBUG
                Assert.False(q.isNormalized);
                Assert.True((q.magnitude == x.magnitude) || (q.magnitude == z.magnitude));
                Assert.True((q.magnitude >= x.magnitude) && (q.magnitude >= z.magnitude));
#endif
                Assert.True(FEIdentical(q, z));
                q = z;
                x = x.NormalizeVar();
                z = z.NormalizeVar();
                Assert.False(x.Equals(z));
                q = q.NormalizeVar();
                q = UInt256_10x26.CMov(q, z, (uint)(i & 1));
# if DEBUG
                Assert.True(q.isNormalized && q.magnitude == 1);
#endif
                for (int j = 0; j < 6; j++)
                {
                    z = z.Negate(j + 1);
                    q = q.NormalizeVar();
                    q = UInt256_10x26.CMov(q, z, (uint)(j & 1));
# if DEBUG
                    Assert.True(!q.isNormalized && q.magnitude == z.magnitude);
#endif
                }
                z = z.NormalizeVar();
                // Test storage conversion and conditional moves.
                UInt256_8x32 xs = x.ToUInt256_8x32();
                UInt256_8x32 ys = y.ToUInt256_8x32();
                UInt256_8x32 zs = z.ToUInt256_8x32();
                zs = UInt256_8x32.CMov(zs, xs, 0);
                zs = UInt256_8x32.CMov(zs, zs, 1);
                Assert.True(Libsecp256k1_CmpVar(xs, zs) != 0);
                ys = UInt256_8x32.CMov(ys, xs, 1);
                Assert.True(Libsecp256k1_CmpVar(xs, ys) == 0);
                x = xs.ToUInt256_10x26();
                y = ys.ToUInt256_10x26();
                z = zs.ToUInt256_10x26();
                // Test that mul_int, mul, and add agree.
                y = y.Add(x);
                y = y.Add(x);
                z = x;
                z = z.Multiply(3);
                Assert.True(CheckFEEqual(y, z));
                y = y.Add(x);
                z = z.Add(x);
                Assert.True(CheckFEEqual(z, y));
                z = x;
                z = z.Multiply(5);
                q = x.Multiply(fe5);
                Assert.True(CheckFEEqual(z, q));
                x = x.Negate(1);
                z = z.Add(x);
                q = q.Add(x);
                Assert.True(CheckFEEqual(y, z));
                Assert.True(CheckFEEqual(q, y));
                // Check secp256k1_fe_half.
                z = x;
                z = z.Half();
                z = z.Add(z);
                Assert.True(CheckFEEqual(x, z));
                z = z.Add(z);
                z = z.Half();
                Assert.True(CheckFEEqual(x, z));
            }
        }


        /// <summary>
        /// test_fe_mul
        /// </summary>
        private static void TestFEMul(in UInt256_10x26 a, in UInt256_10x26 b, bool use_sqr)
        {
            // Variables in LE 16x uint16_t format.
            ushort[] a16 = new ushort[16];
            ushort[] b16 = new ushort[16];
            ushort[] c16 = new ushort[16];
            // Field modulus in LE 16x uint16_t format
            ReadOnlySpan<ushort> m16 = new ushort[16]
            {
                0xfc2f, 0xffff, 0xfffe, 0xffff, 0xffff, 0xffff, 0xffff, 0xffff,
                0xffff, 0xffff, 0xffff, 0xffff, 0xffff, 0xffff, 0xffff, 0xffff,
            };

            // Compute C = A * B in fe format.
            UInt256_10x26 c = use_sqr ? a.Sqr() : a.Multiply(b);

            // Convert A, B, C into LE 16x uint16_t format.
            UInt256_10x26 an = a;
            UInt256_10x26 bn = b;
            c = c.NormalizeVar();
            an = an.NormalizeVar();
            bn = bn.NormalizeVar();
            // Variables in BE 32-byte format.
            byte[] a32 = an.ToByteArray();
            byte[] b32 = bn.ToByteArray();
            byte[] c32 = c.ToByteArray();
            for (int i = 0; i < 16; ++i)
            {
                a16[i] = (ushort)(a32[31 - 2 * i] + (a32[30 - 2 * i] << 8));
                b16[i] = (ushort)(b32[31 - 2 * i] + (b32[30 - 2 * i] << 8));
                c16[i] = (ushort)(c32[31 - 2 * i] + (c32[30 - 2 * i] << 8));
            }
            // Compute T = A * B in LE 16x uint16_t format.
            Span<ushort> t16 = ModInv32Tests.MulMod256(a16, b16, m16);
            // Compare
            // 16 items length which is 32 byte
            Assert.True(Libsecp256k1_CmpVar(t16, c16, 16) == 0);
        }


        [Fact]
        public void Libsecp256k1_FEMulTest()
        {
            // run_fe_mul

            TestRNG rng = new();
            rng.Init(null);

            for (int i = 0; i < 100 * COUNT; ++i)
            {
                UInt256_10x26 a = RandomFE(rng);
                RandomFEMagnitude(ref a, rng);
                UInt256_10x26 b = RandomFE(rng);
                RandomFEMagnitude(ref b, rng);
                UInt256_10x26 c = RandomFETest(rng);
                RandomFEMagnitude(ref c, rng);
                UInt256_10x26 d = RandomFETest(rng);
                RandomFEMagnitude(ref d, rng);
                TestFEMul(a, a, true);
                TestFEMul(c, c, true);
                TestFEMul(a, b, false);
                TestFEMul(a, c, false);
                TestFEMul(c, b, false);
                TestFEMul(c, d, false);
            }
        }


        [Fact]
        public void Libsecp256k1_SqrTest()
        {
            // run_sqr

            UInt256_10x26 x = new(1);
            x = x.Negate(1);

            for (int i = 1; i <= 512; ++i)
            {
                x = x.Multiply(2);
                x = x.Normalize();
                UInt256_10x26 s = x.Sqr();
            }
        }


        /// <summary>
        /// test_sqrt
        /// </summary>
        private static void TestSqrt(in UInt256_10x26 a, in UInt256_10x26? k)
        {
            bool v = a.Sqrt(out UInt256_10x26 r1);
            Assert.True((v == false) == (k == null));

            if (k != null)
            {
                // Check that the returned root is +/- the given known answer
                UInt256_10x26 r2 = r1.Negate(1);
                r1 = r1.Add(k.Value); r2 = r2.Add(k.Value);
                r1 = r1.Normalize(); r2 = r2.Normalize();
                Assert.True(r1.IsZero || r2.IsZero);
            }
        }

        [Fact]
        public void Libsecp256k1_SqrtTest()
        {
            // run_sqrt

            TestRNG rng = new();
            rng.Init(null);

            // Check sqrt(0) is 0
            UInt256_10x26 x = new(0);
            UInt256_10x26 s = x.Sqr();
            TestSqrt(s, x);

            // Check sqrt of small squares (and their negatives)
            for (uint i = 1; i <= 100; i++)
            {
                x = new(i);
                s = x.Sqr();
                TestSqrt(s, x);
                UInt256_10x26 t = s.Negate(1);
                TestSqrt(t, null);
            }

            // Consistency checks for large random values
            for (int i = 0; i < 10; i++)
            {
                int j;
                UInt256_10x26 ns = RandomFENonSquare(rng);
                for (j = 0; j < COUNT; j++)
                {
                    x = RandomFE(rng);
                    s = x.Sqr();
                    Assert.True(s.IsSquareVar());
                    TestSqrt(s, x);
                    UInt256_10x26 t = s.Negate(1);
                    Assert.False(t.IsSquareVar());
                    TestSqrt(t, null);
                    t = s.Multiply(ns);
                    TestSqrt(t, null);
                }
            }
        }

        #endregion // libsecp256k1 tests
    }
}
