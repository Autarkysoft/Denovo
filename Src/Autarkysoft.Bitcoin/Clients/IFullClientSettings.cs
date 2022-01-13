// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Net;

namespace Autarkysoft.Bitcoin.Clients
{
    /// <summary>
    /// Defines methods and properties of a full client settings that is used by all <see cref="Node"/> instances.
    /// Inherits from <see cref="IClientSettings"/>.
    /// </summary>
    public interface IFullClientSettings : IClientSettings
    {
        /// <summary>
        /// Returns the blockchain instance to be shared among all node instances
        /// </summary>
        IBlockchain Blockchain { get; }

        /// <summary>
        /// Gets or sets the memory pool instance that is shared by all node instances
        /// </summary>
        IMemoryPool MemPool { get; set; }

        /// <summary>
        /// Gets or sets the minimum fee rate (in satoshi per byte) of transactions that are accepted in <see cref="IMemoryPool"/>
        /// </summary>
        ulong MinTxRelayFee { get; set; }
        /// <summary>
        /// Gets or sets whether the client should have an open <see cref="System.Net.Sockets.Socket"/> to listen for incoming
        /// connections.
        /// </summary>
        bool AcceptIncomingConnections { get; set; }

        // TODO: remore this method by adding a Add() method to IMemoryPool
        /// <summary>
        /// Adds the given transaction to memory pool
        /// </summary>
        /// <param name="tx"></param>
        /// <returns></returns>
        bool AddToMempool(ITransaction tx);
        /// <summary>
        /// Returns up to <paramref name="count"/> number of <see cref="NetworkAddressWithTime"/>s at random.
        /// If the node address file is not found or is corrupted, returns null.
        /// </summary>
        /// <param name="count">Number of addresses to return</param>
        /// <param name="skipCheck">
        /// True to skip checking the returned values  (for Addr message), false to check the IP and service flags (for connection)
        /// </param>
        /// <returns>Null or at most <paramref name="count"/> number of addresses</returns>
        NetworkAddressWithTime[] GetRandomNodeAddrs(int count, bool skipCheck);
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
    }
}
