// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
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
            yield return new object[]
            {
                new Message(new FilterClearPayload(), NetworkType.MainNet),
                Helper.HexToBytes("f9beb4d9"+"66696c746572636c65617200"+"00000000"+"5df6e0e2"),
                PayloadType.FilterClear
            };
            yield return new object[]
            {
                new Message(new GetAddrPayload(), NetworkType.MainNet),
                Helper.HexToBytes("f9beb4d9"+"676574616464720000000000"+"00000000"+"5df6e0e2"),
                PayloadType.GetAddr
            };
            yield return new object[]
            {
                new Message(new MemPoolPayload(), NetworkType.MainNet),
                Helper.HexToBytes("f9beb4d9"+"6d656d706f6f6c0000000000"+"00000000"+"5df6e0e2"),
                PayloadType.MemPool
            };
            yield return new object[]
            {
                new Message(new SendHeadersPayload(), NetworkType.MainNet),
                Helper.HexToBytes("f9beb4d9"+"73656e646865616465727300"+"00000000"+"5df6e0e2"),
                PayloadType.SendHeaders
            };
            yield return new object[]
            {
                new Message(new VerackPayload(), NetworkType.MainNet),
                Helper.HexToBytes("f9beb4d9"+"76657261636b000000000000"+"00000000"+"5df6e0e2"),
                PayloadType.Verack
            };
        }
        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void PayloadTest(Message msg, byte[] expSer, PayloadType expPlType)
        {
            FastStream stream = new FastStream();
            msg.Serialize(stream);
            byte[] actualSer = stream.ToByteArray();

            Assert.Equal(expSer, actualSer);
            Assert.Equal(expPlType, msg.Payload.PayloadType);
        }

        internal class MockEmptyPayload : EmptyPayloadBase
        {
            public override PayloadType PayloadType => throw new NotImplementedException();
        }

        [Fact]
        public void SerializeTest()
        {
            var mock = new MockEmptyPayload();
            Assert.Empty(mock.Serialize());
        }

        [Fact]
        public void TryDeserializeTest()
        {
            var mock = new MockEmptyPayload();
            var stream = new FastStreamReader(new byte[0]);
            bool b = mock.TryDeserialize(stream, out string error);

            Assert.True(b);
            Assert.Null(error);
        }
    }
}
