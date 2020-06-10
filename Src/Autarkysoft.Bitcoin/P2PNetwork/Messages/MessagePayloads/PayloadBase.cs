// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// Base (abstract) class for all message payloads. Has implemenation for some basic methods shared among different
    /// payloads. Implements <see cref="IMessagePayload"/>.
    /// </summary>
    public abstract class PayloadBase : IMessagePayload
    {
        /// <inheritdoc/>
        public abstract PayloadType PayloadType { get; }

        /// <inheritdoc/>
        public byte[] GetChecksum()
        {
            byte[] data = Serialize();
            if (data.Length == 0)
            {
                return new byte[4] { 0x5d, 0xf6, 0xe0, 0xe2 };
            }
            else
            {
                using Sha256 hash = new Sha256();
                return hash.ComputeChecksum(data);
            }
        }

        /// <summary>
        /// Converts this instance to its byte array representation.
        /// </summary>
        /// <returns>An array of bytes</returns>
        public virtual byte[] Serialize()
        {
            FastStream stream = new FastStream();
            Serialize(stream);
            return stream.ToByteArray();
        }

        /// <inheritdoc/>
        public abstract void Serialize(FastStream stream);

        /// <inheritdoc/>
        public abstract bool TryDeserialize(FastStreamReader stream, out string error);
    }
}
