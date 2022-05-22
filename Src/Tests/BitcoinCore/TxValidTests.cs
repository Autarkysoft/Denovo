// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Bitcoin.Blockchain;
using Xunit;

namespace Tests.BitcoinCore
{
    public class TxValidTests
    {
        private const int MockHeight = 123;

        public static MockConsensus GetConsensus(string flags, int mockHeight)
        {
            Assert.False(flags.Length == 0);
            Assert.False(flags.Contains(' '));

            // The flags in tx_valid.json indicate things that are NOT enabled
            MockConsensus c = new()
            {
                expHeight = mockHeight,
                bip112 = true,
                bip147 = true,
                bip16 = true,
                bip34 = true,
                bip65 = true,
                segWit = true,
                strictDer = true,
                tap = true,
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
                    // There is nothing to enable/disable for NONE.
                    // The rest are standard rules that don't exist in IConsensus
                    // some are found in OpData class.
                }
                else if (flag.Equals("P2SH", StringComparison.OrdinalIgnoreCase))
                {
                    c.bip16 = false;
                }
                else if (flag.Equals("DERSIG", StringComparison.OrdinalIgnoreCase))
                {
                    c.strictDer = false;
                }
                else if (flag.Equals("NULLDUMMY", StringComparison.OrdinalIgnoreCase))
                {
                    c.bip147 = false;
                }
                else if (flag.Equals("CHECKLOCKTIMEVERIFY", StringComparison.OrdinalIgnoreCase))
                {
                    c.bip65 = false;
                }
                else if (flag.Equals("CHECKSEQUENCEVERIFY", StringComparison.OrdinalIgnoreCase))
                {
                    c.bip112 = false;
                }
                else if (flag.Equals("WITNESS", StringComparison.OrdinalIgnoreCase))
                {
                    c.segWit = false;
                }
                // TODO: add taproot here when it was enabled and had tests
                //else if (flag.Equals("TAPROOT", StringComparison.OrdinalIgnoreCase))
                //{
                //    // c.bip341/2 = false
                //}
                else
                {
                    Assert.True(false, $"Undefined flag was found in test case: {flag}.");
                }
            }

            return c;
        }

        public static PubkeyScript GetPubScr(string scr)
        {
            FastStream stream = new(scr.Length / 2 /*A good estimate of size*/);
            foreach (string item in scr.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                // Handle any hex
                if (item.StartsWith("0x"))
                {
                    stream.Write(Helper.HexToBytes(item.Substring(2)));
                }
                // All integers such as numbers in multi-sig 1, 2,...
                else if (long.TryParse(item, out long val))
                {
                    new PushDataOp(val).WriteToStream(stream);
                }
                // All OP codes that sometime are written as OP_XX and sometimes as XX without OP_
                else if (Enum.TryParse(item.StartsWith("OP") ? item.Remove(0, 3) : item, true, out OP op))
                {
                    stream.Write((byte)op);
                }
                // Handles any OP code that fell through because the name starts with 1 (1DUP instead of DUP1)
                else if (Enum.TryParse(item.StartsWith("1") ? $"{item.Substring(1)}1" : item, true, out OP op2))
                {
                    stream.Write((byte)op2);
                }
                // Catch any script that is written badly and is not caught and parsed here
                else
                {
                    Assert.True(false, item);
                }
            }
            return new PubkeyScript(stream.ToByteArray());
        }

        public static IEnumerable<object[]> GetCases()
        {
            // https://github.com/bitcoin/bitcoin/blob/48725e64fbfb85200dd2226386fbf1cfc8fe6c1f/src/test/data/tx_valid.json

            MockMempool mockMempool = new();
            foreach (JToken item in Helper.ReadResource<JArray>("BitcoinCore.tx_valid"))
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
                    //   excluded verifyFlags
                    // ]
                    if (arr1[0] is not JArray arr2 || arr2.Any(x => x is not JArray arr3 || (arr3.Count != 3 && arr3.Count != 4)) ||
                        arr1[1].Type != JTokenType.String ||
                        arr1[2].Type != JTokenType.String)
                    {
                        Assert.True(false, $"Bad test found: {arr1}");
                    }

                    MockConsensus consensus = GetConsensus(arr1[2].ToString(), MockHeight);

                    Transaction tx = new();
                    if (!tx.TryDeserialize(new FastStreamReader(Helper.HexToBytes(arr1[1].ToString())), out Errors error))
                    {
                        Assert.True(false, error.Convert());
                    }

                    ulong totalSpent = (ulong)tx.TxOutList.Sum(x => (long)x.Amount);

                    bool isCoinbase = false;
                    foreach (JArray tinArr in arr1[0])
                    {
                        Digest256 txHash = new(Helper.HexToBytes(tinArr[0].ToString(), true));
                        int index = (int)tinArr[1];
                        isCoinbase = index == -1;
                        if (isCoinbase)
                        {
                            // Check if test vector actually has only 1 input as a coinbase transactions must
                            Assert.Single(arr1[0]);

                            // There are 2 cases neither have BIP34 enabled
                            if (tx.GetTransactionId() == "99d3825137602e577aeaf6a2e3c9620fd0e605323dc5265da4a570593be791d4" ||
                                tx.GetTransactionId() == "c0d67409923040cc766bbea12e4c9154393abef706db065ac2e07d91a9ba4f84")
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
                            PubScript = GetPubScr(tinArr[2].ToString()),
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

            Assert.True(b, BuildErrorStr(tx, error));
            Assert.Null(error);
        }

        public static string BuildErrorStr(ITransaction tx, string error)
        {
            FastStream stream = new();
            tx.Serialize(stream);
            return $"Error message: {error}{Environment.NewLine}Tx: {stream.ToByteArray().ToBase16()}";
        }
    }
}
