// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin
{
    public class FastStreamReaderTests
    {
        [Fact]
        public void ConstructorTest()
        {
            byte[] data = { 1, 2, 3 };
            var stream = new FastStreamReader(data);
            // Make sure data is NOT cloned (default behavior)
            data[0] = 255;

            Helper.ComparePrivateField(stream, "data", new byte[] { 255, 2, 3 });
            Helper.ComparePrivateField(stream, "position", 0);
        }

        [Fact]
        public void Constructor_ClonedTest()
        {
            byte[] data = { 1, 2, 3 };
            var stream = new FastStreamReader(data, true);
            // Make sure data is copied/cloned
            data[0] = 255;

            Helper.ComparePrivateField(stream, "data", new byte[] { 1, 2, 3 });
            Helper.ComparePrivateField(stream, "position", 0);
        }

        [Fact]
        public void Constructor_SubArrayTest()
        {
            byte[] data = { 1, 2, 3, 4, 5 };
            var stream = new FastStreamReader(data, 1, 3);

            Helper.ComparePrivateField(stream, "data", new byte[] { 2, 3, 4 });
            Helper.ComparePrivateField(stream, "position", 0);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new FastStreamReader(null));
            Assert.Throws<ArgumentNullException>(() => new FastStreamReader(null, 0, 1));
            Assert.Throws<IndexOutOfRangeException>(() => new FastStreamReader(new byte[1], 0, 2));
        }


        [Fact]
        public void CheckRemainingTest()
        {
            var stream = new FastStreamReader(new byte[5]);
            Assert.True(stream.CheckRemaining(1));
            Assert.True(stream.CheckRemaining(5));
            Assert.False(stream.CheckRemaining(6));

            _ = stream.TryReadByteArray(2, out _);
            Assert.True(stream.CheckRemaining(1));
            Assert.False(stream.CheckRemaining(5));
            Assert.False(stream.CheckRemaining(6));
        }

        [Fact]
        public void GetCurrentIndexTest()
        {
            var stream = new FastStreamReader(new byte[10]);
            Assert.Equal(0, stream.GetCurrentIndex());
            _ = stream.TryReadByteArray(3, out _);
            Assert.Equal(3, stream.GetCurrentIndex());
            _ = stream.TryReadByteArray(5, out _);
            Assert.Equal(8, stream.GetCurrentIndex());
            _ = stream.TryReadByteArray(2, out _);
            Assert.Equal(10, stream.GetCurrentIndex());
        }

        [Fact]
        public void GetRemainingBytesCountTest()
        {
            var stream = new FastStreamReader(new byte[10]);
            Assert.Equal(10, stream.GetRemainingBytesCount());
            _ = stream.TryReadByteArray(2, out _);
            Assert.Equal(8, stream.GetRemainingBytesCount());
            _ = stream.TryReadByteArray(1, out _);
            Assert.Equal(7, stream.GetRemainingBytesCount());
            _ = stream.TryReadByteArray(7, out _);
            Assert.Equal(0, stream.GetRemainingBytesCount());
        }

        [Fact]
        public void SkipOneByteTest()
        {
            var stream = new FastStreamReader(new byte[5]);
            Assert.Equal(0, stream.GetCurrentIndex());

            stream.SkipOneByte();
            Assert.Equal(1, stream.GetCurrentIndex());

            stream.SkipOneByte();
            Assert.Equal(2, stream.GetCurrentIndex());
        }

        [Theory]
        [InlineData(new byte[] { 1 }, new byte[] { 1 }, false, true)]
        [InlineData(new byte[] { 1 }, new byte[] { 2 }, false, false)]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 }, false, true)]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 1, 2 }, false, true)]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3, 4 }, false, false)]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 2, 3 }, false, false)]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 2, 3 }, true, true)]
        [InlineData(new byte[] { 2, 3 }, new byte[] { 2, 3 }, true, false)]
        public void CompareBytesTest(byte[] data, byte[] other, bool skip, bool expected)
        {
            var stream = new FastStreamReader(data);
            if (skip)
            {
                stream.SkipOneByte();
            }
            bool actual = stream.CompareBytes(other);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new byte[] { 1 }, new byte[] { 1 }, true, 0)]
        [InlineData(new byte[] { 1 }, new byte[] { 2 }, false, 1)]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 }, true, 0)]
        [InlineData(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new byte[] { 4, 5, 6, 7 }, true, 4)]
        [InlineData(new byte[] { 0, 1, 2, 3 }, new byte[] { 10, 20 }, false, 3)]
        public void FindAndSkipTest(byte[] data, byte[] other, bool expected, int expPos)
        {
            var stream = new FastStreamReader(data);
            bool actual = stream.FindAndSkip(other);
            Assert.Equal(expected, actual);
            Helper.ComparePrivateField(stream, "position", expPos);
        }

        [Fact]
        public void ReadByteArrayCheckedTest()
        {
            var stream = new FastStreamReader(new byte[] { 1, 2, 3, 4, 5, 6 });

            byte[] actual = stream.ReadByteArrayChecked(0);
            byte[] expected = new byte[0];
            Assert.Equal(expected, actual);
            Helper.ComparePrivateField(stream, "position", 0);

            actual = stream.ReadByteArrayChecked(2);
            expected = new byte[] { 1, 2 };
            Assert.Equal(expected, actual);
            Helper.ComparePrivateField(stream, "position", 2);

            actual = stream.ReadByteArrayChecked(3);
            expected = new byte[] { 3, 4, 5 };
            Assert.Equal(expected, actual);
            Helper.ComparePrivateField(stream, "position", 5);

            actual = stream.ReadByteArrayChecked(1);
            expected = new byte[] { 6 };
            Assert.Equal(expected, actual);
            Helper.ComparePrivateField(stream, "position", 6);
        }

        [Fact]
        public void ReadByteArray32CheckedTest()
        {
            var stream = new FastStreamReader(Helper.GetBytes(35));
            byte[] actual = stream.ReadByteArray32Checked();
            byte[] expected = Helper.GetBytes(32);

            Assert.Equal(expected, actual);
            Assert.Equal(32, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadByteArrayTest()
        {
            var stream = new FastStreamReader(Helper.GetBytes(12));
            bool b = stream.TryReadByteArray(10, out byte[] actual);
            byte[] expected = Helper.GetBytes(10);

            Assert.True(b);
            Assert.Equal(expected, actual);
            Assert.Equal(10, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadByteArray_FailTest()
        {
            var stream = new FastStreamReader(new byte[9]);
            bool b = stream.TryReadByteArray(10, out byte[] actual);

            Assert.False(b);
            Assert.Null(actual);
        }


        public static IEnumerable<object[]> GetCompactReadCases()
        {
            yield return new object[] { new byte[1], new byte[0] };
            yield return new object[] { new byte[] { 1, 255 }, new byte[] { 255 } };
            yield return new object[] { new byte[] { 2, 10, 20 }, new byte[] { 10, 20 } };

            byte[] b252 = Helper.GetBytes(252);
            yield return new object[] { Helper.ConcatBytes(253, new byte[] { 252 }, b252), b252 };

            byte[] b253 = Helper.GetBytes(253);
            yield return new object[] { Helper.ConcatBytes(256, new byte[] { 253, 253, 0 }, b253), b253 };

            byte[] big = new byte[ushort.MaxValue + 6];
            big[0] = 254;
            big[3] = 1;
            big[5] = 250; // To make sure this byte is not read as part of length
            big[^1] = 251; // to Mark the end
            byte[] expBig = new byte[ushort.MaxValue + 1];
            expBig[0] = 250;
            expBig[^1] = 251;
            yield return new object[] { big, expBig };
        }
        [Theory]
        [MemberData(nameof(GetCompactReadCases))]
        public void TryReadByteArrayCompactIntTest(byte[] ba, byte[] expected)
        {
            var stream = new FastStreamReader(ba);
            bool b = stream.TryReadByteArrayCompactInt(out byte[] actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
        }
        [Theory]
        [InlineData(new byte[] { })]
        [InlineData(new byte[] { 1 })]
        [InlineData(new byte[] { 2, 10 })]
        [InlineData(new byte[] { 253, 1 })]
        [InlineData(new byte[] { 253, 1, 0, 10 })] // Len = 1 is using wrong encoding
        [InlineData(new byte[] { 253, 0, 1, 10 })] // Len = 256 not enough bytes remain to read
        [InlineData(new byte[] { 254, 1 })]
        [InlineData(new byte[] { 254, 1, 0, 0, 0 })] // Len = 1 is using wrong encoding
        [InlineData(new byte[] { 254, 255, 255, 0, 0 })] // same as above
        [InlineData(new byte[] { 254, 255, 255, 0, 128 })] // Huge UInt32 (negative int)
        [InlineData(new byte[] { 255, 1, 1, 1, 1, 1, 1, 1, 1 })] // Too huge
        public void TryReadByteArrayCompactInt_FailTest(byte[] ba)
        {
            var stream = new FastStreamReader(ba);
            bool b = stream.TryReadByteArrayCompactInt(out byte[] actual);

            Assert.False(b);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData(0, 1, new byte[] { 0 })]
        [InlineData(0, 1, new byte[] { 0, 2, 3 })]
        [InlineData(1, 1, new byte[] { 1, 2, 3 })]
        [InlineData(252, 1, new byte[] { 252 })]
        [InlineData(252, 1, new byte[] { 252, 2, 3 })]
        [InlineData(253, 3, new byte[] { 253, 253, 0 })]
        [InlineData(515, 3, new byte[] { 253, 3, 2 })]
        [InlineData(ushort.MaxValue, 3, new byte[] { 253, 255, 255 })]
        public void TryReadSmallCompactIntTest(int expected, int expPos, byte[] ba)
        {
            var stream = new FastStreamReader(ba);
            bool b = stream.TryReadSmallCompactInt(out int actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
            Helper.ComparePrivateField(stream, "position", expPos);
        }
        [Theory]
        [InlineData(new byte[] { })]
        [InlineData(new byte[] { 253 })]
        [InlineData(new byte[] { 253, 255 })]
        [InlineData(new byte[] { 253, 0, 0 })]
        [InlineData(new byte[] { 253, 252, 0 })]
        [InlineData(new byte[] { 254 })]
        [InlineData(new byte[] { 255 })]
        [InlineData(new byte[] { 254, 1 })]
        [InlineData(new byte[] { 254, 255, 255, 255, 255 })] // Valid CompactInt but rejected by this method
        [InlineData(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255 })] // same as above
        public void TryReadSmallCompactInt_FailTest(byte[] ba)
        {
            var stream = new FastStreamReader(ba);
            bool b = stream.TryReadSmallCompactInt(out _);
            Assert.False(b);
        }

        [Fact]
        public void ReadByteCheckedTest()
        {
            var stream = new FastStreamReader(new byte[] { 10, 20, 30 });
            Helper.ComparePrivateField(stream, "position", 0);

            byte b1 = stream.ReadByteChecked();
            Assert.Equal(10, b1);
            Helper.ComparePrivateField(stream, "position", 1);

            byte b2 = stream.ReadByteChecked();
            Assert.Equal(20, b2);
            Helper.ComparePrivateField(stream, "position", 2);

            byte b3 = stream.ReadByteChecked();
            Assert.Equal(30, b3);
            Helper.ComparePrivateField(stream, "position", 3);
        }

        [Fact]
        public void TryPeekByteTest()
        {
            var stream = new FastStreamReader(new byte[3] { 10, 20, 30 });
            bool b = stream.TryPeekByte(out byte res);

            Assert.True(b);
            Assert.Equal((byte)10, res);
            Assert.Equal(0, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadByteTest()
        {
            var stream = new FastStreamReader(new byte[3] { 10, 20, 30 });
            bool b = stream.TryReadByte(out byte res);

            Assert.True(b);
            Assert.Equal((byte)10, res);
            Assert.Equal(1, stream.GetCurrentIndex());
        }

        [Theory]
        [InlineData(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[4] { 0xff, 0x00, 0x00, 0x00 }, 255)]
        [InlineData(new byte[4] { 0xa9, 0x57, 0x31, 0x28 }, 674322345)]
        [InlineData(new byte[4] { 0xff, 0xff, 0xff, 0x7f }, int.MaxValue)]
        [InlineData(new byte[4] { 0xff, 0xff, 0xff, 0xff }, -1)]
        [InlineData(new byte[4] { 0x00, 0x00, 0x00, 0x80 }, int.MinValue)]
        public void ReadInt32CheckedTest(byte[] data, int expected)
        {
            var stream = new FastStreamReader(data);
            int actual = stream.ReadInt32Checked();

            Assert.Equal(expected, actual);
            Assert.Equal(4, stream.GetCurrentIndex());
        }

        [Theory]
        [InlineData(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[4] { 0xff, 0x00, 0x00, 0x00 }, 255)]
        [InlineData(new byte[4] { 0xa9, 0x57, 0x31, 0x28 }, 674322345)]
        [InlineData(new byte[4] { 0xff, 0xff, 0xff, 0x7f }, int.MaxValue)]
        [InlineData(new byte[4] { 0xff, 0xff, 0xff, 0xff }, -1)]
        [InlineData(new byte[4] { 0x00, 0x00, 0x00, 0x80 }, int.MinValue)]
        public void TryReadInt32Test(byte[] data, int expected)
        {
            var stream = new FastStreamReader(data);
            bool b = stream.TryReadInt32(out int actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
            Assert.Equal(4, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadInt32_FailTest()
        {
            var stream = new FastStreamReader(new byte[3]);
            bool b = stream.TryReadInt32(out int _);
            Assert.False(b);
        }

        [Theory]
        [InlineData(new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[8] { 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 255)]
        [InlineData(new byte[8] { 0xf0, 0x49, 0x32, 0xa3, 0xff, 0xab, 0xc9, 0x0b }, 849399119179041264)]
        [InlineData(new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f }, long.MaxValue)]
        [InlineData(new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }, -1)]
        [InlineData(new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, long.MinValue)]
        public void ReadInt64CheckedTest(byte[] data, long expected)
        {
            var stream = new FastStreamReader(data);
            long actual = stream.ReadInt64Checked();

            Assert.Equal(expected, actual);
            Assert.Equal(8, stream.GetCurrentIndex());
        }

        [Theory]
        [InlineData(new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0)]
        [InlineData(new byte[8] { 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 255)]
        [InlineData(new byte[8] { 0xf0, 0x49, 0x32, 0xa3, 0xff, 0xab, 0xc9, 0x0b }, 849399119179041264)]
        [InlineData(new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f }, long.MaxValue)]
        [InlineData(new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }, -1)]
        [InlineData(new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, long.MinValue)]
        public void TryReadInt64Test(byte[] data, long expected)
        {
            var stream = new FastStreamReader(data);
            bool b = stream.TryReadInt64(out long actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
            Assert.Equal(8, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadInt64_FailTest()
        {
            var stream = new FastStreamReader(new byte[7]);
            bool b = stream.TryReadInt64(out long _);
            Assert.False(b);
        }


        [Theory]
        [InlineData(new byte[2] { 0x00, 0x00 }, 0)]
        [InlineData(new byte[2] { 0xff, 0x00, }, 255)]
        [InlineData(new byte[2] { 0x37, 0xc9 }, 51511)]
        [InlineData(new byte[2] { 0xff, 0xff }, ushort.MaxValue)]
        public void ReadUInt16CheckedTest(byte[] data, ushort expected)
        {
            var stream = new FastStreamReader(data);
            ushort actual = stream.ReadUInt16Checked();

            Assert.Equal(expected, actual);
            Assert.Equal(2, stream.GetCurrentIndex());
        }

        [Theory]
        [InlineData(new byte[2] { 0x00, 0x00 }, 0)]
        [InlineData(new byte[2] { 0xff, 0x00, }, 255)]
        [InlineData(new byte[2] { 0x37, 0xc9 }, 51511)]
        [InlineData(new byte[2] { 0xff, 0xff }, ushort.MaxValue)]
        public void TryReadUInt16Test(byte[] data, ushort expected)
        {
            var stream = new FastStreamReader(data);
            bool b = stream.TryReadUInt16(out ushort actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
            Assert.Equal(2, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadUInt16_FailTest()
        {
            var stream = new FastStreamReader(new byte[1]);
            bool b = stream.TryReadUInt16(out ushort _);
            Assert.False(b);
        }


        [Theory]
        [InlineData(new byte[2] { 0x00, 0x00 }, 0)]
        [InlineData(new byte[2] { 0x00, 0xff, }, 255)]
        [InlineData(new byte[2] { 0xc9, 0x37 }, 51511)]
        [InlineData(new byte[2] { 0xff, 0xff }, ushort.MaxValue)]
        public void ReadUInt16BigEndianCheckedTest(byte[] data, ushort expected)
        {
            var stream = new FastStreamReader(data);
            ushort actual = stream.ReadUInt16BigEndianChecked();

            Assert.Equal(expected, actual);
            Assert.Equal(2, stream.GetCurrentIndex());
        }

        [Theory]
        [InlineData(new byte[2] { 0x00, 0x00 }, 0)]
        [InlineData(new byte[2] { 0x00, 0xff, }, 255)]
        [InlineData(new byte[2] { 0xc9, 0x37 }, 51511)]
        [InlineData(new byte[2] { 0xff, 0xff }, ushort.MaxValue)]
        public void TryReadUInt16BigEndianTest(byte[] data, ushort expected)
        {
            var stream = new FastStreamReader(data);
            bool b = stream.TryReadUInt16BigEndian(out ushort actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
            Assert.Equal(2, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadUInt16BigEndian_FailTest()
        {
            var stream = new FastStreamReader(new byte[1]);
            bool b = stream.TryReadUInt16BigEndian(out ushort _);
            Assert.False(b);
        }


        [Theory]
        [InlineData(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0U)]
        [InlineData(new byte[4] { 0xff, 0x00, 0x00, 0x00 }, 255U)]
        [InlineData(new byte[4] { 0xe3, 0xfd, 0xd6, 0x03 }, 64421347U)]
        [InlineData(new byte[4] { 0xff, 0xff, 0xff, 0xff }, uint.MaxValue)]
        public void ReadUInt32CheckedTest(byte[] data, uint expected)
        {
            var stream = new FastStreamReader(data);
            uint actual = stream.ReadUInt32Checked();

            Assert.Equal(expected, actual);
            Assert.Equal(4, stream.GetCurrentIndex());
        }

        [Theory]
        [InlineData(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0U)]
        [InlineData(new byte[4] { 0xff, 0x00, 0x00, 0x00 }, 255U)]
        [InlineData(new byte[4] { 0xe3, 0xfd, 0xd6, 0x03 }, 64421347U)]
        [InlineData(new byte[4] { 0xff, 0xff, 0xff, 0xff }, uint.MaxValue)]
        public void TryReadUInt32Test(byte[] data, uint expected)
        {
            var stream = new FastStreamReader(data);
            bool b = stream.TryReadUInt32(out uint actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
            Assert.Equal(4, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadUInt32_FailTest()
        {
            var stream = new FastStreamReader(new byte[3]);
            bool b = stream.TryReadUInt32(out uint _);
            Assert.False(b);
        }


        [Theory]
        [InlineData(new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0UL)]
        [InlineData(new byte[8] { 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 255UL)]
        [InlineData(new byte[8] { 0xa0, 0xaf, 0xf4, 0xc9, 0x15, 0xba, 0x3e, 0x87 }, 9745431246421667744UL)]
        [InlineData(new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }, ulong.MaxValue)]
        public void ReadUInt64CheckedTest(byte[] data, ulong expected)
        {
            var stream = new FastStreamReader(data);
            ulong actual = stream.ReadUInt64Checked();

            Assert.Equal(expected, actual);
            Assert.Equal(8, stream.GetCurrentIndex());
        }

        [Theory]
        [InlineData(new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0UL)]
        [InlineData(new byte[8] { 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 255UL)]
        [InlineData(new byte[8] { 0xa0, 0xaf, 0xf4, 0xc9, 0x15, 0xba, 0x3e, 0x87 }, 9745431246421667744UL)]
        [InlineData(new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }, ulong.MaxValue)]
        public void TryReadUInt64Test(byte[] data, ulong expected)
        {
            var stream = new FastStreamReader(data);
            bool b = stream.TryReadUInt64(out ulong actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
            Assert.Equal(8, stream.GetCurrentIndex());
        }

        [Fact]
        public void TryReadUInt64_FailTest()
        {
            var stream = new FastStreamReader(new byte[7]);
            bool b = stream.TryReadUInt64(out ulong _);
            Assert.False(b);
        }

        [Theory]
        [InlineData(new byte[] { }, false, 0, 0)]
        [InlineData(new byte[] { 0, 255, 255 }, true, 0, 1)]
        [InlineData(new byte[] { 1, 255, 255 }, true, 1, 1)]
        [InlineData(new byte[] { 127, 255, 255 }, true, 127, 1)] // 127=0b01111111
        [InlineData(new byte[] { 128, 255, 255 }, true, 128, 1)] // 128=0b10000000
        [InlineData(new byte[] { 129, 5, 255, 255 }, true, 5, 2)] // 129=0b10000001
        [InlineData(new byte[] { 129, 129, 255, 255 }, true, 129, 2)]
        [InlineData(new byte[] { 130, 0, 2, 255 }, true, 2, 3)] // 130=0b10000010
        [InlineData(new byte[] { 130, 26, 50, 255 }, true, 6706, 3)]
        [InlineData(new byte[] { 131, 82, 13, 10, 255 }, true, 5377290, 4)] // 131=0b10000011
        public void TryReadDerLengthTest(byte[] data, bool success, int expected, int expPos)
        {
            var stream = new FastStreamReader(data);
            bool b = stream.TryReadDerLength(out int actual);

            Assert.Equal(success, b);
            Assert.Equal(expected, actual);
            Helper.ComparePrivateField(stream, "position", expPos);
        }
    }
}
