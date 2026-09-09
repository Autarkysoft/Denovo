// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using BenchmarkDotNet.Attributes;
using System;

namespace Benchmarks.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    [InProcess]
    [RankColumn]
    public class Scalar4x64_AddBench
    {
        [GlobalSetup]
        public void Setup()
        {
            (libSc1, libSc2) = Helper.BuildScalars();
            (altSc1, altSc2) = Helper.BuildScalarAltss();

            Helper.AssertEqual(altSc1, libSc1, true);
            Helper.AssertEqual(altSc2, libSc2, true);

            Scalar4x64 add1 = libSc1.Add(libSc2, out bool addOf1);
            Scalar4x64Alt add2 = altSc1.Add(altSc2, out bool addOf2);

            Helper.AssertEqual(add2, add1, true);
            if (addOf1 != addOf2)
            {
                throw new Exception("Unexpected overflow values when adding.");
            }

            (sc32_1, sc32_2) = Helper.BuildScalars32();
        }


        Scalar4x64 libSc1, libSc2;
        Scalar4x64Alt altSc1, altSc2;


        [Benchmark(Baseline = true)]
        public Scalar4x64 LibAdd() => libSc1.Add(libSc2, out _);
        [Benchmark]
        public Scalar4x64Alt AltAdd() => altSc1.Add(altSc2, out _);

#pragma warning disable CS0618 // Type or member is obsolete
        Scalar8x32 sc32_1, sc32_2;
        [Benchmark]
        public Scalar8x32 LibAdd32() => sc32_1.Add(sc32_2, out _);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
