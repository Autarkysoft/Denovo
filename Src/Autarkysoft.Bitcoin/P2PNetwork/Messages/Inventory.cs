// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    public class Inventory : IDeserializable
    {
        private uint _invT;
        public uint InvType
        {
            get => _invT;
            set => _invT = value;
        }

        private byte[] _hash;
        public byte[] Hash
        {
            get => _hash;
            set => _hash = value;
        }



        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write(InvType);
            stream.Write(Hash);
        }


        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadUInt32(out _invT))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!stream.TryReadByteArray(32, out _hash))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }
    }
}
