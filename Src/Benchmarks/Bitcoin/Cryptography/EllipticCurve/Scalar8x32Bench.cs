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
    public class Scalar8x32Bench
    {
        [GlobalSetup]
        public void Setup()
        {
            byte[] ba1 = new byte[32];
            byte[] ba2 = new byte[32];
            Random rng = new(17);
            rng.NextBytes(ba1);
            rng.NextBytes(ba2);

            a = new(ba1, out bool of1);
            b = new(ba1, out bool of2);
            if (of1 || of2)
            {
                throw new Exception("Unexpected overflow.");
            }
            aa = new(ba1);
            bb = new(ba1);

            var res1 = a.Multiply(b);
            var res2 = aa.Multiply(bb);

            if (!res2.Equals(res1))
            {
                throw new Exception("Not equal.");
            }
        }

        Scalar8x32 a, b;
        Scalar8x32Alt aa, bb;


        [Benchmark(Baseline = true)]
        public Scalar8x32 Optimized() => a.Multiply(b);

        [Benchmark]
        public Scalar8x32Alt NotOptimized() => aa.Multiply(bb);
    }
}
