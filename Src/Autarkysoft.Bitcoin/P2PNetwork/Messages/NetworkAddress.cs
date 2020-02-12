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
        /// <summary>
        /// Initializes an empty instance of <see cref="NetworkAddress"/>.
        /// </summary>
        public NetworkAddress()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NetworkAddress"/> using the given parameters.
        /// </summary>
        /// <param name="servs">Services that this node supports</param>
        /// <param name="ip">IP address of this node</param>
        /// <param name="port">Port (use <see cref="Constants"/> for default values)</param>
        public NetworkAddress(NodeServiceFlags servs, IPAddress ip, ushort port)
        {
            NodeServices = servs;
            NodeIP = ip;
            NodePort = port;
        }


        /// <summary>
        /// The services that this node supports
        /// </summary>
        public NodeServiceFlags NodeServices { get; set; }

        private IPAddress _ip = IPAddress.Loopback;
        /// <summary>
        /// [Default value =  Loopback IP (127.0.0.1)]
        /// Node's IP address
        /// </summary>
        public IPAddress NodeIP
        {
            get => _ip;
            set => _ip = (value is null) ? IPAddress.Loopback : value;
        }

        private ushort _port = Constants.MainNetPort;
        /// <summary>
        /// [Default value = MainNetPort (8333)] 
        /// Node's port
        /// </summary>
        public ushort NodePort
        {
            get => _port;
            set => _port = value;
        }


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

            if (!stream.TryReadUInt64(out ulong servs))
            {
                error = Err.EndOfStream;
                return false;
            }

            // For the sake of forward compatibility, service flag is not strict (undefined enums are also accepted)
            // the caller can decide which bits they understand or support.
            NodeServices = (NodeServiceFlags)servs;

            if (!stream.TryReadByteArray(16, out byte[] ipBytes))
            {
                error = Err.EndOfStream;
                return false;
            }

            // IPAddress constructor throws 2 exceptions: 
            //    1. ArgumentNullException -> if the byte[] is null
            //    2. ArgumentException -> if byte[].Length != 4 && != 16
            // As a result there is no need for try/catch block
            // https://github.com/microsoft/referencesource/blob/17b97365645da62cf8a49444d979f94a59bbb155/System/net/System/Net/IPAddress.cs#L114-L135
            NodeIP = new IPAddress(ipBytes);

            if (NodeIP.IsIPv4MappedToIPv6)
            {
                NodeIP = NodeIP.MapToIPv4();
            }

            if (!stream.TryReadUInt16BigEndian(out _port))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
