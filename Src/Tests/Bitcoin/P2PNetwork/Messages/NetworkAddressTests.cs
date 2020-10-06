// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages
{
    public class NetworkAddressTests
    {
        [Fact]
        public void Constructor_NullIpTest()
        {
            NetworkAddress addr = new NetworkAddress(NodeServiceFlags.NodeNetwork, null, 8333);
            Assert.Equal(IPAddress.Loopback, addr.NodeIP);
        }

        [Fact]
        public void SerializeTest()
        {
            NetworkAddress addr = new NetworkAddress(NodeServiceFlags.NodeNetwork, IPAddress.Parse("192.0.2.51"), 8333);
            FastStream stream = new FastStream(26);
            addr.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes("010000000000000000000000000000000000ffffc0000233208d");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            NetworkAddress addr = new NetworkAddress();
            var stream = new FastStreamReader(Helper.HexToBytes("010000000000000000000000000000000000ffffc0000233208d"));
            bool b = addr.TryDeserialize(stream, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(NodeServiceFlags.NodeNetwork, addr.NodeServices);
            Assert.Equal(IPAddress.Parse("192.0.2.51"), addr.NodeIP);
            Assert.Equal((ushort)8333, addr.NodePort);
        }

        [Fact]
        public void TryDeserialize_InvalidServiceTest()
        {
            NetworkAddress addr = new NetworkAddress();
            var stream = new FastStreamReader(Helper.HexToBytes("ffffffffffffffff00000000000000000000ffffc0000233208d"));
            bool b = addr.TryDeserialize(stream, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(ulong.MaxValue, (ulong)addr.NodeServices);
            Assert.Equal(IPAddress.Parse("192.0.2.51"), addr.NodeIP);
            Assert.Equal((ushort)8333, addr.NodePort);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, "Stream can not be null." };
            yield return new object[] { new FastStreamReader(new byte[1]), Err.EndOfStream };
            yield return new object[] { new FastStreamReader(new byte[9]), Err.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(Helper.HexToBytes("010000000000000000000000000000000000ffffc000023320")),
                Err.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, string expErr)
        {
            NetworkAddress addr = new NetworkAddress();

            bool b = addr.TryDeserialize(stream, out string error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }

        public static IEnumerable<object[]> GetEqulsCases()
        {
            var addr1 = new NetworkAddress(NodeServiceFlags.NodeNone, IPAddress.Parse("1.2.3.4"), 111);
            var addr2 = new NetworkAddress(NodeServiceFlags.NodeNone, IPAddress.Parse("1.2.3.4"), 111);
            var addr3 = new NetworkAddress(NodeServiceFlags.NodeNetwork, IPAddress.Parse("1.2.3.4"), 111);
            var addr4 = new NetworkAddress(NodeServiceFlags.NodeNone, IPAddress.Parse("1.2.3.5"), 111);
            var addr5 = new NetworkAddress(NodeServiceFlags.NodeNone, IPAddress.Parse("1.2.3.4"), 112);

            yield return new object[] { addr1, addr1, true };
            yield return new object[] { addr1, addr2, true };
            yield return new object[] { addr1, addr3, true };
            yield return new object[] { addr1, addr4, false };
            yield return new object[] { addr1, addr5, false };
        }
        [Theory]
        [MemberData(nameof(GetEqulsCases))]
        public void EqualsTest(NetworkAddress addr1, NetworkAddress addr2, bool expected)
        {
            Assert.Equal(expected, addr1.Equals(addr2));
            Assert.Equal(expected, addr1.Equals((object)addr2));

            Assert.Equal(expected, addr2.Equals(addr1));
            Assert.Equal(expected, addr2.Equals((object)addr1));

            Assert.True(addr1.Equals(addr1));
            Assert.True(addr1.Equals((object)addr1));

            Assert.True(addr2.Equals(addr2));
            Assert.True(addr2.Equals((object)addr2));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            var addr1 = new NetworkAddress(NodeServiceFlags.NodeNone, IPAddress.Parse("1.2.3.4"), 111);
            var addr2 = new NetworkAddress(NodeServiceFlags.NodeNone, IPAddress.Parse("1.2.3.4"), 111);
            var addr3 = new NetworkAddress(NodeServiceFlags.NodeNetwork, IPAddress.Parse("1.2.3.4"), 111);
            var addr4 = new NetworkAddress(NodeServiceFlags.NodeNone, IPAddress.Parse("1.2.3.5"), 111);
            var addr5 = new NetworkAddress(NodeServiceFlags.NodeNone, IPAddress.Parse("1.2.3.4"), 112);

            int h1 = addr1.GetHashCode();
            int h2 = addr2.GetHashCode();
            int h3 = addr3.GetHashCode();
            int h4 = addr4.GetHashCode();
            int h5 = addr5.GetHashCode();

            Assert.Equal(h1, h2);
            Assert.Equal(h1, h3);
            Assert.NotEqual(h1, h4);
            Assert.NotEqual(h1, h5);
            Assert.NotEqual(h4, h5);
        }
    }
}
