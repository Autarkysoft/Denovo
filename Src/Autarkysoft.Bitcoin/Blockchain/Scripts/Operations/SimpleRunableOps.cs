// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base (abstract) class for operations that don't do anything with the stack.
    /// Implements the Run() method and inherits from <see cref="BaseOperation"/> class.
    /// </summary>
    public abstract class SimpleRunableOpsBase : BaseOperation
    {
        /// <summary>
        /// Doesn't do anything.
        /// </summary>
        /// <param name="opData">Stack object (won't be used)</param>
        /// <param name="error">Error message (always <see cref="Errors.None"/>)</param>
        /// <returns>True (always successful)</returns>
        public sealed override bool Run(IOpData opData, out Errors error)
        {
            error = Errors.None;
            return true;
        }
    }



    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOPOp : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP1Op : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP1;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP4Op : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP4;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP5Op : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP5;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP6Op : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP6;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP7Op : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP7;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP8Op : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP8;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP9Op : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP9;
    }

    /// <summary>
    /// Ignored operation (it doesn't do anything). Could be used for future soft-forks.
    /// </summary>
    public class NOP10Op : SimpleRunableOpsBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NOP10;
    }

}
