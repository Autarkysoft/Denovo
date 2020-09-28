// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.P2PNetwork;
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
        public MainWindowViewModel()
        {
            AllNodes = new NodePool(5);
            var clientSettings = new ClientSettings() { UserAgent = "/Satoshi:0.20.1/", Relay = false };
            WinMan = new WindowManager();
            connector = new NodeConnector(AllNodes, new MockBlockChain(), clientSettings);
            listener = new NodeListener(AllNodes, new MockBlockChain(), clientSettings);

            listener.StartListen(new IPEndPoint(IPAddress.Any, testPortToUse));

            DisconnectCommand = new BindableCommand(Disconnect, CanDisconnect);
        }

        public NodePool AllNodes { get; set; }

        public IWindowManager WinMan { get; set; }
        public void Config() => WinMan.ShowDialog(new SettingsViewModel());


        internal class MockBlockChain : IBlockchain
        {
            public int Height => 0;
            public int FindHeight(ReadOnlySpan<byte> prevHash) => throw new NotImplementedException();
            public Target GetTarget(int height) => throw new NotImplementedException();
            public bool ProcessBlock(IBlock block) => true;
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


        [DependsOnProperty(nameof(SelectedNode))]
        public string NodeInfo => SelectedNode is null ?
            "Nothing is selected." :
            $"UA: {SelectedNode.NodeStatus.UserAgent}{Environment.NewLine}" +
            $"IP: {SelectedNode.NodeStatus.IP}{Environment.NewLine}" +
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
            $"Violation: {((NodeStatus)SelectedNode.NodeStatus).Violation}{Environment.NewLine}";


        private readonly NodeConnector connector;
        private readonly NodeListener listener;
        private const int testPortToUse = 8333;


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
                    Task.Run(() => connector.StartConnect(new IPEndPoint(ip, testPortToUse)));
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
            AllNodes.Remove(SelectedNode);
        }


        public string Risk
        {
            get
            {
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                string verInfo = ver.Major == 0 ?
                    (ver.Minor == 0 ? "Version zero is incomplete [preview] release (high chance of having bugs)." :
                    ver.Minor == 1 ? "First beta is a moderately stable version with little bugs but good chance of having more." :
                    "Beta versions are moderately stable but have small chance of having unfound bugs.") :
                    "Stable release";

                return $"The current version is {ver.ToString(4)}{Environment.NewLine}{verInfo}";
            }
        }
    }
}
