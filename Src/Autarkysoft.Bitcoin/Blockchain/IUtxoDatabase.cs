// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties that an unconfirmed transaction output database implements.
    /// </summary>
    public interface IUtxoDatabase
    {
        /// <summary>
        /// Searches inside the UTXO database for the given transaction input and returns the <see cref="IUtxo"/>
        /// if it was found, otherwise null.
        /// </summary>
        /// <param name="tin">Input to search for</param>
        /// <returns>The <see cref="IUtxo"/> instance if the input was found; otherwise null.</returns>
        public IUtxo Find(TxIn tin);

        void MarkSpent(TxIn[] txInList);
        ulong MarkSpentAndGetFee(TxIn[] txInList);
    }
}
