// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts
{
    public class RedeemScriptTests
    {
        private const int MockHeight = 123;

        [Fact]
        public void ConstructorTest()
        {
            var scr = new RedeemScript();
            Assert.Empty(scr.Data);
        }

        [Fact]
        public void Constructor_WithNullBytesTest()
        {
            byte[] data = null;
            var scr = new RedeemScript(data);
            Assert.Empty(scr.Data); // NotNull
        }

        [Fact]
        public void Constructor_WithBytesTest()
        {
            byte[] data = Helper.GetBytes(10);
            var scr = new RedeemScript(data);
            Assert.Equal(data, scr.Data);
        }

        [Fact]
        public void Constructor_OpsTest()
        {
            var scr = new RedeemScript(new IOperation[] { new DUPOp(), new PushDataOp(new byte[] { 10, 20, 30 }) });
            Assert.Equal(new byte[] { (byte)OP.DUP, 3, 10, 20, 30 }, scr.Data);
        }

        [Fact]
        public void Constructor_EmptyOpsTest()
        {
            var scr = new RedeemScript(new IOperation[0]);
            Assert.Equal(new byte[0], scr.Data);
        }

        [Fact]
        public void Constructor_NullOpsTest()
        {
            IOperation[] ops = null;
            Assert.Throws<ArgumentNullException>(() => new RedeemScript(ops));
        }

        public static IEnumerable<object[]> GetScrTypeCases()
        {
            yield return new object[] { new RedeemScript(), RedeemScriptType.Empty };
            yield return new object[] { new RedeemScript(new byte[] { 255 }), RedeemScriptType.Unknown };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new DUPOp() }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(OP._0), new PushDataOp(new byte[20]) }),
                RedeemScriptType.P2SH_P2WPKH
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(OP._1), new PushDataOp(new byte[20]) }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(OP._0), new PushDataOp(new byte[21]) }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(OP._0), new PushDataOp(OP._1) }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(new byte[20]), new PushDataOp(new byte[20]) }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(OP._0), new PushDataOp(new byte[32]) }),
                RedeemScriptType.P2SH_P2WSH
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(OP._1), new PushDataOp(new byte[32]) }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(OP._0), new PushDataOp(new byte[33]) }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[]
                                {
                                    new PushDataOp(123),
                                    new CheckLocktimeVerifyOp(),
                                    new DROPOp(),
                                    new PushDataOp(new byte[33]),
                                    new CheckSigOp()
                                }),
                RedeemScriptType.CheckLocktimeVerify
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[]
                                {
                                    new PushDataOp(123),
                                    new CheckLocktimeVerifyOp(),
                                    new DROPOp(),
                                    new PushDataOp(new byte[65]),
                                    new CheckSigOp()
                                }),
                RedeemScriptType.CheckLocktimeVerify
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[]
                                {
                                    new PushDataOp(new byte[6]),
                                    new CheckLocktimeVerifyOp(),
                                    new DROPOp(),
                                    new PushDataOp(new byte[33]),
                                    new CheckSigOp()
                                }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[]
                                {
                                    new PushDataOp(123),
                                    new CheckLocktimeVerifyOp(),
                                    new DROPOp(),
                                    new PushDataOp(new byte[34]),
                                    new CheckSigOp()
                                }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[]
                                {
                                    new PushDataOp(123),
                                    new CheckLocktimeVerifyOp(),
                                    new DROPOp(),
                                    new PushDataOp(OP._1),
                                    new CheckSigOp()
                                }),
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new CheckMultiSigOp() }),
                RedeemScriptType.MultiSig
            };
            yield return new object[]
            {
                new RedeemScript(new IOperation[] { new PushDataOp(OP._1), new AddOp(), new CheckMultiSigOp() }),
                RedeemScriptType.Unknown
            };
        }
        [Theory]
        [MemberData(nameof(GetScrTypeCases))]
        public void GetRedeemScriptTypeTest(IRedeemScript scr, RedeemScriptType expected)
        {
            RedeemScriptType actual = scr.GetRedeemScriptType();
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetSpecialScrTypeCases()
        {
            yield return new object[] { new MockConsensus() { segWit = true }, null, RedeemScriptSpecialType.None };
            yield return new object[] { new MockConsensus() { segWit = false }, null, RedeemScriptSpecialType.None };
            yield return new object[] { new MockConsensus() { segWit = true }, new byte[22], RedeemScriptSpecialType.None };
            yield return new object[] { new MockConsensus() { segWit = false }, new byte[22], RedeemScriptSpecialType.None };
            // The following 2 are from https://github.com/bitcoin/bips/blob/master/bip-0143.mediawiki
            yield return new object[]
            {
                new MockConsensus() { segWit = false },
                Helper.HexToBytes("001479091972186c449eb1ded22b78e40d009bdf0089"),
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes("001479091972186c449eb1ded22b78e40d009bdf0089"),
                RedeemScriptSpecialType.P2SH_P2WPKH
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = false },
                Helper.HexToBytes("0020a16b5755f7f6f96dbd65f5f0d6ab9418b89af4b1f14a1bb8a09062c35f0dcb54"),
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes("0020a16b5755f7f6f96dbd65f5f0d6ab9418b89af4b1f14a1bb8a09062c35f0dcb54"),
                RedeemScriptSpecialType.P2SH_P2WSH
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes($"0014{Helper.GetBytesHex(21)}"), // Has 1 extra byte outside of the push => is not witness
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = false },
                Helper.HexToBytes($"0014{Helper.GetBytesHex(21)}"),
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes($"0015{Helper.GetBytesHex(21)}"), // Invalid push length
                RedeemScriptSpecialType.InvalidWitness
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = false },
                Helper.HexToBytes($"0015{Helper.GetBytesHex(21)}"),
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes($"0015{Helper.GetBytesHex(21)}"),
                RedeemScriptSpecialType.InvalidWitness
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = false },
                Helper.HexToBytes($"0015{Helper.GetBytesHex(21)}"),
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes($"5101ff"), // OP_1 push(0xff) -> len < 4 -> not witness
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes($"6029{Helper.GetBytesHex(41)}"), // OP_16 push(data40) -> len > 42 -> not witness
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes($"5114{Helper.GetBytesHex(20)}"), // This case may need to update when version 1 is added
                RedeemScriptSpecialType.UnknownWitness
            };
            yield return new object[]
            {
                new MockConsensus() { bip16 = true, segWit = true },
                Helper.HexToBytes($"0014{Helper.GetBytesHex(20)}87"), // Has an extra OP code at the end
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes("604c020001"), // 0x60 is OP_16 and 0x4c is OP_PushData1
                RedeemScriptSpecialType.None
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes("6003020001"), // 0x60 is OP_16 but push is correct
                RedeemScriptSpecialType.UnknownWitness
            };
            yield return new object[]
            {
                new MockConsensus() { segWit = true },
                Helper.HexToBytes("010103abcdef"), // Starts with 0x01 instead of OP_1=0x51
                RedeemScriptSpecialType.None
            };
        }
        [Theory]
        [MemberData(nameof(GetSpecialScrTypeCases))]
        public void RedeemScriptSpecialTypeTest(IConsensus c, byte[] data, RedeemScriptSpecialType expected)
        {
            var scr = new RedeemScript(data);
            RedeemScriptSpecialType actual = scr.GetSpecialType(c);
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetSigOpCountCases()
        {
            yield return new object[] { new IOperation[0], 0 };
            yield return new object[] { new IOperation[] { new DUPOp(), new Hash160Op(), new Sha1Op() }, 0 };
            yield return new object[] { new IOperation[] { new ADD1Op(), new CheckSigOp(), new CheckSigVerifyOp() }, 2 };
            yield return new object[]
            {
                new IOperation[] { new CheckMultiSigOp() },
                20
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new CheckMultiSigOp() },
                20
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(OP.Negative1), new CheckMultiSigOp() },
                20
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(OP._1), new CheckMultiSigOp() },
                1
            };
            yield return new object[]
            {
                // Core counts 0 as 20 sigops!
                // https://github.com/bitcoin/bitcoin/blob/24f70290642c9c5108d3dc62dbe055f5d1bcff9d/src/script/script.cpp#L162-L165
                new IOperation[] { new PushDataOp(OP._0), new CheckMultiSigOp() },
                20
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(OP._5), new CheckMultiSigOp(), new PushDataOp(OP._6), new CheckMultiSigOp() },
                11
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new PushDataOp(OP._5), new PushDataOp(OP._1), new IFOp(new IOperation[] { new CheckMultiSigOp() }, null)
                },
                20
            };
        }
        [Theory]
        [MemberData(nameof(GetSigOpCountCases))]
        public void CountSigOpsTest(IOperation[] ops, int expected)
        {
            var scr = new RedeemScript();
            int actual = scr.CountSigOps(ops);
            Assert.Equal(expected, actual);
        }
        [Theory]
        [MemberData(nameof(GetSigOpCountCases))]
        public void CountSigOps_OverrideMethodTest(IOperation[] ops, int expected)
        {
            // Make sure the default sigop counter is overriden
            var scr = new RedeemScript(ops);
            int actual = scr.CountSigOps();
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetSetToMultiSigCases()
        {
            yield return new object[]
            {
                1, new PublicKey[] { KeyHelper.Pub1 }, true,
                Helper.HexToBytes($"5121{KeyHelper.Pub1CompHex}51ae")
            };
            yield return new object[]
            {
                1, new PublicKey[] { KeyHelper.Pub1 }, false,
                Helper.HexToBytes($"5141{KeyHelper.Pub1UnCompHex}51ae")
            };
            yield return new object[]
            {
                1, new PublicKey[] { KeyHelper.Pub1, KeyHelper.Pub2 }, true,
                Helper.HexToBytes($"5121{KeyHelper.Pub1CompHex}21{KeyHelper.Pub2CompHex}52ae")
            };
            yield return new object[]
            {
                1, new PublicKey[] { KeyHelper.Pub1, KeyHelper.Pub2 }, false,
                Helper.HexToBytes($"5141{KeyHelper.Pub1UnCompHex}41{KeyHelper.Pub2UnCompHex}52ae")
            };
            yield return new object[]
            {
                2, new PublicKey[] { KeyHelper.Pub1, KeyHelper.Pub2 }, false,
                Helper.HexToBytes($"5241{KeyHelper.Pub1UnCompHex}41{KeyHelper.Pub2UnCompHex}52ae")
            };
        }
        [Theory]
        [MemberData(nameof(GetSetToMultiSigCases))]
        public void SetToMultiSigTest(int m, PublicKey[] pubs, bool comp, byte[] expected)
        {
            RedeemScript scr = new RedeemScript();
            scr.SetToMultiSig(m, pubs, comp);

            Assert.Equal(expected, scr.Data);
        }

        public static IEnumerable<object[]> GetSetToMultiSigFailCases()
        {
            yield return new object[]
            {
                -1, new PublicKey[2], true,
                "M must be between 1 and (7 uncomprssed or 15 compressed) public keys and smaller than N."
            };
            yield return new object[]
            {
                0, new PublicKey[2], true,
                "M must be between 1 and (7 uncomprssed or 15 compressed) public keys and smaller than N."
            };
            yield return new object[]
            {
                3, new PublicKey[2], true,
                "M must be between 1 and (7 uncomprssed or 15 compressed) public keys and smaller than N."
            };
            yield return new object[]
            {
                16, new PublicKey[16], true,
                "M must be between 1 and (7 uncomprssed or 15 compressed) public keys and smaller than N."
            };
            yield return new object[]
            {
                8, new PublicKey[8], false,
                "M must be between 1 and (7 uncomprssed or 15 compressed) public keys and smaller than N."
            };
            yield return new object[]
            {
                1, new PublicKey[16], true,
                "Pubkey list must contain at least 1 and at most 15 compressed or 7 uncompressed keys."
            };
            yield return new object[]
            {
                1, new PublicKey[8], false,
                "Pubkey list must contain at least 1 and at most 15 compressed or 7 uncompressed keys."
            };
        }
        [Theory]
        [MemberData(nameof(GetSetToMultiSigFailCases))]
        public void SetToMultiSig_OutOfRangeExceptionTest(int m, PublicKey[] pubs, bool comp, string expError)
        {
            RedeemScript scr = new RedeemScript();
            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => scr.SetToMultiSig(m, pubs, comp));
            Assert.Contains(expError, ex.Message);
        }

        [Fact]
        public void SetToMultiSig_NullExceptionTest()
        {
            RedeemScript scr = new RedeemScript();
            Assert.Throws<ArgumentNullException>(() => scr.SetToMultiSig(1, null, true));
            Assert.Throws<ArgumentNullException>(() => scr.SetToMultiSig(1, new PublicKey[0], true));
        }


        [Fact]
        public void SetToP2SH_P2WPKH_CompTest()
        {
            RedeemScript scr = new RedeemScript();
            scr.SetToP2SH_P2WPKH(KeyHelper.Pub1, true);
            byte[] expected = Helper.HexToBytes($"0014{KeyHelper.Pub1CompHashHex}");

            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_P2WPKH_UnCompTest()
        {
            RedeemScript scr = new RedeemScript();
            // This is non-standard
            scr.SetToP2SH_P2WPKH(KeyHelper.Pub1, false);

            byte[] expected = Helper.HexToBytes($"0014{KeyHelper.Pub1UnCompHashHex}");

            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_P2WPKH_ExceptionTest()
        {
            RedeemScript scr = new RedeemScript();
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WPKH(null, false));
        }


        [Fact]
        public void SetToP2SH_P2WSHTest()
        {
            RedeemScript scr = new RedeemScript();
            MockSerializableScript mockScr = new MockSerializableScript(new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2SH_P2WSH(mockScr);
            byte[] expected = Helper.HexToBytes("0020039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81");

            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_P2WSH_ExceptionTest()
        {
            RedeemScript scr = new RedeemScript();
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WSH(null));
        }
    }
}
