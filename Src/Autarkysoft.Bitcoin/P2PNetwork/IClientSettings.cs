// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Threading;

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

        /// <summary>
        /// Length of the buffer in bytes used by each <see cref="System.Net.Sockets.SocketAsyncEventArgs"/>
        /// </summary>
        int BufferLength { get; }
        /// <summary>
        /// Maximum number of nodes to connect to. Will also determine the total allocated buffer length.
        /// </summary>
        int MaxConnectionCount { get; set; }
        /// <summary>
        /// A <see cref="Semaphore"/> used to limit the number of connections based on <see cref="MaxConnectionCount"/>
        /// </summary>
        Semaphore MaxConnectionEnforcer { get; }
        /// <summary>
        /// A pool (stack) of <see cref="System.Net.Sockets.SocketAsyncEventArgs"/> objects used in send/receive operations.
        /// There should be 2 items per connection (a total of 2 * <see cref="MaxConnectionCount"/>)
        /// </summary>
        SocketAsyncEventArgsPool SendReceivePool { get; }

        /// <summary>
        /// Returns an array of known node network addresses to this client.
        /// </summary>
        /// <returns>
        /// An array of <see cref="NetworkAddressWithTime"/> (may contain more items than <see cref="Constants.MaxAddrCount"/>
        /// limit)
        /// </returns>
        NetworkAddressWithTime[] GetNodeAddrs();
        /// <summary>
        /// Updates the list of node IP addresses (should also handle storing to disk).
        /// </summary>
        /// <param name="nodeAddresses">List of timestamped nodes network addresses</param>
        void UpdateNodeAddrs(NetworkAddressWithTime[] nodeAddresses);
    }
}