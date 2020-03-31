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
            CodeSeparatorOp op = new CodeSeparatorOp();

            Assert.False(op.IsExecuted);
            Assert.Equal(OP.CodeSeparator, op.OpValue);

            bool b = op.Run(new MockOpData(), out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.True(op.IsExecuted);
        }
    }
}
