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

        internal readonly Dictionary<byte[], List<IUtxo>> database = new Dictionary<byte[], List<IUtxo>>(new ByteArrayComparer());

        internal void Add(byte[] hash, IUtxo output)
        {
            if (!database.ContainsKey(hash))
            {
                database.Add(hash, new List<IUtxo>() { output });
            }
            else if (!database[hash].Contains(output))
            {
                database[hash].Add(output);
            }
            else
            {
                Assert.True(false, "Dictionary contains duplicates.");
            }
        }

        public IUtxo Find(TxIn tin)
        {
            if (database.TryGetValue(tin.TxHash, out List<IUtxo> res))
            {
                foreach (var item in res)
                {
                    if (item.Index == tin.Index)
                    {
                        return item;
                    }
                }

                Assert.True(false, "TxIn index doesn't match any of the expected one.");
                return null;
            }
            else
            {
                Assert.True(false, $"Unexpected TxIn was given {Helper.BytesToHex(tin.TxHash)}:{tin.Index}.");
                return null;
            }
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
