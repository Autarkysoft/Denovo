// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.ArithmeticOps
{
    public class DoubleNumArithmeticOpsBaseTests
    {
        internal class MockDoubleBase : DoubleNumArithmeticOpsBase
        {
            public MockDoubleBase(long first, long second, bool pass)
            {
                expNum1 = first;
                expNum2 = second;
                expBool = pass;
            }


            private readonly long expNum1, expNum2;
            private readonly bool expBool;


            public override OP OpValue => throw new NotImplementedException();
            public override bool Run(IOpData opData, out string error)
            {
                Assert.Equal(expBool, TrySetValues(opData, out error));
                Assert.Equal(expNum1, a);
                Assert.Equal(expNum2, b);
                return expBool;
            }
        }


        public static IEnumerable<object[]> GetValueCases()
        {
            yield return new object[] { 0, 1, OpTestCaseHelper.num0, OpTestCaseHelper.num1 };
            yield return new object[] { -1, int.MaxValue, OpTestCaseHelper.numNeg1, OpTestCaseHelper.maxInt };
        }
        [Theory]
        [MemberData(nameof(GetValueCases))]
        public void TrySetValueTest(int first, int second, byte[] ba1, byte[] ba2)
        {
            MockDoubleBase op = new MockDoubleBase(first, second, true);
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
            };

            bool b = op.Run(data, out string error);
            Assert.True(b, error);
            Assert.Null(error);
        }



        [Fact]
        public void TrySetValueTest_FailTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 1,
            };

            MockDoubleBase op = new MockDoubleBase(0, 0, false);
            bool b = op.Run(data, out string error);
            Assert.False(b);
            Assert.Equal(Err.OpNotEnoughItems, error);
        }

        [Fact]
        public void TrySetValueTest_FailTest2()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 2,
                StrictNumberEncoding = true,
                popData = new byte[][] { OpTestCaseHelper.num0, new byte[] { 0, 0 } },
            };

            MockDoubleBase op = new MockDoubleBase(0, 0, false);
            bool b = op.Run(data, out string error);
            Assert.False(b);
            Assert.Equal("Invalid number format.", error);
        }

        [Fact]
        public void TrySetValueTest_FailTest3()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop)
            {
                _itemCount = 2,
                StrictNumberEncoding = true,
                popData = new byte[][] { new byte[] { 0, 0 }, OpTestCaseHelper.num0 },
            };

            MockDoubleBase op = new MockDoubleBase(0, 0, false);
            bool b = op.Run(data, out string error);
            Assert.False(b);
            Assert.Equal("Invalid number format.", error);
        }
    }
}
