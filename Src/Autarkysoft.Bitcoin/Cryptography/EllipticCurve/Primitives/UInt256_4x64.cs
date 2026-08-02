// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    /// <summary>
    /// 256-bit unsigned integer using radix-2^64 representation
    /// </summary>
    public readonly struct UInt256_4x64
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_4x64"/> using the given parameters.
        /// </summary>
        /// <param name="u0">1st 64 bits (least significant)</param>
        /// <param name="u1">2nd 64 bits</param>
        /// <param name="u2">3rd 64 bits</param>
        /// <param name="u3">4th 64 bits (most significant)</param>
        public UInt256_4x64(ulong u0, ulong u1, ulong u2, ulong u3)
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_4x64"/> using the given parameters (5x 52 bits + 48 bits).
        /// </summary>
        /// <param name="u52">UInt256 in radix-2^52</param>
        public UInt256_4x64(in UInt256_5x52 u52)
        {
#if DEBUG
            u52.Verify();
            Debug.Assert(u52.isNormalized);
#endif
            b0 = u52.b0 | u52.b1 << 52;
            b1 = u52.b1 >> 12 | u52.b2 << 40;
            b2 = u52.b2 >> 24 | u52.b3 << 28;
            b3 = u52.b3 >> 36 | u52.b4 << 16;
        }


        /// <summary>
        /// Bit chunks
        /// </summary>
        public readonly ulong b0, b1, b2, b3;


        /// <summary>
        /// Converts this instance to <see cref="UInt256_5x52"/>
        /// </summary>
        /// <returns>Result</returns>
        public UInt256_5x52 ToUInt256_5x52() => new UInt256_5x52(b0, b1, b2, b3);


        /// <summary>
        /// Conditional move. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is true (=1).
        /// </summary>
        /// <remarks>
        /// This method is constant time.
        /// </remarks>
        /// <param name="r">Destination</param>
        /// <param name="a">Source</param>
        /// <param name="flag">Zero or one. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is one.</param>
        /// <returns><paramref name="a"/> if flag was one; otherwise r.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt256_4x64 CMov(in UInt256_4x64 r, in UInt256_4x64 a, uint flag)
        {
            Debug.Assert(flag == 0 || flag == 1);

            ulong mask0 = flag + ~0UL;
            ulong mask1 = ~mask0;
            return new UInt256_4x64(
                (r.b0 & mask0) | (a.b0 & mask1),
                (r.b1 & mask0) | (a.b1 & mask1),
                (r.b2 & mask0) | (a.b2 & mask1),
                (r.b3 & mask0) | (a.b3 & mask1));
        }

        /// <summary>
        /// Checks if the value of the given <see cref="UInt256_4x64"/> is equal to the value of this instance.
        /// </summary>
        /// <param name="other">Other <see cref="UInt256_4x64"/> value to compare to this instance.</param>
        /// <returns>true if the value is equal to the value of this instance; otherwise, false.</returns>
        public bool Equals(in UInt256_4x64 other)
        {
            return ((b0 ^ other.b0) | (b1 ^ other.b1) | (b2 ^ other.b2) | (b3 ^ other.b3)) == 0;
        }
    }
}
