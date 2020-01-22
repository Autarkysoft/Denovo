// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Text;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class VersionPayload : PayloadBase
    {
        private int _ver;
        /// <summary>
        /// Protocol version
        /// </summary>
        public int Version
        {
            get => _ver;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Version), "Version can not be negative.");

                _ver = value;
            }
        }

        /// <summary>
        /// Services supported by the transmitting node
        /// </summary>
        public NodeServiceFlags Services { get; set; }

        private long _time;
        /// <summary>
        /// The current Unix epoch time according to the transmitting node's clock.
        /// </summary>
        public long Timestamp
        {
            get => _time;
            set => _time = value;
        }

        public NetworkAddress ReceivingNodeNetworkAddress { get; set; }
        public NetworkAddress TransmittingNodeNetworkAddress { get; set; }

        private ulong _nonce;
        /// <summary>
        /// A random nonce which can help a node detect a connection to itself. 
        /// If the nonce is 0, the nonce field is ignored. If the nonce is anything else, a node should terminate 
        /// the connection on receipt of a version message with a nonce it previously sent.
        /// </summary>
        public ulong Nonce
        {
            get => _nonce;
            set => _nonce = value;
        }

        /// <summary>
        /// User agent as defined by <see cref="BIP0014"/> (Can be empty).
        /// </summary>
        public string UserAgent { get; set; }

        private int _sHeight;
        /// <summary>
        /// The height of the transmitting node's best block chain.
        /// </summary>
        public int StartHeight
        {
            get => _sHeight;
            set => _sHeight = value;
        }

        /// <summary>
        /// Transaction relay flag
        /// </summary>
        public bool Relay { get; set; }


        // TODO: investigate what value is best to set here.
        private const int UserAgentMaxSize = 100;


        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.Version;


        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            byte[] UA = Encoding.UTF8.GetBytes(UserAgent);
            CompactInt UASize = new CompactInt(UA.Length);

            stream.Write(Version);
            stream.Write((ulong)Services);
            stream.Write(Timestamp);
            ReceivingNodeNetworkAddress.Serialize(stream);
            TransmittingNodeNetworkAddress.Serialize(stream);
            stream.Write(Nonce);
            UASize.WriteToStream(stream);
            stream.Write(UA);
            stream.Write(StartHeight);
            stream.Write(Relay ? (byte)1 : (byte)0);
        }


        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }


            if (!stream.TryReadInt32(out _ver))
            {
                error = Err.EndOfStream;
                return false;
            }
            // TODO: using version decide which fields should exist (according to protocol version).

            if (!stream.TryReadUInt64(out ulong serv))
            {
                error = Err.EndOfStream;
                return false;
            }
            Services = (NodeServiceFlags)serv;
            if ((Services & NodeServiceFlags.All) != Services)
            {
                error = "Invalid services flags.";
                return false;
            }

            if (!stream.TryReadInt64(out _time))
            {
                error = Err.EndOfStream;
                return false;
            }

            ReceivingNodeNetworkAddress = new NetworkAddress();
            if (!ReceivingNodeNetworkAddress.TryDeserialize(stream, out error))
            {
                return false;
            }

            TransmittingNodeNetworkAddress = new NetworkAddress();
            if (!TransmittingNodeNetworkAddress.TryDeserialize(stream, out error))
            {
                return false;
            }

            if (!stream.TryReadUInt64(out _nonce))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!CompactInt.TryRead(stream, out CompactInt UASize, out error))
            {
                return false;
            }
            if (UASize > UserAgentMaxSize)
            {
                error = "User agent size is too big.";
                return false;
            }

            if (!stream.TryReadByteArray((int)UASize, out byte[] ua))
            {
                error = Err.EndOfStream;
                return false;
            }
            UserAgent = Encoding.UTF8.GetString(ua);

            if (!stream.TryReadInt32(out _sHeight))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!stream.TryReadByte(out byte b))
            {
                Relay = true;
            }
            else if (b == 0)
            {
                Relay = false;
            }
            else if (b == 1)
            {
                Relay = true;
            }
            else
            {
                error = "Relay must be 0 or 1.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
