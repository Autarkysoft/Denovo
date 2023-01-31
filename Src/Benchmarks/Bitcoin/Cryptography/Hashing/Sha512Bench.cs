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
    public class Sha512Bench
    {
        [GlobalSetup]
        public void Setup()
        {
            data = new byte[Length];
            new Random(23).NextBytes(data);
        }

        [Params(33, 65, 250)]
        public int Length;

        private byte[] data;
        private readonly SHA512 sysSha = SHA512.Create();

        [Benchmark(Baseline = true)]
        public byte[] Library_Sha512() => Sha512.ComputeHash(data);

        [Benchmark]
        public byte[] System_Sha512() => sysSha.ComputeHash(data);
        [Benchmark]

        public byte[] InstanceSha512()
        {
            using Sha512Instance sha = new();
            return sha.ComputeHash(data);
        }
    }
}
