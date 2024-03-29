﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation to check equality of top two stack items.
    /// </summary>
    public class EqualOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.EQUAL;

        /// <summary>
        /// Removes top two stack item and pushes the result of their equality check (true for equality and false otherwiwe) 
        /// onto the stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 2)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            ReadOnlySpan<byte> item1 = opData.Pop();
            ReadOnlySpan<byte> item2 = opData.Pop();

            opData.Push(item1.SequenceEqual(item2));

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation to check and verify equality of top two stack items.
    /// </summary>
    public class EqualVerifyOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.EqualVerify;

        /// <summary>
        /// Removes top two stack item checks their equality, only fails if not equal. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 2)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            ReadOnlySpan<byte> item1 = opData.Pop();
            ReadOnlySpan<byte> item2 = opData.Pop();

            if (item1.SequenceEqual(item2))
            {
                error = Errors.None;
                return true;
            }
            else
            {
                error = Errors.UnequalStackItems;
                return false;
            }
        }
    }
}
