// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Defined script types by 
    /// <see cref="PubkeyScript"/>, <see cref="SignatureScript"/>, <see cref="RedeemScript"/>, <see cref="WitnessScript"/> 
    /// classes
    /// </summary>
    public enum ScriptType
    {
        /// <summary>
        /// Unknown script type
        /// </summary>
        Unknown,
        /// <summary>
        /// Public key script used in <see cref="PubkeyScript"/>
        /// </summary>
        ScriptPub,
        /// <summary>
        /// Signature script used in <see cref="SignatureScript"/>
        /// </summary>
        ScriptSig,
        /// <summary>
        /// Redeem script used in <see cref="RedeemScript"/>
        /// </summary>
        ScriptRedeem,
        /// <summary>
        /// Witness script used in <see cref="WitnessScript"/>
        /// </summary>
        ScriptWitness,
    }


    /// <summary>
    /// Defined script types in <see cref="PubkeyScript"/>
    /// </summary>
    public enum PubkeyScriptType
    {
        /// <summary>
        /// An empty <see cref="PubkeyScript"/> instance
        /// </summary>
        Empty,
        /// <summary>
        /// Unknown or undefined script type
        /// </summary>
        Unknown,
        /// <summary>
        /// "Pay to public key" public script
        /// </summary>
        P2PK,
        /// <summary>
        /// "Pay to public key hash" public script
        /// </summary>
        P2PKH,
        /// <summary>
        /// "Pay to script hash" public script
        /// </summary>
        P2SH,
        /// <summary>
        /// "Pay to multi-sig" public script
        /// </summary>
        P2MS,
        /// <summary>
        /// <see cref="OP.CheckLocktimeVerify"/> public script
        /// </summary>
        CheckLocktimeVerify,
        /// <summary>
        /// <see cref="OP.RETURN"/> public script
        /// </summary>
        RETURN,
        /// <summary>
        /// "Pay to witness public key hash" public script
        /// </summary>
        P2WPKH,
        /// <summary>
        /// "Pay to witness script hash" public script
        /// </summary>
        P2WSH
    }


    /// <summary>
    /// Defined script types in <see cref="RedeemScript"/>
    /// </summary>
    public enum RedeemScriptType
    {
        /// <summary>
        /// An empty <see cref="RedeemScript"/> instance
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
}
