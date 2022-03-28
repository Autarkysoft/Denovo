// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    class MockBlockVerifier : IBlockVerifier
    {
        internal IBlock expDbBlock;
        public void UpdateDB(IBlock block)
        {
            Assert.Same(expDbBlock, block);
        }


        internal IBlock expVerifyBlock;
        internal bool verifyResult;
        public bool Verify(IBlock block, out string error)
        {
            Assert.Same(expVerifyBlock, block);
            error = verifyHeaderResult ? string.Empty : "Foo";
            return verifyResult;
        }


        internal BlockHeader expHeader;
        internal Target expTarget;
        internal bool verifyHeaderResult;
        public bool VerifyHeader(BlockHeader header, Target expectedTarget)
        {
            Assert.Equal(expHeader.GetHash(), header.GetHash());
            Assert.Equal(expTarget, expectedTarget);
            return verifyHeaderResult;
        }
    }
}
