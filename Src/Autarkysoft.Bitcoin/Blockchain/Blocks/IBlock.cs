// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    /// <summary>
    /// Defines methods and properties that a block class implements.
    /// <para/>Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface IBlock : IDeserializable
    {
        /// <summary>
        /// Returns total size (block size when serialized)
        /// </summary>
        int TotalSize { get; }
        /// <summary>
        /// Returns stripped (or base) size (block size with all witnesses removedaa when serialized)
        /// </summary>
        int StrippedSize { get; }
        /// <summary>
        /// Returns block weight defined in BIP-141 as (BaseSize * 3 + TotalSize)
        /// </summary>
        int Weight { get; }

        /// <summary>
        /// The block header
        /// </summary>
        BlockHeader Header { get; set; }

        /// <summary>
        /// List of transactions in this block
        /// </summary>
        ITransaction[] TransactionList { get; set; }


        /// <summary>
        /// Adds stripped serialized size of this instance to the given counter.
        /// </summary>
        /// <param name="counter">Size counter to use</param>
        void AddStrippedSerializedSize(SizeCounter counter);

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
        Digest256 ComputeMerkleRoot();

        /// <summary>
        /// Returns merkle root hash of witnesses in this block using the list of transactions.
        /// </summary>
        /// <param name="commitment">32 byte witness commitment</param>
        /// <returns>Merkle root</returns>
        Digest256 ComputeWitnessMerkleRoot(byte[] commitment);
    }
}
