// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using System;
using Xunit;

namespace Tests
{
    internal class MockRng : IRandomNumberGenerator
    {
        public MockRng(byte[] baToReturn) => data = baToReturn;
        public MockRng(string hex) => data = Helper.HexToBytes(hex);

        private readonly byte[] data;

        public void GetBytes(byte[] toFill)
        {
            Assert.True(data != null && data.Length == toFill.Length,
                            "Mock RNG was initialized with a different length pre-defined data.");
            Buffer.BlockCopy(data, 0, toFill, 0, data.Length);
        }

        public void Dispose() { }
    }
}
