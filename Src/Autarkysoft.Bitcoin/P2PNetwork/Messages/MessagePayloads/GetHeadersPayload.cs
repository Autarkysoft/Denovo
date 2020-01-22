// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    // TODO: this is identical to GetHeadersPayload (except MaximumHashes)

    public class GetHeadersPayload : PayloadBase
    {
        private const int MaximumHashes = 2000;
        public int Version { get; set; }
        public byte[][] Hashes { get; set; }

        private byte[] _stopHash;
        public byte[] StopHash
        {
            get => _stopHash;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(StopHash), "Stop hash can not be null.");
                if (value.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(StopHash), "Stop hash must be 32 bytes.");

                _stopHash = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.GetHeaders;



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


            if (!CompactInt.TryRead(stream, out CompactInt hashCount, out error))
            {
                return false;
            }

            if (hashCount > MaximumHashes)
            {
                error = $"GetHeaders message can not contain more than {MaximumHashes} header hashes.";
                return false;
            }

            Hashes = new byte[(int)hashCount][];
            for (int i = 0; i < (int)hashCount; i++)
            {
                if (!stream.TryReadByteArray(32, out Hashes[i]))
                {
                    return false;
                }
            }

            if (!stream.TryReadByteArray(32, out _stopHash))
            {
                return false;
            }

            error = null;
            return true;
        }

    }
}
