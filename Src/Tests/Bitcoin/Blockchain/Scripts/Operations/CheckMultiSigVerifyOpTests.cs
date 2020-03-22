// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CheckMultiSigVerifyOpTests
    {
        private static readonly Signature sig1 = new Signature(1, 2) { SigHash = SigHashType.All };
        private static readonly Signature sig2 = new Signature(10, 20) { SigHash = SigHashType.All };

        private static readonly byte[] pub2Bytes = Helper.HexToBytes("0377af7bee92f893844ba467e3b312efd034fccc001dbbe40e13035973ef5e0094");
        private static readonly byte[] pub3Bytes = Helper.HexToBytes("02dfa5c61b6b5c7f57a09d312b9c724d130148607159d577cfb3e728968388c331");
        private PublicKey GetPub2()
        {
            PublicKey.TryRead(pub2Bytes, out PublicKey res);
            return res;
        }
        private PublicKey GetPub3()
        {
            PublicKey.TryRead(pub3Bytes, out PublicKey res);
            return res;
        }


        [Fact]
        public void Run_CorrectSigsTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop)
            {
                _itemCount = 8,
                popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num3 },
                popIndexData = new Dictionary<int, byte[]> { { 3, OpTestCaseHelper.num2 } },
                popCountData = new byte[][][]
                {
                    new byte[][]
                    {
                        Helper.GetPubkeySampleBytes(true), pub2Bytes, pub3Bytes
                    },
                    new byte[][]
                    {
                        sig1.ToByteArray(), sig2.ToByteArray(),
                    }
                },
                expectedSigs = new Signature[] { sig1, sig2 },
                expectedPubkeys = new PublicKey[] { Helper.GetPubkeySample(), GetPub2(), GetPub3() },
                expectedMultiSigGarbage = OpTestCaseHelper.b7,
                sigVerificationSuccess = true,
            };

            OpTestCaseHelper.RunTest<CheckMultiSigVerifyOp>(data, OP.CheckMultiSigVerify);
        }
    }
}
