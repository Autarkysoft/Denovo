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
    public class CheckMultiSigOpTests
    {
        private static readonly Signature sig1 = new Signature(1, 2) { SigHash = SigHashType.All };
        private static readonly Signature sig2 = new Signature(10, 20) { SigHash = SigHashType.All };

        private static readonly byte[] pub2Bytes = Helper.HexToBytes("0377af7bee92f893844ba467e3b312efd034fccc001dbbe40e13035973ef5e0094");
        private static readonly byte[] pub3Bytes = Helper.HexToBytes("02dfa5c61b6b5c7f57a09d312b9c724d130148607159d577cfb3e728968388c331");
        private static PublicKey GetPub2()
        {
            PublicKey.TryRead(pub2Bytes, out PublicKey res);
            return res;
        }
        private static PublicKey GetPub3()
        {
            PublicKey.TryRead(pub3Bytes, out PublicKey res);
            return res;
        }


        [Fact]
        public void Run_CorrectSigsTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop, FuncCallName.Push)
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
                pushData = new byte[][] { OpTestCaseHelper.TrueBytes }
            };

            OpTestCaseHelper.RunTest<CheckMultiSigOp>(data, OP.CheckMultiSig);
        }

        [Fact]
        public void Run_WrongSigsTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                                             FuncCallName.PopCount, FuncCallName.PopCount,
                                             FuncCallName.Pop, FuncCallName.Push)
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
                sigVerificationSuccess = false,
                pushData = new byte[][] { OpTestCaseHelper.FalseBytes }
            };

            OpTestCaseHelper.RunTest<CheckMultiSigOp>(data, OP.CheckMultiSig);
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
                null
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
                null
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
                null
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
                "The extra item should be OP_0."
            };
        }
        [Theory]
        [MemberData(nameof(GetSpecialCase))]
        public void Run_SpecialCaseTest(bool isStrict, IOperation[] operations, bool expBool, string expError)
        {
            // 0of0 multisig => OP_0 [] OP_0 [] OP_0
            OpData data = new OpData()
            {
                IsStrictMultiSigGarbage = isStrict
            };

            // Run the first 3 PushOps
            for (int i = 0; i < operations.Length - 1; i++)
            {
                bool b1 = operations[i].Run(data, out string error1);
                Assert.True(b1, error1);
                Assert.Null(error1);
            }

            // Run the OP_CheckMultiSig operation
            bool b2 = operations[^1].Run(data, out string error2);
            Assert.Equal(expBool, b2);
            Assert.Equal(expError, error2);
        }


        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[]
            {
                new MockOpData()
                {
                    _itemCount = 2
                },
                Err.OpNotEnoughItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    popData = new byte[1][] { new byte[2] { 1, 2 } }
                },
                "Invalid number (n) format."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    popData = new byte[1][] { new byte[1] { 21 } }
                },
                "Invalid number of public keys in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    popData = new byte[1][] { OpTestCaseHelper.numNeg1 }
                },
                "Invalid number of public keys in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop)
                {
                    _itemCount = 3,
                    popData = new byte[1][] { OpTestCaseHelper.num2 }
                },
                Err.OpNotEnoughItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, new byte[2] { 1, 2 } } },
                },
                "Invalid number (m) format."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, new byte[1] { 21 } } },
                },
                "Invalid number of signatures in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.num2 } }, // m > n
                },
                "Invalid number of signatures in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 3,
                    popData = new byte[1][] { OpTestCaseHelper.num1 },
                    popIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.numNeg1 } },
                },
                "Invalid number of signatures in multi-sig."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 4,
                    popData = new byte[1][] { OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                },
                Err.OpNotEnoughItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex)
                {
                    _itemCount = 4,
                    popData = new byte[1][] { OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                },
                Err.OpNotEnoughItems
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                               FuncCallName.PopCount)
                {
                    _itemCount = 7,
                    popData = new byte[][] { OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                    popCountData = new byte[][][]
                    {
                        new byte[][]
                        {
                            pub2Bytes, new byte[3]
                        },

                    }
                },
                "Invalid public key."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                               FuncCallName.PopCount, FuncCallName.PopCount)
                {
                    _itemCount = 7,
                    popData = new byte[][] { OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                    popCountData = new byte[][][]
                    {
                        new byte[][]
                        {
                            pub2Bytes, pub3Bytes
                        },
                        new byte[][]
                        {
                            sig1.ToByteArray(), new byte[3]
                        },
                    }
                },
                "Invalid signature (Invalid DER encoding length.)."
            };
            yield return new object[]
            {
                new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex,
                               FuncCallName.PopCount, FuncCallName.PopCount,
                               FuncCallName.Pop)
                {
                    _itemCount = 7,
                    popData = new byte[][] { OpTestCaseHelper.b7, OpTestCaseHelper.num2 },
                    popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.num2 } },
                    popCountData = new byte[][][]
                    {
                        new byte[][]
                        {
                            pub2Bytes, pub3Bytes
                        },
                        new byte[][]
                        {
                            sig1.ToByteArray(), sig2.ToByteArray()
                        },
                    },
                    expectedPubkeys = new PublicKey[] { GetPub2(), GetPub3() },
                    expectedSigs = new Signature[] { sig1, sig2 },
                    expectedMultiSigGarbage = OpTestCaseHelper.b7,
                    garbageCheckResult = false
                },
                "The extra item should be OP_0."
            };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_ErrorTest(MockOpData data, string expError)
        {
            OpTestCaseHelper.RunFailTest<CheckMultiSigOp>(data, expError);
        }
    }
}
