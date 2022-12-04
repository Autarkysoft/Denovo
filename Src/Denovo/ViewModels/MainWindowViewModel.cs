// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Clients;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Denovo.Models;
using Denovo.MVVM;
using Denovo.Services;
using System;
using System.Reflection;

namespace Denovo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // TODO: idea:
        //       Add a BanManager which would receive its rules from user (in config window) and can ban nodes
        //       For example a node that was malicious before and its IP is stored locally.
        //          a node that has a certain flag set and is causing problems (https://github.com/bitcoin/bitcoin/pull/10982/files)
        //          a node with a certain user agent, version, ...

        /// <summary>
        /// This constructor is here to make designer happy and can be used for testing but nothing else.
        /// </summary>
        public MainWindowViewModel()
        {
            IsInitialized = true;
        }

        public MainWindowViewModel(NetworkType network)
        {
            Network = network;
            WinMan = new WindowManager();
            DisconnectCommand = new BindableCommand(Disconnect, CanDisconnect);

            FileMan = new FileManager(network);
            Configuration config = FileMan.ReadConfig();
            if (config is not null && !config.IsDefault)
            {
                IsInitialized = true;
                Init(config);
            }
            else
            {
                IsInitialized = false;
            }
        }


        public NetworkType Network { get; }
        public NodePool AllNodes { get; set; }

        public IWindowManager WinMan { get; set; }
        public IDenovoFileManager FileMan { get; set; }
        public bool IsInitialized { get; }

        private IFullClientSettings clientSettings;


        private void Init(Configuration config)
        {
            FileMan.SetBlockPath(config.BlockchainPath);

            AllNodes = new NodePool(config.MaxConnectionCount);
            UtxoDatabase utxoDb = new(FileMan);
            MemoryPool memPool = new();
            clientSettings =
                new FullClientSettings(
                    config.AcceptIncoming,
                    config.Network,
                    config.MaxConnectionCount,
                    NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeWitness,
                    AllNodes,
                    FileMan,
                    utxoDb,
                    memPool)
                {
                    Relay = config.Relay,
                    UserAgent = config.UserAgent,
                    DnsSeeds = config.PeerList.Split(Environment.NewLine),
                    ListenPort = config.ListenPort
                };

            clientSettings.Time.WrongClockEvent += Time_WrongClockEvent;

            MyInfo = $"My node information:{Environment.NewLine}" +
                     $"Network: {config.Network}{Environment.NewLine}" +
                     $"User agent: {config.UserAgent}{Environment.NewLine}" +
                     $"Protocol version: {clientSettings.ProtocolVersion}{Environment.NewLine}" +
                     $"Max connection count: {clientSettings.MaxConnectionCount}{Environment.NewLine}" +
                     $"Best block height: {clientSettings.Blockchain.Height}{Environment.NewLine}";
        }


        private void Time_WrongClockEvent(object sender, EventArgs e)
        {
            Result = "The computer's clock is possibly wrong.";
        }


        public async void OpenConfig()
        {
            Configuration config = FileMan.ReadConfig();
            if (config is null)
            {
                config = new Configuration(Network) { IsDefault = true };
            }

            ConfigurationViewModel vm = new(config);
            await WinMan.ShowDialog(vm);

            if (vm.IsChanged)
            {
                FileMan.WriteConfig(config);
            }

            if (!IsInitialized && !config.IsDefault)
            {
                Init(config);
            }
        }

        public async void OpenMiner()
        {
            // TODO: A better way is to make the following VM disposable
            MinerViewModel vm = new();
            await WinMan.ShowDialog(vm);
            vm.StopMining();
        }

        public async void OpenEcies()
        {
            EciesViewModel vm = new();
            await WinMan.ShowDialog(vm);
        }

        public async void OpenVerifyTx()
        {
            VerifyTxViewModel vm = new();
            await WinMan.ShowDialog(vm);
        }

        public async void OpenWifHelper()
        {
            WifHelperViewModel vm = new();
            await WinMan.ShowDialog(vm);
        }

        public async void OpenPushTx()
        {
            PushTxViewModel vm = new();
            await WinMan.ShowDialog(vm);
            vm.Dispose();
        }

        public async void OpenAbout()
        {
            AboutViewModel vm = new();
            await WinMan.ShowDialog(vm);
        }

        private Node _selNode;
        public Node SelectedNode
        {
            get => _selNode;
            set
            {
                if (SetField(ref _selNode, value))
                {
                    DisconnectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string MyInfo { get; private set; }

        [DependsOnProperty(nameof(SelectedNode))]
        public string PeerInfo => SelectedNode is null ?
            "Select a node from the list to see its information." :
            $"UA: {SelectedNode.NodeStatus.UserAgent}{Environment.NewLine}" +
            $"IP: {SelectedNode.NodeStatus.IP}{Environment.NewLine}" +
            $"Port: {SelectedNode.NodeStatus.Port}{Environment.NewLine}" +
            $"Prot. Ver.: {SelectedNode.NodeStatus.ProtocolVersion}{Environment.NewLine}" +
            $"Handshake: {SelectedNode.NodeStatus.HandShake}{Environment.NewLine}" +
            $"Last seen: {SelectedNode.NodeStatus.LastSeen}{Environment.NewLine}" +
            $"Height: {SelectedNode.NodeStatus.StartHeight}{Environment.NewLine}" +
            $"Services: {SelectedNode.NodeStatus.Services}{Environment.NewLine}" +
            $"IsDead: {SelectedNode.NodeStatus.IsDisconnected}{Environment.NewLine}" +
            $"Relay: {SelectedNode.NodeStatus.Relay}{Environment.NewLine}" +
            $"Send Cmpt: {SelectedNode.NodeStatus.SendCompact}{Environment.NewLine}" +
            $"Send Cmpt ver: {SelectedNode.NodeStatus.SendCompactVer}{Environment.NewLine}" +
            $"Fee filter: {SelectedNode.NodeStatus.FeeFilter}{Environment.NewLine}" +
            $"Nonce: {SelectedNode.NodeStatus.Nonce}{Environment.NewLine}" +
            $"Latency: {SelectedNode.NodeStatus.Latency.TotalMilliseconds} ms{Environment.NewLine}" +
            $"Violation: {((NodeStatus)SelectedNode.NodeStatus).Violation}{Environment.NewLine}";



        private string _res;
        public string Result
        {
            get => _res;
            set => SetField(ref _res, value);
        }


        public BindableCommand DisconnectCommand { get; }
        public bool CanDisconnect() => SelectedNode != null;
        public void Disconnect()
        {
            if (!(SelectedNode is null))
            {
                AllNodes.Remove(SelectedNode);
            }
        }


        private FullClient fullClient;
        public void StartFullClient()
        {
            if (fullClient is null)
            {
                fullClient = new FullClient(clientSettings);
            }

            fullClient.Start();
        }

        public void StopFullClient()
        {
            fullClient?.Dispose();
            fullClient = null;
        }


        public static string Risk
        {
            get
            {
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                string verInfo = "The current version is a showcase of different Bitcoin.Net features and is not " +
                                 "yet a complete bitcoin client.";
                return $"The current version is {ver.ToString(4)}{Environment.NewLine}{verInfo}";
            }
        }
    }
}
