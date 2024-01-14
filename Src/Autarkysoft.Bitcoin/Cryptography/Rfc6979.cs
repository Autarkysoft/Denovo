// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Cryptography
{
    /// <summary>
    /// Implementation of deterministic generation of the ephemeral key (k) for usage in 
    /// Elliptic Curve Digital Signature Algorithm (ECDSA) for bitcoin.
    /// Implements <see cref="IDisposable"/>.
    /// <para/>This is made to only use bitcoin curve (namely curve ornder N and its length) and HMAC-SHA256
    /// <para/>https://tools.ietf.org/html/rfc6979
    /// </summary>
    public sealed class Rfc6979 : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Rfc6979"/> with default parameters.
        /// </summary>
        public Rfc6979()
        {
            HmacK = new HmacSha256();
        }



        private const int QLen = 256;
        private HmacSha256 HmacK;



        private static BigInteger BitsToInt(byte[] ba)
        {
            BigInteger big = ba.ToBigInt(true, true);
            int vLen = ba.Length * 8;
            if (vLen > QLen)
            {
                return big >> (vLen - QLen);
            }
            return big;
        }



        /// <summary>
        /// Generates a deterministic random value k (used for signatures) based on data that is being
        /// signed and private key value.
        /// </summary>
        /// <remarks>
        /// Source: https://tools.ietf.org/html/rfc6979#section-3.2
        /// </remarks>
        /// <param name="data">Hashed data (it needs to be hashed by the caller using the appropriate hash function).</param>
        /// <param name="keyBytes">Private key bytes to use for signing (must be fixed 32 bytes, pad if needed).</param>
        /// <param name="extraEntropy">An extra entropy (can be null)</param>
        /// <returns>A deterministic random K.</returns>
        public BigInteger GetK(byte[] data, byte[] keyBytes, byte[] extraEntropy)
        {
            // Step a (compute hash of message) is performed by the caller
            // b.
            byte[] v = new byte[32];
            ((Span<byte>)v).Fill(1);

            // c.
            byte[] k = new byte[32];

            // d. 
            // K = HMAC_K(V || 0x00 || int2octets(x) || bits2octets(h1))
            int entLen = extraEntropy is null ? 0 : extraEntropy.Length;
            // 97 = 32 + 1 + 32 + 32
            byte[] bytesToHash = new byte[97 + entLen];
            Scalar8x32 sc = new Scalar8x32(data, out _);
            byte[] dataBa = sc.ToByteArray();

            Buffer.BlockCopy(v, 0, bytesToHash, 0, 32);
            // Set item at index 32 to 0x00
            Buffer.BlockCopy(keyBytes, 0, bytesToHash, 33, 32);
            Buffer.BlockCopy(dataBa, 0, bytesToHash, 97 - dataBa.Length, dataBa.Length);
            if (!(extraEntropy is null))
            {
                Buffer.BlockCopy(extraEntropy, 0, bytesToHash, 97, extraEntropy.Length);
            }

            k = HmacK.ComputeHash(bytesToHash, k);

            // e.
            v = HmacK.ComputeHash(v, k);

            // f. K = HMAC_K(V || 0x01 || int2octets(x) || bits2octets(h1))
            Buffer.BlockCopy(v, 0, bytesToHash, 0, 32);
            // Set item at index 33 to 0x01 this time
            bytesToHash[32] = 0x01;
            k = HmacK.ComputeHash(bytesToHash, k);

            // g. 
            v = HmacK.ComputeHash(v, k);

            while (true)
            {
                // h.1. & h.2.
                // Since hashLen is always equal to qLen there is no need for 2 steps
                // v is 32 bytes and T=byte[0] | V is 32 bytes too
                v = HmacK.ComputeHash(v, k);

                // h.3.
                Scalar8x32 temp = new Scalar8x32(v, out bool of);
                if (!temp.IsZero && !of)
                {
                    return new BigInteger(v, isUnsigned: true, isBigEndian: true);
                }
                else
                {
                    k = HmacK.ComputeHash(v.AppendToEnd(0), k);
                    v = HmacK.ComputeHash(v, k);
                }
            }
        }


        private bool isDisposed = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!isDisposed)
            {
                if (!(HmacK is null))
                    HmacK.Dispose();
                HmacK = null;

                isDisposed = true;
            }
        }
    }
}
