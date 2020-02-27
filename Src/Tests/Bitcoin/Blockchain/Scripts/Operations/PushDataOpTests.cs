// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class PushDataOpTests
    {
        [Fact]
        public void Constructor_DefaultTest()
        {
            PushDataOp op = new PushDataOp();
            FastStream stream = new FastStream();
            op.WriteToStream(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[1] { 0 };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_FromBytesTest()
        {
            byte[] data = { 1, 2, 3 };
            PushDataOp op = new PushDataOp(data);

            Helper.ComparePrivateField(op, "data", data);
            Assert.Equal(3, (byte)op.OpValue);
        }

        [Fact]
        public void Constructor_FromBytes_NullExceptionTest()
        {
            byte[] data = null;
            Assert.Throws<ArgumentNullException>(() => new PushDataOp(data));
        }

        [Fact]
        public void Constructor_FromBytes_ArgumentExceptionTest()
        {
            byte[] data = { 1 };
            Assert.Throws<ArgumentException>(() => new PushDataOp(data));
        }


        [Fact]
        public void Constructor_FromScriptTest()
        {
            byte[] data = { 1, 2, 3 };
            MockSerializableScript scr = new MockSerializableScript(data, 255);
            PushDataOp op = new PushDataOp(scr);

            Helper.ComparePrivateField(op, "data", data);
            Assert.Equal(3, (byte)op.OpValue);
        }

        [Fact]
        public void Constructor_FromScript_ExceptionTest()
        {
            IScript scr = null;
            Assert.Throws<ArgumentNullException>(() => new PushDataOp(scr));
        }


        [Theory]
        [InlineData(OP._0)]
        [InlineData(OP.Negative1)]
        [InlineData(OP._1)]
        [InlineData(OP._2)]
        [InlineData(OP._3)]
        [InlineData(OP._4)]
        [InlineData(OP._5)]
        [InlineData(OP._6)]
        [InlineData(OP._7)]
        [InlineData(OP._8)]
        [InlineData(OP._9)]
        [InlineData(OP._10)]
        [InlineData(OP._11)]
        [InlineData(OP._12)]
        [InlineData(OP._13)]
        [InlineData(OP._14)]
        [InlineData(OP._15)]
        [InlineData(OP._16)]
        public void Constructor_FromOpNumTest(OP val)
        {
            PushDataOp op = new PushDataOp(val);
            Assert.Equal(val, op.OpValue);
        }

        [Theory]
        [InlineData((OP)1)]
        [InlineData((OP)0x4b)]
        [InlineData(OP.PushData1)]
        [InlineData(OP.PushData2)]
        [InlineData(OP.PushData4)]
        [InlineData(OP.Reserved)]
        [InlineData(OP.NOP)]
        public void Constructor_FromOpNum_ExceptionTest(OP val)
        {
            Assert.Throws<ArgumentException>(() => new PushDataOp(val));
        }


        [Theory]
        [InlineData(0, OP._0)]
        [InlineData(-1, OP.Negative1)]
        [InlineData(1, OP._1)]
        [InlineData(2, OP._2)]
        [InlineData(3, OP._3)]
        [InlineData(4, OP._4)]
        [InlineData(5, OP._5)]
        [InlineData(6, OP._6)]
        [InlineData(7, OP._7)]
        [InlineData(8, OP._8)]
        [InlineData(9, OP._9)]
        [InlineData(10, OP._10)]
        [InlineData(11, OP._11)]
        [InlineData(12, OP._12)]
        [InlineData(13, OP._13)]
        [InlineData(14, OP._14)]
        [InlineData(15, OP._15)]
        [InlineData(16, OP._16)]
        public void Constructor_FromInt_HasOpNum_Test(int i, OP expected)
        {
            PushDataOp op = new PushDataOp(i);
            Assert.Equal(expected, op.OpValue);
        }

        [Theory]
        [InlineData(17, new byte[] { 17 }, (OP)1)]
        [InlineData(75, new byte[] { 75 }, (OP)1)]
        [InlineData(128, new byte[] { 128, 0 }, (OP)2)]
        [InlineData(256, new byte[] { 0, 1 }, (OP)2)]
        [InlineData(-2, new byte[] { 0b10000010 }, (OP)1)] // 0b10000010 = 0x82 = 130
        [InlineData(-8388607, new byte[] { 255, 255, 255 }, (OP)3)]
        public void Constructor_FromInt_Test(int i, byte[] expectedBa, OP expectedOP)
        {
            PushDataOp op = new PushDataOp(i);

            Helper.ComparePrivateField(op, "data", expectedBa);
            Assert.Equal(expectedOP, op.OpValue);
        }


        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[] { new PushDataOp(0), new byte[0] };
            yield return new object[] { new PushDataOp(-1), new byte[] { 0b10000001 } };
            yield return new object[] { new PushDataOp(1), new byte[] { 1 } };
            yield return new object[] { new PushDataOp(2), new byte[] { 2 } };
            yield return new object[] { new PushDataOp(16), new byte[] { 16 } };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2, 3 }), new byte[] { 1, 2, 3 } };
        }
        [Theory]
        [MemberData(nameof(GetRunCases))]
        public void RunTest(PushDataOp op, byte[] expectedData)
        {
            MockOpData opData = new MockOpData(FuncCallName.Push)
            {
                pushData = new byte[][] { expectedData }
            };

            bool b = op.Run(opData, out string error);

            // The mock data is already checking the call type and the data that was pushed to be correct.
            Assert.True(b);
            Assert.Null(error);
        }


        [Theory]
        [InlineData(new byte[] { 17 }, 17)]
        [InlineData(new byte[] { 128, 128 }, -128)]
        public void TryGetNumberTest(byte[] data, long expected)
        {
            PushDataOp op = new PushDataOp(data);

            bool b = op.TryGetNumber(out long actual, out string error);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(OP._0, 0)]
        [InlineData(OP.Negative1, -1)]
        [InlineData(OP._1, 1)]
        [InlineData(OP._2, 2)]
        [InlineData(OP._16, 16)]
        public void TryGetNumber_NullDataTest(OP val, long expected)
        {
            PushDataOp op = new PushDataOp(val);

            bool b = op.TryGetNumber(out long actual, out string error);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryGetNumber_FailTest()
        {
            PushDataOp op = new PushDataOp(Helper.GetBytes(9));

            bool b = op.TryGetNumber(out long actual, out string error);

            Assert.False(b);
            Assert.Equal("Invalid number format.", error);
            Assert.Equal(0, actual);
        }

        [Fact]
        public void TryGetNumber_SpecialCaseTest()
        {
            PushDataOp op = new PushDataOp();
            FastStreamReader stream = new FastStreamReader(new byte[2] { 1, 2 });
            bool didRead = op.TryRead(stream, out string error, false);

            Assert.True(didRead, error);
            Assert.Null(error);

            // The value is 2 but didn't use OP_2, instead it used byte[] { 2 }
            bool b = op.TryGetNumber(out long actual, out error, false);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(2, actual);
        }


        public static TheoryData GetReadCases()
        {
            // 0x4c4d.... => script (0x4c4d = StackInt len) len-value=77
            // 0x4c4d.... => witness (0x4c = CompactInt len) len-value=76
            byte[] data77 = Helper.GetBytes(2 + 77);
            data77[0] = 76; // set StackInt(77) first byte will also be CompactInt(76)
            data77[1] = 77;
            byte[] exp77 = new byte[77];
            Buffer.BlockCopy(data77, 2, exp77, 0, 77);
            byte[] expWit77 = new byte[76];
            Buffer.BlockCopy(data77, 1, expWit77, 0, 76);

            return new TheoryData<byte[], bool, byte[], OP>()
            {
                { new byte[1], true, null, OP._0 },
                { new byte[1], false, null, OP._0 },
                { new byte[] { 0x4f }, true, null, OP.Negative1 },
                { new byte[] { 0x4f }, false, null, OP.Negative1 },
                { new byte[] { 0x51 },  true, null, OP._1 },
                { new byte[] { 0x51, 2 }, false, null, OP._1 },
                { new byte[] { 0x60 }, true, null, OP._16 },
                { new byte[] { 0x60 }, false, null, OP._16 },

                { new byte[] { 1, 0 }, true, new byte[1], (OP)1 }, // not strict
                { new byte[] { 1, 0 },  false,  new byte[1], (OP)1 }, // not strict
                { new byte[] { 1, 5 }, true, new byte[1] { 5 }, (OP)1 }, // not strict
                { new byte[] { 1, 3, 4 }, false, new byte[1] { 3 }, (OP)1 }, // not strict

                { data77, true, expWit77, (OP)0x4c }, //0x4c=OP.PushData1=76
                { data77, false, exp77, OP.PushData1 },

            };
        }
        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryReadTest(byte[] data, bool isWit, byte[] expData, OP expOP)
        {
            PushDataOp op = new PushDataOp();
            FastStreamReader stream = new FastStreamReader(data);
            bool b = op.TryRead(stream, out string error, isWit);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(expOP, op.OpValue);
            if (expData != null)
            {
                Helper.ComparePrivateField(op, "data", expData);
            }
        }

        [Theory]
        [InlineData(new byte[0], Err.EndOfStream)]
        public void TryRead_FailTest(byte[] ba, string expErr)
        {
            PushDataOp op = new PushDataOp();
            FastStreamReader stream = new FastStreamReader(ba);
            bool b = op.TryRead(stream, out string error, false);

            Assert.False(b);
            Assert.Equal(expErr, error);
        }

        [Fact]
        public void TryRead_FailNullStreamTest()
        {
            PushDataOp op = new PushDataOp();
            FastStreamReader stream = null;
            bool b = op.TryRead(stream, out string error, true);

            Assert.False(b);
            Assert.Equal("Stream can not be null.", error);
        }


        public static TheoryData WriteToStreamCases()
        {
            byte[] data75 = Helper.GetBytes(75);
            byte[] exp75 = new byte[76];
            exp75[0] = 75;
            Buffer.BlockCopy(data75, 0, exp75, 1, data75.Length);
            byte[] expWit75 = new byte[76];
            expWit75[0] = 75;
            Buffer.BlockCopy(data75, 0, expWit75, 1, data75.Length);

            byte[] data76 = Helper.GetBytes(76);
            byte[] exp76 = new byte[78];
            exp76[0] = 76;
            exp76[1] = 76;
            Buffer.BlockCopy(data76, 0, exp76, 2, data76.Length);
            byte[] expWit76 = new byte[77];
            expWit76[0] = 76;
            Buffer.BlockCopy(data76, 0, expWit76, 1, data76.Length);

            byte[] data253 = Helper.GetBytes(253);
            byte[] exp253 = new byte[255]; // 253 = 0x4cfd <-- StackInt
            exp253[0] = 0x4c;
            exp253[1] = 0xfd;
            Buffer.BlockCopy(data253, 0, exp253, 2, data253.Length);
            byte[] expWit253 = new byte[256]; // 253 = 0xfdfd00 <-- CompactInt
            expWit253[0] = 0xfd;
            expWit253[1] = 0xfd;
            expWit253[2] = 0;
            Buffer.BlockCopy(data253, 0, expWit253, 3, data253.Length);

            return new TheoryData<PushDataOp, byte[], byte[]>()
            {
                { new PushDataOp(-1), new byte[1] { 0x4f }, new byte[1] { 0x4f } },
                { new PushDataOp(0), new byte[1] { 0 }, new byte[1] { 0 } },
                { new PushDataOp(1), new byte[1] { 0x51 }, new byte[1] { 0x51 } },
                { new PushDataOp(2), new byte[1] { 0x52 }, new byte[1] { 0x52 } },
                { new PushDataOp(16), new byte[1] { 0x60 }, new byte[1] { 0x60 } },
                { new PushDataOp(17), new byte[2] { 1, 17 }, new byte[2] { 1, 17 } },
                { new PushDataOp(data75), exp75, expWit75 },
                { new PushDataOp(data76), exp76, expWit76 },
                { new PushDataOp(data253), exp253, expWit253 },
            };
        }
        [Theory]
        [MemberData(nameof(WriteToStreamCases))]
        public void WriteToStreamTest(PushDataOp op, byte[] expected, byte[] expectedWit)
        {
            FastStream actualStream = new FastStream();
            FastStream actualStreamWit = new FastStream();
            op.WriteToStream(actualStream, false);
            op.WriteToStream(actualStreamWit, true);

            byte[] actual = actualStream.ToByteArray();
            byte[] actualWit = actualStreamWit.ToByteArray();

            Assert.Equal(expected, actual);
            Assert.Equal(expectedWit, actualWit);
        }


        public static IEnumerable<object[]> GetEqualCases()
        {
            yield return new object[] { new PushDataOp(1), new PushDataOp(1), true };
            yield return new object[] { new PushDataOp(1), new PushDataOp(0), false };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2, 3 }), new PushDataOp(0), false };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2, 3 }), new PushDataOp(new byte[] { 1, 2, 3 }), true };
            yield return new object[] { new PushDataOp(new byte[] { 1, 4, 3 }), new PushDataOp(new byte[] { 1, 2, 3 }), false };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2 }), new PushDataOp(new byte[] { 1, 2, 3 }), false };
        }
        [Theory]
        [MemberData(nameof(GetEqualCases))]
        public void EqualsTest(PushDataOp op1, PushDataOp op2, bool expected)
        {
            bool actual = op1.Equals(op2);
            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetHashCodeCases()
        {
            yield return new object[] { new PushDataOp(1), OP._1.GetHashCode() };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2, 3 }), 507473 };
        }
        [Theory]
        [MemberData(nameof(GetHashCodeCases))]
        public void GetHashCodeTest(PushDataOp op, int expected)
        {
            int actual = op.GetHashCode();
            Assert.Equal(expected, actual);
        }
    }
}
