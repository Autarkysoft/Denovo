// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class DUPOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek, FuncCallName.Push)
            {
                _itemCount = 1,
                _altItemCount = 0,
                peekData = new byte[][] { OpTestCaseHelper.b1 },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<DUPOp>(data, OP.DUP);
        }


        [Fact]
        public void Run_FailTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek)
            {
                _itemCount = 0,
            };

            OpTestCaseHelper.RunFailTest<DUPOp>(data, Err.OpNotEnoughItems);
        }

        [Fact]
        public void Run_ItemCountOverflowTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek, FuncCallName.Push)
            {
                _itemCount = 999,
                _altItemCount = 2,
                peekData = new byte[][] { OpTestCaseHelper.b1 },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunFailTest<DUPOp>(data, Err.OpStackItemOverflow);
        }
    }
}
