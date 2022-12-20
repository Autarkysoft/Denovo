// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// Signatures produced by Elliptic Curve Schnorr Digital Signature Algorithms (ECSDSA)
    /// storing R and S values and signature hash type.
    /// </summary>
    public class SchnorrSignature
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SchnorrSignature"/> using the given parameters.
        /// </summary>
        /// <param name="r">R value</param>
        /// <param name="s">S value</param>
        /// <param name="sigHash">Signature hash type</param>
        public SchnorrSignature(in UInt256_10x26 r, in Scalar8x32 s, SigHashType sigHash)
        {
            R = r;
            S = s;
            SigHash = sigHash;
        }


        /// <summary>
        /// The r value in a signature
        /// </summary>
        public UInt256_10x26 R { get; set; }
        /// <summary>
        /// The s value in a signature
        /// </summary>
        public Scalar8x32 S { get; set; }
        /// <summary>
        /// Signature hash type used during transaction signing
        /// </summary>
        public SigHashType SigHash { get; set; }


        /// <summary>
        /// Creates a new instance of <see cref="SchnorrSignature"/> by reading it from a fixed length byte array encoding
        /// used by Schnorr signatures. Return value indicates success.
        /// </summary>
        /// <param name="data">Signature bytes containing the signature (64 or 65 bytes)</param>
        /// <param name="result">Resulting signature (null in case of failure)</param>
        /// <param name="error">Error message</param>
        /// <returns>True if successful, otherwise false.</returns>
        public static bool TryRead(ReadOnlySpan<byte> data, out SchnorrSignature result, out Errors error)
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

            UInt256_10x26 r = new UInt256_10x26(data.Slice(0, 32), out bool valid);
            if (!valid)
            {
                result = null;
                error = Errors.InvalidDerRFormat;
                return false;
            }

            Scalar8x32 s = new Scalar8x32(data.Slice(32, 32), out valid);
            if (valid) // This is overflow not validity
            {
                result = null;
                error = Errors.InvalidDerSFormat;
                return false;
            }

            result = new SchnorrSignature(r, s, sigHash);
            error = Errors.None;
            return true;
        }

        /// <summary>
        /// Converts this instance to its byte array representation with the specified <see cref="SigHashType"/>
        /// added to the end if needed, using a fixed length encoding used in Schnorr signatures and returns the result.
        /// </summary>
        /// <returns>The encoded signature bytes with <see cref="SigHashType"/></returns>
        public byte[] ToByteArray()
        {
            var stream = new FastStream(65);
            WriteToStream(stream);
            return stream.ToByteArray();
        }

        /// <summary>
        /// Converts this instance to its byte array representation with the specified <see cref="SigHashType"/>
        /// added to the end if needed, using a fixed length encoding used in Schnorr signatures and writes the result to the given 
        /// <see cref="FastStream"/>.
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void WriteToStream(FastStream stream)
        {
            stream.Write(R);
            stream.Write(S);
            if (SigHash != SigHashType.Default)
            {
                stream.Write((byte)SigHash);
            }
        }
    }
}
