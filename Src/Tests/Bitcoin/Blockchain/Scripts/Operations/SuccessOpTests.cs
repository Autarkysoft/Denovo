// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class SuccessOpTests
    {
        [Fact]
        public void ConstructorTest()
        {
            // SuccessOp ctor does not validate OP value (it should be validated by the interpreter)
            var op1 = new SuccessOp(0);
            Assert.Equal(OP._0, op1.OpValue);

            var op2 = new SuccessOp(254);
            Assert.Equal((OP)254, op2.OpValue);
        }

        [Fact]
        public void RunTest()
        {
            var op = new SuccessOp(254);
            bool b = op.Run(null, out string error);

            Assert.True(b, error);
            Assert.Null(error);
        }
    }
}
