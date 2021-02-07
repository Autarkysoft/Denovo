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
using System.Threading;
using System.Threading.Tasks;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Implementation of a full verifying node
    /// </summary>
    public class FullClient
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FullClient"/> using the given parameters.
        /// </summary>
        /// <param name="settings">Client settings</param>
        public FullClient(IClientSettings settings)
        {
            Settings = settings;
            connector = new NodeConnector(settings.AllNodes, settings);
            if (settings.AcceptIncomingConnections)
            {
                listener = new NodeListener(settings.AllNodes, settings);
            }

            connector.ConnectFailureEvent += Connector_ConnectFailureEvent;

            Settings.Blockchain.HeaderSyncEndEvent += Blockchain_HeaderSyncEndEvent;
            Settings.Blockchain.BlockSyncEndEvent += Blockchain_BlockSyncEndEvent;
            settings.AllNodes.AddRemoveEvent += AllNodes_AddRemoveEvent;
        }


        private void AllNodes_AddRemoveEvent(object sender, NodePool.AddRemoveEventArgs e)
        {
            if (e.Action == NodePool.CollectionAction.Add)
            {
                if (inQueue > 0)
                {
                    Interlocked.Decrement(ref inQueue);
                }
            }
            else if (e.Action == NodePool.CollectionAction.Remove)
            {
                ConnectToPeers(Settings.MaxConnectionCount - Settings.AllNodes.Count - inQueue);
            }
        }


        private void Connector_ConnectFailureEvent(object sender, IPAddress e)
        {
            Settings.RemoveNodeAddr(e);
            Interlocked.Decrement(ref inQueue);
            ConnectToPeers(Settings.MaxConnectionCount - Settings.AllNodes.Count - inQueue);
        }


        private void Blockchain_HeaderSyncEndEvent(object sender, EventArgs e)
        {
            // Increase the number of connections to max connection and start dowloading blocks
            ConnectToPeers(Settings.MaxConnectionCount - Settings.AllNodes.Count - inQueue);
        }

        private void Blockchain_BlockSyncEndEvent(object sender, EventArgs e)
        {
            if (!(listener is null))
            {
                listener.StartListen(new IPEndPoint(IPAddress.Any, Settings.ListenPort));
            }
        }


        private readonly NodeListener listener;
        private readonly NodeConnector connector;
        private int inQueue;

        private const int DnsDigCount = 3;

        /// <summary>
        /// Settings used in this client
        /// </summary>
        public IClientSettings Settings { get; set; }


        private async Task<IPAddress[]> DigDnsSeeds(bool all)
        {
            List<IPAddress> result = new List<IPAddress>();
            int[] indices;
            if (all)
            {
                indices = Enumerable.Range(0, Settings.DnsSeeds.Length).ToArray();
            }
            else
            {
                indices = Settings.Rng.GetDistinct(0, Settings.DnsSeeds.Length, Math.Min(DnsDigCount, Settings.DnsSeeds.Length));
            }

            foreach (int i in indices)
            {
                try
                {
                    IPAddress[] temp = await Dns.GetHostAddressesAsync(Settings.DnsSeeds[i]);
                    if (!(temp is null))
                    {
                        result.AddRange(temp);
                    }
                }
                catch (Exception) { }
            }

            return result.ToArray();
        }

        private async void ConnectToPeers(int count)
        {
            if (count <= 0)
            {
                return;
            }

            NetworkAddressWithTime[] addrs = Settings.GetRandomNodeAddrs(count, count, false);
            if (!(addrs is null))
            {
                foreach (var item in addrs)
                {
                    Interlocked.Increment(ref inQueue);
                    await Task.Run(() => connector.StartConnect(new IPEndPoint(item.NodeIP, item.NodePort)));
                }
            }

            // Make sure we are connecting to the "count" number of nodes
            count -= (addrs is null) ? 0 : addrs.Length;
            if (count > 0)
            {
                // Dig a small subset of DNS seeds at random.
                IPAddress[] ips = await DigDnsSeeds(false);
                if (ips is null)
                {
                    // If random dig failed, dig all of them.
                    ips = await DigDnsSeeds(true);
                    if (ips is null)
                    {
                        // TODO: we need some sort of message manager or logger to post results, errors,... to
                        return;
                    }
                }

                int[] indices = Settings.Rng.GetDistinct(0, ips.Length, Math.Min(ips.Length, count));
                foreach (var index in indices)
                {
                    Interlocked.Increment(ref inQueue);
                    await Task.Run(() => connector.StartConnect(new IPEndPoint(ips[index], Settings.ListenPort)));
                }
            }
        }

        /// <summary>
        /// Start this client by selecting a random peer's IP address and connecting to it.
        /// </summary>
        public void Start()
        {
            // When client starts, whether the blockchain is not synced at all (0 blocks) or partly synced (some blocks) or
            // fully synced (all blocks to the actual tip), the connection always starts with 1 other node to download
            // a "map" (ie. the block headers) to figure out what the local blockchain status actually is (behind, same
            // or ahead).
            // The message/reply mangers have to handle the sync process and raise an event to add more peers to the pool.
            Settings.Blockchain.State = BlockchainState.HeadersSync;
            ConnectToPeers(1);
        }
    }
}
