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
    public class BIP0085Tests
    {
        private const string Xprv = "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb";

        [Fact]
        public void Constructor_ExceptionTest()
        {
            string xprv = "xprv9wNUHWVKZVkpYBCLJsjAbde4JXA5Zbaqcm6BXBh9W8VPRwH4AHjkhgN75CVah892f9sZzBKt5mbaDgDnugLEygoyDPdMvhycHN9buGLeZx8";
            Exception ex = Assert.Throws<ArgumentException>(() => new BIP0085(xprv));
            Assert.Contains("BIP-85 is only defined for master extended keys", ex.Message);
        }

        public static IEnumerable<object[]> GetSimpleCases()
        {
            yield return new object[]
            {
                "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb",
                new BIP0032Path("m/83696968'/0'/0'"),
                Helper.HexToBytes("efecfbccffea313214232d29e71563d941229afb4338c21f9517c41aaa0d16f00b83d2a09ef747e7a64e8e2bd5a14869e693da66ce94ac2da570ab7ee48618f7")
            };
            yield return new object[]
            {
                "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb",
                new BIP0032Path("m/83696968'/0'/1'"),
                Helper.HexToBytes("70c6e3e8ebee8dc4c0dbba66076819bb8c09672527c4277ca8729532ad711872218f826919f6b67218adde99018a6df9095ab2b58d803b5b93ec9802085a690e")
            };
        }
        [Theory]
        [MemberData(nameof(GetSimpleCases))]
        public void DeriveEntropyTest(string xprv, BIP0032Path path, byte[] expected)
        {
            using BIP0085 bip85 = new BIP0085(xprv);
            byte[] actual = bip85.DeriveEntropy(path);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DeriveEntropy_ExceptionTest()
        {
            var bip85 = new BIP0085(Xprv);
            var badPath = new BIP0032Path(1);
            var badPath2 = new BIP0032Path("m/1'/0/0'");

            Assert.Throws<ArgumentNullException>(() => bip85.DeriveEntropy(null));
            Assert.Throws<ArgumentException>(() => bip85.DeriveEntropy(badPath));
            Assert.Throws<ArgumentException>(() => bip85.DeriveEntropy(badPath2));

            bip85.Dispose();
            Assert.Throws<ObjectDisposedException>(() => bip85.DeriveEntropy(new BIP0032Path(uint.MaxValue)));
        }

        public static IEnumerable<object[]> GetBip39Cases()
        {
            yield return new object[]
            {
                "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb",
                BIP0039.WordLists.English,
                12,
                0,
                Helper.HexToBytes("6250b68daf746d12a24d58b4787a714b"),
                "girl mad pet galaxy egg matter matrix prison refuse sense ordinary nose"
            };
            yield return new object[]
            {
                "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb",
                BIP0039.WordLists.English,
                18,
                0,
                Helper.HexToBytes("938033ed8b12698449d4bbca3c853c66b293ea1b1ce9d9dc"),
                "near account window bike charge season chef number sketch tomorrow excuse sniff circle vital hockey outdoor supply token"
            };
            yield return new object[]
            {
                "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb",
                BIP0039.WordLists.English,
                24,
                0,
                Helper.HexToBytes("ae131e2312cdc61331542efe0d1077bac5ea803adf24b313a4f0e48e9c51f37f"),
                "puppy ocean match cereal symbol another shed magic wrap hammer bulb intact gadget divorce twin tonight reason outdoor destroy simple truth cigar social volcano"
            };
        }
        [Theory]
        [MemberData(nameof(GetBip39Cases))]
        public void DeriveEntropyBip39Test(string xprv, BIP0039.WordLists lang, int wordLen, uint index, byte[] expectedEnt, string expectedMn)
        {
            using BIP0085 bip85 = new BIP0085(xprv);
            byte[] actualEnt = bip85.DeriveEntropyBip39(lang, wordLen, index);

            using BIP0039 bip39 = new BIP0039(actualEnt, lang);
            string actualMn = bip39.ToMnemonic();

            Assert.Equal(expectedEnt, actualEnt);
            Assert.Equal(expectedMn, actualMn);
        }

        [Fact]
        public void DeriveEntropyBip39_ExceptionTest()
        {
            var bip85 = new BIP0085(Xprv);
            var wl = (BIP0039.WordLists)1000;

            Exception ex = Assert.Throws<ArgumentException>(() => bip85.DeriveEntropyBip39(wl, 12, 0));
            Assert.Contains("Word-list is not defined.", ex.Message);

            ex = Assert.Throws<ArgumentException>(() => bip85.DeriveEntropyBip39(BIP0039.WordLists.English, 0, 0));
            Assert.Contains("Invalid seed length", ex.Message);

            bip85.Dispose();
            Assert.Throws<ObjectDisposedException>(() => bip85.DeriveEntropyBip39(BIP0039.WordLists.English, 12, 0));
        }

        public static IEnumerable<object[]> GetHdSeedWifCases()
        {
            yield return new object[]
            {
                "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb",
                0,
                Helper.HexToBytes("7040bb53104f27367f317558e78a994ada7296c6fde36a364e5baf206e502bb1")
            };
        }
        [Theory]
        [MemberData(nameof(GetHdSeedWifCases))]
        public void DeriveEntropyHdSeedWifTest(string xprv, uint index, byte[] expected)
        {
            using BIP0085 bip85 = new BIP0085(xprv);
            byte[] actual = bip85.DeriveEntropyHdSeedWif(index);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DeriveEntropyHdSeedWif_ExceptionTest()
        {
            var bip85 = new BIP0085(Xprv);
            bip85.Dispose();
            Assert.Throws<ObjectDisposedException>(() => bip85.DeriveEntropyHdSeedWif(1));
        }

        public static IEnumerable<object[]> GetXprvCases()
        {
            yield return new object[]
            {
                "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb",
                0,
                "xprv9s21ZrQH143K2srSbCSg4m4kLvPMzcWydgmKEnMmoZUurYuBuYG46c6P71UGXMzmriLzCCBvKQWBUv3vPB3m1SATMhp3uEjXHJ42jFg7myX"
            };
        }
        [Theory]
        [MemberData(nameof(GetXprvCases))]
        public void DeriveEntropyXprvTest(string xprv, uint index, string expected)
        {
            using BIP0085 bip85 = new BIP0085(xprv);
            string actual = bip85.DeriveEntropyXprv(index);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DeriveEntropyXprv_ExceptionTest()
        {
            var bip85 = new BIP0085(Xprv);
            bip85.Dispose();
            Assert.Throws<ObjectDisposedException>(() => bip85.DeriveEntropyXprv(1));
        }

        public static IEnumerable<object[]> GetHexCases()
        {
            yield return new object[]
            {
                "xprv9s21ZrQH143K2LBWUUQRFXhucrQqBpKdRRxNVq2zBqsx8HVqFk2uYo8kmbaLLHRdqtQpUm98uKfu3vca1LqdGhUtyoFnCNkfmXRyPXLjbKb",
                64,
                0,
                "492db4698cf3b73a5a24998aa3e9d7fa96275d85724a91e71aa2d645442f878555d078fd1f1f67e368976f04137b1f7a0d19232136ca50c44614af72b5582a5c"
            };
        }
        [Theory]
        [MemberData(nameof(GetHexCases))]
        public void DeriveEntropyHexTest(string xprv, int byteLen, uint index, string expected)
        {
            using BIP0085 bip85 = new BIP0085(xprv);
            string actual = bip85.DeriveEntropyHex(byteLen, index);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DeriveEntropyHex_ExceptionTest()
        {
            var bip85 = new BIP0085(Xprv);

            Assert.Throws<ArgumentOutOfRangeException>(() => bip85.DeriveEntropyHex(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => bip85.DeriveEntropyHex(15, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => bip85.DeriveEntropyHex(65, 0));

            bip85.Dispose();
            Assert.Throws<ObjectDisposedException>(() => bip85.DeriveEntropyHex(16, 0));
        }
    }
}
