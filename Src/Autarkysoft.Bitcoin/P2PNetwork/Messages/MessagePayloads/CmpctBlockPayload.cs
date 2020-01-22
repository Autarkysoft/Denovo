// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class CmpctBlockPayload : PayloadBase
    {
        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.CmpctBlock;

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            throw new NotImplementedException();
        }
    }
}
