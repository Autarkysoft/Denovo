// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
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
        // Small height (the following 2 cases) pad the script with "Bitcoin.Net" to avoid invalid coinbase script length
        [InlineData(1, null, new byte[] { (byte)OP._1, 11, 66, 105, 116, 99, 111, 105, 110, 46, 78, 101, 116 })]
        [InlineData(2, null, new byte[] { (byte)OP._2, 11, 66, 105, 116, 99, 111, 105, 110, 46, 78, 101, 116 })]
        [InlineData(17, null, new byte[] { 1, 17 })]
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
            yield return new object[] { new byte[0], null, false };
            yield return new object[] { new byte[1], null, false };
            yield return new object[] { new byte[101], null, false };
            yield return new object[] { new byte[2], new MockConsensus() { expHeight = 123, bip34 = false }, true };
            yield return new object[] { new byte[2], new MockConsensus() { expHeight = 123, bip34 = true }, false };
            yield return new object[] { new byte[] { 1, 123 }, new MockConsensus() { expHeight = 123, bip34 = true }, true };
            yield return new object[] { new byte[] { 2, 123, 0 }, new MockConsensus() { expHeight = 123, bip34 = true }, false };
            yield return new object[] { new byte[] { 3, 123 }, new MockConsensus() { expHeight = 123, bip34 = true }, false };
            // Test endianness, taken from block #643158
            yield return new object[]
            {
                new byte[] { 0x03, 0x56, 0xd0, 0x09 }, new MockConsensus() { expHeight = 643158,  bip34 = true }, true
            };
            yield return new object[]
            {
                new byte[] { 0x03, 0x09, 0xd0, 0x56 }, new MockConsensus() { expHeight = 643158, bip34 = true }, false
            };
            // Bad StackInt is used for pushing
            yield return new object[]
            {
                new byte[] { (byte)OP.PushData1, 1, 123 }, new MockConsensus() { expHeight = 123, bip34 = true }, false
            };
        }
        [Theory]
        [MemberData(nameof(GetVerifyCoinbaseCases))]
        public void VerifyCoinbaseTest(byte[] data, IConsensus consensus, bool expected)
        {
            SignatureScript scr = new SignatureScript(data);
            bool actual = scr.VerifyCoinbase(consensus);
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


        // A helper to be used only for small pushes
        private static string GetPush(string hexToPush) => $"{hexToPush.Length / 2:x2}{hexToPush}";

        public static IEnumerable<object[]> GetMultiSigCases()
        {
            string rdm1of1 = $"51{GetPush(KeyHelper.Pub1CompHex)}51ae";
            string rdm1of2 = $"51{GetPush(KeyHelper.Pub1CompHex)}{GetPush(KeyHelper.Pub2CompHex)}52ae";
            string rdm2of2 = $"52{GetPush(KeyHelper.Pub1CompHex)}{GetPush(KeyHelper.Pub2CompHex)}52ae";

            Signature.TryReadStrict(Helper.HexToBytes(KeyHelper.VerifiableSignature1), out Signature sig1, out _);
            Signature.TryReadStrict(Helper.HexToBytes(KeyHelper.VerifiableSignature2), out Signature sig2, out _);
            Signature.TryReadStrict(Helper.HexToBytes(KeyHelper.VerifiableSignature3), out Signature sig3, out _);

            yield return new object[]
            {
                // Set signature for a 1of1 multi sig with nothing set yet
                null,
                sig1,
                new RedeemScript(Helper.HexToBytes(rdm1of1)),
                new MockSignableTx(Helper.HexToBytes(KeyHelper.VerifiableDataToSign)){ TxInList = new TxIn[1] },
                0,
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(rdm1of1)}")
            };
            yield return new object[]
            {
                // Same as above but OP_0 and RedeemScript are already set
                Helper.HexToBytes($"00{GetPush(rdm1of1)}"),
                sig1,
                new RedeemScript(Helper.HexToBytes(rdm1of1)),
                new MockSignableTx(Helper.HexToBytes(KeyHelper.VerifiableDataToSign)){ TxInList = new TxIn[1] },
                0,
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(rdm1of1)}")
            };
            yield return new object[]
            {
                // Same as above but other bytes are set in SignatureScript (missing RedeemScript)
                new byte[] { (byte)OP._0, (byte)OP._1 },
                sig1,
                new RedeemScript(Helper.HexToBytes(rdm1of1)),
                new MockSignableTx(Helper.HexToBytes(KeyHelper.VerifiableDataToSign)){ TxInList = new TxIn[1] },
                0,
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(rdm1of1)}")
            };
            yield return new object[]
            {
                // 1of2 with nothing set yet
                null,
                sig1,
                new RedeemScript(Helper.HexToBytes(rdm1of2)),
                new MockSignableTx(Helper.HexToBytes(KeyHelper.VerifiableDataToSign)){ TxInList = new TxIn[1] },
                0,
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(rdm1of2)}")
            };
            yield return new object[]
            {
                // Same as above with sig for the other pubkey
                null,
                sig2,
                new RedeemScript(Helper.HexToBytes(rdm1of2)),
                new MockSignableTx(Helper.HexToBytes(KeyHelper.VerifiableDataToSign)){ TxInList = new TxIn[1] },
                0,
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature2)}{GetPush(rdm1of2)}")
            };
            yield return new object[]
            {
                // Same as above but another valid sig is already set
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(rdm1of2)}"),
                sig2,
                new RedeemScript(Helper.HexToBytes(rdm1of2)),
                new MockSignableTx(Helper.HexToBytes(KeyHelper.VerifiableDataToSign)){ TxInList = new TxIn[1] },
                0,
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature2)}{GetPush(rdm1of2)}")
            };
            yield return new object[]
            {
                // Same as above but same valid sig is already set
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(rdm1of2)}"),
                sig1,
                new RedeemScript(Helper.HexToBytes(rdm1of2)),
                new MockSignableTx(Helper.HexToBytes(KeyHelper.VerifiableDataToSign)){ TxInList = new TxIn[1] },
                0,
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(rdm1of2)}")
            };
            yield return new object[]
            {
                // 2of2 with 1 sig already set
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(rdm2of2)}"),
                sig2,
                new RedeemScript(Helper.HexToBytes(rdm2of2)),
                new MockSignableTx(Helper.HexToBytes(KeyHelper.VerifiableDataToSign)){ TxInList = new TxIn[1] },
                0,
                Helper.HexToBytes($"00{GetPush(KeyHelper.VerifiableSignature1)}{GetPush(KeyHelper.VerifiableSignature2)}" +
                                  $"{GetPush(rdm2of2)}")
            };
        }
        [Theory]
        [MemberData(nameof(GetMultiSigCases))]
        public void SetToMultiSigTest(byte[] scrData, Signature sig, IRedeemScript rdm, ITransaction tx, int index, byte[] expected)
        {
            var scr = new SignatureScript(scrData);
            scr.SetToMultiSig(sig, rdm, tx, index);
            Assert.Equal(expected, scr.Data);
        }

        public static IEnumerable<object[]> GetMultiSigNullExCases()
        {
            var rdm = new MockSerializableRedeemScript(RedeemScriptType.MultiSig, new byte[1], 0);
            var tx = new MockTxIdTx("") { TxInList = new TxIn[1] };

            yield return new object[] { null, rdm, tx, 0, "Signature can not be null." };
            yield return new object[] { Helper.ShortSig1, null, tx, 0, "Redeem script can not be null." };
            yield return new object[] { Helper.ShortSig1, rdm, null, 0, "Transaction can not be null." };
        }
        [Theory]
        [MemberData(nameof(GetMultiSigNullExCases))]
        public void SetToMultiSig_NullExceptionTest(Signature sig, IRedeemScript rdm, ITransaction tx, int index, string expErr)
        {
            var scr = new SignatureScript();
            Exception ex = Assert.Throws<ArgumentNullException>(() => scr.SetToMultiSig(sig, rdm, tx, index));
            Assert.Contains(expErr, ex.Message);
        }

        public static IEnumerable<object[]> GetMultiSigOutOfRangeExCases()
        {
            var rdm = new MockSerializableRedeemScript(RedeemScriptType.MultiSig, new byte[1], 0);
            var tx = new MockTxIdTx("") { TxInList = new TxIn[1] };
            var zero = new PushDataOp(OP._0);
            var one = new PushDataOp(OP._1);
            var two = new PushDataOp(OP._2);
            var neg = new PushDataOp(OP.Negative1);
            var chsig = new CheckMultiSigOp();

            yield return new object[] { Helper.ShortSig1, rdm, tx, -1, "Invalid input index." };
            yield return new object[] { Helper.ShortSig1, rdm, tx, 1, "Invalid input index." };
            yield return new object[]
            {
                Helper.ShortSig1,
                new MockSerializableRedeemScript(RedeemScriptType.MultiSig, new byte[Constants.MaxScriptItemLength+1], 0),
                tx, 0, "Redeem script is bigger than allowed length."
            };
            yield return new object[]
            {
                Helper.ShortSig1,
                new MockEvaluatableRedeemScript(RedeemScriptType.MultiSig, new IOperation[] { neg }, 0),
                tx, 0, "M can not be negative."
            };
            yield return new object[]
            {
                Helper.ShortSig1,
                new MockEvaluatableRedeemScript(RedeemScriptType.MultiSig, new IOperation[] { zero }, 0),
                tx, 0, "M value zero is not allowed"
            };
            yield return new object[]
            {
                Helper.ShortSig1,
                new MockEvaluatableRedeemScript(RedeemScriptType.MultiSig, new IOperation[] { one, neg, chsig }, 0),
                tx, 0, "N can not be negative."
            };
            yield return new object[]
            {
                Helper.ShortSig1,
                new MockEvaluatableRedeemScript(RedeemScriptType.MultiSig, new IOperation[] { one, zero, chsig }, 0),
                tx, 0, "N value zero is not allowed"
            };
            yield return new object[]
            {
                Helper.ShortSig1,
                new MockEvaluatableRedeemScript(RedeemScriptType.MultiSig, new IOperation[] { two, one, chsig }, 0),
                tx, 0, "M can not be bigger than N."
            };
        }
        [Theory]
        [MemberData(nameof(GetMultiSigOutOfRangeExCases))]
        public void SetToMultiSig_OutOfRangeExceptionTest(Signature sig, IRedeemScript rdm, ITransaction tx, int index, string expErr)
        {
            var scr = new SignatureScript();
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => scr.SetToMultiSig(sig, rdm, tx, index));
            Assert.Contains(expErr, ex.Message);
        }

        public static IEnumerable<object[]> GetMultiSigArgExCases()
        {
            var tx = new MockTxIdTx("") { TxInList = new TxIn[1] };
            PushDataOp badNum = new PushDataOp();
            badNum.TryRead(new FastStreamReader(new byte[] { 1, 0 }), out _);
            var two = new PushDataOp(OP._2);
            var chsig = new CheckMultiSigOp();

            yield return new object[]
            {
                Helper.ShortSig1, new MockSerializableRedeemScript(RedeemScriptType.Empty, new byte[0], 0),
                tx, 0, "Invalid redeem script type."
            };
            yield return new object[]
            {
                Helper.ShortSig1, new MockEvaluatableRedeemScript(RedeemScriptType.MultiSig, null, 0),
                tx, 0, "Can not evaluate redeem script: Foo"
            };
            yield return new object[]
            {
                Helper.ShortSig1,
                new MockEvaluatableRedeemScript(RedeemScriptType.MultiSig, new IOperation[]{ badNum }, 0),
                tx, 0, "Invalid m"
            };
            yield return new object[]
            {
                Helper.ShortSig1,
                new MockEvaluatableRedeemScript(RedeemScriptType.MultiSig, new IOperation[]{ two, badNum, chsig }, 0),
                tx, 0, "Invalid n"
            };
        }
        [Theory]
        [MemberData(nameof(GetMultiSigArgExCases))]
        public void SetToMultiSig_ArgumentExceptionTest(Signature sig, IRedeemScript rdm, ITransaction tx, int index, string expErr)
        {
            var scr = new SignatureScript();
            Exception ex = Assert.Throws<ArgumentException>(() => scr.SetToMultiSig(sig, rdm, tx, index));
            Assert.Contains(expErr, ex.Message);
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
