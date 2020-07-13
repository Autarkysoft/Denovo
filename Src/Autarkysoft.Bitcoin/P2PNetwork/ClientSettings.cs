// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Reflection;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines client settings. 1 instance should be used by the client and passed to each node instance.
    /// Implements <see cref="IClientSettings"/>.
    /// </summary>
    public class ClientSettings : IClientSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettings"/> with default parameters.
        /// </summary>
        public ClientSettings()
            : this(Constants.P2PProtocolVersion,
                   true,
                   new BIP0014("Bitcoin.Net", Assembly.GetExecutingAssembly().GetName().Version, "Bitcoin from scratch").ToString(),
                   NetworkType.MainNet,
                   NodeServiceFlags.All)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettings"/> with the given parameters.
        /// </summary>
        /// <param name="pver">Protocol version</param>
        /// <param name="relay">True to relay blocks and transactions; false otherwise</param>
        /// <param name="ua">User agent</param>
        /// <param name="netType">Network type</param>
        /// <param name="servs">Services supported by this node</param>
        public ClientSettings(int pver, bool relay, BIP0014 ua, NetworkType netType, NodeServiceFlags servs)
            : this(pver, relay, ua.ToString(), netType, servs)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettings"/> with the given parameters.
        /// </summary>
        /// <param name="pver">Protocol version</param>
        /// <param name="relay">True to relay blocks and transactions; false otherwise</param>
        /// <param name="ua">User agent as defined by <see cref="BIP0014"/></param>
        /// <param name="netType">Network type</param>
        /// <param name="servs">Services supported by this node</param>
        public ClientSettings(int pver, bool relay, string ua, NetworkType netType, NodeServiceFlags servs)
        {
            ProtocolVersion = pver;
            Relay = relay;
            UserAgent = ua;
            Network = netType;
            Services = servs;
        }


        /// <inheritdoc/>
        public int ProtocolVersion { get; set; }
        /// <inheritdoc/>
        public bool Relay { get; set; }
        /// <inheritdoc/>
        public string UserAgent { get; set; }
        /// <inheritdoc/>
        public NetworkType Network { get; set; }
        /// <inheritdoc/>
        public NodeServiceFlags Services { get; set; }
        /// <inheritdoc/>
        public long Time => UnixTimeStamp.GetEpochUtcNow();
        /// <inheritdoc/>
        public ushort Port { get; set; }
    }
}
