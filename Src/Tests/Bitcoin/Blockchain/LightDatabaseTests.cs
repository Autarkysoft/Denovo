// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class LightDatabaseTests
    {
        private static readonly Digest256 MockHash = new(2);

        [Fact]
        public void ConstructorTest()
        {
            LightDatabase db1 = new();
            LightDatabase db2 = new(2);
            LightDatabase db3 = new(-5);

            Assert.NotNull(db1.hashes);
            Assert.NotNull(db2.hashes);
            Assert.NotNull(db3.hashes);
            Assert.NotNull(db1.coins);
            Assert.NotNull(db2.coins);
            Assert.NotNull(db3.coins);

            Assert.Equal(LightDatabase.DefaultCapacity, db1.hashes.Capacity);
            Assert.Equal(LightDatabase.DefaultCapacity, db1.coins.Capacity);
            Assert.Equal(2, db2.hashes.Capacity);
            Assert.Equal(2, db2.coins.Capacity);
            Assert.Equal(LightDatabase.DefaultCapacity, db3.hashes.Capacity);
            Assert.Equal(LightDatabase.DefaultCapacity, db3.coins.Capacity);
        }

        [Fact]
        public void AddTest()
        {
            LightDatabase db = new(2);
            Digest256 hash = Digest256.One;
            List<IUtxo> coins = new(1);

            db.Add(hash, coins);

            Assert.Single(db.hashes);
            Assert.Single(db.coins);
            Assert.Equal(hash, db.hashes[0]);
            Assert.Same(coins, db.coins[0]);
        }

        [Fact]
        public void Add_DulicateTest()
        {
            LightDatabase db = new(2);
            Digest256 hash = Digest256.One;
            List<IUtxo> coins1 = new(1);
            List<IUtxo> coins2 = new(1);

            db.Add(hash, coins1);

            Assert.Single(db.hashes);
            Assert.Single(db.coins);
            Assert.Equal(hash, db.hashes[0]);
            Assert.Same(coins1, db.coins[0]);

            db.Add(hash, coins2);
            Assert.Single(db.hashes);
            Assert.Single(db.coins);
            Assert.Equal(hash, db.hashes[0]);
            Assert.Same(coins2, db.coins[0]); // Changed
        }

        [Fact]
        public void ClearTest()
        {
            LightDatabase db = new(2);
            db.Clear();
            Assert.Empty(db.hashes);
            Assert.Empty(db.coins);

            db.Add(MockHash, new List<IUtxo>());
            db.Clear();
            Assert.Empty(db.hashes);
            Assert.Empty(db.coins);
        }

        [Fact]
        public void ContainsTest()
        {
            LightDatabase db = new(2);
            Digest256 hash = Digest256.One;
            List<IUtxo> coins = new(new IUtxo[1] { new MockUtxo() { Index = 1 } });
            db.Add(hash, coins);

            Assert.True(db.Contains(hash, 1));
            Assert.False(db.Contains(hash, 2));
            Assert.False(db.Contains(MockHash, 1));
        }

        [Fact]
        public void FindTest()
        {
            LightDatabase db = new(2);
            Digest256 hash = Digest256.One;
            IUtxo utxoToAdd = new MockUtxo() { Index = 1 };
            List<IUtxo> coins = new(new IUtxo[1] { utxoToAdd });
            db.Add(hash, coins);

            IUtxo utxo1 = db.Find(new TxIn(hash, 1, null, 0));
            IUtxo utxo2 = db.Find(new TxIn(hash, 2, null, 0));
            IUtxo utxo3 = db.Find(new TxIn(MockHash, 1, null, 0));

            Assert.NotNull(utxo1);
            Assert.Null(utxo2);
            Assert.Null(utxo3);
            Assert.Same(utxoToAdd, utxo1);
        }

        [Fact]
        public void RemoveTest()
        {
            LightDatabase db = new(2);
            Digest256 h1 = Digest256.One;
            IUtxo utxo1 = new MockUtxo() { Index = 1 };

            Digest256 h2 = new(2);
            IUtxo utxo21 = new MockUtxo() { Index = 21 };
            IUtxo utxo22 = new MockUtxo() { Index = 22 };
            IUtxo utxo23 = new MockUtxo() { Index = 23 };

            Digest256 h3 = new(3);
            IUtxo utxo31 = new MockUtxo() { Index = 31 };
            IUtxo utxo32 = new MockUtxo() { Index = 32 };

            Digest256 h4 = new(4);
            IUtxo utxo41 = new MockUtxo() { Index = 41 };

            db.Add(h1, new List<IUtxo>(new IUtxo[] { utxo1 }));
            db.Add(h2, new List<IUtxo>(new IUtxo[] { utxo21, utxo22, utxo23 }));
            db.Add(h3, new List<IUtxo>(new IUtxo[] { utxo31, utxo32 }));
            db.Add(h4, new List<IUtxo>(new IUtxo[] { utxo41 }));

            Assert.Equal(4, db.hashes.Count);
            Assert.Equal(4, db.coins.Count);

            ITransaction tx1 = new Transaction() { TxInList = new[] { new TxIn() { TxHash = h1, Index = 1 } } };
            ITransaction tx2 = new Transaction()
            {
                TxInList = new[] { new TxIn() { TxHash = h2, Index = 22 }, new TxIn() { TxHash = h2, Index = 21 } }
            };
            ITransaction tx3 = new Transaction()
            {
                TxInList = new[] { new TxIn() { TxHash = h3, Index = 31 }, new TxIn() { TxHash = h4, Index = 41 } }
            };

            db.Remove(tx1);
            Assert.Equal(3, db.hashes.Count);
            Assert.Equal(3, db.coins.Count);
            Assert.DoesNotContain(h1, db.hashes);

            db.Remove(tx2);
            Assert.Equal(3, db.hashes.Count);
            Assert.Equal(3, db.coins.Count);
            Assert.Contains(h2, db.hashes);
            Assert.Single(db.coins[0]);
            Assert.True(db.coins[0][0].Index == 23);

            db.Remove(tx3);
            Assert.Equal(2, db.hashes.Count);
            Assert.Equal(2, db.coins.Count);
            Assert.DoesNotContain(h4, db.hashes);
            Assert.Single(db.coins[1]);
            Assert.True(db.coins[1][0].Index == 32);
        }
    }
}
