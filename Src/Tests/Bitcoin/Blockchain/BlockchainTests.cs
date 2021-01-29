// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Bitcoin.Blockchain.Blocks;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    public class BlockchainTests
    {
        private static Autarkysoft.Bitcoin.Blockchain.Blockchain GetChain(IFileManager fman, BlockVerifier bver, IConsensus c)
        {
            // TODO: we can mock Time too
            return new Autarkysoft.Bitcoin.Blockchain.Blockchain(fman, bver, c) { Time = new ClientTime() };
        }

        private static Autarkysoft.Bitcoin.Blockchain.Blockchain GetChain()
        {
            var c = new Consensus();
            var fman = new MockFileManager()
            {
                expReadFN = "Headers",
                returnReadData = BlockHeaderTests.GetSampleBlockHeaderBytes(),
            };
            return GetChain(fman, new BlockVerifier(null, c), c);
        }

        private static IEnumerable<BlockHeader> GetHeaders(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new BlockHeader(i, new byte[32], new byte[32], 0, 0, 0);
            }
        }


        [Theory]
        // https://github.com/bitcoin/bitcoin/blob/32b191fb66e644c690c94cbfdae6ddbc754769d7/src/test/pow_tests.cpp#L14-L60
        [InlineData(1231006505, 1233061996, 0x1d00ffffU, 0x1d00ffffU)] // 0 & 2015
        [InlineData(1261130161, 1262152739, 0x1d00ffffU, 0x1d00d86aU)] // 30240 & 32255
        [InlineData(1279008237, 1279297671, 0x1c05a3f4U, 0x1c0168fdU)] // 66528 & 68543
        [InlineData(1263163443, 1269211443, 0x1c387f6fU, 0x1d00e1fdU)] // Mocked & 46367
        public void GetNextTargetTest(uint first, uint last, uint lastNBits, uint expNBits)
        {
            var chain = GetChain();
            var hd1 = new BlockHeader() { BlockTime = first };
            var hd2 = new BlockHeader() { BlockTime = last, NBits = lastNBits };
            Target actual = chain.GetNextTarget(hd1, hd2);
            Assert.Equal((Target)expNBits, actual);
        }

        public static IEnumerable<object[]> GetLocatorCases()
        {
            var fman = new MockFileManager()
            {
                expReadFN = "Headers",
                returnReadData = BlockHeaderTests.GetSampleBlockHeaderBytes(),
            };
            var consensus = new MockConsensus();
            var bver = new BlockVerifier(null, consensus);

            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(1),
                GetHeaders(1)
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(2),
                GetHeaders(2).Reverse()
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(10),
                GetHeaders(10).Reverse()
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(11),
                GetHeaders(11).Reverse()
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(12),
                GetHeaders(12).Reverse()
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(13),
                new BlockHeader[12]
                {
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(5, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(4, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(3, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(2, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(14),
                new BlockHeader[13]
                {
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(5, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(4, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(3, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(1, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(15),
                new BlockHeader[13]
                {
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(5, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(4, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(2, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(16),
                new BlockHeader[13]
                {
                    new BlockHeader(15, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(5, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(3, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(17),
                new BlockHeader[13]
                {
                    new BlockHeader(16, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(15, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(4, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(18),
                new BlockHeader[14]
                {
                    new BlockHeader(17, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(16, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(15, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(5, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(1, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                GetHeaders(19),
                new BlockHeader[14]
                {
                    new BlockHeader(18, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(17, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(16, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(15, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(2, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                new BlockHeader[19]
                {
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(1, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(2, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(3, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(4, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(5, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(15, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(16, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(17, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(18, new byte[32], new byte[32], (uint)UnixTimeStamp.TimeToEpoch(DateTime.Now.Subtract(TimeSpan.FromHours(1))), 0, 0),
                },
                new BlockHeader[14]
                {
                    // Last block (18) is not included
                    new BlockHeader(17, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(16, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(15, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(5, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(1, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };

            uint yesterday = (uint)UnixTimeStamp.TimeToEpoch(DateTime.UtcNow.Subtract(TimeSpan.FromHours(25)));
            yield return new object[]
            {
                GetChain(fman, bver, consensus),
                new BlockHeader[19]
                {
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(1, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(2, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(3, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(4, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(5, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(7, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(15, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(16, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(17, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(18, new byte[32], new byte[32], yesterday, 0, 0),
                },
                new BlockHeader[14]
                {
                    new BlockHeader(18, new byte[32], new byte[32], yesterday, 0, 0),
                    new BlockHeader(17, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(16, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(15, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(14, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(13, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(12, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(11, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(10, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(9, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(8, new byte[32], new byte[32], 0, 0, 0),

                    new BlockHeader(6, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(2, new byte[32], new byte[32], 0, 0, 0),
                    new BlockHeader(0, new byte[32], new byte[32], 0, 0, 0),
                }
            };
        }

        [Theory]
        [MemberData(nameof(GetLocatorCases))]
        public void GetBlockHeaderLocatorTest(Autarkysoft.Bitcoin.Blockchain.Blockchain chain,
                                              BlockHeader[] toSet, BlockHeader[] expected)
        {
            Helper.SetPrivateField(chain, "headerList", new List<BlockHeader>(toSet));
            BlockHeader[] headers = chain.GetBlockHeaderLocator();

            Assert.Equal(expected.Length, headers.Length);
            for (int i = 0; i < headers.Length; i++)
            {
                Assert.Equal(expected[i].Serialize(), headers[i].Serialize());
            }
        }
    }
}
