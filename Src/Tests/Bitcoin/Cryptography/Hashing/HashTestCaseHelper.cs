// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class HashTestCaseHelper
    {
        /// <summary>
        /// Returns the 8 common hash cases
        /// </summary>
        /// <param name="name">Name of the hash function (it has to be registered inside HashTestData.json file)</param>
        /// <returns></returns>
        public static IEnumerable<object[]> GetCommonHashCases(string name)
        {
            // * Source of Test cases:
            // RIPEMD160: https://homes.esat.kuleuven.be/~bosselae/ripemd160.html
            // SHA256 and SHA512:
            // Used .Net Framework 4.7.2 System.Security.Cryptography.SHA256Managed

            foreach (var item in Helper.ReadResource<JArray>("HashTestData"))
            {
                string msg = item["Message"].ToString();
                string hash = item[name].ToString();

                byte[] msgBytes = Encoding.UTF8.GetBytes(msg);
                byte[] hashBytes = Helper.HexToBytes(hash);
                yield return new byte[2][] { msgBytes, hashBytes };
            }
        }


        public static byte[] GetAMillionA()
        {
            byte[] message = new byte[1_000_000];
            for (int i = 0; i < message.Length; i++)
            {
                message[i] = (byte)'a';
            }
            return message;
        }


        private static IEnumerable<object[]> GetNistShortLongCases(string sh_lo_va, string fileName)
        {
            JObject jObjs = Helper.ReadResource<JObject>(fileName);
            string jKey = sh_lo_va == "sh" ? "ShortMessage"
                        : sh_lo_va == "lo" ? "LongMessage"
                        : throw new ArgumentException();

            foreach (var item in jObjs[jKey])
            {
                byte[] msg = Helper.HexToBytes(item["Message"].ToString());
                byte[] hash = Helper.HexToBytes(item["Hash"].ToString());

                yield return new byte[][] { msg, hash };
            }
        }

        /// <summary>
        /// Returns test cases defind by NIST for "short messages".
        /// <para/> Link: https://csrc.nist.gov/projects/cryptographic-algorithm-validation-program/secure-hashing
        /// </summary>
        /// <param name="name">Name of the hash function (eg. Sha256)</param>
        /// <returns></returns>
        public static IEnumerable<object[]> GetNistShortCases(string name)
        {
            return GetNistShortLongCases("sh", $"{name}NistTestData");
        }

        /// <summary>
        /// Returns test cases defind by NIST for "long messages".
        /// <para/> Link: https://csrc.nist.gov/projects/cryptographic-algorithm-validation-program/secure-hashing
        /// </summary>
        /// <param name="name">Name of the hash function (eg. Sha3_256)</param>
        /// <returns></returns>
        public static IEnumerable<object[]> GetNistLongCases(string name)
        {
            return GetNistShortLongCases("lo", $"{name}NistTestData");
        }


        // Test cases are taken from https://tools.ietf.org/html/rfc4231
        public static IEnumerable<object[]> GetHmacSha_Rfc_Cases(int lenBit)
        {
            string name = lenBit == 256 ? "HMACSHA256" :
                          lenBit == 512 ? "HMACSHA512" :
                          throw new ArgumentException("HMAC length is not defined.");

            foreach (var item in Helper.ReadResource<JArray>("HmacShaRfcTestData"))
            {
                byte[] msgBytes = Helper.HexToBytes(item["Message"].ToString());
                byte[] keyBytes = Helper.HexToBytes(item["Key"].ToString());

                byte[] hashBytes = Helper.HexToBytes(item[name].ToString());
                yield return new object[] { msgBytes, keyBytes, hashBytes };
            }
        }


        // Test cases are taken from:
        // https://csrc.nist.gov/Projects/cryptographic-algorithm-validation-program/Message-Authentication#Testing
        public static IEnumerable<object[]> GetHmacSha_Nist_Cases(int lenBit)
        {
            string name = lenBit == 256 ? "HMACSHA256" :
                          lenBit == 512 ? "HMACSHA512" :
                          throw new ArgumentException("HMAC length is not defined.");

            JObject hmacData = Helper.ReadResource<JObject>("HmacShaNistTestData");
            foreach (var item in hmacData[name])
            {
                byte[] msgBytes = Helper.HexToBytes(item["Message"].ToString());
                byte[] keyBytes = Helper.HexToBytes(item["Key"].ToString());
                byte[] hashBytes = Helper.HexToBytes(item["Mac"].ToString());
                int len = (int)item["Tlen"];

                yield return new object[] { msgBytes, keyBytes, hashBytes, len, len != (lenBit / 8) };
            }
        }
    }
}
