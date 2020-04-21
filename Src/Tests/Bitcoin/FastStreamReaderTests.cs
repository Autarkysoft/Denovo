// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using Xunit;

namespace Tests.Bitcoin
{
    public class FastStreamReaderTests
    {
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
    }
}
