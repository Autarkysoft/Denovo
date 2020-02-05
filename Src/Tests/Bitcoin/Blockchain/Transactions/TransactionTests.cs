// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
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
            Transaction prevTx = new Transaction();
            prevTx.TryDeserialize(new FastStreamReader(Helper.HexToBytes("0200000001443ae503e1a4082af188f50d946e22fededc37be8f327b4dce238852f58dec9c000000006a47304402203e1d50ffd7ee37405b562aa7e58c71503f6728ce888e43541d2c4f4edae6dc7d02205c0150d7cb1a6b9a15ed8cbfcbb0901cbd8356ba95b189a10cc78bb03c3a48f70121020ba6fef9cf323988bd76ce27d961adcf40a1ea152f26cc7d238d565cc9fef7a8ffffffff150e870100000000004341045cce0ce7f36105cb2e9779e37e06939327545f8cc64df4beb981162f6ed8961ba654949a16c977437fb22e54c5bfdd6473d54efef6735db0762038e30bde8071ac36870100000000002321035cce0ce7f36105cb2e9779e37e06939327545f8cc64df4beb981162f6ed8961bac04870100000000001976a914f814d73d5a89d2781048312718143e26b65008d988ac68870100000000001976a914c8f86d0b6ada16364003713d71facc3bbf7ba19588accc870100000000001976a914c8f86d0b6ada16364003713d71facc3bbf7ba19588ac30880100000000001976a91492c5966bdd571eca551036188a3853750d5a5c1b88ac3a880100000000001976a91492c5966bdd571eca551036188a3853750d5a5c1b88ac44880100000000001976a91492c5966bdd571eca551036188a3853750d5a5c1b88ac31880100000000001976a914dea359ebbf1d7e8617ac1e32cde802a0593abed588ac32880100000000001976a914dea359ebbf1d7e8617ac1e32cde802a0593abed588ac33880100000000001976a914dea359ebbf1d7e8617ac1e32cde802a0593abed588ac94880100000000001976a914d662fb8731e706b9adb7c11292223af0a56ac23288ac9e880100000000001976a914d662fb8731e706b9adb7c11292223af0a56ac23288aca8880100000000001976a914d662fb8731e706b9adb7c11292223af0a56ac23288acf8880100000000001976a9143f8a223cb05cae3c62c9b487b4de054503cbeff288ac02890100000000001976a9143f8a223cb05cae3c62c9b487b4de054503cbeff288ac0c890100000000001976a9143f8a223cb05cae3c62c9b487b4de054503cbeff288ac5c890100000000001976a914476abdf1bf9125fd4b49a420f66bf2f7c42d608888ac66890100000000001976a914476abdf1bf9125fd4b49a420f66bf2f7c42d608888ac70890100000000001976a914476abdf1bf9125fd4b49a420f66bf2f7c42d608888acef104002000000001976a914b50a33e43bec2c74112d1d6ea42a174f435aabd688ac00000000")), out string _);

            // Transactions are signed with bitcoin-core version 0.19.0.1 (bitcoin-0.19.0.1-x86_64-linux-gnu.tar)
            // All transactions are broadcast and are included in blocks on testnet.
            JArray jar = Helper.ReadResource<JArray>("SignedTxTestData");
            foreach (var item in jar)
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
                byte[] actualBytes = tx.GetBytesToSign(prvTx, indexes[i], sht);
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
