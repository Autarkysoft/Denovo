// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork;
using System;
using System.Net;
using System.Reflection;

namespace Denovo.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        internal class MockBlockChain : IBlockchain
        {
            public int Height => 0;
        }

        private readonly Node node = new Node(new MockBlockChain());


        private string _ip;
        public string IpAddress
        {
            get => _ip;
            set => SetField(ref _ip, value);
        }

        private string _res;
        public string Result
        {
            get => _res;
            set => SetField(ref _res, value);
        }

        public void Connect()
        {
            try
            {
                Result = string.Empty;
                if (IPAddress.TryParse(IpAddress, out IPAddress ip))
                {
                    node.StartConnect(new IPEndPoint(ip, 8333));
                }
                else
                {
                    Result = "Can't parse given IP address.";
                }
            }
            catch (Exception ex)
            {
                Result = $"An exception of type {ex.GetType()} was thrown:{Environment.NewLine}{ex.Message}" +
                    $"{Environment.NewLine}Stack trace:{Environment.NewLine}{ex.StackTrace}";
            }

        }


        public string Risk
        {
            get
            {
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                string verInfo = ver.Major == 0 ?
                    (ver.Minor == 0 ? "Version zero is incomplete [preview] release (high chance of having bugs)." :
                    ver.Minor == 1 ? "First beta is a moderately stable version with little bugs but good chance of having more." :
                    "Beta versions are moderately stable but have small chance of having unfound bugs.") :
                    "Stable release";

                return $"The current version is {ver.ToString(4)}{Environment.NewLine}{verInfo}";
            }
        }
    }
}
