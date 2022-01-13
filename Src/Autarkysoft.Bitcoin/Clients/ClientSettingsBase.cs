// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettingsBase"/> with the given parameters.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="netType">Network type</param>
        /// <param name="maxConnection">Maximum number of connections</param>
        /// <param name="nodes">List of peers (can be null)</param>
        /// <param name="servs">Services supported by this node</param>
        public ClientSettingsBase(NetworkType netType, int maxConnection, NodePool nodes, NodeServiceFlags servs)
        {
            Network = netType;
            MaxConnectionCount = maxConnection;
            Services = servs;
            AllNodes = nodes ?? new NodePool(maxConnection);

            DefaultPort = Network switch
            {
                NetworkType.MainNet => Constants.MainNetPort,
                NetworkType.TestNet => Constants.TestNetPort,
                NetworkType.RegTest => Constants.RegTestPort,
                _ => throw new ArgumentException("Undefined network"),
            };

            ListenPort = DefaultPort;

            // TODO: the following values are for testing, they should be set by the caller
            //       they need more checks for correct and optimal values
            BufferLength = 16384; // 16 KB
            int totalBytes = BufferLength * MaxConnectionCount * 2;
            MaxConnectionEnforcer = new Semaphore(MaxConnectionCount, MaxConnectionCount);
            SendReceivePool = new SocketAsyncEventArgsPool(MaxConnectionCount * 2);
            // TODO: can Memory<byte> be used here instead of byte[]?
            byte[] bufferBlock = new byte[totalBytes];
            for (int i = 0; i < MaxConnectionCount * 2; i++)
            {
                var sArg = new SocketAsyncEventArgs();
                sArg.SetBuffer(bufferBlock, i * BufferLength, BufferLength);
                SendReceivePool.Push(sArg);
            }
        }

        /// <inheritdoc/>
        public NodePool AllNodes { get; }
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
        public ushort DefaultPort { get; }
        /// <inheritdoc/>
        public ushort ListenPort { get; set; }

        /// <inheritdoc/>
        public string[] DnsSeeds { get; set; }

        /// <inheritdoc/>
        public int BufferLength { get; }
        /// <inheritdoc/>
        public int MaxConnectionCount { get; }
        /// <inheritdoc/>
        public Semaphore MaxConnectionEnforcer { get; }
        /// <inheritdoc/>
        public SocketAsyncEventArgsPool SendReceivePool { get; }

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
