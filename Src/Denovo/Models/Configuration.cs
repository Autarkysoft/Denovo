// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Denovo.MVVM;

namespace Denovo.Models
{
    public class Configuration: InpcBase
    {
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
    }
}
