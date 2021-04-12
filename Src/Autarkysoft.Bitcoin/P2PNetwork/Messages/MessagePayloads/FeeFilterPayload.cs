// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

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
        /// <exception cref="ArgumentOutOfRangeException"/>
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
        /// <exception cref="ArgumentOutOfRangeException"/>
        public ulong FeeRate
        {
            get => _feeRate;
            set
            {
                // Fee rate is per kilobyte
                if (value > Constants.TotalSupply * 1000)
                    throw new ArgumentOutOfRangeException(nameof(FeeRate), "Fee rate can not be bigger than total supply.");

                _feeRate = value;
            }
        }

        /// <summary>
        /// Returns fee rate in satoshis per byte (rounds up)
        /// </summary>
        public ulong FeeRatePerByte => (ulong)MathF.Ceiling((float)FeeRate / 1000);

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.FeeFilter;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter) => counter.AddUInt64();

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

            // We reject stupidly high fee rates (no need for *1000 here).
            if (_feeRate >= Constants.TotalSupply)
            {
                error = "Fee rate filter is huge.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
