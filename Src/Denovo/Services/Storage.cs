// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Denovo.Services
{
    class IpConv : JsonConverter<IPAddress>
    {
        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return IPAddress.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }


    /// <summary>
    /// WARNING: any format used here is version 0 and will be subject to change and may not be backward compatible.
    /// </summary>
    public class Storage : IStorage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <param name="netType">Network type to use</param>
        public Storage(NetworkType netType)
        {
            // Main directory is C:\Users\USERNAME\AppData\Roaming\Autarkysoft\Denovo on Windows
            // or ~/.config/Autarkysoft/Denovo on Unix systems such as Linux

            // Note that "Environment.SpecialFolder.ApplicationData" returns the correct ~/.config 
            // following XDG Base Directory Specification
            mainDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Autarkysoft", "Denovo");
            // For "TestNet" and "RegTest" everything is placed in a separate folder with the same name
            if (netType == NetworkType.TestNet || netType == NetworkType.RegTest)
            {
                mainDir = Path.Combine(mainDir, netType.ToString());
            }
            else if (netType != NetworkType.MainNet)
            {
                throw new ArgumentException("Undefined network type", nameof(netType));
            }

            if (!Directory.Exists(mainDir))
            {
                Directory.CreateDirectory(mainDir);
            }
        }


        private readonly string mainDir;

        public string GetAppPath() => Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);


        private HashSet<NetworkAddressWithTime> localAddrs;
        private readonly object addrLock = new object();
        private const string NodeAddrs = "NodeAddrs";

        private T ReadFile<T>(string name, JsonSerializerOptions options = null)
        {
            string path = Path.Combine(mainDir, $"{name}.json");
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

        private void WriteFile<T>(T value, string name, JsonSerializerOptions options = null)
        {
            string path = Path.Combine(mainDir, $"{name}.json");
            using FileStream stream = File.Create(path);
            string json = JsonSerializer.Serialize(value, options);
            stream.Write(Encoding.UTF8.GetBytes(json));
        }

        private JsonSerializerOptions GetAddrJsonOps()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            options.Converters.Add(new IpConv());
            return options;
        }

        /// <inheritdoc/>
        public NetworkAddressWithTime[] ReadAddrs()
        {
            lock (addrLock)
            {
                if (localAddrs is null)
                {
                    if (File.Exists(Path.Combine(mainDir, NodeAddrs)))
                    {
                        var temp = ReadFile<NetworkAddressWithTime[]>(NodeAddrs, GetAddrJsonOps());
                        localAddrs = new HashSet<NetworkAddressWithTime>(temp);
                        return localAddrs.ToArray();
                    }
                    return new NetworkAddressWithTime[0];
                }
                else
                {
                    return localAddrs.ToArray();
                }
            }
        }

        /// <inheritdoc/>
        public void WriteAddrs(NetworkAddressWithTime[] addrs)
        {
            lock (addrLock)
            {
                if (localAddrs is null)
                {
                    WriteFile(addrs, NodeAddrs, GetAddrJsonOps());
                    localAddrs = new HashSet<NetworkAddressWithTime>(addrs);
                }
                else
                {
                    int count = localAddrs.Count;
                    localAddrs.UnionWith(addrs);
                    if (localAddrs.Count != count)
                    {
                        WriteFile(localAddrs.ToArray(), NodeAddrs, GetAddrJsonOps());
                    }
                }
            }
        }
    }
}
