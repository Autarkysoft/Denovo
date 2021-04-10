// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace Tests.Bitcoin.ExtentionsAndHelpersTests
{
    public class ByteArrayExtensionTests
    {
        private readonly byte[] bytes1 = { 1, 2, 3, 4, 5 };
        private readonly byte[] bytes2 = { 6, 7 };
        private readonly byte[] emptyBytes = Array.Empty<byte>();
        private readonly byte[] nullBytes = null;


        [Theory]
        [InlineData(new byte[0], 10, new byte[] { 10 })]
        [InlineData(new byte[] { 1, 2, 3 }, 10, new byte[] { 10, 1, 2, 3 })]
        public void AppendToBeginningTest(byte[] data, byte b, byte[] expected)
        {
            byte[] actual = data.AppendToBeginning(b);
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void AppendToBeginning_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => nullBytes.AppendToBeginning(1));
        }


        [Theory]
        [InlineData(new byte[0], 10, new byte[] { 10 })]
        [InlineData(new byte[] { 1, 2, 3 }, 10, new byte[] { 1, 2, 3, 10 })]
        public void AppendToEndTest(byte[] data, byte b, byte[] expected)
        {
            byte[] actual = data.AppendToEnd(b);
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void AppendToEnd_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => nullBytes.AppendToEnd(1));
        }


        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        [InlineData(new byte[] { 1, 2, 3 })]
        public void CloneByteArrayTest(byte[] ba)
        {
            byte[] actualCloned = ba.CloneByteArray();

            Assert.Equal(ba, actualCloned);
            if (ba != null) // null is always the same!
            {
                Assert.NotSame(ba, actualCloned);
            }
        }


        [Theory]
        [InlineData(new byte[0], new byte[] { 4, 5, 6 }, new byte[] { 4, 5, 6 })]
        [InlineData(new byte[] { 4, 5, 6 }, new byte[0], new byte[] { 4, 5, 6 })]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 }, new byte[] { 1, 2, 3, 4, 5, 6 })]
        [InlineData(new byte[] { 120, 57 }, new byte[] { 255, 0, 12, 11, 50 }, new byte[] { 120, 57, 255, 0, 12, 11, 50 })]
        public void ConcatFastTest(byte[] first, byte[] second, byte[] expected)
        {
            byte[] actual = first.ConcatFast(second);
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void ConcatFast_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => nullBytes.ConcatFast(bytes1));
            Assert.Throws<ArgumentNullException>(() => bytes1.ConcatFast(nullBytes));
        }


        [Theory]
        [InlineData(new byte[0], 0, 0, new byte[0])]
        [InlineData(new byte[] { 1, 2, 3 }, 0, 0, new byte[0])]
        [InlineData(new byte[] { 1, 2, 3 }, 2, 0, new byte[0])]
        [InlineData(new byte[] { 1, 2, 3 }, 0, 3, new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 1, 2, 3 }, 1, 1, new byte[] { 2 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, 2, 3, new byte[] { 3, 4, 5 })]
        public void SubArray_WithIndexCountTest(byte[] ba, int index, int count, byte[] expected)
        {
            byte[] actual = ba.SubArray(index, count);
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void SubArray_WithIndexCount_NullExceptionTest()
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => nullBytes.SubArray(0, 0));
            Assert.Contains("Input can not be null", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => nullBytes.SubArray(0, 2));
            Assert.Contains("Input can not be null", ex.Message);
        }
        [Theory]
        [InlineData(new byte[] { 1, 2, 3 }, -1, 2, "Index or count can not be negative.")]
        [InlineData(new byte[] { 1, 2, 3 }, 0, -2, "Index or count can not be negative.")]
        [InlineData(new byte[] { 1, 2, 3 }, -2, -2, "Index or count can not be negative.")]
        [InlineData(new byte[] { }, 2, 2, "Index can not be bigger than array length.")]
        [InlineData(new byte[] { 1, 2, 3 }, 12, 2, "Index can not be bigger than array length.")]
        [InlineData(new byte[] { }, 0, 3, "Array is not long enough")]
        [InlineData(new byte[] { 1, 2, 3 }, 0, 4, "Array is not long enough")]
        [InlineData(new byte[] { 1, 2, 3 }, 2, 3, "Array is not long enough")]
        public void SubArray_WithIndexCount_IndexExceptionTest(byte[] ba, int index, int count, string expMsg)
        {
            Exception ex = Assert.Throws<IndexOutOfRangeException>(() => ba.SubArray(index, count));
            Assert.Contains(expMsg, ex.Message);
        }


        [Theory]
        [InlineData(new byte[] { }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 2, 3 }, 0, new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 255, 12, 0, 52, 112 }, 3, new byte[] { 52, 112 })]
        [InlineData(new byte[] { 255, 12, 0, 52, 112 }, 4, new byte[] { 112 })]
        public void SubArray_WithIndexTest(byte[] ba, int index, byte[] expected)
        {
            byte[] actual = ba.SubArray(index);
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void SubArray_WithIndex_NullExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => nullBytes.SubArray(0));
        }
        [Theory]
        [InlineData(new byte[] { }, 1, "Index can not be bigger than array length.")]
        [InlineData(new byte[] { 1, 2, 3 }, 3, "Index can not be bigger than array length.")]
        [InlineData(new byte[] { 1, 2, 3 }, -1, "Index or count can not be negative.")]
        public void SubArray_WithIndex_ExceptionTest(byte[] ba, int index, string expMsg)
        {
            Exception ex = Assert.Throws<IndexOutOfRangeException>(() => ba.SubArray(index));
            Assert.Contains(expMsg, ex.Message);
        }


        [Theory]
        [InlineData(new byte[] { }, 0, new byte[] { })]
        [InlineData(new byte[] { 125 }, 0, new byte[] { })]
        [InlineData(new byte[] { 125 }, 1, new byte[] { 125 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 }, 0, new byte[] { })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 }, 1, new byte[] { 5 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 }, 4, new byte[] { 2, 3, 4, 5 })]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 }, 5, new byte[] { 1, 2, 3, 4, 5 })]
        public void SubArrayFromEndTest(byte[] ba, int count, byte[] expected)
        {
            byte[] actual = ba.SubArrayFromEnd(count);
            Assert.Equal(expected, actual);
        }
        [Fact]
        public void SubArrayFromEnd_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => nullBytes.SubArrayFromEnd(0));
            Assert.Throws<ArgumentNullException>(() => nullBytes.SubArrayFromEnd(2));
            Assert.Throws<IndexOutOfRangeException>(() => emptyBytes.SubArrayFromEnd(2));
            Assert.Throws<IndexOutOfRangeException>(() => bytes1.SubArrayFromEnd(8));
            Assert.Throws<IndexOutOfRangeException>(() => bytes1.SubArrayFromEnd(-1));
        }


        [Theory]
        [InlineData(new byte[] { }, "")]
        [InlineData(new byte[] { 0 }, "00")]
        [InlineData(new byte[] { 0, 0 }, "0000")]
        [InlineData(new byte[] { 16, 42, 255 }, "102aff")]
        [InlineData(new byte[] { 0, 0, 2 }, "000002")]
        public void ToBase16Test(byte[] ba, string expected)
        {
            string actual = ba.ToBase16();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToBase16_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => nullBytes.ToBase16());
        }

        [Fact]
        public void ToBase64Test()
        {
            Assert.Equal("", emptyBytes.ToBase64());
            Assert.Throws<ArgumentNullException>(() => nullBytes.ToBase64());
        }


        public static IEnumerable<object[]> GetBigInts()
        {
            yield return new object[] { "", BigInteger.Zero, BigInteger.Zero };
            yield return new object[] { "00", BigInteger.Zero, BigInteger.Zero };
            yield return new object[] { "01", BigInteger.One, BigInteger.One };
            yield return new object[] { "ff", BigInteger.Parse("255"), BigInteger.MinusOne };
            yield return new object[] { "78", BigInteger.Parse("120"), BigInteger.Parse("120") };
            yield return new object[] { "80", BigInteger.Parse("128"), BigInteger.Parse("-128") };
            yield return new object[] { "0100", BigInteger.Parse("256"), BigInteger.Parse("256") };
            yield return new object[] { "0400", BigInteger.Parse("1024"), BigInteger.Parse("1024") };
            yield return new object[]
            {
                "7fffffffffffffff",
                BigInteger.Parse("9223372036854775807"),
                BigInteger.Parse("9223372036854775807")
            };
            yield return new object[]
            {
                "8000000000000000",
                BigInteger.Parse("9223372036854775808"),
                BigInteger.Parse("-9223372036854775808")
            };
            yield return new object[]
            {
                "0bf57db7ae7222c705faf57218514706bf12a5d86c955df606c2ef4cdb112e084a3a",
                BigInteger.Parse("354496448798491597687421477965438779167865543454566144545988433114569455671200314"),
                BigInteger.Parse("354496448798491597687421477965438779167865543454566144545988433114569455671200314")
            };
        }

        [Theory]
        [MemberData(nameof(GetBigInts))]
        public void ToBigIntTest(string hex, BigInteger posNum, BigInteger negNum)
        {
            byte[] bigEndianBytes = Helper.HexToBytes(hex);
            byte[] littleEndianBytes = bigEndianBytes.Reverse().ToArray();

            Assert.Equal(posNum, bigEndianBytes.ToBigInt(true, true));
            Assert.Equal(posNum, littleEndianBytes.ToBigInt(false, true));
            Assert.Equal(negNum, bigEndianBytes.ToBigInt(true, false));
            Assert.Equal(negNum, littleEndianBytes.ToBigInt(false, false));

            // Make sure byte arrays aren't changed after being passed in the function!
            byte[] unchangedBigEndianBytes = Helper.HexToBytes(hex);
            byte[] unchangedLittleEndianBytes = unchangedBigEndianBytes.Reverse().ToArray();
            Assert.Equal(unchangedBigEndianBytes, bigEndianBytes);
            Assert.Equal(unchangedLittleEndianBytes, littleEndianBytes);
        }

        [Fact]
        public void ToBigInt_ExceptionsTest()
        {
            Assert.Throws<ArgumentNullException>(() => nullBytes.ToBigInt(true, true));
            Assert.Throws<ArgumentNullException>(() => nullBytes.ToBigInt(false, false));
        }


        [Theory]
        [InlineData(new byte[] { 10 }, new byte[] { 10 })]
        [InlineData(new byte[] { 10, 0 }, new byte[] { 10 })]
        [InlineData(new byte[] { 10, 0, 0 }, new byte[] { 10 })]
        [InlineData(new byte[] { 0, 10 }, new byte[] { 0, 10 })]
        [InlineData(new byte[] { 10, 20, 30 }, new byte[] { 10, 20, 30 })]
        [InlineData(new byte[] { 10, 20, 30, 0 }, new byte[] { 10, 20, 30 })]
        [InlineData(new byte[] { 10, 20, 30, 0, 0 }, new byte[] { 10, 20, 30 })]
        [InlineData(new byte[] { 0, 10, 20, 0, 0, 0 }, new byte[] { 0, 10, 20 })]
        [InlineData(new byte[] { 0, 0, 0, 0, 0 }, new byte[] { })]
        public void TrimEndTest(byte[] ba, byte[] expected)
        {
            byte[] actual = ba.TrimEnd();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new byte[] { 10 }, new byte[] { 10 })]
        [InlineData(new byte[] { 0, 10 }, new byte[] { 10 })]
        [InlineData(new byte[] { 0, 0, 10 }, new byte[] { 10 })]
        [InlineData(new byte[] { 10, 0 }, new byte[] { 10, 0 })]
        [InlineData(new byte[] { 10, 20, 30 }, new byte[] { 10, 20, 30 })]
        [InlineData(new byte[] { 0, 10, 20, 30 }, new byte[] { 10, 20, 30 })]
        [InlineData(new byte[] { 0, 0, 10, 20, 30 }, new byte[] { 10, 20, 30 })]
        [InlineData(new byte[] { 0, 0, 10, 20, 0 }, new byte[] { 10, 20, 0 })]
        [InlineData(new byte[] { 0, 0, 0, 0, 0 }, new byte[] { })]
        public void TrimStartTest(byte[] ba, byte[] expected)
        {
            byte[] actual = ba.TrimStart();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Trim_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => nullBytes.TrimStart());
            Assert.Throws<ArgumentNullException>(() => nullBytes.TrimEnd());
        }
    }
}
