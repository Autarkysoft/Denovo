// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class Digest256Tests
    {
        [Theory]
        [InlineData(0U)]
        [InlineData(1U)]
        [InlineData(uint.MaxValue)]
        public void Constructor_FromUintTest(uint val)
        {
            Digest256 hash = new(val);
            Assert.Equal(val, hash.b0);
            Assert.Equal(0U, hash.b1);
            Assert.Equal(0U, hash.b2);
            Assert.Equal(0U, hash.b3);
            Assert.Equal(0U, hash.b4);
            Assert.Equal(0U, hash.b5);
            Assert.Equal(0U, hash.b6);
            Assert.Equal(0U, hash.b7);
        }

        [Theory]
        [InlineData(0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U)]
        [InlineData(1U, 2U, 3U, 4U, 5U, 6U, 7U, 8U)]
        [InlineData(0x834aef36, 0x1a6330a5, 0x3b90bcb9, 0x16285ca0, 0x69bc7eae, 0x28d96a90, 0x34e74d06, 0xcaf5616c)]
        public unsafe void Constructor_FromUintArrayTest(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7)
        {
            uint[] array = new[] { u0, u1, u2, u3, u4, u5, u6, u7 };
            Digest256 hash = new(array);
            Assert.Equal(u0, hash.b0);
            Assert.Equal(u1, hash.b1);
            Assert.Equal(u2, hash.b2);
            Assert.Equal(u3, hash.b3);
            Assert.Equal(u4, hash.b4);
            Assert.Equal(u5, hash.b5);
            Assert.Equal(u6, hash.b6);
            Assert.Equal(u7, hash.b7);
        }

        [Theory]
        [InlineData(0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U)]
        [InlineData(1U, 2U, 3U, 4U, 5U, 6U, 7U, 8U)]
        [InlineData(0x834aef36, 0x1a6330a5, 0x3b90bcb9, 0x16285ca0, 0x69bc7eae, 0x28d96a90, 0x34e74d06, 0xcaf5616c)]
        public unsafe void Constructor_FromUintsTest(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7)
        {
            Digest256 hash = new(u0, u1, u2, u3, u4, u5, u6, u7);
            Assert.Equal(u0, hash.b0);
            Assert.Equal(u1, hash.b1);
            Assert.Equal(u2, hash.b2);
            Assert.Equal(u3, hash.b3);
            Assert.Equal(u4, hash.b4);
            Assert.Equal(u5, hash.b5);
            Assert.Equal(u6, hash.b6);
            Assert.Equal(u7, hash.b7);
        }

        [Theory]
        [InlineData(0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U, 0U)]
        [InlineData(0x834aef36, 0x1a6330a5, 0x3b90bcb9, 0x16285ca0, 0x69bc7eae, 0x28d96a90, 0x34e74d06, 0xcaf5616c,
                    0x36ef4a83, 0xa530631a, 0xb9bc903b, 0xa05c2816, 0xae7ebc69, 0x906ad928, 0x064de734, 0x6c61f5ca)]
        public unsafe void Constructor_FromPointerTest(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7,
            uint exp0, uint exp1, uint exp2, uint exp3, uint exp4, uint exp5, uint exp6, uint exp7)
        {
            uint[] array = new[] { u0, u1, u2, u3, u4, u5, u6, u7 };
            fixed (uint* hpt = &array[0])
            {
                Digest256 hash = new(hpt);
                Assert.Equal(exp0, hash.b0);
                Assert.Equal(exp1, hash.b1);
                Assert.Equal(exp2, hash.b2);
                Assert.Equal(exp3, hash.b3);
                Assert.Equal(exp4, hash.b4);
                Assert.Equal(exp5, hash.b5);
                Assert.Equal(exp6, hash.b6);
                Assert.Equal(exp7, hash.b7);
            }
        }

        [Fact]
        public void Constructor_FromByteArrayTest()
        {
            byte[] array = new byte[32]
            {
                0xa6, 0xf4, 0x56, 0x29,
                0xc1, 0xa5, 0xc6, 0x49,
                0x0b, 0x39, 0x88, 0xe1,
                0xe4, 0x18, 0x32, 0xc5,
                0xff, 0x29, 0x72, 0x98,
                0x9e, 0x5b, 0x96, 0xf0,
                0xfa, 0x9b, 0xe2, 0x84,
                0x5a, 0x53, 0x64, 0x3d
            };

            Digest256 hash = new(array);

            Assert.Equal(0x2956f4a6U, hash.b0);
            Assert.Equal(0x49c6a5c1U, hash.b1);
            Assert.Equal(0xe188390bU, hash.b2);
            Assert.Equal(0xc53218e4U, hash.b3);
            Assert.Equal(0x987229ffU, hash.b4);
            Assert.Equal(0xf0965b9eU, hash.b5);
            Assert.Equal(0x84e29bfaU, hash.b6);
            Assert.Equal(0x3d64535aU, hash.b7);
        }

        [Fact]
        public void ConstructorExceptionTest()
        {
            byte[] nba = null;
            uint[] nua = null;

            Assert.Throws<ArgumentNullException>(() => new Digest256(nba));
            Assert.Throws<ArgumentNullException>(() => new Digest256(nua));

            Assert.Throws<ArgumentOutOfRangeException>(() => new Digest256(new byte[31]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Digest256(new byte[33]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Digest256(new uint[7]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Digest256(new uint[9]));
        }

        [Fact]
        public void StaticMemberTest()
        {
            Digest256 zero = Digest256.Zero;
            Assert.Equal(0U, zero.b0);
            Assert.Equal(0U, zero.b1);
            Assert.Equal(0U, zero.b2);
            Assert.Equal(0U, zero.b3);
            Assert.Equal(0U, zero.b4);
            Assert.Equal(0U, zero.b5);
            Assert.Equal(0U, zero.b6);
            Assert.Equal(0U, zero.b7);

            Digest256 one = Digest256.One;
            Assert.Equal(1U, one.b0);
            Assert.Equal(0U, one.b1);
            Assert.Equal(0U, one.b2);
            Assert.Equal(0U, one.b3);
            Assert.Equal(0U, one.b4);
            Assert.Equal(0U, one.b5);
            Assert.Equal(0U, one.b6);
            Assert.Equal(0U, one.b7);
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, true)]
        [InlineData(1, 0, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 1, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 1, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 1, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 1, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 1, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 1, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 1, false)]
        public void IsZeroTest(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7, bool expected)
        {
            Digest256 hash = new(u0, u1, u2, u3, u4, u5, u6, u7);
            Assert.Equal(expected, hash.IsZero);
        }

        [Fact]
        public void ToByteArrayTest()
        {
            byte[] array = new byte[32]
            {
                0xa6, 0xf4, 0x56, 0x29,
                0xc1, 0xa5, 0xc6, 0x49,
                0x0b, 0x39, 0x88, 0xe1,
                0xe4, 0x18, 0x32, 0xc5,
                0xff, 0x29, 0x72, 0x98,
                0x9e, 0x5b, 0x96, 0xf0,
                0xfa, 0x9b, 0xe2, 0x84,
                0x5a, 0x53, 0x64, 0x3d
            };

            Digest256 hash = new(array);

            Assert.Equal(array, hash.ToByteArray());
        }


    }
}
