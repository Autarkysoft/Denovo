// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;

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

            calc = new EllipticCurveCalculator();
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

            calc = new EllipticCurveCalculator();
        }



        private const int DefaultCapacity = 10;
        // Don't rename (used by test through reflection).
        private byte[][] holder;

        // TODO: complete the code for signing part + tests
        public ITransaction Tx { get; set; }
        public IOperation[] ExecutingScript { get; set; }
        public int TxInIndex { get; set; }
        public ulong AmountBeingSpent { get; set; }
        public bool IsSegWit { get; set; }


        /// <summary>
        /// [Default value = true]
        /// If true it will enforce the extra item popped by <see cref="OP.CheckMultiSig"/> to be <see cref="OP._0"/>,
        /// otherwise it can be anything.
        /// </summary>
        public bool IsStrictMultiSigGarbage { get; set; }

        public bool ForceLowS { get; set; }

        /// <inheritdoc/>
        public bool StrictNumberEncoding { get; set; }

        /// <inheritdoc/>
        public int OpCount { get; set; }

        private readonly EllipticCurveCalculator calc;
        private readonly ScriptSerializer scriptSer = new ScriptSerializer();


        /// <inheritdoc/>
        public bool Verify(Signature sig, PublicKey pubKey, ReadOnlySpan<byte> sigBa)
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

            return calc.Verify(dataToSign, sig, pubKey, ForceLowS);
        }

        /// <inheritdoc/>
        public bool Verify(byte[][] sigs, byte[][] pubKeys, int m, out string error)
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

                if (!PublicKey.TryRead(pubKeys[pubIndex--], out PublicKey pubK))
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

                if (calc.Verify(dataToSign, sig, pubK, ForceLowS))
                {
                    sigIndex--;
                    m--;
                }
            }

            error = null;
            return m == 0;
        }

        /// <inheritdoc/>
        public bool CheckMultiSigGarbage(byte[] garbage) => IsStrictMultiSigGarbage ? garbage.Length == 0 : true;


        /// <inheritdoc/>
        public bool IsBip65Enabled { get; set; }

        /// <inheritdoc/>
        public bool IsBip112Enabled { get; set; }

        /// <inheritdoc/>
        public bool IsStrictDerSig { get; set; }

        /// <inheritdoc/>
        public bool CompareLocktimes(long other, out string error)
        {
            if (other < 0)
            {
                error = "Extracted locktime from script can not be negative.";
                return false;
            }

            if (!Tx.LockTime.IsSameType(other))
            {
                error = "Extracted locktime from script should be the same type as transaction's locktime.";
                return false;
            }

            if (Tx.LockTime < other)
            {
                error = "Input is not spendable (locktime not reached).";
                return false;
            }

            // TODO: This could be simplified but we need to first implement TransactionVerifier and BlockVerifier classes
            // https://bitcoin.stackexchange.com/questions/40706/why-is-op-checklocktimeverify-disabled-by-maximum-sequence-number
            foreach (var tin in Tx.TxInList)
            {
                if (tin.Sequence == uint.MaxValue)
                {
                    error = "Sequence should be less than maximum.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <inheritdoc/>
        public bool CompareSequences(long other, out string error)
        {
            if (other < 0)
            {
                error = "Extracted sequence from script can not be negative.";
                return false;
            }

            if ((other & 0b10000000_00000000_00000000_00000000U) != 0)
            {
                error = null;
                return true;
            }

            if (Tx.Version < 2)
            {
                error = "Transaction version must be bigger than 2.";
                return false;
            }

            if ((Tx.TxInList[TxInIndex].Sequence & 0b10000000_00000000_00000000_00000000U) != 0)
            {
                error = "Input's sequence's highest bit should not be set.";
                return false;
            }

            LockTime temp = new LockTime(Tx.TxInList[TxInIndex].Sequence & 0b00000000_01000000_11111111_11111111U);
            other &= 0b00000000_01000000_11111111_11111111U;
            if (!temp.IsSameType(other))
            {
                error = "Extracted sequence from script should be the same type as transaction's sequence as locktime.";
                return false;
            }


            if (temp < other)
            {
                error = "Input is not spendable (locktime not reached).";
                return false;
            }

            error = null;
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
