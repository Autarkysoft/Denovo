// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Cryptography.Hashing;

namespace Tests.Bitcoin.Blockchain
{
    public class MockConsensus : IConsensus
    {
#pragma warning disable CS0649 // Field is never assigned to
        internal int expHeight = -1, maxSigOp = 0, halving = 0;
        internal ulong blockReward;
        internal bool? bip16, bip30, bip34, bip65, bip94, bip112, bip147, strictDer, segWit, tap;

        private const string UnexpectedCall = "Unexpected call was made";

        public int BlockHeight
        {
            get
            {
                Assert.False(expHeight == -1, "Expected height should be set first.");
                return expHeight;
            }
            set
            {
                Assert.False(expHeight == -1, "Expected height should be set first.");
                Assert.Equal(expHeight, value);
            }
        }


        public int MaxSigOpCount
        {
            get
            {
                // NotEqual zero makes sure value is set by tester otherwise this is considerd an unexpected call
                Assert.NotEqual(0, maxSigOp);
                return maxSigOp;
            }
        }

        public int HalvingInterval
        {
            get
            {
                Assert.NotEqual(0, halving);
                return halving;
            }
        }

        public ulong BlockReward
        {
            get
            {
                Assert.NotEqual(0UL, blockReward);
                return blockReward;
            }
        }

        public bool IsBip112Enabled
        {
            // NotNull check makes sure value is set by tester otherwise this is an unexpected call
            get
            {
                Assert.True(bip112.HasValue, UnexpectedCall);
                return bip112.Value;
            }
        }

        public bool IsBip147Enabled
        {
            get
            {
                Assert.True(bip147.HasValue, UnexpectedCall);
                return bip147.Value;
            }
        }

        public bool IsBip94
        {
            get
            {
                Assert.True(bip94.HasValue, UnexpectedCall);
                return bip94.Value;
            }
        }

        public bool IsBip16Enabled
        {
            get
            {
                Assert.True(bip16.HasValue, UnexpectedCall);
                return bip16.Value;
            }
        }

        public bool IsBip30Enabled
        {
            get
            {
                Assert.True(bip30.HasValue, UnexpectedCall);
                return bip30.Value;
            }
        }

        public bool IsBip34Enabled
        {
            get
            {
                Assert.True(bip34.HasValue, UnexpectedCall);
                return bip34.Value;
            }
        }

        public bool IsBip65Enabled
        {
            get
            {
                Assert.True(bip65.HasValue, UnexpectedCall);
                return bip65.Value;
            }
        }

        public bool IsSegWitEnabled
        {
            get
            {
                Assert.True(segWit.HasValue, UnexpectedCall);
                return segWit.Value;
            }
        }

        public bool IsTaprootEnabled
        {
            get
            {
                Assert.True(tap.HasValue, UnexpectedCall);
                return tap.Value;
            }
        }

        public bool IsStrictDerSig
        {
            get
            {
                Assert.True(strictDer.HasValue, UnexpectedCall);
                return strictDer.Value;
            }
        }


        internal int? _minVer;
        public int MinBlockVersion
        {
            get
            {
                Assert.True(_minVer.HasValue, UnexpectedCall);
                return _minVer.Value;
            }
        }


        internal bool? _allowMinDiff;
        public bool AllowMinDifficultyBlocks
        {
            get
            {
                Assert.True(_allowMinDiff.HasValue, UnexpectedCall);
                return _allowMinDiff.Value;
            }
        }


        internal bool? _noPowRetarget;
        public bool NoPowRetarget
        {
            get
            {
                Assert.True(_noPowRetarget.HasValue, UnexpectedCall);
                return _noPowRetarget.Value;
            }
        }


        internal Digest256? _powLimit;
        public Digest256 PowLimit
        {
            get
            {
                Assert.True(_powLimit.HasValue, UnexpectedCall);
                return _powLimit.Value;
            }
        }

        internal IBlock? _genesis;
        public IBlock GetGenesisBlock()
        {
            Assert.True(_genesis is not null, UnexpectedCall);
            return _genesis;
        }

#pragma warning restore CS0649 // Field is never assigned to
    }
}
