// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class OVEROpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new(FuncCallName.PeekIndex, FuncCallName.Push)
            {
                _itemCount = 2,
                _altItemCount = 0,
                peekIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.b1 } },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunTest<OVEROp>(data, OP.OVER);
        }

        [Fact]
        public void RunErrorTest()
        {
            MockOpData data = new(FuncCallName.PeekIndex)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<OVEROp>(data, Errors.NotEnoughStackItems);
        }

        [Fact]
        public void Run_ItemCountOverflowTest()
        {
            MockOpData data = new(FuncCallName.PeekIndex, FuncCallName.Push)
            {
                _itemCount = 1001,
                _altItemCount = 2,
                peekIndexData = new Dictionary<int, byte[]> { { 1, OpTestCaseHelper.b1 } },
                pushData = new byte[][] { OpTestCaseHelper.b1 },
            };

            OpTestCaseHelper.RunFailTest<OVEROp>(data, Errors.StackItemCountOverflow);
        }
    }
}
