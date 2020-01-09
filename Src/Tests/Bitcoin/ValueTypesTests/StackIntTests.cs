// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.ValueTypesTests
{
    public class StackIntTests
    {
        [Fact]
        public void ConstructorTests()
        {
            StackInt zeroI = new StackInt(0);
            StackInt zeroU = new StackInt(0U);
            Helper.ComparePrivateField(zeroI, "value", 0U);
            Helper.ComparePrivateField(zeroU, "value", 0U);

            StackInt cI = new StackInt(123);
            StackInt cU = new StackInt(123U);
            Helper.ComparePrivateField(cI, "value", 123U);
            Helper.ComparePrivateField(cU, "value", 123U);

            StackInt maxI = new StackInt(int.MaxValue);
            StackInt maxU = new StackInt(uint.MaxValue);
            Helper.ComparePrivateField(maxI, "value", (uint)int.MaxValue);
            Helper.ComparePrivateField(maxU, "value", uint.MaxValue);

            int negI = -1;
            Assert.Throws<ArgumentOutOfRangeException>(() => new StackInt(negI));
        }


        [Theory]
        [InlineData(0, OP._0)]
        [InlineData(1, (OP)1)]
        [InlineData(2, (OP)2)]
        [InlineData(0x4b, (OP)0x4b)]
        [InlineData(0x4c, OP.PushData1)]
        [InlineData(0x4d, OP.PushData1)]
        [InlineData(256, OP.PushData2)]
        [InlineData(ushort.MaxValue - 1, OP.PushData2)]
        [InlineData(ushort.MaxValue, OP.PushData2)]
        [InlineData(ushort.MaxValue + 1, OP.PushData4)]
        [InlineData(uint.MaxValue, OP.PushData4)]
        public void GetOpCodeTest(uint val, OP expected)
        {
            StackInt si = new StackInt(val);
            OP actual = si.GetOpCode();

            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetReadCases()
        {
            yield return new object[] { new byte[] { 0 }, 1, 0 };
            yield return new object[] { new byte[] { 5, 1, 2 }, 1, 5 };
            yield return new object[] { new byte[] { 75 }, 1, 75 };
            yield return new object[] { new byte[] { 76, 76 }, 2, 76 };
            yield return new object[] { new byte[] { 76, 255, 12 }, 2, 255 };
            yield return new object[] { new byte[] { 77, 0, 1 }, 3, 256 };
            yield return new object[] { new byte[] { 77, 171, 205 }, 3, 52651 };
            yield return new object[] { new byte[] { 77, 255, 255 }, 3, ushort.MaxValue };
            yield return new object[] { new byte[] { 78, 0, 0, 1, 0 }, 5, ushort.MaxValue + 1 };
            yield return new object[] { new byte[] { 78, 2, 1, 15, 15 }, 5, 252641538 };
            yield return new object[] { new byte[] { 78, 255, 255, 255, 255 }, 5, uint.MaxValue };
        }
        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryReadTest(byte[] data, int finalPos, uint expected)
        {
            FastStreamReader stream = new FastStreamReader(data);
            bool b = StackInt.TryRead(stream, out StackInt actual, out string error);

            Assert.True(b);
            Assert.Null(error);
            Helper.ComparePrivateField(stream, "position", finalPos);
            Helper.ComparePrivateField(actual, "value", expected);
        }


        public static IEnumerable<object[]> GetReadFailCases()
        {
            yield return new object[] { new byte[] { }, 0, Err.EndOfStream };
            yield return new object[] { new byte[] { 76 }, 1, "OP_PushData1 needs to be followed by at least one byte." };
            yield return new object[] { new byte[] { 76, 0 }, 2, "For OP_PushData1 the data value must be bigger than 75." };
            yield return new object[] { new byte[] { 76, 75 }, 2, "For OP_PushData1 the data value must be bigger than 75." };
            yield return new object[] { new byte[] { 77 }, 1, "OP_PushData2 needs to be followed by at least two byte." };
            yield return new object[] { new byte[] { 77, 5 }, 1, "OP_PushData2 needs to be followed by at least two byte." };
            yield return new object[] { new byte[] { 77, 0, 0 }, 3, "For OP_PushData2 the data value must be bigger than 255." };
            yield return new object[] { new byte[] { 78 }, 1, "OP_PushData4 needs to be followed by at least 4 byte." };
            yield return new object[]
            {
                new byte[] { 78, 12, 255, 78 },
                1,
                "OP_PushData4 needs to be followed by at least 4 byte."
            };
            yield return new object[]
            {
                new byte[] { 78, 0, 0, 0, 0 },
                5,
                $"For OP_PushData4 the data value must be bigger than {ushort.MaxValue}."
            };
            yield return new object[]
            {
                new byte[] { 78, 255, 255, 0, 0 },
                5,
                $"For OP_PushData4 the data value must be bigger than {ushort.MaxValue}."
            };
            yield return new object[] { new byte[] { 79, 255, 255 }, 1, "Unknown OP_Push value." };
            yield return new object[] { new byte[] { 255, 255, 255 }, 1, "Unknown OP_Push value." };
        }
        [Theory]
        [MemberData(nameof(GetReadFailCases))]
        public void TryRead_FailTest(byte[] data, int finalPos, string expError)
        {
            FastStreamReader stream = new FastStreamReader(data);
            bool b = StackInt.TryRead(stream, out StackInt actual, out string error);

            Assert.False(b);
            Assert.Equal(expError, error);
            Helper.ComparePrivateField(stream, "position", finalPos);
            Helper.ComparePrivateField(actual, "value", 0U);
        }

        [Fact]
        public void TryRead_Fail_NullStreamTest()
        {
            bool b = StackInt.TryRead(null, out StackInt actual, out string error);

            Assert.False(b);
            Assert.Equal("Stream can not be null.", error);
            Helper.ComparePrivateField(actual, "value", 0U);
        }


        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void ToByteArrayTest(byte[] data, int finalOffset, uint val)
        {
            StackInt ci = new StackInt(val);
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
            uint ui = 0x11223344U;
            ushort us = 0x1122;
            byte b = 0x01;
            int i = 0x11223344;
            int negi = -1;

            StackInt c1 = ui;
            StackInt c2 = us;
            StackInt c3 = b;
            StackInt c4 = (StackInt)i;
            StackInt c5 = (StackInt)negi;

            Helper.ComparePrivateField(c1, "value", (uint)ui);
            Helper.ComparePrivateField(c2, "value", (uint)us);
            Helper.ComparePrivateField(c3, "value", (uint)b);
            Helper.ComparePrivateField(c4, "value", (uint)i);
            Helper.ComparePrivateField(c5, "value", (uint)negi);
        }

        [Fact]
        public void Cast_ToNumberTest()
        {
            StackInt c1 = new StackInt(10);
            StackInt c2 = new StackInt(uint.MaxValue);

            uint ui1 = c1;
            uint ui2 = c2;
            ushort us1 = (ushort)c1;
            ushort us2 = (ushort)c2;
            byte b1 = (byte)c1;
            byte b2 = (byte)c2;
            int i1 = (int)c1;
            int i2 = (int)c2;

            Assert.Equal((uint)10, ui1);
            Assert.Equal(uint.MaxValue, ui2);
            Assert.Equal((ushort)10, us1);
            Assert.Equal(unchecked((ushort)uint.MaxValue), us2);
            Assert.Equal((byte)10, b1);
            Assert.Equal(unchecked((byte)uint.MaxValue), b2);
            Assert.Equal(10, i1);
            Assert.Equal(unchecked((int)uint.MaxValue), i2);
        }


        [Fact]
        public void ComparisonTest()
        {
            StackInt big = new StackInt(1);
            StackInt small = new StackInt(0);

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
            StackInt first = new StackInt(1);
            StackInt second = new StackInt(1);

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
            StackInt ci = new StackInt(c);

            Assert.Equal(expected, ci > i);
            Assert.Equal(expectedEq, ci >= i);

            Assert.Equal(!expected, i > ci);
            Assert.Equal(!expectedEq, i >= ci);

            Assert.Equal(!expected, ci < i);
            Assert.Equal(!expectedEq, ci <= i);

            Assert.Equal(expected, i < ci);
            Assert.Equal(expectedEq, i <= ci);
        }

        [Fact]
        public void Comparison_BigSmall_EqualIntTest()
        {
            StackInt ci = new StackInt(1);
            int i = 1;

            Assert.False(ci > i);
            Assert.True(ci >= i);

            Assert.False(i > ci);
            Assert.True(i >= ci);

            Assert.False(ci < i);
            Assert.True(ci <= i);

            Assert.False(i < ci);
            Assert.True(i <= ci);
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(1, 2, false)]
        [InlineData(1, -1, false)]
        public void Comparison_WithInt_EqualTest(int c, int i, bool expected)
        {
            StackInt ci = new StackInt(c);

            Assert.Equal(expected, ci == i);
            Assert.Equal(!expected, ci != i);

            Assert.Equal(expected, i == ci);
            Assert.Equal(!expected, i != ci);
        }


        [Fact]
        public void CompareToTest()
        {
            StackInt si = new StackInt(100);
            object nObj = null;
            object sObj = "StackInt!";

            Assert.Equal(1, si.CompareTo(nObj));
            Assert.Throws<ArgumentException>(() => si.CompareTo(sObj));
        }

        [Fact]
        public void EqualsTest()
        {
            StackInt si = new StackInt(100);
            object sObj = "StackInt!";
            object nl = null;

            Assert.False(si.Equals(sObj));
            Assert.False(si.Equals(nl));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            int expected = 1250U.GetHashCode();
            int actual = new StackInt(1250).GetHashCode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToStringTest()
        {
            StackInt si = new StackInt(123);
            string actual = si.ToString();

            Assert.Equal("123", actual);
        }

    }
}
