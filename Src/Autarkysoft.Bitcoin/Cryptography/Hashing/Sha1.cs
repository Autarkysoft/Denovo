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
    public class Sha1 : IHashFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sha1"/>.
        /// </summary>
        /// <param name="isDouble">Determines whether the hash should be performed twice.</param>
        public Sha1(bool isDouble = false)
        {
            IsDouble = isDouble;
        }



        /// <summary>
        /// Indicates whether the hash function should be performed twice on message.
        /// For example Double SHA256 that bitcoin uses.
        /// </summary>
        public bool IsDouble { get; set; }

        /// <summary>
        /// Size of the hash result in bytes.
        /// </summary>
        public virtual int HashByteSize => 20;

        /// <summary>
        /// Size of the blocks used in each round.
        /// </summary>
        public virtual int BlockByteSize => 64;


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

            return IsDouble ? hash.ComputeHash(hash.ComputeHash(data)) : hash.ComputeHash(data);
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
        /// Releases the resources used by the <see cref="Sha1"/> class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!(hash is null))
                        hash.Dispose();
                    hash = null;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Sha1"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
