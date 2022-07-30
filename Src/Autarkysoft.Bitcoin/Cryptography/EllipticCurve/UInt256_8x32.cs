// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// 256-bit unsigned integer using radix-2^32 representation
    /// </summary>
    public readonly struct UInt256_8x32
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_8x32"/> using the given parameters.
        /// </summary>
        /// <param name="u0">1st 32 bits (least significant)</param>
        /// <param name="u1">2nd 32 bits</param>
        /// <param name="u2">3rd 32 bits</param>
        /// <param name="u3">4th 32 bits</param>
        /// <param name="u4">5th 32 bits</param>
        /// <param name="u5">6th 32 bits</param>
        /// <param name="u6">7th 32 bits</param>
        /// <param name="u7">8th 32 bits (most significant)</param>
        public UInt256_8x32(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7)
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3;
            b4 = u4; b5 = u5; b6 = u6; b7 = u7;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UInt256_8x32"/> using the given parameters (9x 26 bits + 22 bits).
        /// </summary>
        /// <param name="u26">UInt256 in radix-2^26</param>
        public UInt256_8x32(in UInt256_10x26 u26)
        {
            b0 = u26.b0 | u26.b1 << 26;
            b1 = u26.b1 >> 6 | u26.b2 << 20;
            b2 = u26.b2 >> 12 | u26.b3 << 14;
            b3 = u26.b3 >> 18 | u26.b4 << 8;
            b4 = u26.b4 >> 24 | u26.b5 << 2 | u26.b6 << 28;
            b5 = u26.b6 >> 4 | u26.b7 << 22;
            b6 = u26.b7 >> 10 | u26.b8 << 16;
            b7 = u26.b8 >> 16 | u26.b9 << 10;
        }


        /// <summary>
        /// Bit chunks
        /// </summary>
        public readonly uint b0, b1, b2, b3, b4, b5, b6, b7;


        /// <summary>
        /// Converts this instance to <see cref="UInt256_10x26"/>
        /// </summary>
        /// <returns>Result</returns>
        public UInt256_10x26 ToUInt256_10x26() => new UInt256_10x26(b0, b1, b2, b3, b4, b5, b6, b7);
    }
}
