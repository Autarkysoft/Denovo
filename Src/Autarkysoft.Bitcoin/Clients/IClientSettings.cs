// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Net;
using System.Threading;

namespace Autarkysoft.Bitcoin.Clients
{
    /// <summary>
    /// Defines methods and properties of all client types and is used by all <see cref="Node"/> instances.
    /// </summary>
    public interface IClientSettings
    {
        /// <summary>
        /// Returns list of all nodes (peers) connected to this client.
        /// </summary>
        NodePool AllNodes { get; }
        /// <summary>
        /// Returns the client time object
        /// </summary>
        IClientTime Time { get; }
        /// <summary>
        /// Gets or sets the weak random number generator
        /// </summary>
        IRandomNonceGenerator Rng { get; set; }

        /// <summary>
        /// Gets or sets the P2P protocol version that the client supports
        /// </summary>
        int ProtocolVersion { get; set; }
        /// <summary>
        /// Gets or sets whether the client will relay blocks and transactions or not
        /// </summary>
        bool Relay { get; set; }
        /// <summary>
        /// Gets or sets name of the client as defined by <see cref="ImprovementProposals.BIP0014"/> used in version messages
        /// </summary>
        string UserAgent { get; set; }
        /// <summary>
        /// Gets or sets the network type
        /// </summary>
        NetworkType Network { get; set; }
        /// <summary>
        /// Gets or sets the services this client supports
        /// </summary>
        NodeServiceFlags Services { get; set; }
        /// <summary>
        /// Returns the default port used when connecting to other peers (mainly for IP addresses received from DNS seeds)
        /// </summary>
        public ushort DefaultPort { get; }
        /// <summary>
        /// Gets or sets the port that this client listens to
        /// </summary>
        ushort ListenPort { get; set; }
        /// <summary>
        /// Gets or sets the list of DNS seeds
        /// </summary>
        string[] DnsSeeds { get; set; }

        /// <summary>
        /// Returns total buffer size in bytes shared among all <see cref="System.Net.Sockets.SocketAsyncEventArgs"/>
        /// instances
        /// </summary>
        int BufferLength { get; }
        /// <summary>
        /// Returns maximum number of nodes to connect to. Will also determine the total allocated buffer length.
        /// </summary>
        int MaxConnectionCount { get; }
        /// <summary>
        /// Returns the <see cref="Semaphore"/> used to limit the number of connections based on <see cref="MaxConnectionCount"/>
        /// </summary>
        Semaphore MaxConnectionEnforcer { get; }
        /// <summary>
        /// Returns the pool (stack) of <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> objects used in send/receive
        /// operations. There should be 2 items per connection (a total of 2 * <see cref="MaxConnectionCount"/>)
        /// </summary>
        SocketAsyncEventArgsPool SendReceivePool { get; }

        /// <summary>
        /// Returns best known IP address of this client (<see cref="IPAddress.Loopback"/> if nothing is found).
        /// </summary>
        /// <returns>Best known IP address</returns>
        IPAddress GetMyIP();
        /// <summary>
        /// Updates this client's IP address.
        /// </summary>
        /// <param name="addr">IP address to use</param>
        void UpdateMyIP(IPAddress addr);

        /// <summary>
        /// Creates and returns a new instance <see cref="IReplyManager"/>.
        /// </summary>
        /// <param name="nodeStatus">Node status</param>
        /// <returns>A new instance of <see cref="IReplyManager"/></returns>
        IReplyManager CreateReplyManager(INodeStatus nodeStatus);
    }
}
