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
    public class SpvClient : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SpvClient"/> using the given parameters.
        /// </summary>
        /// <param name="settings">Client settings</param>
        public SpvClient(ISpvClientSettings settings)
        {
            Settings = settings;
            connector = new NodeConnector(settings);

            connector.ConnectFailureEvent += Connector_ConnectFailureEvent;
            settings.AllNodes.AddRemoveEvent += AllNodes_AddRemoveEvent;
            Settings.Blockchain.HeaderSyncEndEvent += Blockchain_HeaderSyncEndEvent;
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


        private readonly NodeConnector connector;
        private int inQueue;

        private const int DnsDigCount = 3;

        /// <summary>
        /// Settings used in this client
        /// </summary>
        public ISpvClientSettings Settings { get; set; }


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
            Settings.Blockchain.State = BlockchainState.HeadersSync;
            ConnectToPeers(1);
        }


        /// <summary>
        /// Sends the given message to all connected peers.
        /// </summary>
        /// <param name="msg">Message to send</param>
        public void Send(Message msg)
        {
            foreach (var peer in Settings.AllNodes)
            {
                peer.Send(msg);
            }
        }


        private bool isDisposed = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                Settings.AllNodes.Dispose();
                connector?.Dispose();
            }
        }
    }
}
