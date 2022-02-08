// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base (abstract) class for operations that can not be run 
    /// (should fail when <see cref="IOperation.Run(IOpData, out Errors)"/> is called).
    /// Implements the Run() method (to fail on call) and inherits from <see cref="BaseOperation"/> class.
    /// </summary>
    public abstract class NotRunableOps : BaseOperation
    {
        /// <summary>
        /// Fails when called.
        /// </summary>
        /// <remarks>
        /// There is an IOperation instance defined for OP.Reserved,... here because they can exist in a transaction 
        /// but they can not be run. For example in a not-executed IF branch.
        /// </remarks>
        /// <param name="opData">Stack object (won't be used)</param>
        /// <param name="error">Error message</param>
        /// <returns>False (always failing)</returns>
        public sealed override bool Run(IOpData opData, out Errors error)
        {
            error = Errors.NotRunableOp;
            return false;
        }
    }



    /// <summary>
    /// Reserved operation, will fail on running.
    /// </summary>
    public class ReservedOp : NotRunableOps
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.Reserved;
    }

    /// <summary>
    /// Removed operation, will fail on running.
    /// </summary>
    public class VEROp : NotRunableOps
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.VER;
    }

    /// <summary>
    /// Reserved operation, will fail on running.
    /// </summary>
    public class Reserved1Op : NotRunableOps
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.Reserved1;
    }

    /// <summary>
    /// Reserved operation, will fail on running.
    /// </summary>
    public class Reserved2Op : NotRunableOps
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.Reserved2;
    }
}
