// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Result of processing a block or header
    /// </summary>
    public enum BlockProcessResult
    {
        /// <summary>
        /// Block or header with an unknown previous hash
        /// </summary>
        UnknownBlocks,
        /// <summary>
        /// Block or header that is invalid
        /// </summary>
        InvalidBlocks,
        /// <summary>
        /// A valid block or header that is on another chain (shorter than locally stored best chain)
        /// </summary>
        ForkBlocks,
        /// <summary>
        /// A valid block or header
        /// </summary>
        Success
    }


    /// <summary>
    /// Implementation of the blockchain that handles validation and storage of blocks and block headers.
    /// <para/>Implements <see cref="IBlockchain"/>.
    /// </summary>
    public class Blockchain : IBlockchain
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Blockchain"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="fileMan">File manager</param>
        /// <param name="blockVerifier">Block verifier</param>
        /// <param name="consensus">Consensus rules</param>
        public Blockchain(IFileManager fileMan, BlockVerifier blockVerifier, IConsensus consensus)
        {
            FileMan = fileMan ?? throw new ArgumentNullException(nameof(fileMan));
            Consensus = consensus ?? throw new ArgumentNullException(nameof(consensus));
            BlockVer = blockVerifier ?? throw new ArgumentNullException(nameof(blockVerifier));

            // TODO: find a better initial capacity
            headerList = new List<BlockHeader>(700_000);
            ReadHeaders();

            // TODO: read blocks and others
        }


        private const string HeadersFile = "Headers";
        private readonly List<BlockHeader> headerList;
        private readonly object mainLock = new object();

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
        public BlockVerifier BlockVer { get; set; }


        /// <inheritdoc/>
        public int Height => headerList.Count - 1;

        private void ReadHeaders()
        {
            byte[] hadba = FileMan.ReadData(HeadersFile);
            if (hadba is null || hadba.Length % Constants.BlockHeaderSize != 0)
            {
                // File doesn't exist or is corrupted
                BlockHeader genesis = Consensus.GetGenesisBlock().Header;
                headerList.Add(genesis);
                FileMan.WriteData(genesis.Serialize(), HeadersFile);
            }
            else
            {
                int count = hadba.Length / Constants.BlockHeaderSize;
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
                        headerList.Clear();
                        BlockHeader genesis = Consensus.GetGenesisBlock().Header;
                        headerList.Add(genesis);
                        FileMan.WriteData(genesis.Serialize(), HeadersFile);
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public int FindHeight(ReadOnlySpan<byte> prevHash)
        {
            for (int i = 0; i < headerList.Count; i++)
            {
                if (prevHash.SequenceEqual(headerList[i].GetHash()))
                {
                    return i + 1;
                }
            }
            return -1;
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
                var first = headerList[h];
                var last = headerList[^1];
                var timeDiff = last.BlockTime - first.BlockTime;
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
        }


        /// <inheritdoc/>
        public bool ProcessBlock(IBlock block)
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc/>
        public BlockProcessResult ProcessHeaders(BlockHeader[] headers)
        {
            // TODO: handle forks/reorgs
            lock (mainLock)
            {
                // Find index of the block that the first header in the array references
                int lstIndex = headerList.FindIndex(x =>
                                        ((ReadOnlySpan<byte>)headers[0].PreviousBlockHeaderHash).SequenceEqual(x.GetHash()));

                int arrIndex = 0;
                if (lstIndex < 0)
                {
                    // The previous block was not found => headers are either invalid or from far ahead so
                    // the client has to send another locator to find the first block the 2 peers share.
                    return BlockProcessResult.UnknownBlocks;
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
                            return BlockProcessResult.ForkBlocks;
                        }
                    }
                }

                if (arrIndex == headers.Length)
                {
                    return BlockProcessResult.Success;
                }
                else
                {
                    int count = 0;
                    for (int i = arrIndex; i < headers.Length; i++)
                    {
                        Consensus.BlockHeight = headerList.Count;
                        if (BlockVer.VerifyHeader(headers[i], GetNextTarget()))
                        {
                            headerList.Add(headers[i]);
                            count++;
                        }
                        else
                        {
                            AppendHeadrs(headers.AsSpan().Slice(arrIndex, count).ToArray(), count);
                            return BlockProcessResult.InvalidBlocks;
                        }
                    }

                    AppendHeadrs(headers.AsSpan()[arrIndex..].ToArray(), count);

                    return BlockProcessResult.Success;
                }
            }
        }

        private void AppendHeadrs(IEnumerable<BlockHeader> headers, int count)
        {
            var stream = new FastStream(count * Constants.BlockHeaderSize);
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
                long timeDiff = UnixTimeStamp.GetEpochUtcNow() - headerList[^1].BlockTime;
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
