﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

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
        public override byte[] Serialize() => new byte[0];

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            // Empty payload doesn't write anything to stream
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }
            else
            {
                error = null;
                return true;
            }
        }
    }
}