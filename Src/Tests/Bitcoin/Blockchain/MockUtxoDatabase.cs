// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class MockUtxoDatabase : IUtxoDatabase
    {
        public MockUtxoDatabase()
        {
        }

        public MockUtxoDatabase(string hashHex, IUtxo output) : this(Digest256.ParseHex(hashHex), output)
        {
        }

        public MockUtxoDatabase(Digest256 hash, IUtxo output)
        {
            Add(hash, output);
        }

        public MockUtxoDatabase(Digest256[] hashes, IUtxo[] toReturn)
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


        private Dictionary<Digest256, List<Utxo>> database;


        internal void Add(Digest256 hash, IUtxo output)
        {
            if (database is null)
            {
                database = new Dictionary<Digest256, List<Utxo>>();
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


        public bool Contains(Digest256 hash, uint index)
        {
            Assert.NotNull(database);
            bool b = database.ContainsKey(hash);
            return b;
        }

        public IUtxo Find(TxIn tin)
        {
            Assert.NotNull(database);
            bool b = database.ContainsKey(tin.TxHash);
            Assert.True(b, "Input not found in database.");

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
