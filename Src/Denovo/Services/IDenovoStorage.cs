// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Denovo.Models;

namespace Denovo.Services
{
    public interface IDenovoStorage : IStorage
    {
        Configuration ReadConfig();
        void WriteConfig(Configuration config);
    }
}
