// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class BIP0178Tests
    {
        private const string ValidVersionWif = "KwdMAjGmerYanjeui5SHS7JkmpZvVipYvB2LJGU1ZxJwYx2CqY6c";
        private const string ValidElectrumVersionWif = "KwdMAjGmerYanjeui5SHS7JkmpZvVipYvB2LJGU1ZxJwYvP98617";


        public static IEnumerable<object[]> GetDecodeCases()
        {
            foreach (var item in Helper.ReadResource<JArray>("BIP0178TestData"))
            {
                byte[] bytes = Helper.HexToBytes(item["PrvHex"].ToString());
                string P2PKH = item["P2PKH"].ToString();
                string P2WPKH = item["P2WPKH"].ToString();
                string P2WPKH_P2SH = item["P2WPKH_P2SH"].ToString();
                string P2WPKH_Electrum = item["P2WPKH_Electrum"].ToString();
                string P2WPKH_P2SH_Electrum = item["P2WPKH_P2SH_Electrum"].ToString();
                string P2SH_Electrum = item["P2SH_Electrum"].ToString();
                string P2WSH_Electrum = item["P2WSH_Electrum"].ToString();
                string P2WSH_P2SH_Electrum = item["P2WSH_P2SH_Electrum"].ToString();

                yield return new object[]
                {
                    bytes,
                    P2PKH, P2WPKH, P2WPKH_P2SH,
                    P2WPKH_Electrum, P2WPKH_P2SH_Electrum, P2SH_Electrum, P2WSH_Electrum, P2WSH_P2SH_Electrum
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetDecodeCases))]
        public void DecodeTest(byte[] expected, string P2PKH, string P2WPKH, string P2WPKH_P2SH,
                               string P2WPKH_Electrum, string P2WPKH_P2SH_Electrum,
                               string P2SH_Electrum, string P2WSH_Electrum, string P2WSH_P2SH_Electrum)
        {
            BIP0178 bip = new BIP0178();

            Assert.Equal(expected, bip.Decode(P2PKH).ToBytes());
            Assert.Equal(expected, bip.Decode(P2WPKH).ToBytes());
            Assert.Equal(expected, bip.Decode(P2WPKH_P2SH).ToBytes());

            Assert.Equal(expected, bip.DecodeElectrumVersionedWif(P2WPKH_Electrum).ToBytes());
            Assert.Equal(expected, bip.DecodeElectrumVersionedWif(P2WPKH_P2SH_Electrum).ToBytes());
            Assert.Equal(expected, bip.DecodeElectrumVersionedWif(P2SH_Electrum).ToBytes());
            Assert.Equal(expected, bip.DecodeElectrumVersionedWif(P2WSH_Electrum).ToBytes());
            Assert.Equal(expected, bip.DecodeElectrumVersionedWif(P2WSH_P2SH_Electrum).ToBytes());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Decode_NullExceptionTest(string wif)
        {
            BIP0178 bip = new BIP0178();
            Exception ex = Assert.Throws<ArgumentNullException>(() => bip.Decode(wif));
            Assert.Contains("Input WIF can not be null or empty.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => bip.DecodeElectrumVersionedWif(wif));
            Assert.Contains("Input WIF can not be null or empty.", ex.Message);
        }

        [Fact]
        public void Decode_ArgumentExceptionTest()
        {
            BIP0178 bip = new BIP0178();
            NetworkType nt = (NetworkType)100;

            Exception ex = Assert.Throws<ArgumentException>(() => bip.Decode(ValidVersionWif, nt));
            Assert.Contains("Network type is not defined.", ex.Message);

            ex = Assert.Throws<ArgumentException>(() => bip.DecodeElectrumVersionedWif(ValidElectrumVersionWif, nt));
            Assert.Contains("Network type is not defined.", ex.Message);
        }

        [Theory]
        [InlineData(" ", "Input is not a valid base-58 encoded string.")]
        [InlineData("L5oLkpV3aqBjhki6LmvChTCq73v9gyymzzMpBbhDLjDpLCfkwaDM", "Invalid first byte."),]
        [InlineData("5HpHagT65TZzG1PH3CSu63k8DbpvD8s5ip4nEB3kEsreAnchuDf", "Given WIF is uncompressed and is not extended with a version byte."),]
        [InlineData("KwDiBf89QgGbjEhKnhXJuH7LrciVrZi3qYjgd9M7rFU73sVHnoWn", "Given WIF is normal, and not extended with a version byte."),]
        [InlineData("L53fCHmQhbNp1B4JipfBtfeHZH7cAibzG9oK19XfiFzxHibjcqi9", "Wrong byte was used to extend this private key."),]
        [InlineData("Cm2qEgpogzPHzuQn2HTbkcWK85uB5zkhG", "Invalid WIF bytes length."),]
        public void Decode_FormatExceptionTest(string wif, string expectedMessage)
        {
            BIP0178 bip = new BIP0178();
            Exception ex = Assert.Throws<FormatException>(() => bip.Decode(wif));
            Assert.Contains(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData("5HpHagT65TZzG1PH3CSu63k8DbpvD8s5ip4nEB3kEsreAnchuDf", "Given WIF is uncompressed and is not extended with a version byte.")]
        [InlineData("L53fCHmQhbNp1B4JipfBtfeHZH7cAibzG9oK19XfiFzxHgENMRnQ", "Invalid compressed byte")]
        [InlineData("KwDiBf89QgGbjEhKnhXJuH7LrciVrZi3qYjgd9M7rFU73sVHnoWn", "Given WIF is normal, and not extended with a version byte.")]
        [InlineData("Ko3ibZusUhdSpDe9A13PdwDGXPNGfJYpkjQCjp7v5UZEGb99cCUR", "Wrong byte was used to extend this private key.")]
        [InlineData("Cm2qEgpogzPHzuQn2HTbkcWK85uB5zkhG", "Invalid WIF bytes length.")]
        public void DecodeElectrumVersionedWif_FormatExceptionTest(string wif, string expectedMessage)
        {
            BIP0178 bip = new BIP0178();
            Exception ex = Assert.Throws<FormatException>(() => bip.DecodeElectrumVersionedWif(wif));
            Assert.Contains(expectedMessage, ex.Message);
        }


        [Theory]
        [MemberData(nameof(GetDecodeCases))]
        public void EncodeTest(byte[] bytes, string P2PKH, string P2WPKH, string P2WPKH_P2SH,
                               string P2WPKH_Electrum, string P2WPKH_P2SH_Electrum,
                               string P2SH_Electrum, string P2WSH_Electrum, string P2WSH_P2SH_Electrum)
        {
            BIP0178 bip = new BIP0178();
            using PrivateKey key = new PrivateKey(bytes);

            Assert.Equal(P2PKH, bip.Encode(key, BIP0178.VersionSuffix.P2PKH));
            Assert.Equal(P2WPKH, bip.Encode(key, BIP0178.VersionSuffix.P2WPKH));
            Assert.Equal(P2WPKH_P2SH, bip.Encode(key, BIP0178.VersionSuffix.P2WPKH_P2SH));

            Assert.Equal(P2WPKH_Electrum, bip.EncodeElectrumVersionedWif(key, BIP0178.ElectrumVersionPrefix.P2WPKH));
            Assert.Equal(P2WPKH_P2SH_Electrum, bip.EncodeElectrumVersionedWif(key, BIP0178.ElectrumVersionPrefix.P2WPKH_P2SH));
            Assert.Equal(P2SH_Electrum, bip.EncodeElectrumVersionedWif(key, BIP0178.ElectrumVersionPrefix.P2SH));
            Assert.Equal(P2WSH_Electrum, bip.EncodeElectrumVersionedWif(key, BIP0178.ElectrumVersionPrefix.P2WSH));
            Assert.Equal(P2WSH_P2SH_Electrum, bip.EncodeElectrumVersionedWif(key, BIP0178.ElectrumVersionPrefix.P2WSH_P2SH));
        }


        [Fact]
        public void EncodeWithScriptTypeTest()
        {
            BIP0178 bip = new BIP0178();
            byte[] ba = Helper.HexToBytes("e9873d79c6d87dc0fb6a5778633389f4453213303da61f20bd67fc233aa33262");
            using var key = new PrivateKey(ba);

            string actual = bip.EncodeWithScriptType(key, BIP0178.ElectrumVersionPrefix.P2WPKH);
            string expected = "p2wpkh:L53fCHmQhbNp1B4JipfBtfeHZH7cAibzG9oK19XfiFzxHgAkz6JK";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EncodeWithScriptType_NoUnderscoreTest()
        {
            BIP0178 bip = new BIP0178();
            byte[] ba = Helper.HexToBytes("e9873d79c6d87dc0fb6a5778633389f4453213303da61f20bd67fc233aa33262");
            using var key = new PrivateKey(ba);

            string actual = bip.EncodeWithScriptType(key, BIP0178.ElectrumVersionPrefix.P2WPKH_P2SH);
            string expected = "p2wpkh-p2sh:L53fCHmQhbNp1B4JipfBtfeHZH7cAibzG9oK19XfiFzxHgAkz6JK";

            Assert.Equal(expected, actual);
        }
    }
}
