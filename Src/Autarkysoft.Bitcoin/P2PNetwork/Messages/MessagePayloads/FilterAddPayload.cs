// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing a single element to be added to the existing bloom filter.
    /// <para/> Sent: unsolicited
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bips/blob/master/bip-0037.mediawiki
    /// </remarks>
    public class FilterAddPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="FilterAddPayload"/> used for deserialization.
        /// </summary>
        public FilterAddPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FilterAddPayload"/> with the given element.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="element">The element to add</param>
        public FilterAddPayload(byte[] element)
        {
            Element = element;
        }



        private const int MaxElementLength = 520;

        private byte[] _element;
        /// <summary>
        /// List of network addresses (node information)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] Element
        {
            get => _element;
            set
            {
                if (value is null || value.Length == 0)
                    throw new ArgumentNullException(nameof(Element), "Element can not be null or empty.");
                if (value.Length > MaxElementLength)
                    throw new ArgumentOutOfRangeException(nameof(Element), "Element is too long.");

                _element = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.FilterAdd;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            counter.AddCompactIntCount(Element.Length);
            counter.Add(Element.Length);
        }

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt len = new CompactInt(Element.Length);

            len.WriteToStream(stream);
            stream.Write(Element);
        }


        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt len, out Errors err))
            {
                error = err.Convert();
                return false;
            }

            if (len > MaxElementLength)
            {
                error = "Invalid element length.";
                return false;
            }

            if (!stream.TryReadByteArray((int)len, out _element))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
