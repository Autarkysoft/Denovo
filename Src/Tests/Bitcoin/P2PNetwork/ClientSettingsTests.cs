// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Net;
using Tests.Bitcoin.Blockchain;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class ClientSettingsTests
    {
        [Theory]
        [InlineData(NodeServiceFlags.NodeNone, BlockchainState.HeadersSync, false)]
        [InlineData(NodeServiceFlags.NodeNetwork, BlockchainState.HeadersSync, true)]
        [InlineData(NodeServiceFlags.NodeNetworkLimited, BlockchainState.HeadersSync, true)]
        [InlineData(NodeServiceFlags.NodeNetwork, BlockchainState.BlocksSync, false)]
        [InlineData(NodeServiceFlags.NodeNetworkLimited, BlockchainState.BlocksSync, false)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeWitness, BlockchainState.BlocksSync, true)]
        public void HasNeededServicesTest(NodeServiceFlags flags, BlockchainState state, bool expected)
        {
            var cs = new ClientSettings();
            Helper.SetReadonlyProperty(cs, nameof(cs.Blockchain), new MockBlockchain() { _stateToReturn = state });
            bool actual = cs.HasNeededServices(flags);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(NodeServiceFlags.NodeNone, false)]
        [InlineData(NodeServiceFlags.NodeBloom | NodeServiceFlags.NodeWitness | NodeServiceFlags.NodeGetUtxo, false)]
        [InlineData(NodeServiceFlags.NodeNetwork, true)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeGetUtxo, true)]
        [InlineData(NodeServiceFlags.NodeNetworkLimited, true)]
        [InlineData(NodeServiceFlags.NodeNetworkLimited | NodeServiceFlags.NodeGetUtxo, true)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeNetworkLimited, true)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeNetworkLimited | NodeServiceFlags.NodeWitness, true)]
        public void IsGoodForHeaderSyncTest(NodeServiceFlags flags, bool expected)
        {
            var cs = new ClientSettings();
            bool actual = cs.IsGoodForHeaderSync(flags);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(NodeServiceFlags.NodeNone, false)]
        [InlineData(NodeServiceFlags.NodeBloom | NodeServiceFlags.NodeWitness | NodeServiceFlags.NodeGetUtxo, false)]
        [InlineData(NodeServiceFlags.NodeNetwork, false)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeGetUtxo, false)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeNetworkLimited, false)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeWitness, true)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeWitness | NodeServiceFlags.NodeNetworkLimited, true)]
        [InlineData(NodeServiceFlags.NodeNetworkLimited, false)]
        [InlineData(NodeServiceFlags.NodeNetworkLimited | NodeServiceFlags.NodeGetUtxo, false)]
        [InlineData(NodeServiceFlags.NodeNetworkLimited | NodeServiceFlags.NodeWitness, false)]
        public void IsGoodForBlockSyncTest(NodeServiceFlags flags, bool expected)
        {
            var cs = new ClientSettings();
            bool actual = cs.IsGoodForBlockSync(flags);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(NodeServiceFlags.NodeNone, false)]
        [InlineData(NodeServiceFlags.NodeBloom | NodeServiceFlags.NodeWitness | NodeServiceFlags.NodeGetUtxo, false)]
        [InlineData(NodeServiceFlags.NodeNetwork, false)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeGetUtxo, false)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeNetworkLimited, false)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeWitness, false)]
        [InlineData(NodeServiceFlags.NodeNetwork | NodeServiceFlags.NodeWitness | NodeServiceFlags.NodeNetworkLimited, false)]
        [InlineData(NodeServiceFlags.NodeNetworkLimited, true)]
        public void IsPrunedTest(NodeServiceFlags flags, bool expected)
        {
            var cs = new ClientSettings();
            bool actual = cs.IsPruned(flags);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void UpdateMyIP_GetMyIP_Test()
        {
            var cs = new ClientSettings();

            var ip1_1 = IPAddress.Parse("1.1.1.1");
            var ip1_2 = IPAddress.Parse("1.1.1.1");
            var ip2_1 = IPAddress.Parse("2.2.2.2");
            var ip2_2 = IPAddress.Parse("2.2.2.2");
            var ip2_3 = IPAddress.Parse("2.2.2.2");
            var ip2_4 = IPAddress.Parse("2.2.2.2");
            var ip2_5 = IPAddress.Parse("2.2.2.2");

            cs.UpdateMyIP(IPAddress.Loopback);
            Assert.Empty(cs.localIP);
            Assert.Equal(IPAddress.Loopback, cs.GetMyIP());

            cs.UpdateMyIP(IPAddress.IPv6Loopback);
            Assert.Empty(cs.localIP);
            Assert.Equal(IPAddress.Loopback, cs.GetMyIP());

            cs.UpdateMyIP(ip1_1);
            Assert.Single(cs.localIP);
            Assert.Equal(0, cs.localIP[ip1_1]);
            Assert.Equal(IPAddress.Loopback, cs.GetMyIP());

            cs.UpdateMyIP(ip2_1);
            Assert.Equal(2, cs.localIP.Count);
            Assert.Equal(0, cs.localIP[ip1_1]);
            Assert.Equal(0, cs.localIP[ip2_1]);
            Assert.Equal(IPAddress.Loopback, cs.GetMyIP());

            cs.UpdateMyIP(ip2_2);
            Assert.Equal(2, cs.localIP.Count);
            Assert.Equal(0, cs.localIP[ip1_1]);
            Assert.Equal(1, cs.localIP[ip2_1]);
            Assert.Equal(IPAddress.Loopback, cs.GetMyIP());

            cs.UpdateMyIP(ip2_3);
            Assert.Equal(2, cs.localIP.Count);
            Assert.Equal(0, cs.localIP[ip1_1]);
            Assert.Equal(2, cs.localIP[ip2_1]);
            Assert.Equal(IPAddress.Loopback, cs.GetMyIP());

            cs.UpdateMyIP(ip1_2);
            Assert.Equal(2, cs.localIP.Count);
            Assert.Equal(1, cs.localIP[ip1_1]);
            Assert.Equal(2, cs.localIP[ip2_1]);
            Assert.Equal(IPAddress.Loopback, cs.GetMyIP());

            cs.UpdateMyIP(ip2_4);
            Assert.Equal(2, cs.localIP.Count);
            Assert.Equal(1, cs.localIP[ip1_1]);
            Assert.Equal(3, cs.localIP[ip2_1]);
            Assert.Equal(IPAddress.Loopback, cs.GetMyIP());

            cs.UpdateMyIP(ip2_5);
            Assert.Equal(2, cs.localIP.Count);
            Assert.Equal(1, cs.localIP[ip1_1]);
            Assert.Equal(4, cs.localIP[ip2_1]);
            Assert.Equal(IPAddress.Parse("2.2.2.2"), cs.GetMyIP());
        }
    }
}
