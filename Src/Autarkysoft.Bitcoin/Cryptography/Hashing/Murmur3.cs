// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// Implementation of version 3 of the non-cryptographic hash function called MurmurHash
    /// </summary>
    public sealed class Murmur3
    {
        /// <summary>
        /// Computes 32-bit hash of the given data.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="data">Data to hash</param>
        /// <param name="seed">Seed to use</param>
        /// <returns>32-bit unsigned integer (hash result)</returns>
        public unsafe uint ComputeHash32(byte[] data, uint seed)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");


            if (data.Length == 0)
            {
                uint res = seed ^ (uint)data.Length;
                res ^= res >> 16;
                res *= 0x85ebca6b;
                res ^= res >> 13;
                res *= 0xc2b2ae35;
                res ^= res >> 16;

                return res;
            }

            uint hash = seed;
            fixed (byte* dPt = &data[0])
            {
                int rem = data.Length & 3; /*=(data.Length % 4)*/
                for (int i = 0; i < data.Length - rem; i += 4)
                {
                    uint k1 = (uint)(dPt[i] | (dPt[i + 1] << 8) | (dPt[i + 2] << 16) | (dPt[i + 3] << 24));

                    k1 *= 0xcc9e2d51;
                    k1 = ((k1 << 15) | (k1 >> 17));
                    k1 *= 0x1b873593;

                    hash ^= k1;
                    hash = (hash << 13) | (hash >> 19);
                    hash = (hash * 5) + 0xe6546b64;
                }

                if (rem == 0)
                {
                    hash ^= (uint)data.Length;
                    hash ^= hash >> 16;
                    hash *= 0x85ebca6b;
                    hash ^= hash >> 13;
                    hash *= 0xc2b2ae35;
                    hash ^= hash >> 16;

                    return hash;
                }
                else
                {
                    uint k2 = 0;
                    switch (rem)
                    {
                        case 3:
                            k2 = (uint)(dPt[data.Length - 3] |
                                       (dPt[data.Length - 2] << 8) |
                                       (dPt[data.Length - 1] << 16));
                            break;
                        case 2:
                            k2 = (uint)(dPt[data.Length - 2] |
                                       (dPt[data.Length - 1] << 8));
                            break;
                        case 1:
                            k2 = dPt[data.Length - 1];
                            break;
                    }

                    k2 *= 0xcc9e2d51;
                    k2 = (k2 << 15) | (k2 >> 17);
                    k2 *= 0x1b873593;

                    hash ^= k2;


                    hash ^= (uint)data.Length;
                    hash ^= hash >> 16;
                    hash *= 0x85ebca6b;
                    hash ^= hash >> 13;
                    hash *= 0xc2b2ae35;
                    hash ^= hash >> 16;

                    return hash;
                }
            }
        }
    }
}
