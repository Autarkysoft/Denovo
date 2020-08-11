// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Defines methods that redeem scripts use. Inherits from <see cref="IScript"/>.
    /// </summary>
    public interface IRedeemScript : IScript
    {
        /// <summary>
        /// Returns type of this redeem script instance (used to get pre-defined type for signing transactions so that signer
        /// knows how to sign and set the signature).
        /// </summary>
        /// <returns><see cref="RedeemScriptType"/> enum</returns>
        RedeemScriptType GetRedeemScriptType();

        /// <summary>
        /// Returns the special type of this instance (types that require additional steps during transaction verification).
        /// </summary>
        /// <param name="consensus">Consensus rules</param>
        /// <param name="height">Block height</param>
        /// <returns><see cref="RedeemScriptSpecialType"/> enum</returns>
        RedeemScriptSpecialType GetSpecialType(IConsensus consensus, int height);

        /// <inheritdoc cref="IScript.CountSigOps"/>
        /// <param name="ops">List of operations</param>
        int CountSigOps(IOperation[] ops);
    }
}
