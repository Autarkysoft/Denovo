// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockTx : ITransaction
    {
        public MockTx(byte[] txHashToReturn)
        {
            TxHash = txHashToReturn;
        }

        public MockTx(string txIdToReturn) : this(Helper.HexToBytes(txIdToReturn, true))
        {
        }



        private readonly byte[] TxHash;



        public int Version { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TxIn[] TxInList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TxOut[] TxOutList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IWitnessScript[] WitnessList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public LockTime LockTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytesToSign(ITransaction prvTx, int inputIndex, SigHashType sht, IRedeemScript redeemScript)
        {
            throw new NotImplementedException();
        }

        public byte[] GetTransactionHash()
        {
            if (TxHash == null)
            {
                Assert.True(false, "Mock transaction doesn't have any tx hash set.");
            }
            return TxHash;
        }

        public string GetTransactionId()
        {
            if (TxHash == null)
            {
                Assert.True(false, "Mock transaction doesn't have any tx hash set.");
            }
            return Helper.BytesToHex(TxHash.Reverse().ToArray());
        }

        public string GetWitnessTransactionId()
        {
            throw new NotImplementedException();
        }

        public void Serialize(FastStream stream)
        {
            throw new NotImplementedException();
        }

        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            throw new NotImplementedException();
        }

        public void WriteScriptSig(Signature sig, PublicKey pubKey, ITransaction prevTx, int inputIndex)
        {
            throw new NotImplementedException();
        }
    }
}
