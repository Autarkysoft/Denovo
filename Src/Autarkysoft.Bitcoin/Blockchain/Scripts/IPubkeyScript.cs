// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Defines methods that pubkey scripts (ie. the lockcing scripts) use. Inherits from <see cref="IScript"/>.
    /// </summary>
    public interface IPubkeyScript : IScript
    {
        /// <summary>
        /// Returns type of this pubkey script instance (used to get pre-defined type for signing transactions so that signer
        /// knows how to sign and set the signature).
        /// </summary>
        /// <returns><see cref="PubkeyScriptType"/> enum</returns>
        PubkeyScriptType GetPublicScriptType();

        /// <summary>
        /// Returns the special type of this instance (types that require additional steps during transaction verification).
        /// </summary>
        /// <returns><see cref="PubkeyScriptSpecialType"/> enum</returns>
        PubkeyScriptSpecialType GetSpecialType();

        /// <summary>
        /// Returns if this instance is surely unspendable based on its size and existence of <see cref="OP.RETURN"/> at the start.
        /// </summary>
        /// <returns>
        /// True if script starts with <see cref="OP.RETURN"/> or the size is bigger than <see cref="Constants.MaxScriptLength"/>
        /// </returns>
        bool IsUnspendable();
    }
}
