// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
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
    public class BlockVerifier : IBlockVerifier
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



        /// <inheritdoc/>
        public bool VerifyHeader(BlockHeader header, Target expectedTarget)
        {
            Digest256 tar = header.NBits.ToDigest256();
            return header.Version >= consensus.MinBlockVersion &&
                   header.NBits != 0 && // TODO: possible pointless check
                   tar <= consensus.PowLimit && // TODO: possible pointless check (next line should be enough)
                   header.NBits == expectedTarget &&
                   header.Hash <= tar;
        }


        private void UndoAllUtxos(IBlock block)
        {
            txVer.UtxoDb.Undo(block.TransactionList, block.TransactionList.Length - 1);
        }

        /// <inheritdoc/>
        public bool Verify(IBlock block, out string error)
        {
            if (block.TransactionList.Length < 1)
            {
                error = "Block must contain at least 1 transaction (coinbase).";
                return false;
            }
            if (block.TransactionList.Length * Constants.WitnessScaleFactor > Constants.MaxBlockWeight)
            {
                error = "Block transaction list is too big.";
                return false;
            }
            if (block.Weight > Constants.MaxBlockWeight)
            {
                error = "Block weight is too big.";
                return false;
            }
            if (!consensus.IsSegWitEnabled && block.TransactionList.Any(x => x.WitnessList != null && x.WitnessList.Length != 0))
            {
                error = "Blocks before activation of SegWit can not contain witness.";
                return false;
            }

            if (consensus.IsBip30Enabled)
            {
                foreach (ITransaction tx in block.TransactionList)
                {
                    for (uint i = 0; i < tx.TxOutList.Length; i++)
                    {
                        if (txVer.UtxoDb.Contains(tx.GetTransactionHash(), i, true))
                        {
                            error = "BIP-30 violation (duplicate transaction found).";
                            return false;
                        }
                    }
                }
            }

            txVer.Init();

            ITransaction coinbase = block.TransactionList[0];
            if (!txVer.VerifyCoinbasePrimary(coinbase, out error))
            {
                return false;
            }
            for (int i = 1; i < block.TransactionList.Length; i++)
            {
                // TODO: verify each tx's locktime here
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

            Digest256 actual = block.ComputeMerkleRoot();
            if (!block.Header.MerkleRootHash.Equals(actual))
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

                if (commitPos != -1)
                {
                    if (coinbase.WitnessList == null || coinbase.WitnessList.Length != 1 ||
                        coinbase.WitnessList[0].Items.Length != 1 || coinbase.WitnessList[0].Items[0].Length != 32)
                    {
                        UndoAllUtxos(block);
                        error = "Invalid or non-existant witness commitment in coinbase output.";
                        return false;
                    }

                    byte[] commitment = coinbase.WitnessList[0].Items[0];

                    // An output expected in coinbase with its PubkeyScript.Data.Length of _at least_ 38 bytes
                    // starting with 0x6a24aa21a9ed and followed by 32 byte commitment hash
                    Digest256 root = block.ComputeWitnessMerkleRoot(commitment);
                    byte[] witPubScr = new byte[38];
                    witPubScr[0] = 0x6a;
                    witPubScr[1] = 0x24;
                    witPubScr[2] = 0xaa;
                    witPubScr[3] = 0x21;
                    witPubScr[4] = 0xa9;
                    witPubScr[5] = 0xed;
                    Buffer.BlockCopy(root.ToByteArray(), 0, witPubScr, 6, 32);

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
            }

            // TotalFee must be set already in ITransactionVerifier
            if (!txVer.VerifyCoinbaseOutput(coinbase, out error))
            {
                UndoAllUtxos(block);
                error = $"Invalid coinbase output: {error}";
                return false;
            }

            error = null;
            return true;
        }

        /// <inheritdoc/>
        public void UpdateDB(IBlock block)
        {
            txVer.UtxoDb.Update(block.TransactionList);
        }
    }
}
