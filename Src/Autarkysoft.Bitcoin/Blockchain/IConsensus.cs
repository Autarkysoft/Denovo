// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties defining the consensus rules used in bitcoin.
    /// </summary>
    public interface IConsensus
    {
        /// <summary>
        /// Returns maximum number of allowed signature operations in a block
        /// </summary>
        int MaxSigOpCount { get; }

        /// <summary>
        /// Returns the number of blocks between two block reward halvings
        /// </summary>
        int HalvingInterval { get; }

        /// <summary>
        /// Returns maximum allowed block reward based on its height.
        /// </summary>
        /// <param name="height">Block height</param>
        /// <returns>Block reward in satoshi</returns>
        ulong GetBlockReward(int height);

        /// <summary>
        /// BIP-16 enables P2SH scrips
        /// </summary>
        /// <param name="height">Block height</param>
        /// <returns>True if BIP-16 is enabled on this height; otherwise false.</returns>
        bool IsBip16Enabled(int height);

        /// <summary>
        /// BIP-34 requires coinbase transactions to include the block height.
        /// </summary>
        /// <param name="height">Block height</param>
        /// <returns>True if BIP-34 is enabled on this height; otherwise false.</returns>
        bool IsBip34Enabled(int height);

        /// <summary>
        /// Returns if BIP-65 has enabled <see cref="Scripts.OP.CheckLocktimeVerify"/> OP code
        /// </summary>
        /// /// <param name="height">Block height</param>
        /// <returns>True if BIP-65 is enabled on this height; otherwise false.</returns>
        bool IsBip65Enabled(int height);

        /// <summary>
        /// Returns if BIP-66 is enabled to enforce strict DER encoding for signatures.
        /// </summary>
        /// <param name="height">Block height</param>
        /// <returns>True if BIP-66 is enabled on this height; otherwise false.</returns>
        bool IsStrictDerSig(int height);

        /// <summary>
        /// Returns if BIP-112 has enabled <see cref="Scripts.OP.CheckSequenceVerify"/> OP code
        /// </summary>
        /// <param name="height">Block height</param>
        /// <returns>True if BIP-112 is enabled on this height; otherwise false.</returns>
        bool IsBip112Enabled(int height);

        /// <summary>
        /// Returns if BIP-147 has enabled enforcing the dummy stack element that <see cref="Scripts.OP.CheckMultiSig"/> ops pop.
        /// </summary>
        /// <param name="height">Block height</param>
        /// <returns>True if BIP-147 is enabled on this height; otherwise false.</returns>
        bool IsBip147Enabled(int height);

        /// <summary>
        /// Segregated Witness soft fork
        /// </summary>
        /// <param name="height">Block height</param>
        /// <returns>True if SegWit is enabled on this height; otherwise false.</returns>
        bool IsSegWitEnabled(int height);
    }
}
