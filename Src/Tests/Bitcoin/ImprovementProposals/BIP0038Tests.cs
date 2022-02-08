// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.ImprovementProposals;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class BIP0038Tests
    {
        public static IEnumerable<object[]> GetEncryptCases()
        {
            yield return new object[]
            {
                new PrivateKey(Helper.HexToBytes("cbf4b9f70470856bb4f40f80b87edb90865997ffee6df315ab166d713af433a5")),
                "TestingOneTwoThree",
                "6PYNKZ1EAgYgmQfmNVamxyXVWHzK5s6DGhwP4J5o44cvXdoY7sRzhtpUeo",
                "6PRVWUbkzzsbcVac2qwfssoUJAN1Xhrg6bNk8J7Nzm5H7kxEbn2Nh2ZoGg"
            };
            yield return new object[]
            {
                new PrivateKey(Helper.HexToBytes("09c2686880095b1a4c249ee3ac4eea8a014f11e6f986d0b5025ac1f39afbd9ae")),
                "Satoshi",
                "6PYLtMnXvfG3oJde97zRyLYFZCYizPU5T3LwgdYJz1fRhh16bU7u6PPmY7",
                "6PRNFFkZc2NZ6dJqFfhRoFNMR9Lnyj7dYGrzdgXXVMXcxoKTePPX1dWByq"
            };
            yield return new object[]
            {
                new PrivateKey(Helper.HexToBytes("64eeab5f9be2a01a8365a579511eb3373c87c40da6d2a25f05bda68fe077b66e")),
                "\u03D2\u0301\u0000\U00010400\U0001F4A9",
                "6PYVUZ9Y3Q75k52AdHHwGXgW39VvEEgey5N1ziNHWH9xowCxT76fQnnf6P",
                "6PRW5o9FLp4gJDDVqJQKJFTpMvdsSGJxMYHtHaQBF3ooa8mwD69bapcDQn"
            };
        }


        [Theory]
        [MemberData(nameof(GetEncryptCases))]
        public void DecryptTest(PrivateKey key, string pass, string encryptedComp, string encryptedUnComp)
        {
            using BIP0038 bip = new();
            PrivateKey actualKey = bip.Decrypt(encryptedComp, pass, out bool isComp1);
            PrivateKey actualKey2 = bip.Decrypt(encryptedUnComp, pass, out bool isComp2);

            Assert.True(isComp1);
            Assert.False(isComp2);

            byte[] expected = key.ToBytes();
            Assert.Equal(expected, actualKey.ToBytes());
            Assert.Equal(expected, actualKey2.ToBytes());
        }

        [Fact]
        public void Decrypt_NullExceptionTest()
        {
            using BIP0038 bip = new();
            string nstr = null;
            byte[] nba = null;

            Assert.Throws<ArgumentNullException>(() => bip.Decrypt(null, "", out bool isComp));
            Assert.Throws<ArgumentNullException>(() => bip.Decrypt(" ", "", out bool isComp));
            Assert.Throws<ArgumentNullException>(() => bip.Decrypt(null, new byte[1], out bool isComp));
            Assert.Throws<ArgumentNullException>(() => bip.Decrypt(" ", new byte[1], out bool isComp));
            Assert.Throws<ArgumentNullException>(() => bip.Decrypt("aaa", nstr, out bool isComp));
            Assert.Throws<ArgumentNullException>(() => bip.Decrypt("aaa", nba, out bool isComp));
        }

        [Fact]
        public void Decrypt_DisposedExceptionTest()
        {
            BIP0038 bip = new();
            bip.Dispose();
            Assert.Throws<ObjectDisposedException>(() => bip.Decrypt("aa", "", out bool isComp));
        }

        [Theory]
        [InlineData("$", "Input is not a valid base-58 encoded string.")]
        [InlineData("142viJrTYHA4TzryiEiuQkYk4Ay5TfpzqW", "Invalid encrypted bytes length.")]
        [InlineData("6Mc5gZg3pNQNMsnHDmZeRfhL1QnC24yBd1VERr3HSnKap5x2wcxYaJivvW", "Invalid prefix.")] // 0x0140
        [InlineData("6PRW5o9FLp4gJDDVqJQKJFTpMvdsSGJxMYHtHaQBF3ooa8mwD69bapcDQn", "Wrong password (derived address hash is not the same).")]
        public void Decrypt_FormatExceptionTest(string encrypted, string expError)
        {
            using BIP0038 bip = new();
            Exception ex = Assert.Throws<FormatException>(() => bip.Decrypt(encrypted, "wrong pass!", out _));
            Assert.Contains(expError, ex.Message);
        }


        [Theory]
        [MemberData(nameof(GetEncryptCases))]
        public void EncryptTest(PrivateKey key, string pass, string expectedComp, string expectedUncomp)
        {
            using BIP0038 bip = new();
            string actualComp = bip.Encrypt(key, pass, true);
            string actualUncomp = bip.Encrypt(key, pass, false);

            Assert.Equal(expectedComp, actualComp);
            Assert.Equal(expectedUncomp, actualUncomp);
        }

        [Fact]
        public void Encrypt_String_ExceptionTest()
        {
            BIP0038 bip = new();
            PrivateKey k = KeyHelper.Prv1;
            string nstr = null;

            Exception ex = Assert.Throws<ArgumentNullException>(() => bip.Encrypt(null, "a", true));
            Assert.Contains("Private key can not be null.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => bip.Encrypt(k, nstr, true));
            Assert.Contains("Password can not be null or empty.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => bip.Encrypt(k, "", true));
            Assert.Contains("Password can not be null or empty.", ex.Message);

            bip.Dispose();
            ex = Assert.Throws<ObjectDisposedException>(() => bip.Encrypt(k, "a", true));
            Assert.Contains("Instance was disposed.", ex.Message);
        }

        [Fact]
        public void Encrypt_Bytes_ExceptionTest()
        {
            BIP0038 bip = new();
            PrivateKey k = KeyHelper.Prv1;
            byte[] nba = null;

            Exception ex = Assert.Throws<ArgumentNullException>(() => bip.Encrypt(null, new byte[1], true));
            Assert.Contains("Private key can not be null.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => bip.Encrypt(k, nba, true));
            Assert.Contains("Password can not be null or empty.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => bip.Encrypt(k, Array.Empty<byte>(), true));
            Assert.Contains("Password can not be null or empty.", ex.Message);

            bip.Dispose();
            ex = Assert.Throws<ObjectDisposedException>(() => bip.Encrypt(k, new byte[1], true));
            Assert.Contains("Instance was disposed.", ex.Message);
        }
    }
}
