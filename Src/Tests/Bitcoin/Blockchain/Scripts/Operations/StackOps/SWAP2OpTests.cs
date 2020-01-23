// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.StackOps
{
    public class SWAP2OpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopCount, FuncCallName.PushMulti)
            {
                _itemCount = 4,
                popCountData = new byte[][][]
                {
                    new byte[][]
                    {
                        OpTestCaseHelper.b1, OpTestCaseHelper.b2, OpTestCaseHelper.b3, OpTestCaseHelper.b4,
                    }
                },
                pushMultiData = new byte[][][]
                {
                    new byte[][]
                    {
                        OpTestCaseHelper.b3, OpTestCaseHelper.b4, OpTestCaseHelper.b1, OpTestCaseHelper.b2,
                    }
                }
            };

            OpTestCaseHelper.RunTest<SWAP2Op>(data, OP.SWAP2);
        }

        [Fact]
        public void RunErrorTest()
        {
            MockOpData data = new MockOpData(FuncCallName.PopCount)
            {
                _itemCount = 3,
            };

            OpTestCaseHelper.RunFailTest<SWAP2Op>(data, Err.OpNotEnoughItems);
        }
    }
}
