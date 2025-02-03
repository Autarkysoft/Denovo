// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Clients;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Denovo.Models;
using Denovo.MVVM;
using Denovo.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace Denovo.ViewModels
{
    public class MinerViewModel : VmWithSizeBase, IDisposable
    {
        /// <summary>
        /// Make designer happy
        /// </summary>
        public MinerViewModel() : this(null, null)
        {
        }

        public MinerViewModel(IClipboard cb, Configuration config) : base(650, 650)
        {
            clipboard = cb;
            this.config = config;

            StartClientCommand = new BindableCommand(StartClient, () => !IsClientBuilt);
            StartMiningCommand = new(StartMining, () => CanStartMining && !IsMining);
            StopMiningCommand = new(StopMining, () => IsMining);
            CopyCommand = new(Copy);
            ClearMessageCommand = new(ClearMessage);
            AddCommand = new(Add);
            RemoveCommand = new(Remove, () => SelectedTx is not null);
            ClearCommand = new(Clear);
        }


        private readonly IClipboard clipboard;
        private readonly Configuration config;
        private SpvClient client;
        private IConsensus consensus;

        public NodePool AllNodes { get; set; }

        private Node _selNode;
        public Node SelectedNode
        {
            get => _selNode;
            set => SetField(ref _selNode, value);
        }

        [DependsOnProperty(nameof(SelectedNode))]
        public string NodeInfo => SelectedNode is null ?
            $"Select a peer" :
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
            $"Latency: {SelectedNode.NodeStatus.Latency.TotalMilliseconds} ms{Environment.NewLine}" +
            $"Violation: {((NodeStatus)SelectedNode.NodeStatus).Violation}{Environment.NewLine}";


        public bool IsTestNet3 { get; set; } = true;

        private bool _isClBuilt = false;
        public bool IsClientBuilt
        {
            get => _isClBuilt;
            set
            {
                if (SetField(ref _isClBuilt, value))
                {
                    StartClientCommand.RaiseCanExecuteChanged();
                    StartMiningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _canMine = false;
        public bool CanStartMining
        {
            get => _canMine;
            set
            {
                if (SetField(ref _canMine, value))
                {
                    StartMiningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isMining = false;
        public bool IsMining
        {
            get => _isMining;
            set
            {
                if (SetField(ref _isMining, value))
                {
                    StartMiningCommand.RaiseCanExecuteChanged();
                    StopMiningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private int _blkH;
        public int BlockHeight
        {
            get => _blkH;
            set => SetField(ref _blkH, value);
        }

        private string _txHex = string.Empty;
        public string TxHex
        {
            get => _txHex;
            set => SetField(ref _txHex, value);
        }

        private ulong _txFee = 0;
        public ulong TxFee
        {
            get => _txFee;
            set => SetField(ref _txFee, value);
        }

        private ulong _totalFee = 0;
        public ulong TotalFee
        {
            get => _totalFee;
            set => SetField(ref _totalFee, value);
        }

        private string _blkhex = string.Empty;
        public string BlockHex
        {
            get => _blkhex;
            set => SetField(ref _blkhex, value);
        }

        private string _msg = string.Empty;
        public string Message
        {
            get => _msg;
            set => SetField(ref _msg, value);
        }

        public ObservableCollection<TxWithFeeModel> TxList { get; set; } = new();

        private TxWithFeeModel _tx;
        public TxWithFeeModel SelectedTx
        {
            get => _tx;
            set
            {
                if (SetField(ref _tx, value))
                {
                    RemoveCommand.RaiseCanExecuteChanged();
                }
            }
        }



        public BindableCommand CopyCommand { get; }
        private void Copy()
        {
            if (!string.IsNullOrEmpty(BlockHex))
            {
                clipboard.SetTextAsync(BlockHex);
            }
        }

        public BindableCommand ClearMessageCommand { get; }
        private void ClearMessage()
        {
            Message = string.Empty;
        }


        private void AddMessage(string msg)
        {
            Dispatcher.UIThread.Post(() => Message += string.IsNullOrEmpty(Message) ? msg : $"{Environment.NewLine}{msg}");
        }


        private void SetTotals()
        {
            TotalFee = 0;
            foreach (var item in TxList)
            {
                TotalFee += item.Fee;
            }
        }

        public BindableCommand AddCommand { get; }
        private void Add()
        {
            if (Base16.TryDecode(TxHex.Trim(), out byte[] data))
            {
                Transaction t = new();
                if (t.TryDeserialize(new FastStreamReader(data), out Errors error))
                {
                    TxList.Add(new TxWithFeeModel(t, TxFee));
                    TxHex = string.Empty;
                    TxFee = 0;
                    SetTotals();
                }
                else
                {
                    AddMessage($"Could not deserialize tx. Error message={error.Convert()}");
                }
            }
            else
            {
                AddMessage("Invalid Base-16 tx.");
            }
        }

        public BindableCommand RemoveCommand { get; }
        private void Remove()
        {
            if (SelectedTx is not null)
            {
                TxList.Remove(SelectedTx);
                SetTotals();
            }
        }

        public BindableCommand ClearCommand { get; }
        private void Clear()
        {
            TxList.Clear();
            SetTotals();
        }


        public BindableCommand StartClientCommand { get; }
        private void StartClient()
        {
            AddMessage("Creating client. Reading Headers file can take some time.");

            NetworkType nt = IsTestNet3 ? NetworkType.TestNet3 : NetworkType.TestNet4;
            AllNodes = new NodePool(5);
            FileManager fileMan = new(nt);
            fileMan.SetBlockPath(config.BlockchainPath);
            SpvClientSettings clientSettings = new(nt, 5, AllNodes, fileMan)
            {
                DnsSeeds = IsTestNet3 ? Constants.GetTestNet3DnsSeeds() : Constants.GetTestNet4DnsSeeds(),
                UserAgent = "/Satoshi:0.22.0/",
            };
            clientSettings.Blockchain.HeaderSyncEndEvent += Blockchain_HeaderSyncEndEvent;
            consensus = ((Chain)clientSettings.Blockchain).Consensus;
            client = new SpvClient(clientSettings);
            BlockHeight = clientSettings.Blockchain.HeaderCount;

            client.Start();
            AddMessage("Starting client and syncing headers.");

            IsClientBuilt = true;
        }

        private void Blockchain_NewHeaderEvent(object? sender, EventArgs e)
        {
            if (IsMining)
            {
                Dispatcher.UIThread.Post(StopMining);
                AddMessage("New block(s) received. Stopped mining.");
            }

            BlockHeight = client.Settings.Blockchain.HeaderCount;
        }

        private void Blockchain_HeaderSyncEndEvent(object? sender, EventArgs e)
        {
            AddMessage("Header sync complete. Can start mining now.");
            Dispatcher.UIThread.Post(() => CanStartMining = true);
            client.Settings.Blockchain.HeaderSyncEndEvent -= Blockchain_HeaderSyncEndEvent;
            client.Settings.Blockchain.NewHeaderEvent += Blockchain_NewHeaderEvent;
            BlockHeight = client.Settings.Blockchain.HeaderCount;
        }


        private CancellationTokenSource tokenSource;
        public BindableCommand StartMiningCommand { get; }
        private async void StartMining()
        {
            IsMining = true;

            BlockHex = $"Mining block #{BlockHeight:n0} ({TxList.Count}+1 tx & {TotalFee:n0} fee)";

            TestNetMiner miner = new();
            tokenSource?.Dispose();
            tokenSource = new CancellationTokenSource();

            IBlock? result = await miner.Start(client.Settings.Blockchain.LastHeader, consensus, TxList, tokenSource.Token);

            if (result is not null)
            {
                FastStream stream = new();
                result.Serialize(stream);
                BlockHex = stream.ToByteArray().ToBase16();

                NetworkType nt = IsTestNet3 ? NetworkType.TestNet3 : NetworkType.TestNet4;
                Message msg = new(new BlockPayload(result), nt);

                client.Send(msg);
            }
            else
            {
                BlockHex = "Failed to find";
            }

            IsMining = false;
        }

        public BindableCommand StopMiningCommand { get; }
        internal void StopMining()
        {
            tokenSource?.Cancel();
            IsMining = false;
        }


        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
