// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Cryptography;
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
        /// <summary>
        /// A weak RNG to generate nonces for messages.
        /// </summary>
        public IRandomNonceGenerator rng = new RandomNonceGenerator();

        /// <inheritdoc/>
        public Message GetVersionMsg() => GetVersionMsg(new NetworkAddress(settings.Services, IPAddress.Loopback, settings.Port));

        /// <inheritdoc/>
        public Message GetVersionMsg(NetworkAddress recvAddr)
        {
            var ver = new VersionPayload()
            {
                Version = settings.ProtocolVersion,
                Services = settings.Services,
                Timestamp = settings.Time,
                ReceivingNodeNetworkAddress = recvAddr,
                TransmittingNodeNetworkAddress = new NetworkAddress(settings.Services, IPAddress.Loopback, settings.Port),
                Nonce = (ulong)rng.NextInt64(),
                UserAgent = settings.UserAgent,
                StartHeight = blockchain.Height,
                Relay = settings.Relay
            };
            return new Message(ver, settings.Network);
        }

        private bool Deser<T>(byte[] data, out T pl) where T : IMessagePayload, new()
        {
            pl = new T();
            if (pl.TryDeserialize(new FastStreamReader(data), out _))
            {
                return true;
            }
            else
            {
                nodeStatus.AddSmallViolation();
                return false;
            }
        }

        /// <inheritdoc/>
        public Message[] GetReply(Message msg)
        {
            if (!Enum.TryParse(Encoding.ASCII.GetString(msg.PayloadName.TrimEnd()), ignoreCase: true, out PayloadType plt))
            {
                // Undefined payload type
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
                // TODO: write the missing parts inside each "if" to use the deserialized object
                case PayloadType.Addr:
                    if (Deser(msg.PayloadData, out AddrPayload nodeAddresses))
                    {
                        settings.UpdateNodeAddrs(nodeAddresses.Addresses);
                    }
                    break;
                case PayloadType.Alert:
                    // Alert messages are ignored
                    break;
                case PayloadType.Block:
                    if (Deser(msg.PayloadData, out BlockPayload blk))
                    {
                        if (!blockchain.ProcessBlock(blk.BlockData))
                        {
                            nodeStatus.AddMediumViolation();
                        }
                    }
                    break;
                case PayloadType.BlockTxn:
                    if (Deser(msg.PayloadData, out BlockTxnPayload blkTxn))
                    {

                    }
                    break;
                case PayloadType.CmpctBlock:
                    if (Deser(msg.PayloadData, out CmpctBlockPayload cmBlk))
                    {

                    }
                    break;
                case PayloadType.FeeFilter:
                    if (Deser(msg.PayloadData, out FeeFilterPayload feeFilter))
                    {

                    }
                    break;
                case PayloadType.FilterAdd:
                    if (Deser(msg.PayloadData, out FilterAddPayload filterAdd))
                    {

                    }
                    break;
                case PayloadType.FilterClear:
                    // Empty payload
                    // TODO: nodestatus has to clear the set filters here
                    break;
                case PayloadType.FilterLoad:
                    if (Deser(msg.PayloadData, out FilterLoadPayload filterLoad))
                    {

                    }
                    break;
                case PayloadType.GetAddr:
                    // Empty payload
                    NetworkAddressWithTime[] availableAddrs = settings.GetNodeAddrs();
                    if (availableAddrs.Length != 0 && availableAddrs.Length <= Constants.MaxAddrCount)
                    {
                        result = new Message[1] { new Message(new AddrPayload(availableAddrs), settings.Network) };
                    }
                    else if (availableAddrs.Length != 0)
                    {
                        result = new Message[(int)Math.Ceiling((double)availableAddrs.Length / Constants.MaxAddrCount)];
                        int offset = 0;
                        int i = 0;
                        while (offset < availableAddrs.Length)
                        {
                            int rem = availableAddrs.Length - offset;
                            int count = rem > Constants.MaxAddrCount ? Constants.MaxAddrCount : rem;
                            var temp = new NetworkAddressWithTime[count];
                            Array.Copy(availableAddrs, offset, temp, 0, temp.Length);
                            offset += count;
                            result[i++] = new Message(new AddrPayload(temp), settings.Network);
                        }
                    }
                    break;
                case PayloadType.GetBlocks:
                    if (Deser(msg.PayloadData, out GetBlocksPayload getBlks))
                    {

                    }
                    break;
                case PayloadType.GetBlockTxn:
                    if (Deser(msg.PayloadData, out GetBlockTxnPayload getBlkTxn))
                    {

                    }
                    break;
                case PayloadType.GetData:
                    if (Deser(msg.PayloadData, out GetDataPayload getData))
                    {

                    }
                    break;
                case PayloadType.GetHeaders:
                    if (Deser(msg.PayloadData, out GetHeadersPayload getHdrs))
                    {

                    }
                    break;
                case PayloadType.Headers:
                    if (Deser(msg.PayloadData, out HeadersPayload hdrs))
                    {

                    }
                    break;
                case PayloadType.Inv:
                    if (Deser(msg.PayloadData, out InvPayload inv))
                    {

                    }
                    break;
                case PayloadType.MemPool:
                    // Empty payload
                    break;
                case PayloadType.MerkleBlock:
                    if (Deser(msg.PayloadData, out MerkleBlockPayload mrklBlk))
                    {

                    }
                    break;
                case PayloadType.NotFound:
                    if (Deser(msg.PayloadData, out NotFoundPayload notFound))
                    {

                    }
                    break;
                case PayloadType.Ping:
                    if (Deser(msg.PayloadData, out PingPayload ping))
                    {
                        result = new Message[1] { new Message(new PongPayload(ping.Nonce), settings.Network) };
                    }
                    break;
                case PayloadType.Pong:
                    Deser(msg.PayloadData, out PongPayload _);
                    break;
                case PayloadType.Reject:
                    // Reject messages are ignored
                    break;
                case PayloadType.SendCmpct:
                    if (Deser(msg.PayloadData, out SendCmpctPayload sendCmp))
                    {

                    }
                    break;
                case PayloadType.SendHeaders:
                    // Empty payload
                    break;
                case PayloadType.Tx:
                    if (Deser(msg.PayloadData, out TxPayload tx))
                    {

                    }
                    break;
                case PayloadType.Verack:
                    CheckVerack();
                    break;
                case PayloadType.Version:
                    result = CheckVersion(msg);
                    break;
                default:
                    break;
            }

            nodeStatus.UpdateTime();
            return result;
        }


        private void CheckVerack()
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
                    break;
                case HandShakeState.Sent:
                    nodeStatus.HandShake = HandShakeState.SentAndConfirmed;
                    break;
                default:
                    break;
            }
        }

        private Message[] CheckVersion(Message msg)
        {
            var version = new VersionPayload();
            if (!version.TryDeserialize(new FastStreamReader(msg.PayloadData), out _))
            {
                nodeStatus.AddSmallViolation();
                return null;
            }

            Message[] result = null;

            switch (nodeStatus.HandShake)
            {
                case HandShakeState.None:
                    nodeStatus.HandShake = HandShakeState.ReceivedAndReplied;
                    result = new Message[2]
                    {
                        new Message(new VerackPayload(), settings.Network),
                        GetVersionMsg(version.TransmittingNodeNetworkAddress)
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
                    result = new Message[1]
                    {
                        new Message(new VerackPayload(), settings.Network)
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
