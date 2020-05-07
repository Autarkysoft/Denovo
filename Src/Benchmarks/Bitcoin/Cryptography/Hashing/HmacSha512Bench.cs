// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Benchmarks.Bitcoin.Cryptography.Hashing
{
    [InProcess]
    [RankColumn]
    [MemoryDiagnoser]
    public class HmacSha512Bench
    {
        private static readonly byte[] key = { 1, 2, 3 };
        private readonly HMACSHA512 sysHmac = new HMACSHA512(key);
        private readonly HmacSha512 libHmac = new HmacSha512(key);

        public IEnumerable<object[]> GetData()
        {
            byte[] small = new byte[33];
            byte[] big = new byte[250];
            Random rnd = new Random(42);
            rnd.NextBytes(small);
            rnd.NextBytes(big);

            yield return new object[] { small };
            yield return new object[] { big };
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetData))]
        public byte[] System_HmacSha512(byte[] data) => sysHmac.ComputeHash(data);

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] Library_HmacSha512(byte[] data) => libHmac.ComputeHash(data);
    }
}
