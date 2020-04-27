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
    public class CheckSigOpTests
    {
        [Fact]
        public void Run_CorrectSigTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopCount, FuncCallName.Push)
            {
                _itemCount = 2,
                expectedSig = Helper.ShortSig1,
                expectedPubkey = KeyHelper.Pub1,
                expectedSigBa = Helper.ShortSig1Bytes,
                sigVerificationSuccess = true,
                popCountData = new byte[][][] { new byte[][] { Helper.ShortSig1Bytes, KeyHelper.Pub1CompBytes } },
                pushData = new byte[][] { OpTestCaseHelper.TrueBytes }
            };

            OpTestCaseHelper.RunTest<CheckSigOp>(data, OP.CheckSig);
        }

        [Fact]
        public void Run_WrongSigTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopCount, FuncCallName.Push)
            {
                _itemCount = 2,
                expectedSig = Helper.ShortSig1,
                expectedPubkey = KeyHelper.Pub1,
                expectedSigBa = Helper.ShortSig1Bytes,
                sigVerificationSuccess = false,
                popCountData = new byte[][][] { new byte[][] { Helper.ShortSig1Bytes, KeyHelper.Pub1CompBytes } },
                pushData = new byte[][] { OpTestCaseHelper.FalseBytes }
            };

            OpTestCaseHelper.RunTest<CheckSigOp>(data, OP.CheckSig);
        }

        [Fact]
        public void Run_SpecialCase_SigTest()
        {
            // Signature bytes are invalid (empty bytes) but the execution must not fail (pre BIP-66)
            // Also the IOpData.Verify() should not be called since it is pointless.
            // Instead the result (OP_False) should be pushed to the stack
            MockOpData data = new MockOpData(FuncCallName.PopCount, FuncCallName.Push)
            {
                _itemCount = 2,
                IsStrictDerSig = false,
                popCountData = new byte[][][] { new byte[][] { new byte[0], KeyHelper.Pub1CompBytes } },
                pushData = new byte[][] { OpTestCaseHelper.FalseBytes }
            };

            OpTestCaseHelper.RunTest<CheckSigOp>(data, OP.CheckSig);
        }

        [Fact]
        public void Run_SpecialCase_PubkeyTest()
        {
            // Same as above with public key
            MockOpData data = new MockOpData(FuncCallName.PopCount, FuncCallName.Push)
            {
                _itemCount = 2,
                IsStrictDerSig = false,
                popCountData = new byte[][][] { new byte[][] { new byte[0], KeyHelper.Pub1CompBytes } },
                pushData = new byte[][] { OpTestCaseHelper.FalseBytes }
            };

            OpTestCaseHelper.RunTest<CheckSigOp>(data, OP.CheckSig);
        }

        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[] { null, 1, false, null, Err.OpNotEnoughItems };
            yield return new object[] { null, 1, true, null, Err.OpNotEnoughItems };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                2,
                true,
                new byte[][][] { new byte[][] { new byte[] { 1, 2, 3 }, new byte[0] } },
                "Invalid DER encoding length."
            };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_ErrorTest(FuncCallName[] expFuncCalls, int count, bool strict, byte[][][] expPopData, string expErr)
        {
            MockOpData data = new MockOpData(expFuncCalls)
            {
                _itemCount = count,
                popCountData = expPopData,
                IsStrictDerSig = strict
            };

            OpTestCaseHelper.RunFailTest<CheckSigOp>(data, expErr);
        }
    }
}
