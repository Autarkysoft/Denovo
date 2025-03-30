// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;

namespace Benchmarks.Bitcoin.Cryptography.Hashing
{
    [RankColumn]
    [MemoryDiagnoser]
    [CategoriesColumn]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class Digest256Bench
    {
        public Digest256Bench()
        {
            Span<byte> ba1 = new byte[32];
            Span<byte> ba2 = new byte[32];

            Random rng = new(7);
            rng.NextBytes(ba1);
            rng.NextBytes(ba2);

            digest1 = new(ba1);
            digest2 = new(ba2);
            digest1_copy = new(ba1);
            digestZero = new(new byte[32]);

            if (digest1 == digest2 || digest1 != digest1_copy || !digestZero.IsZero)
            {
                throw new ArgumentException("Values are not set correctly!");
            }
        }


        private readonly Digest256 digest1;
        private readonly Digest256 digest2;
        private readonly Digest256 digest1_copy;
        private readonly Digest256 digestZero;


        [Benchmark, BenchmarkCategory("Equality")]
        public bool EqualsMethod_NonEqual() => digest1.Equals(digest2);

        [Benchmark, BenchmarkCategory("Equality")]
        public bool EqualsOperator_NonEqual() => digest1 == digest2;

        [Benchmark, BenchmarkCategory("Equality")]
        public bool EqualsMethod_Equal() => digest1.Equals(digest1_copy);

        [Benchmark, BenchmarkCategory("Equality")]
        public bool EqualsOperator_Equal() => digest1 == digest1_copy;


        [Benchmark, BenchmarkCategory("IsZero")]
        public bool EqualsZero_NonZero() => digest1 == Digest256.Zero;

        [Benchmark, BenchmarkCategory("IsZero")]
        public bool IsZero_NonZero() => digest1.IsZero;

        [Benchmark, BenchmarkCategory("IsZero")]
        public bool EqualsZero_Zero() => digestZero == Digest256.Zero;

        [Benchmark, BenchmarkCategory("IsZero")]
        public bool IsZero_Zero() => digestZero.IsZero;
    }
}
