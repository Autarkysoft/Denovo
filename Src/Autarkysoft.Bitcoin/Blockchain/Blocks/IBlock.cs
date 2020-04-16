// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    /// <summary>
    /// Defines methods and properties that a block class implements. Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface IBlock : IDeserializable
    {
        /// <summary>
        /// This block's height
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Block version
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// Hash of the previous block header
        /// </summary>
        byte[] PreviousBlockHeaderHash { get; set; }

        /// <summary>
        /// The merkle root hash
        /// </summary>
        byte[] MerkleRootHash { get; set; }

        /// <summary>
        /// Block time
        /// </summary>
        uint BlockTime { get; set; }

        /// <summary>
        /// Target of this block, used for defining difficulty
        /// </summary>
        Target NBits { get; set; }

        /// <summary>
        /// Nonce (a random 32-bit integer used in mining)
        /// </summary>
        uint Nonce { get; set; }

        /// <summary>
        /// List of transactions in this block
        /// </summary>
        ITransaction[] TransactionList { get; set; }


        /// <summary>
        /// Returns hash of this block using the defined hash function.
        /// </summary>
        /// <returns>Block hash</returns>
        byte[] GetBlockHash();

        /// <summary>
        /// Returns hash of this block as a base-16 encoded string.
        /// </summary>
        /// <returns>Base-16 encoded block hash</returns>
        string GetBlockID();

        /// <summary>
        /// Returns merkle root of this block using the list of transactions.
        /// </summary>
        /// <returns>Merkle root</returns>
        byte[] ComputeMerkleRoot();

        /// <summary>
        /// Returns merkle root hash of witnesses in this block using the list of transactions.
        /// </summary>
        /// <param name="commitment">32 byte witness commitment</param>
        /// <returns>Merkle root</returns>
        byte[] ComputeWitnessMerkleRoot(byte[] commitment);

        /// <summary>
        /// Converts this block's header into its byte array representation and writes the result to the given stream.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        void SerializeHeader(FastStream stream);

        /// <summary>
        /// Deserializes the given byte array from the given stream. The return value indicates success.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if deserialization was successful, false if otherwise</returns>
        bool TryDeserializeHeader(FastStreamReader stream, out string error);
    }
}
