// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Bitcoin.Blockchain;
using Xunit;

namespace Tests.BitcoinCore
{
    public class TxInvalidTests
    {
        private const int MockHeight = 123;

        public static MockConsensus GetConsensus(string flags, int mockHeight)
        {
            Assert.False(flags.Length == 0);
            Assert.False(flags.Contains(' '));

            MockConsensus c = new()
            {
                expHeight = mockHeight,
                bip112 = false,
                bip147 = false,
                bip16 = false,
                bip34 = false,
                bip65 = false,
                segWit = false,
                strictDer = false,
                tap = false,
            };

            // https://github.com/bitcoin/bitcoin/blob/48725e64fbfb85200dd2226386fbf1cfc8fe6c1f/src/test/transaction_tests.cpp#L43-L62
            foreach (string flag in flags.Split(','))
            {
                if (flag.Equals("NONE", StringComparison.OrdinalIgnoreCase) ||
                    // STRICTENC (SCRIPT_VERIFY_STRICTENC flag): strict DER sig and pubkey encodings
                    flag.Equals("STRICTENC", StringComparison.OrdinalIgnoreCase) ||
                    // LOW_S (SCRIPT_VERIFY_LOW_S flag)
                    flag.Equals("LOW_S", StringComparison.OrdinalIgnoreCase) ||
                    // SIGPUSHONLY (SCRIPT_VERIFY_SIGPUSHONLY flag)
                    flag.Equals("SIGPUSHONLY", StringComparison.OrdinalIgnoreCase) ||
                    // MINIMALDATA (SCRIPT_VERIFY_MINIMALDATA flag)
                    flag.Equals("MINIMALDATA", StringComparison.OrdinalIgnoreCase) ||
                    // SCRIPT_VERIFY_DISCOURAGE_UPGRADABLE_NOPS flag: rejects NOPs
                    flag.Equals("DISCOURAGE_UPGRADABLE_NOPS", StringComparison.OrdinalIgnoreCase) ||
                    // SCRIPT_VERIFY_CLEANSTACK flag: exactly 1 item must remain after script execution
                    flag.Equals("CLEANSTACK", StringComparison.OrdinalIgnoreCase) ||
                    // SCRIPT_VERIFY_MINIMALIF flag
                    flag.Equals("MINIMALIF", StringComparison.OrdinalIgnoreCase) ||
                    // SCRIPT_VERIFY_NULLFAIL flag
                    flag.Equals("NULLFAIL", StringComparison.OrdinalIgnoreCase) ||
                    // SCRIPT_VERIFY_DISCOURAGE_UPGRADABLE_WITNESS_PROGRAM flag
                    flag.Equals("DISCOURAGE_UPGRADABLE_WITNESS_PROGRAM", StringComparison.OrdinalIgnoreCase) ||
                    // SCRIPT_VERIFY_WITNESS_PUBKEYTYPE flag
                    flag.Equals("WITNESS_PUBKEYTYPE", StringComparison.OrdinalIgnoreCase) ||
                    // SCRIPT_VERIFY_CONST_SCRIPTCODE
                    flag.Equals("CONST_SCRIPTCODE", StringComparison.OrdinalIgnoreCase))
                {
                    // There is nothing to enable/disable for NONE
                    // The rest are standard rules that don't exist in IConsensus
                }
                else if (flag.Equals("BADTX", StringComparison.OrdinalIgnoreCase))
                {
                    // Ignore
                }
                else if (flag.Equals("P2SH", StringComparison.OrdinalIgnoreCase))
                {
                    c.bip16 = true;
                }
                else if (flag.Equals("DERSIG", StringComparison.OrdinalIgnoreCase))
                {
                    c.strictDer = true;
                }
                else if (flag.Equals("NULLDUMMY", StringComparison.OrdinalIgnoreCase))
                {
                    c.bip147 = true;
                }
                else if (flag.Equals("CHECKLOCKTIMEVERIFY", StringComparison.OrdinalIgnoreCase))
                {
                    c.bip65 = true;
                }
                else if (flag.Equals("CHECKSEQUENCEVERIFY", StringComparison.OrdinalIgnoreCase))
                {
                    c.bip112 = true;
                }
                else if (flag.Equals("WITNESS", StringComparison.OrdinalIgnoreCase))
                {
                    c.segWit = true;
                }
                // TODO: add taproot here when it was enabled and had tests
                //else if (flag.Equals("TAPROOT", StringComparison.OrdinalIgnoreCase))
                //{
                //    // c.bip341/2 = true
                //}
                else
                {
                    Assert.True(false, $"Undefined flag was found in test case: {flag}.");
                }
            }

            return c;
        }

        public static IEnumerable<object[]> GetCases()
        {
            // https://github.com/bitcoin/bitcoin/blob/48725e64fbfb85200dd2226386fbf1cfc8fe6c1f/src/test/data/tx_invalid.json

            MockMempool mockMempool = new(null);
            foreach (JToken item in Helper.ReadResource<JArray>("BitcoinCore.tx_invalid"))
            {
                MockUtxoDatabase mockDB = new();
                if (item is JArray arr1 && arr1.Count == 3)
                {
                    // [
                    //   [
                    //     [prevout hash, prevout index, prevout scriptPubKey, amount?],
                    //     [input 2],
                    //     ...
                    //   ],
                    //   serializedTransaction,
                    //   verifyFlags
                    // ]
                    if (arr1[0] is not JArray arr2 || arr2.Any(x => x is not JArray arr3 || (arr3.Count != 3 && arr3.Count != 4)) ||
                        arr1[1].Type != JTokenType.String ||
                        arr1[2].Type != JTokenType.String)
                    {
                        Assert.True(false, $"Bad test found: {arr1}");
                    }

                    if (arr1[1].ToString() == "01000000010001000000000000000000000000000000000000000000000000000000000000000000006d483045022027deccc14aa6668e78a8c9da3484fbcd4f9dcc9bb7d1b85146314b21b9ae4d86022100d0b43dece8cfb07348de0ca8bc5b86276fa88f7f2138381128b7c36ab2e42264012321029bb13463ddd5d2cc05da6e84e37536cb9525703cfd8f43afdb414988987a92f6acffffffff020040075af075070001510001000000000000015100000000")
                    {
                        // This tx has 2 outputs one with max supply (21 million) and another with a small one
                        // Since total is higher than max supply the tx is invalid.
                        // We rely on the fact that our UTXO database is correct and reliable
                        continue;
                    }

                    if (arr1[1].ToString() == "0100000000010300010000000000000000000000000000000000000000000000000000000000000000000000ffffffff00010000000000000000000000000000000000000000000000000000000000000100000000ffffffff00010000000000000000000000000000000000000000000000000000000000000200000000ffffffff03e8030000000000000151d0070000000000000151b80b00000000000001510002483045022100a3cec69b52cba2d2de623ffffffffff1606184ea55476c0f8189fda231bc9cbb022003181ad597f7c380a7d1c740286b1d022b8b04ded028b833282e055e03b8efef812103596d3451025c19dbbdeb932d6bf8bfb4ad499b95b6f88db8899efac102e5fc710000000000" ||
                        arr1[1].ToString() == "010000000100010000000000000000000000000000000000000000000000000000000000000000000000ffffffff01e803000000000000015100000000")
                    {
                        // This has a higher than 0 witness program version and by "standard rules" is rejected
                        // We don't have this rule and probably won't add it either.
                        Assert.Contains("DISCOURAGE_UPGRADABLE_WITNESS_PROGRAM", arr1[2].ToString());
                        continue;
                    }

                    string flags = arr1[2].ToString();
                    if (flags.Contains("CONST_SCRIPTCODE"))
                    {
                        // We don't reject CodeSeparator and signature in script being signed
                        continue;
                    }

                    MockConsensus consensus = GetConsensus(arr1[2].ToString(), MockHeight);

                    Transaction tx = new();
                    if (!tx.TryDeserialize(new FastStreamReader(Helper.HexToBytes(arr1[1].ToString())), out Errors error))
                    {
                        if (error == Errors.TxOutCountZero)
                        {
                            // This check takes place during deserialization of blocks/transactions not during verification.
                            // In other words there is no way to instantiate a transaction with zero inputs/outputs.
                            // When TryDeserialize fails with this error, we consider this test successful.
                            continue;
                        }
                        if (error == Errors.TxAmountOverflow)
                        {
                            // Another check that should take place during deserialization not verification.
                            continue;
                        }

                        // Any other error and this test case is considered broken:
                        Assert.True(false, $"{error}{Environment.NewLine}{arr1[1]}");
                    }

                    ulong totalSpent = (ulong)tx.TxOutList.Sum(x => (long)x.Amount);

                    bool isCoinbase = false;
                    foreach (JArray tinArr in arr1[0])
                    {
                        byte[] txHash = Helper.HexToBytes(tinArr[0].ToString(), true);
                        int index = (int)tinArr[1];
                        isCoinbase = index == -1;
                        if (isCoinbase)
                        {
                            // There are 2 cases neither have BIP34 enabled
                            if (tx.GetTransactionId() == "bf93c6fe89592b2508f2876c6200d5ffadbd741e18c57b5e9fe5eff3101137b3" ||
                                tx.GetTransactionId() == "d12fb29f9b00aaa9e89f5c34a27f43dd73f729aa796b36da0738b97f00587d0b" ||
                                tx.GetTransactionId() == "a87439547de75492b9fa7299b0ba1cf729c2ae53e84979bb713175171e93fe74" ||
                                tx.GetTransactionId() == "2d3f5c65b0ae97f5f52bef7f3b9c8ffeaeed0957289b5750a1c75bf30dd4493a")
                            {
                                consensus.bip34 = false;
                            }
                            else
                            {
                                Assert.True(false, "A new coinbase test vector is detected");
                            }

                            break;
                        }
                        MockUtxo utxo = new()
                        {
                            Index = (uint)index,
                            PubScript = TxValidTests.GetPubScr(tinArr[2].ToString()),
                            // If amount is not set, it is changed to maximum spent by tx to satisfy the fee check
                            Amount = tinArr.Count == 4 ? (ulong)tinArr[3] : totalSpent
                        };

                        mockDB.Add(txHash, utxo);
                    }


                    yield return new object[]
                    {
                        mockDB, mockMempool, consensus, tx, isCoinbase
                    };
                }
            }
        }
        [Theory]
        [MemberData(nameof(GetCases))]
        public void TxTest(IUtxoDatabase utxoDb, IMemoryPool mempool, IConsensus consensus, ITransaction tx, bool isCoinbase)
        {
            TransactionVerifier verifier = new(false, utxoDb, mempool, consensus);

            bool b = isCoinbase ?
                     verifier.VerifyCoinbasePrimary(tx, out string error) :
                     verifier.Verify(tx, out error);

            Assert.False(b, TxValidTests.BuildErrorStr(tx, error));
            Assert.NotNull(error);
        }
    }
}
