// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class NotRunableOpsTests
    {
        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[] { new ReservedOp(), OP.Reserved.ToString() };
            yield return new object[] { new VEROp(), OP.VER.ToString() };
            yield return new object[] { new Reserved1Op(), OP.Reserved1.ToString() };
            yield return new object[] { new Reserved2Op(), OP.Reserved2.ToString() };
            yield return new object[] { new ReturnOp(), OP.RETURN.ToString() };
        }
        [Theory]
        [MemberData(nameof(GetRunCases))]
        public void RunTest(NotRunableOps op, string name)
        {
            bool b = op.Run(null, out string error);
            Assert.False(b);
            Assert.Equal($"Can not run an OP_{name} operation.", error);
        }
    }
}
