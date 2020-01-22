// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class SendCmpctPayload : PayloadBase
    {
        public bool Announce { get; set; }

        private ulong _cVer;
        public ulong CmpctVersion
        {
            get => _cVer;
            set => _cVer = value;
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.SendCmpct;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            stream.Write(Announce ? (byte)1 : (byte)0);
            stream.Write(CmpctVersion);
        }


        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadByte(out byte b))
            {
                error = Err.EndOfStream;
                return false;
            }
            switch (b)
            {
                case 0:
                    Announce = false;
                    break;
                case 1:
                    Announce = true;
                    break;
                default:
                    error = "First byte representing announce bool should be 0 or 1.";
                    return false;
            }

            if (!stream.TryReadUInt64(out _cVer))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
