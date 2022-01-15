// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockMempool : IMemoryPool
    {
        public MockMempool(byte[] expectedTxHash)
        {
            hash = expectedTxHash;
        }

        private readonly byte[] hash;

        public bool Add(ITransaction tx)
        {
            Assert.Equal(hash, tx.GetTransactionHash());
            return true;
        }

        public bool Contains(ITransaction tx)
        {
            if (hash == null)
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
