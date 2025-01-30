// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class PrivateKeyTests
    {
        private static readonly BigInteger MaxValue = BigInteger.Parse("115792089237316195423570985008687907852837564279074904382605163141518161494336");

        public static IEnumerable<object[]> GetConstructorCases()
        {
            yield return new object[]
            {
                // Min value
                BigInteger.One,
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001")
            };
            yield return new object[]
            {
                // Max value
                MaxValue,
                Helper.HexToBytes("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364140")
            };
            yield return new object[]
            {
                // https://en.bitcoin.it/wiki/Wallet_import_format
                BigInteger.Parse("5500171714335001507730457227127633683517613019341760098818554179534751705629"),
                Helper.HexToBytes("0c28fca386c7a227600b2fe50b7cae11ec86d3bf1fbe471be89827e19d72aa1d"),
            };
        }

        [Theory]
        [MemberData(nameof(GetConstructorCases))]
        public void Constructor_IntByteScalarTest(BigInteger big, byte[] ba)
        {
            Scalar8x32 scalar = new(ba, out _);

            using PrivateKey k1 = new(big);
            using PrivateKey k2 = new(ba);
            using PrivateKey k3 = new(scalar);

            Helper.ComparePrivateField(k1, "keyBytes", ba);
            Helper.ComparePrivateField(k2, "keyBytes", ba);
            Helper.ComparePrivateField(k3, "keyBytes", ba);
        }

        [Theory]
        [InlineData("01", 1)]
        [InlineData("0005", 5)]
        public void Constructor_SmallBytesTest(string hex, byte b)
        {
            byte[] ba = Helper.HexToBytes(hex);
            byte[] expected = new byte[32];
            expected[^1] = b;
            using PrivateKey k = new(ba);

            Helper.ComparePrivateField(k, "keyBytes", expected);
        }


        [Fact]
        public void Constructor_IntExceptionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PrivateKey(BigInteger.Zero));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PrivateKey(MaxValue + 1));
        }

        [Fact]
        public void Constructor_ByteExceptionTest()
        {
            byte[] nba = null;
            Assert.Throws<ArgumentNullException>(() => new PrivateKey(nba));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PrivateKey(new byte[32]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PrivateKey(new byte[33]));

            byte[] maxBa = (MaxValue + 1).ToByteArray(true, true);
            Assert.Throws<ArgumentOutOfRangeException>(() => new PrivateKey(maxBa));

            maxBa = new byte[32];
            ((Span<byte>)maxBa).Fill(0xff);
            Assert.Throws<ArgumentOutOfRangeException>(() => new PrivateKey(maxBa));
        }


        [Fact]
        public void Constructor_RNGTest()
        {
            byte[] expected = new byte[32];
            expected[31] = 1;

            using PrivateKey key = new(new MockRng(expected));
            byte[] actual = key.ToBytes();

            Assert.Equal(expected, actual);
        }

        class BrokenMockRng : IRandomNumberGenerator
        {
            // Will not change the given array ie. it will return all zeros.
            public void GetBytes(byte[] toFill) { }
            public void Dispose() { }
        }

        class OutOfRangeRng : IRandomNumberGenerator
        {
            // Returns the biggest possible value which is out of range of bitcoin curve
            public void GetBytes(byte[] toFill) => ((Span<byte>)toFill).Fill(0xff);
            public void Dispose() { }
        }

        [Fact]
        public void Constructor_RNG_ExceptionTest()
        {
            IRandomNumberGenerator nRng = null;
            Assert.Throws<ArgumentNullException>(() => new PrivateKey(nRng));
            Assert.Throws<ArgumentException>(() => new PrivateKey(new BrokenMockRng()));
            Assert.Throws<ArgumentException>(() => new PrivateKey(new OutOfRangeRng()));
        }


        public static IEnumerable<object[]> GetWifCases()
        {
            yield return new object[]
            {
                // Min value
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001"),
                "5HpHagT65TZzG1PH3CSu63k8DbpvD8s5ip4nEB3kEsreAnchuDf",
                "KwDiBf89QgGbjEhKnhXJuH7LrciVrZi3qYjgd9M7rFU73sVHnoWn",
                "91avARGdfge8E4tZfYLoxeJ5sGBdNJQH4kvjJoQFacbgwmaKkrx",
                "cMahea7zqjxrtgAbB7LSGbcQUr1uX1ojuat9jZodMN87JcbXMTcA"
            };
            yield return new object[]
            {
                // Max value
                Helper.HexToBytes("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364140"),
                "5Km2kuu7vtFDPpxywn4u3NLpbr5jKpTB3jsuDU2KYEqetqj84qw",
                "L5oLkpV3aqBjhki6LmvChTCV6odsp4SXM6FfU2Gppt5kFLaHLuZ9",
                "93XfLeifX7KMMtUGa7xouxtnFWSSUyzNPgjrJ6Npsyahfqjy7oJ",
                "cWALDjUu1tszsCBMjBjL4mhYj2wHUWYDR8Q8aSjLKzjkW5eBtpzu"
            };
            yield return new object[]
            {
                // https://en.bitcoin.it/wiki/Wallet_import_format
                Helper.HexToBytes("0c28fca386c7a227600b2fe50b7cae11ec86d3bf1fbe471be89827e19d72aa1d"),
                "5HueCGU8rMjxEXxiPuD5BDku4MkFqeZyd4dZ1jvhTVqvbTLvyTJ",
                "KwdMAjGmerYanjeui5SHS7JkmpZvVipYvB2LJGU1ZxJwYvP98617",
                "91gGn1HgSap6CbU12F6z3pJri26xzp7Ay1VW6NHCoEayNXwRpu2",
                "cMzLdeGd5vEqxB8B6VFQoRopQ3sLAAvEzDAoQgvX54xwofSWj1fx"
            };
            yield return new object[]
            {
                // https://en.bitcoin.it/wiki/Private_key
                Helper.HexToBytes("e9873d79c6d87dc0fb6a5778633389f4453213303da61f20bd67fc233aa33262"),
                "5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF",
                "L53fCHmQhbNp1B4JipfBtfeHZH7cAibzG9oK19XfiFzxHgAkz6JK",
                "93MmL5UhauaYksC1FZ41xxYLykpaij5ESeNUSWDxL7igKYUR9N1",
                "cVQefCmG8f55AcXa7EUKFz9MBWR1qAhgLBwn7ZzBDNexYRHvBXZm"
            };
            yield return new object[]
            {
                // https://en.bitcoin.it/wiki/Technical_background_of_version_1_Bitcoin_addresses
                Helper.HexToBytes("18e14a7b6a307f426a94f8114701e7c8e774e7f9a47e2c2035db29a206321725"),
                "5J1F7GHadZG3sCCKHCwg8Jvys9xUbFsjLnGec4H125Ny1V9nR6V",
                "Kx45GeUBSMPReYQwgXiKhG9FzNXrnCeutJp4yjTd5kKxCitadm3C",
                "91msh178DnLBqFhbuYqazuUwWpKBkRQvgj8bggdWMp81nVp9PfM",
                "cNR4jZU2sR5goytD4wXT4aeKcbqGSekbxLxY69v8aryxTU1SMnJZ"
            };
        }

        [Theory]
        [MemberData(nameof(GetWifCases))]
        public void Constructor_WifTest(byte[] expected, string wif, string wifC, string wifT, string wifTC)
        {
            using PrivateKey keyW = new(wif, NetworkType.MainNet);
            using PrivateKey keyWC = new(wifC, NetworkType.MainNet);
            using PrivateKey keyWT = new(wifT, NetworkType.TestNet3);
            using PrivateKey keyWTC = new(wifTC, NetworkType.TestNet3);
            // RegTest and TestNet3 are the same
            using PrivateKey keyWRT = new(wifT, NetworkType.RegTest);

            Assert.Equal(expected, keyW.ToBytes());
            Assert.Equal(expected, keyWC.ToBytes());
            Assert.Equal(expected, keyWT.ToBytes());
            Assert.Equal(expected, keyWTC.ToBytes());
            Assert.Equal(expected, keyWRT.ToBytes());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_Wif_NullExceptionTest(string wif)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => new PrivateKey(wif));
            Assert.Contains("Input WIF can not be null or empty", ex.Message);
        }

        [Theory]
        [InlineData("5GPHYxaeAAqL2egjmyU8Kaaqh831aw12XJ92y2rgRWi1zc", "Invalid first byte")] // first byte is 3
        [InlineData("L5HydKmZoMcqfoY9Rgi8BRnWGmDw9YhoUS9ArnToVxvyFbM9GyfJ", "Invalid compressed byte")] // 0x54
        [InlineData("KwFAa6AumokBD2dVqQLPou42jHiVsvThY1n25HJ8Ji8REf1wxAQb", "Invalid compressed byte")] // 0xcb
        public void Constructor_WIF_FormatException_Test(string wif, string error)
        {
            Exception ex = Assert.Throws<FormatException>(() => new PrivateKey(wif));
            Assert.Contains(error, ex.Message);
        }


        [Theory]
        [InlineData("5HpHagT65TZzG1PH3CSu63k8DbpvD8s5ip4nEB3kEsreAbuatmU", NetworkType.MainNet)] // 0
        [InlineData("KwDiBf89QgGbjEhKnhXJuH7LrciVrZi3qYjgd9M7rFU73Nd2Mcv1", NetworkType.MainNet)] // 0
        [InlineData("91avARGdfge8E4tZfYLoxeJ5sGBdNJQH4kvjJoQFacbgwi1C2GD", NetworkType.TestNet3)] // 0
        [InlineData("cMahea7zqjxrtgAbB7LSGbcQUr1uX1ojuat9jZodMN87J7g8rY9t", NetworkType.TestNet3)] // 0
        [InlineData("5Km2kuu7vtFDPpxywn4u3NLpbr5jKpTB3jsuDU2KYEqetwr388P", NetworkType.MainNet)] // max value + 1
        [InlineData("L5oLkpV3aqBjhki6LmvChTCV6odsp4SXM6FfU2Gppt5kFqRzExJJ", NetworkType.MainNet)] // max value + 1
        [InlineData("93XfLeifX7KMMtUGa7xouxtnFWSSUyzNPgjrJ6Npsyahfwcd8gd", NetworkType.TestNet3)] // max value + 1
        [InlineData("cWALDjUu1tszsCBMjBjL4mhYj2wHUWYDR8Q8aSjLKzjkWaXMLRaY", NetworkType.TestNet3)] // max value + 1
        public void Constructor_WIF_OutOfRangeException_Test(string wif, NetworkType netType)
        {
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => new PrivateKey(wif, netType));
            Assert.Contains("Given key value is outside the defined range by the curve", ex.Message);
        }


        [Fact]
        public void ToBytesTest()
        {
            using PrivateKey key = new(new byte[] { 10 });

            byte[] actual = key.ToBytes();
            byte[] expected = new byte[32];
            expected[31] = 10;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToBytes_ObjectDisposedException_Test()
        {
            PrivateKey key = new(new byte[] { 10 });
            key.Dispose();
            Assert.Throws<ObjectDisposedException>(() => key.ToBytes());
        }


        [Theory]
        [MemberData(nameof(GetConstructorCases))]
        public void ToBigIntTest(BigInteger expected, byte[] bytesToUse)
        {
            using PrivateKey key = new(bytesToUse);
            BigInteger actual = key.ToBigInt();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToBigInt_ObjectDisposedException_Test()
        {
            PrivateKey key = new(new byte[] { 10 });
            key.Dispose();
            Assert.Throws<ObjectDisposedException>(() => key.ToBigInt());
        }


        [Theory]
        [InlineData("0000000000000000000000000000000000000000000000000000000000000001")]
        [InlineData("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364140")]
        [InlineData("0c28fca386c7a227600b2fe50b7cae11ec86d3bf1fbe471be89827e19d72aa1d")]
        public void ToScalarTest(string hex)
        {
            byte[] ba = Helper.HexToBytes(hex);
            Scalar8x32 expected = new(ba, out bool overflow);
            Assert.False(overflow);

            using PrivateKey key = new(ba);
            Scalar8x32 actual = key.ToScalar();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToScalar_ObjectDisposedException_Test()
        {
            PrivateKey key = new(new byte[] { 10 });
            key.Dispose();
            Assert.Throws<ObjectDisposedException>(() => key.ToScalar());
        }


        [Theory]
        [MemberData(nameof(GetWifCases))]
        public void ToWifTest(byte[] bytesToUse, string wif, string wifC, string wifT, string wifTC)
        {
            using PrivateKey prvKey = new(bytesToUse);
            Assert.Equal(wif, prvKey.ToWif(false, NetworkType.MainNet));
            Assert.Equal(wifC, prvKey.ToWif(true, NetworkType.MainNet));
            Assert.Equal(wifT, prvKey.ToWif(false, NetworkType.TestNet3));
            Assert.Equal(wifTC, prvKey.ToWif(true, NetworkType.TestNet3));
            // RegTest is the same as TestNet3
            Assert.Equal(wifTC, prvKey.ToWif(true, NetworkType.RegTest));
        }

        [Fact]
        public void ToWif_ObjectDisposedExceptionTest()
        {
            PrivateKey key = new(new byte[] { 10 });
            key.Dispose();
            Assert.Throws<ObjectDisposedException>(() => key.ToWif(true, NetworkType.MainNet));
        }

        [Fact]
        public void ToWif_ArgumentException_Test()
        {
            using PrivateKey key = new(new byte[] { 10 });
            NetworkType netType = (NetworkType)100;
            Exception ex = Assert.Throws<ArgumentException>(() => key.ToWif(true, netType));
            Assert.Contains("Network type is not defined", ex.Message);
        }
    }
}
