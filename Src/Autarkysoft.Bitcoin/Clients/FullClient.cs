// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Autarkysoft.Bitcoin.Clients
{
    /// <summary>
    /// Implementation of a full verifying node
    /// </summary>
    public sealed class FullClient : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FullClient"/> using the given parameters.
        /// </summary>
        /// <param name="settings">Client settings</param>
        public FullClient(IFullClientSettings settings)
        {
            Settings = settings;
            connector = new NodeConnector(settings);
            if (settings.AcceptIncomingConnections)
            {
                listener = new NodeListener(settings);
            }

            connector.ConnectFailureEvent += Connector_ConnectFailureEvent;

            Settings.Blockchain.HeaderSyncEndEvent += Blockchain_HeaderSyncEndEvent;
            Settings.Blockchain.BlockSyncEndEvent += Blockchain_BlockSyncEndEvent;
            settings.AllNodes.AddRemoveEvent += AllNodes_AddRemoveEvent;
        }


        private void AllNodes_AddRemoveEvent(object sender, NodePool.AddRemoveEventArgs e)
        {
            if (isDisposed)
                return;

            if (e.Action == NodePool.CollectionAction.Add)
            {
                if (inQueue > 0)
                {
                    Interlocked.Decrement(ref inQueue);
                }
            }
            else if (e.Action == NodePool.CollectionAction.Remove)
            {
                if (Settings.Blockchain.State == BlockchainState.HeadersSync)
                {
                    ConnectToPeers(1);
                }
                else
                {
                    ConnectToPeers(Settings.MaxConnectionCount - Settings.AllNodes.Count - inQueue);
                }
            }
        }


        private void Connector_ConnectFailureEvent(object sender, IPAddress e)
        {
            if (isDisposed)
                return;

            Settings.RemoveNodeAddr(e);
            Debug.Assert(inQueue > 0);
            Interlocked.Decrement(ref inQueue);
            if (Settings.Blockchain.State == BlockchainState.HeadersSync)
            {
                ConnectToPeers(1);
            }
            else
            {
                ConnectToPeers(Settings.MaxConnectionCount - Settings.AllNodes.Count - inQueue);
            }
        }


        private void Blockchain_HeaderSyncEndEvent(object sender, EventArgs e)
        {
            if (isDisposed)
                return;

            // Increase the number of connections to max connection and start dowloading blocks
            ConnectToPeers(Settings.MaxConnectionCount - Settings.AllNodes.Count - inQueue);
        }

        private void Blockchain_BlockSyncEndEvent(object sender, EventArgs e)
        {
            if (isDisposed)
                return;

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
        public IFullClientSettings Settings { get; set; }


        private async Task<IPAddress[]> DigDnsSeeds(bool all)
        {
            // // TODO: this could be used for DNS seeds
            // https://github.com/sipa/bitcoin-seeder/blob/a09d2870d1b7f4dd3c1753bbf4fd0bc3690b7ef9/main.cpp#L165-L174
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
            // TODO: should we add a "connectLock" in callers? It could prevent calling this method before inQueue is updated

            if (count <= 0 || isDisposed)
            {
                return;
            }

            List<NetworkAddressWithTime> addrs = new List<NetworkAddressWithTime>(count);

            int added = Settings.GetRandomNodeAddrs(count, false, addrs);
            if (added < count)
            {
                IPAddress[] ips = await DigDnsSeeds(false);
                if (ips is null)
                {
                    // If random dig failed, dig all of them.
                    ips = await DigDnsSeeds(true);
                    if (ips is null && added == 0)
                    {
                        // This could mean we can not connect to the internet
                        // TODO: we need some sort of message manager or logger to post results, errors,... to
                        // TODO: shut down FullClient?
                        return;
                    }
                }

                // This is like shuffling the whole array without changing the array itself:
                int[] indices = Settings.Rng.GetDistinct(0, ips.Length, ips.Length);
                int index = 0;
                while (added < count && index < indices.Length)
                {
                    NetworkAddressWithTime toAdd = new NetworkAddressWithTime()
                    {
                        NodeIP = ips[index++],
                        NodePort = Settings.DefaultPort
                    };

                    if (!addrs.Contains(toAdd))
                    {
                        addrs.Add(toAdd);
                        added++;
                    }
                }
            }

            // inQueue has to be incremented here instead of one at a time otherwise if halfway through the list a connection
            // fails ConnectToPeers() will be called with a wrong count and will create a connection queue in connector.
            Interlocked.Add(ref inQueue, addrs.Count);
            foreach (var item in addrs)
            {
                await Task.Run(() => connector.StartConnect(new IPEndPoint(item.NodeIP, item.NodePort)));
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


        private bool isDisposed = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                Settings.Database.WriteToDisk();
                Settings.AllNodes.Dispose();
                listener?.Dispose();
                connector?.Dispose();
            }
        }
    }
}
