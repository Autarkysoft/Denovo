// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;

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
        /// <param name="invType"></param>
        /// <param name="hash">32 byte hash</param>
        public Inventory(InventoryType invType, in Digest256 hash)
        {
            InvType = invType;
            Hash = hash;
        }


        /// <summary>
        /// Size of each inventory object (4 byte type + 32 byte hash)
        /// </summary>
        public const int Size = 36;

        /// <summary>
        /// Type of this inventory
        /// </summary>
        public InventoryType InvType { get; set; }

        /// <summary>
        /// A hash, can be block or transaction hash depending on the type
        /// </summary>
        public Digest256 Hash { get; set; }


        /// <inheritdoc/>
        public void AddSerializedSize(SizeCounter counter) => counter.Add(Size);

        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write((uint)InvType);
            stream.Write(Hash);
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            if (!stream.CheckRemaining(Size))
            {
                error = Errors.EndOfStream;
                return false;
            }

            // Don't be strict about inv. type being valid here, the caller (eg. MessageManager) can decide to reject it.
            InvType = (InventoryType)stream.ReadUInt32Checked();
            Hash = stream.ReadDigest256Checked();

            error = Errors.None;
            return true;
        }
    }
}
