// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;

namespace Tests.Bitcoin.Blockchain
{
    public abstract class MockTxBase : ITransaction
    {
        public virtual int Version { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual TxIn[] TxInList { get; set; }
        public virtual TxOut[] TxOutList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual IWitness[] WitnessList
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public virtual LockTime LockTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual bool IsVerified { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual int SigOpCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual int TotalSize => throw new NotImplementedException();
        public virtual int BaseSize => throw new NotImplementedException();
        public virtual int Weight => throw new NotImplementedException();
        public virtual int VirtualSize => throw new NotImplementedException();

        public virtual Digest256 GetTransactionHash() => throw new NotImplementedException();
        public virtual string GetTransactionId() => throw new NotImplementedException();
        public virtual Digest256 GetWitnessTransactionHash() => throw new NotImplementedException();
        public virtual string GetWitnessTransactionId() => throw new NotImplementedException();
        public virtual void AddSerializedSize(SizeCounter counter) => throw new NotImplementedException();
        public virtual void AddSerializedSizeWithoutWitness(SizeCounter counter) => throw new NotImplementedException();
        public virtual void Serialize(FastStream stream) => throw new NotImplementedException();
        public virtual void SerializeWithoutWitness(FastStream stream) => throw new NotImplementedException();
        public virtual bool TryDeserialize(FastStreamReader stream, out Errors error) => throw new NotImplementedException();
        public virtual byte[] SerializeForSigning(byte[] spendScr, int inputIndex, SigHashType sht) => throw new NotImplementedException();
        public virtual byte[] SerializeForSigningSegWit(byte[] prevOutScript, int inputIndex, ulong amount, SigHashType sht)
            => throw new NotImplementedException();
        public virtual byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht, IRedeemScript redeemScript, IRedeemScript witRedeem)
            => throw new NotImplementedException();
        public virtual void WriteScriptSig(Signature sig, in Point pubKey, ITransaction prevTx, int inputIndex, IRedeemScript redeem)
            => throw new NotImplementedException();

        public virtual byte[] SerializeForSigningTaproot(byte epoch, SigHashType sht, IUtxo[] spentOutputs, byte extFlag, int inputIndex, byte[] annexHash, byte[] tapLeafHash, byte keyVersion, uint codeSeparatorPos) => throw new NotImplementedException();

        public byte[] SerializeForSigningTaproot_KeyPath(SigHashType sht, IUtxo[] spentOutputs, int inputIndex, byte[] annexHash) => throw new NotImplementedException();

        public byte[] SerializeForSigningTaproot_ScriptPath(SigHashType sht, IUtxo[] spentOutputs, int inputIndex, byte[] annexHash, byte[] tapLeafHash, uint codeSeparatorPos) => throw new NotImplementedException();
    }



    public class MockTxIdTx : MockTxBase
    {
        public MockTxIdTx(Digest256 txHashToReturn)
        {
            TxHash = txHashToReturn;
        }

        public MockTxIdTx(string txIdToReturn) : this(new Digest256(Helper.HexToBytes(txIdToReturn, true)))
        {
        }


        private readonly Digest256? TxHash;


        public override Digest256 GetTransactionHash()
        {
            if (!TxHash.HasValue)
            {
                Assert.Fail("Mock transaction doesn't have any tx hash set.");
            }
            return TxHash.Value;
        }

        public override string GetTransactionId()
        {
            if (TxHash == null)
            {
                Assert.Fail("Mock transaction doesn't have any tx hash set.");
            }
            return TxHash.ToString()[2..];
        }
    }



    public class MockWTxIdTx : MockTxBase
    {
        public MockWTxIdTx(Digest256 wtxHashToReturn)
        {
            WTxHash = wtxHashToReturn;
        }

        public MockWTxIdTx(string txIdToReturn) : this(new Digest256(Helper.HexToBytes(txIdToReturn, true)))
        {
        }


        private readonly Digest256? WTxHash;


        public override Digest256 GetWitnessTransactionHash()
        {
            if (!WTxHash.HasValue)
            {
                Assert.Fail("Mock transaction doesn't have any wtx hash set.");
            }
            return WTxHash.Value;
        }

        public override string GetWitnessTransactionId()
        {
            if (WTxHash == null)
            {
                Assert.Fail("Mock transaction doesn't have any wtx hash set.");
            }
            return WTxHash.ToString()[2..];
        }
    }



    public class MockSerializableTx : MockTxBase
    {
        public MockSerializableTx(byte[] serializedResult, bool isWitness = false)
        {
            ba = serializedResult;
            isWit = isWitness;
        }

        private readonly byte[] ba;
        private readonly bool isWit;

        public override void AddSerializedSize(SizeCounter counter) => counter.Add(ba.Length);
        public override void AddSerializedSizeWithoutWitness(SizeCounter counter) => counter.Add(ba.Length);

        public override void Serialize(FastStream stream)
        {
            Assert.False(isWit);
            stream.Write(ba);
        }

        public override void SerializeWithoutWitness(FastStream stream)
        {
            Assert.True(isWit);
            stream.Write(ba);
        }
    }


    public class MockSignableTx : MockTxBase
    {
        public MockSignableTx(byte[] returnResult) => result = returnResult;

        private readonly byte[] result;

        public override byte[] SerializeForSigning(byte[] spendScr, int inputIndex, SigHashType sht) => result;
    }



    public class MockDeserializableTx : MockTxBase
    {
        /// <summary>
        /// Use this mock object to mock deserialization. It will check current index in stream and fails if it is not expected.
        /// Also it can return true or false depending on whether error is null or not.
        /// </summary>
        /// <param name="streamIndex">Expected current stream index</param>
        /// <param name="bytesToRead">Number of bytes to read (move stream index forward)</param>
        /// <param name="errorToReturn">Set to true to fail deserializing and return <see cref="Errors.ForTesting"/> error</param>
        public MockDeserializableTx(int streamIndex, int bytesToRead, bool returnError = false)
        {
            expectedIndex = streamIndex;
            retError = returnError;
            this.bytesToRead = bytesToRead;
        }

        private readonly int expectedIndex;
        private readonly int bytesToRead;
        private readonly bool retError;

        public override bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            int actualIndex = stream.GetCurrentIndex();
            Assert.Equal(expectedIndex, actualIndex);

            if (!stream.TryReadByteArray(bytesToRead, out _))
            {
                Assert.Fail("Stream doesn't have enough bytes.");
            }

            error = retError ? Errors.ForTesting : Errors.None;
            return !retError;
        }
    }



    public class MockTxPropInOut : MockTxBase
    {
        public MockTxPropInOut(TxIn[] txIns, TxOut[] txOuts, IWitness[] witnesses = null)
        {
            _tins = txIns;
            _touts = txOuts;
            _wits = witnesses;
        }

        private TxIn[] _tins;
        public override TxIn[] TxInList
        {
            get
            {
                if (_tins == null)
                {
                    Assert.Fail("TxIn array was not set.");
                }
                return _tins;
            }

            set => _tins = value;
        }


        private TxOut[] _touts;
        public override TxOut[] TxOutList
        {
            get
            {
                if (_touts == null)
                {
                    Assert.Fail("TxOut array was not set.");
                }
                return _touts;
            }

            set => _touts = value;
        }


        private IWitness[] _wits;
        public override IWitness[] WitnessList
        {
            get => _wits;
            set => _wits = value;
        }
    }
}
