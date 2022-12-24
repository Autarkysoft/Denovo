// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Security.Cryptography;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// This is a wrapper around .Net SHA1 implementation.
    /// </summary>
    public static class Sha1
    {
        /// <summary>
        /// Size of the hash result in bytes.
        /// </summary>
        public const int HashByteSize = 20;

        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">The byte array to compute hash for</param>
        /// <returns>The computed hash</returns>
        public static byte[] ComputeHash(byte[] data)
        {
            using SHA1 hash = SHA1.Create();
            return hash.ComputeHash(data);
        }

        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="buffer">The byte array to compute hash for</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash</returns>
        public static byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            using SHA1 hash = SHA1.Create();
            return hash.ComputeHash(buffer, offset, count);
        }
    }
}
