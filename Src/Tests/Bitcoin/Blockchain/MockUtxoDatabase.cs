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

            for (int i = 0; i < hashes.Length; i++)
            {
                Add(hashes[i], toReturn[i]);
            }
        }


        private Dictionary<byte[], List<Utxo>> database;

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
            if (database is null)
            {
                database = new Dictionary<byte[], List<Utxo>>(new ByteArrayComparer());
            }


            if (database.ContainsKey(hash))
            {
                database[hash].Add(new Utxo(output.Index, output.Amount, output.PubScript));
            }
            else
            {
                database.Add(hash, new List<Utxo>() { new Utxo(output.Index, output.Amount, output.PubScript) });
            }
        }


        public IUtxo Find(TxIn tin)
        {
            Assert.NotNull(database);
            bool b = database.ContainsKey(tin.TxHash);
            Assert.True(database.ContainsKey(tin.TxHash), "Input not found in database.");

            List<Utxo> ulist = database[tin.TxHash];
            Utxo utxo = ulist.Find(x => x.Index == tin.Index);
            if (utxo is not null)
            {
                ulist.Remove(utxo);
            }
            return utxo;
        }


        public void Update(ITransaction[] txs)
        {
            throw new NotImplementedException();
        }

        public void Undo(ITransaction[] txs, int lastIndex)
        {
            throw new NotImplementedException();
        }
    }
}
