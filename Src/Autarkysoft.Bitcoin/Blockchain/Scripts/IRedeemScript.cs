// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Defines methods that redeem scripts use. Inherits from <see cref="IScript"/>.
    /// </summary>
    public interface IRedeemScript : IScript
    {
        /// <summary>
        /// Returns type of this redeem script instance.
        /// </summary>
        /// <returns><see cref="RedeemScriptType"/> enum</returns>
        RedeemScriptType GetRedeemScriptType();
    }
}
