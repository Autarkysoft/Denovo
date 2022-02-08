// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class SWAPOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new(FuncCallName.PopCount, FuncCallName.PushMulti)
            {
                _itemCount = 2,
                popCountData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 } },
                pushMultiData = new byte[][][] { new byte[][] { OpTestCaseHelper.b2, OpTestCaseHelper.b1 } },
            };

            OpTestCaseHelper.RunTest<SWAPOp>(data, OP.SWAP);
        }

        [Fact]
        public void Run_ErrorTest()
        {
            MockOpData data = new(FuncCallName.PopCount)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<SWAPOp>(data, Errors.NotEnoughStackItems);
        }
    }
}
