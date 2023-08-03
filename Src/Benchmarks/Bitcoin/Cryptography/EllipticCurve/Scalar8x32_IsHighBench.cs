// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using BenchmarkDotNet.Attributes;
using System;

namespace Benchmarks.Bitcoin.Cryptography.EllipticCurve
{
    [InProcess]
    [RankColumn]
    public class Scalar8x32_IsHighBench
    {
        [GlobalSetup]
        public void Setup()
        {
            byte[] ba = new byte[32];
            Random rng = new();
            rng.NextBytes(ba);

            a = new(ba, out bool of1);
            b = new(ba);

            if (a.IsHigh != b.IsHigh)
            {
                throw new Exception("IsHigh results are not equal.");
            }
        }

        Scalar8x32 a;
        Scalar8x32Alt b;


        [Benchmark(Baseline = true)]
        public bool Lib() => a.IsHigh;

        [Benchmark]
        public bool Alt() => b.IsHigh;
    }
}
