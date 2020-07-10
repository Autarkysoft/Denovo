// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
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
        /// <param name="nt">Network type</param>
        public ReplyManager(INodeStatus ns, NetworkType nt)
        {
            nodeStatus = ns;
            netType = nt;
        }


        private readonly NetworkType netType;
        private readonly INodeStatus nodeStatus;


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
                        if (nodeStatus.HandShake == HandShakeState.None ||
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
                        if (nodeStatus.HandShake == HandShakeState.None)
                        {
                            nodeStatus.HandShake = HandShakeState.ReceivedAndReplied;
                            return new Message[2]
                            {
                                new Message(new VerackPayload(), netType), new Message(new VersionPayload(), netType)
                            };
                        }
                        else if (nodeStatus.HandShake == HandShakeState.Sent)
                        {
                            nodeStatus.HandShake = HandShakeState.SentAndReceived;
                            return new Message[1]
                            {
                                new Message(new VerackPayload(), netType)
                            };
                        }
                        else if (nodeStatus.HandShake == HandShakeState.SentAndConfirmed)
                        {
                            nodeStatus.HandShake = HandShakeState.Finished;
                            return new Message[1]
                            {
                                new Message(new VerackPayload(), netType)
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
                            ping.TryDeserialize(new FastStreamReader(msg.PayloadData), out _);
                            return new Message[1] { new Message(new PongPayload(ping.Nonce), netType) };
                        case PayloadType.Pong:
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

            return null;
        }
    }
}
