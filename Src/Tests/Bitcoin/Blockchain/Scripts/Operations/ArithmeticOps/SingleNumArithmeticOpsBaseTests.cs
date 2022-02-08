// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.ArithmeticOps
{
    public class SingleNumArithmeticOpsBaseTests
    {
        internal class MockSingleBase : SingleNumArithmeticOpsBase
        {
            public MockSingleBase(long number, bool pass)
            {
                expNum = number;
                expBool = pass;
            }


            private readonly long expNum;
            private readonly bool expBool;


            public override OP OpValue => throw new NotImplementedException();
            public override bool Run(IOpData opData, out Errors error)
            {
                Assert.Equal(expBool, TrySetValue(opData, out error));
                Assert.Equal(expNum, a);
                return expBool;
            }
        }


        public static IEnumerable<object[]> GetValueCases()
        {
            yield return new object[] { 0, OpTestCaseHelper.num0 };
            yield return new object[] { 1, OpTestCaseHelper.num1 };
            yield return new object[] { -1, OpTestCaseHelper.numNeg1 };
            yield return new object[] { int.MaxValue, OpTestCaseHelper.maxInt };
        }
        [Theory]
        [MemberData(nameof(GetValueCases))]
        public void TrySetValueTest(int i, byte[] ba)
        {
            MockSingleBase op = new(i, true);
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 1,
                popData = new byte[][] { ba },
            };

            bool b = op.Run(data, out Errors error);
            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
        }


        [Fact]
        public void TrySetValue_FailTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 0,
            };

            MockSingleBase op = new(0, false);
            bool b = op.Run(data, out Errors error);
            Assert.False(b);
            Assert.Equal(Errors.NotEnoughStackItems, error);
        }

        [Fact]
        public void TrySetValueTest_FailTest2()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 1,
                StrictNumberEncoding = true,
                popData = new byte[][] { new byte[] { 0, 0 } },
            };

            MockSingleBase op = new(0, false);
            bool b = op.Run(data, out Errors error);
            Assert.False(b);
            Assert.Equal(Errors.InvalidStackNumberFormat, error);
        }
    }
}
