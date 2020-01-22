using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class GetBlockTxnPayload : PayloadBase
    {
        private byte[] _blkHash;
        public byte[] BlockHash
        {
            get => _blkHash;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(BlockHash), "Block hash can not be null or empty.");
                if (value.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(BlockHash), "Block hash must be 32 bytes.");

                _blkHash = value;
            }
        }

        private Transaction[] _txs;
        public Transaction[] TransactionList
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
        public override PayloadType PayloadType => PayloadType.GetBlockTxn;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt txCount = new CompactInt(TransactionList.Length);

            stream.Write(BlockHash);
            txCount.WriteToStream(stream);
            foreach (var tx in TransactionList)
            {
                tx.Serialize(stream);
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

            if (!CompactInt.TryRead(stream, out CompactInt txCount, out error))
            {
                return false;
            }
            if (txCount > int.MaxValue)
            {
                error = "Tx count is too big.";
                return false;
            }

            TransactionList = new Transaction[(int)txCount];
            for (int i = 0; i < (int)txCount; i++)
            {
                Transaction temp = new Transaction();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                TransactionList[i] = temp;
            }

            error = null;
            return true;
        }
    }
}
