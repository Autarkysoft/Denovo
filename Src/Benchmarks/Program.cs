// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using BenchmarkDotNet.Running;
using Benchmarks.Bitcoin.Cryptography.EllipticCurve;
using Benchmarks.Bitcoin.Cryptography.EllipticCurve.Primitives;
using Benchmarks.Bitcoin.Cryptography.Hashing;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<Scalar4x64_AddBench>();
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
