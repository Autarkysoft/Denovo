// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Defines methods and properties that a blockchain (or database) manager implements.
    /// </summary>
    public interface IBlockchain
    {
        /// <summary>
        /// Returns the best block height (tip of the stored chain)
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Returns the current block height based on its previous block hash
        /// </summary>
        /// <param name="prevHash">Previous block hash</param>
        /// <returns>Block height (>=0) or -1 if previous block hash was not found in the database</returns>
        int FindHeight(ReadOnlySpan<byte> prevHash);

        /// <summary>
        /// Returns the expected target for the given block height.
        /// </summary>
        /// <param name="height">Block height</param>
        /// <returns>Target</returns>
        Target GetTarget(int height);

        /// <summary>
        /// Processes the given block by validating the header, transactions,... and adds it to the database.
        /// Should also handle forks and reorgs.
        /// The return value indicates evaluation success.
        /// </summary>
        /// <param name="block">Block to process</param>
        /// <returns>True if the block was valid; otherwise false.</returns>
        bool ProcessBlock(IBlock block);
        /// <summary>
        /// Process given block headers and update the header database
        /// </summary>
        /// <param name="headers">An array of block headers</param>
        void ProcessHeaders(BlockHeader[] headers);

        /// <summary>
        /// Returns an array of <see cref="BlockHeader"/>s from the tip to be used in 
        /// <see cref="P2PNetwork.Messages.MessagePayloads.GetHeadersPayload"/> for initial sync.
        /// </summary>
        /// <param name="max">Maximum number of items to return (min will be 1)</param>
        /// <returns>An array of <see cref="BlockHeader"/> with at least 1 and at most <paramref name="max"/> items.</returns>
        BlockHeader[] GetBlockHeaderLocator(int max);
    }
}
