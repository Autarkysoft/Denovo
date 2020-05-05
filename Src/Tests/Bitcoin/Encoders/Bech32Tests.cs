// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Encoders
{
    public class Bech32Tests
    {
        private readonly Bech32 encoder = new Bech32();


        [Theory]
        [InlineData("A12UEL5L")]
        [InlineData("a12uel5l")]
        [InlineData("an83characterlonghumanreadablepartthatcontainsthenumber1andtheexcludedcharactersbio1tt5tgs")]
        [InlineData("abcdef1qpzry9x8gf2tvdw0s3jn54khce6mua7lmqqqxw")]
        [InlineData("11qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqc8247j")]
        [InlineData("split1checkupstagehandshakeupstreamerranterredcaperred2y9e3w")]
        [InlineData("?1ezyfcl")]
        public void IsValidTest(string input)
        {
            Assert.True(encoder.IsValid(input));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a12UEl5l")]
        [InlineData("A12UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L")]
        [InlineData("an84characterslonghumanreadablepartthatcontainsthenumber1andtheexcludedcharactersbio1569pvx")]
        [InlineData("pzry9x0s0muk")]
        [InlineData("1pzry9x0s0muk")]
        [InlineData("x1b4n0q5v")]
        [InlineData("li1dgmt3")]
        [InlineData("10a06t8")]
        [InlineData("1qzzfhee")]
        [InlineData("A1G7SGD8")]
        [InlineData("a12uel00")]
        [InlineData("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t5")]
        [InlineData("tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3q0sL5k7")]
        public void IsValid_FailTest(string input)
        {
            Assert.False(encoder.IsValid(input));
        }

        [Fact]
        public void IsValid_Fail_SpecialCaseTest()
        {
            // HRP has an out of range character
            Assert.False(encoder.IsValid($"{(char)0x20}1nwldj5"));
            Assert.False(encoder.IsValid($"{(char)0x7F}1axkwrx"));
            Assert.False(encoder.IsValid($"{(char)0x80}1eym55h"));
            // Invalid character in checksum
            Assert.False(encoder.IsValid($"de1lg7wt{(char)0xFF}"));
        }


        public static IEnumerable<object[]> GetEncodeCases()
        {
            // byte[] data, byte witVer, string hrp, string bech32
            yield return new object[]
            {
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323f1433bd6"),
                0,
                "bc",
                "BC1QW508D6QEJXTDG4Y5R3ZARVARY0C5XW7KV8F3T4"
            };
            yield return new object[]
            {
                Helper.HexToBytes("1863143c14c5166804bd19203356da136c985678cd4d27a1b8c6329604903262"),
                0,
                "tb",
                "tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3q0sl5k7"
            };
            yield return new object[]
            {
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323f1433bd6751e76e8199196d454941c45d1b3a323f1433bd6"),
                1,
                "bc",
                "bc1pw508d6qejxtdg4y5r3zarvary0c5xw7kw508d6qejxtdg4y5r3zarvary0c5xw7k7grplx"
            };
            yield return new object[]
            {
                Helper.HexToBytes("751e"),
                16,
                "bc",
                "BC1SW50QA3JX3S"
            };
            yield return new object[]
            {
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323"),
                2,
                "bc",
                "bc1zw508d6qejxtdg4y5r3zarvaryvg6kdaj"
            };
            yield return new object[]
            {
                Helper.HexToBytes("000000c4a5cad46221b2a187905e5266362b99d5e91c6ce24d165dab93e86433"),
                0,
                "tb",
                "tb1qqqqqp399et2xygdj5xreqhjjvcmzhxw4aywxecjdzew6hylgvsesrxh6hy"
            };
        }

        [Theory]
        [MemberData(nameof(GetEncodeCases))]
        public void DecodeTest(byte[] expBa, byte expWitVer, string expHrp, string bechStr)
        {
            byte[] actualBa = encoder.Decode(bechStr, out byte actualWitVer, out string actualHrp);

            Assert.Equal(expBa, actualBa);
            Assert.Equal(expWitVer, actualWitVer);
            Assert.Equal(expHrp, actualHrp);
        }

        [Theory]
        [MemberData(nameof(GetEncodeCases))]
        public void EncodeTest(byte[] data, byte witVer, string hrp, string expected)
        {
            string actual = encoder.Encode(data, witVer, hrp);
            Assert.Equal(expected, actual, ignoreCase: true);
        }


        [Theory]
        [InlineData(null, 255, ".", "Input is not a valid bech32 encoded string.")]
        [InlineData("a12uel2l", 255, "a", "Invalid checksum.")]
        [InlineData("abcdef1qpzry9x8gf2tvdw0s3jn54khce6mua7lmqqqxw", 255, "abcdef", "Invalid data format.")]
        [InlineData("bc1zw508d6qejxtdg4y5r3zarvaryvqyzf3du", 255, "bc", "Invalid data format.")]
        [InlineData("tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3pjxtptv", 255, "tb", "Invalid data format.")]
        [InlineData("tb1qr", 255, ".", "Input is not a valid bech32 encoded string.")] // Short data
        public void Decode_ExceptionTests(string bech, byte expWitVer, string expHrp, string expErr)
        {
            byte actWitVer = 255;
            string actHrp = ".";
            Exception ex = Assert.Throws<FormatException>(() => encoder.Decode(bech, out actWitVer, out actHrp));

            Assert.Equal(expWitVer, actWitVer);
            Assert.Equal(expHrp, actHrp);
            Assert.Contains(expErr, ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        public void Encode_NullExceptionTests(byte[] data)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => encoder.Encode(data, 0, "bc"));
            Assert.Contains("Data can not be null or empty.", ex.Message);
        }

        [Theory]
        [InlineData(32)]
        [InlineData(byte.MaxValue)]
        public void Encode_OutOfRangeExceptionTests(byte wver)
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new byte[1], wver, "bc"));
            Assert.Contains("Witness version can not be bigger than 31.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("012345678901234567890123456789012345678901234567890123456789012345678901234567891234")]

        public void Encode_FormatExceptionTests(string hrp)
        {
            Exception ex = Assert.Throws<FormatException>(() => encoder.Encode(new byte[1], 0, hrp));
            Assert.Contains("Invalid HRP.", ex.Message);
        }

    }
}
