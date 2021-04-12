// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload sent to confirm the connection to another node is still alive.
    /// <para/> Sent: unsolicited
    /// </summary>
    public class PingPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="PingPayload"/>.
        /// </summary>
        public PingPayload()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PingPayload"/> using the given nonce.
        /// </summary>
        /// <param name="nonce">Random 64-bit integer</param>
        public PingPayload(long nonce)
        {
            Nonce = nonce;
        }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Ping;

        private long _nonce;
        /// <summary>
        /// A random nonce that should be sent back in a <see cref="PongPayload"/>
        /// </summary>
        /// <remarks>Note that sign of the nonce doesn't matter (no point using ulong)</remarks>
        public long Nonce
        {
            get => _nonce;
            set => _nonce = value;
        }


        /// <summary>
        /// Sets the nonce to current unix timestamp
        /// </summary>
        public void SetToCurrentTime()
        {
            Nonce = UnixTimeStamp.GetEpochUtcNow();
        }

        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter) => counter.AddInt64();

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            stream.Write(Nonce);
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadInt64(out _nonce))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
