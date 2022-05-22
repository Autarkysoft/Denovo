// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
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
            TransactionVerifier verifier = new(true, new MockUtxoDatabase(), new MockMempool(), new MockConsensus());

            Helper.ComparePrivateField(verifier, "isMempool", true);
            Assert.False(verifier.ForceLowS);
            Assert.False(verifier.StrictNumberEncoding);
            Assert.Equal(0UL, verifier.TotalFee);
            Assert.Equal(0, verifier.TotalSigOpCount);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            MockUtxoDatabase ut = new();
            MockMempool mem = new();
            MockConsensus c = new();

            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, null, mem, c));
            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, ut, null, c));
            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, ut, mem, null));
        }


        public static IEnumerable<object[]> GetCoinBasePrimCases()
        {
            MockConsensus c = new() { expHeight = MockHeight, bip34 = true };

            yield return new object[]
            {
                c,
                new MockTxPropInOut(Array.Empty<TxIn>(), null),
                false,
                "Coinbase transaction must contain only one input.",
                0,
                false
            };
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[2], null),
                false,
                "Coinbase transaction must contain only one input.",
                0,
                false
            };
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1], Array.Empty<TxOut>()),
                false,
                "Transaction must contain at least one output.",
                0,
                false
            };
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(Digest256.One, uint.MaxValue, null, 1234) }, new TxOut[1]),
                false,
                "Invalid coinbase outpoint.",
                0,
                false
            };
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(Digest256.Zero, 0, null, 1234) }, new TxOut[1]),
                false,
                "Invalid coinbase outpoint.",
                0,
                false
            };

            MockCoinbaseVerifySigScript mockFailSigScr = new(MockHeight, false);
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(Digest256.Zero, uint.MaxValue, mockFailSigScr, 1234) }, new TxOut[1]),
                false,
                "Invalid coinbase signature script.",
                0,
                false
            };

            MockCoinbaseVerifySigScript mockPassSigScr = new(MockHeight, true, 100);
            TxOut t1 = new(50, new MockSigOpCountPubScript(20));
            TxOut t2 = new(60, new MockSigOpCountPubScript(3));
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(Digest256.Zero, uint.MaxValue, mockPassSigScr, 1234) },
                                    new TxOut[2]{ t1, t2 }, Array.Empty<IWitness>()),
                true,
                null,
                492, // 4*100 + 4*20 + 4*3
                false
            };
            yield return new object[]
            {
                c,
                new MockTxPropInOut(new TxIn[1] { new TxIn(Digest256.Zero, uint.MaxValue, mockPassSigScr, 1234) },
                                    new TxOut[2]{ t1, t2 }, new Witness[1] ),
                true,
                null,
                492, // 4*100 + 4*20 + 4*3
                true
            };
        }
        [Theory]
        [MemberData(nameof(GetCoinBasePrimCases))]
        public void VerifyCoinbasePrimaryTest(IConsensus consensus, ITransaction tx, bool expB, string expErr, int expOpCount,
            bool expSegWit)
        {
            TransactionVerifier verifier = new(false, new MockUtxoDatabase(), new MockMempool(), consensus);

            bool actualB = verifier.VerifyCoinbasePrimary(tx, out string error);

            Assert.Equal(expB, actualB);
            Assert.Equal(expErr, error);
            Assert.Equal(expOpCount, verifier.TotalSigOpCount);
            Assert.Equal(0UL, verifier.TotalFee);
            Assert.Equal(expSegWit, verifier.AnySegWit);
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
            TransactionVerifier verifier = new(false, new MockUtxoDatabase(), new MockMempool(), c)
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
            MockConsensus c = new()
            {
                expHeight = MockHeight,
                bip65 = true,
                bip112 = true,
                strictDer = true,
                bip147 = true,
                bip16 = true,
                segWit = true,
            };
            Digest256 simpTxHash1 = Digest256.One;
            Digest256 simpTxHash2 = new(2);
            Digest256 simpTxHash3 = new(3);

            PubkeyScript simpPubScr = new();
            SignatureScript simpSigScr = new(new byte[1] { (byte)OP._1 });

            // *** Fee Tests ***

            yield return new object[]
            {
                new MockUtxoDatabase(simpTxHash1, new MockUtxo() { Amount = 0, Index = 0, PubScript = simpPubScr }),
                new MockMempool(),
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
                new MockMempool(),
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
                new MockMempool(),
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
                new MockMempool(),
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
                new MockUtxoDatabase(new Digest256[] { simpTxHash1, simpTxHash2, simpTxHash3 },
                new IUtxo[]
                    {
                        new MockUtxo() { Amount = 13, Index = 3, PubScript = simpPubScr },
                        new MockUtxo() { Amount = 57, Index = 7, PubScript = simpPubScr },
                        new MockUtxo() { Amount = 73, Index = 5, PubScript = simpPubScr },
                    }),
                new MockMempool(),
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
                new MockUtxoDatabase(new Digest256[] { simpTxHash1, simpTxHash2, simpTxHash3 },
                new IUtxo[]
                    {
                        new MockUtxo() { Amount = 13, Index = 3, PubScript = simpPubScr },
                        new MockUtxo() { Amount = 57, Index = 7, PubScript = simpPubScr },
                        new MockUtxo() { Amount = 73, Index = 5, PubScript = simpPubScr },
                    }),
                new MockMempool(),
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
            foreach (JToken Case in Helper.ReadResource<JArray>("SignedTxTestData"))
            {
                Transaction prevTx = new();
                prevTx.TryDeserialize(new FastStreamReader(Helper.HexToBytes(Case["TxToSpend"].ToString())), out _);
                Digest256 prevTxHash = prevTx.GetTransactionHash();
                MockUtxoDatabase utxoDb = new();

                for (int i = 0; i < prevTx.TxOutList.Length; i++)
                {
                    MockUtxo utxo = new()
                    {
                        Amount = prevTx.TxOutList[i].Amount,
                        Index = (uint)i,
                        PubScript = prevTx.TxOutList[i].PubScript
                    };

                    utxoDb.Add(prevTxHash, utxo);
                }

                foreach (JToken item in Case["Cases"])
                {
                    Transaction tx = new();
                    FastStreamReader stream = new(Helper.HexToBytes(item["SignedTx"].ToString()));
                    if (!tx.TryDeserialize(stream, out Errors err))
                    {
                        throw new ArgumentException($"Could not deseralize the given tx case: " +
                            $"{item["TestName"]}. Error: {err.Convert()}");
                    }

                    int sigOpCount = (int)item["SigOpCount"];
                    ulong fee = (ulong)item["Fee"];

                    yield return new object[]
                    {
                        utxoDb,
                        new MockMempool(),
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

            // P2SH special case (tx can be found on testnet)
            Transaction p2shSpecial = new("02000000000101270e3210e2b0feebbf577ac4640dba3f41cf93d3845f432762047d7a15de283e00000000171600145c9ac58215220fb727ad5d4592e39eade0c2f324feffffff0229511d000000000017a914b472a266d0bd89c13706a4132ccfb16f7c3b9fcb8795bff4390200000017a9149ebf6e32dbdd43b7f6b62687049454edf902358c870247304402207b1091aef93cc0f3663225a6aa82b0f4f23bb8c930bfbd48cdba7157f1de32e8022039d236eea85f2ddf31d51ab5ebbeb9acd1385209617576f311ce9f2530c7c1ec0121028d14fce7ae0b7618a7b8a18a237c836e44e8725880ef19164c0e69157262f2e756781900");
            yield return new object[]
            {
                new MockUtxoDatabase(p2shSpecial.GetTransactionHash(),
                                     new Utxo(0, p2shSpecial.TxOutList[0].Amount,p2shSpecial.TxOutList[0].PubScript)),
                new MockMempool(),
                c,
                new Transaction("010000000163bd811526dc34ece567872b7c9e2bee5580bfbde647ba6f18f879a32f98964c00000000025100ffffffff01414d1d000000000017a914a89aec4cd53e6d74215332459b7fea3ec4aca9758700000000"),
                true, // Verification success
                null, // Error
                0, // Added SigOpCount
                1000, // Added fee
                false, // AnySegWit
            };
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

            TransactionVerifier verifier = new(false, utxoDB, memPool, c)
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
