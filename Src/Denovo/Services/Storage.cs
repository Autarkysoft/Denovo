// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Denovo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class Storage : IDenovoStorage
    {
        public Storage(NetworkType netType, IDenovoFileManager fileManager = null)
        {
            fileMan = fileManager ?? new FileManager(netType);
            network = netType;
        }


        private readonly NetworkType network;
        private readonly IDenovoFileManager fileMan;

        private HashSet<NetworkAddressWithTime> localAddrs;
        private readonly object addrLock = new object();
        private const string NodeAddrs = "NodeAddrs";


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
                    NetworkAddressWithTime[] temp = fileMan.ReadJson<NetworkAddressWithTime[]>(NodeAddrs, GetAddrJsonOps());
                    if (temp is null)
                    {
                        temp = new NetworkAddressWithTime[0];
                    }
                    localAddrs = new HashSet<NetworkAddressWithTime>(temp);
                }

                return localAddrs.ToArray();
            }
        }

        /// <inheritdoc/>
        public void WriteAddrs(NetworkAddressWithTime[] addrs)
        {
            lock (addrLock)
            {
                if (localAddrs is null)
                {
                    ReadAddrs();
                }

                int count = localAddrs.Count;
                localAddrs.UnionWith(addrs);
                if (localAddrs.Count != count)
                {
                    fileMan.WriteJson(localAddrs.ToArray(), NodeAddrs, GetAddrJsonOps());
                }
            }
        }

        public Configuration ReadConfig()
        {
            return fileMan.ReadJson<Configuration>("Config", null) ?? new Configuration(network) { IsDefault = true };
        }

        public void WriteConfig(Configuration config)
        {
            fileMan.WriteJson(config, "Config", null);
        }

        public BlockHeader[] ReadHeaders()
        {
            byte[] data = fileMan.ReadData("Headers");
            if (data is null || data.Length % Constants.BlockHeaderSize != 0)
            {
                return null;
            }
            else
            {
                var result = new BlockHeader[data.Length / Constants.BlockHeaderSize];
                var stream = new FastStreamReader(data);
                for (int i = 0; i < result.Length; i++)
                {
                    var temp = new BlockHeader();
                    if (temp.TryDeserialize(stream, out _))
                    {
                        result[i] = temp;
                    }
                    else
                    {
                        return null;
                    }
                }
                return result;
            }
        }

        public void AppendBlockHeaders(BlockHeader[] headers)
        {
            var stream = new FastStream(headers.Length * Constants.BlockHeaderSize);
            foreach (var item in headers)
            {
                item.Serialize(stream);
            }
            fileMan.AppendData(stream.ToByteArray(), "Headers");
        }

        public void WriteBlockHeaders(BlockHeader[] headers)
        {
            var stream = new FastStream(headers.Length * Constants.BlockHeaderSize);
            foreach (var item in headers)
            {
                item.Serialize(stream);
            }
            fileMan.WriteData(stream.ToByteArray(), "Headers");
        }
    }
}
