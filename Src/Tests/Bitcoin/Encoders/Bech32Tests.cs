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
        [Theory]
        [InlineData("A12UEL5L", Bech32.Mode.B32)]
        [InlineData("a12uel5l", Bech32.Mode.B32)]
        [InlineData("an83characterlonghumanreadablepartthatcontainsthenumber1andtheexcludedcharactersbio1tt5tgs", Bech32.Mode.B32)]
        [InlineData("abcdef1qpzry9x8gf2tvdw0s3jn54khce6mua7lmqqqxw", Bech32.Mode.B32)]
        [InlineData("11qqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqqc8247j", Bech32.Mode.B32)]
        [InlineData("split1checkupstagehandshakeupstreamerranterredcaperred2y9e3w", Bech32.Mode.B32)]
        [InlineData("?1ezyfcl", Bech32.Mode.B32)]

        [InlineData("A1LQFN3A", Bech32.Mode.B32m)]
        [InlineData("a1lqfn3a", Bech32.Mode.B32m)]
        [InlineData("an83characterlonghumanreadablepartthatcontainsthetheexcludedcharactersbioandnumber11sg7hg6", Bech32.Mode.B32m)]
        [InlineData("abcdef1l7aum6echk45nj3s0wdvt2fg8x9yrzpqzd3ryx", Bech32.Mode.B32m)]
        [InlineData("11llllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllludsr8", Bech32.Mode.B32m)]
        [InlineData("split1checkupstagehandshakeupstreamerranterredcaperredlc445v", Bech32.Mode.B32m)]
        [InlineData("?1v759aa", Bech32.Mode.B32m)]
        public void IsValidTest(string input, Bech32.Mode mode)
        {
            Assert.True(Bech32.IsValid(input, mode));
        }


        [Theory]
        [InlineData(null, Bech32.Mode.B32)]
        [InlineData("", Bech32.Mode.B32)]
        [InlineData(" ", Bech32.Mode.B32)]
        [InlineData("a12UEl5l", Bech32.Mode.B32)]
        [InlineData("A12UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L2UEL5L", Bech32.Mode.B32)]
        [InlineData("an84characterslonghumanreadablepartthatcontainsthenumber1andtheexcludedcharactersbio1569pvx", Bech32.Mode.B32)]
        [InlineData("pzry9x0s0muk", Bech32.Mode.B32)]
        [InlineData("1pzry9x0s0muk", Bech32.Mode.B32)]
        [InlineData("x1b4n0q5v", Bech32.Mode.B32)]
        [InlineData("li1dgmt3", Bech32.Mode.B32)]
        [InlineData("10a06t8", Bech32.Mode.B32)]
        [InlineData("1qzzfhee", Bech32.Mode.B32)]
        [InlineData("A1G7SGD8", Bech32.Mode.B32)]
        [InlineData("a12uel00", Bech32.Mode.B32)]
        [InlineData("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t5", Bech32.Mode.B32)]
        [InlineData("tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3q0sL5k7", Bech32.Mode.B32)]
        // Valid Bech32m but invalid Bech32
        [InlineData("A1LQFN3A", Bech32.Mode.B32)]
        [InlineData("a1lqfn3a", Bech32.Mode.B32)]
        [InlineData("an83characterlonghumanreadablepartthatcontainsthetheexcludedcharactersbioandnumber11sg7hg6", Bech32.Mode.B32)]
        [InlineData("abcdef1l7aum6echk45nj3s0wdvt2fg8x9yrzpqzd3ryx", Bech32.Mode.B32)]
        [InlineData("11llllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllludsr8", Bech32.Mode.B32)]
        [InlineData("split1checkupstagehandshakeupstreamerranterredcaperredlc445v", Bech32.Mode.B32)]
        [InlineData("?1v759aa", Bech32.Mode.B32)]
        // BIP-350 tests
        [InlineData("an84characterslonghumanreadablepartthatcontainsthetheexcludedcharactersbioandnumber11d6pts4", Bech32.Mode.B32m)]
        [InlineData("qyrz8wqd2c9m", Bech32.Mode.B32m)]
        [InlineData("1qyrz8wqd2c9m", Bech32.Mode.B32m)]
        [InlineData("y1b0jsk6g", Bech32.Mode.B32m)]
        [InlineData("lt1igcx5c0", Bech32.Mode.B32m)]
        [InlineData("in1muywd", Bech32.Mode.B32m)]
        [InlineData("mm1crxm3i", Bech32.Mode.B32m)]
        [InlineData("au1s5cgom", Bech32.Mode.B32m)]
        [InlineData("M1VUXWEZ", Bech32.Mode.B32m)]
        [InlineData("16plkw9", Bech32.Mode.B32m)]
        [InlineData("1p2gdwpf", Bech32.Mode.B32m)]

        [InlineData(null, Bech32.Mode.B32m)]
        [InlineData("", Bech32.Mode.B32m)]
        [InlineData(" ", Bech32.Mode.B32m)]
        [InlineData("a12UEl5l", Bech32.Mode.B32m)]
        public void IsValid_FailTest(string input, Bech32.Mode mode)
        {
            Assert.False(Bech32.IsValid(input, mode));
        }

        [Fact]
        public void IsValid_Fail_SpecialCaseTest()
        {
            // HRP has an out of range character
            Assert.False(Bech32.IsValid($"{(char)0x20}1nwldj5", Bech32.Mode.B32));
            Assert.False(Bech32.IsValid($"{(char)0x20}1nwldj5", Bech32.Mode.B32m));
            Assert.False(Bech32.IsValid($"{(char)0x20}1xj0phk", Bech32.Mode.B32));
            Assert.False(Bech32.IsValid($"{(char)0x20}1xj0phk", Bech32.Mode.B32m));

            Assert.False(Bech32.IsValid($"{(char)0x7F}1axkwrx", Bech32.Mode.B32));
            Assert.False(Bech32.IsValid($"{(char)0x7F}1axkwrx", Bech32.Mode.B32m));
            Assert.False(Bech32.IsValid($"{(char)0x7F}1g6xzxy", Bech32.Mode.B32));
            Assert.False(Bech32.IsValid($"{(char)0x7F}1g6xzxy", Bech32.Mode.B32m));

            Assert.False(Bech32.IsValid($"{(char)0x80}1eym55h", Bech32.Mode.B32));
            Assert.False(Bech32.IsValid($"{(char)0x80}1eym55h", Bech32.Mode.B32m));
            Assert.False(Bech32.IsValid($"{(char)0x80}1vctc34", Bech32.Mode.B32));
            Assert.False(Bech32.IsValid($"{(char)0x80}1vctc34", Bech32.Mode.B32m));
            // Invalid character in checksum
            Assert.False(Bech32.IsValid($"de1lg7wt{(char)0xFF}", Bech32.Mode.B32));
            Assert.False(Bech32.IsValid($"de1lg7wt{(char)0xFF}", Bech32.Mode.B32m));
        }


        public static IEnumerable<object[]> GetEncodeCases()
        {
            // byte[] data, encoding mode, byte witVer, string hrp, string bech32
            yield return new object[]
            {
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323f1433bd6"),
                Bech32.Mode.B32,
                0,
                "bc",
                "BC1QW508D6QEJXTDG4Y5R3ZARVARY0C5XW7KV8F3T4"
            };
            yield return new object[]
            {
                Helper.HexToBytes("1863143c14c5166804bd19203356da136c985678cd4d27a1b8c6329604903262"),
                Bech32.Mode.B32,
                0,
                "tb",
                "tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3q0sl5k7"
            };
            yield return new object[]
            {
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323f1433bd6751e76e8199196d454941c45d1b3a323f1433bd6"),
                Bech32.Mode.B32,
                1,
                "bc",
                "bc1pw508d6qejxtdg4y5r3zarvary0c5xw7kw508d6qejxtdg4y5r3zarvary0c5xw7k7grplx"
            };
            yield return new object[]
            {
                Helper.HexToBytes("751e"),
                Bech32.Mode.B32,
                16,
                "bc",
                "BC1SW50QA3JX3S"
            };
            yield return new object[]
            {
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323"),
                Bech32.Mode.B32,
                2,
                "bc",
                "bc1zw508d6qejxtdg4y5r3zarvaryvg6kdaj"
            };
            yield return new object[]
            {
                Helper.HexToBytes("000000c4a5cad46221b2a187905e5266362b99d5e91c6ce24d165dab93e86433"),
                Bech32.Mode.B32,
                0,
                "tb",
                "tb1qqqqqp399et2xygdj5xreqhjjvcmzhxw4aywxecjdzew6hylgvsesrxh6hy"
            };
            // BIP-350
            yield return new object[]
            {
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323f1433bd6751e76e8199196d454941c45d1b3a323f1433bd6"),
                Bech32.Mode.B32m,
                1,
                "bc",
                "bc1pw508d6qejxtdg4y5r3zarvary0c5xw7kw508d6qejxtdg4y5r3zarvary0c5xw7kt5nd6y"
            };
            yield return new object[]
            {
                Helper.HexToBytes("751e"),
                Bech32.Mode.B32m,
                16,
                "bc",
                "BC1SW50QGDZ25J"
            };
            yield return new object[]
            {
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323"),
                Bech32.Mode.B32m,
                2,
                "bc",
                "bc1zw508d6qejxtdg4y5r3zarvaryvaxxpcs"
            };
            yield return new object[]
            {
                Helper.HexToBytes("000000c4a5cad46221b2a187905e5266362b99d5e91c6ce24d165dab93e86433"),
                Bech32.Mode.B32m,
                1,
                "tb",
                "tb1pqqqqp399et2xygdj5xreqhjjvcmzhxw4aywxecjdzew6hylgvsesf3hn0c"
            };
            yield return new object[]
            {
                Helper.HexToBytes("79be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798"),
                Bech32.Mode.B32m,
                1,
                "bc",
                "bc1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vqzk5jj0"
            };
        }

        [Theory]
        [MemberData(nameof(GetEncodeCases))]
        public void DecodeTest(byte[] expBa, Bech32.Mode mode, byte expWitVer, string expHrp, string bechStr)
        {
            byte[] actualBa = Bech32.Decode(bechStr, mode, out byte actualWitVer, out string actualHrp);

            Assert.Equal(expBa, actualBa);
            Assert.Equal(expWitVer, actualWitVer);
            Assert.Equal(expHrp, actualHrp);
        }
        [Theory]
        [MemberData(nameof(GetEncodeCases))]
        public void TryDecodeTest(byte[] expBa, Bech32.Mode mode, byte expWitVer, string expHrp, string bechStr)
        {
            bool b = Bech32.TryDecode(bechStr, mode, out byte[] actualBa, out byte actualWitVer, out string actualHrp);

            Assert.True(b);
            Assert.Equal(expBa, actualBa);
            Assert.Equal(expWitVer, actualWitVer);
            Assert.Equal(expHrp, actualHrp);
        }

        [Theory]
        [MemberData(nameof(GetEncodeCases))]
        public void EncodeTest(byte[] data, Bech32.Mode mode, byte witVer, string hrp, string expected)
        {
            string actual = Bech32.Encode(data, mode, witVer, hrp);
            Assert.Equal(expected, actual, ignoreCase: true);
        }


        [Theory]
        [InlineData(null, Bech32.Mode.B32, 255, ".", "Input is not a valid bech32 encoded string.")]
        [InlineData("a12uel2l", Bech32.Mode.B32, 255, "a", "Invalid checksum.")]
        [InlineData("abcdef1qpzry9x8gf2tvdw0s3jn54khce6mua7lmqqqxw", Bech32.Mode.B32, 255, "abcdef", "Invalid data format.")]
        [InlineData("bc1zw508d6qejxtdg4y5r3zarvaryvqyzf3du", Bech32.Mode.B32, 255, "bc", "Invalid data format.")]
        [InlineData("tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3pjxtptv", Bech32.Mode.B32, 255, "tb", "Invalid data format.")]
        [InlineData("tb1qr", Bech32.Mode.B32, 255, ".", "Input is not a valid bech32 encoded string.")] // Short data
        // BIP-350
        [InlineData("bc1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vqh2y7hd", Bech32.Mode.B32m, 255, "bc", "Invalid checksum.")]
        [InlineData("tb1z0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vqglt7rf", Bech32.Mode.B32m, 255, "tb", "Invalid checksum.")]
        [InlineData("BC1S0XLXVLHEMJA6C4DQV22UAPCTQUPFHLXM9H8Z3K2E72Q4K9HCZ7VQ54WELL", Bech32.Mode.B32m, 255, "bc", "Invalid checksum.")]
        [InlineData("bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kemeawh", Bech32.Mode.B32, 255, "bc", "Invalid checksum.")]
        [InlineData("tb1q0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vq24jc47", Bech32.Mode.B32, 255, "tb", "Invalid checksum.")]
        [InlineData("bc1p38j9r5y49hruaue7wxjce0updqjuyyx0kh56v8s25huc6995vvpql3jow4", Bech32.Mode.B32m, 255, ".", "Input is not a valid bech32 encoded string.")]
        [InlineData("tb1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vq47Zagq", Bech32.Mode.B32m, 255, ".", "Input is not a valid bech32 encoded string.")]
        [InlineData("bc1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7v07qwwzcrf", Bech32.Mode.B32m, 255, "bc", "Invalid data format.")]
        [InlineData("tb1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vpggkg4j", Bech32.Mode.B32m, 255, "tb", "Invalid data format.")]
        public void Decode_ExceptionTests(string bech, Bech32.Mode mode, byte expWitVer, string expHrp, string expErr)
        {
            byte actWitVer = 255;
            string actHrp = ".";
            Exception ex = Assert.Throws<FormatException>(() => Bech32.Decode(bech, mode, out actWitVer, out actHrp));

            Assert.Equal(expWitVer, actWitVer);
            Assert.Equal(expHrp, actHrp);
            Assert.Contains(expErr, ex.Message);

            bool b = Bech32.TryDecode(bech, mode, out byte[] result, out actWitVer, out actHrp);
            Assert.False(b);
            Assert.Null(result);
            Assert.Equal(0, actWitVer);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        public void Encode_NullExceptionTests(byte[] data)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => Bech32.Encode(data, Bech32.Mode.B32, 0, "bc"));
            Assert.Contains("Data can not be null or empty.", ex.Message);
        }

        [Theory]
        [InlineData(32)]
        [InlineData(byte.MaxValue)]
        public void Encode_OutOfRangeExceptionTests(byte wver)
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => Bech32.Encode(new byte[1], Bech32.Mode.B32, wver, "bc"));
            Assert.Contains("Witness version can not be bigger than 31.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("012345678901234567890123456789012345678901234567890123456789012345678901234567891234")]

        public void Encode_FormatExceptionTests(string hrp)
        {
            Exception ex = Assert.Throws<FormatException>(() => Bech32.Encode(new byte[1], Bech32.Mode.B32, 0, hrp));
            Assert.Contains("Invalid HRP.", ex.Message);
        }

    }
}
