// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages.MessagePayloads
{
    public class InvPayloadTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var invs = new Inventory[]
            {
                new Inventory(InventoryType.Block, new byte[32]),
                new Inventory(InventoryType.CompactBlock, Helper.GetBytes(32))
            };
            var pl = new InvPayload(invs);

            Assert.Equal(PayloadType.Inv, pl.PayloadType);
            Assert.Same(invs, pl.InventoryList);
        }

        [Fact]
        public void Constructor_MaxItemTest()
        {
            var pl = new InvPayload(new Inventory[InvPayload.MaxInvCount]);
            Assert.Equal(InvPayload.MaxInvCount, pl.InventoryList.Length);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new InvPayload(null));
            Assert.Throws<ArgumentNullException>(() => new InvPayload(new Inventory[0]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new InvPayload(new Inventory[InvPayload.MaxInvCount + 1]));
        }

        public static IEnumerable<object[]> GetSerCases()
        {
            yield return new object[]
            {
                new Inventory[]
                {
                    new Inventory(InventoryType.WTx, Helper.GetBytes(32))
                },
                Helper.HexToBytes("01"+"05000000"+Helper.GetBytesHex(32))
            };
            yield return new object[]
            {
                new Inventory[]
                {
                    new Inventory(InventoryType.WTx, Helper.GetBytes(32)),
                    new Inventory(InventoryType.FilteredBlock, new byte[32])
                },
                Helper.HexToBytes("02"+"05000000"+Helper.GetBytesHex(32)
                                      +"03000000"+"0000000000000000000000000000000000000000000000000000000000000000")
            };
            yield return new object[]
            {
                Enumerable.Repeat(new Inventory(InventoryType.Block, new byte[32]), 253).ToArray(),
                Helper.HexToBytes("fdfd00"+
                  string.Concat(
                    Enumerable.Repeat("02000000"+"0000000000000000000000000000000000000000000000000000000000000000", 253))),
            };
        }
        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void SerializeTest(Inventory[] items, byte[] expected)
        {
            var pl = new InvPayload(items);
            var stream = new FastStream((items.Length * Inventory.Size) + 2);
            pl.Serialize(stream);
            byte[] actual = stream.ToByteArray();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void TryDeserializeTest(Inventory[] items, byte[] data)
        {
            var pl = new InvPayload();
            var stream = new FastStreamReader(data);
            bool success = pl.TryDeserialize(stream, out string error);

            Assert.True(success, error);
            Assert.Null(error);
            Assert.Equal(items.Length, pl.InventoryList.Length);
            for (int i = 0; i < items.Length; i++)
            {
                Assert.Equal(items[i].InvType, pl.InventoryList[i].InvType);
                Assert.Equal(items[i].Hash, pl.InventoryList[i].Hash);
            }
        }

        [Fact]
        public void TryDeserialize_0Count_Test()
        {
            var pl = new InvPayload();
            var stream = new FastStreamReader(new byte[1]);
            bool success = pl.TryDeserialize(stream, out string error);

            Assert.True(success, error);
            Assert.Null(error);
            Assert.Empty(pl.InventoryList);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, "Stream can not be null." };
            yield return new object[] { new FastStreamReader(new byte[0]), "Count is too big or an invalid CompactInt." };
            yield return new object[] { new FastStreamReader(new byte[] { 1 }), Err.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0xfe, 0x00, 0x00, 0x01, 0x00 }), // Valid but big CompactInt
                "Count is too big or an invalid CompactInt."
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0xfd, 0x51, 0xc3 }), // Bigger than 50k
                "Maximum number of allowed inventory was exceeded."
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, string expError)
        {
            var pl = new InvPayload();
            bool success = pl.TryDeserialize(stream, out string error);

            Assert.False(success);
            Assert.Equal(expError, error);
        }
    }
}
