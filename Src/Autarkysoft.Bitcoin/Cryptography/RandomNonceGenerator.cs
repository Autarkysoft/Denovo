// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography
{
    /// <summary>
    /// Implementation of a weak random number generator using <see cref="Random"/> class useful for generating nonces in
    /// P2P messages and anywhere that doesn't require a cryptographically secure RNG.
    /// Implements <see cref="IRandomNonceGenerator"/>.
    /// </summary>
    public class RandomNonceGenerator : IRandomNonceGenerator
    {
        private Random rng = new Random();

        /// <inheritdoc/>
        public int NextInt32()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RandomNonceGenerator));

            return rng.Next();
        }

        /// <inheritdoc/>
        public long NextInt64()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RandomNonceGenerator));

            byte[] temp = new byte[8];
            rng.NextBytes(temp);
            return BitConverter.ToInt64(temp);
        }

        private bool isDisposed = false;
        /// <inheritdoc/>
        public void Dispose()
        {
            if (!isDisposed)
            {
                rng = null;
                isDisposed = true;
            }
        }
    }
}
