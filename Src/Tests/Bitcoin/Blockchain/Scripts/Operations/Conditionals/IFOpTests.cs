// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.Conditionals
{
    public class IFOpTests
    {
        [Fact]
        public void ConstructorTest()
        {
            IFOp op = new IFOp(null, null);

            Assert.Equal(OP.IF, op.OpValue);
            Helper.ComparePrivateField(op, "runWithTrue", true);
        }


        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[]
            {
                new IOperation[] { new MockOp(true, null) },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.TrueBytes,
                true, // checkRes
                true, // runRes
                null // error
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "Foo") },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.TrueBytes,
                true, // checkRes
                false, // runRes
                "Foo"
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(true, null) },
                OpTestCaseHelper.FalseBytes,
                true, // checkRes
                true, // runRes
                null
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(false, "Foo") },
                OpTestCaseHelper.FalseBytes,
                true, // checkRes
                false, // runRes
                "Foo"
            };
            // Fail on checking the popped data
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.b7,
                false, // checkRes
                false, // runRes
                "True/False item popped by conditional OPs must be strict."
            };
            // Null ElseOps
            yield return new object[]
            {
                new IOperation[] { new MockOp(true, null) },
                null,
                OpTestCaseHelper.TrueBytes,
                true, // checkRes
                true, // runRes
                null
            };
            // Null ElseOps (trying to run ElseOps
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                null,
                OpTestCaseHelper.FalseBytes,
                true, // checkRes
                true, // runRes
                null
            };
        }
        [Theory]
        [MemberData(nameof(GetRunCases))]
        public void RunTest(IOperation[] main, IOperation[] other, byte[] popData, bool checkRes, bool runResult, string expErr)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 1,
                conditionalBoolCheckResult = checkRes,
                expectedConditionalBoolBytes = popData,
                popData = new byte[][] { popData }
            };

            IFOp op = new IFOp(main, other);
            bool b = op.Run(data, out string error);

            Assert.Equal(runResult, b);
            Assert.Equal(expErr, error);
        }

        [Fact]
        public void Run_FailTest()
        {
            IFOp op = new IFOp(null, null);
            MockOpData data = new MockOpData()
            {
                _itemCount = 0
            };

            bool b = op.Run(data, out string error);

            Assert.False(b);
            Assert.Equal(Err.OpNotEnoughItems, error);
        }
    }
}
