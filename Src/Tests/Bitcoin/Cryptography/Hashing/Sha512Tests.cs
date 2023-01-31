// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class Sha512Tests
    {
        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetCommonHashCases), parameters: "SHA512", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHashTest(byte[] message, byte[] expectedHash)
        {
            byte[] actualHash = Sha512.ComputeHash(message);
            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_AMillionATest()
        {
            byte[] actualHash = Sha512.ComputeHash(HashTestCaseHelper.GetAMillionA());
            byte[] expectedHash = Helper.HexToBytes("e718483d0ce769644e2e42c7bc15b4638e1f98b13b2044285632a803afa973ebde0ff244877ea60a4cb0432ce577c31beb009c5c2c49aa2e4eadb217ad8cc09b");

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_ReuseTest()
        {
            byte[] msg1 = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
            byte[] msg2 = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy cog");
            byte[] exp1 = Helper.HexToBytes("07e547d9586f6a73f73fbac0435ed76951218fb7d0c8d788a309d785436bbb642e93a252a954f23912547d1e8a3b5ed6e1bfd7097821233fa0538f3db854fee6");
            byte[] exp2 = Helper.HexToBytes("3eeee1d0e11733ef152a6c29503b3ae20c4f1f3cda4cb26f1bc1a41f91c7fe4ab3bd86494049e201c4bd5155f31ecb7a3c8606843c4cc8dfcab7da11c8ae5045");

            byte[] act1 = Sha512.ComputeHash(msg1);
            byte[] act2 = Sha512.ComputeHash(msg2);

            Assert.Equal(exp1, act1);
            Assert.Equal(exp2, act2);
        }

        [Fact]
        public void ComputeHash_WithIndexTest()
        {
            byte[] data = Helper.HexToBytes("123fab54686520717569636b2062726f776e20666f78206a756d7073206f76657220746865206c617a7920646f67f3a25c92");
            byte[] actualHash = Sha512.ComputeHash(data, 3, 43);
            byte[] expectedHash = Helper.HexToBytes("07e547d9586f6a73f73fbac0435ed76951218fb7d0c8d788a309d785436bbb642e93a252a954f23912547d1e8a3b5ed6e1bfd7097821233fa0538f3db854fee6");

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_DoubleTest()
        {
            var data = Helper.HexToBytes("fb8049137747e712628240cf6d7056ea2870170cb7d9bc713d91e901b514c6ae7d7dda3cd03ea1b99cf85046a505f3590541123d3f8f2c22c4d7d6e65de65c4ebb9251f09619");
            byte[] actualHash = Sha512.ComputeHashTwice(data);
            byte[] expectedHash = Helper.HexToBytes("00920ac1123d211929f0ef40d0ab3775abc987c606219301eb5995ff1053043a3c24906e88a74e4b2d6e1f6aa830a4f8b7e5e6edb7d090d37033abe45153a8e2");

            Assert.Equal(expectedHash, actualHash);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetCommonHashCases), parameters: "SHA512", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHashTwiceTest(byte[] message, byte[] expectedHash)
        {
            byte[] actualHash = Sha512.ComputeHashTwice(message);
            expectedHash = Sha512.ComputeHash(expectedHash);
            Assert.Equal(expectedHash, actualHash);
        }


        [Fact]
        public void ComputeHash_ExceptionsTest()
        {
            byte[] goodBa = { 1, 2, 3 };

            Assert.Throws<ArgumentNullException>(() => Sha512.ComputeHash(null));
            Assert.Throws<ArgumentNullException>(() => Sha512.ComputeHash(null, 0, 1));
            Assert.Throws<IndexOutOfRangeException>(() => Sha512.ComputeHash(goodBa, 0, 5));
            Assert.Throws<IndexOutOfRangeException>(() => Sha512.ComputeHash(goodBa, 10, 1));
        }


        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetNistShortCases), parameters: "Sha512", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NistShortTest(byte[] message, byte[] expected)
        {
            byte[] actual = Sha512.ComputeHash(message);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetNistLongCases), parameters: "Sha512", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NistLongTest(byte[] message, byte[] expected)
        {
            byte[] actual = Sha512.ComputeHash(message);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void ComputeHash_NistMonteCarloTest()
        {
            byte[] seed = Helper.HexToBytes("5c337de5caf35d18ed90b5cddfce001ca1b8ee8602f367e7c24ccca6f893802fb1aca7a3dae32dcd60800a59959bc540d63237876b799229ae71a2526fbc52cd");
            JObject jObjs = Helper.ReadResource<JObject>("Sha512NistTestData");
            int size = 64;
            byte[] toHash = new byte[3 * size];

            byte[] M0 = seed;
            byte[] M1 = seed;
            byte[] M2 = seed;

            foreach (var item in jObjs["MonteCarlo"])
            {
                byte[] expected = Helper.HexToBytes(item.ToString());
                for (int i = 0; i < 1000; i++)
                {
                    Buffer.BlockCopy(M0, 0, toHash, 0, size);
                    Buffer.BlockCopy(M1, 0, toHash, size, size);
                    Buffer.BlockCopy(M2, 0, toHash, size * 2, size);

                    M0 = M1;
                    M1 = M2;
                    M2 = Sha512.ComputeHash(toHash);
                }
                M0 = M2;
                M1 = M2;

                Assert.Equal(expected, M2);
            }
        }
    }
}
