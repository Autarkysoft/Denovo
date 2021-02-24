// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Denovo.Services
{
    public class MemoryPool : IMemoryPool
    {
        public void Add(ITransaction tx)
        {

        }

        public bool Contains(ITransaction tx)
        {
            return false;
        }

        public void Remove(ITransaction tx)
        {
            
        }
    }
}
