// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base (abstract) class for locktime operations. Inherits from <see cref="BaseOperation"/> class.
    /// </summary>
    public abstract class LockTimeOpBase : BaseOperation
    {
        /// <summary>
        /// The locktime value converted from the top stack item (without popping it)
        /// </summary>
        protected long lt;

        /// <summary>
        /// Converts the top stack item to locktime without removing it. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        protected bool TrySetLockTime(IOpData opData, out Errors error)
        {
            if (opData.ItemCount < 1)
            {
                error = Errors.NotEnoughStackItems;
                return false;
            }

            // The two locktime OPs (CheckLocktimeVerify and CheckSequenceVerify) used to be NOPs. NOPs don't do anything.
            // For backward compatibility of the softfork, Run() Peeks at the top item of the stack instead of Poping it.
            byte[] data = opData.Peek();

            if (!TryConvertToLong(data, out lt, opData.StrictNumberEncoding, 5))
            {
                error = Errors.InvalidStackNumberFormat;
                return false;
            }

            if (lt < 0)
            {
                error = Errors.NegativeLocktime;
                return false;
            }

            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Operation to check locktime of the transaction versus the value extracted from the script according to BIP-65.
    /// </summary>
    public class CheckLocktimeVerifyOp : LockTimeOpBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckLocktimeVerify;

        /// <summary>
        /// If BIP-65 is not enabled it will run as a <see cref="OP.NOP"/> (pre soft-fork) otherwise removes top stack item
        /// and converts it to a locktime to compare with the transction's <see cref="LockTime"/>.
        /// Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (!opData.IsBip65Enabled)
            {
                error = Errors.None;
                return true;
            }

            if (!TrySetLockTime(opData, out error))
            {
                return false;
            }

            return opData.CompareLocktimes(lt, out error);
        }
    }



    /// <summary>
    /// Operation to check locktime of the transaction versus the value extracted from the script according to BIP-112.
    /// </summary>
    public class CheckSequenceVerifyOp : LockTimeOpBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckSequenceVerify;

        /// <summary>
        /// If BIP-112 is not enabled it will run as a <see cref="OP.NOP"/> (pre soft-fork) otherwise removes top stack item
        /// and converts it to a locktime to compare with the transction's <see cref="LockTime"/>.
        /// Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out Errors error)
        {
            if (!opData.IsBip112Enabled)
            {
                error = Errors.None;
                return true;
            }

            // The value is not "locktime" but the process is the same so we use the same method.
            if (!TrySetLockTime(opData, out error))
            {
                return false;
            }

            return opData.CompareSequences(lt, out error);
        }
    }
}
