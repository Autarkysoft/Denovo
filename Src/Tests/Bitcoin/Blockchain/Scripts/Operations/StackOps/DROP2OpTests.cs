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
    public class DROP2OpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new(FuncCallName.PopCount)
            {
                _itemCount = 2,
                popCountData = new byte[][][] { new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 } },
            };

            OpTestCaseHelper.RunTest<DROP2Op>(data, OP.DROP2);
        }

        [Fact]
        public void Run_ErrorTest()
        {
            MockOpData data = new(FuncCallName.PopCount)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<DROP2Op>(data, Errors.NotEnoughStackItems);
        }
    }
}
