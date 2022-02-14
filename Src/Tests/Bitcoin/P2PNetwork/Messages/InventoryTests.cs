// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork.Messages
{
    public class InventoryTests
    {
        [Fact]
        public void ConstructorTest()
        {
            Inventory inv = new((InventoryType)10000, Helper.GetBytes(32));

            Assert.Equal((InventoryType)10000, inv.InvType);
            Assert.Equal(Helper.GetBytes(32), inv.Hash);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new Inventory(InventoryType.Block, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Inventory(InventoryType.Block, new byte[31]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Inventory(InventoryType.Block, new byte[33]));
        }

        public static IEnumerable<object[]> GetSerCases()
        {
            yield return new object[]
            {
                InventoryType.FilteredBlock,
                Helper.GetBytes(32),
                Helper.HexToBytes($"03000000{Helper.GetBytesHex(32)}")
            };
            yield return new object[]
            {
                InventoryType.Unknown,
                new byte[32],
                Helper.HexToBytes($"000000000000000000000000000000000000000000000000000000000000000000000000")
            };
            yield return new object[]
            {
                (InventoryType)1241455512, // Always pass on undefined type
                Helper.GetBytes(32),
                Helper.HexToBytes($"981bff49{Helper.GetBytesHex(32)}")
            };
        }
        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void SerializeTest(InventoryType t, byte[] hash, byte[] expected)
        {
            Inventory inv = new(t, hash);
            FastStream stream = new(Inventory.Size);
            inv.Serialize(stream);
            byte[] actual = stream.ToByteArray();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void TryDeserializeTest(InventoryType t, byte[] hash, byte[] data)
        {
            FastStreamReader stream = new(data);
            Inventory inv = new();
            bool actual = inv.TryDeserialize(stream, out Errors error);

            Assert.True(actual, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(t, inv.InvType);
            Assert.Equal(hash, inv.Hash);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(Array.Empty<byte>()), Errors.EndOfStream };
            yield return new object[] { new FastStreamReader(new byte[Inventory.Size - 1]), Errors.EndOfStream };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, Errors expected)
        {
            Inventory inv = new();
            bool actual = inv.TryDeserialize(stream, out Errors error);

            Assert.False(actual);
            Assert.Equal(expected, error);
        }
    }
}
