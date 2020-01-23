// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class IfDupOpTests
    {
        [Fact]
        public void Run_DuplicateTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek, FuncCallName.Push)
            {
                _itemCount = 1,
                peekData = new byte[][] { OpTestCaseHelper.b1 }, // b1 is not zero so it should be duplicated
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<IfDupOp>(data, OP.IfDup);
        }

        [Fact]
        public void Run_NoDuplicateTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek)
            {
                _itemCount = 1,
                peekData = new byte[][] { OpTestCaseHelper.num0 }, // num0 is zero so no duplication occurs
            };

            OpTestCaseHelper.RunTest<IfDupOp>(data, OP.IfDup);
        }

        [Fact]
        public void Run_ErrorTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek)
            {
                _itemCount = 0,
            };

            OpTestCaseHelper.RunFailTest<IfDupOp>(data, Err.OpNotEnoughItems);
        }
    }
}
