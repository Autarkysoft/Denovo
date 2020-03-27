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
            NotIfOp op = new NotIfOp(null, null, true);

            Assert.Equal(OP.NotIf, op.OpValue);
            Helper.ComparePrivateField(op, "isWitness", true);
            Helper.ComparePrivateField(op, "runWithTrue", false);
        }


        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(true, null) },
                OpTestCaseHelper.TrueBytes,
                false, // isWit
                true, // runRes
                null
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(false, "Foo") },
                OpTestCaseHelper.TrueBytes,
                false, // isWit
                false, // runRes
                "Foo"
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(true, null) },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.FalseBytes,
                false, // isWit
                true, // runRes
                null
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "Foo") },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.FalseBytes,
                false, // isWit
                false, // runRes
                "Foo"
            };
            // Change the popData
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(true, null) },
                OpTestCaseHelper.b7,
                false, // isWit
                true, // runRes
                null
            };
            // All the above for Witnesses
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(true, null) },
                OpTestCaseHelper.TrueBytes,
                true, // isWit
                true, // runRes
                null
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(false, "Foo") },
                OpTestCaseHelper.TrueBytes,
                true, // isWit
                false, // runRes
                "Foo"
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(true, null) },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.FalseBytes,
                true, // isWit
                true, // runRes
                null
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "Foo") },
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.FalseBytes,
                true, // isWit
                false, // runRes
                "Foo"
            };
            // Change the popData
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                new IOperation[] { new MockOp(true, null) },
                OpTestCaseHelper.b7,
                true, // isWit
                false, // runRes
                "True/False item popped by conditional OPs in a witness script must be strinct."
            };
            // Null operation lists
            yield return new object[]
            {
                null,
                null,
                OpTestCaseHelper.TrueBytes,
                false, // isWit
                true, // runRes
                null
            };
            yield return new object[]
            {
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                null,
                OpTestCaseHelper.TrueBytes,
                true, // isWit
                true, // runRes
                null
            };
            yield return new object[]
            {
                null,
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.FalseBytes,
                false, // isWit
                true, // runRes
                null
            };
            yield return new object[]
            {
                null,
                new IOperation[] { new MockOp(false, "This should not have been run!") },
                OpTestCaseHelper.FalseBytes,
                true, // isWit
                true, // runRes
                null
            };
        }
        [Theory]
        [MemberData(nameof(GetRunCases))]
        public void RunTest(IOperation[] main, IOperation[] other, byte[] popData, bool isWit, bool runResult, string expErr)
        {
            MockOpData data = new MockOpData(FuncCallName.Pop)
            {
                _itemCount = 1,
                popData = new byte[][] { popData }
            };

            NotIfOp op = new NotIfOp(main, other, isWit);
            bool b = op.Run(data, out string error);

            if (runResult)
            {
                Assert.True(b, error);
                Assert.Null(error);
            }
            else
            {
                Assert.False(b);
                Assert.Equal(expErr, error);
            }
        }

        [Fact]
        public void Run_FailTest()
        {
            NotIfOp op = new NotIfOp(null, null, true);
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
