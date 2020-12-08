// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;

namespace Autarkysoft.Bitcoin.Blockchain
{
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
        /// <param name="consensus">Consensus rules</param>
        public Blockchain(IFileManager fileMan, IConsensus consensus)
        {
            FileMan = fileMan ?? throw new ArgumentNullException(nameof(fileMan));
            Consensus = consensus ?? throw new ArgumentNullException(nameof(consensus));

            // TODO: find a better initial capacity
            headers = new List<BlockHeader>(700_000);
            ReadHeaders();

            // TODO: read blocks and others
        }


        private const string HeadersFile = "Headers";
        private readonly List<BlockHeader> headers;

        /// <summary>
        /// File manager responsible for handling data
        /// </summary>
        public IFileManager FileMan { get; set; }
        /// <summary>
        /// Consensus rules instance
        /// </summary>
        public IConsensus Consensus { get; set; }


        /// <inheritdoc/>
        public int Height => 0;

        private void ReadHeaders()
        {
            byte[] hadba = FileMan.ReadData(HeadersFile);
            if (hadba is null || hadba.Length % Constants.BlockHeaderSize != 0)
            {
                // File doesn't exist or is corrupted
                BlockHeader genesis = Consensus.GetGenesisBlock().Header;
                headers.Add(genesis);
                FileMan.WriteData(genesis.Serialize(), HeadersFile);
            }
            else
            {
                var result = new BlockHeader[hadba.Length / Constants.BlockHeaderSize];
                var stream = new FastStreamReader(hadba);
                for (int i = 0; i < result.Length; i++)
                {
                    var temp = new BlockHeader();
                    if (temp.TryDeserialize(stream, out _))
                    {
                        result[i] = temp;
                    }
                    else
                    {
                        // File is corrupted, has to be instantiated
                        headers.Clear();
                        BlockHeader genesis = Consensus.GetGenesisBlock().Header;
                        headers.Add(genesis);
                        FileMan.WriteData(genesis.Serialize(), HeadersFile);
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public int FindHeight(ReadOnlySpan<byte> prevHash)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                if (prevHash.SequenceEqual(headers[i].GetHash()))
                {
                    return i + 1;
                }
            }
            return -1;
        }

        /// <inheritdoc/>
        public Target GetTarget(int height)
        {
            if (height == Height + 1)
            {
                // Next target
            }
            else
            {

            }
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool ProcessBlock(IBlock block)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ProcessHeaders(BlockHeader[] headers)
        {


            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public BlockHeader[] GetBlockHeaderLocator()
        {
            var result = new List<BlockHeader>(32);
            int step = 1;
            int index = headers.Count - 1;
            // Add first 10 headers from the tip in reverse order then increase the steps exponetially
            // until we run out of headers
            while (index >= 0)
            {
                result.Add(headers[index]);
                if (result.Count > 10)
                {
                    step *= 2;
                }
                index -= step;
            }

            // We want the last item to always be genesis block
            if (!ReferenceEquals(result[^1], headers[^1]))
            {
                result.Add(headers[^1]);
            }

            return result.ToArray();
        }

        /// <inheritdoc/>
        public BlockHeader[] GetMissingHeaders(byte[][] hashesToCompare, byte[] stopHash)
        {
            int index = -1;
            if (hashesToCompare.Length == 0)
            {
                index = headers.FindIndex(x => ((ReadOnlySpan<byte>)stopHash).SequenceEqual(x.GetHash()));
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
                    index = headers.FindIndex(x => ((ReadOnlySpan<byte>)item).SequenceEqual(x.GetHash()));
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
            while (result.Count < HeadersPayload.MaxCount && index < headers.Count)
            {
                BlockHeader toAdd = headers[index++];
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
