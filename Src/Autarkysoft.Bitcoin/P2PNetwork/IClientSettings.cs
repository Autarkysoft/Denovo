// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Net;
using System.Threading;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines methods and properties of a client and is used by all <see cref="Node"/> instances.
    /// </summary>
    public interface IClientSettings
    {
        /// <summary>
        /// List of all nodes (peers) connected to this client.
        /// </summary>
        NodePool AllNodes { get; set; }
        /// <summary>
        /// Gets or sets the client time
        /// </summary>
        IClientTime Time { get; set; }

        /// <summary>
        /// Gets or sets the blockchain instance to be shared among all node instances
        /// </summary>
        IBlockchain Blockchain { get; set; }

        /// <summary>
        /// Gets or sets the memory pool instance that is shared by all node instances
        /// </summary>
        IMemoryPool MemPool { get; set; }

        /// <summary>
        /// A weak random number generator
        /// </summary>
        IRandomNonceGenerator Rng { get; set; }

        /// <summary>
        /// Protocol version that the client supports
        /// </summary>
        int ProtocolVersion { get; set; }
        /// <summary>
        /// Returns whether the client will relay blocks and transactions or not
        /// </summary>
        bool Relay { get; set; }
        /// <summary>
        /// Gets or sets the minimum fee rate (in satoshi per byte) of transactions that are accepted in <see cref="IMemoryPool"/>
        /// </summary>
        ulong MinTxRelayFee { get; set; }
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
        /// Port that this client listens to and makes connection over
        /// </summary>
        ushort Port { get; set; }
        /// <summary>
        /// Gets or sets whether the client should have an open <see cref="System.Net.Sockets.Socket"/> to listen for incoming
        /// connections.
        /// </summary>
        bool AcceptIncomingConnections { get; set; }
        /// <summary>
        /// List of DNS seeds
        /// </summary>
        string[] DnsSeeds { get; set; }

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
        /// Returns if the provided service flags contains services that are needed for syncing based on
        /// <see cref="IBlockchain"/>'s current state.
        /// </summary>
        /// <param name="flags">Flags to check</param>
        /// <returns>True if the required services are available; otherwise false.</returns>
        bool HasNeededServices(NodeServiceFlags flags);
        /// <summary>
        /// Returns if the provided service flags contains services that are needed for syncing block headers.
        /// <para/>Requires: <see cref="NodeServiceFlags.NodeNetwork"/> or <see cref="NodeServiceFlags.NodeNetworkLimited"/>
        /// bits
        /// </summary>
        /// <param name="flags">Flags to check</param>
        /// <returns>True if the required services are available; otherwise false.</returns>
        bool IsGoodForHeaderSync(NodeServiceFlags flags);
        /// <summary>
        /// Returns if the provided service flags contains services that are needed for syncing blocks.
        /// <para/>Requires: <see cref="NodeServiceFlags.NodeNetwork"/> and <see cref="NodeServiceFlags.NodeWitness"/>
        /// bits but but no <see cref="NodeServiceFlags.NodeNetworkLimited"/> bit.
        /// </summary>
        /// <param name="flags">Flags to check</param>
        /// <returns>True if the required services are available; otherwise false.</returns>
        bool IsGoodForBlockSync(NodeServiceFlags flags);
        /// <summary>
        /// Returns if the provided service flags contains <see cref="NodeServiceFlags.NodeNetworkLimited"/>
        /// indicating the peer is a pruned node.
        /// </summary>
        /// <param name="flags">Flags to check</param>
        /// <returns>True if the required services are available; otherwise false.</returns>
        bool IsPruned(NodeServiceFlags flags);

        /// <summary>
        /// Adds the given transaction to memory pool
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        bool AddToMempool(ITransaction tx);

        /// <summary>
        /// Returns between <paramref name="min"/> and <paramref name="max"/> number of <see cref="NetworkAddressWithTime"/>s.
        /// If the node address file is not found or is corrupted, returns null.
        /// </summary>
        /// <param name="min">Minimum number of addresses to return</param>
        /// <param name="max">Maximum number of addresses to return</param>
        /// <param name="skipCheck">
        /// True to skip checking the returned values  (for Addr message), false to check the IP and service flags (for connection)
        /// </param>
        /// <returns>Null or at least <paramref name="min"/> and at most <paramref name="max"/> number of addresses</returns>
        NetworkAddressWithTime[] GetRandomNodeAddrs(int min, int max, bool skipCheck);
        /// <summary>
        /// Removes the peer from peer list that has the given IP address.
        /// </summary>
        /// <param name="ip">IP address of the peer to remove</param>
        void RemoveNodeAddr(IPAddress ip);
        /// <summary>
        /// Updates the list of node IP addresses (should also handle storing to disk).
        /// </summary>
        /// <param name="nodeAddresses">List of timestamped nodes network addresses</param>
        void UpdateNodeAddrs(NetworkAddressWithTime[] nodeAddresses);

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
    }
}