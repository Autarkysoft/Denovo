// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload telling the requesting node that the requested data could not be found.
    /// <para/> Sent: in response to <see cref="PayloadType.GetData"/>
    /// </summary>
    public class NotFoundPayload : InvPayload
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="NotFoundPayload"/>.
        /// </summary>
        public NotFoundPayload() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NotFoundPayload"/> using the given parameters.
        /// </summary>
        /// <inheritdoc/>
        public NotFoundPayload(Inventory[] items) : base(items)
        {
        }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.NotFound;
    }
}
