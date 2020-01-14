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
    public class Ripemd160Tests
    {
        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetCommonHashCases), "RIPEMD160", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHashTest(byte[] message, byte[] expectedHash)
        {
            using Ripemd160 rip = new Ripemd160();
            byte[] actualHash = rip.ComputeHash(message);
            Assert.Equal(expectedHash, actualHash);
        }

        private static byte[] GetBytes(int len)
        {
            byte[] result = new byte[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = (byte)(i + 1);
            }

            return result;
        }
        public static TheoryData GetProgressiveCase()
        {
            TheoryData<byte[], byte[]> result = new TheoryData<byte[], byte[]>();
            int len = 1;
            // Hash values were computed using .Net framework 4.7.2 System.Security.Cryptography.RIPEMD160Managed
            foreach (var item in Helper.ReadResource<JArray>("Ripemd160ProgressiveTestData"))
            {
                byte[] msgBytes = GetBytes(len++);
                byte[] hashBytes = Helper.HexToBytes(item.ToString());

                result.Add(msgBytes, hashBytes);
            }
            return result;
        }
        [Theory]
        [MemberData(nameof(GetProgressiveCase))]
        public void ComputeHash_ProgressiveTest(byte[] message, byte[] expectedHash)
        {
            using Ripemd160 rip = new Ripemd160();
            byte[] actualHash = rip.ComputeHash(message);
            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_AMillionATest()
        {
            using Ripemd160 rip = new Ripemd160();
            byte[] actualHash = rip.ComputeHash(HashTestCaseHelper.GetAMillionA());
            byte[] expectedHash = Helper.HexToBytes("52783243c1697bdbe16d37f97f68f08325dc1528");

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_ReuseTest()
        {
            // From https://en.wikipedia.org/wiki/RIPEMD#RIPEMD-160_hashes
            byte[] msg1 = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
            byte[] msg2 = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy cog");
            byte[] exp1 = Helper.HexToBytes("37f332f68db77bd9d7edd4969571ad671cf9dd3b");
            byte[] exp2 = Helper.HexToBytes("132072df690933835eb8b6ad0b77e7b6f14acad7");

            using Ripemd160 rip = new Ripemd160();
            byte[] act1 = rip.ComputeHash(msg1);
            byte[] act2 = rip.ComputeHash(msg2);

            Assert.Equal(exp1, act1);
            Assert.Equal(exp2, act2);
        }

        [Fact]
        public void ComputeHash_WithIndexTest()
        {
            using Ripemd160 rip = new Ripemd160();
            byte[] data = Helper.HexToBytes("123fab54686520717569636b2062726f776e20666f78206a756d7073206f76657220746865206c617a7920646f67f3a25c92");
            byte[] actualHash = rip.ComputeHash(data, 3, 43);
            byte[] expectedHash = Helper.HexToBytes("37f332f68db77bd9d7edd4969571ad671cf9dd3b");

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_DoubleTest()
        {
            using Ripemd160 rip = new Ripemd160(true);
            byte[] data = Helper.HexToBytes("fb8049137747e712628240cf6d7056ea2870170cb7d9bc713d91e901b514c6ae7d7dda3cd03ea1b99cf85046a505f3590541123d3f8f2c22c4d7d6e65de65c4ebb9251f09619");
            byte[] actualHash = rip.ComputeHash(data);
            byte[] expectedHash = Helper.HexToBytes("dd0ea2c8b8f2fba4e1a1b58080fb59b0350c7aba");

            Assert.Equal(expectedHash, actualHash);
        }


        [Fact]
        public void ComputeHash_ExceptionsTest()
        {
            byte[] goodBa = { 1, 2, 3 };
            Ripemd160 rip = new Ripemd160();

            Assert.Throws<ArgumentNullException>(() => rip.ComputeHash(null));
            Assert.Throws<ArgumentNullException>(() => rip.ComputeHash(null, 0, 1));
            Assert.Throws<IndexOutOfRangeException>(() => rip.ComputeHash(goodBa, 0, 5));
            Assert.Throws<IndexOutOfRangeException>(() => rip.ComputeHash(goodBa, 10, 1));

            rip.Dispose();
            Assert.Throws<ObjectDisposedException>(() => rip.ComputeHash(goodBa));
            Assert.Throws<ObjectDisposedException>(() => rip.ComputeHash(goodBa, 0, 2));
        }
    }
}
