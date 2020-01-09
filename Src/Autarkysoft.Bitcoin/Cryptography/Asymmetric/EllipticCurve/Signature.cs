// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve
{
    public class Signature
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Signature"/> with empty parameters. Can be used to read stream.
        /// </summary>
        public Signature()
        {

        }
        /// <summary>
        /// Initializes a new instance of <see cref="Signature"/> using the given parameters.
        /// </summary>
        /// <param name="r">R value</param>
        /// <param name="s">S value</param>
        /// <param name="v">Recovery ID</param>
        public Signature(BigInteger r, BigInteger s, byte? v = null)
        {
            R = r;
            S = s;
            RecoveryId = v;
        }



        private const byte SequenceTag = 0x30;
        private const byte IntegerTag = 0x02;

        public BigInteger R { get; set; }
        public BigInteger S { get; set; }
        public byte? RecoveryId { get; set; }
        public SigHashType SigHash { get; set; }




        public static bool TryRead(byte[] derSig, out Signature result, out string error)
        {
            result = new Signature();

            if (derSig is null || derSig.Length == 0)
            {
                error = "Byte array can not be null or empty.";
                return false;
            }

            // Signature format is: 
            //   Sequence tag = 0x30            -> 1 byte
            //   Sequence length ->             -> 1 byte
            //     R
            //       Int tag = 0x02             -> 1 byte
            //       Int length -> DerInt       -> 1 byte
            //       R bytes in big-endian      -> 0<len<=33
            //     S
            //       Int tag = 0x02             -> 1 byte
            //       Int length ->              -> 1 byte
            //       S bytes in big-endian      -> 0<len<=33
            //   SigHashType                    -> 1 byte

            if (derSig.Length < 9 || derSig.Length > 73)
            {
                error = "Invalid DER encoding length";
                return false;
            }

            if (derSig[0] != SequenceTag)
            {
                error = "Sequence tag was not found in DER encoded signature.";
                return false;
            }

            if (derSig[1] != derSig.Length - 3)
            {
                error = "Invalid DER encoding sequence length.";
                return false;
            }

            if (derSig[2] != IntegerTag)
            {
                error = "First integer tag was not found in DER encoded signature.";
                return false;
            }

            int rLen = derSig[3];

            if (rLen == 0 || rLen == 1 && derSig[4] == 0 || rLen > 33)
            {
                error = "Invalid r length.";
                return false;
            }

            if (derSig.Length - 3 < rLen)
            {
                error = "Data length is not valid according to first integer.";
                return false;
            }

            byte[] rBa = new byte[rLen];
            Buffer.BlockCopy(derSig, 4, rBa, 0, rLen);
            if ((rBa[0] & 0x80) != 0 || rLen > 1 && (rBa[1] == 0) && (rBa[2] & 0x80) != 0)
            {
                error = "Invalid r format.";
                return false;
            }
            Array.Reverse(rBa);
            result.R = new BigInteger(rBa);


            if (derSig[rLen + 4] != IntegerTag)
            {
                error = "Second integer tag was not found in DER encoded signature.";
                return false;
            }

            int sLen = derSig[rLen + 5];

            if (sLen == 0 || sLen == 1 && derSig[rLen + 6] == 0 || sLen > 33)
            {
                error = "Invalid s length.";
                return false;
            }

            if (derSig.Length - rLen - 5 < sLen)
            {
                error = "Data length is not valid according to second integer.";
                return false;
            }

            byte[] sBa = new byte[sLen];
            Buffer.BlockCopy(derSig, rLen + 6, sBa, 0, sLen);
            if ((sBa[0] & 0x80) != 0 || sLen > 1 && (sBa[1] == 0) && (sBa[2] & 0x80) != 0)
            {
                error = "Invalid s format.";
                return false;
            }
            Array.Reverse(sBa);
            result.S = new BigInteger(sBa);

            error = null;
            return true;
        }



        public void WriteToStream(FastStream stream)
        {
            byte[] rBa = R.ToByteArray(isBigEndian: true);
            byte[] sBa = S.ToByteArray(isBigEndian: true);

            stream.Write(SequenceTag);
            stream.Write(rBa.Length + sBa.Length + 4);
            stream.Write(IntegerTag);
            stream.Write(rBa.Length);
            stream.Write(rBa);
            stream.Write(IntegerTag);
            stream.Write(sBa.Length);
            stream.Write(sBa);
            stream.Write((byte)SigHash);
        }

        public void WriteToStreamWithRecId(FastStream stream, bool isCompressed)
        {
            // TODO: write a test case for when R and/or S are smaller than 32 bytes

            stream.Write((byte)(27 + RecoveryId + (isCompressed ? 4 : 0)));

            byte[] temp = R.ToByteArray(true, true);
            if (temp.Length == 32)
            {
                stream.Write(temp);
            }
            else
            {
                byte[] rBa = new byte[32];
                Buffer.BlockCopy(temp, 0, rBa, 32 - temp.Length, temp.Length);
                stream.Write(rBa);
            }

            temp = S.ToByteArray(true, true);
            if (temp.Length == 32)
            {
                stream.Write(temp);
            }
            else
            {
                byte[] sBa = new byte[32];
                Buffer.BlockCopy(temp, 0, sBa, 32 - temp.Length, temp.Length);
                stream.Write(sBa);
            }
        }

    }
}
