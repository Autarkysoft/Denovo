// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Denovo.Models;
using System.Text.Json;

namespace Denovo.Services
{
    public interface IDenovoFileManager : IFileManager
    {
        T ReadJson<T>(string fileName, JsonSerializerOptions options);
        void WriteJson<T>(T value, string fileName, JsonSerializerOptions options);
        Configuration ReadConfig();
        void WriteConfig(Configuration config);
    }
}
