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
    public class SpvClientSettings : ClientSettingsBase, ISpvClientSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ClientSettingsBase"/> with the given parameters.
        /// </summary>
        /// <param name="netType">Network type</param>
        /// <param name="maxConnection">Maximum number of connections</param>
        /// <param name="nodes">List of peers (can be null)</param>
        /// <param name="fileMan">File manager</param>
        public SpvClientSettings(NetworkType netType, int maxConnection, NodePool nodes, IFileManager fileMan)
            : base(netType, maxConnection, nodes, NodeServiceFlags.NodeNone)
        {
            Relay = false;

            FileMan = fileMan ?? throw new ArgumentNullException();

            var c = new Consensus(netType);
            Blockchain = new Chain(FileMan, new BlockVerifier(null, c), c, Time, netType);
            FileMan = fileMan;

            // TODO: find a better way for this
            supportsIpV6 = NetworkInterface.GetAllNetworkInterfaces().All(x => x.Supports(NetworkInterfaceComponent.IPv6));
        }


        private readonly bool supportsIpV6;
        private readonly object addrLock = new object();
        private const string NodeAddrs = "NodeAddrs";

        /// <inheritdoc/>
        public override IReplyManager CreateReplyManager(INodeStatus nodeStatus) => new SpvReplyManager(nodeStatus, this);


        /// <inheritdoc/>
        public IFileManager FileMan { get; }
        /// <inheritdoc/>
        public IChain Blockchain { get; }


        private const ulong HdrSyncMask = (ulong)(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeNetworkLimited);
        /// <inheritdoc/>
        public bool IsGoodForHeaderSync(NodeServiceFlags flags) => ((ulong)flags & HdrSyncMask) != 0;
        /// <inheritdoc/>
        public bool HasNeededServices(NodeServiceFlags flags)
        {
            return IsGoodForHeaderSync(flags);
        }


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
    }
}
