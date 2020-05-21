// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts
{
    public class SignatureScriptTests
    {
        [Fact]
        public void ConstructorTest()
        {
            SignatureScript scr = new SignatureScript();
            Assert.Empty(scr.Data);
        }

        [Fact]
        public void Constructor_WithNullBytesTest()
        {
            byte[] data = null;
            SignatureScript scr = new SignatureScript(data);
            Assert.Empty(scr.Data); // NotNull
        }

        [Fact]
        public void Constructor_WithBytesTest()
        {
            byte[] data = { 1, 2, 3 };
            SignatureScript scr = new SignatureScript(data);
            data[0] = 255; // Make sure data is cloned
            Assert.Equal(new byte[] { 1, 2, 3 }, scr.Data);
        }

        [Fact]
        public void Constructor_OpsTest()
        {
            SignatureScript scr = new SignatureScript(new IOperation[] { new DUPOp(), new PushDataOp(new byte[] { 10, 20, 30 }) });
            Assert.Equal(new byte[] { (byte)OP.DUP, 3, 10, 20, 30 }, scr.Data);
        }

        [Fact]
        public void Constructor_EmptyOpsTest()
        {
            SignatureScript scr = new SignatureScript(new IOperation[0]);
            Assert.Equal(new byte[0], scr.Data);
        }

        [Fact]
        public void Constructor_Ops_ExceptionTest()
        {
            IOperation[] ops = null;
            Assert.Throws<ArgumentNullException>(() => new SignatureScript(ops));
        }

        [Theory]
        [InlineData(100, null, new byte[] { 1, 100 })]
        [InlineData(256, null, new byte[] { 2, 0, 1 })]
        [InlineData(600000, new byte[] { 10, 20, 30, 40, 50 }, new byte[] { 3, 192, 39, 9, 5, 10, 20, 30, 40, 50 })]
        public void Constructor_CoinbaseTest(int height, byte[] extra, byte[] expected)
        {
            SignatureScript scr = new SignatureScript(height, extra);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void Constructor_Coinbase_ExceptionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SignatureScript(-1, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SignatureScript(1, new byte[97]));
        }

        [Fact]
        public void Data_PropertySetter_Test()
        {
            SignatureScript scr = new SignatureScript()
            {
                Data = null
            };
            Assert.Empty(scr.Data); // NotNull
        }


        public static IEnumerable<object[]> GetVerifyCoinbaseCases()
        {
            yield return new object[] { new byte[0], 123, null, false };
            yield return new object[] { new byte[1], 123, null, false };
            yield return new object[] { new byte[101], 123, null, false };
            yield return new object[] { new byte[2], 123, new MockConsensus(123) { bip34 = false }, true };
            yield return new object[] { new byte[2], 123, new MockConsensus(123) { bip34 = true }, false };
            yield return new object[] { new byte[] { 1, 123 }, 123, new MockConsensus(123) { bip34 = true }, true };
            yield return new object[] { new byte[] { 2, 123, 0 }, 123, new MockConsensus(123) { bip34 = true }, false };
            yield return new object[] { new byte[] { 3, 123 }, 123, new MockConsensus(123) { bip34 = true }, false };
        }
        [Theory]
        [MemberData(nameof(GetVerifyCoinbaseCases))]
        public void VerifyCoinbaseTest(byte[] data, int height, IConsensus consensus, bool expected)
        {
            SignatureScript scr = new SignatureScript(data);
            bool actual = scr.VerifyCoinbase(height, consensus);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SetToEmptyTest()
        {
            SignatureScript scr = new SignatureScript(new byte[2]);
            Assert.NotEmpty(scr.Data);
            scr.SetToEmpty();
            Assert.Empty(scr.Data);
        }

        [Fact]
        public void SetToP2PKTest()
        {
            SignatureScript scr = new SignatureScript();
            scr.SetToP2PK(Helper.ShortSig1);
            byte[] expected = Helper.HexToBytes($"{Helper.ShortSig1Hex.Length / 2:x2}{Helper.ShortSig1Hex}");

            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2PKTest_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PK(null));
        }


        public static IEnumerable<object[]> GetP2PKHCases()
        {
            yield return new object[]
            {
                true, KeyHelper.Pub1, Helper.ShortSig1,
                Helper.HexToBytes($"{Helper.ShortSig1Hex.Length / 2:x2}{Helper.ShortSig1Hex}21{KeyHelper.Pub1CompHex}")
            };
            yield return new object[]
            {
                false, KeyHelper.Pub1, Helper.ShortSig1,
                Helper.HexToBytes($"{Helper.ShortSig1Hex.Length / 2:x2}{Helper.ShortSig1Hex}41{KeyHelper.Pub1UnCompHex}")
            };
        }
        [Theory]
        [MemberData(nameof(GetP2PKHCases))]
        public void SetToP2PKHTest(bool useComp, PublicKey pub, Signature sig, byte[] expected)
        {
            SignatureScript scr = new SignatureScript();
            scr.SetToP2PKH(sig, pub, useComp);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2PKH_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(null, KeyHelper.Pub1, true));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(Helper.ShortSig1, null, true));
        }




        [Fact]
        public void SetToP2SH_P2WPKH_FromScriptTest()
        {
            SignatureScript scr = new SignatureScript();
            var rdm = new MockSerializableRedeemScript(RedeemScriptType.P2SH_P2WPKH, new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2SH_P2WPKH(rdm);
            byte[] expected = new byte[] { 3, 1, 2, 3 };
            Assert.Equal(expected, scr.Data);
        }

        public static IEnumerable<object[]> GetP2sh_P2wpkhCases()
        {
            yield return new object[] { KeyHelper.Pub1, true, Helper.HexToBytes($"160014{KeyHelper.Pub1CompHashHex}") };
            yield return new object[] { KeyHelper.Pub1, false, Helper.HexToBytes($"160014{KeyHelper.Pub1UnCompHashHex}") };
        }
        [Theory]
        [MemberData(nameof(GetP2sh_P2wpkhCases))]
        public void SetToP2SH_P2WPKH_FromPubkeyTest(PublicKey pub, bool comp, byte[] expected)
        {
            SignatureScript scr = new SignatureScript();
            scr.SetToP2SH_P2WPKH(pub, comp);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_P2WPKH_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();
            var rdm = new MockSerializableRedeemScript(RedeemScriptType.Empty, new byte[0], 0);

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WPKH(null));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WPKH(null, true));
            Assert.Throws<ArgumentException>(() => scr.SetToP2SH_P2WPKH(rdm));
        }


        [Fact]
        public void SetToP2SH_P2WSHTest()
        {
            SignatureScript scr = new SignatureScript();
            var rdm = new MockSerializableRedeemScript(RedeemScriptType.P2SH_P2WSH, new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2SH_P2WSH(rdm);
            byte[] expected = new byte[] { 3, 1, 2, 3 };
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_P2WSH_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();
            var rdm = new MockSerializableRedeemScript(RedeemScriptType.Empty, new byte[0], 0);

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WSH(null));
            Assert.Throws<ArgumentException>(() => scr.SetToP2SH_P2WSH(rdm));
        }


        [Fact]
        public void SetToCheckLocktimeVerifyTest()
        {
            SignatureScript scr = new SignatureScript();
            var rdm = new MockSerializableRedeemScript(RedeemScriptType.CheckLocktimeVerify, new byte[] { 1, 2, 3 }, 255);
            scr.SetToCheckLocktimeVerify(Helper.ShortSig1, rdm);
            byte[] expected = Helper.HexToBytes($"{Helper.ShortSig1Hex.Length / 2:x2}{Helper.ShortSig1Hex}03010203");

            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToCheckLocktimeVerify_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();
            var rdm = new MockSerializableRedeemScript(RedeemScriptType.Empty, new byte[] { 1, 2, 3 }, 255);

            Assert.Throws<ArgumentException>(() => scr.SetToCheckLocktimeVerify(Helper.ShortSig1, rdm));
            Assert.Throws<ArgumentNullException>(() => scr.SetToCheckLocktimeVerify(null, rdm));
            Assert.Throws<ArgumentNullException>(() => scr.SetToCheckLocktimeVerify(Helper.ShortSig1, null));
        }
    }
}
