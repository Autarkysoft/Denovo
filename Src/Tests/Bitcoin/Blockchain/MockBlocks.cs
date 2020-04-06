// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockBlockBase : IBlock
    {
        public virtual int Height { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual int Version { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual byte[] PreviousBlockHeaderHash
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public virtual byte[] MerkleRootHash
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        public virtual uint BlockTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual Target NBits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual uint Nonce { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual ITransaction[] TransactionList
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual byte[] ComputeMerkleRoot() => throw new NotImplementedException();
        public virtual byte[] GetBlockHash() => throw new NotImplementedException();
        public virtual string GetBlockID() => throw new NotImplementedException();
        public virtual void Serialize(FastStream stream) => throw new NotImplementedException();
        public virtual void SerializeHeader(FastStream stream) => throw new NotImplementedException();
        public virtual bool TryDeserialize(FastStreamReader stream, out string error) => throw new NotImplementedException();
        public virtual bool TryDeserializeHeader(FastStreamReader stream, out string error) => throw new NotImplementedException();
    }



    public class MockBlockIdBlock : MockBlockBase
    {
        public MockBlockIdBlock(byte[] txHashToReturn)
        {
            TxHash = txHashToReturn;
        }

        public MockBlockIdBlock(string txIdToReturn) : this(Helper.HexToBytes(txIdToReturn, true))
        {
        }


        private readonly byte[] TxHash;


        public override byte[] GetBlockHash()
        {
            if (TxHash == null)
            {
                Assert.True(false, "Mock transaction doesn't have any tx hash set.");
            }
            return TxHash;
        }

        public override string GetBlockID()
        {
            if (TxHash == null)
            {
                Assert.True(false, "Mock transaction doesn't have any tx hash set.");
            }
            return Helper.BytesToHex(TxHash.Reverse().ToArray());
        }
    }



    public class MockSerializableBlock : MockBlockBase
    {
        public MockSerializableBlock(byte[] serializedResult)
        {
            ba = serializedResult;
        }

        private readonly byte[] ba;

        public override void Serialize(FastStream stream)
        {
            stream.Write(ba);
        }
    }



    public class MockDeserializableBlock : MockBlockBase
    {
        /// <summary>
        /// Use this mock object to mock deserialization. It will check current index in stream and fails if it is not expected.
        /// Also it can return true or false depending on whether error is null or not.
        /// </summary>
        /// <param name="streamIndex">Expected current stream index</param>
        /// <param name="bytesToRead">Number of bytes to read (move stream index forward)</param>
        /// <param name="errorToReturn">Custom error to return (null returns true, otherwise false)</param>
        public MockDeserializableBlock(int streamIndex, int bytesToRead, string errorToReturn = null)
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
