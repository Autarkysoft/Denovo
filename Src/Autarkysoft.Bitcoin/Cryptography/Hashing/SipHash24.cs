// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// Implementation of non-cryptographic hash function called SipHash with c=2 and d=4
    /// <para/>https://131002.net/siphash/siphash.pdf
    /// </summary>
    public class SipHash24
    {
        /// <summary>
        /// Computes 64-bit hash of the given data with the given 128-bit key.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="key">Key to use (it must be 128-bits or 16-bytes)</param>
        /// <param name="data">Data to hash</param>
        /// <returns>The 64-bit hash</returns>
        public unsafe ulong ComputeHash(byte[] key, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key can not be null.");
            if (key.Length != 16)
                throw new ArgumentOutOfRangeException(nameof(key), "Key length must be 16.");

            fixed (byte* kPt = &key[0], dPt = data) // &data[0] will throw if len==0
            {
                /**** 1) Initialization ****/
                ulong k0 = kPt[0] |
                           (ulong)kPt[1] << 8 |
                           (ulong)kPt[2] << 16 |
                           (ulong)kPt[3] << 24 |
                           (ulong)kPt[4] << 32 |
                           (ulong)kPt[5] << 40 |
                           (ulong)kPt[6] << 48 |
                           (ulong)kPt[7] << 56;

                ulong k1 = kPt[8] |
                           (ulong)kPt[9] << 8 |
                           (ulong)kPt[10] << 16 |
                           (ulong)kPt[11] << 24 |
                           (ulong)kPt[12] << 32 |
                           (ulong)kPt[13] << 40 |
                           (ulong)kPt[14] << 48 |
                           (ulong)kPt[15] << 56;

                ulong v0 = 0x736f6d6570736575UL ^ k0;
                ulong v1 = 0x646f72616e646f6dUL ^ k1;
                ulong v2 = 0x6c7967656e657261UL ^ k0;
                ulong v3 = 0x7465646279746573UL ^ k1;

                /**** 2) Compression ****/
                int rem = data.Length % 8;
                int dIndex = 0;
                for (; dIndex < data.Length - rem; dIndex += 8)
                {
                    ulong m = dPt[dIndex] |
                              (ulong)dPt[dIndex + 1] << 8 |
                              (ulong)dPt[dIndex + 2] << 16 |
                              (ulong)dPt[dIndex + 3] << 24 |
                              (ulong)dPt[dIndex + 4] << 32 |
                              (ulong)dPt[dIndex + 5] << 40 |
                              (ulong)dPt[dIndex + 6] << 48 |
                              (ulong)dPt[dIndex + 7] << 56;
                    v3 ^= m;

                    // c iterations of SipRound (c=2 in this class)
                    // Round 1
                    v0 += v1;
                    v1 = (v1 << 13) | (v1 >> 51);
                    v1 ^= v0;
                    v0 = (v0 << 32) | (v0 >> 32);
                    v2 += v3;
                    v3 = (v3 << 16) | (v3 >> 48);
                    v3 ^= v2;
                    v0 += v3;
                    v3 = (v3 << 21) | (v3 >> 43);
                    v3 ^= v0;
                    v2 += v1;
                    v1 = (v1 << 17) | (v1 >> 47);
                    v1 ^= v2;
                    v2 = (v2 << 32) | (v2 >> 32);
                    // Round 2
                    v0 += v1;
                    v1 = (v1 << 13) | (v1 >> 51);
                    v1 ^= v0;
                    v0 = (v0 << 32) | (v0 >> 32);
                    v2 += v3;
                    v3 = (v3 << 16) | (v3 >> 48);
                    v3 ^= v2;
                    v0 += v3;
                    v3 = (v3 << 21) | (v3 >> 43);
                    v3 ^= v0;
                    v2 += v1;
                    v1 = (v1 << 17) | (v1 >> 47);
                    v1 ^= v2;
                    v2 = (v2 << 32) | (v2 >> 32);

                    v0 ^= m;
                }

                // Padd with (Length % 256)
                ulong b = (ulong)data.Length << 56;
                for (int i = 0; i < rem; i++, dIndex++)
                {
                    b |= (ulong)dPt[dIndex] << (i * 8);
                }

                v3 ^= b;
                // c iterations of SipRound (c=2 in this class)
                // Round 1
                v0 += v1;
                v1 = (v1 << 13) | (v1 >> 51);
                v1 ^= v0;
                v0 = (v0 << 32) | (v0 >> 32);
                v2 += v3;
                v3 = (v3 << 16) | (v3 >> 48);
                v3 ^= v2;
                v0 += v3;
                v3 = (v3 << 21) | (v3 >> 43);
                v3 ^= v0;
                v2 += v1;
                v1 = (v1 << 17) | (v1 >> 47);
                v1 ^= v2;
                v2 = (v2 << 32) | (v2 >> 32);
                // Round 2
                v0 += v1;
                v1 = (v1 << 13) | (v1 >> 51);
                v1 ^= v0;
                v0 = (v0 << 32) | (v0 >> 32);
                v2 += v3;
                v3 = (v3 << 16) | (v3 >> 48);
                v3 ^= v2;
                v0 += v3;
                v3 = (v3 << 21) | (v3 >> 43);
                v3 ^= v0;
                v2 += v1;
                v1 = (v1 << 17) | (v1 >> 47);
                v1 ^= v2;
                v2 = (v2 << 32) | (v2 >> 32);

                v0 ^= b;

                /**** 3) Finalization ****/
                v2 ^= 0xff;
                // d iterations of SipRound (d=4 in this class)
                // Round 1
                v0 += v1;
                v1 = (v1 << 13) | (v1 >> 51);
                v1 ^= v0;
                v0 = (v0 << 32) | (v0 >> 32);
                v2 += v3;
                v3 = (v3 << 16) | (v3 >> 48);
                v3 ^= v2;
                v0 += v3;
                v3 = (v3 << 21) | (v3 >> 43);
                v3 ^= v0;
                v2 += v1;
                v1 = (v1 << 17) | (v1 >> 47);
                v1 ^= v2;
                v2 = (v2 << 32) | (v2 >> 32);
                // Round 2
                v0 += v1;
                v1 = (v1 << 13) | (v1 >> 51);
                v1 ^= v0;
                v0 = (v0 << 32) | (v0 >> 32);
                v2 += v3;
                v3 = (v3 << 16) | (v3 >> 48);
                v3 ^= v2;
                v0 += v3;
                v3 = (v3 << 21) | (v3 >> 43);
                v3 ^= v0;
                v2 += v1;
                v1 = (v1 << 17) | (v1 >> 47);
                v1 ^= v2;
                v2 = (v2 << 32) | (v2 >> 32);
                // Round 3
                v0 += v1;
                v1 = (v1 << 13) | (v1 >> 51);
                v1 ^= v0;
                v0 = (v0 << 32) | (v0 >> 32);
                v2 += v3;
                v3 = (v3 << 16) | (v3 >> 48);
                v3 ^= v2;
                v0 += v3;
                v3 = (v3 << 21) | (v3 >> 43);
                v3 ^= v0;
                v2 += v1;
                v1 = (v1 << 17) | (v1 >> 47);
                v1 ^= v2;
                v2 = (v2 << 32) | (v2 >> 32);
                // Round 4
                v0 += v1;
                v1 = (v1 << 13) | (v1 >> 51);
                v1 ^= v0;
                v0 = (v0 << 32) | (v0 >> 32);
                v2 += v3;
                v3 = (v3 << 16) | (v3 >> 48);
                v3 ^= v2;
                v0 += v3;
                v3 = (v3 << 21) | (v3 >> 43);
                v3 ^= v0;
                v2 += v1;
                v1 = (v1 << 17) | (v1 >> 47);
                v1 ^= v2;
                v2 = (v2 << 32) | (v2 >> 32);

                return v0 ^ v1 ^ v2 ^ v3;
            }
        }
    }
}
