// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Net;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// Information about a node such as IP, port and service flags it has. 
    /// Implements <see cref="IDeserializable"/>.
    /// </summary>
    public class NetworkAddress : IDeserializable
    {
        private NodeServiceFlags _servs;
        /// <summary>
        /// The services that this node supports
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public NodeServiceFlags NodeServices
        {
            get => _servs;
            set
            {
                if ((value & NodeServiceFlags.All) != value)
                    throw new ArgumentException("Invalid services flags");

                _servs = value;
            }
        }

        private IPAddress _ip = IPAddress.Parse("127.0.0.1");
        /// <summary>
        /// [Default value = 127.0.0.1]
        /// Node's IP address
        /// </summary>
        public IPAddress NodeIP
        {
            get => _ip;
            set => _ip = (value is null) ? IPAddress.Parse("127.0.0.1") : value;
        }

        private ushort _port = 8333;
        /// <summary>
        /// [Default value = MainNetPort (8333)] 
        /// Node's port
        /// </summary>
        public ushort NodePort
        {
            get => _port;
            set => _port = value;
        }

        /// <summary>
        /// 8 + 16 + 2
        /// </summary>
        protected const int Size = 26;


        /// <inheritdoc/>
        public virtual void Serialize(FastStream stream)
        {
            stream.Write((ulong)NodeServices);
            stream.Write(NodeIP.MapToIPv6().GetAddressBytes());
            stream.WriteBigEndian(NodePort);
        }


        /// <inheritdoc/>
        public virtual bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadUInt64(out ulong serices))
            {
                error = Err.EndOfStream;
                return false;
            }

            _servs = (NodeServiceFlags)serices;

            if ((_servs & NodeServiceFlags.All) != _servs)
            {
                error = "Invalid services flags";
                return false;
            }

            if (!stream.TryReadByteArray(16, out byte[] ipBytes))
            {
                error = Err.EndOfStream;
                return false;
            }

            try
            {
                NodeIP = new IPAddress(ipBytes);
            }
            catch (Exception)
            {
                error = "Bytes contain an invalid IP address";
                return false;
            }

            if (NodeIP.IsIPv4MappedToIPv6)
            {
                NodeIP = NodeIP.MapToIPv4();
            }

            if (!stream.TryReadUInt16(out _port))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
