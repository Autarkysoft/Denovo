// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Full client's current state
    /// </summary>
    public enum BlockchainState
    {
        /// <summary>
        /// Not started yet
        /// </summary>
        None,
        /// <summary>
        /// First sync step, downloading headers from only one peer
        /// </summary>
        HeadersSync,
        /// <summary>
        /// Second sync step, downloading blocks by having the headers and knowing their hashes
        /// </summary>
        BlocksSync,
        /// <summary>
        /// Fully synchronize client that has to only download new blocks and can provide history to other peers,
        /// has a mempool, ...
        /// </summary>
        Synchronized
    }
}
