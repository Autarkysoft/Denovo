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
    public class SizeOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new(FuncCallName.Peek, FuncCallName.Push)
            {
                _itemCount = 1,
                _altItemCount = 0,
                peekData = new byte[][] { OpTestCaseHelper.b1 },
                pushData = new byte[][] { OpTestCaseHelper.num3 },
            };

            OpTestCaseHelper.RunTest<SizeOp>(data, OP.SIZE);
        }


        [Fact]
        public void Run_FailTest()
        {
            MockOpData data = new(FuncCallName.Peek)
            {
                _itemCount = 0,
            };

            OpTestCaseHelper.RunFailTest<SizeOp>(data, Errors.NotEnoughStackItems);
        }

        [Fact]
        public void Run_ItemCountOverflowTest()
        {
            MockOpData data = new(FuncCallName.Peek, FuncCallName.Push)
            {
                _itemCount = 1001,
                _altItemCount = 0,
                peekData = new byte[][] { OpTestCaseHelper.b1 },
                pushData = new byte[][] { OpTestCaseHelper.num3 },
            };

            OpTestCaseHelper.RunFailTest<SizeOp>(data, Errors.StackItemCountOverflow);
        }

        [Fact]
        public void Run_ItemCountOverflowTest2()
        {
            MockOpData data = new(FuncCallName.Peek, FuncCallName.Push)
            {
                _itemCount = 501,
                _altItemCount = 500,
                peekData = new byte[][] { OpTestCaseHelper.b1 },
                pushData = new byte[][] { OpTestCaseHelper.num3 },
            };

            OpTestCaseHelper.RunFailTest<SizeOp>(data, Errors.StackItemCountOverflow);
        }
    }
}
