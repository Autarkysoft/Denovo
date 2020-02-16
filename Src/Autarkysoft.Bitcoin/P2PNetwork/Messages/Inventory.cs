// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// The object to put inside each <see cref="MessagePayloads.InvPayload"/>.
    /// </summary>
    public class Inventory : IDeserializable
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="Inventory"/>.
        /// </summary>
        public Inventory()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Inventory"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="invType"></param>
        /// <param name="hash">32 byte hash</param>
        public Inventory(InventoryType invType, byte[] hash)
        {
            InvType = invType;
            Hash = hash;
        }


        /// <summary>
        /// Type of this inventory
        /// </summary>
        public InventoryType InvType { get; set; }

        private byte[] _hash;
        /// <summary>
        /// A hash, can be block or transaction hash depending on the type
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] Hash
        {
            get => _hash;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Hash), "Hash can not be null.");
                if (value.Length != 32)
                    throw new ArgumentOutOfRangeException(nameof(Hash), "Hash must be 32 bytes.");

                _hash = value;
            }
        }


        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write((uint)InvType);
            stream.Write(Hash);
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadUInt32(out uint val))
            {
                error = Err.EndOfStream;
                return false;
            }
            // Don't be strict about inv. type being valid here, the caller (eg. MessageManager) can decide to reject it.
            InvType = (InventoryType)val;

            if (!stream.TryReadByteArray(32, out _hash))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
