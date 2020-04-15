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
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public abstract class MockTxBase : ITransaction
    {
        public virtual int Version { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual TxIn[] TxInList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual TxOut[] TxOutList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IWitness[] WitnessList
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public virtual LockTime LockTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool IsVerified { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SigOpCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual byte[] GetTransactionHash() => throw new NotImplementedException();
        public virtual string GetTransactionId() => throw new NotImplementedException();
        public virtual string GetWitnessTransactionId() => throw new NotImplementedException();
        public virtual void Serialize(FastStream stream) => throw new NotImplementedException();
        public virtual bool TryDeserialize(FastStreamReader stream, out string error) => throw new NotImplementedException();
        public byte[] SerializeForSigning(IOperation[] ops, int inputIndex, SigHashType sht, ReadOnlySpan<byte> sig) 
            => throw new NotImplementedException();
        public byte[] SerializeForSigningSegWit(byte[] prevOutScript, int inputIndex, ulong amount, SigHashType sht) 
            => throw new NotImplementedException();
        public virtual byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht)
            => throw new NotImplementedException();
        public virtual byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht, IRedeemScript redeemScript)
            => throw new NotImplementedException();
        public virtual void WriteScriptSig(Signature sig, PublicKey pubKey, ITransaction prevTx, int inputIndex, IRedeemScript redeem)
            => throw new NotImplementedException();
    }



    public class MockTxIdTx : MockTxBase
    {
        public MockTxIdTx(byte[] txHashToReturn)
        {
            TxHash = txHashToReturn;
        }

        public MockTxIdTx(string txIdToReturn) : this(Helper.HexToBytes(txIdToReturn, true))
        {
        }


        private readonly byte[] TxHash;


        public override byte[] GetTransactionHash()
        {
            if (TxHash == null)
            {
                Assert.True(false, "Mock transaction doesn't have any tx hash set.");
            }
            return TxHash;
        }

        public override string GetTransactionId()
        {
            if (TxHash == null)
            {
                Assert.True(false, "Mock transaction doesn't have any tx hash set.");
            }
            return Helper.BytesToHex(TxHash.Reverse().ToArray());
        }
    }



    public class MockSerializableTx : MockTxBase
    {
        public MockSerializableTx(byte[] serializedResult)
        {
            ba = serializedResult;
        }

        private readonly byte[] ba;

        public override void Serialize(FastStream stream)
        {
            stream.Write(ba);
        }
    }



    public class MockDeserializableTx : MockTxBase
    {
        /// <summary>
        /// Use this mock object to mock deserialization. It will check current index in stream and fails if it is not expected.
        /// Also it can return true or false depending on whether error is null or not.
        /// </summary>
        /// <param name="streamIndex">Expected current stream index</param>
        /// <param name="bytesToRead">Number of bytes to read (move stream index forward)</param>
        /// <param name="errorToReturn">Custom error to return (null returns true, otherwise false)</param>
        public MockDeserializableTx(int streamIndex, int bytesToRead, string errorToReturn = null)
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
}
