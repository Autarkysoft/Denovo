// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using System;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.Cryptography
{
    public class RandomNonceGeneratorTests
    {
        [Fact]
        public void NextInt32Test()
        {
            using var rng = new RandomNonceGenerator();
            int actual1 = rng.NextInt32();
            int actual2 = rng.NextInt32();
            Assert.NotEqual(actual1, actual2);
        }

        [Fact]
        public void NextInt64Test()
        {
            using var rng = new RandomNonceGenerator();
            long actual1 = rng.NextInt64();
            long actual2 = rng.NextInt64();
            Assert.NotEqual(actual1, actual2);
        }

        [Fact]
        public void GetDistinctTest()
        {
            using var rng = new RandomNonceGenerator();
            for (int count = 0; count < 10; count++)
            {
                int[] actual = rng.GetDistinct(0, 10, count);
                int[] expected = actual.Distinct().ToArray();

                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(-1, 10, 5, "Parameters can not be negative.")]
        [InlineData(0, -1, 5, "Parameters can not be negative.")]
        [InlineData(10, 10, 0, "Min value should be smaller than max value.")]
        [InlineData(0, 10, 11, "There aren't enough elements.")]
        public void GetDistinct_ExceptionTest(int min, int max, int count, string err)
        {
            using var rng = new RandomNonceGenerator();

            Exception ex = Assert.Throws<ArgumentOutOfRangeException>(() => rng.GetDistinct(min, max, count));
            Assert.Contains(err, ex.Message);
        }

        [Fact]
        public void DisposedExceptionTest()
        {
            var rng = new RandomNonceGenerator();
            rng.Dispose();

            Assert.Throws<ObjectDisposedException>(() => rng.NextInt32());
            Assert.Throws<ObjectDisposedException>(() => rng.NextInt64());
            Assert.Throws<ObjectDisposedException>(() => rng.GetDistinct(0, 10, 2));
        }
    }
}
