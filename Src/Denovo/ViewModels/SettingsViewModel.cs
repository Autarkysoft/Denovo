// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Denovo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Denovo.ViewModels
{
    public class SettingsViewModel : VmWithSizeBase
    {
        public SettingsViewModel() : base(500, 600)
        {
            Config = new Configuration();
            ClientTypes = Enum.GetValues(typeof(ClientType)).Cast<ClientType>();
            Config.PropertyChanged += Config_PropertyChanged;
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
        }


        public Configuration Config { get; set; }
        public IEnumerable<ClientType> ClientTypes { get; set; }

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
    }
}
