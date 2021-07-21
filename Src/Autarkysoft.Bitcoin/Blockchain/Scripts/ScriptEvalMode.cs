namespace Autarkysoft.Bitcoin.Blockchain.Scripts
{
    /// <summary>
    /// Specifies the mode used in script evaluation. It will affect OP count, ignoring or rejecting certain OP codes, script
    /// length check, etc. depending on the specified mode activated with forks.
    /// </summary>
    /// <remarks>
    /// Bitcoin core refers to this as "SigVersion"
    /// https://github.com/bitcoin/bitcoin/blob/1c046bb7ac0261d1d8f231ae1d8b39551ee60955/src/script/interpreter.h#L176-L182
    /// </remarks>
    public enum ScriptEvalMode
    {
        /// <summary>
        /// Any legacy transaction (P2PKH, P2SH, etc.)
        /// </summary>
        Legacy,
        /// <summary>
        /// Witness version 0 (P2WPKH and P2WSH)
        /// </summary>
        WitnessV0,
        /// <summary>
        /// Witness version 1 (Taproot)
        /// </summary>
        WitnessV1
    }
}
