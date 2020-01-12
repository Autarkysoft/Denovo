// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Encoders
{
    public class Base16Tests
    {
        [Theory]
        [InlineData("")]
        [InlineData("0x")]
        [InlineData("01")]
        [InlineData("12abcf")]
        [InlineData("12ABCF")]
        [InlineData("0x12abcf")]
        public void IsValidTest(string hex)
        {
            Assert.True(Base16.IsValid(hex));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("3")]
        [InlineData("abc")]
        [InlineData("ABC")]
        [InlineData("abcg")]
        [InlineData("0x1")]
        public void IsValid_FalseTest(string hex)
        {
            Assert.False(Base16.IsValid(hex));
        }

        [Theory]
        [InlineData(new byte[] { }, "")]
        [InlineData(new byte[] { 0 }, "00")]
        [InlineData(new byte[] { 0, 0 }, "0000")]
        [InlineData(new byte[] { 16, 42, 255 }, "102aff")]
        [InlineData(new byte[] { 16, 42, 255 }, "0x102aff")]
        [InlineData(new byte[] { 0, 0, 2 }, "000002")]
        public void DecodeTest(byte[] expected, string hex)
        {
            byte[] actual = Base16.Decode(hex);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" ")]
        [InlineData("x")]
        public void Decode_ExceptionTest(string hex)
        {
            Assert.Throws<ArgumentException>(() => Base16.Decode(hex));
        }

        [Theory]
        [InlineData(new byte[] { }, "")]
        [InlineData(new byte[] { 0 }, "00")]
        [InlineData(new byte[] { 0, 0 }, "0000")]
        [InlineData(new byte[] { 16, 42, 255 }, "102aff")]
        [InlineData(new byte[] { 0, 0, 2 }, "000002")]
        public void EncodeTest(byte[] ba, string expected)
        {
            string actual = Base16.Encode(ba);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Encode_AllCharsTest()
        {
            byte[] allHexChars = new byte[256];
            StringBuilder sb = new StringBuilder(256 * 2);
            for (int i = 0; i < 256; i++)
            {
                allHexChars[i] = (byte)i;
                sb.Append($"{allHexChars[i]:x2}");
            }

            Assert.Equal(sb.ToString(), Base16.Encode(allHexChars));
        }

        [Fact]
        public void Encode_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => Base16.Encode(null));
        }

    }
}
