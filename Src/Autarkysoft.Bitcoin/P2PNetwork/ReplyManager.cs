// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Implementation of a reply manager to handle creation of new <see cref="Message"/>s to return in response to
    /// received <see cref="Message"/>s.
    /// Implements <see cref="IReplyManager"/>.
    /// </summary>
    public class ReplyManager : IReplyManager
    {
        private readonly NetworkType netType;

        /// <inheritdoc/>
        public Message GetReject(PayloadType plt, string error)
        {
            return new Message(netType)
            {
                Payload = new RejectPayload()
                {
                    Code = RejectCode.FailedToDecodeMessage,
                    RejectedMessage = plt,
                    Reason = error
                }
            };
        }

        /// <inheritdoc/>
        public Message GetReply(Message msg)
        {
            if (!(msg.Payload is null))
            {
                // TODO: add all cases
                if (msg.Payload is PingPayload ping)
                {
                    return new Message(new PongPayload(ping.Nonce), netType);
                }
                else if (msg.Payload is VersionPayload)
                {
                    return new Message(new VerackPayload(), netType);
                }
            }

            return null;
        }
    }
}
