// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation to verify if the top stack item is true.
    /// </summary>
    public class VerifyOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.VERIFY;

        /// <summary>
        /// Removes top stack item and only passes if its value is true. Return value indicates success.
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

            // Check the top stack value, only fail if False
            bool b = IsNotZero(opData.Pop());
            if (!b)
            {
                error = Errors.FalseTopStackItem;
                return false;
            }

            error = Errors.None;
            return true;
        }
    }
}
