// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    /// <summary>
    /// A message payload asking the other node to send compact blocks instead (BIP-152).
    /// <para/> Sent: unsolicited
    /// </summary>
    /// <remarks>
    /// https://github.com/bitcoin/bips/blob/master/bip-0152.mediawiki
    /// </remarks>
    public class SendCmpctPayload : PayloadBase
    {
        /// <summary>
        /// Initializes an empty instance of <see cref="SendCmpctPayload"/> used for deserialization.
        /// </summary>
        public SendCmpctPayload()
        {
        }

        /// <summary>
        /// Initializes an empty instance of <see cref="SendCmpctPayload"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="ann">
        /// A boolean indicating whether to reply with <see cref="CmpctBlockPayload"/> (true) or <see cref="InvPayload"/> and
        /// <see cref="HeadersPayload"/> (false).
        /// </param>
        /// <param name="ver">Compact block version (only 1 and 2 are supported)</param>
        public SendCmpctPayload(bool ann, ulong ver)
        {
            Announce = ann;
            CmpctVersion = ver;
        }


        /// <summary>
        /// Size of this instance (bool + ulong)
        /// </summary>
        public const int Size = 9;

        /// <summary>
        /// A boolean indicating whether to reply with <see cref="CmpctBlockPayload"/> (true) or <see cref="InvPayload"/> and
        /// <see cref="HeadersPayload"/> (false).
        /// </summary>
        public bool Announce { get; set; }

        private ulong _cVer;
        /// <summary>
        /// Compact block version (only 1 and 2 are supported)
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public ulong CmpctVersion
        {
            get => _cVer;
            set
            {
                if (value != 1 && value != 2)
                    throw new ArgumentOutOfRangeException(nameof(CmpctVersion), "Only version 1 and 2 are defined.");

                _cVer = value;
            }
        }

        /// <inheritdoc/>
        public override PayloadType PayloadType => PayloadType.SendCmpct;


        /// <inheritdoc/>
        public override void AddSerializedSize(SizeCounter counter) => counter.Add(Size);

        /// <inheritdoc/>
        public override void Serialize(FastStream stream)
        {
            stream.Write(Announce ? (byte)1 : (byte)0);
            stream.Write(CmpctVersion);
        }

        /// <inheritdoc/>
        public override bool TryDeserialize(FastStreamReader stream, out Errors error)
        {
            if (stream is null)
            {
                error = Errors.NullStream;
                return false;
            }

            if (!stream.CheckRemaining(1 + 8))
            {
                error = Errors.EndOfStream;
                return false;
            }

            // Core doesn't seem to be strict here but BIP-152 dicates usage of only 0 and 1 so we are strict
            // https://github.com/bitcoin/bitcoin/blob/45a6811d36fc59ce0d7e2be7a848059a05b0486e/src/serialize.h#L258
            switch (stream.ReadByteChecked())
            {
                case 0:
                    Announce = false;
                    break;
                case 1:
                    Announce = true;
                    break;
                default:
                    error = Errors.MsgSendCmpctInvalidAnn;
                    return false;
            }

            // This has to set the field instead of prop. for forward compatibility, also it can throw an exception
            _cVer = stream.ReadUInt64Checked();

            error = Errors.None;
            return true;
        }
    }
}
