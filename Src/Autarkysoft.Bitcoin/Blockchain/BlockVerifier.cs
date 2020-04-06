// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Implementation of a block verifier used to validate new blocks.
    /// </summary>
    public class BlockVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BlockVerifier"/> using the given parameters.
        /// </summary>
        /// <param name="blockchain">Blockchain data base</param>
        /// <param name="txVerifier">Transaction verifier</param>
        /// <param name="consensus">Consensus rules</param>
        public BlockVerifier(IBlockchain blockchain, ITransactionVerifier txVerifier, IConsensus consensus)
        {
            chain = blockchain;
            txVer = txVerifier;
            this.consensus = consensus;
        }



        private readonly IBlockchain chain;
        private readonly ITransactionVerifier txVer;
        private readonly IConsensus consensus;



        /// <summary>
        /// Verifies validity of the given block. Return value indicates succcess.
        /// </summary>
        /// <param name="block">Block to use</param>
        /// <param name="error">Error message (null if valid, otherwise contains information about the reason).</param>
        /// <returns>True if block header was valid, otherwise false.</returns>
        public bool Verify(IBlock block, out string error)
        {
            if (block.Height < 0)
            {
                error = "Block height is not set.";
                return false;
            }
            // We can only verify _new_ blocks since verification requires an up to date UTXO set.
            // In case of a split (orphan,...) the caller must update IBlockchain and IUtxo first.
            if (block.Height != chain.Height + 1)
            {
                error = "Block is not new.";
                return false;
            }

            if (block.NBits != chain.GetTarget(block.Height))
            {
                error = "Block's target is not the same as current target.";
                return false;
            }
            if (block.GetBlockHash().ToBigInt(false, true) > block.NBits.ToBigInt())
            {
                error = "Wrong proof of work.";
                return false;
            }

            if (block.TransactionList.Length < 1)
            {
                error = "Block must contain at least 1 transaction (coinbase).";
                return false;
            }

            txVer.BlockHeight = block.Height;
            txVer.SigOpCount = 0;
            if (!txVer.VerifyCoinbase(block.TransactionList[0], out error))
            {
                return false;
            }
            for (int i = 1; i < block.TransactionList.Length; i++)
            {
                if (!txVer.Verify(block.TransactionList[i], out error))
                {
                    return false;
                }
                if (txVer.SigOpCount > consensus.MaxSigOpCount)
                {
                    error = "Maximum allowed sigops exceeded.";
                    return false;
                }
            }

            if (!((ReadOnlySpan<byte>)block.MerkleRootHash).SequenceEqual(block.ComputeMerkleRoot()))
            {
                error = "Block has invalid merkle root hash.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
