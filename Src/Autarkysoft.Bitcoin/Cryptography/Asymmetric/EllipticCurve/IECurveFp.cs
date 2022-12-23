// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve
{
    /// <summary>
    /// Defines methods and properties that an elliptic curve over Fp should implement.
    /// </summary>
    [Obsolete]
    public interface IECurveFp
    {
        /// <summary>
        /// Prime
        /// </summary>
        BigInteger P { get; }

        /// <summary>
        /// Curve element 'a'
        /// </summary>
        BigInteger A { get; }

        /// <summary>
        /// Curve element 'b'
        /// </summary>
        BigInteger B { get; }

        /// <summary>
        /// Order of <see cref="G"/>
        /// </summary>
        BigInteger N { get; }

        /// <summary>
        /// Base point
        /// </summary>
        EllipticCurvePoint G { get; }

        /// <summary>
        /// Cofactor
        /// </summary>
        short H { get; }

        /// <summary>
        /// [optional] seed used for creating the curve.
        /// </summary>
        byte[] Seed => null;

        /// <summary>
        /// Size of the curve in bits. 
        /// <para/>= (int)Math.Ceiling(BigInteger.Log(p, 2));
        /// </summary>
        int SizeInBits { get; }

        /// <summary>
        /// Approximate level of security in bits that the curve offers, also known as "t". 
        /// <para/> Log2(P)/2
        /// </summary>
        int SecurityLevel { get; }

        /// <summary>
        /// Checks to see if the given point is on the elliptic curve.
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <returns>True if the point is on the curve, false if otherwise.</returns>
        bool IsOnCurve(EllipticCurvePoint point);
    }
}
