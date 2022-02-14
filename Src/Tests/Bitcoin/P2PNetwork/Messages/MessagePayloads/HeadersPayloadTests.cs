// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Bitcoin.Blockchain.Blocks;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class HeadersPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            BlockHeader[] hds = new BlockHeader[1];
            HeadersPayload pl = new(hds);

            Assert.Same(hds, pl.Headers);
            Assert.Equal(PayloadType.Headers, pl.PayloadType);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new HeadersPayload(null));
            Assert.Throws<ArgumentNullException>(() => new HeadersPayload(Array.Empty<BlockHeader>()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HeadersPayload(new BlockHeader[HeadersPayload.MaxCount + 1]));
        }

        [Fact]
        public void SerializeTest()
        {
            HeadersPayload pl = new(new BlockHeader[] { BlockHeaderTests.GetSampleBlockHeader() });
            FastStream stream = new(BlockHeader.Size + 2);
            pl.Serialize(stream);

            byte[] hd = BlockHeaderTests.GetSampleBlockHeaderBytes();
            byte[] expected = new byte[BlockHeader.Size + 2];
            expected[0] = 1;
            Buffer.BlockCopy(hd, 0, expected, 1, BlockHeader.Size);
            expected[^1] = 0;

            Assert.Equal(expected, stream.ToByteArray());
        }

        [Fact]
        public void Serialize_MultiTest()
        {
            int count = 254;
            int totalSize = 3 + (BlockHeader.Size + 1) * count;

            IEnumerable<BlockHeader> hds = Enumerable.Repeat(BlockHeaderTests.GetSampleBlockHeader(), count);
            HeadersPayload pl = new(hds.ToArray());
            FastStream stream = new(totalSize);
            pl.Serialize(stream);

            byte[] hd = BlockHeaderTests.GetSampleBlockHeaderBytes();
            byte[] expected = new byte[totalSize];
            int j = 0;
            expected[j++] = 253;
            expected[j++] = 254;
            expected[j++] = 0;
            for (int i = 0; i < count; i++)
            {
                Buffer.BlockCopy(hd, 0, expected, j, BlockHeader.Size);
                j += BlockHeader.Size;
                expected[j++] = 0;
            }

            Assert.Equal(expected, stream.ToByteArray());
        }

        [Fact]
        public void TryDeserializeTest()
        {
            byte[] hd = BlockHeaderTests.GetSampleBlockHeaderBytes();
            byte[] data = new byte[(BlockHeader.Size * 2) + 3];
            data[0] = 2;
            Buffer.BlockCopy(hd, 0, data, 1, BlockHeader.Size);
            data[BlockHeader.Size + 1] = 0;
            Buffer.BlockCopy(hd, 0, data, BlockHeader.Size + 2, BlockHeader.Size);
            data[(BlockHeader.Size * 2) + 2] = 0;

            HeadersPayload pl = new();
            FastStreamReader stream = new(data);
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(2, pl.Headers.Length);
            Assert.Equal(hd, pl.Headers[0].Serialize());
            Assert.Equal(hd, pl.Headers[1].Serialize());
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(new byte[1] { 1 }), Errors.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0xfe, 0x00, 0x00, 0x01, 0x00 }), Errors.InvalidCompactInt
            };
            yield return new object[] { new FastStreamReader(new byte[] { 0xfd, 0xd1, 0x07 }), Errors.MsgHeaderCountOverflow };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, Errors expErr)
        {
            HeadersPayload pl = new();

            bool b = pl.TryDeserialize(stream, out Errors error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
