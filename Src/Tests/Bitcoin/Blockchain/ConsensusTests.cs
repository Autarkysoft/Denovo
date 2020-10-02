// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using System;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class ConsensusTests
    {
        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentException>(() => new Consensus(0, (NetworkType)100));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Consensus(-1, NetworkType.MainNet));
        }

        [Theory]
        [InlineData(1, 50_0000_0000U)]
        [InlineData(209_999, 50_0000_0000U)]
        [InlineData(210_000, 25_0000_0000U)]
        [InlineData(210_001, 25_0000_0000U)]
        [InlineData(419_999, 25_0000_0000U)]
        [InlineData(420_000, 12_5000_0000U)]
        [InlineData(420_001, 12_5000_0000U)]
        [InlineData(13_440_000, 0U)]
        public void BlockRewardTest(int height, ulong expected)
        {
            Consensus cs = new Consensus(height, NetworkType.MainNet);
            ulong actual = cs.BlockReward;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(170059, true)]
        [InlineData(170060, false)]
        [InlineData(170061, true)]
        public void IsBip16Enabled(int height, bool expected)
        {
            Consensus cs = new Consensus(height, NetworkType.MainNet);
            Assert.Equal(expected, cs.IsBip16Enabled);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(227930, false)]
        [InlineData(227931, true)]
        [InlineData(227932, true)]
        public void IsBip34EnabledTest(int height, bool expected)
        {
            Consensus cs = new Consensus(height, NetworkType.MainNet);
            Assert.Equal(expected, cs.IsBip34Enabled);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(388380, false)]
        [InlineData(388381, true)]
        [InlineData(388382, true)]
        public void IsBip65EnabledTest(int height, bool expected)
        {
            Consensus cs = new Consensus(height, NetworkType.MainNet);
            Assert.Equal(expected, cs.IsBip65Enabled);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(363724, false)]
        [InlineData(363725, true)]
        [InlineData(363726, true)]
        public void IsStrictDerSigTest(int height, bool expected)
        {
            Consensus cs = new Consensus(height, NetworkType.MainNet);
            Assert.Equal(expected, cs.IsStrictDerSig);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(419327, false)]
        [InlineData(419328, true)]
        [InlineData(419329, true)]
        public void IsBip112EnabledTest(int height, bool expected)
        {
            Consensus cs = new Consensus(height, NetworkType.MainNet);
            Assert.Equal(expected, cs.IsBip112Enabled);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(481823, false)]
        [InlineData(481824, true)]
        [InlineData(481825, true)]
        public void IsBip147EnabledTest(int height, bool expected)
        {
            Consensus cs = new Consensus(height, NetworkType.MainNet);
            Assert.Equal(expected, cs.IsBip147Enabled);
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(481823, false)]
        [InlineData(481824, true)]
        [InlineData(481825, true)]
        public void IsSegWitEnabledTest(int height, bool expected)
        {
            Consensus cs = new Consensus(height, NetworkType.MainNet);
            Assert.Equal(expected, cs.IsSegWitEnabled);
        }
    }
}
