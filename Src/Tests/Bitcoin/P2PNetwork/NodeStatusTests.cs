// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class NodeStatusTests
    {
        [Fact]
        public void SendCompactVerTest()
        {
            var ns = new NodeStatus();
            Assert.Equal(0UL, ns.SendCompactVer);
            
            ns.SendCompactVer = 0;
            Assert.Equal(0UL, ns.SendCompactVer);

            ns.SendCompactVer = 1;
            Assert.Equal(1UL, ns.SendCompactVer);

            ns.SendCompactVer = 0;
            Assert.Equal(1UL, ns.SendCompactVer);

            ns.SendCompactVer = 2;
            Assert.Equal(2UL, ns.SendCompactVer);

            ns.SendCompactVer = 1;
            Assert.Equal(2UL, ns.SendCompactVer);
        }
    }
}
