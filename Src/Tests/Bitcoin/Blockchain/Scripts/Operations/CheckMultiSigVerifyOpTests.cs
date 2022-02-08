// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CheckMultiSigVerifyOpTests
    {
        [Fact]
        public void Run_CorrectSigsTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop)
            {
                _itemCount = 8,
                _opCountToReturn = 0,
                popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num3 },
                popIndexData = new Dictionary<int, byte[]> { { 3, OpTestCaseHelper.num2 } },
                popCountData = new byte[][][]
                {
                    new byte[][]
                    {
                        KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes
                    },
                    new byte[][]
                    {
                        Helper.ShortSig1Bytes, Helper.ShortSig2Bytes
                    }
                },
                expectedSigs = new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                expectedPubkeys = new byte[][] { KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes },
                expectedMultiSigGarbage = OpTestCaseHelper.b7,
                sigVerificationSuccess = true,
            };

            OpTestCaseHelper.RunTest<CheckMultiSigVerifyOp>(data, OP.CheckMultiSigVerify);
        }

        [Fact]
        public void Run_WrongSigsTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop)
            {
                _itemCount = 8,
                _opCountToReturn = 0,
                popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num3 },
                popIndexData = new Dictionary<int, byte[]> { { 3, OpTestCaseHelper.num2 } },
                popCountData = new byte[][][]
                {
                    new byte[][]
                    {
                        KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes
                    },
                    new byte[][]
                    {
                        Helper.ShortSig1Bytes, Helper.ShortSig2Bytes
                    }
                },
                expectedSigs = new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                expectedPubkeys = new byte[][] { KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes },
                expectedMultiSigGarbage = OpTestCaseHelper.b7,
                sigVerificationSuccess = false,
            };

            OpTestCaseHelper.RunFailTest<CheckMultiSigVerifyOp>(data, Errors.FailedSignatureVerification);
        }

        [Fact]
        public void Run_ErrorTest()
        {
            MockOpData data = new()
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<CheckMultiSigVerifyOp>(data, Errors.NotEnoughStackItems);
        }
    }
}
