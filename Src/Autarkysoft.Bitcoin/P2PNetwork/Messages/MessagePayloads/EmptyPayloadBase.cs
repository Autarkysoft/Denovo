// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// Base (abstract) class for empty payloads. Overrides the <see cref="Serialize()"/>, <see cref="Serialize(FastStream)"/>
    /// and <see cref="TryDeserialize(FastStreamReader, out string)"/> methods 
    /// and inherits from <see cref="PayloadBase"/>.
    /// </summary>
    public abstract class EmptyPayloadBase : PayloadBase
    {
        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter)
        {
            // Empty payload doesn't have any size
        }

        /// <inheritdoc/>
        public override byte[] Serialize() => Array.Empty<byte>();

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            // Empty payload doesn't write anything to stream
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            error = Errors.None;
            return true;
        }
    }
}
