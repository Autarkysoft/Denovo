// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

#if !NET10_0
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    /// <summary>
    /// 128-bit unsigned integer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct UInt128 : IEquatable<UInt128>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UInt128"/> using the given parameters.
        /// </summary>
        /// <param name="upper">The upper 64-bits of the 128-bit value.</param>
        /// <param name="lower">The lower 64-bits of the 128-bit value.</param>
        public UInt128(ulong upper, ulong lower)
        {
            this.upper = upper;
            this.lower = lower;
        }


        private readonly ulong lower;
        private readonly ulong upper;


        /// <summary>
        /// Exposes the lower 64 bits for testing.
        /// </summary>
        public ulong Lower => lower;
        /// <summary>
        /// Exposes the upper 64 bits for testing.
        /// </summary>
        public ulong Upper => upper;

        /// <summary>
        /// Adds two values together to compute their sum.
        /// </summary>
        /// <param name="left">The value to which right is added.</param>
        /// <param name="right">The value that is added to left.</param>
        /// <returns>The sum of left and right.</returns>
        public static UInt128 operator +(UInt128 left, UInt128 right)
        {
            ulong lower = left.lower + right.lower;
            ulong carry = (lower < left.lower) ? 1UL : 0UL;

            ulong upper = left.upper + right.upper + carry;
            return new UInt128(upper, lower);
        }

        /// <summary>
        /// Multiplies two values together to compute their product.
        /// </summary>
        /// <param name="left">The value that right multiplies.</param>
        /// <param name="right">The value that multiplies left.</param>
        /// <returns>The product of left multiplied-by right.</returns>
        public static UInt128 operator *(UInt128 left, UInt128 right)
        {
            // Copied from dotnet 10 source code
            uint al = (uint)left.lower;
            uint ah = (uint)(left.lower >> 32);
            uint bl = (uint)right.lower;
            uint bh = (uint)(right.lower >> 32);

            ulong mull = ((ulong)al) * bl;
            ulong t = ((ulong)ah) * bl + (mull >> 32);
            ulong tl = ((ulong)al) * bh + (uint)t;

            ulong low = tl << 32 | (uint)mull;
            ulong up = ((ulong)ah) * bh + (t >> 32) + (tl >> 32);

            up += (left.upper * right.lower) + (left.lower * right.upper);
            return new UInt128(up, low);
        }

        /// <summary>
        /// Shifts a value right by a given amount.
        /// </summary>
        /// <param name="value">The value that is shifted right by <paramref name="shiftAmount"/>.</param>
        /// <param name="shiftAmount">The amount by which value is shifted right.</param>
        /// <returns>The result of shifting value right by <paramref name="shiftAmount"/>.</returns>
        public static UInt128 operator >>(UInt128 value, int shiftAmount)
        {
            // C# automatically masks the shift amount for UInt64 to be 0x3F. So we
            // need to specially handle things if the 7th bit is set.

            shiftAmount &= 0x7F;

            if ((shiftAmount & 0x40) != 0)
            {
                // In the case it is set, we know the entire upper bits must be zero
                // and so the lower bits are just the upper shifted by the remaining
                // masked amount

                ulong lower = value.upper >> shiftAmount;
                return new UInt128(0, lower);
            }
            else if (shiftAmount != 0)
            {
                // Otherwise we need to shift both upper and lower halves by the masked
                // amount and then or that with whatever bits were shifted "out" of upper

                ulong lower = (value.lower >> shiftAmount) | (value.upper << (64 - shiftAmount));
                ulong upper = value.upper >> shiftAmount;

                return new UInt128(upper, lower);
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Computes the bitwise-AND of two values.
        /// </summary>
        /// <param name="left">The value to AND with right.</param>
        /// <param name="right">The value to AND with left.</param>
        /// <returns>The bitwise-AND of left and right.</returns>
        public static UInt128 operator &(UInt128 left, UInt128 right) => new UInt128(left.upper & right.upper, left.lower & right.lower);

        /// <summary>
        /// Computes the bitwise-OR of two values.
        /// </summary>
        /// <param name="left">The value to OR with right.</param>
        /// <param name="right">The value to OR with left.</param>
        /// <returns>The bitwise-OR of left and right.</returns>
        public static UInt128 operator |(UInt128 left, UInt128 right) => new UInt128(left.upper | right.upper, left.lower | right.lower);

        /// <summary>
        /// Computes the exclusive-OR of two values.
        /// </summary>
        /// <param name="left">The value to XOR with right.</param>
        /// <param name="right">The value to XOR with left.</param>
        /// <returns>The exclusive-OR of left and right.</returns>
        public static UInt128 operator ^(UInt128 left, UInt128 right) => new UInt128(left.upper ^ right.upper, left.lower ^ right.lower);
        /// <summary>
        /// Computes the ones-complement representation of a given value.
        /// </summary>
        /// <param name="value">The value for which to compute the ones-complement.</param>
        /// <returns>The ones-complement of value.</returns>
        public static UInt128 operator ~(UInt128 value) => new UInt128(~value.upper, ~value.lower);

        /// <summary>
        /// Explicitly converts a 128-bit unsigned integer to a <see cref="ulong" /> value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns><paramref name="value" /> converted to a <see cref="ulong" />.</returns>
        public static explicit operator ulong(UInt128 value) => value.lower;

        /// <summary>
        /// Implicitly converts a <see cref="ulong" /> value to a 128-bit unsigned integer.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns><paramref name="value" /> converted to a 128-bit unsigned integer.</returns>
        public static implicit operator UInt128(ulong value) => new UInt128(0, value);

        /// <summary>
        /// Compares two values to determine equality.
        /// </summary>
        /// <param name="left">The value to compare with right.</param>
        /// <param name="right">The value to compare with left.</param>
        /// <returns>true if left is equal to right; otherwise, false.</returns>
        public static bool operator ==(UInt128 left, UInt128 right) => (left.lower == right.lower) && (left.upper == right.upper);

        /// <summary>
        /// Compares two values to determine inequality.
        /// </summary>
        /// <param name="left">The value to compare with right.</param>
        /// <param name="right">The value to compare with left.</param>
        /// <returns>True if left is not equal to right; otherwise, false.</returns>
        public static bool operator !=(UInt128 left, UInt128 right) => (left.lower != right.lower) || (left.upper != right.upper);


        ///<inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => (obj is UInt128 other) && Equals(other);
        /// <summary>
        /// Indicates whether the given <see cref="UInt128"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The value to compare this instance to.</param>
        /// <returns>True if the given <see cref="UInt128"/> is equal to this instance; otherwise, false.</returns>
        public bool Equals(UInt128 other) => this == other;
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(upper, lower);
    }
}
#endif
