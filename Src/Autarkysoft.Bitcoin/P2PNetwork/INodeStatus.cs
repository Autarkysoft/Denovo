// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Net;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines methods and properties used for showing a node's status.
    /// </summary>
    public interface INodeStatus
    {
        // TODO: should this be the block hashes?
        /// <summary>
        /// List of block heights to download
        /// </summary>
        List<int> BlocksToGet { get; }

        /// <summary>
        /// IP address of this node
        /// </summary>
        IPAddress IP { get; set; }
        /// <summary>
        /// Port of this node
        /// </summary>
        ushort Port { get; set; }
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
        /// Send compact version
        /// </summary>
        ulong SendCompactVer { get; set; }
        /// <summary>
        /// Gets or sets if this nodes wants to receive block headers first
        /// </summary>
        bool SendHeaders { get; set; }
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
        /// Returns if the connection to this node should be terminated due to excessive violations
        /// </summary>
        bool HasTooManyViolations { get; }
        /// <summary>
        /// Returns if the addr message was sent to this node (prevents multiple requests from each nodes)
        /// </summary>
        bool IsAddrSent { get; set; }
        /// <summary>
        /// Gets or sets the node's latency as a <see cref="TimeSpan"/> (calculated using ping messages)
        /// </summary>
        TimeSpan Latency { get; set; }
        /// <summary>
        /// Returns if the other node hasn't answered to too many of our Ping messages.
        /// </summary>
        bool HasTooManyUnansweredPings { get; }
        /// <summary>
        /// Stores the random nonce of the sent ping message to this node alongside current time for further reference.
        /// Return value indicates whether the nonce wasn't added before.
        /// </summary>
        /// <param name="nonce">The random 64-bit number used in ping message</param>
        /// <returns>True if the nonce is new; otherwise false.</returns>
        bool StorePing(long nonce);
        /// <summary>
        /// Checks the given random nonce received in the pong message versus the local stored value 
        /// (in <see cref="StorePing(long)"/> method), removes it from local storage and sets the latency.
        /// </summary>
        /// <param name="nonce">The random 64-bit number used in ping message and received in pong message</param>
        void CheckPing(long nonce);

        /// <summary>
        /// An event to be raised whenever the connection has to be terminated (could be due to high violation score,
        /// or simply dead connection)
        /// </summary>
        event EventHandler DisconnectEvent;
        /// <summary>
        /// Raises the disconnect event to signal disconnecting from this node.
        /// </summary>
        void SignalDisconnect();

        /// <summary>
        /// Disposes the disconnect timer (useful to free up resources).
        /// </summary>
        void DisposeDisconnectTimer();
        /// <summary>
        /// Restarts the disconnect timer
        /// </summary>
        void ReStartDisconnectTimer();
        /// <summary>
        /// Starts the disconnect timer to call <see cref="SignalDisconnect"/> when the <paramref name="interval"/>
        /// is reached to shut down this peer. Useful for disconnecting peers that don't reply to important messages
        /// such as during initial synchronization.
        /// </summary>
        /// <param name="interval">The timer's interval in milliseconds</param>
        void StartDisconnectTimer(double interval);
        /// <summary>
        /// Stops the disconnect timer
        /// </summary>
        void StopDisconnectTimer();

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
