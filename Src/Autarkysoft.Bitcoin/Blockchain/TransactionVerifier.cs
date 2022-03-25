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
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Blockchain
{
    // TODO: possible bug: P2WSH (witness version 0) witness items don't have the stack item count limitation
    //                     (reject > Constants.MaxScriptStackItemCount). It looks like "witness stack" can have 1001 items
    //                     and be valid. When PushDataOP objects used in IWitness are run they will fail on 1001 items.
    //                     This needs to be tested.
    //                     A solution could be changing IWitness to hold byte[][] instead and check the item count limit
    //                     after running each OP (in the loop) and perform an initial item length check for witness version 1.
    //                     Reminder: either way each byte[] must be still smaller than Constants.MaxScriptItemLength
    //                     https://github.com/bitcoin/bitcoin/blob/04437ee721e66a7b76bef5ec2f88dd1efcd03b84/src/script/interpreter.cpp#L1832-L1833



    /// <summary>
    /// Implementation of a transaction verifier to validate all transactions in a block while counting all the signature operations
    /// updating state of the transaction inside memory pool and UTXO set.
    /// Implements <see cref="ITransactionVerifier"/>.
    /// </summary>
    public class TransactionVerifier : ITransactionVerifier, IDisposable
    {
        /// <summary>
        /// A simplified constructor used for testing. Skips setting <see cref="UtxoDb"/> and <see cref="mempool"/>.
        /// </summary>
        /// <param name="consensus">Consensus rules to use</param>
        public TransactionVerifier(IConsensus consensus)
        {
            this.consensus = consensus;

            calc = new EllipticCurveCalculator();
            scrSer = new ScriptSerializer();
            hash160 = new Ripemd160Sha256();
            sha256 = new Sha256();
        }

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
            UtxoDb = utxoDatabase;
            mempool = memoryPool;
            this.consensus = consensus;

            localDb = new Dictionary<byte[], List<Utxo>>(new ByteArrayComparer());
            calc = new EllipticCurveCalculator();
            scrSer = new ScriptSerializer();
            hash160 = new Ripemd160Sha256();
            sha256 = new Sha256();
        }


        private readonly bool isMempool;
        private readonly Dictionary<byte[], List<Utxo>> localDb;
        private readonly EllipticCurveCalculator calc;
        private readonly ScriptSerializer scrSer;
        private readonly IMemoryPool mempool;
        private readonly IConsensus consensus;
        private Ripemd160Sha256 hash160;
        private Sha256 sha256;

        private const byte TaprootLeafMask = 0xfe;
        private const byte TaprootLeafTapscript = 0xc0;
        private const byte AnnexTag = 0x50;
        private const int TaprootControlBaseSize = 33;
        private const int TaprootControlNodeSize = 32;
        private const int TaprootControlMaxNodeCount = 128;
        private const int TaprootControlMaxSize = TaprootControlBaseSize + TaprootControlNodeSize * TaprootControlMaxNodeCount;
        private const byte TapLeafMask = 0xfe;

        /// <inheritdoc/>
        public IUtxoDatabase UtxoDb { get; }
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
        /// <summary>
        /// If true only strict encoding of true/false is accepted for conditional operations.
        /// This is a standard rule for legacy and witness version 0, and a consensus rule for Taproot scripts.
        /// </summary>
        public bool IsStrictConditionalOpBool { get; set; } = false;


        /// <inheritdoc/>
        public void Init()
        {
            TotalSigOpCount = 0;
            TotalFee = 0;
            AnySegWit = false;
            localDb.Clear();
        }

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

            if (!((ReadOnlySpan<byte>)coinbase.TxInList[0].TxHash).SequenceEqual(ZeroBytes.B32) ||
                coinbase.TxInList[0].Index != uint.MaxValue)
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

            AnySegWit = coinbase.WitnessList != null && coinbase.WitnessList.Length > 0;

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
            byte[] actualHash = hash160.ComputeHash(pubPush.data);
            if (!pubScrData.Slice(3, 20).SequenceEqual(actualHash))
            {
                error = $"Invalid hash (OP_EqualVerify failed for P2PKH input at index={index}).";
                return false;
            }

            Signature sig;
            if (consensus.IsStrictDerSig)
            {
                if (!Signature.TryReadStrict(sigPush.data, out sig, out Errors er))
                {
                    error = $"{er.Convert()}{Environment.NewLine}" +
                            $"(OP_CheckSig failed to read signature with strict rules for input at index={index}.";
                    return false;
                }
            }
            else
            {
                if (!Signature.TryReadLoose(sigPush.data, out sig, out Errors er))
                {
                    error = $"{er.Convert()}{Environment.NewLine}" +
                            $"(OP_CheckSig failed to read signature with loose rules for input at index={index}.";
                    return false;
                }
            }

            if (!PublicKey.TryRead(pubPush.data, out PublicKey pubK))
            {
                error = $"OP_CheckSig failed due to invalid public key for input at index={index}.";
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
                error = $"OP_CheckSig failed due to invalid ECDSA signature for input at index={index}.";
                return false;
            }
        }

        // TODO: change the 2 byte[] to ReadOnlySpan<byte> (it needs changing Signature and PublicKey classes)
        private bool VerifyP2wpkh(ITransaction tx, int index, byte[] sigPush, byte[] pubPush,
                                  ReadOnlySpan<byte> pubScrData, ulong amount, out string error)
        {
            if (sigPush == null || pubPush == null)
            {
                error = "Signature or public key in P2WPKH witness can not be null.";
                return false;
            }

            ReadOnlySpan<byte> program = pubScrData.Slice(2, 20);
            if (!IsNotZero20(program))
            {
                error = Err.ZeroByteWitness;
                return false;
            }

            byte[] actualHash = hash160.ComputeHash(pubPush);
            if (!program.SequenceEqual(actualHash))
            {
                error = $"Invalid hash (OP_EqualVerify failed for P2WPKH input at index={index}).";
                return false;
            }

            if (!Signature.TryReadStrict(sigPush, out Signature sig, out Errors er))
            {
                error = $"{er.Convert()}{Environment.NewLine}(OP_CheckSig failed to read signature with strict rules" +
                        $" for input at index={index}.";
                return false;
            }

            if (!PublicKey.TryRead(pubPush, out PublicKey pubK))
            {
                error = $"OP_CheckSig failed due to invalid public key for input at index={index}.";
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
                error = $"OP_CheckSig failed due to invalid ECDSA signature for input at index={index}.";
                return false;
            }
        }

        private bool VerifyP2wsh(ITransaction tx, int index, IRedeemScript redeem, ReadOnlySpan<byte> pubScrData,
                                 ulong amount, out string error)
        {
            Debug.Assert(pubScrData.Length == 34);
            ReadOnlySpan<byte> program = pubScrData.Slice(2, 32);
            if (!IsNotZero32(program))
            {
                error = Err.ZeroByteWitness;
                return false;
            }

            ReadOnlySpan<byte> actualHash = sha256.ComputeHash(redeem.Data);
            if (!actualHash.SequenceEqual(program))
            {
                error = "Invalid hash.";
                return false;
            }

            if (!redeem.TryEvaluate(ScriptEvalMode.WitnessV0, out IOperation[] redeemOps, out int redeemOpCount, out Errors e))
            {
                error = $"Redeem script evaluation failed for input at index={index}." +
                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                        $"{Environment.NewLine}More info: {e.Convert()}";
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
                IsStrictConditionalOpBool = IsStrictConditionalOpBool,
            };

            // Note that the top witness item (redeem bytes) is not subject to MaxScriptItemLength check
            for (int j = 0; j < tx.WitnessList[index].Items.Length - 1; j++)
            {
                if (tx.WitnessList[index].Items[j].Length > Constants.MaxScriptItemLength)
                {
                    error = $"Witness items can not be bigger than {Constants.MaxScriptItemLength} bytes.";
                    return false;
                }
                stack.Push(tx.WitnessList[index].Items[j]);
            }

            stack.AmountBeingSpent = amount;
            stack.IsSegWit = true;
            stack.OpCount = redeemOpCount;
            stack.ExecutingScript = redeemOps;

            foreach (var op in redeemOps)
            {
                if (!op.Run(stack, out Errors er))
                {
                    error = $"Script evaluation failed on {op.OpValue} for input at index={index}." +
                            $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                            $"{Environment.NewLine}More info: {er.Convert()}";
                    return false;
                }
            }

            return CheckWitnessStack(stack, out error);
        }


        private bool VerifyTaprootCommitment(ReadOnlySpan<byte> control, ReadOnlySpan<byte> program, ReadOnlySpan<byte> k)
        {
            // Internal pubkey
            PublicKey.PublicKeyType ptype = PublicKey.TryReadTaproot(control.Slice(1, 32), out PublicKey p);
            if (ptype == PublicKey.PublicKeyType.None || ptype == PublicKey.PublicKeyType.Unknown)
            {
                return false;
            }

            // Output pubkey (taken from the PubKey script)
            PublicKey.PublicKeyType qtype = PublicKey.TryReadTaproot(program, out PublicKey q);
            if (qtype == PublicKey.PublicKeyType.None || qtype == PublicKey.PublicKeyType.Unknown)
            {
                return false;
            }

            int pathLen = (control.Length - TaprootControlBaseSize) / TaprootControlNodeSize;
            // Compute the Merkle root from the leaf and the provided path.
            for (int i = 0; i < pathLen; ++i)
            {
                // e is each inner node
                ReadOnlySpan<byte> e = control.Slice(TaprootControlBaseSize + TaprootControlNodeSize * i, TaprootControlNodeSize);
                if (k.SequenceCompareTo(e) < 0) // k < e
                {
                    k = sha256.ComputeTaggedHash_TapBranch(k, e);
                }
                else // k >= e
                {
                    k = sha256.ComputeTaggedHash_TapBranch(e, k);
                }
            }

            // Verify that the output pubkey matches the tweaked internal pubkey, after correcting for parity.
            byte[] t = sha256.ComputeTaggedHash_TapTweak(control.Slice(1, 32), k);
            BigInteger tInt = t.ToBigInt(true, true);
            if (tInt >= BigInteger.Parse("115792089237316195423570985008687907852837564279074904382605163141518161494337"))
            {
                return false;
            }

            EllipticCurvePoint Q = calc.AddChecked(calc.MultiplyByG(tInt), p.ToPoint());
            if (q.ToPoint().X != Q.X || (control[0] & 1) != Q.Y % 2)
            {
                return false;
            }

            return true;
        }


        internal static bool IsNotZero20(ReadOnlySpan<byte> data)
        {
            Debug.Assert(data.Length == 20);
            return !data.SequenceEqual(ZeroBytes.B20) && !data.SequenceEqual(ZeroBytes.B20N);
        }

        internal static bool IsNotZero32(ReadOnlySpan<byte> data)
        {
            Debug.Assert(data.Length == 32);
            return !data.SequenceEqual(ZeroBytes.B32) && !data.SequenceEqual(ZeroBytes.B32N);
        }

        internal static bool IsNotZero(ReadOnlySpan<byte> data)
        {
            // https://github.com/bitcoin/bitcoin/blob/04437ee721e66a7b76bef5ec2f88dd1efcd03b84/src/script/interpreter.cpp#L35-L48
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
            // https://github.com/bitcoin/bitcoin/blob/04437ee721e66a7b76bef5ec2f88dd1efcd03b84/src/script/interpreter.cpp#L1994-L1997
            if (stack.ItemCount == 0)
            {
                error = "Emtpy stack";
                return false;
            }
            else if (IsNotZero(stack.Pop()))
            {
                error = null;
                return true;
            }
            else
            {
                error = "Top stack item is not true.";
                return false;
            }
        }

        private bool CheckWitnessStack(OpData stack, out string error)
        {
            // https://github.com/bitcoin/bitcoin/blob/57982f419e36d0023c83af2dd0d683ca3160dc2a/src/script/interpreter.cpp#L1845-L1846
            // Stack must only have 1 item left
            if (stack.ItemCount == 1)
            {
                if (IsNotZero(stack.Pop()))
                {
                    error = null;
                    return true;
                }
                else
                {
                    error = "Top stack item is not true.";
                    return false;
                }
            }
            else
            {
                error = "Stack must only have 1 item after witness execution.";
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
                    IUtxo utxo = UtxoDb.Find(item);
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

            var utxos = new IUtxo[tx.TxInList.Length];
            for (int i = 0; i < utxos.Length; i++)
            {
                IUtxo temp = UtxoDb.Find(tx.TxInList[i]);
                if (temp is null && localDb.TryGetValue(tx.TxInList[i].TxHash, out List<Utxo> value))
                {
                    var index = value.FindIndex(x => x.Index == tx.TxInList[i].Index);
                    temp = index < 0 ? null : value[index];
                }
                utxos[i] = temp;
            }

            return Verify(tx, utxos, out error);
        }

        /// <inheritdoc/>
        public bool Verify(ITransaction tx, IUtxo[] utxos, out string error)
        {
            if (utxos is null || tx.TxInList.Length != utxos.Length)
            {
                error = "Invalid number of UTXOs.";
                return false;
            }

            // TODO: these 2 checks should be performed during creation of tx (ctor or Deserialize)
            if (tx.TxInList.Length == 0 || tx.TxOutList.Length == 0)
            {
                error = "Invalid number of inputs or outputs.";
                return false;
            }

            // TODO: this seems like a redundant check since we already have checks in tx.TryDeserialize method
            // https://github.com/bitcoin/bitcoin/blob/e1e1e708fa0fbc0c51460305da5d401ed8f218f3/src/consensus/tx_check.cpp#L18-L19
            if (tx.Weight > Constants.MaxBlockWeight)
            {
                error = "Transaction is too big.";
                return false;
            }

            ulong toSpend = 0;

            for (int i = 0; i < tx.TxInList.Length; i++)
            {
                TxIn currentInput = tx.TxInList[i];
                IUtxo prevOutput = utxos[i];
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
                    if (tx.WitnessList != null &&
                        tx.WitnessList.Length != 0 && tx.WitnessList.Length != tx.TxInList.Length ||
                        (tx.WitnessList != null && tx.WitnessList[i].Items.Length != 0))
                    {
                        error = $"Unexpected witness for input at index={i}." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                        return false;
                    }

                    if (!currentInput.SigScript.TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] signatureOps,
                                                            out int signatureOpCount, out Errors e))
                    {
                        error = $"Invalid transaction signature script for input at index={i}." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {e.Convert()}";
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

                        if (!prevOutput.PubScript.TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] pubOps,
                                                              out int pubOpCount, out e))
                        {
                            error = $"Failed to evaluate given pubkey script for input at index={i}." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {e.Convert()}";
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
                            IsStrictConditionalOpBool = IsStrictConditionalOpBool,
                        };

                        // Note that checking OP count is below max is done during Evaluate() and op.Run()
                        stack.ExecutingScript = signatureOps;
                        stack.OpCount = signatureOpCount;
                        foreach (var op in signatureOps)
                        {
                            if (!op.Run(stack, out Errors er))
                            {
                                error = $"Script evaluation failed on {op.OpValue} for input at index={i}." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {er.Convert()}";
                                return false;
                            }
                        }

                        stack.ExecutingScript = pubOps;
                        stack.OpCount = pubOpCount;
                        foreach (var op in pubOps)
                        {
                            if (!op.Run(stack, out Errors er))
                            {
                                error = $"Script evaluation failed on {op.OpValue} for input at index={i}." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {er.Convert()}";
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
                    if (!currentInput.SigScript.TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] signatureOps, out _, out Errors e))
                    {
                        error = $"Failed to evaluate signature script for input at index={i}." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                $"{Environment.NewLine}More info: {e.Convert()}";
                        return false;
                    }

                    // Note that the top stack item after running signature script can not be result of OP_num push
                    // eg. sig pushes OP_2 to stack, top stack item is now 2
                    // Redeem script reading 2 expects a push of 2 bytes and fails
                    // This however works fine with OP_0 since redeem script reading empty array works fine
                    if (signatureOps.Length == 0 || !(signatureOps[^1] is PushDataOp rdmPush) || rdmPush.data == null
                                                                                              && rdmPush.OpValue != OP._0)
                    {
                        error = "Redeem script was not found.";
                        return false;
                    }

                    var redeem = new RedeemScript(rdmPush.data);

                    ReadOnlySpan<byte> actualHash = hash160.ComputeHash(redeem.Data);
                    ReadOnlySpan<byte> expectedHash = ((ReadOnlySpan<byte>)prevOutput.PubScript.Data).Slice(2, 20);
                    if (!actualHash.SequenceEqual(expectedHash))
                    {
                        error = $"Invalid hash (OP_Equal failed for P2SH input at index={i}).";
                        return false;
                    }

                    RedeemScriptSpecialType rdmType = redeem.GetSpecialType(consensus);

                    if (rdmType == RedeemScriptSpecialType.None)
                    {
                        if (tx.WitnessList != null &&
                            tx.WitnessList.Length != 0 && tx.WitnessList.Length != tx.TxInList.Length &&
                            tx.WitnessList[i].Items.Length != 0)
                        {
                            error = $"Unexpected witness for input at index={i}." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
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
                            IsStrictConditionalOpBool = IsStrictConditionalOpBool,
                        };

                        // There is no need to set the following 2 since all sigOps are PushOps
                        //stack.ExecutingScript = signatureOps;
                        //stack.OpCount = signatureOpCount;

                        // There is no need to check or run the last Op, it is the redeem push which is checked and hashed
                        for (int j = 0; j < signatureOps.Length - 1; j++)
                        {
                            IOperation op = signatureOps[j];
                            // Signature script of P2SH must be push-only
                            Errors er = Errors.None;
                            if (!(signatureOps[j] is PushDataOp) || !op.Run(stack, out er))
                            {
                                error = $"Script evaluation failed on {op.OpValue} for input at index={i} ." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {er.Convert()}";
                                return false;
                            }
                        }

                        // There is no need to run pubOps

                        if (!redeem.TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] redeemOps, out int redeemOpCount,
                                                out e))
                        {
                            error = $"Script evaluation failed (invalid redeem script)." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                    $"{Environment.NewLine}More info: {e.Convert()}";
                            return false;
                        }

                        TotalSigOpCount += redeem.CountSigOps(redeemOps) * Constants.WitnessScaleFactor;

                        stack.ExecutingScript = redeemOps;
                        stack.OpCount = redeemOpCount;
                        foreach (var op in redeemOps)
                        {
                            if (!op.Run(stack, out Errors er))
                            {
                                error = $"Script evaluation failed on {op.OpValue} for input at index={i} ." +
                                        $"{Environment.NewLine}TxId: {tx.GetTransactionId()}" +
                                        $"{Environment.NewLine}More info: {er.Convert()}";
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

                        if (signatureOps.Length != 1)
                        {
                            error = "Signature script of P2SH-P2WPKH must contain the redeem script only.";
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
                        // TODO: write a test for empty redeem script (byte[0])
                        AnySegWit = true;
                        if (tx.WitnessList == null ||
                            tx.WitnessList.Length != tx.TxInList.Length ||
                            tx.WitnessList[i].Items.Length < 1)
                        {
                            error = $"Mandatory witness is not found." +
                                    $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                            return false;
                        }

                        if (signatureOps.Length != 1)
                        {
                            error = "Signature script of P2SH-P2WSH must contain the redeem script only.";
                            return false;
                        }

                        RedeemScript witRdm = new RedeemScript(tx.WitnessList[i].Items[^1]);
                        if (!VerifyP2wsh(tx, i, witRdm, redeem.Data, prevOutput.Amount, out error))
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
                        ReadOnlySpan<byte> program = ((ReadOnlySpan<byte>)prevOutput.PubScript.Data).Slice(2);
                        if (!IsNotZero(program))
                        {
                            error = Err.ZeroByteWitness;
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
                    // TODO: write a test for empty redeem script (byte[0])
                    if (tx.WitnessList == null ||
                        tx.WitnessList.Length != tx.TxInList.Length ||
                        tx.WitnessList[i].Items.Length < 1) // At least 1 item is needed as the redeem script
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

                    RedeemScript redeem = new RedeemScript(tx.WitnessList[i].Items[^1]);
                    if (!VerifyP2wsh(tx, i, redeem, prevOutput.PubScript.Data, prevOutput.Amount, out error))
                    {
                        return false;
                    }
                }
                else if (pubType == PubkeyScriptSpecialType.P2TR)
                {
                    // If PubkeyScript is returning this type the fork must be active; otherwise it should return UnknownWitness
                    Debug.Assert(consensus.IsTaprootEnabled);

                    AnySegWit = true;
                    if (currentInput.SigScript.Data.Length != 0)
                    {
                        error = "Non-empty signature script for witness versoion 1.";
                        return false;
                    }
                    if (tx.WitnessList == null ||
                        tx.WitnessList.Length != tx.TxInList.Length ||
                        tx.WitnessList[i].Items.Length < 1) // At least 1 item is needed
                    {
                        error = $"Mandatory witness is not found." +
                                $"{Environment.NewLine}TxId: {tx.GetTransactionId()}";
                        return false;
                    }

                    // This is our stack size too since signature script is empty (pubkey script is already checked and 
                    // removed from stack)
                    int witItemCount = tx.WitnessList[i].Items.Length;

                    byte[] annexHash = null;
                    if (witItemCount >= 2 &&
                        tx.WitnessList[i].Items[^1].Length > 0 &&
                        tx.WitnessList[i].Items[^1][0] == AnnexTag)
                    {
                        var tempStream = new FastStream(tx.WitnessList[i].Items[^1].Length + 2);
                        tempStream.WriteWithCompactIntLength(tx.WitnessList[i].Items[^1]);
                        annexHash = sha256.ComputeHash(tempStream.ToByteArray());
                        // Annex is removed from stack (we don't remove it from witness items, just change the loop)
                        witItemCount--;
                    }

                    Debug.Assert(prevOutput.PubScript.Data.Length == 34); // PubkeyScript has to have checked it
                    var program = ((ReadOnlySpan<byte>)prevOutput.PubScript.Data).Slice(2, 32);

                    if (witItemCount == 1)
                    {
                        // Key path spending:
                        // The witness item is a signature
                        if (!Signature.TryReadSchnorr(tx.WitnessList[i].Items[0], out Signature sig, out Errors er))
                        {
                            error = er.Convert();
                            return false;
                        }
                        if (sig.SigHash.ToOutputType() == SigHashType.Single && i >= tx.TxOutList.Length)
                        {
                            error = "Index for SigHash_Single is out of range.";
                            return false;
                        }

                        PublicKey.PublicKeyType ptype = PublicKey.TryReadTaproot(program, out PublicKey pub);
                        if (ptype == PublicKey.PublicKeyType.None || ptype == PublicKey.PublicKeyType.Unknown)
                        {
                            error = "Invalid public key.";
                            return false;
                        }

                        byte[] sigHash = tx.SerializeForSigningTaproot_KeyPath(sig.SigHash, utxos, i, annexHash);
                        if (!calc.VerifySchnorr(sigHash, sig, pub))
                        {
                            error = "Invalid signature.";
                            return false;
                        }
                    }
                    else
                    {
                        // Script path spending:
                        Debug.Assert(witItemCount >= 2);

                        byte[] control = tx.WitnessList[i].Items[--witItemCount];
                        if (control.Length < TaprootControlBaseSize ||
                            control.Length > TaprootControlMaxSize ||
                            ((control.Length - TaprootControlBaseSize) % TaprootControlNodeSize) != 0)
                        {
                            error = "Invalid Taproot control-block size.";
                            return false;
                        }

                        byte[] scrBa = tx.WitnessList[i].Items[--witItemCount];
                        var redeem = new RedeemScript(scrBa);

                        // Compute tapleaf hash
                        var stream = new FastStream(redeem.Data.Length + 3);
                        byte leafVersion = (byte)(control[0] & TapLeafMask);
                        stream.Write(leafVersion);
                        redeem.Serialize(stream);
                        // k is tapleafHash
                        byte[] tapLeafHash = sha256.ComputeTaggedHash_TapLeaf(stream.ToByteArray());

                        if (!VerifyTaprootCommitment(control, program, tapLeafHash))
                        {
                            error = "Invalid taproot commitment.";
                            return false;
                        }

                        if ((control[0] & TaprootLeafMask) == TaprootLeafTapscript)
                        {
                            // Tapscript (leaf version 0xc0)

                            // Note that the following evaluation is a minimal check of validity of the script. It will
                            // continue until it reaches OP_SUCCESS or the end.
                            // https://github.com/bitcoin/bitcoin/blob/57982f419e36d0023c83af2dd0d683ca3160dc2a/src/script/interpreter.cpp#L1817-L1830
                            if (!redeem.TryEvaluateOpSuccess(out bool skip))
                            {
                                error = "Failed to evaluate redeem script.";
                                return false;
                            }

                            if (!skip)
                            {
                                if (witItemCount > Constants.MaxScriptStackItemCount)
                                {
                                    error = Err.OpStackItemOverflow;
                                    return false;
                                }

                                var counter = new SizeCounter();
                                tx.WitnessList[i].AddSerializedSize(counter);

                                var stack = new OpData()
                                {
                                    Tx = tx,
                                    TxInIndex = i,
                                    AnnexHash = annexHash,
                                    TapLeafHash = tapLeafHash,
                                    SigOpLimitLeft = counter.Size + Constants.ValidationWeightOffset,
                                    UtxoList = utxos,

                                    ForceLowS = ForceLowS,
                                    StrictNumberEncoding = StrictNumberEncoding,
                                    // TODO: change this to accept IConsensus
                                    IsBip65Enabled = consensus.IsBip65Enabled,
                                    IsBip112Enabled = consensus.IsBip112Enabled,
                                    IsStrictDerSig = consensus.IsStrictDerSig,
                                    IsBip147Enabled = consensus.IsBip147Enabled,
                                    IsStrictConditionalOpBool = true,
                                };

                                for (int j = 0; j < witItemCount; j++)
                                {
                                    if (tx.WitnessList[i].Items[j].Length > Constants.MaxScriptItemLength)
                                    {
                                        error = $"Witness item can not be bigger than {Constants.MaxScriptItemLength} bytes";
                                        return false;
                                    }
                                    stack.Push(tx.WitnessList[i].Items[j]);
                                }

                                Debug.Assert(stack.ItemCount <= Constants.MaxScriptStackItemCount);

                                if (!redeem.TryEvaluate(ScriptEvalMode.WitnessV1, out IOperation[] rdmOps, out _, out Errors er))
                                {
                                    error = er.Convert();
                                    return false;
                                }

                                foreach (var op in rdmOps)
                                {
                                    if (!op.Run(stack, out er))
                                    {
                                        error = er.Convert();
                                        return false;
                                    }
                                }

                                if (!CheckWitnessStack(stack, out error))
                                {
                                    return false;
                                }
                            }
                        }

                        // TODO: we can add a "standard" rule here to reject other Taproot versions
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
                    ReadOnlySpan<byte> program = ((ReadOnlySpan<byte>)prevOutput.PubScript.Data).Slice(2);
                    if (!IsNotZero(program))
                    {
                        error = Err.ZeroByteWitness;
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

            // Update local UTXO database (n+1)st tx can spend (n)th tx's UTXOs
            List<Utxo> temp = new List<Utxo>(tx.TxOutList.Length);
            for (int i = 0; i < tx.TxOutList.Length; i++)
            {
                TxOut item = tx.TxOutList[i];
                if (!item.PubScript.IsUnspendable())
                {
                    temp.Add(new Utxo((uint)i, item.Amount, item.PubScript));
                }
            }
            if (temp.Count > 0)
            {
                localDb.Add(tx.GetTransactionHash(), temp);
            }

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
