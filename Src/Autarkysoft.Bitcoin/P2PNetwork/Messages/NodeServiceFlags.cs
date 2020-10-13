// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// Service bits for unauthenticated advertisement of services that nodes support.
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bitcoin/blob/af22322dab1a2277483b2512723491a5fad1a606/src/protocol.h#L268-L302
    /// XThin flag is removed (old link):
    /// https://github.com/bitcoin/bitcoin/blob/fa2510d5c1cdf9c2cd5cc9887302ced4378c7202/src/protocol.h#L246-L279
    /// </remarks>
    [Flags]
    public enum NodeServiceFlags : ulong
    {
        /// <summary>
        /// Is Not a full node.
        /// </summary>
        NodeNone = 0,
        /// <summary>
        /// Indicates a full node capable of serving the whole blockchain.
        /// </summary>
        NodeNetwork = 1, //(1 << 0)
        /// <summary>
        /// Indicates a node capable of responding to the GetUtxo protocol request (BIP64).
        /// </summary>
        NodeGetUtxo = (1 << 1),
        /// <summary>
        /// Indicates a node capable of handling bloom filters.
        /// </summary>
        NodeBloom = (1 << 2),
        /// <summary>
        /// Indicates a node capable of handling witness data in blocks and transactions.
        /// </summary>
        NodeWitness = (1 << 3),
        /// <summary>
        /// Indicates a node capable of creating and handling Xtreme Thinblocks.
        /// </summary>
        NodeXThin = (1 << 4),
        /// <summary>
        /// Indicates a node capable of processing block filter requests as defined in BIP-157 and BIP-158.
        /// </summary>
        NodeCompactFilters = (1 << 6),
        /// <summary>
        /// Indicates a node similar to <see cref="NodeNetwork"/> but the node has at least 
        /// the last 288 blocks (last 2 days) (BIP159).
        /// </summary>
        NodeNetworkLimited = (1 << 10),

        /// <summary>
        /// Indicates a full node that supports all the services (except <see cref="NodeNetworkLimited"/>).
        /// </summary>
        All = NodeNone | NodeNetwork | NodeGetUtxo | NodeBloom | NodeWitness | NodeXThin | NodeCompactFilters,
        /// <summary>
        /// Indicates a limited (pruned) full node that supports all the services.
        /// </summary>
        AllLimited = NodeNone | NodeNetwork | NodeGetUtxo | NodeBloom | NodeWitness | NodeXThin | NodeCompactFilters | NodeNetworkLimited,
    }
}
