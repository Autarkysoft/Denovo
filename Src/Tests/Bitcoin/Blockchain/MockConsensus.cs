// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockConsensus : IConsensus
    {
        public MockConsensus(int mockHeight)
        {
            expHeight = mockHeight;
        }

        private readonly int expHeight;
        internal int maxSigOp, halving;
        internal ulong blockReward;
        internal bool? bip16, bip34, bip65, bip112, bip147, strictDer, segWit;

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

        public ulong GetBlockReward(int height)
        {
            Assert.NotEqual(0UL, blockReward);
            Assert.Equal(expHeight, height);
            return blockReward;
        }

        public bool IsBip112Enabled(int height)
        {
            // NotNull check makes sure value is set by tester otherwise this is an unexpected call
            Assert.NotNull(bip112);
            Assert.Equal(expHeight, height);
            return (bool)bip112;
        }

        public bool IsBip147Enabled(int height)
        {
            Assert.NotNull(bip147);
            Assert.Equal(expHeight, height);
            return (bool)bip147;
        }

        public bool IsBip16Enabled(int height)
        {
            Assert.NotNull(bip16);
            Assert.Equal(expHeight, height);
            return (bool)bip16;
        }

        public bool IsBip34Enabled(int height)
        {
            Assert.NotNull(bip34);
            Assert.Equal(expHeight, height);
            return (bool)bip34;
        }

        public bool IsBip65Enabled(int height)
        {
            Assert.NotNull(bip65);
            Assert.Equal(expHeight, height);
            return (bool)bip65;
        }

        public bool IsSegWitEnabled(int height)
        {
            Assert.NotNull(segWit);
            Assert.Equal(expHeight, height);
            return (bool)segWit;
        }

        public bool IsStrictDerSig(int height)
        {
            Assert.NotNull(strictDer);
            Assert.Equal(expHeight, height);
            return (bool)strictDer;
        }
    }
}
