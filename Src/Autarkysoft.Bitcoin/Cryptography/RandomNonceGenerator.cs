// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Autarkysoft.Bitcoin.Cryptography
{
    /// <summary>
    /// Implementation of a weak random number generator using <see cref="Random"/> class useful for generating nonces in
    /// P2P messages and anywhere that doesn't require a cryptographically secure RNG.
    /// Implements <see cref="IRandomNonceGenerator"/>.
    /// </summary>
    public class RandomNonceGenerator : IRandomNonceGenerator
    {
        /// <summary>
        /// Initializes a new instance of <see cref="RandomNonceGenerator"/>.
        /// </summary>
        public RandomNonceGenerator()
        {
            rng = new Random();
        }

        private Random rng;

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException"/>
        public int NextInt32()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RandomNonceGenerator));

            return rng.Next();
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException"/>
        public int NextInt32(int min, int max)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RandomNonceGenerator));

            return rng.Next(min, max);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException"/>
        public long NextInt64()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RandomNonceGenerator));

            byte[] temp = new byte[8];
            rng.NextBytes(temp);
            return BitConverter.ToInt64(temp);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        public int[] GetDistinct(int min, int max, int count)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(RandomNonceGenerator));
            if (min < 0 || max < 0 || count < 0)
                throw new ArgumentOutOfRangeException("min-max-count", "Parameters can not be negative.");
            if (min >= max)
                throw new ArgumentOutOfRangeException(nameof(min), "Min value should be smaller than max value.");
            if (count > max - min)
                throw new ArgumentOutOfRangeException(nameof(count), "There aren't enough elements.");

            HashSet<int> hs = new HashSet<int>(count);
            while (hs.Count < count)
            {
                hs.Add(rng.Next(min, max));
            }
            return hs.ToArray();
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
