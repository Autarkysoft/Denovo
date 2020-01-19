// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using System;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs
{
    public class PublicKey
    {
        public PublicKey(EllipticCurvePoint ellipticCurvePoint)
        {
            point = ellipticCurvePoint;
        }

        private readonly EllipticCurvePoint point;


        public static bool TryRead(byte[] pubBa, out PublicKey pubK, out string error)
        {
            throw new NotImplementedException();
        }

        public byte[] ToByteArray(bool useCompressed)
        {
            throw new NotImplementedException();
        }

        public EllipticCurvePoint ToPoint()
        {
            throw new NotImplementedException();
        }


       
    }
}
