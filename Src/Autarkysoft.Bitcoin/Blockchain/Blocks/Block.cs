﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    /// <summary>
    /// The main component of the blockchain that contains transactions and shapes up the chain by referencing
    /// the previous block and is secured using cryptography in form of the proof of work algorithm.
    /// Implements <see cref="IBlock"/>.
    /// </summary>
    public class Block : IBlock
    {
        /// <summary>
        /// Initializes a new empty instance of <see cref="Block"/>.
        /// </summary>
        public Block()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Block"/> using given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="header">Block header</param>
        /// <param name="txs">List of transactions</param>
        public Block(in BlockHeader header, ITransaction[] txs)
        {
            Header = header;
            TransactionList = txs;
        }


        /// <inheritdoc/>
        public int Height { get; set; } = -1;

        /// <inheritdoc/>
        public int TotalSize
        {
            get
            {
                var counter = new SizeCounter();
                AddSerializedSize(counter);
                return counter.Size;
            }
        }

        /// <inheritdoc/>
        public int StrippedSize
        {
            get
            {
                var counter = new SizeCounter();
                AddStrippedSerializedSize(counter);
                return counter.Size;
            }
        }

        /// <inheritdoc/>
        public int Weight
        {
            get
            {
                var counter = new SizeCounter();
                AddStrippedSerializedSize(counter);
                int baseSize = counter.Size;

                counter.Reset();
                AddSerializedSize(counter);

                return (baseSize * 3) + counter.Size;
            }
        }

        /// <inheritdoc/>
        public BlockHeader Header { get; set; }

        private ITransaction[] _txs = new ITransaction[0];
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public ITransaction[] TransactionList
        {
            get => _txs;
            set
            {
                if (value is null || value.Length == 0)
                    throw new ArgumentNullException(nameof(TransactionList), "Transaction list can not be null or empty.");

                _txs = value;
            }
        }



        /// <inheritdoc/>
        public byte[] GetBlockHash() => Header.Hash.ToByteArray();

        /// <inheritdoc/>
        public string GetBlockID() => Header.GetID();


        // TODO: change SHA256 and the following 2 methods to return Digest256 directly

        /// <inheritdoc/>
        public unsafe Digest256 ComputeMerkleRoot()
        {
            if (TransactionList.Length == 1)
            {
                // Only has Coinbase transaction
                return TransactionList[0].GetTransactionHash();
            }
            else
            {
                // To compute merkle root all transaction hashes are placed inside a big buffer, buffer is padded
                // by repeating the last hash if needed to be divisible into groups of 2.
                // Then hash of each group is computed and copied into the same buffer from the start.
                // The process ends when there is only 1 hash remaining.
                using Sha256 sha = new Sha256();
                bool needDup = TransactionList.Length % 2 != 0;
                byte[] buffer = new byte[(needDup ? TransactionList.Length + 1 : TransactionList.Length) * 32];
                int hashCount = buffer.Length / 32;
                int offset = 0;
                foreach (var tx in TransactionList)
                {
                    Buffer.BlockCopy(tx.GetTransactionHash().ToByteArray(), 0, buffer, offset, 32);
                    offset += 32;
                }
                if (needDup)
                {
                    Buffer.BlockCopy(TransactionList[^1].GetTransactionHash().ToByteArray(), 0, buffer, offset, 32);
                }

                fixed (uint* hPt = &sha.hashState[0], wPt = &sha.w[0])
                fixed (byte* bufPt = &buffer[0])
                {
                    while (hashCount > 1)
                    {
                        byte* src = bufPt;
                        byte* dst = bufPt;
                        for (int i = 0; i < hashCount / 2; i++)
                        {
                            // Since each "data" being hashed is fixed 64 byte, an optimized SHA256 method is called
                            // to compute SHA256(SHA256(64-byte-data))
                            sha.Compress64Double(src, dst, hPt, wPt);
                            src += 64;
                            dst += 32;
                        }
                        hashCount /= 2;
                        if (hashCount == 1)
                        {
                            break;
                        }
                        needDup = hashCount % 2 != 0;
                        if (needDup)
                        {
                            Buffer.MemoryCopy(dst - 32, dst, buffer.Length, 32);
                            hashCount++;
                        }
                    }
                }

                byte[] result = new byte[32];
                Buffer.BlockCopy(buffer, 0, result, 0, 32);
                return new Digest256(result);
            }
        }

        /// <inheritdoc/>
        public unsafe Digest256 ComputeWitnessMerkleRoot(byte[] commitment)
        {
            // This is the same as MerkleRoot but the first hash (of coinbase tx) is all zeros and
            // witness hashes are used and
            // an extra 32 byte (the witness commitment) is added to the final result and hashed again twice (DoubleSha256)
            if (TransactionList.Length == 1)
            {
                // Only has Coinbase transaction and WitnessTxId of coinbase transaction is empty
                byte[] toHash = new byte[64];
                byte[] result = new byte[32];
                Buffer.BlockCopy(commitment, 0, toHash, 32, 32);
                using Sha256 sha = new Sha256();
                // TODO: this could be optimized more by knowing first 32 bytes are zero
                //       also optimized more for when commitment is also all zeros
                fixed (uint* hPt = &sha.hashState[0], wPt = &sha.w[0])
                fixed (byte* bufPt = &toHash[0], rs = &result[0])
                {
                    sha.Compress64Double(bufPt, rs, hPt, wPt);
                    return new Digest256(result);
                }
            }
            else
            {
                using Sha256 sha = new Sha256();
                bool needDup = TransactionList.Length % 2 != 0;
                byte[] buffer = new byte[(needDup ? TransactionList.Length + 1 : TransactionList.Length) * 32];
                int hashCount = buffer.Length / 32;
                int offset = 32; // Start from offset 32 to set first 32 bytes to zero and copy tx[1].hash to buffer
                for (int i = 1; i < TransactionList.Length; i++)
                {
                    ITransaction tx = TransactionList[i];
                    // Use witness transaction hash instead of transaction has here
                    Buffer.BlockCopy(tx.GetWitnessTransactionHash().ToByteArray(), 0, buffer, offset, 32);
                    offset += 32;
                }
                if (needDup)
                {
                    Buffer.BlockCopy(TransactionList[^1].GetWitnessTransactionHash().ToByteArray(), 0, buffer, offset, 32);
                }

                fixed (uint* hPt = &sha.hashState[0], wPt = &sha.w[0])
                fixed (byte* bufPt = &buffer[0], cmt = &commitment[0])
                {
                    while (hashCount > 1)
                    {
                        byte* src = bufPt;
                        byte* dst = bufPt;
                        for (int i = 0; i < hashCount / 2; i++)
                        {
                            // Since each "data" being hashed is fixed 64 byte, an optimized SHA256 method is called
                            // to compute SHA256(SHA256(64-byte-data))
                            sha.Compress64Double(src, dst, hPt, wPt);
                            src += 64;
                            dst += 32;
                        }
                        hashCount /= 2;
                        if (hashCount == 1)
                        {
                            break;
                        }
                        needDup = hashCount % 2 != 0;
                        if (needDup)
                        {
                            Buffer.MemoryCopy(dst - 32, dst, buffer.Length, 32);
                            hashCount++;
                        }
                    }

                    // Merkle root is computed here and is written in first 32 bytes of the dst (buffer),
                    // copy commitment after it and compute hash of the 64 byte block.
                    Buffer.MemoryCopy(cmt, bufPt + 32, buffer.Length, 32);
                    sha.Compress64Double(bufPt, bufPt, hPt, wPt);
                }

                byte[] result = new byte[32];
                Buffer.BlockCopy(buffer, 0, result, 0, 32);
                return new Digest256(result);
            }
        }


        /// <inheritdoc/>
        public void AddSerializedSize(SizeCounter counter)
        {
            Header.AddSerializedSize(counter);
            counter.AddCompactIntCount(TransactionList.Length);
            foreach (var tx in TransactionList)
            {
                tx.AddSerializedSize(counter);
            }
        }

        /// <inheritdoc/>
        public void AddStrippedSerializedSize(SizeCounter counter)
        {
            Header.AddSerializedSize(counter);
            counter.AddCompactIntCount(TransactionList.Length);
            foreach (ITransaction tx in TransactionList)
            {
                tx.AddSerializedSizeWithoutWitness(counter);
            }
        }

        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] SerializeWithoutWitness()
        {
            FastStream stream = new FastStream(TransactionList.Length * 250);
            SerializeWithoutWitness(stream);
            return stream.ToByteArray();
        }

        /// <inheritdoc/>
        public void SerializeWithoutWitness(FastStream stream)
        {
            Header.Serialize(stream);

            CompactInt txCount = new CompactInt(TransactionList.Length);
            txCount.WriteToStream(stream);
            foreach (ITransaction tx in TransactionList)
            {
                tx.SerializeWithoutWitness(stream);
            }
        }

        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            Header.Serialize(stream);

            CompactInt txCount = new CompactInt(TransactionList.Length);
            txCount.WriteToStream(stream);
            foreach (ITransaction tx in TransactionList)
            {
                tx.Serialize(stream);
            }
        }

        /// <summary>
        /// Converts this instance into its byte array representation.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public byte[] Serialize()
        {
            FastStream stream = new FastStream(TransactionList.Length * 250);
            Serialize(stream);
            return stream.ToByteArray();
        }


        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (!BlockHeader.TryDeserialize(stream, out BlockHeader hdr, out error))
            {
                return false;
            }
            Header = hdr;

            if (!CompactInt.TryRead(stream, out CompactInt txCount, out error))
            {
                return false;
            }
            if (txCount > int.MaxValue)
            {
                error = Errors.TxCountOverflow;
                return false;
            }

            TransactionList = new Transaction[(int)txCount];
            for (int i = 0; i < TransactionList.Length; i++)
            {
                Transaction temp = new Transaction();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                TransactionList[i] = temp;
            }

            error = Errors.None;
            return true;
        }
    }
}
