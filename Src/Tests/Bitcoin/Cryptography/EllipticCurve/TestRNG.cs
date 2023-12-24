// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// A non-cryptographic RNG used only for test infrastructure
    /// <para/>xoshiro PRNG https://prng.di.unimi.it/
    /// <para/>https://github.com/bitcoin-core/secp256k1/blob/77af1da9f631fa622fb5b5895fd27be431432368/src/testrand.h
    /// <para/>https://github.com/bitcoin-core/secp256k1/blob/77af1da9f631fa622fb5b5895fd27be431432368/src/testrand_impl.h
    /// </summary>
    public class TestRNG
    {
        private readonly ulong[] state = new ulong[4];

        /// <summary>
        /// Seed the pseudorandom number generator for testing
        /// </summary>
        /// <remarks>secp256k1_testrand_seed</remarks>
        private void Seed(byte[] seed16)
        {
            Assert.Equal(16, seed16.Length);
            byte[] PREFIX = Encoding.UTF8.GetBytes("secp256k1 test init");
            Assert.Equal(19, PREFIX.Length);

            // Use SHA256(PREFIX || seed16) as initial state.
            using Sha256 sha = new();
            byte[] out32 = sha.ComputeHash(PREFIX.ConcatFast(seed16));

            for (int i = 0; i < 4; i++)
            {
                ulong s = 0;
                for (int j = 0; j < 8; ++j)
                {
                    s = (s << 8) | out32[8 * i + j];
                }
                state[i] = s;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ROTL(ulong x, int k) => (x << k) | (x >> (64 - k));

        /// <summary>
        /// Generate a pseudorandom number in the range [0..2**64-1].
        /// </summary>
        /// <remarks>secp256k1_testrand64</remarks>
        public ulong Rand64()
        {
            // Test-only Xoshiro256++ RNG. See https://prng.di.unimi.it/
            ulong result = ROTL(state[0] + state[3], 23) + state[0];
            ulong t = state[1] << 17;
            state[2] ^= state[0];
            state[3] ^= state[1];
            state[1] ^= state[2];
            state[0] ^= state[3];
            state[2] ^= t;
            state[3] = ROTL(state[3], 45);
            return result;
        }

        /// <summary>
        /// Generate a pseudorandom number in the range [0..2**bits-1].
        /// </summary>
        /// <remarks>secp256k1_testrand_bits</remarks>
        /// <param name="bits">Bits must be 1 or more</param>
        public ulong RandBits(int bits)
        {
            return bits == 0 ? 0 : Rand64() >> (64 - bits);
        }

        /// <summary>
        /// Generate a pseudorandom number in the range [0..2**32-1]
        /// </summary>
        /// <remarks>secp256k1_testrand32</remarks>
        public uint Rand32()
        {
            return (uint)(Rand64() >> 32);
        }

        /// <summary>
        /// Generate a pseudorandom number in the range [0..range-1]
        /// </summary>
        /// <remarks>secp256k1_testrand_int</remarks>
        public uint RandInt(uint range)
        {
            uint mask = 0;
            uint range_copy;
            // Reduce range by 1, changing its meaning to "maximum value".
            Assert.True(range != 0);
            range -= 1;
            // Count the number of bits in range.
            range_copy = range;
            while (range_copy != 0)
            {
                mask = (mask << 1) | 1U;
                range_copy >>= 1;
            }
            // Generation loop.
            while (true)
            {
                uint val = (uint)(Rand64() & mask);
                if (val <= range)
                {
                    return val;
                }
            }
        }

        /// <summary>
        /// Generate a pseudorandom 32-byte array.
        /// </summary>
        /// <remarks>secp256k1_testrand256</remarks>
        public void Rand256(Span<byte> b32)
        {
            Assert.True(b32.Length == 32);
            for (int i = 0; i < b32.Length; i += 8)
            {
                ulong val = Rand64();
                b32[i + 0] = (byte)val;
                b32[i + 1] = (byte)(val >> 8);
                b32[i + 2] = (byte)(val >> 16);
                b32[i + 3] = (byte)(val >> 24);
                b32[i + 4] = (byte)(val >> 32);
                b32[i + 5] = (byte)(val >> 40);
                b32[i + 6] = (byte)(val >> 48);
                b32[i + 7] = (byte)(val >> 56);
            }
        }

        /// <summary>
        /// Generate pseudorandom bytes with long sequences of zero and one bits.
        /// </summary>
        /// <remarks>secp256k1_testrand_bytes_test</remarks>
        public void RandBytesTest(Span<byte> bytes, int len)
        {
            for (int i = 0; i < len; i++)
            {
                bytes[i] = 0;
            }

            int bits = 0;
            while (bits < len * 8)
            {
                int now = (int)(1 + (RandBits(6) * RandBits(5) + 16) / 31);
                uint val = (uint)RandBits(1);
                while (now > 0 && bits < len * 8)
                {
                    bytes[bits / 8] |= (byte)(val << (bits % 8));
                    now--;
                    bits++;
                }
            }
        }

        /// <summary>
        /// Generate a pseudorandom 32-byte array with long sequences of zero and one bits.
        /// </summary>
        /// <remarks>secp256k1_testrand256_test</remarks>
        public void Rand256Test(Span<byte> b32)
        {
            Assert.True(b32.Length == 32);
            RandBytesTest(b32, 32);
        }

        public void Rand256Test(Span<ushort> arr16)
        {
            Assert.True(arr16.Length == 16);
            byte[] b32 = new byte[32];
            RandBytesTest(b32, 32);
            for (int i = 0, j = 0; i < b32.Length && j < arr16.Length; i += 2, j++)
            {
                arr16[j] = (ushort)(b32[i] | b32[i + 1] << 8);
            }
        }

        /// <summary>
        /// Flip a single random bit in a byte array
        /// </summary>
        /// <remarks>secp256k1_testrand_flip</remarks>
        public void RandFlip(Span<byte> b, uint len)
        {
            b[(int)RandInt(len)] ^= (byte)(1 << (int)RandBits(3));
        }


        /// <summary>
        /// Initialize the test RNG using (hex encoded) array up to 16 bytes, or randomly if hexseed is NULL.
        /// </summary>
        /// <remarks>secp256k1_testrand_init</remarks>
        public void Init(string hexseed)
        {
            byte[] seed16 = new byte[16];
            if (!string.IsNullOrEmpty(hexseed))
            {
                Span<byte> temp = Helper.HexToBytes(hexseed);
                if (temp.Length <= seed16.Length)
                {
                    temp.CopyTo(seed16);
                }
                else
                {
                    temp.Slice(0, seed16.Length).CopyTo(seed16);
                }
            }
            else
            {
                RandomNumberGenerator.Fill(seed16);
            }

            Seed(seed16);
        }



        public void RunXoshiro256ppTests()
        {
            // Sanity check that we run before the actual seeding.
            for (int i = 0; i < state.Length; i++)
            {
                Assert.True(state[i] == 0);
            }
            byte[] buf32 = new byte[32];
            byte[] seed16 = "CHICKEN!CHICKEN!"u8.ToArray();
            byte[] buf32_expected = new byte[32]
            {
                0xAF, 0xCC, 0xA9, 0x16, 0xB5, 0x6C, 0xE3, 0xF0,
                0x44, 0x3F, 0x45, 0xE0, 0x47, 0xA5, 0x08, 0x36,
                0x4C, 0xCC, 0xC1, 0x18, 0xB2, 0xD8, 0x8F, 0xEF,
                0x43, 0x26, 0x15, 0x57, 0x37, 0x00, 0xEF, 0x30,
            };
            Seed(seed16);
            for (int i = 0; i < 17; i++)
            {
                Rand256(buf32);
            }
            Assert.Equal(buf32_expected, buf32);
        }
    }
}
