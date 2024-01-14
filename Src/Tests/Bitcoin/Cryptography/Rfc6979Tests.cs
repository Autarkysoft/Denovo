// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;

namespace Tests.Bitcoin.Cryptography
{
    public class Rfc6979Tests
    {
        private static void Init(byte[] data, byte[] keyBytes, out byte[] k, out byte[] v)
        {
            using HmacSha256 HmacK = new();
            v = new byte[32];
            ((Span<byte>)v).Fill(1);
            k = new byte[32];
            byte[] bytesToHash = new byte[97];
            Scalar8x32 sc = new(data, out _);
            byte[] dataBa = sc.ToByteArray();

            Buffer.BlockCopy(v, 0, bytesToHash, 0, 32);
            Buffer.BlockCopy(keyBytes, 0, bytesToHash, 33, 32);
            Buffer.BlockCopy(dataBa, 0, bytesToHash, 65, 32);

            k = HmacK.ComputeHash(bytesToHash, k);
            v = HmacK.ComputeHash(v, k);
            Buffer.BlockCopy(v, 0, bytesToHash, 0, 32);
            bytesToHash[32] = 0x01;
            k = HmacK.ComputeHash(bytesToHash, k);
            v = HmacK.ComputeHash(v, k);
        }

        private static byte[] Generate(byte[] k, byte[] v, bool retry)
        {
            using HmacSha256 HmacK = new();

            v = HmacK.ComputeHash(v, k);

            if (retry)
            {
                k = HmacK.ComputeHash(v.AppendToEnd(0), k);
                v = HmacK.ComputeHash(v, k);
                v = HmacK.ComputeHash(v, k);
            }

            return v;
        }


        [Fact]
        public void RandomTest()
        {
            Rfc6979 rfc = new();

            byte[] key = new byte[32];
            byte[] hash = new byte[32];
            RandomNumberGenerator.Fill(key);
            RandomNumberGenerator.Fill(hash);

            rfc.Init(hash, key);
            byte[] actual = rfc.Generate();

            Init(hash, key, out byte[] k, out byte[] v);
            byte[] expecteed = Generate(k, v, false);

            Assert.Equal(expecteed, actual);

            // Secondt nonce
            byte[] actual2 = rfc.Generate();
            expecteed = Generate(k, v, true);

            Assert.NotEqual(actual, actual2);
            Assert.Equal(expecteed, actual2);
        }


        // The two 256-bit curve tests with a different order and using SHA256 from here:
        // https://tools.ietf.org/html/rfc6979#appendix-A.2.5
        [Fact]
        public void GetK_RFCTest1()
        {
            BigInteger order =
                BigInteger.Parse("00FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551", NumberStyles.HexNumber);
            byte[] data = Helper.HexToBytes("AF2BDBE1AA9B6EC1E2ADE1D694F41FC71A831D0268E9891562113D8A62ADD1BF");
            byte[] keyBytes = Helper.HexToBytes("C9AFA9D845BA75166B5C215767B1D6934E50C3DB36E89B127B8A622B120F6721");

            using Rfc6979 rfc = new();

            BigInteger actual = rfc.GetK(data, keyBytes, null);
            BigInteger expected = BigInteger.Parse("00A6E3C57DD01ABE90086538398355DD4C3B17AA873382B0F24D6129493D8AAD60", NumberStyles.HexNumber);

            Assert.Equal(expected, actual);

            rfc.Init(data, keyBytes);
            byte[] actual2 = rfc.Generate();
            Assert.Equal(expected.ToByteArray(true, true), actual2);
        }

        [Fact]
        public void GetK_RFCTest2()
        {
            BigInteger order =
                BigInteger.Parse("00FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551", NumberStyles.HexNumber);
            byte[] data = Helper.HexToBytes("9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08");
            byte[] keyBytes = Helper.HexToBytes("C9AFA9D845BA75166B5C215767B1D6934E50C3DB36E89B127B8A622B120F6721");

            using Rfc6979 rfc = new();

            BigInteger actual = rfc.GetK(data, keyBytes, null);
            BigInteger expected = BigInteger.Parse("00D16B6AE827F17175E040871A1C7EC3500192C4C92677336EC2537ACAEE0008E0", NumberStyles.HexNumber);

            Assert.Equal(expected, actual);

            rfc.Init(data, keyBytes);
            byte[] actual2 = rfc.Generate();
            Assert.Equal(expected.ToByteArray(true, true), actual2);
        }
    }
}
