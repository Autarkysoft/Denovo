// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing multiple block headers.
    /// <para/> Sent: in response to <see cref="GetHeadersPayload"/>
    /// </summary>
    public class HeadersPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="HeadersPayload"/> used for deserialization.
        /// </summary>
        public HeadersPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="HeadersPayload"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="headers">Headers to use (array must contain between 1 and <see cref="MaxCount"/> items)</param>
        public HeadersPayload(BlockHeader[] headers)
        {
            Headers = headers;
        }

        /// <summary>
        /// Maximum number of allowed <see cref="BlockHeader"/>s in this payload type
        /// </summary>
        /// <remarks>
        /// https://github.com/bitcoin/bitcoin/blob/3f512f3d563954547061ee743648b57a900cbe04/src/net_processing.cpp#L97-L99
        /// </remarks>
        public const int MaxCount = 2000;

        private BlockHeader[] _hds;
        /// <summary>
        /// An array of block headers (must contain between 1 and <see cref="MaxCount"/> items)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public BlockHeader[] Headers
        {
            get => _hds;
            set
            {
                if (value is null || value.Length == 0)
                    throw new ArgumentNullException(nameof(Headers), "Array can not be null or empty.");
                if (value.Length > MaxCount)
                    throw new ArgumentOutOfRangeException(nameof(Headers), $"Headers count must be smaller than {MaxCount}");

                _hds = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Headers;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            counter.AddCompactIntCount(Headers.Length);
            counter.Add((Headers.Length * BlockHeader.Size) + Headers.Length);
        }

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt count = new CompactInt(Headers.Length);

            count.WriteToStream(stream);
            foreach (var hd in Headers)
            {
                hd.Serialize(stream);
                // Block serialization of header doesn't add tx count since there is no need for it.
                // However, in a header payload (for unknown reason) one extra byte indicating zero tx count
                // is added to each block header.
                stream.Write((byte)0);
            }
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            // The following method is correct since max is 2000
            if (!stream.TryReadSmallCompactInt(out int count))
            {
                error = Errors.InvalidCompactInt;
                return false;
            }

            if (count > MaxCount)
            {
                error = Errors.MsgHeaderCountOverflow;
                return false;
            }

            _hds = new BlockHeader[count];
            for (int i = 0; i < _hds.Length; i++)
            {
                if (!BlockHeader.TryDeserialize(stream, out BlockHeader temp, out error))
                {
                    return false;
                }
                _hds[i] = temp;

                // Headers messages contain a tx count that is not used anywhere specially since it is always set to 0!
                // https://github.com/bitcoin/bitcoin/blob/3f512f3d563954547061ee743648b57a900cbe04/src/net_processing.cpp#L3455
                if (!CompactInt.TryRead(stream, out _, out error))
                {
                    return false;
                }
            }

            error = Errors.None;
            return true;
        }
    }
}
