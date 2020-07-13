// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Net;
using System.Text;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Implementation of a reply manager to handle creation of new <see cref="Message"/>s to return in response to
    /// received <see cref="Message"/>s.
    /// Implements <see cref="IReplyManager"/>.
    /// </summary>
    public class ReplyManager : IReplyManager
    {
        /// <summary>
        /// Initializes a new instanse of <see cref="ReplyManager"/> using the given parameters.
        /// </summary>
        /// <param name="ns">Node status</param>
        /// <param name="bc">Blockchain database</param>
        /// <param name="cs">Client settings</param>
        public ReplyManager(INodeStatus ns, IBlockchain bc, IClientSettings cs)
        {
            nodeStatus = ns;
            settings = cs;
            blockchain = bc;
        }


        private readonly INodeStatus nodeStatus;
        private readonly IClientSettings settings;
        private readonly IBlockchain blockchain;

        /// <inheritdoc/>
        public Message GetVersionMsg() => GetVersionMsg(new NetworkAddress(settings.Services, IPAddress.Loopback, settings.Port));

        /// <inheritdoc/>
        public Message GetVersionMsg(NetworkAddress addr)
        {
            byte[] temp = new byte[8];
            new Random().NextBytes(temp);

            var ver = new VersionPayload()
            {
                Version = settings.ProtocolVersion,
                Services = settings.Services,
                Timestamp = settings.Time,
                ReceivingNodeNetworkAddress = addr,
                TransmittingNodeNetworkAddress = new NetworkAddress(settings.Services, IPAddress.Loopback, settings.Port),
                Nonce = BitConverter.ToUInt64(temp),
                UserAgent = settings.UserAgent,
                StartHeight = blockchain.Height,
                Relay = settings.Relay
            };
            return new Message(ver, settings.Network);
        }

        /// <inheritdoc/>
        public Message[] GetReply(Message msg)
        {
            if (!Enum.TryParse(Encoding.ASCII.GetString(msg.PayloadName.TrimEnd()), ignoreCase: true, out PayloadType plt))
            {
                // Undefined payload type
                nodeStatus.AddSmallViolation();
            }
            else
            {
                if (nodeStatus.HandShake != HandShakeState.Finished)
                {
                    if (plt == PayloadType.Verack)
                    {
                        var verack = new VerackPayload();
                        if (!verack.TryDeserialize(new FastStreamReader(msg.PayloadData), out _))
                        {
                            nodeStatus.AddSmallViolation();
                        }
                        else if (nodeStatus.HandShake == HandShakeState.None ||
                                 nodeStatus.HandShake == HandShakeState.Finished ||
                                 nodeStatus.HandShake == HandShakeState.SentAndConfirmed)
                        {
                            nodeStatus.AddMediumViolation();
                        }
                        else if (nodeStatus.HandShake == HandShakeState.ReceivedAndReplied ||
                                 nodeStatus.HandShake == HandShakeState.SentAndReceived)
                        {
                            nodeStatus.HandShake = HandShakeState.Finished;
                        }
                        else if (nodeStatus.HandShake == HandShakeState.Sent)
                        {
                            nodeStatus.HandShake = HandShakeState.SentAndConfirmed;
                        }
                    }
                    else if (plt == PayloadType.Version)
                    {
                        var version = new VersionPayload();
                        if (!version.TryDeserialize(new FastStreamReader(msg.PayloadData), out _))
                        {
                            nodeStatus.AddSmallViolation();
                        }
                        else if (nodeStatus.HandShake == HandShakeState.None)
                        {
                            nodeStatus.HandShake = HandShakeState.ReceivedAndReplied;
                            return new Message[2]
                            {
                                new Message(new VerackPayload(), settings.Network),
                                GetVersionMsg(version.TransmittingNodeNetworkAddress)
                            };
                        }
                        else if (nodeStatus.HandShake == HandShakeState.Sent)
                        {
                            nodeStatus.HandShake = HandShakeState.SentAndReceived;
                            return new Message[1]
                            {
                                new Message(new VerackPayload(), settings.Network)
                            };
                        }
                        else if (nodeStatus.HandShake == HandShakeState.SentAndConfirmed)
                        {
                            nodeStatus.HandShake = HandShakeState.Finished;
                            return new Message[1]
                            {
                                new Message(new VerackPayload(), settings.Network)
                            };
                        }
                        else if (nodeStatus.HandShake == HandShakeState.ReceivedAndReplied ||
                                 nodeStatus.HandShake == HandShakeState.SentAndReceived ||
                                 nodeStatus.HandShake == HandShakeState.Finished)
                        {
                            nodeStatus.AddMediumViolation();
                        }
                    }
                    else
                    {
                        // HandShake is not complete but the other node is sending other types of messages
                        nodeStatus.AddMediumViolation();
                    }
                }
                else
                {
                    switch (plt)
                    {
                        case PayloadType.Addr:
                            break;
                        case PayloadType.Alert:
                            // Alert messages are ignored
                            break;
                        case PayloadType.Block:
                            break;
                        case PayloadType.BlockTxn:
                            break;
                        case PayloadType.CmpctBlock:
                            break;
                        case PayloadType.FeeFilter:
                            break;
                        case PayloadType.FilterAdd:
                            break;
                        case PayloadType.FilterClear:
                            break;
                        case PayloadType.FilterLoad:
                            break;
                        case PayloadType.GetAddr:
                            break;
                        case PayloadType.GetBlocks:
                            break;
                        case PayloadType.GetBlockTxn:
                            break;
                        case PayloadType.GetData:
                            break;
                        case PayloadType.GetHeaders:
                            break;
                        case PayloadType.Headers:
                            break;
                        case PayloadType.Inv:
                            break;
                        case PayloadType.MemPool:
                            break;
                        case PayloadType.MerkleBlock:
                            break;
                        case PayloadType.NotFound:
                            break;
                        case PayloadType.Ping:
                            var ping = new PingPayload();
                            if (!ping.TryDeserialize(new FastStreamReader(msg.PayloadData), out _))
                            {
                                nodeStatus.AddSmallViolation();
                                break;
                            }
                            return new Message[1] { new Message(new PongPayload(ping.Nonce), settings.Network) };
                        case PayloadType.Pong:
                            var pong = new PongPayload();
                            if (!pong.TryDeserialize(new FastStreamReader(msg.PayloadData), out _))
                            {
                                nodeStatus.AddSmallViolation();
                                break;
                            }
                            break;
                        case PayloadType.Reject:
                            // Reject messages are ignored
                            break;
                        case PayloadType.SendCmpct:
                            break;
                        case PayloadType.SendHeaders:
                            break;
                        case PayloadType.Tx:
                            break;
                        case PayloadType.Verack:
                        case PayloadType.Version:
                            nodeStatus.AddMediumViolation();
                            break;
                        default:
                            break;
                    }
                }
            }

            nodeStatus.UpdateTime();
            return null;
        }
    }
}
