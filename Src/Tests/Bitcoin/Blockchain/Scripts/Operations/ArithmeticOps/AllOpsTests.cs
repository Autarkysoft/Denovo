// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.ArithmeticOps
{
    public class AllOpsTests
    {
        [Fact]
        public void ADD1OpTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { OpTestCaseHelper.num1 },
                pushData = new byte[][] { OpTestCaseHelper.num2 },
            };

            OpTestCaseHelper.RunTest<ADD1Op>(data, OP.ADD1);
        }
        [Fact]
        public void ADD1Op_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<ADD1Op>(data, Errors.NotEnoughStackItems);
        }


        [Fact]
        public void SUB1OpTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { OpTestCaseHelper.num1 },
                pushData = new byte[][] { OpTestCaseHelper.num0 },
            };

            OpTestCaseHelper.RunTest<SUB1Op>(data, OP.SUB1);
        }
        [Fact]
        public void SUB1Op_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<SUB1Op>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetNegateCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.numNeg1 };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.numNeg1, OpTestCaseHelper.num1 };
        }
        [Theory]
        [MemberData(nameof(GetNegateCases))]
        public void NEGATEOpTest(byte[] toPop, byte[] toPush)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { toPop },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<NEGATEOp>(data, OP.NEGATE);
        }
        [Fact]
        public void NEGATEOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<NEGATEOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetAbsCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.numNeg1, OpTestCaseHelper.num1 };
        }
        [Theory]
        [MemberData(nameof(GetAbsCases))]
        public void ABSOpTest(byte[] toPop, byte[] toPush)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { toPop },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<ABSOp>(data, OP.ABS);
        }
        [Fact]
        public void ABSOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<ABSOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetNotCases()
        {
            yield return new object[] { OpTestCaseHelper.num0, true };
            yield return new object[] { OpTestCaseHelper.num1, false };
            yield return new object[] { OpTestCaseHelper.numNeg1, false };
            yield return new object[] { OpTestCaseHelper.maxInt, false };
        }
        [Theory]
        [MemberData(nameof(GetNotCases))]
        public void NOTOpTest(byte[] toPop, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 1,
                pushBool = expBool,
                popData = new byte[][] { toPop },
            };

            OpTestCaseHelper.RunTest<NOTOp>(data, OP.NOT);
        }
        [Fact]
        public void NOTOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<NOTOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetNotEq0Cases()
        {
            yield return new object[] { OpTestCaseHelper.num0, false };
            yield return new object[] { OpTestCaseHelper.num1, true };
            yield return new object[] { OpTestCaseHelper.numNeg1, true };
            yield return new object[] { OpTestCaseHelper.num2, true };
        }
        [Theory]
        [MemberData(nameof(GetNotEq0Cases))]
        public void NotEqual0OpTest(byte[] toPop, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 1,
                popData = new byte[][] { toPop },
                pushBool = expBool
            };

            OpTestCaseHelper.RunTest<NotEqual0Op>(data, OP.NotEqual0);
        }
        [Fact]
        public void NotEqual0Op_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<NotEqual0Op>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetAddCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.num3 };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.numNeg1, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.maxInt, OpTestCaseHelper.num1, OpTestCaseHelper.maxIntPlus1 };
            yield return new object[] { OpTestCaseHelper.maxInt, OpTestCaseHelper.num2, OpTestCaseHelper.maxIntPlus2 };
            yield return new object[]
            {
                OpTestCaseHelper.maxNegInt,
                OpTestCaseHelper.maxNegInt,
                new byte[] { 254, 255, 255, 255, 128 }
            };
        }
        [Theory]
        [MemberData(nameof(GetAddCases))]
        public void AddOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush }
            };

            OpTestCaseHelper.RunTest<AddOp>(data, OP.ADD);
        }
        [Fact]
        public void AddOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<AddOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetSubCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.numNeg1 };
            yield return new object[] { OpTestCaseHelper.num5, OpTestCaseHelper.num2, OpTestCaseHelper.num3 };
            yield return new object[] { OpTestCaseHelper.maxNegInt, OpTestCaseHelper.num1, new byte[] { 0, 0, 0, 128, 128 } };
            yield return new object[]
            {
                OpTestCaseHelper.maxNegInt,
                OpTestCaseHelper.maxInt,
                new byte[] { 254, 255, 255, 255, 128 }
            };
        }
        [Theory]
        [MemberData(nameof(GetSubCases))]
        public void SUBOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush }
            };

            OpTestCaseHelper.RunTest<SUBOp>(data, OP.SUB);
        }
        [Fact]
        public void SUBOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<SUBOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetBoolAndCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, true };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.numNeg1, true };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num4, false };
            yield return new object[] { OpTestCaseHelper.maxInt, OpTestCaseHelper.num0, false };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, false };
        }
        [Theory]
        [MemberData(nameof(GetBoolAndCases))]
        public void BoolAndOpTest(byte[] toPop1, byte[] toPop2, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushBool = expBool
            };

            OpTestCaseHelper.RunTest<BoolAndOp>(data, OP.BoolAnd);
        }
        [Fact]
        public void BoolAndOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<BoolAndOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetBoolOrCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, true };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.numNeg1, true };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num4, true };
            yield return new object[] { OpTestCaseHelper.maxInt, OpTestCaseHelper.num0, true };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, false };
        }
        [Theory]
        [MemberData(nameof(GetBoolOrCases))]
        public void BoolOrOpTest(byte[] toPop1, byte[] toPop2, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushBool = expBool
            };

            OpTestCaseHelper.RunTest<BoolOrOp>(data, OP.BoolOr);
        }
        [Fact]
        public void BoolOrOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<BoolOrOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetNumEqualCases()
        {
            yield return new object[] { OpTestCaseHelper.num2, OpTestCaseHelper.num2, true };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.numNeg1, false };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, true };
            yield return new object[] { OpTestCaseHelper.numNeg1, OpTestCaseHelper.numNeg1, true };
        }
        [Theory]
        [MemberData(nameof(GetNumEqualCases))]
        public void NumEqualOpTest(byte[] toPop1, byte[] toPop2, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushBool = expBool
            };

            OpTestCaseHelper.RunTest<NumEqualOp>(data, OP.NumEqual);
        }
        [Fact]
        public void NumEqualOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<NumEqualOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetNumEqualVerifyCases()
        {
            yield return new object[] { OpTestCaseHelper.num2, OpTestCaseHelper.num2, true };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, true };
            yield return new object[] { OpTestCaseHelper.numNeg1, OpTestCaseHelper.num0, false };
        }
        [Theory]
        [MemberData(nameof(GetNumEqualVerifyCases))]
        public void NumEqualVerifyOpTest(byte[] toPop1, byte[] toPop2, bool pass)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
            };

            if (pass)
            {
                OpTestCaseHelper.RunTest<NumEqualVerifyOp>(data, OP.NumEqualVerify);
            }
            else
            {
                OpTestCaseHelper.RunFailTest<NumEqualVerifyOp>(data, Errors.UnequalStackNumbers);
            }
        }
        [Fact]
        public void NumEqualVerifyOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<NumEqualVerifyOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetNumNotEqualCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, true };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, false };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, false };
        }
        [Theory]
        [MemberData(nameof(GetNumNotEqualCases))]
        public void NumNotEqualOpTest(byte[] toPop1, byte[] toPop2, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushBool = expBool,
            };

            OpTestCaseHelper.RunTest<NumNotEqualOp>(data, OP.NumNotEqual);
        }
        [Fact]
        public void NumNotEqualOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<NumNotEqualOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetLessThanCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, true };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, false };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, false };
        }
        [Theory]
        [MemberData(nameof(GetLessThanCases))]
        public void LessThanOpTest(byte[] toPop1, byte[] toPop2, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushBool = expBool,
            };

            OpTestCaseHelper.RunTest<LessThanOp>(data, OP.LessThan);
        }
        [Fact]
        public void LessThanOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<LessThanOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetGreaterThanCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, false };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, false };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, true };
        }
        [Theory]
        [MemberData(nameof(GetGreaterThanCases))]
        public void GreaterThanOpTest(byte[] toPop1, byte[] toPop2, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushBool = expBool,
            };

            OpTestCaseHelper.RunTest<GreaterThanOp>(data, OP.GreaterThan);
        }
        [Fact]
        public void GreaterThanOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<GreaterThanOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetLessThanOrEqualCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, true };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, true };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, false };
        }
        [Theory]
        [MemberData(nameof(GetLessThanOrEqualCases))]
        public void LessThanOrEqualOpTest(byte[] toPop1, byte[] toPop2, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushBool = expBool,
            };

            OpTestCaseHelper.RunTest<LessThanOrEqualOp>(data, OP.LessThanOrEqual);
        }
        [Fact]
        public void LessThanOrEqualOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<LessThanOrEqualOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetGreaterThanOrEqualCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, false };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, true };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, true };
        }
        [Theory]
        [MemberData(nameof(GetGreaterThanOrEqualCases))]
        public void GreaterThanOrEqualOpTest(byte[] toPop1, byte[] toPop2, bool expBool)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushBool = expBool
            };

            OpTestCaseHelper.RunTest<GreaterThanOrEqualOp>(data, OP.GreaterThanOrEqual);
        }
        [Fact]
        public void GreaterThanOrEqualOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<GreaterThanOrEqualOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetMinCases()
        {
            yield return new object[] { OpTestCaseHelper.num2, OpTestCaseHelper.num3, OpTestCaseHelper.num2 };
            yield return new object[] { OpTestCaseHelper.num2, OpTestCaseHelper.num2, OpTestCaseHelper.num2 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, OpTestCaseHelper.num2 };
        }
        [Theory]
        [MemberData(nameof(GetMinCases))]
        public void MINOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<MINOp>(data, OP.MIN);
        }
        [Fact]
        public void MINOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<MINOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetMaxCases()
        {
            yield return new object[] { OpTestCaseHelper.num2, OpTestCaseHelper.num3, OpTestCaseHelper.num3 };
            yield return new object[] { OpTestCaseHelper.num2, OpTestCaseHelper.num2, OpTestCaseHelper.num2 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, OpTestCaseHelper.num3 };
        }
        [Theory]
        [MemberData(nameof(GetMaxCases))]
        public void MAXOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<MAXOp>(data, OP.MAX);
        }
        [Fact]
        public void MAXOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<MAXOp>(data, Errors.NotEnoughStackItems);
        }


        public static IEnumerable<object[]> GetWITHINCases()
        {
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num0, OpTestCaseHelper.num3,
                true
            };
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num2, OpTestCaseHelper.num3,
                true
            };
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num3, OpTestCaseHelper.num3,
                false
            };
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num0, OpTestCaseHelper.num2,
                false
            };
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num3, OpTestCaseHelper.num1,
                false
            };
        }
        [Theory]
        [MemberData(nameof(GetWITHINCases))]
        public void WITHINOpTest(byte[] toPop1, byte[] toPop2, byte[] toPop3, bool expBool)
        {
            MockOpData data = new(FuncCallName.PopCount, FuncCallName.PushBool)
            {
                _itemCount = 3,
                popCountData = new byte[][][] { new byte[][] { toPop1, toPop2, toPop3 } },
                pushBool = expBool
            };

            OpTestCaseHelper.RunTest<WITHINOp>(data, OP.WITHIN);
        }

        public static IEnumerable<object[]> GetWithinFailCases()
        {
            yield return new object[] { null, 2, null, null, Errors.NotEnoughStackItems };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                3,
                new byte[][][] { new byte[][] { null, null, new byte[5] } },
                null,
                Errors.InvalidStackNumberFormat
            };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                3,
                new byte[][][] { new byte[][] { null, new byte[5], OpTestCaseHelper.num0 } },
                null,
                Errors.InvalidStackNumberFormat
            };
            yield return new object[]
            {
                new FuncCallName[] { FuncCallName.PopCount },
                3,
                new byte[][][] { new byte[][] { new byte[5], OpTestCaseHelper.num0, OpTestCaseHelper.num0 } },
                null,
                Errors.InvalidStackNumberFormat
            };
        }
        [Theory]
        [MemberData(nameof(GetWithinFailCases))]
        public void WITHINOp_FailTest(FuncCallName[] calls, int count, byte[][][] pop, byte[][] push, Errors expErr)
        {
            MockOpData data = new(calls)
            {
                _itemCount = count,
                popCountData = pop,
                pushData = push,
            };

            OpTestCaseHelper.RunFailTest<WITHINOp>(data, expErr);
        }
    }
}
