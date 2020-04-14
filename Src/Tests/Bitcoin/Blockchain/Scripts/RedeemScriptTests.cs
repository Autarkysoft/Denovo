// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

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
        [Fact]
        public void ConstructorTest()
        {
            RedeemScript scr = new RedeemScript();
            Assert.Empty(scr.Data);
        }

        public static IEnumerable<object[]> GetScrTypeCases()
        {
            yield return new object[] { new RedeemScript(), RedeemScriptType.Empty };
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
        }
        [Theory]
        [MemberData(nameof(GetScrTypeCases))]
        public void GetRedeemScriptTypeTest(IRedeemScript scr, RedeemScriptType expected)
        {
            RedeemScriptType actual = scr.GetRedeemScriptType();
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
