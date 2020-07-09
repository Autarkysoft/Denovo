// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines methods and properties used for showing a node's status.
    /// </summary>
    public interface INodeStatus
    {
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
