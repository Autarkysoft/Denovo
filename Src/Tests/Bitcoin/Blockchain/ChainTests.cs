// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Bitcoin.Blockchain.Blocks;
using Tests.Bitcoin.P2PNetwork;
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

        private static byte[] GetBlockInfo(int height, BlockHeader last)
        {
            int len = (height + 1) * (32 + 4 + 4);
            byte[] result = new byte[len];
            Buffer.BlockCopy(last.GetHash(), 0, result, result.Length - 40, 32);
            return result;
        }

        private static readonly IBlock MockGenesis = new Block() { Header = BlockHeaderTests.GetSampleBlockHeader() };
        private static readonly byte[] MockGenesisBytes = BlockHeaderTests.GetSampleBlockHeaderBytes();
        private static readonly byte[] MockGenesisHash = BlockHeaderTests.GetSampleBlockHash();

        // Blocks 620279 and 620280
        private static BlockHeader Header1 => new()
        {
            Version = 536870912,
            PreviousBlockHeaderHash = Helper.HexToBytes("0000000000000000000ff7d9a0ff8e0947a2ac2d13403bc980766b95115fc437", true),
            MerkleRootHash = Helper.HexToBytes("d1ba9e18f76e3490815416f3a0f84b47c005d9b2b32669f5a01b6123cf8c658c", true),
            BlockTime = 1583387996,
            NBits = 387067068,
            Nonce = 1310529803
        };
        private static readonly BlockHeader Header2 = new()
        {
            Version = 536870912,
            PreviousBlockHeaderHash = Helper.HexToBytes("00000000000000000000b4269e0bf78432f91bbe7cc3a5b0ce9c476b8398d6c1", true),
            MerkleRootHash = Helper.HexToBytes("59026994738b6a77758e78797543fa3906628a1fd5f11a15da3df75d7c5e9397", true),
            BlockTime = 1583388040,
            NBits = 387067068,
            Nonce = 3358651144
        };
        private static readonly byte[] HeaderBytes1 = Helper.HexToBytes("0000002037c45f11956b7680c93b40132daca247098effa0d9f70f0000000000000000008c658ccf23611ba0f56926b3b2d905c0474bf8a0f316548190346ef7189ebad15c95605ebc2c12170b191d4e");
        private static readonly byte[] HeaderBytes2 = Helper.HexToBytes("00000020c1d698836b479cceb0a5c37cbe1bf93284f70b9e26b40000000000000000000097935e7c5df73dda151af1d51f8a620639fa437579788e75776a8b73946902598895605ebc2c121708f330c8");
        private static readonly byte[] HeaderHash1 = Helper.HexToBytes("00000000000000000000b4269e0bf78432f91bbe7cc3a5b0ce9c476b8398d6c1", true);
        private static readonly byte[] HeaderHash2 = Helper.HexToBytes("000000000000000000053ff96dc5b3e7894fcd2f0aa2993884a6e6bedd58885c", true);


        public static IEnumerable<object[]> GetCtorCases()
        {
            MockBlockVerifier blockVer = new();
            MockConsensus c = new();
            MockClientTime t = new();

            yield return new object[]
            {
                // Header and block info files don't exist
                blockVer,
                new MockConsensus() { _genesis = MockGenesis },
                t,
                0,
                MockGenesisHash,
                new MockFileManager(
                    new FileManCallName[4]
                    {
                        FileManCallName.ReadData_Headers, FileManCallName.WriteData_Headers,
                        FileManCallName.ReadBlockInfo, FileManCallName.WriteBlock
                    },
                    new byte[4][] { null, MockGenesisBytes, null, null })
                {
                    expBlock = MockGenesis
                },
                new BlockHeader[] { MockGenesis.Header }
            };
            yield return new object[]
            {
                // Header and block info files are corrupted
                blockVer,
                new MockConsensus() { _genesis = MockGenesis },
                t,
                0,
                MockGenesisHash,
                new MockFileManager(
                    new FileManCallName[4]
                    {
                        FileManCallName.ReadData_Headers, FileManCallName.WriteData_Headers,
                        FileManCallName.ReadBlockInfo, FileManCallName.WriteBlock
                    },
                    new byte[4][] { new byte[3], MockGenesisBytes, new byte[3], null })
                {
                    expBlock = MockGenesis
                },
                new BlockHeader[] { MockGenesis.Header }
            };
            yield return new object[]
            {
                // Header file has a valid header but is corrupted and block info file doesn't exist
                blockVer,
                new MockConsensus() { _genesis = MockGenesis },
                t,
                0,
                MockGenesisHash,
                new MockFileManager(
                    new FileManCallName[4]
                    {
                        FileManCallName.ReadData_Headers, FileManCallName.WriteData_Headers,
                        FileManCallName.ReadBlockInfo, FileManCallName.WriteBlock
                    },
                    new byte[4][]
                    {
                        // Second header is not written correctly (the whole file is considered corrupted)
                        HeaderBytes1.ConcatFast(new byte[3]),
                        MockGenesisBytes, // Note that genesis block is written to disk not header1
                        null,
                        null
                    })
                {
                    expBlock = MockGenesis
                },
                new BlockHeader[] { MockGenesis.Header }
            };
            yield return new object[]
            {
                // Header and block info file are both good
                blockVer, c, t,
                1, // 2 blocks
                HeaderHash2, // Second block's hash is tip
                new MockFileManager(
                    new FileManCallName[2] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo },
                    new byte[2][]
                    {
                        Helper.ConcatBytes(160, HeaderBytes1, HeaderBytes2),
                        Helper.ConcatBytes(80, HeaderHash1, new byte[8], HeaderHash2, new byte[8])
                    }),
                new BlockHeader[] { Header1, Header2 }
            };
        }
        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void ConstructorTest(IBlockVerifier bver, IConsensus c, IClientTime time, int expHeight, byte[] expTip,
                                    IFileManager fMan, BlockHeader[] expHeaders)
        {
            Chain chain = new(fMan, bver, c, time, NetworkType.MainNet);

            Assert.Same(bver, chain.BlockVer);
            Assert.Same(c, chain.Consensus);
            Assert.Same(fMan, chain.FileMan);
            Assert.Same(time, chain.Time);
            Assert.Equal(expHeight, chain.Height);
            Assert.Equal(BlockchainState.None, chain.State);
            Assert.Equal(expTip, chain.Tip);
            Assert.Equal(expHeaders.Length, chain.headerList.Count);

            for (int i = 0; i < expHeaders.Length; i++)
            {
                Assert.Equal(expHeaders[i].GetHash(), chain.headerList[i].GetHash());
            }

            if (fMan is MockFileManager mockFM)
            {
                // Make sure all calls were made
                mockFM.AssertIndex();
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


        public static IEnumerable<object[]> GetMissingBlockCases()
        {
            MockBlockVerifier blockVer = new();
            MockConsensus c = new();
            MockClientTime t = new();
            BlockHeader[] headers = GetHeaders(40).ToArray();
            FileManCallName[] calls = new FileManCallName[] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo };
            MockFileManager fman = new(calls, new byte[2][] { headers[0].Serialize(), GetBlockInfo(0, headers[0]) });

            yield return new object[]
            {
                // Only have Genesis block header so there is no block to download
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, 1).ToArray(),
                null,
                Array.Empty<byte[][]>(),
                null,
                null,
            };
            yield return new object[]
            {
                // There is only one block ahead of the tip to download
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, 2).ToArray(),
                null,
                Array.Empty<byte[][]>(),
                Array.Empty<byte[]>(),
                new byte[][] { headers[1].GetHash() },
            };
            yield return new object[]
            {
                // There is less than max number of blocks ahead of tip to download.
                // After first request the missing list will be empty.
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet).ToArray(),
                null,
                Array.Empty<byte[][]>(),
                Array.Empty<byte[]>(),
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet-1).ToArray().Select(x => x.GetHash()),
            };
            yield return new object[]
            {
                // There is exactly max number of blocks ahead of tip to download.
                // After first request the missing list will be empty.
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+1).ToArray(),
                null,
                Array.Empty<byte[][]>(),
                Array.Empty<byte[]>(),
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet).ToArray().Select(x => x.GetHash()),
            };
            yield return new object[]
            {
                // There is more than max number of blocks ahead of tip to download.
                // After first request the missing list will contain more hashes to download.
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+2).ToArray(),
                null,
                Array.Empty<byte[][]>(),
                new byte[][] { headers[Chain.MaxMissingBlockToGet+1].GetHash() },
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet).ToArray().Select(x => x.GetHash()),
            };
            yield return new object[]
            {
                // Same as before but with more remaining block hashes
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+3).ToArray(),
                null,
                Array.Empty<byte[][]>(),
                new byte[][] { headers[Chain.MaxMissingBlockToGet+1].GetHash(), headers[Chain.MaxMissingBlockToGet+2].GetHash() },
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet).ToArray().Select(x => x.GetHash()),
            };
            yield return new object[]
            {
                // The block height is bigger than 0 so download has to correctly start from height+1
                fman, blockVer, c, t,
                2,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+5).ToArray(),
                null,
                Array.Empty<byte[][]>(),
                new byte[][] { headers[Chain.MaxMissingBlockToGet+3].GetHash(), headers[Chain.MaxMissingBlockToGet+4].GetHash() },
                ((Span<BlockHeader>)headers).Slice(3, Chain.MaxMissingBlockToGet).ToArray().Select(x => x.GetHash()),
            };

            // In tests so far, Chain didn't have any failed block hashes to re-download. The following tests do:

            yield return new object[]
            {
                // There is 1 item (list of hashes) in Chain's failed array so the main DL list is ignored
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet).ToArray(),
                new byte[1][][]
                {
                    new byte[3][] { headers[4].GetHash(), headers[5].GetHash(), headers[6].GetHash() }
                },
                Array.Empty<byte[][]>(), // Failed list has to be emptied after the call
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet-1).ToArray().Select(x => x.GetHash()),
                new byte[3][] // Inv is the same as failed list
                {
                    headers[4].GetHash(), headers[5].GetHash(), headers[6].GetHash()
                }
            };
            yield return new object[]
            {
                // There are multiple items in Chain's failed array so the lowest height has to be chosen
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+2).ToArray(),
                new byte[4][][]
                {
                    new byte[3][] { headers[8].GetHash(), headers[9].GetHash(), headers[10].GetHash() },
                    new byte[2][] { headers[13].GetHash(), headers[14].GetHash() },
                    new byte[4][] { headers[3].GetHash(), headers[4].GetHash(), headers[5].GetHash(), headers[6].GetHash() },
                    new byte[1][] { headers[15].GetHash() }
                },
                new byte[3][][] // The item with lowest height is removed
                {
                    new byte[3][] { headers[8].GetHash(), headers[9].GetHash(), headers[10].GetHash() },
                    new byte[2][] { headers[13].GetHash(), headers[14].GetHash() },
                    new byte[1][] { headers[15].GetHash() }
                },
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet+1).ToArray().Select(x => x.GetHash()),
                new byte[4][] // Inv is the item in failed list that starts with lowest height
                {
                    headers[3].GetHash(), headers[4].GetHash(), headers[5].GetHash(), headers[6].GetHash()
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetMissingBlockCases))]
        public void SetMissingBlockHashesTest(MockFileManager fman, IBlockVerifier bver, IConsensus c, IClientTime t, int height,
                                              BlockHeader[] hdrs, byte[][][] failed,
                                              byte[][][] expFailed, byte[][] expMissHash, byte[][] expInvs)
        {
            fman.ResetIndex();
            Chain chain = new(fman, bver, c, t, NetworkType.MainNet);
            Assert.Null(chain.missingBlockHashes);
            Assert.Empty(chain.failedBlockHashes);
            Assert.Equal(0, chain.Height);
            Assert.Equal(hdrs[0].GetHash(), chain.Tip);

            // Fake chain's properties:
            chain.headerList.Clear();
            chain.headerList.AddRange(hdrs);
            Helper.SetReadonlyProperty(chain, nameof(chain.Height), height);
            Buffer.BlockCopy(hdrs[height].GetHash(), 0, chain.Tip, 0, 32);
            if (failed is not null)
            {
                chain.failedBlockHashes.AddRange(failed);
            }

            // Set missing blocks:
            MockNodeStatus ns = new();
            chain.SetMissingBlockHashes(ns);

            Assert.Equal(expFailed, chain.failedBlockHashes);

            if (expMissHash is null)
            {
                Assert.Null(chain.missingBlockHashes);
                Assert.Empty(ns.InvsToGet);
            }
            else
            {
                Assert.NotNull(chain.missingBlockHashes);
                Assert.Equal(expMissHash.Length, chain.missingBlockHashes.Count);
                for (int i = 0; i < expMissHash.Length; i++)
                {
                    Assert.Equal(expMissHash[i], chain.missingBlockHashes.ElementAt(i));
                }

                Assert.Equal(expInvs.Length, ns.InvsToGet.Count);
                for (int i = 0; i < expInvs.Length; i++)
                {
                    Assert.Equal(InventoryType.WitnessBlock, ns.InvsToGet[i].InvType);
                    Assert.Equal(expInvs[i], ns.InvsToGet[i].Hash);
                }
            }
        }


        public static IEnumerable<object[]> GetPutBackFailedCases()
        {
            byte[] rand32 = Helper.GetBytes(32);

            yield return new object[]
            {
                new List<Inventory>(0), // Set capacity to save memory
                null,
                Array.Empty<byte[][]>()
            };
            yield return new object[]
            {
                new List<Inventory>(1),
                new byte[][][] { new byte[1][] { rand32 } },
                new byte[][][] { new byte[1][] { rand32 } }
            };
            yield return new object[]
            {
                new List<Inventory>(new Inventory[] { new(InventoryType.WitnessBlock, HeaderHash1) }),
                null,
                new byte[][][] { new byte[1][] { HeaderHash1 } }
            };
            yield return new object[]
            {
                new List<Inventory>(new Inventory[] { new(InventoryType.WitnessBlock, HeaderHash1) }),
                new byte[][][] { new byte[1][] { rand32 } },
                new byte[][][] { new byte[1][] { rand32 }, new byte[1][] { HeaderHash1 } }
            };
            yield return new object[]
            {
                new List<Inventory>(new Inventory[]
                {
                    new(InventoryType.WitnessBlock, HeaderHash1), new(InventoryType.WitnessBlock, HeaderHash2)
                }),
                new byte[][][] { new byte[1][] { rand32 } },
                new byte[][][] { new byte[1][] { rand32 }, new byte[2][] { HeaderHash1, HeaderHash2 } }
            };
        }
        [Theory]
        [MemberData(nameof(GetPutBackFailedCases))]
        public void PutBackMissingBlocksTest(List<Inventory> hashes, byte[][][] toSet, byte[][][] expected)
        {
            Chain chain = GetChain();
            Assert.Empty(chain.failedBlockHashes);

            if (toSet is not null)
            {
                chain.failedBlockHashes.AddRange(toSet);
            }

            chain.PutBackMissingBlocks(hashes);
            Assert.Equal(expected, chain.failedBlockHashes);
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
            chain.headerList.Clear();
            chain.headerList.AddRange(toSet);

            BlockHeader[] headers = chain.GetBlockHeaderLocator();

            Assert.Equal(expected.Length, headers.Length);
            for (int i = 0; i < headers.Length; i++)
            {
                Assert.Equal(expected[i].Serialize(), headers[i].Serialize());
            }
        }
    }
}
