// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Arithmetic;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Arithmetic
{
    public class LegendreTests
    {
        // TODO: add better tests with bigger integers

        public static IEnumerable<object[]> GetSymbolCases()
        {
            yield return new object[] { new BigInteger(1), new BigInteger(3), 1 };
            yield return new object[] { new BigInteger(1), new BigInteger(5), 1 };
            yield return new object[] { new BigInteger(2), new BigInteger(3), -1 };
            yield return new object[] { new BigInteger(2), new BigInteger(5), -1 };
            yield return new object[] { new BigInteger(2), new BigInteger(7), 1 };
            yield return new object[] { new BigInteger(2), new BigInteger(11), -1 };
            yield return new object[] { new BigInteger(3), new BigInteger(3), 0 };
            yield return new object[] { new BigInteger(3), new BigInteger(5), -1 };
            yield return new object[] { new BigInteger(3), new BigInteger(7), -1 };
            yield return new object[] { new BigInteger(17), new BigInteger(127), 1 };
        }
        [Theory]
        [MemberData(nameof(GetSymbolCases))]
        public void SymbolTest(BigInteger n, BigInteger p, int expected)
        {
            int actual = Legendre.Symbol(n, p);
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void Symbol_BigValue_Test()
        {
            // secp256k1 prime
            BigInteger prime = BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007908834671663");
            Random rnd = new Random();
            byte[] ba = new byte[32];
            rnd.NextBytes(ba);
            BigInteger n = ba.ToBigInt(true, true);
            BigInteger ls = BigInteger.ModPow(n, (prime - 1) / 2, prime);

            int expected = (ls == prime - 1) ? -1 : (int)ls;
            int actual = Legendre.Symbol(n, prime);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Symbol_ExceptionTest()
        {
            Assert.Throws<ArithmeticException>(() => Legendre.Symbol(2, 1));
        }
    }
}
