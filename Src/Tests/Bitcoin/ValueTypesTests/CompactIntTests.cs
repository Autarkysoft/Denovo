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
    public class CompactIntTests
    {
        [Fact]
        public void Constructor_FromIntTest()
        {
            CompactInt zeroI = new CompactInt(0);
            CompactInt zeroL = new CompactInt(0L);
            Helper.ComparePrivateField(zeroI, "value", 0UL);
            Helper.ComparePrivateField(zeroL, "value", 0UL);

            CompactInt cI = new CompactInt(123);
            CompactInt cL = new CompactInt((long)123);
            Helper.ComparePrivateField(cI, "value", 123UL);
            Helper.ComparePrivateField(cL, "value", 123UL);

            CompactInt maxI = new CompactInt(int.MaxValue);
            CompactInt maxL = new CompactInt(long.MaxValue);
            Helper.ComparePrivateField(maxI, "value", (ulong)int.MaxValue);
            Helper.ComparePrivateField(maxL, "value", (ulong)long.MaxValue);

            int negI = -1;
            long negL = -1;
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompactInt(negI));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompactInt(negL));
        }

        [Fact]
        public void Constructor_FromUIntTest()
        {
            CompactInt zeroUI = new CompactInt(0U);
            CompactInt zeroUL = new CompactInt(0UL);
            Helper.ComparePrivateField(zeroUI, "value", 0UL);
            Helper.ComparePrivateField(zeroUL, "value", 0UL);

            CompactInt cUI = new CompactInt(123U);
            CompactInt cUL = new CompactInt(123UL);
            Helper.ComparePrivateField(cUI, "value", 123UL);
            Helper.ComparePrivateField(cUL, "value", 123UL);

            CompactInt maxUI = new CompactInt(uint.MaxValue);
            CompactInt maxUL = new CompactInt(ulong.MaxValue);
            Helper.ComparePrivateField(maxUI, "value", (ulong)uint.MaxValue);
            Helper.ComparePrivateField(maxUL, "value", ulong.MaxValue);
        }


        public static IEnumerable<object[]> GetReadCases()
        {
            yield return new object[] { new byte[] { 0 }, 1, 0 };
            yield return new object[] { new byte[] { 252 }, 1, 252 };
            yield return new object[] { new byte[] { 253, 253, 0 }, 3, 253 };
            yield return new object[] { new byte[] { 253, 3, 2, 255, 12 }, 3, 515 };
            yield return new object[] { new byte[] { 253, 255, 255 }, 3, ushort.MaxValue };
            yield return new object[] { new byte[] { 254, 0, 0, 1, 0, 255, 255 }, 5, ushort.MaxValue + 1 };
            yield return new object[] { new byte[] { 254, 211, 222, 81, 53 }, 5, 894557907 };
            yield return new object[] { new byte[] { 254, 255, 255, 255, 255 }, 5, uint.MaxValue };
            yield return new object[] { new byte[] { 255, 0, 0, 0, 0, 1, 0, 0, 0 }, 9, uint.MaxValue + 1UL };
            yield return new object[]
            {
                new byte[] { 255, 36, 75, 226, 255, 219, 49, 188, 60, 255, 12, 255, 255, 255 },
                9,
                4376427758857898788UL
            };
            yield return new object[]
            {
                new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255 },
                9,
                ulong.MaxValue
            };
        }
        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryReadTest(byte[] data, int finalPos, ulong expected)
        {
            FastStreamReader stream = new FastStreamReader(data);
            bool b = CompactInt.TryRead(stream, out CompactInt actual, out string error);

            Assert.True(b);
            Assert.Null(error);
            Helper.ComparePrivateField(stream, "position", finalPos);
            Helper.ComparePrivateField(actual, "value", expected);
        }


        public static IEnumerable<object[]> GetReadFailCases()
        {
            yield return new object[] { new byte[] { }, 0, Err.EndOfStream };
            yield return new object[] { new byte[] { 253 }, 1, "First byte 253 needs to be followed by at least 2 byte." };
            yield return new object[]
            {
                new byte[] { 253, 1, 0 },
                3,
                $"For values less than 253, one byte format of {nameof(CompactInt)} should be used."
            };
            yield return new object[]
            {
                new byte[] { 253, 252, 0 },
                3,
                $"For values less than 253, one byte format of {nameof(CompactInt)} should be used."
            };
            yield return new object[] { new byte[] { 254 }, 1, "First byte 254 needs to be followed by at least 4 byte." };
            yield return new object[]
            {
                new byte[] { 254, 255, 255, 255 },
                1,
                "First byte 254 needs to be followed by at least 4 byte."
            };
            yield return new object[]
            {
                new byte[] { 254, 255, 255, 0, 0 },
                5,
                "For values less than 2 bytes, the [253, ushort] format should be used."
            };
            yield return new object[] { new byte[] { 255 }, 1, "First byte 255 needs to be followed by at least 8 byte." };
            yield return new object[]
            {
                new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 },
                1,
                "First byte 255 needs to be followed by at least 8 byte."
            };
            yield return new object[]
            {
                new byte[] { 255, 1, 0, 0, 0, 0, 0, 0, 0 },
                9,
                "For values less than 4 bytes, the [254, uint] format should be used."
            };
        }
        [Theory]
        [MemberData(nameof(GetReadFailCases))]
        public void TryRead_FailTest(byte[] data, int finalPos, string expError)
        {
            FastStreamReader stream = new FastStreamReader(data);
            bool b = CompactInt.TryRead(stream, out CompactInt actual, out string error);

            Assert.False(b);
            Assert.Equal(expError, error);
            Helper.ComparePrivateField(stream, "position", finalPos);
            Helper.ComparePrivateField(actual, "value", 0UL);
        }

        [Fact]
        public void TryRead_Fail_NullStreamTest()
        {
            bool b = CompactInt.TryRead(null, out CompactInt actual, out string error);

            Assert.False(b);
            Assert.Equal("Stream can not be null.", error);
            Helper.ComparePrivateField(actual, "value", 0UL);
        }



        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void WriteToStreamTest(byte[] data, int finalOffset, ulong val)
        {
            CompactInt ci = new CompactInt(val);
            FastStream stream = new FastStream(10);
            ci.WriteToStream(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[finalOffset];
            Buffer.BlockCopy(data, 0, expected, 0, finalOffset);

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Cast_FromNumberTest()
        {
            ulong ul = 0x1122334455667788U;
            uint ui = 0x11223344U;
            ushort us = 0x1122;
            byte b = 0x01;
            long l = 0x1122334455667788;
            int i = 0x11223344;
            int negi = -1;
            long negl = -1L;

            CompactInt c1 = ul;
            CompactInt c2 = ui;
            CompactInt c3 = us;
            CompactInt c4 = b;
            CompactInt c5 = (CompactInt)l;
            CompactInt c6 = (CompactInt)i;
            CompactInt c7 = (CompactInt)negi;
            CompactInt c8 = (CompactInt)negl;

            Helper.ComparePrivateField(c1, "value", ul);
            Helper.ComparePrivateField(c2, "value", (ulong)ui);
            Helper.ComparePrivateField(c3, "value", (ulong)us);
            Helper.ComparePrivateField(c4, "value", (ulong)b);
            Helper.ComparePrivateField(c5, "value", (ulong)l);
            Helper.ComparePrivateField(c6, "value", (ulong)i);
            Helper.ComparePrivateField(c7, "value", (ulong)negi);
            Helper.ComparePrivateField(c8, "value", (ulong)negl);
        }

        [Fact]
        public void Cast_ToNumberTest()
        {
            CompactInt c1 = new CompactInt(10);
            CompactInt c2 = new CompactInt(ulong.MaxValue);

            ulong ul1 = c1;
            ulong ul2 = c2;
            uint ui1 = (uint)c1;
            uint ui2 = (uint)c2;
            ushort us1 = (ushort)c1;
            ushort us2 = (ushort)c2;
            byte b1 = (byte)c1;
            byte b2 = (byte)c2;
            long l1 = (long)c1;
            long l2 = (long)c2;
            int i1 = (int)c1;
            int i2 = (int)c2;

            Assert.Equal(10U, ul1);
            Assert.Equal(ulong.MaxValue, ul2);
            Assert.Equal((uint)10, ui1);
            Assert.Equal(unchecked((uint)ulong.MaxValue), ui2);
            Assert.Equal((ushort)10, us1);
            Assert.Equal(unchecked((ushort)ulong.MaxValue), us2);
            Assert.Equal((byte)10, b1);
            Assert.Equal(unchecked((byte)ulong.MaxValue), b2);
            Assert.Equal(10L, l1);
            Assert.Equal(unchecked((long)ulong.MaxValue), l2);
            Assert.Equal(10, i1);
            Assert.Equal(unchecked((int)ulong.MaxValue), i2);
        }


        [Fact]
        public void ComparisonTest()
        {
            CompactInt big = new CompactInt(1);
            CompactInt small = new CompactInt(0);

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
            CompactInt first = new CompactInt(1);
            CompactInt second = new CompactInt(1);

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
            CompactInt ci = new CompactInt(c);

            Assert.Equal(expected, ci > i);
            Assert.Equal(expectedEq, ci >= i);
            Assert.Equal(expected, ci > (long)i);
            Assert.Equal(expectedEq, ci >= (long)i);

            Assert.Equal(!expected, i > ci);
            Assert.Equal(!expectedEq, i >= ci);
            Assert.Equal(!expected, (long)i > ci);
            Assert.Equal(!expectedEq, (long)i >= ci);

            Assert.Equal(!expected, ci < i);
            Assert.Equal(!expectedEq, ci <= i);
            Assert.Equal(!expected, ci < (long)i);
            Assert.Equal(!expectedEq, ci <= (long)i);

            Assert.Equal(expected, i < ci);
            Assert.Equal(expectedEq, i <= ci);
            Assert.Equal(expected, (long)i < ci);
            Assert.Equal(expectedEq, (long)i <= ci);
        }

        [Fact]
        public void Comparison_BigSmall_EqualIntTest()
        {
            CompactInt ci = new CompactInt(1);
            int i = 1;

            Assert.False(ci > i);
            Assert.True(ci >= i);
            Assert.False(ci > (long)i);
            Assert.True(ci >= (long)i);

            Assert.False(i > ci);
            Assert.True(i >= ci);
            Assert.False((long)i > ci);
            Assert.True((long)i >= ci);

            Assert.False(ci < i);
            Assert.True(ci <= i);
            Assert.False(ci < (long)i);
            Assert.True(ci <= (long)i);

            Assert.False(i < ci);
            Assert.True(i <= ci);
            Assert.False((long)i < ci);
            Assert.True((long)i <= ci);
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, false)]
        [InlineData(1, -1, false)]
        public void Comparison_WithInt_EqualTest(int c, int i, bool expected)
        {
            CompactInt ci = new CompactInt(c);

            Assert.Equal(expected, ci == i);
            Assert.Equal(expected, ci == (long)i);
            Assert.Equal(!expected, ci != i);
            Assert.Equal(!expected, ci != (long)i);

            Assert.Equal(expected, i == ci);
            Assert.Equal(expected, (long)i == ci);
            Assert.Equal(!expected, i != ci);
            Assert.Equal(!expected, (long)i != ci);
        }

        [Fact]
        public void CompareToTest()
        {
            CompactInt ci = new CompactInt(1);
            object nObj = null;
            object sObj = "CompactInt!";

            Assert.Equal(1, ci.CompareTo(nObj));
            Assert.Throws<ArgumentException>(() => ci.CompareTo(sObj));
        }

        [Fact]
        public void Equals_ObjectTest()
        {
            CompactInt ci = new CompactInt(100);
            object sObj = "CompactInt!";
            object nl = null;

            Assert.False(ci.Equals(sObj));
            Assert.False(ci.Equals(nl));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            int expected = 1250UL.GetHashCode();
            int actual = new CompactInt(1250).GetHashCode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToStringTest()
        {
            CompactInt ci = new CompactInt(123);
            Assert.Equal("123", ci.ToString());
        }

    }
}
