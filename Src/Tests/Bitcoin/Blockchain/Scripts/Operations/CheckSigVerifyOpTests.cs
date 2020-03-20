// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CheckSigVerifyOpTests
    {
        private static readonly Signature sig = new Signature(1, 2) { SigHash = SigHashType.All };

        [Fact]
        public void Run_CorrectSigTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopCount)
            {
                _itemCount = 2,
                expectedSig = sig,
                expectedPubkey = Helper.GetPubkeySample(),
                sigVerificationSuccess = true,
                popCountData = new byte[][][] { new byte[][] { sig.ToByteArray(), Helper.GetPubkeySampleBytes(true) } },
            };

            OpTestCaseHelper.RunTest<CheckSigVerifyOp>(data, OP.CheckSigVerify);
        }

        [Fact]
        public void Run_WrongSigTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopCount)
            {
                _itemCount = 2,
                expectedSig = sig,
                expectedPubkey = Helper.GetPubkeySample(),
                sigVerificationSuccess = false,
                popCountData = new byte[][][] { new byte[][] { sig.ToByteArray(), Helper.GetPubkeySampleBytes(true) } },
            };

            OpTestCaseHelper.RunFailTest<CheckSigVerifyOp>(data, "Invalid signature.");
        }

        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[] { null, 1, null, Err.OpNotEnoughItems };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                2,
                new byte[][][] { new byte[][] { new byte[] { 1, 2, 3 }, new byte[0] } },
                "Invalid DER encoding length."
            };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                2,
                new byte[][][] { new byte[][] { sig.ToByteArray(), new byte[] { 1, 2, 3 } } },
                "Invalid public key format."
            };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_ErrorTest(FuncCallName[] expFuncCalls, int count, byte[][][] expPopData, string expErr)
        {
            MockOpData data = new MockOpData(expFuncCalls)
            {
                _itemCount = count,
                popCountData = expPopData
            };

            OpTestCaseHelper.RunFailTest<CheckSigVerifyOp>(data, expErr);
        }
    }
}
