// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System.Net;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class VersionPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            VersionPayload pl = new();
            Assert.Equal(PayloadType.Version, pl.PayloadType);
        }

        [Fact]
        public void SerializeTest()
        {
            VersionPayload pl = new()
            {
                Version = 70002,
                Services = NodeServiceFlags.NodeNetwork,
                Timestamp = 1415483324,
                ReceivingNodeNetworkAddress = new NetworkAddress(NodeServiceFlags.NodeNetwork, IPAddress.Parse("198.27.100.9"), 8333),
                TransmittingNodeNetworkAddress = new NetworkAddress(NodeServiceFlags.NodeNetwork, IPAddress.Parse("203.0.113.192"), 8333),
                Nonce = 17893779652077781010,
                UserAgent = "/Satoshi:0.9.3/",
                StartHeight = 329167,
                Relay = true
            };
            FastStream stream = new();
            pl.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes("721101000100000000000000bc8f5e5400000000010000000000000000000000000000000000ffffc61b6409208d010000000000000000000000000000000000ffffcb0071c0208d128035cbc97953f80f2f5361746f7368693a302e392e332fcf05050001");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            VersionPayload pl = new();
            byte[] data = Helper.HexToBytes("721101000100000000000000bc8f5e5400000000010000000000000000000000000000000000ffffc61b6409208d010000000000000000000000000000000000ffffcb0071c0208d128035cbc97953f80f2f5361746f7368693a302e392e332fcf05050001");
            bool b = pl.TryDeserialize(new FastStreamReader(data), out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
        }
    }
}
