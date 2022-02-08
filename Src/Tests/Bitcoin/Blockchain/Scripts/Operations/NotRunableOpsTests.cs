// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class NotRunableOpsTests
    {
        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[] { new ReservedOp() };
            yield return new object[] { new VEROp() };
            yield return new object[] { new Reserved1Op() };
            yield return new object[] { new Reserved2Op() };
            yield return new object[] { new ReturnOp() };
        }
        [Theory]
        [MemberData(nameof(GetRunCases))]
        public void RunTest(NotRunableOps op)
        {
            bool b = op.Run(null, out Errors error);
            Assert.False(b);
            Assert.Equal(Errors.NotRunableOp, error);
        }
    }
}
