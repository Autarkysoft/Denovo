// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// Signatures produced by Elliptic Curve digital signature algorithms (ECDSA and ECSDSA) 
    /// storing R and S values with recovery ID and signature hash type.
    /// </summary>
    public class Signature
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Signature"/> using the given parameters.
        /// </summary>
        /// <param name="r">R value</param>
        /// <param name="s">S value</param>
        /// <param name="v">[Default value = 0] Recovery ID, an optional byte created during signing process</param>
        public Signature(in Scalar8x32 r, in Scalar8x32 s, byte v = 0)
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
        public Signature(in Scalar8x32 r, in Scalar8x32 s, SigHashType sigHash)
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
        public Scalar8x32 R { get; set; }
        /// <summary>
        /// The s value in a signature
        /// </summary>
        public Scalar8x32 S { get; set; }
        /// <summary>
        /// The one byte recovery ID used in fixed length message signatures
        /// </summary>
        public byte RecoveryId { get; set; }
        /// <summary>
        /// Signature hash type used during transaction signing
        /// </summary>
        public SigHashType SigHash { get; set; }



        private static bool TryCreateScalar(ReadOnlySpan<byte> ba, out Scalar8x32 res)
        {
            int start = 0;
            while (start < ba.Length)
            {
                if (ba[start] != 0)
                {
                    break;
                }
                start++;
            }

            int len = ba.Length - start;
            if (len > 32)
            {
                res = Scalar8x32.Zero;
                return false;
            }

            byte[] temp = new byte[32];
            Buffer.BlockCopy(ba.ToArray(), start, temp, 32 - len, len);

            res = new Scalar8x32(temp, out bool overflow);
            return !overflow;
        }

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
            // https://github.com/bitcoin/bitcoin/blob/e7a0e9627196655be5aa6c2738d4b57646a03726/src/pubkey.cpp#L25-L175
            result = null;

            if (derSig == null || derSig.Length == 0)
            {
                error = Errors.NullOrEmptyBytes;
                return false;
            }

            FastStreamReader stream = new FastStreamReader(derSig);

            if (!stream.TryReadByte(out byte seqTag) || seqTag != SequenceTag)
            {
                error = Errors.MissingDerSeqTag;
                return false;
            }

            // TODO: core is only reading 1 byte here (maybe run a check on all pre-BIP66 sigs and simplify this)
            // https://github.com/bitcoin/bitcoin/blob/e7a0e9627196655be5aa6c2738d4b57646a03726/src/pubkey.cpp#L55-L62
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

            if (!TryCreateScalar(rBa, out Scalar8x32 r))
            {
                error = Errors.InvalidDerRFormat;
                return false;
            }
            if (!TryCreateScalar(sBa, out Scalar8x32 s))
            {
                error = Errors.InvalidDerSFormat;
                return false;
            }

            result = new Signature(r, s, (SigHashType)derSig[^1]);
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
            // https://github.com/bitcoin/bitcoin/blob/master/src/script/interpreter.cpp#L107
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

            if (!TryCreateScalar(rBa, out Scalar8x32 r))
            {
                error = Errors.InvalidDerRFormat;
                return false;
            }
            if (!TryCreateScalar(sBa, out Scalar8x32 s))
            {
                error = Errors.InvalidDerSFormat;
                return false;
            }

            result = new Signature(r, s, (SigHashType)derSig[^1]);
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
            // Note that empty signature is accepted as valid in Tapscripts (return true for parsing, false for success),
            // but we handle it in CheckSigTapOp and are strict here.
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

            if (!TryCreateScalar(data.Slice(0, 32), out Scalar8x32 r))
            {
                result = null;
                error = Errors.InvalidDerRFormat;
                return false;
            }
            if (!TryCreateScalar(data.Slice(32, 32), out Scalar8x32 s))
            {
                result = null;
                error = Errors.InvalidDerSFormat;
                return false;
            }

            result = new Signature(r, s, sigHash);
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
        public static bool TryReadWithRecId(ReadOnlySpan<byte> data, out Signature result, out string error)
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

            result = new Signature(new Scalar8x32(data.Slice(1, 32), out _), new Scalar8x32(data.Slice(33, 32), out _), data[0]);

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
            Span<byte> rBa = new byte[33]; R.WriteToSpan(rBa.Slice(1));
            Span<byte> sBa = new byte[33]; R.WriteToSpan(sBa.Slice(1));
            int rLen = 33; int rStart = 0;
            int sLen = 33; int sStart = 0;
            while (rLen > 1 && rBa[0] == 0 && rBa[1] < 0x80) { rLen--; rStart++; }
            while (sLen > 1 && sBa[0] == 0 && sBa[1] < 0x80) { sLen--; sStart++; }

            stream.Write(SequenceTag);
            stream.Write((byte)(rLen + sLen + 4));
            stream.Write(IntegerTag);
            stream.Write((byte)rLen);
            stream.Write(rBa.Slice(rStart, rLen).ToArray());
            stream.Write(IntegerTag);
            stream.Write((byte)sLen);
            stream.Write(sBa.Slice(sStart, sLen).ToArray());
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
            stream.Write(R);
            stream.Write(S);
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
            stream.Write(R);
            stream.Write(S);
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
