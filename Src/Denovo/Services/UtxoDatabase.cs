// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Denovo.Services
{
    // TODO: Idea for coinbase transactions:
    //       Each coinbase tx must mature before it can be spent, which takes 100 blocks.
    //       The idea is to use a "queue" in our UTXO DB to hold the coinbase txs and only add them to the actual DB when
    //       they reach maturity. So there is no need to check the maturity of each "coin" aka UTXO and the caller can be sure
    //       that what it receives is spendable already.
    //       The queue can be a simple array that avoids array copy (push>pop>push>pop) like this
    //       assuming maturity is 5 confirmation and with 2 indexes i1 and i2:
    //       [null,null,null,null,null] i1=0, i2=0
    //       [C1,  null,null,null,null] i1=1, i2=0
    //       [C1,  C2,  null,null,null] i1=2, i2=0
    //       [C1,  C2,  C3,  null,null] i1=3, i2=0
    //       [C1,  C2,  C3,  C4,  null] i1=4, i2=0
    //       [C1,  C2,  C3,  C4,   C5 ] i1=5, i2=0
    //       Right now there is no coinbase tx in DB and if anyone tries spending any of these outputs the DB will return null
    //       and the transaction verification fails. As next block is mined and DB is updated since array is full (i1=max) the
    //       first coinbase tx is matured and has to be added to DB, instead of popping C1 and moving all items with an Array.Copy
    //       we simply keep track of the index witn i1 and replace the item inside the array:
    //       [C6,  C2,  C3,  C4,   C5 ] i1=5, i2=1   => C1 added to DB
    //       [C6,  C7,  C3,  C4,   C5 ] i1=5, i2=2   => C2 added to DB
    //       In case of a small reorg. the items are replaced starting at i2-2
    //       [C6', C7', C3,  C4,   C5'] i1=5, i2=2
    //       In case of bigger reorg (>100) "undo data" has to be used to remove matured but invalid the coinbase txs from DB


    // TODO: This needs improvement to actually use a database. This implementation is mostly a placeholder while
    //       we complete other parts and decide which database to use and how.
    public class UtxoDatabase : IUtxoDatabase
    {
        public UtxoDatabase(IDenovoFileManager fileMan)
        {
            this.fileMan = fileMan;
            database = new Dictionary<byte[], List<Utxo>>(new ByteComparer());
            Init();
        }

        public class ByteComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] x, byte[] y)
            {
                return ((Span<byte>)x).SequenceEqual(y);
            }

            public int GetHashCode([DisallowNull] byte[] obj)
            {
                return HashCode.Combine(obj);
            }
        }


        private readonly IDenovoFileManager fileMan;
        private const string DbName = "UtxoDb";
        private readonly Dictionary<byte[], List<Utxo>> database;


        private void Init()
        {
            byte[] data = fileMan.ReadData(DbName);
            if (data is not null && data.Length != 0)
            {
                var stream = new FastStreamReader(data);
                while (true)
                {
                    var utxo = new Utxo();
                    if (stream.TryReadByteArray(32, out byte[] hash) && utxo.TryDeserialize(stream, out _))
                    {
                        if (database.ContainsKey(hash))
                        {
                            database[hash].Add(utxo);
                        }
                        else
                        {
                            database.Add(hash, new List<Utxo>() { utxo });
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void WriteToDisk()
        {
            var stream = new FastStream(database.Count * 90);
            foreach (var item in database)
            {
                foreach (var item2 in item.Value)
                {
                    stream.Write(item.Key);
                    item2.Serialize(stream);
                }
            }

            fileMan.WriteData(stream.ToByteArray(), DbName);
        }

        public IUtxo Find(TxIn tin)
        {
            if (database.TryGetValue(tin.TxHash, out List<Utxo> value))
            {
                var index = value.FindIndex(x => x.Index == tin.Index);
                return index < 0 ? null : value[index];
            }
            else
            {
                return null;
            }
        }

        // TODO: get rid of the following 2 in interface and here
        public void MarkSpent(TxIn[] txInList)
        {
            throw new NotImplementedException();
        }

        public ulong MarkSpentAndGetFee(TxIn[] txInList)
        {
            throw new NotImplementedException();
        }


        private void UpdateCoinbase(ITransaction coinbase)
        {
            // Add to coinbase queue
        }

        public void Update(ITransaction[] txs)
        {
            UpdateCoinbase(txs[0]);

            for (uint i = 1; i < txs.Length; i++)
            {
                foreach (TxIn item in txs[i].TxInList)
                {
                    int index = database[item.TxHash].FindIndex(x => x.Index == item.Index);
                    database[item.TxHash].RemoveAt(index);
                    if (database[item.TxHash].Count == 0)
                    {
                        database.Remove(item.TxHash);
                    }
                }

                database.Add(txs[i].GetTransactionHash(),
                    new List<Utxo>(txs[i].TxOutList.Select(x => new Utxo(i, x.Amount, x.PubScript))));
            }

            WriteToDisk();
        }

        public void Undo(ITransaction[] txs, int lastIndex)
        {
            for (int i = 1; i <= lastIndex; i++)
            {
                foreach (var item in txs[i].TxInList)
                {
                    IUtxo utxo = Find(item);
                    utxo.IsBlockSpent = false;
                }
            }
        }
    }
}
