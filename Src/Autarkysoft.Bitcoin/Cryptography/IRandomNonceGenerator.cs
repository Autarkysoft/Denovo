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
        /// Returns a new 64-bit signed integer generated using a random number generator.
        /// </summary>
        /// <returns>The randomly generated 64-bit signed integer</returns>
        long NextInt64();
    }
}
