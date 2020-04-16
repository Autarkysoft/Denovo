// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Transactions
{
    /// <summary>
    /// Defines methods that a transaction class implements. Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface ITransaction : IDeserializable
    {
        /// <summary>
        /// Transaction version. Currently only versions 1 and 2 are defined.
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// List of transaction inputs.
        /// </summary>
        TxIn[] TxInList { get; set; }

        /// <summary>
        /// List of transaction outputs.
        /// </summary>
        TxOut[] TxOutList { get; set; }

        /// <summary>
        /// List of transaction witnesses. It can be null if transaction has no witness.
        /// </summary>
        IWitness[] WitnessList { get; set; }

        /// <summary>
        /// Transaction LockTime
        /// </summary>
        LockTime LockTime { get; set; }

        /// <summary>
        /// Returns if this transaction is verified
        /// </summary>
        bool IsVerified { get; set; }

        /// <summary>
        /// Total number of sigops in this transaction (must be set during verification)
        /// </summary>
        int SigOpCount { get; set; }

        /// <summary>
        /// Returns hash of this instance using the defined hash function.
        /// </summary>
        /// <remarks>
        /// This is the value used in Outpoint's TxHash.
        /// </remarks>
        /// <returns>Hash digest</returns>
        byte[] GetTransactionHash();

        /// <summary>
        /// Returns transaction ID of this instance encoded using base-16 encoding.
        /// </summary>
        /// <returns>Base-16 encoded transaction ID</returns>
        string GetTransactionId();

        /// <summary>
        /// Returns witness hash of this instance using the defined hash function.
        /// </summary>
        /// <returns>Hash digest</returns>
        byte[] GetWitnessTransactionHash();

        /// <summary>
        /// Returns witness transaction ID of this instance encoded using base-16 encoding.
        /// </summary>
        /// <returns>Base-16 encoded transaction ID</returns>
        string GetWitnessTransactionId();

        /// <summary>
        /// A special serialization done with the given <see cref="IScript"/> and based on the <see cref="SigHashType"/>
        /// used in signing operations. Return result is the hash result.
        /// </summary>
        /// <param name="ops">
        /// An array of <see cref="IOperation"/>s from the executing script that needs to be placed instead of 
        /// signing input's <see cref="SignatureScript"/> while serializing.
        /// </param>
        /// <param name="inputIndex">Index of the input being signed</param>
        /// <param name="sht">Signature hash type</param>
        /// <param name="sig">Signature bytes to remove</param>
        /// <returns>32 byte hash</returns>
        byte[] SerializeForSigning(IOperation[] ops, int inputIndex, SigHashType sht, ReadOnlySpan<byte> sig);

        /// <summary>
        /// A special serialization done with the given <see cref="IScript"/> and based on the <see cref="SigHashType"/>
        /// used in signing operations for SegWit transactions. Return result is the hash result.
        /// </summary>
        /// <param name="prevOutScript">Script bytes used in signing SegWit outputs</param>
        /// <param name="inputIndex">Index of the input being signed</param>
        /// <param name="amount">The amount in satoshi that is being spent</param>
        /// <param name="sht">Signature hash type</param>
        /// <returns>32 byte hash</returns>
        byte[] SerializeForSigningSegWit(byte[] prevOutScript, int inputIndex, ulong amount, SigHashType sht);

        /// <summary>
        /// Returns the hash result that needs to be signed with the private key. 
        /// <para/>This method should only used by wallets to sign predefined (standard) transactions.
        /// For verification of already signed transactions use <see cref="TransactionVerifier"/> which calls
        /// <see cref="SerializeForSigning(IOperation[], int, SigHashType, ReadOnlySpan{byte})"/> and
        /// <see cref="SerializeForSigningSegWit(byte[], int, ulong, SigHashType)"/> methods.
        /// </summary>
        /// <param name="prvTx">The transaction being spent</param>
        /// <param name="inputIndex">Index of the input in <see cref="TxInList"/> to be signed</param>
        /// <param name="sht">Signature hash type</param>
        /// <param name="redeem">Redeem script for spending pay-to-script outputs (can be null)</param>
        /// <returns>Byte array to use for signin</returns>
        byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht, IRedeemScript redeem);

        /// <summary>
        /// Sets the <see cref="SignatureScript"/> of the <see cref="TxIn"/> at the given <paramref name="inputIndex"/> 
        /// to the given <see cref="Signature"/>.
        /// </summary>
        /// <param name="sig">Signature</param>
        /// <param name="pubKey">Public key</param>
        /// <param name="prevTx">The transaction being spent</param>
        /// <param name="inputIndex">Index of the input in <see cref="TxInList"/> that was signed</param>
        /// <param name="redeem">Redeem script for spending pay-to-script outputs (can be null)</param>
        void WriteScriptSig(Signature sig, PublicKey pubKey, ITransaction prevTx, int inputIndex, IRedeemScript redeem);
    }
}
