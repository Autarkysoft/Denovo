// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Implementation of a transaction verifier to validate all transactions in a block while counting all the signature operations
    /// updating state of the transaction inside memory pool and UTXO set.
    /// Implements <see cref="ITransactionVerifier"/>.
    /// </summary>
    public class TransactionVerifier : ITransactionVerifier, IDisposable
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
            hash160 = new Ripemd160Sha256();
            sha256 = new Sha256();
        }


        private readonly bool isMempool;
        private readonly EllipticCurveCalculator calc;
        private readonly ScriptSerializer scrSer;
        private readonly IUtxoDatabase utxoDb;
        private readonly IMemoryPool mempool;
        private readonly IConsensus consensus;
        private Ripemd160Sha256 hash160;
        private Sha256 sha256;


        /// <inheritdoc/>
        public int TotalSigOpCount { get; set; }
        /// <inheritdoc/>
        public ulong TotalFee { get; set; }
        /// <inheritdoc/>
        public bool AnySegWit { get; set; }
        /// <summary>
        /// If true will only accept low s values in signatures. This is a standard rule.
        /// </summary>
        public bool ForceLowS { get; set; }
        /// <summary>
        /// Returns if numbers inside scripts (or the popped data from stack to be converted to numbers)
        /// should be checked for strict and shortest encoding. This is a standard rule.
        /// </summary>
        public bool StrictNumberEncoding { get; set; }

        /// <inheritdoc/>
        public bool VerifyCoinbasePrimary(ITransaction coinbase, out string error)
        {
            if (coinbase.TxInList.Length != 1)
            {
                error = "Coinbase transaction must contain only one input.";
                return false;
            }
            if (coinbase.TxOutList.Length == 0)
            {
                error = "Transaction must contain at least one output.";
                return false;
            }

            var expInputHash = new ReadOnlySpan<byte>(new byte[32]);
            if (!expInputHash.SequenceEqual(coinbase.TxInList[0].TxHash) || coinbase.TxInList[0].Index != uint.MaxValue)
            {
                error = "Invalid coinbase outpoint.";
                return false;
            }
            if (!coinbase.TxInList[0].SigScript.VerifyCoinbase(consensus))
            {
                error = "Invalid coinbase signature script.";
                return false;
            }

            TotalSigOpCount += coinbase.TxInList[0].SigScript.CountSigOps() * Constants.WitnessScaleFactor;
            foreach (var tout in coinbase.TxOutList)
            {
                TotalSigOpCount += tout.PubScript.CountSigOps() * Constants.WitnessScaleFactor;
            }

            error = null;
            return true;
        }

        /// <inheritdoc/>
        public bool VerifyCoinbaseOutput(ITransaction coinbase, out string error)
        {
            ulong totalAmount = 0;
            ulong maxAllowed = consensus.BlockReward + TotalFee;
            foreach (var item in coinbase.TxOutList)
            {
                totalAmount += item.Amount;
                if (totalAmount > maxAllowed)
                {
                    error = "Coinbase generates more coins than it should.";
                    return false;
                }
            }

            error = null;
            return true;
        }


        private bool VerifyP2pkh(ITransaction tx, int index, PushDataOp sigPush, PushDataOp pubPush,
                                 ReadOnlySpan<byte> pubScrData, out string error)
        {
            var actualHash = hash160.ComputeHash(pubPush.data);
            if (!pubScrData.Slice(3, 20).SequenceEqual(actualHash))
            {
                error = "Invalid hash.";
                return false;
            }

            Signature sig;
            if (consensus.IsStrictDerSig)
            {
                if (!Signature.TryReadStrict(sigPush.data, out sig, out error))
                {
                    return false;
                }
            }
            else
            {
                if (!Signature.TryReadLoose(sigPush.data, out sig, out error))
                {
                    return false;
                }
            }

            if (!PublicKey.TryRead(pubPush.data, out PublicKey pubK))
            {
                error = "Invalid public key";
                return false;
            }

            byte[] toSign = tx.SerializeForSigning(pubScrData.ToArray(), index, sig.SigHash);
            if (calc.Verify(toSign, sig, pubK, ForceLowS))
            {
                error = null;
                return true;
            }
            else
            {
                error = "Invalid signature";
                return false;
            }
        }

        private bool VerifyP2wpkh(ITransaction tx, int index, PushDataOp sigPush, PushDataOp pubPush,
                                  ReadOnlySpan<byte> pubScrData, ulong amount, out string error)
        {
            if (sigPush.data == null || pubPush.data == null)
            {
                error = "Invalid data pushes in P2WPKH witness.";
                return false;
            }

            var actualHash = hash160.ComputeHash(pubPush.data);
            if (!pubScrData.Slice(2, 20).SequenceEqual(actualHash))
            {
                error = "Invalid hash.";
                return false;
            }

            if (!Signature.TryReadStrict(sigPush.data, out Signature sig, out error))
            {
                error = $"Invalid signature ({error})";
                return false;
            }

            if (!PublicKey.TryRead(pubPush.data, out PublicKey pubK))
            {
                error = "Invalid public key";
                return false;
            }

            byte[] toSign = tx.SerializeForSigningSegWit(scrSer.ConvertP2wpkh(actualHash), index, amount, sig.SigHash);
            if (calc.Verify(toSign, sig, pubK, ForceLowS))
            {
                error = null;
                return true;
            }
            else
            {
                error = "Invalid signature";
                return false;
            }
        }

        private bool VerifyP2wsh(ITransaction tx, int index, IRedeemScript redeem, ReadOnlySpan<byte> expectedHash,
                                 ulong amount, out string error)
        {
            ReadOnlySpan<byte> actualHash = sha256.ComputeHash(redeem.Data);
            if (!actualHash.SequenceEqual(expectedHash))
            {
                error = "Invalid hash.";
                return false;
            }

            if (!redeem.TryEvaluate(out IOperation[] redeemOps, out int redeemOpCount, out error))
            {
                error = $"Script evaluation failed." +
                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                        $"{Environment.NewLine}More info: {error}";
                return false;
            }

            // Note that there is no *Constants.WitnessScaleFactor here anymore
            TotalSigOpCount += redeem.CountSigOps(redeemOps);

            var stack = new OpData()
            {
                Tx = tx,
                TxInIndex = index,

                ForceLowS = ForceLowS,
                StrictNumberEncoding = StrictNumberEncoding,
                // TODO: change this to accept IConsensus
                IsBip65Enabled = consensus.IsBip65Enabled,
                IsBip112Enabled = consensus.IsBip112Enabled,
                IsStrictDerSig = consensus.IsStrictDerSig,
                IsBip147Enabled = consensus.IsBip147Enabled,
            };

            for (int j = 0; j < tx.WitnessList[index].Items.Length - 1; j++)
            {
                if (!tx.WitnessList[index].Items[j].Run(stack, out error))
                {
                    error = $"Script evaluation failed." +
                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                            $"{Environment.NewLine}More info: {error}";
                    return false;
                }
            }

            stack.AmountBeingSpent = amount;
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

            // Stack has to only have 1 item left
            if (stack.ItemCount == 1 && IsNotZero(stack.Pop()))
            {
                error = null;
                return true;
            }
            else
            {
                error = stack.ItemCount != 1 ?
                    "Stack has to have only 1 item after witness execution." :
                    "Top stack item is not true";
                return false;
            }
        }


        private bool IsNotZero(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    // Can be negative zero
                    if (i == data.Length - 1 && data[i] == 0x80)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        private bool CheckStack(OpData stack, out string error)
        {
            if (stack.ItemCount > 0 && IsNotZero(stack.Pop()))
            {
                error = null;
                return true;
            }
            else
            {
                error = stack.ItemCount == 0 ? "Emtpy stack" : "Top stack item is not true.";
                return false;
            }
        }

        /// <inheritdoc/>
        public bool Verify(ITransaction tx, out string error)
        {
            // If a tx is already in memory pool it must have been verified and be valid.
            // The SigOpCount property must be set by the caller (mempool dependency).
            if (!isMempool && mempool.Contains(tx))
            {
                // TODO: get the tx object from mempool the passed tx (from block) doesn't have any properties set
                TotalSigOpCount += tx.SigOpCount;
                ulong totalToSpend = 0;
                foreach (TxIn item in tx.TxInList)
                {
                    IUtxo utxo = utxoDb.Find(item);
                    // If the tx is valid and is in mempool the UTXOs must be available in UTXO database
                    Debug.Assert(!(utxo is null));
                    Debug.Assert(utxo.IsMempoolSpent);
                    Debug.Assert(!utxo.IsBlockSpent);

                    totalToSpend += utxo.Amount;
                    utxo.IsBlockSpent = true;
                }

                ulong totalSpent = 0;
                foreach (TxOut item in tx.TxOutList)
                {
                    totalSpent += item.Amount;
                }

                TotalFee += (totalToSpend - totalSpent);


                if (!AnySegWit)
                {
                    AnySegWit = tx.WitnessList != null;
                }
                error = null;
                return true;
            }

            // TODO: these 2 checks should be performed during creation of tx (ctor or Deserialize)
            if (tx.TxInList.Length == 0 || tx.TxOutList.Length == 0)
            {
                error = "Invalid number of inputs or outputs.";
                return false;
            }

            ulong toSpend = 0;

            for (int i = 0; i < tx.TxInList.Length; i++)
            {
                TxIn currentInput = tx.TxInList[i];
                // TODO: add a condition in UTXO for when it is a coinbase transaction (they are not spendable if haven't 
                // reached maturity ie. 100 blocks -> thisHeight - spendingCoinbaseHeight >= 100)
                IUtxo prevOutput = utxoDb.Find(currentInput);
                if (prevOutput is null)
                {
                    // TODO: add a ToString() method to TxIn?
                    error = $"Input {currentInput.TxHash.ToBase16()}:{currentInput.Index} was not found.";
                    return false;
                }

                if (isMempool && prevOutput.IsMempoolSpent)
                {
                    // TODO: verify but have to decide if this is a double spend or RBF and if we want to accept/replace it in mempool
                }
                else if (!isMempool && prevOutput.IsBlockSpent)
                {
                    error = "Output is already spent in a previous transaction in this block.";
                    return false;
                }

                toSpend += prevOutput.Amount;

                PubkeyScriptSpecialType pubType = prevOutput.PubScript.GetSpecialType(consensus);

                if (pubType == PubkeyScriptSpecialType.None || pubType == PubkeyScriptSpecialType.P2PKH)
                {
                    // If the type is not witness there shouldn't be any witness item
                    if (tx.WitnessList != null && tx.WitnessList.Length != 0 && tx.WitnessList[i].Items.Length != 0)
                    {
                        error = $"Unexpected witness." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                        return false;
                    }

                    if (!currentInput.SigScript.TryEvaluate(out IOperation[] signatureOps, out int signatureOpCount, out error))
                    {
                        error = $"Invalid transaction signature script." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    // P2PKH is not a special type so the signature can contain extra OPs (eg. PushData() OP_DROP <sig> <pub>).
                    // The optimization only works when the signature script is standard (Push<sig> Push<pub>).
                    // The optimization doesn't need to use OpData, evaluate PubkeyScript, convert PubkeyScript (FindAndDelete),
                    // run the operations, count Ops, count sigOps, check for stack item overflow,
                    // or check stack after execution.
                    if (pubType == PubkeyScriptSpecialType.P2PKH && signatureOps.Length == 2 &&
                        signatureOps[0] is PushDataOp sigPush && sigPush.data != null &&
                        signatureOps[1] is PushDataOp pubPush && pubPush.data != null)
                    {
                        if (!VerifyP2pkh(tx, i, sigPush, pubPush, prevOutput.PubScript.Data, out error))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        TotalSigOpCount += currentInput.SigScript.CountSigOps() * Constants.WitnessScaleFactor;

                        if (!prevOutput.PubScript.TryEvaluate(out IOperation[] pubOps, out int pubOpCount, out error))
                        {
                            error = $"Invalid input transaction pubkey script." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }

                        var stack = new OpData()
                        {
                            Tx = tx,
                            TxInIndex = i,

                            ForceLowS = ForceLowS,
                            StrictNumberEncoding = StrictNumberEncoding,

                            IsBip65Enabled = consensus.IsBip65Enabled,
                            IsBip112Enabled = consensus.IsBip112Enabled,
                            IsStrictDerSig = consensus.IsStrictDerSig,
                            IsBip147Enabled = consensus.IsBip147Enabled,
                        };

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

                        if (!CheckStack(stack, out error))
                        {
                            return false;
                        }
                    }
                }
                else if (pubType == PubkeyScriptSpecialType.P2SH)
                {
                    // P2SH signature script is all pushes so there is no need to count OPs in it for running which changes with
                    // OP_CheckMultiSig(Verify) Ops only (the normal count is already checked during evaluation)
                    if (!currentInput.SigScript.TryEvaluate(out IOperation[] signatureOps, out _, out error))
                    {
                        error = $"Invalid transaction signature script." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {error}";
                        return false;
                    }

                    // Note that the top stack item after running signature script can not be result of OP_num push
                    // eg. sig pushes OP_2 to stack, top stack item is now 2
                    // Redeem script reading 2 expects a push of 2 bytes and fails
                    if (signatureOps.Length == 0 || !(signatureOps[^1] is PushDataOp rdmPush) || rdmPush.data == null)
                    {
                        error = "Redeem script was not found.";
                        return false;
                    }

                    RedeemScript redeem = new RedeemScript(rdmPush.data);

                    ReadOnlySpan<byte> actualHash = hash160.ComputeHash(redeem.Data);
                    ReadOnlySpan<byte> expectedHash = ((ReadOnlySpan<byte>)prevOutput.PubScript.Data).Slice(2, 20);
                    if (!actualHash.SequenceEqual(expectedHash))
                    {
                        error = "Invalid hash.";
                        return false;
                    }

                    RedeemScriptSpecialType rdmType = redeem.GetSpecialType(consensus);

                    if (rdmType == RedeemScriptSpecialType.None)
                    {
                        if (tx.WitnessList != null && tx.WitnessList.Length != 0 && tx.WitnessList[i].Items.Length != 0)
                        {
                            error = $"Unexpected witness." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }

                        var stack = new OpData()
                        {
                            Tx = tx,
                            TxInIndex = i,

                            ForceLowS = ForceLowS,
                            StrictNumberEncoding = StrictNumberEncoding,

                            IsBip65Enabled = consensus.IsBip65Enabled,
                            IsBip112Enabled = consensus.IsBip112Enabled,
                            IsStrictDerSig = consensus.IsStrictDerSig,
                            IsBip147Enabled = consensus.IsBip147Enabled,
                        };

                        // There is no need to set the following 2 since all sigOps are PushOps
                        //stack.ExecutingScript = signatureOps;
                        //stack.OpCount = signatureOpCount;

                        // There is no need to check or run the last Op, it is the redeem push which is checked and hashed
                        for (int j = 0; j < signatureOps.Length - 1; j++)
                        {
                            IOperation op = signatureOps[j];
                            // Signature script of P2SH must be push-only
                            if (!(signatureOps[j] is PushDataOp) || !op.Run(stack, out error))
                            {
                                error = $"Script evaluation failed." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {error}";
                                return false;
                            }
                        }

                        // There is no need to run pubOps

                        if (!redeem.TryEvaluate(out IOperation[] redeemOps, out int redeemOpCount, out error))
                        {
                            error = $"Script evaluation failed (invalid redeem script)." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {error}";
                            return false;
                        }

                        TotalSigOpCount += redeem.CountSigOps(redeemOps) * Constants.WitnessScaleFactor;

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

                        if (!CheckStack(stack, out error))
                        {
                            return false;
                        }
                    }
                    else if (rdmType == RedeemScriptSpecialType.P2SH_P2WPKH)
                    {
                        AnySegWit = true;
                        if (tx.WitnessList == null ||
                            tx.WitnessList.Length != tx.TxInList.Length ||
                            tx.WitnessList[i].Items.Length != 2)
                        {
                            error = $"Mandatory witness is not found." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                            return false;
                        }

                        TotalSigOpCount++;
                        if (!VerifyP2wpkh(tx, i, tx.WitnessList[i].Items[0], tx.WitnessList[i].Items[1],
                            redeem.Data, prevOutput.Amount, out error))
                        {
                            return false;
                        }
                    }
                    else if (rdmType == RedeemScriptSpecialType.P2SH_P2WSH)
                    {
                        AnySegWit = true;
                        if (tx.WitnessList == null ||
                            tx.WitnessList.Length != tx.TxInList.Length ||
                            tx.WitnessList[i].Items.Length < 1 ||
                            tx.WitnessList[i].Items[^1].data == null)
                        {
                            error = $"Mandatory witness is not found." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                            return false;
                        }

                        RedeemScript witRdm = new RedeemScript(tx.WitnessList[i].Items[^1].data);
                        expectedHash = ((ReadOnlySpan<byte>)redeem.Data).Slice(2);
                        if (!VerifyP2wsh(tx, i, witRdm, expectedHash, prevOutput.Amount, out error))
                        {
                            return false;
                        }
                    }
                    else if (rdmType == RedeemScriptSpecialType.InvalidWitness)
                    {
                        error = "Invalid witness pubkey script was found.";
                        return false;
                    }
                    else if (rdmType == RedeemScriptSpecialType.UnknownWitness)
                    {
                        // The unknown witness versions must have an empty signature script but there is no checking the IWitness
                        if (currentInput.SigScript.Data.Length != 0)
                        {
                            error = "Non-empty signature script for witness (unknown version).";
                            return false;
                        }

                        AnySegWit = true;

                        // We already know that PubkeyScript is in form of OP_num PushData(>=2 && <=40 bytes)
                        // we only have to make sure the data is not all zeros
                        ReadOnlySpan<byte> thePush = ((ReadOnlySpan<byte>)prevOutput.PubScript.Data).Slice(2);
                        if (thePush.SequenceEqual(new byte[thePush.Length]))
                        {
                            error = "Witness program can not be all zeros.";
                            return false;
                        }
                    }
                }
                else if (pubType == PubkeyScriptSpecialType.P2WPKH)
                {
                    AnySegWit = true;
                    TotalSigOpCount++;

                    if (tx.WitnessList == null ||
                        tx.WitnessList.Length != tx.TxInList.Length ||
                        tx.WitnessList[i].Items.Length != 2)
                    {
                        error = $"Mandatory witness is not found." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                        return false;
                    }
                    if (currentInput.SigScript.Data.Length != 0)
                    {
                        error = "Signature script must be empty for spending witness outputs.";
                        return false;
                    }

                    // This optimization doesn't need to use OpData, evaluate PubkeyScript, evaluate signature script,
                    // run the operations, count Ops, count sigOps, check for stack item overflow,
                    // or check stack after execution.
                    if (!VerifyP2wpkh(tx, i, tx.WitnessList[i].Items[0], tx.WitnessList[i].Items[1],
                        prevOutput.PubScript.Data, prevOutput.Amount, out error))
                    {
                        return false;
                    }
                }
                else if (pubType == PubkeyScriptSpecialType.P2WSH)
                {
                    AnySegWit = true;
                    if (tx.WitnessList == null ||
                        tx.WitnessList.Length != tx.TxInList.Length ||
                        tx.WitnessList[i].Items.Length < 1 || // At least 1 item is needed as the redeem script
                        tx.WitnessList[i].Items[^1].data == null) // That 1 item can not be a OP_num
                    {
                        error = $"Mandatory witness is not found." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                        return false;
                    }
                    if (currentInput.SigScript.Data.Length != 0)
                    {
                        error = "Signature script must be empty for spending witness outputs.";
                        return false;
                    }

                    RedeemScript redeem = new RedeemScript(tx.WitnessList[i].Items[^1].data);
                    ReadOnlySpan<byte> expectedHash = ((ReadOnlySpan<byte>)prevOutput.PubScript.Data).Slice(2);
                    if (!VerifyP2wsh(tx, i, redeem, expectedHash, prevOutput.Amount, out error))
                    {
                        return false;
                    }
                }
                else if (pubType == PubkeyScriptSpecialType.UnknownWitness)
                {
                    // The unknown witness versions must have an empty signature script but there is no checking the IWitness
                    if (currentInput.SigScript.Data.Length != 0)
                    {
                        error = "Non-empty signature script for witness (unknown version).";
                        return false;
                    }

                    AnySegWit = true;

                    // We already know that PubkeyScript is in form of OP_num PushData(>=2 && <=40 bytes)
                    // we only have to make sure the data is not all zeros
                    ReadOnlySpan<byte> thePush = ((ReadOnlySpan<byte>)prevOutput.PubScript.Data).Slice(2);
                    if (thePush.SequenceEqual(new byte[thePush.Length]))
                    {
                        error = "Witness program can not be all zeros.";
                        return false;
                    }
                }
                else if (pubType == PubkeyScriptSpecialType.InvalidWitness)
                {
                    error = "Invalid witness pubkey script was found.";
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
                TotalSigOpCount += tout.PubScript.CountSigOps() * Constants.WitnessScaleFactor;
            }

            if (spent > toSpend)
            {
                error = "Transaction is spending more than it can.";
                return false;
            }
            TotalFee += toSpend - spent;

            error = null;
            return true;
        }


        private bool isDisposed;

        /// <summary>
        /// Releases the resources used by this instance.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (!(hash160 is null))
                        hash160.Dispose();
                    hash160 = null;

                    if (!(sha256 is null))
                        sha256.Dispose();
                    sha256 = null;
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose() => Dispose(true);
    }
}
