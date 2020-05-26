// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class ReturnOpTests
    {
        [Fact]
        public void Constructor_EmptyTest()
        {
            ReturnOp op = new ReturnOp();

            Assert.Equal(OP.RETURN, op.OpValue);
            Helper.ComparePrivateField<ReturnOp, byte[]>(op, "data", new byte[1] { 0x6a });
        }

        [Theory]
        [InlineData(null, new byte[] { 0x6a })]
        [InlineData(new byte[0], new byte[] { 0x6a })]
        [InlineData(new byte[] { 1 }, new byte[] { 0x6a, 1, 1 })]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 0x6a, 3, 1, 2, 3 })]
        public void Constructor_DataWithSizeTest(byte[] ba, byte[] expected)
        {
            ReturnOp op = new ReturnOp(ba, true);
            Helper.ComparePrivateField(op, "data", expected);
        }

        [Theory]
        [InlineData(null, new byte[] { 0x6a })]
        [InlineData(new byte[0], new byte[] { 0x6a })]
        [InlineData(new byte[] { 1 }, new byte[] { 0x6a, 1 })]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 0x6a, 1, 2, 3 })]
        public void Constructor_DataNoSizeTest(byte[] ba, byte[] expected)
        {
            ReturnOp op = new ReturnOp(ba, false);
            Helper.ComparePrivateField(op, "data", expected);
        }

        [Fact]
        public void Constructor_ScriptWithSizeTest()
        {
            var scr = new MockSerializableScript(new byte[] { 100, 200 }, 255);
            ReturnOp op = new ReturnOp(scr, true);
            byte[] expected = new byte[] { 0x6a, 2, 100, 200 };

            Helper.ComparePrivateField(op, "data", expected);
        }

        [Fact]
        public void Constructor_ScriptNoSizeTest()
        {
            var scr = new MockSerializableScript(new byte[] { 100, 200 }, 255);
            ReturnOp op = new ReturnOp(scr, false);
            byte[] expected = new byte[] { 0x6a, 100, 200 };

            Helper.ComparePrivateField(op, "data", expected);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            IScript scr = null;
            Assert.Throws<ArgumentNullException>(() => new ReturnOp(scr, false));
        }


        [Theory]
        [InlineData(new byte[] { 0x6a }, 1, new byte[] { 0x6a })]
        [InlineData(new byte[] { 0x6a, 1 }, 1, new byte[] { 0x6a })]
        [InlineData(new byte[] { 0x6a, 1, 2, 3 }, 3, new byte[] { 0x6a, 1, 2 })]
        public void TryReadTest(byte[] ba, int len, byte[] expData)
        {
            ReturnOp op = new ReturnOp();
            FastStreamReader stream = new FastStreamReader(ba);
            bool b = op.TryRead(stream, len, out string err);

            Assert.True(b);
            Assert.Null(err);
            Helper.ComparePrivateField(op, "data", expData);
        }

        [Theory]
        [InlineData(new byte[0], 1, Err.EndOfStream)]
        [InlineData(new byte[] { 0x6a }, 0, "OP_RETURN script length must be at least 1 byte.")]
        [InlineData(new byte[] { 0x6a }, -1, "OP_RETURN script length must be at least 1 byte.")]
        [InlineData(new byte[] { 0x6b }, 0, "Stream doesn't start with appropriate (OP_Return) byte.")]
        [InlineData(new byte[] { 0x6a }, 2, Err.EndOfStream)]
        public void TryRead_FailTest(byte[] ba, int len, string expErr)
        {
            ReturnOp op = new ReturnOp();
            FastStreamReader stream = new FastStreamReader(ba);
            bool b = op.TryRead(stream, len, out string err);

            Assert.False(b);
            Assert.Equal(expErr, err);
        }

        [Fact]
        public void TryRead_NullStreamFailTest()
        {
            ReturnOp op = new ReturnOp();
            FastStreamReader stream = null;
            bool b = op.TryRead(stream, 3, out string err);

            Assert.False(b);
            Assert.Equal("Stream can not be null.", err);
        }


        [Theory]
        [InlineData(null, new byte[] { 0x6a })]
        [InlineData(new byte[0], new byte[] { 0x6a })]
        [InlineData(new byte[] { 5 }, new byte[] { 0x6a, 5 })]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 0x6a, 1, 2, 3 })]
        public void WriteToStreamTest(byte[] ba, byte[] expected)
        {
            ReturnOp op = new ReturnOp(ba, false);
            FastStream stream = new FastStream();

            op.WriteToStream(stream);
            byte[] actual = stream.ToByteArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EqualsTest()
        {
            IOperation r1_1 = new ReturnOp();
            IOperation r1_2 = new ReturnOp();
            IOperation r2_1 = new ReturnOp(new byte[] { 1, 2 }, false);
            IOperation r2_2 = new ReturnOp(new byte[] { 1, 2 }, false);
            IOperation r2_3 = new ReturnOp(new byte[] { 1, 2 }, true);
            IOperation r2_4 = new PushDataOp(new byte[] { 1, 2 });
            string diff = "ReturnOp";

            Assert.True(r1_1.Equals(r1_1));
            Assert.True(r1_1.Equals(r1_2));
            Assert.False(r1_1.Equals(r2_1));

            Assert.True(r2_1.Equals(r2_1));
            Assert.True(r2_1.Equals(r2_2));
            Assert.False(r2_1.Equals(r2_3));
            Assert.False(r2_1.Equals(r2_4));
            Assert.False(r2_1.Equals(diff));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            var r1 = new ReturnOp(new byte[] { 1, 2 });
            var r2 = new ReturnOp(new byte[] { 1, 2 });
            var r3 = new ReturnOp(new byte[] { 1, 2, 3 });

            int h1 = r1.GetHashCode();
            int h2 = r2.GetHashCode();
            int h3 = r3.GetHashCode();

            Assert.Equal(h1, h2);
            Assert.NotEqual(h1, h3);
        }
    }
}
