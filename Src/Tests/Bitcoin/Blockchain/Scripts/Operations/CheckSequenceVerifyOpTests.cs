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
    public class CheckSequenceVerifyOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new MockOpData(FuncCallName.Peek)
            {
                _itemCount = 1,
                bip112 = true,
                expectedSequence = 17,
                SequenceVerificationSuccess = true,
                peekData = new byte[][] { OpTestCaseHelper.num17 }
            };

            OpTestCaseHelper.RunTest<CheckSequenceVerifyOp>(data, OP.CheckSequenceVerify);
        }

        [Fact]
        public void Run_NoBip112Test()
        {
            MockOpData data = new MockOpData()
            {
                bip112 = false
            };

            OpTestCaseHelper.RunTest<CheckSequenceVerifyOp>(data, OP.CheckSequenceVerify);
        }


        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[] { 0, -1, null, Err.OpNotEnoughItems };
            yield return new object[] { 1, -1, new byte[6], "Invalid number format." };
            yield return new object[] { 1, -10, OpTestCaseHelper.numNeg1, "Locktime can not be negative." };
            yield return new object[] { 1, 16, OpTestCaseHelper.num16, "Foo" };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_FailTest(int count, long expSeq, byte[] peekData, string expErr)
        {
            MockOpData data = new MockOpData(FuncCallName.Peek)
            {
                _itemCount = count,
                bip112 = true,
                expectedSequence = expSeq,
                SequenceVerificationSuccess = false,
                peekData = new byte[][] { peekData }
            };

            OpTestCaseHelper.RunFailTest<CheckSequenceVerifyOp>(data, expErr);
        }
    }
}
