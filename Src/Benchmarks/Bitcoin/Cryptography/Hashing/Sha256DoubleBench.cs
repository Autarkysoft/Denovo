﻿// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Benchmarks.Bitcoin.Cryptography.Hashing
{
    [InProcess]
    [RankColumn]
    [MemoryDiagnoser]
    public class Sha256DoubleBench
    {
        private readonly SHA256 sysSha = SHA256.Create();
        private readonly Sha256 libSha = new Sha256();

        public static IEnumerable<object[]> GetData()
        {
            byte[] small = new byte[33];
            byte[] big = new byte[250];
            Random rnd = new Random(42);
            rnd.NextBytes(small);
            rnd.NextBytes(big);

            yield return new object[] { small };
            yield return new object[] { big };
        }

        [Benchmark]
        [ArgumentsSource(nameof(GetData))]
        public byte[] System_Sha256d(byte[] data) => sysSha.ComputeHash(sysSha.ComputeHash(data));

        [Benchmark(Baseline = true)]
        [ArgumentsSource(nameof(GetData))]
        public byte[] Library_Sha256d(byte[] data) => libSha.ComputeHashTwice(data);
    }
}
