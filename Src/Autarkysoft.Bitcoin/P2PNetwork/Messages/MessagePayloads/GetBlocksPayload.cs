// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing the request for .
    /// <para/> Sent: unsolicited or in response to <see cref="PayloadType.GetAddr"/>
    /// </summary>
    public class GetBlocksPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="GetBlocksPayload"/> used for deserialization.
        /// </summary>
        public GetBlocksPayload()
        {
        }



        private const int MaximumHashes = 500;

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
                    throw new ArgumentOutOfRangeException(nameof(Hashes), $"Only a maximum of {MaximumHashes} are allowed.");

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

            if (!CompactInt.TryRead(stream, out CompactInt len, out error))
            {
                return false;
            }
            if (len > MaximumHashes)
            {
                error = $"Only {MaximumHashes} hashes are accepted.";
                return false;
            }

            Hashes = new byte[(byte)len][];
            for (int i = 0; i < (int)len; i++)
            {
                stream.TryReadByteArray(32, out Hashes[i]);
            }

            stream.TryReadByteArray(32, out _stopHash);

            error = null;
            return true;
        }
    }
}
