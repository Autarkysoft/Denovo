// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using Tests.Bitcoin.Blockchain;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class BlockPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            Assert.Throws<ArgumentNullException>(() => new BlockPayload(null));
        }

        [Fact]
        public void SerializeTest()
        {
            BlockPayload pl = new()
            {
                BlockData = new MockSerializableBlock(new byte[] { 1, 2, 3 })
            };

            FastStream stream = new(3);
            pl.Serialize(stream);
            byte[] actual = pl.Serialize();
            byte[] expected = new byte[] { 1, 2, 3 };

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryDeserializeTest()
        {
            BlockPayload pl = new()
            {
                BlockData = new MockDeserializableBlock(0, 3)
            };
            FastStreamReader stream = new(new byte[3]);
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            // Mock block has its own tests.
            Assert.Equal(PayloadType.Block, pl.PayloadType);
        }

        [Fact]
        public void TryDeserialize_DefaultFieldTest()
        {
            BlockPayload pl = new(); // field is not set
            // Block 00000000d1145790a8694403d4063f323d499e655c83426834d4ce2f8dd4a2ee
            FastStreamReader stream = new(Helper.HexToBytes("0100000055bd840a78798ad0da853f68974f3d183e2bd1db6a842c1feecf222a00000000ff104ccb05421ab93e63f8c3ce5c2c2e9dbb37de2764b3a3175c8166562cac7d51b96a49ffff001d283e9e700201000000010000000000000000000000000000000000000000000000000000000000000000ffffffff0704ffff001d0102ffffffff0100f2052a01000000434104d46c4968bde02899d2aa0963367c7a6ce34eec332b32e42e5f3407e052d64ac625da6f0718e7b302140434bd725706957c092db53805b821a85b23a7ac61725bac000000000100000001c997a5e56e104102fa209c6a852dd90660a20b2d9c352423edce25857fcd3704000000004847304402204e45e16932b8af514961a1d3a1a25fdf3f4f7732e9d624c6c61548ab5fb8cd410220181522ec8eca07de4860a4acdd12909d831cc56cbbac4622082221a8768d1d0901ffffffff0200ca9a3b00000000434104ae1a62fe09c5f51b13905f07f06b99a2f7159b2225f374cd378d71302fa28414e7aab37397f554a7df5f142c21c1b7303b8a0626f1baded5c72a704f7e6cd84cac00286bee0000000043410411db93e1dcdb8a016b49840f8c53bc1eb68a382e97b1482ecad7b148a6909a5cb2e0eaddfb84ccf9744464f82e160bfa9b8b64f9d4c03f999b8643f656b412a3ac00000000"));
            bool b = pl.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(2, pl.BlockData.TransactionList.Length);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[]
            {
                new FastStreamReader(new byte[1]),
                new MockDeserializableBlock(0, 1, true),
                Errors.ForTesting
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, MockDeserializableBlock block, Errors expErr)
        {
            BlockPayload pl = new()
            {
                BlockData = block
            };

            bool b = pl.TryDeserialize(stream, out Errors error);
            Assert.False(b);
            Assert.Equal(expErr, error);
            // Mock block has its own tests.
        }
    }
}
