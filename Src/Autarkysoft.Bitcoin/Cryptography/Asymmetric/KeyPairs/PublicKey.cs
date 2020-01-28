// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using System;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs
{
    /// <summary>
    /// The public part of the key pair
    /// </summary>
    public class PublicKey
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PublicKey"/> with the given <see cref="EllipticCurvePoint"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="ellipticCurvePoint">The point to use.</param>
        public PublicKey(EllipticCurvePoint ellipticCurvePoint)
        {
            calc.CheckOnCurve(ellipticCurvePoint);
            point = ellipticCurvePoint;
        }



        private readonly EllipticCurvePoint point;
        private static readonly EllipticCurveCalculator calc = new EllipticCurveCalculator();



        /// <summary>
        /// Converts the given byte array to a <see cref="PublicKey"/>. Return value indicates success.
        /// </summary>
        /// <param name="pubBa">Byte array containing a public key</param>
        /// <param name="pubK">The result</param>
        /// <returns>True if the conversion was successful, otherwise false.</returns>
        public static bool TryRead(byte[] pubBa, out PublicKey pubK)
        {
            if (calc.TryGetPoint(pubBa, out EllipticCurvePoint pt))
            {
                pubK = new PublicKey(pt);
                return true;
            }
            else
            {
                pubK = null;
                return false;
            }
        }

        /// <summary>
        /// Converts this instance to its byte array representation.
        /// </summary>
        /// <param name="useCompressed">If true the 33 byte representation is returned, otherwise the 65 one.</param>
        /// <returns>An array of bytes with 33 or 65 length</returns>
        public byte[] ToByteArray(bool useCompressed)
        {
            byte[] xBytes = point.X.ToByteArray(true, true);
            // Public key's x and y bytes must always be 32 bytes so they may need padding
            if (useCompressed)
            {
                byte[] result = new byte[33];
                result[0] = point.Y.IsEven ? (byte)2 : (byte)3;
                Buffer.BlockCopy(xBytes, 0, result, 33 - xBytes.Length, xBytes.Length);
                return result;
            }
            else
            {
                byte[] result = new byte[65];
                result[0] = 4;
                byte[] yBytes = point.Y.ToByteArray(true, true);
                Buffer.BlockCopy(xBytes, 0, result, 33 - xBytes.Length, xBytes.Length);
                Buffer.BlockCopy(yBytes, 0, result, 65 - yBytes.Length, yBytes.Length);
                return result;
            }
        }

        /// <summary>
        /// Returns the <see cref="EllipticCurvePoint"/> of this public key.
        /// </summary>
        /// <returns></returns>
        public EllipticCurvePoint ToPoint()
        {
            return point;
        }
    }
}
