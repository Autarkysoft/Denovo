// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class SendHeadersPayload : EmptyPayloadBase
    {
        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.SendHeaders;
    }
}
