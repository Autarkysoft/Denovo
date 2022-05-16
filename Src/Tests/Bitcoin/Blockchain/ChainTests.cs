// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Bitcoin.Blockchain.Blocks;
using Tests.Bitcoin.P2PNetwork;
using Tests.Bitcoin.ValueTypesTests;
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
                yield return new BlockHeader(i, Digest256.Zero, Digest256.Zero, 0, 0, 0);
            }
        }

        private static byte[] GetBlockInfo(int height, BlockHeader last)
        {
            int len = (height + 1) * (32 + 4 + 4);
            byte[] result = new byte[len];
            Buffer.BlockCopy(last.Hash.ToByteArray(), 0, result, result.Length - 40, 32);
            return result;
        }

        private static readonly IBlock MockGenesis = new Block() { Header = BlockHeaderTests.GetSampleBlockHeader() };
        private static readonly byte[] MockGenesisBytes = BlockHeaderTests.GetSampleBlockHeaderBytes();
        private static readonly byte[] MockGenesisHash = BlockHeaderTests.GetSampleBlockHash();

        // Blocks 620279 and 620280
        private static BlockHeader Header1
        {
            get
            {
                Digest256 prv = new(Helper.HexToBytes("0000000000000000000ff7d9a0ff8e0947a2ac2d13403bc980766b95115fc437", true));
                Digest256 mrkl = new(Helper.HexToBytes("d1ba9e18f76e3490815416f3a0f84b47c005d9b2b32669f5a01b6123cf8c658c", true));
                return new(536870912, prv, mrkl, 1583387996, 387067068, 1310529803);
            }
        }

        private static readonly BlockHeader Header2 = new(536870912,
            new(Helper.HexToBytes("00000000000000000000b4269e0bf78432f91bbe7cc3a5b0ce9c476b8398d6c1", true)),
            new(Helper.HexToBytes("59026994738b6a77758e78797543fa3906628a1fd5f11a15da3df75d7c5e9397", true)),
            1583388040, 387067068, 3358651144);

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
                    expBlocks = new IBlock[] { MockGenesis }
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
                    expBlocks = new IBlock[] { MockGenesis }
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
                    expBlocks = new IBlock[] { MockGenesis }
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
            Assert.Equal(expTip, chain.Tip.ToByteArray());
            Assert.Equal(expHeaders.Length, chain.headerList.Count);

            for (int i = 0; i < expHeaders.Length; i++)
            {
                Assert.Equal(expHeaders[i].Hash, chain.headerList[i].Hash);
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
                Array.Empty<Digest256[]>(),
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
                Array.Empty<Digest256[]>(),
                Array.Empty<Digest256>(),
                new Digest256[] { headers[1].Hash },
            };
            yield return new object[]
            {
                // There is less than max number of blocks ahead of tip to download.
                // After first request the missing list will be empty.
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet).ToArray(),
                null,
                Array.Empty<Digest256[]>(),
                Array.Empty<Digest256>(),
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet-1).ToArray().Select(x => x.Hash).ToArray(),
            };
            yield return new object[]
            {
                // There is exactly max number of blocks ahead of tip to download.
                // After first request the missing list will be empty.
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+1).ToArray(),
                null,
                Array.Empty<Digest256[]>(),
                Array.Empty<Digest256>(),
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet).ToArray().Select(x => x.Hash).ToArray(),
            };
            yield return new object[]
            {
                // There is more than max number of blocks ahead of tip to download.
                // After first request the missing list will contain more hashes to download.
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+2).ToArray(),
                null,
                Array.Empty<Digest256[]>(),
                new Digest256[] { headers[Chain.MaxMissingBlockToGet+1].Hash },
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet).ToArray().Select(x => x.Hash).ToArray(),
            };
            yield return new object[]
            {
                // Same as before but with more remaining block hashes
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+3).ToArray(),
                null,
                Array.Empty<Digest256[]>(),
                new Digest256[] { headers[Chain.MaxMissingBlockToGet+1].Hash, headers[Chain.MaxMissingBlockToGet+2].Hash },
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet).ToArray().Select(x => x.Hash).ToArray(),
            };
            yield return new object[]
            {
                // The block height is bigger than 0 so download has to correctly start from height+1
                fman, blockVer, c, t,
                2,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+5).ToArray(),
                null,
                Array.Empty<Digest256[]>(),
                new Digest256[] { headers[Chain.MaxMissingBlockToGet+3].Hash, headers[Chain.MaxMissingBlockToGet+4].Hash },
                ((Span<BlockHeader>)headers).Slice(3, Chain.MaxMissingBlockToGet).ToArray().Select(x => x.Hash).ToArray(),
            };

            // In tests so far, Chain didn't have any failed block hashes to re-download. The following tests do:

            yield return new object[]
            {
                // There is 1 item (list of hashes) in Chain's failed array so the main DL list is ignored
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet).ToArray(),
                new Digest256[1][]
                {
                    new Digest256[3] { headers[4].Hash, headers[5].Hash, headers[6].Hash }
                },
                Array.Empty<Digest256[]>(), // Failed list has to be emptied after the call
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet-1).ToArray().Select(x => x.Hash).ToArray(),
                new Digest256[3] // Inv is the same as failed list
                {
                    headers[4].Hash, headers[5].Hash, headers[6].Hash
                }
            };
            yield return new object[]
            {
                // There are multiple items in Chain's failed array so the lowest height has to be chosen
                fman, blockVer, c, t,
                0,
                ((Span<BlockHeader>)headers).Slice(0, Chain.MaxMissingBlockToGet+2).ToArray(),
                new Digest256[4][]
                {
                    new Digest256[3] { headers[8].Hash, headers[9].Hash, headers[10].Hash },
                    new Digest256[2] { headers[13].Hash, headers[14].Hash },
                    new Digest256[4] { headers[3].Hash, headers[4].Hash, headers[5].Hash, headers[6].Hash },
                    new Digest256[1] { headers[15].Hash }
                },
                new Digest256[3][] // The item with lowest height is removed
                {
                    new Digest256[3] { headers[8].Hash, headers[9].Hash, headers[10].Hash },
                    new Digest256[2] { headers[13].Hash, headers[14].Hash },
                    new Digest256[1] { headers[15].Hash }
                },
                ((Span<BlockHeader>)headers).Slice(1, Chain.MaxMissingBlockToGet+1).ToArray().Select(x => x.Hash).ToArray(),
                new Digest256[4] // Inv is the item in failed list that starts with lowest height
                {
                    headers[3].Hash, headers[4].Hash, headers[5].Hash, headers[6].Hash
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetMissingBlockCases))]
        public void SetMissingBlockHashesTest(MockFileManager fman, IBlockVerifier bver, IConsensus c, IClientTime t, int height,
                                              BlockHeader[] hdrs, Digest256[][] failed,
                                              Digest256[][] expFailed, Digest256[] expMissHash, Digest256[] expInvs)
        {
            fman.ResetIndex();
            Chain chain = new(fman, bver, c, t, NetworkType.MainNet);
            Assert.Null(chain.missingBlockHashes);
            Assert.Empty(chain.failedBlockHashes);
            Assert.Equal(0, chain.Height);
            Assert.Equal(hdrs[0].Hash, chain.Tip);

            // Fake chain's properties:
            chain.headerList.Clear();
            chain.headerList.AddRange(hdrs);
            Helper.SetReadonlyProperty(chain, nameof(chain.Height), height);
            Helper.SetReadonlyProperty(chain, "Tip", hdrs[height].Hash);
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
            Digest256 rand32 = new(Helper.GetBytes(32));
            Digest256 h1 = new(HeaderHash1);
            Digest256 h2 = new(HeaderHash2);

            yield return new object[]
            {
                new List<Inventory>(0), // Set capacity to save memory
                null,
                Array.Empty<Digest256[]>()
            };
            yield return new object[]
            {
                new List<Inventory>(1),
                new Digest256[][] { new Digest256[1] { rand32 } },
                new Digest256[][] { new Digest256[1] { rand32 } }
            };
            yield return new object[]
            {
                new List<Inventory>(new Inventory[] { new(InventoryType.WitnessBlock, h1) }),
                null,
                new Digest256[][] { new Digest256[1] { h1 } }
            };
            yield return new object[]
            {
                new List<Inventory>(new Inventory[] { new(InventoryType.WitnessBlock, h1) }),
                new Digest256[][] { new Digest256[1] { rand32 } },
                new Digest256[][] { new Digest256[1] { rand32 }, new Digest256[1] { h1 } }
            };
            yield return new object[]
            {
                new List<Inventory>(new Inventory[]
                {
                    new(InventoryType.WitnessBlock, h1), new(InventoryType.WitnessBlock, h2)
                }),
                new Digest256[][] { new Digest256[1] { rand32 } },
                new Digest256[][] { new Digest256[1] { rand32 }, new Digest256[2] { h1, h2 } }
            };
        }
        [Theory]
        [MemberData(nameof(GetPutBackFailedCases))]
        public void PutBackMissingBlocksTest(List<Inventory> hashes, Digest256[][] toSet, Digest256[][] expected)
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

        private static Inventory[] BuildInv(params IBlock[] blocks)
        {
            return blocks.Select(x => new Inventory(InventoryType.WitnessBlock, x.Header.Hash)).ToArray();
        }

        public static IEnumerable<object[]> GetProcessQueueCases()
        {
            Consensus c = new();
            MockClientTime t = new();
            MockBlockVerifier bver = new();
            FileManCallName[] calls = new[] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo };
            MockFileManager fman = new(calls, new byte[][] { MockGenesis.Header.Serialize(), GetBlockInfo(0, MockGenesis.Header) });
            Block blk1 = new() { Header = new(1, new(MockGenesisHash), Digest256.Zero, 0, 0, 0) };
            Block blk2 = new() { Header = new(2, blk1.Header.Hash, Digest256.Zero, 0, 0, 0) };
            Block blk3 = new() { Header = new(3, blk2.Header.Hash, Digest256.Zero, 0, 0, 0) };
            Block blk4 = new() { Header = new(4, blk3.Header.Hash, Digest256.Zero, 0, 0, 0) };
            Block blk5 = new() { Header = new(5, blk4.Header.Hash, Digest256.Zero, 0, 0, 0) };

            Digest256 tip = new(MockGenesisHash);

            MockNodeStatus ns1 = new() { InvsToGet = new(BuildInv(blk1)), DownloadedBlocks = new(new[] { blk1 }), _isDead = false };
            MockNodeStatus ns2 = new() { InvsToGet = new(BuildInv(blk2)), DownloadedBlocks = new(new[] { blk2 }), _isDead = false };
            MockNodeStatus ns3 = new() { InvsToGet = new(BuildInv(blk3)), DownloadedBlocks = new(new[] { blk3 }) };
            MockNodeStatus ns12 = new() { InvsToGet = new(BuildInv(blk1, blk2)), DownloadedBlocks = new(new[] { blk1, blk2 }), _isDead = false };
            MockNodeStatus ns34 = new() { InvsToGet = new(BuildInv(blk3, blk4)), DownloadedBlocks = new(new[] { blk3, blk4 }), _isDead = false };
            MockNodeStatus ns45 = new() { InvsToGet = new(BuildInv(blk4, blk5)), DownloadedBlocks = new(new[] { blk4, blk5 }), _isDead = false };

            yield return new object[]
            {
                // Peer queue is empty and the new block is not after the tip to be verified
                c, t, 3, 3, // Height doesn't change
                tip, tip, // Tip doesn't change
                ns2,
                bver,
                fman,
                Array.Empty<INodeStatus>(), // Start queue
                true, // Add node status to queue
                Array.Empty<int>(), // Index of items to remove from queue
                null, // Mock failed block hashes
                Array.Empty<Digest256[]>(), // Failed blocks
            };
            yield return new object[]
            {
                // Peer queue has one item and the new block is not after the tip to be verified
                c, t, 3, 3, // Height doesn't change
                tip, tip, // Tip doesn't change
                ns2,
                bver,
                fman,
                new INodeStatus[1] { ns3 }, // Start queue
                true, // Add node status to queue
                Array.Empty<int>(), // Index of items to remove from queue
                null, // Mock failed block hashes
                Array.Empty<Digest256[]>(), // Failed blocks
            };
            yield return new object[]
            {
                // Peer queue has 1 item and the new block is after the tip and is verified 
                // but the next item is not after the new tip
                c, t, 3, 4, // Height is increased by the number of blocks processed
                tip, blk1.Header.Hash, // Tip changes to last block
                new MockNodeStatus()
                {
                    InvsToGet = new(BuildInv(blk1)),
                    DownloadedBlocks = new(new[] { blk1 }),
                    expectNewInvSignal = true,
                    _isDead = false
                },
                new MockBlockVerifier(new[] { blk1 }, new[] { blk1 }, new[] { true }),
                new MockFileManager(
                    new[] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo, FileManCallName.WriteBlock },
                    new byte[][] { MockGenesis.Header.Serialize(), GetBlockInfo(0, MockGenesis.Header), null })
                {
                    expBlocks = new[] { blk1 }
                },
                new INodeStatus[1] { ns3 }, // Start queue
                false, // Add node status to queue
                Array.Empty<int>(), // Index of items to remove from queue
                null, // Mock failed block hashes
                Array.Empty<Digest256[]>(), // Failed blocks
            };
            yield return new object[]
            {
                // Same as before but each item in queue has more than one block
                c, t, 3, 5, // Height is increased by the number of blocks processed
                tip, blk2.Header.Hash, // Tip changes to last block
                new MockNodeStatus()
                {
                    InvsToGet = new(BuildInv(blk1, blk2)),
                    DownloadedBlocks = new(new[] { blk1, blk2 }),
                    expectNewInvSignal = true,
                    _isDead = false
                },
                new MockBlockVerifier(new[] { blk1, blk2 }, new[] { blk1, blk2 }, new[] { true, true }),
                new MockFileManager(
                    new[] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo, FileManCallName.WriteBlock, FileManCallName.WriteBlock },
                    new byte[][] { MockGenesis.Header.Serialize(), GetBlockInfo(0, MockGenesis.Header), null, null })
                {
                    expBlocks = new[] { blk1, blk2 }
                },
                new INodeStatus[1] { ns45 }, // Start queue
                false, // Add node status to queue
                Array.Empty<int>(), // Index of items to remove from queue
                null, // Mock failed block hashes
                Array.Empty<Digest256[]>(), // Failed blocks
            };
            yield return new object[]
            {
                // The new item can be verified (is after the tip) and the item in queue is also after the new tip
                c, t, 3, 8, // Height is increased by the number of blocks processed
                tip, blk5.Header.Hash, // Tip changes to last block
                new MockNodeStatus()
                {
                    InvsToGet = new(BuildInv(blk1, blk2, blk3)),
                    DownloadedBlocks = new(new[] { blk1, blk2, blk3 }),
                    expectNewInvSignal = true,
                    _isDead = false
                },
                new MockBlockVerifier(new[] { blk1, blk2, blk3, blk4, blk5 },
                                      new[] { blk1, blk2, blk3, blk4, blk5 },
                                      new[] { true, true, true, true, true }),
                new MockFileManager(
                    new[] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo,
                            FileManCallName.WriteBlock, FileManCallName.WriteBlock, FileManCallName.WriteBlock,
                            FileManCallName.WriteBlock, FileManCallName.WriteBlock },
                    new byte[7][] { MockGenesis.Header.Serialize(), GetBlockInfo(0, MockGenesis.Header),null,null,null,null,null})
                {
                    expBlocks = new[] { blk1, blk2, blk3, blk4, blk5 }
                },
                new INodeStatus[1]
                {
                    new MockNodeStatus()
                    {
                        InvsToGet = new(BuildInv(blk4, blk5)),
                        DownloadedBlocks = new(new[] { blk4, blk5 }),
                        expectNewInvSignal = true,
                    _isDead = false
                    }
                },
                false, // Add node status to queue
                new int[1] { 0 }, // Index of items to remove from queue
                null, // Mock failed block hashes
                Array.Empty<Digest256[]>(), // Failed blocks
            };

            yield return new object[]
            {
                // The new block is verified but is invalid
                c, t, 3, 3, // Height doesn't change
                tip, tip, // Tip doesn't change
                new MockNodeStatus()
                {
                    InvsToGet = new(BuildInv(blk1)),
                    DownloadedBlocks = new(new[] { blk1 }),
                    bigViolation = true,
                    _isDead = true
                },
                new MockBlockVerifier(new[] { blk1 }, Array.Empty<IBlock>(), new[] { false }),
                fman,
                Array.Empty<INodeStatus>(), // Start queue
                false, // Add node status to queue
                Array.Empty<int>(), // Index of items to remove from queue
                null, // Mock failed block hashes
                new Digest256[][] { new Digest256[] { blk1.Header.Hash } }, // Failed blocks
            };
            yield return new object[]
            {
                // There are 5 new blocks, first 2 are valid and the next 3 are invalid
                c, t, 3, 5, // Height only increases by 2
                tip, blk2.Header.Hash, // Tip is second block
                new MockNodeStatus()
                {
                    InvsToGet = new(BuildInv(blk1, blk2, blk3, blk4, blk5)),
                    DownloadedBlocks = new(new[] { blk1, blk2, blk3, blk4, blk5 }),
                    bigViolation = true,
                    _isDead = true
                },
                new MockBlockVerifier(new[] { blk1, blk2, blk3 }, new[] { blk1, blk2 }, new[] { true, true, false }),
                new MockFileManager(
                    new[] { FileManCallName.ReadData_Headers, FileManCallName.ReadBlockInfo,
                            FileManCallName.WriteBlock, FileManCallName.WriteBlock },
                    new byte[4][] { MockGenesis.Header.Serialize(), GetBlockInfo(0, MockGenesis.Header),null,null})
                {
                    expBlocks = new[] { blk1, blk2 }
                },
                Array.Empty<INodeStatus>(), // Start queue
                false, // Add node status to queue
                Array.Empty<int>(), // Index of items to remove from queue
                new Digest256[][] { new Digest256[] { new(Helper.GetBytes(32)) } }, // Mock failed block hashes
                new Digest256[][]
                {
                    new Digest256[] { new(Helper.GetBytes(32)) },
                    new Digest256[] { blk3.Header.Hash, blk4.Header.Hash, blk5.Header.Hash }
                }, // Failed blocks
            };
        }
        [Theory]
        [MemberData(nameof(GetProcessQueueCases))]
        public void ProcessReceivedBlocksTest(IConsensus c, IClientTime t, int mockHeight, int expHeight,
                                              Digest256 mockTip, Digest256 expTip, MockNodeStatus ns, MockBlockVerifier bver,
                                              MockFileManager fman, INodeStatus[] mockPeerQ, bool addNsToQ, int[] qIndex,
                                              Digest256[][] mockFail, Digest256[][] expFail)
        {
            fman.ResetIndex();
            Chain chain = new(fman, bver, c, t, NetworkType.MainNet);
            Assert.Empty(chain.peerBlocksQueue);
            // Fake chain's properties:
            if (mockPeerQ is not null)
            {
                chain.peerBlocksQueue.AddRange(mockPeerQ);
            }
            if (mockFail is not null)
            {
                chain.failedBlockHashes.AddRange(mockFail);
            }
            Helper.SetReadonlyProperty(chain, nameof(chain.Height), mockHeight);
            Helper.SetReadonlyProperty(chain, nameof(chain.Tip), mockTip);

            chain.ProcessPeerQueue(ns);

            bver.AssertIndex();
            Assert.False(ns.expectNewInvSignal); // Raising event always changes bool to false
            Assert.Equal(expHeight, chain.Height);
            Assert.Equal(expTip, chain.Tip);
            Assert.Equal(expFail, chain.failedBlockHashes);

            int added = addNsToQ ? 1 : 0;
            int removed = qIndex.Length;
            if (removed > 0 || added > 0)
            {
                INodeStatus[] temp = new INodeStatus[mockPeerQ.Length + added - removed];
                int j = 0;
                for (int i = 0; i < mockPeerQ.Length; i++, j++)
                {
                    if (!qIndex.Contains(i))
                    {
                        temp[j] = mockPeerQ[i];
                    }
                }
                if (added > 0)
                {
                    temp[j] = ns;
                }

                mockPeerQ = temp;
            }

            Assert.Equal(mockPeerQ.Length, chain.peerBlocksQueue.Count);
            for (int i = 0; i < mockPeerQ.Length; i++)
            {
                Assert.Same(mockPeerQ[i], chain.peerBlocksQueue[i]);
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
            Chain chain = GetChain();
            BlockHeader hd1 = new(0, Digest256.Zero, Digest256.Zero, first, TargetTests.Example, 0);
            BlockHeader hd2 = new(0, Digest256.Zero, Digest256.Zero, last, lastNBits, 0);

            Target actual = chain.GetNextTarget(hd1, hd2);
            Assert.Equal((Target)expNBits, actual);
        }

        public static IEnumerable<object[]> GetLocatorCases()
        {
            Chain chain = GetChain();

            yield return new object[]
            {
                chain,
                GetHeaders(1).ToArray(),
                GetHeaders(1).ToArray()
            };
            yield return new object[]
            {
                chain,
                GetHeaders(2).ToArray(),
                GetHeaders(2).Reverse().ToArray()
            };
            yield return new object[]
            {
                chain,
                GetHeaders(10).ToArray(),
                GetHeaders(10).Reverse().ToArray()
            };
            yield return new object[]
            {
                chain,
                GetHeaders(11).ToArray(),
                GetHeaders(11).Reverse().ToArray()
            };
            yield return new object[]
            {
                chain,
                GetHeaders(12).ToArray(),
                GetHeaders(12).Reverse().ToArray()
            };
            yield return new object[]
            {
                chain,
                GetHeaders(13).ToArray(),
                new BlockHeader[12]
                {
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(5, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(4, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(3, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(2, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                }
            };
            yield return new object[]
            {
                chain,
                GetHeaders(14).ToArray(),
                new BlockHeader[13]
                {
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(5, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(4, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(3, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(1, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                }
            };
            yield return new object[]
            {
                chain,
                GetHeaders(15).ToArray(),
                new BlockHeader[13]
                {
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(5, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(4, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(2, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                }
            };
            yield return new object[]
            {
                chain,
                GetHeaders(16).ToArray(),
                new BlockHeader[13]
                {
                    new BlockHeader(15, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(5, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(3, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                }
            };
            yield return new object[]
            {
                chain,
                GetHeaders(17).ToArray(),
                new BlockHeader[13]
                {
                    new BlockHeader(16, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(15, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(4, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                }
            };
            yield return new object[]
            {
                chain,
                GetHeaders(18).ToArray(),
                new BlockHeader[14]
                {
                    new BlockHeader(17, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(16, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(15, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(5, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(1, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                }
            };
            yield return new object[]
            {
                chain,
                GetHeaders(19).ToArray(),
                new BlockHeader[14]
                {
                    new BlockHeader(18, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(17, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(16, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(15, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(2, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                }
            };
            yield return new object[]
            {
                chain,
                new BlockHeader[19]
                {
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(1, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(2, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(3, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(4, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(5, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(15, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(16, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(17, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(18, Digest256.Zero, Digest256.Zero, (uint)UnixTimeStamp.TimeToEpoch(DateTime.Now.Subtract(TimeSpan.FromHours(1))), 0, 0),
                },
                new BlockHeader[14]
                {
                    // Last block (18) is not included
                    new BlockHeader(17, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(16, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(15, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(5, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(1, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                }
            };

            uint yesterday = (uint)UnixTimeStamp.TimeToEpoch(DateTime.UtcNow.Subtract(TimeSpan.FromHours(25)));
            yield return new object[]
            {
                chain,
                new BlockHeader[19]
                {
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(1, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(2, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(3, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(4, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(5, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(7, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(15, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(16, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(17, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(18, Digest256.Zero, Digest256.Zero, yesterday, 0, 0),
                },
                new BlockHeader[14]
                {
                    new BlockHeader(18, Digest256.Zero, Digest256.Zero, yesterday, 0, 0),
                    new BlockHeader(17, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(16, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(15, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(14, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(13, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(12, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(11, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(10, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(9, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(8, Digest256.Zero, Digest256.Zero, 0, 0, 0),

                    new BlockHeader(6, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(2, Digest256.Zero, Digest256.Zero, 0, 0, 0),
                    new BlockHeader(0, Digest256.Zero, Digest256.Zero, 0, 0, 0),
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
