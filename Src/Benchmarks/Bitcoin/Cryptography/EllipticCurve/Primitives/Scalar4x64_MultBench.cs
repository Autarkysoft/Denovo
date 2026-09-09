// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using BenchmarkDotNet.Attributes;

namespace Benchmarks.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    [InProcess]
    [RankColumn]
    public class Scalar4x64_MultBench
    {
        [GlobalSetup]
        public void Setup()
        {
            (libSc1, libSc2) = Helper.BuildScalars();
            (altSc1, altSc2) = Helper.BuildScalarAltss();

            Helper.AssertEqual(altSc1, libSc1, true);
            Helper.AssertEqual(altSc2, libSc2, true);

            Scalar4x64 mult1 = libSc1.Multiply(libSc2);
            Scalar4x64Alt mult2 = altSc1.Multiply(altSc2);

            Helper.AssertEqual(mult2, mult1, true);

            (sc32_1, sc32_2) = Helper.BuildScalars32();
        }


        Scalar4x64 libSc1, libSc2;
        Scalar4x64Alt altSc1, altSc2;


        [Benchmark(Baseline = true)]
        public Scalar4x64 LibMult() => libSc1.Multiply(libSc2);
        [Benchmark]
        public Scalar4x64Alt AltMult() => altSc1.Multiply(altSc2);

#pragma warning disable CS0618 // Type or member is obsolete
        Scalar8x32 sc32_1, sc32_2;
        [Benchmark]
        public Scalar8x32 LibMult32() => sc32_1.Multiply(sc32_2);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
