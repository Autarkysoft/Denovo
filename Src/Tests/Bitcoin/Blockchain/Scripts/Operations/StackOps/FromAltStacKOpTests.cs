// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class FromAltStackOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.AltPop, FuncCallName.Push)
            {
                _altItemCount = 1,
                altPopData = new byte[][] { OpTestCaseHelper.b1 },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<FromAltStackOp>(data, OP.FromAltStack);
        }

        [Fact]
        public void RunErrorTest()
        {
            MockOpData data = new MockOpData(FuncCallName.AltPop)
            {
                _altItemCount = 0,
            };

            OpTestCaseHelper.RunFailTest<FromAltStackOp>(data, Err.OpNotEnoughItems + "(alt stack)");
        }

    }
}
