// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public interface IMessagePayload : IDeserializable
    {
        /// <summary>
        /// Type of this payload
        /// </summary>
        PayloadType PayloadType { get; }

        /// <summary>
        /// Returns 4 byte calculated checksum using double SHA-256 hash of the serialized bytes.
        /// </summary>
        /// <returns>A 4 byte checksum</returns>
        byte[] GetChecksum();
    }
}
