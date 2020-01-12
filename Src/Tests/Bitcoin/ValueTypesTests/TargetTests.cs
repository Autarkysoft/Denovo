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

        [Fact]
        public void ConstructorTests()
        {
            Target ti = new Target(Example);
            Target tu = new Target((int)Example);
            Helper.ComparePrivateField(ti, "value", Example);
            Helper.ComparePrivateField(tu, "value", Example);

            int negI = -1;
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Target(negI));
            Assert.Contains("Target value can not be negative.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Target(0x2100ffff));
            Assert.Contains("Target is only defined for 256 bit numbers, so the first byte can not be bigger than 32.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Target(0));
            Assert.Contains("First byte of target can not be smaller than 3.", ex.Message);
        }


        public static IEnumerable<object[]> GetReadCases()
        {
            yield return new object[] { new byte[] { 0xff, 0xff, 0x00, 0x1d }, 0x1d00ffffU };
            yield return new object[] { new byte[] { 0xff, 0xff, 0x00, 0x1d, 0x00, 0xff }, 0x1d00ffffU };
            yield return new object[] { new byte[] { 0xcb, 0x04, 0x04, 0x1b }, 0x1b0404cbU };
            yield return new object[] { new byte[] { 0x9b, 0x0d, 0x1f, 0x17 }, 387911067U }; // 0x171f0d9b from block #586540
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
                new byte[] { 0xcb, 0x04, 0x04, 0x21 },
                4,
                "Target is only defined for 256 bit numbers, so the first byte can not be bigger than 32."
            };
            yield return new object[]
            {
                new byte[] { 0xcb, 0x04, 0x04, 0x02 },
                4,
                "Target's first byte can not be smaller than 3."
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
            Helper.ComparePrivateField(actual, "value", 0x03_00_00_00U);
        }

        [Fact]
        public void TryRead_Fail_NullStreamTest()
        {
            bool b = Target.TryRead(null, out Target actual, out string error);

            Assert.False(b);
            Assert.Equal("Stream can not be null.", error);
            Helper.ComparePrivateField(actual, "value", 0x03_00_00_00U);
        }


        [Fact]
        public void ToBigIntTest()
        {
            Target tar = new Target(0x1d00ffffU);

            BigInteger actual = tar.ToBigInt();
            BigInteger expected = BigInteger.Parse("00000000FFFF0000000000000000000000000000000000000000000000000000",
                                                   NumberStyles.HexNumber);

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

        [Fact]
        public void ToUInt32ArrayTest1()
        {
            Target tar = new Target(0x1b0404cbU);

            uint[] actual = tar.ToUInt32Array();
            uint[] expected = { 0, 0x000404cb, 0, 0, 0, 0, 0, 0 };

            Assert.Equal(expected, actual);
        }
        [Fact]
        public void ToUInt32ArrayTest2()
        {
            Target tar = new Target(0x090404cbU);

            uint[] actual = tar.ToUInt32Array();
            uint[] expected = { 0, 0, 0, 0, 0, 0x000004, 0x04cb0000, 0 };

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





        [Fact]
        public void CompareToTest()
        {
            Target tar = new Target(0x1b0404cbU);
            object nObj = null;
            object sObj = "Target!";

            Assert.Equal(1, tar.CompareTo(nObj));
            Assert.Throws<ArgumentException>(() => tar.CompareTo(sObj));
        }

        [Fact]
        public void EqualsTest()
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
