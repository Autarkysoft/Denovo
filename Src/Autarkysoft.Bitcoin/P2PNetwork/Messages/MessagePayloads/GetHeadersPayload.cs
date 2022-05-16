// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Cryptography.Hashing;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing the request for block headers.
    /// <para/> Sent: unsolicited
    /// </summary>
    public class GetHeadersPayload : GetBlocksPayload
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="GetHeadersPayload"/> used for deserialization.
        /// </summary>
        public GetHeadersPayload() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GetHeadersPayload"/> using the given parameters.
        /// </summary>
        /// <inheritdoc/>
        public GetHeadersPayload(int ver, Digest256[] headerHashes, Digest256 stopHash) : base(ver, headerHashes, stopHash)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GetHeadersPayload"/> using the given parameters.
        /// </summary>
        /// <inheritdoc/>
        public GetHeadersPayload(int ver, BlockHeader[] headers, BlockHeader? stopHash) : base(ver, headers, stopHash)
        {
        }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.GetHeaders;
    }
}
