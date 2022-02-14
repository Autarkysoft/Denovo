// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class MerkleBlockPayload : PayloadBase
    {
        public BlockHeader Header { get; set; } = new BlockHeader();
        private uint _txCount;
        public uint TransactionCount
        {
            get => _txCount;
            set => _txCount = value;
        }

        public byte[][] Hashes { get; set; }

        private byte[] _flags;
        public byte[] Flags
        {
            get => _flags;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Flags), "Flags can not be null.");

                _flags = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.MerkleBlock;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            counter.AddCompactIntCount(Hashes.Length);
            counter.AddCompactIntCount(Flags.Length);
            counter.Add(BlockHeader.Size + (Hashes.Length * 32) + Flags.Length);
        }

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt hashCount = new CompactInt(Hashes.Length);
            CompactInt flagsLength = new CompactInt(Flags.Length);

            Header.Serialize(stream);
            stream.Write(TransactionCount);
            hashCount.WriteToStream(stream);
            foreach (var item in Hashes)
            {
                stream.Write(item);
            }
            flagsLength.WriteToStream(stream);
            stream.Write(Flags);
        }


        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }


            if (!Header.TryDeserialize(stream, out error))
            {
                return false;
            }

            if (!stream.TryReadUInt32(out _txCount))
            {
                error = Errors.EndOfStream;
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt hashCount, out error))
            {
                return false;
            }
            if (hashCount > int.MaxValue)
            {
                error = Errors.MsgMerkleBlockHashCountOverflow;
                return false;
            }

            Hashes = new byte[(int)hashCount][];
            for (int i = 0; i < (int)hashCount; i++)
            {
                if (!stream.TryReadByteArray(32, out Hashes[i]))
                {
                    return false;
                }
            }

            if (!CompactInt.TryRead(stream, out CompactInt flagsLength, out error))
            {
                return false;
            }
            if (flagsLength > int.MaxValue)
            {
                error = Errors.MsgMerkleBlockFlagLenOverflow;
                return false;
            }

            if (!stream.TryReadByteArray((int)flagsLength, out _flags))
            {
                error = Errors.EndOfStream;
                return false;
            }

            return true;
        }
    }
}
