// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties that a transaction verifier implements.
    /// </summary>
    public interface ITransactionVerifier
    {
        /// <summary>
        /// Returns the <see cref="IUtxoDatabase"/> instance used by this <see cref="ITransactionVerifier"/>.
        /// </summary>
        IUtxoDatabase UtxoDb { get; }
        /// <summary>
        /// Total number of signature operations that this instance has verified so far.
        /// Must be set/reset by the caller for each block.
        /// </summary>
        int TotalSigOpCount { get; set; }
        /// <summary>
        /// Total amount of fees from all the transactions that this instance verified so far.
        /// Must be set/reset by the caller for each block.
        /// </summary>
        ulong TotalFee { get; set; }
        /// <summary>
        /// Returns if this instance verified any SegWit transaction.
        /// Must be reset to false by the caller for each block.
        /// </summary>
        bool AnySegWit { get; set; }

        /// <summary>
        /// Performs primary checks (1-of-2) on the coinbase transactions (input/output, adds SigOp count)
        /// </summary>
        /// <param name="coinbase">Coinbase transaction to verify</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if verification was successful; otherwise false.</returns>
        bool VerifyCoinbasePrimary(ITransaction coinbase, out string error);
        /// <summary>
        /// Performs checks (2of2) on coinbase transaction's output (output amount, WTxId commitment for blocks containing 
        /// SegWit txs). This method should be called after <see cref="Verify(ITransaction, out string)"/> is called for 
        /// all transactions in the block to set the <see cref="TotalFee"/> property.
        /// </summary>
        /// <param name="coinbase">Coinbase transaction to verify</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if verification was successful; otherwise false.</returns>
        bool VerifyCoinbaseOutput(ITransaction coinbase, out string error);
        /// <summary>
        /// Performs all verifications on the given transaction and updates <see cref="TotalSigOpCount"/> and updates
        /// transaction's status inside memory pool and UTXO database.
        /// </summary>
        /// <param name="tx">Transaction to verify (anything except coinbase tx)</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if verification was successful; otherwise false.</returns>
        bool Verify(ITransaction tx, out string error);
    }
}
