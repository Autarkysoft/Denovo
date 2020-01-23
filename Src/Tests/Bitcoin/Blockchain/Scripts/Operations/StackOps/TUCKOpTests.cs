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
    public class TUCKOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek, FuncCallName.Insert)
            {
                _itemCount = 2,
                peekData = new byte[][] { OpTestCaseHelper.b1, OpTestCaseHelper.b2 },
                insertData = new Dictionary<int, byte[]> { { 2, OpTestCaseHelper.b2 } }
            };

            OpTestCaseHelper.RunTest<TUCKOp>(data, OP.TUCK);
        }

        [Fact]
        public void Run_ErrorTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek)
            {
                _itemCount = 1,
            };

            OpTestCaseHelper.RunFailTest<TUCKOp>(data, Err.OpNotEnoughItems);
        }
    }
}
