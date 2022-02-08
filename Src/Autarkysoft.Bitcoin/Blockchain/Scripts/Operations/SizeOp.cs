// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation to push the size of the top stack item to the stack.
    /// </summary>
    public class SizeOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.SIZE;

        /// <summary>
        /// Pushes the size of the top stack item to the stack without removing the item. Return value indicates success.
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

            byte[] temp = opData.Peek();
            opData.Push(IntToByteArray(temp.Length));

            return CheckItemCount(opData, out error);
        }
    }
}
