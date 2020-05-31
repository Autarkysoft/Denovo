// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Encoders;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Blocks
{
    public class MinerTest
    {
        [Fact]
        public void MineTest()
        {
            Block blk = new Block();
            // TestNet block #1,670,926
            blk.TryDeserializeHeader(new FastStreamReader(Base16.Decode("00e0ff3ff79fa236e509c35d006c58546db4f27c4874e6dfa4dd5b30b01b1b000000000034310adae6b8d3cca58e56a42eb55ab3c17599cc696b133afe0e4c2c49ecfa2cb5b1795eb334011aabf54a10")), out _);
            uint expected = 424342955;
            Assert.NotEqual(expected, blk.Nonce);

            using Miner miner = new Miner();

            bool success = miner.Mine(blk);
            Assert.True(success);
            Assert.Equal(expected, blk.Nonce);
            Assert.Equal("000000000000005befb0b49dec35738f6ad493f9c7a6e777d69836323f1cf5f2", blk.GetBlockID());
        }
    }
}
