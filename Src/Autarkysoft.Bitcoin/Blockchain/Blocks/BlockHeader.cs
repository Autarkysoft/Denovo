// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    /// <summary>
    /// Block header is the 80-byte structure that each block starts with. It is hashed in proof of work.
    /// <para/>Implements <see cref="IDeserializable"/>
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
        /// <param name="merkle">Merkle root hash</param>
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
        /// Returns hash of this header (double SHA256).
        /// <para/>Note that after the first call to this function, the hash result will be stored to avoid future repeated
        /// computation. If any of the properties change, this function has to be called with <paramref name="recompute"/> = true
        /// to force re-computation of the hash.
        /// </summary>
        /// <param name="recompute">
        /// [Default value = false] Indicates whether the hash should be recomputed or the cached value is still valid
        /// </param>
        /// <returns>32 byte block header hash</returns>
        public unsafe byte[] GetHash(bool recompute = false)
        {
            if (recompute || hash is null)
            {
                using Sha256 hashFunc = new Sha256();
                fixed (byte* prvBlkH = &PreviousBlockHeaderHash[0], mrkl = &MerkleRootHash[0])
                fixed (uint* hPt = &hashFunc.hashState[0], wPt = &hashFunc.w[0])
                {
                    hashFunc.Init(hPt);
                    // 4 byte version
                    wPt[0] = (uint)((Version >> 24) | (Version << 24) | ((Version >> 8) & 0xff00) | ((Version << 8) & 0xff0000));
                    // 32 byte previous block header hash
                    wPt[1] = (uint)(prvBlkH[0] << 24 | prvBlkH[1] << 16 | prvBlkH[2] << 8 | prvBlkH[3]);
                    wPt[2] = (uint)(prvBlkH[4] << 24 | prvBlkH[5] << 16 | prvBlkH[6] << 8 | prvBlkH[7]);
                    wPt[3] = (uint)(prvBlkH[8] << 24 | prvBlkH[9] << 16 | prvBlkH[10] << 8 | prvBlkH[11]);
                    wPt[4] = (uint)(prvBlkH[12] << 24 | prvBlkH[13] << 16 | prvBlkH[14] << 8 | prvBlkH[15]);
                    wPt[5] = (uint)(prvBlkH[16] << 24 | prvBlkH[17] << 16 | prvBlkH[18] << 8 | prvBlkH[19]);
                    wPt[6] = (uint)(prvBlkH[20] << 24 | prvBlkH[21] << 16 | prvBlkH[22] << 8 | prvBlkH[23]);
                    wPt[7] = (uint)(prvBlkH[24] << 24 | prvBlkH[25] << 16 | prvBlkH[26] << 8 | prvBlkH[27]);
                    wPt[8] = (uint)(prvBlkH[28] << 24 | prvBlkH[29] << 16 | prvBlkH[30] << 8 | prvBlkH[31]);
                    // 28 (of 32) byte MerkleRoot hash
                    wPt[9] = (uint)(mrkl[0] << 24 | mrkl[1] << 16 | mrkl[2] << 8 | mrkl[3]);
                    wPt[10] = (uint)(mrkl[4] << 24 | mrkl[5] << 16 | mrkl[6] << 8 | mrkl[7]);
                    wPt[11] = (uint)(mrkl[8] << 24 | mrkl[9] << 16 | mrkl[10] << 8 | mrkl[11]);
                    wPt[12] = (uint)(mrkl[12] << 24 | mrkl[13] << 16 | mrkl[14] << 8 | mrkl[15]);
                    wPt[13] = (uint)(mrkl[16] << 24 | mrkl[17] << 16 | mrkl[18] << 8 | mrkl[19]);
                    wPt[14] = (uint)(mrkl[20] << 24 | mrkl[21] << 16 | mrkl[22] << 8 | mrkl[23]);
                    wPt[15] = (uint)(mrkl[24] << 24 | mrkl[25] << 16 | mrkl[26] << 8 | mrkl[27]);
                    hashFunc.CompressBlock(hPt, wPt);

                    // 4 (of 32) byte MerkleRoot hash
                    wPt[0] = (uint)(mrkl[28] << 24 | mrkl[29] << 16 | mrkl[30] << 8 | mrkl[31]);
                    wPt[1] = (BlockTime >> 24) | (BlockTime << 24) | ((BlockTime >> 8) & 0xff00) | ((BlockTime << 8) & 0xff0000);
                    wPt[2] = ((uint)NBits).SwapEndian();
                    wPt[3] = (Nonce >> 24) | (Nonce << 24) | ((Nonce >> 8) & 0xff00) | ((Nonce << 8) & 0xff0000);
                    wPt[4] = 0b10000000_00000000_00000000_00000000U;
                    wPt[5] = 0;
                    wPt[6] = 0;
                    wPt[7] = 0;
                    wPt[8] = 0;
                    wPt[9] = 0;
                    wPt[10] = 0;
                    wPt[11] = 0;
                    wPt[12] = 0;
                    wPt[13] = 0;
                    wPt[14] = 0;
                    wPt[15] = 640;
                    hashFunc.CompressBlock(hPt, wPt);

                    hashFunc.ComputeSecondHash(hPt, wPt);
                    hash = hashFunc.GetBytes(hPt);
                }
            }

            return hash;
        }

        /// <summary>
        /// Returns hash of this block as a base-16 encoded string.
        /// </summary>
        /// <param name="recompute">
        /// [Default value = false] Indicates whether the hash should be recomputed or the cached value is still valid
        /// </param>
        /// <returns>Base-16 encoded block hash</returns>
        public string GetID(bool recompute = false)
        {
            byte[] hashRes = GetHash(recompute);
            return Base16.EncodeReverse(hashRes);
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
            FastStream stream = new FastStream(Size);
            Serialize(stream);
            return stream.ToByteArray();
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (!stream.CheckRemaining(Size))
            {
                error = Errors.EndOfStream;
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

            error = Errors.None;
            return true;
        }
    }
}
