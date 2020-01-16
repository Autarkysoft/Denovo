// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography
{
    /// <summary>
    /// Defines methods that random number generator classes implement
    /// </summary>
    public interface IRandomNumberGenerator : IDisposable
    {
        /// <summary>
        /// Fills the given array of bytes with a cryptographically strong random sequence of values.
        /// </summary>
        /// <param name="data">The array to fill with cryptographically strong random bytes.</param>
        void GetBytes(byte[] data);
    }
}
