// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation that transfers one item from main stack to the alt stack.
    /// </summary>
    public class ToAltStackOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.ToAltStack;

        /// <summary>
        /// Removes top stack item and puts it in alt-stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 1)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            opData.AltPush(opData.Pop());

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation that transfers one item from alt stack to the main stack.
    /// </summary>
    public class FromAltStackOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.FromAltStack;

        /// <summary>
        /// Removes top alt-stack item and puts it in stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.AltItemCount < 1)
            {
                error = Errors.NotEnoughAltStackItems;
                return false;
            }

            opData.Push(opData.AltPop());

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation that removes the top 2 stack items.
    /// </summary>
    public class DROP2Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.DROP2;

        /// <summary>
        /// Removes (discards) top two stack items. Return value indicates success.
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

            // Throw away the 2 items
            _ = opData.Pop(2);

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation that duplicates the top 2 stack items.
    /// </summary>
    public class DUP2Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.DUP2;

        /// <summary>
        /// Duplicates top two stack items. Return value indicates success.
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

            opData.Push(opData.Peek(2));
            return CheckItemCount(opData, out error);
        }
    }



    /// <summary>
    /// Operation that duplicates the top 3 stack items.
    /// </summary>
    public class DUP3Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.DUP3;

        /// <summary>
        /// Duplicates top three stack items. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 3)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            opData.Push(opData.Peek(3));
            return CheckItemCount(opData, out error);
        }
    }



    /// <summary>
    /// Operation copies two items to the top (x1 x2 x3 x4 -> x1 x2 x3 x4 x1 x2).
    /// </summary>
    public class OVER2Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.OVER2;

        /// <summary>
        /// Copies 2 items from stack to top of the stack like this: x1 x2 x3 x4 -> x1 x2 x3 x4 x1 x2
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 4)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            byte[] data1 = opData.PeekAtIndex(3);
            byte[] data2 = opData.PeekAtIndex(2);

            opData.Push(new byte[2][] { data1, data2 });
            return CheckItemCount(opData, out error);
        }
    }



    /// <summary>
    /// Operation moves two items to the top (x1 x2 x3 x4 x5 x6 -> x3 x4 x5 x6 x1 x2).
    /// </summary>
    public class ROT2Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.ROT2;

        /// <summary>
        /// Moves 2 items from stack to top of the stack like this: x1 x2 x3 x4 x5 x6 -> x3 x4 x5 x6 x1 x2
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 6)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            byte[][] data = opData.Pop(6);
            // (x0 x1 x2 x3 x4 x5 -> x2 x3 x4 x5 x0 x1)
            opData.Push(new byte[6][] { data[2], data[3], data[4], data[5], data[0], data[1] });

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation that swaps top two pairs of items (x1 x2 x3 x4 -> x3 x4 x1 x2).
    /// </summary>
    public class SWAP2Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.SWAP2;

        /// <summary>
        /// Swaps top two item pairs on top of the stack: x1 x2 x3 x4 -> x3 x4 x1 x2
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 4)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            byte[][] data = opData.Pop(4);
            // x0 x1 x2 x3 -> x2 x3 x0 x1
            opData.Push(new byte[4][] { data[2], data[3], data[0], data[1] });

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation that duplicates top stack item if its value is not zero.
    /// </summary>
    public class IfDupOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.IfDup;

        /// <summary>
        /// Duplicates top stack item if its value is not 0. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 1)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            byte[] data = opData.Peek();
            if (IsNotZero(data))
            {
                opData.Push(data);
            }

            return CheckItemCount(opData, out error);
        }
    }



    /// <summary>
    /// Operation that pushes the number of stack items onto the stack.
    /// </summary>
    public class DEPTHOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.DEPTH;

        /// <summary>
        /// Pushes the number of stack items onto the stack. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            opData.Push(IntToByteArray(opData.ItemCount));
            return CheckItemCount(opData, out error);
        }
    }



    /// <summary>
    /// Operation that removes the top stack item.
    /// </summary>
    public class DROPOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.DROP;

        /// <summary>
        /// Removes the top stack item. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 1)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            // Throw away the top stack item
            _ = opData.Pop();

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation that duplicates the top stack item.
    /// </summary>
    public class DUPOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.DUP;

        /// <summary>
        /// Duplicates the top stack item. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 1)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            opData.Push(opData.Peek());
            return CheckItemCount(opData, out error);
        }
    }



    /// <summary>
    /// Operation that removes the item before last from the stack.
    /// </summary>
    public class NIPOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NIP;

        /// <summary>
        /// Removes the second item from top of stack. Return value indicates success.
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

            // Throw away the popped value
            _ = opData.PopAtIndex(1);

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation that the item before last to the top of the stack.
    /// </summary>
    public class OVEROp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.OVER;

        /// <summary>
        /// Copies the second item from top of the stack to the top. Return value indicates success.
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

            byte[] data = opData.PeekAtIndex(1);
            opData.Push(data);

            return CheckItemCount(opData, out error);
        }
    }



    /// <summary>
    /// Operation to pick an item from specified index and copies it to the top of the stack.
    /// </summary>
    public class PICKOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.PICK;

        /// <summary>
        /// Copies the nth item from top of the stack to the top. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            // At least 2 items is needed. 1 telling us the index and the other is the item to copy
            if (opData.ItemCount < 2)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            byte[] data = opData.Pop();
            if (!TryConvertToLong(data, out long n, true)) // TODO: set isStrict field based on BIP62
            {
                error = Errors.InvalidStackNumberFormat;
                return false;
            }

            if (n < 0)
            {
                error = Errors.NegativeStackInteger;
                return false;
            }
            // 'n' is index so it can't be equal to ItemCount
            if (opData.ItemCount <= n)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            opData.Push(opData.PeekAtIndex((int)n));

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation to pick an item from specified index and move it to the top of the stack.
    /// </summary>
    public class ROLLOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.ROLL;

        /// <summary>
        /// Moves the nth item from top of the stack to the top. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            // At least 2 items is needed. 1 telling us the index and the other is the item to move
            if (opData.ItemCount < 2)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            byte[] data = opData.Pop();
            // TODO: set isStrict field based on BIP62
            if (!TryConvertToLong(data, out long n, true))
            {
                error = Errors.InvalidStackNumberFormat;
                return false;
            }
            if (n < 0)
            {
                error = Errors.NegativeStackInteger;
                return false;
            }
            // 'n' is index so it can't be equal to ItemCount
            if (opData.ItemCount <= n)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            opData.Push(opData.PopAtIndex((int)n));

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation to rotate the top 3 stack items.
    /// </summary>
    public class ROTOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.ROT;

        /// <summary>
        /// Rotates top 3 items on top of the stack to the left. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 3)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            byte[][] data = opData.Pop(3);
            // (x0 x1 x2 -> x1 x2 x0)
            opData.Push(new byte[3][] { data[1], data[2], data[0] });

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation to swap the top 2 stack items.
    /// </summary>
    public class SWAPOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.SWAP;

        /// <summary>
        /// Swaps the position of top 2 items on top of the stack. Return value indicates success.
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

            byte[][] data = opData.Pop(2);
            opData.Push(new byte[2][] { data[1], data[0] });

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation to tuck the top stack item before item before last.
    /// </summary>
    public class TUCKOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.TUCK;

        /// <summary>
        /// The item at the top of the stack is copied and inserted before the second-to-top item. Return value indicates success.
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

            byte[] data = opData.Peek();
            opData.Insert(data, 2);

            return CheckItemCount(opData, out error);
        }
    }
}
