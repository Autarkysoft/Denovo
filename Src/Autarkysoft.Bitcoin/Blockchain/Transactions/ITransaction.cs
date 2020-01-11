// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;

namespace Autarkysoft.Bitcoin.Blockchain.Transactions
{
    /// <summary>
    /// Defines methods that a transaction class implements. Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface ITransaction: IDeserializable
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
        public IWitnessScript[] WitnessList { get; set; }

        /// <summary>
        /// Transaction LockTime
        /// </summary>
        public LockTime LockTime { get; set; }

        /// <summary>
        /// Returns hash of this instance using the defined hash function.
        /// </summary>
        /// <remarks>
        /// This is the value used in Outpoint's TxHash.
        /// </remarks>
        /// <returns>Hash digest</returns>
        byte[] GetTransactionHash();

        /// <summary>
        /// Returns the hash result that needs to be signed with the private key.
        /// </summary>
        /// <remarks>
        /// For spending a pay-to-script output use 
        /// <see cref="GetBytesToSign(ITransaction, int, SigHashType, IRedeemScript)"/> method.
        /// </remarks>
        /// <param name="prvTx">The transaction being spent</param>
        /// <param name="inputIndex">Index of the input in <see cref="TxInList"/> to be signed</param>
        /// <param name="sht">Signature hash type</param>
        /// <returns>Byte array to use for signin</returns>
        byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht);

        /// <summary>
        /// Returns the hash result that needs to be signed with the private key.
        /// </summary>
        /// <param name="prvTx">The transaction being spent</param>
        /// <param name="inputIndex">Index of the input in <see cref="TxInList"/> to be signed</param>
        /// <param name="sht">Signature hash type</param>
        /// <param name="redeemScript">Redeem script</param>
        /// <returns>Byte array to use for signin</returns>
        byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht, IRedeemScript redeemScript);
    }
}
