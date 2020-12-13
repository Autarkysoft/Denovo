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
        public void ConstructorTest()
        {
            Target ti1 = new Target((int)Example);
            Target tu1 = new Target(Example);
            Helper.ComparePrivateField(ti1, "value", Example);
            Helper.ComparePrivateField(tu1, "value", Example);

            Target ti2 = new Target(0);
            Target tu2 = new Target(0U);
            Helper.ComparePrivateField(ti2, "value", 0U);
            Helper.ComparePrivateField(tu2, "value", 0U);

            int negI = -1;
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Target(negI));
            Assert.Contains("Target value can not be negative.", ex.Message);
        }

        [Theory]
        [InlineData(0x04800001U, true)]
        [InlineData(0x23000001U, false)]
        [InlineData(0x22000100U, false)]
        [InlineData(0x21010000U, false)]
        public void Constructor_ExceptionTest(uint val, bool isNeg)
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
            Target tar = new Target(big);
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
            FastStreamReader stream = new FastStreamReader(data);
            bool b = Target.TryRead(stream, out Target actual, out string error);

            Assert.True(b);
            Assert.Null(error);
            Helper.ComparePrivateField(stream, "position", 4);
            Helper.ComparePrivateField(actual, "value", expected);
        }

        public static IEnumerable<object[]> GetReadFailCases()
        {
            yield return new object[] { new byte[] { }, 0, Err.EndOfStream };
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
            FastStreamReader stream = new FastStreamReader(data);
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
            Target tar = new Target(val);

            BigInteger actual = tar.ToBigInt();
            BigInteger expected = BigInteger.Parse(hex, NumberStyles.HexNumber);

            Assert.Equal(expected, actual);
        }


        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void ToByteArrayTest(byte[] data, uint val)
        {
            Target tar = new Target(val);

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
            Target tar = new Target(val);
            uint[] actual = tar.ToUInt32Array();
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void ToDifficultyTest()
        {
            Target tar = new Target(0x1b0404cbU);
            Target max = 0x1d00ffff;

            BigInteger actual = tar.ToDifficulty(max);
            BigInteger expected = 16307;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToHashrateTest()
        {
            Target tar = new Target(0x1b0404cbU);
            Target max = 0x1d00ffff;

            BigInteger actual = tar.ToHashrate(max);
            BigInteger expected = 116730052826;

            Assert.Equal(expected, actual);
        }


        // TODO: add comparison tests. Target may need some significant changes before


        [Fact]
        public void CompareTo_EdgeTest()
        {
            Target tar = new Target(0x1b0404cbU);
            object nObj = null;
            object sObj = "Target!";

            Assert.Equal(1, tar.CompareTo(nObj));
            Assert.Throws<ArgumentException>(() => tar.CompareTo(sObj));
        }

        [Fact]
        public void Equals_EdgeTest()
        {
            Target tar = new Target(0x1b0404cbU);
            object sObj = "Target!";
            Assert.False(tar.Equals(sObj));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            int expected = 0x1b0404cbU.GetHashCode();
            int actual = new Target(0x1b0404cbU).GetHashCode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToStringTest()
        {
            Target tar = new Target(0x1b0404cbU);
            Assert.Equal("453248203", tar.ToString());
        }
    }
}
