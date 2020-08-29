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
    public class MockBlockchain : IBlockchain
    {
#pragma warning disable CS0649 // Field is never assigned to

        private const string UnexpectedCall = "Unexpected call was made";

        internal int _height = -1;
        public int Height
        {
            get
            {
                Assert.True(_height != -1, UnexpectedCall);
                return _height;
            }
        }

        internal byte[] expectedHash;
        internal int heightToReturn = -1;
        public int FindHeight(ReadOnlySpan<byte> prevHash)
        {
            Assert.True(expectedHash != null, UnexpectedCall);
            Assert.True(heightToReturn != -1, UnexpectedCall);

            Assert.True(prevHash.SequenceEqual(expectedHash));
            return heightToReturn;
        }

        internal Target? targetToReturn;
        internal int expectedTargetHeight = -1;
        public Target GetTarget(int height)
        {
            Assert.True(targetToReturn.HasValue, UnexpectedCall);
            Assert.True(expectedTargetHeight != -1, UnexpectedCall);

            Assert.Equal(expectedTargetHeight, height);
            return targetToReturn.Value;
        }

        internal string expProcessBlk;
        internal bool blkProcessSuccess;
        public bool ProcessBlock(IBlock block)
        {
            Assert.Equal(expProcessBlk, block.GetBlockID());
            return blkProcessSuccess;
        }

#pragma warning restore CS0649 // Field is never assigned to
    }
}
