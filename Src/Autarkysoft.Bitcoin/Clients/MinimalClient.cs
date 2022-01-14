// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Autarkysoft.Bitcoin.Clients
{
    // TODO: the DNS dig and connection could be turned into a dependency

    /// <summary>
    /// Client with least amount of capabilities only connecting to nodes (handshake) and can send messages manually.
    /// </summary>
    public sealed class MinimalClient : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MinimalClient"/> using the given settings.
        /// </summary>
        /// <param name="settings">Client settings</param>
        public MinimalClient(IMinimalClientSettings settings)
        {
            Settings = settings;
            connector = new NodeConnector(settings);
        }


        private const int DnsDigCount = 3;

        private readonly NodeConnector connector;
        private int inQueue;

        /// <summary>
        /// Settings used in this client
        /// </summary>
        public IMinimalClientSettings Settings { get; set; }

        private async Task<IPAddress[]> DigDnsSeeds(bool all)
        {
            // TODO: this could be used for DNS seeds
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
            if (count > 0 && !isDisposed)
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
                Interlocked.Add(ref inQueue, indices.Length);
                foreach (var index in indices)
                {
                    await Task.Run(() => connector.StartConnect(new IPEndPoint(ips[index], Settings.DefaultPort)));
                }
            }
        }

        /// <summary>
        /// Start this client by selecting a random peer's IP address and connecting to it.
        /// </summary>
        public void Start()
        {
            ConnectToPeers(Settings.MaxConnectionCount);
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
            }
        }
    }
}
