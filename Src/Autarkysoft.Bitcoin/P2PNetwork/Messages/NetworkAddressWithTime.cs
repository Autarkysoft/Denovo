// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// Same as <see cref="NetworkAddress"/> but with a timestamp.
    /// </summary>
    public class NetworkAddressWithTime : NetworkAddress
    {
        internal new const int Size = NetworkAddress.Size + 4;

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
        public void SetTimeToNow()
        {
            Time = (uint)UnixTimeStamp.GetEpochUtcNow();
        }

        /// <summary>
        /// Returns <see cref="DateTime"/> representation of <see cref="Time"/>.
        /// </summary>
        /// <returns>DateTime</returns>
        public DateTime GetDateTime()
        {
            return UnixTimeStamp.EpochToTime(Time);
        }


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
