// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CheckSigVerifyOpTests
    {
        [Fact]
        public void Run_CorrectSigTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopCount)
            {
                _itemCount = 2,
                expectedSig = Helper.ShortSig1,
                expectedPubkey = KeyHelper.Pub1,
                expectedSigBa = Helper.ShortSig1Bytes,
                sigVerificationSuccess = true,
                popCountData = new byte[][][] { new byte[][] { Helper.ShortSig1Bytes, KeyHelper.Pub1CompBytes } },
            };

            OpTestCaseHelper.RunTest<CheckSigVerifyOp>(data, OP.CheckSigVerify);
        }

        [Fact]
        public void Run_WrongSigTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopCount)
            {
                _itemCount = 2,
                expectedSig = Helper.ShortSig1,
                expectedPubkey = KeyHelper.Pub1,
                expectedSigBa = Helper.ShortSig1Bytes,
                sigVerificationSuccess = false,
                popCountData = new byte[][][] { new byte[][] { Helper.ShortSig1Bytes, KeyHelper.Pub1CompBytes } },
            };

            OpTestCaseHelper.RunFailTest<CheckSigVerifyOp>(data, "Signature verification failed.");
        }

        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[] { null, 1, false, null, Err.OpNotEnoughItems };
            yield return new object[] { null, 1, true, null, Err.OpNotEnoughItems };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                2,
                false,
                new byte[][][] { new byte[][] { new byte[] { 1, 2, 3 }, new byte[0] } },
                "Signature verification failed."
            };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                2,
                true,
                new byte[][][] { new byte[][] { new byte[] { 1, 2, 3 }, new byte[0] } },
                "Invalid DER encoding length."
            };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                2,
                false,
                new byte[][][] { new byte[][] { Helper.ShortSig1Bytes, new byte[0] } },
                "Signature verification failed."
            };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                2,
                true,
                new byte[][][] { new byte[][] { Helper.ShortSig1Bytes, new byte[0] } },
                "Signature verification failed."
            };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_ErrorTest(FuncCallName[] expFuncCalls, int count, bool strict, byte[][][] expPopData, string expErr)
        {
            // Note that MockOpData makes sure IOpData.Verify() is _not_ called in any of these cases
            MockOpData data = new MockOpData(expFuncCalls)
            {
                _itemCount = count,
                popCountData = expPopData,
                IsStrictDerSig = strict
            };

            OpTestCaseHelper.RunFailTest<CheckSigVerifyOp>(data, expErr);
        }
    }
}
