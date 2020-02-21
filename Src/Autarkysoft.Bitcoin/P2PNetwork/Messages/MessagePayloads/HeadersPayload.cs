// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class HeadersPayload : PayloadBase
    {
        public IBlock[] Headers { get; set; }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Headers;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            CompactInt count = new CompactInt(Headers.Length);

            count.WriteToStream(stream);
            foreach (var hd in Headers)
            {
                hd.SerializeHeader(stream);
                // Block serialization of header doesn't add tx count since there is no need for it.
                // However, in a header payload (for unknown reason) one extra byte indicating zero tx count
                // is added to each block header.
                stream.Write((byte)0);
            }
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt count, out error))
            {
                return false;
            }

            Headers = new Block[count];
            for (int i = 0; i < (int)count; i++)
            {
                Block temp = new Block();
                if (!temp.TryDeserializeHeader(stream, out error))
                {
                    return false;
                }
                Headers[i] = temp;

                if (!stream.TryReadByte(out byte zero))
                {
                    error = Err.EndOfStream;
                    return false;
                }
                if (zero != 0)
                {
                    error = "Transaction count in a headers message must be zero.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
