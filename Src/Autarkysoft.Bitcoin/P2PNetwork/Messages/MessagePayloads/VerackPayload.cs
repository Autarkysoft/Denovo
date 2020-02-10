// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// An empty message payload acknowledging that a <see cref="PayloadType.Version"/> message was accepted.
    /// <para/> Sent: in response to <see cref="PayloadType.Version"/>
    /// </summary>
    public class VerackPayload : EmptyPayloadBase
    {
        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Verack;
    }
}
