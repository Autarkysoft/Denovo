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
        private Signature()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Signature"/> using the given parameters.
        /// </summary>
        /// <param name="r">R value</param>
        /// <param name="s">S value</param>
        /// <param name="v">[Default value = 0] Recovery ID, an optional byte created during signing process</param>
        public Signature(BigInteger r, BigInteger s, byte v = 0)
        {
            R = r;
            S = s;
            RecoveryId = v;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Signature"/> using the given parameters.
        /// </summary>
        /// <param name="r">R value</param>
        /// <param name="s">S value</param>
        /// <param name="sigHash">Signature hash type</param>
        public Signature(BigInteger r, BigInteger s, SigHashType sigHash)
        {
            R = r;
            S = s;
            SigHash = sigHash;
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
        /// Creates a new instance of <see cref="Signature"/> by reading it from a DER encoded byte array with loose rules.
        /// Return value indicates success.
        /// </summary>
        /// <param name="derSig">Signature bytes encoded using DER encoding</param>
        /// <param name="result">Resulting signature (null in case of failure)</param>
        /// <param name="error">Error message</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryReadLoose(byte[] derSig, out Signature result, out Errors error)
        {
            result = null;

            if (derSig == null)
            {
                error = Errors.NullBytes;
                return false;
            }

            // Min = 3006[0201(01)0201(01)]-01
            if (derSig.Length < 9)
            {
                // This also handles the Length == 0 case
                error = Errors.InvalidDerEncodingLength;
                return false;
            }

            FastStreamReader stream = new FastStreamReader(derSig);

            if (!stream.TryReadByte(out byte seqTag) || seqTag != SequenceTag)
            {
                error = Errors.MissingDerSeqTag;
                return false;
            }

            if (!stream.TryReadDerLength(out int seqLen))
            {
                error = Errors.InvalidDerSeqLength;
                return false;
            }

            if (seqLen < 6 || !stream.CheckRemaining(seqLen + 1)) // +1 is the SigHash byte (at least 1 byte)
            {
                error = Errors.InvalidDerSeqLength;
                return false;
            }

            if (!stream.TryReadByte(out byte intTag1) || intTag1 != IntegerTag)
            {
                error = Errors.MissingDerIntTag1;
                return false;
            }

            if (!stream.TryReadDerLength(out int rLen) || rLen == 0)
            {
                error = Errors.InvalidDerRLength;
                return false;
            }

            if (!stream.TryReadByteArray(rLen, out byte[] rBa))
            {
                error = Errors.EndOfStream;
                return false;
            }

            if (!stream.TryReadByte(out byte intTag2) || intTag2 != IntegerTag)
            {
                error = Errors.MissingDerIntTag2;
                return false;
            }

            if (!stream.TryReadDerLength(out int sLen) || sLen == 0)
            {
                error = Errors.InvalidDerSLength;
                return false;
            }

            if (!stream.TryReadByteArray(sLen, out byte[] sBa))
            {
                error = Errors.EndOfStream;
                return false;
            }

            // Make sure _at least_ one byte remains to be read
            if (!stream.CheckRemaining(1))
            {
                error = Errors.EndOfStream;
                return false;
            }

            result = new Signature()
            {
                R = new BigInteger(rBa, true, true),
                S = new BigInteger(sBa, true, true),
                SigHash = (SigHashType)derSig[^1]
            };

            error = Errors.None;
            return true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Signature"/> by reading it from a DER encoded byte array while requiring
        /// strict encoding according to BIP-66.
        /// Return value indicates success.
        /// </summary>
        /// <param name="derSig">Signature bytes encoded using DER encoding</param>
        /// <param name="result">Resulting signature (null in case of failure)</param>
        /// <param name="error">Error message</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryReadStrict(byte[] derSig, out Signature result, out Errors error)
        {
            result = null;

            if (derSig == null)
            {
                error = Errors.NullBytes;
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
                error = Errors.InvalidDerEncodingLength;
                return false;
            }

            if (derSig[0] != SequenceTag)
            {
                error = Errors.MissingDerSeqTag;
                return false;
            }

            if (derSig[1] != derSig.Length - 3)
            {
                error = Errors.InvalidDerSeqLength;
                return false;
            }

            if (derSig[2] != IntegerTag)
            {
                error = Errors.MissingDerIntTag1;
                return false;
            }

            int rLen = derSig[3];
            if (rLen == 0 || (rLen == 1 && derSig[4] == 0) || rLen > 33)
            {
                error = Errors.InvalidDerRLength;
                return false;
            }

            // -8 is 4 bytes read so far + 1 SigHash + 3 byte at least for S
            if (derSig.Length - 8 < rLen)
            {
                error = Errors.InvalidDerIntLength1;
                return false;
            }

            if (derSig[rLen + 4] != IntegerTag)
            {
                error = Errors.MissingDerIntTag2;
                return false;
            }

            int sLen = derSig[rLen + 5];
            if (sLen == 0 || (sLen == 1 && derSig[rLen + 6] == 0) || sLen > 33)
            {
                error = Errors.InvalidDerSLength;
                return false;
            }

            // +7 is 2 for seqTag+len, 2 for r_intTag+len, 2 for s_intTag+len, 1 for SigHash
            if (rLen + sLen + 7 != derSig.Length)
            {
                error = Errors.InvalidDerIntLength2;
                return false;
            }

            //TODO: both r and s should use ModUInt256 type
            byte[] rBa = new byte[rLen];
            Buffer.BlockCopy(derSig, 4, rBa, 0, rLen);
            if ((rBa[0] & 0x80) != 0 || (rLen > 1 && (rBa[0] == 0) && (rBa[1] & 0x80) == 0))
            {
                error = Errors.InvalidDerRFormat;
                return false;
            }

            byte[] sBa = new byte[sLen];
            Buffer.BlockCopy(derSig, rLen + 6, sBa, 0, sLen);
            if ((sBa[0] & 0x80) != 0 || (sLen > 1 && (sBa[0] == 0) && (sBa[1] & 0x80) == 0))
            {
                error = Errors.InvalidDerSFormat;
                return false;
            }

            result = new Signature()
            {
                R = new BigInteger(rBa, false, true),
                S = new BigInteger(sBa, false, true),
                SigHash = (SigHashType)derSig[^1]
            };

            error = Errors.None;
            return true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Signature"/> by reading it from a fixed length byte array encoding
        /// used by Schnorr signatures. Return value indicates success.
        /// </summary>
        /// <param name="data">Signature bytes containing the signature</param>
        /// <param name="result">Resulting signature (null in case of failure)</param>
        /// <param name="error">Error message</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryReadSchnorr(ReadOnlySpan<byte> data, out Signature result, out Errors error)
        {
            if (data == null || data.Length == 0)
            {
                result = null;
                error = Errors.NullOrEmptyBytes;
                return false;
            }

            SigHashType sigHash;
            if (data.Length == 64)
            {
                sigHash = SigHashType.Default;
            }
            else if (data.Length == 65)
            {
                sigHash = (SigHashType)data[^1];
                if (sigHash == SigHashType.Default)
                {
                    result = null;
                    error = Errors.SigHashTypeZero;
                    return false;
                }
                else if (!((int)sigHash <= 0x03 || ((int)sigHash >= 0x81 && (int)sigHash <= 0x83)))
                {
                    result = null;
                    error = Errors.InvalidSigHashType;
                    return false;
                }
            }
            else
            {
                result = null;
                error = Errors.InvalidSchnorrSigLength;
                return false;
            }

            result = new Signature()
            {
                R = new BigInteger(data.Slice(0, 32), true, true),
                S = new BigInteger(data.Slice(32, 32), true, true),
                SigHash = sigHash
            };

            error = Errors.None;
            return true;
        }

        /// <summary>
        /// Creates a new instance of <see cref="Signature"/> by reading it from a fixed length byte array encoding
        /// used by signatures with a recovery ID as their first byte. Return value indicates success.
        /// </summary>
        /// <param name="data">Signature bytes containing the signature</param>
        /// <param name="result">Resulting signature (null in case of failure)</param>
        /// <param name="error">Error message (null if sucessful, otherwise contains information about the failure)</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryReadWithRecId(byte[] data, out Signature result, out string error)
        {
            if (data == null || data.Length == 0)
            {
                result = null;
                error = "Byte array can not be null or empty.";
                return false;
            }

            if (data.Length != 65)
            {
                result = null;
                error = "Signatures with recovery ID must be fixed 65 bytes.";
                return false;
            }

            result = new Signature()
            {
                R = new BigInteger(((ReadOnlySpan<byte>)data).Slice(1, 32), true, true),
                S = new BigInteger(((ReadOnlySpan<byte>)data).Slice(33, 32), true, true),
                RecoveryId = data[0]
            };

            error = null;
            return true;
        }


        /// <summary>
        /// Converts this instance to its byte array representation using DER encoding with the specified
        /// <see cref="SigHashType"/> added to the end, and returns the result.
        /// </summary>
        /// <returns>A DER encoded signature bytes with <see cref="SigHashType"/></returns>
        public byte[] ToByteArray()
        {
            var stream = new FastStream(72);
            WriteToStream(stream);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Converts this instance to its byte array representation using DER encoding with the specified
        /// <see cref="SigHashType"/> added to the end and writes the result to the given <see cref="FastStream"/>.
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
        /// added to the end using a fixed length encoding used in Schnorr signatures and returns the result.
        /// </summary>
        /// <returns>A DER encoded signature bytes with <see cref="SigHashType"/></returns>
        public byte[] ToByteArraySchnorr()
        {
            var stream = new FastStream(65);
            WriteToStreamSchnorr(stream);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Converts this instance to its byte array representation with the specified <see cref="SigHashType"/>
        /// added to the end using a fixed length encoding used in Schnorr signatures and writes the result to the given 
        /// <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void WriteToStreamSchnorr(FastStream stream)
        {
            byte[] rBa = R.ToByteArray(isUnsigned: true, isBigEndian: true);
            byte[] sBa = S.ToByteArray(isUnsigned: true, isBigEndian: true);

            stream.Write(32, rBa);
            stream.Write(32, sBa);
            if (SigHash != SigHashType.Default)
            {
                stream.Write((byte)SigHash);
            }
        }


        /// <summary>
        /// Converts this instance to a fixed length (65) byte array with a starting recovery ID (without changing the 
        /// already set value). This is useful when reporting message signatures and the result is usually encoded 
        /// using Base-64 encoding.
        /// </summary>
        /// <returns>A fixed length encoded signature with a recovery ID</returns>
        public byte[] ToByteArrayWithRecId()
        {
            var stream = new FastStream(65);
            WriteToStreamWithRecId(stream);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Converts this instance to a fixed length (65) byte array with a starting recovery ID (changes the current value
        /// based on <paramref name="isCompressed"/> parameter). This is useful when reporting message signatures and 
        /// the result is usually encoded using Base-64 encoding.
        /// </summary>
        /// <param name="isCompressed">Indicates whether compressed public key was used for creation of the address</param>
        /// <returns>A fixed length encoded signature with a recovery ID</returns>
        public byte[] ToByteArrayWithRecId(bool isCompressed)
        {
            var stream = new FastStream(65);
            WriteToStreamWithRecId(stream, isCompressed);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Converts this instance to a fixed length (65) byte array with a starting recovery ID (without changing the set
        /// value) and writes the result to the given <see cref="FastStream"/>. This is useful when reporting message 
        /// signatures and the result is usually encoded using Base-64 encoding.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void WriteToStreamWithRecId(FastStream stream)
        {
            stream.Write(RecoveryId);

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

        /// <summary>
        /// Converts this instance to a fixed length (65) byte array with a starting recovery ID (changes the current value
        /// based on <paramref name="isCompressed"/> parameter) and writes the result to the given <see cref="FastStream"/>.
        /// This is useful when reporting message signatures and the result is usually encoded using Base-64 encoding.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="isCompressed">Indicates whether compressed public key was used for creation of the address</param>
        public void WriteToStreamWithRecId(FastStream stream, bool isCompressed)
        {
            RecoveryId = (byte)(27 + RecoveryId + (isCompressed ? 4 : 0));
            WriteToStreamWithRecId(stream);
        }
    }
}
