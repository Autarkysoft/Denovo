// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    internal class MockOp : IOperation
    {
        public MockOp(bool runSuccessful, string errorIfFail, OP opValue = (OP)255)
        {
            succeed = runSuccessful;
            errMsg = errorIfFail;
            OpValue = opValue;
        }

        private readonly string errMsg;
        private readonly bool succeed;
        public OP OpValue { get; }

        public bool Run(IOpData opData, out string error)
        {
            if (succeed)
            {
                error = null;
                return true;
            }
            else
            {
                error = errMsg;
                return false;
            }
        }
    }
}
