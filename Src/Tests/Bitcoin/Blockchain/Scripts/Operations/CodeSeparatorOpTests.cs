// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CodeSeparatorOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData();
            Assert.Equal(0, data.CodeSeparatorCount);
            OpTestCaseHelper.RunTest<CodeSeparatorOp>(data, OP.CodeSeparator);
            Assert.Equal(1, data.CodeSeparatorCount);
            OpTestCaseHelper.RunTest<CodeSeparatorOp>(data, OP.CodeSeparator);
            Assert.Equal(2, data.CodeSeparatorCount);
        }
    }
}
