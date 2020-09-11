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
            networkMagic = netType switch
            {
                // https://github.com/bitcoin/bitcoin/blob/b1b173994406158e5faa3c83b113da9d971ac104/src/chainparams.cpp
                // (pchMessageStart)
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
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="payload">Message payload</param>
        /// <param name="netType">Network type</param>
        public Message(IMessagePayload payload, NetworkType netType) : this(netType)
        {
            if (payload is null)
                throw new ArgumentNullException(nameof(payload), "Payload can not be null.");

            byte[] temp = Encoding.ASCII.GetBytes(payload.PayloadType.ToString().ToLower());
            if (temp.Length > CommandNameSize)
            {
                throw new ArgumentOutOfRangeException(nameof(payload.PayloadType),
                                                      $"Payload name can not be longer than {CommandNameSize}.");
            }

            PayloadName = new byte[CommandNameSize];
            Buffer.BlockCopy(temp, 0, PayloadName, 0, temp.Length);

            var stream = new FastStream();
            payload.Serialize(stream);
            PayloadData = stream.ToByteArray();
        }


        private const int CheckSumSize = 4;
        private const int CommandNameSize = 12;

        private readonly byte[] networkMagic;

        /// <summary>
        /// ASCII bytes of the payload type name with null padding to a fixed length of 12.
        /// </summary>
        public byte[] PayloadName { get; private set; } = new byte[CommandNameSize];

        private byte[] _plData = new byte[0];
        /// <summary>
        /// The payload bytes (can be null)
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public byte[] PayloadData
        {
            get => _plData;
            set
            {
                if (value.Length > Constants.MaxPayloadSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(PayloadData),
                              $"Payload length can not be bigger than {Constants.MaxPayloadSize} bytes.");
                }

                _plData = value ?? new byte[0];
            }
        }

        /// <summary>
        /// Returns the <see cref="PayloadType"/> enum value of the <see cref="PayloadName"/> property if possible.
        /// </summary>
        /// <param name="plt">Payload type</param>
        /// <returns>True if the type was available; otherwise false.</returns>
        public bool TryGetPayloadType(out PayloadType plt)
        {
            string name = Encoding.ASCII.GetString(PayloadName.TrimEnd());
            return Enum.TryParse(name, ignoreCase: true, out plt);
        }


        /// <inheritdoc/>
        public void Serialize(FastStream stream)
        {
            stream.Write(networkMagic);
            stream.Write(PayloadName);
            stream.Write(PayloadData.Length);
            stream.Write(CalculateChecksum(PayloadData));
            stream.Write(PayloadData);
        }

        private byte[] CalculateChecksum(byte[] data)
        {
            using Sha256 hash = new Sha256(true);
            return hash.ComputeHash(data).SubArray(0, CheckSumSize);
        }

        /// <summary>
        /// Result returned by <see cref="Read(FastStreamReader)"/> method.
        /// </summary>
        public enum ReadResult
        {
            /// <summary>
            /// Reading message bytes was successful
            /// </summary>
            Success,
            /// <summary>
            /// There was not enough bytes to read (either smaller than fixed header size or smaller based on
            /// the payload size in header)
            /// </summary>
            NotEnoughBytes,
            /// <summary>
            /// The payload size is bigger than <see cref="Constants.MaxPayloadSize"/>
            /// </summary>
            PayloadOverflow,
            /// <summary>
            /// Network magic is different from what this instance expects
            /// </summary>
            InvalidNetwork,
            /// <summary>
            /// Header has an invalid checksum
            /// </summary>
            InvalidChecksum
        }

        /// <summary>
        /// Reads the message from the given stream and returns an enum rerporting the result.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <returns>Result of reading process</returns>
        public ReadResult Read(FastStreamReader stream)
        {
            if (!stream.CheckRemaining(Constants.MessageHeaderSize))
            {
                return ReadResult.NotEnoughBytes;
            }

            // Magic is set in constructor based on network type and should be checked here (instead of setting it)
            if (!((ReadOnlySpan<byte>)networkMagic).SequenceEqual(stream.ReadByteArrayChecked(4)))
            {
                return ReadResult.InvalidNetwork;
            }

            PayloadName = stream.ReadByteArrayChecked(CommandNameSize);

            uint plSize = stream.ReadUInt32Checked();
            if (plSize > Constants.MaxPayloadSize)
            {
                return ReadResult.PayloadOverflow;
            }

            ReadOnlySpan<byte> expectedCS = stream.ReadByteArrayChecked(CheckSumSize);

            if (plSize == 0)
            {
                if (!new ReadOnlySpan<byte>(new byte[4] { 0x5d, 0xf6, 0xe0, 0xe2 }).SequenceEqual(expectedCS))
                {
                    return ReadResult.InvalidChecksum;
                }
            }
            else
            {
                if (!stream.TryReadByteArray((int)plSize, out _plData))
                {
                    return ReadResult.NotEnoughBytes;
                }
                else if (!expectedCS.SequenceEqual(CalculateChecksum(_plData)))
                {
                    return ReadResult.InvalidChecksum;
                }
            }

            return ReadResult.Success;
        }


        /// <inheritdoc/>
        public bool TryDeserialize(FastStreamReader stream, out string error)
        {
            ReadResult res = Read(stream);
            error = res switch
            {
                ReadResult.Success => null,
                ReadResult.NotEnoughBytes => Err.EndOfStream,
                ReadResult.PayloadOverflow => $"Payload size is bigger than allowed size ({Constants.MaxPayloadSize}).",
                ReadResult.InvalidNetwork => "Invalid message magic.",
                ReadResult.InvalidChecksum => "Invalid checksum",
                _ => "Underfined error."
            };

            return res == ReadResult.Success;
        }
    }
}
