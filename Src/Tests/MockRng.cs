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

    internal class MockNonceRng : IRandomNonceGenerator
    {
        public MockNonceRng(int val) => val32 = val;
        public MockNonceRng(long val) => val64 = val;
        public MockNonceRng(int min, int max, int count, int[] result)
        {
            expMin = min;
            expMax = max;
            expCount = count;
            expArr = result;
        }

        private readonly int val32;
        public int NextInt32() => val32;

        private readonly long val64;
        public long NextInt64() => val64;

        private readonly int expMin, expMax, expCount;
        private readonly int[] expArr;
        public int[] GetDistinct(int min, int max, int count)
        {
            Assert.False(expArr == null, "Mock RNG is not instantiated corrently.");
            Assert.Equal(expMin, min);
            Assert.Equal(expMax, max);
            Assert.Equal(expCount, count);

            return expArr;
        }

        public void Dispose() { }
    }
}
