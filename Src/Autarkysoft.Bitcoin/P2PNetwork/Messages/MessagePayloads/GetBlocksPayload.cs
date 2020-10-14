// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

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
        public GetBlocksPayload(int ver, byte[][] headerHashes, byte[] stopHash)
        {
            Version = ver;
            Hashes = headerHashes;
            StopHash = stopHash;
        }


        /// <summary>
        /// Maximum number of hashes allowed in the hash list
        /// </summary>
        /// <remarks>
        /// https://github.com/bitcoin/bitcoin/blob/ec0453cd57736df33e9f50c004d88bea10428ad5/src/net_processing.cpp#L71-L72
        /// </remarks>
        protected virtual int MaximumHashes => 101;

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

        private byte[][] _hashes;
        /// <summary>
        /// One or more block header hashes (with heighest height first)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[][] Hashes
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

        private byte[] _stopHash;
        /// <summary>
        /// The header hash of the last header hash being requested
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] StopHash
        {
            get => _stopHash;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(StopHash), "Stop hash can not be null.");
                if (value.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(StopHash), "Stop hash length must be 32 bytes.");

                _stopHash = value;
            }
        }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.GetBlocks;

        // TODO: add a method here to take IBlockchain (the database manager) and set header hashes itself using that

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
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadInt32(out _ver))
            {
                error = Err.EndOfStream;
                return false;
            }
            if (_ver < 0)
            {
                error = "Invalid version";
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt count, out error))
            {
                return false;
            }
            if (count > MaximumHashes)
            {
                error = $"Only {MaximumHashes} hashes are accepted.";
                return false;
            }
            int iCount = (int)count;
            if (!stream.CheckRemaining((iCount * 32) + 32))
            {
                error = Err.EndOfStream;
                return false;
            }

            _hashes = new byte[iCount][];
            for (int i = 0; i < _hashes.Length; i++)
            {
                _hashes[i] = stream.ReadByteArray32Checked();
            }

            _stopHash = stream.ReadByteArray32Checked();

            error = null;
            return true;
        }
    }
}
