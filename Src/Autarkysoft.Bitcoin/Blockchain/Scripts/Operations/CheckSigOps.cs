// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base (abstract) class for signle check signature operations.
    /// Inherits from <see cref="BaseOperation"/>.
    /// </summary>
    public abstract class CheckSigOpBase : BaseOperation
    {
        /// <summary>
        /// Removes top two stack items as public key and signature and calls <see cref="IOpData.Verify(Signature, PublicKey)"/>.
        /// Return value indicates success.
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public bool ExtractAndVerify(IOpData opData, out string error)
        {
            if (opData.ItemCount < 2)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            byte[][] values = opData.Pop(2);

            if (!Signature.TryRead(values[0], out Signature sig, out error))
            {
                return false;
            }

            if (!PublicKey.TryRead(values[1], out PublicKey pubK))
            {
                error = "Invalid public key format.";
                return false;
            }

            return opData.Verify(sig, pubK);
        }
    }


    /// <summary>
    /// Operation to check the transaction signature.
    /// </summary>
    public class CheckSigOp : CheckSigOpBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckSig;

        /// <summary>
        /// Removes top two stack items (signature and public key) and verifies the transaction signature.
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            bool b = ExtractAndVerify(opData, out error);
            if (error is null)
            {
                opData.Push(b ? new byte[] { 1 } : new byte[0]);
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    /// <summary>
    /// Operation to check the transaction signature.
    /// </summary>
    public class CheckSigVerifyOp : CheckSigOpBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckSigVerify;

        /// <summary>
        /// Same as <see cref="CheckSigOp"/> but runs <see cref="VerifyOp"/> afterwards.
        /// </summary>
        /// <inheritdoc/>
        public override bool Run(IOpData opData, out string error)
        {
            bool b = ExtractAndVerify(opData, out error);
            if (error is null)
            {
                if (b)
                {
                    return true;
                }
                else
                {
                    error = "Invalid signature.";
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }



    /// <summary>
    /// Base (abstract) class for multiple check signature operations.
    /// Inherits from <see cref="BaseOperation"/>.
    /// </summary>
    public abstract class CheckMultiSigOpBase : BaseOperation
    {
        /// <summary>
        /// Removes all needed items from the stack as public keys and signatures and calls 
        /// <see cref="IOpData.Verify(Signature[], PublicKey[])"/>.
        /// Return value indicates success.
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public bool ExtractAndVerify(IOpData opData, out string error)
        {
            // A multi-sig stack is (extra item, usually OP_0) + (m*sig) + (OP_m) + (n*pub) + (OP_n)
            // both m and n values are needed and the extra item is also mandatory. but since both m and n can be zero
            // the key[] and sig[] can be empty so the smallest stack item count should be 3 items [OP_0 (m=0) (n=0)]
            if (opData.ItemCount < 3)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            byte[] nBa = opData.Pop();
            if (!TryConvertToLong(nBa, out long n, true, 1))
            {
                error = "Invalid number (n) format";
                return false;
            }
            if (n < 0 || n > 20)
            {
                error = "Invalid number of public keys in multi-sig.";
                return false;
            }

            // By knowing n we know the number of public keys and the "index" of m to be popped
            // eg. indes:item => n=3 => 3:m 2:pub1 1:pub2 0:pub3    n=2 => 2:m 1:pub1 0:pub2
            if (opData.ItemCount < n + 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            byte[] mBa = opData.PopAtIndex((int)n);
            if (!TryConvertToLong(mBa, out long m, true, 1))
            {
                error = "Invalid number (m) format";
                return false;
            }
            if (m < 0 || m > n)
            {
                error = "Invalid number of signatures in multi-sig.";
                return false;
            }

            // Note that m and n are already popped (removed) from the stack so it looks like this:
            // (extra item, usually OP_0) + (m*sig) + (n*pub)
            if (opData.ItemCount < n + m + 1)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            PublicKey[] pubs = new PublicKey[n];
            Signature[] sigs = new Signature[m];

            // TODO: benchmark using PopMulti versus Pop
            byte[][] allPubs = opData.Pop(pubs.Length);
            for (int i = 0; i < pubs.Length; i++)
            {
                if (!PublicKey.TryRead(allPubs[allPubs.Length - 1 - i], out pubs[i]))
                {
                    error = "Invalid public key.";
                    return false;
                }
            }
            // TODO: same benchmark as above needed
            byte[][] allSigs = opData.Pop(sigs.Length);
            for (int i = 0; i < sigs.Length; i++)
            {
                if (!Signature.TryRead(allSigs[allSigs.Length - 1 - i], out sigs[i], out string err))
                {
                    error = $"Invalid signature {err}";
                    return false;
                }
            }

            // Handle bitcoin-core bug (has to pop 1 extra item)
            byte[] garbage = opData.Pop();
            if (!opData.CheckMultiSigGarbage(garbage))
            {
                error = "The extra item must have been OP_0.";
                return false;
            }

            error = null;
            return opData.Verify(sigs, pubs);
        }
    }


    /// <summary>
    /// Operation to check multiple transaction signatures.
    /// </summary>
    public class CheckMultiSigOp : CheckMultiSigOpBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckMultiSig;

        /// <summary>
        /// Evaluation starts by popping top stack item in the following pattern: 
        /// [garbage] [between 0 to m signatures] [OP_m between 0 to n] [n publickeys] [OP_n between 0 to ?]
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            bool b = ExtractAndVerify(opData, out error);
            if (error is null)
            {
                opData.Push(b ? new byte[] { 1 } : new byte[0]);
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    /// <summary>
    /// Operation to check multiple transaction signatures.
    /// </summary>
    public class CheckMultiSigVerifyOp : CheckMultiSigOpBase
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckMultiSigVerify;

        /// <summary>
        /// Same as <see cref="CheckMultiSigOp"/> but runs <see cref="VerifyOp"/> afterwards.
        /// </summary>
        /// <inheritdoc/>
        public override bool Run(IOpData opData, out string error)
        {
            bool b = ExtractAndVerify(opData, out error);
            if (error is null)
            {
                if (b)
                {
                    return true;
                }
                else
                {
                    error = "Invalid signature.";
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
