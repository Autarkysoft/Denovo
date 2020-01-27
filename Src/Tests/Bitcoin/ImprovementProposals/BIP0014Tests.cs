// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.ImprovementProposals;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class BIP0014Tests
    {
        public static IEnumerable<object[]> GetBip14Cases()
        {
            yield return new object[] { "Satoshi", new Version(0, 9, 3), null, "/Satoshi:0.9.3/" };
            yield return new object[] { "Satoshi", null, null, "/Satoshi:0.0/" };
            yield return new object[] { "Satoshi", null, "some comment", "/Satoshi:0.0(some comment)/" };
            yield return new object[] { "AndroidBuild", new Version(1, 3, 7, 4), "", "/AndroidBuild:1.3.7.4/" };
            yield return new object[]
            {
                "BitcoinJ", new Version(0, 2), "iPad; U; CPU OS 3_2_1", "/BitcoinJ:0.2(iPad; U; CPU OS 3_2_1)/"
            };
        }


        [Theory]
        [MemberData(nameof(GetBip14Cases))]
        public void ConstructorTest(string name, Version ver, string cmt, string expected)
        {
            BIP0014 bip = new BIP0014(name, ver, cmt);

            string actualStr = bip.ToString();
            byte[] actualBa = bip.ToByteArray();

            Assert.Equal(name, bip.ClientName);
            Assert.Equal(ver ?? new Version(0, 0), bip.ClientVersion);
            Assert.Equal(cmt, bip.Comment);

            Assert.Equal(expected, actualStr);
            Assert.Equal(Encoding.UTF8.GetBytes(expected), actualBa);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new BIP0014(null, new Version(), "comment"));
            Assert.Throws<FormatException>(() => new BIP0014("Satoshi(2)", new Version(), "comment"));
            Assert.Throws<FormatException>(() => new BIP0014("Satoshi:", new Version(), "comment"));
            Assert.Throws<FormatException>(() => new BIP0014("Satoshi", new Version(), ": is invalid"));
            Assert.Throws<FormatException>(() => new BIP0014("Satoshi", new Version(), "/ is also invalid"));
        }


        [Fact]
        public void ToByteArrayMultiTest()
        {
            BIP0014[] bips =
            {
                new BIP0014("Satoshi", new Version(0, 9, 3)),
                new BIP0014("BitcoinJ", new Version(0, 2), "iPad; U; CPU OS 3_2_1"),
                new BIP0014("AndroidBuild", new Version(0, 8))
            };

            byte[] actual = BIP0014.ToByteArrayMulti(bips);
            byte[] expected = Encoding.UTF8.GetBytes("/Satoshi:0.9.3/BitcoinJ:0.2(iPad; U; CPU OS 3_2_1)/AndroidBuild:0.8/");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeList_ExceptionTest()
        {
            BIP0014[] bips =
            {
                new BIP0014("Satoshi", new Version(0, 9, 3), null),
                null,
                new BIP0014("Satoshi", new Version(0, 12, 3), "comment")
            };

            Assert.Throws<ArgumentNullException>(() => BIP0014.ToByteArrayMulti(bips));
            Assert.Throws<ArgumentNullException>(() => BIP0014.ToByteArrayMulti(null));
        }


        [Theory]
        [MemberData(nameof(GetBip14Cases))]
        public void TryParseTest(string name, Version ver, string cmt, string serializedString)
        {
            bool b = BIP0014.TryParse(serializedString, out BIP0014[] bips);

            Assert.True(b);
            Assert.Single(bips);
            Assert.Equal(name, bips[0].ClientName);
            Assert.Equal(ver ?? new Version(0, 0), bips[0].ClientVersion);
            // TryParse returns null comment
            string expCmt = (cmt == "") ? null : cmt;
            Assert.Equal(expCmt, bips[0].Comment);
        }


        [Fact]
        public void TryParse_MultiTest()
        {
            bool b = BIP0014.TryParse("/BitcoinJ:0.2(iPad; U; CPU OS 3_2_1)/AndroidBuild:0.8.6/", out BIP0014[] bips);

            Assert.True(b);
            Assert.Equal(2, bips.Length);

            Assert.Equal("BitcoinJ", bips[0].ClientName);
            Assert.Equal("AndroidBuild", bips[1].ClientName);

            Assert.Equal(new Version(0, 2), bips[0].ClientVersion);
            Assert.Equal(new Version(0, 8, 6), bips[1].ClientVersion);

            Assert.Equal("iPad; U; CPU OS 3_2_1", bips[0].Comment);
            Assert.Null(bips[1].Comment);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Satoshi:0.9")]
        [InlineData("/Satoshi:0.9")]
        [InlineData("Satoshi:0.9/")]
        [InlineData("/Satoshi0.9/")]
        [InlineData("/:Satoshi0.9/")]
        [InlineData("/Satoshi:0.9(comment/")]
        [InlineData("/Satoshi:0.9comment)/")]
        [InlineData("/Satoshi:0.9(co(mm)ent)/")]
        [InlineData("/Satos(hi:0.9comment)/")]
        [InlineData("/Satos)hi:0.9(comment/")]
        [InlineData("/Satoshi:0.x(comment)/")]
        public void TryParse_ErrorTest(string toParse)
        {
            bool b = BIP0014.TryParse(toParse, out BIP0014[] actualBips);

            Assert.False(b);
            Assert.Null(actualBips);
        }
    }
}
