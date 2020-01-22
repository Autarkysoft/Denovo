// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class InvPayload : PayloadBase
    {
        private const int MaxInvCount = 50_000;

        public Inventory[] InventoryList { get; set; }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Inv;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt count = new CompactInt(InventoryList.Length);

            count.WriteToStream(stream);
            foreach (var item in InventoryList)
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

            if (!CompactInt.TryRead(stream, out CompactInt count, out error))
            {
                return false;
            }
            if (count > MaxInvCount)
            {
                error = "Maximum number of allowed inventory was exceeded.";
                return false;
            }

            InventoryList = new Inventory[(int)count];
            for (int i = 0; i < InventoryList.Length; i++)
            {
                Inventory temp = new Inventory();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                InventoryList[i] = temp;
            }

            error = null;
            return true;
        }

    }
}
