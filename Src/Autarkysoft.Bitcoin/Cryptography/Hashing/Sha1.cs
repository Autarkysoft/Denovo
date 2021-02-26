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
    /// <para/>Implements <see cref="IDisposable"/>
    /// </summary>
    public sealed class Sha1 : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sha1"/>.
        /// </summary>
        public Sha1()
        {
        }


        /// <summary>
        /// Size of the hash result in bytes.
        /// </summary>
        public const int HashByteSize = 20;

        /// <summary>
        /// Size of the blocks used in each round.
        /// </summary>
        public const int BlockByteSize = 64;


        private SHA1 hash = SHA1.Create();



        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">The byte array to compute hash for</param>
        /// <returns>The computed hash</returns>
        public byte[] ComputeHash(byte[] data)
        {
            if (disposedValue)
                throw new ObjectDisposedException("Instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");

            return hash.ComputeHash(data);
        }


        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="buffer">The byte array to compute hash for</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash</returns>
        public byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            return ComputeHash(buffer.SubArray(offset, count));
        }



        private bool disposedValue = false;

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Sha1"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!disposedValue)
            {
                if (!(hash is null))
                    hash.Dispose();
                hash = null;

                disposedValue = true;
            }
        }
    }
}
