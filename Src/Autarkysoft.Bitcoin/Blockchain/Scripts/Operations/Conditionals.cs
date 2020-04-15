// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base (abstract) class for conditional operations (covering <see cref="OP.IF"/>, <see cref="OP.NotIf"/>, 
    /// <see cref="OP.ELSE"/> and <see cref="OP.EndIf"/> OPs).
    /// </summary>
    public abstract class IfElseOpsBase : BaseOperation
    {
        /// <summary>
        /// Each IF operation pops an item from the stack first and converts it to a boolean. 
        /// <see cref="OP.IF"/> expressions run if that item was true while 
        /// <see cref="OP.NotIf"/> expressions run if that item was false.
        /// </summary>
        protected bool runWithTrue;

        /// <summary>
        /// The main operations under the <see cref="OP.IF"/> or <see cref="OP.NotIf"/> OP.
        /// </summary>
        protected internal IOperation[] mainOps;
        /// <summary>
        /// The "else" operations under the <see cref="OP.ELSE"/> op.
        /// </summary>
        protected internal IOperation[] elseOps;
        /// <summary>
        /// Indicates whether this <see cref="IOperation"/> is inside a <see cref="IWitness"/>.
        /// </summary>
        protected internal bool isWitness;


        /// <summary>
        /// Runs all the operations in main or else list. Return value indicates success.
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            // Remove top stack item and convert it to bool
            // OP_IF: runs if True
            // OP_NOTIF: runs if false
            byte[] topItem = opData.Pop();

            if (isWitness && (topItem.Length > 1 || (topItem.Length == 1 && topItem[0] != 1)))
            {
                error = "True/False item popped by conditional OPs in a witness script must be strinct.";
                return false;
            }

            if (IsNotZero(topItem) == runWithTrue)
            {
                foreach (var op in mainOps)
                {
                    if (!op.Run(opData, out error))
                    {
                        return false;
                    }
                }
            }
            else if (elseOps != null && elseOps.Length != 0)
            {
                foreach (var op in elseOps)
                {
                    if (!op.Run(opData, out error))
                    {
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }


        /// <summary>
        /// Returns if there is any executed <see cref="OP.CodeSeparator"/> operations among the operation lists.
        /// </summary>
        /// <returns>True if there were any executed <see cref="OP.CodeSeparator"/>s, otherwise false.</returns>
        public bool HasExecutedCodeSeparator()
        {
            foreach (var op in mainOps)
            {
                if (op is CodeSeparatorOp cs && cs.IsExecuted)
                {
                    return true;
                }
            }
            if (elseOps != null)
            {
                foreach (var op in elseOps)
                {
                    if (op is CodeSeparatorOp cs && cs.IsExecuted)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <inheritdoc/>
        public override void WriteToStream(FastStream stream)
        {
            // Start with OP_IF or OP_NotIf
            stream.Write((byte)OpValue);
            foreach (var op in mainOps)
            {
                op.WriteToStream(stream);
            }

            // Continue with OP_ELSE if it exists
            if (elseOps != null)
            {
                stream.Write((byte)OP.ELSE);
                foreach (var op in elseOps)
                {
                    op.WriteToStream(stream);
                }
            }

            // End with OP_EndIf
            stream.Write((byte)OP.EndIf);
        }


        /// <inheritdoc/>
        public override void WriteToStreamForSigning(FastStream stream, ReadOnlySpan<byte> sig)
        {
            int lastExecutedElseCSep = -1;
            if (elseOps != null)
            {
                for (int i = elseOps.Length - 1; i >= 0; i--)
                {
                    if (elseOps[i] is CodeSeparatorOp cs && cs.IsExecuted)
                    {
                        lastExecutedElseCSep = i;
                        break;
                    }
                }
            }

            if (lastExecutedElseCSep >= 0)
            {
                WriteElse(stream, lastExecutedElseCSep, sig);
            }
            else
            {
                int lastExecutedCSep = -1;
                for (int i = mainOps.Length - 1; i >= 0; i--)
                {
                    if (mainOps[i] is CodeSeparatorOp cs && cs.IsExecuted)
                    {
                        lastExecutedCSep = i;
                        break;
                    }
                }

                if (lastExecutedCSep >= 0)
                {
                    WriteMain(stream, lastExecutedCSep, sig);
                    if (elseOps != null)
                    {
                        stream.Write((byte)OP.ELSE);
                        WriteElse(stream, 0, sig);
                    }
                }
                else
                {
                    // This branch is when there is either no OP_CodeSeparator or they weren't executed
                    // (eg. the whole IF/ELSE be in unexecuted branch of another IF/ELSE)
                    stream.Write((byte)OpValue);
                    WriteMain(stream, 0, sig);
                    if (elseOps != null)
                    {
                        stream.Write((byte)OP.ELSE);
                        WriteElse(stream, 0, sig);
                    }
                }
            }

            // End with OP_EndIf
            stream.Write((byte)OP.EndIf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteMain(FastStream stream, int start, ReadOnlySpan<byte> sig)
        {
            for (int i = start; i < mainOps.Length; i++)
            {
                mainOps[i].WriteToStreamForSigning(stream, sig);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteElse(FastStream stream, int start, ReadOnlySpan<byte> sig)
        {
            for (int i = start; i < elseOps.Length; i++)
            {
                elseOps[i].WriteToStreamForSigning(stream, sig);
            }
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object, flase if otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is IfElseOpsBase other)
            {
                if (other.OpValue == OpValue)
                {
                    if (other.mainOps.Length == mainOps.Length)
                    {
                        for (int i = 0; i < mainOps.Length; i++)
                        {
                            if (!other.mainOps[i].Equals(mainOps[i]))
                            {
                                return false;
                            }
                        }

                        if (other.elseOps == null && elseOps != null ||
                            other.elseOps != null && elseOps == null ||
                            other.elseOps != null && elseOps != null && other.elseOps.Length != elseOps.Length)
                        {
                            return false;
                        }
                        if (other.elseOps != null && elseOps != null && other.elseOps.Length == elseOps.Length)
                        {
                            for (int i = 0; i < elseOps.Length; i++)
                            {
                                if (!other.elseOps[i].Equals(elseOps[i]))
                                {
                                    return false;
                                }
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode()
        {
            int result = 17;
            foreach (var item in mainOps)
            {
                result ^= item.GetHashCode();
            }
            if (elseOps != null)
            {
                result ^= 74;
                foreach (var item in elseOps)
                {
                    result ^= item.GetHashCode();
                }
            }

            return result;
        }
    }





    /// <summary>
    /// Operation to run a set of operations (if expressions) if preceeded by True (anything except zero).
    /// </summary>
    public class IFOp : IfElseOpsBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="IFOp"/> for internal use
        /// </summary>
        /// <param name="isWit">
        /// Indicates whether this <see cref="IOperation"/> is inside a <see cref="IWitness"/>.
        /// </param>
        internal IFOp(bool isWit)
        {
            runWithTrue = true;
            isWitness = isWit;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IFOp"/> with the given <see cref="IOperation"/> arrays.
        /// </summary>
        /// <param name="ifBlockOps">The main array of operations to run after <see cref="OP.IF"/></param>
        /// <param name="elseBlockOps">The alternative set of operations to run if the previous expresions didn't run.</param>
        /// <param name="isWit">
        /// Indicates whether this <see cref="IOperation"/> is inside a <see cref="IWitness"/>.
        /// </param>
        public IFOp(IOperation[] ifBlockOps, IOperation[] elseBlockOps, bool isWit)
        {
            runWithTrue = true;
            isWitness = isWit;
            mainOps = ifBlockOps ?? new IOperation[0];
            elseOps = elseBlockOps;
        }

        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.IF;
    }


    /// <summary>
    /// Operation to run a set of operations (NotIf expressions) if preceeded by False (an empty array aka OP_0).
    /// </summary>
    public class NotIfOp : IfElseOpsBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NotIfOp"/> for internal use
        /// </summary>
        /// <param name="isWit">
        /// Indicates whether this <see cref="IOperation"/> is inside a <see cref="IWitness"/>.
        /// </param>
        internal NotIfOp(bool isWit)
        {
            runWithTrue = false;
            isWitness = isWit;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IFOp"/> with the given <see cref="IOperation"/> arrays.
        /// </summary>
        /// <param name="ifBlockOps">The main array of operations to run after <see cref="OP.IF"/></param>
        /// <param name="elseBlockOps">The alternative set of operations to run if the previous expresions didn't run.</param>
        /// <param name="isWit">
        /// Indicates whether this <see cref="IOperation"/> is inside a <see cref="IWitness"/>.
        /// </param>
        public NotIfOp(IOperation[] ifBlockOps, IOperation[] elseBlockOps, bool isWit)
        {
            runWithTrue = false;
            isWitness = isWit;
            mainOps = ifBlockOps ?? new IOperation[0];
            elseOps = elseBlockOps;
        }

        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NotIf;
    }
}
