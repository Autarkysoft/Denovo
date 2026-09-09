// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using Autarkysoft.Bitcoin.Encoders;
using Benchmarks.Bitcoin.Cryptography.EllipticCurve.Primitives;
using System;

namespace Benchmarks
{
    public static class Helper
    {
        // Consecutive SHA256 hash of the word "Benchmark"
        // SHA256(UTF8("Benchmark")) | SHA256(SHA256(UTF8("Benchmark"))) | SHA256(SHA256(SHA256(UTF8("Benchmark")))) ...
        private const string Hex = "1fa330e271ba9b77e05c07ed9b7aacfceed8593aa76b21d9bc4640cf3c9ac483" +
                                   "2e2987ac392e129e5e899e398a96857dcd01c914690a29520c002dd6ef41d56b" +
                                   "b4fc192a393623234868b2bc30140e3ad015963adcefe102b6111fcb9420483a" +
                                   "7db873234074c2782f98a8e9e54155b32336810c8ebe9a021078dfdc78e5d57e" +
                                   "26fc044a776586c4bf3ef5abc27e34a1b24b865d11f005bbb401a66aef0f662a" +
                                   "7d7248c8420465457d01ddf3a28f368cf4504baf6ce8d2dff64ee7a2737e6bd0";


        private static void ThrowIfEqualityMismatch(string name, bool expectedEqual)
        {
            throw new ArgumentException($"The two {name} were unexpectedly {(expectedEqual ? "unequal" : "equal")}.");
        }

        public static void AssertEqual(ReadOnlySpan<byte> first, ReadOnlySpan<byte> second, bool expectedEqual)
        {
            if (first.SequenceEqual(second) != expectedEqual)
            {
                ThrowIfEqualityMismatch("byte arrays", expectedEqual);
            }
        }

        public static void AssertEqual(in Scalar4x64 first, in Scalar4x64 second, bool expectedEqual)
        {
            if (first.Equals(second) != expectedEqual)
            {
                ThrowIfEqualityMismatch("scalar4x64 values", expectedEqual);
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public static void AssertEqual(in Scalar8x32 first, in Scalar8x32 second, bool expectedEqual)
        {
            if (first.Equals(second) != expectedEqual)
            {
                ThrowIfEqualityMismatch("Scalar8x32 values", expectedEqual);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        public static void AssertEqual(in Scalar4x64Alt first, in Scalar4x64Alt second, bool expectedEqual)
        {
            if (first.Equals(second) != expectedEqual)
            {
                ThrowIfEqualityMismatch("scalar4x64Alt values", expectedEqual);
            }
        }

        public static void AssertEqual(in Scalar4x64Alt first, in Scalar4x64 second, bool expectedEqual)
        {
            if (first.Equals(second) != expectedEqual)
            {
                ThrowIfEqualityMismatch("scalar4x64 values", expectedEqual);
            }
        }


        public static Span<byte> GetBytes(int len)
        {
            if (len < 0 || len > Hex.Length / 2)
            {
                throw new ArgumentOutOfRangeException(nameof(len));
            }

            return Base16.Decode(Hex).AsSpan(len);
        }

        public static Scalar4x64 BuildScalar()
        {
            Span<byte> ba = GetBytes(32);
            return new Scalar4x64(ba, out bool of);
        }

        public static (Scalar4x64, Scalar4x64) BuildScalars()
        {
            Span<byte> ba = GetBytes(64);
            Scalar4x64 sc1 = new(ba.Slice(0, 32), out bool of1);
            Scalar4x64 sc2 = new(ba.Slice(32, 32), out bool of2);
            AssertEqual(sc1, sc2, false);
            return (sc1, sc2);
        }

        public static (Scalar4x64Alt, Scalar4x64Alt) BuildScalarAltss()
        {
            Span<byte> ba = GetBytes(64);
            Scalar4x64Alt sc1 = new(ba.Slice(0, 32), out bool of1);
            Scalar4x64Alt sc2 = new(ba.Slice(32, 32), out bool of2);
            AssertEqual(sc1, sc2, false);
            return (sc1, sc2);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public static (Scalar8x32, Scalar8x32) BuildScalars32()
        {
            Span<byte> ba = GetBytes(64);
            Scalar8x32 sc1 = new(ba.Slice(0, 32), out bool of1);
            Scalar8x32 sc2 = new(ba.Slice(32, 32), out bool of2);
            AssertEqual(sc1, sc2, false);
            return (sc1, sc2);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
