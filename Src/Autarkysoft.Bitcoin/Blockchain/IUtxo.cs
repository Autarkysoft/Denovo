// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties needed for an Unspent Transaction Output
    /// <para/>Inherits from <see cref="IDeserializable"/>
    /// </summary>
    public interface IUtxo : IDeserializable
    {
        /// <summary>
        /// Gets or sets whether this output is spent in mempool (not yet included in a block)
        /// </summary>
        public bool IsMempoolSpent { get; set; }
        /// <summary>
        /// Gets or sets whether this output is spent in a block
        /// </summary>
        public bool IsBlockSpent { get; set; }

        /// <summary>
        /// Index of the output inside <see cref="Transactions.TxOut"/> list
        /// </summary>
        public uint Index { get; set; }
        /// <summary>
        /// Value of this output in satoshi
        /// </summary>
        public ulong Amount { get; set; }
        /// <summary>
        /// Pubkey script of this output
        /// </summary>
        public IPubkeyScript PubScript { get; set; }
    }
}
