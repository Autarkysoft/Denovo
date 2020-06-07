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
        private readonly NetworkType netType;

        /// <inheritdoc/>
        public Message GetReply(Message msg)
        {
            if (Enum.TryParse(Encoding.ASCII.GetString(msg.PayloadName.TrimEnd()), ignoreCase: true, out PayloadType plt))
            {
                switch (plt)
                {
                    case PayloadType.Addr:
                        break;
                    case PayloadType.Alert:
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
                        return new Message(new PongPayload(ping.Nonce), netType);
                    case PayloadType.Pong:
                        break;
                    case PayloadType.Reject:
                        break;
                    case PayloadType.SendCmpct:
                        break;
                    case PayloadType.SendHeaders:
                        break;
                    case PayloadType.Tx:
                        break;
                    case PayloadType.Verack:
                        break;
                    case PayloadType.Version:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // TODO: add violation
            }


            return null;
        }
    }
}
