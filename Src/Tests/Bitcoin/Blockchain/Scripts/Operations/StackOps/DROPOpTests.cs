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
    public class DROPOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 1,
                popData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<DROPOp>(data, OP.DROP);
        }

        [Fact]
        public void RunErrorTest()
        {
            MockOpData data = new(FuncCallName.Pop)
            {
                _itemCount = 0,
            };

            OpTestCaseHelper.RunFailTest<DROPOp>(data, Errors.NotEnoughStackItems);
        }
    }
}
