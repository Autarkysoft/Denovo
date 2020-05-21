// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Defines methods that signature scripts use. Inherits from <see cref="IScript"/>.
    /// </summary>
    public interface ISignatureScript : IScript
    {
        /// <summary>
        /// Verifies signature script of the coinbase transaction
        /// </summary>
        /// <param name="height">Determines if the coinbase script must have height included</param>
        /// <param name="consensus">Consensus rules</param>
        /// <returns>True if script is valid for coinbase input, otherwise false.</returns>
        bool VerifyCoinbase(int height, IConsensus consensus);

        /// <summary>
        /// Sets this instance to an empty script. This is useful for spending scripts such as P2WPKH.
        /// </summary>
        void SetToEmpty();

        /// <summary>
        /// Sets this script to a single signature used for spending a "pay to pubkey" output,
        /// using the given <see cref="Signature"/>.
        /// </summary>
        /// <param name="sig">Signature to use (must have its <see cref="SigHashType"/> set)</param>
        void SetToP2PK(Signature sig);

        /// <summary>
        /// Sets this script to a single signature used for spending a "pay to pubkey hash" output,
        /// using the given <see cref="Signature"/> and the <see cref="PublicKey"/> with its compression type.
        /// </summary>
        /// <param name="sig">Signature to use (must have its <see cref="SigHashType"/> set)</param>
        /// <param name="pubKey">Public key to use </param>
        /// <param name="useCompressed">Indicates whether to use the compressed or uncompressed public key</param>
        void SetToP2PKH(Signature sig, PublicKey pubKey, bool useCompressed);

        /// <summary>
        /// Sets this script to a multi-signature script using the given parameters. 
        /// This method can be called subsequently with other signatures as they are created.
        /// </summary>
        /// <param name="sig">Signature to use</param>
        /// <param name="pub">Public key of the key used to create the signature</param>
        /// <param name="redeem">Redeem script</param>
        /// <param name="tx"></param>
        /// <param name="prevTx"></param>
        /// <param name="inputIndex"></param>
        void SetToMultiSig(Signature sig, PublicKey pub, IRedeemScript redeem, ITransaction tx, ITransaction prevTx, int inputIndex);

        /// <summary>
        /// Sets this instance to a P2SH-P2WPKH script using the given redeem script.
        /// </summary>
        /// <param name="redeem">Redeem script to use</param>
        void SetToP2SH_P2WPKH(IRedeemScript redeem);

        /// <summary>
        /// Sets this instance to a P2SH-P2WPKH script using the given public key.
        /// </summary>
        /// <param name="pubKey">Public key to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates whether to use compressed or uncompressed public key in the redeem script.
        /// <para/> * Note that uncompressed public keys are non-standard and can lead to funds being lost.
        /// </param>
        void SetToP2SH_P2WPKH(PublicKey pubKey, bool useCompressed);

        /// <summary>
        /// Sets this instance to a P2SH-P2WSH script using the given redeem script.
        /// </summary>
        /// <param name="redeem"></param>
        void SetToP2SH_P2WSH(IRedeemScript redeem);

        /// <summary>
        /// Sets this instance to a locktime script using the given parameters.
        /// </summary>
        /// <param name="sig">Signature to use</param>
        /// <param name="redeem">CheckLocktime OP redeem script</param>
        void SetToCheckLocktimeVerify(Signature sig, IRedeemScript redeem);
    }
}
