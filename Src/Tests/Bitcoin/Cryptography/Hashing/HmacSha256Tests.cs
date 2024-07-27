// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class HmacSha256Tests
    {
        public static IEnumerable<object[]> GetCtorCases()
        {
            yield return new object[] { Array.Empty<byte>(), false };
            yield return new object[] { Helper.GetBytes(1), false };
            yield return new object[] { Helper.GetBytes(64), false };
            yield return new object[] { Helper.GetBytes(65), true };
        }
        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void ConstructorTest(byte[] key, bool isKeyHashed)
        {
            using var sysHmac = new System.Security.Cryptography.HMACSHA256(key);
            byte[] expectedKey = sysHmac.Key;

            // Set key in constructor:
            using (HmacSha256 hmac = new(key))
            {
                Assert.Equal(expectedKey, hmac.Key);
                if (!isKeyHashed)
                {
                    Assert.Equal(key, hmac.Key);
                    // Make sure small unhashed keys are cloned:
                    Assert.NotSame(key, hmac.Key);
                }
                else
                {
                    Assert.NotEqual(key, hmac.Key);
                }
            }

            // Set key using the property:
            using (HmacSha256 hmac = new())
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
            Assert.Throws<ArgumentNullException>(() => new HmacSha256(null));

            HmacSha256 hmac = new();
            Assert.Throws<ArgumentNullException>(() => hmac.Key = null);
        }


        [Fact]
        public void ComputeHash_ExceptionTest()
        {
            HmacSha256 hmac = new();

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
            HmacSha256 hmac = new();

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
            using var sysHmac = new System.Security.Cryptography.HMACSHA256(key);
            byte[] expected = sysHmac.ComputeHash(data);

            using HmacSha256 hmac = new(key);
            byte[] actual = hmac.ComputeHash(data);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(new byte[0], new byte[0])]
        [InlineData(new byte[1] { 0 }, new byte[0])]
        [InlineData(new byte[0], new byte[1] { 0 })]
        public void ComputeHash_Empty_WithKey_Test(byte[] key, byte[] data)
        {
            using var sysHmac = new System.Security.Cryptography.HMACSHA256(key);
            byte[] expected = sysHmac.ComputeHash(data);

            using HmacSha256 hmac = new();
            byte[] actual = hmac.ComputeHash(data, key);
            Assert.Equal(expected, actual);
        }


        public static TheoryData GetHmacSha_Rfc_Cases()
        {
            // Test cases are taken from https://tools.ietf.org/html/rfc4231
            TheoryData<byte[], byte[], byte[]> result = new();
            foreach (var item in Helper.ReadResource<JArray>("HmacShaRfcTestData"))
            {
                byte[] msgBytes = Helper.HexToBytes(item["Message"].ToString());
                byte[] keyBytes = Helper.HexToBytes(item["Key"].ToString());

                byte[] hashBytes = Helper.HexToBytes(item["HMACSHA256"].ToString());
                result.Add(msgBytes, keyBytes, hashBytes);
            }

            return result;
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetHmacSha_Rfc_Cases), parameters: 256, MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_rfc_Test(byte[] msg, byte[] key, byte[] expected)
        {
            using HmacSha256 hmac = new();
            byte[] actual = hmac.ComputeHash(msg, key);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetHmacSha_Rfc_Cases), parameters: 256, MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_rfc_CtorKey_Test(byte[] msg, byte[] key, byte[] expected)
        {
            using HmacSha256 hmac = new(key);
            byte[] actual = hmac.ComputeHash(msg);
            Assert.Equal(expected, actual);
        }


        [Theory]
        [MemberData(nameof(HashTestCaseHelper.GetHmacSha_Nist_Cases), parameters: 256, MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NIST_Test(byte[] msg, byte[] key, byte[] expected, int len, bool truncate)
        {
            using HmacSha256 hmac = new();
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
        [MemberData(nameof(HashTestCaseHelper.GetHmacSha_Nist_Cases), parameters: 256, MemberType = typeof(HashTestCaseHelper))]
        public void ComputeHash_NIST_CtorKey_Test(byte[] msg, byte[] key, byte[] expected, int len, bool truncate)
        {
            using HmacSha256 hmac = new(key);
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
            byte[] expected1_1 = Helper.HexToBytes("29684ceb72bf854a2cd077d15d95ae9ef87e7b77603a2ef09ce692982d07df17");
            byte[] data1_2 = Helper.HexToBytes("1e93d45c26115e0e140227b95fcd9e258ded8722658dd8585522137ea5a5410695e2ca50c4ae6f226953945d0d82398fe27bc30991b11b91ba7c0604fde0686fc020247c024d60df7d65b911f87a4bd2e9839b2ae13875e4c7399548a86b2588ea259ced5ebc8d941ae48b2cf809983f30331a79453d8d88aa40b4c8b8");
            byte[] expected1_2 = Helper.HexToBytes("03319ae9045656782695f0eadabaf9ce78f2da1f952a7d707f027c3acfd1dffd");

            byte[] key2 = Helper.HexToBytes("2362b519acf338ed56bc5fdffd04886949919f5f62d193d6e39599bb192c47824602a3d24a093b0427086ea221af4fe1a02521936810f452cede35175a7cd32975dd98fb61b5");
            byte[] data2_1 = Helper.HexToBytes("5772e27fe4a8976b8ab658c734ff23e8f760f6c45f818f4ce6288e6a24077056");
            byte[] expected2_1 = Helper.HexToBytes("4df4a58f204a0e58d7987acf0a4c96e7b17f4f357f26859a36fa4b8fb191870e");
            byte[] data2_2 = Helper.HexToBytes("33229af20bbb253b60ad8395eb2e4c64c0f0157881a09525ae0e5a8a0a5c80d1ce6b986501fede2b8aa056ce2a25591bc41fa783d44984928d7443fc619b30032cb2c67f95e762c1");
            byte[] expected2_2 = Helper.HexToBytes("97a5011e66f97c1ba6817383441d1f4240750b8158d92ce5f08f99570302360f");

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
            using HmacSha256 hmac = new(key1);
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
            using HmacSha256 hmac = new();
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
