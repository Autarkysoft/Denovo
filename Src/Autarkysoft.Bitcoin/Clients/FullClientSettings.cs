// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Autarkysoft.Bitcoin.Clients
{
    /// <summary>
    /// Defines client settings. One instance should be used by the client and passed to all node instance through both
    /// <see cref="NodeListener"/> and <see cref="NodeConnector"/>.
    /// Implements <see cref="IFullClientSettings"/>.
    /// </summary>
    public class FullClientSettings : ClientSettingsBase, IFullClientSettings
    {
        /// <summary>
        /// Default constructor used for tests only
        /// </summary>
        public FullClientSettings() : base(NetworkType.MainNet, 2, null, NodeServiceFlags.NodeNone)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FullClientSettings"/> with the given parameters.
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
        public FullClientSettings(bool listen, NetworkType netType, int maxConnection, NodeServiceFlags servs,
                                  NodePool nodes, IFileManager fileMan, IUtxoDatabase utxoDb, IMemoryPool memPool)
            : base(netType, maxConnection, nodes, servs)
        {
            // TODO: add AcceptSAEAPool here based on listen
            AcceptIncomingConnections = listen;
            FileMan = fileMan ?? throw new ArgumentNullException();

            // TODO: find a better way for this
            supportsIpV6 = NetworkInterface.GetAllNetworkInterfaces().All(x => x.Supports(NetworkInterfaceComponent.IPv6));

            var c = new Consensus(netType);
            var txVer = new TransactionVerifier(false, utxoDb, memPool, c);
            Blockchain = new Chain(FileMan, new BlockVerifier(txVer, c), c, Time, netType);
        }


        private readonly bool supportsIpV6;

        /// <inheritdoc/>
        public IFileManager FileMan { get; }
        /// <inheritdoc/>
        public IChain Blockchain { get; }
        /// <inheritdoc/>
        public IMemoryPool MemPool { get; set; }
        /// <inheritdoc/>
        public ulong MinTxRelayFee { get; set; }
        /// <inheritdoc/>
        public bool AcceptIncomingConnections { get; set; }


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


        private readonly object addrLock = new object();
        private const string NodeAddrs = "NodeAddrs";


        /// <inheritdoc/>
        public int GetRandomNodeAddrs(int count, bool skipCheck, List<NetworkAddressWithTime> result)
        {
            if (count <= 0)
            {
                return 0;
            }

            lock (addrLock)
            {
                byte[] data = FileMan.ReadData(NodeAddrs);
                if (data is null || data.Length == 0 || data.Length % NetworkAddressWithTime.Size != 0)
                {
                    // File doesn't exist or is corrupted
                    return 0;
                }
                else
                {
                    int total = data.Length / NetworkAddressWithTime.Size;
                    // This is like shuffling the entire array itself, but we just have the random index
                    int[] indices = Rng.GetDistinct(0, total, total);

                    var stream = new FastStreamReader(data);
                    int i = 0;
                    int temp = result.Count;
                    while (result.Count < count && i < indices.Length)
                    {
                        stream.ChangePosition(indices[i] * NetworkAddressWithTime.Size);
                        var addr = new NetworkAddressWithTime();
                        if (addr.TryDeserialize(stream, out _))
                        {
                            if ((skipCheck ||
                                !AllNodes.Contains(addr.NodeIP) &&
                                (supportsIpV6 || addr.NodeIP.AddressFamily != AddressFamily.InterNetworkV6) &&
                                HasNeededServices(addr.NodeServices)) &&
                                !result.Contains(addr))
                            {
                                result.Add(addr);
                            }
                        }
                        i++;
                    }

                    return result.Count - temp;
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
                PingReply rep = ping.Send(nodeIP, TimeConstants.MilliSeconds.TenSec);
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

        /// <inheritdoc/>
        public override IReplyManager CreateReplyManager(INodeStatus nodeStatus) => new ReplyManager(nodeStatus, this);
    }
}
