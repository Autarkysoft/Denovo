// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Benchmarks.Bitcoin.Cryptography.Hashing
{
    [InProcess]
    [MemoryDiagnoser]
    public class Ripemd160Sha256Bench
    {
        private readonly Ripemd160Sha256 libRipSha = new Ripemd160Sha256();

        public IEnumerable<object[]> GetData()
        {
            byte[] small = new byte[33];
            byte[] big = new byte[65];
            Random rnd = new Random(42);
            rnd.NextBytes(small);
            rnd.NextBytes(big);

            yield return new object[] { small };
            yield return new object[] { big };
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetData))]
        public byte[] RipSha(byte[] data) => libRipSha.ComputeHash(data);
    }
}
