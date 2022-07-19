// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// A simple implementation of a database to be used in <see cref="TransactionVerifier"/> to store <see cref="IUtxo"/>s.
    /// </summary>
    public class LightDatabase
    {
        /// <summary>
        /// Initializes a new empty instance of <see cref="LightDatabase"/> with the default capacity.
        /// </summary>
        public LightDatabase() : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Initializes a new empty instance of <see cref="LightDatabase"/> with the given capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity of the lists</param>
        public LightDatabase(int capacity)
        {
            if (capacity <= 0)
            {
                capacity = DefaultCapacity;
            }

            hashes = new List<Digest256>(capacity);
            coins = new List<List<IUtxo>>(capacity);
        }


        /// <summary>
        /// The default capacity used in lists
        /// </summary>
        public const int DefaultCapacity = 20000;

        /// <summary>
        /// List of unspent transaction hashes
        /// </summary>
        public readonly List<Digest256> hashes;
        /// <summary>
        /// List of unspent transaction outputs
        /// </summary>
        public readonly List<List<IUtxo>> coins;


        private int FindIndex(in Digest256 hash)
        {
            for (int i = hashes.Count - 1; i >= 0; i--)
            {
                if (hash == hashes[i])
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Adds the given output to the database
        /// </summary>
        /// <param name="hash">Transaction hash</param>
        /// <param name="utxos">unspent transaction outputs</param>
        public void Add(in Digest256 hash, List<IUtxo> utxos)
        {
            int index = FindIndex(hash);
            if (index >= 0)
            {
                coins[index] = utxos;
            }
            else
            {
                hashes.Add(hash);
                coins.Add(utxos);
                Debug.Assert(hashes.Count == coins.Count);
            }
        }

        /// <summary>
        /// Removes all elements from the database
        /// </summary>
        public void Clear()
        {
            hashes.Clear();
            coins.Clear();
        }

        /// <summary>
        /// Returns if the output exists in this database
        /// </summary>
        /// <param name="hash">Transaction hash</param>
        /// <param name="index">output index</param>
        /// <returns></returns>
        public bool Contains(in Digest256 hash, uint index)
        {
            int i = FindIndex(hash);
            return i >= 0 && coins[i].Any(u => u.Index == index);
        }

        /// <summary>
        /// Finds and returns the UTXO being spent by this input. Returns null if not found.
        /// </summary>
        /// <param name="tin">Transaction input</param>
        /// <returns>The UTXO if it exists; otherwise null.</returns>
        public IUtxo Find(TxIn tin)
        {
            int i = FindIndex(tin.TxHash);
            if (i >= 0)
            {
                int j = coins[i].FindIndex(x => x.Index == tin.Index);
                return j < 0 ? null : coins[i][j];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Removes UTXOs that were spent by this transaction.
        /// </summary>
        /// <remarks>
        /// This method assumes all the UTXOs are present in the database
        /// </remarks>
        /// <param name="tx">Transaction</param>
        public void Remove(ITransaction tx)
        {
            for (int i = 0; i < tx.TxInList.Length; i++)
            {
                int index = FindIndex(tx.TxInList[i].TxHash);
                Debug.Assert(index >= 0);
                Debug.Assert(coins.Count > index);

                int j = coins[index].FindIndex(x => x.Index == tx.TxInList[i].Index);
                Debug.Assert(j >= 0);

                if (coins[index].Count == 1)
                {
                    hashes.RemoveAt(index);
                    coins.RemoveAt(index);
                }
                else
                {
                    coins[index].RemoveAt(j);
                }

                Debug.Assert(hashes.Count == coins.Count);
            }
        }
    }
}
