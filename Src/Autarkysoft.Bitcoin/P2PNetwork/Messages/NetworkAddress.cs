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
    /// Implements <see cref="IEquatable&#60;NetworkAddress&#62;"/> <see cref="IDeserializable"/>.
    /// </summary>
    public class NetworkAddress : IEquatable<NetworkAddress>, IDeserializable
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
        /// Total size of any <see cref="NetworkAddress"/> instance (8 + 16 + 2)
        /// </summary>
        public const int Size = 26;

        /// <summary>
        /// The services that this node supports
        /// </summary>
        public NodeServiceFlags NodeServices { get; set; }

        private IPAddress _ip = IPAddress.Loopback;
        /// <summary>
        /// [Default value = Loopback IP (127.0.0.1)]
        /// Node's IP address
        /// </summary>
        public IPAddress NodeIP
        {
            get => _ip;
            set => _ip = (value is null) ? IPAddress.Loopback : value;
        }

        /// <summary>
        /// [Default value = MainNetPort (8333)] 
        /// Node's port
        /// </summary>
        public ushort NodePort { get; set; } = Constants.MainNetPort;


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

            if (!stream.CheckRemaining(Size))
            {
                error = Err.EndOfStream;
                return false;
            }

            // For the sake of forward compatibility, service flag is not strict (undefined enums are also accepted)
            // the caller can decide which bits they understand or support.
            NodeServices = (NodeServiceFlags)stream.ReadUInt64Checked();

            // IPAddress constructor throws 2 exceptions: 
            //    1. ArgumentNullException -> if the byte[] is null
            //    2. ArgumentException -> if byte[].Length != 4 && != 16
            // As a result there is no need for try/catch block
            // https://github.com/microsoft/referencesource/blob/17b97365645da62cf8a49444d979f94a59bbb155/System/net/System/Net/IPAddress.cs#L114-L135
            NodeIP = new IPAddress(stream.ReadByteArrayChecked(16));

            if (NodeIP.IsIPv4MappedToIPv6)
            {
                NodeIP = NodeIP.MapToIPv4();
            }

            NodePort = stream.ReadUInt16BigEndianChecked();

            error = null;
            return true;
        }

        /// <summary>
        /// Compares equality of the given <see cref="NetworkAddress"/> with this instance based on their IP and port.
        /// </summary>
        /// <param name="other">Other <see cref="NetworkAddress"/> to use</param>
        /// <returns>True if both instances have the same IP and port.</returns>
        public bool Equals(NetworkAddress other)
        {
            if (other is null)
                return false;

            return ReferenceEquals(this, other) || (NodeIP.Equals(other.NodeIP) && NodePort == other.NodePort);
        }

        /// <summary>
        /// Compares equality of the given object based on its type and this instance based on their IP and port.
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if <paramref name="obj"/> is of type <see cref="NetworkAddress"/> and has same IP and port.</returns>
        public override bool Equals(object obj) => Equals(obj as NetworkAddress);

        /// <summary>
        /// Returns the unique hash code of this instance based on only IP and port.
        /// </summary>
        /// <returns>The 32-bit signed integer as hash code</returns>
        public override int GetHashCode() => HashCode.Combine(NodeIP, NodePort);
    }
}
