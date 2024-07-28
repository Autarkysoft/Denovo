// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;

namespace Tests.Bitcoin.Blockchain
{
    public class MockChain : IChain
    {
#pragma warning disable CS0649 // Field is never assigned to
#pragma warning disable CS0067 // Field is never used

        private const string UnexpectedCall = "Unexpected call was made";

        internal BlockchainState? _stateToReturn;
        internal BlockchainState? _stateToSet;
        public BlockchainState State
        {
            get
            {
                Assert.True(_stateToReturn.HasValue, UnexpectedCall);
                return _stateToReturn.Value;
            }
            set
            {
                Assert.True(_stateToSet.HasValue, UnexpectedCall);
                Assert.Equal(_stateToSet.Value, value);
            }
        }

        public event EventHandler HeaderSyncEndEvent;
        public event EventHandler BlockSyncEndEvent;

        internal int _height = -1;
        public int Height
        {
            get
            {
                Assert.True(_height != -1, UnexpectedCall);
                return _height;
            }
        }

        internal Digest256 _tip;
        public Digest256 Tip => _tip;

        internal Target? targetToReturn;
        public Target GetNextTarget(in BlockHeader hdr)
        {
            Assert.True(targetToReturn.HasValue, UnexpectedCall);
            return targetToReturn.Value;
        }

        internal string expProcessBlk;
        internal bool blkProcessSuccess;
        public bool ProcessBlock(IBlock block, INodeStatus nodeStatus)
        {
            Assert.NotNull(nodeStatus);
            Assert.Equal(expProcessBlk, block.GetBlockID());
            return blkProcessSuccess;
        }

        internal INodeStatus expRecvBlockNS;
        public void ProcessReceivedBlocks(INodeStatus nodeStatus)
        {
            Assert.Same(expRecvBlockNS, nodeStatus);
        }

        internal BlockHeader[] _expHeaders;
        internal bool? _expHdrProcessResult;
        public bool ProcessHeaders(BlockHeader[] headers, INodeStatus nodeStatus)
        {
            Assert.Equal(_expHeaders.Length, headers.Length);
            Assert.True(_expHdrProcessResult.HasValue, UnexpectedCall);
            for (int i = 0; i < headers.Length; i++)
            {
                Assert.Equal(_expHeaders[i].Serialize(), headers[i].Serialize());
            }

            return _expHdrProcessResult.Value;
        }

        internal BlockHeader[] headerLocatorToReturn;
        public BlockHeader[] GetBlockHeaderLocator()
        {
            Assert.True(!(headerLocatorToReturn is null), UnexpectedCall);
            return headerLocatorToReturn;
        }

        internal BlockHeader[] missingHeadersToReturn;
        internal Digest256[] expCompareHashes;
        internal Digest256 expStopHash;
        public BlockHeader[] GetMissingHeaders(Digest256[] hashesToCompare, Digest256 stopHash)
        {
            Assert.False(expCompareHashes is null, UnexpectedCall);
            Assert.False(missingHeadersToReturn is null, UnexpectedCall);
            Assert.False(hashesToCompare is null, UnexpectedCall);

            Assert.Equal(expCompareHashes.Length, hashesToCompare.Length);
            for (int i = 0; i < expCompareHashes.Length; i++)
            {
                Assert.Equal(expCompareHashes[i], hashesToCompare[i]);
            }
            Assert.Equal(expStopHash, stopHash);

            return missingHeadersToReturn;
        }

        public void PutBackMissingBlocks(List<Inventory> blockInvs)
        {
            throw new NotImplementedException();
        }

        public void SetMissingBlockHashes(INodeStatus nodeStatus)
        {
            Assert.NotNull(nodeStatus);
        }

#pragma warning restore CS0649 // Field is never assigned to
#pragma warning restore CS0067 // Field is never used
    }
}
