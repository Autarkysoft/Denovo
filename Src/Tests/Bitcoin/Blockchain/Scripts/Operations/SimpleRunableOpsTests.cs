// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class SimpleRunableOpsTests
    {
        private class MockSimpRunOp : SimpleRunableOpsBase
        {
            public override OP OpValue => throw new NotImplementedException();
        }


        [Fact]
        public void Run_BaseClassTest()
        {
            MockSimpRunOp op = new MockSimpRunOp();
            bool b = op.Run(null, out string error);

            Assert.True(b);
            Assert.Null(error);
        }

        public static IEnumerable<object[]> GetCases()
        {
            yield return new object[] { new NOPOp(), OP.NOP };
            yield return new object[] { new NOP1Op(), OP.NOP1 };
            // NOP 2 and 3 are already changed to new OPs
            yield return new object[] { new NOP4Op(), OP.NOP4 };
            yield return new object[] { new NOP5Op(), OP.NOP5 };
            yield return new object[] { new NOP6Op(), OP.NOP6 };
            yield return new object[] { new NOP7Op(), OP.NOP7 };
            yield return new object[] { new NOP8Op(), OP.NOP8 };
            yield return new object[] { new NOP9Op(), OP.NOP9 };
            yield return new object[] { new NOP10Op(), OP.NOP10 };
        }
        [Theory]
        [MemberData(nameof(GetCases))]
        public void RunTest(IOperation op, OP expVal)
        {
            bool b = op.Run(null, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(expVal, op.OpValue);
        }
    }
}
