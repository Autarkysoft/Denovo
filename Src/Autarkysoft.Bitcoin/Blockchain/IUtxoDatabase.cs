// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties that an unconfirmed transaction output database implements.
    /// </summary>
    public interface IUtxoDatabase
    {
        /// <summary>
        /// Returns if the database contains the given output (UTXO is not spent)
        /// </summary>
        /// <param name="hash">Hash of the transaction</param>
        /// <param name="index">Index of the output</param>
        /// <param name="checkCoinbases">Check the coinbase queue too</param>
        /// <returns>True if the UTXO exists; otherwise false.</returns>
        public bool Contains(in Digest256 hash, uint index, bool checkCoinbases);
        /// <summary>
        /// Searches inside the UTXO database for the given transaction input and returns the <see cref="IUtxo"/>
        /// if it was found, otherwise null.
        /// </summary>
        /// <param name="tin">Input to search for</param>
        /// <returns>The <see cref="IUtxo"/> instance if the input was found; otherwise null.</returns>
        public IUtxo Find(TxIn tin);
        /// <summary>
        /// Update database with the given transaction array from the block. First transaction must be the coinbase.
        /// </summary>
        /// <param name="txs">Array of transactions to use</param>
        void Update(ITransaction[] txs);
        /// <summary>
        /// Undo the changes made to each <see cref="IUtxo.IsBlockSpent"/>.
        /// Useful if block verification fails due to an invalid transaction.
        /// </summary>
        /// <param name="txs">Array of transactions in the block</param>
        /// <param name="lastIndex">Last index of the transaction that was checked</param>
        void Undo(ITransaction[] txs, int lastIndex);
        /// <summary>
        /// Writes database to disk.
        /// </summary>
        void WriteToDisk();
    }
}
