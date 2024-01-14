// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.Numerics;

namespace Benchmarks.Bitcoin.Cryptography
{
    [InProcess]
    [RankColumn]
    [MemoryDiagnoser]
    [CategoriesColumn]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class Rfc6979Bench
    {
        [GlobalSetup]
        public void Setup()
        {
            Random rng = new(23);
            rng.NextBytes(data);
            rng.NextBytes(key);
        }


        private readonly Rfc6979 rfc = new();
        private readonly byte[] data = new byte[32];
        private readonly byte[] key = new byte[32];


        [Benchmark(Baseline = true), BenchmarkCategory("Single")]
        public BigInteger SingleTry_Old()
        {
            uint count = 1;
            byte[] extraEntropy = new byte[32];
            extraEntropy[0] = (byte)count;
            extraEntropy[1] = (byte)(count >> 8);
            extraEntropy[2] = (byte)(count >> 16);
            extraEntropy[3] = (byte)(count >> 24);
            count++;

            return rfc.GetK(data, key, extraEntropy);
        }

        [Benchmark, BenchmarkCategory("Single")]
        public byte[] SingleTry_New()
        {
            rfc.Init(data, key);
            return rfc.Generate();
        }


        [Benchmark(Baseline = true), BenchmarkCategory("Double")]
        public BigInteger TwoTry_Old()
        {
            uint count = 1;
            byte[] extraEntropy = new byte[32];
            BigInteger result;
            do
            {
                extraEntropy[0] = (byte)count;
                extraEntropy[1] = (byte)(count >> 8);
                extraEntropy[2] = (byte)(count >> 16);
                extraEntropy[3] = (byte)(count >> 24);
                count++;

                result = rfc.GetK(data, key, extraEntropy);

            } while (count < 3);

            return result;
        }

        [Benchmark, BenchmarkCategory("Double")]
        public byte[] TwoTry_New()
        {
            rfc.Init(data, key);
            rfc.Generate();
            return rfc.Generate();
        }
    }
}
