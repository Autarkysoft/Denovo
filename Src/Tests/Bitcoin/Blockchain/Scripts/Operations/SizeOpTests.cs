// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

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
            MockOpData data = new MockOpData(FuncCallName.Peek, FuncCallName.Push)
            {
                _itemCount = 1,
                peekData = new byte[][] { OpTestCaseHelper.b1 },
                pushData = new byte[][] { OpTestCaseHelper.num3 },
            };

            OpTestCaseHelper.RunTest<SizeOp>(data, OP.SIZE);
        }


        [Fact]
        public void Run_FailTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek)
            {
                _itemCount = 0,
            };

            OpTestCaseHelper.RunFailTest<SizeOp>(data, Err.OpNotEnoughItems);
        }
    }
}
