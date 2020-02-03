// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve
{
    /// <summary>
    /// Implementation of signatures produced by Elliptic Curve digital signature algorithms (ECDSA and ECSDSA)
    /// holding R and S values.
    /// </summary>
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
        public Signature(BigInteger r, BigInteger s, byte v = 0)
        {
            R = r;
            S = s;
            RecoveryId = v;
        }



        private const byte SequenceTag = 0x30;
        private const byte IntegerTag = 0x02;

        /// <summary>
        /// The r value in a signature
        /// </summary>
        public BigInteger R { get; set; }

        /// <summary>
        /// The s value in a signature
        /// </summary>
        public BigInteger S { get; set; }

        /// <summary>
        /// The one byte recovery ID used in fixed length message signatures
        /// </summary>
        public byte RecoveryId { get; set; }

        /// <summary>
        /// Signature hash type used during transaction signing
        /// </summary>
        public SigHashType SigHash { get; set; }



        /// <summary>
        /// Creates a new instance of <see cref="Signature"/> by reading it from a DER encoded byte array.
        /// Return value indicates success.
        /// </summary>
        /// <param name="derSig">Signature bytes encoded using DER encoding</param>
        /// <param name="result">Resulting signature (empty in case of failure)</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryRead(byte[] derSig, out Signature result, out string error)
        {
            result = null;

            if (derSig == null)
            {
                error = "Byte array can not be null.";
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

            // Note that even though lengths are DER-lengths due to small length of r and s they will always be 1 byte long.

            // Min = 3006[0201(01)0201(01)]-01
            // Max = 3046[0221(00{32})0221(00{32})]-01
            if (derSig.Length < 9 || derSig.Length > 73)
            {
                error = "Invalid DER encoding length.";
                return false;
            }

            if (derSig[0] != SequenceTag)
            {
                error = "Sequence tag was not found in DER encoded signature.";
                return false;
            }

            if (derSig[1] != derSig.Length - 3)
            {
                error = "Invalid data length according to sequence length.";
                return false;
            }

            if (derSig[2] != IntegerTag)
            {
                error = "First integer tag was not found in DER encoded signature.";
                return false;
            }

            int rLen = derSig[3];
            if (rLen == 0 || (rLen == 1 && derSig[4] == 0) || rLen > 33)
            {
                error = "Invalid r length.";
                return false;
            }

            // -8 is 4 bytes read so far + 1 SigHash + 3 byte at least for S
            if (derSig.Length - 8 < rLen)
            {
                error = "Invalid data length according to first integer length.";
                return false;
            }

            if (derSig[rLen + 4] != IntegerTag)
            {
                error = "Second integer tag was not found in DER encoded signature.";
                return false;
            }

            int sLen = derSig[rLen + 5];
            if (sLen == 0 || (sLen == 1 && derSig[rLen + 6] == 0) || sLen > 33)
            {
                error = "Invalid s length.";
                return false;
            }

            // +7 is 2 for seq+len, 2 for r_int+len, 2 for s_int+len, 1 for SigHash
            if (rLen + sLen + 7 != derSig.Length)
            {
                error = "Invalid data length according to second integer length.";
                return false;
            }

            //TODO: both r and s should use ModUInt256 type
            byte[] rBa = new byte[rLen];
            Buffer.BlockCopy(derSig, 4, rBa, 0, rLen);
            if ((rBa[0] & 0x80) != 0 || (rLen > 1 && (rBa[0] == 0) && (rBa[1] & 0x80) == 0))
            {
                error = "Invalid r format.";
                return false;
            }

            byte[] sBa = new byte[sLen];
            Buffer.BlockCopy(derSig, rLen + 6, sBa, 0, sLen);
            if ((sBa[0] & 0x80) != 0 || (sLen > 1 && (sBa[0] == 0) && (sBa[1] & 0x80) == 0))
            {
                error = "Invalid s format.";
                return false;
            }

            result = new Signature()
            {
                R = new BigInteger(rBa, false, true),
                S = new BigInteger(sBa, false, true),
                SigHash = (SigHashType)derSig[^1]
            };

            error = null;
            return true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Signature"/> by reading it from a fixed length byte array encoding
        /// used by Schnorr signatures. Return value indicates success.
        /// </summary>
        /// <param name="data">Signature bytes containing the signature</param>
        /// <param name="result">Resulting signature (empty in case of failure)</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryReadSchnorr(byte[] data, out Signature result, out string error)
        {
            if (data == null || data.Length == 0)
            {
                result = null;
                error = "Byte array can not be null or empty.";
                return false;
            }

            // TODO: we assume Shrnorr sigs (bytes taken from a PushDataOp in a tx) also have the SigHash at the end 
            // Schnorr is still a proposal at this time and there is no tx cases to check
            if (data.Length != 65)
            {
                result = null;
                error = "Schnorr signature length must be 65 bytes.";
                return false;
            }

            result = new Signature()
            {
                R = new BigInteger(((ReadOnlySpan<byte>)data).Slice(0, 32), true, true),
                S = new BigInteger(((ReadOnlySpan<byte>)data).Slice(32, 32), true, true),
                SigHash = (SigHashType)data[^1]
            };

            error = null;
            return true;
        }


        /// <summary>
        /// Converts this instance to its byte array representation using DER encoding with the specified
        /// <see cref="SigHashType"/> and returns the result.
        /// </summary>
        /// <returns>A DER encoded signature bytes with <see cref="SigHashType"/></returns>
        public byte[] ToByteArray()
        {
            FastStream stream = new FastStream();
            WriteToStream(stream);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Converts this instance to its byte array representation with the specified <see cref="SigHashType"/>
        /// using a fixed length encoding used in Schnorr signatures and returns the result.
        /// </summary>
        /// <returns>A DER encoded signature bytes with <see cref="SigHashType"/></returns>
        public byte[] ToByteArraySchnorr()
        {
            FastStream stream = new FastStream();
            WriteToStreamSchnorr(stream);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Converts this instance to its byte array representation using DER encoding with the specified
        /// <see cref="SigHashType"/> and writes the result to the given <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void WriteToStream(FastStream stream)
        {
            byte[] rBa = R.ToByteArray(isBigEndian: true);
            byte[] sBa = S.ToByteArray(isBigEndian: true);

            stream.Write(SequenceTag);
            stream.Write((byte)(rBa.Length + sBa.Length + 4));
            stream.Write(IntegerTag);
            stream.Write((byte)rBa.Length);
            stream.Write(rBa);
            stream.Write(IntegerTag);
            stream.Write((byte)sBa.Length);
            stream.Write(sBa);
            stream.Write((byte)SigHash);
        }

        /// <summary>
        /// Converts this instance to its byte array representation with the specified <see cref="SigHashType"/>
        /// using a fixed length encoding used in Schnorr signatures and writes the result to the given 
        /// <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void WriteToStreamSchnorr(FastStream stream)
        {
            byte[] rBa = R.ToByteArray(isBigEndian: true);
            byte[] sBa = S.ToByteArray(isBigEndian: true);

            stream.Write(rBa, 32);
            stream.Write(sBa, 32);
            stream.Write((byte)SigHash);
        }

        /// <summary>
        /// Converts this instance to a fixed length (65) byte array with a starting recovery ID and
        /// writes the result to the given <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="isCompressed">Indicates whether compressed public key was used for creation of the address</param>
        public void WriteToStreamWithRecId(FastStream stream, bool isCompressed)
        {
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
