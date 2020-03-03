// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public abstract class MockScriptBase : IScript
    {
        public virtual bool IsWitness => throw new NotImplementedException();
        public virtual ScriptType ScriptType => throw new NotImplementedException();
        public virtual IOperation[] OperationList
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual void Serialize(FastStream stream)
        {
            throw new NotImplementedException();
        }

        public virtual void ToByteArray(FastStream stream)
        {
            throw new NotImplementedException();
        }

        public virtual byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }

        public virtual bool TryDeserialize(FastStreamReader stream, out string error)
        {
            throw new NotImplementedException();
        }
    }



    public class MockSerializableScript : MockScriptBase
    {
        public MockSerializableScript(byte[] serializedResult, byte streamFirstByte)
        {
            serBa = new byte[serializedResult.Length + 1];
            Buffer.BlockCopy(serializedResult, 0, serBa, 1, serializedResult.Length);
            serBa[0] = streamFirstByte;

            ba = serializedResult;
        }

        private readonly byte[] serBa;
        private readonly byte[] ba;

        public override void Serialize(FastStream stream)
        {
            stream.Write(serBa);
        }

        public override void ToByteArray(FastStream stream)
        {
            stream.Write(ba);
        }

        public override byte[] ToByteArray()
        {
            return ba;
        }
    }


    public class MockSerializablePubScript : MockSerializableScript, IPubkeyScript
    {
        public MockSerializablePubScript(PubkeyScriptType typeResult, byte[] serializedResult, byte streamFirstByte)
            : base(serializedResult, streamFirstByte)
        {
            typeToReturn = typeResult;
        }

        public MockSerializablePubScript(byte[] serializedResult, byte streamFirstByte)
            : this(PubkeyScriptType.Unknown, serializedResult, streamFirstByte)
        {
        }

        private readonly PubkeyScriptType typeToReturn;

        public PubkeyScriptType GetPublicScriptType() => typeToReturn;
    }


    public class MockSerializableSigScript : MockSerializableScript, ISignatureScript
    {
        public MockSerializableSigScript(byte[] serializedResult, byte streamFirstByte) : base(serializedResult, streamFirstByte)
        {
        }

        public void SetToEmpty() => throw new NotImplementedException();
        public void SetToMultiSig(Signature sig, PublicKey pub, IRedeemScript redeem, ITransaction tx, ITransaction prevTx, int inputIndex) => throw new NotImplementedException();
        public void SetToP2PK(Signature sig) => throw new NotImplementedException();
        public void SetToP2PKH(Signature sig, PublicKey pubKey, bool useCompressed) => throw new NotImplementedException();
        public void SetToP2SH_P2WPKH(IRedeemScript redeem) => throw new NotImplementedException();
        public void SetToP2SH_P2WPKH(PublicKey pubKey) => throw new NotImplementedException();
        public void SetToP2SH_P2WSH(IRedeemScript redeem) => throw new NotImplementedException();
        public void SetToCheckLocktimeVerify(Signature sig, IRedeemScript redeem) => throw new NotImplementedException();
    }


    public class MockSerializableRedeemScript : MockSerializableScript, IRedeemScript
    {
        public MockSerializableRedeemScript(RedeemScriptType typeResult, byte[] serializedResult, byte streamFirstByte)
            : base(serializedResult, streamFirstByte)
        {
            typeToReturn = typeResult;
        }

        public MockSerializableRedeemScript(byte[] serializedResult, byte streamFirstByte)
            : this(RedeemScriptType.Unknown, serializedResult, streamFirstByte)
        {
        }

        private readonly RedeemScriptType typeToReturn;

        public RedeemScriptType GetRedeemScriptType() => typeToReturn;
    }


    public class MockDeserializableScript : MockScriptBase
    {
        /// <summary>
        /// Use this mock object to mock deserialization. It will check current index in stream and fails if it is not expected.
        /// Also it can return true or false depending on whether error is null or not.
        /// </summary>
        /// <param name="streamIndex">Expected current stream index</param>
        /// <param name="bytesToRead">Number of bytes to read (move stream index forward)</param>
        /// <param name="errorToReturn">Custom error to return (null returns true, otherwise false)</param>
        public MockDeserializableScript(int streamIndex, int bytesToRead, string errorToReturn = null)
        {
            expectedIndex = streamIndex;
            retError = errorToReturn;
            this.bytesToRead = bytesToRead;
        }

        private readonly int expectedIndex;
        private readonly int bytesToRead;
        private readonly string retError;

        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            int actualIndex = stream.GetCurrentIndex();
            Assert.Equal(expectedIndex, actualIndex);

            if (!stream.TryReadByteArray(bytesToRead, out _))
            {
                Assert.True(false, "Stream doesn't have enough bytes.");
            }

            error = retError;
            return string.IsNullOrEmpty(retError);
        }
    }


    public class MockDeserializablePubScript : MockDeserializableScript, IPubkeyScript
    {
        public MockDeserializablePubScript(PubkeyScriptType pubT, int streamIndex, int bytesToRead, string errorToReturn = null)
            : base(streamIndex, bytesToRead, errorToReturn)
        {
            typeToReturn = pubT;
        }
        public MockDeserializablePubScript(int streamIndex, int bytesToRead, string errorToReturn = null)
            : this(PubkeyScriptType.Unknown, streamIndex, bytesToRead, errorToReturn)
        {
        }

        private readonly PubkeyScriptType typeToReturn;

        public PubkeyScriptType GetPublicScriptType() => typeToReturn;
    }


    public class MockDeserializableSigScript : MockDeserializableScript, ISignatureScript
    {
        public MockDeserializableSigScript(int streamIndex, int bytesToRead, string errorToReturn = null)
            : base(streamIndex, bytesToRead, errorToReturn)
        {
        }

        public void SetToEmpty() => throw new NotImplementedException();
        public void SetToMultiSig(Signature sig, PublicKey pub, IRedeemScript redeem, ITransaction tx, ITransaction prevTx, int inputIndex) => throw new NotImplementedException();
        public void SetToP2PK(Signature sig) => throw new NotImplementedException();
        public void SetToP2PKH(Signature sig, PublicKey pubKey, bool useCompressed) => throw new NotImplementedException();
        public void SetToP2SH_P2WPKH(IRedeemScript redeem) => throw new NotImplementedException();
        public void SetToP2SH_P2WPKH(PublicKey pubKey) => throw new NotImplementedException();
        public void SetToP2SH_P2WSH(IRedeemScript redeem) => throw new NotImplementedException();
        public void SetToCheckLocktimeVerify(Signature sig, IRedeemScript redeem) => throw new NotImplementedException();
    }
}
