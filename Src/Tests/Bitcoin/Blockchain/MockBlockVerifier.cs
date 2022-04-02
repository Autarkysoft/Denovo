// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockBlockVerifier : IBlockVerifier
    {
        public MockBlockVerifier() : this(null, null, null, null, null, null)
        {
        }

        public MockBlockVerifier(IBlock[] blks, IBlock[] dbBlks, bool[] res) : this(blks, dbBlks, res, null, null, null)
        {
        }

        public MockBlockVerifier(BlockHeader[] hdrs, Target[] tars, bool[] res) : this(null, null, null, hdrs, tars, res)
        {
        }

        public MockBlockVerifier(IBlock[] blks, IBlock[] dbBlks, bool[] blkRes, BlockHeader[] hdrs, Target[] tars, bool[] hdrRes)
        {
            expVerifyBlocks = blks ?? Array.Empty<IBlock>();
            expDbBlocks = dbBlks ?? Array.Empty<IBlock>();
            verifyResults = blkRes ?? Array.Empty<bool>();

            expHeaders = hdrs ?? Array.Empty<BlockHeader>();
            verifyHeaderResults = hdrRes ?? Array.Empty<bool>();
            expTargets = tars ?? Array.Empty<Target>();

            Assert.Equal(expVerifyBlocks.Length, verifyResults.Length);
            Assert.Equal(expHeaders.Length, verifyHeaderResults.Length);
            Assert.Equal(expHeaders.Length, expTargets.Length);
        }


        internal void AssertIndex()
        {
            Assert.Equal(verifyIndex, expVerifyBlocks.Length);
            Assert.Equal(dbIndex, expDbBlocks.Length);
            Assert.Equal(hdrIndex, expHeaders.Length);
        }


        internal int verifyIndex, dbIndex, hdrIndex;

        internal IBlock[] expDbBlocks;
        public void UpdateDB(IBlock block)
        {
            Assert.True(dbIndex < expDbBlocks.Length, Helper.UnexpectedCall);
            Assert.Same(expDbBlocks[dbIndex], block);
            dbIndex++;
        }


        internal IBlock[] expVerifyBlocks;
        internal bool[] verifyResults;
        public bool Verify(IBlock block, out string error)
        {
            Assert.True(verifyIndex < expVerifyBlocks.Length, Helper.UnexpectedCall);
            Assert.Same(expVerifyBlocks[verifyIndex], block);
            error = verifyResults[verifyIndex] ? string.Empty : "Foo";
            return verifyResults[verifyIndex++];
        }


        internal BlockHeader[] expHeaders;
        internal Target[] expTargets;
        internal bool[] verifyHeaderResults;
        public bool VerifyHeader(BlockHeader header, Target expectedTarget)
        {
            Assert.True(hdrIndex < expHeaders.Length, Helper.UnexpectedCall);
            Assert.Equal(expHeaders[hdrIndex].GetHash(), header.GetHash());
            Assert.Equal(expTargets[hdrIndex], expectedTarget);
            return verifyHeaderResults[hdrIndex++];
        }
    }
}
