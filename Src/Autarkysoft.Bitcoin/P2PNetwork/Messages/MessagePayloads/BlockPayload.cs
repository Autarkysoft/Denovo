// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing a single <see cref="Block"/>.
    /// <para/> Sent: unsolicited or in response to <see cref="PayloadType.GetData"/>
    /// </summary>
    public class BlockPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="BlockPayload"/> used for deserialization.
        /// </summary>
        public BlockPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlockPayload"/> with the given <see cref="Block"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="block">The block to send</param>
        public BlockPayload(IBlock block)
        {
            BlockData = block;
        }


        private IBlock _block = new Block();
        /// <summary>
        /// The block to send
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public IBlock BlockData
        {
            get => _block;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(BlockData), "Block can not be null.");

                _block = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Block;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter) => BlockData.AddSerializedSize(counter);

        /// <inheritdoc/>
        public override void Serialize(FastStream stream) => BlockData.Serialize(stream);

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
                              => BlockData.TryDeserialize(stream, out error);
    }
}
