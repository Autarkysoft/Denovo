// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.KeyDerivationFunctions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Cryptography.KeyDerivationFunctions
{
    public class ScryptTests
    {
        [Fact]
        public void Constructor_ExceptionTest()
        {
            Exception ex = Assert.Throws<ArgumentException>(() => new Scrypt(1, 1, 1));
            Assert.Contains("Cost parameter must be a multiple of 2^n and bigger than 1.", ex.Message);

            ex = Assert.Throws<ArgumentException>(() => new Scrypt(3, 1, 1));
            Assert.Contains("Cost parameter must be a multiple of 2^n and bigger than 1.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Scrypt(2, 0, 1));
            Assert.Contains("Blocksize factor must be bigger than 0.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Scrypt(2, -1, 1));
            Assert.Contains("Blocksize factor must be bigger than 0.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Scrypt(2, 1, 0));
            Assert.Contains("Parallelization factor must be bigger than 0.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Scrypt(2, 1, -1));
            Assert.Contains("Parallelization factor must be bigger than 0.", ex.Message);
        }

        [Fact]
        public void GetBytes_ExceptionTest()
        {
            Scrypt sc = new Scrypt(2, 1, 1);

            Assert.Throws<ArgumentNullException>(() => sc.GetBytes(null, new byte[1], 1));
            Assert.Throws<ArgumentNullException>(() => sc.GetBytes(new byte[1], null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => sc.GetBytes(new byte[1], new byte[1], 0));

            sc.Dispose();
            Assert.Throws<ObjectDisposedException>(() => sc.GetBytes(new byte[1], new byte[1], 1));
        }

        // Tests are taken from https://tools.ietf.org/html/rfc7914#section-12
        public static IEnumerable<object[]> GetSCryptCases()
        {
            yield return new object[]
            {
                Encoding.UTF8.GetBytes(""),
                Encoding.UTF8.GetBytes(""),
                Helper.HexToBytes("77d6576238657b203b19ca42c18a0497f16b4844e3074ae8dfdffa3fede21442fcd0069ded0948f8326a753a0fc81f17e8d3e0fb2e0d3628cf35e20c38d18906"),
                16, 1, 1, 64
            };
            yield return new object[]
            {
                Encoding.UTF8.GetBytes("password"),
                Encoding.UTF8.GetBytes("NaCl"),
                Helper.HexToBytes("fdbabe1c9d3472007856e7190d01e9fe7c6ad7cbc8237830e77376634b3731622eaf30d92e22a3886ff109279d9830dac727afb94a83ee6d8360cbdfa2cc0640"),
                1024, 8, 16, 64
            };
            yield return new object[]
            {
                Encoding.UTF8.GetBytes("pleaseletmein"),
                Encoding.UTF8.GetBytes("SodiumChloride"),
                Helper.HexToBytes("7023bdcb3afd7348461c06cd81fd38ebfda8fbba904f8e3ea9b543f6545da1f2d5432955613f0fcf62d49705242a9af9e61e85dc0d651e40dfcf017b45575887"),
                16384, 8, 1, 64
            };
            yield return new object[]
            {
                Encoding.UTF8.GetBytes("pleaseletmein"),
                Encoding.UTF8.GetBytes("SodiumChloride"),
                Helper.HexToBytes("2101cb9b6a511aaeaddbbe09cf70f881ec568d574a2ffd4dabe5ee9820adaa478e56fd8f4ba5d09ffa1c6d927c40f4c337304049e8a952fbcbf45c6fa77a41a4"),
                1048576, 8, 1, 64
            };
        }
        [Theory]
        [MemberData(nameof(GetSCryptCases))]
        public void ScryptTest(byte[] password, byte[] salt, byte[] expectedDK, int n, int r, int p, int dkLen)
        {
            Scrypt sc = new Scrypt(n, r, p);
            byte[] actual = sc.GetBytes(password, salt, dkLen);
            Assert.Equal(expectedDK, actual);
        }
    }
}
