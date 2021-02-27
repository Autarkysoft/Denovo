// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Operation to perform RIPEMD-160 hash on top stack item.
    /// </summary>
    public class RipeMd160Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public sealed override OP OpValue => OP.RIPEMD160;

        /// <summary>
        /// Replaces top stack item with its hash digest. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            using Ripemd160 hash = new Ripemd160();
            opData.Push(hash.ComputeHash(opData.Pop()));

            error = null;
            return true;
        }
    }


    /// <summary>
    /// Operation to perform SHA-1 hash on top stack item.
    /// </summary>
    public class Sha1Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public sealed override OP OpValue => OP.SHA1;

        /// <summary>
        /// Replaces top stack item with its hash digest. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            using Sha1 hash = new Sha1();
            opData.Push(hash.ComputeHash(opData.Pop()));

            error = null;
            return true;
        }
    }


    /// <summary>
    /// Operation to perform SHA-256 hash on top stack item.
    /// </summary>
    public class Sha256Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public sealed override OP OpValue => OP.SHA256;

        /// <summary>
        /// Replaces top stack item with its hash digest. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            using Sha256 hash = new Sha256();
            opData.Push(hash.ComputeHash(opData.Pop()));

            error = null;
            return true;
        }
    }


    /// <summary>
    /// Operation to perform SHA-256 then RIPEMD-160 hash on top stack item.
    /// </summary>
    public class Hash160Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public sealed override OP OpValue => OP.HASH160;

        /// <summary>
        /// Replaces top stack item with its hash digest. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            using Ripemd160Sha256 hash = new Ripemd160Sha256();
            opData.Push(hash.ComputeHash(opData.Pop()));

            error = null;
            return true;
        }
    }


    /// <summary>
    /// Operation to perform SHA-256 hash twice on top stack item.
    /// </summary>
    public class Hash256Op : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public sealed override OP OpValue => OP.HASH256;

        /// <summary>
        /// Replaces top stack item with its hash digest. Return value indicates success.
        /// </summary>
        /// <param name="opData">Data to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            using Sha256 hash = new Sha256();
            opData.Push(hash.ComputeHashTwice(opData.Pop()));

            error = null;
            return true;
        }
    }
}
