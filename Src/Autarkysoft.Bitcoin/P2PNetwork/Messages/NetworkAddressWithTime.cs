// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Net;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// Same as <see cref="NetworkAddress"/> but with a timestamp.
    /// </summary>
    public class NetworkAddressWithTime : NetworkAddress
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="NetworkAddress"/>.
        /// </summary>
        public NetworkAddressWithTime() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NetworkAddress"/> using the given parameters.
        /// </summary>
        /// <param name="servs">Services that this node supports</param>
        /// <param name="ip">IP address of this node</param>
        /// <param name="port">Port (use <see cref="Constants"/> for default values)</param>
        /// <param name="time">
        /// Unix timestamp (Now if advertising own address, otherwise the same time received from the other node)
        /// </param>
        public NetworkAddressWithTime(NodeServiceFlags servs, IPAddress ip, ushort port, uint time) : base(servs, ip, port)
        {
            Time = time;
        }


        private uint _time;
        /// <summary>
        /// Unix epoch time. Added in protocol version 31402
        /// <para/> * If advertising own IP address: Current time.
        /// <para/> * If advertising other IP address: Last time connected to that node.
        /// <para/> * Out of range values won't be rejected in this class.
        /// </summary>
        public uint Time
        {
            get => _time;
            set => _time = value;
        }


        /// <summary>
        /// Sets the time value to current epoch time UTC.
        /// </summary>
        public void SetTimeToNow() => Time = (uint)UnixTimeStamp.GetEpochUtcNow();

        /// <summary>
        /// Returns <see cref="DateTime"/> representation of <see cref="Time"/>.
        /// </summary>
        /// <returns>DateTime</returns>
        public DateTime GetDateTime() => UnixTimeStamp.EpochToTime(Time);


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            stream.Write(Time);
            base.Serialize(stream);
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadUInt32(out _time))
            {
                error = Err.EndOfStream;
                return false;
            }

            return base.TryDeserialize(stream, out error);
        }
    }
}
