// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Defined script types in <see cref="IPubkeyScript"/>s
    /// </summary>
    public enum PubkeyScriptType
    {
        /// <summary>
        /// An empty <see cref="IPubkeyScript"/> instance
        /// </summary>
        Empty,
        /// <summary>
        /// Unknown or undefined script type
        /// </summary>
        Unknown,
        /// <summary>
        /// "Pay to public key" public script type
        /// </summary>
        P2PK,
        /// <summary>
        /// "Pay to public key hash" public script type
        /// </summary>
        P2PKH,
        /// <summary>
        /// "Pay to script hash" public script type
        /// </summary>
        P2SH,
        /// <summary>
        /// <see cref="OP.CheckLocktimeVerify"/> public script type
        /// </summary>
        CheckLocktimeVerify,
        /// <summary>
        /// <see cref="OP.RETURN"/> public script type
        /// </summary>
        RETURN,
        /// <summary>
        /// "Pay to witness public key hash" public script type
        /// </summary>
        P2WPKH,
        /// <summary>
        /// "Pay to witness script hash" public script type
        /// </summary>
        P2WSH
    }

    /// <summary>
    /// Defined special script types in <see cref="IPubkeyScript"/>s that require additional steps
    /// during transaction verification.
    /// </summary>
    public enum PubkeyScriptSpecialType
    {
        /// <summary>
        /// Any script that doesn't require special attention
        /// </summary>
        None,
        /// <summary>
        /// "Pay to script hash" public script type (top stack item is interpreted as an <see cref="IRedeemScript"/>).
        /// </summary>
        P2SH,
        /// <summary>
        /// "Pay to witness public key hash" public script type
        /// (<see cref="ISignatureScript"/> must be empty and <see cref="IWitness"/> must contain only 2 items:
        /// signature + public key)
        /// </summary>
        P2WPKH,
        /// <summary>
        /// "Pay to witness script hash" public script type
        /// </summary>
        P2WSH
    }

    /// <summary>
    /// Defined script types in <see cref="IRedeemScript"/>s
    /// </summary>
    public enum RedeemScriptType
    {
        /// <summary>
        /// An empty <see cref="IRedeemScript"/> instance
        /// </summary>
        Empty,
        /// <summary>
        /// Unknown or undefined script type
        /// </summary>
        Unknown,
        /// <summary>
        /// Redeem script for m of n multi-signature scripts
        /// </summary>
        MultiSig,
        /// <summary>
        /// Redeem script for <see cref="OP.CheckLocktimeVerify"/> scripts
        /// </summary>
        CheckLocktimeVerify,
        /// <summary>
        /// Redeem script for "pay to witness pubkey hash in a pay to script hash" scripts
        /// </summary>
        P2SH_P2WPKH,
        /// <summary>
        /// Redeem script for "pay to witness script hash in a pay to script hash" scripts
        /// </summary>
        P2SH_P2WSH,
        /// <summary>
        /// Redeem script for "pay to witness script hash" scripts
        /// </summary>
        P2WSH,
    }
    
    /// <summary>
    /// Defined script types in <see cref="IRedeemScript"/>s
    /// </summary>
    public enum RedeemScriptSpecialType
    {
        /// <summary>
        /// Any script that doesn't require special attention
        /// </summary>
        None,
        /// <summary>
        /// Redeem script for "pay to witness pubkey hash in a pay to script hash" scripts
        /// </summary>
        P2SH_P2WPKH,
        /// <summary>
        /// Redeem script for "pay to witness script hash in a pay to script hash" scripts
        /// </summary>
        P2SH_P2WSH,
    }
}
