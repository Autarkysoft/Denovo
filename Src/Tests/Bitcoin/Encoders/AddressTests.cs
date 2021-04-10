// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Collections.Generic;
using Tests.Bitcoin.Blockchain;
using Xunit;

namespace Tests.Bitcoin.Encoders
{
    public class AddressTests
    {
        private const string P2pkh_main = "17VZNX1SN5NtKa8UQFxwQbFeFc3iqRYhem";
        private const string P2pkh_test = "mipcBbFg9gMiCh81Kj8tqqdgoZub1ZJRfn";
        private const string P2sh_main = "3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX";
        private const string P2sh_test = "2MzQwSSnBHWHqSAqtTVQ6v47XtaisrJa1Vc";
        private const string P2wpkh_main = "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4";
        private const string P2wpkh_test = "tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsx";
        private const string P2wpkh_reg = "bcrt1qkutd3e9qttu3v8w27rtze6r5whyx8mmjy8yf7z";
        private const string P2wsh_main = "bc1qyh75xxtnxrtwjcdtqssjglfy89xrlr3cvnhcvv5w39mwxaty2w7q59qg86";
        private const string P2wsh_test = "tb1qyh75xxtnxrtwjcdtqssjglfy89xrlr3cvnhcvv5w39mwxaty2w7qrdk8a4";

        public static IEnumerable<object[]> GetTypeCases()
        {
            yield return new object[] { P2pkh_main, NetworkType.MainNet, AddressType.P2PKH };
            yield return new object[] { P2pkh_main, NetworkType.TestNet, AddressType.Unknown };
            yield return new object[] { P2pkh_main, NetworkType.RegTest, AddressType.P2PKH };
            yield return new object[] { "17VZNX1SN5NtKa8UQFxwQbFeFc3iqRYhe1", NetworkType.MainNet, AddressType.Unknown };

            yield return new object[] { P2pkh_test, NetworkType.MainNet, AddressType.Unknown };
            yield return new object[] { P2pkh_test, NetworkType.TestNet, AddressType.P2PKH };
            yield return new object[] { P2pkh_test, NetworkType.RegTest, AddressType.Unknown };
            yield return new object[] { "mipcBbFg9gMiCh81Kj8tqqdgoZub1ZJRf1", NetworkType.TestNet, AddressType.Unknown };

            yield return new object[] { P2sh_main, NetworkType.MainNet, AddressType.P2SH };
            yield return new object[] { P2sh_main, NetworkType.TestNet, AddressType.Unknown };
            yield return new object[] { P2sh_main, NetworkType.RegTest, AddressType.P2SH };
            yield return new object[] { "3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQ1", NetworkType.MainNet, AddressType.Unknown };

            yield return new object[] { P2sh_test, NetworkType.MainNet, AddressType.Unknown };
            yield return new object[] { P2sh_test, NetworkType.TestNet, AddressType.P2SH };
            yield return new object[] { P2sh_test, NetworkType.RegTest, AddressType.Unknown };
            yield return new object[] { "2MzQwSSnBHWHqSAqtTVQ6v47XtaisrJa1V1", NetworkType.TestNet, AddressType.Unknown };

            yield return new object[] { P2wpkh_main, NetworkType.MainNet, AddressType.P2WPKH };
            yield return new object[] { P2wpkh_main, NetworkType.TestNet, AddressType.Invalid };
            yield return new object[] { P2wpkh_main, NetworkType.RegTest, AddressType.Invalid };
            yield return new object[] { P2wpkh_main + "1", NetworkType.MainNet, AddressType.Unknown };

            yield return new object[] { P2wpkh_test, NetworkType.MainNet, AddressType.Invalid };
            yield return new object[] { P2wpkh_test, NetworkType.TestNet, AddressType.P2WPKH };
            yield return new object[] { P2wpkh_test, NetworkType.RegTest, AddressType.Invalid };
            yield return new object[] { P2wpkh_test + "1", NetworkType.MainNet, AddressType.Unknown };

            yield return new object[] { P2wpkh_reg, NetworkType.MainNet, AddressType.Invalid };
            yield return new object[] { P2wpkh_reg, NetworkType.TestNet, AddressType.Invalid };
            yield return new object[] { P2wpkh_reg, NetworkType.RegTest, AddressType.P2WPKH };
            yield return new object[] { P2wpkh_reg + "1", NetworkType.MainNet, AddressType.Unknown };

            yield return new object[] { P2wsh_main, NetworkType.MainNet, AddressType.P2WSH };
            yield return new object[] { P2wsh_main, NetworkType.TestNet, AddressType.Invalid };
            yield return new object[] { P2wsh_main, NetworkType.RegTest, AddressType.Invalid };
            yield return new object[] { P2wsh_main + "1", NetworkType.MainNet, AddressType.Unknown };

            yield return new object[] { P2wsh_test, NetworkType.MainNet, AddressType.Invalid };
            yield return new object[] { P2wsh_test, NetworkType.TestNet, AddressType.P2WSH };
            yield return new object[] { P2wsh_test, NetworkType.RegTest, AddressType.Invalid };
            yield return new object[] { P2wsh_test + "1", NetworkType.MainNet, AddressType.Unknown };

            // BIP-173
            yield return new object[] { "tc1qw508d6qejxtdg4y5r3zarvary0c5xw7kg3g4ty", NetworkType.MainNet, AddressType.Invalid };
            yield return new object[] { "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t5", NetworkType.MainNet, AddressType.Unknown };
            yield return new object[] { "BC13W508D6QEJXTDG4Y5R3ZARVARY0C5XW7KN40WF2", NetworkType.MainNet, AddressType.Invalid };
            yield return new object[] { "bc1rw5uspcuh", NetworkType.MainNet, AddressType.Invalid };
            yield return new object[]
            {
                "bc10w508d6qejxtdg4y5r3zarvary0c5xw7kw508d6qejxtdg4y5r3zarvary0c5xw7kw5rljs90",
                NetworkType.MainNet,
                AddressType.Invalid
            };
            yield return new object[] { "BC1QR508D6QEJXTDG4Y5R3ZARVARYV98GJ9P", NetworkType.MainNet, AddressType.Invalid };
            yield return new object[]
            {
                "tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3q0sL5k7", NetworkType.MainNet, AddressType.Unknown
            };
            yield return new object[] { "bc1zw508d6qejxtdg4y5r3zarvaryvqyzf3du", NetworkType.MainNet, AddressType.Unknown };
            yield return new object[]
            {
                "tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3pjxtptv", NetworkType.MainNet, AddressType.Unknown
            };
            yield return new object[] { "bc1gmk9yu", NetworkType.MainNet, AddressType.Unknown };

            // BIP-350
            yield return new object[]
            {
                "tc1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vq5zuyut", NetworkType.MainNet, AddressType.Unknown
            };
            yield return new object[]
            {
                "bc1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vqh2y7hd", NetworkType.MainNet, AddressType.Invalid
            };
            yield return new object[]
            {
                "tb1z0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vqglt7rf", NetworkType.TestNet, AddressType.Invalid
            };
            yield return new object[]
            {
                "BC1S0XLXVLHEMJA6C4DQV22UAPCTQUPFHLXM9H8Z3K2E72Q4K9HCZ7VQ54WELL", NetworkType.MainNet, AddressType.Invalid
            };
            yield return new object[]
            {
                "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kemeawh", NetworkType.MainNet, AddressType.Invalid
            };
            yield return new object[]
            {
                "tb1q0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vq24jc47", NetworkType.TestNet, AddressType.Invalid
            };
            yield return new object[]
            {
                "bc1p38j9r5y49hruaue7wxjce0updqjuyyx0kh56v8s25huc6995vvpql3jow4", NetworkType.MainNet, AddressType.Unknown
            };
            yield return new object[]
            {
                "BC130XLXVLHEMJA6C4DQV22UAPCTQUPFHLXM9H8Z3K2E72Q4K9HCZ7VQ7ZWS8R", NetworkType.MainNet, AddressType.Invalid
            };
            yield return new object[] { "bc1pw5dgrnzv", NetworkType.MainNet, AddressType.Invalid };
            yield return new object[]
            {
                "bc1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7v8n0nx0muaewav253zgeav",
                NetworkType.MainNet,
                AddressType.Invalid
            };
            yield return new object[]
            {
                "tb1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vq47Zagq", NetworkType.TestNet, AddressType.Unknown
            };
            yield return new object[]
            {
                "bc1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7v07qwwzcrf", NetworkType.MainNet, AddressType.Unknown
            };
            yield return new object[]
            {
                "tb1p0xlxvlhemja6c4dqv22uapctqupfhlxm9h8z3k2e72q4k9hcz7vpggkg4j", NetworkType.TestNet, AddressType.Unknown
            };
        }
        [Theory]
        [MemberData(nameof(GetTypeCases))]
        public void GetAddressTypeTest(string address, NetworkType nt, AddressType expected)
        {
            AddressType actual = Address.GetAddressType(address, nt);
            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetP2pkhCases()
        {
            yield return new object[] { KeyHelper.Pub1, true, NetworkType.MainNet, KeyHelper.Pub1CompAddr };
            yield return new object[] { KeyHelper.Pub1, false, NetworkType.MainNet, KeyHelper.Pub1UnCompAddr };
            yield return new object[] { KeyHelper.Pub1, true, NetworkType.RegTest, KeyHelper.Pub1CompAddr };
            yield return new object[] { KeyHelper.Pub1, false, NetworkType.RegTest, KeyHelper.Pub1UnCompAddr };
            yield return new object[] { KeyHelper.Pub1, true, NetworkType.TestNet, "miYt1MwSMJbKF7LbRohHEfm4vAZnPCKArd" };
            yield return new object[] { KeyHelper.Pub1, false, NetworkType.TestNet, "mxD3KcWE9qwhv8hwXma75XDoYqrvHfxHtF" };

            yield return new object[] { KeyHelper.Pub2, true, NetworkType.MainNet, KeyHelper.Pub2CompAddr };
            yield return new object[] { KeyHelper.Pub2, false, NetworkType.MainNet, KeyHelper.Pub2UnCompAddr };

            yield return new object[] { KeyHelper.Pub3, true, NetworkType.MainNet, KeyHelper.Pub3CompAddr };
            yield return new object[] { KeyHelper.Pub3, false, NetworkType.MainNet, KeyHelper.Pub3UnCompAddr };
        }
        [Theory]
        [MemberData(nameof(GetP2pkhCases))]
        public void GetP2pkhTest(PublicKey pub, bool comp, NetworkType netType, string expected)
        {
            string actual = Address.GetP2pkh(pub, comp, netType);
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GetP2pkhTest_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => Address.GetP2pkh(null));
            Assert.Throws<ArgumentException>(() => Address.GetP2pkh(KeyHelper.Pub1, netType: (NetworkType)100));
        }


        public static IEnumerable<object[]> GetP2shCases()
        {
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                NetworkType.MainNet,
                "3Fte5yfJErKGBSVMHpf93sdF6RmtSbTmL1"
            };
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                NetworkType.TestNet,
                "2N7Sr9ibKrJpcPE7txxH1fpcWJmz4FdhJiU"
            };
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                NetworkType.RegTest,
                "3Fte5yfJErKGBSVMHpf93sdF6RmtSbTmL1"
            };
        }
        [Theory]
        [MemberData(nameof(GetP2shCases))]
        public void GetP2shTest(IScript script, NetworkType netType, string expected)
        {
            string actual = Address.GetP2sh(script, netType);
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GetP2sh_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => Address.GetP2sh(null));
            Assert.Throws<ArgumentException>(() => Address.GetP2sh(new MockSerializableRedeemScript(Array.Empty<byte>(), 0), (NetworkType)100));
        }


        public static IEnumerable<object[]> GetP2wpkhCases()
        {
            yield return new object[] { KeyHelper.Pub1, 0, true, NetworkType.MainNet, KeyHelper.Pub1BechAddr };
            yield return new object[] { KeyHelper.Pub1, 0, false, NetworkType.MainNet, KeyHelper.Pub1BechAddrUncomp };
            yield return new object[] { KeyHelper.Pub1, 0, true, NetworkType.RegTest, "bcrt1qy9z6z37mpr5da7c4m07tn95fw85ckqfgzgvcwh" };
            yield return new object[] { KeyHelper.Pub1, 0, false, NetworkType.RegTest, "bcrt1qkutd3e9qttu3v8w27rtze6r5whyx8mmjy8yf7z" };
            yield return new object[]
            {
                KeyHelper.Pub1, 0, true, NetworkType.TestNet, "tb1qy9z6z37mpr5da7c4m07tn95fw85ckqfgqp44e7"
            };
            yield return new object[]
            {
                KeyHelper.Pub1, 0, false, NetworkType.TestNet, "tb1qkutd3e9qttu3v8w27rtze6r5whyx8mmjxwayft"
            };

            yield return new object[] { KeyHelper.Pub2, 0, true, NetworkType.MainNet, KeyHelper.Pub2BechAddr };
            yield return new object[] { KeyHelper.Pub3, 0, true, NetworkType.MainNet, KeyHelper.Pub3BechAddr };
        }
        [Theory]
        [MemberData(nameof(GetP2wpkhCases))]
        public void GetP2wpkhTest(PublicKey pub, byte ver, bool comp, NetworkType netType, string expected)
        {
            string actual = Address.GetP2wpkh(pub, ver, comp, netType);
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GetP2wpkh_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => Address.GetP2wpkh(null, 0));

            Exception ex = Assert.Throws<ArgumentException>(() => Address.GetP2wpkh(KeyHelper.Pub1, 1, netType: (NetworkType)100));
            Assert.Contains("witVer", ex.Message);

            ex = Assert.Throws<ArgumentException>(() => Address.GetP2wpkh(KeyHelper.Pub1, 0, netType: (NetworkType)100));
            Assert.Contains(Err.InvalidNetwork, ex.Message);
        }


        public static IEnumerable<object[]> GetP2sh_P2wpkhCases()
        {
            yield return new object[] { KeyHelper.Pub1, 0, true, NetworkType.MainNet, KeyHelper.Pub1NestedSegwit };
            yield return new object[] { KeyHelper.Pub1, 0, false, NetworkType.MainNet, KeyHelper.Pub1NestedSegwitUncomp };
            yield return new object[] { KeyHelper.Pub1, 0, true, NetworkType.RegTest, KeyHelper.Pub1NestedSegwit };
            yield return new object[] { KeyHelper.Pub1, 0, false, NetworkType.RegTest, KeyHelper.Pub1NestedSegwitUncomp };
            yield return new object[] { KeyHelper.Pub1, 0, true, NetworkType.TestNet, "2N1UvtAhuV4nYsqVznNuYTPU2R9ajf49xaV" };
            yield return new object[] { KeyHelper.Pub1, 0, false, NetworkType.TestNet, "2N6t2wK9J7Yi8NZgCV1nXHFKGFLK4xyDkQe" };

            yield return new object[] { KeyHelper.Pub2, 0, true, NetworkType.MainNet, KeyHelper.Pub2NestedSegwit };
            yield return new object[] { KeyHelper.Pub3, 0, true, NetworkType.MainNet, KeyHelper.Pub3NestedSegwit };

        }
        [Theory]
        [MemberData(nameof(GetP2sh_P2wpkhCases))]
        public void GetP2sh_P2wpkhTest(PublicKey pub, byte ver, bool comp, NetworkType netType, string expected)
        {
            string actual = Address.GetP2sh_P2wpkh(pub, ver, comp, netType);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetP2sh_P2wpkh_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => Address.GetP2sh_P2wpkh(null, 0));

            Exception ex = Assert.Throws<ArgumentException>(() => Address.GetP2sh_P2wpkh(KeyHelper.Pub1, 1, netType: (NetworkType)100));
            Assert.Contains("witVer", ex.Message);

            ex = Assert.Throws<ArgumentException>(() => Address.GetP2sh_P2wpkh(KeyHelper.Pub1, 0, netType: (NetworkType)100));
            Assert.Contains(Err.InvalidNetwork, ex.Message);
        }


        public static IEnumerable<object[]> GetP2wshCases()
        {
            yield return new object[]
            {
                // https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki#examples
                new MockSerializableRedeemScript(Helper.HexToBytes("210279BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798ac"), 255),
                0, NetworkType.MainNet,
                "bc1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3qccfmv3"
            };
            yield return new object[]
            {
                // https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki#examples
                new MockSerializableRedeemScript(Helper.HexToBytes("210279BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798ac"), 255),
                0, NetworkType.TestNet,
                "tb1qrp33g0q5c5txsp9arysrx4k6zdkfs4nce4xj0gdcccefvpysxf3q0sl5k7"
            };
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                0, NetworkType.MainNet,
                "bc1qqwg933hjcr95jtzn8v9y6980wlxq779ten8d22rasjs6yqgulwqs2z4mcu"
            };
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                0, NetworkType.TestNet,
                "tb1qqwg933hjcr95jtzn8v9y6980wlxq779ten8d22rasjs6yqgulwqsa2r5zn"
            };
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                0, NetworkType.RegTest,
                "bcrt1qqwg933hjcr95jtzn8v9y6980wlxq779ten8d22rasjs6yqgulwqssnfjhf"
            };
        }
        [Theory]
        [MemberData(nameof(GetP2wshCases))]
        public void GetP2wshTest(IScript script, byte witVer, NetworkType netType, string expected)
        {
            string actual = Address.GetP2wsh(script, witVer, netType);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetP2wsh_ExceptionTest()
        {
            var scr = new MockSerializableRedeemScript(Array.Empty<byte>(), 0);

            Assert.Throws<ArgumentNullException>(() => Address.GetP2wsh(null, 0));
            Assert.Throws<ArgumentException>(() => Address.GetP2wsh(scr, 1));
            Assert.Throws<ArgumentException>(() => Address.GetP2wsh(scr, 0, (NetworkType)100));
        }


        public static IEnumerable<object[]> GetP2sh_P2wshCases()
        {
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                0, NetworkType.MainNet,
                "3DrYxHaW5vdirRa74yaLDaes3S2cghg7V4"
            };
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                0, NetworkType.TestNet,
                "2N5Qm22WXhP954DCek7CCqXe8FnEnVjZqZK"
            };
            yield return new object[]
            {
                new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255),
                0, NetworkType.MainNet,
                "3DrYxHaW5vdirRa74yaLDaes3S2cghg7V4"
            };
        }
        [Theory]
        [MemberData(nameof(GetP2sh_P2wshCases))]
        public void GetP2sh_P2wshTest(IScript script, byte witVer, NetworkType netType, string expected)
        {
            string actual = Address.GetP2sh_P2wsh(script, witVer, netType);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetP2sh_P2wsh_ExceptionTest()
        {
            var scr = new MockSerializableRedeemScript(Array.Empty<byte>(), 0);

            Assert.Throws<ArgumentNullException>(() => Address.GetP2sh_P2wsh(null, 0));
            Assert.Throws<ArgumentException>(() => Address.GetP2sh_P2wsh(scr, 1));
            Assert.Throws<ArgumentException>(() => Address.GetP2sh_P2wsh(scr, 0, (NetworkType)100));
        }


        public static IEnumerable<object[]> GetVerifyCases()
        {
            yield return new object[]
            {
                P2pkh_main,
                PubkeyScriptType.P2PKH,
                Helper.HexToBytes("47376c6f537d62177a2c41c4ca9b45829ab99083")
            };
            yield return new object[]
            {
                P2pkh_test,
                PubkeyScriptType.P2PKH,
                Helper.HexToBytes("243f1394f44554f4ce3fd68649c19adc483ce924")
            };
            yield return new object[]
            {
                "3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX",
                PubkeyScriptType.P2SH,
                Helper.HexToBytes("8f55563b9a19f321c211e9b9f38cdf686ea07845")
            };
            yield return new object[]
            {
                "2MzQwSSnBHWHqSAqtTVQ6v47XtaisrJa1Vc",
                PubkeyScriptType.P2SH,
                Helper.HexToBytes("4e9f39ca4688ff102128ea4ccda34105324305b0")
            };
            yield return new object[]
            {
                "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4",
                PubkeyScriptType.P2WPKH,
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323f1433bd6")
            };
            yield return new object[]
            {
                "tb1qw508d6qejxtdg4y5r3zarvary0c5xw7kxpjzsx",
                PubkeyScriptType.P2WPKH,
                Helper.HexToBytes("751e76e8199196d454941c45d1b3a323f1433bd6")
            };
            yield return new object[]
            {
                "bc1qyh75xxtnxrtwjcdtqssjglfy89xrlr3cvnhcvv5w39mwxaty2w7q59qg86",
                PubkeyScriptType.P2WSH,
                Helper.HexToBytes("25fd43197330d6e961ab0421247d24394c3f8e3864ef86328e8976e3756453bc")
            };
            yield return new object[]
            {
                "tb1qyh75xxtnxrtwjcdtqssjglfy89xrlr3cvnhcvv5w39mwxaty2w7qrdk8a4",
                PubkeyScriptType.P2WSH,
                Helper.HexToBytes("25fd43197330d6e961ab0421247d24394c3f8e3864ef86328e8976e3756453bc")
            };
        }
        [Theory]
        [MemberData(nameof(GetVerifyCases))]
        public void VerifyTypeTest(string address, PubkeyScriptType scrType, byte[] expected)
        {
            bool b = Address.VerifyType(address, scrType, out byte[] actual);

            Assert.True(b);
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetVerifyFailCases()
        {
            yield return new object[] { null, PubkeyScriptType.P2PKH };
            yield return new object[] { "", PubkeyScriptType.P2PKH };
            yield return new object[] { " ", PubkeyScriptType.P2PKH };
            yield return new object[] { P2pkh_main, PubkeyScriptType.P2PK };
            yield return new object[] { P2pkh_main, PubkeyScriptType.P2SH };
            yield return new object[] { P2pkh_main, PubkeyScriptType.P2WPKH };
            yield return new object[] { P2pkh_main, PubkeyScriptType.P2WSH };
            yield return new object[] { P2pkh_main, (PubkeyScriptType)100 };
            yield return new object[] { "3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", PubkeyScriptType.P2PK };
            yield return new object[] { "3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", PubkeyScriptType.P2PKH };
            yield return new object[] { "3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", PubkeyScriptType.P2WPKH };
            yield return new object[] { "3EktnHQD7RiAE6uzMj2ZifT9YgRrkSgzQX", PubkeyScriptType.P2WSH };
            yield return new object[] { "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", PubkeyScriptType.P2PK };
            yield return new object[] { "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", PubkeyScriptType.P2PKH };
            yield return new object[] { "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", PubkeyScriptType.P2SH };
            yield return new object[] { "bc1qw508d6qejxtdg4y5r3zarvary0c5xw7kv8f3t4", PubkeyScriptType.P2WSH };
            yield return new object[] { "bc1qyh75xxtnxrtwjcdtqssjglfy89xrlr3cvnhcvv5w39mwxaty2w7q59qg86", PubkeyScriptType.P2PK };
            yield return new object[] { "bc1qyh75xxtnxrtwjcdtqssjglfy89xrlr3cvnhcvv5w39mwxaty2w7q59qg86", PubkeyScriptType.P2PKH };
            yield return new object[] { "bc1qyh75xxtnxrtwjcdtqssjglfy89xrlr3cvnhcvv5w39mwxaty2w7q59qg86", PubkeyScriptType.P2SH };
            yield return new object[] { "bc1qyh75xxtnxrtwjcdtqssjglfy89xrlr3cvnhcvv5w39mwxaty2w7q59qg86", PubkeyScriptType.P2WPKH };
        }
        [Theory]
        [MemberData(nameof(GetVerifyFailCases))]
        public void VerifyType_FailTest(string address, PubkeyScriptType scrType)
        {
            bool b = Address.VerifyType(address, scrType, out byte[] actual);

            Assert.False(b);
            Assert.Null(actual);
        }
    }
}
