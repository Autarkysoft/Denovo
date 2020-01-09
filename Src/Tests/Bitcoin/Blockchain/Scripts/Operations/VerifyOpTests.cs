// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class VerifyOpTests
    {
        [Fact]
        public void Run_Verifiable_Test()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 1,
                // Any value apart from 0 and -0 are considered true
                popData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<VerifyOp>(data, OP.VERIFY);
        }


        [Theory]
        [InlineData(new byte[] { })]
        [InlineData(new byte[] { 0x80 })]
        [InlineData(new byte[] { 0, 0x80 })]
        public void Run_NotVerifiable_Test(byte[] ba)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 1,
                popData = new byte[][] { ba },
            };

            OpTestCaseHelper.RunFailTest<VerifyOp>(data, "Top stack item value was 'false'.");
        }

        [Fact]
        public void Run_FailTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek)
            {
                _itemCount = 0,
            };

            OpTestCaseHelper.RunFailTest<VerifyOp>(data, Err.OpNotEnoughItems);
        }
    }
}
