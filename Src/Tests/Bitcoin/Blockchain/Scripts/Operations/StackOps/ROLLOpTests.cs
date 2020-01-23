// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class ROLLOpTests
    {
        [Fact]
        public void RunTest1()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.num0 },
                popIndexData = new Dictionary<int, byte[]> { { 0, OpTestCaseHelper.b1 } },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<ROLLOp>(data, OP.ROLL);
        }

        [Fact]
        public void RunTest2()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex, FuncCallName.Push)
            {
                _itemCount = 4,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3, OpTestCaseHelper.num1 },
                popIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.b2 } },
                pushData = new byte[][] { OpTestCaseHelper.b2 },
            };

            OpTestCaseHelper.RunTest<ROLLOp>(data, OP.ROLL);
        }

        [Fact]
        public void RunTest3()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop, FuncCallName.PopIndex, FuncCallName.Push)
            {
                _itemCount = 4,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3, OpTestCaseHelper.num2 },
                popIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.b1 } },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<ROLLOp>(data, OP.ROLL);
        }


        [Fact]
        public void Run_Error_NotEnoughItemTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<ROLLOp>(data, Err.OpNotEnoughItems);
        }

        [Fact]
        public void Run_Error_HugeNumTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, new byte[5] { 1, 1, 1, 1, 1 } },
            };

            OpTestCaseHelper.RunFailTest<ROLLOp>(data, "'n' is too big.");
        }

        [Fact]
        public void Run_Error_InvalidNumTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, new byte[] { 1, 0 } },
            };

            OpTestCaseHelper.RunFailTest<ROLLOp>(data, "Invalid number format.");
        }

        [Fact]
        public void Run_Error_NegNumTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.numNeg1 },
            };

            OpTestCaseHelper.RunFailTest<ROLLOp>(data, "'n' can not be negative.");
        }

        [Fact]
        public void Run_Error_NotEnoughItemIndexTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.num2 },
            };

            OpTestCaseHelper.RunFailTest<ROLLOp>(data, Err.OpNotEnoughItems);
        }
    }
}
