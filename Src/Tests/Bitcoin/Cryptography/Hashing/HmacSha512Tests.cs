// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Collections.Generic;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class HmacSha512Tests
    {
        public static IEnumerable<object[]> GetCtorCases()
        {
            yield return new object[] { Array.Empty<byte>(), false };
            yield return new object[] { Helper.GetBytes(1), false };
            yield return new object[] { Helper.GetBytes(128), false };
            yield return new object[] { Helper.GetBytes(129), true };
        }
        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void ConstructorTest(byte[] key, bool isKeyHashed)
        {
            using var sysHmac = new System.Security.Cryptography.HMACSHA512(key);
            byte[] expectedKey = sysHmac.Key;

            // Set key in constructor:
            using (HmacSha512 hmac = new(key))
            {
                Assert.Equal(expectedKey, hmac.Key);
                if (!isKeyHashed)
                {
                    Assert.Equal(key, hmac.Key);
                    // Make sure small unhashed keys are cloned
                    Assert.NotSame(key, hmac.Key);
                }
                else
                {
                    Assert.NotEqual(key, hmac.Key);
                }
            }

            // Set key using the property:
            using (HmacSha512 hmac = new())
            {
                Assert.Null(hmac.Key);
                hmac.Key = key;
                if (!isKeyHashed)
                {
                    Assert.Equal(key, hmac.Key);
                    // Make sure small unhashed keys are cloned
                    Assert.NotSame(key, hmac.Key);
                }
                else
                {
                    Assert.NotEqual(key, hmac.Key);
                }
            }
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new HmacSha512(null));

            HmacSha512 hmac = new();
            Assert.Throws<ArgumentNullException>(() => hmac.Key = null);
        }


        [Fact]
        public void ComputeHash_ExceptionTest()
        {
            HmacSha512 hmac = new();

            Exception ex = Assert.Throws<ArgumentNullException>(() => hmac.ComputeHash(null));
            Assert.Contains("Data can not be null", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => hmac.ComputeHash(new byte[1]));
            Assert.Contains("Key must be set before calling this function", ex.Message);

            hmac.Key = new byte[1];
            ex = Assert.Throws<ArgumentNullException>(() => hmac.ComputeHash(null));
            Assert.Contains("Data can not be null", ex.Message);

            hmac.Dispose();
            ex = Assert.Throws<ObjectDisposedException>(() => hmac.ComputeHash(new byte[1]));
        }

        [Fact]
        public void ComputeHash_WithKey_ExceptionTest()
        {
            HmacSha512 hmac = new();

            Exception ex = Assert.Throws<ArgumentNullException>(() => hmac.ComputeHash(null, new byte[1]));
            Assert.Contains("Data can not be null", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => hmac.ComputeHash(new byte[1], null));
            Assert.Contains("Key can not be null", ex.Message);

            hmac.Dispose();
            ex = Assert.Throws<ObjectDisposedException>(() => hmac.ComputeHash(new byte[1], new byte[1]));
        }


        [Theory]
        [InlineData(new byte[0], new byte[0])]
        [InlineData(new byte[1] { 0 }, new byte[0])]
        [InlineData(new byte[0], new byte[1] { 0 })]
        public void ComputeHash_EmptyTest(byte[] key, byte[] data)
        {
            using var sysHmac = new System.Security.Cryptography.HMACSHA512(key);
            byte[] expected = sysHmac.ComputeHash(data);

            using HmacSha512 hmac = new(key);
            byte[] actual = hmac.ComputeHash(data);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new byte[0], new byte[0])]
        [InlineData(new byte[1] { 0 }, new byte[0])]
        [InlineData(new byte[0], new byte[1] { 0 })]
        public void ComputeHash_Empty_WithKey_Test(byte[] key, byte[] data)
        {
            using var sysHmac = new System.Security.Cryptography.HMACSHA512(key);
            byte[] expected = sysHmac.ComputeHash(data);

            using HmacSha512 hmac = new();
            byte[] actual = hmac.ComputeHash(data, key);
            Assert.Equal(expected, actual);
        }


        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetHmacSha_Rfc_Cases), parameters: 512, MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_rfc_Test(byte[] msg, byte[] key, byte[] expected)
        {
            using HmacSha512 hmac = new();
            byte[] actual = hmac.ComputeHash(msg, key);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetHmacSha_Rfc_Cases), parameters: 512, MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_rfc_CtorKey_Test(byte[] msg, byte[] key, byte[] expected)
        {
            using HmacSha512 hmac = new(key);
            byte[] actual = hmac.ComputeHash(msg);
            Assert.Equal(expected, actual);
        }


        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetHmacSha_Nist_Cases), parameters: 512, MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NIST_Test(byte[] msg, byte[] key, byte[] expected, int len, bool truncate)
        {
            using HmacSha512 hmac = new();
            byte[] actual = hmac.ComputeHash(msg, key);
            if (truncate)
            {
                byte[] temp = new byte[len];
                Buffer.BlockCopy(actual, 0, temp, 0, len);
                actual = temp;
            }
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetHmacSha_Nist_Cases), parameters: 512, MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NIST_CtorKey_Test(byte[] msg, byte[] key, byte[] expected, int len, bool truncate)
        {
            using HmacSha512 hmac = new(key);
            byte[] actual = hmac.ComputeHash(msg);
            if (truncate)
            {
                byte[] temp = new byte[len];
                Buffer.BlockCopy(actual, 0, temp, 0, len);
                actual = temp;
            }
            Assert.Equal(expected, actual);
        }


        public static TheoryData<byte[], byte[], byte[], byte[], byte[], byte[], byte[], byte[], byte[], byte[]> GetReuseCases()
        {
            byte[] key1 = Helper.HexToBytes("27d94d0e34b0066e29f23c9a6597cfe77a1df7e27f2740914c9a44e49c6a7b12");
            byte[] data1_1 = Helper.HexToBytes("24344356060cde62834b9a1e6143f89fac4485eb8993e8a811226f74e670fbf4");
            byte[] expected1_1 = Helper.HexToBytes("fad2b18c968efbf49a95df1005fc8ac9da6d92b99eecc11a5dca62d51816cf6991c2af61c1796cb676abc9f6ba449767e95f3a30376f67f5f3cfec6e8b7d2f0e");
            byte[] data1_2 = Helper.HexToBytes("1e93d45c26115e0e140227b95fcd9e258ded8722658dd8585522137ea5a5410695e2ca50c4ae6f226953945d0d82398fe27bc30991b11b91ba7c0604fde0686fc020247c024d60df7d65b911f87a4bd2e9839b2ae13875e4c7399548a86b2588ea259ced5ebc8d941ae48b2cf809983f30331a79453d8d88aa40b4c8b84268f144be7c671d23");
            byte[] expected1_2 = Helper.HexToBytes("6bb430b16b38eea9f4d5dceb2288e6891fb87775100df8bd12c75567fc50585a618d2f0895dd19c67badb320a6f59a25fd63a2711cd460f6eadcc166c1c6768a");

            byte[] key2 = Helper.HexToBytes("2362b519acf338edd2f68d210189946859f9678473ee7d4935efc73e56b3dda3a459462ba7d7a1335507d1c65c6a78a13ae47a208a88462a9bfa392d91d96601b57bc5296f6bd8c967aa50fab543791d953c5fdffd04886949919f5f62d193d6e39599bb192c47824602a3d24a093b0427086ea221af4fe1a02521936810f452cede35175a7cd32975dd98fb61b5");
            byte[] data2_1 = Helper.HexToBytes("5772e27fe4a8976b8ab658c734ff23e8f760f6c45f818f4ce6288e6a2407");
            byte[] expected2_1 = Helper.HexToBytes("63df08cd94699cf0e8bc366cd3c13e697f318225a25b2d43354d017aa83029ebe39b4141f08e57a9c388160a6beb177723891a276bfada8a3231556645f7b60c");
            byte[] data2_2 = Helper.HexToBytes("33229af20bbb253b6098715f0b1869f2021a13ac7632ca4d293ad3b546ac0b2b4cb4a159a312c98e21df4a89ea7ed2bacbb3988808141b77a1da9c82d7a97ac002a90968a85a2f71c8df5c9a3f82cd6db3c8c1ac647a2fa3451e5ac5dfad8395eb2e4c64c0f0157881a09525ae0e5a8a0a5c80d1ce6b986501fede2b8aa056ce2a25591bc41fa783d44984928d7443fc619b30032cb2c67f95e762c1");
            byte[] expected2_2 = Helper.HexToBytes("d43d2c2d44d4205fda1d539a3d9df4e5690c7e4f623e2988c8d31e335b9e95a6b96587320512d5422a5dc042f2756ba391bbcaca8d08053c5b2c6e7a57c2b112");

            return new TheoryData<byte[], byte[], byte[], byte[], byte[], byte[], byte[], byte[], byte[], byte[]>()
            {
                {
                    key1, data1_1, expected1_1, data1_2, expected1_2,
                    key2, data2_1, expected2_1, data2_2, expected2_2
                }
            };
        }

        [Theory]
        [MemberData(nameof(GetReuseCases))]
        public void ComputeHash_CtorKey_ReuseTest(byte[] key1, byte[] data1_1, byte[] exp1_1, byte[] data1_2, byte[] exp1_2,
            byte[] key2, byte[] data2_1, byte[] exp2_1, byte[] data2_2, byte[] exp2_2)
        {
            using HmacSha512 hmac = new(key1);
            byte[] actual1_1 = hmac.ComputeHash(data1_1);
            Assert.Equal(exp1_1, actual1_1);

            byte[] actual1_2 = hmac.ComputeHash(data1_2);
            Assert.Equal(exp1_2, actual1_2);

            // change key
            hmac.Key = key2;
            byte[] actual2_1 = hmac.ComputeHash(data2_1);
            Assert.Equal(exp2_1, actual2_1);

            byte[] actual2_2 = hmac.ComputeHash(data2_2);
            Assert.Equal(exp2_2, actual2_2);
        }

        [Theory]
        [MemberData(nameof(GetReuseCases))]
        public void ComputeHash_Reuse_Test(byte[] key1, byte[] data1_1, byte[] exp1_1, byte[] data1_2, byte[] exp1_2,
            byte[] key2, byte[] data2_1, byte[] exp2_1, byte[] data2_2, byte[] exp2_2)
        {
            using HmacSha512 hmac = new();
            byte[] actual1_1 = hmac.ComputeHash(data1_1, key1);
            Assert.Equal(exp1_1, actual1_1);

            byte[] actual2_1 = hmac.ComputeHash(data2_1, key2); // use different key on each call
            Assert.Equal(exp2_1, actual2_1);

            byte[] actual1_2 = hmac.ComputeHash(data1_2, key1);
            Assert.Equal(exp1_2, actual1_2);

            byte[] actual2_2 = hmac.ComputeHash(data2_2, key2);
            Assert.Equal(exp2_2, actual2_2);
        }

    }
}
