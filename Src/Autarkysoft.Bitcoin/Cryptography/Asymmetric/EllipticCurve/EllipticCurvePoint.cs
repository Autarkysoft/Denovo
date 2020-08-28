// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve
{
    /// <summary>
    /// Represents a (X,Y) coordinate pair for elliptic curve cryptography (ECC) structures.
    /// </summary>
    public readonly struct EllipticCurvePoint : IEquatable<EllipticCurvePoint>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EllipticCurvePoint"/> with given x and y coordinates.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        public EllipticCurvePoint(BigInteger x, BigInteger y)
        {
            X = x;
            Y = y;
        }


        /// <summary>
        /// X coordinate
        /// </summary>
        public BigInteger X { get; }
        /// <summary>
        /// Y coordinate
        /// </summary>
        public BigInteger Y { get; }


        /// <summary>
        /// Represents the point at infinity.
        /// </summary>
        public static EllipticCurvePoint InfinityPoint => new EllipticCurvePoint(0, 0);


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool operator ==(EllipticCurvePoint p1, EllipticCurvePoint p2)
        {
            // TODO: (x,y) is equal to (x,-y)
            return p1.X == p2.X && p1.Y == p2.Y;
        }
        public static bool operator !=(EllipticCurvePoint p1, EllipticCurvePoint p2) => !(p1 == p2);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is EllipticCurvePoint point && this == point;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// Checks if the value of the given <see cref="EllipticCurvePoint"/> is equal to the value of this instance.
        /// </summary>
        /// <param name="other">Other <see cref="EllipticCurvePoint"/> value to compare to this instance.</param>
        /// <returns>true if the value is equal to the value of this instance; otherwise, false.</returns>
        public bool Equals(EllipticCurvePoint other) => this == other;
    }
}
