// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Denovo.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Denovo.Services
{
    public class FileManager : IDenovoFileManager
    {
        /// <exception cref="UnauthorizedAccessException"/>
        public FileManager(NetworkType netType)
        {
            network = netType;
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


        private readonly NetworkType network;
        private readonly string mainDir;
        private string blockDir;


        public static string GetAppPath() =>
                             Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);


        public void SetBlockPath(string blockPath)
        {
            blockDir = network switch
            {
                NetworkType.MainNet => Path.Combine(blockPath, "Autarkysoft", "Denovo", "Blocks"),
                _ => Path.Combine(blockPath, "Autarkysoft", "Denovo", network.ToString(), "Blocks"),
            };

            if (!Directory.Exists(blockDir))
            {
                Directory.CreateDirectory(blockDir);
            }

            string[] blockFiles = Directory.GetFiles(blockDir, "*.ddat", SearchOption.TopDirectoryOnly);
            for (int i = blockFiles.Length - 1; i >= 0; i--)
            {
                string name = Path.GetFileNameWithoutExtension(blockFiles[i]);
                if (name.Length == 11 &&
                    name.StartsWith("Block") &&
                    int.TryParse(name[5..], out int tempNum) &&
                    blockFileNum < tempNum)
                {
                    blockFileNum = tempNum;
                }
            }
        }

        private void AppendData(byte[] data, string fileName, string dir)
        {
            string path = Path.Combine(dir, $"{fileName}.ddat");
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

        /// <inheritdoc/>
        public void AppendData(byte[] data, string fileName) => AppendData(data, fileName, mainDir);


        private byte[] ReadData(string fileName, string dir)
        {
            string path = Path.Combine(dir, $"{fileName}.ddat");
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public byte[] ReadData(string fileName) => ReadData(fileName, mainDir);


        private void WriteData(byte[] data, string fileName, string dir)
        {
            string path = Path.Combine(dir, $"{fileName}.ddat");
            using FileStream stream = File.Create(path);
            stream.Write(data);
        }

        /// <inheritdoc/>
        public void WriteData(byte[] data, string fileName) => WriteData(data, fileName, mainDir);


        private int blockFileNum;
        private const int Max = 0x08000000;
        private const string BlockInfo = "BlockInfo";


        /// <inheritdoc/>
        public byte[] ReadBlockInfo() => ReadData(BlockInfo);

        private void WriteBlockInfo(IBlock block)
        {
            var stream = new FastStream(32 + 4 + 4);
            stream.Write(block.Header.GetHash(false));
            stream.Write(block.BlockSize);
            stream.Write(blockFileNum);

            // Writes to main directory
            AppendData(stream.ToByteArray(), BlockInfo);
        }

        /// <inheritdoc/>
        public void WriteBlock(IBlock block)
        {
            var temp = new FastStream(block.BlockSize);
            block.Serialize(temp);

            string fileName = $"Block{blockFileNum:D6}";
            var info = new FileInfo(Path.Combine(blockDir, fileName));
            long len = info.Exists ? info.Length : 0;
            if (len + temp.GetSize() > Max)
            {
                blockFileNum++;
                fileName = $"Block{blockFileNum:D6}";
            }

            WriteBlockInfo(block);
            AppendData(temp.ToByteArray(), fileName, blockDir);
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


        public Configuration ReadConfig()
        {
            return ReadJson<Configuration>("Config", null);
        }

        public void WriteConfig(Configuration config)
        {
            WriteJson(config, "Config", null);
        }
    }
}
