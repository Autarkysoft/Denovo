// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Transactions
{
    public class TransactionTests
    {
        private readonly Calc calc = new();
        private readonly DSA dsa = new();

        public static IEnumerable<object[]> GetCtorNullExCases()
        {
            yield return new object[] { null, new TxOut[1], "List of transaction inputs can not be null or empty." };
            yield return new object[] { Array.Empty<TxIn>(), new TxOut[1], "List of transaction inputs can not be null or empty." };
            yield return new object[] { new TxIn[1], null, "List of transaction outputs can not be null or empty." };
            yield return new object[] { new TxIn[1], Array.Empty<TxOut>(), "List of transaction outputs can not be null or empty." };
        }
        [Theory]
        [MemberData(nameof(GetCtorNullExCases))]
        public void Constructor_NullExceptionTest(TxIn[] tins, TxOut[] touts, string expErr)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => new Transaction(1, tins, touts, 0));
            Assert.Contains(expErr, ex.Message);
        }


        public enum TxCaseType
        {
            BytesToSign,
            TxId,
            FinalSignedTx,
            Size
        }
        public static IEnumerable<object[]> GetSignCases(TxCaseType mode)
        {
            // Transactions are signed with bitcoin-core version 0.19.0.1 (bitcoin-0.19.0.1-x86_64-linux-gnu.tar)
            // All transactions are broadcast and are included in blocks on testnet.
            JArray jar = Helper.ReadResource<JArray>("SignedTxTestData");
            foreach (var Case in jar)
            {
                var prevTx = new Transaction(Case["TxToSpend"].ToString());
                foreach (var item in Case["Cases"])
                {
                    if (mode == TxCaseType.BytesToSign || mode == TxCaseType.FinalSignedTx)
                    {
                        var tx = new Transaction(item["TxToSign"].ToString());
                        SigHashType sht = (SigHashType)(uint)item["SigHashType"];
                        int[] indexes = item["Indexes"].ToObject<int[]>();

                        if (mode == TxCaseType.BytesToSign)
                        {
                            string[] hashes = item["BytesToSign"].ToObject<string[]>();
                            yield return new object[] { tx, prevTx, sht, indexes, hashes };
                        }
                        else
                        {
                            string key = item["PrivateKey"].ToString();
                            byte[] signedTx = Helper.HexToBytes(item["SignedTx"].ToString());
                            yield return new object[] { key, tx, prevTx, sht, indexes, signedTx };
                        }
                    }
                    else if (mode == TxCaseType.TxId)
                    {
                        var tx = new Transaction(item["SignedTx"].ToString());
                        string txId = item["TxId"].ToString();
                        string wtxId = item["WTxId"].ToString();

                        yield return new object[] { tx, txId, wtxId };
                    }
                    else if (mode == TxCaseType.Size)
                    {
                        string hex = item["SignedTx"].ToString();
                        var tx = new Transaction(hex);
                        int totalSize = hex.Length / 2;
                        int baseSize = (int)item["BaseSize"];
                        int weight = (int)item["Weight"];
                        int virtualSize = (int)item["VirtualSize"];

                        yield return new object[] { tx, totalSize, baseSize, weight, virtualSize };
                    }
                    else
                    {
                        throw new ArgumentException("Undefined mode");
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetSignCases), TxCaseType.TxId)]
        public void GetTxId_And_HashTest(ITransaction tx, string expTxId, string expWTxId)
        {
            string actualTxId = tx.GetTransactionId();
            string actualWTxId = tx.GetWitnessTransactionId();
            Digest256 actualTxHash = tx.GetTransactionHash();
            byte[] expTxHash = Helper.HexToBytes(expTxId, true);

            Assert.Equal(expTxId, actualTxId);
            Assert.Equal(expWTxId, actualWTxId);
            Assert.Equal(expTxHash, actualTxHash.ToByteArray());
        }


        [Theory]
        [MemberData(nameof(GetSignCases), TxCaseType.BytesToSign)]
        public void GetBytesToSignTest(ITransaction tx, ITransaction prvTx, SigHashType sht, int[] indexes, string[] expBytes)
        {
            Assert.Equal(indexes.Length, expBytes.Length);

            for (int i = 0; i < indexes.Length; i++)
            {
                byte[] actualBytes = tx.GetBytesToSign(prvTx, indexes[i], sht, null, null);
                Assert.Equal(Helper.HexToBytes(expBytes[i]), actualBytes);
            }
        }

        [Theory]
        [MemberData(nameof(GetSignCases), TxCaseType.Size)]
        public void SizeTest(ITransaction tx, int totalSize, int baseSize, int weight, int virtualSize)
        {
            Assert.Equal(totalSize, tx.TotalSize);
            Assert.Equal(baseSize, tx.BaseSize);
            Assert.Equal(weight, tx.Weight);
            Assert.Equal(virtualSize, tx.VirtualSize);
        }

        [Theory]
        [MemberData(nameof(GetSignCases), TxCaseType.FinalSignedTx)]
        public void Key_SignTxTest(string wif, ITransaction tx, ITransaction prvTx, SigHashType sht, int[] indexes, byte[] expSer)
        {
            using PrivateKey key = new(wif, NetworkType.TestNet);
            for (int i = 0; i < indexes.Length; i++)
            {
                key.Sign(dsa, calc, tx, prvTx, indexes[i], sht);
            }

            var stream = new FastStream(expSer.Length);
            tx.Serialize(stream);
            byte[] actualSer = stream.ToByteArray();

            Assert.Equal(expSer, actualSer);
        }

        [Fact]
        public void SizeChangeTest()
        {
            var tin = new TxIn(Digest256.Zero, 0, new MockSerializableSigScript(new byte[4], 1), 0);
            var tout = new TxOut(0, new MockSerializablePubScript(new byte[2], 1));
            var wit = new Witness(new byte[][] { new byte[6] });
            var tx = new Transaction()
            {
                Version = 1,
                TxInList = new TxIn[1] { tin },
                TxOutList = new TxOut[1] { tout },
                WitnessList = null,
                LockTime = 0
            };

            Assert.Equal(66, tx.TotalSize);
            Assert.Equal(66, tx.BaseSize);
            Assert.Equal(66, tx.VirtualSize);
            Assert.Equal(264, tx.Weight);

            // Change transaction and check sizes haven't changed
            tx.TxInList = new TxIn[2] { tin, tin };
            Assert.Equal(66, tx.TotalSize);
            Assert.Equal(66, tx.BaseSize);
            Assert.Equal(66, tx.VirtualSize);
            Assert.Equal(264, tx.Weight);

            // Call re-compute method and check sizes are updated
            tx.ComputeSizes();
            Assert.Equal(111, tx.TotalSize);
            Assert.Equal(111, tx.BaseSize);
            Assert.Equal(111, tx.VirtualSize);
            Assert.Equal(444, tx.Weight);

            // Same but add witness
            tx.WitnessList = new IWitness[2] { wit, wit };
            Assert.Equal(111, tx.TotalSize);
            Assert.Equal(111, tx.BaseSize);
            Assert.Equal(111, tx.VirtualSize);
            Assert.Equal(444, tx.Weight);

            tx.ComputeSizes();
            Assert.Equal(129, tx.TotalSize);
            Assert.Equal(111, tx.BaseSize);
            Assert.Equal(116, tx.VirtualSize);
            Assert.Equal(462, tx.Weight);
        }

        [Fact]
        public void HashChangeTest()
        {
            var tx = new Transaction("01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff0704ffff001d0104ffffffff0100f2052a0100000043410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac00000000");
            Assert.Equal("0e3e2357e806b6cdb1f70b54c3a3a17b6714ee1f0e68bebb44a74b1efd512098", tx.GetTransactionId());
            Assert.Equal("0e3e2357e806b6cdb1f70b54c3a3a17b6714ee1f0e68bebb44a74b1efd512098", tx.GetWitnessTransactionId());

            // Change tranasction and check hash didn't change
            tx.Version = 2;
            Assert.Equal("0e3e2357e806b6cdb1f70b54c3a3a17b6714ee1f0e68bebb44a74b1efd512098", tx.GetTransactionId());
            Assert.Equal("0e3e2357e806b6cdb1f70b54c3a3a17b6714ee1f0e68bebb44a74b1efd512098", tx.GetWitnessTransactionId());

            // Call re-compute method and check hash is changed
            tx.ComputeTransactionHashes();
            Assert.Equal("5ea04451af738d113f0ae8559225b7f893f186f099d88c72230a5e19c0bb830d", tx.GetTransactionId());
            Assert.Equal("5ea04451af738d113f0ae8559225b7f893f186f099d88c72230a5e19c0bb830d", tx.GetWitnessTransactionId());

            // Add witness for different WTxID and TxID
            tx.WitnessList = new IWitness[1] { new Witness(new byte[][] { new byte[] { 1, 2 } }) };
            // Still unchanged:
            Assert.Equal("5ea04451af738d113f0ae8559225b7f893f186f099d88c72230a5e19c0bb830d", tx.GetTransactionId());
            Assert.Equal("5ea04451af738d113f0ae8559225b7f893f186f099d88c72230a5e19c0bb830d", tx.GetWitnessTransactionId());

            // Only WTxID changes and not TxID
            tx.ComputeTransactionHashes();
            Assert.Equal("5ea04451af738d113f0ae8559225b7f893f186f099d88c72230a5e19c0bb830d", tx.GetTransactionId());
            Assert.Equal("a0761c7eb15419c58b02a2048c9d6a7795130158a57fa7a7cd0d7e209b13afde", tx.GetWitnessTransactionId());
        }
    }
}
