// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    /// <summary>
    /// Block header is the 80-byte structure that each block starts with.
    /// </summary>
    public class BlockHeader : IDeserializable
    {
        /// <summary>
        /// Initializes a new empty instance of <see cref="BlockHeader"/>.
        /// </summary>
        public BlockHeader()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlockHeader"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ver">Block version</param>
        /// <param name="prevHd">Hash of previous block header</param>
        /// <param name="merkle">Hash of merkle root</param>
        /// <param name="blockTime">Block time</param>
        /// <param name="nbits">Block target</param>
        /// <param name="nonce">Block nonce</param>
        public BlockHeader(int ver, byte[] prevHd, byte[] merkle, uint blockTime, Target nbits, uint nonce)
        {
            Version = ver;
            PreviousBlockHeaderHash = prevHd;
            MerkleRootHash = merkle;
            BlockTime = blockTime;
            NBits = nbits;
            Nonce = nonce;
        }


        /// <summary>
        /// Block version
        /// </summary>
        public int Version { get; set; }

        private byte[] _prvBlkHash = new byte[32];
        /// <summary>
        /// Hash of the previous block header
        /// </summary>
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
        /// <summary>
        /// The merkle root hash
        /// </summary>
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

        /// <summary>
        /// Block time
        /// </summary>
        public uint BlockTime { get; set; }

        private Target _nBits;
        /// <summary>
        /// Target of this block, used for defining difficulty
        /// </summary>
        public Target NBits
        {
            get => _nBits;
            set => _nBits = value;
        }

        /// <summary>
        /// Nonce (a random 32-bit integer used in mining)
        /// </summary>
        public uint Nonce { get; set; }

        /// <summary>
        /// Returns hash of this header using the defined hash function.
        /// </summary>
        /// <returns>32 byte block header hash</returns>
        public byte[] GetHash()
        {
            byte[] bytesToHash = Serialize();
            using Sha256 hashFunc = new Sha256(true);
            return hashFunc.ComputeHash(bytesToHash);
        }

        /// <inheritdoc/>
        public void Serialize(FastStream stream)
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
        public byte[] Serialize()
        {
            FastStream stream = new FastStream(Constants.BlockHeaderSize);
            Serialize(stream);
            return stream.ToByteArray();
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (!stream.CheckRemaining(Constants.BlockHeaderSize))
            {
                error = Err.EndOfStream;
                return false;
            }

            Version = stream.ReadInt32Checked();
            _prvBlkHash = stream.ReadByteArray32Checked();
            _merkle = stream.ReadByteArray32Checked();
            BlockTime = stream.ReadUInt32Checked();

            if (!Target.TryRead(stream, out _nBits, out error))
            {
                return false;
            }

            Nonce = stream.ReadUInt32Checked();

            error = null;
            return true;
        }
    }
}
