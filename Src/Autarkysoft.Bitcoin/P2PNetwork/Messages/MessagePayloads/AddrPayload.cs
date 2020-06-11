// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing the connection information for one or more bitcoin nodes.
    /// <para/> Sent: unsolicited or in response to <see cref="PayloadType.GetAddr"/>
    /// </summary>
    public class AddrPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="AddrPayload"/> used for deserialization.
        /// </summary>
        public AddrPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AddrPayload"/> with the given network address list.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="netAddrList">List of network addresses</param>
        public AddrPayload(NetworkAddressWithTime[] netAddrList)
        {
            Addresses = netAddrList;
        }


        /// <summary>
        /// Maximum length of the <see cref="Addresses"/> list
        /// </summary>
        private const int MaxAddrCount = 1000;

        private NetworkAddressWithTime[] _addrs;
        /// <summary>
        /// List of network addresses (node information)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public NetworkAddressWithTime[] Addresses
        {
            get => _addrs;
            set
            {
                if (value is null || value.Length == 0)
                    throw new ArgumentNullException(nameof(Addresses), "NetworkAddress list can not be null or empty.");
                if (value.Length > MaxAddrCount)
                    throw new ArgumentOutOfRangeException(nameof(Addresses), "NetworkAddress list has too many items.");

                _addrs = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Addr;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt count = new CompactInt(Addresses.Length);
            count.WriteToStream(stream);
            foreach (var item in Addresses)
            {
                item.Serialize(stream);
            }
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt count, out error))
            {
                return false;
            }

            if (count > MaxAddrCount)
            {
                error = $"AddressCount can not be bigger than {MaxAddrCount}.";
                return false;
            }

            int c = (int)count;
            if (!stream.CheckRemaining(c * (8 + 16 + 2 + 4)))
            {
                error = Err.EndOfStream;
                return false;
            }

            Addresses = new NetworkAddressWithTime[c];
            for (int i = 0; i < Addresses.Length; i++)
            {
                var temp = new NetworkAddressWithTime();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                Addresses[i] = temp;
            }

            error = null;
            return true;
        }
    }
}
