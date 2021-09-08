// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        /// 
        /// </summary>
        public ClientSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettings"/> with the given parameters.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="listen">True to open a listening socket; false otherwise</param>
        /// <param name="netType">Network type</param>
        /// <param name="servs">Services supported by this node</param>
        /// <param name="nodes">List of peers (can be null)</param>
        /// <param name="fileMan">File manager</param>
        /// <param name="utxoDb">UTXO database</param>
        /// <param name="memPool">Memory pool</param>
        /// <param name="maxConnection">Maximum number of connections</param>
        public ClientSettings(bool listen, NetworkType netType, int maxConnection, NodeServiceFlags servs,
                              NodePool nodes, IFileManager fileMan, IUtxoDatabase utxoDb, IMemoryPool memPool)
        {
            // TODO: add AcceptSAEAPool here based on listen
            AcceptIncomingConnections = listen;
            Network = netType;
            MaxConnectionCount = maxConnection;
            Services = servs;
            AllNodes = nodes ?? new NodePool(maxConnection);
            FileMan = fileMan ?? throw new ArgumentNullException();

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

            // TODO: find a better way for this
            supportsIpV6 = NetworkInterface.GetAllNetworkInterfaces().All(x => x.Supports(NetworkInterfaceComponent.IPv6));

            var c = new Consensus(netType);
            var txVer = new TransactionVerifier(false, utxoDb, memPool, c);
            Blockchain = new Blockchain.Blockchain(FileMan, new BlockVerifier(txVer, c), c)
            {
                Time = Time,
                State = BlockchainState.None
            };
        }


        private readonly bool supportsIpV6;

        /// <inheritdoc/>
        public NodePool AllNodes { get; }

        /// <inheritdoc/>
        public IClientTime Time { get; } = new ClientTime();

        /// <inheritdoc/>
        public IFileManager FileMan { get; }

        /// <inheritdoc/>
        public IBlockchain Blockchain { get; }

        /// <inheritdoc/>
        public IMemoryPool MemPool { get; set; }

        /// <inheritdoc/>
        public IRandomNonceGenerator Rng { get; set; } = new RandomNonceGenerator();

        /// <inheritdoc/>
        public int ProtocolVersion { get; set; } = Constants.P2PProtocolVersion;
        /// <inheritdoc/>
        public bool Relay { get; set; }
        /// <inheritdoc/>
        public ulong MinTxRelayFee { get; set; }

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
        public bool AcceptIncomingConnections { get; set; }
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


        private const ulong HdrSyncMask = (ulong)(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeNetworkLimited);
        private const ulong BlkSyncMask = (ulong)(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeWitness);

        /// <inheritdoc/>
        public bool HasNeededServices(NodeServiceFlags flags)
        {
            if (Blockchain.State == BlockchainState.HeadersSync)
            {
                return IsGoodForHeaderSync(flags);
            }
            else if (Blockchain.State == BlockchainState.BlocksSync)
            {
                return IsGoodForBlockSync(flags);
            }

            // TODO: additional conditions can be added here
            return true;
        }
        /// <inheritdoc/>
        public bool IsGoodForHeaderSync(NodeServiceFlags flags) => ((ulong)flags & HdrSyncMask) != 0;
        /// <inheritdoc/>
        public bool IsGoodForBlockSync(NodeServiceFlags flags) => ((ulong)flags & BlkSyncMask) == BlkSyncMask && !IsPruned(flags);
        /// <inheritdoc/>
        public bool IsPruned(NodeServiceFlags flags) => flags.HasFlag(NodeServiceFlags.NodeNetworkLimited) &&
                                                        !flags.HasFlag(NodeServiceFlags.NodeNetwork);

        /// <inheritdoc/>
        public bool AddToMempool(ITransaction tx)
        {
            // TODO: this needs improvement of IMemoryPool first, it needs to return a report, 
            //       invalid tx, existing tx, double spend tx, ... aren't added but some may need adding violation to nodes status
            return true;
        }


        private readonly object addrLock = new object();
        private const string NodeAddrs = "NodeAddrs";


        /// <inheritdoc/>
        public NetworkAddressWithTime[] GetRandomNodeAddrs(int count, bool skipCheck)
        {
            if (count <= 0)
            {
                return null;
            }

            lock (addrLock)
            {
                byte[] data = FileMan.ReadData(NodeAddrs);
                if (data is null || data.Length == 0 || data.Length % NetworkAddressWithTime.Size != 0)
                {
                    // File doesn't exist or is corrupted
                    return null;
                }
                else
                {
                    int total = data.Length / NetworkAddressWithTime.Size;
                    // This is like shuffling the entire array itself, but we just have the random index
                    int[] indices = Rng.GetDistinct(0, total, total);

                    var result = new List<NetworkAddressWithTime>(count);
                    var stream = new FastStreamReader(data);
                    int i = 0;
                    while (result.Count < count && i < indices.Length)
                    {
                        stream.ChangePosition(indices[i] * NetworkAddressWithTime.Size);
                        var addr = new NetworkAddressWithTime();
                        if (addr.TryDeserialize(stream, out _))
                        {
                            if (skipCheck ||
                                !AllNodes.Contains(addr.NodeIP) &&
                                (supportsIpV6 || addr.NodeIP.AddressFamily != AddressFamily.InterNetworkV6) &&
                                HasNeededServices(addr.NodeServices))
                            {
                                result.Add(addr);
                            }
                        }
                        i++;
                    }

                    return result.ToArray();
                }
            }
        }


        /// <inheritdoc/>
        public void RemoveNodeAddr(IPAddress ip)
        {
            lock (addrLock)
            {
                byte[] data = FileMan.ReadData(NodeAddrs);
                if (!(data is null) && data.Length % NetworkAddressWithTime.Size == 0)
                {
                    int total = data.Length / NetworkAddressWithTime.Size;
                    var reader = new FastStreamReader(data);
                    for (int i = 0; i < total; i++)
                    {
                        var addr = new NetworkAddressWithTime();
                        if (addr.TryDeserialize(reader, out _) && addr.NodeIP.Equals(ip))
                        {
                            byte[] result = new byte[data.Length - NetworkAddressWithTime.Size];
                            int startPos = i * NetworkAddressWithTime.Size;
                            int EndPos = startPos + NetworkAddressWithTime.Size;
                            Buffer.BlockCopy(data, 0, result, 0, startPos);
                            Buffer.BlockCopy(data, EndPos, result, startPos, data.Length - EndPos);

                            FileMan.WriteData(result, NodeAddrs);
                            break;
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void UpdateNodeAddrs(NetworkAddressWithTime[] nodeAddresses)
        {
            lock (addrLock)
            {
                byte[] data = FileMan.ReadData(NodeAddrs);
                if (data is null || data.Length % NetworkAddressWithTime.Size != 0)
                {
                    // File doesn't exist or is corrupted
                    var stream = new FastStream(nodeAddresses.Length * NetworkAddressWithTime.Size);
                    foreach (var item in nodeAddresses)
                    {
                        item.Serialize(stream);
                    }
                    FileMan.WriteData(stream.ToByteArray(), NodeAddrs);
                }
                else
                {
                    int total = data.Length / NetworkAddressWithTime.Size;
                    var reader = new FastStreamReader(data);
                    var toSkip = new List<int>(nodeAddresses.Length);
                    for (int i = 0; i < total; i++)
                    {
                        var addr = new NetworkAddressWithTime();
                        if (addr.TryDeserialize(reader, out _))
                        {
                            int index = Array.IndexOf(nodeAddresses, addr);
                            if (index >= 0)
                            {
                                toSkip.Add(index);
                            }
                        }
                    }

                    var stream = new FastStream((nodeAddresses.Length - toSkip.Count) * NetworkAddressWithTime.Size);
                    for (int i = 0; i < nodeAddresses.Length; i++)
                    {
                        if (!toSkip.Contains(i) && PingIp(nodeAddresses[i].NodeIP))
                        {
                            nodeAddresses[i].Serialize(stream);
                        }
                    }

                    if (stream.GetSize() > 0)
                    {
                        FileMan.AppendData(stream.ToByteArray(), NodeAddrs);
                    }
                }
            }
        }

        private bool PingIp(IPAddress nodeIP)
        {
            if (!supportsIpV6 && nodeIP.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return false;
            }

            var ping = new Ping();
            try
            {
                PingReply rep = ping.Send(nodeIP, TimeConstants.TenSeconds_Milliseconds);
                return rep.Status == IPStatus.Success;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                ping.Dispose();
            }
        }


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
