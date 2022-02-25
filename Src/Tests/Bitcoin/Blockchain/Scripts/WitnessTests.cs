// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts
{
    public class WitnessTests
    {
        [Fact]
        public void ConstructorTest()
        {
            Witness wit = new();
            Assert.Empty(wit.Items);
        }

        [Fact]
        public void Constructor_FromBytesTest()
        {
            byte[][] data = new byte[][] { new byte[] { 1, 2, 3 }, new byte[] { 10, 20 }, new byte[] { 255, 255 } };
            Witness wit = new(data);
            Assert.Equal(data, wit.Items);
        }

        [Fact]
        public void Constructor_FromNullTest()
        {
            Witness wit = new(null);
            Assert.Empty(wit.Items);
        }

        [Fact]
        public void Constructor_NullExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new Witness(new byte[][] { Array.Empty<byte>(), null, new byte[1] }));
        }

        public static IEnumerable<object[]> GetSerSizeCases()
        {
            yield return new object[] { null, 1 };
            yield return new object[] { Array.Empty<byte[]>(), 1 };
            yield return new object[] { new byte[][] { Array.Empty<byte>() }, 2 };
            yield return new object[] { new byte[][] { Array.Empty<byte>(), Array.Empty<byte>() }, 3 };
            yield return new object[]
            {
                new byte[][] { Helper.GetBytes(253) },
                1 + 3 + 253 // count + 0xfdfd00 + 253
            };
            yield return new object[]
            {
                Enumerable.Repeat(Array.Empty<byte>(), 253).ToArray(),
                3 + 253 // 0xfdfd00 + 253*1
            };
            yield return new object[]
            {
                Enumerable.Repeat(new byte[1] { 255 }, 253).ToArray(),
                3 + (253*2) // 0xfdfd00 + (01ff)x253
            };
        }
        [Theory]
        [MemberData(nameof(GetSerSizeCases))]
        public void AddSerializedSizeTest(byte[][] items, int expectedSize)
        {
            SizeCounter counter = new();
            Witness wit = new(items);
            wit.AddSerializedSize(counter);
            Assert.Equal(expectedSize, counter.Size);
        }

        public static IEnumerable<object[]> GetSerCases()
        {
            yield return new object[] { null, new byte[1] };
            yield return new object[] { Array.Empty<byte[]>(), new byte[1] };
            yield return new object[]
            {
                new byte[][] { Array.Empty<byte>(), new byte[] { 1, 2, 3 }, new byte[] { 0x53 } },
                new byte[]
                {
                    3, // item count
                    0x00, // OP_0
                    3, 1, 2, 3, // Push 3 bytes
                    1, 0x53 // OP_3
                }
            };
            yield return new object[]
            {
                new byte[][] { Helper.GetBytes(77) }, // 77 is 0x4d as CompactInt and 0x4c4d as StackInt
                Helper.ConcatBytes(79, new byte[]
                {
                    1, // item count
                    0x4d, // Push length as CompactInt
                },
                Helper.GetBytes(77))
            };
            yield return new object[]
            {
                new byte[][] { Helper.GetBytes(255) }, // 255 is 0xfdff00 as CompactInt and 0x4cff as StackInt
                Helper.ConcatBytes(259, new byte[]
                {
                    1, // item count
                    0xfd, // Push length as CompactInt
                    0xff,
                    0x00
                },
                Helper.GetBytes(255))
            };
        }
        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void SerializeTest(byte[][] items, byte[] expected)
        {
            Witness wit = new() { Items = items };
            FastStream stream = new(expected.Length); // Setting length for small optimization
            wit.Serialize(stream);

            Assert.Equal(expected, stream.ToByteArray());
        }

        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void TryDeserializeTest(byte[][] expected, byte[] data)
        {
            Witness wit = new();
            FastStreamReader stream = new(data);
            bool b = wit.TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expected ?? Array.Empty<byte[]>(), wit.Items);
        }

        public static IEnumerable<object[]> GetSerFailCases()
        {
            yield return new object[] { new byte[] { 253 }, Errors.ShortCompactInt2 };
            yield return new object[] { new byte[] { 0xfe, 0x00, 0x00, 0x00, 0x80 }, Errors.WitnessCountOverflow };
            yield return new object[] { new byte[] { 1 }, Errors.EndOfStream };
        }
        [Theory]
        [MemberData(nameof(GetSerFailCases))]
        public void TryDeserialize_FailTest(byte[] data, Errors expErr)
        {
            Witness wit = new();
            FastStreamReader stream = new(data);
            bool b = wit.TryDeserialize(stream, out Errors error);

            Assert.False(b);
            Assert.Equal(expErr, error);
        }

        [Fact]
        public void TryDeserialize_NullStreamTest()
        {
            Witness wit = new();
            bool b = wit.TryDeserialize(null, out Errors error);

            Assert.False(b);
            Assert.Equal(Errors.NullStream, error);
        }

        [Fact]
        public void SetToP2WPKH_CompressedTest()
        {
            Witness wit = new();
            wit.SetToP2WPKH(Helper.ShortSig1, KeyHelper.Pub1);
            byte[][] expected = new byte[][] { Helper.ShortSig1Bytes, KeyHelper.Pub1CompBytes };

            Assert.Equal(expected, wit.Items);
        }

        [Fact]
        public void SetToP2WPKH_UncompressedTest()
        {
            Witness wit = new();
            wit.SetToP2WPKH(Helper.ShortSig1, KeyHelper.Pub1, false);
            byte[][] expected = new byte[][] { Helper.ShortSig1Bytes, KeyHelper.Pub1UnCompBytes };

            Assert.Equal(expected, wit.Items);
        }

        [Fact]
        public void SetToP2WPKH_NullExceptionTest()
        {
            Witness wit = new();
            Assert.Throws<ArgumentNullException>(() => wit.SetToP2WPKH(null, KeyHelper.Pub1));
            Assert.Throws<ArgumentNullException>(() => wit.SetToP2WPKH(Helper.ShortSig1, null));
        }
    }
}
