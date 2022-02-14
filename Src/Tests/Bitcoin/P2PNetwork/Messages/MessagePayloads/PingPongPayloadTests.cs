// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class PingPongPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            PingPayload ping = new();
            ping.SetToCurrentTime();
            PongPayload pong = new(123);

            Assert.NotEqual(0L, ping.Nonce);
            Assert.Equal(PayloadType.Ping, ping.PayloadType);
            Assert.Equal(123L, pong.Nonce);
            Assert.Equal(PayloadType.Pong, pong.PayloadType);
        }

        [Fact]
        public void SerializeTest()
        {
            PingPayload ping = new(1);
            FastStream stream = new(8);
            ping.Serialize(stream);

            Assert.Equal(new byte[8] { 1, 0, 0, 0, 0, 0, 0, 0 }, stream.ToByteArray());
        }

        [Fact]
        public void TryDeserializeTest()
        {
            PingPayload ping = new();
            FastStreamReader stream = new(new byte[8] { 2, 0, 0, 0, 0, 0, 0, 0 });
            bool b = ping.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(2L, ping.Nonce);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(new byte[1]), Errors.EndOfStream };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, Errors expErr)
        {
            PingPayload ping = new();

            bool b = ping.TryDeserialize(stream, out Errors error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
