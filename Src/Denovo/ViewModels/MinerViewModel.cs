// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using Denovo.MVVM;
using Denovo.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Denovo.ViewModels
{
    public class MinerViewModel : VmWithSizeBase
    {
        public MinerViewModel() : base(650, 650)
        {
            AllNodes = new NodePool(5);
            var fileMan = new FileManager(NetworkType.TestNet);
            var clientSettings = new ClientSettings(false, NetworkType.TestNet, 5, NodeServiceFlags.NodeNone,
                                                    AllNodes, fileMan, new UtxoDatabase(fileMan), new MemoryPool())
            {
                UserAgent = "/Satoshi:0.20.1/",
                Relay = false,
                Network = NetworkType.TestNet,
            };
            connector = new NodeConnector(AllNodes, clientSettings);
        }


        internal class MockBlockChain : IBlockchain
        {
            public int Height => 1;

            public BlockchainState State { get => BlockchainState.None; set { } }

            public event EventHandler HeaderSyncEndEvent;
            public event EventHandler BlockSyncEndEvent;

            public int FindHeight(ReadOnlySpan<byte> prevHash) => -1;
            public Target GetNextTarget() => throw new NotImplementedException();
            public bool ProcessBlock(IBlock block, INodeStatus nodeStatus) => true;
            public BlockProcessResult ProcessHeaders(BlockHeader[] headers) => BlockProcessResult.Success;

            public BlockHeader[] GetBlockHeaderLocator()
            {
                return new BlockHeader[]
                {
                    new Consensus(NetworkType.TestNet).GetGenesisBlock().Header
                };
            }

            public BlockHeader[] GetMissingHeaders(byte[][] hashesToCompare, byte[] stopHash) => null;

            public void SetMissingBlockHashes(INodeStatus nodeStatus) { }
            public void PutBackMissingBlocks(List<Inventory> blockInvs) { }
            public void ProcessReceivedBlocks(INodeStatus nodeStatus) { }
        }

        public NodePool AllNodes { get; set; }
        private readonly NodeConnector connector;

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

        private int _blkH = 1905232;
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

            var prvBlock = new Block();
            try
            {
                if (!prvBlock.Header.TryDeserialize(new FastStreamReader(Base16.Decode(PreviousBlockHex)), out string error))
                {
                    throw new ArgumentException(error);
                }
            }
            catch (Exception ex)
            {
                BlockHex = ex.ToString();
                return;
            }


            IPAddress[] addrs = Dns.GetHostAddresses("seed.tbtc.petertodd.org");

            TestNetMiner miner = new TestNetMiner();

            if (!(tokenSource is null))
            {
                tokenSource.Dispose();
                tokenSource = null;
            }

            tokenSource = new CancellationTokenSource();

            IBlock result = await miner.Start(prvBlock, BlockHeight, tokenSource.Token);

            if (!(result is null))
            {
                var stream = new FastStream();
                result.Serialize(stream);
                BlockHex = stream.ToByteArray().ToBase16();

                var msg = new Message(new BlockPayload(result), NetworkType.TestNet);

                connector.StartConnect(new IPEndPoint(addrs[0], 18333));
                connector.StartConnect(new IPEndPoint(addrs[1], 18333));
                connector.StartConnect(new IPEndPoint(addrs[2], 18333));
                connector.StartConnect(new IPEndPoint(addrs[3], 18333));
                connector.StartConnect(new IPEndPoint(addrs[4], 18333));


                await Task.Delay(TimeSpan.FromSeconds(5));

                foreach (var node in AllNodes)
                {
                    node.Send(msg);
                }
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
