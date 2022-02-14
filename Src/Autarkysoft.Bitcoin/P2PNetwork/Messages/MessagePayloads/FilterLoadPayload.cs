// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing the filter that this node wants to set.
    /// <para/> Sent: unsolicited
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bips/blob/master/bip-0037.mediawiki
    /// </remarks>
    public class FilterLoadPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="FilterLoadPayload"/> used for deserialization.
        /// </summary>
        public FilterLoadPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FilterLoadPayload"/> with the given parameters.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="filter">Filter to use</param>
        /// <param name="hashCount">Number of hash functions to use</param>
        /// <param name="tweak">The tweak</param>
        /// <param name="flags">Bloom filter flags</param>
        public FilterLoadPayload(byte[] filter, uint hashCount, uint tweak, BloomFlags flags)
        {
            Filter = filter;
            HashFuncCount = hashCount;
            Tweak = tweak;
            Flags = flags;
        }



        /// <summary>
        /// Maximum length of a filter
        /// </summary>
        public const int MaxFilterLength = 36000;
        /// <summary>
        /// Maximum number of hash functions
        /// </summary>
        public const int MaxHashFuncs = 50;

        private byte[] _filter;
        /// <summary>
        /// The filter to use
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] Filter
        {
            get => _filter;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(Filter), "Filter can not be null or empty.");
                if (value.Length > MaxFilterLength)
                    throw new ArgumentOutOfRangeException(nameof(Filter), "Filter is too long.");

                _filter = value;
            }
        }

        private uint _hCount;
        /// <summary>
        /// The number of hash functions to use in this filter. The maximum value allowed in this field is 50.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public uint HashFuncCount
        {
            get => _hCount;
            set
            {
                if (value > MaxHashFuncs)
                    throw new ArgumentOutOfRangeException(nameof(HashFuncCount), "Number of hash functions exceeds maximum.");

                _hCount = value;
            }
        }

        private uint _tweak;
        /// <summary>
        /// An arbitrary value to add to the seed value in the hash function used by the bloom filter.
        /// </summary>
        public uint Tweak
        {
            get => _tweak;
            set => _tweak = value;
        }

        private BloomFlags _flags;
        /// <summary>
        /// A set of flags that control how outpoints corresponding to a matched pubkey script are added to the filter.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public BloomFlags Flags
        {
            get => _flags;
            set
            {
                if (!Enum.IsDefined(typeof(BloomFlags), value))
                    throw new ArgumentException("Flag is not defined.");

                _flags = value;
            }
        }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.FilterLoad;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            counter.AddCompactIntCount(Filter.Length);
            counter.Add(Filter.Length + 4 + 4 + 1);
        }

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt filterLen = new CompactInt(Filter.Length);
            filterLen.WriteToStream(stream);
            stream.Write(Filter);
            stream.Write(HashFuncCount);
            stream.Write(Tweak);
            stream.Write((byte)Flags);
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt filterLen, out error))
            {
                return false;
            }

            // TODO: should we reject len=0?
            if (filterLen > MaxFilterLength)
            {
                error = Errors.MsgFilterLenOverflow;
                return false;
            }

            if (!stream.TryReadByteArray((int)filterLen, out _filter))
            {
                error = Errors.EndOfStream;
                return false;
            }

            if (!stream.CheckRemaining(4 + 4 + 1))
            {
                error = Errors.EndOfStream;
                return false;
            }

            _hCount = stream.ReadUInt32Checked();
            if (_hCount > MaxHashFuncs)
            {
                error = Errors.MsgFilterHashOverflow;
                return false;
            }

            Tweak = stream.ReadUInt32Checked();
            Flags = (BloomFlags)stream.ReadByteChecked();

            error = Errors.None;
            return true;
        }
    }
}
