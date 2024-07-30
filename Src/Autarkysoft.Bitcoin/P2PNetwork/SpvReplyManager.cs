// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Clients;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    public class SpvReplyManager : ReplyManagerBase
    {
        /// <summary>
        /// Initializes a new instanse of <see cref="SpvReplyManager"/> using the given parameters.
        /// </summary>
        /// <param name="ns">Node status</param>
        /// <param name="cs">Client settings</param>
        public SpvReplyManager(INodeStatus ns, ISpvClientSettings cs) : base(ns, cs)
        {
            spvSettings = cs;
        }


        private readonly ISpvClientSettings spvSettings;

        private Message[] GetSettingsMessages(Message extraMsg)
        {
            var result = new List<Message>(7);
            if (!(extraMsg is null))
            {
                result.Add(extraMsg);
            }

            if (!spvSettings.HasNeededServices(nodeStatus.Services))
            {
                nodeStatus.SignalDisconnect();
                return null;
            }

            // We want the other node to respond to our initial settings quickly to check headers.
            // TODO: this may be a bad thing to enfoce on all nodes. Maybe force it based on Blockchain.State
            nodeStatus.StartDisconnectTimer(TimeConstants.MilliSeconds.OneMin);

            //result.Add(new Message(new GetAddrPayload(), settings.Network));

            if (nodeStatus.ProtocolVersion > Constants.P2PBip31ProtVer)
            {
                // We don't bother sending ping to a node that doesn't support nonce in ping/pong messages.
                // This will set default value for latency and this node will be ignored when latency is used later.
                result.Add(GetPingMsg());
            }

            if (nodeStatus.ProtocolVersion >= Constants.P2PBip130ProtVer)
            {
                result.Add(new Message(new SendHeadersPayload(), settings.Network));
            }

            // Always send GetHeaders message during handshake
            result.Add(GetLocatorMessage());

            return result.Count == 0 ? null : result.ToArray();
        }


        private Message GetLocatorMessage()
        {
            BlockHeader[] headers = spvSettings.Blockchain.GetBlockHeaderLocator();
            if (headers.Length > GetHeadersPayload.MaximumHashes)
            {
                // This should never happen but since IBlockchain is a dependency we have to check it here
                // to prevent an exception being thrown.
                BlockHeader[] temp = new BlockHeader[GetHeadersPayload.MaximumHashes];
                Array.Copy(headers, 0, temp, 0, temp.Length);
                headers = temp;
            }
            return new Message(new GetHeadersPayload(settings.ProtocolVersion, headers, null), settings.Network);
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
                case PayloadType.GetHeaders:
                    if (spvSettings.Blockchain.State == BlockchainState.HeadersSync)
                    {
                        nodeStatus.UpdateTime();
                        // If the client is syncing its headers it can't provide headers
                        return null;
                    }

                    if (Deser(msg.PayloadData, out GetHeadersPayload getHdrs))
                    {
                        BlockHeader[] hds = spvSettings.Blockchain.GetMissingHeaders(getHdrs.Hashes, getHdrs.StopHash);
                        if (!(hds is null))
                        {
                            if (hds.Length > HeadersPayload.MaxCount)
                            {
                                // This should never happen but since IBlockchain is a dependency we have to check it here
                                // to prevent an exception being thrown.
                                BlockHeader[] temp = new BlockHeader[HeadersPayload.MaxCount];
                                Array.Copy(hds, 0, temp, 0, temp.Length);
                                hds = temp;
                            }

                            result = new Message[1] { new Message(new HeadersPayload(hds), settings.Network) };
                        }
                    }
                    break;
                case PayloadType.Headers:
                    if (Deser(msg.PayloadData, out HeadersPayload hdrs))
                    {
                        if (hdrs.Headers.Length == 0)
                        {
                            nodeStatus.UpdateTime();
                            // Header locator will always create a request that will fetch at least one header.
                            // Additionally sending an empty header array is a violation on its own.
                            nodeStatus.AddMediumViolation();
                            return null;
                        }

                        if (spvSettings.Blockchain.ProcessHeaders(hdrs.Headers, nodeStatus))
                        {
                            nodeStatus.ReStartDisconnectTimer();
                            result = new Message[1] { GetLocatorMessage() };
                        }
                    }
                    break;
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
                case PayloadType.SendHeaders:
                    // Empty payload
                    if (nodeStatus.SendHeaders)
                    {
                        // It's a violation if the other node "spams" the same settings more than once.
                        nodeStatus.AddSmallViolation();
                    }
                    else
                    {
                        nodeStatus.SendHeaders = true;
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

        /// <inheritdoc/>
        public override Message GetVersionMsg() => GetVersionMsg(new NetworkAddress(0, nodeStatus.IP, nodeStatus.Port), 0);

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
                    return GetSettingsMessages(null);
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
                    result = GetSettingsMessages(new Message(new VerackPayload(), settings.Network));
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
