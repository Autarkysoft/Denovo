// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Clients;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using Denovo.Models;
using Denovo.MVVM;
using Denovo.Services;
using System;
using System.Threading;

namespace Denovo.ViewModels
{
    public class MinerViewModel : VmWithSizeBase
    {
        public MinerViewModel() : base(650, 650)
        {
            AllNodes = new NodePool(5);
            MinimalClientSettings clientSettings = new(NetworkType.TestNet, 5, AllNodes)
            {
                DnsSeeds = Configuration.DnsTest,
                UserAgent = "/Satoshi:0.22.0/",
            };
            client = new MinimalClient(clientSettings);

            client.Start();
        }


        private readonly MinimalClient client;
        public NodePool AllNodes { get; set; }

        private Node _selNode;
        public Node SelectedNode
        {
            get => _selNode;
            set => SetField(ref _selNode, value);
        }

        [DependsOnProperty(nameof(SelectedNode), nameof(BlockHeight))]
        public string NodeInfo => SelectedNode is null ?
            $"Block height being mined: {BlockHeight}" :
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

        private int _blkH = 2135863;
        public int BlockHeight
        {
            get => _blkH;
            set => SetField(ref _blkH, value);
        }

        private string _prvBlk = "block hex here";
        public string PreviousBlockHex
        {
            get => _prvBlk;
            set => SetField(ref _prvBlk, value);
        }

        private string _blkhex;
        public string BlockHex
        {
            get => _blkhex;
            set => SetField(ref _blkhex, value);
        }

        private CancellationTokenSource tokenSource;

        public async void StartMining()
        {
            BlockHex = $"Mining block #{BlockHeight:n0}";
            tokenSource?.Dispose();
            tokenSource = null;

            Block prvBlock = new();
            if (!Base16.TryDecode(PreviousBlockHex, out byte[] data))
            {
                BlockHex = "Invalid hex.";
                return;
            }
            else if (!BlockHeader.TryDeserialize(new FastStreamReader(data), out BlockHeader hdr, out Errors error))
            {
                BlockHex = $"Error occured while deserializing header: {error.Convert()}";
                return;
            }
            else
            {
                prvBlock.Header = hdr;
            }


            TestNetMiner miner = new();
            tokenSource = new CancellationTokenSource();

            IBlock result = await miner.Start(prvBlock, BlockHeight, tokenSource.Token);

            if (!(result is null))
            {
                FastStream stream = new();
                result.Serialize(stream);
                BlockHex = stream.ToByteArray().ToBase16();

                Message msg = new(new BlockPayload(result), NetworkType.TestNet);

                client.Send(msg);
            }
            else
            {
                BlockHex = "Failed to find";
            }
        }

        public void StopMining()
        {
            tokenSource?.Cancel();
        }
    }
}
