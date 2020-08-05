// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using System;
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
        public void DisposedExceptionTest()
        {
            var rng = new RandomNonceGenerator();
            rng.Dispose();

            Assert.Throws<ObjectDisposedException>(() => rng.NextInt32());
            Assert.Throws<ObjectDisposedException>(() => rng.NextInt64());
        }
    }
}
