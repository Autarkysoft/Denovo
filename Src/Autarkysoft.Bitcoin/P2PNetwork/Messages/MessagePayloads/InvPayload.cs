// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing one or more <see cref="Inventory"/> objects.
    /// <para/> Sent: unsolicited
    /// </summary>
    public class InvPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="InvPayload"/>.
        /// </summary>
        public InvPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="InvPayload"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="items">An array of inventory objects</param>
        public InvPayload(Inventory[] items)
        {
            InventoryList = items;
        }

        /// <summary>
        /// Maximum number of inventory items in a payload.
        /// <para/>https://github.com/bitcoin/bitcoin/blob/68196a891056f98c1df0debacf09fb2ea4682a43/src/net_processing.cpp#L73-L74
        /// </summary>
        public const int MaxInvCount = 50_000;

        private Inventory[] _invList;
        /// <summary>
        /// An array of <see cref="Inventory"/> objects
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public Inventory[] InventoryList
        {
            get => _invList;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(InventoryList), "Hash can not be null.");
                if (value.Length > MaxInvCount)
                    throw new ArgumentOutOfRangeException(nameof(InventoryList), $"List can only contain {MaxInvCount} items.");

                _invList = value;
            }
        }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Inv;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            counter.AddCompactIntCount(InventoryList.Length);
            counter.Add(InventoryList.Length * Inventory.Size);
        }

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

            // This works because 50k is smaller than ushort.Max
            if (!stream.TryReadSmallCompactInt(out int count))
            {
                error = "Count is too big or an invalid CompactInt.";
                return false;
            }
            if (count > MaxInvCount)
            {
                error = "Maximum number of allowed inventory was exceeded.";
                return false;
            }

            _invList = new Inventory[count];
            for (int i = 0; i < _invList.Length; i++)
            {
                var temp = new Inventory();
                if (!temp.TryDeserialize(stream, out error))
                {
                    return false;
                }
                _invList[i] = temp;
            }

            error = null;
            return true;
        }
    }
}
