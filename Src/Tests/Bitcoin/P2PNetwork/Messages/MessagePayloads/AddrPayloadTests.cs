// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class AddrPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            Assert.Throws<ArgumentNullException>(() => new AddrPayload(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AddrPayload(new NetworkAddressWithTime[1001]));
        }

        private readonly NetworkAddressWithTime addr1 =
            new(NodeServiceFlags.NodeNetwork, IPAddress.Parse("192.0.2.51"), 8333, 1414012889);
        private readonly NetworkAddressWithTime addr2 =
            new(NodeServiceFlags.NodeXThin | NodeServiceFlags.NodeWitness, IPAddress.Parse("123.45.67.89"), 9823, 1581412378);
        private readonly string addr1Hex = "d91f4854010000000000000000000000000000000000ffffc0000233208d";
        private readonly string addr2Hex = "1a70425e180000000000000000000000000000000000ffff7b2d4359265f";

        [Fact]
        public void SerializeTest()
        {
            AddrPayload pl = new(new NetworkAddressWithTime[] { addr1, addr2 });
            FastStream stream = new(1 + (2 * 30));
            pl.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes("02" + addr1Hex + addr2Hex);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            AddrPayload pl = new();
            FastStreamReader stream = new(Helper.HexToBytes("02" + addr1Hex + addr2Hex));
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(2, pl.Addresses.Length);

            Assert.Equal(addr1.Time, pl.Addresses[0].Time);
            Assert.Equal(addr1.NodeServices, pl.Addresses[0].NodeServices);
            Assert.Equal(addr1.NodeIP, pl.Addresses[0].NodeIP);
            Assert.Equal(addr1.NodePort, pl.Addresses[0].NodePort);

            Assert.Equal(addr2.Time, pl.Addresses[1].Time);
            Assert.Equal(addr2.NodeServices, pl.Addresses[1].NodeServices);
            Assert.Equal(addr2.NodeIP, pl.Addresses[1].NodeIP);
            Assert.Equal(addr2.NodePort, pl.Addresses[1].NodePort);

            Assert.Equal(PayloadType.Addr, pl.PayloadType);
        }

        [Fact]
        public void TryDeserialize_EmptyListTest()
        {
            AddrPayload pl = new();
            FastStreamReader stream = new(Helper.HexToBytes("00"));
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Empty(pl.Addresses);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 253 }),
                Errors.ShortCompactInt2
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0xfd, 0xe9, 0x03 }),
                Errors.MsgAddrCountOverflow
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 2 }),
                Errors.EndOfStream
            };
            yield return new object[]
            {
                new FastStreamReader(Helper.HexToBytes("01d91f4854010000000000000000000000000000000000ffffc000023320")),
                Errors.EndOfStream
            };
            yield return new object[]
            {
                new FastStreamReader(Helper.HexToBytes("02d91f4854010000000000000000000000000000000000ffffc0000233208d1a70425e1f0400000000000000000000000000000000ffff7b2d435926")),
                Errors.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, Errors expErr)
        {
            AddrPayload pl = new();
            bool b = pl.TryDeserialize(stream, out Errors error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
