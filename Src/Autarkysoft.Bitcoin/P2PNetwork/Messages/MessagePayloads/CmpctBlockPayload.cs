// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload sending a single block with compact format.
    /// <para/> Sent: unsolicited (if <see cref="SendCmpctPayload"/> was received initially) or in response to 
    /// <see cref="GetDataPayload"/> with <see cref="InventoryType.CompactBlock"/>
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bips/blob/master/bip-0152.mediawiki
    /// </remarks>
    public class CmpctBlockPayload : PayloadBase
    {
        public CmpctBlockPayload()
        {
        }

        public CmpctBlockPayload(IBlock block)
        {
            if (block is null)
                throw new ArgumentNullException(nameof(block), "Block can not be null.");

            FastStream stream = new FastStream(Constants.BlockHeaderSize);
            block.Header.Serialize(stream);
            BlockHeader = stream.ToByteArray();
        }


        private byte[] _header;
        /// <summary>
        /// 80 byte block header
        /// </summary>
        public byte[] BlockHeader
        {
            get => _header;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(BlockHeader), "Block header can not be null.");
                if (value.Length != Constants.BlockHeaderSize)
                    throw new ArgumentOutOfRangeException(nameof(BlockHeader), $"Header length must be {Constants.BlockHeaderSize}");

                _header = value;
            }
        }

        private ulong _nonce;
        public ulong Nonce
        {
            get => _nonce;
            set => _nonce = value;
        }

        public ulong[] ShortIDs { get; set; }
        public PrefilledTransaction[] Txs { get; set; }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.CmpctBlock;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            counter.Add(BlockHeader.Length);
            counter.AddUInt64();
            counter.AddCompactIntCount(ShortIDs.Length);
            counter.Add(ShortIDs.Length * 6);
            counter.AddCompactIntCount(Txs.Length);
            foreach (var item in Txs)
            {
                item.AddSerializedSize(counter);
            }
        }

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            stream.Write(BlockHeader);
            stream.Write(Nonce);
            new CompactInt(ShortIDs.Length).WriteToStream(stream);
            foreach (var val in ShortIDs)
            {
                stream.Write(new byte[6]
                {
                    (byte)val, (byte)(val >> 8), (byte)(val >> 16), (byte)(val >> 24), (byte)(val >> 32), (byte)(val >> 40)
                });
            }
            new CompactInt(Txs.Length).WriteToStream(stream);
            foreach (var item in Txs)
            {
                item.Serialize(stream);
            }
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (!stream.TryReadByteArray(80, out _header))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!stream.TryReadUInt64(out _nonce))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt count, out error))
            {
                return false;
            }

            if (count > int.MaxValue)
            {
                error = "Short ID count is too big.";
                return false;
            }

            ShortIDs = new ulong[(int)count];
            for (int i = 0; i < ShortIDs.Length; i++)
            {
                stream.TryReadByteArray(6, out byte[] temp);
                ShortIDs[i] = temp[0]
                         | ((ulong)temp[1] << 8)
                         | ((ulong)temp[2] << 16)
                         | ((ulong)temp[3] << 24)
                         | ((ulong)temp[4] << 32)
                         | ((ulong)temp[5] << 40);
            }

            if (!CompactInt.TryRead(stream, out count, out error))
            {
                return false;
            }

            if (count > int.MaxValue)
            {
                error = "Tx count is too big.";
                return false;
            }

            Txs = new PrefilledTransaction[(int)count];
            for (int i = 0; i < Txs.Length; i++)
            {
                var temp = new PrefilledTransaction();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                Txs[i] = temp;
            }

            error = null;
            return true;
        }
    }
}
