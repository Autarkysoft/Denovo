// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Asymmetric.KeyPairs
{
    public class MiniPrivateKeyTests
    {
        // TODO: this is a bad test as it depends on the RNG, it should be predefined.
        [Fact]
        public void Constructor_RNGTest()
        {
            using MiniPrivateKey key = new MiniPrivateKey(new SharpRandom());
            string actual = key.ToString();

            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] keyBytes = Encoding.UTF8.GetBytes(actual);
            byte[] bytesToHash = new byte[keyBytes.Length + 1];
            Buffer.BlockCopy(keyBytes, 0, bytesToHash, 0, keyBytes.Length);
            bytesToHash[^1] = (byte)'?';
            byte[] hash = sha.ComputeHash(bytesToHash);

            Assert.StartsWith("S", actual);
            Assert.Equal(0, hash[0]);
        }

        [Fact]
        public void Constructor_RNG_NullExceptionTest()
        {
            IRandomNumberGenerator nrng = null;
            Assert.Throws<ArgumentNullException>(() => new MiniPrivateKey(nrng));
        }


        [Theory]
        [InlineData("SzavMBLoXU6kDrqtUVmffv", "e9873d79c6d87dc0fb6a5778633389f4453213303da61f20bd67fc233aa33262")]
        [InlineData("S6c56bnXQiBjk9mqSYE7ykVQ7NzrRy", "4c7a9640c72dc2099f23715d0c8a0d8a35f8906e3cab61dd3f78b67bf887c9ab")]
        public void Constructor_StringTest(string key, string hex)
        {
            using MiniPrivateKey mKey = new MiniPrivateKey(key);

            string actualStr = mKey.ToString();
            // Test if base is set correctly
            byte[] actualBa = mKey.ToBytes();
            byte[] expectedBytes = Helper.HexToBytes(hex);

            Assert.Equal(key, actualStr);
            Assert.Equal(expectedBytes, actualBa);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_String_NullExceptionTest(string key)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => new MiniPrivateKey(key));
            Assert.Contains("Key can not be null or empty.", ex.Message);
        }

        [Theory]
        [InlineData("SzavMB", "Key must be 22 or 26 or 30 character long")]
        [InlineData("S6c56bnXQiBjk9mqSYE7ykVQ7NzrRyDjkd3452", "Key must be 22 or 26 or 30 character long.")]
        public void Constructor_String_OutOfRangeExceptionTest(string key, string error)
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new MiniPrivateKey(key));
            Assert.Contains(error, ex.Message);
        }

        [Theory]
        [InlineData("zzavMBLoXU6kDrqtUVmffv", "Key must start with letter 'S'.")]
        [InlineData("S1c56bnXQiBjk9mqSYE7ykVQ7NzrRy", "Invalid character was found in given key")]
        [InlineData("S6c56bnXQiBjk9mqSYE7ykVQ7NzrX2", "Invalid key (wrong hash).")]
        public void Constructor_String_FormatExceptionTest(string key, string error)
        {
            Exception ex = Assert.Throws<FormatException>(() => new MiniPrivateKey(key));
            Assert.Contains(error, ex.Message);
        }


        [Fact]
        public void ToString_DisposedExceptionTest()
        {
            MiniPrivateKey key = new MiniPrivateKey("SzavMBLoXU6kDrqtUVmffv");
            key.Dispose();
            Exception ex = Assert.Throws<ObjectDisposedException>(() => key.ToString());
        }
    }
}
