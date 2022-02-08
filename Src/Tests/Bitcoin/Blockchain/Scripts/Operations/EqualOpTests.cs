// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
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
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
                pushBool = false
            };

            OpTestCaseHelper.RunTest<EqualOp>(data, OP.EQUAL);
        }

        [Theory]
        [InlineData(new byte[0] { }, new byte[0] { })]
        [InlineData(new byte[] { 1 }, new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2 })]
        public void Run_Equal_Test(byte[] ba1, byte[] ba2)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop, FuncCallName.PushBool)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
                pushBool = true
            };

            OpTestCaseHelper.RunTest<EqualOp>(data, OP.EQUAL);
        }

        [Fact]
        public void Run_FailTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<EqualOp>(data, Errors.NotEnoughStackItems);
        }


        [Theory]
        [InlineData(new byte[0] { }, new byte[] { 1 })]
        [InlineData(new byte[] { 1 }, new byte[] { 2 })]
        [InlineData(new byte[] { 1 }, new byte[] { 1, 2 })]
        public void Run_NotEqualVerify_Test(byte[] ba1, byte[] ba2)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
            };

            OpTestCaseHelper.RunFailTest<EqualVerifyOp>(data, Errors.UnequalStackItems);
        }

        [Theory]
        [InlineData(new byte[0] { }, new byte[0] { })]
        [InlineData(new byte[] { 1 }, new byte[] { 1 })]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2 })]
        public void Run_EqualVerify_Test(byte[] ba1, byte[] ba2)
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { ba1, ba2 },
            };

            OpTestCaseHelper.RunTest<EqualVerifyOp>(data, OP.EqualVerify);
        }

        [Fact]
        public void Run_VerifyFailTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<EqualVerifyOp>(data, Errors.NotEnoughStackItems);
        }
    }
}
