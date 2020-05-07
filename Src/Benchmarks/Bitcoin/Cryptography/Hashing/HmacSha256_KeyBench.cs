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
    public class HmacSha256_KeyBench
    {
        private readonly HMACSHA256 sysHmac = new HMACSHA256();
        private readonly HmacSha256 libHmac = new HmacSha256();

        public IEnumerable<object[]> GetData()
        {
            byte[] sData = new byte[32];
            byte[] bData = new byte[250];
            byte[] sKey = new byte[32];
            byte[] bKey = new byte[250];

            Random rnd = new Random(42);
            rnd.NextBytes(sData);
            rnd.NextBytes(bData);

            yield return new object[] { sData, sKey };
            yield return new object[] { sData, bKey };
            yield return new object[] { bData, sKey };
            yield return new object[] { bData, bKey };
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetData))]
        public byte[] System_HmacSha256(byte[] data, byte[] key)
        {
            sysHmac.Key = key;
            return sysHmac.ComputeHash(data);
        }

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] Library_HmacSha256(byte[] data, byte[] key) => libHmac.ComputeHash(data, key);
    }
}
