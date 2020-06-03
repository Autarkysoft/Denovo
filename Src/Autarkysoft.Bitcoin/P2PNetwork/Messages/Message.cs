// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Text;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// P2P network messages used in communication between bitcoin nodes.
    /// Implements <see cref="IDeserializable"/>.
    /// </summary>
    public class Message : IDeserializable
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="Message"/> and sets network magic based on the given
        /// <see cref="NetworkType"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="netType">Network type</param>
        public Message(NetworkType netType)
        {
            Magic = netType switch
            {
                NetworkType.MainNet => new byte[] { 0xf9, 0xbe, 0xb4, 0xd9 },
                NetworkType.TestNet => new byte[] { 0x0b, 0x11, 0x09, 0x07 },
                NetworkType.RegTest => new byte[] { 0xfa, 0xbf, 0xb5, 0xda },
                _ => throw new ArgumentException("Invalid network type.")
            };
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Message"/> with the given <see cref="IMessagePayload"/> 
        /// and sets network magic based on the given <see cref="NetworkType"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="payload">Message payload</param>
        /// <param name="netType">Network type</param>
        public Message(IMessagePayload payload, NetworkType netType) : this(netType)
        {
            Payload = payload;
        }



        /// <summary>
        /// 4 magic + 12 command + 4 payloadSize + 4 checksum + 0 empty payload
        /// </summary>
        public const int MinSize = 24;
        // https://github.com/bitcoin/bitcoin/blob/5879bfa9a541576100d939d329a2639b79d9e4f9/src/net.h#L55-L56
        private const uint MaxPayloadSize = 4 * 1000 * 1000;
        private const int CheckSumSize = 4;
        private const int CommandNameSize = 12;


        private byte[] _magic;
        /// <summary>
        /// Network magi bytes (must be 4 bytes). Use the constructor to set this based on <see cref="NetworkType"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] Magic
        {
            get => _magic;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Magic), "Magic bytes can not be null.");
                if (value.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(Magic), "Magic bytes must be 4 bytes.");

                _magic = value;
            }
        }

        internal uint payloadSize;
        internal byte[] checkSum;

        private IMessagePayload _payload;
        /// <summary>
        /// Message payload
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public IMessagePayload Payload
        {
            get => _payload;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(Payload), "Payload can not be null.");

                _payload = value;
            }
        }


        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            byte[] commandName = Encoding.ASCII.GetBytes(Payload.PayloadType.ToString().ToLower());

            FastStream temp = new FastStream();
            Payload.Serialize(temp);
            byte[] plBa = temp.ToByteArray();

            byte[] checksum = CalculateChecksum(plBa);

            stream.Write(Magic);
            stream.Write(commandName, CommandNameSize);
            stream.Write(plBa.Length);
            stream.Write(checksum);
            stream.Write(plBa);
        }

        public void SerializeHeader(FastStream stream)
        {
            byte[] commandName = Encoding.ASCII.GetBytes(Payload.PayloadType.ToString().ToLower());

            stream.Write(Magic);
            stream.Write(commandName, CommandNameSize);
            stream.Write(payloadSize);
            stream.Write(checkSum);
        }

        public byte[] SerializeHeader()
        {
            FastStream stream = new FastStream(MinSize);
            SerializeHeader(stream);
            return stream.ToByteArray();
        }

        private byte[] CalculateChecksum(byte[] data)
        {
            using Sha256 hash = new Sha256(true);
            return hash.ComputeHash(data).SubArray(0, CheckSumSize);
        }

        public bool VerifyChecksum()
        {
            return !(Payload is null) && checkSum != null && ((ReadOnlySpan<byte>)checkSum).SequenceEqual(Payload.GetChecksum());
        }


        private bool TrySetPayload(PayloadType plt)
        {
            Payload = plt switch
            {
                PayloadType.Addr => new AddrPayload(),
                PayloadType.Block => new BlockPayload(),
                PayloadType.BlockTxn => new BlockTxnPayload(),
                PayloadType.CmpctBlock => new CmpctBlockPayload(),
                PayloadType.FeeFilter => new FeeFilterPayload(),
                PayloadType.FilterAdd => new FilterAddPayload(),
                PayloadType.FilterClear => new FilterClearPayload(),
                PayloadType.FilterLoad => new FilterLoadPayload(),
                PayloadType.GetAddr => new GetAddrPayload(),
                PayloadType.GetBlocks => new GetBlocksPayload(),
                PayloadType.GetBlockTxn => new GetBlockTxnPayload(),
                PayloadType.GetData => new GetDataPayload(),
                PayloadType.GetHeaders => new GetHeadersPayload(),
                PayloadType.Headers => new HeadersPayload(),
                PayloadType.Inv => new InvPayload(),
                PayloadType.MemPool => new MemPoolPayload(),
                PayloadType.MerkleBlock => new MerkleBlockPayload(),
                PayloadType.NotFound => new NotFoundPayload(),
                PayloadType.Ping => new PingPayload(),
                PayloadType.Pong => new PongPayload(),
                PayloadType.Reject => new RejectPayload(),
                PayloadType.SendCmpct => new SendCmpctPayload(),
                PayloadType.SendHeaders => new SendHeadersPayload(),
                PayloadType.Tx => new TxPayload(),
                PayloadType.Verack => new VerackPayload(),
                PayloadType.Version => new VersionPayload(),
                _ => null,
            };

            return Payload != null;
        }


        /// <summary>
        /// Only deserializs header from the given stream. Return value indicates success.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure).</param>
        /// <returns>True if reading was successful, false if otherwise.</returns>
        public bool TryDeserializeHeader(FastStreamReader stream, out string error)
        {
            if (stream is null)
            {
                error = "Stream can not be null.";
                return false;
            }

            if (!stream.TryReadByteArray(4, out byte[] actualMagic))
            {
                error = Err.EndOfStream;
                return false;
            }

            // Magic is set in constructor based on network type and should be checked here (instead of setting it)
            if (!((Span<byte>)actualMagic).SequenceEqual(Magic))
            {
                error = "Invalid message magic.";
                return false;
            }

            if (!stream.TryReadByteArray(CommandNameSize, out byte[] cmd))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (!Enum.TryParse(Encoding.ASCII.GetString(cmd.TrimEnd()), ignoreCase: true, out PayloadType plt))
            {
                error = "Invalid command name.";
                return false;
            }

            if (!TrySetPayload(plt))
            {
                error = "Undefined payload.";
                return false;
            }

            if (!stream.TryReadUInt32(out payloadSize))
            {
                error = Err.EndOfStream;
                return false;
            }

            if (Payload is EmptyPayloadBase)
            {
                if (payloadSize != 0)
                {
                    error = "Payload size for empty payload types must be zero.";
                    return false;
                }
            }
            else if (payloadSize == 0)
            {
                error = "Payload size for none empty payload types can not be zero.";
                return false;
            }


            if (payloadSize > MaxPayloadSize)
            {
                error = $"Payload size is bigger than allowed size ({MaxPayloadSize}).";
                return false;
            }

            if (!stream.TryReadByteArray(CheckSumSize, out checkSum))
            {
                error = Err.EndOfStream;
                return false;
            }

            error = null;
            return true;
        }

        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            if (!TryDeserializeHeader(stream, out error))
            {
                return false;
            }

            int startOfPayLoad = stream.GetCurrentIndex();
            int expectedEnd = startOfPayLoad + (int)payloadSize;

            if (!Payload.TryDeserialize(stream, out error))
            {
                return false;
            }

            byte[] actualChecksum = Payload.GetChecksum();

            if (!((Span<byte>)actualChecksum).SequenceEqual(checkSum))
            {
                error = "Invalid checksum.";
                return false;
            }

            if (stream.GetCurrentIndex() != expectedEnd)
            {
                error = "Invalid payload length in header.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
