// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;

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
        /// <summary>
        /// Returns if numbers inside scripts (or the popped data from stack to be converted to numbers)
        /// should be checked for strict and shortest encoding. This is a standard rule.
        /// </summary>
        bool StrictNumberEncoding { get; set; }

        /// <summary>
        /// Returns if BIP-65 has enabled <see cref="OP.CheckLocktimeVerify"/> OP code
        /// </summary>
        bool IsBip65Enabled { get; set; }

        /// <summary>
        /// Returns if BIP-66 is enabled to enforce strict DER encoding for signatures.
        /// </summary>
        bool IsStrictDerSig { get; set; }

        /// <summary>
        /// Returns if BIP-112 has enabled <see cref="OP.CheckSequenceVerify"/> OP code
        /// </summary>
        bool IsBip112Enabled { get; set; }

        /// <summary>
        /// Number of OPs in the script that is being evaluated.
        /// Must be reset for each script and be updated by 
        /// <see cref="CheckMultiSigOp"/> and <see cref="CheckMultiSigVerifyOp"/> operations.
        /// </summary>
        int OpCount { get; set; }

        /// <summary>
        /// Gets or sets the annex hash for Taproot scripts. Will be null if annex was not present.
        /// </summary>
        byte[] AnnexHash { get; set; }

        /// <summary>
        /// Gets or sets the TapLeaf hash used in Taproot scripts.
        /// </summary>
        byte[] TapLeafHash { get; set; }

        /// <summary>
        /// Gets or sets the remaining SigOp limit for Taproot scripts.
        /// </summary>
        /// <remarks>
        /// Set the value once at the start of evaluation and reduce by 50 for each SigOp
        /// </remarks>
        int SigOpLimitLeft { get; set; }

        /// <summary>
        /// Gets or sets the position of the last executed <see cref="OP.CodeSeparator"/> to be used by Taproot scripts.
        /// Set to <see cref="uint.MaxValue"/> if none existed.
        /// </summary>
        int CodeSeparatorPosition { get; set; }

        /// <summary>
        /// Verifies correctness of the given signature with the given public key using
        /// the transaction and scripts set in constructor.
        /// </summary>
        /// <param name="sig">Signature</param>
        /// <param name="pubKey">Public key</param>
        /// <param name="sigBa">Signature bytes to remove</param>
        /// <returns>True if verification succeeds, otherwise false.</returns>
        bool Verify(Signature sig, PublicKey pubKey, ReadOnlySpan<byte> sigBa);

        /// <summary>
        /// Verifies multiple signatures versus multiple public keys (for <see cref="OP.CheckMultiSig"/> operations).
        /// Assumes there are less signatures than public keys.
        /// </summary>
        /// <param name="sigs">Array of signatures</param>
        /// <param name="pubKeys">Array of public keys</param>
        /// <param name="m">Number of signatures that have to pass</param>
        /// <param name="error">Error message (null if successful, otherwise will contain information about failure)</param>
        /// <returns>True if all verifications succeed, otherwise false.</returns>
        bool Verify(byte[][] sigs, byte[][] pubKeys, int m, out string error);

        /// <summary>
        /// Checks to see if the extra (last) item that a <see cref="OP.CheckMultiSig"/> operation pops is valid
        /// according to consensus rules.
        /// </summary>
        /// <param name="garbage">An arbitrary byte array</param>
        /// <returns>True if the data was valid, otherwise false.</returns>
        bool CheckMultiSigGarbage(byte[] garbage);

        /// <summary>
        /// Checks the item popped by the conditional OPs to be a strict true/false value.
        /// This is a standard rule for legacy and witness version 0 but a consensus rule for Taproot scripts.
        /// </summary>
        /// <param name="data">Top stack item that was popped by <see cref="OP.IF"/> or <see cref="OP.NotIf"/></param>
        /// <returns>True if the item is strictly encoded; otherwise false.</returns>
        bool CheckConditionalOpBool(byte[] data);

        /// <summary>
        /// Compares locktime for a <see cref="OP.CheckLocktimeVerify"/> operation.
        /// </summary>
        /// <param name="other">The converted locktime value from the stack</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if the locktime is past and the transaction is spendable; otherwise false.</returns>
        bool CompareLocktimes(long other, out string error);

        /// <summary>
        /// Compares sequences for a <see cref="OP.CheckSequenceVerify"/> operation.
        /// </summary>
        /// <param name="other">The converted sequence value from the stack</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if the transaction is spendable; otherwise false.</returns>
        bool CompareSequences(long other, out string error);

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
        /// Pushes (or inserts) byte array equivalant of the given boolean at the top of the stack.
        /// </summary>
        /// <param name="b">Boolean value to push</param>
        void Push(bool b);

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
