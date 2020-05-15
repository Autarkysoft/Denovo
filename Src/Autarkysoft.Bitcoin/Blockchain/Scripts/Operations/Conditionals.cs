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
        /// Each IF operation first pops an item from the stack and converts it to a boolean. 
        /// <see cref="OP.IF"/> expressions run if that item was true, while
        /// <see cref="OP.NotIf"/> expressions run if that item was false;
        /// Otherwise the <see cref="OP.ELSE"/> expressions run if they exist.
        /// </summary>
        protected bool runWithTrue;

        /// <summary>
        /// The main operations under the <see cref="OP.IF"/> or <see cref="OP.NotIf"/> OP.
        /// <para/>Note that this must never be null but it can be empty (constructor must set it correctly)
        /// </summary>
        protected internal IOperation[] mainOps;
        /// <summary>
        /// The "else" operations under the <see cref="OP.ELSE"/> op.
        /// <para/>Note that this can be null but null and empty array have different meaning (null array won't write 
        /// OP_ELSE to stream while empty array writes OP_ELSE and follows it up with OP_EndIf)
        /// </summary>
        protected internal IOperation[] elseOps;


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

            if (!opData.CheckConditionalOpBool(topItem))
            {
                error = "True/False item popped by conditional OPs must be strict.";
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
        /// Returns accurate number of <see cref="OP.CheckSig"/>, <see cref="OP.CheckSigVerify"/>, <see cref="OP.CheckMultiSig"/>
        /// and <see cref="OP.CheckMultiSigVerify"/> operations in this instance without a full script evaluation.
        /// </summary>
        /// <returns>Number of "SigOps"</returns>
        public int CountSigOps()
        {
            int res = 0;
            for (int i = 0; i < mainOps.Length; i++)
            {
                if (mainOps[i] is CheckSigOp || mainOps[i] is CheckSigVerifyOp)
                {
                    res++;
                }
                else if (mainOps[i] is CheckMultiSigOp || mainOps[i] is CheckMultiSigVerifyOp)
                {
                    if (i > 0 && mainOps[i - 1] is PushDataOp push && (push.OpValue >= OP._1 && push.OpValue <= OP._16))
                    {
                        res += (int)push.OpValue - 0x50;
                    }
                    else
                    {
                        res += 20;
                    }
                }
                else if (mainOps[i] is IfElseOpsBase conditional)
                {
                    res += conditional.CountSigOps();
                }
            }
            if (elseOps != null)
            {
                for (int i = 0; i < elseOps.Length; i++)
                {
                    if (elseOps[i] is CheckSigOp || elseOps[i] is CheckSigVerifyOp)
                    {
                        res++;
                    }
                    else if (elseOps[i] is CheckMultiSigOp || elseOps[i] is CheckMultiSigVerifyOp)
                    {
                        if (i > 0 && elseOps[i - 1] is PushDataOp push && (push.OpValue >= OP._1 && push.OpValue <= OP._16))
                        {
                            res += (int)push.OpValue - 0x50;
                        }
                        else
                        {
                            res += 20;
                        }
                    }
                    else if (elseOps[i] is IfElseOpsBase conditional)
                    {
                        res += conditional.CountSigOps();
                    }
                }
            }

            return res;
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
                else if (op is IfElseOpsBase conditional && conditional.HasExecutedCodeSeparator())
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
                    else if (op is IfElseOpsBase conditional && conditional.HasExecutedCodeSeparator())
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


        private int GetLastExecutedCSIndexMain()
        {
            for (int i = mainOps.Length - 1; i >= 0; i--)
            {
                if (mainOps[i] is CodeSeparatorOp cs && cs.IsExecuted)
                {
                    return i;
                }
                else if (mainOps[i] is IfElseOpsBase conditional && conditional.HasExecutedCodeSeparator())
                {
                    return i;
                }
            }
            return -1;
        }
        private int GetLastExecutedCSIndexElse()
        {
            if (elseOps != null)
            {
                for (int i = elseOps.Length - 1; i >= 0; i--)
                {
                    if (elseOps[i] is CodeSeparatorOp cs && cs.IsExecuted)
                    {
                        return i;
                    }
                    else if (elseOps[i] is IfElseOpsBase conditional && conditional.HasExecutedCodeSeparator())
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        /// <inheritdoc/>
        public override void WriteToStreamForSigning(FastStream stream, ReadOnlySpan<byte> sig)
        {
            int elseCSep = GetLastExecutedCSIndexElse();
            if (elseCSep >= 0)
            {
                WriteElseForSigning(stream, elseCSep, sig);
            }
            else
            {
                int mainCSep = GetLastExecutedCSIndexMain();
                if (mainCSep >= 0)
                {
                    WriteMainForSigning(stream, mainCSep, sig);
                    if (elseOps != null)
                    {
                        stream.Write((byte)OP.ELSE);
                        WriteElseForSigning(stream, 0, sig);
                    }
                }
                else
                {
                    // This branch is when there is either no OP_CodeSeparator or they weren't executed
                    // (eg. the whole IF/ELSE be in unexecuted branch of another IF/ELSE)
                    stream.Write((byte)OpValue);
                    WriteMainForSigning(stream, 0, sig);
                    if (elseOps != null)
                    {
                        stream.Write((byte)OP.ELSE);
                        WriteElseForSigning(stream, 0, sig);
                    }
                }
            }

            // Always end with OP_EndIf
            stream.Write((byte)OP.EndIf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteMainForSigning(FastStream stream, int start, ReadOnlySpan<byte> sig)
        {
            for (int i = start; i < mainOps.Length; i++)
            {
                mainOps[i].WriteToStreamForSigning(stream, sig);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteElseForSigning(FastStream stream, int start, ReadOnlySpan<byte> sig)
        {
            for (int i = start; i < elseOps.Length; i++)
            {
                elseOps[i].WriteToStreamForSigning(stream, sig);
            }
        }


        /// <inheritdoc/>
        public override void WriteToStreamForSigning(FastStream stream, byte[][] sigs)
        {
            int elseCSep = GetLastExecutedCSIndexElse();
            if (elseCSep >= 0)
            {
                WriteElseForSigning(stream, elseCSep, sigs);
            }
            else
            {
                int mainCSep = GetLastExecutedCSIndexMain();
                if (mainCSep >= 0)
                {
                    WriteMainForSigning(stream, mainCSep, sigs);
                    if (elseOps != null)
                    {
                        stream.Write((byte)OP.ELSE);
                        WriteElseForSigning(stream, 0, sigs);
                    }
                }
                else
                {
                    // This branch is when there is either no OP_CodeSeparator or they weren't executed
                    // (eg. the whole IF/ELSE be in unexecuted branch of another IF/ELSE)
                    stream.Write((byte)OpValue);
                    WriteMainForSigning(stream, 0, sigs);
                    if (elseOps != null)
                    {
                        stream.Write((byte)OP.ELSE);
                        WriteElseForSigning(stream, 0, sigs);
                    }
                }
            }

            // Always end with OP_EndIf
            stream.Write((byte)OP.EndIf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteMainForSigning(FastStream stream, int start, byte[][] sigs)
        {
            for (int i = start; i < mainOps.Length; i++)
            {
                mainOps[i].WriteToStreamForSigning(stream, sigs);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteElseForSigning(FastStream stream, int start, byte[][] sigs)
        {
            for (int i = start; i < elseOps.Length; i++)
            {
                elseOps[i].WriteToStreamForSigning(stream, sigs);
            }
        }


        /// <inheritdoc/>
        public override void WriteToStreamForSigningSegWit(FastStream stream)
        {
            int elseCSep = GetLastExecutedCSIndexElse();
            if (elseCSep >= 0)
            {
                WriteElseForSigningSegWit(stream, elseCSep);
            }
            else
            {
                int mainCSep = GetLastExecutedCSIndexMain();
                if (mainCSep >= 0)
                {
                    WriteMainForSigningSegWit(stream, mainCSep);
                    if (elseOps != null)
                    {
                        stream.Write((byte)OP.ELSE);
                        WriteElseForSigningSegWit(stream, 0);
                    }
                }
                else
                {
                    // This branch is when there is either no OP_CodeSeparator or they weren't executed
                    // (eg. the whole IF/ELSE be in unexecuted branch of another IF/ELSE)
                    stream.Write((byte)OpValue);
                    WriteMainForSigningSegWit(stream, 0);
                    if (elseOps != null)
                    {
                        stream.Write((byte)OP.ELSE);
                        WriteElseForSigningSegWit(stream, 0);
                    }
                }
            }

            // End with OP_EndIf
            stream.Write((byte)OP.EndIf);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteMainForSigningSegWit(FastStream stream, int start)
        {
            for (int i = start; i < mainOps.Length; i++)
            {
                mainOps[i].WriteToStreamForSigningSegWit(stream);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteElseForSigningSegWit(FastStream stream, int start)
        {
            for (int i = start; i < elseOps.Length; i++)
            {
                elseOps[i].WriteToStreamForSigningSegWit(stream);
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
        internal IFOp()
        {
            runWithTrue = true;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IFOp"/> with the given <see cref="IOperation"/> arrays.
        /// </summary>
        /// <param name="ifBlockOps">The main array of operations to run after <see cref="OP.IF"/></param>
        /// <param name="elseBlockOps">The alternative set of operations to run if the previous expresions didn't run.</param>
        public IFOp(IOperation[] ifBlockOps, IOperation[] elseBlockOps)
        {
            runWithTrue = true;
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
        internal NotIfOp()
        {
            runWithTrue = false;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IFOp"/> with the given <see cref="IOperation"/> arrays.
        /// </summary>
        /// <param name="ifBlockOps">The main array of operations to run after <see cref="OP.IF"/></param>
        /// <param name="elseBlockOps">The alternative set of operations to run if the previous expresions didn't run.</param>
        public NotIfOp(IOperation[] ifBlockOps, IOperation[] elseBlockOps)
        {
            runWithTrue = false;
            mainOps = ifBlockOps ?? new IOperation[0];
            elseOps = elseBlockOps;
        }

        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NotIf;
    }
}
