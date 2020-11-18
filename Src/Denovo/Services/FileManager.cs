// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Denovo.Services
{
    public class FileManager : IFileManager
    {
        /// <exception cref="UnauthorizedAccessException"/>
        public FileManager(NetworkType netType)
        {
            // Main directory is C:\Users\USERNAME\AppData\Roaming\Autarkysoft\Denovo on Windows
            // or ~/.config/Autarkysoft/Denovo on Unix systems such as Linux

            // Note that "Environment.SpecialFolder.ApplicationData" returns the correct ~/.config 
            // following XDG Base Directory Specification
            mainDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autarkysoft", "Denovo");
            // For other networks everything is placed in a separate folder using the network name
            if (netType != NetworkType.MainNet)
            {
                mainDir = Path.Combine(mainDir, netType.ToString());
            }

            if (!Directory.Exists(mainDir))
            {
                Directory.CreateDirectory(mainDir);
            }
        }


        private readonly string mainDir;


        public string GetAppPath() => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);


        public void AppendData(byte[] data, string fileName)
        {
            string path = Path.Combine(mainDir, $"{fileName}.ddat");
            if (File.Exists(path))
            {
                using FileStream stream = new FileStream(path, FileMode.Append);
                stream.Write(data);
            }
            else
            {
                using FileStream stream = File.Create(path);
                stream.Write(data);
            }
        }

        public byte[] ReadData(string fileName)
        {
            string path = Path.Combine(mainDir, $"{fileName}.ddat");
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            else
            {
                return null;
            }
        }

        public void WriteData(byte[] data, string fileName)
        {
            string path = Path.Combine(mainDir, $"{fileName}.ddat");
            using FileStream stream = File.Create(path);
            stream.Write(data);
        }

        public T ReadJson<T>(string fileName, JsonSerializerOptions options = null)
        {
            string path = Path.Combine(mainDir, $"{fileName}.json");
            if (File.Exists(path))
            {
                ReadOnlySpan<byte> data = File.ReadAllBytes(path);
                return JsonSerializer.Deserialize<T>(data, options);
            }
            else
            {
                return default;
            }
        }

        public void WriteJson<T>(T value, string fileName, JsonSerializerOptions options = null)
        {
            string path = Path.Combine(mainDir, $"{fileName}.json");
            using FileStream stream = File.Create(path);
            string json = JsonSerializer.Serialize(value, options);
            stream.Write(Encoding.UTF8.GetBytes(json));
        }
    }
}
