// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;

namespace Autarkysoft.Bitcoin.Clients
{
    /// <summary>
    /// Base (abstract) class for all client settings.
    /// Implements <see cref="IClientSettings"/>.
    /// </summary>
    public abstract class ClientSettingsBase : IClientSettings
    {
        /// <inheritdoc/>
        public NodePool AllNodes { get; protected set; }
        /// <inheritdoc/>
        public IClientTime Time { get; } = new ClientTime();
        /// <inheritdoc/>
        public IRandomNonceGenerator Rng { get; set; } = new RandomNonceGenerator();

        /// <inheritdoc/>
        public int ProtocolVersion { get; set; } = Constants.P2PProtocolVersion;
        /// <inheritdoc/>
        public bool Relay { get; set; }

        private string _ua =
            new BIP0014("Bitcoin.Net", Assembly.GetExecutingAssembly().GetName().Version, "Bitcoin from scratch").ToString();
        /// <inheritdoc/>
        public string UserAgent
        {
            get => _ua;
            set => _ua = value ?? string.Empty;
        }

        /// <inheritdoc/>
        public NetworkType Network { get; set; }
        /// <inheritdoc/>
        public NodeServiceFlags Services { get; set; }
        /// <inheritdoc/>
        public ushort DefaultPort { get; protected set; }
        /// <inheritdoc/>
        public ushort ListenPort { get; set; }

        /// <inheritdoc/>
        public string[] DnsSeeds { get; set; }

        /// <inheritdoc/>
        public int BufferLength { get; protected set; }
        /// <inheritdoc/>
        public int MaxConnectionCount { get; protected set; }
        /// <inheritdoc/>
        public Semaphore MaxConnectionEnforcer { get; protected set; }
        /// <inheritdoc/>
        public SocketAsyncEventArgsPool SendReceivePool { get; protected set; }

        /// <inheritdoc/>
        public abstract IReplyManager CreateReplyManager(INodeStatus nodeStatus);

        /// <summary>
        /// A list of IP addresses that other peers claimed are ours with the number of times each were received.
        /// </summary>
        public Dictionary<IPAddress, int> localIP = new Dictionary<IPAddress, int>(MaxIpCapacity);
        private readonly object ipLock = new object();
        private const int MaxIpCapacity = 4;

        /// <inheritdoc/>
        public IPAddress GetMyIP()
        {
            lock (ipLock)
            {
                if (localIP.Count > 0)
                {
                    KeyValuePair<IPAddress, int> best = localIP.Aggregate((a, b) => a.Value > b.Value ? a : b);
                    if (best.Value > 3)
                    {
                        // at least 4 nodes have approved this IP
                        return best.Key;
                    }
                }
                return IPAddress.Loopback;
            }
        }

        /// <inheritdoc/>
        public void UpdateMyIP(IPAddress addr)
        {
            lock (ipLock)
            {
                // Prevent the dictionary from becoming too big.
                if (localIP.Count >= MaxIpCapacity)
                {
                    KeyValuePair<IPAddress, int> smallest = localIP.Aggregate((a, b) => a.Value < b.Value ? a : b);
                    localIP.Remove(smallest.Key);
                }

                if (!IPAddress.IsLoopback(addr) && !localIP.TryAdd(addr, 0))
                {
                    localIP[addr]++;
                }
            }
        }
    }
}
