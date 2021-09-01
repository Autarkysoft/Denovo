// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockBlockBase : IBlock
    {
        public virtual int Height { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int TotalSize => throw new NotImplementedException();
        public int StrippedSize => throw new NotImplementedException();
        public int Weight => throw new NotImplementedException();
        public virtual BlockHeader Header { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual ITransaction[] TransactionList
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual byte[] ComputeMerkleRoot() => throw new NotImplementedException();
        public virtual byte[] ComputeWitnessMerkleRoot(byte[] commitment) => throw new NotImplementedException();
        public virtual byte[] GetBlockHash(bool recompute) => throw new NotImplementedException();
        public virtual string GetBlockID(bool recompute) => throw new NotImplementedException();
        public virtual void AddSerializedSize(SizeCounter counter) => throw new NotImplementedException();
        public virtual void Serialize(FastStream stream) => throw new NotImplementedException();
        public virtual bool TryDeserialize(FastStreamReader stream, out string error) => throw new NotImplementedException();
    }



    public class MockBlockIdBlock : MockBlockBase
    {
        public MockBlockIdBlock(byte[] blockHashToReturn)
        {
            hash = blockHashToReturn;
        }

        public MockBlockIdBlock(string txIdToReturn) : this(Helper.HexToBytes(txIdToReturn, true))
        {
        }


        private readonly byte[] hash;


        public override byte[] GetBlockHash(bool recompute)
        {
            if (hash == null)
            {
                Assert.True(false, "Mock block doesn't have any block hash set.");
            }
            return hash;
        }

        public override string GetBlockID(bool recompute)
        {
            if (hash == null)
            {
                Assert.True(false, "Mock block doesn't have any block hash set.");
            }
            return Helper.BytesToHex(hash.Reverse().ToArray());
        }
    }



    public class MockSerializableBlock : MockBlockBase
    {
        public MockSerializableBlock(byte[] serializedResult)
        {
            ba = serializedResult;
            using Sha256 sha = new();
            var hdr = ((Span<byte>)ba).Slice(0, 80);
            hash = sha.ComputeHashTwice(hdr);
        }

        private readonly byte[] ba, hash;

        public override byte[] GetBlockHash(bool recompute) => hash;
        public override string GetBlockID(bool recompute) => Helper.BytesToHex(hash);

        public override void AddSerializedSize(SizeCounter counter) => counter.Add(ba.Length);
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
