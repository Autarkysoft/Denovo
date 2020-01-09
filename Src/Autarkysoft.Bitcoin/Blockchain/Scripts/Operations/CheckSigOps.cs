// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    public class CheckSigOp : BaseOperation
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
            if (opData.ItemCount < 2)
            {
                error = Err.OpNotEnoughItems;
                return false;
            }

            byte[] pubBa = opData.Pop();
            byte[] sigBa = opData.Pop();

            if (!Signature.TryRead(sigBa, out Signature sig, out error))
            {
                return false;
            }

            if (!PublicKey.TryRead(pubBa, out PublicKey pubK, out error))
            {
                return false;
            }

            byte[] toSign = opData.GetBytesToSign(sig.SigHash);

            bool b = opData.Calc.Verify(toSign, sig, pubK);

            opData.Push(b ? new byte[] { 1 } : new byte[0]);

            error = null;
            return true;
        }
    }

    public class CheckSigVerifyOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckSigVerify;

        public override bool Run(IOpData opData, out string error)
        {
            IOperation cs = new CheckSigOp();
            IOperation ver = new VerifyOp();

            if (!cs.Run(opData, out error))
            {
                return false;
            }

            return ver.Run(opData, out error);
        }
    }

    public class CheckMultiSigOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckMultiSig;

        public override bool Run(IOpData opData, out string error)
        {
            throw new NotImplementedException();
        }
    }

    public class CheckMultiSigVerifyOp : BaseOperation
    {
        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.CheckMultiSigVerify;

        public override bool Run(IOpData opData, out string error)
        {
            IOperation cms = new CheckMultiSigOp();
            IOperation ver = new VerifyOp();

            if (!cms.Run(opData, out error))
            {
                return false;
            }

            return ver.Run(opData, out error);
        }
    }
}
