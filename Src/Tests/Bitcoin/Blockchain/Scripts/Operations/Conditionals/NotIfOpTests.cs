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
    public class NotIfOpTests
    {
        [Fact]
        public void ConstructorTest()
        {
            NotIfOp op = new NotIfOp(null, null);

            Assert.Equal(OP.NotIf, op.OpValue);
            Helper.ComparePrivateField(op, "runWithTrue", false);
        }


        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(true, null) },
                OpTestCaseHelper.TrueBytes,
                true, // checkRes
                true, // runRes
                null // error
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(false, "Foo") },
                OpTestCaseHelper.TrueBytes,
                true, // checkRes
                false, // runRes
                "Foo"
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(true, null) },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.FalseBytes,
                true, // checkRes
                true, // runRes
                null
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "Foo") },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
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

            NotIfOp op = new NotIfOp(main, other);
            bool b = op.Run(data, out string error);

            Assert.Equal(runResult, b);
            Assert.Equal(expErr, error);
        }

        [Fact]
        public void Run_FailTest()
        {
            NotIfOp op = new NotIfOp(null, null);
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
