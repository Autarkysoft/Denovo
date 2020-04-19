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
        /// Height of the block containing the transaction(s) to be verified. It must be set/changed by the caller before calling
        /// Verify* methods since it is used to determine current consensus rules.
        /// </summary>
        int BlockHeight { get; set; }
        /// <summary>
        /// Total number of signature operations that this instance has verified so far.
        /// Must be set/reset by the caller.
        /// </summary>
        int TotalSigOpCount { get; set; }
        /// <summary>
        /// Total amount of fees from all the transactions that this instance verified so far.
        /// Must be set/reset by the caller.
        /// </summary>
        ulong TotalFee { get; set; }

        /// <summary>
        /// Performs primary checks (1-of-2) on the coinbase transactions (input/output, adds SigOp count)
        /// </summary>
        /// <param name="transaction">Coinbase transaction to verify</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if verification was successful; otherwise false.</returns>
        bool VerifyCoinbasePrimary(ITransaction transaction, out string error);
        /// <summary>
        /// Performs checks on coinbase transaction's output (2of2) (output amount, WTxId commitment
        /// for blocks containing SegWit txs)
        /// </summary>
        /// <param name="transaction">Coinbase transaction to verify</param>
        /// <param name="witPubScr">The expected pubkey script data (null means there is no witness check)</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if verification was successful; otherwise false.</returns>
        bool VerifyCoinbaseOutput(ITransaction transaction, ReadOnlySpan<byte> witPubScr, out string error);
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
