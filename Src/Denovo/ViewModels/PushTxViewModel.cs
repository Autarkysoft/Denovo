// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Clients;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using Denovo.MVVM;
using System;
using System.Diagnostics;

namespace Denovo.ViewModels
{
    public sealed class PushTxViewModel : VmWithSizeBase, IDisposable
    {
        public PushTxViewModel() : base(450, 650)
        {
            ConnectCommand = new BindableCommand(Connect, () => !IsConnected);
            PushCommand = new BindableCommand(Push, () => IsConnected);

            NetworkList = new NetworkType[] { NetworkType.MainNet, NetworkType.TestNet, NetworkType.TestNet4 };
        }


        private MinimalClientSettings settings;
        private MinimalClient client;


        public NetworkType[] NetworkList { get; }

        private NetworkType _SelNet;
        public NetworkType SelectedNetwork
        {
            get => _SelNet;
            set => SetField(ref _SelNet, value);
        }

        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetField(ref _isConnected, value))
                {
                    ConnectCommand.RaiseCanExecuteChanged();
                    PushCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _txHex;
        public string TxHex
        {
            get => _txHex;
            set => SetField(ref _txHex, value);
        }

        private string _res;
        public string Result
        {
            get => _res;
            set => SetField(ref _res, value);
        }

        public BindableCommand ConnectCommand { get; }
        public void Connect()
        {
            settings = new(SelectedNetwork, 4, null)
            {
                UserAgent = "/Satoshi:0.22.0/",
            };

            if (SelectedNetwork == NetworkType.MainNet)
            {
                settings.DnsSeeds = Constants.GetMainNetDnsSeeds();
            }
            else if (SelectedNetwork == NetworkType.TestNet)
            {
                settings.DnsSeeds = Constants.GetTestNetDnsSeeds();
            }
            else if (SelectedNetwork == NetworkType.TestNet4)
            {
                settings.DnsSeeds = Constants.GetTestNet4DnsSeeds();
            }
            else
            {
                Result = "Network is not defined.";
                return;
            }

            client = new(settings);
            client.Start();
            IsConnected = true;
        }

        public BindableCommand PushCommand { get; }
        public void Push()
        {
            Debug.Assert(IsConnected);
            if (settings.AllNodes.Count < 1)
            {
                Result = "Not yet connected, please wait.";
                return;
            }

            if (!Base16.TryDecode(TxHex, out byte[] ba))
            {
                Result = "Invalid hex.";
                return;
            }

            Transaction tx = new();
            if (!tx.TryDeserialize(new FastStreamReader(ba), out Errors error))
            {
                Result = $"Could not deserialize transaction hex. Error message: {error.Convert()}";
                return;
            }

            Message msg = new(new TxPayload(tx), SelectedNetwork);

            client.Send(msg);
            Result = "Successfully sent.";
        }


        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
