// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Collections.Generic;
using System.Net;

namespace Autarkysoft.Bitcoin.Clients
{
    public interface ISpvClientSettings : IClientSettings
    {
        /// <summary>
        /// Returns the blockchain instance to be shared among all node instances
        /// </summary>
        IChain Blockchain { get; }

        int GetRandomNodeAddrs(int count, bool v, List<NetworkAddressWithTime> addrs);

        /// <summary>
        /// Returns if the provided service flags contains services that are needed for syncing based on
        /// <see cref="IChain"/>'s current state.
        /// </summary>
        /// <param name="flags">Flags to check</param>
        /// <returns>True if the required services are available; otherwise false.</returns>
        bool HasNeededServices(NodeServiceFlags flags);
        void RemoveNodeAddr(IPAddress e);
    }
}
