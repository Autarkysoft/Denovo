// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Defines methods and properties to handle database and storage related operations
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// Reads network addresses from disk and returns the result as an array.
        /// </summary>
        /// <returns>An array of <see cref="NetworkAddressWithTime"/></returns>
        NetworkAddressWithTime[] ReadAddrs();
        /// <summary>
        /// Writes the given array of network addresses to disk.
        /// </summary>
        /// <param name="addrs">Node network addresses</param>
        void WriteAddrs(NetworkAddressWithTime[] addrs);

        /// <summary>
        /// Reads block headers from disk and returns the result as an array.
        /// </summary>
        /// <returns>An array of <see cref="BlockHeader"/></returns>
        BlockHeader[] ReadHeaders();
        /// <summary>
        /// Appends the given array of headers to the existing file.
        /// </summary>
        /// <param name="headers">Headers to add</param>
        void AppendBlockHeaders(BlockHeader[] headers);
        /// <summary>
        /// Writes the entire array of headers to a new file.
        /// </summary>
        /// <param name="headers">Headers to write</param>
        void WriteBlockHeaders(BlockHeader[] headers);
    }
}
