// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// Point storage
    /// </summary>
    public readonly struct PointStorage
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PointStorage"/> using the given parameters.
        /// </summary>
        /// <param name="x26">x coordinate</param>
        /// <param name="y26">y coordinate</param>
        public PointStorage(in UInt256_10x26 x26, in UInt256_10x26 y26)
        {
            x = x26.Normalize().ToUInt256_8x32();
            y = y26.Normalize().ToUInt256_8x32();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PointStorage"/> using the given parameters.
        /// </summary>
        /// <param name="x32">x coordinate</param>
        /// <param name="y32">y coordinate</param>
        public PointStorage(in UInt256_8x32 x32, in UInt256_8x32 y32)
        {
            x = x32;
            y = y32;
        }


        /// <summary>
        /// Coordinates
        /// </summary>
        public readonly UInt256_8x32 x, y;


        /// <summary>
        /// Converts this instance to a <see cref="Point"/>.
        /// </summary>
        /// <returns>Result</returns>
        public Point ToPoint() => new Point(x.ToUInt256_10x26(), y.ToUInt256_10x26(), false);


        /// <summary>
        /// Conditional move. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is true (=1).
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="flag">Zero or one. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is one.</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointStorage CMov(in PointStorage r, in PointStorage a, uint flag)
        {
            UInt256_8x32 rx = UInt256_8x32.CMov(r.x, a.x, flag);
            UInt256_8x32 ry = UInt256_8x32.CMov(r.y, a.y, flag);
            return new PointStorage(rx, ry);
        }
    }
}
