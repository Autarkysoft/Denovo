// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;
using System.Threading;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Blocks
{
    public class MinerTest
    {
        private IBlock GetBlock()
        {
            Block blk = new Block();
            // TestNet block #1,670,926
            blk.TryDeserializeHeader(new FastStreamReader(Helper.HexToBytes("00e0ff3ff79fa236e509c35d006c58546db4f27c4874e6dfa4dd5b30b01b1b000000000034310adae6b8d3cca58e56a42eb55ab3c17599cc696b133afe0e4c2c49ecfa2cb5b1795eb334011aabf54a10")), out _);
            return blk;
        }

        [Theory]
        [InlineData(0)] // Max
        [InlineData(1)] // 1 core
        [InlineData(2)] // 2 cores
        public async void MineTest(int coreCount)
        {
            IBlock blk = GetBlock();
            uint expected = 424342955;
            Assert.NotEqual(expected, blk.Nonce);

            Miner miner = new Miner();

            // Cancel mining after 5 seconds to prevent the test from going on too long in case the code has any bugs
            // This should be more than enough time finish the job that takes a micro second to complete.
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            CancellationToken token = cts.Token;

            bool success = await miner.Mine(blk, token, coreCount);

            Assert.True(success);
            Assert.Equal(expected, blk.Nonce);
            Assert.Equal("000000000000005befb0b49dec35738f6ad493f9c7a6e777d69836323f1cf5f2", blk.GetBlockID());
        }

        [Fact]
        public async void Mine_CancelTest()
        {
            IBlock blk = GetBlock();
            uint initialNonce = blk.Nonce;
            uint initialTime = blk.BlockTime;
            blk.Version++; // Change version so that miner can not find any result within reasonable time

            Miner miner = new Miner();

            // Cancel mining after 2 seconds to exit all threads and return false
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            CancellationToken token = cts.Token;

            bool success = await miner.Mine(blk, token);

            Assert.False(success);
            // Make sure properties aren't changed
            Assert.Equal(initialNonce, blk.Nonce);
            Assert.Equal(initialTime, blk.BlockTime);
        }
    }
}
