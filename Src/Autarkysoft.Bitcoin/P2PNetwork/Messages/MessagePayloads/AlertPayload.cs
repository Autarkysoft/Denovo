// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// Alert system is retired and due to lack of examples this class is incomplete.
    /// https://en.bitcoin.it/wiki/Protocol_documentation#alert
    /// </summary>
    public class AlertPayload : PayloadBase
    {
        /// <inheritdoc/>
        public override PayloadType PayloadType =>  PayloadType.Alert;

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
