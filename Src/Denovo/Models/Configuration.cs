﻿// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Denovo.MVVM;
using System;
using System.Reflection;

namespace Denovo.Models
{
    public class Configuration : InpcBase
    {
        // This ctor makes JSON deserialization possible
        public Configuration()
        {
        }

        public Configuration(NetworkType network)
        {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            Network = network;
            SetPeerList();
            UserAgent = new BIP0014("Denovo", ver).ToString(3);
            MaxConnectionCount = 8;
            ListenPort = network switch
            {
                NetworkType.MainNet => Constants.MainNetPort,
                NetworkType.TestNet3 => Constants.TestNet3Port,
                NetworkType.TestNet4 => Constants.TestNet4Port,
                NetworkType.RegTest => Constants.RegTestPort,
                _ => throw new ArgumentException("Undefined network type.")
            };
        }


        public bool IsDefault { get; set; }

        public NetworkType Network { get; set; }

        private ClientType _clientType;
        public ClientType SelectedClientType
        {
            get => _clientType;
            set => SetField(ref _clientType, value);
        }

        private int _prSize;
        public int PrunedSize
        {
            get => _prSize;
            set => SetField(ref _prSize, value);
        }


        private string _bcPath;
        public string BlockchainPath
        {
            get => _bcPath;
            set => SetField(ref _bcPath, value);
        }


        private bool _listen;
        public bool AcceptIncoming
        {
            get => _listen;
            set => SetField(ref _listen, value);
        }

        private ushort _port;
        public ushort ListenPort
        {
            get => _port;
            set => SetField(ref _port, value);
        }

        private bool _relay;
        public bool Relay
        {
            get => _relay;
            set => SetField(ref _relay, value);
        }


        private PeerDiscoveryOption _selDiscoverOpt;
        public PeerDiscoveryOption SelectedPeerDiscoveryOption
        {
            get => _selDiscoverOpt;
            set
            {
                if (SetField(ref _selDiscoverOpt, value))
                {
                    SetPeerList();
                }
            }
        }


        private string GetDnsList()
        {
            return Network switch
            {
                NetworkType.MainNet => string.Join(Environment.NewLine, Constants.GetMainNetDnsSeeds()),
                NetworkType.TestNet3 => string.Join(Environment.NewLine, Constants.GetTestNet3DnsSeeds()),
                NetworkType.TestNet4 => string.Join(Environment.NewLine, Constants.GetTestNet4DnsSeeds()),
                NetworkType.RegTest => "Not defined.",
                _ => "Not defined."
            };
        }
        private void SetPeerList()
        {
            PeerList = SelectedPeerDiscoveryOption switch
            {
                PeerDiscoveryOption.DNS => GetDnsList(),
                PeerDiscoveryOption.CustomIP => "192.168.1.1",
                _ => "Not defined!",
            };
        }

        private string _peers;
        [DependsOnProperty(nameof(SelectedPeerDiscoveryOption))]
        public string PeerList
        {
            get => _peers;
            set => SetField(ref _peers, value);
        }


        private string _ua;
        public string UserAgent
        {
            get => _ua;
            set => SetField(ref _ua, value);
        }

        private int _maxConn;
        public int MaxConnectionCount
        {
            get => _maxConn;
            set => SetField(ref _maxConn, value);
        }
    }
}
