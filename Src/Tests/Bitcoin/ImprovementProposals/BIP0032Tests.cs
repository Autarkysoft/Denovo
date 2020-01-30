// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class BIP0032Tests
    {
        public static IEnumerable<object[]> GetByteCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("000102030405060708090a0b0c0d0e0f"),
                "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi"
            };
            yield return new object[]
            {
                Helper.HexToBytes("fffcf9f6f3f0edeae7e4e1dedbd8d5d2cfccc9c6c3c0bdbab7b4b1aeaba8a5a29f9c999693908d8a8784817e7b7875726f6c696663605d5a5754514e4b484542"),
                "xprv9s21ZrQH143K31xYSDQpPDxsXRTUcvj2iNHm5NUtrGiGG5e2DtALGdso3pGz6ssrdK4PFmM8NSpSBHNqPqm55Qn3LqFtT2emdEXVYsCzC2U"
            };
            yield return new object[]
            {
                Helper.HexToBytes("4b381541583be4423346c643850da4b320e46a87ae3d2a4e6da11eba819cd4acba45d239319ac14f863b8d5ab5a0d0c64d2e8a1e7d1457df2e5a3c51c73235be"),
                "xprv9s21ZrQH143K25QhxbucbDDuQ4naNntJRi4KUfWT7xo4EKsHt2QJDu7KXp1A3u7Bi1j8ph3EGsZ9Xvz9dGuVrtHHs7pXeTzjuxBrCmmhgC6"
            };
        }
        [Theory]
        [MemberData(nameof(GetByteCases))]
        public void Constructor_FromBytesTest(byte[] entropy, string expected)
        {
            using BIP0032 bip = new BIP0032(entropy);
            string actual = bip.ToBase58(false);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_FromBytes_NullExceptionTest()
        {
            byte[] nba = null;
            Assert.Throws<ArgumentNullException>(() => new BIP0032(nba));
        }
        [Fact]
        public void Initialize_FromBytes_OutOfRangeExceptionTest()
        {
            byte[] small = Helper.GetBytes(15);
            byte[] big = Helper.GetBytes(65);

            Assert.Throws<ArgumentOutOfRangeException>(() => new BIP0032(small));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BIP0032(big));
        }


        [Fact]
        public void Constructor_FromRngTest()
        {
            MockRng rng = new MockRng("000102030405060708090a0b0c0d0e0f");
            using BIP0032 bip = new BIP0032(rng, 16);

            string actual = bip.ToBase58(false);
            string expected = "xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_FromRng_NullExceptionTest()
        {
            IRandomNumberGenerator nRng = null;
            Assert.Throws<ArgumentNullException>(() => new BIP0032(nRng));
        }

        [Fact]
        public void Constructor_FromRng_OutOfRangeExceptionTest()
        {
            MockRng rng = new MockRng(new byte[0]);

            Assert.Throws<ArgumentOutOfRangeException>(() => new BIP0032(rng, 15));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BIP0032(rng, 65));
        }


        public static IEnumerable<object[]> GetInitializeFromStringCases()
        {
            // Test vectors are taken from https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki#Test_Vectors
            JArray jarr = Helper.ReadResource<JArray>("BIP0032TestData");
            foreach (var item in jarr)
            {
                byte[] ent = Helper.HexToBytes(item["Entropy"].ToString());
                foreach (var chain in item["Chains"])
                {
                    string xprv = chain["Xprv"].ToString();
                    string xpub = chain["Xpub"].ToString();

                    yield return new object[] { xprv, xpub };
                }
            }
        }
        [Theory]
        [MemberData(nameof(GetInitializeFromStringCases))]
        public void Constructor_FromStringTest(string xprv, string xpub)
        {
            using BIP0032 bipFromPrivate = new BIP0032(xprv);
            string actualXprv = bipFromPrivate.ToBase58(false);
            string actualXpub = bipFromPrivate.ToBase58(true);

            Assert.Equal(xprv, actualXprv);
            Assert.Equal(xpub, actualXpub);

            using BIP0032 bipFromPublic = new BIP0032(xpub);
            string actualXpub2 = bipFromPublic.ToBase58(true);

            Exception ex = Assert.Throws<ArgumentNullException>(() => bipFromPublic.ToBase58(false));
            Assert.Contains("Can not get extended private key from public key.", ex.Message);
            Assert.Equal(xpub, actualXpub2);
        }

        [Fact]
        public void Constructor_FromString_ArgumentExceptionTest()
        {
            NetworkType netType = (NetworkType)100;
            Exception ex = Assert.Throws<ArgumentException>(() =>
            {
                return new BIP0032("xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi",
                    netType);
            });
            Assert.Contains("Invalid network type.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_FromString_NullExceptionTest(string xprv)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => new BIP0032(xprv));
            Assert.Contains("Extended key can not be null or empty.", ex.Message);
        }

        [Theory]
        [InlineData("xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkg5hntwdZH6QYdrGVYWUCS66mqCW8kosZeVrCfDGU1sF5iJPrP", "Given key value is outside the defined range by the curve.")]
        [InlineData("xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29PWtmMb4tsV2435TLyLBKVoS4FRTtheBHVaYuSgKPKL5yKRDs", "Invalid public key format.")]
        public void Constructor_FromString_OutOfRangeExceptionTest(string xprv, string error)
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new BIP0032(xprv));
            Assert.Contains(error, ex.Message);
        }

        [Theory]
        [InlineData("abc", "Input is not a valid base-58 encoded string.")]
        [InlineData("xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHI", "Input is not a valid base-58 encoded string.")] // invalid base58 char
        [InlineData("xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBudW9Cd", "Invalid checksum.")] // invalid base58 checksum
        [InlineData("xprv1uNFY38Kz2jpT5Ud1yJDF", "Extended key length should be 78 bytes but it is 15 bytes.")]
        [InlineData("xpub7EZ7S93RrTePcLZA5eqPk", "Extended key length should be 78 bytes but it is 15 bytes.")]
        [InlineData("xprvan84nZvKw4RS4MKBgvjJ3qPFLDMPCusNf2XaEpe8ThPdrAPhJHuQjpcMgLFKUm2XefKMLvb1DaWymjVLAMtFhC1NVHneNCYb4ZSE8tBSvc8", "Unknown extended key version.")]
        [InlineData("xpubX17RC5TDmRyjGqPenxGJQyKytFBscNbE2FTB3D3k22vcixiqqqDfHcvqXbtczeSbaCDHqsc9rssHaFgUTbiuKE1TGzWsXj4Bpg2yBBE3xEK", "Unknown extended key version.")]
        [InlineData("xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChrLAT4frwYLduwdnp5eEwiT46mDXR9yuFLPy38oj2wvRfjZDocV", "The key has an invalid first byte, it should be 0 for private keys but it is 0x03.")]
        public void Constructor_FromString_FormatExceptionTest(string xprv, string error)
        {
            Exception ex = Assert.Throws<FormatException>(() => new BIP0032(xprv));
            Assert.Equal(error, ex.Message);
        }



        public static IEnumerable<object[]> GetPrivateKeyCases()
        {
            JArray jarr = Helper.ReadResource<JArray>("BIP0032TestData");
            foreach (var item in jarr)
            {
                byte[] ent = Helper.HexToBytes(item["Entropy"].ToString());
                foreach (var chain in item["Chains"])
                {
                    BIP0032Path path = new BIP0032Path(chain["Path"].ToString());
                    string pr1 = chain["Prv0"].ToString();
                    string pr2 = chain["Prv5"].ToString();
                    string pr3 = chain["Prv10"].ToString();
                    string pu1 = chain["Pub10"].ToString();
                    string pu2 = chain["Pub22"].ToString();
                    string pu3 = chain["Pub34"].ToString();

                    yield return new object[] { ent, path, new string[] { pr1, pr2, pr3 }, new string[] { pu1, pu2, pu3 } };
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetPrivateKeyCases))]
        public void GetPrivatePublicKeysTest(byte[] ent, BIP0032Path path, string[] expectedPrvKeys, string[] expectedPubKeys)
        {
            using BIP0032 bip32 = new BIP0032(ent);
            string[] actualPrvKeys = bip32.GetPrivateKeys(path, 3, 0, 5)
                                          .Select(x => x.ToWif(true))
                                          .ToArray();
            string[] actualPubKeys = bip32.GetPublicKeys(path, 3, 10, 12)
                                          .Select(x => Helper.BytesToHex(x.ToByteArray(true)))
                                          .ToArray();

            Assert.Equal(expectedPrvKeys, actualPrvKeys);
            Assert.Equal(expectedPubKeys, actualPubKeys);
        }


        [Fact]
        public void ToBase58_DisposedExceptionTest()
        {
            BIP0032 bip32 = new BIP0032("xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi");
            bip32.Dispose();

            Assert.Throws<ObjectDisposedException>(() => bip32.ToBase58(false));
        }

        [Fact]
        public void ToBase58_ArgumentExceptionTest()
        {
            BIP0032 bip32 = new BIP0032("xprv9s21ZrQH143K3QTDL4LXw2F7HEK3wJUD2nW2nRk4stbPy6cq3jPPqjiChkVvvNKmPGJxWUtg6LnF5kejMRNNU3TGtRBeJgk33yuGBxrMPHi");
            NetworkType netType = (NetworkType)100;

            Exception ex = Assert.Throws<ArgumentException>(() => bip32.ToBase58(false, netType));
            Assert.Contains("Network type is not defined.", ex.Message);
        }

        [Fact]
        public void ToBase58_NullExceptionTest()
        {
            using BIP0032 bip32 = new BIP0032("xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8");
            Exception ex = Assert.Throws<ArgumentNullException>(() => bip32.ToBase58(false));
            Assert.Contains("Can not get extended private key from public key.", ex.Message);
        }
    }
}
