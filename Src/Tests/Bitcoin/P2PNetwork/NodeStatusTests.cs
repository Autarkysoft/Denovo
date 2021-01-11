// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class NodeStatusTests
    {
        [Fact]
        public void DisconnectEvent_WithViolationProp_Test()
        {
            var ns = new NodeStatus();
            bool raised = false;
            ns.DisconnectEvent += (sender, e) =>
            {
                raised = true;
            };
            ns.Violation = 99;
            Assert.False(raised);
            ns.Violation = 10;
            Assert.False(raised);
            ns.Violation = 100;
            Assert.True(raised);
        }

        [Fact]
        public void DisconnectEvent_WithAddViolation_Test()
        {
            var ns = new NodeStatus();
            bool raised = false;
            ns.DisconnectEvent += (sender, e) =>
            {
                raised = true;
            };
            ns.AddBigViolation();
            Assert.False(raised);
            ns.AddBigViolation();
            Assert.True(raised);
        }

        [Fact]
        public void InpcTest()
        {
            var ns = new NodeStatus();
            Assert.PropertyChanged(ns, nameof(ns.FeeFilter), () => ns.FeeFilter = 1);
            Assert.PropertyChanged(ns, nameof(ns.HandShake), () => ns.HandShake = HandShakeState.Finished);
            Assert.PropertyChanged(ns, nameof(ns.IP), () => ns.IP = IPAddress.Loopback);
            Assert.PropertyChanged(ns, nameof(ns.Nonce), () => ns.Nonce = 123);
            Assert.PropertyChanged(ns, nameof(ns.ProtocolVersion), () => ns.ProtocolVersion = 123);
            Assert.PropertyChanged(ns, nameof(ns.Relay), () => ns.Relay = true);
            Assert.PropertyChanged(ns, nameof(ns.SendCompact), () => ns.SendCompact = true);
            Assert.PropertyChanged(ns, nameof(ns.SendCompactVer), () => ns.SendCompactVer = 2);
            Assert.PropertyChanged(ns, nameof(ns.Services), () => ns.Services = NodeServiceFlags.NodeBloom);
            Assert.PropertyChanged(ns, nameof(ns.StartHeight), () => ns.StartHeight = 1);
            Assert.PropertyChanged(ns, nameof(ns.UserAgent), () => ns.UserAgent = "Foo");
        }

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

        [Fact]
        public async void ReStartDisconnectTimerTest()
        {
            var ns = new NodeStatus();
            ns.StartDisconnectTimer(TimeSpan.FromSeconds(3).TotalMilliseconds);
            Assert.False(ns.IsDisconnected);

            await Task.Delay(TimeSpan.FromSeconds(2));
            ns.ReStartDisconnectTimer();
            Assert.False(ns.IsDisconnected);

            await Task.Delay(TimeSpan.FromSeconds(2));
            ns.ReStartDisconnectTimer();
            Assert.False(ns.IsDisconnected);

            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.True(ns.IsDisconnected);
        }

        [Fact]
        public async void StartDisconnectTimerTest()
        {
            var ns = new NodeStatus();
            ns.StartDisconnectTimer(TimeSpan.FromSeconds(2).TotalMilliseconds);
            Assert.False(ns.IsDisconnected);
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.True(ns.IsDisconnected);
        }

        [Fact]
        public async void StopDisconnectTimerTest()
        {
            var ns = new NodeStatus();
            ns.StartDisconnectTimer(TimeSpan.FromSeconds(2).TotalMilliseconds);
            Assert.False(ns.IsDisconnected);
            ns.StopDisconnectTimer();
            await Task.Delay(TimeSpan.FromSeconds(4));
            Assert.False(ns.IsDisconnected);
        }
    }
}
