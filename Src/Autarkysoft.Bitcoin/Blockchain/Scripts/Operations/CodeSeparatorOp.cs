// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Does nothing (but affects how hash for signatures are produced).
    /// </summary>
    public class CodeSeparatorOp : BaseOperation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CodeSeparatorOp"/>.
        /// </summary>
        public CodeSeparatorOp()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CodeSeparatorOp"/> with the given position.
        /// </summary>
        /// <param name="pos">Position of this <see cref="CodeSeparatorOp"/> inside a Taproot script</param>
        public CodeSeparatorOp(uint pos)
        {
            Position = pos;
        }


        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CodeSeparator;

        /// <summary>
        /// Indicates whether this operation was executed or not.
        /// </summary>
        public bool IsExecuted = false;

        /// <summary>
        /// Returns the position of this <see cref="OP.CodeSeparator"/> inside the Taproot script.
        /// </summary>
        public uint Position { get; }

        /// <summary>
        /// Does nothing. 
        /// <para/> Under the hood, it changes the <see cref="IsExecuted"/> value to false which would affect how
        /// the script containing this operation is going to be serialized for signing.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Always null</param>
        /// <returns>Always true</returns>
        public override bool Run(IOpData opData, out string error)
        {
            IsExecuted = true;
            opData.CodeSeparatorPosition = Position;
            error = null;
            return true;
        }

        /// <summary>
        /// Writes nothing to stream since <see cref="OP.CodeSeparator"/>s are not included in scripts while signing.
        /// </summary>
        /// <param name="stream">Doesn't write anything to stream</param>
        /// <param name="sig">Doesn't write anything to stream</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteToStreamForSigning(FastStream stream, ReadOnlySpan<byte> sig) { }

        /// <summary>
        /// Writes nothing to stream since <see cref="OP.CodeSeparator"/>s are not included in scripts while signing.
        /// </summary>
        /// <param name="stream">Doesn't write anything to stream</param>
        /// <param name="sigs">Doesn't write anything to stream</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteToStreamForSigning(FastStream stream, byte[][] sigs) { }

        /// <summary>
        /// Writes <see cref="OP.CodeSeparator"/> to stream only if it was not executed.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public override void WriteToStreamForSigningSegWit(FastStream stream)
        {
            if (!IsExecuted)
            {
                stream.Write((byte)OpValue);
            }
        }
    }
}
