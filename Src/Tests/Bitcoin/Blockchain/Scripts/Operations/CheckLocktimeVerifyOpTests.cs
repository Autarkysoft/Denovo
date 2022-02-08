// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CheckLocktimeVerifyOpTests
    {
        [Fact]
        public void RunTest()
        {
            MockOpData data = new(FuncCallName.Peek)
            {
                _itemCount = 1,
                bip65 = true,
                expectedLocktime = 16,
                locktimeVerificationSuccess = true,
                peekData = new byte[][] { OpTestCaseHelper.num16 }
            };

            OpTestCaseHelper.RunTest<CheckLocktimeVerifyOp>(data, OP.CheckLocktimeVerify);
        }

        [Fact]
        public void Run_NoBip65Test()
        {
            MockOpData data = new()
            {
                bip65 = false
            };

            OpTestCaseHelper.RunTest<CheckLocktimeVerifyOp>(data, OP.CheckLocktimeVerify);
        }


        public static IEnumerable<object[]> GetErrorCases()
        {
            yield return new object[] { 0, -1, null, Errors.NotEnoughStackItems };
            yield return new object[] { 1, -1, new byte[6], Errors.InvalidStackNumberFormat };
            yield return new object[] { 1, -10, OpTestCaseHelper.numNeg1, Errors.NegativeLocktime };
            yield return new object[] { 1, 16, OpTestCaseHelper.num16, Errors.ForTesting };
        }
        [Theory]
        [MemberData(nameof(GetErrorCases))]
        public void Run_FailTest(int count, long expLt, byte[] peekData, Errors expErr)
        {
            MockOpData data = new(FuncCallName.Peek)
            {
                _itemCount = count,
                bip65 = true,
                expectedLocktime = expLt,
                locktimeVerificationSuccess = false,
                peekData = new byte[][] { peekData }
            };

            OpTestCaseHelper.RunFailTest<CheckLocktimeVerifyOp>(data, expErr);
        }
    }
}
