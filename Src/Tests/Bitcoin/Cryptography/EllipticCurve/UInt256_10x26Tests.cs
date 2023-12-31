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
        internal static UInt256_10x26 RandomFETest(TestRNG rng)
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
        internal static void RandomFEMagnitude(ref UInt256_10x26 fe, int m, TestRNG rng)
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
        internal static void RandomFEMagnitude(ref UInt256_10x26 fe, TestRNG rng)
        {
            RandomFEMagnitude(ref fe, 8, rng);
        }

        /// <summary>
        /// random_fe_non_zero
        /// </summary>
        internal static UInt256_10x26 RandomFENonZero(TestRNG rng)
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


        private static readonly UInt256_10x26 _m1 = new(0xFFFFFC2E, 0xFFFFFFFE, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF);
        internal static ref readonly UInt256_10x26 Minus_One => ref _m1;


        // These tests test the following identities:
        //
        // for x==0: 1/x == 0
        // for x!=0: x*(1/x) == 1
        // for x!=0 and x!=1: 1/(1/x - 1) + 1 == -1/(x-1)
        private static void TestInverseField(in UInt256_10x26 x, int var, out UInt256_10x26 _out)
        {
            UInt256_10x26 l = var == 0 ? x.Inverse() : x.InverseVar();    // l = 1/x
            _out = l;

            UInt256_10x26 t = x;                            // t = x
            if (t.IsZeroNormalizedVar())
            {
                Assert.True(l.IsZeroNormalized());
                return;
            }
            t = x.Multiply(l);                              // t = x*(1/x)
            t = t.Add(Minus_One);                           // t = x*(1/x)-1
            Assert.True(t.IsZeroNormalized());              // x*(1/x)-1 == 0
            UInt256_10x26 r = x;                            // r = x
            r = r.Add(Minus_One);                           // r = x-1
            if (r.IsZeroNormalizedVar())
            {
                return;
            }
            r = var == 0 ? r.Inverse() : r.InverseVar();    // r = 1/(x-1)
            l = l.Add(Minus_One);                           // l = 1/x-1
            l = var == 0 ? l.Inverse() : l.InverseVar();    // l = 1/(1/x-1)
            l = l.Add(1);                                   // l = 1/(1/x-1)+1
            l = l.Add(r);                                   // l = 1/(1/x-1)+1 + 1/(x-1)
            Assert.True(l.IsZeroNormalizedVar());           // l == 0
        }

        public static IEnumerable<object[]> GetInvCases()
        {
            // Fixed test cases for field inverses: pairs of (x, 1/x) mod p.
            yield return new object[]
            {
                // 0
                new UInt256_10x26(0, 0, 0, 0, 0, 0, 0, 0),
                new UInt256_10x26(0, 0, 0, 0, 0, 0, 0, 0)
            };
            yield return new object[]
            {
                // 1
                new UInt256_10x26(1, 0, 0, 0, 0, 0, 0, 0),
                new UInt256_10x26(1, 0, 0, 0, 0, 0, 0, 0)
            };
            yield return new object[]
            {
                // -1
                new UInt256_10x26(0xfffffc2e, 0xfffffffe, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff),
                new UInt256_10x26(0xfffffc2e, 0xfffffffe, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff)
            };
            yield return new object[]
            {
                // 2
                new UInt256_10x26(2, 0, 0, 0, 0, 0, 0, 0),
                new UInt256_10x26(0x7ffffe18, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0x7fffffff)
            };
            yield return new object[]
            {
                // 2**128
                new UInt256_10x26(0, 0, 0, 0, 1, 0, 0, 0),
                new UInt256_10x26(0x434dd931, 0xffffffff, 0xffffffff, 0xffffffff, 0xd2253530, 0xd838091d, 0xdc24a059, 0xbcb223fe)
            };
            yield return new object[]
            {
                // Input known to need 637 divsteps
                new UInt256_10x26(0x19199ec3, 0x7c11ca84, 0x06f3f996, 0x66885408, 0xdb8a1320, 0x0dcb632a, 0x6bee8a84, 0xe34e9c95),
                new UInt256_10x26(0x19b618e5, 0x1aaadf92, 0x8a3f09fb, 0x870152b0, 0x2582ac0c, 0x9bccda44, 0x1c536828, 0xbd2cbd8f)
            };
            yield return new object[]
            {
                // Input known to need 567 divsteps starting with delta=1/2.
                new UInt256_10x26(0xa7549bfc, 0x6672982b, 0x15985661, 0x0988e234, 0x2c21d619, 0x3e46357d, 0x636451c4, 0xf6bc3ba3),
                new UInt256_10x26(0xfbb440ba, 0x389d87d4, 0xeef6d9d0, 0x73df6b75, 0xbd481425, 0x426c585f, 0x5547451e, 0xb024fdc7)
            };
            yield return new object[]
            {
                // Input known to need 566 divsteps starting with delta=1/2.
                new UInt256_10x26(0x8d1063ae, 0x6f87d7a5, 0x29f9e618, 0x9a0a50aa, 0xe4865af7, 0x482dbc65, 0x2e3c1e2f, 0xb595d81b),
                new UInt256_10x26(0xe5b908de, 0x059bd8ef, 0xce6eef86, 0xa0428a0b, 0x0b53afb5, 0x49918330, 0x5d5c74e1, 0xc983337c)
            };
            yield return new object[]
            {
                // Set of 10 inputs accessing all 128 entries in the modinv32 divsteps_var table
                new UInt256_10x26(0x00000000, 0xfeff0100, 0x00000000, 0x00000000, 0x1f000000, 0xe0ff1f80, 0x00000000, 0x00000000),
                new UInt256_10x26(0xd62e9e3d, 0x29eddf8c, 0x045e7fd7, 0x18c9e30c, 0xef70b893, 0x0b5e7a1b, 0x77e5049d, 0x9faf9316)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xccd31192, 0x6277c0d1, 0x32ba0a46, 0x6317c3ac, 0x53f889a4, 0x35688252, 0x511b2780, 0x621a538d),
                new UInt256_10x26(0x6ae8bcff, 0x6a841a4c, 0xeaa66943, 0x34bda011, 0x9b394d8c, 0xe29e882e, 0x5eba856f, 0x38513b0c)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xffff0100, 0xffffffff, 0xfffcffff, 0xffffffff, 0x0000e0ff, 0x00000000, 0xf0ffff1f, 0x00000200),
                new UInt256_10x26(0xedbf8b2f, 0x0484217c, 0xf048c5b6, 0x6c1e3519, 0x0c7591b7, 0x13e64343, 0x3640de9e, 0x5da42a52)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x9f2103e8, 0xa686a8ff, 0x9f26998d, 0x4ab46410, 0x4ea1281b, 0x7c52a2ee, 0x4b952621, 0xd1343ef9),
                new UInt256_10x26(0xcb318bd1, 0xb0775aa3, 0x9ffab128, 0x6b7fb47d, 0xa47e0c46, 0x74e35b6d, 0x9a4619bf, 0x84044385)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xd2e4e440, 0xe926ba62, 0x5df22c6a, 0xbe621bdd, 0xd50d23a4, 0x210db37a, 0xc56a52be, 0xb27235d2),
                new UInt256_10x26(0x53ae429b, 0x8d2775fe, 0xdca9b1bd, 0xb9ec9981, 0xd258ab3d, 0xa568469e, 0x483a9d3c, 0x67a26e54)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xffffffff, 0x000000e0, 0x3f00f00f, 0xffffffff, 0xffffff83, 0x00e0ffff, 0x00000000, 0x00000000),
                new UInt256_10x26(0x5a6acef6, 0x00d0e615, 0xc763bcee, 0x8d357d7f, 0x076c9a45, 0xac94907d, 0x23bbfab0, 0x310e10f8)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x7f0000fe, 0xffff0100, 0x0fffffff, 0xffffffff, 0x0ff0ffff, 0xf80700c0, 0x001c0000, 0xfeff0300),
                new UInt256_10x26(0x59269b0c, 0x1d527a71, 0x32f978d5, 0x530cf21f, 0x3453a370, 0x86f598b0, 0x0709168b, 0x28e2fdb4)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xd5574d2f, 0x85c66753, 0x54d3c453, 0xbb0b28e0, 0x85c14f87, 0x090bb273, 0x7bb98ef7, 0xc2591afa),
                new UInt256_10x26(0x6113b503, 0xba4140ed, 0x5f63a058, 0x07ffb15c, 0x848a6dbb, 0x95e66fae, 0x70ce627c, 0xfdca70a2)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xc7d3e690, 0x8eec1060, 0x1cf5ad27, 0xc625828e, 0xeaeb452f, 0x411c047e, 0xedc7b5a3, 0xf5475db3),
                new UInt256_10x26(0xaecb2c5a, 0x6aba7164, 0xde5eb88d, 0x2e9dec01, 0xec8cc2d8, 0xdc6a215e, 0xf963f4b9, 0x5eb756c0)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x00000000, 0xffffff7f, 0x00000000, 0xe0ff1f00, 0x01000000, 0xffffffff, 0x00f8ffff, 0x00000000),
                new UInt256_10x26(0xbe110037, 0x7bfa444e, 0xf1125d81, 0x7dd28167, 0x1a7f02ca, 0xe54e88c2, 0x49b6157d, 0xe0d2e3d8)
            };
            yield return new object[]
            {
                // Selection of randomly generated inputs that reach high/low d/e values in various configurations.
                new UInt256_10x26(0xffffe950, 0xe24d9be1, 0x09ab3b13, 0xc4109221, 0x54c46c67, 0x179c3e67, 0xd8c41f0f, 0x13cc08a4),
                new UInt256_10x26(0x51008cd1, 0xe92c4441, 0x64767a2d, 0x966dd3d0, 0xcf6714f4, 0xcabd71e5, 0xd16abaa7, 0xb80c8006)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xc6e72057, 0x46f40993, 0x99fffc16, 0xf49ff938, 0x0602e24a, 0x3cc6ff71, 0x95efbca1, 0xaa6db990),
                new UInt256_10x26(0xa5b93e06, 0x9ca482f9, 0xca1d731d, 0x9223f8a9, 0xe639e48c, 0x285f1d49, 0xb0c195e5, 0xd5d3dd69)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x2f67a058, 0x94449e1b, 0x0015f2e0, 0xa3b08108, 0x1781e3de, 0x9bdc4aee, 0xaeabffd8, 0x1c680eac),
                new UInt256_10x26(0xc127444b, 0x32ed1719, 0x4b323393, 0xc5622590, 0x245c373d, 0x6510f475, 0x31254f29, 0x7f083f8d)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xb65b2f0a, 0x267cbc1a, 0x8b962707, 0x9ba6be96, 0x1a44a870, 0xc160d386, 0x012d83f8, 0x147d44b3),
                new UInt256_10x26(0x6ed9087c, 0x0ca6cd33, 0x7a8aded1, 0xafadb458, 0xe51fbd36, 0x50a43002, 0x170aef1e, 0x555554ff)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x4000b98c, 0x916a3c37, 0x3b018013, 0xa1fbf3b2, 0x5384d107, 0xf9ca017c, 0x22f0fe61, 0x12423796),
                new UInt256_10x26(0xfd19b6d7, 0xae38edb9, 0x95ec4589, 0x8ed1fbd2, 0x136c01f5, 0x1177e306, 0x08668f94, 0x20257700)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x3a9dcf21, 0xb5a17695, 0x08909a20, 0x39699b52, 0xdcd23619, 0x93ffa181, 0x9ab42cb4, 0xdcf2d030),
                new UInt256_10x26(0x982b06b6, 0x2e7b12eb, 0xa40b6142, 0x29fe1e40, 0x63a0f51c, 0x4f37180d, 0xe211fb1f, 0x1f701dea)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xdfd5b600, 0x375308c5, 0xf902432e, 0xe32369ea, 0xca1c7d7f, 0xb35a55e6, 0xa6314ed3, 0x79a851f6),
                new UInt256_10x26(0xafe4476a, 0xbd183d71, 0x7895dcbf, 0xa02c8549, 0x38cba42c, 0x9dabb737, 0xe6b43851, 0xcaae00c5)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x307196ec, 0x7fa27c9a, 0x28701870, 0xfb66bc7b, 0xdb8d37e2, 0x4fec6c6c, 0xcfc92bf1, 0xede78fdd),
                new UInt256_10x26(0xa3418265, 0x88865427, 0x1de05422, 0x23ae7bed, 0x13e473f6, 0x2a760c64, 0x9a8b87a7, 0x68193a6c)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x3fc3e66c, 0x2711595d, 0x75138eae, 0x174df343, 0x89baf5ae, 0xa7617997, 0xb8f88e89, 0xa40b2079),
                new UInt256_10x26(0x7fdd2655, 0x3389c93d, 0x6bbae0ed, 0x358c692b, 0x9d9c4576, 0xd4b87c37, 0x6d685267, 0x9f99c6a5)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x08a45696, 0x10a9be07, 0x15b2113a, 0xcefee074, 0x7f06e321, 0x72645cf1, 0xe98d9151, 0x7c74c6b6),
                new UInt256_10x26(0xe227a8ee, 0x8364cc3b, 0xe15bb19e, 0x9ba0ac40, 0x12e655b7, 0x77f26f97, 0x898bc1e0, 0x8c919a88)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xee0e723e, 0xaec5f643, 0xec6eb99b, 0xb7a79e5b, 0xeb1069f4, 0xa1cec2b2, 0xdafa6d4a, 0x109ba1ce),
                new UInt256_10x26(0x7ecb65cc, 0xa407fe1b, 0x6f057a4a, 0x7191401c, 0xdbe9f359, 0xe64f5a71, 0x4bb0bcf9, 0x93d13eb8)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xded36a78, 0x3dbe1e91, 0x74cbc4e0, 0xeeedd2d0, 0x90e23e06, 0xf61dd138, 0xec74a5c9, 0x3db076cd),
                new UInt256_10x26(0xe660b107, 0x16545564, 0xcdd53010, 0xcb92ddbf, 0x02b5e9d5, 0x706c71df, 0x8e2a1e09, 0x3f07f966)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x4cf67b0d, 0x6bf7c0f8, 0xf7d2460c, 0x98b522fd, 0x4cdec153, 0x02ae35f7, 0xb4c4b82c, 0xe31c73ed),
                new UInt256_10x26(0x1fe5b843, 0x59c50666, 0xefaba629, 0xdf0a7ffb, 0xa319cd31, 0x19af0ff6, 0x94e8b070, 0x4b8f1faf)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x12f99870, 0x79652ddb, 0xf552d50d, 0x16698897, 0xbbd85497, 0xc0e3e9f1, 0x83392ab6, 0x4c8b0e6e),
                new UInt256_10x26(0x62da4bca, 0x438544c3, 0x5cc34424, 0xcf18e70a, 0xf24022ef, 0x17dc38d6, 0xd23b7949, 0x56d5101f)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x775417dc, 0xea2cbc90, 0xbc930358, 0x28888137, 0x7fccb178, 0x7dd5c611, 0x40cc35da, 0xb0e040e2),
                new UInt256_10x26(0x7c7800cd, 0x900ae35d, 0xa9b44270, 0x68ed9155, 0x96e08d69, 0xab3ae576, 0x016dd7c8, 0xca37f0d4)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x667ddaef, 0x35055590, 0x862577ef, 0xbdf69178, 0x8e2105b2, 0x69724a9d, 0x7fbb0bae, 0x8a32ea49),
                new UInt256_10x26(0x11768e96, 0x7341e08d, 0xf43645ea, 0x64f9f425, 0xdaef1ffc, 0x559c9d72, 0xc5e190f0, 0xd02d7ead)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xbfffbfc6, 0xb6998f0f, 0x85f9a2ce, 0xe242ab73, 0xbb0857a8, 0x579ebea6, 0x9abe289d, 0xa3592d98),
                new UInt256_10x26(0x44b43ddc, 0x7fe3777a, 0xff525430, 0x589c35f4, 0x0039599e, 0x6aa46070, 0x32032efa, 0x093c1533)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x99b10709, 0x61b961e0, 0x97fb7c6a, 0x1e1bc9c9, 0xcce3fdd9, 0xcc98521a, 0x229e607b, 0x647178a3),
                new UInt256_10x26(0xc8e2feb3, 0xa1fcf17e, 0xcb46d07a, 0x602ca683, 0xdaebd908, 0x96310e77, 0xd51ddf78, 0x98217c13)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xdcab5787, 0x5e3235d8, 0xc658227e, 0x1b95870d, 0xf5964958, 0x99464b4b, 0x73f98968, 0x7334627c),
                new UInt256_10x26(0x98514307, 0x6cc5c74c, 0x2d088418, 0x07603b9b, 0xe51d495c, 0x40ae367a, 0xc7e9dd94, 0x000006fd)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x00005760, 0x335ed01a, 0x7a50825f, 0xc048637d, 0x605c3ad1, 0xa50dd1c5, 0x96c28938, 0x82e83876),
                new UInt256_10x26(0xfec7f02d, 0xb4f9a3ea, 0xf3e16e80, 0x60b3e704, 0x5287d961, 0xf5607e2e, 0x9f2aa55e, 0xb0393f9f)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x771ae51d, 0xe76bb6bd, 0xae813529, 0xfe06297a, 0x3c1970a1, 0x98d24b58, 0x3ee6b8dc, 0xc97b6cec),
                new UInt256_10x26(0x253a5db1, 0xfc34b364, 0x7bf80d0b, 0x79f48f79, 0xf6625419, 0x47ddeb06, 0xd407d097, 0x0507c702)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x0bb99bd3, 0x380bb19d, 0x7ce0dfac, 0x10e7d18b, 0x5c7a4bbb, 0x3cf1ad14, 0x77ea9bc4, 0xd559af63),
                new UInt256_10x26(0xb0ec8b0b, 0xaa8754c8, 0x163356ca, 0xd2daa33a, 0xbbdc42fc, 0x34edfdb5, 0xb9b00d92, 0x00196119)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xcb29fce4, 0xda9c0afc, 0xbde52514, 0xca2d33b2, 0x0af8512a, 0x640519dc, 0x52918da0, 0x8ddfa3dc),
                new UInt256_10x26(0x6ba35b02, 0x7b31e6aa, 0xf09def92, 0x62518ba8, 0xc23acce0, 0xcd54388b, 0x5cb69148, 0xb3e4878d)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x5194fb91, 0x546d41f9, 0xc05da837, 0x00ca112e, 0x0bfff996, 0x65285f2b, 0xe3049f0a, 0xf8207492),
                new UInt256_10x26(0x611c5f95, 0x3b82ea41, 0x290d046e, 0x071441c7, 0x81419a5c, 0xf6469930, 0xa8ed4bbd, 0x7b7ee50b)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x3ec2926a, 0xfd4d3e07, 0xbd8828d7, 0xa4e39f5c, 0x5ce74db7, 0x823cb724, 0x5bcd3c6b, 0x050f7c80),
                new UInt256_10x26(0xbdc3f59e, 0x1d5e91c6, 0xdea0b9db, 0x48fd61da, 0xee157117, 0x4764053d, 0xb0171314, 0x000d6730)
            };
            yield return new object[]
            {
                new UInt256_10x26(0xe67c62f9, 0xbd57087c, 0x3fc182a3, 0x088f6f0d, 0xb3cb3ac9, 0x23009263, 0x05d760cf, 0x3e3ea8eb),
                new UInt256_10x26(0xdd511e8b, 0xebd833dd, 0x51043bf4, 0x49929305, 0xab1e4720, 0x4456aed6, 0xa29c1bf6, 0xbe988716)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x768be5cd, 0x301ac80a, 0x2f487ef6, 0xea0c1b5f, 0x142f4029, 0xa5959249, 0xa7fa6501, 0x6964d2a9),
                new UInt256_10x26(0x2c0cb3c6, 0x9ec2f2ad, 0x0de2191c, 0xaffd7cb4, 0x3df95f8f, 0xed24d0b7, 0x07492543, 0x3918ffe4)
            };
            yield return new object[]
            {
                new UInt256_10x26(0x39877ccb, 0x95c4d156, 0xb95e91f3, 0x11b5b81c, 0xb5c7e4de, 0x2b42fd5e, 0xf6ddca57, 0x37c93520),
                new UInt256_10x26(0x1a6bf956, 0xd728edef, 0xe12a6b1f, 0x077b0595, 0xac5262a8, 0x4c975b8b, 0x57eb71ee, 0x9a94b9b5)
            };
        }

        [Theory]
        [MemberData(nameof(GetInvCases))]
        public void Libsecp256k1_InverseTest(in UInt256_10x26 a, in UInt256_10x26 b)
        {
            // run_inverse_tests

            // Test fixed test cases through test_inverse_{scalar,field}, both ways.
            for (int useVar = 0; useVar <= 1; useVar++)
            {
                TestInverseField(a, useVar, out UInt256_10x26 x_fe);
                Assert.True(b.Equals(x_fe));
                TestInverseField(b, useVar, out x_fe);
                Assert.True(a.Equals(x_fe));
            }
        }


        [Fact]
        public void Libsecp256k1_InverseRandomTest()
        {
            // run_inverse_tests

            UInt256_10x26 x_fe;
            // Test inputs 0..999 and their respective negations.
            byte[] b32 = new byte[32];
            for (int i = 0; i < 1000; i++)
            {
                b32[31] = (byte)i;
                b32[30] = (byte)(i >> 8);
                x_fe = new(b32, out _);
                for (int var = 0; var <= 1; ++var)
                {
                    TestInverseField(x_fe, var, out _);
                }
                x_fe = x_fe.Negate(1);
                for (int var = 0; var <= 1; ++var)
                {
                    TestInverseField(x_fe, var, out _);
                }
            }

            TestRNG rng = new();
            rng.Init(null);
            // test 128*count random inputs; half with testrand256_test, half with testrand256 */
            for (int testrand = 0; testrand <= 1; ++testrand)
            {
                for (int i = 0; i < 64 * COUNT; ++i)
                {
                    if (testrand == 0)
                    {
                        rng.Rand256(b32);
                    }
                    else
                    {
                        rng.Rand256Test(b32);
                    }

                    x_fe = new(b32);
                    for (int var = 0; var <= 1; ++var)
                    {
                        TestInverseField(x_fe, var, out _);
                    }
                }
            }
        }

        #endregion // libsecp256k1 tests
    }
}
