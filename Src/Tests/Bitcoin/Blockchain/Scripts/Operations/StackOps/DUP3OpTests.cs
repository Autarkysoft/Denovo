// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class DUP3OpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekCount, FuncCallName.PushMulti)
            {
                _itemCount = 3,
                _altItemCount = 0,
                peekCountData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3 } },
                pushMultiData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3 } },
            };

            OpTestCaseHelper.RunTest<DUP3Op>(data, OP.DUP3);
        }

        [Fact]
        public void RunErrorTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekCount)
            {
                _itemCount = 2,
            };

            OpTestCaseHelper.RunFailTest<DUP3Op>(data, Err.OpNotEnoughItems);
        }

        [Fact]
        public void Run_ItemCountOverflowTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PeekCount, FuncCallName.PushMulti)
            {
                _itemCount = 1000,
                _altItemCount = 1,
                peekCountData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3 } },
                pushMultiData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3 } },
            };

            OpTestCaseHelper.RunFailTest<DUP3Op>(data, Err.OpStackItemOverflow);
        }
    }
}
