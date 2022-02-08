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
    public class CheckMultiSigOpTests
    {
        [Fact]
        public void Run_CorrectSigsTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 8,
                _opCountToReturn = 1,
                popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num3 },
                popIndexData = new Dictionary<int, byte[]> { { 3, OpTestCaseHelper.num2 } },
                popCountData = new byte[][][]
                {
                    new byte[][]
                    {
                        KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes,
                    },
                    new byte[][]
                    {
                        Helper.ShortSig1Bytes, Helper.ShortSig2Bytes,
                    }
                },
                expectedSigs = new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                expectedPubkeys = new byte[][] { KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes },
                expectedMultiSigGarbage = OpTestCaseHelper.b7,
                sigVerificationSuccess = true,
                pushBool = true
            };

            OpTestCaseHelper.RunTest<CheckMultiSigOp>(data, OP.CheckMultiSig);
            Assert.Equal(4, data.OpCount);
        }

        [Fact]
        public void Run_WrongSigsTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 8,
                _opCountToReturn = 5,
                popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num3 },
                popIndexData = new Dictionary<int, byte[]> { { 3, OpTestCaseHelper.num2 } },
                popCountData = new byte[][][]
                {
                    new byte[][]
                    {
                        KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes,
                    },
                    new byte[][]
                    {
                        Helper.ShortSig1Bytes, Helper.ShortSig2Bytes,
                    }
                },
                expectedSigs = new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                expectedPubkeys = new byte[][] { KeyHelper.Pub1CompBytes, KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes },
                expectedMultiSigGarbage = OpTestCaseHelper.b7,
                sigVerificationSuccess = false,
                pushBool = false
            };

            OpTestCaseHelper.RunTest<CheckMultiSigOp>(data, OP.CheckMultiSig);
            Assert.Equal(8, data.OpCount);
        }

        public static IEnumerable<object[]> GetSpecialCase()
        {
            yield return new object[]
            {
                true,
                new IOperation[]
                {
                    new PushDataOp(OP._0), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(OP._0), // n
                    new CheckMultiSigOp(),
                },
                true,
                Errors.None,
                0
            };
            yield return new object[]
            {
                false,
                new IOperation[]
                {
                    new PushDataOp(OP._0), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(OP._0), // n
                    new CheckMultiSigOp(),
                },
                true,
                Errors.None,
                0
            };
            yield return new object[]
            {
                false,
                new IOperation[]
                {
                    new PushDataOp(OP._2), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(OP._0), // n
                    new CheckMultiSigOp(),
                },
                true,
                Errors.None,
                0
            };
            yield return new object[]
            {
                true,
                new IOperation[]
                {
                    new PushDataOp(OP._2), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(OP._0), // n
                    new CheckMultiSigOp(),
                },
                false,
                Errors.InvalidMultiSigDummy,
                0
            };
            yield return new object[]
            {
                true,
                new IOperation[]
                {
                    new PushDataOp(OP._0), // garbage
                    new PushDataOp(OP._0), // m
                    new PushDataOp(new byte[]{1,2,3}),
                    new PushDataOp(OP._1), // n
                    new CheckMultiSigOp(),
                },
                true,
                Errors.None,
                1
            };
        }
        [Theory]
        [MemberData(nameof(GetSpecialCase))]
        public void Run_SpecialCaseTest(bool isStrict, IOperation[] operations, bool expBool, Errors expError, int expOpCount)
        {
            // 0of0 multisig => OP_0 [] OP_0 [] OP_0
            OpData data = new()
            {
                IsBip147Enabled = isStrict
            };

            // Run all PushOps (all items in array except last)
            for (int i = 0; i < operations.Length - 1; i++)
            {
                bool b1 = operations[i].Run(data, out Errors error1);
                Assert.True(b1, error1.Convert());
                Assert.Equal(Errors.None, error1);
            }

            // Run the OP_CheckMultiSig operation
            bool b2 = operations[^1].Run(data, out Errors error2);
            Assert.Equal(expBool, b2);
            Assert.Equal(expError, error2);
            Assert.Equal(expOpCount, data.OpCount);
        }


        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[]
            {
                new MockOpData()
                {
                    _itemCount = 2
                },
                Errors.NotEnoughStackItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    _opCountToReturn = Constants.MaxScriptOpCount - 2,
                    popData = new byte[1][] { OpTestCaseHelper.num3 }
                },
                Errors.OpCountOverflow
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { new byte[1] { 21 } }
                },
                Errors.InvalidMultiSigPubkeyCount
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.numNeg1 }
                },
                Errors.InvalidMultiSigPubkeyCount
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num2 }
                },
                Errors.NotEnoughStackItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { new byte[5] { 1, 2, 3, 4, 5 } },
                },
                Errors.InvalidStackNumberFormat
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, new byte[5] { 1, 2, 3, 4, 5 } } },
                },
                Errors.InvalidStackNumberFormat
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, new byte[1] { 21 } } },
                },
                Errors.InvalidMultiSigSignatureCount
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.num2 } }, // m > n
                },
                Errors.InvalidMultiSigSignatureCount
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.numNeg1 } },
                },
                Errors.InvalidMultiSigSignatureCount
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 4,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                },
                Errors.NotEnoughStackItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 4,
                    _opCountToReturn = 0,
                    popData = new byte[1][] { OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                },
                Errors.NotEnoughStackItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                               FuncCallName.PopCount, FuncCallName.PopCount,
                               FuncCallName.Pop)
                {
                    _itemCount = 7,
                    _opCountToReturn = 0,
                    popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                    popCountData = new byte[][][]
                    {
                        new byte[][]
                        {
                            KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes
                        },
                        new byte[][]
                        {
                            Helper.ShortSig1Bytes, Helper.ShortSig2Bytes
                        },
                    },
                    expectedPubkeys = new byte[][] { KeyHelper.Pub2CompBytes, KeyHelper.Pub3CompBytes},
                    expectedSigs = new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                    expectedMultiSigGarbage = OpTestCaseHelper.b7,
                    garbageCheckResult = false
                },
                Errors.InvalidMultiSigDummy
            };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_ErrorTest(MockOpData data, Errors expError)
        {
            OpTestCaseHelper.RunFailTest<CheckMultiSigOp>(data, expError);
        }
    }
}
