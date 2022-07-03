// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Denovo.Services
{
    // TODO: Add a temp DB that is not stored to disk but updated on each call by TxVerifier to add outputs so that they
    //       can be spent in txs in the same block


    // TODO: Idea for coinbase transactions:
    //       Each coinbase tx must mature before it can be spent, which takes 100 blocks.
    //       The idea is to use a "queue" in our UTXO DB to hold the coinbase txs and only add them to the actual DB when
    //       they reach maturity. So there is no need to check the maturity of each "coin" aka UTXO and the caller can be sure
    //       that what it receives by calling Find() is always spendable (mature).
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
    //       In case of a small reorg (<maturity=100) the popped coinbase outputs are already matured when block N is replaced
    //       by block N'. The replacement starts at i2-reorgCount
    //       [C6', C7', C3,  C4,   C5'] i1=5, i2=2
    //       In case of bigger reorg (>100) "undo data" has to be used to remove matured but invalid the coinbase txs from DB


    // TODO: This needs improvement to actually use a database. This implementation is mostly a placeholder while
    //       we complete other parts and decide which database to use and how.
    public class UtxoDatabase : IUtxoDatabase
    {
        public UtxoDatabase(IDenovoFileManager fileMan)
        {
            this.fileMan = fileMan;
            database = new Dictionary<Digest256, List<Utxo>>(2000);
            Init();
        }


        private readonly IDenovoFileManager fileMan;
        private const string DbName = "UtxoDb";
        private const string CoinBaseDbName = "CoinbaseDb";
        // TODO: using dictionary is a flaw due to its hash collision
        private readonly Dictionary<Digest256, List<Utxo>> database;
        private readonly ITransaction[] coinbaseQueue = new ITransaction[99];
        private int i1, i2;
        private int writeQueueCount;

        private const int MaxWriteQueue = 500;


        private void Init()
        {
            byte[] data = fileMan.ReadData(DbName);
            if (data is not null && data.Length != 0)
            {
                FastStreamReader stream = new(data);
                while (true)
                {
                    Utxo utxo = new();
                    if (stream.TryReadDigest256(out Digest256 hash) && utxo.TryDeserialize(stream, out _))
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

            data = fileMan.ReadData(CoinBaseDbName);
            if (data is not null && data.Length != 0)
            {
                FastStreamReader stream = new(data);
                if (!stream.CheckRemaining(8))
                {
                    return;
                }
                i1 = stream.ReadInt32Checked();
                i2 = stream.ReadInt32Checked();
                int index = 0;
                while (true)
                {
                    Transaction coinbase = new();
                    if (coinbase.TryDeserialize(stream, out _))
                    {
                        coinbaseQueue[index++] = coinbase;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void WriteToDisk()
        {
            writeQueueCount = 0;
            WriteCoinbaseToDisk();
            WriteDbToDisk();
        }

        private void WriteDbToDisk()
        {
            SizeCounter counter = new();
            foreach (var item in database)
            {
                foreach (var item2 in item.Value)
                {
                    counter.AddHash256();
                    item2.AddSerializedSize(counter);
                }
            }

            FastStream stream = new(counter.Size);
            foreach (KeyValuePair<Digest256, List<Utxo>> item in database)
            {
                foreach (Utxo item2 in item.Value)
                {
                    stream.Write(item.Key);
                    item2.Serialize(stream);
                }
            }

            fileMan.WriteData(stream.ToByteArray(), DbName);
        }

        private void WriteCoinbaseToDisk()
        {
            FastStream stream = new(coinbaseQueue.Length * 250);
            stream.Write(i1);
            stream.Write(i2);
            foreach (ITransaction item in coinbaseQueue)
            {
                if (item is null)
                {
                    break;
                }

                item.Serialize(stream);
            }

            fileMan.WriteData(stream.ToByteArray(), CoinBaseDbName);
        }


        private void UpdateCoinbase(ITransaction coinbase)
        {
            Debug.Assert(coinbase is not null);
            if (i1 < coinbaseQueue.Length)
            {
                coinbaseQueue[i1++] = coinbase;
            }
            else
            {
                if (i2 >= coinbaseQueue.Length)
                {
                    i2 = 0;
                }

                ITransaction pop = coinbaseQueue[i2];
                database.Add(pop.GetTransactionHash(),
                    new List<Utxo>(pop.TxOutList.Select((x, i) => new Utxo((uint)i, x.Amount, x.PubScript))));

                coinbaseQueue[i2++] = coinbase;
            }
        }


        public bool Contains(Digest256 hash, uint index)
        {
            return database.TryGetValue(hash, out List<Utxo> value) && value.Any(u => u.Index == index);
        }


        public IUtxo Find(TxIn tin)
        {
            if (database.TryGetValue(tin.TxHash, out List<Utxo> value))
            {
                int index = value.FindIndex(x => x.Index == tin.Index);
                return index < 0 ? null : value[index];
            }
            else
            {
                return null;
            }
        }


        public void Update(ITransaction[] txs)
        {
            Debug.Assert(txs is not null && txs.Length > 0);

            UpdateCoinbase(txs[0]);

            for (uint i = 1; i < txs.Length; i++)
            {
                // Remove spent inputs from DB
                foreach (TxIn item in txs[i].TxInList)
                {
                    int index = database[item.TxHash].FindIndex(x => x.Index == item.Index);
                    Debug.Assert(index >= 0);
                    database[item.TxHash].RemoveAt(index);
                    if (database[item.TxHash].Count == 0)
                    {
                        database.Remove(item.TxHash);
                    }
                }

                // Add new outputs to DB
                List<Utxo> temp = new(txs[i].TxOutList.Length);
                for (int j = 0; j < txs[i].TxOutList.Length; j++)
                {
                    TxOut item = txs[i].TxOutList[j];
                    if (!item.PubScript.IsUnspendable())
                    {
                        temp.Add(new Utxo((uint)j, item.Amount, item.PubScript));
                    }
                }
                if (temp.Count > 0)
                {
                    // TODO: replace duplicate (BIP30)
                    database.Add(txs[i].GetTransactionHash(), temp);
                }
            }

            if (++writeQueueCount > MaxWriteQueue)
            {
                WriteToDisk();
            }
        }


        public void Undo(ITransaction[] txs, int lastIndex)
        {
            Debug.Assert(txs is not null && txs.Length > 0);

            for (int i = 1; i <= lastIndex; i++)
            {
                foreach (TxIn item in txs[i].TxInList)
                {
                    IUtxo utxo = Find(item);
                    if (utxo is not null)
                    {
                        utxo.IsBlockSpent = false;
                    }
                }
            }
        }
    }
}
