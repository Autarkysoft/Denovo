// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Implementation of a transaction verifier to validate all transactions in a block while counting all the signature operations
    /// updating state of the transaction inside memory pool and UTXO set.
    /// Implements <see cref="ITransactionVerifier"/>.
    /// </summary>
    public class TransactionVerifier : ITransactionVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TransactionVerifier"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="isMempool">Indicates whether this instance is used by the memory pool or block</param>
        /// <param name="utxoDatabase">UTXO database</param>
        /// <param name="memoryPool">Memory pool</param>
        /// <param name="consensus">Consensus rules</param>
        public TransactionVerifier(bool isMempool, IUtxoDatabase utxoDatabase, IMemoryPool memoryPool, IConsensus consensus)
        {
            if (utxoDatabase is null)
                throw new ArgumentNullException(nameof(utxoDatabase), "UTXO database can not be null.");
            if (memoryPool is null)
                throw new ArgumentNullException(nameof(memoryPool), "Memory pool can not be null.");
            if (consensus is null)
                throw new ArgumentNullException(nameof(consensus), "Consensus rules can not be null.");

            this.isMempool = isMempool;
            utxoDb = utxoDatabase;
            mempool = memoryPool;
            this.consensus = consensus;
            calc = new EllipticCurveCalculator();
        }


        private readonly bool isMempool;
        private readonly EllipticCurveCalculator calc;
        private readonly IUtxoDatabase utxoDb;
        private readonly IMemoryPool mempool;
        private readonly IConsensus consensus;


        /// <inheritdoc/>
        public int BlockHeight { get; set; }
        /// <inheritdoc/>
        public int TotalSigOpCount { get; set; }
        /// <inheritdoc/>
        public ulong TotalFee { get; set; }

        /// <inheritdoc/>
        public bool VerifyCoinbasePrimary(ITransaction transaction, out string error)
        {
            if (transaction.TxInList.Length != 1)
            {
                error = "Coinbase transaction must contain only one input.";
                return false;
            }
            if (transaction.TxOutList.Length == 0)
            {
                error = "Transaction must contain at least one output.";
                return false;
            }

            var expInputHash = new ReadOnlySpan<byte>(new byte[32]);
            if (!expInputHash.SequenceEqual(transaction.TxInList[0].TxHash) || transaction.TxInList[0].Index != uint.MaxValue)
            {
                error = "Invalid coinbase outpoint.";
                return false;
            }
            if (!transaction.TxInList[0].SigScript.VerifyCoinbase(BlockHeight, consensus))
            {
                error = "Invalid coinbase signature script.";
                return false;
            }

            TotalSigOpCount += transaction.TxInList[0].SigScript.CountSigOps();
            foreach (var tout in transaction.TxOutList)
            {
                TotalSigOpCount += tout.PubScript.CountSigOps();
            }

            error = null;
            return true;
        }

        /// <inheritdoc/>
        public bool VerifyCoinbaseOutput(ITransaction transaction, ReadOnlySpan<byte> witPubScr, out string error)
        {
            ulong totalAmount = 0;
            foreach (var item in transaction.TxOutList)
            {
                totalAmount += item.Amount;
                if (totalAmount > consensus.GetBlockReward(BlockHeight) + TotalFee)
                {
                    error = "Coinbase generates more coins than it should.";
                    return false;
                }
            }

            if (witPubScr != null)
            {
                if (transaction.WitnessList == null || transaction.WitnessList.Length != 1 ||
                transaction.WitnessList[0].Items.Length != 1 || transaction.WitnessList[0].Items[0].data?.Length != 32)
                {
                    error = "Invalid coinbase witness.";
                    return false;
                }

                bool valid = false;
                for (int i = transaction.TxOutList.Length - 1; i >= 0; i--)
                {
                    if (witPubScr.SequenceEqual(transaction.TxOutList[i].PubScript.Data))
                    {
                        valid = true;
                        break;
                    }
                }
                if (!valid)
                {
                    error = "Invalid witness commitment in coinbase output.";
                    return false;
                }
            }

            error = null;
            return true;
        }


        private bool VerifySegWit(ITransaction tx, byte[] prevOutScript, int index, ulong amount,
                                  PushDataOp sigPush, PushDataOp pubPush, out string error)
        {
            if (!Signature.TryRead(sigPush.data, out Signature sig, out error))
            {
                return false;
            }
            if (!PublicKey.TryRead(pubPush.data, out PublicKey pub))
            {
                error = "Invalid public key.";
                return false;
            }

            byte[] dataToSign = tx.SerializeForSigningSegWit(prevOutScript, index, amount, sig.SigHash);
            return calc.Verify(dataToSign, sig, pub);
        }

        /// <inheritdoc/>
        public bool Verify(ITransaction tx, out string error)
        {
            // TODO: 2 things currently missing (will be fixed while adding tests):
            //       1) amounts need to be checked and total fee should be set at the end
            //       2) SegWit script evaluation needs to be fixed.

            // If a tx is already in memory pool it must have been verified and be valid
            if (mempool.Contains(tx))
            {
                TotalSigOpCount += tx.SigOpCount;
                utxoDb.MarkSpent(tx.TxInList);
                error = null;
                return true;
            }

            if (tx.TxInList.Length == 0 || tx.TxOutList.Length == 0)
            {
                error = "Invalid number of inputs or outputs.";
                return false;
            }

            for (int i = 0; i < tx.TxInList.Length; i++)
            {
                TxIn item = tx.TxInList[i];
                IUtxo prevOutput = utxoDb.Find(item);
                if (prevOutput is null)
                {
                    // TODO: add a ToString() method to TxIn?
                    error = $"Input {item.TxHash.ToBase16()}:{item.Index} was not found.";
                    return false;
                }

                if (!prevOutput.PubScript.TryEvaluate(out IOperation[] pubOps, out error))
                {
                    error = $"Invalid input transaction pubkey script." +
                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                            $"{Environment.NewLine}More info: {error}";
                    return false;
                }

                if (!item.SigScript.TryEvaluate(out IOperation[] sigOps, out error))
                {
                    error = $"Invalid transaction signature script." +
                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                            $"{Environment.NewLine}More info: {error}";
                    return false;
                }

                OpData stack = new OpData();

                PubkeyScriptSpecialType pubType = prevOutput.PubScript.GetSpecialType();
                if (pubType == PubkeyScriptSpecialType.None)
                {
                    // TODO: check witness of this item at its corresponding indes is empty
                    stack.prevScript = sigOps;
                    foreach (var op in sigOps)
                    {
                        if (!op.Run(stack, out error))
                        {
                            error = $"Script evaluation failed." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }
                    }

                    stack.prevScript = pubOps;
                    foreach (var op in pubOps)
                    {
                        if (!op.Run(stack, out error))
                        {
                            error = $"Script evaluation failed." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }
                    }
                }
                else if (pubType == PubkeyScriptSpecialType.P2SH)
                {
                    if (sigOps.Length > 0 && sigOps[^1] is PushDataOp pushRedeem)
                    {
                        RedeemScript redeem = new RedeemScript(pushRedeem.data);
                        RedeemScriptSpecialType rdmType = redeem.GetSpecialType();
                        if (!redeem.TryEvaluate(out IOperation[] redeemOps, out error))
                        {
                            error = $"Script evaluation failed." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }

                        stack.prevScript = sigOps;
                        foreach (var op in sigOps)
                        {
                            if (!op.Run(stack, out error))
                            {
                                error = $"Script evaluation failed." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }
                        }

                        stack.prevScript = pubOps;
                        foreach (var op in pubOps)
                        {
                            if (!op.Run(stack, out error))
                            {
                                error = $"Script evaluation failed." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }
                        }

                        if (rdmType == RedeemScriptSpecialType.None)
                        {
                            // TODO: check witness of this item at its corresponding indes is empty
                            stack.prevScript = redeemOps;
                            foreach (var op in redeemOps)
                            {
                                if (!op.Run(stack, out error))
                                {
                                    error = $"Script evaluation failed." +
                                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                            $"{Environment.NewLine}More info: {error}";
                                    return false;
                                }
                            }
                        }
                        else if (rdmType == RedeemScriptSpecialType.P2SH_P2WPKH)
                        {
                            if (tx.WitnessList.Length != tx.TxInList.Length || tx.WitnessList[i].Items.Length != 2)
                            {
                                error = $"Mandatory witness is not found." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }

                            byte[] temp = new byte[26];
                            temp[0] = 25;
                            temp[1] = (byte)OP.DUP;
                            temp[2] = (byte)OP.HASH160;
                            // Copy both push_size+hash from prev script
                            Buffer.BlockCopy(pushRedeem.data, 1, temp, 3, 21);
                            temp[^2] = (byte)OP.EqualVerify;
                            temp[^1] = (byte)OP.CheckSig;

                            return VerifySegWit(tx, temp, i, prevOutput.Amount,
                                                tx.WitnessList[i].Items[0], tx.WitnessList[i].Items[1], out error);
                        }
                    }
                    else
                    {
                        // TODO: this should be changed!
                        error = "Redeem script was not found.";
                        return false;
                    }
                }
                else if (pubType == PubkeyScriptSpecialType.P2WPKH)
                {
                    if (tx.WitnessList.Length != tx.TxInList.Length || tx.WitnessList[i].Items.Length != 2)
                    {
                        error = $"Mandatory witness is not found." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    byte[] temp = new byte[26];
                    temp[0] = 25;
                    temp[1] = (byte)OP.DUP;
                    temp[2] = (byte)OP.HASH160;
                    // Copy both push_size+hash from prev script
                    Buffer.BlockCopy(prevOutput.PubScript.Data, 1, temp, 3, 21);
                    temp[^2] = (byte)OP.EqualVerify;
                    temp[^1] = (byte)OP.CheckSig;

                    return VerifySegWit(tx, temp, i, prevOutput.Amount,
                                        tx.WitnessList[i].Items[0], tx.WitnessList[i].Items[1], out error);
                }
                else if (pubType == PubkeyScriptSpecialType.P2WSH)
                {
                    if (tx.WitnessList.Length != tx.TxInList.Length || tx.WitnessList[i].Items.Length < 1)
                    {
                        error = $"Mandatory witness is not found." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    RedeemScript redeem = new RedeemScript(tx.WitnessList[i].Items[^1].data);
                    if (!redeem.TryEvaluate(out IOperation[] redeemOps, out error))
                    {
                        error = $"Script evaluation failed." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    foreach (var op in tx.WitnessList[i].Items)
                    {
                        if (!op.Run(stack, out error))
                        {
                            error = $"Script evaluation failed." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }
                    }

                    if (!new Sha256Op().Run(stack, out error))
                    {
                        error = $"Script evaluation failed." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    // TODO: CheckSigOps need to change
                    foreach (var op in redeemOps)
                    {
                        if (!op.Run(stack, out error))
                        {
                            error = $"Script evaluation failed." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Pubkey special script type is not defined.");
                }



                if (stack.ItemCount == 0)
                {
                    error = $"Script evaluation failed (empty stack after execution)." +
                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                    return false;
                }
                else if (new VerifyOp().Run(stack, out _))
                {
                    error = $"Script evaluation failed (top stack item is false)." +
                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                    return false;
                }

                if (isMempool)
                {
                    prevOutput.IsMempoolSpent = true;
                }
                else
                {
                    prevOutput.IsBlockSpent = true;
                }

            }


            error = null;
            return true;
        }


    }
}
