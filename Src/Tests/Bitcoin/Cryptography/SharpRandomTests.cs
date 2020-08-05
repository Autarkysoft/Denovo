// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using System;
using Xunit;

namespace Tests.Bitcoin.Cryptography
{
    public class SharpRandomTests
    {
        [Fact]
        public void GetBytesTest()
        {
            using SharpRandom rng = new SharpRandom();
            byte[] actual1 = new byte[32];
            byte[] actual2 = new byte[32];
            Assert.Equal(actual1, actual2);
            rng.GetBytes(actual1);
            rng.GetBytes(actual2);

            Assert.NotEqual(actual1, actual2);
        }

        [Fact]
        public void GetBytes_NullExceptionTest()
        {
            SharpRandom rng = new SharpRandom();

            Assert.Throws<ArgumentNullException>(() => rng.GetBytes(null));
            Assert.Throws<ArgumentNullException>(() => rng.GetBytes(new byte[0]));
        }

        [Fact]
        public void GetBytes_ObjectDisposedExceptionTest()
        {
            SharpRandom rng = new SharpRandom();
            rng.Dispose();

            Assert.Throws<ObjectDisposedException>(() => rng.GetBytes(new byte[1]));
        }
    }
}
