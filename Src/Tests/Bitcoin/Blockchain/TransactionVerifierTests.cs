// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
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
            var verifier = new TransactionVerifier(true, new MockUtxoDatabase(), new MockMempool(null), new MockConsensus(MockHeight));

            Helper.ComparePrivateField(verifier, "isMempool", true);
            Assert.Equal(0, verifier.BlockHeight);
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
            var c = new MockConsensus(MockHeight);

            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, null, mem, c));
            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, ut, null, c));
            Assert.Throws<ArgumentNullException>(() => new TransactionVerifier(true, ut, mem, null));
        }


        public static IEnumerable<object[]> GetCoinBasePrimCases()
        {
            var c = new MockConsensus(MockHeight) { bip34 = true };

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
            var verifier = new TransactionVerifier(false, new MockUtxoDatabase(), new MockMempool(null), consensus)
            {
                BlockHeight = MockHeight
            };

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
                new MockConsensus(MockHeight) { blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(102, null) }),
                1,
                false,
                "Coinbase generates more coins than it should."
            };
            yield return new object[]
            {
                new MockConsensus(MockHeight) { blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(102, null) }),
                10, // 110 > 102
                true,
                null
            };
            yield return new object[]
            {
                new MockConsensus(MockHeight) { blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(102, null) }),
                2, // 102 >= 102
                true,
                null
            };
            yield return new object[]
            {
                new MockConsensus(MockHeight) { blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(50, null), new TxOut(20, null), new TxOut(50, null) }),
                0,
                false,
                "Coinbase generates more coins than it should."
            };
            yield return new object[]
            {
                new MockConsensus(MockHeight) { blockReward = 100 },
                new MockTxPropInOut(null, new TxOut[] { new TxOut(50, null), new TxOut(20, null), new TxOut(50, null) }),
                150, // 150 > 120
                true,
                null
            };
            yield return new object[]
            {
                new MockConsensus(MockHeight) { blockReward = 100 },
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
                BlockHeight = MockHeight,
                TotalFee = fee,
                TotalSigOpCount = 987
            };

            bool actualB = verifier.VerifyCoinbaseOutput(tx, out string error);

            Assert.Equal(expB, actualB);
            Assert.Equal(expErr, error);
            Assert.Equal(987, verifier.TotalSigOpCount); // SigOps aren't counted/changed here anymore
            Assert.Equal(fee, verifier.TotalFee); // Fee shouldn't change
        }
    }
}
