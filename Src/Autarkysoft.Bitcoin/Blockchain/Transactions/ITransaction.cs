// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;

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
        /// Returns total transaction size in bytes (serialized size including everything)
        /// </summary>
        int TotalSize { get; }
        /// <summary>
        /// Returns Transaction size without witness (serialized size removing witnesses)
        /// </summary>
        int BaseSize { get; }
        /// <summary>
        /// Returns transaction weight (ie. 3x <see cref="BaseSize"/> + <see cref="TotalSize"/>)
        /// </summary>
        int Weight { get; }
        /// <summary>
        /// Returns virtual transaction size (ie. 1/4 * <see cref="Weight"/>)
        /// </summary>
        int VirtualSize { get; }

        /// <summary>
        /// Adds the serialized size of this instance with witnesses removed to the given counter.
        /// </summary>
        /// <param name="counter">Size counter to use</param>
        void AddSerializedSizeWithoutWitness(SizeCounter counter);

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
        /// A special serialization done with the given spending script bytes and based on the <see cref="SigHashType"/>
        /// used in signing operations. Return result is the hash result.
        /// </summary>
        /// <remarks>
        /// This method is mainly for internal use (transaction verification,...) and it covers all possible cases,
        /// with extra care while creating the <paramref name="spendScript"/> it could be used for special cases that the 
        /// strict <see cref="GetBytesToSign(ITransaction, int, SigHashType, IRedeemScript, IRedeemScript)"/>
        /// method doesn't support.
        /// </remarks>
        /// <param name="spendScript">Serialization of the script being spent</param>
        /// <param name="inputIndex">Index of the input being signed</param>
        /// <param name="sht">Signature hash type</param>
        /// <returns>32 byte hash</returns>
        byte[] SerializeForSigning(byte[] spendScript, int inputIndex, SigHashType sht);

        /// <summary>
        /// A special serialization done with the given spending script bytes and based on the <see cref="SigHashType"/>
        /// used in signing operations for SegWit transactions. Return result is the hash result.
        /// </summary>
        /// <remarks>
        /// Same as <see cref="SerializeForSigning(byte[], int, SigHashType)"/>
        /// </remarks>
        /// <param name="spendScript">Script bytes used in signing SegWit outputs (aka scriptCode)</param>
        /// <param name="inputIndex">Index of the input being signed</param>
        /// <param name="amount">The amount in satoshi that is being spent</param>
        /// <param name="sht">Signature hash type</param>
        /// <returns>32 byte hash</returns>
        byte[] SerializeForSigningSegWit(byte[] spendScript, int inputIndex, ulong amount, SigHashType sht);

        /// <summary>
        /// A special serialization used in signing operations for SegWit version 1 transactions (Taproot).
        /// Returned value is the hash result.
        /// </summary>
        /// <param name="epoch">Epoch is always 0</param>
        /// <param name="sht">Signature hash type</param>
        /// <param name="spentOutputs">Outputs being spent by this transaction</param>
        /// <param name="extFlag">Flag</param>
        /// <param name="inputIndex">Index of the input being signed</param>
        /// <param name="annexHash">Annex hash</param>
        /// <param name="tapLeafHash">Tap leaf hash</param>
        /// <param name="keyVersion">Key version is always 0</param>
        /// <param name="codeSeparatorPos">Code separator position</param>
        /// <returns>32 byte hash</returns>
        byte[] SerializeForSigningTaproot(byte epoch, SigHashType sht, IUtxo[] spentOutputs,
                                          byte extFlag, int inputIndex, byte[] annexHash,
                                          byte[] tapLeafHash, byte keyVersion, uint codeSeparatorPos);

        /// <summary>
        /// Returns the hash result that needs to be signed with the private key. 
        /// <para/>This method is very strict and should only used by wallets to sign predefined (standard) transactions.
        /// For verification of already signed transactions use <see cref="TransactionVerifier"/> which calls
        /// <see cref="SerializeForSigning(byte[], int, SigHashType)"/> and
        /// <see cref="SerializeForSigningSegWit(byte[], int, ulong, SigHashType)"/> methods.
        /// </summary>
        /// <param name="prvTx">The transaction being spent</param>
        /// <param name="inputIndex">Index of the input in <see cref="TxInList"/> to be signed</param>
        /// <param name="sht">Signature hash type</param>
        /// <param name="redeem">Redeem script for spending pay-to-script outputs (can be null)</param>
        /// <param name="witRedeem">Redeem script for spending pay-to-witness-script outputs (can be null)</param>
        /// <returns>Byte array to use for signing</returns>
        byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht, IRedeemScript redeem, IRedeemScript witRedeem);

        /// <summary>
        /// Sets the <see cref="SignatureScript"/> of the <see cref="TxIn"/> at the given <paramref name="inputIndex"/> 
        /// to the given <see cref="Signature"/>.
        /// <para/>Similar to <see cref="GetBytesToSign(ITransaction, int, SigHashType, IRedeemScript, IRedeemScript)"/>,
        /// this method is also strict and should only be used for predefined (standard) transactions.
        /// </summary>
        /// <param name="sig">Signature</param>
        /// <param name="pubKey">Public key</param>
        /// <param name="prevTx">The transaction being spent</param>
        /// <param name="inputIndex">Index of the input in <see cref="TxInList"/> that was signed</param>
        /// <param name="redeem">Redeem script for spending pay-to-script outputs (can be null)</param>
        void WriteScriptSig(Signature sig, PublicKey pubKey, ITransaction prevTx, int inputIndex, IRedeemScript redeem);
    }
}
