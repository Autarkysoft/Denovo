// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.ValueTypesTests
{
    public class LockTimeTests
    {
        [Fact]
        public void ConstructorTests()
        {
            LockTime zeroI = new LockTime(0);
            LockTime zeroU = new LockTime(0U);
            Helper.ComparePrivateField(zeroI, "value", 0U);
            Helper.ComparePrivateField(zeroU, "value", 0U);

            LockTime ltI = new LockTime(123);
            LockTime ltU = new LockTime(123U);
            Helper.ComparePrivateField(ltI, "value", 123U);
            Helper.ComparePrivateField(ltU, "value", 123U);

            LockTime maxI = new LockTime(int.MaxValue);
            LockTime maxU = new LockTime(uint.MaxValue);
            Helper.ComparePrivateField(maxI, "value", (uint)int.MaxValue);
            Helper.ComparePrivateField(maxU, "value", uint.MaxValue);

            int negI = -1;
            Assert.Throws<ArgumentOutOfRangeException>(() => new LockTime(negI));
        }

        [Fact]
        public void Constructor_FromDateTimeTest()
        {
            DateTime today = new DateTime(2019, 2, 24, 14, 17, 38);
            Helper.ComparePrivateField(new LockTime(today), "value", 1551017858U);

            DateTime smallestValidDt = new DateTime(1985, 11, 5, 0, 53, 20);
            Helper.ComparePrivateField(new LockTime(smallestValidDt), "value", 500000000U);

            DateTime negTwoDt = new DateTime(1969, 12, 31, 23, 59, 58); //-2
            DateTime smallDt = new DateTime(1985, 11, 5, 0, 53, 19); //499999999
            DateTime maxDt = new DateTime(2106, 2, 7, 6, 28, 16); //uint.MaxValue

            Assert.Throws<ArgumentOutOfRangeException>(() => new LockTime(negTwoDt));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LockTime(smallDt));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LockTime(maxDt));
        }

        [Fact]
        public void StaticMemberTest()
        {
            Helper.ComparePrivateField(LockTime.Minimum, "value", 0U);
            Helper.ComparePrivateField(LockTime.Maximum, "value", uint.MaxValue);
        }


        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(0, 1, true)]
        [InlineData(0, 499999999U, true)]
        [InlineData(0, 500000000U, false)]
        [InlineData(0, uint.MaxValue, false)]
        [InlineData(500000000U, 500000000U, true)]
        [InlineData(500000000U, 500000001U, true)]
        [InlineData(500000000U, uint.MaxValue, true)]
        [InlineData(uint.MaxValue, uint.MaxValue, true)]
        public void IsSameTypeTest(uint ltVal, long value, bool expected)
        {
            LockTime lt = new LockTime(ltVal);
            bool actual = lt.IsSameType(value);
            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetReadCases()
        {
            yield return new object[] { new byte[] { 0, 0, 0, 0 }, 0 };
            yield return new object[] { new byte[] { 2, 0, 0, 0 }, 2 };
            yield return new object[] { new byte[] { 64, 1, 185, 2, }, 45678912U };
            yield return new object[] { new byte[] { 255, 255, 255, 255 }, uint.MaxValue };
        }

        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryReadTest(byte[] data, uint expected)
        {
            FastStreamReader stream = new FastStreamReader(data);
            bool b = LockTime.TryRead(stream, out LockTime actual, out string error);

            Assert.True(b);
            Assert.Null(error);
            Helper.ComparePrivateField(stream, "position", 4);
            Helper.ComparePrivateField(actual, "value", expected);
        }

        [Fact]
        public void TryRead_Fail_NullStreamTest()
        {
            bool b = LockTime.TryRead(null, out LockTime actual, out string error);

            Assert.False(b);
            Assert.Equal("Stream can not be null.", error);
            Helper.ComparePrivateField(actual, "value", 0U);
        }

        [Fact]
        public void TryRead_Fail_SmallStreamTest()
        {
            FastStreamReader stream = new FastStreamReader(new byte[3] { 1, 2, 3 });
            bool b = LockTime.TryRead(stream, out LockTime actual, out string error);

            Assert.False(b);
            Assert.Equal(Err.EndOfStream, error);
            Helper.ComparePrivateField(actual, "value", 0U);
        }

        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void WriteToStreamTest(byte[] expected, uint value)
        {
            LockTime lt = new LockTime(value);
            FastStream stream = new FastStream();
            lt.WriteToStream(stream);

            Assert.Equal(expected, stream.ToByteArray());
        }


        [Fact]
        public void CastTest()
        {
            byte b = 10;
            ushort us = 10;
            uint ui = 10;
            int i = 10;
            int negi = -10;
            DateTime dt = new DateTime(2019, 2, 24, 14, 17, 38);
            DateTime smallDt = new DateTime(1985, 11, 5, 0, 53, 19); //499999999

            LockTime lt1 = b;
            LockTime lt2 = us;
            LockTime lt3 = ui;
            LockTime lt4 = (LockTime)i;
            LockTime lt5 = (LockTime)dt;
            LockTime lt6 = (LockTime)negi;
            LockTime lt7 = (LockTime)smallDt;

            Helper.ComparePrivateField(lt1, "value", 10U);
            Helper.ComparePrivateField(lt2, "value", 10U);
            Helper.ComparePrivateField(lt3, "value", 10U);
            Helper.ComparePrivateField(lt4, "value", 10U);
            Helper.ComparePrivateField(lt5, "value", 1551017858U);
            Helper.ComparePrivateField(lt5, "value", 1551017858U);
            Helper.ComparePrivateField(lt5, "value", 1551017858U);
            Helper.ComparePrivateField(lt6, "value", (uint)negi);
            Helper.ComparePrivateField(lt7, "value", uint.MaxValue);
        }

        [Fact]
        public void CastTest2()
        {
            LockTime lt1 = new LockTime(10);
            LockTime lt2 = new LockTime(uint.MaxValue);

            uint ui1 = lt1;
            uint ui2 = lt2;
            Assert.Equal(10U, ui1);
            Assert.Equal(uint.MaxValue, ui2);

            ushort us1 = (ushort)lt1;
            ushort us2 = (ushort)lt2;
            Assert.Equal((ushort)10, us1);
            Assert.Equal(unchecked((ushort)uint.MaxValue), us2);

            byte b1 = (byte)lt1;
            byte b2 = (byte)lt2;
            Assert.Equal((byte)10, b1);
            Assert.Equal(unchecked((byte)uint.MaxValue), b2);

            int i1 = (int)lt1;
            int i2 = (int)lt2;
            Assert.Equal(10, i1);
            Assert.Equal(unchecked((int)uint.MaxValue), i2);

            LockTime lt3 = new LockTime(1551017858U);
            DateTime dt = (DateTime)lt3;
            Assert.Equal(new DateTime(2019, 2, 24, 14, 17, 38), dt);
        }

        [Fact]
        public void CastDateTimeTest()
        {
            LockTime lt1 = 0;
            LockTime lt2 = uint.MaxValue;

            DateTime actual1 = (DateTime)lt1;
            DateTime actual2 = (DateTime)lt2;
            DateTime expected = new DateTime(1970, 1, 1, 0, 0, 0);

            Assert.Equal(expected, actual1);
            Assert.Equal(expected, actual2);
        }

        [Fact]
        public void ComparisonTest()
        {
            LockTime big = new LockTime(1);
            LockTime small = new LockTime(0);

            Assert.True(big > small);
            Assert.True(big >= small);
            Assert.True(small < big);
            Assert.True(small <= big);
            Assert.False(big == small);
            Assert.True(big != small);
            Assert.Equal(1, big.CompareTo(small));
            Assert.Equal(1, big.CompareTo((object)small));
            Assert.Equal(-1, small.CompareTo(big));
            Assert.Equal(-1, small.CompareTo((object)big));
            Assert.False(big.Equals(small));
            Assert.False(big.Equals((object)small));
        }

        [Fact]
        public void Comparison_EqualTest()
        {
            LockTime first = new LockTime(1);
            LockTime second = new LockTime(1);

            Assert.False(first > second);
            Assert.True(first >= second);
            Assert.False(second < first);
            Assert.True(second <= first);
            Assert.True(first == second);
            Assert.False(first != second);
            Assert.Equal(0, first.CompareTo(second));
            Assert.Equal(0, first.CompareTo((object)second));
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
        }

        [Theory]
        [InlineData(1, 2, false, false)]
        [InlineData(1, 0, true, true)]
        [InlineData(1, -1, true, true)]
        public void Comparison_WithIntTest(int c, int i, bool expected, bool expectedEq)
        {
            LockTime lt = new LockTime(c);

            Assert.Equal(expected, lt > i);
            Assert.Equal(expectedEq, lt >= i);

            Assert.Equal(!expected, i > lt);
            Assert.Equal(!expectedEq, i >= lt);

            Assert.Equal(!expected, lt < i);
            Assert.Equal(!expectedEq, lt <= i);

            Assert.Equal(expected, i < lt);
            Assert.Equal(expectedEq, i <= lt);
        }

        [Fact]
        public void Comparison_BigSmall_EqualIntTest()
        {
            LockTime lt = new LockTime(1);
            int i = 1;

            Assert.False(lt > i);
            Assert.True(lt >= i);

            Assert.False(i > lt);
            Assert.True(i >= lt);

            Assert.False(lt < i);
            Assert.True(lt <= i);

            Assert.False(i < lt);
            Assert.True(i <= lt);
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, false)]
        [InlineData(1, -1, false)]
        public void Comparison_WithInt_EqualTest(int c, int i, bool expected)
        {
            LockTime lt = new LockTime(c);

            Assert.Equal(expected, lt == i);
            Assert.Equal(!expected, lt != i);

            Assert.Equal(expected, i == lt);
            Assert.Equal(!expected, i != lt);
        }

        [Fact]
        public void CompareToTest()
        {
            LockTime lt = new LockTime(100);
            object nObj = null;
            object sObj = "LockTime!";

            Assert.Equal(1, lt.CompareTo(nObj));
            Assert.Throws<ArgumentException>(() => lt.CompareTo(sObj));
        }

        [Fact]
        public void EqualsTest()
        {
            LockTime lt = new LockTime(100);
            object sObj = "LockTime!";

            Assert.False(lt.Equals(sObj));
            Assert.False(lt.Equals(null));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            int expected = 1250U.GetHashCode();
            int actual = new LockTime(1250).GetHashCode();

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetToStringCases()
        {
            DateTime today = new DateTime(2019, 2, 24, 14, 17, 38);

            yield return new object[] { new LockTime(0), "0" };
            yield return new object[] { new LockTime(LockTime.Threshold - 1), "499999999" };
            yield return new object[] { new LockTime(LockTime.Threshold), new DateTime(1985, 11, 5, 0, 53, 20).ToString() };
            yield return new object[] { new LockTime(today), today.ToString() };
        }
        [Theory]
        [MemberData(nameof(GetToStringCases))]
        public void ToStringTest(LockTime first, string expected)
        {
            string actual = first.ToString();
            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetIncrementCases()
        {
            yield return new object[] { new LockTime(0), new LockTime(1), new LockTime(0) };
            yield return new object[] { new LockTime(1000), new LockTime(1001), new LockTime(1000) };
            yield return new object[] { new LockTime(uint.MaxValue), new LockTime(uint.MaxValue), new LockTime(uint.MaxValue) };
            yield return new object[]
            {
                new LockTime(uint.MaxValue - 1),
                new LockTime(uint.MaxValue),
                new LockTime(uint.MaxValue - 1)
            };
        }
        [Theory]
        [MemberData(nameof(GetIncrementCases))]
        public void IncrementTest(LockTime lt, LockTime expected, LockTime unChanged)
        {
            LockTime actual = lt.Increment();

            Assert.Equal(expected, actual);
            Assert.Equal(unChanged, lt);
        }

        public static IEnumerable<object[]> GetIncrementOpCases()
        {
            yield return new object[] { new LockTime(0), new LockTime(1) };
            yield return new object[] { new LockTime(1000), new LockTime(1001) };
            yield return new object[] { new LockTime(uint.MaxValue - 1), new LockTime(uint.MaxValue) };
            yield return new object[] { new LockTime(uint.MaxValue), new LockTime(uint.MaxValue) };
        }
        [Theory]
        [MemberData(nameof(GetIncrementOpCases))]
        public void Increment_OperatorTest(LockTime lt, LockTime expected)
        {
            lt++;
            Assert.Equal(expected, lt);
        }

    }
}
