// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.ImprovementProposals;
using System;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class BIP0032PathTests
    {
        private const uint Hard = 0x80000000;

        [Fact]
        public void Constructor_FromUIntsTest()
        {
            uint[] uints = { 0x80000000, 1, 0x80000000 + 2, 2 };
            BIP0032Path path = new BIP0032Path(uints);
            Assert.Equal(uints, path.Indexes);
        }

        [Fact]
        public void Constructor_FromUInts_NullTest()
        {
            BIP0032Path path = new BIP0032Path();
            Assert.Empty(path.Indexes);
        }


        [Theory]
        [InlineData("m", new uint[0])]
        [InlineData("m/", new uint[0])]
        [InlineData("m/0", new uint[1] { 0 })]
        [InlineData("m/5", new uint[1] { 5 })]
        [InlineData("m/0'", new uint[1] { Hard })]
        [InlineData("m/5'", new uint[1] { Hard + 5 })]
        [InlineData("m/0'/1/2'/43", new uint[4] { Hard, 1, Hard + 2, 43 })]
        // With extra spaces:
        [InlineData("m/8 '/ 1/0 ' /12 ", new uint[4] { Hard + 8, 1, Hard, 12 })]
        // Using 'h' and "H" to indicate hardened paths:
        [InlineData("m/3h/2H/6", new uint[3] { Hard + 3, Hard + 2, 6 })]
        // Empty item at the end (extra /) is ignored
        [InlineData("m/5/", new uint[1] { 5 })]
        public void Constructor_FromStringTest(string toUse, uint[] expected)
        {
            var path = new BIP0032Path(toUse);
            Assert.Equal(expected, path.Indexes);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_FromString_NullExceptionTest(string path)
        {
            Assert.Throws<ArgumentNullException>(() => new BIP0032Path(path));
        }

        [Theory]
        [InlineData("0'/1/2'/2", "Path should start with letter \"m\".")]
        [InlineData("m/1/2'h", "Input contains more than one indicator for hardened key.")]
        [InlineData("m/1/g", "Input (g) is not a valid positive number.")]
        [InlineData("m/1/-1", "Input (-1) is not a valid positive number.")]
        [InlineData("m/2147483648/2", "Index is too big.")]
        public void Constructor_FromString_FormatExceptionTest(string path, string expErr)
        {
            Exception ex = Assert.Throws<FormatException>(() => new BIP0032Path(path));
            Assert.Contains(expErr, ex.Message);
        }

        [Fact]
        public void Constructor_FromString_DepthOverflowTest()
        {
            char[] temp = new char[511];
            temp[0] = 'm';
            for (int i = 1; i < temp.Length; i += 2)
            {
                temp[i] = '/';
                temp[i + 1] = '1';
            }
            Exception ex = Assert.Throws<FormatException>(() => new BIP0032Path(new string(temp)));
            Assert.Contains("Depth can not be bigger than 1 byte.", ex.Message);
        }

        [Theory]
        [InlineData(new uint[0], "m")]
        [InlineData(new uint[] { 0 }, "m/0")]
        [InlineData(new uint[] { 2 }, "m/2")]
        [InlineData(new uint[] { Hard, 1, Hard + 2, 2 }, "m/0'/1/2'/2")]
        public void ToStringTest(uint[] toUse, string expected)
        {
            var path = new BIP0032Path(toUse);
            string actual = path.ToString();
            Assert.Equal(expected, actual);
        }
    }
}
