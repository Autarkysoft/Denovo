// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Implementation of a minimal reply manager used by minimal clients. This class will only only handle handshake and
    /// ping/pong messages between peers.
    /// Inherits from <see cref="ReplyManagerBase"/>.
    /// </summary>
    public class MinimalReplyManager : ReplyManagerBase
    {
        /// <summary>
        /// Initializes a new instanse of <see cref="MinimalReplyManager"/> using the given parameters.
        /// </summary>
        /// <param name="ns">Node status</param>
        /// <param name="cs">Client settings</param>
        public MinimalReplyManager(INodeStatus ns, IClientSettings cs) : base(ns, cs)
        {
        }


        /// <inheritdoc/>
        public override Message[] GetReply(Message msg)
        {
            if (!msg.TryGetPayloadType(out PayloadType plt))
            {
                // Undefined payload type (this is a violation since other node knows our protocol version)
                nodeStatus.AddSmallViolation();
                nodeStatus.UpdateTime();
                return null;
            }

            if (nodeStatus.HandShake != HandShakeState.Finished && plt != PayloadType.Version && plt != PayloadType.Verack)
            {
                nodeStatus.AddMediumViolation();
                nodeStatus.UpdateTime();
                return null;
            }

            Message[] result = null;

            switch (plt)
            {
                case PayloadType.Ping:
                    if (Deser(msg.PayloadData, out PingPayload ping))
                    {
                        result = new Message[1] { new Message(new PongPayload(ping.Nonce), settings.Network) };
                    }
                    break;
                case PayloadType.Pong:
                    if (Deser(msg.PayloadData, out PongPayload pong))
                    {
                        nodeStatus.CheckPing(pong.Nonce);
                    }
                    break;
                case PayloadType.Verack:
                    result = CheckVerack();
                    break;
                case PayloadType.Version:
                    result = CheckVersion(msg);
                    break;
            }

            nodeStatus.UpdateTime();
            return result;
        }

        private Message[] CheckVerack()
        {
            // VerackPayload doesn't have a body and won't deserialize anything
            // If anything were added to it in the future a TryDeserialize() should be written here

            switch (nodeStatus.HandShake)
            {
                case HandShakeState.None:
                case HandShakeState.SentAndConfirmed:
                case HandShakeState.Finished:
                    nodeStatus.AddMediumViolation();
                    break;
                case HandShakeState.ReceivedAndReplied:
                case HandShakeState.SentAndReceived:
                    nodeStatus.HandShake = HandShakeState.Finished;
                    return new Message[] { GetPingMsg() };
                case HandShakeState.Sent:
                    nodeStatus.HandShake = HandShakeState.SentAndConfirmed;
                    break;
                default:
                    break;
            }

            return null;
        }

        private Message[] CheckVersion(Message msg)
        {
            var version = new VersionPayload();
            if (!version.TryDeserialize(new FastStreamReader(msg.PayloadData), out _))
            {
                nodeStatus.AddSmallViolation();
                return null;
            }

            if (version.Version < Constants.P2PMinProtoVer)
            {
                nodeStatus.SignalDisconnect();
                return null;
            }

            nodeStatus.ProtocolVersion = version.Version;
            nodeStatus.Services = version.Services;
            nodeStatus.Nonce = version.Nonce;
            nodeStatus.UserAgent = version.UserAgent;
            nodeStatus.StartHeight = version.StartHeight;
            nodeStatus.Relay = version.Relay;
            settings.UpdateMyIP(version.ReceivingNodeNetworkAddress.NodeIP);
            settings.Time.UpdateTime(version.Timestamp);

            Message[] result = null;

            switch (nodeStatus.HandShake)
            {
                case HandShakeState.None:
                    nodeStatus.HandShake = HandShakeState.ReceivedAndReplied;
                    result = new Message[2]
                    {
                        new Message(new VerackPayload(), settings.Network),
                        GetVersionMsg()
                    };
                    break;
                case HandShakeState.Sent:
                    nodeStatus.HandShake = HandShakeState.SentAndReceived;
                    result = new Message[1]
                    {
                        new Message(new VerackPayload(), settings.Network)
                    };
                    break;
                case HandShakeState.SentAndConfirmed:
                    nodeStatus.HandShake = HandShakeState.Finished;
                    result = new Message[]
                    {
                        new Message(new VerackPayload(), settings.Network),
                        GetPingMsg()
                    };
                    break;
                case HandShakeState.ReceivedAndReplied:
                case HandShakeState.SentAndReceived:
                case HandShakeState.Finished:
                    nodeStatus.AddMediumViolation();
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
