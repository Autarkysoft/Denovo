// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class BIP0039Tests
    {
        public static IEnumerable<object[]> GetEntropyCases(bool includeEntropy)
        {
            // Test cases are from:
            // https://github.com/trezor/python-mnemonic/blob/eb8a010da91fefac2d43cb8ede834ed90b62601f/vectors.json
            JArray jarr = Helper.ReadResource<JArray>("BIP0039TestData");
            foreach (var item in jarr)
            {
                byte[] ent = Helper.HexToBytes(item[0].ToString());
                string words = item[1].ToString();
                //// This is the BIP-32 entropy (or seed) resulted from PBKDF2 that we don't use in our tests here
                //string extendedEnt = item[2].ToString();
                string xprv = item[3].ToString();

                yield return includeEntropy ? new object[] { ent, words, xprv } : new object[] { words, xprv };
            }
        }

        [Theory]
        [MemberData(nameof(GetEntropyCases), true)]
        public void Constructor_FromBytesTest(byte[] ent, string expMnemonic, string expXprv)
        {
            using BIP0039 bip39 = new BIP0039(ent, passPhrase: "TREZOR");
            string actMnemonic = bip39.ToMnemonic();
            // Also check if base class (BIP-32) is set correctly:
            string actXprv = bip39.ToBase58(false);

            Assert.Equal(expMnemonic, actMnemonic);
            Assert.Equal(expXprv, actXprv);
        }

        [Fact]
        public void Constructor_FromBytes_NullExceptionTest()
        {
            byte[] nba = null;
            Assert.Throws<ArgumentNullException>(() => new BIP0039(nba));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(33)]
        public void Constructor_FromBytes_OutOfRangeExceptionTest(int entLen)
        {
            byte[] entropy = new byte[entLen];
            Assert.Throws<ArgumentOutOfRangeException>(() => new BIP0039(entropy));
        }

        [Fact]
        public void Constructor_FromBytes_ArgumentExceptionTest()
        {
            byte[] entropy = Helper.GetBytes(16);
            BIP0039.WordLists wl = (BIP0039.WordLists)100;

            Exception ex = Assert.Throws<ArgumentException>(() => new BIP0039(entropy, wl));
            Assert.Contains("Given word list is not defined.", ex.Message);
        }


        [Fact]
        public void Constructor_FromRngTest()
        {
            var rng = new MockRng("bfdf93686a31cd55fc5c1b8fd290fe39");
            using BIP0039 bip = new BIP0039(rng, 16);

            string actual = bip.ToMnemonic();
            string expected = "save wish sure stamp broom priority vapor lock more nest display inch";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_FromRng_NullExceptionTest()
        {
            IRandomNumberGenerator nrng = null;
            Assert.Throws<ArgumentNullException>(() => new BIP0039(nrng, 16));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(33)]
        public void Constructor_FromRng_OutOfRangeExceptionTest(int entLen)
        {
            MockRng rng = new MockRng(new byte[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => new BIP0039(rng, entLen));
        }


        [Theory]
        [MemberData(nameof(GetEntropyCases), false)]
        public void Constructor_FromStringTest(string expMnemonic, string expXprv)
        {
            using BIP0039 bip = new BIP0039(expMnemonic, passPhrase: "TREZOR");
            string actMnemonic = bip.ToMnemonic();
            // Also check if base class (BIP-32) is set correctly:
            string actXprv = bip.ToBase58(false);

            Assert.Equal(expMnemonic, actMnemonic);
            Assert.Equal(expXprv, actXprv);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_FromString_NullExceptionTest(string mnemonic)
        {
            Assert.Throws<ArgumentNullException>(() => new BIP0039(mnemonic));
        }

        [Fact]
        public void Constructor_FromString_ArgumentExceptionTest()
        {
            string mnemonic = "save wish sure stamp broom priority vapor lock more nest display inch";
            BIP0039.WordLists wl = (BIP0039.WordLists)100;

            Exception ex = Assert.Throws<ArgumentException>(() => new BIP0039(mnemonic, wl, "Pass"));
            Assert.Contains("Given word list is not defined.", ex.Message);
        }

        [Theory]
        [InlineData("legal winner thank year wave sausage worth useful legal winner thank yello", "Seed has invalid words.")]
        [InlineData("legal winner thank year wave sausage worth useful legal winner thank !", "Seed has invalid words.")]
        public void Constructor_FromString_ArgumentExceptionTest2(string mnemonic, string error)
        {
            Exception ex = Assert.Throws<ArgumentException>(() => new BIP0039(mnemonic));
            Assert.Contains(error, ex.Message);
        }

        [Theory]
        [InlineData("legal winner thank year wave sausage worth useful legal winner thank", "Invalid seed length.")]
        [InlineData("legal winner thank year wave sausage worth useful legal winner thank thank", "Wrong checksum.")]
        public void Constructor_FromString_FormatExceptionTest(string mnemonic, string error)
        {
            Exception ex = Assert.Throws<FormatException>(() => new BIP0039(mnemonic));
            Assert.Contains(error, ex.Message);
        }

        [Fact]
        public void Constructor_FromString_MultipleSpacesTest()
        {
            string mnemonic = " legal   winner  thank year wave sausage worth useful legal   winner thank yellow   ";
            using BIP0039 bip = new BIP0039(mnemonic);

            string actual = bip.ToMnemonic();
            string expected = "legal winner thank year wave sausage worth useful legal winner thank yellow";

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetJapEntropyCases(bool includeEntropy)
        {
            // Test cases are from: 
            // https://github.com/bip32JP/bip32JP.github.io/blob/0d1ac71933458f08a56d6ab44dd0f251158cc865/test_JP_BIP39.json
            JArray jarr = Helper.ReadResource<JArray>("BIP0039JapTestData");
            foreach (var item in jarr)
            {
                byte[] ent = Helper.HexToBytes(item["entropy"].ToString());
                string words = item["mnemonic"].ToString();
                string pass = item["passphrase"].ToString();
                string xprv = item["bip32_xprv"].ToString();

                yield return includeEntropy ? new object[] { ent, words, pass, xprv } : new object[] { words, pass, xprv };
            }
        }

        [Theory]
        [MemberData(nameof(GetJapEntropyCases), true)]
        public void Constructor_FromBytes_JapTest(byte[] entropy, string expMnemonic, string pass, string expXprv)
        {
            // The passphrase is not normalized, this test makes sure the constructor normalizes it correctly
            using BIP0039 bip = new BIP0039(entropy, BIP0039.WordLists.Japanese, pass);
            string actMnemonic = bip.ToMnemonic();
            // Also check if base class (BIP-32) is set correctly:
            string actXprv = bip.ToBase58(false);

            Assert.Equal(expMnemonic.Normalize(NormalizationForm.FormKD), actMnemonic);
            Assert.Equal(expXprv, actXprv);
        }

        [Theory]
        [MemberData(nameof(GetJapEntropyCases), false)]
        public void Constructor_FromString_JapTest(string expMnemonic, string pass, string expXprv)
        {
            // Both mnemonic and passphrase are not normalized, this test makes sure the constructor normalizes them correctly
            using BIP0039 bip = new BIP0039(expMnemonic, BIP0039.WordLists.Japanese, pass);
            string actMnemonic = bip.ToMnemonic();
            // Also check if base class (BIP-32) is set correctly:
            string actXprv = bip.ToBase58(false);

            Assert.Equal(expMnemonic.Normalize(NormalizationForm.FormKD), actMnemonic);
            Assert.Equal(expXprv, actXprv);
        }

        [Theory]
        [InlineData(BIP0039.WordLists.English, "abandon")]
        [InlineData(BIP0039.WordLists.ChineseSimplified, "的")]
        [InlineData(BIP0039.WordLists.ChineseTraditional, "的")]
        [InlineData(BIP0039.WordLists.French, "abaisser")]
        [InlineData(BIP0039.WordLists.Italian, "abaco")]
        [InlineData(BIP0039.WordLists.Japanese, "あいこくしん")]
        [InlineData(BIP0039.WordLists.Korean, "가격")]
        [InlineData(BIP0039.WordLists.Spanish, "ábaco")]
        public void GetAllWordsTest(BIP0039.WordLists wl, string first)
        {
            string[] actual = BIP0039.GetAllWords(wl);
            Assert.NotNull(actual);
            Assert.Equal(2048, actual.Length);
            Assert.Equal(first, actual[0]);
        }

        [Fact]
        public void GetAllWords_ExceptionTest()
        {
            Assert.Throws<ArgumentException>(() => BIP0039.GetAllWords((BIP0039.WordLists)100));
        }
    }
}
