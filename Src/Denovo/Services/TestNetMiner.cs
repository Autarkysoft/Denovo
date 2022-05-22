// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
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

        public async Task<IBlock> Start(IBlock prev, int height, CancellationToken token)
        {
            // Certain things are hard-coded here because this tool is meant for testing.
            // Eventually it will use IWallet.NextAddress() to get a new address to mine to from the wallet instance
            // and IBlockchain.GetTarget() to mine at the correct difficulty.
            // For now it is a good way of mining any transaction that won't propagate in TestNet by bitcoin core clients.

            Consensus consensus = new(height, NetworkType.TestNet);
            string cbText = "Mined using Denovo v0.1.0";
            // A weak key used only for testing
            using PrivateKey key = new(new Sha256().ComputeHash(Encoding.UTF8.GetBytes(cbText)));
            PubkeyScript pkScr = new();
            pkScr.SetToP2WPKH(key.ToPublicKey());

            byte[] commitment = null;

            //var tx1 = new Transaction();
            //tx1.TryDeserialize(new FastStreamReader(Base16.Decode("")), out _);

            ulong fee = 0;

            Transaction coinbase = new()
            {
                Version = 1,
                TxInList = new TxIn[]
                {
                    new TxIn(Digest256.Zero, uint.MaxValue, new SignatureScript(height, Encoding.UTF8.GetBytes($"{cbText} by Coding Enthusiast")), uint.MaxValue)
                },
                TxOutList = new TxOut[]
                {
                    new TxOut(consensus.BlockReward + fee, pkScr),
                    new TxOut(0, new PubkeyScript())
                },
                LockTime = new LockTime(height)
            };


            ((PubkeyScript)coinbase.TxOutList[1].PubScript).SetToReturn(Encoding.UTF8.GetBytes("Testing Mining View Model."));


            Block block = new()
            {
                TransactionList = new Transaction[] { coinbase }
            };

            if (block.TransactionList.Length > 1 && commitment != null)
            {
                coinbase.WitnessList = new Witness[1]
                {
                    new Witness(new byte[][]{ commitment })
                };

                TxOut[] temp = new TxOut[coinbase.TxOutList.Length + 1];
                Array.Copy(coinbase.TxOutList, 0, temp, 0, coinbase.TxOutList.Length);
                temp[^1] = new TxOut();

                // This has to be down here after tx1, tx2,... are set and merkle root is computable
                Digest256 root = block.ComputeWitnessMerkleRoot(commitment);
                coinbase.TxOutList[^1].PubScript.SetToWitnessCommitment(root.ToByteArray());
            }
            uint t = (uint)UnixTimeStamp.TimeToEpoch(DateTime.UtcNow.AddMinutes(22));
            block.Header = new(prev.Header.Version, prev.Header.Hash, block.ComputeMerkleRoot(), t, 0x1d00ffffU, 0);

            bool success = await miner.Mine(block, token, 3);

            return success ? block : null;
        }
    }
}
