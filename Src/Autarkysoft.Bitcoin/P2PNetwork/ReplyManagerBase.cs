// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Clients;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System.Diagnostics;
using System.Net;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Base (abstract) class for all ReplyManger classes.
    /// Implements <see cref="IReplyManager"/>.
    /// </summary>
    public abstract class ReplyManagerBase : IReplyManager
    {
        /// <summary>
        /// Sets the readonly properties in <see cref="ReplyManagerBase"/> 
        /// </summary>
        /// <param name="ns">Node status</param>
        /// <param name="cs">Client settings</param>
        public ReplyManagerBase(INodeStatus ns, IClientSettings cs)
        {
            nodeStatus = ns;
            settings = cs;
        }


        /// <summary>
        /// Node status
        /// </summary>
        protected readonly INodeStatus nodeStatus;
        /// <summary>
        /// Client settings
        /// </summary>
        protected readonly IClientSettings settings;

        /// <inheritdoc/>
        public Message GetPingMsg()
        {
            long nonce = settings.Rng.NextInt64();
            // TODO: latency may have a small error this way (maybe the following line should be moved to Node class)
            // Chances of nonce being repeated is 1 in 2^64 which is why the returned bool is ignored here
            nodeStatus.StorePing(nonce);
            return new Message(new PingPayload(nonce), settings.Network);
        }

        /// <inheritdoc/>
        // Node constructor sets the IP and port on INodeStatus
        // TODO: this is bitcoin-core's behavior, it can be changed if needed
        public abstract Message GetVersionMsg();

        /// <summary>
        /// Creates a new version message using the given network address.
        /// </summary>
        /// <param name="recvAddr">Network address to use</param>
        /// <param name="height">Best block height</param>
        /// <returns>A new version message</returns>
        public Message GetVersionMsg(NetworkAddress recvAddr, int height)
        {
            var ver = new VersionPayload()
            {
                Version = settings.ProtocolVersion,
                Services = settings.Services,
                Timestamp = settings.Time.Now,
                ReceivingNodeNetworkAddress = recvAddr,
                // TODO: IP and port zero are bitcoin-core's behavior, it can be changed if needed
                TransmittingNodeNetworkAddress = new NetworkAddress(settings.Services, IPAddress.IPv6Any, 0),
                Nonce = (ulong)settings.Rng.NextInt64(),
                UserAgent = settings.UserAgent,
                StartHeight = height,
                Relay = settings.Relay
            };
            return new Message(ver, settings.Network);
        }

        /// <summary>
        /// Instantiates a new instance of message payload and deserializes the given data using that instance.
        /// Return value indicates success.
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        /// <param name="data">Data to deserialize</param>
        /// <param name="pl">Instantiated payload</param>
        /// <returns>True if deserialization was successful; otherwise false.</returns>
        protected bool Deser<T>(byte[] data, out T pl) where T : IMessagePayload, new()
        {
            pl = new T();
            if (pl.TryDeserialize(new FastStreamReader(data), out string error))
            {
                Debug.Assert(error is null);
                return true;
            }
            else
            {
                nodeStatus.AddSmallViolation();
                return false;
            }
        }

        /// <inheritdoc/>
        public abstract Message[] GetReply(Message msg);
    }
}
