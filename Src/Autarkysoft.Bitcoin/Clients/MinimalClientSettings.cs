// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;

namespace Autarkysoft.Bitcoin.Clients
{
    /// <summary>
    /// Defines minimal client settings. One instance should be used by the client and passed to all node instance through both
    /// <see cref="NodeListener"/> and <see cref="NodeConnector"/>.
    /// <para/>Inherits from <see cref="ClientSettingsBase"/>. Implements <see cref="IMinimalClientSettings"/>.
    /// </summary>
    public class MinimalClientSettings : ClientSettingsBase, IMinimalClientSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettingsBase"/> with the given parameters.
        /// </summary>
        /// <param name="netType">Network type</param>
        /// <param name="maxConnection">Maximum number of connections</param>
        /// <param name="nodes">List of peers (can be null)</param>
        public MinimalClientSettings(NetworkType netType, int maxConnection, NodePool nodes)
            : base(netType, maxConnection, nodes, NodeServiceFlags.NodeNone)
        {
            Relay = false;
        }

        /// <inheritdoc/>
        public override IReplyManager CreateReplyManager(INodeStatus nodeStatus) => new MinimalReplyManager(nodeStatus, this);
    }
}
