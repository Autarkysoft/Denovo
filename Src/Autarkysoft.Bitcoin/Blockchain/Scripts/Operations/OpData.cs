// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// An advanced implementation of a last-in-first-out (LIFO) collection (similar to <see cref="System.Collections.Stack"/>)
    /// to be used with <see cref="IOperation"/>s as their data provider.
    /// <para/>All the functions in this class skip checks (eg. checking ItemCount before popping an item). 
    /// Caller must perform checks.
    /// </summary>
    public class OpData : IOpData
    {
        public OpData()
        {
            holder = new byte[DefaultCapacity][];
        }


        public OpData(byte[][] dataToPush)
        {
            if (dataToPush == null || dataToPush.Length == 0)
            {
                holder = new byte[DefaultCapacity][];
            }
            else if (dataToPush.Length < DefaultCapacity)
            {
                holder = new byte[DefaultCapacity][];
                Array.Copy(dataToPush, holder, dataToPush.Length);
                ItemCount = dataToPush.Length;
            }
            else
            {
                holder = dataToPush;
                ItemCount = dataToPush.Length;
            }
        }



        private const int DefaultCapacity = 10;
        private byte[][] holder;

        private readonly ITransaction Tx;
        private readonly ITransaction PrvTx;
        private readonly int TxInIndex;
        private readonly int TxOutIndex;
        /// <inheritdoc/>
        public EllipticCurveCalculator Calc { get; private set; }
        

        public byte[] GetBytesToSign(SigHashType sht)
        {
            return Tx.GetBytesToSign(PrvTx, TxInIndex, sht);
        }


        /// <inheritdoc/>
        public int ItemCount { get; private set; }



        /// <summary>
        /// Returns the item at the top of the stack without removing it.
        /// </summary>
        /// <returns>The byte array at the top of the stack</returns>
        public byte[] Peek()
        {
            return holder[ItemCount - 1];
        }

        /// <summary>
        /// Returns multiple items from the top of the stack without removing them.
        /// </summary>
        /// <param name="count">Number of items to return</param>
        /// <returns>An array of byte arrays from the top of the stack</returns>
        public byte[][] Peek(int count)
        {
            byte[][] res = new byte[count][];
            Array.Copy(holder, ItemCount - count, res, 0, count);

            return res;
        }

        /// <summary>
        ///  Returns the item at a specific index starting from the top of the stack without removing it.
        /// <para/>NOTE: Index starts from zero meaning the item at the end (length-1) is index 0, the item before end is 1 and so on.
        /// </summary>
        /// <param name="index">Index of item from end to return (starting from 0)</param>
        /// <returns>The byte array at the specified intex</returns>
        public byte[] PeekAtIndex(int index)
        {
            return holder[ItemCount - 1 - index];
        }


        /// <summary>
        /// Removes and returns the item at the top of the stack.
        /// </summary>
        /// <returns>The removed byte array at the top of the stack</returns>
        public byte[] Pop()
        {
            byte[] res = holder[--ItemCount];
            holder[ItemCount] = null;
            return res;
        }

        /// <summary>
        /// Removes multiple items from the top of the stack and returns all of them without changing the order ([1234] -> [34]).
        /// </summary>
        /// <param name="count">Number of items to remove and return</param>
        /// <returns>An array of byte arrays removed from the top of the stack</returns>
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

        /// <summary>
        /// Removes and returns the item at the specified index (will shift the items in its place).
        /// <para/>NOTE: Index starts from zero meaning the item at the end (length-1) is index 0, the item before end is 1 and so on.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The byte array removed from the specified intex</returns>
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


        /// <summary>
        /// Pushes (or inserts) an item at the top of the stack.
        /// </summary>
        /// <param name="data">Byte array to push onto the stack</param>
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

        /// <summary>
        /// Pushes (or inserts) multiple items at the top of the stack in the same order.
        /// </summary>
        /// <param name="data">Arrays of byte array to push</param>
        public void Push(byte[][] data)
        {
            if (ItemCount + data.Length > holder.Length)
            {
                int extraCap = (Math.DivRem(data.Length, DefaultCapacity, out int rem) + ((rem != 0) ? 1 : 0)) * DefaultCapacity;
                byte[][] holder2 = new byte[holder.Length + extraCap][];
                Array.Copy(holder, 0, holder2, 0, ItemCount);
                holder = holder2;
            }

            Array.Copy(data, 0, holder, ItemCount, data.Length);
            ItemCount += data.Length;
        }

        /// <summary>
        /// Inserts an item at the specified index (from the top) of the stack.
        /// <para/>NOTE: Index starts from zero meaning the item at the end (length-1) is index 0, the item before end is 1 and so on.
        /// </summary>
        /// <param name="data">Byte array to insert in the stack</param>
        /// <param name="index">Index at which to insert the <paramref name="data"/></param>
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

        /// <summary>
        /// Inserts multiple items at the specified index (from the top) of the stack.
        /// <para/>NOTE: Index starts from zero meaning the item at the end (length-1) is index 0, the item before end is 1 and so on.
        /// </summary>
        /// <param name="data">Array of Byte arrays to insert in the stack</param>
        /// <param name="index">Index at which to insert the <paramref name="data"/></param>
        public void Insert(byte[][] data, int index)
        {
            int realIndex = ItemCount - index;
            if (ItemCount + data.Length > holder.Length)
            {
                int extraCap = (Math.DivRem(data.Length, DefaultCapacity, out int rem) + ((rem != 0) ? 1 : 0)) * DefaultCapacity;
                byte[][] holder2 = new byte[holder.Length + extraCap][];

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




        private byte[][] altHolder;

        /// <summary>
        /// Returns number of available items in the "alt-stack"
        /// </summary>
        public int AltItemCount { get; private set; }

        /// <summary>
        /// Removes and returns the item at the top of the "alt-stack".
        /// </summary>
        /// <returns>The removed byte array at the top of the "alt-stack"</returns>
        public byte[] AltPop()
        {
            byte[] res = altHolder[--AltItemCount];
            altHolder[AltItemCount] = null;
            return res;
        }

        /// <summary>
        /// Pushes (or inserts) an item at the top of the "alt-stack".
        /// </summary>
        /// <param name="data">Byte array to push onto the "alt-stack"</param>
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






        /// <summary>
        /// Returns the private data holder field (for testing)!
        /// </summary>
        /// <returns><see cref="holder"/></returns>
        public byte[][] ToArray()
        {
            // TODO: since this has not other usage except for testing 
            //       maybe reveal the private field to unit test project so that we don't need to call this?
            return holder;
        }

    }
}
