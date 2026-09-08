// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using BenchmarkDotNet.Attributes;
using System;

namespace Benchmarks.Bitcoin.Cryptography.EllipticCurve
{
#pragma warning disable CS0618 // Type or member is obsolete
    [InProcess]
    [RankColumn]
    public class Scalar8x32_MultiplyBench
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
            lib2 = new(ba2, out bool of2);
            if (of1 || of2)
            {
                throw new Exception("Unexpected overflow.");
            }
            alt1 = new(ba1);
            alt2 = new(ba2);

            Scalar8x32 res1 = lib1.Multiply(lib2);
            Scalar8x32Alt res2 = alt1.Multiply(alt2);
            Scalar8x32Alt res3 = alt1.Multiply_Inlined(alt2);

            if (!res2.Equals(res1))
            {
                throw new Exception("Not equal.");
            }
            if (!res3.Equals(res1))
            {
                throw new Exception("Not equal.");
            }
        }

        Scalar8x32 lib1, lib2;
        Scalar8x32Alt alt1, alt2;


        [Benchmark(Baseline = true)]
        public Scalar8x32 LibMult() => lib1.Multiply(lib2);

        [Benchmark]
        public Scalar8x32Alt AltMult() => alt1.Multiply(alt2);

        [Benchmark]
        public Scalar8x32Alt AltMult_Inlined() => alt1.Multiply_Inlined(alt2);
    }
#pragma warning restore CS0618 // Type or member is obsolete
}
