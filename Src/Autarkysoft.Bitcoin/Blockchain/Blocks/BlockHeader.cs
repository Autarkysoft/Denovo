// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Linq;

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
        /// Block header size in bytes when serialized
        /// </summary>
        public const int Size = 80;

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


        private byte[] hash;

        /// <summary>
        /// Returns hash of this header using the defined hash function.
        /// <para/>Note that after the first call to this function, the hash result will be stored to avoid future repeated
        /// computation. If any of the properties change this function has to be called with <paramref name="recompute"/> = true
        /// to force re-computation of the hash.
        /// </summary>
        /// <param name="recompute">
        /// [Default alue = false] Indicates whether the hash should be recomputed or the cached value is still valid
        /// </param>
        /// <returns>32 byte block header hash</returns>
        public byte[] GetHash(bool recompute = false)
        {
            if (recompute || hash is null)
            {
                byte[] bytesToHash = Serialize();
                using Sha256 hashFunc = new Sha256();
                hash = hashFunc.ComputeHashTwice(bytesToHash);
            }

            return hash;
        }

        /// <summary>
        /// Returns hash of this block as a base-16 encoded string.
        /// </summary>
        /// <param name="recompute">
        /// [Default alue = false] Indicates whether the hash should be recomputed or the cached value is still valid
        /// </param>
        /// <returns>Base-16 encoded block hash</returns>
        public string GetID(bool recompute)
        {
            byte[] hashRes = GetHash(recompute);
            return Base16.Encode(hashRes.Reverse().ToArray());
        }


        /// <inheritdoc/>
        public void AddSerializedSize(SizeCounter counter) => counter.Add(Size);

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
