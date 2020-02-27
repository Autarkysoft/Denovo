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
        // 2 randomly generated public keys:
        private static readonly byte[] pubBaC1 = Helper.HexToBytes("02a17d82262d4ab8d9499d664c637c075a3ddc1bf2bb0b392188ba9e1043514ffd");
        private static readonly byte[] pubBaUC1 = Helper.HexToBytes("04a17d82262d4ab8d9499d664c637c075a3ddc1bf2bb0b392188ba9e1043514ffd2ec1051619dec03da6be55608dff5a2a800907e8358b3b76ea86f90f22cd2fc6");
        private static readonly byte[] pubBaC1_hash = Helper.HexToBytes("03814c6125f6ac2ebfc42d74339af43dc7530313");
        private static readonly byte[] pubBaUC1_hash = Helper.HexToBytes("7f8b56fd6eeb910db9c0bca69aebada4d3e16d6f");

        private static readonly byte[] pubBaC2 = Helper.HexToBytes("02a63ea6d772bc7127a67be7fc4310164737430de14003c1d7c3f8d5a190f3dfd0");
        private static readonly byte[] pubBaUC2 = Helper.HexToBytes("04a63ea6d772bc7127a67be7fc4310164737430de14003c1d7c3f8d5a190f3dfd033fbf623236b512c709008c11307453980a926fd361968c7e039375d9662f228");

        [Fact]
        public void ConstructorTest()
        {
            RedeemScript scr = new RedeemScript();

            Assert.Empty(scr.OperationList);
            Assert.False(scr.IsWitness);
            Assert.Equal(ScriptType.ScriptRedeem, scr.ScriptType);
        }

        public static IEnumerable<object[]> GetScrTypeCases()
        {
            yield return new object[] { new RedeemScript(), RedeemScriptType.Empty };
            yield return new object[]
            {
                new RedeemScript() { OperationList = new IOperation[] { new DUPOp() } },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript() { OperationList = new IOperation[] { new PushDataOp(OP._0), new PushDataOp(new byte[20]) } },
                RedeemScriptType.P2SH_P2WPKH
            };
            yield return new object[]
            {
                new RedeemScript() { OperationList = new IOperation[] { new PushDataOp(OP._1), new PushDataOp(new byte[20]) } },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript() { OperationList = new IOperation[] { new PushDataOp(OP._0), new PushDataOp(new byte[21]) } },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript() { OperationList = new IOperation[] { new PushDataOp(OP._0), new PushDataOp(OP._1) } },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript()
                {
                    OperationList = new IOperation[] { new PushDataOp(new byte[20]), new PushDataOp(new byte[20]) }
                },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript() { OperationList = new IOperation[] { new PushDataOp(OP._0), new PushDataOp(new byte[32]) } },
                RedeemScriptType.P2SH_P2WSH
            };
            yield return new object[]
            {
                new RedeemScript() { OperationList = new IOperation[] { new PushDataOp(OP._1), new PushDataOp(new byte[32]) } },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript() { OperationList = new IOperation[] { new PushDataOp(OP._0), new PushDataOp(new byte[33]) } },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript()
                {
                    OperationList = new IOperation[]
                    {
                        new PushDataOp(123),
                        new CheckLocktimeVerifyOp(),
                        new DROPOp(),
                        new PushDataOp(new byte[33]),
                        new CheckSigOp()
                    }
                },
                RedeemScriptType.CheckLocktimeVerify
            };
            yield return new object[]
            {
                new RedeemScript()
                {
                    OperationList = new IOperation[]
                    {
                        new PushDataOp(123),
                        new CheckLocktimeVerifyOp(),
                        new DROPOp(),
                        new PushDataOp(new byte[65]),
                        new CheckSigOp()
                    }
                },
                RedeemScriptType.CheckLocktimeVerify
            };
            yield return new object[]
            {
                new RedeemScript()
                {
                    OperationList = new IOperation[]
                    {
                        new PushDataOp(new byte[6]),
                        new CheckLocktimeVerifyOp(),
                        new DROPOp(),
                        new PushDataOp(new byte[33]),
                        new CheckSigOp()
                    }
                },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript()
                {
                    OperationList = new IOperation[]
                    {
                        new PushDataOp(123),
                        new CheckLocktimeVerifyOp(),
                        new DROPOp(),
                        new PushDataOp(new byte[34]),
                        new CheckSigOp()
                    }
                },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript()
                {
                    OperationList = new IOperation[]
                    {
                        new PushDataOp(123),
                        new CheckLocktimeVerifyOp(),
                        new DROPOp(),
                        new PushDataOp(OP._1),
                        new CheckSigOp()
                    }
                },
                RedeemScriptType.Unknown
            };
            yield return new object[]
            {
                new RedeemScript()
                {
                    OperationList = new IOperation[]
                    {
                        new CheckMultiSigOp()
                    }
                },
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
            PublicKey.TryRead(pubBaUC1, out PublicKey pub1);
            PublicKey.TryRead(pubBaUC2, out PublicKey pub2);

            yield return new object[]
            {
                1, new PublicKey[] { pub1 }, true,
                new IOperation[] { new PushDataOp(1), new PushDataOp(pubBaC1), new PushDataOp(1), new CheckMultiSigOp() }
            };
            yield return new object[]
            {
                1, new PublicKey[] { pub1 }, false,
                new IOperation[] { new PushDataOp(1), new PushDataOp(pubBaUC1), new PushDataOp(1), new CheckMultiSigOp() }
            };
            yield return new object[]
            {
                1, new PublicKey[] { pub1, pub2 }, true,
                new IOperation[]
                {
                    new PushDataOp(1), new PushDataOp(pubBaC1), new PushDataOp(pubBaC2), new PushDataOp(2), new CheckMultiSigOp()
                }
            };
            yield return new object[]
            {
                1, new PublicKey[] { pub1, pub2 }, false,
                new IOperation[]
                {
                    new PushDataOp(1), new PushDataOp(pubBaUC1), new PushDataOp(pubBaUC2), new PushDataOp(2), new CheckMultiSigOp()
                }
            };
            yield return new object[]
            {
                2, new PublicKey[] { pub1, pub2 }, false,
                new IOperation[]
                {
                    new PushDataOp(2), new PushDataOp(pubBaUC1), new PushDataOp(pubBaUC2), new PushDataOp(2), new CheckMultiSigOp()
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetSetToMultiSigCases))]
        public void SetToMultiSigTest(int m, PublicKey[] pubs, bool comp, IOperation[] expected)
        {
            RedeemScript scr = new RedeemScript();
            scr.SetToMultiSig(m, pubs, comp);

            Assert.Equal(expected, scr.OperationList);
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
            PublicKey.TryRead(pubBaUC1, out PublicKey pub);
            scr.SetToP2SH_P2WPKH(pub, true);

            IOperation[] expected = new IOperation[] { new PushDataOp(OP._0), new PushDataOp(pubBaC1_hash) };

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2SH_P2WPKH_UnCompTest()
        {
            RedeemScript scr = new RedeemScript();
            PublicKey.TryRead(pubBaUC1, out PublicKey pub);
            // This is non-standard
            scr.SetToP2SH_P2WPKH(pub, false);

            IOperation[] expected = new IOperation[] { new PushDataOp(OP._0), new PushDataOp(pubBaUC1_hash) };

            Assert.Equal(expected, scr.OperationList);
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

            IOperation[] expected = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(Helper.HexToBytes("039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81"))
            };

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2SH_P2WSH_ExceptionTest()
        {
            RedeemScript scr = new RedeemScript();
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WSH(null));
        }
    }
}
