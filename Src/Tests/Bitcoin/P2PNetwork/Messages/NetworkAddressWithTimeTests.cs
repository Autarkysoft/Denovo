// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages
{
    public class NetworkAddressWithTimeTests
    {
        [Fact]
        public void SetTimeToNowTest()
        {
            var addr = new NetworkAddressWithTime();
            Assert.Equal(0U, addr.Time);

            addr.SetTimeToNow();
            uint actual = (uint)UnixTimeStamp.GetEpochUtcNow();
            bool b = (actual == addr.Time) || (actual - addr.Time < 2);

            Assert.True(b);
        }

        [Fact]
        public void GetDateTimeTest()
        {
            var addr = new NetworkAddressWithTime()
            {
                Time = 1414012889U
            };

            DateTime actual = addr.GetDateTime();
            // Converted using https://www.epochconverter.com/
            DateTime expected = new DateTime(2014, 10, 22, 21, 21, 29);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeTest()
        {
            var addr = new NetworkAddressWithTime(NodeServiceFlags.NodeNetwork, IPAddress.Parse("192.0.2.51"), 8333, 1414012889);
            FastStream stream = new FastStream(26);
            addr.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.HexToBytes("d91f4854010000000000000000000000000000000000ffffc0000233208d");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            var addr = new NetworkAddressWithTime();
            var stream = new FastStreamReader(Helper.HexToBytes("d91f4854010000000000000000000000000000000000ffffc0000233208d"));
            bool b = addr.TryDeserialize(stream, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(1414012889U, addr.Time);
            Assert.Equal(NodeServiceFlags.NodeNetwork, addr.NodeServices);
            Assert.Equal(IPAddress.Parse("192.0.2.51"), addr.NodeIP);
            Assert.Equal((ushort)8333, addr.NodePort);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, "Stream can not be null." };
            yield return new object[] { new FastStreamReader(new byte[1]), Err.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(Helper.HexToBytes("010000000000000000000000000000000000ffffc0000233208d")),
                Err.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, string expErr)
        {
            var addr = new NetworkAddressWithTime();

            bool b = addr.TryDeserialize(stream, out string error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
