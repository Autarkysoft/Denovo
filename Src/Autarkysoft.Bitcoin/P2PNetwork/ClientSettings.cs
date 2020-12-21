// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines client settings. One instance should be used by the client and passed to all node instance through both
    /// <see cref="NodeListener"/> and <see cref="NodeConnector"/>.
    /// Implements <see cref="IClientSettings"/>.
    /// </summary>
    public class ClientSettings : IClientSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettings"/> with default parameters.
        /// </summary>
        public ClientSettings()
            : this(Constants.P2PProtocolVersion,
                   true,
                   new BIP0014("Bitcoin.Net", Assembly.GetExecutingAssembly().GetName().Version, "Bitcoin from scratch").ToString(),
                   NetworkType.MainNet,
                   NodeServiceFlags.All)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettings"/> with the given parameters.
        /// </summary>
        /// <param name="pver">Protocol version</param>
        /// <param name="relay">True to relay blocks and transactions; false otherwise</param>
        /// <param name="ua">User agent</param>
        /// <param name="netType">Network type</param>
        /// <param name="servs">Services supported by this node</param>
        public ClientSettings(int pver, bool relay, BIP0014 ua, NetworkType netType, NodeServiceFlags servs)
            : this(pver, relay, ua.ToString(), netType, servs)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettings"/> with the given parameters.
        /// </summary>
        /// <param name="pver">Protocol version</param>
        /// <param name="relay">True to relay blocks and transactions; false otherwise</param>
        /// <param name="ua">User agent as defined by <see cref="BIP0014"/></param>
        /// <param name="netType">Network type</param>
        /// <param name="servs">Services supported by this node</param>
        public ClientSettings(int pver, bool relay, string ua, NetworkType netType, NodeServiceFlags servs)
        {
            ProtocolVersion = pver;
            Relay = relay;
            UserAgent = ua;
            Network = netType;
            Services = servs;

            // TODO: the following values are for testing, they should be set by the caller
            //       they need more checks for correct and optimal values

            MaxConnectionCount = 5;
            BufferLength = 16384; // 16 KB
            int totalBytes = BufferLength * MaxConnectionCount * 2;
            MaxConnectionEnforcer = new Semaphore(MaxConnectionCount, MaxConnectionCount);
            SendReceivePool = new SocketAsyncEventArgsPool(MaxConnectionCount * 2);
            var buffMan = new BufferManager(totalBytes, BufferLength);

            for (int i = 0; i < MaxConnectionCount * 2; i++)
            {
                var sArg = new SocketAsyncEventArgs();
                buffMan.SetBuffer(sArg);
                SendReceivePool.Push(sArg);
            }
        }


        /// <inheritdoc/>
        public IBlockchain Blockchain { get; set; }


        /// <inheritdoc/>
        public IMemoryPool MemPool { get; set; }

        /// <inheritdoc/>
        public IStorage Storage { get; set; }

        /// <inheritdoc/>
        public bool IsCatchingUp { get; set; }
        /// <inheritdoc/>
        public int ProtocolVersion { get; set; }
        /// <inheritdoc/>
        public bool Relay { get; set; }
        /// <inheritdoc/>
        public ulong MinTxRelayFee { get; set; }
        /// <inheritdoc/>
        public string UserAgent { get; set; }
        /// <inheritdoc/>
        public NetworkType Network { get; set; }
        /// <inheritdoc/>
        public NodeServiceFlags Services { get; set; }
        /// <inheritdoc/>
        public long Time => UnixTimeStamp.GetEpochUtcNow();
        /// <inheritdoc/>
        public ushort Port { get; set; }
        /// <inheritdoc/>
        public bool AcceptIncomingConnections { get; set; }
        /// <inheritdoc/>
        public string[] DnsSeeds { get; set; }

        /// <inheritdoc/>
        public int BufferLength { get; }
        /// <inheritdoc/>
        public int MaxConnectionCount { get; set; }
        /// <inheritdoc/>
        public Semaphore MaxConnectionEnforcer { get; }
        /// <inheritdoc/>
        public SocketAsyncEventArgsPool SendReceivePool { get; }

        /// <inheritdoc/>
        public bool AddToMempool(ITransaction tx)
        {
            // TODO: this needs improvement of IMemoryPool first, it needs to return a report, 
            //       invalid tx, existing tx, double spend tx, ... aren't added but some may need adding violation to nodes status
            return true;
        }


        /// <inheritdoc/>
        public NetworkAddressWithTime[] GetNodeAddrs()
        {
            if (!(Storage is null))
            {
                NetworkAddressWithTime[] allAddrs = Storage.ReadAddrs();
                // TODO: this value can change or it could be set by the user. For not it is for testing
                // Maximum number of items to return
                int maxToReturn = 50;
                if (allAddrs.Length <= maxToReturn)
                {
                    return allAddrs;
                }
                else
                {
                    using var rng = new RandomNonceGenerator();
                    int randCount = rng.NextInt32() % maxToReturn;
                    int[] indices = rng.GetDistinct(0, allAddrs.Length, randCount == 0 ? maxToReturn : randCount);
                    NetworkAddressWithTime[] result = new NetworkAddressWithTime[randCount];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = allAddrs[indices[i]];
                    }

                    return result;
                }
            }
            else
            {
                return new NetworkAddressWithTime[0];
            }
        }

        /// <inheritdoc/>
        public void UpdateNodeAddrs(NetworkAddressWithTime[] nodeAddresses)
        {
            if (!(Storage is null))
            {
                Storage.WriteAddrs(nodeAddresses);
            }
        }


        /// <summary>
        /// A list of IP addresses that other peers claimed are ours with the number of times each were received.
        /// </summary>
        public Dictionary<IPAddress, int> localIP = new Dictionary<IPAddress, int>(3);

        /// <inheritdoc/>
        public IPAddress GetMyIP()
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

        /// <inheritdoc/>
        public void UpdateMyIP(IPAddress addr)
        {
            if (!IPAddress.IsLoopback(addr) && !localIP.TryAdd(addr, 0))
            {
                localIP[addr]++;
            }
        }
    }
}
