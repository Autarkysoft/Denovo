// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Defines methods that witnesses (stack items) use. Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface IWitness : IDeserializable
    {
        /// <summary>
        /// Items (data) to be used in this witness object
        /// </summary>
        PushDataOp[] Items { get; set; }

        /// <summary>
        /// Set this instance to the signature and public key used in claiming P2WPKH outputs.
        /// </summary>
        /// <param name="sig">Signature to use</param>
        /// <param name="pubKey">Public key to use</param>
        /// <param name="useCompressed">
        /// [Default value = true]
        /// Indicates whether to use compressed or uncompressed public key in this witness.
        /// <para/> * Note that uncompressed public keys are non-standard and can lead to funds being lost.
        /// </param>
        void SetToP2WPKH(Signature sig, PublicKey pubKey, bool useCompressed = true);
    }
}
