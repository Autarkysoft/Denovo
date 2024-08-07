﻿// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Avalonia.Platform.Storage;
using Denovo.Models;
using Denovo.MVVM;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Denovo.ViewModels
{
    public class ConfigurationViewModel : VmWithSizeBase
    {
        // This will make designer happy
        public ConfigurationViewModel() : this(new Configuration(NetworkType.MainNet))
        {
        }

        public ConfigurationViewModel(Configuration config) : base(500, 600)
        {
            Config = config;
            Config.PropertyChanged += Config_PropertyChanged;

            ClientTypes = EnumHelper.GetAllEnumValues<ClientType>();
            PeerDiscoveryOptions = EnumHelper.GetAllEnumValues<PeerDiscoveryOption>();
        }


        private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Config.SelectedClientType))
            {
                Desc = Config.SelectedClientType switch
                {
                    ClientType.Full => "Full verifying node",
                    ClientType.FullPruned => "Full verifying node that only stores a small portion of the blockchain.",
                    ClientType.Spv => "Simplified payment verification.",
                    ClientType.SpvElectrum => "Simplified payment verification using Electrum nodes.",
                    _ => "Description is not defined for this client type.",
                };

                ShowPruneSize = Config.SelectedClientType == ClientType.FullPruned;
            }

            HasPendingChanges = true;
        }

        public Configuration Config { get; set; }
        public IEnumerable<ClientType> ClientTypes { get; }
        public IEnumerable<PeerDiscoveryOption> PeerDiscoveryOptions { get; }

        private string _desc;
        public string Desc
        {
            get => _desc;
            set => SetField(ref _desc, value);
        }

        private bool _isPruned;
        public bool ShowPruneSize
        {
            get => _isPruned;
            set => SetField(ref _isPruned, value);
        }

        private bool _pendingChange;
        public bool HasPendingChanges
        {
            get => _pendingChange;
            set => SetField(ref _pendingChange, value);

        }

        public bool IsChanged { get; private set; }
        public IStorageProvider StorageProvider { get; set; }

        public async void SetBlockchainDir()
        {
            FolderPickerOpenOptions opts = new()
            {
                AllowMultiple = false,
                Title = "Blockchain directory"
            };
            IReadOnlyList<IStorageFolder> dir = await StorageProvider.OpenFolderPickerAsync(opts);
            if (dir != null && dir.Count > 0)
            {
                Config.BlockchainPath= dir.ElementAt(0).Path.LocalPath;
            }
        }


        public void Ok()
        {
            Config.IsDefault = false;
            IsChanged = true;
            RaiseCloseEvent();
        }

        public void Cancel()
        {
            IsChanged = false;
            RaiseCloseEvent();
        }
    }
}
