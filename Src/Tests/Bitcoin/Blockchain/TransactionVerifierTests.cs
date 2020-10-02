// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class TransactionVerifierTests
    {
        private const int MockHeight = 123;

        [Fact]
        public void ConstructorTest()
        {
            var verifier = new TransactionVerifier(true, new MockUtxoDatabase(), new MockMempool(null), new MockConsensus());

            Helper.ComparePrivateField(verifier, "isMempool", true);
            Assert.False(verifier.ForceLowS);
            Assert.False(verifier.StrictNumberEncoding);
            Assert.Equal(0UL, verifier.TotalFee);
            Assert.Equal(0, verifier.TotalSigOpCount);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            var ut = new MockUtxoDatabase();
            var mem = new MockMempool(null);
            var c = new MockConsensus();

            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, null, mem, c));
            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, ut, null, c));
            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, ut, mem, null));
        }


        public static IEnumerable<object[]> GetCoinBasePrimCases()
        {
            var c = new MockConsensus() { expHeight = MockHeight, bip34 = true };

            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[0], null),
                false,
                "Coinbase transaction must contain only one input.",
                0
            };
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[2], null),
                false,
                "Coinbase transaction must contain only one input.",
                0
            };
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1], new TxOut[0]),
                false,
                "Transaction must contain at least one output.",
                0
            };

            byte[] badHash = new byte[32];
            badHash[0] = 1;
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(badHash, uint.MaxValue, null, 1234) }, new TxOut[1]),
                false,
                "Invalid coinbase outpoint.",
                0
            };
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(new byte[32], 0, null, 1234) }, new TxOut[1]),
                false,
                "Invalid coinbase outpoint.",
                0
            };

            var mockFailSigScr = new MockCoinbaseVerifySigScript(MockHeight, false);
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(new byte[32], uint.MaxValue, mockFailSigScr, 1234) }, new TxOut[1]),
                false,
                "Invalid coinbase signature script.",
                0
            };

            var mockPassSigScr = new MockCoinbaseVerifySigScript(MockHeight, true, 100);
            TxOut t1 = new TxOut(50, new MockSigOpCountPubScript(20));
            TxOut t2 = new TxOut(60, new MockSigOpCountPubScript(3));
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(new byte[32], uint.MaxValue, mockPassSigScr, 1234) }, new TxOut[2]{ t1, t2 }),
                true,
                null,
                492 // 4*100 + 4*20 + 4*3
            };
        }
        [Theory]
        [MemberData(nameof(GetCoinBasePrimCases))]
        public void VerifyCoinbasePrimaryTest(IConsensus consensus, ITransaction tx, bool expB, string expErr, int expOpCount)
        {
            var verifier = new TransactionVerifier(false, new MockUtxoDatabase(), new MockMempool(null), consensus);

            bool actualB = verifier.VerifyCoinbasePrimary(tx, out string error);

            Assert.Equal(expB, actualB);
            Assert.Equal(expErr, error);
            Assert.Equal(expOpCount, verifier.TotalSigOpCount);
            Assert.Equal(0UL, verifier.TotalFee);
        }

        public static IEnumerable<object[]> GetCoinBaseOutputCases()
        {
            yield return new object[]
            {
                new MockConsensus() { expHeight = MockHeight, blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(102, null) }),
                1,
                false,
                "Coinbase generates more coins than it should."
            };
            yield return new object[]
            {
                new MockConsensus() { expHeight = MockHeight, blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(102, null) }),
                10, // 110 > 102
                true,
                null
            };
            yield return new object[]
            {
                new MockConsensus() { expHeight = MockHeight, blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(102, null) }),
                2, // 102 >= 102
                true,
                null
            };
            yield return new object[]
            {
                new MockConsensus() { expHeight = MockHeight, blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(50, null), new TxOut(20, null), new TxOut(50, null) }),
                0,
                false,
                "Coinbase generates more coins than it should."
            };
            yield return new object[]
            {
                new MockConsensus() { expHeight = MockHeight, blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(50, null), new TxOut(20, null), new TxOut(50, null) }),
                150, // 150 > 120
                true,
                null
            };
            yield return new object[]
            {
                new MockConsensus() { expHeight = MockHeight, blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(50, null), new TxOut(20, null), new TxOut(50, null) }),
                120, // 120 >= 120
                true,
                null
            };
        }
        [Theory]
        [MemberData(nameof(GetCoinBaseOutputCases))]
        public void VerifyCoinbaseOutputTest(IConsensus c, ITransaction tx, ulong fee, bool expB, string expErr)
        {
            var verifier = new TransactionVerifier(false, new MockUtxoDatabase(), new MockMempool(null), c)
            {
                TotalFee = fee,
                TotalSigOpCount = 987
            };

            bool actualB = verifier.VerifyCoinbaseOutput(tx, out string error);

            Assert.Equal(expB, actualB);
            Assert.Equal(expErr, error);
            Assert.Equal(987, verifier.TotalSigOpCount); // SigOps aren't counted/changed here anymore
            Assert.Equal(fee, verifier.TotalFee); // Fee shouldn't change
        }


        public static IEnumerable<object[]> GetTxVerifyCases()
        {
            var c = new MockConsensus()
            {
                expHeight = MockHeight,
                bip65 = true,
                bip112 = true,
                strictDer = true,
                bip147 = true,
                bip16 = true,
                segWit = true,
            };
            byte[] simpTxHash1 = new byte[32];
            simpTxHash1[1] = 1;
            byte[] simpTxHash2 = new byte[32];
            simpTxHash2[1] = 2;
            byte[] simpTxHash3 = new byte[32];
            simpTxHash3[1] = 3;

            var simpPubScr = new PubkeyScript();
            var simpSigScr = new SignatureScript(new byte[1] { (byte)OP._1 });

            // *** Fee Tests ***

            yield return new object[]
            {
                new MockUtxoDatabase(simpTxHash1, new MockUtxo() { Amount = 0, Index = 0, PubScript = simpPubScr }),
                new MockMempool(null),
                c,
                new Transaction()
                {
                    TxInList = new TxIn[1]
                    {
                        new TxIn(simpTxHash1, 0, simpSigScr, uint.MaxValue)
                    },
                    TxOutList = new TxOut[1]
                    {
                        new TxOut(0, simpPubScr)
                    }
                },
                true, // Verification success
                null, // Error
                0, // Added SigOpCount
                0, // Added fee
                false, // AnySegWit
            };
            yield return new object[]
            {
                new MockUtxoDatabase(simpTxHash1, new MockUtxo() { Amount = 0, Index = 0, PubScript = simpPubScr }),
                new MockMempool(null),
                c,
                new Transaction()
                {
                    TxInList = new TxIn[1]
                    {
                        new TxIn(simpTxHash1, 0, simpSigScr, uint.MaxValue)
                    },
                    TxOutList = new TxOut[1]
                    {
                        new TxOut(1, simpPubScr)
                    }
                },
                false, // Verification success
                "Transaction is spending more than it can.", // Error
                0, // Added SigOpCount
                0, // Added fee
                false, // AnySegWit
            };
            yield return new object[]
            {
                new MockUtxoDatabase(simpTxHash1, new MockUtxo() { Amount = 123, Index = 0, PubScript = simpPubScr }),
                new MockMempool(null),
                c,
                new Transaction()
                {
                    TxInList = new TxIn[1]
                    {
                        new TxIn(simpTxHash1, 0, simpSigScr, uint.MaxValue)
                    },
                    TxOutList = new TxOut[1]
                    {
                        new TxOut(3, simpPubScr)
                    }
                },
                true, // Verification success
                null, // Error
                0, // Added SigOpCount
                120, // Added fee
                false, // AnySegWit
            };
            yield return new object[]
            {
                // Make sure ulong is being used for calculation of fee
                new MockUtxoDatabase(simpTxHash1, new MockUtxo() { Amount = Constants.TotalSupply, Index = 0, PubScript = simpPubScr }),
                new MockMempool(null),
                c,
                new Transaction()
                {
                    TxInList = new TxIn[1]
                    {
                        new TxIn(simpTxHash1, 0, simpSigScr, uint.MaxValue)
                    },
                    TxOutList = new TxOut[1]
                    {
                        new TxOut(1000, simpPubScr)
                    }
                },
                true, // Verification success
                null, // Error
                0, // Added SigOpCount
                Constants.TotalSupply - 1000UL, // Added fee
                false, // AnySegWit
            };
            yield return new object[]
            {
                new MockUtxoDatabase(new byte[][] { simpTxHash1, simpTxHash2, simpTxHash3 },
                new IUtxo[]
                    {
                        new MockUtxo() { Amount = 13, Index = 3, PubScript = simpPubScr },
                        new MockUtxo() { Amount = 57, Index = 7, PubScript = simpPubScr },
                        new MockUtxo() { Amount = 73, Index = 5, PubScript = simpPubScr },
                    }),
                new MockMempool(null),
                c,
                new Transaction()
                {
                    TxInList = new TxIn[3]
                    {
                        new TxIn(simpTxHash1, 3, simpSigScr, uint.MaxValue),
                        new TxIn(simpTxHash2, 7, simpSigScr, uint.MaxValue),
                        new TxIn(simpTxHash3, 5, simpSigScr, uint.MaxValue),
                    },
                    TxOutList = new TxOut[3]
                    {
                        new TxOut(140, simpPubScr),
                        new TxOut(3, simpPubScr),
                        new TxOut(1, simpPubScr),
                    }
                },
                false, // Verification success
                "Transaction is spending more than it can.", // Error
                0, // Added SigOpCount
                0, // Added fee
                false, // AnySegWit
            };
            yield return new object[]
            {
                new MockUtxoDatabase(new byte[][] { simpTxHash1, simpTxHash2, simpTxHash3 },
                new IUtxo[]
                    {
                        new MockUtxo() { Amount = 13, Index = 3, PubScript = simpPubScr },
                        new MockUtxo() { Amount = 57, Index = 7, PubScript = simpPubScr },
                        new MockUtxo() { Amount = 73, Index = 5, PubScript = simpPubScr },
                    }),
                new MockMempool(null),
                c,
                new Transaction()
                {
                    TxInList = new TxIn[3]
                    {
                        new TxIn(simpTxHash1, 3, simpSigScr, uint.MaxValue),
                        new TxIn(simpTxHash2, 7, simpSigScr, uint.MaxValue),
                        new TxIn(simpTxHash3, 5, simpSigScr, uint.MaxValue),
                    },
                    TxOutList = new TxOut[3]
                    {
                        new TxOut(12, simpPubScr),
                        new TxOut(6, simpPubScr),
                        new TxOut(50, simpPubScr),
                    }
                },
                true, // Verification success
                null, // Error
                0, // Added SigOpCount
                75, // Added fee
                false, // AnySegWit
            };

            // *** Transaction Tests (from signtx cases) ***
            foreach (var Case in Helper.ReadResource<JArray>("SignedTxTestData"))
            {
                Transaction prevTx = new Transaction();
                prevTx.TryDeserialize(new FastStreamReader(Helper.HexToBytes(Case["TxToSpend"].ToString())), out string _);
                byte[] prevTxHash = prevTx.GetTransactionHash();
                var utxoDb = new MockUtxoDatabase();

                for (int i = 0; i < prevTx.TxOutList.Length; i++)
                {
                    var utxo = new MockUtxo()
                    {
                        Amount = prevTx.TxOutList[i].Amount,
                        Index = (uint)i,
                        PubScript = prevTx.TxOutList[i].PubScript
                    };

                    utxoDb.Add(prevTxHash, utxo);
                }

                foreach (var item in Case["Cases"])
                {
                    Transaction tx = new Transaction();
                    FastStreamReader stream = new FastStreamReader(Helper.HexToBytes(item["SignedTx"].ToString()));
                    if (!tx.TryDeserialize(stream, out string err))
                    {
                        throw new ArgumentException($"Could not deseralize the given tx case: " +
                            $"{item["TestName"]}. Error: {err}");
                    }

                    int sigOpCount = (int)item["SigOpCount"];
                    ulong fee = (ulong)item["Fee"];

                    yield return new object[]
                    {
                        utxoDb,
                        new MockMempool(null),
                        c,
                        tx,
                        true, // Verification success
                        null, // Error
                        sigOpCount,
                        fee,
                        tx.WitnessList != null, // AnySegWit
                    };
                }
            }
        }
        [Theory]
        [MemberData(nameof(GetTxVerifyCases))]
        public void VerifyTest(IUtxoDatabase utxoDB, IMemoryPool memPool, IConsensus c, ITransaction tx, bool expB, string expErr,
                               int expSigOp, ulong expFee, bool expSeg)
        {
            // An initial amount is set for both TotalFee and TotalSigOpCount to make sure Verify() 
            // method always adds to previous values instead of setting them
            ulong initialFee = 10;
            int initialSigOp = 50;

            var verifier = new TransactionVerifier(false, utxoDB, memPool, c)
            {
                TotalFee = initialFee,
                TotalSigOpCount = initialSigOp
            };

            bool actualB = verifier.Verify(tx, out string error);

            Assert.Equal(expB, actualB);
            Assert.Equal(expErr, error);
            Assert.Equal(expSigOp + initialSigOp, verifier.TotalSigOpCount);
            Assert.Equal(expFee + initialFee, verifier.TotalFee);
            Assert.Equal(expSeg, verifier.AnySegWit);
        }
    }
}
