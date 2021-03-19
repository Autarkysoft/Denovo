// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;
using System.Linq;
using System.Numerics;

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
        /// <param name="txVerifier">Transaction verifier</param>
        /// <param name="consensus">Consensus rules</param>
        public BlockVerifier(ITransactionVerifier txVerifier, IConsensus consensus)
        {
            txVer = txVerifier;
            this.consensus = consensus;
        }



        private readonly ITransactionVerifier txVer;
        private readonly IConsensus consensus;



        /// <summary>
        /// Verifies validity of thegiven <see cref="BlockHeader"/> by performing basic verifications (verion, target and PoW).
        /// Return value indicates succcess.
        /// <para/><see cref="IConsensus"/> dependency has to be updated by the caller before calling this method.
        /// </summary>
        /// <param name="header">Block header to verify</param>
        /// <param name="expectedTarget">The target that this header must have (calculated considering difficulty adjustment)</param>
        /// <returns>True if the given block header was valid; otherwise false.</returns>
        public bool VerifyHeader(BlockHeader header, Target expectedTarget)
        {
            BigInteger tar = header.NBits.ToBigInt();
            return header.Version >= consensus.MinBlockVersion &&
                   header.NBits != 0 && // TODO: possible pointless check
                   tar <= consensus.PowLimit && // TODO: possible pointless check (next line should be enough)
                   header.NBits == expectedTarget &&
                   new BigInteger(header.GetHash(), isUnsigned: true, isBigEndian: false) <= tar;
        }


        private void UndoAllUtxos(IBlock block)
        {
            txVer.UtxoDb.Undo(block.TransactionList, block.TransactionList.Length - 1);
        }

        /// <summary>
        /// Verifies validity of the given block. Return value indicates succcess.
        /// <para/>Header has to be verified before using <see cref="VerifyHeader(BlockHeader, Target)"/> method.
        /// <para/><see cref="IConsensus"/> dependency has to be updated by the caller before calling this method.
        /// </summary>
        /// <param name="block">Block to use</param>
        /// <param name="error">Error message (null if valid, otherwise contains information about the reason).</param>
        /// <returns>True if block was valid, otherwise false.</returns>
        public bool Verify(IBlock block, out string error)
        {
            if (block.TransactionList.Length < 1)
            {
                error = "Block must contain at least 1 transaction (coinbase).";
                return false;
            }

            txVer.Init();

            ITransaction coinbase = block.TransactionList[0];
            if (!txVer.VerifyCoinbasePrimary(coinbase, out error))
            {
                return false;
            }
            for (int i = 1; i < block.TransactionList.Length; i++)
            {
                if (!txVer.Verify(block.TransactionList[i], out error))
                {
                    txVer.UtxoDb.Undo(block.TransactionList, i);
                    return false;
                }
                if (txVer.TotalSigOpCount > consensus.MaxSigOpCount)
                {
                    txVer.UtxoDb.Undo(block.TransactionList, i);
                    error = "Maximum allowed sigops exceeded.";
                    return false;
                }
            }

            if (!((ReadOnlySpan<byte>)block.Header.MerkleRootHash).SequenceEqual(block.ComputeMerkleRoot()))
            {
                UndoAllUtxos(block);
                error = "Block has invalid merkle root hash.";
                return false;
            }

            // https://github.com/bitcoin/bitcoin/blob/40a04814d130dfc9131af3f568eb44533e2bcbfc/src/validation.cpp#L3574-L3609
            if (consensus.IsSegWitEnabled)
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
                    UndoAllUtxos(block);
                    error = "Witness commitment was not found in coinbase output.";
                    return false;
                }
                if (txVer.AnySegWit &&
                    coinbase.WitnessList == null || coinbase.WitnessList.Length != 1 ||
                    coinbase.WitnessList[0].Items.Length != 1 || coinbase.WitnessList[0].Items[0].data?.Length != 32)
                {
                    UndoAllUtxos(block);
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

                // Script data size is already checked when commitPos was found and is bigger than min length.
                if (!((ReadOnlySpan<byte>)coinbase.TxOutList[commitPos].PubScript.Data)
                      .Slice(0, Constants.MinWitnessCommitmentLen)
                      .SequenceEqual(witPubScr))
                {
                    UndoAllUtxos(block);
                    error = "Invalid witness commitment in coinbase output.";
                    return false;
                }
            }

            // TotalFee must be set already in ITransactionVerifier
            if (!txVer.VerifyCoinbaseOutput(coinbase, out error))
            {
                UndoAllUtxos(block);
                error = $"Invalid coinbase output: {error}";
                return false;
            }

            // TODO: add block size and weight checks

            txVer.UtxoDb.Update(block.TransactionList);

            error = null;
            return true;
        }
    }
}
