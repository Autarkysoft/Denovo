// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class Ripemd160Sha256Tests
    {
        // This is a randomly generated key:
        private const string PubComp = "0220ddccb21a216e59c016b558a7958e44d499b1fd6f15bfae5cec507cf1fa7cc6";
        private const string PubComp_hash = "110cfc8ec7339796ecf0ec842de0fd3a99ff5e36";
        private const string PubUnComp = "0420DDCCB21A216E59C016B558A7958E44D499B1FD6F15BFAE5CEC507CF1FA7CC666C2F19BA10F741DBEAF886BBF17AD6CF523F3FBF6A69FEEE6D111F3EF6FB024";
        private const string PubUnComp_hash = "a0c003f27ed1d90349670df720371385282bd5b9";


        public static IEnumerable<object[]> GetHashCases()
        {
            yield return new object[]
            {
                // Special case length == 33
                Helper.HexToBytes(PubComp),
                Helper.HexToBytes(PubComp_hash)
            };
            yield return new object[]
            {
                // Special case length == 65
                Helper.HexToBytes(PubUnComp),
                Helper.HexToBytes(PubUnComp_hash)
            };
            yield return new object[]
            {
                Array.Empty<byte>(),
                Helper.HexToBytes("b472a266d0bd89c13706a4132ccfb16f7c3b9fcb")
            };
            yield return new object[]
            {
                Helper.HexToBytes("0c2d4e7200743c6273ef415e308a483762f9d8109fc194c3608878f83e029a1eb23b35f5c66a72e8"),
                Helper.HexToBytes("4c9ffc3832a230a1fb30d7afd0efa43d4d30cf60")
            };
            yield return new object[]
            {
                Helper.HexToBytes("ac7ef2f2a2cb526c6f943c9b1ce3fb91cdf41a192690e3df9b82cc55bb0c7e7cfdbe296662d64b6f99037a98b58234ccc0433a90390f7f6e60ba1aa16d06941237a98b"),
                Helper.HexToBytes("0f2a898d090e67d0b3f2c5dff930e643526a83db")
            };
        }
        [Theory]
        [MemberData(nameof(GetHashCases))]
        public void ComputeHashTest(byte[] data, byte[] expected)
        {
            using Ripemd160Sha256 hash160 = new();
            byte[] actual = hash160.ComputeHash(data);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ComputeHash_ReuseTest()
        {
            using Ripemd160Sha256 hash160 = new();
            byte[] data1 = Helper.HexToBytes(PubComp);
            byte[] expected1 = Helper.HexToBytes(PubComp_hash);

            byte[] data2 = Helper.HexToBytes(PubUnComp);
            byte[] expected2 = Helper.HexToBytes(PubUnComp_hash);

            byte[] data3 = Helper.HexToBytes("1e26fa6d260804f1a6d94b24be80ec1ab0ac6f7483dbb3dc9d30fb47bf50c514471040e26a9a8379");
            byte[] expected3 = Helper.HexToBytes("3900c473d221f26099b6df89526b7203d1a7b5d2");

            hash160.ComputeHash(data1);

            byte[] actual1 = hash160.ComputeHash(data1);
            byte[] actual2 = hash160.ComputeHash(data2);
            byte[] actual3 = hash160.ComputeHash(data3);

            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);
        }
    }
}
