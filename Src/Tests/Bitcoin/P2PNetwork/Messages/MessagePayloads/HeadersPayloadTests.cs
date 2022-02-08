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
            var hds = new BlockHeader[1];
            var pl = new HeadersPayload(hds);

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
            var pl = new HeadersPayload(new BlockHeader[] { BlockHeaderTests.GetSampleBlockHeader() });
            var stream = new FastStream(BlockHeader.Size + 2);
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

            var hds = Enumerable.Repeat(BlockHeaderTests.GetSampleBlockHeader(), count);
            var pl = new HeadersPayload(hds.ToArray());
            var stream = new FastStream(totalSize);
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

            var pl = new HeadersPayload();
            var stream = new FastStreamReader(data);
            bool b = pl.TryDeserialize(stream, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(2, pl.Headers.Length);
            Assert.Equal(hd, pl.Headers[0].Serialize());
            Assert.Equal(hd, pl.Headers[1].Serialize());
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, "Stream can not be null." };
            yield return new object[] { new FastStreamReader(new byte[1] { 1 }), Err.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0xfe, 0x00, 0x00, 0x01, 0x00 }), "Invalid CompactInt format."
            };
            yield return new object[] { new FastStreamReader(new byte[] { 0xfd, 0xd1, 0x07 }), "Header count is too big." };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, string expErr)
        {
            var pl = new HeadersPayload();

            bool b = pl.TryDeserialize(stream, out string error);
            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
