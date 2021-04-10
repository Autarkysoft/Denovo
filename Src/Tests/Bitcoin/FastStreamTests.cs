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
    public class FastStreamTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var stream = new FastStream();
            byte[] expected = new byte[FastStream.DefaultCapacity];

            Helper.ComparePrivateField(stream, "buffer", expected);
            Helper.ComparePrivateField(stream, "position", 0);
            Assert.Equal(Array.Empty<byte>(), stream.ToByteArray());
        }

        [Theory]
        [InlineData(-1, FastStream.DefaultCapacity)]
        [InlineData(0, FastStream.DefaultCapacity)]
        [InlineData(1, 1)]
        [InlineData(FastStream.DefaultCapacity, FastStream.DefaultCapacity)]
        [InlineData(FastStream.DefaultCapacity + 1, FastStream.DefaultCapacity + 1)]
        public void Constructor_CapactiyTest(int cap, int expLen)
        {
            var stream = new FastStream(cap);
            byte[] expBuffer = new byte[expLen];

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 0);
            Assert.Equal(Array.Empty<byte>(), stream.ToByteArray());
        }


        [Fact]
        public void GetSizeTest()
        {
            var stream = new FastStream();
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
            var stream = new FastStream();
            byte[] ba1 = stream.ToByteArray();
            stream.Write((byte)1);
            byte[] ba2 = stream.ToByteArray();
            stream.Write(2);
            byte[] ba3 = stream.ToByteArray();

            Assert.Equal(Array.Empty<byte>(), ba1);
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
            var stream = new FastStream(10);
            stream.Write(val);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 4);
        }

        [Fact]
        public void Write_int_ResizeTest()
        {
            var stream = new FastStream(1);

            Helper.ComparePrivateField(stream, "buffer", new byte[1]);

            stream.Write(184104331);
            byte[] expBuffer = new byte[1 + FastStream.DefaultCapacity];
            expBuffer[0] = 0x8b;
            expBuffer[1] = 0x35;
            expBuffer[2] = 0xf9;
            expBuffer[3] = 0x0a;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", sizeof(int));
        }

        [Theory]
        [InlineData(0, new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(-1, new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff })]
        [InlineData(7101930557053599503, new byte[8] { 0x0f, 0xff, 0xab, 0xc8, 0x36, 0x20, 0x8f, 0x62 })]
        [InlineData(long.MaxValue, new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f })]
        [InlineData(long.MinValue, new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 })]
        public void Write_longTest(long val, byte[] expected)
        {
            var stream = new FastStream(10);
            stream.Write(val);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 8);
        }

        [Fact]
        public void Write_long_ResizeTest()
        {
            var stream = new FastStream(2);

            Helper.ComparePrivateField(stream, "buffer", new byte[2]);

            stream.Write(7101930557053599503);
            byte[] expBuffer = new byte[2 + FastStream.DefaultCapacity];
            expBuffer[0] = 0x0f;
            expBuffer[1] = 0xff;
            expBuffer[2] = 0xab;
            expBuffer[3] = 0xc8;
            expBuffer[4] = 0x36;
            expBuffer[5] = 0x20;
            expBuffer[6] = 0x8f;
            expBuffer[7] = 0x62;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", sizeof(long));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(255)]
        public void Write_byteTest(byte val)
        {
            var stream = new FastStream(10);
            stream.Write(val);

            byte[] expBuffer = new byte[10];
            expBuffer[0] = val;

            Assert.Equal(new byte[1] { val }, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 1);
        }

        [Fact]
        public void Write_byte_ResizeTest()
        {
            var stream = new FastStream(1);

            Helper.ComparePrivateField(stream, "buffer", new byte[1]);

            stream.Write((byte)1);
            Helper.ComparePrivateField(stream, "buffer", new byte[1] { 1 });
            stream.Write((byte)2);
            byte[] expBuffer = new byte[1 + FastStream.DefaultCapacity];
            expBuffer[0] = 1;
            expBuffer[1] = 2;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", sizeof(byte) * 2);
        }

        [Theory]
        [InlineData(0, new byte[2] { 0, 0 })]
        [InlineData(31534, new byte[2] { 0x2e, 0x7b })]
        [InlineData(ushort.MaxValue, new byte[2] { 0xff, 0xff })]
        public void Write_ushortTest(ushort val, byte[] expected)
        {
            var stream = new FastStream(10);
            stream.Write(val);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 2);
        }

        [Fact]
        public void Write_ushort_ResizeTest()
        {
            var stream = new FastStream(1);

            Helper.ComparePrivateField(stream, "buffer", new byte[1]);

            stream.Write((ushort)31534);
            byte[] expBuffer = new byte[1 + FastStream.DefaultCapacity];
            expBuffer[0] = 0x2e;
            expBuffer[1] = 0x7b;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", sizeof(ushort));
        }

        [Theory]
        [InlineData(0, new byte[4] { 0, 0, 0, 0 })]
        [InlineData(1274051374U, new byte[4] { 0x2e, 0x7b, 0xf0, 0x4b })]
        [InlineData(uint.MaxValue, new byte[4] { 0xff, 0xff, 0xff, 0xff })]
        public void Write_uintTest(uint val, byte[] expected)
        {
            var stream = new FastStream(10);
            stream.Write(val);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 4);
        }

        [Fact]
        public void Write_uint_ResizeTest()
        {
            var stream = new FastStream(1);

            Helper.ComparePrivateField(stream, "buffer", new byte[1]);

            stream.Write(1274051374U);
            byte[] expBuffer = new byte[1 + FastStream.DefaultCapacity];
            expBuffer[0] = 0x2e;
            expBuffer[1] = 0x7b;
            expBuffer[2] = 0xf0;
            expBuffer[3] = 0x4b;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", sizeof(uint));
        }

        [Theory]
        [InlineData(0, new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 })]
        [InlineData(18387565198080768814UL, new byte[8] { 0x2e, 0x7b, 0xf0, 0x4b, 0x20, 0xc1, 0x2d, 0xff })]
        [InlineData(ulong.MaxValue, new byte[8] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff })]
        public void Write_ulongTest(ulong val, byte[] expected)
        {
            var stream = new FastStream(10);
            stream.Write(val);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 8);
        }

        [Fact]
        public void Write_ulong_ResizeTest()
        {
            var stream = new FastStream(2);

            Helper.ComparePrivateField(stream, "buffer", new byte[2]);

            stream.Write(18387565198080768814UL);
            byte[] expBuffer = new byte[2 + FastStream.DefaultCapacity];
            expBuffer[0] = 0x2e;
            expBuffer[1] = 0x7b;
            expBuffer[2] = 0xf0;
            expBuffer[3] = 0x4b;
            expBuffer[4] = 0x20;
            expBuffer[5] = 0xc1;
            expBuffer[6] = 0x2d;
            expBuffer[7] = 0xff;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", sizeof(ulong));
        }

        [Theory]
        [InlineData(new byte[] { })]
        [InlineData(new byte[1] { 1 })]
        [InlineData(new byte[3] { 1, 2, 3 })]
        public void Write_bytes_smallTest(byte[] data)
        {
            var stream = new FastStream(10);
            stream.Write(data);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(data, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", data.Length);
        }

        [Fact]
        public void Write_bytes_ResizeTest()
        {
            var stream = new FastStream(2);

            Helper.ComparePrivateField(stream, "buffer", new byte[2]);

            stream.Write(new byte[] { 1, 2, 3 });
            byte[] expBuffer = new byte[2 + FastStream.DefaultCapacity];
            expBuffer[0] = 1;
            expBuffer[1] = 2;
            expBuffer[2] = 3;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 3);
        }

        [Fact]
        public void Write_bytes_bigTest()
        {
            var stream = new FastStream(1);
            byte[] data = Helper.GetBytes(FastStream.DefaultCapacity + 10);
            stream.Write(data);

            byte[] expBuffer = data;

            Assert.Equal(data, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", data.Length);
        }

        [Fact]
        public void Write_bytesFromIndex_test()
        {
            var stream = new FastStream(10);
            byte[] data = new byte[] { 1, 2, 3, 4, 5 };
            stream.Write(data, 1, 3);
            byte[] expBuffer = new byte[] { 2, 3, 4 };

            Assert.Equal(expBuffer, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "position", expBuffer.Length);
        }

        [Fact]
        public void Write_bytesFromIndex_ZeroCount_test()
        {
            var stream = new FastStream(10);
            byte[] data = new byte[] { 1, 2, 3, 4, 5 };
            stream.Write(data, 1, 0);

            Assert.Equal(Array.Empty<byte>(), stream.ToByteArray());
            Helper.ComparePrivateField(stream, "position", 0);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2 }, -1, new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2 }, 0, new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2 }, 1, new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2 }, 2, new byte[] { 1, 2 })]
        [InlineData(new byte[] { 1, 2 }, 3, new byte[] { 1, 2, 0 })]
        [InlineData(new byte[] { 1, 2 }, 4, new byte[] { 1, 2, 0, 0 })]
        public void Write_bytes_withPadTest(byte[] data, int pad, byte[] expBytes)
        {
            var stream = new FastStream(10);
            stream.Write(data, pad);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(expBytes, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", expBytes.Length);
        }

        [Fact]
        public void Write_bytes_withPad_ResizeTest()
        {
            var stream = new FastStream(1);
            byte[] data = new byte[] { 1, 2 };
            byte[] expBytes = new byte[] { 1, 2, 0 };
            stream.Write(data, 3);

            byte[] expBuffer = new byte[FastStream.DefaultCapacity + 1];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(expBytes, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", expBytes.Length);
        }

        public static IEnumerable<object[]> GetWriteCompactIntCases()
        {
            var rng = new Random(17);
            byte[] big = new byte[17059];
            byte[] usmax = new byte[ushort.MaxValue];
            byte[] veryBig = new byte[ushort.MaxValue + 1];
            rng.NextBytes(big);
            rng.NextBytes(usmax);
            rng.NextBytes(veryBig);

            yield return new object[] { Array.Empty<byte>(), new byte[1], 1 };
            yield return new object[] { new byte[] { 10 }, new byte[] { 1, 10 }, 2 };
            yield return new object[]
            {
                Helper.GetBytes(252), Helper.ConcatBytes(253, new byte[] { 252 }, Helper.GetBytes(252)), 253
            };
            yield return new object[]
            {
                Helper.GetBytes(253), Helper.ConcatBytes(256, new byte[] { 253, 253, 0 }, Helper.GetBytes(253)), 256
            };
            yield return new object[]
            {
                big, Helper.ConcatBytes(17059 + 3, new byte[] { 253, 163, 66 }, big), 17059 + 3
            };
            yield return new object[]
            {
                usmax, Helper.ConcatBytes(ushort.MaxValue + 3, new byte[] { 253, 255, 255 }, usmax), ushort.MaxValue + 3
            };
            yield return new object[]
            {
                veryBig, Helper.ConcatBytes(veryBig.Length + 5, new byte[] { 254, 0, 0, 1, 0 }, veryBig), veryBig.Length + 5
            };
        }
        [Theory]
        [MemberData(nameof(GetWriteCompactIntCases))]
        public void WriteWithCompactIntLength(byte[] data, byte[] expBuffer, int expPos)
        {
            var stream = new FastStream(expPos);
            stream.WriteWithCompactIntLength(data);

            Assert.Equal(expBuffer, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "position", expPos);
        }

        [Fact]
        public void Write_streamTest()
        {
            var stream = new FastStream(10);
            var toWrite = new FastStream(8);
            toWrite.Write(new byte[] { 1, 2, 3 });
            stream.Write(toWrite);

            byte[] expBuffer = new byte[10];
            expBuffer[0] = 1;
            expBuffer[1] = 2;
            expBuffer[2] = 3;

            Assert.Equal(new byte[3] { 1, 2, 3 }, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 3);
        }

        [Fact]
        public void Write_stream_ResizeTest()
        {
            var stream = new FastStream(2);
            var toWrite = new FastStream(8);
            toWrite.Write(new byte[] { 1, 2, 3 });
            stream.Write(toWrite);

            byte[] expBuffer = new byte[FastStream.DefaultCapacity + 2];
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
            var stream = new FastStream(10);
            stream.WriteBigEndian(val);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 2);
        }

        [Fact]
        public void Write_ushort_BE_ResizeTest()
        {
            var stream = new FastStream(1);

            Helper.ComparePrivateField(stream, "buffer", new byte[1]);
            ushort value = 31534;
            stream.WriteBigEndian(value);
            byte[] expBuffer = new byte[1 + FastStream.DefaultCapacity];
            expBuffer[0] = 0x7b;
            expBuffer[1] = 0x2e;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", sizeof(ushort));
        }

        [Theory]
        [InlineData(0, new byte[4] { 0, 0, 0, 0 })]
        [InlineData(1274051374U, new byte[4] { 0x4b, 0xf0, 0x7b, 0x2e })]
        [InlineData(uint.MaxValue, new byte[4] { 0xff, 0xff, 0xff, 0xff })]
        public void Write_uint_BETest(uint val, byte[] expected)
        {
            var stream = new FastStream(10);
            stream.WriteBigEndian(val);

            byte[] expBuffer = new byte[10];
            Buffer.BlockCopy(expected, 0, expBuffer, 0, expected.Length);

            Assert.Equal(expected, stream.ToByteArray());
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", 4);
        }

        [Fact]
        public void Write_uint_BE_ResizeTest()
        {
            var stream = new FastStream(1);

            Helper.ComparePrivateField(stream, "buffer", new byte[1]);
            uint value = 1274051374U;
            stream.WriteBigEndian(value);
            byte[] expBuffer = new byte[1 + FastStream.DefaultCapacity];
            expBuffer[0] = 0x4b;
            expBuffer[1] = 0xf0;
            expBuffer[2] = 0x7b;
            expBuffer[3] = 0x2e;

            Helper.ComparePrivateField(stream, "buffer", expBuffer);
            Helper.ComparePrivateField(stream, "position", sizeof(uint));
        }

        [Fact]
        public void Resize_smallTest()
        {
            var stream = new FastStream();
            int dataLen = FastStream.DefaultCapacity + 1;
            byte[] data = Helper.GetBytes(dataLen);
            stream.Write(data);

            int actualSize = stream.GetSize();
            int expectedSize = dataLen;
            byte[] expBuffer = new byte[FastStream.DefaultCapacity + FastStream.DefaultCapacity];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(expectedSize, actualSize);
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
        }

        [Fact]
        public void Resize_bigTest()
        {
            var stream = new FastStream();
            int dataLen = FastStream.DefaultCapacity + (FastStream.DefaultCapacity + 5);
            byte[] data = Helper.GetBytes(dataLen);
            stream.Write(data);

            int actualSize = stream.GetSize();
            int expectedSize = dataLen;
            byte[] expBuffer = new byte[dataLen];
            Buffer.BlockCopy(data, 0, expBuffer, 0, data.Length);

            Assert.Equal(expectedSize, actualSize);
            Helper.ComparePrivateField(stream, "buffer", expBuffer);
        }
    }
}
