// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Transactions
{
    public class TransactionTests
    {
        public static IEnumerable<object[]> GetCtorNullExCases()
        {
            yield return new object[] { null, new TxOut[1], "List of transaction inputs can not be null or empty." };
            yield return new object[] { new TxIn[0], new TxOut[1], "List of transaction inputs can not be null or empty." };
            yield return new object[] { new TxIn[1], null, "List of transaction outputs can not be null or empty." };
            yield return new object[] { new TxIn[1], new TxOut[0], "List of transaction outputs can not be null or empty." };
        }
        [Theory]
        [MemberData(nameof(GetCtorNullExCases))]
        public void Constructor_NullExceptionTest(TxIn[] tins, TxOut[] touts, string expErr)
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => new Transaction(1, tins, touts, 0));
            Assert.Contains(expErr, ex.Message);
        }

        [Fact]
        public void Constructor_OutOfRangeExceptionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Transaction(-1, new TxIn[1], new TxOut[1], 0));
        }


        public enum TxCaseType
        {
            BytesToSign,
            TxId,
            FinalSignedTx
        }
        public static IEnumerable<object[]> GetSignCases(TxCaseType mode)
        {
            // Transactions are signed with bitcoin-core version 0.19.0.1 (bitcoin-0.19.0.1-x86_64-linux-gnu.tar)
            // All transactions are broadcast and are included in blocks on testnet.
            JArray jar = Helper.ReadResource<JArray>("SignedTxTestData");
            foreach (var Case in jar)
            {
                Transaction prevTx = new Transaction();
                prevTx.TryDeserialize(new FastStreamReader(Helper.HexToBytes(Case["TxToSpend"].ToString())), out string _);
                foreach (var item in Case["Cases"])
                {
                    if (mode == TxCaseType.BytesToSign || mode == TxCaseType.FinalSignedTx)
                    {
                        Transaction tx = new Transaction();
                        FastStreamReader stream = new FastStreamReader(Helper.HexToBytes(item["TxToSign"].ToString()));
                        if (!tx.TryDeserialize(stream, out string err))
                        {
                            throw new ArgumentException($"Could not deseralize the given tx case: " +
                                $"{item["TestName"]}. Error: {err}");
                        }
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
                        Transaction tx = new Transaction();
                        FastStreamReader stream = new FastStreamReader(Helper.HexToBytes(item["SignedTx"].ToString()));
                        if (!tx.TryDeserialize(stream, out string err))
                        {
                            throw new ArgumentException($"Could not deseralize the given tx case: " +
                                $"{item["TestName"]}. Error: {err}");
                        }
                        string txId = item["TxId"].ToString();
                        string wtxId = item["WTxId"].ToString();

                        yield return new object[] { tx, txId, wtxId };
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
            byte[] actualTxHash = tx.GetTransactionHash();
            byte[] expTxHash = Helper.HexToBytes(expTxId, true);

            Assert.Equal(expTxId, actualTxId);
            Assert.Equal(expWTxId, actualWTxId);
            Assert.Equal(expTxHash, actualTxHash);
        }


        [Theory]
        [MemberData(nameof(GetSignCases), TxCaseType.BytesToSign)]
        public void GetBytesToSignTest(ITransaction tx, ITransaction prvTx, SigHashType sht, int[] indexes, string[] expBytes)
        {
            Assert.Equal(indexes.Length, expBytes.Length);

            for (int i = 0; i < indexes.Length; i++)
            {
                byte[] actualBytes = tx.GetBytesToSign(prvTx, indexes[i], sht, null);
                Assert.Equal(Helper.HexToBytes(expBytes[i]), actualBytes);
            }
        }

        [Theory]
        [MemberData(nameof(GetSignCases), TxCaseType.FinalSignedTx)]
        public void Key_SignTxTest(string wif, ITransaction tx, ITransaction prvTx, SigHashType sht, int[] indexes, byte[] expSer)
        {
            using PrivateKey key = new PrivateKey(wif, NetworkType.TestNet);
            for (int i = 0; i < indexes.Length; i++)
            {
                key.Sign(tx, prvTx, indexes[i], sht);
            }

            FastStream stream = new FastStream();
            tx.Serialize(stream);
            byte[] actualSer = stream.ToByteArray();

            Assert.Equal(expSer, actualSer);
        }
    }
}
