// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Cryptography.Arithmetic
{
    /// <summary>
    /// Implementation of square root in modular arithmetic.
    /// </summary>
    [Obsolete]
    public static class SquareRoot
    {
        /// <summary>
        /// Finds modular square root r such that r^2≡a (mod p) for a &#60; p and p is the secp256k1 prime (p%4 = 3).
        /// Return value indicates success.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="p">Secp256k1 prime</param>
        /// <param name="result">Result if square root exists</param>
        /// <returns>True if the square root exists; false if otherwise.</returns>
        public static bool TryFind(BigInteger a, BigInteger p, out BigInteger result)
        {
            if (Legendre.Symbol(a, p) != 1)
            {
                return false;
            }

            result = BigInteger.ModPow(a, (p + 1) / 4, p);
            return true;
        }


        /// <summary>
        /// Finds modular square root r such that r^2≡a (mod p) where p is any prime.
        /// </summary>
        /// <exception cref="ArithmeticException"/>
        /// <param name="a">a &#60; p</param>
        /// <param name="p">Any prime</param>
        /// <returns>Square root</returns>
        public static BigInteger FindSquareRoot(BigInteger a, BigInteger p)
        {
            return TonelliShanks(a, p);
        }


        private static BigInteger TonelliShanks(BigInteger a, BigInteger p)
        {
            if (a >= p)
            {
                throw new ArithmeticException("The residue, 'a' cannot be greater than the modulus 'p'!");
            }
            if (Legendre.Symbol(a, p) != 1) // a^(p-1 / 2) % p == p-1
            {
                throw new ArithmeticException($"Parameter 'a' is not a quadratic residue, mod 'p'");
            }
            // This will be true for secp256k1 curve prime
            if (p % 4 == 3)
            {
                return BigInteger.ModPow(a, (p + 1) / 4, p);
            }

            //Initialize 
            BigInteger s = p - 1;
            BigInteger e = 0;
            while (s % 2 == 0)
            {
                s /= 2;
                e += 1;
            }

            BigInteger n = FindGenerator(p);

            BigInteger x = BigInteger.ModPow(a, (s + 1) / 2, p);
            BigInteger b = BigInteger.ModPow(a, s, p);
            BigInteger g = BigInteger.ModPow(n, s, p);
            BigInteger r = e;
            BigInteger m = Order(b, p);
            if (m == 0)
            {
                return x;
            }

            while (m > 0)
            {
                x = (x * BigInteger.ModPow(g, TwoExp(r - m - 1), p)) % p;
                b = (b * BigInteger.ModPow(g, TwoExp(r - m), p)) % p;
                g = BigInteger.ModPow(g, TwoExp(r - m), p);
                r = m;
                m = Order(b, p);
            }

            return x;
        }

        private static BigInteger FindGenerator(BigInteger p)
        {
            BigInteger n = 2;
            while (BigInteger.ModPow(n, (p - 1) / 2, p) == 1)
            {
                n++;
            }

            return n;
        }


        private static BigInteger Order(BigInteger b, BigInteger p)
        {
            BigInteger m = 1;
            BigInteger e = 0;

            while (BigInteger.ModPow(b, m, p) != 1)
            {
                m *= 2;
                e++;
            }

            return e;
        }

        private static BigInteger TwoExp(BigInteger exp)
        {
            return BigInteger.Pow(2, (int)exp);
        }

    }
}
