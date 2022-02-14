// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class EmptyPayloadTests
    {
        public static IEnumerable<object[]> GetSerCases()
        {
            yield return new object[] { new FilterClearPayload(), PayloadType.FilterClear };
            yield return new object[] { new GetAddrPayload(), PayloadType.GetAddr };
            yield return new object[] { new MemPoolPayload(), PayloadType.MemPool };
            yield return new object[] { new SendHeadersPayload(), PayloadType.SendHeaders };
            yield return new object[] { new VerackPayload(), PayloadType.Verack };
        }
        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void PayloadTest(IMessagePayload payload, PayloadType expPlType)
        {
            FastStream stream = new();
            payload.Serialize(stream);

            Assert.Empty(stream.ToByteArray());
            Assert.Equal(expPlType, payload.PayloadType);
            Assert.Equal(new byte[] { 0x5d, 0xf6, 0xe0, 0xe2 }, payload.GetChecksum());
        }

        internal class MockEmptyPayload : EmptyPayloadBase
        {
            public override PayloadType PayloadType => throw new NotImplementedException();
        }

        [Fact]
        public void SerializeTest()
        {
            MockEmptyPayload mock = new();
            Assert.Empty(mock.Serialize());
        }

        [Fact]
        public void TryDeserializeTest()
        {
            MockEmptyPayload mock = new();
            FastStreamReader stream = new(Array.Empty<byte>());
            bool b = mock.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
        }
    }
}
