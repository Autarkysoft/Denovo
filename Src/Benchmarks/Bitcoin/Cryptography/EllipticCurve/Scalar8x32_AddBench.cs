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
    public class Scalar8x32_AddBench
    {
        [GlobalSetup]
        public void Setup()
        {
            byte[] ba1 = new byte[32];
            byte[] ba2 = new byte[32];
            Random rng = new(17);
            rng.NextBytes(ba1);
            rng.NextBytes(ba2);

            lib1 = new(ba1, out bool of1);
            lib2 = new(ba1, out bool of2);
            if (of1 || of2)
            {
                throw new Exception("Unexpected overflow.");
            }
            alt1 = new(ba1);
            alt2 = new(ba1);

            Scalar8x32 res1 = lib1.Add(lib2, out _);
            Scalar8x32Alt res2 = alt1.Add(alt2, out _);

            if (!res2.Equals(res1))
            {
                throw new Exception("Not equal.");
            }
        }

        Scalar8x32 lib1, lib2;
        Scalar8x32Alt alt1, alt2;


        [Benchmark(Baseline = true)]
        public Scalar8x32 LibAdd() => lib1.Add(lib2, out _);

        [Benchmark]
        public Scalar8x32Alt AltAdd() => alt1.Add(alt2, out _);
    }
}
