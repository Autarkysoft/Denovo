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
    public class SendCmpctPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            SendCmpctPayload pl = new(true, 2);

            Assert.Equal(PayloadType.SendCmpct, pl.PayloadType);
            Assert.True(pl.Announce);
            Assert.Equal(2UL, pl.CmpctVersion);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SendCmpctPayload(true, 1000));
        }

        [Fact]
        public void SerializeTest()
        {
            SendCmpctPayload pl = new(true, 1);
            FastStream stream = new(9);
            pl.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[9] { 1, 1, 0, 0, 0, 0, 0, 0, 0 };

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetDeserCases()
        {
            yield return new object[] { new byte[9] { 1, 0, 0, 0, 0, 0, 0, 0, 0 }, true, 0 };
            yield return new object[] { new byte[9] { 0, 1, 0, 0, 0, 0, 0, 0, 0 }, false, 1 };
            yield return new object[] { new byte[9] { 1, 1, 2, 3, 4, 5, 6, 7, 8 }, true, 578437695752307201UL };
        }
        [Theory]
        [MemberData(nameof(GetDeserCases))]
        public void TryDeserializeTest(byte[] data, bool ann, ulong ver)
        {
            SendCmpctPayload pl = new();
            bool success = pl.TryDeserialize(new FastStreamReader(data), out Errors error);

            Assert.True(success, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(ann, pl.Announce);
            Assert.Equal(ver, pl.CmpctVersion);

            // Test Serialize here to cover undefined (future) cases for version
            FastStream stream = new(9);
            pl.Serialize(stream);
            Assert.Equal(data, stream.ToByteArray());
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(new byte[8]), Errors.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[9] { 2, 0, 0, 0, 0, 0, 0, 0, 0 }),
                Errors.MsgSendCmpctInvalidAnn
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, Errors expErr)
        {
            SendCmpctPayload pl = new();
            bool success = pl.TryDeserialize(stream, out Errors error);

            Assert.False(success);
            Assert.Equal(expErr, error);
        }
    }
}
