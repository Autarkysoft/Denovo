﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Transactions
{
    public class TxOutTests
    {
        [Fact]
        public void ConstructorTest()
        {
            // Amount can be set to zero and pubkey script to null
            TxOut tx = new(0, null);
            Assert.Equal(0UL, tx.Amount);
            Assert.NotNull(tx.PubScript);

            // Can't set amount to bigger than supply
            Assert.Throws<ArgumentOutOfRangeException>(() => new TxOut(Constants.TotalSupply + 1, null));
        }

        [Fact]
        public void AddSerializedSize()
        {
            SizeCounter counter = new();
            TxOut tx = new(0, new MockSerializablePubScript(new byte[3], 3));
            tx.AddSerializedSize(counter);
            Assert.Equal(8 + 1 + 3, counter.Size);
        }

        [Fact]
        public void SerializeTest()
        {
            MockSerializablePubScript scr = new(Helper.GetBytes(5), 5);
            TxOut tx = new(12_633_113_1334_7895, scr);
            FastStream stream = new(8 + 5 + 1);
            tx.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            // 8 bytes amount + 1 byte push + 5 byte script
            byte[] expected = new byte[8 + 5 + 1];
            Buffer.BlockCopy(Helper.HexToBytes("37a51296f97c04"), 0, expected, 0, 7);
            expected[8] = 5;
            Buffer.BlockCopy(Helper.GetBytes(5), 0, expected, 9, 5);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serialize_EmptyScrTest()
        {
            TxOut tx = new(0, null);
            FastStream stream = new(9);
            tx.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            // 8 bytes amount + 1 byte empty script
            byte[] expected = new byte[9];

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void SerializeSigHashSingleTest()
        {
            FastStream stream = new(9);
            TxOut tx = new();
            tx.SerializeSigHashSingle(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[9];
            long val = -1;
            expected[0] = (byte)val;
            expected[1] = (byte)(val >> 8);
            expected[2] = (byte)(val >> 16);
            expected[3] = (byte)(val >> 24);
            expected[4] = (byte)(val >> 32);
            expected[5] = (byte)(val >> 40);
            expected[6] = (byte)(val >> 48);
            expected[7] = (byte)(val >> 56);
            expected[8] = 0; // Empty script

            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetDeserCases()
        {
            yield return new object[] { new byte[9], new MockDeserializablePubScript(8, 1), 0 };
            yield return new object[]
            {
                new byte[11] { 26, 76, 39, 140, 143, 113, 0, 0, 1, 2, 3 },
                new MockDeserializablePubScript(8, 3),
                1_248_613_4564_7642
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserCases))]
        public void TryDeserializeTest(byte[] data, MockDeserializablePubScript scr, ulong expAmount)
        {
            TxOut tx = new()
            {
                PubScript = scr
            };
            FastStreamReader stream = new(data);
            bool b = tx.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expAmount, tx.Amount);
            // Mock script has its own tests.
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, null, Errors.NullStream };
            yield return new object[] { new FastStreamReader(new byte[7]), null, Errors.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[8]{ 1, 64, 7, 90, 240, 117, 7, 0 }),
                null,
                Errors.TxAmountOverflow
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[8]{ 1, 0, 0, 0, 0, 0, 0, 0 }),
                new MockDeserializablePubScript(8, 0, true),
                Errors.ForTesting
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, MockDeserializablePubScript scr, Errors expErr)
        {
            TxOut tx = new()
            {
                PubScript = scr
            };
            bool b = tx.TryDeserialize(stream, out Errors error);

            Assert.False(b);
            Assert.Equal(expErr, error);
            // Mock script has its own tests.
        }
    }
}
