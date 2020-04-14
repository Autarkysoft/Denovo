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
    /// Defines methods that witness scripts (witness stack items) use. Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface IWitnessScript : IDeserializable
    {
        // TODO: rename and remove the term "Script"

        PushDataOp[] Items { get; set; }

        void SetToP2WPKH(Signature sig, PublicKey pubKey, bool compressed);
    }
}
