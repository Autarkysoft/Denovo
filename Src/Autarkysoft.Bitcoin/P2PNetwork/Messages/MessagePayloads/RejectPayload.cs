// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Text;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    // TODO: reject messages are deprecated in core 0.18.0 may need to make this obsolete here too

    /// <summary>
    /// A message payload sent in reply to any message that is invalid.
    /// <para/> Sent: in response to any message
    /// </summary>
    public class RejectPayload : PayloadBase
    {
        private PayloadType _rejMsgType;
        /// <summary>
        /// The type of the received message that is being rejected
        /// </summary>
        public PayloadType RejectedMessage
        {
            get => _rejMsgType;
            set => _rejMsgType = value;
        }

        /// <summary>
        /// One byte indicating the reason for rejection
        /// </summary>
        public RejectCode Code { get; set; }

        private byte[] _reason;
        /// <summary>
        /// An additional string explaining the reason for rejection
        /// </summary>
        public string Reason
        {
            get => Encoding.ASCII.GetString(_reason);
            set => _reason = string.IsNullOrEmpty(value) ? new byte[0] : Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// An additioanl optional data
        /// </summary>
        public byte[] ExtraData { get; set; }


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Reject;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            byte[] rejMsg = Encoding.ASCII.GetBytes(RejectedMessage.ToString().ToLower());
            CompactInt rejMsgLen = new CompactInt(rejMsg.Length);
            byte[] rsn = Encoding.ASCII.GetBytes(Reason);
            CompactInt rsnLen = new CompactInt(rsn.Length);

            rejMsgLen.WriteToStream(stream);
            stream.Write(rejMsg);
            stream.Write((byte)Code);
            rsnLen.WriteToStream(stream);
            stream.Write(rsn);
            if (ExtraData != null)
            {
                stream.Write(ExtraData);
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

            if (!CompactInt.TryRead(stream, out CompactInt msgLen, out error))
            {
                return false;
            }
            if (msgLen > 12)
            {
                error = "Rejected message type name was too long.";
                return false;
            }

            if (!stream.TryReadByteArray((int)msgLen, out byte[] cmd))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!Enum.TryParse(Encoding.ASCII.GetString(cmd), true, out _rejMsgType))
            {
                error = "Invalid rejected message type.";
                return false;
            }

            if (!stream.TryReadByte(out byte b))
            {
                error = Err.EndOfStream;
                return false;
            }
            if (!Enum.IsDefined(typeof(RejectCode), b))
            {
                error = "Undefined rejected message code.";
                return false;
            }
            Code = (RejectCode)b;

            if (!CompactInt.TryRead(stream, out CompactInt rsnLen, out error))
            {
                return false;
            }

            if (!stream.TryReadByteArray((int)rsnLen, out _reason))
            {
                error = Err.EndOfStream;
                return false;
            }

            // TODO: add read for extradata

            error = null;
            return true;
        }
    }
}
