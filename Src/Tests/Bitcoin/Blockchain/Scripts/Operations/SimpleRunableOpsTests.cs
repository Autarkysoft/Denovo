// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class SimpleRunableOpsTests
    {
        private class MockOp : SimpleRunableOps
        {
            public override OP OpValue => throw new NotImplementedException();
        }


        [Fact]
        public void RunTest()
        {
            MockOp op = new MockOp();
            bool b = op.Run(null, out string error);

            Assert.True(b);
            Assert.Null(error);
        }
    }
}
