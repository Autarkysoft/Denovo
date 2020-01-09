// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base (abstract) class for locktime operations. Inherits from <see cref="BaseOperation"/> class.
    /// </summary>
    public abstract class LockTimeOp : BaseOperation
    {
        /// <summary>
        /// The locktime value converted from the top stack item (without popping it)
        /// </summary>
        protected long lt;

        /// <summary>
        /// Converts the top stack item to locktime without removing it. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        protected bool TrySetLockTime(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            // The two locktime OPs (CheckLocktimeVerify and CheckSequenceVerify) used to be NOPs. NOPs don't do anything.
            // For backward compatibility of the softfork, Run() Peeks at the top item of the stack instead of Poping it.
            byte[] data = opData.Peek();

            // TODO: move this check to converter
            // (for locktimes max length is 5 for others it is 4)
            if (data.Length > 5)
            {
                error = "Data length for locktimes can not be bigger than 5.";
                return false;
            }

            if (!TryConvertToLong(data, out lt, true))
            {
                error = "Invalid number format.";
                return false;
            }

            if (lt < 0)
            {
                error = "Locktime can not be negative.";
                return false;
            }

            error = null;
            return true;
        }
    }



    public class CheckLocktimeVerifyOp : LockTimeOp
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckLocktimeVerify;

        public override bool Run(IOpData opData, out string error)
        {
            if (!TrySetLockTime(opData, out error))
            {
                return false;
            }

            // Compare to tx.locktime (we assume it is valid and skip this!)
            // TODO: change this for this tool if transactions were set inside IOpdata one day...

            error = null;
            return true;
        }
    }



    public class CheckSequenceVerifyOp : LockTimeOp
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckSequenceVerify;

        public override bool Run(IOpData opData, out string error)
        {
            if (!TrySetLockTime(opData, out error))
            {
                return false;
            }

            // Compare to tx.locktime, as relative locktime (we assume it is valid and skip this!)
            // TODO: change this for this tool if transactions were set inside IOpdata one day...

            error = null;
            return true;
        }
    }
}
