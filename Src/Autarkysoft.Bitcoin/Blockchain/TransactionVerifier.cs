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
            scrSer = new ScriptSerializer();
        }


        private readonly bool isMempool;
        private readonly EllipticCurveCalculator calc;
        private readonly ScriptSerializer scrSer;
        private readonly IUtxoDatabase utxoDb;
        private readonly IMemoryPool mempool;
        private readonly IConsensus consensus;


        /// <inheritdoc/>
        public int BlockHeight { get; set; }
        /// <inheritdoc/>
        public int TotalSigOpCount { get; set; }
        /// <inheritdoc/>
        public ulong TotalFee { get; set; }

        public bool ForceLowS { get; set; }
        public bool ForceStrictPush { get; set; }

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


        /// <inheritdoc/>
        public bool Verify(ITransaction tx, out string error)
        {
            // If a tx is already in memory pool it must have been verified and be valid.
            // The SigOpCount property must be set by the caller (mempool dependency).
            if (mempool.Contains(tx))
            {
                TotalSigOpCount += tx.SigOpCount;
                TotalFee += utxoDb.MarkSpentAndGetFee(tx.TxInList);
                error = null;
                return true;
            }

            if (tx.TxInList.Length == 0 || tx.TxOutList.Length == 0)
            {
                error = "Invalid number of inputs or outputs.";
                return false;
            }

            ulong toSpend = 0;

            for (int i = 0; i < tx.TxInList.Length; i++)
            {
                TxIn currentInput = tx.TxInList[i];
                IUtxo prevOutput = utxoDb.Find(currentInput);
                if (prevOutput is null)
                {
                    // TODO: add a ToString() method to TxIn?
                    error = $"Input {currentInput.TxHash.ToBase16()}:{currentInput.Index} was not found.";
                    return false;
                }
                toSpend += prevOutput.Amount;

                if (!prevOutput.PubScript.TryEvaluate(out IOperation[] pubOps, out int pubOpCount, out error))
                {
                    error = $"Invalid input transaction pubkey script." +
                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                            $"{Environment.NewLine}More info: {error}";
                    return false;
                }

                if (!currentInput.SigScript.TryEvaluate(out IOperation[] signatureOps, out int signatureOpCount, out error))
                {
                    error = $"Invalid transaction signature script." +
                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                            $"{Environment.NewLine}More info: {error}";
                    return false;
                }

                OpData stack = new OpData()
                {
                    Tx = tx,
                    TxInIndex = i,

                    ForceLowS = ForceLowS,
                    StrictNumberEncoding = ForceStrictPush,

                    IsBip65Enabled = consensus.IsBip65Enabled(BlockHeight),
                    IsBip112Enabled = consensus.IsBip112Enabled(BlockHeight),
                    IsStrictDerSig = consensus.IsStrictDerSig(BlockHeight),
                    IsStrictMultiSigGarbage = consensus.IsBip147Enabled(BlockHeight),
                };

                // TODO: add Is*Enabled bool to below GetSpecialType() method
                PubkeyScriptSpecialType pubType = prevOutput.PubScript.GetSpecialType();
                if (pubType == PubkeyScriptSpecialType.P2SH && !consensus.IsBip16Enabled(BlockHeight))
                {
                    pubType = PubkeyScriptSpecialType.None;
                }
                else if ((pubType == PubkeyScriptSpecialType.P2WPKH || pubType == PubkeyScriptSpecialType.P2WSH)
                         && !consensus.IsSegWitEnabled(BlockHeight))
                {
                    pubType = PubkeyScriptSpecialType.None;
                }
                else if (pubOps.Length == 2 && pubOps[0] is PushDataOp p1 && (p1.OpValue >= OP._1 && p1.OpValue <= OP._16) &&
                         pubOps[1] is PushDataOp)
                {
                    pubType = PubkeyScriptSpecialType.UnknownWitness;
                }

                if (pubType == PubkeyScriptSpecialType.None)
                {
                    TotalSigOpCount += prevOutput.PubScript.CountSigOps() + currentInput.SigScript.CountSigOps();

                    if ((tx.WitnessList != null && tx.WitnessList.Length != 0) &&
                        (tx.WitnessList[i].Items.Length != 0))
                    {
                        error = $"Unexpected witness." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    // Note that checking OP count is below max is done during Evaluate() and op.Run()
                    stack.ExecutingScript = signatureOps;
                    stack.OpCount = signatureOpCount;
                    foreach (var op in signatureOps)
                    {
                        if (!op.Run(stack, out error))
                        {
                            error = $"Script evaluation failed." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }
                    }

                    stack.ExecutingScript = pubOps;
                    stack.OpCount = pubOpCount;
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
                    if (signatureOps.Length == 0 || !(signatureOps[^1] is PushDataOp rdmPush))
                    {
                        error = "Redeem script was not found.";
                        return false;
                    }
                    else
                    {
                        RedeemScript redeem = new RedeemScript(rdmPush.data);
                        RedeemScriptSpecialType rdmType = redeem.GetSpecialType();
                        if (!redeem.TryEvaluate(out IOperation[] redeemOps, out int redeemOpCount, out error))
                        {
                            error = $"Script evaluation failed (invalid redeem script)." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }

                        stack.ExecutingScript = signatureOps;
                        stack.OpCount = signatureOpCount;
                        foreach (var op in signatureOps)
                        {
                            if (!op.Run(stack, out error))
                            {
                                error = $"Script evaluation failed." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }
                        }

                        stack.ExecutingScript = pubOps;
                        stack.OpCount = pubOpCount;
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

                        if (!new VerifyOp().Run(stack, out _))
                        {
                            error = $"Script evaluation failed (top stack item is false: redeem script hash is not the same)." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                            return false;
                        }

                        if (rdmType == RedeemScriptSpecialType.None)
                        {
                            if ((tx.WitnessList != null && tx.WitnessList.Length != 0) &&
                                (tx.WitnessList[i].Items.Length != 0))
                            {
                                error = $"Unexpected witness." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }

                            TotalSigOpCount += redeem.CountSigOps(redeemOps);

                            stack.ExecutingScript = redeemOps;
                            stack.OpCount = redeemOpCount;
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

                            TotalSigOpCount++;

                            if (!Signature.TryReadStrict(tx.WitnessList[i].Items[0].data, out Signature sig, out error))
                            {
                                error = $"Invalid signature encoding." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }
                            if (!PublicKey.TryRead(tx.WitnessList[i].Items[1].data, out PublicKey pub))
                            {
                                stack.Push(new byte[0]);
                            }
                            else
                            {
                                // Push pubkey
                                tx.WitnessList[i].Items[1].Run(stack, out _);
                                // Replace it with HASH160
                                new Hash160Op().Run(stack, out _);
                                // Push expected hash
                                redeemOps[1].Run(stack, out _);
                                // Check equality
                                if (!new EqualVerifyOp().Run(stack, out error))
                                {
                                    error = $"Script evaluation failed." +
                                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                            $"{Environment.NewLine}More info: {error}";
                                    return false;
                                }

                                byte[] spendScr = scrSer.ConvertP2wpkh(redeemOps);
                                byte[] dataToSign = tx.SerializeForSigningSegWit(spendScr, i, prevOutput.Amount, sig.SigHash);
                                bool b = calc.Verify(dataToSign, sig, pub, ForceLowS);
                                stack.Push(b);
                            }
                        }
                        else if (rdmType == RedeemScriptSpecialType.P2SH_P2WSH)
                        {
                            if (tx.WitnessList.Length != tx.TxInList.Length || tx.WitnessList[i].Items.Length < 1)
                            {
                                error = $"Mandatory witness is not found." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }

                            RedeemScript witRdm = new RedeemScript(tx.WitnessList[i].Items[^1].data);
                            if (!witRdm.TryEvaluate(out IOperation[] witRdmOps, out int witRdmOpCount, out error))
                            {
                                error = $"Script evaluation failed." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }

                            TotalSigOpCount += witRdm.CountSigOps(witRdmOps);

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

                            if (!redeemOps[^1].Run(stack, out error))
                            {
                                error = $"Script evaluation failed." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }

                            if (!new EqualVerifyOp().Run(stack, out error))
                            {
                                error = $"Script evaluation failed." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }

                            stack.AmountBeingSpent = prevOutput.Amount;
                            stack.IsSegWit = true;
                            stack.OpCount = witRdmOpCount;
                            stack.ExecutingScript = witRdmOps;

                            foreach (var op in witRdmOps)
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

                    TotalSigOpCount++;

                    if (!Signature.TryReadStrict(tx.WitnessList[i].Items[0].data, out Signature sig, out error))
                    {
                        error = $"Invalid signature encoding." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }
                    if (!PublicKey.TryRead(tx.WitnessList[i].Items[1].data, out PublicKey pub))
                    {
                        stack.Push(new byte[0]);
                    }
                    else
                    {
                        // Push pubkey
                        tx.WitnessList[i].Items[1].Run(stack, out _);
                        // Replace it with HASH160
                        new Hash160Op().Run(stack, out _);
                        // Push expected hash
                        pubOps[1].Run(stack, out _);
                        // Check equality
                        if (!new EqualVerifyOp().Run(stack, out error))
                        {
                            error = $"Script evaluation failed." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }

                        byte[] spendScr = scrSer.ConvertP2wpkh(pubOps);
                        byte[] dataToSign = tx.SerializeForSigningSegWit(spendScr, i, prevOutput.Amount, sig.SigHash);
                        bool b = calc.Verify(dataToSign, sig, pub, ForceLowS);
                        stack.Push(b);
                    }
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
                    if (!redeem.TryEvaluate(out IOperation[] redeemOps, out int redeemOpCount, out error))
                    {
                        error = $"Script evaluation failed." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    TotalSigOpCount += redeem.CountSigOps(redeemOps);

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

                    if (!pubOps[^1].Run(stack, out error))
                    {
                        error = $"Script evaluation failed." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    if (!new EqualVerifyOp().Run(stack, out error))
                    {
                        error = $"Script evaluation failed." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    stack.AmountBeingSpent = prevOutput.Amount;
                    stack.IsSegWit = true;
                    stack.OpCount = redeemOpCount;
                    stack.ExecutingScript = redeemOps;

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
                else if (pubType == PubkeyScriptSpecialType.UnknownWitness)
                {
                    foreach (var op in pubOps)
                    {
                        op.Run(stack, out _);
                    }
                    // VerifyOp will make sure top stack item is not "False"
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
                else if (!new VerifyOp().Run(stack, out _))
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

            ulong spent = 0;
            foreach (var tout in tx.TxOutList)
            {
                spent += tout.Amount;
            }
            ulong fee = toSpend - spent;
            if (fee < 0)
            {
                error = "Transaction is spending more than it can.";
                return false;
            }
            TotalFee += fee;

            error = null;
            return true;
        }
    }
}
