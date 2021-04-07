// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin
{
    public class BlockCopyHelpersTests
    {
        private static byte[] GetBytes(int size) => Enumerable.Range(1, size).Select(x => (byte)x).ToArray();

        [Fact]
        public void Block16_ByteTest()
        {
            byte[] source = GetBytes(16);
            byte[] destination = new byte[31];
            byte[] expected = new byte[destination.Length];
            int offset = 5;
            unsafe
            {
                fixed (byte* src = &source[0], des = &destination[offset])
                {
                    *(Block16*)des = *(Block16*)src;
                }
            }

            Buffer.BlockCopy(source, 0, expected, offset, source.Length);

            Assert.Equal(expected, destination);
        }

        [Fact]
        public void Block16_UIntTest()
        {
            uint[] source = new uint[16 / sizeof(uint)]
            {
                0x01020304,
                0x05060708,
                0x09101112,
                0x13141516
            };
            uint[] destination = new uint[31];
            uint[] expected = new uint[destination.Length];
            int offset = 3;
            unsafe
            {
                fixed (uint* src = &source[0], des = &destination[offset])
                {
                    *(Block16*)des = *(Block16*)src;
                }
            }

            Buffer.BlockCopy(source, 0, expected, offset * sizeof(uint), source.Length * sizeof(uint));

            Assert.Equal(expected, destination);
        }


        [Fact]
        public void Block32_ByteTest()
        {
            byte[] source = GetBytes(32);
            byte[] destination = new byte[37];
            byte[] expected = new byte[destination.Length];
            int offset = 5;
            unsafe
            {
                fixed (byte* src = &source[0], des = &destination[offset])
                {
                    *(Block32*)des = *(Block32*)src;
                }
            }

            Buffer.BlockCopy(source, 0, expected, offset, source.Length);

            Assert.Equal(expected, destination);
        }

        [Fact]
        public void Block32_UIntTest()
        {
            uint[] source = new uint[32 / sizeof(uint)]
            {
                0x01020304, 0x05060708, 0x09101112, 0x13141516,
                0x17181920, 0x21222324, 0x25262728, 0x29303132
            };
            uint[] destination = new uint[37];
            uint[] expected = new uint[destination.Length];
            int offset = 3;
            unsafe
            {
                fixed (uint* src = &source[0], des = &destination[offset])
                {
                    *(Block32*)des = *(Block32*)src;
                }
            }

            Buffer.BlockCopy(source, 0, expected, offset * sizeof(uint), source.Length * sizeof(uint));

            Assert.Equal(expected, destination);
        }


        [Fact]
        public void Block64_ByteTest()
        {
            byte[] source = GetBytes(64);
            byte[] destination = new byte[75];
            byte[] expected = new byte[destination.Length];
            int offset = 5;
            unsafe
            {
                fixed (byte* src = &source[0], des = &destination[offset])
                {
                    *(Block64*)des = *(Block64*)src;
                }
            }

            Buffer.BlockCopy(source, 0, expected, offset, source.Length);

            Assert.Equal(expected, destination);
        }

        [Fact]
        public void Block64_UIntTest()
        {
            uint[] source = new uint[64 / sizeof(uint)]
            {
                0x01020304, 0x05060708, 0x09101112, 0x13141516,
                0x17181920, 0x21222324, 0x25262728, 0x29303132,
                0x33343536, 0x37383940, 0x41424344, 0x45464748,
                0x49505152, 0x53545556, 0x57585960, 0x61626364

            };
            uint[] destination = new uint[75];
            uint[] expected = new uint[destination.Length];
            int offset = 3;
            unsafe
            {
                fixed (uint* src = &source[0], des = &destination[offset])
                {
                    *(Block64*)des = *(Block64*)src;
                }
            }

            Buffer.BlockCopy(source, 0, expected, offset * sizeof(uint), source.Length * sizeof(uint));

            Assert.Equal(expected, destination);
        }
    }
}
