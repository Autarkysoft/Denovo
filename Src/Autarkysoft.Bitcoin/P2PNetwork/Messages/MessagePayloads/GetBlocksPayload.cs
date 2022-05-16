// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Linq;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing the request for block header hashes.
    /// <para/> Sent: unsolicited
    /// </summary>
    public class GetBlocksPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="GetBlocksPayload"/> used for deserialization.
        /// </summary>
        public GetBlocksPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GetBlocksPayload"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ver">Protocol version</param>
        /// <param name="headerHashes">List of header hashes</param>
        /// <param name="stopHash">Stop hash</param>
        public GetBlocksPayload(int ver, Digest256[] headerHashes, Digest256 stopHash)
        {
            Version = ver;
            Hashes = headerHashes;
            StopHash = stopHash;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="GetBlocksPayload"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ver">Protocol version</param>
        /// <param name="headers">List of headers (hash of each header will be used)</param>
        /// <param name="stopHash">Stop hash (can be null)</param>
        public GetBlocksPayload(int ver, BlockHeader[] headers, BlockHeader? stopHash)
            : this(ver, headers.Select(hd => hd.Hash).ToArray(), stopHash.HasValue ? stopHash.Value.Hash : Digest256.Zero)
        {
        }


        /// <summary>
        /// Maximum number of hashes allowed in the hash list
        /// </summary>
        /// <remarks>
        /// https://github.com/bitcoin/bitcoin/blob/81d5af42f4dba5b68a597536cad7f61894dc22a3/src/net_processing.cpp#L71-L72
        /// </remarks>
        public const int MaximumHashes = 101;

        private int _ver;
        /// <summary>
        /// The protocol version
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public int Version
        {
            get => _ver;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Version), "Version can not be negative.");

                _ver = value;
            }
        }

        private Digest256[] _hashes;
        /// <summary>
        /// One or more block header hashes (with heighest height first)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public Digest256[] Hashes
        {
            get => _hashes;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Hashes), "Hash list can not be null.");
                if (value.Length > MaximumHashes)
                    throw new ArgumentOutOfRangeException(nameof(Hashes), $"Only a maximum of {MaximumHashes} hashes are allowed.");

                _hashes = value;
            }
        }

        /// <summary>
        /// The header hash of the last header hash being requested
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public Digest256 StopHash { get; set; }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.GetBlocks;

        // TODO: add a method here to take IBlockchain (the database manager) and set header hashes itself using that


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            counter.AddCompactIntCount(Hashes.Length);
            counter.Add(4 + (Hashes.Length * 32) + 32);
        }

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt hashCount = new CompactInt(Hashes.Length);

            stream.Write(Version);
            hashCount.WriteToStream(stream);
            foreach (var item in Hashes)
            {
                stream.Write(item);
            }
            stream.Write(StopHash);
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            if (!stream.TryReadInt32(out _ver))
            {
                error = Errors.EndOfStream;
                return false;
            }
            // TODO: should we be flexible here and accept negative version and reject it in ReplyManager?
            if (_ver < 0)
            {
                error = Errors.InvalidBlocksPayloadVersion;
                return false;
            }

            if (!stream.TryReadSmallCompactInt(out int count))
            {
                error = Errors.InvalidCompactInt;
                return false;
            }
            if (count > MaximumHashes)
            {
                error = Errors.MsgBlocksHashCountOverflow;
                return false;
            }
            if (!stream.CheckRemaining((count * 32) + 32))
            {
                error = Errors.EndOfStream;
                return false;
            }

            _hashes = new Digest256[count];
            for (int i = 0; i < _hashes.Length; i++)
            {
                _hashes[i] = stream.ReadDigest256Checked();
            }

            StopHash = stream.ReadDigest256Checked();

            error = Errors.None;
            return true;
        }
    }
}
