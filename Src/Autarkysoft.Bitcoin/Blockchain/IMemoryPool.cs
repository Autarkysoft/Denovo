// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties that the memory pool implements.
    /// </summary>
    public interface IMemoryPool
    {
        /// <summary>
        /// Returns if the given transaction is found inside memory pool.
        /// </summary>
        /// <param name="tx">Transaction to check</param>
        /// <returns>True if the transaction was in mempool; otherwise false.</returns>
        bool Contains(ITransaction tx);

        /// <summary>
        /// Removes the given transaction from the memory pool.
        /// </summary>
        /// <param name="tx">Transaction to remove.</param>
        void Remove(ITransaction tx);
    }
}
