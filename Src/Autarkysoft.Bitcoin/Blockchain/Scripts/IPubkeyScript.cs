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
        /// Returns type of this pubkey script instance.
        /// </summary>
        /// <returns><see cref="PubkeyScriptType"/> enum</returns>
        PubkeyScriptType GetPublicScriptType();
    }
}
