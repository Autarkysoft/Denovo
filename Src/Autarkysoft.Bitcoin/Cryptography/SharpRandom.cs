// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Security.Cryptography;

namespace Autarkysoft.Bitcoin.Cryptography
{
    /// <summary>
    /// Implementation of a cryptographic Random Number Generator (RNG) using the <see cref="RNGCryptoServiceProvider"/> class.
    /// Implements <see cref="IRandomNumberGenerator"/>.
    /// </summary>
    public class SharpRandom : IRandomNumberGenerator
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SharpRandom"/>.
        /// </summary>
        public SharpRandom()
        {
            rng = new RNGCryptoServiceProvider();
        }
        // TODO: add extra entropy to be added correctly to the result. using AES would be a good place to start.


        private RNGCryptoServiceProvider rng;



        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"/>
        public void GetBytes(byte[] data)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(SharpRandom));
            if (data is null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "Data to fill can not be null or empty.");

            rng.GetBytes(data);
        }



        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by the <see cref="SharpRandom"/> class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (!(rng is null))
                        rng.Dispose();
                    rng = null;
                }

                isDisposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
