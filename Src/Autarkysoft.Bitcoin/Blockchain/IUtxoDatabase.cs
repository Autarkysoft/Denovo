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
        public IUtxo Find(TxIn tin);
        void MarkSpent(TxIn[] txInList);
    }
}
