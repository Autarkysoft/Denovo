// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography
{
    /// <summary>
    /// Defines methods that a random nonce generator implements that doesn't have to be cryptographically secure.
    /// Inherits from <see cref="IDisposable"/>.
    /// </summary>
    public interface IRandomNonceGenerator : IDisposable
    {
        /// <summary>
        /// Returns a new 32-bit signed integer generated using a random number generator.
        /// </summary>
        /// <returns>The randomly generated 32-bit signed integer</returns>
        int NextInt32();
        /// <summary>
        /// Returns a new 32-bit signed integer between <paramref name="min"/> and <paramref name="max"/> values
        /// [<paramref name="min"/>, <paramref name="max"/>) generated using a random number generator.
        /// </summary>
        /// <param name="min">Minimum value (inclusive)</param>
        /// <param name="max">Maximum value (exclusive)</param>
        /// <returns></returns>
        int NextInt32(int min, int max);
        /// <summary>
        /// Returns a new 64-bit signed integer generated using a random number generator.
        /// </summary>
        /// <returns>The randomly generated 64-bit signed integer</returns>
        long NextInt64();
        /// <summary>
        /// Creates an array with requested number of elements and populates it with random but distinct items
        /// that are between <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        /// <param name="min">Array elements will be bigger than or equal to this</param>
        /// <param name="max">Array elements will be smaller than this</param>
        /// <param name="count">Number of elements in the returned array</param>
        /// <returns>An array of distinct and randomly generated 32-bit signed integers</returns>
        int[] GetDistinct(int min, int max, int count);
    }
}
