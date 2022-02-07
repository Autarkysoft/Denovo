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
            ReturnOp op = new();

            Assert.Equal(OP.RETURN, op.OpValue);
            Helper.ComparePrivateField(op, "data", new byte[1] { 0x6a });
        }

        [Theory]
        [InlineData(null, new byte[] { 0x6a })]
        [InlineData(new byte[0], new byte[] { 0x6a })]
        [InlineData(new byte[] { 1 }, new byte[] { 0x6a, 1, 1 })]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 0x6a, 3, 1, 2, 3 })]
        public void Constructor_DataWithSizeTest(byte[] ba, byte[] expected)
        {
            ReturnOp op = new(ba, true);
            Helper.ComparePrivateField(op, "data", expected);
        }

        [Theory]
        [InlineData(null, new byte[] { 0x6a })]
        [InlineData(new byte[0], new byte[] { 0x6a })]
        [InlineData(new byte[] { 1 }, new byte[] { 0x6a, 1 })]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 0x6a, 1, 2, 3 })]
        public void Constructor_DataNoSizeTest(byte[] ba, byte[] expected)
        {
            ReturnOp op = new(ba, false);
            Helper.ComparePrivateField(op, "data", expected);
        }

        [Fact]
        public void Constructor_ScriptWithSizeTest()
        {
            MockSerializableScript scr = new(new byte[] { 100, 200 }, 255);
            ReturnOp op = new(scr, true);
            byte[] expected = new byte[] { 0x6a, 2, 100, 200 };

            Helper.ComparePrivateField(op, "data", expected);
        }

        [Fact]
        public void Constructor_ScriptNoSizeTest()
        {
            MockSerializableScript scr = new(new byte[] { 100, 200 }, 255);
            ReturnOp op = new(scr, false);
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
            ReturnOp op = new();
            FastStreamReader stream = new(ba);
            bool b = op.TryRead(stream, len, out Errors err);

            Assert.True(b, err.Convert());
            Assert.Equal(Errors.None, err);
            Helper.ComparePrivateField(op, "data", expData);
        }

        [Theory]
        [InlineData(new byte[0], 1, Errors.EndOfStream)]
        [InlineData(new byte[] { 0x6a }, 0, Errors.ShortOpReturn)]
        [InlineData(new byte[] { 0x6a }, -1, Errors.ShortOpReturn)]
        [InlineData(new byte[] { 0x6b }, 0, Errors.WrongOpReturnByte)]
        [InlineData(new byte[] { 0x6a }, 2, Errors.EndOfStream)]
        public void TryRead_FailTest(byte[] ba, int len, Errors expErr)
        {
            ReturnOp op = new();
            FastStreamReader stream = new(ba);
            bool b = op.TryRead(stream, len, out Errors err);

            Assert.False(b);
            Assert.Equal(expErr, err);
        }

        [Fact]
        public void TryRead_NullStreamFailTest()
        {
            ReturnOp op = new();
            FastStreamReader stream = null;
            bool b = op.TryRead(stream, 3, out Errors err);

            Assert.False(b);
            Assert.Equal(Errors.NullStream, err);
        }


        [Theory]
        [InlineData(null, new byte[] { 0x6a })]
        [InlineData(new byte[0], new byte[] { 0x6a })]
        [InlineData(new byte[] { 5 }, new byte[] { 0x6a, 5 })]
        [InlineData(new byte[] { 1, 2, 3 }, new byte[] { 0x6a, 1, 2, 3 })]
        public void WriteToStreamTest(byte[] ba, byte[] expected)
        {
            ReturnOp op = new(ba, false);
            FastStream stream = new();

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
            ReturnOp r1 = new(new byte[] { 1, 2 });
            ReturnOp r2 = new(new byte[] { 1, 2 });
            ReturnOp r3 = new(new byte[] { 1, 2, 3 });

            int h1 = r1.GetHashCode();
            int h2 = r2.GetHashCode();
            int h3 = r3.GetHashCode();

            Assert.Equal(h1, h2);
            Assert.NotEqual(h1, h3);
        }
    }
}
