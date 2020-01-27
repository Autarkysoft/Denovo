// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.ImprovementProposals;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class BIP0021Tests
    {
        [Fact]
        public void ConstructorTest()
        {
            BIP0021 bip = new BIP0021("TheAddress", "FoO");
            // Address is not changed but scheme is lowered
            Assert.Equal("foo", bip.Scheme);
            Assert.Equal("TheAddress", bip.Address);
            Assert.Equal(0m, bip.Amount);
            Assert.Null(bip.Label);
            Assert.Null(bip.Message);
            Assert.Empty(bip.AdditionalOptions);

            bip.Amount = 20.3m;
            Assert.Equal(20.3m, bip.Amount);

            // Labels,... are not changed here. They are changed before encoding.
            bip.Label = "Some Label";
            Assert.Equal("Some Label", bip.Label);

            bip.Message = "Some Message";
            Assert.Equal("Some Message", bip.Message);
        }

        [Theory]
        [InlineData(null, "bitcoin", "Address can not be null or empty.")]
        [InlineData("", "bitcoin", "Address can not be null or empty.")]
        [InlineData(" ", "bitcoin", "Address can not be null or empty.")]
        [InlineData("175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", null, "Scheme can not be null or empty.")]
        [InlineData("175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", "", "Scheme can not be null or empty.")]
        [InlineData("175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", " ", "Scheme can not be null or empty.")]
        public void Constructor_NullExceptionTest(string address, string scheme, string expErr)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => new BIP0021(address, scheme));
            Assert.Contains(expErr, ex.Message);
        }

        [Theory]
        [InlineData("175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", "5bitcoin", "Scheme must begin with a letter.")]
        [InlineData("175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", "b!tcoin", "Scheme contains invalid characters")]
        public void Constructor_FormatExceptionTest(string address, string scheme, string expErr)
        {
            Exception ex = Assert.Throws<FormatException>(() => new BIP0021(address, scheme));
            Assert.Contains(expErr, ex.Message);
        }

        [Fact]
        public void Constructor_AmountException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BIP0021("Address") { Amount = -1 });
        }


        public static IEnumerable<object[]> GetBip21Cases()
        {
            // (string toDecode, string coin, string addr, decimal amnt, string lbl, string msg)
            yield return new object[]
            {
                "bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W",
                "bitcoin", "175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", 0, null, null
            };
            yield return new object[]
            {
                "bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?amount=20.3",
                "bitcoin", "175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", 20.3m, null, null
            };
            yield return new object[]
            {
                "bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?amount=20.3&label=FooBar",
                "bitcoin", "175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", 20.3m, "FooBar", null
            };
            yield return new object[]
            {
                "bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?amount=50&message=Donation%20for%20project%20xyz",
                "bitcoin", "175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", 50m, null, "Donation for project xyz"
            };
        }

        [Theory]
        [MemberData(nameof(GetBip21Cases))]
        public void EncodeDecodeTest(string toDecode, string coin, string addr, decimal amnt, string lbl, string msg)
        {
            BIP0021 actual = BIP0021.Decode(toDecode);

            Assert.Equal(coin, actual.Scheme);
            Assert.Equal(addr, actual.Address);
            Assert.Equal(amnt, actual.Amount);
            Assert.Equal(lbl, actual.Label);
            Assert.Equal(msg, actual.Message);

            string actualStr = actual.Encode();
            Assert.Equal(toDecode, actualStr);
        }


        [Fact]
        public void EncodeDecode_SpecialCasesTest()
        {
            BIP0021 actual = BIP0021.Decode("BitCoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W?" +
                "somethingyoudontunderstand=50&message=&somethingelseyoudontget=999&Foo=baR");
            Assert.Equal("bitcoin", actual.Scheme);
            Assert.Equal("175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", actual.Address);
            Assert.Equal(0m, actual.Amount);
            Assert.Null(actual.Label);
            // Message is null even though it exists in the string
            Assert.Null(actual.Message);
            Assert.Equal(3, actual.AdditionalOptions.Count);
            Assert.Equal("50", actual.AdditionalOptions["somethingyoudontunderstand"]);
            Assert.Equal("999", actual.AdditionalOptions["somethingelseyoudontget"]);
            Assert.Equal("baR", actual.AdditionalOptions["Foo"]);
        }

        [Theory]
        [InlineData(null, "Input can not be null or empty.")]
        [InlineData("", "Input can not be null or empty.")]
        [InlineData(" ", "Input can not be null or empty.")]
        [InlineData(":175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", "Scheme can not be null or empty.")]
        [InlineData("bitcoin:", "Address can not be null or empty.")]
        [InlineData("bitcoin:?label=FooBar", "Address can not be null or empty.")]
        public void Decode_NullExceptionTest(string toDecode, string expErr)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => BIP0021.Decode(toDecode));
            Assert.Contains(expErr, ex.Message);
        }

        [Theory]
        [InlineData("someinvalidtext", "No scheme separator was found.")]
        [InlineData("1bitcoin:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", "Scheme must begin with a letter.")]
        [InlineData("bitcoin?:175tWpb8K1S7NmH4Zx6rewF9WQrcZv245W", "Scheme contains invalid characters.")]
        public void Decode_FormatExceptionTest(string toDecode, string expErr)
        {
            Exception ex = Assert.Throws<FormatException>(() => BIP0021.Decode(toDecode));
            Assert.Contains(expErr, ex.Message);
        }
    }
}
