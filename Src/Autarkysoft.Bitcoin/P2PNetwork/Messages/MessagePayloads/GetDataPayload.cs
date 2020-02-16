// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload requesting one or more <see cref="Inventory"/> objects.
    /// <para/> Sent: unsolicited
    /// </summary>
    public class GetDataPayload : InvPayload
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="GetDataPayload"/>.
        /// </summary>
        public GetDataPayload() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="GetDataPayload"/> using the given parameters.
        /// </summary>
        /// <inheritdoc/>
        public GetDataPayload(Inventory[] items) : base(items)
        {
        }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.GetData;
    }
}
