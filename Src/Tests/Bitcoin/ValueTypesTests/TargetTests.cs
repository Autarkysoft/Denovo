// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Xunit;

namespace Tests.Bitcoin.ValueTypesTests
{
    public class TargetTests
    {
        private const uint Example = 0x1d00ffffU;
        private const uint NotNegative = 0x00800000U;

        [Fact]
        public void Constructor_FromIntTest()
        {
            var ti1 = new Target((int)Example);
            Helper.ComparePrivateField(ti1, "value", Example);

            var ti2 = new Target(0);
            Helper.ComparePrivateField(ti2, "value", 0U);
        }

        [Theory]
        [InlineData(-1, true)]
        [InlineData(0x04800001, true)]
        [InlineData(0x23000001, false)]
        [InlineData(0x22000100, false)]
        [InlineData(0x21010000, false)]
        public void Constructor_FromInt_ExceptionTest(int val, bool isNeg)
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Target(val));
            if (isNeg)
            {
                Assert.Contains("Target value can not be negative.", ex.Message);
            }
            else
            {
                Assert.Contains("Target is defined as a 256-bit number (value overflow).", ex.Message);
            }
        }

        [Fact]
        public void Constructor_FromUIntTest()
        {
            var tu1 = new Target(Example);
            Helper.ComparePrivateField(tu1, "value", Example);

            var tu2 = new Target(0U);
            Helper.ComparePrivateField(tu2, "value", 0U);
        }

        [Theory]
        [InlineData(0x04800001U, true)]
        [InlineData(0x23000001U, false)]
        [InlineData(0x22000100U, false)]
        [InlineData(0x21010000U, false)]
        public void Constructor_FromUInt_ExceptionTest(uint val, bool isNeg)
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Target(val));
            if (isNeg)
            {
                Assert.Contains("Target value can not be negative.", ex.Message);
            }
            else
            {
                Assert.Contains("Target is defined as a 256-bit number (value overflow).", ex.Message);
            }
        }

        [Theory]
        [InlineData(0x1d00ffff, "00000000FFFF0000000000000000000000000000000000000000000000000000")]
        [InlineData(0x1b0404cb, "00000000000404CB000000000000000000000000000000000000000000000000")]
        [InlineData(0x00000000, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x01120000, "0000000000000000000000000000000000000000000000000000000000000012")]
        [InlineData(0x02123400, "0000000000000000000000000000000000000000000000000000000000001234")]
        [InlineData(0x03123456, "0000000000000000000000000000000000000000000000000000000000123456")]
        [InlineData(0x04123456, "0000000000000000000000000000000000000000000000000000000012345600")]
        [InlineData(0x05009234, "0000000000000000000000000000000000000000000000000000000092340000")]
        [InlineData(0x20123456, "1234560000000000000000000000000000000000000000000000000000000000")]
        public void Constructor_FromBigIntTest(uint expected, string hex)
        {
            BigInteger big = BigInteger.Parse(hex, NumberStyles.HexNumber);
            var tar = new Target(big);
            Helper.ComparePrivateField(tar, "value", expected);
        }

        [Fact]
        public void Constructor_FromBigInt_ExceptionTest()
        {
            BigInteger big = BigInteger.Pow(2, 256);
            Assert.Throws<ArgumentOutOfRangeException>(() => new Target(big));
        }

        public static IEnumerable<object[]> GetReadCases()
        {
            yield return new object[] { new byte[] { 0xff, 0xff, 0x00, 0x1d }, 0x1d00ffffU };
            yield return new object[] { new byte[] { 0xff, 0xff, 0x00, 0x1d, 0x00, 0xff }, 0x1d00ffffU };
            yield return new object[] { new byte[] { 0xcb, 0x04, 0x04, 0x1b }, 0x1b0404cbU };
            yield return new object[] { new byte[] { 0x9b, 0x0d, 0x1f, 0x17 }, 387911067U }; // 0x171f0d9b from block #586540

            yield return new object[] { new byte[] { 0x00, 0x00, 0x80, 0x00 }, 0x00800000U };

            yield return new object[] { new byte[] { 0, 0, 0, 0 }, 0 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x12, 0x00 }, 0x00123456 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x00, 0x01 }, 0x01003456 };
            yield return new object[] { new byte[] { 0x56, 0x00, 0x00, 0x02 }, 0x02000056 };
            yield return new object[] { new byte[] { 0x00, 0x00, 0x00, 0x03 }, 0x03000000 };
            yield return new object[] { new byte[] { 0x00, 0x00, 0x00, 0x04 }, 0x04000000 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x92, 0x00 }, 0x00923456 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x80, 0x01 }, 0x01803456 };
            yield return new object[] { new byte[] { 0x56, 0x00, 0x80, 0x02 }, 0x02800056 };
            yield return new object[] { new byte[] { 0x00, 0x00, 0x80, 0x03 }, 0x03800000 };
            yield return new object[] { new byte[] { 0x00, 0x00, 0x80, 0x04 }, 0x04800000 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x12, 0x01 }, 0x01123456 };
            yield return new object[] { new byte[] { 0x00, 0x80, 0x00, 0x02 }, 0x02008000 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x12, 0x02 }, 0x02123456 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x12, 0x03 }, 0x03123456 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x12, 0x04 }, 0x04123456 };
            yield return new object[] { new byte[] { 0x34, 0x92, 0x00, 0x05 }, 0x05009234 };
            yield return new object[] { new byte[] { 0x56, 0x34, 0x12, 0x20 }, 0x20123456 };
        }
        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryReadTest(byte[] data, uint expected)
        {
            var stream = new FastStreamReader(data);
            bool b = Target.TryRead(stream, out Target actual, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Helper.ComparePrivateField(stream, "position", 4);
            Helper.ComparePrivateField(actual, "value", expected);
        }

        public static IEnumerable<object[]> GetReadFailCases()
        {
            yield return new object[] { Array.Empty<byte>(), 0, Err.EndOfStream };
            yield return new object[] { new byte[] { 0xcb }, 0, Err.EndOfStream };
            yield return new object[]
            {
                new byte[] { 0x01, 0x00, 0x80, 0x04 },
                4,
                "Target can not be negative."
            };
            yield return new object[]
            {
                new byte[] { 0x01, 0x00, 0x80, 0x03 },
                4,
                "Target can not be negative."
            };
            yield return new object[]
            {
                new byte[] { 0x00, 0x01, 0x80, 0x02 },
                4,
                "Target can not be negative."
            };

            yield return new object[]
            {
                // 0x01fedcba
                new byte[] { 0xba, 0xdc, 0xfe, 0x01 },
                4,
                "Target can not be negative."
            };
            yield return new object[]
            {
                // 0x04923456
                new byte[] { 0x56, 0x34, 0x92, 0x04 },
                4,
                "Target can not be negative."
            };
            yield return new object[]
            {
                // 0xff123456
                new byte[] { 0x56, 0x34, 0x12, 0xff },
                4,
                "Target is defined as a 256-bit number (value overflow)."
            };
        }

        [Theory]
        [MemberData(nameof(GetReadFailCases))]
        public void TryRead_FailTest(byte[] data, int finalPos, string expError)
        {
            var stream = new FastStreamReader(data);
            bool b = Target.TryRead(stream, out Target actual, out string error);

            Assert.False(b);
            Assert.Equal(expError, error);
            Helper.ComparePrivateField(stream, "position", finalPos);
            Helper.ComparePrivateField(actual, "value", 0U);
        }

        [Fact]
        public void TryRead_Fail_NullStreamTest()
        {
            bool b = Target.TryRead(null, out Target actual, out string error);

            Assert.False(b);
            Assert.Equal("Stream can not be null.", error);
            Helper.ComparePrivateField(actual, "value", 0U);
        }


        [Fact]
        public void WriteToStreamTest()
        {
            var stream = new FastStream(4);
            var tar = new Target(0x171f0d9b);
            tar.WriteToStream(stream);

            Assert.Equal(new byte[] { 0x9b, 0x0d, 0x1f, 0x17 }, stream.ToByteArray());
        }

        [Theory]
        [InlineData(0x1d00ffff, "00000000FFFF0000000000000000000000000000000000000000000000000000")]
        [InlineData(0x1b0404cb, "00000000000404CB000000000000000000000000000000000000000000000000")]
        [InlineData(0x00000000, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x00123456, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x01003456, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x02000056, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x03000000, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x04000000, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x00923456, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x01803456, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x02800056, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x03800000, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x04800000, "0000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData(0x01123456, "0000000000000000000000000000000000000000000000000000000000000012")]
        [InlineData(0x02123456, "0000000000000000000000000000000000000000000000000000000000001234")]
        [InlineData(0x03123456, "0000000000000000000000000000000000000000000000000000000000123456")]
        [InlineData(0x04123456, "0000000000000000000000000000000000000000000000000000000012345600")]
        [InlineData(0x05009234, "0000000000000000000000000000000000000000000000000000000092340000")]
        [InlineData(0x20123456, "1234560000000000000000000000000000000000000000000000000000000000")]
        public void ToBigIntTest(uint val, string hex)
        {
            var tar = new Target(val);

            BigInteger actual = tar.ToBigInt();
            BigInteger expected = BigInteger.Parse(hex, NumberStyles.HexNumber);

            Assert.Equal(expected, actual);
        }


        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void ToByteArrayTest(byte[] data, uint val)
        {
            var tar = new Target(val);

            byte[] actual = tar.ToByteArray();
            byte[] expected = new byte[4];
            Buffer.BlockCopy(data, 0, expected, 0, 4);

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetToUintCases()
        {
            yield return new object[] { 0, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x00123456, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x01003456, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x02000056, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x03000000, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x04000000, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x00923456, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x01803456, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x02800056, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x03800000, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x04800000, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x01123456, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0x00000012 } };
            yield return new object[] { 0x02123456, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0x00001234 } };
            yield return new object[] { 0x03123456, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0x00123456 } };
            yield return new object[] { 0x04123456, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0x12345600 } };
            yield return new object[] { 0x05009234, new uint[] { 0, 0, 0, 0, 0, 0, 0, 0x92340000 } };
            yield return new object[] { 0x20123456, new uint[] { 0x12345600, 0, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x1d00ffff, new uint[] { 0, 0xffff0000, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x1b0404cbU, new uint[] { 0, 0x000404cb, 0, 0, 0, 0, 0, 0 } };
            yield return new object[] { 0x090404cbU, new uint[] { 0, 0, 0, 0, 0, 0x000004, 0x04cb0000, 0 } };
        }
        [Theory]
        [MemberData(nameof(GetToUintCases))]
        public void ToUInt32ArrayTest(uint val, uint[] expected)
        {
            var tar = new Target(val);
            uint[] actual = tar.ToUInt32Array();
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void ToDifficultyTest()
        {
            var tar = new Target(0x1b0404cbU);
            Target max = 0x1d00ffff;

            BigInteger actual = tar.ToDifficulty(max);
            BigInteger expected = 16307;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToDifficulty_ZeroTest()
        {
            var tar = new Target(0);
            Target max = 0x1d00ffff;

            BigInteger actual = tar.ToDifficulty(max);

            Assert.Equal(BigInteger.Zero, actual);
        }

        [Fact]
        public void ToHashrateTest()
        {
            var tar = new Target(0x1b0404cbU);
            Target max = 0x1d00ffff;

            BigInteger actual = tar.ToHashrate(max);
            BigInteger expected = 116730052826;

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Cast_FromNumberTest()
        {
            uint ui = 0x11223344U;
            ushort us = 0x1122;
            byte b = 0x01;
            int i = 0x11223344;

            Target c1 = ui;
            Target c2 = us;
            Target c3 = b;
            Target c4 = (Target)i;

            Helper.ComparePrivateField(c1, "value", (uint)ui);
            Helper.ComparePrivateField(c2, "value", (uint)us);
            Helper.ComparePrivateField(c3, "value", (uint)b);
            Helper.ComparePrivateField(c4, "value", (uint)i);
        }

        [Fact]
        public void Cast_ToNumberTest()
        {
            var tar1 = new Target(10);
            var tar2 = new Target(Example);

            uint ui1 = tar1;
            uint ui2 = tar2;
            ushort us1 = (ushort)tar1;
            ushort us2 = (ushort)tar2;
            byte b1 = (byte)tar1;
            byte b2 = (byte)tar2;
            int i1 = (int)tar1;
            int i2 = (int)tar2;

            Assert.Equal((uint)10, ui1);
            Assert.Equal(Example, ui2);
            Assert.Equal((ushort)10, us1);
            Assert.Equal(unchecked((ushort)Example), us2);
            Assert.Equal((byte)10, b1);
            Assert.Equal(unchecked((byte)Example), b2);
            Assert.Equal(10, i1);
            Assert.Equal(unchecked((int)Example), i2);
        }


        public static IEnumerable<object[]> GetCompareSameTypeCases()
        {
            yield return new object[]
            {
                new Target(0), new Target(0), new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new Target(1), new Target(0), new ValueCompareResult(true, true, false, false, false, 1)
            };
            yield return new object[]
            {
                new Target(0), new Target(1), new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new Target(1), new Target(1), new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new Target(1), new Target(2), new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new Target(2), new Target(1), new ValueCompareResult(true, true, false, false, false, 1)
            };
            yield return new object[]
            {
                new Target(Example),
                new Target(Example),
                new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new Target(Example),
                new Target(0),
                new ValueCompareResult(true, true, false, false, false, 1),
            };
            yield return new object[]
            {
                new Target(0),
                new Target(Example),
                new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new Target(Example-1),
                new Target(Example),
                new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new Target(Example),
                new Target(Example-1),
                new ValueCompareResult(true, true, false, false, false, 1)
            };
        }
        [Theory]
        [MemberData(nameof(GetCompareSameTypeCases))]
        public void ComparisonOperator_SameTypeTest(Target si1, Target si2, ValueCompareResult expected)
        {
            Assert.Equal(expected.Bigger, si1 > si2);
            Assert.Equal(expected.BiggerEqual, si1 >= si2);
            Assert.Equal(expected.Smaller, si1 < si2);
            Assert.Equal(expected.SmallerEqual, si1 <= si2);

            Assert.Equal(expected.Equal, si1 == si2);
            Assert.Equal(!expected.Equal, si1 != si2);

            Assert.Equal(expected.Equal, si1.Equals(si2));
            Assert.Equal(expected.Equal, si1.Equals((object)si2));

            Assert.Equal(expected.Compare, si1.CompareTo(si2));
            Assert.Equal(expected.Compare, si1.CompareTo((object)si2));
        }

        public static IEnumerable<object[]> GetCompareIntCases()
        {
            yield return new object[] { new Target(0), 0, new ValueCompareResult(false, true, false, true, true) };
            yield return new object[] { new Target(0), 1, new ValueCompareResult(false, false, true, true, false) };
            yield return new object[] { new Target(0), -1, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new Target(0), int.MaxValue, new ValueCompareResult(false, false, true, true, false) };

            yield return new object[] { new Target(1), 0, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new Target(1), 1, new ValueCompareResult(false, true, false, true, true) };
            yield return new object[] { new Target(1), 2, new ValueCompareResult(false, false, true, true, false) };
            yield return new object[] { new Target(1), -1, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new Target(1), int.MaxValue, new ValueCompareResult(false, false, true, true, false) };

            yield return new object[]
            {
                new Target(Example), 0, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new Target(Example), -1, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new Target(Example), 1, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new Target(Example), Example, new ValueCompareResult(false, true, false, true, true)
            };
        }
        [Theory]
        [MemberData(nameof(GetCompareIntCases))]
        public void ComparisonOperator_WithIntTest(Target si, int i, ValueCompareResult expected)
        {
            Assert.Equal(expected.Bigger, si > i);
            Assert.Equal(expected.Bigger, i < si);

            Assert.Equal(expected.BiggerEqual, si >= i);
            Assert.Equal(expected.BiggerEqual, i <= si);

            Assert.Equal(expected.Smaller, si < i);
            Assert.Equal(expected.Smaller, i > si);

            Assert.Equal(expected.SmallerEqual, si <= i);
            Assert.Equal(expected.SmallerEqual, i >= si);

            Assert.Equal(expected.Equal, si == i);
            Assert.Equal(expected.Equal, i == si);

            Assert.Equal(!expected.Equal, si != i);
            Assert.Equal(!expected.Equal, i != si);
        }


        [Fact]
        public void CompareTo_EdgeTest()
        {
            var tar = new Target(0x1b0404cbU);
            object nObj = null;
            object sObj = "Target!";

            Assert.Equal(1, tar.CompareTo(nObj));
            Assert.Throws<ArgumentException>(() => tar.CompareTo(sObj));
        }

        [Fact]
        public void Equals_EdgeTest()
        {
            var tar = new Target(0x1b0404cbU);
            object sObj = "Target!";
            Assert.False(tar.Equals(sObj));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            int expected = 0x1b0404cb;
            int actual = new Target(0x1b0404cbU).GetHashCode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToStringTest()
        {
            var tar = new Target(0x1b0404cbU);
            Assert.Equal("453248203", tar.ToString());
        }
    }
}
