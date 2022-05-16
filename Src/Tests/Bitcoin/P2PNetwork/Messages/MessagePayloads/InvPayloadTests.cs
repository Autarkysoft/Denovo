// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
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
            Inventory[] invs = new Inventory[]
            {
                new Inventory(InventoryType.Block, Digest256.Zero),
                new Inventory(InventoryType.CompactBlock, new Digest256(Helper.GetBytes(32)))
            };
            InvPayload pl = new(invs);

            Assert.Equal(PayloadType.Inv, pl.PayloadType);
            Assert.Same(invs, pl.InventoryList);
        }

        [Fact]
        public void Constructor_MaxItemTest()
        {
            InvPayload pl = new(new Inventory[InvPayload.MaxInvCount]);
            Assert.Equal(InvPayload.MaxInvCount, pl.InventoryList.Length);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new InvPayload(null));
            Assert.Throws<ArgumentNullException>(() => new InvPayload(Array.Empty<Inventory>()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new InvPayload(new Inventory[InvPayload.MaxInvCount + 1]));
        }

        public static IEnumerable<object[]> GetSerCases()
        {
            yield return new object[]
            {
                new Inventory[]
                {
                    new Inventory(InventoryType.WTx, new Digest256(Helper.GetBytes(32)))
                },
                Helper.HexToBytes("01"+"05000000"+Helper.GetBytesHex(32))
            };
            yield return new object[]
            {
                new Inventory[]
                {
                    new Inventory(InventoryType.WTx, new Digest256(Helper.GetBytes(32))),
                    new Inventory(InventoryType.FilteredBlock, Digest256.Zero)
                },
                Helper.HexToBytes("02"+"05000000"+Helper.GetBytesHex(32)
                                      +"03000000"+"0000000000000000000000000000000000000000000000000000000000000000")
            };
            yield return new object[]
            {
                Enumerable.Repeat(new Inventory(InventoryType.Block, Digest256.Zero), 253).ToArray(),
                Helper.HexToBytes("fdfd00"+
                  string.Concat(
                    Enumerable.Repeat("02000000"+"0000000000000000000000000000000000000000000000000000000000000000", 253))),
            };
        }
        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void SerializeTest(Inventory[] items, byte[] expected)
        {
            InvPayload pl = new(items);
            FastStream stream = new((items.Length * Inventory.Size) + 2);
            pl.Serialize(stream);
            byte[] actual = stream.ToByteArray();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void TryDeserializeTest(Inventory[] items, byte[] data)
        {
            InvPayload pl = new();
            FastStreamReader stream = new(data);
            bool success = pl.TryDeserialize(stream, out Errors error);

            Assert.True(success, error.Convert());
            Assert.Equal(Errors.None, error);
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
            InvPayload pl = new();
            FastStreamReader stream = new(new byte[1]);
            bool success = pl.TryDeserialize(stream, out Errors error);

            Assert.True(success, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Empty(pl.InventoryList);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(Array.Empty<byte>()), Errors.InvalidCompactInt };
            yield return new object[] { new FastStreamReader(new byte[] { 1 }), Errors.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0xfe, 0x00, 0x00, 0x01, 0x00 }), // Valid but big CompactInt
                Errors.InvalidCompactInt
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0xfd, 0x51, 0xc3 }), // Bigger than 50k
                Errors.MsgInvCountOverflow
        };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, Errors expError)
        {
            InvPayload pl = new();
            bool success = pl.TryDeserialize(stream, out Errors error);

            Assert.False(success);
            Assert.Equal(expError, error);
        }
    }
}
