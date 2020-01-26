// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

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
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { OpTestCaseHelper.num1 },
                pushData = new byte[][] { OpTestCaseHelper.num2 },
            };

            OpTestCaseHelper.RunTest<ADD1Op>(data, OP.ADD1);
        }


        [Fact]
        public void SUB1OpTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { OpTestCaseHelper.num1 },
                pushData = new byte[][] { OpTestCaseHelper.num0 },
            };

            OpTestCaseHelper.RunTest<SUB1Op>(data, OP.SUB1);
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
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { toPop },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<NEGATEOp>(data, OP.NEGATE);
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
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { toPop },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<ABSOp>(data, OP.ABS);
        }


        public static IEnumerable<object[]> GetNotCases()
        {
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.numNeg1, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.maxInt, OpTestCaseHelper.num0 };
        }
        [Theory]
        [MemberData(nameof(GetNotCases))]
        public void NOTOpTest(byte[] toPop, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { toPop },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<NOTOp>(data, OP.NOT);
        }


        public static IEnumerable<object[]> GetNotEq0Cases()
        {
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.numNeg1, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
        }
        [Theory]
        [MemberData(nameof(GetNotEq0Cases))]
        public void NotEqual0OpTest(byte[] toPop, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { toPop },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<NotEqual0Op>(data, OP.NotEqual0);
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
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush }
            };

            OpTestCaseHelper.RunTest<AddOp>(data, OP.ADD);
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
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush }
            };

            OpTestCaseHelper.RunTest<SUBOp>(data, OP.SUB);
        }


        public static IEnumerable<object[]> GetBoolAndCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.numNeg1, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num4, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.maxInt, OpTestCaseHelper.num0, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, OpTestCaseHelper.num0 };
        }
        [Theory]
        [MemberData(nameof(GetBoolAndCases))]
        public void BoolAndOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush }
            };

            OpTestCaseHelper.RunTest<BoolAndOp>(data, OP.BoolAnd);
        }


        public static IEnumerable<object[]> GetBoolOrCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.numNeg1, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num4, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.maxInt, OpTestCaseHelper.num0, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, OpTestCaseHelper.num0 };
        }
        [Theory]
        [MemberData(nameof(GetBoolOrCases))]
        public void BoolOrOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<BoolOrOp>(data, OP.BoolOr);
        }


        public static IEnumerable<object[]> GetNumEqualCases()
        {
            yield return new object[] { OpTestCaseHelper.num2, OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.numNeg1, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.numNeg1, OpTestCaseHelper.numNeg1, OpTestCaseHelper.num1 };
        }
        [Theory]
        [MemberData(nameof(GetNumEqualCases))]
        public void NumEqualOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<NumEqualOp>(data, OP.NumEqual);
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
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop)
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
                OpTestCaseHelper.RunFailTest<NumEqualVerifyOp>(data, "Numbers are not equal.");
            }
        }


        public static IEnumerable<object[]> GetNumNotEqualCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.num0, OpTestCaseHelper.num0, OpTestCaseHelper.num0 };
        }
        [Theory]
        [MemberData(nameof(GetNumNotEqualCases))]
        public void NumNotEqualOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<NumNotEqualOp>(data, OP.NumNotEqual);
        }


        public static IEnumerable<object[]> GetLessThanCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, OpTestCaseHelper.num0 };
        }
        [Theory]
        [MemberData(nameof(GetLessThanCases))]
        public void LessThanOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<LessThanOp>(data, OP.LessThan);
        }


        public static IEnumerable<object[]> GetGreaterThanCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
        }
        [Theory]
        [MemberData(nameof(GetGreaterThanCases))]
        public void GreaterThanOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<GreaterThanOp>(data, OP.GreaterThan);
        }


        public static IEnumerable<object[]> GetLessThanOrEqualCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, OpTestCaseHelper.num0 };
        }
        [Theory]
        [MemberData(nameof(GetLessThanOrEqualCases))]
        public void LessThanOrEqualOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<LessThanOrEqualOp>(data, OP.LessThanOrEqual);
        }


        public static IEnumerable<object[]> GetGreaterThanOrEqualCases()
        {
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num2, OpTestCaseHelper.num0 };
            yield return new object[] { OpTestCaseHelper.num1, OpTestCaseHelper.num1, OpTestCaseHelper.num1 };
            yield return new object[] { OpTestCaseHelper.num3, OpTestCaseHelper.num2, OpTestCaseHelper.num1 };
        }
        [Theory]
        [MemberData(nameof(GetGreaterThanOrEqualCases))]
        public void GreaterThanOrEqualOpTest(byte[] toPop1, byte[] toPop2, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<GreaterThanOrEqualOp>(data, OP.GreaterThanOrEqual);
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
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<MINOp>(data, OP.MIN);
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
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { toPop1, toPop2 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<MAXOp>(data, OP.MAX);
        }


        public static IEnumerable<object[]> GetWITHINCases()
        {
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num0, OpTestCaseHelper.num3,
                OpTestCaseHelper.num1
            };
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num2, OpTestCaseHelper.num3,
                OpTestCaseHelper.num1
            };
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num3, OpTestCaseHelper.num3,
                OpTestCaseHelper.num0
            };
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num0, OpTestCaseHelper.num2,
                OpTestCaseHelper.num0
            };
            yield return new object[]
            {
                OpTestCaseHelper.num2, OpTestCaseHelper.num3, OpTestCaseHelper.num1,
                OpTestCaseHelper.num0
            };
        }
        [Theory]
        [MemberData(nameof(GetWITHINCases))]
        public void WITHINOpTest(byte[] toPop1, byte[] toPop2, byte[] toPop3, byte[] toPush)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 3,
                popData = new byte[][] { toPop1, toPop2, toPop3 },
                pushData = new byte[][] { toPush },
            };

            OpTestCaseHelper.RunTest<WITHINOp>(data, OP.WITHIN);
        }
    }
}
