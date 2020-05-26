// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    internal class MockOpBase : IOperation
    {
        public MockOpBase(OP opval) => OpValue = opval;

        public OP OpValue { get; }
        public virtual bool Run(IOpData opData, out string error) => throw new NotImplementedException();
        public virtual void WriteToStream(FastStream stream) => throw new NotImplementedException();
        public virtual void WriteToStreamForSigning(FastStream stream, ReadOnlySpan<byte> sig) => throw new NotImplementedException();
        public virtual void WriteToStreamForSigning(FastStream stream, byte[][] sigs) => throw new NotImplementedException();
        public virtual void WriteToStreamForSigningSegWit(FastStream stream) => throw new NotImplementedException();
    }


    internal class MockOp : MockOpBase
    {
        public MockOp(bool runSuccessful, string errorIfFail, OP opValue = (OP)255) : base(opValue)
        {
            succeed = runSuccessful;
            errMsg = errorIfFail;
        }

        private readonly string errMsg;
        private readonly bool succeed;

        public override bool Run(IOpData opData, out string error)
        {
            if (succeed)
            {
                error = null;
                return true;
            }
            else
            {
                error = errMsg;
                return false;
            }
        }
    }


    internal class MockSerializableOp : MockOpBase
    {
        public MockSerializableOp(byte[] ba) : base((OP)255)
        {
            data = ba;
        }

        private readonly byte[] data;

        public override void WriteToStream(FastStream stream) => stream.Write(data);
    }
}
