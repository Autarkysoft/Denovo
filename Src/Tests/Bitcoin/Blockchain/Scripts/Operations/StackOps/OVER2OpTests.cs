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
    public class OVER2OpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekIndex, FuncCallName.PeekIndex, FuncCallName.PushMulti)
            {
                _itemCount = 4,
                _altItemCount = 0,
                peekIndexData = new Dictionary<int, byte[]>
                {
                    { 3, OpTestCaseHelper.b1 }, { 2, OpTestCaseHelper.b2 }
                },
                pushMultiData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 } },
            };

            OpTestCaseHelper.RunTest<OVER2Op>(data, OP.OVER2);
        }

        [Fact]
        public void RunErrorTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekIndex)
            {
                _itemCount = 3,
            };

            OpTestCaseHelper.RunFailTest<OVER2Op>(data, Err.OpNotEnoughItems);
        }

        [Fact]
        public void Run_ItemCountOverflowTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekIndex, FuncCallName.PeekIndex, FuncCallName.PushMulti)
            {
                _itemCount = 1000,
                _altItemCount = 1,
                peekIndexData = new Dictionary<int, byte[]>
                {
                    { 3, OpTestCaseHelper.b1 }, { 2, OpTestCaseHelper.b2 }
                },
                pushMultiData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 } },
            };

            OpTestCaseHelper.RunFailTest<OVER2Op>(data, Err.OpStackItemOverflow);
        }
    }
}
