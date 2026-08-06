// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using System;
using System.Collections.Generic;

namespace Tests.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0300:Simplify collection initialization")]
    public class UInt256_5x52Tests
    {
        internal static UInt256_5x52 CreateRandom()
        {
            for (int i = 0; i < 3; i++)
            {
                byte[] ba = Helper.CreateRandomBytes(32);
                UInt256_5x52 result = new(ba, out bool isValid);
                if (isValid)
                {
                    return result;
                }
            }
            throw new Exception("Something is wrong.");
        }

        private static ulong GetRandomULong(Random rng)
        {
            byte[] ba = new byte[sizeof(ulong)];
            rng.NextBytes(ba);
            return BitConverter.ToUInt64(ba);
        }

        private static ulong[] GetUlongArray(Random rng, int len)
        {
            ulong[] result = new ulong[len];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = GetRandomULong(rng);
            }
            return result;
        }

        private static ulong[] MaskArray(Span<ulong> array, ulong mask, ulong lastMask)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                array[i] &= mask;
            }
            array[^1] &= lastMask;

            return array.ToArray();
        }


        internal static void AssertEqual(in UInt256_5x52 actual, ReadOnlySpan<ulong> expected)
        {
            Assert.Equal(expected[0], actual.b0);
            Assert.Equal(expected[1], actual.b1);
            Assert.Equal(expected[2], actual.b2);
            Assert.Equal(expected[3], actual.b3);
            Assert.Equal(expected[4], actual.b4);
        }

        internal static void AssertEqual(in UInt256_4x64 actual, ReadOnlySpan<ulong> expected)
        {
            Assert.Equal(expected[0], actual.b0);
            Assert.Equal(expected[1], actual.b1);
            Assert.Equal(expected[2], actual.b2);
            Assert.Equal(expected[3], actual.b3);
        }

        internal static void AssertEqual(in UInt256_5x52 expected, in UInt256_5x52 actual)
        {
            Assert.Equal(expected.b0, actual.b0);
            Assert.Equal(expected.b1, actual.b1);
            Assert.Equal(expected.b2, actual.b2);
            Assert.Equal(expected.b3, actual.b3);
            Assert.Equal(expected.b4, actual.b4);
#if DEBUG
            Assert.Equal(expected.magnitude, actual.magnitude);
            Assert.Equal(expected.isNormalized, actual.isNormalized);
#endif
        }

        // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/field.h#L66
        // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/field_5x52.h#L37-L43
        internal static UInt256_5x52 SECP256K1_FE_CONST(uint d7, uint d6, uint d5, uint d4, uint d3, uint d2, uint d1, uint d0)
        {
            ulong b0 = d0 | (((ulong)d1 & 0xFFFFFU) << 32);
            ulong b1 = ((ulong)d1 >> 20) | (((ulong)d2) << 12) | (((ulong)d3 & 0xFFU) << 44);
            ulong b2 = ((ulong)d3 >> 8) | (((ulong)d4 & 0xFFFFFFFU) << 24);
            ulong b3 = ((ulong)d4 >> 28) | (((ulong)d5) << 4) | (((ulong)d6 & 0xFFFFU) << 36);
            ulong b4 = ((ulong)d6 >> 16) | (((ulong)d7) << 16);
#if DEBUG
            int magnitude = ((d7 | d6 | d5 | d4 | d3 | d2 | d1 | d0) == 0) ? 0 : 1;
            bool isNormalized = !((d7 & d6 & d5 & d4 & d3 & d2) == 0xfffffffful && (d1 == 0xfffffffful || (d1 == 0xfffffffe && (d0 >= 0xfffffc2f))));
#endif
            return new UInt256_5x52(b0, b1, b2, b3, b4
#if DEBUG
                , magnitude, isNormalized
#endif
                );
        }


        public static IEnumerable<TheoryDataRow<UInt256_5x52, UInt256_5x52>> GetStaticCases()
        {
            // Our private ctor that builds these constants is in reverse order. This little test copies the exact
            // values from our source to make sure there is no accidental mistakes.

            UInt256_5x52 secp256k1_fe_zero = SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 0);
            // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/field.h#L68-L72
            UInt256_5x52 secp256k1_fe_one = SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 1);
            UInt256_5x52 secp256k1_const_beta = SECP256K1_FE_CONST(
                0x7ae96a2bu, 0x657c0710u, 0x6e64479eu, 0xac3434e9u,
                0x9cf04975u, 0x12f58995u, 0xc1396c28u, 0x719501eeu);
            // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/ecdsa_impl.h#L22-L34
            UInt256_5x52 secp256k1_ecdsa_const_order_as_fe = SECP256K1_FE_CONST(
                0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFEU,
                0xBAAEDCE6U, 0xAF48A03BU, 0xBFD25E8CU, 0xD0364141U);
            UInt256_5x52 secp256k1_ecdsa_const_p_minus_order = SECP256K1_FE_CONST(
                0, 0, 0, 1, 0x45512319U, 0x50B75FC4U, 0x402DA172U, 0x2FC9BAEEU);

            yield return new(UInt256_5x52.Beta, secp256k1_const_beta);
            yield return new(UInt256_5x52.Zero, secp256k1_fe_zero);
            yield return new(UInt256_5x52.One, secp256k1_fe_one);
            yield return new(UInt256_5x52.N, secp256k1_ecdsa_const_order_as_fe);
            yield return new(UInt256_5x52.PMinusN, secp256k1_ecdsa_const_p_minus_order);
        }
        [Theory]
        [MemberData(nameof(GetStaticCases))]
        public void StaticMemberTest(UInt256_5x52 actual, UInt256_5x52 expected)
        {
            AssertEqual(expected, actual);
        }


        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(100, 1)]
        public void Constructor_FromUintTest(uint u, int expMagnitude)
        {
            UInt256_5x52 val = new(u);
            Assert.Equal(u, val.b0);
            Assert.Equal(0UL, val.b1);
            Assert.Equal(0UL, val.b2);
            Assert.Equal(0UL, val.b3);
            Assert.Equal(0UL, val.b4);
#if DEBUG
            Assert.True(val.isNormalized);
            Assert.Equal(expMagnitude, val.magnitude);
#endif
        }


        public static IEnumerable<TheoryDataRow<ulong[], ulong[], int>> GetCtor4ULCases()
        {
            yield return new(new ulong[4], new ulong[5], 0);
            yield return new
            (
                new ulong[4]
                {
                    0x1cad265e623a0bf9, 0x8ac083db7e01ca85, 0x3283ddc67b72f91a, 0xcfb44f134b05e00c
                },
                new ulong[5]
                {
                    0x000d265e623a0bf9, 0x000db7e01ca851ca, 0x000b72f91a8ac083, 0x000e00c3283ddc67, 0x0000cfb44f134b05
                },
                1
            );
        }
        [Theory]
        [MemberData(nameof(GetCtor4ULCases))]
        public void Constructor_From4ULongsTest(ulong[] arr, ulong[] exp, int expMagnitude)
        {
            Assert.Equal(4, arr.Length);
            Assert.Equal(5, exp.Length);

            UInt256_5x52 val = new(arr[0], arr[1], arr[2], arr[3]);
            Assert.Equal(exp[0], val.b0);
            Assert.Equal(exp[1], val.b1);
            Assert.Equal(exp[2], val.b2);
            Assert.Equal(exp[3], val.b3);
            Assert.Equal(exp[4], val.b4);
#if DEBUG
            Assert.True(val.isNormalized);
            Assert.Equal(expMagnitude, val.magnitude);
#endif
        }


        public static IEnumerable<TheoryDataRow<ulong[], int, bool>> GetCtor5ULCases()
        {
            yield return new(new ulong[5], 0, true);
            yield return new
            (
                new ulong[5]
                {
                    0x000d265e623a0bf9, 0x000db7e01ca851ca, 0x000b72f91a8ac083, 0x000e00c3283ddc67, 0x0000cfb44f134b05
                },
                1, true
            );
            yield return new
            (
                new ulong[5]
                {
                    0x000d265e623a0bf9, 0x000db7e01ca851ca, 0x000b72f91a8ac083, 0x000e00c3283ddc67, 0x0000cfb44f134b05
                },
                3, false
            );
        }
        [Theory]
        [MemberData(nameof(GetCtor5ULCases))]
        public void Constructor_From5ULongsTest(ulong[] arr, int expMagnitude, bool expNormalize)
        {
            Assert.Equal(5, arr.Length);

            UInt256_5x52 val = new(arr[0], arr[1], arr[2], arr[3], arr[4]
#if DEBUG
                , expMagnitude, expNormalize
#endif
                );
            Assert.Equal(arr[0], val.b0);
            Assert.Equal(arr[1], val.b1);
            Assert.Equal(arr[2], val.b2);
            Assert.Equal(arr[3], val.b3);
            Assert.Equal(arr[4], val.b4);
#if DEBUG
            Assert.Equal(expNormalize, val.isNormalized);
            Assert.Equal(expMagnitude, val.magnitude);
#endif
        }

        [Theory]
        [MemberData(nameof(GetCtor5ULCases))]
        public void Constructor_FromULongArrayTest(ulong[] arr, int expMagnitude, bool expNormalize)
        {
            UInt256_5x52 val = new(arr
#if DEBUG
                , expMagnitude, expNormalize
#endif
                );
            Assert.Equal(arr[0], val.b0);
            Assert.Equal(arr[1], val.b1);
            Assert.Equal(arr[2], val.b2);
            Assert.Equal(arr[3], val.b3);
            Assert.Equal(arr[4], val.b4);
#if DEBUG
            Assert.Equal(expNormalize, val.isNormalized);
            Assert.Equal(expMagnitude, val.magnitude);
#endif
        }


        public static IEnumerable<TheoryDataRow<byte[], ulong[], bool>> GetCtorBytesCases()
        {
            yield return new(new byte[32], new ulong[5], true);
            yield return new
            (
                new byte[32]
                {
                    0x85, 0x33, 0x8a, 0xb7, 0xd7, 0x3f, 0x69, 0xfe, 0x32, 0xb5, 0xfe, 0xf6, 0xa5, 0xdf, 0x65, 0x13,
                    0x74, 0xd4, 0x1d, 0xad, 0xd7, 0xb7, 0x9f, 0xf6, 0xd5, 0xc1, 0x91, 0x9e, 0x50, 0x02, 0x59, 0x1a
                },
                new ulong[5]
                {
                    0x0001919e5002591a, 0x000add7b79ff6d5c, 0x0005df651374d41d, 0x00069fe32b5fef6a, 0x000085338ab7d73f
                },
                true
            );
            yield return new
            (
                new byte[32]
                {
                    0xfc, 0x72, 0x4a, 0xca, 0x68, 0x39, 0x95, 0x68, 0xff, 0x54, 0xd2, 0xf8, 0x5e, 0x3e, 0xdd, 0x85,
                    0xca, 0xbb, 0x0d, 0x3d, 0x34, 0xd7, 0x8e, 0xc3, 0xf9, 0x11, 0xfd, 0xe9, 0xce, 0xac, 0x71, 0x21
                },
                new ulong[5]
                {
                    0x0001fde9ceac7121, 0x0003d34d78ec3f91, 0x000e3edd85cabb0d, 0x0009568ff54d2f85, 0x0000fc724aca6839
                },
                true
            );
            yield return new // P-1
            (
                new byte[32]
                {
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xff, 0xff, 0xfc, 0x2e
                },
                new ulong[5]
                {
                    0x000ffffefffffc2e, 0x000fffffffffffff, 0x000fffffffffffff, 0x000fffffffffffff, 0x0000ffffffffffff
                },
                true
            );
            yield return new // P
            (
                new byte[32]
                {
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe, 0xff, 0xff, 0xfc, 0x2f
                },
                new ulong[5]
                {
                    0x000ffffefffffc2f, 0x000fffffffffffff, 0x000fffffffffffff, 0x000fffffffffffff, 0x0000ffffffffffff
        },
                false
            );
            yield return new // UInt256.Max
            (
                new byte[32]
                {
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                    0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
                },
                new ulong[5]
                {
                    0x000fffffffffffff, 0x000fffffffffffff, 0x000fffffffffffff, 0x000fffffffffffff, 0x0000ffffffffffff
                },
                false
            );
        }
        [Theory]
        [MemberData(nameof(GetCtorBytesCases))]
        public void Constructor_FromBytesTest(byte[] arr, ulong[] exp, bool expB)
        {
            UInt256_5x52 val_UnChecked = new(arr);
            UInt256_5x52 val_Checkd = new(arr, out bool isValid);
            Assert.Equal(expB, isValid);
            AssertEqual(val_UnChecked, exp);
            AssertEqual(val_Checkd, exp);

#if DEBUG
            // Unchecked value will always have magnitude 1 and will not be normalized
            Assert.Equal(1, val_UnChecked.magnitude);
            Assert.False(val_UnChecked.isNormalized);

            // Checked value will set magnitude to 1 or -1 depending on whether it is valid.
            // Valid values are normalized.
            Assert.Equal(expB, val_Checkd.isNormalized);
            if (expB)
            {
                Assert.Equal(1, val_Checkd.magnitude);
            }
            else
            {
                Assert.Equal(-1, val_Checkd.magnitude);
            }
#endif
        }


        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, false)]
        [InlineData(3, true)]
        [InlineData(uint.MaxValue - 1, false)] // ...94
        [InlineData(uint.MaxValue, true)] // ...95
        public void IsOddTest(uint u, bool expected)
        {
            UInt256_5x52 value = new(u);
            Assert.Equal(expected, value.IsOdd);
        }


        [Theory]
        [InlineData(0, 0, 0, 0, 0, true)]
        [InlineData(1, 0, 0, 0, 0, false)]
        [InlineData(0, 1, 0, 0, 0, false)]
        [InlineData(0, 0, 1, 0, 0, false)]
        [InlineData(0, 0, 0, 1, 0, false)]
        [InlineData(0, 0, 0, 0, 1, false)]
        [InlineData(0, 0, 0, 1, 1, false)]
        [InlineData(1, 1, 1, 1, 1, false)]
        public void IsZeroTest(ulong u0, ulong u1, uint u2, uint u3, uint u4, bool expected)
        {
            UInt256_5x52 value = new(u0, u1, u2, u3, u4
#if DEBUG
                , 1, true
#endif
                );
            Assert.Equal(expected, value.IsZero);
        }


        public static IEnumerable<TheoryDataRow<ulong[], uint, ulong[]>> GetAddUintCases()
        {
            Random rng = new(47);
            ulong u0 = GetRandomULong(rng) & 0x000FFFFFFFFFFFFFUL;
            ulong u1 = GetRandomULong(rng) & 0x000FFFFFFFFFFFFFUL;
            ulong u2 = GetRandomULong(rng) & 0x000FFFFFFFFFFFFFUL;
            ulong u3 = GetRandomULong(rng) & 0x000FFFFFFFFFFFFFUL;
            ulong u4 = GetRandomULong(rng) & 0x0000FFFFFFFFFFFFUL;

            yield return new(new ulong[5] { u0, u1, u2, u3, u4 }, 0, new ulong[5] { u0, u1, u2, u3, u4 });
            yield return new(new ulong[5] { u0, u1, u2, u3, u4 }, 1, new ulong[5] { u0 + 1, u1, u2, u3, u4 });
            yield return new(new ulong[5] { u0, u1, u2, u3, u4 }, 0x7FFF, new ulong[5] { u0 + 0x7FFF, u1, u2, u3, u4 });
        }
        [Theory]
        [MemberData(nameof(GetAddUintCases))]
        public void Add_UintTest(ulong[] array, uint u, ulong[] expected)
        {
            Assert.Equal(5, array.Length);
            Assert.Equal(5, expected.Length);

            UInt256_5x52 a = new(array
#if DEBUG
                , 1, true
#endif
                );

            UInt256_5x52 actual1 = a + u;
            UInt256_5x52 actual2 = a.Add(u);

            AssertEqual(actual1, expected);
            AssertEqual(actual2, expected);
#if DEBUG
            Assert.False(actual1.isNormalized);
            Assert.False(actual2.isNormalized);
            Assert.Equal(2, actual1.magnitude);
            Assert.Equal(2, actual2.magnitude);
#endif
        }


        private static ulong[] Add(ReadOnlySpan<ulong> a, ReadOnlySpan<ulong> b)
        {
            Assert.Equal(a.Length, b.Length);
            ulong[] result = new ulong[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] + b[i];
            }
            return result;
        }
        public static IEnumerable<TheoryDataRow<ulong[], ulong[], ulong[]>> GetAddCases()
        {
            Random rng = new(47);
            ulong[] array1 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);
            ulong[] array2 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);

            yield return new(array1, array2, Add(array1, array2));
        }
        [Theory]
        [MemberData(nameof(GetAddCases))]
        public void AddTest(ulong[] arr1, ulong[] arr2, ulong[] expected)
        {
            UInt256_5x52 a = new(arr1
#if DEBUG
                , 1, true
#endif
                );
            UInt256_5x52 b = new(arr2
#if DEBUG
                , 1, true
#endif
                );

            UInt256_5x52 actual1 = a + b;
            UInt256_5x52 actual2 = b + a;
            UInt256_5x52 actual3 = a.Add(b);
            UInt256_5x52 actual4 = b.Add(a);

            AssertEqual(actual1, expected);
            AssertEqual(actual2, expected);
            AssertEqual(actual3, expected);
            AssertEqual(actual4, expected);

#if DEBUG
            Assert.False(actual1.isNormalized);
            Assert.False(actual2.isNormalized);
            Assert.False(actual3.isNormalized);
            Assert.False(actual4.isNormalized);

            int expectedMagnitude = a.magnitude + b.magnitude;
            Assert.Equal(expectedMagnitude, actual1.magnitude);
            Assert.Equal(expectedMagnitude, actual2.magnitude);
            Assert.Equal(expectedMagnitude, actual3.magnitude);
            Assert.Equal(expectedMagnitude, actual4.magnitude);
#endif
        }


        public static IEnumerable<TheoryDataRow<ulong[], ulong[], ulong[], ulong[]>> GetAdd3Cases()
        {
            Random rng = new(47);
            ulong[] array1 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);
            ulong[] array2 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);
            ulong[] array3 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);

            yield return new(array1, array2, array3, Add(Add(array1, array2), array3));
        }
        [Theory]
        [MemberData(nameof(GetAdd3Cases))]
        public void Add3Test(ulong[] arr1, ulong[] arr2, ulong[] arr3, ulong[] expected)
        {
            UInt256_5x52 a = new(arr1
#if DEBUG
                , 1, true
#endif
                );
            UInt256_5x52 b = new(arr2
#if DEBUG
                , 1, true
#endif
                );
            UInt256_5x52 c = new(arr3
#if DEBUG
                , 1, true
#endif
                );

            UInt256_5x52 actual = UInt256_5x52.Add(a, b, c);
            AssertEqual(actual, expected);
#if DEBUG
            Assert.False(actual.isNormalized);
            Assert.Equal(a.magnitude + b.magnitude + c.magnitude, actual.magnitude);
#endif
        }



        public static IEnumerable<TheoryDataRow<ulong[], ulong[], ulong[], ulong[], ulong[]>> GetAdd4Cases()
        {
            Random rng = new(47);
            ulong[] array1 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);
            ulong[] array2 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);
            ulong[] array3 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);
            ulong[] array4 = MaskArray(GetUlongArray(rng, 5), 0x000FFFFFFFFFFFFFUL, 0x0000FFFFFFFFFFFFUL);

            yield return new(array1, array2, array3, array4, Add(Add(Add(array1, array2), array3), array4));
        }
        [Theory]
        [MemberData(nameof(GetAdd4Cases))]
        public void Add4Test(ulong[] arr1, ulong[] arr2, ulong[] arr3, ulong[] arr4, ulong[] expected)
        {
            UInt256_5x52 a = new(arr1
#if DEBUG
                , 1, true
#endif
                );
            UInt256_5x52 b = new(arr2
#if DEBUG
                , 1, true
#endif
                );
            UInt256_5x52 c = new(arr3
#if DEBUG
                , 1, true
#endif
                );
            UInt256_5x52 d = new(arr4
#if DEBUG
                , 1, true
#endif
                );

            UInt256_5x52 actual = UInt256_5x52.Add(a, b, c, d);
            AssertEqual(actual, expected);
#if DEBUG
            Assert.False(actual.isNormalized);
            Assert.Equal(a.magnitude + b.magnitude + c.magnitude + d.magnitude, actual.magnitude);
#endif
        }


        [Fact]
        public void NegateTest()
        {
            UInt256_5x52 a = CreateRandom();
            UInt256_5x52 an = a.Negate(1);
            UInt256_5x52 actual = a.Add(an);
            actual = actual.Normalize();

            Assert.True(actual.IsZero);
        }


        [Fact]
        public void SqrVsMultTest()
        {
            UInt256_5x52 a = CreateRandom();

            UInt256_5x52 a_mult2 = a * a;
            UInt256_5x52 a_sqr1 = a.Sqr(1);
            AssertEqual(a_mult2, a_sqr1);

            UInt256_5x52 a_mult4 = a * a * a * a;
            UInt256_5x52 a_sqr2 = a.Sqr(2);
            AssertEqual(a_mult4, a_sqr2);

            UInt256_5x52 a_mult8 = a * a * a * a * a * a * a * a;
            UInt256_5x52 a_sqr3 = a.Sqr(3);
            AssertEqual(a_mult8, a_sqr3);
        }


        #region https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/tests.c#L3050-L3480

        private const int COUNT = 16;

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

        private static ulong Libsecp256k1_CmpVar(ReadOnlySpan<ulong> p1, ReadOnlySpan<ulong> p2, int n)
        {
            for (int i = 0; i < n; i++)
            {
                ulong diff = p1[i] - p2[i];
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
        internal static ulong Libsecp256k1_CmpVar(in UInt256_4x64 a, in UInt256_4x64 b)
        {
            ReadOnlySpan<ulong> p1 = new ulong[4] { a.b0, a.b1, a.b2, a.b3 };
            ReadOnlySpan<ulong> p2 = new ulong[4] { b.b0, b.b1, b.b2, b.b3 };
            for (int i = 0; i < p1.Length; i++)
            {
                ulong diff = p1[i] - p2[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return 0;
        }

        // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/testutil.h
        /// <summary>
        /// testutil_random_fe
        /// </summary>
        internal static UInt256_5x52 RandomFE(TestRNG rng)
        {
            byte[] bin = new byte[32];
            do
            {
                rng.Rand256(bin);
                UInt256_5x52 x = new(bin, out bool isValid);
                if (isValid)
                {
                    return x;
                }
            } while (true);
        }

        /// <summary>
        /// testutil_random_fe_magnitude
        /// </summary>
        /// <returns></returns>
        internal static UInt256_5x52 RandomFEMagnitude(TestRNG rng, in UInt256_5x52 fe, int m)
        {
            uint n = rng.RandInt((uint)(m + 1));
            UInt256_5x52 res = fe.Normalize();
            if (n == 0)
            {
                return res;
            }
            UInt256_5x52 zero = new(0);
            zero = zero.Negate(0);
            zero = zero.Multiply(n - 1);
            res = res.Add(zero);
#if DEBUG
            Assert.Equal((int)n, res.magnitude);
#endif
            return res;
        }

        /// <summary>
        /// testutil_random_fe_test
        /// </summary>
        internal static UInt256_5x52 RandomFETest(TestRNG rng)
        {
            byte[] bin = new byte[32];
            do
            {
                rng.Rand256Test(bin);
                UInt256_5x52 x = new(bin, out bool isValid);
                if (isValid)
                {
                    return x;
                }
            } while (true);
        }

        /// <summary>
        /// testutil_random_fe_magnitude
        /// </summary>
        internal static void RandomFEMagnitude(ref UInt256_5x52 fe, int m, TestRNG rng)
        {
            int n = (int)rng.RandInt((uint)(m + 1));
            fe = fe.Normalize();
            if (n == 0)
            {
                return;
            }
            UInt256_5x52 zero = UInt256_5x52.Zero.Negate(0);
            zero = zero.Multiply((uint)(n - 1));
            fe = fe.Add(zero);
#if DEBUG
            Assert.True(fe.magnitude == n);
#endif
        }

        /// <summary>
        /// testutil_random_fe_non_zero
        /// </summary>
        internal static UInt256_5x52 RandomFENonZero(TestRNG rng)
        {
            UInt256_5x52 result;
            do
            {
                result = RandomFE(rng);
            } while (result.IsZero);
            return result;
        }

        /// <summary>
        /// random_fe_non_square
        /// </summary>
        private static UInt256_5x52 RandomFENonSquare(TestRNG rng)
        {
            UInt256_5x52 ns = RandomFENonZero(rng);
            if (ns.Sqrt(out _))
            {
                ns = ns.Negate(1);
            }
            return ns;
        }

        /// <summary>
        /// fe_equal
        /// </summary>
        private static bool CheckFEEqual(in UInt256_5x52 a, in UInt256_5x52 b)
        {
            UInt256_5x52 an = a.NormalizeWeak();
            return an.Equals(b);
        }

        /// <summary>
        /// run_fe_equal_magnitude_boundaries
        /// </summary>
        [Fact]
        public void Libsecp256k1_EqualMagnitudeBoundariesTest()
        {
            TestRNG rng = new();
            rng.Init(null);

            UInt256_5x52 a, b;
            for (int i = 0; i < 100 * COUNT; i++)
            {
                a = RandomFE(rng);
                b = a;
                a = RandomFEMagnitude(rng, a, 1);
                b = RandomFEMagnitude(rng, b, 30);
                Assert.True(a.Equals(b));
            }
        }


        /// <summary>
        /// run_field_convert
        /// </summary>
        [Fact]
        public void Libsecp256k1_FieldConvertTest()
        {
            ReadOnlySpan<byte> b32 = new byte[32]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29,
                0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40
            };
            UInt256_4x64 fes = UInt256_4x64Tests.SECP256K1_FE_STORAGE_CONST(
                0x00010203U, 0x04050607U, 0x11121314U, 0x15161718U,
                0x22232425U, 0x26272829U, 0x33343536U, 0x37383940U);
            UInt256_5x52 fe = SECP256K1_FE_CONST(
                0x00010203U, 0x04050607U, 0x11121314U, 0x15161718U,
                0x22232425U, 0x26272829U, 0x33343536U, 0x37383940U);

            // Check conversions to fe
            UInt256_5x52 fe2 = new(b32, out bool isValid);
            Assert.True(isValid);
            Assert.True(fe.Equals(fe2));
            fe2 = fes.ToUInt256_5x52();
            Assert.True(fe.Equals(fe2));
            // Check conversion from fe
            Span<byte> b322 = fe.ToSpan();
            Assert.Equal(b32, b322);
            UInt256_4x64 fes2 = fe.ToUInt256_4x64();
            Assert.Equal(0UL, Libsecp256k1_CmpVar(fes, fes2));
        }

        /// <summary>
        /// run_field_be32_overflow
        /// </summary>
        [Fact]
        public void Libsecp256k1_FieldBe32OverflowTest()
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            {
                ReadOnlySpan<byte> zero_overflow = new byte[32]
                {
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFE, 0xFF, 0xFF, 0xFC, 0x2F,
                };
                ReadOnlySpan<byte> zero = new byte[32];
                UInt256_5x52 fe = new(zero_overflow, out bool isValid);
                Assert.False(isValid);
                fe = new(zero_overflow);
                Assert.True(fe.IsZeroNormalized());
                fe = fe.Normalize();
                Assert.True(fe.IsZero);
                Span<byte> actual = fe.ToSpan();
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

                UInt256_5x52 fe = new(one_overflow, out bool isValid);
                Assert.False(isValid);
                fe = new(one_overflow);
                fe = fe.Normalize();
                Assert.Equal(0, fe.CompareToVar(UInt256_5x52.One));
                Span<byte> actual = fe.ToSpan();
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

                UInt256_5x52 fe_ff = SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0x01, 0x000003d0);
                UInt256_5x52 fe = new(ff_overflow, out bool isValid);
                Assert.False(isValid);
                fe = new(ff_overflow);
                fe = fe.Normalize();
                Assert.Equal(0, fe.CompareToVar(fe_ff));
                Span<byte> actual = fe.ToSpan();
                Assert.Equal(ff, actual);
            }
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }


        /// <summary>
        /// Returns true if two field elements have the same representation.
        /// <para/>fe_identical
        /// </summary>
        private static int FEIdentical(in UInt256_5x52 a, in UInt256_5x52 b)
        {
            int ret = 1;
            // Compare the struct member that holds the limbs.
            ReadOnlySpan<ulong> an = new ulong[5] { a.b0, a.b1, a.b2, a.b3, a.b4 };
            ReadOnlySpan<ulong> bn = new ulong[5] { b.b0, b.b1, b.b2, b.b3, b.b4 };

            ret &= (Libsecp256k1_CmpVar(an, bn, an.Length) == 0) ? 1 : 0;
            return ret;
        }


        /// <summary>
        /// run_field_half
        /// </summary>
        [Fact]
        public void Libsecp256k1_FieldHalfTest()
        {
            // Check magnitude 0 input
            UInt256_5x52 t = UInt256_5x52.GetBounds(0);

            t = t.Half();
#if DEBUG
            Assert.Equal(1, t.magnitude);
            Assert.False(t.isNormalized);
#endif
            Assert.True(t.IsZeroNormalized());

            // Check non-zero magnitudes in the supported range
            for (int m = 1; m < 32; m++)
            {
                // Check max-value input
                t = UInt256_5x52.GetBounds(m);

                UInt256_5x52 u = t.Half();
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
                t = UInt256_5x52.GetBounds(m);
                Assert.True(t.b0 > 0);
                Assert.Equal(0UL, t.b0 & 1);
                // --t.n[0]; our structs are immutable!
                t = new(t.b0 - 1, t.b1, t.b2, t.b3, t.b4
#if DEBUG
                    , t.magnitude, t.isNormalized
#endif
                    );

                u = t.Half();
#if DEBUG
                Assert.True(u.magnitude == (m >> 1) + 1);
                Assert.False(u.isNormalized);
#endif
                u = u.NormalizeWeak();
                u = u.Add(u);
                Assert.True(CheckFEEqual(t, u));
            }
        }


        /// <summary>
        /// run_field_misc
        /// </summary>
        [Fact]
        public void Libsecp256k1_FieldMiscTest()
        {
            TestRNG rng = new();
            rng.Init(null);

            UInt256_5x52 fe5 = SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 5);
            for (int i = 0; i < 1000 * COUNT; i++)
            {
                UInt256_5x52 x = (i & 1) != 0 ? RandomFE(rng) : RandomFETest(rng);
                UInt256_5x52 y = RandomFENonZero(rng);
                uint v = (uint)rng.RandBits(15);
                // Test that fe_add_int is equivalent to fe_set_int + fe_add.
                UInt256_5x52 q = new(v); // q = v
                UInt256_5x52 z = x; // z = x
                z = z.Add(q); // z = x+v
                q = x; // q = x
                q = q.Add(v); // q = x+v
                Assert.True(CheckFEEqual(q, z));
                // Test the fe equality and comparison operations.
                Assert.Equal(0, x.CompareToVar(x));
                Assert.True(x.Equals(x));
                z = x;
                z = z.Add(y);
                // Test fe conditional move; z is not normalized here.
                q = x;
                x = UInt256_5x52.CMov(x, z, 0);
#if DEBUG
                Assert.False(x.isNormalized);
                Assert.True((x.magnitude == q.magnitude) || (x.magnitude == z.magnitude));
                Assert.True((x.magnitude >= q.magnitude) && (x.magnitude >= z.magnitude));
#endif
                x = q;
                x = UInt256_5x52.CMov(x, x, 1);
                Assert.Equal(0, FEIdentical(x, z));
                Assert.NotEqual(0, FEIdentical(x, q));
                q = UInt256_5x52.CMov(q, z, 1);
#if DEBUG
                Assert.False(q.isNormalized);
                Assert.True((q.magnitude == x.magnitude) || (q.magnitude == z.magnitude));
                Assert.True((q.magnitude >= x.magnitude) && (q.magnitude >= z.magnitude));
#endif
                Assert.NotEqual(0, FEIdentical(q, z));
                q = z;
                x = x.NormalizeVar();
                z = z.NormalizeVar();
                Assert.False(x.Equals(z));
                q = q.NormalizeVar();
                q = UInt256_5x52.CMov(q, z, (uint)(i & 1));
#if DEBUG
                Assert.True(q.isNormalized && q.magnitude == 1);
#endif
                for (int j = 0; j < 6; j++)
                {
                    z = z.Negate(j + 1);
                    q = q.NormalizeVar();
                    q = UInt256_5x52.CMov(q, z, (uint)(j & 1));
#if DEBUG
                    Assert.True(!q.isNormalized && q.magnitude == z.magnitude);
#endif
                }
                z = z.NormalizeVar();
                // Test storage conversion and conditional moves.
                UInt256_4x64 xs = x.ToUInt256_4x64();
                UInt256_4x64 ys = y.ToUInt256_4x64();
                UInt256_4x64 zs = z.ToUInt256_4x64();
                zs = UInt256_4x64.CMov(zs, xs, 0);
                zs = UInt256_4x64.CMov(zs, zs, 1);
                Assert.NotEqual(0UL, Libsecp256k1_CmpVar(xs, zs));
                ys = UInt256_4x64.CMov(ys, xs, 1);
                Assert.Equal(0UL, Libsecp256k1_CmpVar(xs, ys));
                x = xs.ToUInt256_5x52();
                y = ys.ToUInt256_5x52();
                z = zs.ToUInt256_5x52();
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
        private static void TestFEMul(in UInt256_5x52 a, in UInt256_5x52 b, bool use_sqr)
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
            UInt256_5x52 c = use_sqr ? a.Sqr() : a.Multiply(b);
            // Convert A, B, C into LE 16x uint16_t format.
            c = c.NormalizeVar();
            UInt256_5x52 an = a.NormalizeVar();
            UInt256_5x52 bn = b.NormalizeVar();
            // Variables in BE 32-byte format.
            Span<byte> a32 = an.ToSpan();
            Span<byte> b32 = bn.ToSpan();
            Span<byte> c32 = c.ToSpan();
            for (int i = 0; i < 16; i++)
            {
                a16[i] = (ushort)(a32[31 - 2 * i] + (a32[30 - 2 * i] << 8));
                b16[i] = (ushort)(b32[31 - 2 * i] + (b32[30 - 2 * i] << 8));
                c16[i] = (ushort)(c32[31 - 2 * i] + (c32[30 - 2 * i] << 8));
            }
            // Compute T = A * B in LE 16x uint16_t format.
            Span<ushort> t16 = ModInv32Tests.MulMod256(a16, b16, m16);
            // Compare
            // 16 items length which is 32 byte
            Assert.Equal(0, Libsecp256k1_CmpVar(t16, c16, 16));
        }


        /// <summary>
        /// run_fe_mul
        /// </summary>
        [Fact]
        public void Libsecp256k1_FEMulTest()
        {
            TestRNG rng = new();
            rng.Init(null);

            for (int i = 0; i < 100 * COUNT; i++)
            {
                UInt256_5x52 a = RandomFE(rng);
                RandomFEMagnitude(ref a, 8, rng);
                UInt256_5x52 b = RandomFE(rng);
                RandomFEMagnitude(ref b, 8, rng);
                UInt256_5x52 c = RandomFETest(rng);
                RandomFEMagnitude(ref c, 8, rng);
                UInt256_5x52 d = RandomFETest(rng);
                RandomFEMagnitude(ref d, 8, rng);
                TestFEMul(a, a, true);
                TestFEMul(c, c, true);
                TestFEMul(a, b, false);
                TestFEMul(a, c, false);
                TestFEMul(c, b, false);
                TestFEMul(c, d, false);
            }
        }


        /// <summary>
        /// run_sqr
        /// </summary>
        [Fact]
        public void Libsecp256k1_SqrTest()
        {
            TestRNG rng = new();
            rng.Init(null);

            UInt256_5x52 x = new(1);
            x = x.Negate(1);

            for (int i = 1; i <= 512; i++)
            {
                x = x.Multiply(2);
                x = x.Normalize();

                // Check that (x+y)*(x-y) = x^2 - y*2 for some random values y
                UInt256_5x52 y = RandomFETest(rng);

                UInt256_5x52 lhs = x.Add(y);       // lhs = x+y
                UInt256_5x52 tmp = y.Negate(1);    // tmp = -y
                tmp = tmp.Add(x);                  // tmp = x-y
                lhs = lhs.Multiply(tmp);           // lhs = (x+y)*(x-y)

                UInt256_5x52 rhs = x.Sqr();        // rhs = x^2
                tmp = y.Sqr();                     // tmp = y^2
                tmp = tmp.Negate(1);               // tmp = -y^2
                rhs = rhs.Add(tmp);                // rhs = x^2 - y^2

                Assert.True(CheckFEEqual(lhs, rhs));
            }
        }


        /// <summary>
        /// test_sqrt
        /// </summary>
        private static void TestSqrt(in UInt256_5x52 a, in UInt256_5x52? k)
        {
            bool v = a.Sqrt(out UInt256_5x52 r1);
            Assert.True((v == false) == (k == null));

            if (k != null)
            {
                // Check that the returned root is +/- the given known answer
                UInt256_5x52 r2 = r1.Negate(1);
                r1 = r1.Add(k.Value); r2 = r2.Add(k.Value);
                r1 = r1.Normalize(); r2 = r2.Normalize();
                Assert.True(r1.IsZero || r2.IsZero);
            }
        }

        /// <summary>
        /// run_sqrt
        /// </summary>
        [Fact]
        public void Libsecp256k1_SqrtTest()
        {
            TestRNG rng = new();
            rng.Init(null);

            // Check sqrt(0) is 0
            UInt256_5x52 x = new(0);
            UInt256_5x52 s = x.Sqr();
            TestSqrt(s, x);

            // Check sqrt of small squares (and their negatives)
            for (uint i = 1; i <= 100; i++)
            {
                x = new(i);
                s = x.Sqr();
                TestSqrt(s, x);
                UInt256_5x52 t = s.Negate(1);
                TestSqrt(t, null);
            }

            // Consistency checks for large random values
            for (int i = 0; i < 10; i++)
            {
                UInt256_5x52 ns = RandomFENonSquare(rng);
                for (int j = 0; j < COUNT; j++)
                {
                    x = RandomFE(rng);
                    s = x.Sqr();
                    Assert.True(s.IsSquareVar());
                    TestSqrt(s, x);
                    UInt256_5x52 t = s.Negate(1);
                    Assert.False(t.IsSquareVar());
                    TestSqrt(t, null);
                    t = s.Multiply(ns);
                    TestSqrt(t, null);
                }
            }
        }


        /// <summary>
        /// fe_cmov_test
        /// </summary>
        [Fact]
        public void Libsecp256k1_CMovTest()
        {
            // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/tests.c#L7814-L7842
            UInt256_5x52 zero = SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 0);
            UInt256_5x52 one = SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 1);
            UInt256_5x52 max = SECP256K1_FE_CONST(
                0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU,
                0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU);
            UInt256_5x52 r = max;
            UInt256_5x52 a = zero;

            r = UInt256_5x52.CMov(r, a, 0);
            Assert.Equal(1, FEIdentical(r, max));

            r = zero; a = max;
            r = UInt256_5x52.CMov(r, a, 1);
            Assert.Equal(1, FEIdentical(r, max));

            a = zero;
            r = UInt256_5x52.CMov(r, a, 1);
            Assert.Equal(1, FEIdentical(r, zero));

            a = one;
            r = UInt256_5x52.CMov(r, a, 1);
            Assert.Equal(1, FEIdentical(r, one));

            r = one; a = zero;
            r = UInt256_5x52.CMov(r, a, 0);
            Assert.Equal(1, FEIdentical(r, one));
        }


        // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/tests.c#L3482-L3792
        private static readonly UInt256_5x52 _m1 = SECP256K1_FE_CONST(
            0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF,
            0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFE, 0xFFFFFC2E);
        internal static ref readonly UInt256_5x52 Minus_One => ref _m1;


        // These tests test the following identities:
        //
        // for x==0: 1/x == 0
        // for x!=0: x*(1/x) == 1
        // for x!=0 and x!=1: 1/(1/x - 1) + 1 == -1/(x-1)
        private static void TestInverseField(in UInt256_5x52 x, int var, out UInt256_5x52 _out)
        {
            UInt256_5x52 l = var == 0 ? x.Inverse() : x.InverseVar();    // l = 1/x
            _out = l;

            UInt256_5x52 t = x;                             // t = x
            if (t.IsZeroNormalizedVar())
            {
                Assert.True(l.IsZeroNormalized());
                return;
            }
            t = x.Multiply(l);                              // t = x*(1/x)
            t = t.Add(Minus_One);                           // t = x*(1/x)-1
            Assert.True(t.IsZeroNormalized());              // x*(1/x)-1 == 0
            UInt256_5x52 r = x;                             // r = x
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

        public static IEnumerable<TheoryDataRow<UInt256_5x52, UInt256_5x52>> GetInvCases()
        {
            // Fixed test cases for field inverses: pairs of (x, 1/x) mod p.
            yield return new
            (
                // 0
                SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 0),
                SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 0)
            );
            yield return new
            (
                // 1
                SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 1),
                SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 1)
            );
            yield return new
            (
                // -1
                SECP256K1_FE_CONST(0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xfffffffe, 0xfffffc2e),
                SECP256K1_FE_CONST(0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xfffffffe, 0xfffffc2e)
            );
            yield return new
            (
                /* 2 */
                SECP256K1_FE_CONST(0, 0, 0, 0, 0, 0, 0, 2),
                SECP256K1_FE_CONST(0x7fffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff, 0x7ffffe18)
            );
            yield return new
            (
                /* 2**128 */
                SECP256K1_FE_CONST(0, 0, 0, 1, 0, 0, 0, 0),
                SECP256K1_FE_CONST(0xbcb223fe, 0xdc24a059, 0xd838091d, 0xd2253530, 0xffffffff, 0xffffffff, 0xffffffff, 0x434dd931)
            );
            yield return new
            (
                /* Input known to need 637 divsteps */
                SECP256K1_FE_CONST(0xe34e9c95, 0x6bee8a84, 0x0dcb632a, 0xdb8a1320, 0x66885408, 0x06f3f996, 0x7c11ca84, 0x19199ec3),
                SECP256K1_FE_CONST(0xbd2cbd8f, 0x1c536828, 0x9bccda44, 0x2582ac0c, 0x870152b0, 0x8a3f09fb, 0x1aaadf92, 0x19b618e5)
            );
            yield return new
            (
                /* Input known to need 567 divsteps starting with delta=1/2. */
                SECP256K1_FE_CONST(0xf6bc3ba3, 0x636451c4, 0x3e46357d, 0x2c21d619, 0x0988e234, 0x15985661, 0x6672982b, 0xa7549bfc),
                SECP256K1_FE_CONST(0xb024fdc7, 0x5547451e, 0x426c585f, 0xbd481425, 0x73df6b75, 0xeef6d9d0, 0x389d87d4, 0xfbb440ba)
            );
            yield return new
            (
                /* Input known to need 566 divsteps starting with delta=1/2. */
                SECP256K1_FE_CONST(0xb595d81b, 0x2e3c1e2f, 0x482dbc65, 0xe4865af7, 0x9a0a50aa, 0x29f9e618, 0x6f87d7a5, 0x8d1063ae),
                SECP256K1_FE_CONST(0xc983337c, 0x5d5c74e1, 0x49918330, 0x0b53afb5, 0xa0428a0b, 0xce6eef86, 0x059bd8ef, 0xe5b908de)
            );
            yield return new
            (
                /* Set of 10 inputs accessing all 128 entries in the modinv32 divsteps_var table */
                SECP256K1_FE_CONST(0x00000000, 0x00000000, 0xe0ff1f80, 0x1f000000, 0x00000000, 0x00000000, 0xfeff0100, 0x00000000),
                SECP256K1_FE_CONST(0x9faf9316, 0x77e5049d, 0x0b5e7a1b, 0xef70b893, 0x18c9e30c, 0x045e7fd7, 0x29eddf8c, 0xd62e9e3d)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x621a538d, 0x511b2780, 0x35688252, 0x53f889a4, 0x6317c3ac, 0x32ba0a46, 0x6277c0d1, 0xccd31192),
                SECP256K1_FE_CONST(0x38513b0c, 0x5eba856f, 0xe29e882e, 0x9b394d8c, 0x34bda011, 0xeaa66943, 0x6a841a4c, 0x6ae8bcff)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x00000200, 0xf0ffff1f, 0x00000000, 0x0000e0ff, 0xffffffff, 0xfffcffff, 0xffffffff, 0xffff0100),
                SECP256K1_FE_CONST(0x5da42a52, 0x3640de9e, 0x13e64343, 0x0c7591b7, 0x6c1e3519, 0xf048c5b6, 0x0484217c, 0xedbf8b2f)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xd1343ef9, 0x4b952621, 0x7c52a2ee, 0x4ea1281b, 0x4ab46410, 0x9f26998d, 0xa686a8ff, 0x9f2103e8),
                SECP256K1_FE_CONST(0x84044385, 0x9a4619bf, 0x74e35b6d, 0xa47e0c46, 0x6b7fb47d, 0x9ffab128, 0xb0775aa3, 0xcb318bd1)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xb27235d2, 0xc56a52be, 0x210db37a, 0xd50d23a4, 0xbe621bdd, 0x5df22c6a, 0xe926ba62, 0xd2e4e440),
                SECP256K1_FE_CONST(0x67a26e54, 0x483a9d3c, 0xa568469e, 0xd258ab3d, 0xb9ec9981, 0xdca9b1bd, 0x8d2775fe, 0x53ae429b)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x00000000, 0x00000000, 0x00e0ffff, 0xffffff83, 0xffffffff, 0x3f00f00f, 0x000000e0, 0xffffffff),
                SECP256K1_FE_CONST(0x310e10f8, 0x23bbfab0, 0xac94907d, 0x076c9a45, 0x8d357d7f, 0xc763bcee, 0x00d0e615, 0x5a6acef6)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xfeff0300, 0x001c0000, 0xf80700c0, 0x0ff0ffff, 0xffffffff, 0x0fffffff, 0xffff0100, 0x7f0000fe),
                SECP256K1_FE_CONST(0x28e2fdb4, 0x0709168b, 0x86f598b0, 0x3453a370, 0x530cf21f, 0x32f978d5, 0x1d527a71, 0x59269b0c)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xc2591afa, 0x7bb98ef7, 0x090bb273, 0x85c14f87, 0xbb0b28e0, 0x54d3c453, 0x85c66753, 0xd5574d2f),
                SECP256K1_FE_CONST(0xfdca70a2, 0x70ce627c, 0x95e66fae, 0x848a6dbb, 0x07ffb15c, 0x5f63a058, 0xba4140ed, 0x6113b503)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xf5475db3, 0xedc7b5a3, 0x411c047e, 0xeaeb452f, 0xc625828e, 0x1cf5ad27, 0x8eec1060, 0xc7d3e690),
                SECP256K1_FE_CONST(0x5eb756c0, 0xf963f4b9, 0xdc6a215e, 0xec8cc2d8, 0x2e9dec01, 0xde5eb88d, 0x6aba7164, 0xaecb2c5a)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x00000000, 0x00f8ffff, 0xffffffff, 0x01000000, 0xe0ff1f00, 0x00000000, 0xffffff7f, 0x00000000),
                SECP256K1_FE_CONST(0xe0d2e3d8, 0x49b6157d, 0xe54e88c2, 0x1a7f02ca, 0x7dd28167, 0xf1125d81, 0x7bfa444e, 0xbe110037)
            );
            yield return new
            (
                /* Selection of randomly generated inputs that reach high/low d/e values in various configurations. */
                SECP256K1_FE_CONST(0x13cc08a4, 0xd8c41f0f, 0x179c3e67, 0x54c46c67, 0xc4109221, 0x09ab3b13, 0xe24d9be1, 0xffffe950),
                SECP256K1_FE_CONST(0xb80c8006, 0xd16abaa7, 0xcabd71e5, 0xcf6714f4, 0x966dd3d0, 0x64767a2d, 0xe92c4441, 0x51008cd1)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xaa6db990, 0x95efbca1, 0x3cc6ff71, 0x0602e24a, 0xf49ff938, 0x99fffc16, 0x46f40993, 0xc6e72057),
                SECP256K1_FE_CONST(0xd5d3dd69, 0xb0c195e5, 0x285f1d49, 0xe639e48c, 0x9223f8a9, 0xca1d731d, 0x9ca482f9, 0xa5b93e06)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x1c680eac, 0xaeabffd8, 0x9bdc4aee, 0x1781e3de, 0xa3b08108, 0x0015f2e0, 0x94449e1b, 0x2f67a058),
                SECP256K1_FE_CONST(0x7f083f8d, 0x31254f29, 0x6510f475, 0x245c373d, 0xc5622590, 0x4b323393, 0x32ed1719, 0xc127444b)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x147d44b3, 0x012d83f8, 0xc160d386, 0x1a44a870, 0x9ba6be96, 0x8b962707, 0x267cbc1a, 0xb65b2f0a),
                SECP256K1_FE_CONST(0x555554ff, 0x170aef1e, 0x50a43002, 0xe51fbd36, 0xafadb458, 0x7a8aded1, 0x0ca6cd33, 0x6ed9087c)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x12423796, 0x22f0fe61, 0xf9ca017c, 0x5384d107, 0xa1fbf3b2, 0x3b018013, 0x916a3c37, 0x4000b98c),
                SECP256K1_FE_CONST(0x20257700, 0x08668f94, 0x1177e306, 0x136c01f5, 0x8ed1fbd2, 0x95ec4589, 0xae38edb9, 0xfd19b6d7)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xdcf2d030, 0x9ab42cb4, 0x93ffa181, 0xdcd23619, 0x39699b52, 0x08909a20, 0xb5a17695, 0x3a9dcf21),
                SECP256K1_FE_CONST(0x1f701dea, 0xe211fb1f, 0x4f37180d, 0x63a0f51c, 0x29fe1e40, 0xa40b6142, 0x2e7b12eb, 0x982b06b6)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x79a851f6, 0xa6314ed3, 0xb35a55e6, 0xca1c7d7f, 0xe32369ea, 0xf902432e, 0x375308c5, 0xdfd5b600),
                SECP256K1_FE_CONST(0xcaae00c5, 0xe6b43851, 0x9dabb737, 0x38cba42c, 0xa02c8549, 0x7895dcbf, 0xbd183d71, 0xafe4476a)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xede78fdd, 0xcfc92bf1, 0x4fec6c6c, 0xdb8d37e2, 0xfb66bc7b, 0x28701870, 0x7fa27c9a, 0x307196ec),
                SECP256K1_FE_CONST(0x68193a6c, 0x9a8b87a7, 0x2a760c64, 0x13e473f6, 0x23ae7bed, 0x1de05422, 0x88865427, 0xa3418265)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xa40b2079, 0xb8f88e89, 0xa7617997, 0x89baf5ae, 0x174df343, 0x75138eae, 0x2711595d, 0x3fc3e66c),
                SECP256K1_FE_CONST(0x9f99c6a5, 0x6d685267, 0xd4b87c37, 0x9d9c4576, 0x358c692b, 0x6bbae0ed, 0x3389c93d, 0x7fdd2655)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x7c74c6b6, 0xe98d9151, 0x72645cf1, 0x7f06e321, 0xcefee074, 0x15b2113a, 0x10a9be07, 0x08a45696),
                SECP256K1_FE_CONST(0x8c919a88, 0x898bc1e0, 0x77f26f97, 0x12e655b7, 0x9ba0ac40, 0xe15bb19e, 0x8364cc3b, 0xe227a8ee)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x109ba1ce, 0xdafa6d4a, 0xa1cec2b2, 0xeb1069f4, 0xb7a79e5b, 0xec6eb99b, 0xaec5f643, 0xee0e723e),
                SECP256K1_FE_CONST(0x93d13eb8, 0x4bb0bcf9, 0xe64f5a71, 0xdbe9f359, 0x7191401c, 0x6f057a4a, 0xa407fe1b, 0x7ecb65cc)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x3db076cd, 0xec74a5c9, 0xf61dd138, 0x90e23e06, 0xeeedd2d0, 0x74cbc4e0, 0x3dbe1e91, 0xded36a78),
                SECP256K1_FE_CONST(0x3f07f966, 0x8e2a1e09, 0x706c71df, 0x02b5e9d5, 0xcb92ddbf, 0xcdd53010, 0x16545564, 0xe660b107)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xe31c73ed, 0xb4c4b82c, 0x02ae35f7, 0x4cdec153, 0x98b522fd, 0xf7d2460c, 0x6bf7c0f8, 0x4cf67b0d),
                SECP256K1_FE_CONST(0x4b8f1faf, 0x94e8b070, 0x19af0ff6, 0xa319cd31, 0xdf0a7ffb, 0xefaba629, 0x59c50666, 0x1fe5b843)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x4c8b0e6e, 0x83392ab6, 0xc0e3e9f1, 0xbbd85497, 0x16698897, 0xf552d50d, 0x79652ddb, 0x12f99870),
                SECP256K1_FE_CONST(0x56d5101f, 0xd23b7949, 0x17dc38d6, 0xf24022ef, 0xcf18e70a, 0x5cc34424, 0x438544c3, 0x62da4bca)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xb0e040e2, 0x40cc35da, 0x7dd5c611, 0x7fccb178, 0x28888137, 0xbc930358, 0xea2cbc90, 0x775417dc),
                SECP256K1_FE_CONST(0xca37f0d4, 0x016dd7c8, 0xab3ae576, 0x96e08d69, 0x68ed9155, 0xa9b44270, 0x900ae35d, 0x7c7800cd)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x8a32ea49, 0x7fbb0bae, 0x69724a9d, 0x8e2105b2, 0xbdf69178, 0x862577ef, 0x35055590, 0x667ddaef),
                SECP256K1_FE_CONST(0xd02d7ead, 0xc5e190f0, 0x559c9d72, 0xdaef1ffc, 0x64f9f425, 0xf43645ea, 0x7341e08d, 0x11768e96)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xa3592d98, 0x9abe289d, 0x579ebea6, 0xbb0857a8, 0xe242ab73, 0x85f9a2ce, 0xb6998f0f, 0xbfffbfc6),
                SECP256K1_FE_CONST(0x093c1533, 0x32032efa, 0x6aa46070, 0x0039599e, 0x589c35f4, 0xff525430, 0x7fe3777a, 0x44b43ddc)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x647178a3, 0x229e607b, 0xcc98521a, 0xcce3fdd9, 0x1e1bc9c9, 0x97fb7c6a, 0x61b961e0, 0x99b10709),
                SECP256K1_FE_CONST(0x98217c13, 0xd51ddf78, 0x96310e77, 0xdaebd908, 0x602ca683, 0xcb46d07a, 0xa1fcf17e, 0xc8e2feb3)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x7334627c, 0x73f98968, 0x99464b4b, 0xf5964958, 0x1b95870d, 0xc658227e, 0x5e3235d8, 0xdcab5787),
                SECP256K1_FE_CONST(0x000006fd, 0xc7e9dd94, 0x40ae367a, 0xe51d495c, 0x07603b9b, 0x2d088418, 0x6cc5c74c, 0x98514307)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x82e83876, 0x96c28938, 0xa50dd1c5, 0x605c3ad1, 0xc048637d, 0x7a50825f, 0x335ed01a, 0x00005760),
                SECP256K1_FE_CONST(0xb0393f9f, 0x9f2aa55e, 0xf5607e2e, 0x5287d961, 0x60b3e704, 0xf3e16e80, 0xb4f9a3ea, 0xfec7f02d)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xc97b6cec, 0x3ee6b8dc, 0x98d24b58, 0x3c1970a1, 0xfe06297a, 0xae813529, 0xe76bb6bd, 0x771ae51d),
                SECP256K1_FE_CONST(0x0507c702, 0xd407d097, 0x47ddeb06, 0xf6625419, 0x79f48f79, 0x7bf80d0b, 0xfc34b364, 0x253a5db1)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xd559af63, 0x77ea9bc4, 0x3cf1ad14, 0x5c7a4bbb, 0x10e7d18b, 0x7ce0dfac, 0x380bb19d, 0x0bb99bd3),
                SECP256K1_FE_CONST(0x00196119, 0xb9b00d92, 0x34edfdb5, 0xbbdc42fc, 0xd2daa33a, 0x163356ca, 0xaa8754c8, 0xb0ec8b0b)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x8ddfa3dc, 0x52918da0, 0x640519dc, 0x0af8512a, 0xca2d33b2, 0xbde52514, 0xda9c0afc, 0xcb29fce4),
                SECP256K1_FE_CONST(0xb3e4878d, 0x5cb69148, 0xcd54388b, 0xc23acce0, 0x62518ba8, 0xf09def92, 0x7b31e6aa, 0x6ba35b02)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0xf8207492, 0xe3049f0a, 0x65285f2b, 0x0bfff996, 0x00ca112e, 0xc05da837, 0x546d41f9, 0x5194fb91),
                SECP256K1_FE_CONST(0x7b7ee50b, 0xa8ed4bbd, 0xf6469930, 0x81419a5c, 0x071441c7, 0x290d046e, 0x3b82ea41, 0x611c5f95)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x050f7c80, 0x5bcd3c6b, 0x823cb724, 0x5ce74db7, 0xa4e39f5c, 0xbd8828d7, 0xfd4d3e07, 0x3ec2926a),
                SECP256K1_FE_CONST(0x000d6730, 0xb0171314, 0x4764053d, 0xee157117, 0x48fd61da, 0xdea0b9db, 0x1d5e91c6, 0xbdc3f59e)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x3e3ea8eb, 0x05d760cf, 0x23009263, 0xb3cb3ac9, 0x088f6f0d, 0x3fc182a3, 0xbd57087c, 0xe67c62f9),
                SECP256K1_FE_CONST(0xbe988716, 0xa29c1bf6, 0x4456aed6, 0xab1e4720, 0x49929305, 0x51043bf4, 0xebd833dd, 0xdd511e8b)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x6964d2a9, 0xa7fa6501, 0xa5959249, 0x142f4029, 0xea0c1b5f, 0x2f487ef6, 0x301ac80a, 0x768be5cd),
                SECP256K1_FE_CONST(0x3918ffe4, 0x07492543, 0xed24d0b7, 0x3df95f8f, 0xaffd7cb4, 0x0de2191c, 0x9ec2f2ad, 0x2c0cb3c6)
            );
            yield return new
            (
                SECP256K1_FE_CONST(0x37c93520, 0xf6ddca57, 0x2b42fd5e, 0xb5c7e4de, 0x11b5b81c, 0xb95e91f3, 0x95c4d156, 0x39877ccb),
                SECP256K1_FE_CONST(0x9a94b9b5, 0x57eb71ee, 0x4c975b8b, 0xac5262a8, 0x077b0595, 0xe12a6b1f, 0xd728edef, 0x1a6bf956)
            );
        }

        /// <summary>
        /// run_inverse_tests (fixed cases)
        /// </summary>
        [Theory]
        [MemberData(nameof(GetInvCases))]
        public void Libsecp256k1_InverseTest(in UInt256_5x52 a, in UInt256_5x52 b)
        {
            // Test fixed test cases through test_inverse_{scalar,field}, both ways.
            for (int useVar = 0; useVar <= 1; useVar++)
            {
                TestInverseField(a, useVar, out UInt256_5x52 x_fe);
                Assert.True(CheckFEEqual(x_fe, b));
                TestInverseField(b, useVar, out x_fe);
                Assert.True(CheckFEEqual(x_fe, a));
            }
        }


        /// <summary>
        /// run_inverse_tests (random cases)
        /// </summary>
        [Fact]
        public void Libsecp256k1_InverseRandomTest()
        {
            UInt256_5x52 x_fe;
            // Test inputs 0..999 and their respective negations.
            Span<byte> b32 = new byte[32];
            for (int i = 0; i < 1000; i++)
            {
                b32[31] = (byte)i;
                b32[30] = (byte)(i >> 8);
                x_fe = new(b32);
                for (int var = 0; var <= 1; var++)
                {
                    TestInverseField(x_fe, var, out _);
                }
                x_fe = x_fe.Negate(1);
                for (int var = 0; var <= 1; var++)
                {
                    TestInverseField(x_fe, var, out _);
                }
            }

            TestRNG rng = new();
            rng.Init(null);
            // test 128*count random inputs; half with testrand256_test, half with testrand256 */
            for (int testrand = 0; testrand <= 1; testrand++)
            {
                for (int i = 0; i < 64 * COUNT; i++)
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
                    for (int var = 0; var <= 1; var++)
                    {
                        TestInverseField(x_fe, var, out _);
                    }
                }
            }
        }

        #endregion // libsecp256k1 tests
    }
}
