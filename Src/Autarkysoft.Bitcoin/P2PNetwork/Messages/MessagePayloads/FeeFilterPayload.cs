// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload containing a fee rate in Sathoshi/kB to filter mempool transactions being relayed to this node.
    /// <para/> Sent: unsolicited or in response to <see cref="PayloadType.GetAddr"/>
    /// </summary>
    public class FeeFilterPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="FeeFilterPayload"/> used for deserialization.
        /// </summary>
        public FeeFilterPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FeeFilterPayload"/> with the given network address list.
        /// </summary>
        /// <param name="feeRate">
        /// The fee rate (in Satoshis per kilobyte) below which transactions should not be relayed to this peer.
        /// </param>
        public FeeFilterPayload(ulong feeRate)
        {
            FeeRate = feeRate;
        }



        private ulong _feeRate;
        /// <summary>
        /// The fee rate (in Satoshis per kilobyte) below which transactions should not be relayed to this peer.
        /// </summary>
        public ulong FeeRate
        {
            get => _feeRate;
            set => _feeRate = value;
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType =>  PayloadType.FeeFilter;



        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            stream.Write(FeeRate);
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadUInt64(out _feeRate))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return false;
        }
    }
}
