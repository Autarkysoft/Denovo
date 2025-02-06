// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Denovo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Denovo.Services
{
    public class TestNetMiner
    {
        public TestNetMiner()
        {
            miner = new Miner();
        }

        private readonly Miner miner;

        public async Task<IBlock?> Start(BlockHeader prvHdr, IConsensus consensus, IEnumerable<TxWithFeeModel> txs,
                                         int maxDegreeOfParallelism, CancellationToken token)
        {
            // Certain things are hard-coded here because this tool is meant for testing.
            // Eventually it will use IWallet.NextAddress() to get a new address to mine to from the wallet instance
            // and IBlockchain.GetTarget() to mine at the correct difficulty.
            // For now it is a good way of mining any transaction that won't propagate in TestNet by bitcoin core clients.

            string cbText = "Mined using Denovo v0.8.0 + Bitcoin.Net v0.27.0";
            // A weak key used only for testing
            using PrivateKey key = new(new Sha256().ComputeHash(Encoding.UTF8.GetBytes(cbText)));
            PubkeyScript pkScr = new();
            pkScr.SetToP2WPKH(key.ToPublicKey(new Calc()));


            Block block = new()
            {
                TransactionList = new Transaction[1 + txs.Count()]
            };

            ulong fee = 0;
            int i = 1;
            bool hasWitness = false;
            foreach (TxWithFeeModel item in txs)
            {
                if (item.Tx.WitnessList is not null && item.Tx.WitnessList.Length != 0)
                {
                    hasWitness = true;
                }
                fee += item.Fee;
                block.TransactionList[i++] = item.Tx;
            }

            Transaction coinbase = new()
            {
                Version = 1,
                TxInList =
                [
                    new TxIn(Digest256.Zero, uint.MaxValue, new SignatureScript(consensus.BlockHeight, Encoding.UTF8.GetBytes($"{cbText} by Coding Enthusiast")), uint.MaxValue)
                ],
                // Outputs are set below
                LockTime = new LockTime(consensus.BlockHeight)
            };


            if (block.TransactionList.Length > 1 && hasWitness)
            {
                byte[] commitment = new byte[32];
                coinbase.WitnessList =
                [
                    new Witness([commitment])
                ];

                coinbase.TxOutList = new TxOut[2];
                coinbase.TxOutList[^1] = new TxOut();

                // This has to be down here after tx1, tx2,... are set and merkle root is computable
                Digest256 root = block.ComputeWitnessMerkleRoot(commitment);
                coinbase.TxOutList[^1].PubScript.SetToWitnessCommitment(root.ToByteArray());
            }

            coinbase.TxOutList[0] = new TxOut(consensus.BlockReward + fee, pkScr);

            block.TransactionList[0] = coinbase;

            Random rng = new();
            uint t = prvHdr.BlockTime + TimeConstants.Seconds.TwentyMin + (uint)rng.Next(2, 10);
            block.Header = new(consensus.MinBlockVersion, prvHdr.Hash, block.ComputeMerkleRoot(), t, 0x1d00ffffU, 0);

            bool success = await miner.Mine(block, token, maxDegreeOfParallelism);

            return success ? block : null;
        }
    }
}
