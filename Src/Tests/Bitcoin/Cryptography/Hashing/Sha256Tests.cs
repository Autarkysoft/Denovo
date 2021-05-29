// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class Sha256Tests
    {
        public static IEnumerable<object[]> GetChecksumCases()
        {
            yield return new object[] { Array.Empty<byte>(), new byte[4] { 0x5d, 0xf6, 0xe0, 0xe2 } };
            yield return new object[] { new byte[1], new byte[4] { 0x14, 0x06, 0xe0, 0x58 } };
            // From Sha256NistTestData.json file but hashed twice
            yield return new object[]
            {
                Helper.HexToBytes("6dd6efd6f6caa63b729aa8186e308bc1bda06307c05a2c0ae5a3684e6e460811748690dc2b58775967cfcc645fd82064b1279fdca771803db9dca0ff53"),
                new byte[4] { 0xea, 0x90, 0x88, 0xf2 }
            };
            yield return new object[]
            {
                Helper.HexToBytes("5a86b737eaea8ee976a0a24da63e7ed7eefad18a101c1211e2b3650c5187c2a8a650547208251f6d4237e661c7bf4c77f335390394c37fa1a9f9be836ac28509"),
                new byte[4] { 0x3c, 0x10, 0xd5, 0xfe }
            };
            yield return new object[]
            {
                Helper.HexToBytes("451101250ec6f26652249d59dc974b7361d571a8101cdfd36aba3b5854d3ae086b5fdd4597721b66e3c0dc5d8c606d9657d0e323283a5217d1f53f2f284f57b85c8a61ac8924711f895c5ed90ef17745ed2d728abd22a5f7a13479a462d71b56c19a74a40b655c58edfe0a188ad2cf46cbf30524f65d423c837dd1ff2bf462ac4198007345bb44dbb7b1c861298cdf61982a833afc728fae1eda2f87aa2c9480858bec"),
                new byte[4] { 0x5e, 0x28, 0x7b, 0x15 }
            };
        }
        [Theory]
        [MemberData(nameof(GetChecksumCases))]
        public void ComputeChecksumTest(byte[] data, byte[] expected)
        {
            using Sha256 sha = new();
            byte[] actual = sha.ComputeChecksum(data);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ComputeChecksum_ExceptionTest()
        {
            Sha256 sha = new();
            Assert.Throws<ArgumentNullException>(() => sha.ComputeChecksum(null));
            sha.Dispose();
            Assert.Throws<ObjectDisposedException>(() => sha.ComputeChecksum(Array.Empty<byte>()));
        }

        [Fact]
        public void ComputeShortIdKeyTest()
        {
            // A priliminary test making sure the method works as we think it should.
            // This will be replaced when we find out some examples of ShortTxIds!
            using Sha256 sha = new();
            var rand = new Random();
            Span<byte> data = new byte[88];
            rand.NextBytes(data);

            ulong nonce = data[87]
                         | ((ulong)data[86] << 8)
                         | ((ulong)data[85] << 16)
                         | ((ulong)data[84] << 24)
                         | ((ulong)data[83] << 32)
                         | ((ulong)data[82] << 40)
                         | ((ulong)data[81] << 48)
                         | ((ulong)data[80] << 56);

            byte[] actual = sha.ComputeShortIdKey(data.Slice(0, 80).ToArray(), nonce);
            byte[] expected = ((Span<byte>)sha.ComputeHash(data.ToArray())).Slice(0, 16).ToArray();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetCommonHashCases), parameters: "SHA256", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHashTest(byte[] message, byte[] expectedHash)
        {
            using Sha256 sha = new();
            byte[] actualHash = sha.ComputeHash(message);
            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_AMillionATest()
        {
            using Sha256 sha = new();
            byte[] actualHash = sha.ComputeHash(HashTestCaseHelper.GetAMillionA());
            byte[] expectedHash = Helper.HexToBytes("cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0");

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_ReuseTest()
        {
            byte[] msg1 = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
            byte[] msg2 = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy cog");
            byte[] exp1 = Helper.HexToBytes("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");
            byte[] exp2 = Helper.HexToBytes("e4c4d8f3bf76b692de791a173e05321150f7a345b46484fe427f6acc7ecc81be");

            using Sha256 sha = new();
            byte[] act1 = sha.ComputeHash(msg1);
            byte[] act2 = sha.ComputeHash(msg2);

            Assert.Equal(exp1, act1);
            Assert.Equal(exp2, act2);
        }

        [Fact]
        public void ComputeHash_WithIndexTest()
        {
            using Sha256 sha = new();
            byte[] data = Helper.HexToBytes("123fab54686520717569636b2062726f776e20666f78206a756d7073206f76657220746865206c617a7920646f67f3a25c92");
            byte[] actualHash = sha.ComputeHash(data, 3, 43);
            byte[] expectedHash = Helper.HexToBytes("d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592");

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void ComputeHash_DoubleTest()
        {
            using Sha256 sha = new();
            var data = Helper.HexToBytes("fb8049137747e712628240cf6d7056ea2870170cb7d9bc713d91e901b514c6ae7d7dda3cd03ea1b99cf85046a505f3590541123d3f8f2c22c4d7d6e65de65c4ebb9251f09619");
            byte[] actualHash = sha.ComputeHashTwice(data);
            byte[] expectedHash = Helper.HexToBytes("d2cee8d3cfaf1819c55cce1214d01cdef1d97446719ccfaad4d76d912a8126f9");

            Assert.Equal(expectedHash, actualHash);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetCommonHashCases), parameters: "SHA256", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHashTwiceTest(byte[] message, byte[] expectedHash)
        {
            using Sha256 sha = new();
            byte[] actualHash = sha.ComputeHashTwice(message);
            expectedHash = sha.ComputeHash(expectedHash);
            Assert.Equal(expectedHash, actualHash);
        }


        [Fact]
        public void ComputeHash_ExceptionsTest()
        {
            byte[] goodBa = { 1, 2, 3 };
            Sha256 sha = new();

            Assert.Throws<ArgumentNullException>(() => sha.ComputeHash(null));
            Assert.Throws<ArgumentNullException>(() => sha.ComputeHash(null, 0, 1));
            Assert.Throws<IndexOutOfRangeException>(() => sha.ComputeHash(goodBa, 0, 5));
            Assert.Throws<IndexOutOfRangeException>(() => sha.ComputeHash(goodBa, 10, 1));

            sha.Dispose();
            Assert.Throws<ObjectDisposedException>(() => sha.ComputeHash(goodBa));
            Assert.Throws<ObjectDisposedException>(() => sha.ComputeHash(goodBa, 0, 2));
        }


        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetNistShortCases), parameters: "Sha256", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NistShortTest(byte[] message, byte[] expected)
        {
            using Sha256 sha = new();
            byte[] actual = sha.ComputeHash(message);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetNistLongCases), parameters: "Sha256", MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NistLongTest(byte[] message, byte[] expected)
        {
            using Sha256 sha = new();
            byte[] actual = sha.ComputeHash(message);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void ComputeHash_NistMonteCarloTest()
        {
            byte[] seed = Helper.HexToBytes("6d1e72ad03ddeb5de891e572e2396f8da015d899ef0e79503152d6010a3fe691");
            JObject jObjs = Helper.ReadResource<JObject>("Sha256NistTestData");
            int size = 32;
            byte[] toHash = new byte[3 * size];

            byte[] M0 = seed;
            byte[] M1 = seed;
            byte[] M2 = seed;

            using Sha256 sha = new();

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
                    M2 = sha.ComputeHash(toHash);
                }
                M0 = M2;
                M1 = M2;

                Assert.Equal(expected, M2);
            }
        }



        // The original MAJ() and CH() functions are defined differently in RFC documentation.
        // The two following tests act as proof that the changed functions are giving the same results.
        private static uint CH_Original(uint x, uint y, uint z)
        {
            return (x & y) ^ ((~x) & z);
        }
        private static uint CH_Changed(uint x, uint y, uint z)
        {
            return z ^ (x & (y ^ z));
        }
        [Fact]
        public void CH_Test()
        {
            for (uint x = 0b00; x <= 0b11; x++)
            {
                for (uint y = 0b00; y <= 0b11; y++)
                {
                    for (uint z = 0b00; z <= 0b11; z++)
                    {
                        Assert.Equal(CH_Original(x, y, z), CH_Changed(x, y, z));
                    }
                }
            }
        }


        private static uint MAJ_Original(uint x, uint y, uint z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }
        private static uint MAJ_Changed(uint x, uint y, uint z)
        {
            return (x & y) | (z & (x | y));
        }
        [Fact]
        public void MAJ_Test()
        {
            for (uint x = 0b00; x <= 0b11; x++)
            {
                for (uint y = 0b00; y <= 0b11; y++)
                {
                    for (uint z = 0b00; z <= 0b11; z++)
                    {
                        Assert.Equal(MAJ_Original(x, y, z), MAJ_Changed(x, y, z));
                    }
                }
            }
        }

        [Fact]
        public void MessageLengthTest()
        {
            int len = 0x493657ad;
            long msgLen = (long)len << 3; // *8

            byte[] first = new byte[8];
            byte[] second = new byte[8];

            // Message length is an Int32 type multiplied by 8 to convert to bit length for padding 
            // so the maximum result is:
            // int.MaxValue * 8 = 17179869176 or int.MaxValue << 3
            // 00000000_00000000_00000000_00000011_11111111_11111111_11111111_11111000
            // This means the first 3 (out of 8) bytes will always be zero

            first[7] = (byte)msgLen;
            first[6] = (byte)(msgLen >> 8);
            first[5] = (byte)(msgLen >> 16);
            first[4] = (byte)(msgLen >> 24);
            first[3] = (byte)(msgLen >> 32);
            first[2] = (byte)(msgLen >> 40); // must be zero
            first[1] = (byte)(msgLen >> 48); // must be zero
            first[0] = (byte)(msgLen >> 56); // must be zero

            // ****** The alternative way: ******/
            // msgLen = len << 3
            // [7] = (byte)msgLen           = (byte)(len << 3)
            // [6] = (byte)(msgLen >> 8)    = (byte)((len << 3) >> 8) = (byte)(len >> 5)
            // ...
            // [3] = (byte)(msgLen >> 32)   = (byte)((len << 3) >> 32) = (byte)(len >> 29)

            // [2] = (byte)(msgLen >> 40)   = (byte)((len << 3) >> 40) = (byte)(len >> 37)
            // Assuming len were Int64, 37 bit shift is getting rid of the first 32+3 bits so it must be zero
            second[7] = (byte)(len << 3);
            second[6] = (byte)(len >> 5);
            second[5] = (byte)(len >> 13);
            second[4] = (byte)(len >> 21);
            second[3] = (byte)(len >> 29);
            //second[2] = (byte)(len >> 37); shifts are bigger than 32, won't work as long as len is Int32
            //second[1] = (byte)(len >> 45); 
            //second[0] = (byte)(len >> 53); 

            Assert.Equal(first, second);
            Assert.Equal(0, first[0]);
            Assert.Equal(0, first[1]);
            Assert.Equal(0, first[2]);
        }


        // All the data for Tagged Hashes are computed using System.Random and hashes using
        // System.Security.Cryptography.SHA256 in .Net Framework 4.7.2

        [Fact]
        public void ComputeTaggedHash_BIP0340auxTest()
        {
            using Sha256 sha = new();
            byte[] aux = Helper.HexToBytes("fac9cdf2f2b82e7d6ab47656ea4a294ab886553e6fdb08b49eb9665479be65c7");

            byte[] actual = sha.ComputeTaggedHash("BIP0340/aux", aux);
            byte[] expected = Helper.HexToBytes("7db9b90018a2e4151d4eae8c9ab51975ab4fbc1ad59ca37330c30f41bd473a9d");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ComputeTaggedHash_BIP0340challengeTest()
        {
            using Sha256 sha = new();
            byte[] r = Helper.HexToBytes("c044e32bbe7a7973ed85e6645f648c60a8c3ea5d1989de40aa4b5eb2d6d4f1c1");
            byte[] pub = Helper.HexToBytes("13f714f34a70147d5b2daecb30855b198d17b9e6e20c9f3766a281721bc69d19");
            byte[] data = Helper.HexToBytes("9324506850981637af6d01ebdb120f24ef4525be1a0ade0757dd8da7efa16195");

            byte[] actual = sha.ComputeTaggedHash("BIP0340/challenge", r, pub, data);
            byte[] expected = Helper.HexToBytes("8cb35f776c45345ddcc4658f362d080f7e4e4dc0fc425ba53b08ada81b1f5251");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ComputeTaggedHash_BIP0340nonceTest()
        {
            using Sha256 sha = new();
            byte[] t = Helper.HexToBytes("c044e32bbe7a7973ed85e6645f648c60a8c3ea5d1989de40aa4b5eb2d6d4f1c1");
            byte[] pub = Helper.HexToBytes("13f714f34a70147d5b2daecb30855b198d17b9e6e20c9f3766a281721bc69d19");
            byte[] data = Helper.HexToBytes("9324506850981637af6d01ebdb120f24ef4525be1a0ade0757dd8da7efa16195");

            byte[] actual = sha.ComputeTaggedHash("BIP0340/nonce", t, pub, data);
            byte[] expected = Helper.HexToBytes("0ac9e997ceb2f96fe0d66da37df1e482695b11e38a18bd58f90d92a02ec48da9");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ComputeTaggedHash_GeneralTest()
        {
            using Sha256 sha = new();
            byte[] b1 = Helper.HexToBytes("7e12d002b106336a5ef75ec1871a9494c32fd0338dc8faa1cff751d51b42eb63");
            byte[] b2 = Helper.HexToBytes("bbbf2917e06ab87045b814fd21b38273ada86fca8adb4eb5182f32bab45d4e1c");
            byte[] b3 = Helper.HexToBytes("0bfb05c8f17db4587660eed4b4fc7f1c75de2873d220d8df7074feb037febce7");
            byte[] b4 = Helper.HexToBytes("6af7f125adb9822ff417532aed7a001baf1544d535db4aeb2225577d7e032078");

            byte[] actual = sha.ComputeTaggedHash("Foo", b1, b2, b3, b4);
            byte[] expected = Helper.HexToBytes("70da727b9fd90b4ed4449fa870dfb3e52962ef3f8a783611022eae2edce7c0ce");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ComputeTaggedHash_NullExceptionTest()
        {
            using Sha256 sha = new();

            Exception ex = Assert.Throws<ArgumentNullException>(() => sha.ComputeTaggedHash(null, new byte[32]));
            Assert.Contains("Tag can not be null.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => sha.ComputeTaggedHash("Foo"));
            Assert.Contains("The extra data can not be null or empty.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => sha.ComputeTaggedHash("Foo", null));
            Assert.Contains("The extra data can not be null or empty.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => sha.ComputeTaggedHash("Foo", Array.Empty<byte[]>()));
            Assert.Contains("The extra data can not be null or empty.", ex.Message);
        }

        public static IEnumerable<object[]> GetTaggedHashErrorCases()
        {
            yield return new object[]
            {
                "Foo",
                new byte[][] { new byte[32], new byte[31] },
                "Each additional data must be 32 bytes."
            };
            yield return new object[]
            {
                "BIP0340/aux",
                new byte[][] { new byte[32], new byte[32] },
                "BIP0340/aux tag needs 1 data input."
            };
            yield return new object[]
            {
                "BIP0340/challenge",
                new byte[][] { new byte[32], new byte[32] },
                "BIP0340/challenge tag needs 3 data inputs."
            };
            yield return new object[]
            {
                "BIP0340/nonce",
                new byte[][] { new byte[32], new byte[32] },
                "BIP0340/nonce tag needs 3 data inputs."
            };
        }
        [Theory]
        [MemberData(nameof(GetTaggedHashErrorCases))]
        public void ComputeTaggedHash_OutOfRangeExceptionTest(string tag, byte[][] data, string expError)
        {
            using Sha256 sha = new();

            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => sha.ComputeTaggedHash(tag, data));
            Assert.Contains(expError, ex.Message);
        }
    }
}
