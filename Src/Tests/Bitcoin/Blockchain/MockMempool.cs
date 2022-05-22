// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockMempool : IMemoryPool
    {
        public MockMempool() : this(Digest256.Zero)
        {
        }

        public MockMempool(Digest256 expectedTxHash)
        {
            hash = expectedTxHash;
        }

        private readonly Digest256 hash;

        public bool Add(ITransaction tx)
        {
            Assert.Equal(hash, tx.GetTransactionHash());
            return true;
        }

        public bool Contains(ITransaction tx)
        {
            if (hash == Digest256.Zero)
            {
                return false;
            }

            Assert.Equal(hash, tx.GetTransactionHash());
            return true;
        }

        public void Remove(ITransaction tx)
        {
            Assert.Equal(hash, tx.GetTransactionHash());
        }
    }
}
