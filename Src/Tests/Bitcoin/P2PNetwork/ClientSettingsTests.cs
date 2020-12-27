// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using System.Net;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class ClientSettingsTests
    {
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
