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
            StackInt zeroI = new(0);
            StackInt zeroU = new(0U);
            Helper.ComparePrivateField(zeroI, "value", 0U);
            Helper.ComparePrivateField(zeroU, "value", 0U);

            StackInt cI = new(123);
            StackInt cU = new(123U);
            Helper.ComparePrivateField(cI, "value", 123U);
            Helper.ComparePrivateField(cU, "value", 123U);

            StackInt maxI = new(int.MaxValue);
            StackInt maxU = new(uint.MaxValue);
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
            StackInt si = new(val);
            OP actual = si.GetOpCode();

            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetTryReadCases()
        {
            Errors err1 = Errors.SmallOPPushData1;
            Errors err2 = Errors.SmallOPPushData2;
            Errors err3 = Errors.SmallOPPushData4;

            // Not enough bytes to read
            yield return new object[] { Array.Empty<byte>(), false, null, Errors.EndOfStream, 0, 0 };

            // 1 byte correct encoding (<0x4c)
            yield return new object[] { new byte[] { 0 }, true, null, Errors.None, 1, 0 };
            yield return new object[] { new byte[] { 5, 1, 2 }, true, null, Errors.None, 1, 5 };
            yield return new object[] { new byte[] { 75 }, true, null, Errors.None, 1, 75 };

            // 2 bytes correct encoding (>=0x4c && <=255)
            yield return new object[] { new byte[] { 76, 76 }, true, null, Errors.None, 2, 76 };
            yield return new object[] { new byte[] { 76, 77 }, true, null, Errors.None, 2, 77 };
            yield return new object[] { new byte[] { 76, 78 }, true, null, Errors.None, 2, 78 };
            yield return new object[] { new byte[] { 76, 79 }, true, null, Errors.None, 2, 79 };
            yield return new object[] { new byte[] { 76, 132, 10, 20, 30 }, true, null, Errors.None, 2, 132 };
            yield return new object[] { new byte[] { 76, 255, 12 }, true, null, Errors.None, 2, 255 };

            // 2 byte not enough bytes to read
            yield return new object[]
            {
                new byte[] { 76 }, false, null, Errors.ShortOPPushData1, 1, 0
            };

            // 2 bytes wrong encoding (any value < 0x4c)
            yield return new object[] { new byte[] { 76, 0 }, true, new byte[2] { 76, 0 }, err1, 2, 0 };
            yield return new object[] { new byte[] { 76, 6 }, true, new byte[2] { 76, 6 }, err1, 2, 6 };
            yield return new object[] { new byte[] { 76, 12, 13, 14 }, true, new byte[2] { 76, 12 }, err1, 2, 12 };
            yield return new object[] { new byte[] { 76, 75 }, true, new byte[2] { 76, 75 }, err1, 2, 75 };

            // 3 bytes correct encoding (> 255 && <= 0xffff)
            yield return new object[] { new byte[] { 77, 0, 1 }, true, null, Errors.None, 3, 256 };
            yield return new object[] { new byte[] { 77, 171, 205 }, true, null, Errors.None, 3, 52651 };
            yield return new object[] { new byte[] { 77, 255, 255 }, true, null, Errors.None, 3, ushort.MaxValue };

            // 3 bytes not enough bytes to read
            yield return new object[]
            {
                new byte[] { 77 }, false, null, Errors.ShortOPPushData2, 1, 0
            };
            yield return new object[]
            {
                new byte[] { 77, 255 }, false, null, Errors.ShortOPPushData2, 1, 0
            };

            // 3 bytes wrong encoding
            yield return new object[] { new byte[] { 77, 0, 0 }, true, new byte[3] { 77, 0, 0 }, err2, 3, 0 };
            yield return new object[] { new byte[] { 77, 1, 0 }, true, new byte[3] { 77, 1, 0 }, err2, 3, 1 };
            yield return new object[] { new byte[] { 77, 76, 0 }, true, new byte[3] { 77, 76, 0 }, err2, 3, 76 };
            yield return new object[] { new byte[] { 77, 77, 0 }, true, new byte[3] { 77, 77, 0 }, err2, 3, 77 };
            yield return new object[] { new byte[] { 77, 78, 0 }, true, new byte[3] { 77, 78, 0 }, err2, 3, 78 };
            yield return new object[] { new byte[] { 77, 79, 0 }, true, new byte[3] { 77, 79, 0 }, err2, 3, 79 };
            yield return new object[] { new byte[] { 77, 255, 0 }, true, new byte[3] { 77, 255, 0 }, err2, 3, 255 };

            // 5 bytes correct encoding
            yield return new object[] { new byte[] { 78, 0, 0, 1, 0 }, true, null, Errors.None, 5, ushort.MaxValue + 1 };
            yield return new object[] { new byte[] { 78, 2, 1, 15, 15 }, true, null, Errors.None, 5, 252641538 };
            yield return new object[] { new byte[] { 78, 255, 255, 255, 255 }, true, null, Errors.None, 5, uint.MaxValue };

            // 5 bytes not enough bytes to read
            Errors push4Error = Errors.ShortOPPushData4;
            yield return new object[] { new byte[] { 78 }, false, null, push4Error, 1, 0 };
            yield return new object[] { new byte[] { 78, 255 }, false, null, push4Error, 1, 0 };
            yield return new object[] { new byte[] { 78, 255, 255 }, false, null, push4Error, 1, 0 };
            yield return new object[] { new byte[] { 78, 255, 255, 255 }, false, null, push4Error, 1, 0 };

            // 5 bytes wrong encoding
            yield return new object[] { new byte[] { 78, 0, 0, 0, 0 }, true, new byte[5] { 78, 0, 0, 0, 0 }, err3, 5, 0 };
            yield return new object[] { new byte[] { 78, 1, 0, 0, 0, 5 }, true, new byte[5] { 78, 1, 0, 0, 0 }, err3, 5, 1 };
            yield return new object[] { new byte[] { 78, 76, 0, 0, 0 }, true, new byte[5] { 78, 76, 0, 0, 0 }, err3, 5, 76 };
            yield return new object[] { new byte[] { 78, 77, 0, 0, 0 }, true, new byte[5] { 78, 77, 0, 0, 0 }, err3, 5, 77 };
            yield return new object[] { new byte[] { 78, 78, 0, 0, 0 }, true, new byte[5] { 78, 78, 0, 0, 0 }, err3, 5, 78 };
            yield return new object[] { new byte[] { 78, 79, 0, 0, 0, 1, 1 }, true, new byte[5] { 78, 79, 0, 0, 0 }, err3, 5, 79 };
            yield return new object[] { new byte[] { 78, 0, 1, 0, 0 }, true, new byte[5] { 78, 0, 1, 0, 0 }, err3, 5, 256 };
            yield return new object[]
            {
                new byte[] { 78, 255, 255, 0, 0, 22, 23, 25, 1 }, true, new byte[5] { 78, 255, 255, 0, 0 }, err3, 5, ushort.MaxValue
            };

            // Wrong first byte to be a StackInt
            yield return new object[] { new byte[] { 79, 255, 255, 0, 0 }, false, null, Errors.UnknownOpPush, 1, 0 };
            yield return new object[] { new byte[] { 100, 77, 1, 0 }, false, null, Errors.UnknownOpPush, 1, 0 };
            yield return new object[] { new byte[] { 255 }, false, null, Errors.UnknownOpPush, 1, 0 };
        }
        [Theory]
        [MemberData(nameof(GetTryReadCases))]
        public void TryReadTest(byte[] data, bool expSuccess, byte[] expBytes, Errors expErr, int finalPos, uint expected)
        {
            var stream = new FastStreamReader(data);
            bool actSuccess = StackInt.TryRead(stream, out byte[] actBytes, out StackInt actual, out Errors error);

            Assert.Equal(expSuccess, actSuccess);
            Assert.Equal(expBytes, actBytes);
            Assert.Equal(expErr, error);
            Helper.ComparePrivateField(stream, "position", finalPos);
            Helper.ComparePrivateField(actual, "value", expected);
        }

        [Fact]
        public void TryRead_Fail_NullStreamTest()
        {
            bool b = StackInt.TryRead(null, out _, out StackInt actual, out Errors error);

            Assert.False(b);
            Assert.Equal(Errors.NullStream, error);
            Helper.ComparePrivateField(actual, "value", 0U);
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
        public void ToByteArrayTest(byte[] data, int finalOffset, uint val)
        {
            StackInt ci = new(val);
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
            StackInt c1 = new(10);
            StackInt c2 = new(uint.MaxValue);

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


        public static IEnumerable<object[]> GetCompareSameTypeCases()
        {
            yield return new object[]
            {
                new StackInt(0), new StackInt(0), new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new StackInt(1), new StackInt(0), new ValueCompareResult(true, true, false, false, false, 1)
            };
            yield return new object[]
            {
                new StackInt(0), new StackInt(1), new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new StackInt(1), new StackInt(1), new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new StackInt(1), new StackInt(2), new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new StackInt(2), new StackInt(1), new ValueCompareResult(true, true, false, false, false, 1)
            };
            yield return new object[]
            {
                new StackInt(uint.MaxValue),
                new StackInt(uint.MaxValue),
                new ValueCompareResult(false, true, false, true, true, 0)
            };
            yield return new object[]
            {
                new StackInt(uint.MaxValue),
                new StackInt(0),
                new ValueCompareResult(true, true, false, false, false, 1),
            };
            yield return new object[]
            {
                new StackInt(0),
                new StackInt(uint.MaxValue),
                new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new StackInt(uint.MaxValue-1),
                new StackInt(uint.MaxValue),
                new ValueCompareResult(false, false, true, true, false, -1)
            };
            yield return new object[]
            {
                new StackInt(uint.MaxValue),
                new StackInt(uint.MaxValue-1),
                new ValueCompareResult(true, true, false, false, false, 1)
            };
        }
        [Theory]
        [MemberData(nameof(GetCompareSameTypeCases))]
        public void ComparisonOperator_SameTypeTest(StackInt si1, StackInt si2, ValueCompareResult expected)
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
            yield return new object[] { new StackInt(0), 0, new ValueCompareResult(false, true, false, true, true) };
            yield return new object[] { new StackInt(0), 1, new ValueCompareResult(false, false, true, true, false) };
            yield return new object[] { new StackInt(0), -1, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new StackInt(0), int.MaxValue, new ValueCompareResult(false, false, true, true, false) };

            yield return new object[] { new StackInt(1), 0, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new StackInt(1), 1, new ValueCompareResult(false, true, false, true, true) };
            yield return new object[] { new StackInt(1), 2, new ValueCompareResult(false, false, true, true, false) };
            yield return new object[] { new StackInt(1), -1, new ValueCompareResult(true, true, false, false, false) };
            yield return new object[] { new StackInt(1), int.MaxValue, new ValueCompareResult(false, false, true, true, false) };

            yield return new object[]
            {
                new StackInt(uint.MaxValue), 0, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new StackInt(uint.MaxValue), -1, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new StackInt(uint.MaxValue), 1, new ValueCompareResult(true, true, false, false, false)
            };
            yield return new object[]
            {
                new StackInt(int.MaxValue), int.MaxValue, new ValueCompareResult(false, true, false, true, true)
            };
        }
        [Theory]
        [MemberData(nameof(GetCompareIntCases))]
        public void ComparisonOperator_WithIntTest(StackInt si, int i, ValueCompareResult expected)
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
            StackInt si = new(100);
            object nObj = null;
            object sObj = "StackInt!";

            Assert.Equal(1, si.CompareTo(nObj));
            Assert.Throws<ArgumentException>(() => si.CompareTo(sObj));
        }

        [Fact]
        public void Equals_EdgeTest()
        {
            StackInt si = new(100);
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
            StackInt si = new(123);
            string actual = si.ToString();

            Assert.Equal("123", actual);
        }
    }
}
