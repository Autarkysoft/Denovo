﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class PICKOpTests
    {
        [Fact]
        public void RunTest1()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PeekIndex, FuncCallName.Push)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.num0 },
                peekIndexData = new Dictionary<int, byte[]> { { 0, OpTestCaseHelper.b1 } },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<PICKOp>(data, OP.PICK);
        }

        [Fact]
        public void RunTest2()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PeekIndex, FuncCallName.Push)
            {
                _itemCount = 4,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3, OpTestCaseHelper.num1 },
                peekIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.b2 } },
                pushData = new byte[][] { OpTestCaseHelper.b2 },
            };

            OpTestCaseHelper.RunTest<PICKOp>(data, OP.PICK);
        }

        [Fact]
        public void RunTest3()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.PeekIndex, FuncCallName.Push)
            {
                _itemCount = 4,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3, OpTestCaseHelper.num2 },
                peekIndexData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.b1 } },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<PICKOp>(data, OP.PICK);
        }


        [Fact]
        public void Run_Error_NotEnoughItemTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<PICKOp>(data, Errors.NotEnoughStackItems);
        }

        [Fact]
        public void Run_Error_HugeNumTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, new byte[5] { 1, 1, 1, 1, 1 } },
            };

            OpTestCaseHelper.RunFailTest<PICKOp>(data, Errors.InvalidStackNumberFormat);
        }

        [Fact]
        public void Run_Error_InvalidNumTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, new byte[] { 1, 0 } },
            };

            OpTestCaseHelper.RunFailTest<PICKOp>(data, Errors.InvalidStackNumberFormat);
        }

        [Fact]
        public void Run_Error_NegNumTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.numNeg1 },
            };

            OpTestCaseHelper.RunFailTest<PICKOp>(data, Errors.NegativeStackInteger);
        }

        [Fact]
        public void Run_Error_NotEnoughItemIndexTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 2,
                popData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.num2 },
            };

            OpTestCaseHelper.RunFailTest<PICKOp>(data, Errors.NotEnoughStackItems);
        }
    }
}
