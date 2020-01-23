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
    public class NIPOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopIndex)
            {
                _itemCount = 2,
                popIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.b1 } }
            };

            OpTestCaseHelper.RunTest<NIPOp>(data, OP.NIP);
        }

        [Fact]
        public void RunErrorTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopIndex)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<NIPOp>(data, Err.OpNotEnoughItems);
        }
    }
}
