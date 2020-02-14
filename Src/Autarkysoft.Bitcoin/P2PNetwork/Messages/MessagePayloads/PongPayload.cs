// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload sent in reply to a ping to confirm the connection is still alive.
    /// <para/> Sent: in response to <see cref="PayloadType.Ping"/>
    /// </summary>
    public class PongPayload : PingPayload
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="PongPayload"/>.
        /// </summary>
        public PongPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PongPayload"/> using the given nonce.
        /// </summary>
        /// <param name="nonce">Random 64-bit integer</param>
        public PongPayload(long nonce) : base(nonce)
        {
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Pong;
    }
}
