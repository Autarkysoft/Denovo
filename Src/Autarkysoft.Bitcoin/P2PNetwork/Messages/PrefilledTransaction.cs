// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    public class PrefilledTransaction : IDeserializable
    {
        private CompactInt _index;
        public CompactInt Index { get => _index; set => _index = value; }
        public ITransaction Tx { get; set; } = new Transaction();

        /// <inheritdoc/>
        public void AddSerializedSize(SizeCounter counter)
        {
            Index.AddSerializedSize(counter);
            Tx.AddSerializedSize(counter);
        }

        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            Index.WriteToStream(stream);
            Tx.Serialize(stream);
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (!CompactInt.TryRead(stream, out _index, out error))
            {
                return false;
            }

            if (!Tx.TryDeserialize(stream, out error))
            {
                return false;
            }

            error = null;
            return true;
        }
    }
}
