// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using Xunit;

namespace Tests.Bitcoin
{
    public class FastStreamTests
    {
        private const int Capacity = 100;

        [Fact]
        public void ConstructorTest()
        {
            FastStream stream = new FastStream();
            byte[] expected = new byte[Capacity];

            Helper.ComparePrivateField(stream, "buffer", expected);
            Helper.ComparePrivateField(stream, "position", 0);
            Assert.Equal(new byte[0], stream.ToByteArray());
        }

        [Theory]
        [InlineData(-1, Capacity)]
        [InlineData(0, Capacity)]
        [InlineData(1, Capacity)]
        [InlineData(Capacity, Capacity)]
        [InlineData(Capacity + 1, Capacity + 1)]
        public void Constructor_CapactiyTest(int cap, int expLen)
        {
            FastStream stream = new FastStream(cap);
            byte[] expBuffer = new byte[expLen];

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 0);
            Assert.Equal(new byte[0], stream.ToByteArray());
        }


        [Fact]
        public void GetSizeTest()
        {
            FastStream stream = new FastStream();
            int s1 = stream.GetSize();
            stream.Write((byte)1);
            int s2 = stream.GetSize();
            stream.Write(1);
            int s3 = stream.GetSize();

            Assert.Equal(0, s1);
            Assert.Equal(1, s2);
            Assert.Equal(5, s3);
        }

        [Fact]
        public void ToByteArrayTest()
        {
            FastStream stream = new FastStream();
            byte[] ba1 = stream.ToByteArray();
            stream.Write((byte)1);
            byte[] ba2 = stream.ToByteArray();
            stream.Write(2);
            byte[] ba3 = stream.ToByteArray();

            Assert.Equal(new byte[0], ba1);
            Assert.Equal(new byte[1] { 1 }, ba2);
            Assert.Equal(new byte[5] { 1, 2, 0, 0, 0 }, ba3);
        }


        [Theory]
        [InlineData(0, new byte[4] { 0, 0, 0, 0 })]
        [InlineData(-1, new byte[4] { 0xff, 0xff, 0xff, 0xff })]
        [InlineData(184104331, new byte[4] { 0x8b, 0x35, 0xf9, 0x0a })]
        [InlineData(int.MaxValue, new byte[4] { 0xff, 0xff, 0xff, 0x7f })]
        [InlineData(int.MinValue, new byte[4] { 0x00, 0x00, 0x00, 0x80 })]
        public void Write_intTest(int val, byte[] expected)
        {
            FastStream stream = new FastStream();
            stream.Write(val);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 4);
        }

        [Theory]
        [InlineData(0, new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(-1, new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff })]
        [InlineData(7101930557053599503, new byte[8] { 0x0f, 0xff, 0xab, 0xc8, 0x36, 0x20, 0x8f, 0x62 })]
        [InlineData(long.MaxValue, new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f })]
        [InlineData(long.MinValue, new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 })]
        public void Write_longTest(long val, byte[] expected)
        {
            FastStream stream = new FastStream();
            stream.Write(val);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 8);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(255)]
        public void Write_byteTest(byte val)
        {
            FastStream stream = new FastStream();
            stream.Write(val);

            byte[] expBuffer = new byte[Capacity];
            expBuffer[0] = val;

            Assert.Equal(new byte[1] { val }, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 1);
        }

        [Theory]
        [InlineData(0, new byte[2] { 0, 0 })]
        [InlineData(31534, new byte[2] { 0x2e, 0x7b })]
        [InlineData(ushort.MaxValue, new byte[2] { 0xff, 0xff })]
        public void Write_ushortTest(ushort val, byte[] expected)
        {
            FastStream stream = new FastStream();
            stream.Write(val);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 2);
        }

        [Theory]
        [InlineData(0, new byte[4] { 0, 0, 0, 0 })]
        [InlineData(1274051374U, new byte[4] { 0x2e, 0x7b, 0xf0, 0x4b })]
        [InlineData(uint.MaxValue, new byte[4] { 0xff, 0xff, 0xff, 0xff })]
        public void Write_uintTest(uint val, byte[] expected)
        {
            FastStream stream = new FastStream();
            stream.Write(val);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 4);
        }

        [Theory]
        [InlineData(0, new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(18387565198080768814UL, new byte[8] { 0x2e, 0x7b, 0xf0, 0x4b, 0x20, 0xc1, 0x2d, 0xff })]
        [InlineData(ulong.MaxValue, new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff })]
        public void Write_ulongTest(ulong val, byte[] expected)
        {
            FastStream stream = new FastStream();
            stream.Write(val);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 8);
        }

        [Theory]
        [InlineData(new byte[] { })]
        [InlineData(new byte[1] { 1 })]
        [InlineData(new byte[3] { 1, 2, 3 })]
        public void Write_bytes_smallTest(byte[] data)
        {
            FastStream stream = new FastStream();
            stream.Write(data);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(data, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", data.Length);
        }

        [Fact]
        public void Write_bytes_bigTest()
        {
            FastStream stream = new FastStream();
            byte[] data = Helper.GetBytes(Capacity + 1);
            stream.Write(data);

            byte[] expBuffer = new byte[Capacity * 2];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(data, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", data.Length);
        }

        [Fact]
        public void Write_bytes_withLenTest()
        {
            FastStream stream = new FastStream();
            byte[] data = { 1, 2, 3 };
            int len = 5;
            stream.Write(data, len);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);
            byte[] expBytes = new byte[5] { 1, 2, 3, 0, 0 };

            Assert.Equal(expBytes, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", len);
        }

        [Fact]
        public void Write_streamTest()
        {
            FastStream stream = new FastStream();
            FastStream toWrite = new FastStream();
            toWrite.Write(new byte[] { 1, 2, 3 });
            stream.Write(toWrite);

            byte[] expBuffer = new byte[Capacity];
            expBuffer[0] = 1;
            expBuffer[1] = 2;
            expBuffer[2] = 3;

            Assert.Equal(new byte[3] { 1, 2, 3 }, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 3);
        }

        [Theory]
        [InlineData(0, new byte[2] { 0, 0 })]
        [InlineData(31534, new byte[2] { 0x7b, 0x2e })]
        [InlineData(ushort.MaxValue, new byte[2] { 0xff, 0xff })]
        public void Write_ushort_BETest(ushort val, byte[] expected)
        {
            FastStream stream = new FastStream();
            stream.WriteBigEndian(val);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 2);
        }

        [Theory]
        [InlineData(0, new byte[4] { 0, 0, 0, 0 })]
        [InlineData(1274051374U, new byte[4] { 0x4b, 0xf0, 0x7b, 0x2e })]
        [InlineData(uint.MaxValue, new byte[4] { 0xff, 0xff, 0xff, 0xff })]
        public void Write_uint_BETest(uint val, byte[] expected)
        {
            FastStream stream = new FastStream();
            stream.WriteBigEndian(val);

            byte[] expBuffer = new byte[Capacity];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 4);
        }

        [Fact]
        public void Resize_smallTest()
        {
            FastStream stream = new FastStream();
            int dataLen = Capacity + 1;
            byte[] data = Helper.GetBytes(dataLen);
            stream.Write(data);

            int actualSize = stream.GetSize();
            int expectedSize = dataLen;
            byte[] expBuffer = new byte[Capacity + Capacity];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(expectedSize, actualSize);
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
        }

        [Fact]
        public void Resize_bigTest()
        {
            FastStream stream = new FastStream();
            int dataLen = Capacity + (Capacity + 5);
            byte[] data = Helper.GetBytes(dataLen);
            stream.Write(data);

            int actualSize = stream.GetSize();
            int expectedSize = dataLen;
            byte[] expBuffer = new byte[Capacity + dataLen];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(expectedSize, actualSize);
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
        }
    }
}
