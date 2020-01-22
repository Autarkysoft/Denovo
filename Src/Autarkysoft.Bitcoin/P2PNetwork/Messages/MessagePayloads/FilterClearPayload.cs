// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// An empty message payload telling the other node to clear the set bloom filter.
    /// <para/> Sent: unsolicited
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bips/blob/master/bip-0037.mediawiki
    /// </remarks>
    public class FilterClearPayload : EmptyPayloadBase
    {
        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.FilterClear;
    }
}
