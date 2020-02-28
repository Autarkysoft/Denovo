// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class DEPTHOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Push)
            {
                _itemCount = 1,
                _altItemCount = 0,
                pushData = new byte[][] { OpTestCaseHelper.num1 },
            };

            OpTestCaseHelper.RunTest<DEPTHOp>(data, OP.DEPTH);
        }

        [Fact]
        public void Run_EmptyDataTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Push)
            {
                _itemCount = 0,
                _altItemCount = 0,
                pushData = new byte[][] { OpTestCaseHelper.num0 },
            };

            OpTestCaseHelper.RunTest<DEPTHOp>(data, OP.DEPTH);
        }

        [Fact]
        public void Run_BigDataTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Push)
            {
                _itemCount = 130,
                _altItemCount = 0,
                pushData = new byte[][] { new byte[] { 130, 0 } },
            };

            OpTestCaseHelper.RunTest<DEPTHOp>(data, OP.DEPTH);
        }

        [Fact]
        public void Run_ItemCountOverflowTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Push)
            {
                _itemCount = 1001,
                _altItemCount = 1,
                pushData = new byte[][] { new byte[] { 233, 3 } },
            };

            OpTestCaseHelper.RunFailTest<DEPTHOp>(data, Err.OpStackItemOverflow);
        }
    }
}
