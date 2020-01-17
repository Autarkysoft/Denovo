// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Xunit;

namespace Tests.Bitcoin.ExtentionsAndHelpersTests
{
    public class BigIntegerExtensionTests
    {
        public static IEnumerable<object[]> GetModCases()
        {
            yield return new object[] { BigInteger.Zero, new BigInteger(7), 0 };
            yield return new object[] { BigInteger.One, new BigInteger(7), 1 };
            yield return new object[] { BigInteger.MinusOne, new BigInteger(7), 6 };
            yield return new object[] { new BigInteger(12), new BigInteger(7), 5 };
            yield return new object[] { new BigInteger(-12), new BigInteger(7), 2 };
            yield return new object[] { new BigInteger(12), new BigInteger(6), 0 };
            yield return new object[] { new BigInteger(-12), new BigInteger(6), 0 };
        }
        [Theory]
        [MemberData(nameof(GetModCases))]
        public void ModTest(BigInteger big, BigInteger divisor, BigInteger result)
        {
            Assert.Equal(result, big.Mod(divisor));
        }

        [Fact]
        public void Mod_ExceptionsTest()
        {
            BigInteger val = 5;

            Assert.Throws<DivideByZeroException>(() => val.Mod(BigInteger.Zero));
            Assert.Throws<ArithmeticException>(() => val.Mod(new BigInteger(-5)));
        }


        public static IEnumerable<object[]> GetModInverseCases()
        {
            yield return new object[] { BigInteger.One, new BigInteger(13), 1 };
            yield return new object[] { new BigInteger(2), new BigInteger(13), 7 };
            yield return new object[] { new BigInteger(3), new BigInteger(13), 9 };
            yield return new object[] { new BigInteger(14), new BigInteger(13), 1 };
            yield return new object[] { new BigInteger(-12), new BigInteger(13), 1 };
            yield return new object[]
            {
                BigInteger.Parse("005aff62d23077418e248acf8dd2a74356f4ee458b73ef196e", NumberStyles.HexNumber),
                BigInteger.Parse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141", NumberStyles.HexNumber),
                BigInteger.Parse("00952c531caba3b5b8e7f4ac0e11a8042f230653c813b5ddf256773df55237b4df", NumberStyles.HexNumber)
            };
        }
        [Theory]
        [MemberData(nameof(GetModInverseCases))]
        public void ModInverseTest(BigInteger big, BigInteger divisor, BigInteger result)
        {
            Assert.Equal(result, big.ModInverse(divisor));
        }

        [Fact]
        public void ModInverse_ExceptionsTest()
        {
            BigInteger m = 13;

            BigInteger b1 = 0;
            BigInteger b2 = 13;
            BigInteger b3 = 26;

            Assert.Throws<DivideByZeroException>(() => b1.ModInverse(m));
            Assert.Throws<ArithmeticException>(() => b2.ModInverse(m));
            Assert.Throws<ArithmeticException>(() => b3.ModInverse(m));
        }
    }
}
