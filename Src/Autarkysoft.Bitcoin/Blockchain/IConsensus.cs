// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties defining the consensus rules used in bitcoin.
    /// <para/>Note: Consensus rules change based on block height so it has to be set by the caller before using any of the
    /// properties here and change when the block changes (like in <see cref="BlockVerifier"/> for each block being verified).
    /// </summary>
    public interface IConsensus
    {
        /// <summary>
        /// Gets or sets the block height which will change the consensus (and all other properties).
        /// <para/>Height can not be negative
        /// </summary>
        int BlockHeight { get; set; }

        /// <summary>
        /// Returns maximum number of allowed signature operations in a block
        /// </summary>
        int MaxSigOpCount { get; }

        /// <summary>
        /// Returns the number of blocks between two block reward halvings
        /// </summary>
        int HalvingInterval { get; }

        /// <summary>
        /// Returns maximum allowed block reward based in satoshi
        /// </summary>
        ulong BlockReward { get; }

        /// <summary>
        /// Returns if BIP-16 (P2SH scrips) is enabled
        /// </summary>
        bool IsBip16Enabled { get; }

        /// <summary>
        /// Returns if BIP-34 (coinbase transactions must include block height) is enabled
        /// </summary>
        bool IsBip34Enabled { get; }

        /// <summary>
        /// Returns if BIP-65 (<see cref="Scripts.OP.CheckLocktimeVerify"/>) is enabled
        /// </summary>
        bool IsBip65Enabled { get; }

        /// <summary>
        /// Returns if BIP-66 (enforce strict DER encoding for signatures) is enabled
        /// </summary>
        bool IsStrictDerSig { get; }

        /// <summary>
        /// Returns if BIP-112 (<see cref="Scripts.OP.CheckSequenceVerify"/>) is enabled
        /// </summary>
        bool IsBip112Enabled { get; }

        /// <summary>
        /// Returns if BIP-147 (enforcing strict rules for dummy stack element that <see cref="Scripts.OP.CheckMultiSig"/> ops pop)
        /// is enabled
        /// </summary>
        bool IsBip147Enabled { get; }

        /// <summary>
        /// Returns if Segregated Witness soft fork is enabled
        /// </summary>
        bool IsSegWitEnabled { get; }

        /// <summary>
        /// Builds and returns the genesis block based on <see cref="NetworkType"/>.
        /// </summary>
        /// <returns>Genesis block</returns>
        IBlock GetGenesisBlock();
    }
}
