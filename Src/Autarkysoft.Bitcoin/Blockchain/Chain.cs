// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Implementation of the blockchain that handles validation and storage of blocks and block headers.
    /// <para/>Implements <see cref="IChain"/>.
    /// </summary>
    public class Chain : IChain
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Chain"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="fileMan">File manager</param>
        /// <param name="blockVerifier">Block verifier</param>
        /// <param name="c">Consensus rules</param>
        /// <param name="t">Client time</param>
        /// <param name="netType">Network type</param>
        public Chain(IFileManager fileMan, IBlockVerifier blockVerifier, IConsensus c, IClientTime t, NetworkType netType)
        {
            FileMan = fileMan ?? throw new ArgumentNullException(nameof(fileMan));
            Consensus = c ?? throw new ArgumentNullException(nameof(c));
            BlockVer = blockVerifier ?? throw new ArgumentNullException(nameof(blockVerifier));
            Time = t ?? new ClientTime();

            network = netType;
            // TODO: find a better initial capacity
            headerList = new List<BlockHeader>(1_000_000);
            // Read Headers:
            byte[] hadba = FileMan.ReadData(HeadersFile);
            if (hadba is null || hadba.Length % BlockHeader.Size != 0)
            {
                // File doesn't exist or is corrupted
                ResetHeaders();
            }
            else
            {
                int count = hadba.Length / BlockHeader.Size;
                var stream = new FastStreamReader(hadba);
                for (int i = 0; i < count; i++)
                {
                    var temp = new BlockHeader();
                    if (temp.TryDeserialize(stream, out _))
                    {
                        headerList.Add(temp);
                    }
                    else
                    {
                        // File is corrupted, has to be instantiated
                        ResetHeaders();
                        break;
                    }
                }
            }

            // Read BlockInfo (block hash[32] | block size[4] | file name[4]):
            byte[] data = FileMan.ReadBlockInfo();
            if (data is null || data.Length == 0 || data.Length % (32 + 4 + 4) != 0)
            {
                // File doesn't exist or data is corrupted
                ResetBlockInfo();
            }
            else
            {
                Height = (data.Length / (32 + 4 + 4)) - 1;
                Tip = headerList[Height].GetHash(false);
            }
        }


        private const string HeadersFile = "Headers";
        private readonly object mainLock = new object();
        private readonly NetworkType network;

        /// <summary>
        /// List of headers
        /// </summary>
        public readonly List<BlockHeader> headerList;

        /// <summary>
        /// File manager responsible for handling data
        /// </summary>
        public IFileManager FileMan { get; set; }
        /// <summary>
        /// Consensus rules instance
        /// </summary>
        public IConsensus Consensus { get; set; }
        /// <summary>
        /// Block verifier
        /// </summary>
        public IBlockVerifier BlockVer { get; set; }
        /// <summary>
        /// Client time
        /// </summary>
        public IClientTime Time { get; }
        /// <inheritdoc/>
        public int Height { get; private set; }

        private BlockchainState _state = BlockchainState.None;
        /// <inheritdoc/>
        public BlockchainState State
        {
            get => _state;
            set
            {
                if (_state != value) // This should never be false
                {
                    _state = value;
                    if (_state == BlockchainState.BlocksSync)
                    {
                        HeaderSyncEndEvent?.Invoke(this, EventArgs.Empty);
                    }
                    else if (_state == BlockchainState.Synchronized)
                    {
                        BlockSyncEndEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public byte[] Tip { get; private set; }

        /// <inheritdoc/>
        public event EventHandler HeaderSyncEndEvent;
        /// <inheritdoc/>
        public event EventHandler BlockSyncEndEvent;

        // TODO: replace with a lighter alternative
        // TODO: remove both of the following?
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly List<IBlock> queue = new List<IBlock>(10);

        // TODO: an idea for the peer queue: it could be turned into an array with MaxConnectionCount number of items
        //       where peers are inserted in a FIFO array based on the blocks they need to download so that the process
        //       queue method doesn't have to loop through all items each time (it will only pop the first item and does
        //       the processing.
        private readonly List<INodeStatus> peerBlocksQueue = new List<INodeStatus>(10);
        private Stack<byte[]> missingBlockHashes;
        private readonly List<byte[][]> failedBlockHashes = new List<byte[][]>();


        private void ResetHeaders()
        {
            headerList.Clear();
            BlockHeader genesis = Consensus.GetGenesisBlock().Header;
            headerList.Add(genesis);
            FileMan.WriteData(genesis.Serialize(), HeadersFile);
        }

        private void ResetBlockInfo()
        {
            // TODO: Here we need to read each block file deserialize blocks without verifying them
            //       and re-construct the info file.
            IBlock genesis = Consensus.GetGenesisBlock();
            FileMan.WriteBlock(genesis);
            Height = 0;
            Tip = genesis.GetBlockHash(false);
        }


        /// <inheritdoc/>
        public Target GetNextTarget(BlockHeader first, BlockHeader last)
        {
            uint timeDiff = last.BlockTime - first.BlockTime;
            if (timeDiff < Constants.PowTargetTimespan / 4)
            {
                timeDiff = Constants.PowTargetTimespan / 4;
            }
            if (timeDiff > Constants.PowTargetTimespan * 4)
            {
                timeDiff = Constants.PowTargetTimespan * 4;
            }

            BigInteger newTar = last.NBits.ToBigInt();
            newTar *= timeDiff;
            newTar /= Constants.PowTargetTimespan;
            if (newTar > Consensus.PowLimit)
            {
                newTar = Consensus.PowLimit;
            }

            return new Target(newTar);
        }

        /// <inheritdoc/>
        public Target GetNextTarget()
        {
            // TODO: difficulty calculation is different for testnet
            int height = headerList.Count;
            if (height % Constants.DifficultyAdjustmentInterval != 0)
            {
                return headerList[^1].NBits;
            }
            else
            {
                int h = (height - 1) - (Constants.DifficultyAdjustmentInterval - 1);
                BlockHeader first = headerList[h];
                BlockHeader last = headerList[^1];
                return GetNextTarget(first, last);
            }
        }


        /// <inheritdoc/>
        public void PutBackMissingBlocks(List<Inventory> hashes)
        {
            lock (mainLock)
            {
                failedBlockHashes.Add(hashes.Select(x => x.Hash).ToArray());
            }
        }

        /// <inheritdoc/>
        public void SetMissingBlockHashes(INodeStatus nodeStatus)
        {
            lock (mainLock)
            {
                if (missingBlockHashes is null && headerList.Count - 1 != Height)
                {
                    missingBlockHashes = new Stack<byte[]>(headerList.Count);
                    for (int i = headerList.Count - 1; i > Height; i--)
                    {
                        missingBlockHashes.Push(headerList[i].GetHash(false));
                    }
                }

                if (failedBlockHashes.Count > 0)
                {
                    // Find the deepest blocks in the stack
                    int index = 0;
                    if (failedBlockHashes.Count > 1)
                    {
                        int h = int.MaxValue;
                        for (int i = 0; i < failedBlockHashes.Count; i++)
                        {
                            int j = headerList.FindIndex(x => ((Span<byte>)x.GetHash()).SequenceEqual(failedBlockHashes[i][0]));
                            Debug.Assert(j > 0);
                            if (j < h)
                            {
                                h = j;
                                index = i;
                            }
                        }
                    }

                    foreach (byte[] item in failedBlockHashes[index])
                    {
                        nodeStatus.InvsToGet.Add(new Inventory(InventoryType.WitnessBlock, item));
                    }

                    failedBlockHashes.RemoveAt(index);
                }
                else if (missingBlockHashes != null && missingBlockHashes.Count > 0)
                {
                    // TODO: change 16 to a constant?
                    int max = missingBlockHashes.Count < 16 ? missingBlockHashes.Count : 16;
                    for (int i = 0; i < max; i++)
                    {
                        byte[] h = missingBlockHashes.Pop();
                        nodeStatus.InvsToGet.Add(new Inventory(InventoryType.WitnessBlock, h));
                    }
                }
            }
        }




        /// <inheritdoc/>
        public void ProcessReceivedBlocks(INodeStatus nodeStatus)
        {
            Task.Run(() => ProcessPeerQueue(nodeStatus));
        }

        private void ProcessPeerQueue(INodeStatus nodeStatus)
        {
            lock (mainLock)
            {
                peerBlocksQueue.Add(nodeStatus);

                int index = 0;
                int max = peerBlocksQueue.Count;
                while (index < max)
                {
                    ReadOnlySpan<byte> prvHash = peerBlocksQueue[index].DownloadedBlocks[0].Header.PreviousBlockHeaderHash;
                    if (prvHash.SequenceEqual(Tip))
                    {
                        INodeStatus peer = peerBlocksQueue[index];
                        Debug.Assert(peer.InvsToGet.Count == peer.DownloadedBlocks.Count);
                        for (int i = 0; i < peer.DownloadedBlocks.Count; i++)
                        {
                            IBlock block = peer.DownloadedBlocks[i];
                            Debug.Assert(peer.InvsToGet[i].Hash.SequenceEqual(block.GetBlockHash(false)));

                            Consensus.BlockHeight = Height + 1;
                            if (BlockVer.Verify(block, out string error))
                            {
                                Height++;
                                Tip = block.GetBlockHash(false);
                                BlockVer.UpdateDB(block);
                                FileMan.WriteBlock(block);
                            }
                            else
                            {
                                for (int j = peer.DownloadedBlocks.Count - 1; j >= i; j--)
                                {
                                    missingBlockHashes.Push(peer.InvsToGet[j].Hash);
                                }

                                // TODO: put inventories back?
                                peer.InvsToGet.Clear();
                                nodeStatus.AddBigViolation();
                                break;
                            }
                        }

                        peerBlocksQueue.RemoveAt(index);
                        index = 0;
                        max = peerBlocksQueue.Count;

                        peer.InvsToGet.Clear();
                        peer.DownloadedBlocks.Clear();
                        SetMissingBlockHashes(peer);
                        peer.SignalNewInv();
                    }
                    else
                    {
                        index++;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool ProcessBlock(IBlock block, INodeStatus nodeStatus)
        {
            if (State == BlockchainState.BlocksSync)
            {
                byte[] blockHash = block.GetBlockHash(false);

                if (((ReadOnlySpan<byte>)blockHash).SequenceEqual(nodeStatus.InvsToGet[0].Hash))
                {
                    // Peer has to return blocks in the order that we asked for
                    nodeStatus.InvsToGet.RemoveAt(0);
                }
                else
                {
                    // If the block wasn't the first one in Inv list there are 2 possibilities:
                    //    1. The peer is returning blocks out of order => punish and disconnect
                    //    2. This is a random block => punish and disconnect
                    //    3. This is a new block (check against headers list)
                    //       3.1. Ignore if tip is far behind the last header or couldn't connect
                    //       3.2. Add to queue otherwie

                    int index = nodeStatus.InvsToGet.FindIndex(x => ((ReadOnlySpan<byte>)blockHash).SequenceEqual(x.Hash));
                    if (index < 0)
                    {
                        // Try to find this block's height
                        int h = -1;
                        lock (mainLock)
                        {
                            for (int i = headerList.Count - 1; i >= 0; i--)
                            {
                                if (((Span<byte>)headerList[i].GetHash(false)).SequenceEqual(block.Header.PreviousBlockHeaderHash))
                                {
                                    h = i + 1;
                                    break;
                                }
                            }
                        }

                        if (h < 0)
                        {
                            // An unknown new block, we may be missing headers (ignore)
                            return true;
                        }
                        else if (h <= headerList.Count - 1)
                        {
                            // A random block that we didn't ask for (punish and disconnect)
                            nodeStatus.AddBigViolation();
                            return false;
                        }
                        else if (h - Height > 20)
                        {
                            // A new block that is too far away (ignore)
                            return true;
                        }
                        // Else this block is going to be added to the queue
                    }
                    else
                    {
                        // Sending out of order
                        nodeStatus.AddBigViolation();
                        return false;
                    }
                }

                Task.Run(() => ProcessBlockQueue(block));
            }

            if (nodeStatus.InvsToGet.Count == 0)
            {
                SetMissingBlockHashes(nodeStatus);
            }

            return true;
        }


        private void ProcessBlockQueue(IBlock block)
        {
            semaphore.Wait();
            queue.Add(block);

            int index = 0;
            int max = queue.Count;
            while (index < max)
            {
                if (((ReadOnlySpan<byte>)block.Header.PreviousBlockHeaderHash).SequenceEqual(Tip))
                {
                    ProcessAndSaveBlock(block);
                    queue.RemoveAt(index);
                    index = 0;
                    max = queue.Count;
                }
                else
                {
                    index++;
                }
            }

            semaphore.Release();
        }

        private void ProcessAndSaveBlock(IBlock block)
        {
            Consensus.BlockHeight = Height + 1;
            if (BlockVer.Verify(block, out string error))
            {
                Height++;
                Tip = block.GetBlockHash(false);
                BlockVer.UpdateDB(block);
                FileMan.WriteBlock(block);
            }
            else
            {
                lock (mainLock)
                {
                    missingBlockHashes.Push(block.GetBlockHash(false));
                }
            }
        }


        private long GetMedianTimePast(int startIndex)
        {
            // Select at most 11 blocks from the tip (it can be less if current height is <11)
            var times = new List<long>(11);
            for (int i = 0; i < 11 && startIndex >= 0; i++, startIndex--)
            {
                times.Add(headerList[startIndex].BlockTime);
            }

            times.Sort();
            // Return the middle item (count is smaller than 11 for early blocks)
            return times[times.Count / 2];
        }

        private bool ProcessHeader(BlockHeader header, BlockHeader prvHeader, int height, Target nextTarget)
        {
            Consensus.BlockHeight = height;
            return ((ReadOnlySpan<byte>)header.PreviousBlockHeaderHash).SequenceEqual(prvHeader.GetHash()) &&
                   header.BlockTime > GetMedianTimePast(height - 1) &&
                   header.BlockTime <= Time.Now + Constants.MaxFutureBlockTime &&
                   BlockVer.VerifyHeader(header, nextTarget);
        }

        /// <inheritdoc/>
        public bool ProcessHeaders(BlockHeader[] headers, INodeStatus nodeStatus)
        {
            // TODO: handle forks/reorgs
            lock (mainLock)
            {
                // Find index of the block that the first header in the array references
                // TODO: benchmark this versus using a simple loop
                int lstIndex = headerList.FindLastIndex(x =>
                                        ((ReadOnlySpan<byte>)headers[0].PreviousBlockHeaderHash).SequenceEqual(x.GetHash()));

                int arrIndex = 0;
                if (lstIndex < 0)
                {
                    // The previous block was not found => headers are either invalid or from far ahead so
                    // the client has to send another locator to find the first block the 2 peers share.
                    nodeStatus.AddSmallViolation();
                    return true;
                }
                else if (lstIndex < headerList.Count - 1)
                {
                    // The previous block is not the last block we have. We have to check whether the rest
                    // of the headers are the same or on a fork.
                    // We had A B C D1 the headers are B C D2
                    for (lstIndex++; lstIndex < headerList.Count && arrIndex < headers.Length; lstIndex++, arrIndex++)
                    {
                        if (!((ReadOnlySpan<byte>)headerList[lstIndex].GetHash()).SequenceEqual(headers[arrIndex].GetHash()))
                        {
                            // TODO: we have to store these fork blocks somewhere to handle them later
                            //       possibly in INodeStatus with a boolean indicating there are blocks on another
                            //       chain in this node
                            return true;
                        }
                    }
                }

                if (arrIndex == headers.Length)
                {
                    // No new headers were received
                    if (State == BlockchainState.HeadersSync &&
                        Time.Now - headerList[^1].BlockTime <= TimeConstants.Seconds.OneDay)
                    {
                        State = BlockchainState.BlocksSync;
                    }
                    return false;
                }
                else
                {
                    int count = 0;
                    for (int i = arrIndex; i < headers.Length; i++)
                    {
                        if (ProcessHeader(headers[i], headerList[^1], headerList.Count, GetNextTarget()))
                        {
                            headerList.Add(headers[i]);
                            count++;
                        }
                        else
                        {
                            AppendHeadrs(headers.AsSpan().Slice(arrIndex, count).ToArray(), count);
                            nodeStatus.AddBigViolation();
                            return false;
                        }
                    }

                    AppendHeadrs(headers.AsSpan()[arrIndex..].ToArray(), count);

                    return true;
                }
            }
        }


        private void AppendHeadrs(IEnumerable<BlockHeader> headers, int count)
        {
            var stream = new FastStream(count * BlockHeader.Size);
            foreach (var item in headers)
            {
                item.Serialize(stream);
            }

            FileMan.AppendData(stream.ToByteArray(), HeadersFile);
        }

        /// <inheritdoc/>
        public BlockHeader[] GetBlockHeaderLocator()
        {
            lock (mainLock)
            {
                var result = new List<BlockHeader>(32);
                int step = 1;
                int index = headerList.Count - 1;
                long timeDiff = Time.Now - headerList[^1].BlockTime;
                if (timeDiff <= TimeSpan.FromHours(24).TotalSeconds)
                {
                    // If blockchain is already in sync (or isn't that far behind) don't include the last header
                    // so that we receive at least one header response
                    index--;
                }
                // Add first 10 headers from the tip in reverse order then increase the steps exponetially
                // until we run out of headers
                while (index >= 0)
                {
                    result.Add(headerList[index]);
                    if (result.Count > 10)
                    {
                        step *= 2;
                    }
                    index -= step;
                }

                // We want the last item to always be genesis block
                if (!ReferenceEquals(result[^1], headerList[0]))
                {
                    result.Add(headerList[0]);
                }

                return result.ToArray();
            }
        }

        /// <inheritdoc/>
        public BlockHeader[] GetMissingHeaders(byte[][] hashesToCompare, byte[] stopHash)
        {
            lock (mainLock)
            {
                int index = -1;
                if (hashesToCompare.Length == 0)
                {
                    index = headerList.FindIndex(x => ((ReadOnlySpan<byte>)stopHash).SequenceEqual(x.GetHash()));
                    if (index < 0)
                    {
                        return null;
                    }
                }
                else
                {
                    // hash order is from biggest height to the lowest
                    foreach (byte[] item in hashesToCompare)
                    {
                        index = headerList.FindIndex(x => ((ReadOnlySpan<byte>)item).SequenceEqual(x.GetHash()));
                        if (index >= 0)
                        {
                            // If the biggest height hash is found the lower ones must be the same (skip checking).
                            break;
                        }
                    }
                }

                if (index == -1)
                {
                    // If no common hash were found, we fall back to genesis block (block at index 0)
                    index = 0;
                }

                var result = new List<BlockHeader>(HeadersPayload.MaxCount);
                while (result.Count < HeadersPayload.MaxCount && index < headerList.Count)
                {
                    BlockHeader toAdd = headerList[index++];
                    result.Add(toAdd);
                    if (((ReadOnlySpan<byte>)stopHash).SequenceEqual(toAdd.GetHash()))
                    {
                        break;
                    }
                }

                return result.ToArray();
            }
        }
    }
}
