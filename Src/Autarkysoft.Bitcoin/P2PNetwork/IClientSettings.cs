// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines methods and properties of a client and is used by all <see cref="Node"/> instances.
    /// </summary>
    public interface IClientSettings
    {
        /// <summary>
        /// Protocol version that the client supports
        /// </summary>
        int ProtocolVersion { get; set; }
        /// <summary>
        /// Returns whether the client will relay blocks and transactions or not
        /// </summary>
        bool Relay { get; set; }
        /// <summary>
        /// Name of the client as defined by <see cref="ImprovementProposals.BIP0014"/>
        /// </summary>
        string UserAgent { get; set; }
        /// <summary>
        /// Network type
        /// </summary>
        NetworkType Network { get; set; }
        /// <summary>
        /// Services this client supports
        /// </summary>
        NodeServiceFlags Services { get; set; }
        /// <summary>
        /// Returns the current UTC time as an epoch timestamp
        /// </summary>
        long Time { get; }
        /// <summary>
        /// Port that this client listens to and makes connection over
        /// </summary>
        ushort Port { get; set; }
    }
}