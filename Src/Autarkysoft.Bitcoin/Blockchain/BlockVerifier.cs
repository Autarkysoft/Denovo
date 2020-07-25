// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;
using System.Linq;

namespace Autarkysoft.Bitcoin.Blockchain
{
    // TODO: block validation reminder:
    //       a block with valid POW and valid merkle root can have its transactions modified (duplicate some txs)
    //       by a malicious node along the way to make it invalid. the node sometimes has to remember invalid block hashes
    //       that it received to avoid receiving the same thing again. if the reason for being invalid is only merkle root 
    //       and having those duplicate cases then the hash must not be stored or some workaround must be implemented.
    //       more info:
    // https://github.com/bitcoin/bitcoin/blob/1dbf3350c683f93d7fc9b861400724f6fd2b2f1d/src/consensus/merkle.cpp#L8-L42

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
            txVer.TotalSigOpCount = 0;
            ITransaction coinbase = block.TransactionList[0];
            if (!txVer.VerifyCoinbasePrimary(coinbase, out error))
            {
                return false;
            }
            for (int i = 1; i < block.TransactionList.Length; i++)
            {
                if (!txVer.Verify(block.TransactionList[i], out error))
                {
                    return false;
                }
                if (txVer.TotalSigOpCount > consensus.MaxSigOpCount)
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

            // https://github.com/bitcoin/bitcoin/blob/40a04814d130dfc9131af3f568eb44533e2bcbfc/src/validation.cpp#L3574-L3609
            if (consensus.IsSegWitEnabled(block.Height))
            {
                // Find commitment among outputs (38-byte OP_RETURN)
                ReadOnlySpan<byte> start = new byte[6] { 0x6a, 0x24, 0xaa, 0x21, 0xa9, 0xed };
                int commitPos = -1;
                for (int i = coinbase.TxOutList.Length - 1; i >= 0; i--)
                {
                    if (coinbase.TxOutList[i].PubScript.Data.Length >= Constants.MinWitnessCommitmentLen &&
                        ((ReadOnlySpan<byte>)coinbase.TxOutList[i].PubScript.Data).Slice(0, 4).SequenceEqual(start))
                    {
                        commitPos = i;
                        break;
                    }
                }
                if (txVer.AnySegWit && commitPos == -1)
                {
                    error = "Witness commitment was not found in coinbase output.";
                    return false;
                }
                if (txVer.AnySegWit &&
                    coinbase.WitnessList == null || coinbase.WitnessList.Length != 1 ||
                    coinbase.WitnessList[0].Items.Length != 1 || coinbase.WitnessList[0].Items[0].data?.Length != 32)
                {
                    error = "Invalid or non-existant witness commitment in coinbase output.";
                    return false;
                }

                byte[] commitment = coinbase.WitnessList[0].Items[0].data;

                // An output expected in coinbase with its PubkeyScript.Data.Length of _at least_ 38 bytes
                // starting with 0x6a24aa21a9ed and followed by 32 byte commitment hash
                byte[] root = block.ComputeWitnessMerkleRoot(commitment);
                byte[] witPubScr = new byte[38];
                witPubScr[0] = 0x6a;
                witPubScr[1] = 0x24;
                witPubScr[2] = 0xaa;
                witPubScr[3] = 0x21;
                witPubScr[4] = 0xa9;
                witPubScr[5] = 0xed;
                Buffer.BlockCopy(root, 0, witPubScr, 6, 32);

                if (!((ReadOnlySpan<byte>)coinbase.TxOutList[commitPos].PubScript.Data)
                      .Slice(0, Constants.MinWitnessCommitmentLen)
                      .SequenceEqual(witPubScr))
                {
                    error = "Invalid witness commitment in coinbase output.";
                    return false;
                }
            }

            // TotalFee must be set already in ITransactionVerifier
            if (!txVer.VerifyCoinbaseOutput(coinbase, out error))
            {
                error = $"Invalid coinbase output: {error}";
                return false;
            }

            // TODO: add block size and weight checks
            // TODO: add block version checks
            error = null;
            return true;
        }
    }
}
