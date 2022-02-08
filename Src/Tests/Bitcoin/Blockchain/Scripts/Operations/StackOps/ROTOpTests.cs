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
    public class ROTOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new(FuncCallName.PopCount, FuncCallName.PushMulti)
            {
                _itemCount = 3,
                popCountData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3 } },
                pushMultiData = new byte[][][] { new byte[][] { OpTestCaseHelper.b2, OpTestCaseHelper.b3, OpTestCaseHelper.b1 } },
            };

            OpTestCaseHelper.RunTest<ROTOp>(data, OP.ROT);
        }

        [Fact]
        public void Run_ErrorTest()
        {
            MockOpData data = new(FuncCallName.PopCount)
            {
                _itemCount = 2,
            };

            OpTestCaseHelper.RunFailTest<ROTOp>(data, Errors.NotEnoughStackItems);
        }
    }
}
