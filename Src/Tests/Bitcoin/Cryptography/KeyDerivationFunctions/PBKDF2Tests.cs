// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Cryptography.KeyDerivationFunctions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Cryptography.KeyDerivationFunctions
{
    public class PBKDF2Tests
    {
        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new PBKDF2(1000, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PBKDF2(-2, new HmacSha256()));
        }

        [Fact]
        public void GetBytes_ExceptionTest()
        {
            PBKDF2 kdf = new PBKDF2(1000, new HmacSha256());
            byte[] pass = new byte[1];
            byte[] salt = new byte[1];

            Exception ex = Assert.Throws<ArgumentNullException>(() => kdf.GetBytes(null, salt, 32));
            Assert.Contains("Password can not be null.", ex.Message);

            ex = Assert.Throws<ArgumentNullException>(() => kdf.GetBytes(pass, null, 32));
            Assert.Contains("Salt can not be null.", ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => kdf.GetBytes(pass, salt, -1));
            Assert.Contains("Derived key length must be bigger than zero.", ex.Message);

            kdf.Dispose();
            ex = Assert.Throws<ObjectDisposedException>(() => kdf.GetBytes(pass, salt, 10));
            Assert.Contains("Instance was disposed.", ex.Message);
        }


        // Tests are taken from: 
        // https://github.com/Anti-weakpasswords/PBKDF2-Test-Vectors/releases 
        //     found https://stackoverflow.com/questions/5130513/pbkdf2-hmac-sha2-test-vectors
        public static IEnumerable<object[]> GetKDFCases()
        {
            foreach (var item in Helper.ReadResource<JArray>("PBKDF2TestData"))
            {
                foreach (var item2 in item["Data"])
                {
                    // Set a new HMAC instance for each test (avoids ObjectDisposedException)
                    IHmacFunction hmac = (item["HmacName"].ToString()) switch
                    {
                        "HMACSHA256" => new HmacSha256(),
                        "HMACSHA512" => new HmacSha512(),
                        _ => throw new Exception("Test data file contains an undefined HMAC name."),
                    };
                    byte[] pass = Encoding.UTF8.GetBytes(item2[0].ToString());
                    byte[] salt = Encoding.UTF8.GetBytes(item2[1].ToString());
                    int iter = (int)item2[2];

                    // The following condition skips 68 out of 128 test vectors that have huge iteratio values 
                    // since they are very time consuming (~17min in debug mode) to avoid repeating them
                    // every time all tests are run. Change this if anything in the class changed or
                    // if you want to perform these test for any reason.
                    if (iter > 5000)
                    {
                        continue;
                    }

                    int len = (int)item2[3];
                    byte[] derivedBytes = Helper.HexToBytes(item2[4].ToString());

                    yield return new object[] { hmac, iter, pass, salt, len, derivedBytes };
                }
            }
        }
        [Theory]
        [MemberData(nameof(GetKDFCases))]
        public void GetBytesTest(IHmacFunction hmac, int iter, byte[] pass, byte[] salt, int len, byte[] expected)
        {
            using PBKDF2 kdf = new PBKDF2(iter, hmac);
            byte[] actual = kdf.GetBytes(pass, salt, len);
            Assert.Equal(expected, actual);
        }
    }
}
