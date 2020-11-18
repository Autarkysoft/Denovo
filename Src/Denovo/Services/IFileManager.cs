// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Text.Json;

namespace Denovo.Services
{
    public interface IFileManager
    {
        void AppendData(byte[] data, string fileName);
        byte[] ReadData(string fileName);
        void WriteData(byte[] data, string fileName);
        T ReadJson<T>(string fileName, JsonSerializerOptions options);
        void WriteJson<T>(T value, string fileName, JsonSerializerOptions options);
    }
}
