// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing an array of transactions from a block. (BIP-152)
    /// <para/> Sent: in response to <see cref="GetBlockTxnPayload"/>
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bips/blob/master/bip-0152.mediawiki
    /// </remarks>
    public class BlockTxnPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="AddrPayload"/> used for deserialization.
        /// </summary>
        public BlockTxnPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AddrPayload"/> with the given network address list.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="blockHash">The SHA-256 block hash</param>
        /// <param name="transactionList">The list of transactions</param>
        public BlockTxnPayload(byte[] blockHash, ITransaction[] transactionList)
        {
            BlockHash = blockHash;
            Transactions = transactionList;
        }


        private byte[] _blkHash;
        /// <summary>
        /// The SHA-256 block hash
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] BlockHash
        {
            get => _blkHash;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(BlockHash), "Block hash can not be null.");
                if (value.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(BlockHash), "Block hash must be 32 bytes.");

                _blkHash = value;
            }
        }

        private ITransaction[] _txs;
        /// <summary>
        /// The list of transactions to send
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public ITransaction[] Transactions
        {
            get => _txs;
            set
            {
                if (value is null || value.Length == 0)
                    throw new ArgumentNullException(nameof(Transactions), "Transaction list can not be null or empty.");

                _txs = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.BlockTxn;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            counter.Add(32);
            counter.AddCompactIntCount(Transactions.Length);
            foreach (var item in Transactions)
            {
                item.AddSerializedSize(counter);
            }
        }

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt count = new CompactInt(Transactions.Length);

            stream.Write(BlockHash);
            count.WriteToStream(stream);
            foreach (var item in Transactions)
            {
                item.Serialize(stream);
            }
        }


        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadByteArray(32, out _blkHash))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt count, out Errors err))
            {
                error = err.Convert();
                return false;
            }

            if (count > int.MaxValue)
            {
                error = "Tx count is too big.";
                return false;
            }

            _txs = new Transaction[count];
            for (int i = 0; i < (int)count; i++)
            {
                Transaction temp = new Transaction();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                _txs[i] = temp;
            }

            error = null;
            return true;
        }
    }
}
