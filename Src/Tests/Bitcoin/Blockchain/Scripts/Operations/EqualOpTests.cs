// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class EqualOpTests
    {
        [Theory]
        [InlineData(new byte[0] { }, new byte[] { 1 })]
        [InlineData(new byte[] { 1 }, new byte[] { 2 })]
        [InlineData(new byte[] { 1 }, new byte[] { 1, 2 })]
        public void Run_NotEqual_Test(byte[] ba1, byte[] ba2)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
                pushData = new byte[][] { OpTestCaseHelper.FalseBytes },
            };

            OpTestCaseHelper.RunTest<EqualOp>(data, OP.EQUAL);
        }

        [Theory]
        [InlineData(new byte[0] { }, new byte[0] { })]
        [InlineData(new byte[] { 1 }, new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2 })]
        public void Run_Equal_Test(byte[] ba1, byte[] ba2)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
                pushData = new byte[][] { OpTestCaseHelper.TrueBytes },
            };

            OpTestCaseHelper.RunTest<EqualOp>(data, OP.EQUAL);
        }

        [Fact]
        public void Run_FailTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<EqualOp>(data, Err.OpNotEnoughItems);
        }


        [Theory]
        [InlineData(new byte[0] { }, new byte[] { 1 })]
        [InlineData(new byte[] { 1 }, new byte[] { 2 })]
        [InlineData(new byte[] { 1 }, new byte[] { 1, 2 })]
        public void Run_NotEqualVerify_Test(byte[] ba1, byte[] ba2)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
            };

            OpTestCaseHelper.RunFailTest<EqualVerifyOp>(data, "Top two stack items are not equal.");
        }

        [Theory]
        [InlineData(new byte[0] { }, new byte[0] { })]
        [InlineData(new byte[] { 1 }, new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2 })]
        public void Run_EqualVerify_Test(byte[] ba1, byte[] ba2)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
            };

            OpTestCaseHelper.RunTest<EqualVerifyOp>(data, OP.EqualVerify);
        }

        [Fact]
        public void Run_VerifyFailTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<EqualVerifyOp>(data, Err.OpNotEnoughItems);
        }
    }
}
