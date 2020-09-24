// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines methods and properties used for showing a node's status.
    /// </summary>
    public interface INodeStatus
    {
        /// <summary>
        /// Gets or sets the protocol version that this node supports 
        /// announced in <see cref="Messages.MessagePayloads.VersionPayload"/>
        /// </summary>
        int ProtocolVersion { get; set; }
        /// <summary>
        /// Gets or sets the services this node supports announced in <see cref="Messages.MessagePayloads.VersionPayload"/>
        /// </summary>
        NodeServiceFlags Services { get; set; }
        /// <summary>
        /// Gets or sets the nonce announced in <see cref="Messages.MessagePayloads.VersionPayload"/>
        /// </summary>
        ulong Nonce { get; set; }
        /// <summary>
        /// Gets or sets the user agent (client name usually using <see cref="ImprovementProposals.BIP0014"/>) 
        /// announced in <see cref="Messages.MessagePayloads.VersionPayload"/>
        /// </summary>
        string UserAgent { get; set; }
        /// <summary>
        /// Gets or sets the starting best block height of this node
        /// announced in <see cref="Messages.MessagePayloads.VersionPayload"/>
        /// </summary>
        int StartHeight { get; set; }
        /// <summary>
        /// Gets or sets whether new transactions should be sent to this node,
        /// announced in <see cref="Messages.MessagePayloads.VersionPayload"/>
        /// </summary>
        bool Relay { get; set; }
        /// <summary>
        /// Minimum fee rate in in Satoshis per kilobyte for transactions that this node wishes to receive
        /// </summary>
        ulong FeeFilter { get; set; }
        /// <summary>
        /// Returns if compact blocks should be sent to this node
        /// </summary>
        bool SendCompact { get; set; }
        /// <summary>
        /// Returns if the connection to this node should be terminated due to excessive violations
        /// </summary>
        bool ShouldDisconnect { get; }
        /// <summary>
        /// Last time this node was communicated with
        /// </summary>
        DateTime LastSeen { get; }
        /// <summary>
        /// The current state of hand-shake with this node
        /// </summary>
        HandShakeState HandShake { get; set; }
        /// <summary>
        /// Returns if this node was disconnected (it is safe to be disposed)
        /// </summary>
        bool IsDisconnected { get; set; }

        /// <summary>
        /// Changes <see cref="LastSeen"/> to current time
        /// </summary>
        void UpdateTime();
        /// <summary>
        /// Increments node's violation point by a small value for small violations.
        /// </summary>
        void AddSmallViolation();
        /// <summary>
        /// Increments node's violation point by a medium value for medium violations.
        /// </summary>
        void AddMediumViolation();
        /// <summary>
        /// Increments node's violation point by a big value for big violations that should result in
        /// termination of the connection.
        /// </summary>
        void AddBigViolation();
    }
}
