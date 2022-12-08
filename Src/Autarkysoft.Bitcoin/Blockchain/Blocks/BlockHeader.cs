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
    /// Immutable objects that contain the 80-byte block header structure and the block hash.
    /// </summary>
    public readonly struct BlockHeader
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BlockHeader"/> using given parameters and sets the 
        /// <see cref="BlockTime"/> to current UTC timestamp.
        /// </summary>
        /// <exception cref="NullReferenceException"/>
        /// <param name="consensus">Consensus rules</param>
        /// <param name="tip">The last valid block in the chain</param>
        /// <param name="merkle">Merkle root hash</param>
        /// <param name="nbits">Block target</param>
        public BlockHeader(IConsensus consensus, BlockHeader tip, in Digest256 merkle, Target nbits)
            : this(consensus.MinBlockVersion, tip.Hash, merkle, (uint)UnixTimeStamp.GetEpochUtcNow(), nbits, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlockHeader"/> using given parameters.
        /// </summary>
        /// <param name="ver">Block version</param>
        /// <param name="prevHd">Hash of previous block header</param>
        /// <param name="merkle">Merkle root hash</param>
        /// <param name="blockTime">Block time</param>
        /// <param name="nbits">Block target</param>
        /// <param name="nonce">Block nonce</param>
        public BlockHeader(int ver, in Digest256 prevHd, in Digest256 merkle, uint blockTime, Target nbits, uint nonce)
        {
            Version = ver;
            PreviousBlockHeaderHash = prevHd;
            MerkleRootHash = merkle;
            BlockTime = blockTime;
            NBits = nbits;
            Nonce = nonce;

            unsafe
            {
                using Sha256 hashFunc = new Sha256();
                fixed (uint* hPt = &hashFunc.hashState[0], wPt = &hashFunc.w[0])
                {
                    hashFunc.Init(hPt);
                    // 4 byte version
                    wPt[0] = (uint)((Version >> 24) | (Version << 24) | ((Version >> 8) & 0xff00) | ((Version << 8) & 0xff0000));
                    // 32 byte previous block header hash
                    wPt[1] = PreviousBlockHeaderHash.b0.SwapEndian();
                    wPt[2] = PreviousBlockHeaderHash.b1.SwapEndian();
                    wPt[3] = PreviousBlockHeaderHash.b2.SwapEndian();
                    wPt[4] = PreviousBlockHeaderHash.b3.SwapEndian();
                    wPt[5] = PreviousBlockHeaderHash.b4.SwapEndian();
                    wPt[6] = PreviousBlockHeaderHash.b5.SwapEndian();
                    wPt[7] = PreviousBlockHeaderHash.b6.SwapEndian();
                    wPt[8] = PreviousBlockHeaderHash.b7.SwapEndian();
                    // 28 (of 32) byte MerkleRoot hash
                    wPt[9] = MerkleRootHash.b0.SwapEndian();
                    wPt[10] = MerkleRootHash.b1.SwapEndian();
                    wPt[11] = MerkleRootHash.b2.SwapEndian();
                    wPt[12] = MerkleRootHash.b3.SwapEndian();
                    wPt[13] = MerkleRootHash.b4.SwapEndian();
                    wPt[14] = MerkleRootHash.b5.SwapEndian();
                    wPt[15] = MerkleRootHash.b6.SwapEndian();
                    hashFunc.CompressBlock(hPt, wPt);

                    // 4 (of 32) byte MerkleRoot hash
                    wPt[0] = MerkleRootHash.b7.SwapEndian();
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
                    Hash = new Digest256(hPt);
                }
            }
        }



        private static readonly BlockHeader NULL = new BlockHeader();

        /// <summary>
        /// Block header size in bytes when serialized
        /// </summary>
        public const int Size = 80;

        /// <summary>
        /// Block version
        /// </summary>
        public readonly int Version;
        /// <summary>
        /// Hash of the previous block header
        /// </summary>
        public readonly Digest256 PreviousBlockHeaderHash;
        /// <summary>
        /// The merkle root hash
        /// </summary>
        public readonly Digest256 MerkleRootHash;
        /// <summary>
        /// Block time
        /// </summary>
        public readonly uint BlockTime;
        /// <summary>
        /// Target of this block, used for defining difficulty
        /// </summary>
        public readonly Target NBits;
        /// <summary>
        /// Nonce (a random 32-bit integer used in mining)
        /// </summary>
        public readonly uint Nonce;
        /// <summary>
        /// Block header hash
        /// </summary>
        public readonly Digest256 Hash;


        /// <summary>
        /// Returns hash of this block as a base-16 encoded string.
        /// </summary>
        /// <returns>Base-16 encoded block hash</returns>
        public string GetID() => Base16.EncodeReverse(Hash.ToByteArray());

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
        /// Converts this block header into its byte array representation.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] Serialize()
        {
            FastStream stream = new FastStream(Size);
            Serialize(stream);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Deserializes the given byte array from the given stream. Return value indicates success.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="result">Block header result</param>
        /// <param name="error">Error message</param>
        /// <returns>True if deserialization was successful; otherwise false.</returns>
        public static bool TryDeserialize(FastStreamReader stream, out BlockHeader result, out Errors error)
        {
            if (!stream.CheckRemaining(Size))
            {
                error = Errors.EndOfStream;
                result = NULL;
                return false;
            }

            int v = stream.ReadInt32Checked();
            Digest256 prvH = stream.ReadDigest256Checked();
            Digest256 mrkl = stream.ReadDigest256Checked();
            uint t = stream.ReadUInt32Checked();

            if (!Target.TryRead(stream, out Target nb, out error))
            {
                result = NULL;
                return false;
            }

            uint nonce = stream.ReadUInt32Checked();

            result = new BlockHeader(v, prvH, mrkl, t, nb, nonce);
            error = Errors.None;
            return true;
        }
    }
}
