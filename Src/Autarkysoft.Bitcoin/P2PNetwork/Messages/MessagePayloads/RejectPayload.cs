// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Text;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class RejectPayload : PayloadBase
    {
        private PayloadType _rejMsgType;

        public PayloadType RejectedMessage 
        { 
            get => _rejMsgType;
            set => _rejMsgType = value; 
        }

        public RejectCode Code { get; set; }
        
        private byte[] _reason; 
        
        public string Reason
        {
            get => Encoding.UTF8.GetString(_reason);
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Reason), "Reason can not be null or empty.");

                _reason = Encoding.UTF8.GetBytes(value);
            }
        }

        
        public byte[] ExtraData { get; set; }


        /// <summary>
        /// 1 + 2 (min message type) + 1 (code) + 0 + 0 + 0
        /// </summary>
        public const int MinSize = 3;

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
