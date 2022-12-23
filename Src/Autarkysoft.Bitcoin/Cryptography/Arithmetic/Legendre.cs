// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Cryptography.Arithmetic
{
    /// <summary>
    /// Implementation of Legendre symbol
    /// <para/> https://en.wikipedia.org/wiki/Legendre_symbol
    /// </summary>
    [Obsolete]
    public static class Legendre
    {
        // TODO: benchmark which one is faster, this function or using the following
        // BigInteger ls = BigInteger.ModPow(n, (prime - 1) / 2, prime);
        // return (ls == prime - 1) ? -1 : (int)ls;

        /// <summary>
        /// Finds Legendre symbol (1, −1, 0) for an integer 'a' and an odd prime 'p'.
        /// </summary>
        /// <exception cref="ArithmeticException"/>
        /// <param name="a">The integer</param>
        /// <param name="p">The odd prime (must be bigger than 2)</param>
        /// <returns>Legendre symbol with values 1, −1, 0</returns>
        public static int Symbol(BigInteger a, BigInteger p)
        {
            if (p < 2)
                throw new ArithmeticException($"{nameof(p)} must be >= 2");

            if (a == 0 || a == 1)
            {
                return (int)a;
            }

            int result;
            if (a.IsEven)
            {
                result = Symbol(a / 2, p);
                if (((p * p - 1) & 8) != 0)
                {
                    result = -result;
                }
            }
            else
            {
                result = Symbol(p % a, a);
                if (((a - 1) * (p - 1) & 4) != 0)
                {
                    result = -result;
                }
            }
            return result;
        }

    }
}
