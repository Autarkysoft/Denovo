// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts
{
    public class SignatureScriptTests
    {
        // 3 randomly generated keys
        private static readonly byte[] pubBaC1 = Helper.HexToBytes("02a17d82262d4ab8d9499d664c637c075a3ddc1bf2bb0b392188ba9e1043514ffd");
        private static readonly byte[] pubBaUC1 = Helper.HexToBytes("04a17d82262d4ab8d9499d664c637c075a3ddc1bf2bb0b392188ba9e1043514ffd2ec1051619dec03da6be55608dff5a2a800907e8358b3b76ea86f90f22cd2fc6");
        private static readonly byte[] pubBaC2 = Helper.HexToBytes("02a63ea6d772bc7127a67be7fc4310164737430de14003c1d7c3f8d5a190f3dfd0");
        private static readonly byte[] pubBaUC2 = Helper.HexToBytes("04a63ea6d772bc7127a67be7fc4310164737430de14003c1d7c3f8d5a190f3dfd033fbf623236b512c709008c11307453980a926fd361968c7e039375d9662f228");
        private static readonly byte[] pubBaC3 = Helper.HexToBytes("02dba58d37939585677eb9c994db2ceb05d34b5b38107536b0bea1ec2612506f70");
        private static readonly byte[] pubBaUC3 = Helper.HexToBytes("04dba58d37939585677eb9c994db2ceb05d34b5b38107536b0bea1ec2612506f70ab944593bd1d176feaeebf0e0b61b81cf3bac61a6185b14ec0c14e1cb193f6a2");

        private static readonly Signature sig1 = new Signature(10, 20) { SigHash = SigHashType.All };
        private static readonly byte[] sigBa1 = Helper.HexToBytes("300602010a02011401");
        private static readonly Signature sig2 = new Signature(11, 22) { SigHash = SigHashType.None };
        private static readonly byte[] sigBa2 = Helper.HexToBytes("300602010b02011502");

        private static readonly byte[] validPub1 = Helper.HexToBytes("0445b32ffefdfca43d53e94a00287c0846b8ce96231a89e7650af733ddf128f61f847e448746270aa54f26fb44b11e23ab849e62fbf1ef6fe73c595efb2dc22159");
        private readonly Signature validSig1 = new Signature(
            BigInteger.Parse("50363417002699488739814118762657286243503175042585195569521935286457431087776"),
            BigInteger.Parse("25316327395179500445610111613343584350834534488159293096599901175863901426567"))
        { SigHash = SigHashType.All };
        private readonly byte[] validSigBa1 = Helper.HexToBytes("304402206f58af1129157ac19e9e546563b0d352101b879b0beec55f93e342e3a98ddaa0022037f88894dec2fb483e042f483f3e401a6f269f90fe790386cb71db8ff3fc138701");


        [Fact]
        public void ConstructorTest()
        {
            SignatureScript scr = new SignatureScript();

            Assert.Empty(scr.OperationList);
            Assert.False(scr.IsWitness);
            Assert.Equal(ScriptType.ScriptSig, scr.ScriptType);
        }


        [Fact]
        public void SetToEmptyTest()
        {
            SignatureScript scr = new SignatureScript() { OperationList = new IOperation[] { new DUPOp() } };
            Assert.NotEmpty(scr.OperationList);
            scr.SetToEmpty();
            Assert.Empty(scr.OperationList);
        }

        [Fact]
        public void SetToP2PKTest()
        {
            SignatureScript scr = new SignatureScript();
            scr.SetToP2PK(sig1);
            IOperation[] expected = new IOperation[] { new PushDataOp(sigBa1) };

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2PKTest_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PK(null));
        }


        public static IEnumerable<object[]> GetP2PKHCases()
        {
            yield return new object[] { true, new IOperation[] { new PushDataOp(sigBa1), new PushDataOp(pubBaC1) } };
            yield return new object[] { false, new IOperation[] { new PushDataOp(sigBa1), new PushDataOp(pubBaUC1) } };
        }
        [Theory]
        [MemberData(nameof(GetP2PKHCases))]
        public void SetToP2PKHTest(bool useComp, IOperation[] expected)
        {
            SignatureScript scr = new SignatureScript();
            PublicKey.TryRead(pubBaUC1, out PublicKey pub);
            scr.SetToP2PKH(sig1, pub, useComp);

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2PKH_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();
            PublicKey.TryRead(pubBaUC1, out PublicKey pub);

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(null, pub, true));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(sig1, null, true));
        }

        public static IEnumerable<object[]> GetMultiSigCases()
        {
            PublicKey.TryRead(pubBaUC1, out PublicKey pub1);
            PublicKey.TryRead(pubBaUC2, out PublicKey pub2);
            PublicKey.TryRead(pubBaUC3, out PublicKey pub3);

            RedeemScript rdm0of1 = new RedeemScript()
            {
                OperationList = new IOperation[]
                {
                    new PushDataOp(0), new PushDataOp(pubBaUC1), new PushDataOp(1), new CheckMultiSigOp()
                }
            };
            RedeemScript rdm1of1_comp = new RedeemScript();
            rdm1of1_comp.SetToMultiSig(1, new PublicKey[] { pub1 });
            RedeemScript rdm1of1_uncomp = new RedeemScript();
            rdm1of1_uncomp.SetToMultiSig(1, new PublicKey[] { pub1 }, false);
            RedeemScript rdm2of2_samepub = new RedeemScript();
            rdm2of2_samepub.SetToMultiSig(2, new PublicKey[] { pub1, pub1 });
            RedeemScript rdm2of2_samepubCompUncomp = new RedeemScript()
            {
                OperationList = new IOperation[]
                {
                    new PushDataOp(2), new PushDataOp(pubBaUC1), new PushDataOp(pubBaC1), new PushDataOp(2), new CheckMultiSigOp()
                }
            };
            RedeemScript rdm1of2_samepub = new RedeemScript();
            rdm1of2_samepub.SetToMultiSig(1, new PublicKey[] { pub1, pub1 });
            RedeemScript rdm2of3 = new RedeemScript();
            rdm2of3.SetToMultiSig(2, new PublicKey[] { pub1, pub2, pub3 });
            RedeemScript rdm2of3_samepub = new RedeemScript();
            rdm2of3_samepub.SetToMultiSig(2, new PublicKey[] { pub1, pub2, pub1 });
            RedeemScript rdm1of3_samepub = new RedeemScript();
            rdm1of3_samepub.SetToMultiSig(1, new PublicKey[] { pub1, pub2, pub1 });

            // Initial empty signature scripts:
            yield return new object[]
            {
                new SignatureScript(), null, null, rdm0of1,
                new IOperation[] { new PushDataOp(OP._0), new PushDataOp(rdm0of1) }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub1, rdm1of1_comp,
                new IOperation[] { new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(rdm1of1_comp) }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub1, rdm1of1_uncomp,
                new IOperation[] { new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(rdm1of1_uncomp) }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub1, rdm2of2_samepub,
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(sigBa1), new PushDataOp(rdm2of2_samepub)
                }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub1, rdm2of2_samepubCompUncomp,
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(sigBa1), new PushDataOp(rdm2of2_samepubCompUncomp)
                }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub1, rdm1of2_samepub,
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(rdm1of2_samepub)
                }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub1, rdm2of3,
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(rdm2of3)
                }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub2, rdm2of3,
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(rdm2of3)
                }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub3, rdm2of3,
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(rdm2of3)
                }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub1, rdm2of3_samepub,
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(sigBa1), new PushDataOp(rdm2of3_samepub)
                }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub2, rdm2of3_samepub, // pub1 is duplicate not pub2
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(rdm2of3_samepub)
                }
            };
            yield return new object[]
            {
                new SignatureScript(), sig1, pub1, rdm1of3_samepub,
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(sigBa1), new PushDataOp(rdm1of3_samepub)
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetMultiSigCases))]
        public void SetToMultiSigTest(SignatureScript scr, Signature sig, PublicKey pub, IRedeemScript redeem, IOperation[] expected)
        {
            scr.SetToMultiSig(sig, pub, redeem, null, null, -1);
            Assert.Equal(expected, scr.OperationList);
        }
    }
}
