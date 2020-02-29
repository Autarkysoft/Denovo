// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Defines a last-in-first-out (LIFO) collection (similar to <see cref="System.Collections.Stack"/>)
    /// to be used with <see cref="IOperation"/>s as their data provider.
    /// <para/>All indexes are zero based meaning item at the end (real index = length-1) is index 0, 
    /// the item before last is 1,... the first item (real index=0) is length-1.
    /// </summary>
    public interface IOpData
    {
        EllipticCurveCalculator Calc { get; }

        /// <inheritdoc cref="ITransaction.GetBytesToSign(ITransaction, int, SigHashType, IRedeemScript)"/>
        /// <param name="sht"><inheritdoc/></param>
        /// <param name="redeem"><inheritdoc/></param>
        byte[] GetBytesToSign(SigHashType sht, IRedeemScript redeem);

        /// <summary>
        /// Returns number of available items in the stack.
        /// </summary>
        int ItemCount { get; }

        /// <summary>
        /// Returns the item at the top of the stack without removing it.
        /// </summary>
        /// <returns>The byte array at the top of the stack</returns>
        byte[] Peek();

        /// <summary>
        /// Returns multiple items from the top of the stack without removing them.
        /// </summary>
        /// <param name="count">Number of items to return</param>
        /// <returns>An array of byte arrays from the top of the stack</returns>
        byte[][] Peek(int count);

        /// <summary>
        /// Returns the item at a specific index starting from the top of the stack without removing it.
        /// <para/>NOTE: Index starts from zero meaning the item at the end (length-1) is index 0, the item before end is 1 and so on.
        /// </summary>
        /// <param name="index">Index of item from end to return (starting from 0)</param>
        /// <returns>The byte array at the specified intex</returns>
        byte[] PeekAtIndex(int index);

        /// <summary>
        /// Removes and returns the item at the top of the stack.
        /// </summary>
        /// <returns>The removed byte array at the top of the stack</returns>
        byte[] Pop();

        /// <summary>
        /// Removes multiple items from the top of the stack and returns all of them without changing the order ([1234] -> [34]).
        /// </summary>
        /// <param name="count">Number of items to remove and return</param>
        /// <returns>An array of byte arrays removed from the top of the stack</returns>
        byte[][] Pop(int count);

        /// <summary>
        /// Removes and returns the item at the specified index (will shift the items in its place).
        /// <para/>NOTE: Index starts from zero meaning the item at the end (length-1) is index 0, the item before end is 1 and so on.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The byte array removed from the specified intex</returns>
        byte[] PopAtIndex(int index);

        /// <summary>
        /// Pushes (or inserts) an item at the top of the stack.
        /// </summary>
        /// <param name="data">Byte array to push onto the stack</param>
        void Push(byte[] data);

        /// <summary>
        /// Pushes (or inserts) multiple items at the top of the stack in the same order.
        /// </summary>
        /// <param name="data">Arrays of byte array to push</param>
        void Push(byte[][] data);

        /// <summary>
        /// Inserts an item at the specified index (from the top) of the stack.
        /// <para/>NOTE: Index starts from zero meaning the item at the end (length-1) is index 0, the item before end is 1 and so on.
        /// </summary>
        /// <param name="data">Byte array to insert in the stack</param>
        /// <param name="index">Index at which to insert the given <paramref name="data"/></param>
        void Insert(byte[] data, int index);

        /// <summary>
        /// Inserts multiple items at the specified index (from the top) of the stack.
        /// <para/>NOTE: Index starts from zero meaning the item at the end (length-1) is index 0, the item before end is 1 and so on.
        /// </summary>
        /// <param name="data">Array of Byte arrays to insert in the stack</param>
        /// <param name="index">Index at which to insert the given <paramref name="data"/></param>
        void Insert(byte[][] data, int index);


        /// <summary>
        /// Returns number of available items in the "alt-stack"
        /// </summary>
        int AltItemCount { get; }

        /// <summary>
        /// Removes and returns the item at the top of the "alt-stack".
        /// </summary>
        /// <returns>The removed byte array at the top of the "alt-stack"</returns>
        byte[] AltPop();

        /// <summary>
        /// Pushes (or inserts) an item at the top of the "alt-stack".
        /// </summary>
        /// <param name="data">Byte array to push onto the "alt-stack"</param>
        void AltPush(byte[] data);
    }
}
