// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Denovo.Models;
using Denovo.MVVM;
using Denovo.Services;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

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


        private void Init(Configuration config)
        {
            AllNodes = new NodePool(config.MaxConnectionCount);
            var clientSettings =
                new ClientSettings(
                    config.AcceptIncoming,
                    config.Network,
                    config.MaxConnectionCount,
                    NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeWitness,
                    AllNodes,
                    FileMan)
                {
                    Relay = config.Relay,
                    UserAgent = config.UserAgent,
                    DnsSeeds = config.PeerList.Split(Environment.NewLine),
                    //ListenPort = (ushort)config.ListenPort
                };

            clientSettings.Time.WrongClockEvent += Time_WrongClockEvent;

            // TODO: the following 4 lines are for testing and can be removed later
            connector = new NodeConnector(AllNodes, clientSettings);
            listener = new NodeListener(AllNodes, clientSettings);
            listener.StartListen(new IPEndPoint(IPAddress.Any, port));

            MyInfo = $"My node information:{Environment.NewLine}" +
                     $"Network: {config.Network}{Environment.NewLine}" +
                     $"User agent: {config.UserAgent}{Environment.NewLine}" +
                     $"Protocol version: {clientSettings.ProtocolVersion}{Environment.NewLine}" +
                     $"Max connection count: {clientSettings.MaxConnectionCount}{Environment.NewLine}" +
                     $"Best block height: {clientSettings.Blockchain.Height}{Environment.NewLine}";


            // TODO: only the FullClient is needed
            //FullClient cl = new FullClient(clientSettings);
            //cl.Start();
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

            var vm = new ConfigurationViewModel(config);
            await WinMan.ShowDialog(vm);

            if (!IsInitialized && !config.IsDefault)
            {
                Init(config);
            }
        }

        public async void OpenMiner()
        {
            // TODO: A better way is to make the following VM disposable
            var vm = new MinerViewModel();
            await WinMan.ShowDialog(vm);
            vm.StopMining();
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


        private NodeConnector connector;
        private NodeListener listener;
        private int port;


        private string _ip = "127.0.0.1";
        public string IpAddress
        {
            get => _ip;
            set => SetField(ref _ip, value);
        }

        private string _res;
        public string Result
        {
            get => _res;
            set => SetField(ref _res, value);
        }

        public void Connect()
        {
            try
            {
                Result = string.Empty;
                if (IPAddress.TryParse(IpAddress, out IPAddress ip))
                {
                    Task.Run(() => connector.StartConnect(new IPEndPoint(ip, port)));
                }
                else
                {
                    Result = "Can't parse given IP address.";
                }
            }
            catch (Exception ex)
            {
                Result = $"An exception of type {ex.GetType()} was thrown:{Environment.NewLine}{ex.Message}" +
                    $"{Environment.NewLine}Stack trace:{Environment.NewLine}{ex.StackTrace}";
            }
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
