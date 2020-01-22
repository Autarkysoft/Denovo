// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class PingPayload : PayloadBase
    {
        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Ping;

        private ulong _nonce;

        public ulong Nonce
        {
            get => _nonce;
            set => _nonce = value;
        }


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

            if (!stream.TryReadUInt64(out _nonce))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
