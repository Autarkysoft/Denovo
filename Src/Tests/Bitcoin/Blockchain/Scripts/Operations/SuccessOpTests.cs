// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
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
            SuccessOp op1 = new(0);
            Assert.Equal(OP._0, op1.OpValue);

            SuccessOp op2 = new(254);
            Assert.Equal((OP)254, op2.OpValue);
        }

        [Fact]
        public void RunTest()
        {
            SuccessOp op = new(254);
            bool b = op.Run(null, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
        }
    }
}
