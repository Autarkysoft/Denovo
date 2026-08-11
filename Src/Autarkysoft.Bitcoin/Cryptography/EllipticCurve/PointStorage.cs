// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
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
        /// <param name="x52">x coordinate</param>
        /// <param name="y52">y coordinate</param>
        public PointStorage(in UInt256_5x52 x52, in UInt256_5x52 y52)
        {
            x = x52.Normalize().ToUInt256_4x64();
            y = y52.Normalize().ToUInt256_4x64();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PointStorage"/> using the given parameters.
        /// </summary>
        /// <param name="x64">x coordinate</param>
        /// <param name="y64">y coordinate</param>
        public PointStorage(in UInt256_4x64 x64, in UInt256_4x64 y64)
        {
            x = x64;
            y = y64;
        }


        /// <summary>
        /// Coordinates
        /// </summary>
        public readonly UInt256_4x64 x, y;


        /// <summary>
        /// Converts this instance to a <see cref="Point"/>.
        /// </summary>
        /// <returns>Result</returns>
        public Point ToPoint()
        {
            // secp256k1_ge_from_storage
            Point result = new Point(x.ToUInt256_5x52(), y.ToUInt256_5x52(), false);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Conditional move. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is true (=1).
        /// Constant-time
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="flag">Zero or one. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is one.</param>
        /// <returns>Result</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PointStorage CMov(in PointStorage r, in PointStorage a, uint flag)
        {
            // secp256k1_ge_storage_cmov
            UInt256_4x64 rx = UInt256_4x64.CMov(r.x, a.x, flag);
            UInt256_4x64 ry = UInt256_4x64.CMov(r.y, a.y, flag);
            return new PointStorage(rx, ry);
        }
    }
}
