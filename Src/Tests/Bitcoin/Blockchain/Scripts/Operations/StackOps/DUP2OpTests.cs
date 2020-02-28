// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class DUP2OpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekCount, FuncCallName.PushMulti)
            {
                _itemCount = 2,
                _altItemCount = 0,
                peekCountData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 } },
                pushMultiData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 } },
            };

            OpTestCaseHelper.RunTest<DUP2Op>(data, OP.DUP2);
        }

        [Fact]
        public void RunErrorTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekCount)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<DUP2Op>(data, Err.OpNotEnoughItems);
        }

        [Fact]
        public void Run_ItemCountOverflowTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekCount, FuncCallName.PushMulti)
            {
                _itemCount = 1001,
                _altItemCount = 0,
                peekCountData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 } },
                pushMultiData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 } },
            };

            OpTestCaseHelper.RunFailTest<DUP2Op>(data, Err.OpStackItemOverflow);
        }
    }
}
