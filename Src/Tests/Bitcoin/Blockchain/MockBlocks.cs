﻿// Autarkysoft Tests
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
        public int TotalSize => throw new NotImplementedException();
        public int StrippedSize => throw new NotImplementedException();
        public int Weight => throw new NotImplementedException();
        public virtual BlockHeader Header { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public virtual ITransaction[] TransactionList
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public virtual void AddStrippedSerializedSize(SizeCounter counter) => throw new NotImplementedException();
        public virtual byte[] ComputeMerkleRoot() => throw new NotImplementedException();
        public virtual byte[] ComputeWitnessMerkleRoot(byte[] commitment) => throw new NotImplementedException();
        public virtual byte[] GetBlockHash() => throw new NotImplementedException();
        public virtual string GetBlockID() => throw new NotImplementedException();
        public virtual void AddSerializedSize(SizeCounter counter) => throw new NotImplementedException();
        public virtual void Serialize(FastStream stream) => throw new NotImplementedException();
        public virtual bool TryDeserialize(FastStreamReader stream, out Errors error) => throw new NotImplementedException();
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


        public override byte[] GetBlockHash()
        {
            if (hash == null)
            {
                Assert.True(false, "Mock block doesn't have any block hash set.");
            }
            return hash;
        }

        public override string GetBlockID()
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
        }

        private readonly byte[] ba;

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
        /// <param name="returnError">Set to false to return the test error value</param>
        public MockDeserializableBlock(int streamIndex, int bytesToRead, bool returnError = false)
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
                Assert.True(false, "Stream doesn't have enough bytes.");
            }

            error = retError ? Errors.ForTesting : Errors.None;
            return !retError;
        }
    }
}
