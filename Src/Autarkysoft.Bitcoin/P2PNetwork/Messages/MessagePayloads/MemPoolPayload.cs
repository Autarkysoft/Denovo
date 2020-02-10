// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// An empty message payload telling the other node to send hashes of transactions in its memory pool.
    /// The node can initially set bloom filters using a <see cref="PayloadType.FilterAdd"/> message to only receive 
    /// certain transactions.
    /// <para/> Sent: unsolicited
    /// </summary>
    public class MemPoolPayload : EmptyPayloadBase
    {
        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.MemPool;
    }
}
