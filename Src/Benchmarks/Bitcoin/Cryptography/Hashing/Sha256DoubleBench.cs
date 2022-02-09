// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using BenchmarkDotNet.Attributes;
using System;
using System.Security.Cryptography;

namespace Benchmarks.Bitcoin.Cryptography.Hashing
{
    [InProcess]
    [RankColumn]
    [MemoryDiagnoser]
    [Config(typeof(RatioConfig))]
    public class Sha256DoubleBench
    {
        [GlobalSetup]
        public void Setup()
        {
            data = new byte[Length];
            new Random(23).NextBytes(data);
        }

        [Params(33, 65, 250)]
        public int Length;

        private readonly SHA256 sysSha = SHA256.Create();
        private readonly Sha256 libSha = new();
        private byte[] data;


        [Benchmark(Baseline = true)]
        public byte[] Library_Sha256d() => libSha.ComputeHashTwice(data);

        [Benchmark]
        public byte[] System_Sha256d() => sysSha.ComputeHash(sysSha.ComputeHash(data));

        [Benchmark]
        public byte[] Static_Sha256() => StaticSha256.ComputeHashTwice(data);
    }
}
