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
            CompactInt zeroI = new(0);
            CompactInt zeroL = new(0L);
            Helper.ComparePrivateField(zeroI, "value", 0UL);
            Helper.ComparePrivateField(zeroL, "value", 0UL);

            CompactInt cI = new(123);
            CompactInt cL = new((long)123);
            Helper.ComparePrivateField(cI, "value", 123UL);
            Helper.ComparePrivateField(cL, "value", 123UL);

            CompactInt maxI = new(int.MaxValue);
            CompactInt maxL = new(long.MaxValue);
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
            CompactInt zeroUI = new(0U);
            CompactInt zeroUL = new(0UL);
            Helper.ComparePrivateField(zeroUI, "value", 0UL);
            Helper.ComparePrivateField(zeroUL, "value", 0UL);

            CompactInt cUI = new(123U);
            CompactInt cUL = new(123UL);
            Helper.ComparePrivateField(cUI, "value", 123UL);
            Helper.ComparePrivateField(cUL, "value", 123UL);

            CompactInt maxUI = new(uint.MaxValue);
            CompactInt maxUL = new(ulong.MaxValue);
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
            FastStreamReader stream = new(data);
            bool b = CompactInt.TryRead(stream, out CompactInt actual, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Helper.ComparePrivateField(stream, "position", finalPos);
            Helper.ComparePrivateField(actual, "value", expected);
        }


        public static IEnumerable<object[]> GetReadFailCases()
        {
            yield return new object[] { Array.Empty<byte>(), 0, Errors.EndOfStream };
            yield return new object[] { new byte[] { 253 }, 1, Errors.ShortCompactInt2 };
            yield return new object[] { new byte[] { 253, 1, 0 }, 3, Errors.SmallCompactInt2 };
            yield return new object[] { new byte[] { 253, 252, 0 }, 3, Errors.SmallCompactInt2 };
            yield return new object[] { new byte[] { 254 }, 1, Errors.ShortCompactInt4 };
            yield return new object[] { new byte[] { 254, 255, 255, 255 }, 1, Errors.ShortCompactInt4 };
            yield return new object[] { new byte[] { 254, 255, 255, 0, 0 }, 5, Errors.SmallCompactInt4 };
            yield return new object[] { new byte[] { 255 }, 1, Errors.ShortCompactInt8 };
            yield return new object[] { new byte[] { 255, 255, 255, 255, 255, 255, 255, 255 }, 1, Errors.ShortCompactInt8 };
            yield return new object[] { new byte[] { 255, 1, 0, 0, 0, 0, 0, 0, 0 }, 9, Errors.SmallCompactInt8 };
        }
        [Theory]
        [MemberData(nameof(GetReadFailCases))]
        public void TryRead_FailTest(byte[] data, int finalPos, Errors expError)
        {
            FastStreamReader stream = new(data);
            bool b = CompactInt.TryRead(stream, out CompactInt actual, out Errors error);

            Assert.False(b);
            Assert.Equal(expError, error);
            Helper.ComparePrivateField(stream, "position", finalPos);
            Helper.ComparePrivateField(actual, "value", 0UL);
        }

        [Fact]
        public void TryRead_Fail_NullStreamTest()
        {
            bool b = CompactInt.TryRead(null, out CompactInt actual, out Errors error);

            Assert.False(b);
            Assert.Equal(Errors.NullStream, error);
            Helper.ComparePrivateField(actual, "value", 0UL);
        }


        [Theory]
        [InlineData(0, 0, 1)]
        [InlineData(0, 2, 3)]
        [InlineData(252, 2, 3)]
        [InlineData(253, 3, 6)]
        [InlineData(ushort.MaxValue, 3, 6)]
        [InlineData(ushort.MaxValue + 1, 3, 8)]
        [InlineData(uint.MaxValue, 3, 8)]
        [InlineData(uint.MaxValue + 1UL, 3, 12)]
        [InlineData(ulong.MaxValue, 3, 12)]
        public void AddSerializedSizeTest(ulong val, int init, int expected)
        {
            CompactInt ci = new(val);
            SizeCounter counter = new(init);
            ci.AddSerializedSize(counter);

            FastStream stream = new(10);
            ci.WriteToStream(stream);
            Assert.Equal(expected, stream.GetSize() + init);

            Assert.Equal(expected, counter.Size);
        }


        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void WriteToStreamTest(byte[] data, int finalOffset, ulong val)
        {
            CompactInt ci = new(val);
            FastStream stream = new(10);
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
            CompactInt c1 = new(10);
            CompactInt c2 = new(ulong.MaxValue);

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


        public static IEnumerable<object[]> GetCompareSameTypeCases()
        {
            yield return new object[]
            {
                new CompactInt(0), new CompactInt(0), new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new CompactInt(1), new CompactInt(0), new ValueCompareResult(true, true, false, false, false, 1)
            };
            yield return new object[]
            {
                new CompactInt(0), new CompactInt(1), new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new CompactInt(1), new CompactInt(1), new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new CompactInt(1), new CompactInt(2), new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new CompactInt(2), new CompactInt(1), new ValueCompareResult(true, true, false, false, false, 1)
            };
            yield return new object[]
            {
                new CompactInt(ulong.MaxValue),
                new CompactInt(ulong.MaxValue),
                new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new CompactInt(ulong.MaxValue),
                new CompactInt(0),
                new ValueCompareResult(true, true, false, false, false, 1),
            };
            yield return new object[]
            {
                new CompactInt(0),
                new CompactInt(ulong.MaxValue),
                new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new CompactInt(ulong.MaxValue-1),
                new CompactInt(ulong.MaxValue),
                new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new CompactInt(ulong.MaxValue),
                new CompactInt(ulong.MaxValue-1),
                new ValueCompareResult(true, true, false, false, false, 1)
            };
        }
        [Theory]
        [MemberData(nameof(GetCompareSameTypeCases))]
        public void ComparisonOperator_SameTypeTest(CompactInt ci1, CompactInt ci2, ValueCompareResult expected)
        {
            Assert.Equal(expected.Bigger, ci1 > ci2);
            Assert.Equal(expected.BiggerEqual, ci1 >= ci2);
            Assert.Equal(expected.Smaller, ci1 < ci2);
            Assert.Equal(expected.SmallerEqual, ci1 <= ci2);

            Assert.Equal(expected.Equal, ci1 == ci2);
            Assert.Equal(!expected.Equal, ci1 != ci2);

            Assert.Equal(expected.Equal, ci1.Equals(ci2));
            Assert.Equal(expected.Equal, ci1.Equals((object)ci2));

            Assert.Equal(expected.Compare, ci1.CompareTo(ci2));
            Assert.Equal(expected.Compare, ci1.CompareTo((object)ci2));
        }

        public static IEnumerable<object[]> GetCompareIntCases()
        {
            yield return new object[] { new CompactInt(0), 0, new ValueCompareResult(false, true, false, true, true) };
            yield return new object[] { new CompactInt(0), 1, new ValueCompareResult(false, false, true, true, false) };
            yield return new object[] { new CompactInt(0), -1, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new CompactInt(0), int.MaxValue, new ValueCompareResult(false, false, true, true, false) };

            yield return new object[] { new CompactInt(1), 0, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new CompactInt(1), 1, new ValueCompareResult(false, true, false, true, true) };
            yield return new object[] { new CompactInt(1), 2, new ValueCompareResult(false, false, true, true, false) };
            yield return new object[] { new CompactInt(1), -1, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new CompactInt(1), int.MaxValue, new ValueCompareResult(false, false, true, true, false) };

            yield return new object[]
            {
                new CompactInt(ulong.MaxValue), 0, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new CompactInt(ulong.MaxValue), -1, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new CompactInt(ulong.MaxValue), 1, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new CompactInt(int.MaxValue), int.MaxValue, new ValueCompareResult(false, true, false, true, true)
            };
        }
        [Theory]
        [MemberData(nameof(GetCompareIntCases))]
        public void ComparisonOperator_WithIntTest(CompactInt ci, int i, ValueCompareResult expected)
        {
            Assert.Equal(expected.Bigger, ci > i);
            Assert.Equal(expected.Bigger, ci > (long)i);
            Assert.Equal(expected.Bigger, i < ci);
            Assert.Equal(expected.Bigger, (long)i < ci);

            Assert.Equal(expected.BiggerEqual, ci >= i);
            Assert.Equal(expected.BiggerEqual, ci >= (long)i);
            Assert.Equal(expected.BiggerEqual, i <= ci);
            Assert.Equal(expected.BiggerEqual, (long)i <= ci);

            Assert.Equal(expected.Smaller, ci < i);
            Assert.Equal(expected.Smaller, ci < (long)i);
            Assert.Equal(expected.Smaller, i > ci);
            Assert.Equal(expected.Smaller, (long)i > ci);

            Assert.Equal(expected.SmallerEqual, ci <= i);
            Assert.Equal(expected.SmallerEqual, ci <= (long)i);
            Assert.Equal(expected.SmallerEqual, i >= ci);
            Assert.Equal(expected.SmallerEqual, (long)i >= ci);

            Assert.Equal(expected.Equal, ci == i);
            Assert.Equal(expected.Equal, i == ci);
            Assert.Equal(expected.Equal, ci == (long)i);
            Assert.Equal(expected.Equal, (long)i == ci);

            Assert.Equal(!expected.Equal, ci != i);
            Assert.Equal(!expected.Equal, i != ci);
            Assert.Equal(!expected.Equal, ci != (long)i);
            Assert.Equal(!expected.Equal, (long)i != ci);
        }


        [Fact]
        public void CompareTo_EdgeTest()
        {
            CompactInt ci = new(1);
            object nObj = null;
            object sObj = "CompactInt!";

            Assert.Equal(1, ci.CompareTo(nObj));
            Assert.Throws<ArgumentException>(() => ci.CompareTo(sObj));
        }

        [Fact]
        public void Equals_EdgeTest()
        {
            CompactInt ci = new(100);
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
            CompactInt ci = new(123);
            Assert.Equal("123", ci.ToString());
        }
    }
}
