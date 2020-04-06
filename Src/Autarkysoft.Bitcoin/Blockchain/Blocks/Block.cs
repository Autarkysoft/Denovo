// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    // TODO: block validation reminder:
    //       a block with valid POW and valid merkle root can have its transactions modified (duplicate some txs)
    //       by a malicious node along the way to make it invalid. the node sometimes has to remember invalid block hashes
    //       that it received to avoid receiving the same thing again. if the reason for being invalid is only merkle root 
    //       and having those duplicate cases then the hash must not be stored or some workaround must be implemented.
    //       more info:
    // https://github.com/bitcoin/bitcoin/blob/1dbf3350c683f93d7fc9b861400724f6fd2b2f1d/src/consensus/merkle.cpp#L8-L42

    /// <summary>
    /// The main component of the blockchain that contains transactions and shapes up the chain by referencing
    /// the previous block and is secured using cryptography in form of the proof of work algorithm.
    /// Implements <see cref="IBlock"/>.
    /// </summary>
    public class Block : IBlock
    {
        /// <summary>
        /// Initializes a new empty instance of <see cref="Block"/>.
        /// </summary>
        public Block()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Block"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ver">Block version</param>
        /// <param name="prevHd">Hash of previous block header</param>
        /// <param name="merkle">Hash of merkle root</param>
        /// <param name="blockTime">Block time</param>
        /// <param name="nbits">Block target</param>
        /// <param name="nonce">Block nonce</param>
        /// <param name="txs">List of transactions</param>
        public Block(int ver, byte[] prevHd, byte[] merkle, uint blockTime, Target nbits, uint nonce, ITransaction[] txs)
        {
            Version = ver;
            PreviousBlockHeaderHash = prevHd;
            MerkleRootHash = merkle;
            BlockTime = blockTime;
            NBits = nbits;
            Nonce = nonce;
            TransactionList = txs;
        }



        private const int HeaderSize = 80;
        private readonly Sha256 hashFunc = new Sha256(true);

        /// <inheritdoc/>
        public int Height { get; set; } = -1;

        private int _version;
        /// <inheritdoc/>
        public int Version
        {
            get => _version;
            set => _version = value;
        }

        private byte[] _prvBlkHash = new byte[32];
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] PreviousBlockHeaderHash
        {
            get => _prvBlkHash;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(PreviousBlockHeaderHash), "Previous block Header hash can not be null.");
                if (value.Length != 32)
                {
                    throw new ArgumentOutOfRangeException(nameof(PreviousBlockHeaderHash),
                        "Previous block Header hash length is invalid.");
                }

                _prvBlkHash = value;
            }
        }

        private byte[] _merkle = new byte[32];
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] MerkleRootHash
        {
            get => _merkle;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(MerkleRootHash), "Merkle root hash can not be null.");
                if (value.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(MerkleRootHash), "Merkle root hash length is invalid.");

                _merkle = value;
            }
        }

        private uint _time;
        /// <inheritdoc/>
        public uint BlockTime
        {
            get => _time;
            set => _time = value;
        }

        private Target _nBits;
        /// <inheritdoc/>
        public Target NBits
        {
            get => _nBits;
            set => _nBits = value;
        }

        private uint _nonce;
        /// <inheritdoc/>
        public uint Nonce
        {
            get => _nonce;
            set => _nonce = value;
        }

        private ITransaction[] _txs = new ITransaction[0];
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public ITransaction[] TransactionList
        {
            get => _txs;
            set
            {
                if (value is null || value.Length == 0)
                    throw new ArgumentNullException(nameof(TransactionList), "Transaction list can not be null or empty.");

                _txs = value;
            }
        }



        /// <inheritdoc/>
        public byte[] GetBlockHash()
        {
            byte[] bytesToHash = SerializeHeader();
            return hashFunc.ComputeHash(bytesToHash);
        }

        /// <inheritdoc/>
        public string GetBlockID()
        {
            byte[] hashRes = GetBlockHash();
            Array.Reverse(hashRes);
            return Base16.Encode(hashRes);
        }


        /// <inheritdoc/>
        public unsafe byte[] ComputeMerkleRoot()
        {
            if (TransactionList.Length == 1)
            {
                // Only has Coinbase transaction
                return TransactionList[0].GetTransactionHash();
            }
            else
            {
                // To compute merkle root all transaction hashes are placed inside a big buffer, buffer is padded
                // by repeating the last hash if needed to be divisible into groups of 2.
                // Then hash of each group is computed and copied into the same buffer from the start.
                // The process ends when there is only 1 hash remaining.
                using Sha256 sha = new Sha256();
                bool needDup = TransactionList.Length % 2 != 0;
                byte[] buffer = new byte[(needDup ? TransactionList.Length + 1 : TransactionList.Length) * 32];
                int hashCount = buffer.Length / 32;
                int offset = 0;
                foreach (var tx in TransactionList)
                {
                    Buffer.BlockCopy(tx.GetTransactionHash(), 0, buffer, offset, 32);
                    offset += 32;
                }
                if (needDup)
                {
                    Buffer.BlockCopy(TransactionList[^1].GetTransactionHash(), 0, buffer, offset, 32);
                }

                fixed (uint* hPt = &sha.hashState[0], wPt = &sha.w[0])
                fixed (byte* bufPt = &buffer[0])
                {
                    while (hashCount > 1)
                    {
                        byte* src = bufPt;
                        byte* dst = bufPt;
                        for (int i = 0; i < hashCount / 2; i++)
                        {
                            // Since each "data" being hashed is fixed 64 byte, an optimized SHA256 method is called
                            // to compute SHA256(SHA256(64-byte-data))
                            sha.Compress64Double(src, dst, hPt, wPt);
                            src += 64;
                            dst += 32;
                        }
                        hashCount /= 2;
                        if (hashCount == 1)
                        {
                            break;
                        }
                        needDup = hashCount % 2 != 0;
                        if (needDup)
                        {
                            Buffer.MemoryCopy(dst - 32, dst, buffer.Length, 32);
                            hashCount++;
                        }
                    }
                }

                byte[] result = new byte[32];
                Buffer.BlockCopy(buffer, 0, result, 0, 32);
                return result;
            }
        }


        /// <inheritdoc/>
        public void SerializeHeader(FastStream stream)
        {
            stream.Write(Version);
            stream.Write(PreviousBlockHeaderHash);
            stream.Write(MerkleRootHash);
            stream.Write(BlockTime);
            NBits.WriteToStream(stream);
            stream.Write(Nonce);
        }

        /// <summary>
        /// Converts this block's header into its byte array representation.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] SerializeHeader()
        {
            FastStream stream = new FastStream(HeaderSize);
            SerializeHeader(stream);
            return stream.ToByteArray();
        }


        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            SerializeHeader(stream);

            CompactInt txCount = new CompactInt(TransactionList.Length);
            txCount.WriteToStream(stream);
            foreach (var tx in TransactionList)
            {
                tx.Serialize(stream);
            }
        }

        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] Serialize()
        {
            FastStream stream = new FastStream();
            Serialize(stream);
            return stream.ToByteArray();
        }


        /// <inheritdoc/>
        public bool TryDeserializeHeader(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadInt32(out _version))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!stream.TryReadByteArray(32, out _prvBlkHash))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!stream.TryReadByteArray(32, out _merkle))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!stream.TryReadUInt32(out _time))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!Target.TryRead(stream, out _nBits, out error))
            {
                return false;
            }

            if (!stream.TryReadUInt32(out _nonce))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            // TODO: add block size check

            if (!TryDeserializeHeader(stream, out error))
            {
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt txCount, out error))
            {
                return false;
            }
            if (txCount > int.MaxValue) // TODO: set how many ~tx can be in a block instead of int.Max
            {
                error = "Number of transactions is too big.";
                return false;
            }

            TransactionList = new Transaction[(int)txCount];
            for (int i = 0; i < TransactionList.Length; i++)
            {
                Transaction temp = new Transaction();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                TransactionList[i] = temp;
            }

            ReadOnlySpan<byte> actualMerkle = ComputeMerkleRoot();
            if (!actualMerkle.SequenceEqual(MerkleRootHash))
            {
                error = "Invalid merkle root.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
