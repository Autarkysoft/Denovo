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
    public class ChainTests
    {
        private static Chain GetChain(IFileManager fman, BlockVerifier bver, IConsensus c, IClientTime t)
        {
            // TODO: we can mock Time too
            return new Chain(fman, bver, c, t, NetworkType.MainNet);
        }

        private static Chain GetChain()
        {
            Consensus c = new();
            MockFileManager fman = new(
                new FileManCallName[] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo },
                new byte[][] { BlockHeaderTests.GetSampleBlockHeaderBytes(), new byte[32 + 4 + 4] });
            ClientTime t = new();
            return GetChain(fman, new BlockVerifier(null, c), c, t);
        }

        private static IEnumerable<BlockHeader> GetHeaders(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new BlockHeader(i, new byte[32], new byte[32], 0, 0, 0);
            }
        }




        [Fact]
        public void Constructor_NullExceptionTest()
        {
            MockFileManager fileMan = new(null, null);
            MockBlockVerifier blockVer = new();
            MockConsensus c = new();
            MockClientTime t = new();

            Assert.Throws<ArgumentNullException>(() => new Chain(null, blockVer, c, t, NetworkType.MainNet));
            Assert.Throws<ArgumentNullException>(() => new Chain(fileMan, null, c, t, NetworkType.MainNet));
            Assert.Throws<ArgumentNullException>(() => new Chain(fileMan, blockVer, null, t, NetworkType.MainNet));
        }

        [Fact]
        public void Constructor_NullTimeTest()
        {
            MockFileManager fileMan = new(
                new FileManCallName[] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo },
                new byte[][] { BlockHeaderTests.GetSampleBlockHeaderBytes(), new byte[32 + 4 + 4] });
            MockBlockVerifier blockVer = new();
            MockConsensus c = new();

            // Null time doesn't throw:
            Chain chain = new(fileMan, blockVer, c, null, NetworkType.MainNet);
            Assert.NotNull(chain.Time);
        }


        [Theory]
        // https://github.com/bitcoin/bitcoin/blob/32b191fb66e644c690c94cbfdae6ddbc754769d7/src/test/pow_tests.cpp#L14-L60
        [InlineData(1231006505, 1233061996, 0x1d00ffffU, 0x1d00ffffU)] // 0 & 2015
        [InlineData(1261130161, 1262152739, 0x1d00ffffU, 0x1d00d86aU)] // 30240 & 32255
        [InlineData(1279008237, 1279297671, 0x1c05a3f4U, 0x1c0168fdU)] // 66528 & 68543
        [InlineData(1263163443, 1269211443, 0x1c387f6fU, 0x1d00e1fdU)] // Mocked & 46367
        public void GetNextTargetTest(uint first, uint last, uint lastNBits, uint expNBits)
        {
            Chain chain = GetChain();
            BlockHeader hd1 = new() { BlockTime = first };
            BlockHeader hd2 = new() { BlockTime = last, NBits = lastNBits };
            Target actual = chain.GetNextTarget(hd1, hd2);
            Assert.Equal((Target)expNBits, actual);
        }

        public static IEnumerable<object[]> GetLocatorCases()
        {
            Chain chain = GetChain();

            yield return new object[]
            {
                chain,
                GetHeaders(1),
                GetHeaders(1)
            };
            yield return new object[]
            {
                chain,
                GetHeaders(2),
                GetHeaders(2).Reverse()
            };
            yield return new object[]
            {
                chain,
                GetHeaders(10),
                GetHeaders(10).Reverse()
            };
            yield return new object[]
            {
                chain,
                GetHeaders(11),
                GetHeaders(11).Reverse()
            };
            yield return new object[]
            {
                chain,
                GetHeaders(12),
                GetHeaders(12).Reverse()
            };
            yield return new object[]
            {
                chain,
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
                chain,
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
                chain,
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
                chain,
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
                chain,
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
                chain,
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
                chain,
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
                chain,
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
                chain,
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
        public void GetBlockHeaderLocatorTest(Chain chain, BlockHeader[] toSet, BlockHeader[] expected)
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
