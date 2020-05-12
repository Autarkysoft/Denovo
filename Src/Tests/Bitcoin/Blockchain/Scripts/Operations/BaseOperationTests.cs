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
    public class BaseOperationTests : BaseOperation
    {
        private OP _opVal;
        public override OP OpValue => _opVal;
        public override bool Run(IOpData opData, out string error) => throw new NotImplementedException();

        public static IEnumerable<object[]> GetItemCountCases()
        {
            yield return new object[] { new MockOpData() { _itemCount = 0, _altItemCount = 0 }, true };
            yield return new object[] { new MockOpData() { _itemCount = 1000, _altItemCount = 0 }, true };
            yield return new object[] { new MockOpData() { _itemCount = 0, _altItemCount = 1000 }, true };
            yield return new object[] { new MockOpData() { _itemCount = 999, _altItemCount = 1 }, true };
            yield return new object[] { new MockOpData() { _itemCount = 1, _altItemCount = 999 }, true };
            yield return new object[] { new MockOpData() { _itemCount = 1000, _altItemCount = 1 }, false };
            yield return new object[] { new MockOpData() { _itemCount = 1, _altItemCount = 1000 }, false };
        }
        [Theory]
        [MemberData(nameof(GetItemCountCases))]
        public void CheckItemCountTest(MockOpData data, bool success)
        {
            bool b = CheckItemCount(data, out string error);
            if (success)
            {
                Assert.True(b, error);
            }
            else
            {
                Assert.False(b);
                Assert.Equal(Err.OpStackItemOverflow, error);
            }
        }

        [Theory]
        [InlineData(new byte[] { }, true)]
        [InlineData(new byte[] { 0 }, true)]
        [InlineData(new byte[] { 129 }, true)] // -1
        [InlineData(new byte[] { 1 }, true)]
        [InlineData(new byte[] { 16 }, true)]
        [InlineData(new byte[] { 17 }, false)]
        [InlineData(new byte[] { 130 }, false)]
        [InlineData(new byte[] { 0, 0 }, true)]
        [InlineData(new byte[] { 1, 0 }, true)]
        [InlineData(new byte[] { 16, 0, 0 }, true)]
        [InlineData(new byte[] { 1, 0, 128 }, true)] // -1
        public void HasNumOpTest(byte[] data, bool expected)
        {
            bool actual = HasNumOp(data);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new byte[] { }, false)]
        [InlineData(new byte[] { 0 }, false)]
        [InlineData(new byte[] { 0, 0 }, false)]
        [InlineData(new byte[] { 0x80 }, false)]
        [InlineData(new byte[] { 0, 0x80 }, false)]
        [InlineData(new byte[] { 0, 0, 0x80 }, false)]
        [InlineData(new byte[] { 0, 1, 0x80 }, true)]
        [InlineData(new byte[] { 0, 0, 0x80, 1 }, true)]
        [InlineData(new byte[] { 0, 0, 0x80, 0 }, true)]
        [InlineData(new byte[] { 1 }, true)]
        [InlineData(new byte[] { 1, 0 }, true)]
        [InlineData(new byte[] { 0, 1 }, true)]
        public void IsNotZeroTest(byte[] data, bool expected)
        {
            bool actual = IsNotZero(data);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, new byte[] { })]
        [InlineData(1, new byte[] { 1 })]
        [InlineData(127, new byte[] { 127 })]
        [InlineData(128, new byte[] { 128, 0 })]
        [InlineData(255, new byte[] { 255, 0 })]
        [InlineData(256, new byte[] { 0, 1 })]
        [InlineData(32577, new byte[] { 65, 127 })]
        [InlineData(32833, new byte[] { 65, 128, 0 })]
        [InlineData(-1, new byte[] { 0b1000_0001 })]
        [InlineData(-127, new byte[] { 0b1111_1111 })]
        [InlineData(-128, new byte[] { 128, 0b1000_0000 })]
        [InlineData(-4762405, new byte[] { 37, 171, 0b11001000 })]
        [InlineData(-11971365, new byte[] { 37, 171, 182, 0b1000_0000 })]
        public void IntToByteArrayTest(long val, byte[] expected)
        {
            byte[] actual = IntToByteArray(val);
            Assert.Equal(expected, actual);
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
        public void IsNumberOpTest(OP val)
        {
            Assert.True(IsNumberOp(val));
        }

        [Theory]
        [InlineData((OP)1)]
        [InlineData((OP)0x4b)]
        [InlineData(OP.PushData1)]
        [InlineData(OP.PushData2)]
        [InlineData(OP.PushData4)]
        [InlineData(OP.Reserved)]
        [InlineData(OP.NOP)]
        [InlineData(OP.DUP)]
        public void IsNumberOp_FalseTest(OP val)
        {
            Assert.False(IsNumberOp(val));
        }

        [Theory]
        [InlineData(new byte[] { }, false, 4, true, 0)]
        [InlineData(new byte[] { }, true, 4, true, 0)]
        [InlineData(new byte[] { 0 }, false, 4, true, 0)]
        [InlineData(new byte[] { 0 }, true, 4, false, 0)] // 0 should be empty array in strict rules
        [InlineData(new byte[] { 0x80 }, false, 4, true, 0)]
        [InlineData(new byte[] { 0x80 }, true, 4, false, 0)] // -0
        [InlineData(new byte[] { 1 }, false, 4, true, 1)]
        [InlineData(new byte[] { 1 }, true, 4, true, 1)]
        [InlineData(new byte[] { 127 }, false, 4, true, 127)]
        [InlineData(new byte[] { 127 }, true, 4, true, 127)]
        [InlineData(new byte[] { 127, 0 }, false, 4, true, 127)]
        [InlineData(new byte[] { 127, 0 }, true, 4, false, 0)] // Has extra zero
        [InlineData(new byte[] { 128, 0 }, false, 4, true, 128)]
        [InlineData(new byte[] { 128, 0 }, true, 4, true, 128)]
        [InlineData(new byte[] { 128, 0, 0 }, true, 4, false, 0)] // Has extra zero
        [InlineData(new byte[] { 129, 0 }, false, 4, true, 129)]
        [InlineData(new byte[] { 129, 0 }, true, 4, true, 129)]
        [InlineData(new byte[] { 129 }, false, 4, true, -1)]
        [InlineData(new byte[] { 129 }, true, 4, true, -1)]
        [InlineData(new byte[] { 255, 0 }, false, 4, true, 255)]
        [InlineData(new byte[] { 255, 0 }, true, 4, true, 255)]
        [InlineData(new byte[] { 255, 128 }, false, 4, true, -255)]
        [InlineData(new byte[] { 255, 128 }, true, 4, true, -255)]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 }, false, 4, false, 0)] // Longer than maxLen
        [InlineData(new byte[] { 1, 2, 3, 4, 5 }, true, 4, false, 0)] // Longer than maxLen
        public void TryConvertToLongTest(byte[] data, bool strict, int maxLen, bool success, long expected)
        {
            bool b = TryConvertToLong(data, out long result, strict, maxLen);
            Assert.Equal(success, b);
            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData(0, true, OP._0)]
        [InlineData(-1, true, OP.Negative1)]
        [InlineData(1, true, OP._1)]
        [InlineData(3, true, OP._3)]
        [InlineData(16, true, OP._16)]
        [InlineData(17, false, OP._0)]
        [InlineData(-2, false, OP._0)]
        public void TryConvertToOpTest(int val, bool success, OP expected)
        {
            bool b = TryConvertToOp(val, out OP actual);
            Assert.Equal(success, b);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteToStreamTest()
        {
            byte b = 0xf0;
            _opVal = (OP)b;
            FastStream stream = new FastStream(1);
            WriteToStream(stream);
            Assert.Equal(new byte[] { b }, stream.ToByteArray());
        }

        [Fact]
        public void WriteToStreamForSigningTest()
        {
            byte b = 0xf0;
            _opVal = (OP)b;
            FastStream stream = new FastStream(1);
            WriteToStreamForSigning(stream, new ReadOnlySpan<byte>(new byte[] { 1, 2, 3 }));
            WriteToStreamForSigning(stream, new byte[2][]);
            Assert.Equal(new byte[] { b, b }, stream.ToByteArray());
        }

        [Fact]
        public void WriteToStreamForSigningSegWitTest()
        {
            byte b = 0xf0;
            _opVal = (OP)b;
            FastStream stream = new FastStream(1);
            WriteToStreamForSigningSegWit(stream);
            Assert.Equal(new byte[] { b }, stream.ToByteArray());
        }

        [Fact]
        public void EqualsTest()
        {
            _opVal = OP.DUP;
            IOperation same = new DUPOp();
            IOperation diff1 = new DUP2Op();
            string diff2 = "DUPOp";

            Assert.True(Equals(same));
            Assert.False(Equals(diff1));
            Assert.False(Equals(diff2));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            _opVal = OP.DUP;
            int h1 = GetHashCode();
            _opVal = OP.DUP2;
            int h2 = GetHashCode();

            Assert.NotEqual(h1, h2);
        }
    }
}
