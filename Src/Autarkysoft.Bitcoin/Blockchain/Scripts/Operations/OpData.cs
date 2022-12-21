﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// An advanced implementation of a last-in-first-out (LIFO) collection (similar to <see cref="System.Collections.Stack"/>)
    /// to be used with <see cref="IOperation"/>s as their data provider.
    /// <para/>All the functions in this class skip checks (eg. checking ItemCount before popping an item). 
    /// Caller must perform checks.
    /// <para/>All indexes are zero based whether it is normal index from beginning or index from end:
    /// Item at the end (length-1) is at index 0.
    /// </summary>
    [DebuggerDisplay("Item count = {ItemCount}, Alt item count = {AltItemCount}")]
    public class OpData : IOpData
    {
        /// <summary>
        /// Initializes a new instance of <see cref="OpData"/> with default capacity
        /// </summary>
        public OpData() : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="OpData"/> with the given capacity.
        /// </summary>
        /// <param name="cap">Capacity</param>
        public OpData(int cap)
        {
            if (cap < DefaultCapacity)
            {
                cap = DefaultCapacity;
            }
            holder = new byte[cap][];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="OpData"/> with the given data array.
        /// </summary>
        /// <param name="dataToPush">An array of byte arrays to put in the stack</param>
        public OpData(byte[][] dataToPush)
        {
            if (dataToPush == null || dataToPush.Length == 0)
            {
                holder = new byte[DefaultCapacity][];
            }
            else
            {
                int len = (dataToPush.Length < DefaultCapacity) ? DefaultCapacity : dataToPush.Length + DefaultCapacity;
                holder = new byte[len][];
                ItemCount = dataToPush.Length;
                Array.Copy(dataToPush, holder, dataToPush.Length);
            }
        }



        private const int DefaultCapacity = 10;
        // Don't rename (used by test through reflection).
        private byte[][] holder;

        private readonly DSA dsa = new DSA();
        private readonly ScriptSerializer scriptSer = new ScriptSerializer();

        /// <summary>
        /// Transaction being verified
        /// </summary>
        public ITransaction Tx { get; set; }
        /// <summary>
        /// Index of the input script (in a <see cref="TxIn"/> instance) being verified
        /// </summary>
        public int TxInIndex { get; set; }
        /// <summary>
        /// List of UTXOs spent by the transaction (only used in Taproot scripts).
        /// </summary>
        public IUtxo[] UtxoList { get; set; }
        /// <summary>
        /// An array of operations taken from the script being executed (eg. pubkey script, redeem script,...)
        /// </summary>
        public IOperation[] ExecutingScript { get; set; }
        /// <inheritdoc/>
        public int OpCount { get; set; }
        /// <summary>
        /// Amount of coins in satoshi being spent. Used in SegWit transaction verifications only
        /// </summary>
        public ulong AmountBeingSpent { get; set; }
        /// <summary>
        /// Gets or sets if this stack is used for witness script evaluation
        /// </summary>
        public bool IsSegWit { get; set; }

        /// <summary>
        /// If true only strict encoding of true/false is accepted for conditional operations.
        /// This is a standard rule for legacy and witness version 0, and a consensus rule for Taproot scripts.
        /// </summary>
        public bool IsStrictConditionalOpBool { get; set; }
        /// <summary>
        /// If true will only accept low s values in signatures. This is a standard rule.
        /// </summary>
        public bool ForceLowS { get; set; }
        /// <inheritdoc/>
        public bool StrictNumberEncoding { get; set; }

        /// <inheritdoc/>
        public bool IsBip65Enabled { get; set; }
        /// <inheritdoc/>
        public bool IsStrictDerSig { get; set; }
        /// <inheritdoc/>
        public bool IsBip112Enabled { get; set; }
        /// <summary>
        /// If true it will enforce the extra item popped by <see cref="OP.CheckMultiSig"/> to be <see cref="OP._0"/>,
        /// otherwise it can be anything.
        /// </summary>
        public bool IsBip147Enabled { get; set; }

        /// <inheritdoc/>
        public byte[] AnnexHash { get; set; }

        /// <inheritdoc/>
        public byte[] TapLeafHash { get; set; }

        /// <inheritdoc/>
        public int SigOpLimitLeft { get; set; }

        /// <inheritdoc/>
        public uint CodeSeparatorPosition { get; set; } = uint.MaxValue;


        /// <inheritdoc/>
        public bool Verify(Signature sig, in Point pubKey, ReadOnlySpan<byte> sigBa)
        {
            byte[] spendScr, dataToSign;
            if (IsSegWit)
            {
                spendScr = scriptSer.ConvertWitness(ExecutingScript);
                dataToSign = Tx.SerializeForSigningSegWit(spendScr, TxInIndex, AmountBeingSpent, sig.SigHash);
            }
            else
            {
                spendScr = scriptSer.Convert(ExecutingScript, sigBa);
                dataToSign = Tx.SerializeForSigning(spendScr, TxInIndex, sig.SigHash);
            }

            Scalar8x32 hash = new Scalar8x32(dataToSign, out bool overflow);
            return dsa.VerifySimple(sig, pubKey, hash, ForceLowS);
        }

        /// <inheritdoc/>
        public bool Verify(byte[][] sigs, byte[][] pubKeys, int m, out Errors error)
        {
            byte[] spendScr;
            if (IsSegWit)
            {
                spendScr = scriptSer.ConvertWitness(ExecutingScript);
            }
            else
            {
                spendScr = scriptSer.ConvertMulti(ExecutingScript, sigs);
            }

            int sigIndex = sigs.Length - 1;
            int pubIndex = pubKeys.Length - 1;

            while (m > 0 && sigIndex >= 0 && pubIndex >= 0 && m <= pubIndex + 1)
            {
                // Empty byte signature doesn't fail as being "invalid" signature with or without strict rules
                // but it _IS_ invalid and signature verification fails so there is no need to perform verification
                // and then fail, instead just skip it here.
                if (sigs[sigIndex].Length == 0)
                {
                    sigIndex--;
                    pubIndex--;
                    continue;
                }

                Signature sig;
                if (IsStrictDerSig)
                {
                    if (!Signature.TryReadStrict(sigs[sigIndex], out sig, out error))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Signature.TryReadLoose(sigs[sigIndex], out sig, out _))
                    {
                        sigIndex--;
                        pubIndex--;
                        continue;
                    }
                }

                if (!Point.TryRead(pubKeys[pubIndex--], out Point pubK))
                {
                    continue;
                }

                byte[] dataToSign;
                if (IsSegWit)
                {
                    dataToSign = Tx.SerializeForSigningSegWit(spendScr, TxInIndex, AmountBeingSpent, sig.SigHash);
                }
                else
                {
                    dataToSign = Tx.SerializeForSigning(spendScr, TxInIndex, sig.SigHash);
                }

                Scalar8x32 hash = new Scalar8x32(dataToSign, out bool overflow);
                if (dsa.VerifySimple(sig, pubK, hash, ForceLowS))
                {
                    sigIndex--;
                    m--;
                }
            }

            error = Errors.None;
            return m == 0;
        }

        /// <inheritdoc/>
        public bool VerifySchnorr(ReadOnlySpan<byte> sigBa, in Point pub, out Errors error)
        {
            Debug.Assert(UtxoList != null);

            if (!SchnorrSignature.TryRead(sigBa, out SchnorrSignature sig, out error))
            {
                return false;
            }

            if (sig.SigHash.ToOutputType() == SigHashType.Single && TxInIndex >= Tx.TxOutList.Length)
            {
                error = Errors.OutOfRangeSigHashSingle;
                return false;
            }

            byte[] sigHash = Tx.SerializeForSigningTaproot_ScriptPath(sig.SigHash, UtxoList, TxInIndex, AnnexHash, TapLeafHash, CodeSeparatorPosition);
            if (dsa.VerifySchnorr(sig, pub, sigHash))
            {
                error = Errors.None;
                return true;
            }
            else
            {
                error = Errors.FailedSignatureVerification;
                return false;
            }
        }

        /// <inheritdoc/>
        public bool CheckMultiSigGarbage(byte[] garbage) => !IsBip147Enabled || garbage.Length == 0;

        /// <inheritdoc/>
        public bool CheckConditionalOpBool(byte[] data) => !IsStrictConditionalOpBool ||
                                                            data.Length == 0 ||
                                                            (data.Length == 1 && data[0] == 1);

        /// <inheritdoc/>
        public bool CompareLocktimes(long other, out Errors error)
        {
            if (other < 0)
            {
                error = Errors.NegativeLocktime;
                return false;
            }

            if (!Tx.LockTime.IsSameType(other))
            {
                error = Errors.UnequalLocktimeType;
                return false;
            }

            if (Tx.LockTime < other)
            {
                error = Errors.UnspendableLocktime;
                return false;
            }

            // TODO: This could be simplified but we need to first implement TransactionVerifier and BlockVerifier classes
            // https://bitcoin.stackexchange.com/questions/40706/why-is-op-checklocktimeverify-disabled-by-maximum-sequence-number
            foreach (var tin in Tx.TxInList)
            {
                if (tin.Sequence == uint.MaxValue)
                {
                    error = Errors.MaxTxSequence;
                    return false;
                }
            }

            error = Errors.None;
            return true;
        }

        /// <inheritdoc/>
        public bool CompareSequences(long other, out Errors error)
        {
            if (other < 0)
            {
                error = Errors.NegativeSequence;
                return false;
            }

            if ((other & 0b10000000_00000000_00000000_00000000U) != 0)
            {
                error = Errors.None;
                return true;
            }

            if (Tx.Version < 2)
            {
                error = Errors.InvalidTxVersion;
                return false;
            }

            if ((Tx.TxInList[TxInIndex].Sequence & 0b10000000_00000000_00000000_00000000U) != 0)
            {
                error = Errors.InvalidSequenceHighBit;
                return false;
            }

            long txSeq = Tx.TxInList[TxInIndex].Sequence & 0b00000000_01000000_11111111_11111111U;
            other &= 0b00000000_01000000_11111111_11111111U;
            if (!(txSeq < 0x400000 && other < 0x400000 || txSeq >= 0x400000 && other >= 0x400000))
            {
                error = Errors.UnequalSequenceType;
                return false;
            }

            if (txSeq < other)
            {
                error = Errors.UnspendableLocktime;
                return false;
            }

            error = Errors.None;
            return true;
        }


        /// <inheritdoc/>
        public int ItemCount { get; private set; }


        /// <inheritdoc/>
        public byte[] Peek()
        {
            return holder[ItemCount - 1];
        }

        /// <inheritdoc/>
        public byte[][] Peek(int count)
        {
            byte[][] res = new byte[count][];
            Array.Copy(holder, ItemCount - count, res, 0, count);

            return res;
        }

        /// <inheritdoc/>
        public byte[] PeekAtIndex(int index)
        {
            return holder[ItemCount - 1 - index];
        }


        /// <inheritdoc/>
        public byte[] Pop()
        {
            byte[] res = holder[--ItemCount];
            holder[ItemCount] = null;
            return res;
        }

        /// <inheritdoc/>
        public byte[][] Pop(int count)
        {
            byte[][] res = new byte[count][];
            Array.Copy(holder, ItemCount - count, res, 0, count);
            for (int i = 0; i < count; i++)
            {
                holder[--ItemCount] = null;
            }

            return res;
        }

        /// <inheritdoc/>
        public byte[] PopAtIndex(int index)
        {
            int realIndex = ItemCount - 1 - index;
            byte[] res = holder[realIndex];
            ItemCount--;

            if (index != 0)
            {
                Array.Copy(holder, realIndex + 1, holder, realIndex, index /*index works as count (copy items after realindex)*/);
            }
            holder[ItemCount] = null;

            return res;
        }


        /// <inheritdoc/>
        public void Push(bool b) => Push(b ? new byte[] { 1 } : Array.Empty<byte>());

        /// <inheritdoc/>
        public void Push(byte[] data)
        {
            if (ItemCount == holder.Length)
            {
                // Instead of doubling we add the default value since we don't need that many new items 
                // eg. 10->20 but 20->30 instead of 40.
                byte[][] holder2 = new byte[holder.Length + DefaultCapacity][];
                Array.Copy(holder, 0, holder2, 0, ItemCount);
                holder = holder2;
            }

            holder[ItemCount++] = data;
        }

        /// <inheritdoc/>
        public void Push(byte[][] data)
        {
            if (ItemCount + data.Length > holder.Length)
            {
                byte[][] holder2 = new byte[ItemCount + data.Length + DefaultCapacity][];
                Array.Copy(holder, 0, holder2, 0, ItemCount);
                holder = holder2;
            }

            Array.Copy(data, 0, holder, ItemCount, data.Length);
            ItemCount += data.Length;
        }

        /// <inheritdoc/>
        public void Insert(byte[] data, int index)
        {
            // only call if index < itemcount
            if (ItemCount == holder.Length)
            {
                // Instead of doubling we add the default value since we don't need that many new items.
                byte[][] holder2 = new byte[holder.Length + DefaultCapacity][];
                Array.Copy(holder, 0, holder2, 0, ItemCount);
                holder = holder2;
            }

            int realIndex = ItemCount - index;
            if (realIndex < ItemCount)
            {
                Array.Copy(holder, realIndex, holder, realIndex + 1, ItemCount - realIndex);
            }
            holder[realIndex] = data;
            ItemCount++;
        }

        /// <inheritdoc/>
        public void Insert(byte[][] data, int index)
        {
            int realIndex = ItemCount - index;
            if (ItemCount + data.Length > holder.Length)
            {
                byte[][] holder2 = new byte[ItemCount + data.Length + DefaultCapacity][];

                Array.Copy(holder, 0, holder2, 0, realIndex);
                Array.Copy(data, 0, holder2, realIndex, data.Length);
                Array.Copy(holder, realIndex, holder2, realIndex + data.Length, ItemCount - realIndex);

                holder = holder2;
            }
            else
            {
                Array.Copy(holder, realIndex, holder, realIndex + data.Length, ItemCount - realIndex);
                Array.Copy(data, 0, holder, realIndex, data.Length);
            }

            ItemCount += data.Length;
        }



        // Don't rename (used by test through reflection).
        private byte[][] altHolder;

        /// <inheritdoc/>
        public int AltItemCount { get; private set; }

        /// <inheritdoc/>
        public byte[] AltPop()
        {
            byte[] res = altHolder[--AltItemCount];
            altHolder[AltItemCount] = null;
            return res;
        }

        /// <inheritdoc/>
        public void AltPush(byte[] data)
        {
            if (altHolder == null)
            {
                // We set alt-holder here to keep it null for majority of cases that never use this stack.
                altHolder = new byte[DefaultCapacity][];
            }
            if (AltItemCount == altHolder.Length)
            {
                // Instead of doubling we add the default value since we don't need that many new items 
                // eg. 10->20 but 20->30 instead of 40.
                byte[][] temp = new byte[altHolder.Length + DefaultCapacity][];
                Array.Copy(altHolder, 0, temp, 0, AltItemCount);
                altHolder = temp;
            }

            altHolder[AltItemCount++] = data;
        }
    }
}
