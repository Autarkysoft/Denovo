// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockUtxoDatabase : IUtxoDatabase
    {
        public MockUtxoDatabase()
        {
        }

        public MockUtxoDatabase(string hashHex, IUtxo output) : this(Helper.HexToBytes(hashHex), output)
        {
        }

        public MockUtxoDatabase(byte[] hash, IUtxo output)
        {
            Add(hash, output);
        }

        public MockUtxoDatabase(byte[][] hashes, IUtxo[] toReturn)
        {
            if (hashes is null || toReturn is null || hashes.Length != toReturn.Length)
            {
                throw new ArgumentException("Invalid inputs.");
            }

            index = 0;
            this.hashes = new List<byte[]>(hashes);
            database = new List<IUtxo>(toReturn);
        }

        private int index;
        private List<byte[]> hashes;
        private List<IUtxo> database;

        private class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                return ((ReadOnlySpan<byte>)left).SequenceEqual(right);
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                {
                    return 0;
                }

                int hash = 17;
                foreach (byte b in key)
                {
                    hash = (hash * 31) + b.GetHashCode();
                }
                return hash;
            }
        }


        internal void Add(byte[] hash, IUtxo output)
        {
            if (hashes is null)
            {
                hashes = new List<byte[]>(1);
                database = new List<IUtxo>(1);
            }

            hashes.Add(hash);
            database.Add(output);
        }

        public IUtxo Find(TxIn tin)
        {
            Assert.NotNull(hashes);
            Assert.True(index < hashes.Count, "More calls were made to UTXO-Database.Find() than expected");

            Assert.Equal(hashes[index], tin.TxHash);
            Assert.Equal(database[index].Index, tin.Index);

            return database[index++];
        }

        public void MarkSpent(TxIn[] txInList)
        {
            foreach (var tin in txInList)
            {
                IUtxo utxo = Find(tin);
                utxo.IsBlockSpent = true;
            }
        }

        public ulong MarkSpentAndGetFee(TxIn[] txInList)
        {
            throw new NotImplementedException();
        }
    }
}
