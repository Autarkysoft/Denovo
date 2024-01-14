// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography
{
    /// <summary>
    /// Implementation of deterministic generation of the ephemeral key (k) for usage in 
    /// Elliptic Curve Digital Signature Algorithm (ECDSA) for bitcoin.
    /// <para/>This is made to only use bitcoin curve (namely curve ornder N and its length) and HMAC-SHA256
    /// <para/>https://tools.ietf.org/html/rfc6979
    /// <para/>Implements <see cref="IDisposable"/>.
    /// </summary>
    /// <remarks>
    /// Use by calling <see cref="Init(ReadOnlySpan{byte}, ReadOnlySpan{byte})"/> first to initialize the instance
    /// then use <see cref="Generate"/> as many times to get different nonce values each time.
    /// </remarks>
    public sealed class Rfc6979 : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Rfc6979"/> with default parameters.
        /// </summary>
        public Rfc6979()
        {
        }



        private uint[] kv = new uint[16];
        private bool retry = false;



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InitInner(uint* hPt)
        {
            hPt[0] = 0xf454deadU;
            hPt[1] = 0x9725214fU;
            hPt[2] = 0x90daf2a0U;
            hPt[3] = 0xdf1228eaU;
            hPt[4] = 0x64e5750fU;
            hPt[5] = 0xa3924181U;
            hPt[6] = 0x824a932bU;
            hPt[7] = 0xf8e04e32U;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void InitOuter(uint* hPt)
        {
            hPt[0] = 0xd385480fU;
            hPt[1] = 0x7abb6477U;
            hPt[2] = 0x37c9c538U;
            hPt[3] = 0x5dd82467U;
            hPt[4] = 0x8e043a72U;
            hPt[5] = 0x753434b0U;
            hPt[6] = 0xdeb82818U;
            hPt[7] = 0x361d45a6U;
        }


        /// <summary>
        /// Initialize RFC-6979 using the hash and key.
        /// </summary>
        /// <param name="hash">32-byte hash</param>
        /// <param name="key">32-byte key</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public unsafe void Init(ReadOnlySpan<byte> hash, ReadOnlySpan<byte> key)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(Rfc6979));
            if (hash == null)
                throw new ArgumentNullException(nameof(hash), "Hash can not be null.");
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key can not be null.");
            if (hash.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(hash), "Hash length must be 32.");
            if (key.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(key), "Key length must be 32.");

            InitChecked(hash, key);
        }

        internal unsafe void InitChecked(ReadOnlySpan<byte> hash, ReadOnlySpan<byte> key)
        {
            retry = false;

            // Step a. Compute hash of message) is performed by the caller
            // Step b. Set V = 0x01...01 (32 bytes)
            // Step c. Set K = 0x00...00 (32 bytes)
            // Step d. K = HMAC_K(V || 0x00 || int2octets(x) || bits2octets(h1))

            Debug.Assert(hash != null && hash.Length == 32);
            Debug.Assert(key != null && key.Length == 32);

            Scalar8x32 sc = new Scalar8x32(hash, out _);
            hash = sc.ToByteArray();

            Debug.Assert(hash.Length == 32);

            using Sha256 sha = new Sha256();
            // TODO: after making Sha256 static use: uint* pt = stackalloc uint[8 + 64];

            uint* tPt = stackalloc uint[17];
            fixed (byte* kPt = &key[0], dPt = &hash[0])
            {
                // Store the working vector for (Key | Hash) to reuse later
                tPt[0] = (uint)((kPt[0] << 16) | (kPt[1] << 8) | kPt[2]);
                tPt[1] = (uint)((kPt[3] << 24) | (kPt[4] << 16) | (kPt[5] << 8) | kPt[6]);
                tPt[2] = (uint)((kPt[7] << 24) | (kPt[8] << 16) | (kPt[9] << 8) | kPt[10]);
                tPt[3] = (uint)((kPt[11] << 24) | (kPt[12] << 16) | (kPt[13] << 8) | kPt[14]);
                tPt[4] = (uint)((kPt[15] << 24) | (kPt[16] << 16) | (kPt[17] << 8) | kPt[18]);
                tPt[5] = (uint)((kPt[19] << 24) | (kPt[20] << 16) | (kPt[21] << 8) | kPt[22]);
                tPt[6] = (uint)((kPt[23] << 24) | (kPt[24] << 16) | (kPt[25] << 8) | kPt[26]);
                tPt[7] = (uint)((kPt[27] << 24) | (kPt[28] << 16) | (kPt[29] << 8) | kPt[30]);
                tPt[8] = (uint)((kPt[31] << 24) | (dPt[0] << 16) | (dPt[1] << 8) | dPt[2]);
                tPt[9] = (uint)((dPt[3] << 24) | (dPt[4] << 16) | (dPt[5] << 8) | dPt[6]);
                tPt[10] = (uint)((dPt[7] << 24) | (dPt[8] << 16) | (dPt[9] << 8) | dPt[10]);
                tPt[11] = (uint)((dPt[11] << 24) | (dPt[12] << 16) | (dPt[13] << 8) | dPt[14]);
                tPt[12] = (uint)((dPt[15] << 24) | (dPt[16] << 16) | (dPt[17] << 8) | dPt[18]);
                tPt[13] = (uint)((dPt[19] << 24) | (dPt[20] << 16) | (dPt[21] << 8) | dPt[22]);
                tPt[14] = (uint)((dPt[23] << 24) | (dPt[24] << 16) | (dPt[25] << 8) | dPt[26]);
                tPt[15] = (uint)((dPt[27] << 24) | (dPt[28] << 16) | (dPt[29] << 8) | dPt[30]);
                tPt[16] = (uint)((dPt[31] << 24) | 0x00800000U);
            }

            uint* oPt = stackalloc uint[8];
            uint* ktPt = stackalloc uint[8];
            fixed (uint* hPt = &sha.hashState[0], wPt = &sha.w[0])
            {
                // To compute K = HMAC_K(V | 0 | Key | Hash)
                // 1. Compute SHA256(inner_pad | data) where data is (V | 0 | Key | Hash)

                InitInner(hPt); // Precomputed hash-state when key is 0
                wPt[0] = 0x01010101U;
                wPt[1] = 0x01010101U;
                wPt[2] = 0x01010101U;
                wPt[3] = 0x01010101U;
                wPt[4] = 0x01010101U;
                wPt[5] = 0x01010101U;
                wPt[6] = 0x01010101U;
                wPt[7] = 0x01010101U;
                *(Block32*)(wPt + 8) = *(Block32*)tPt;
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)(tPt + 8);
                wPt[8] = tPt[16];
                wPt[9] = 0x00000000U;
                wPt[10] = 0x00000000U;
                wPt[11] = 0x00000000U;
                wPt[12] = 0x00000000U;
                wPt[13] = 0x00000000U;
                wPt[14] = 0x00000000U;
                wPt[15] = 0x00000508U; // total len = 161 (64 + 97)
                sha.CompressBlock(hPt, wPt);

                // 2. Compute SHA256(outer_pad | hash)
                // Copy 32 byte2 and fill unto index 7 in wPt
                *(Block32*)wPt = *(Block32*)hPt;
                wPt[8] = 0b10000000_00000000_00000000_00000000U; // 1 followed by 0 bits to fill pad1
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                // The total data length is: oPad.Length(=64) + hashState.Lengh(=32) = 96 byte *8 = 768 bit
                wPt[14] = 0;
                wPt[15] = 768;

                InitOuter(hPt); // Precomputed hash-state when key is 0
                sha.CompressBlock(hPt, wPt);
                // Now hPt holds K from step (d)

                // * Step e. V = HMAC_K(V)
                *(Block32*)ktPt = *(Block32*)hPt; // Store to use in step f
                oPt[0] = hPt[0] ^ 0x5c5c5c5cU;
                oPt[1] = hPt[1] ^ 0x5c5c5c5cU;
                oPt[2] = hPt[2] ^ 0x5c5c5c5cU;
                oPt[3] = hPt[3] ^ 0x5c5c5c5cU;
                oPt[4] = hPt[4] ^ 0x5c5c5c5cU;
                oPt[5] = hPt[5] ^ 0x5c5c5c5cU;
                oPt[6] = hPt[6] ^ 0x5c5c5c5cU;
                oPt[7] = hPt[7] ^ 0x5c5c5c5cU;

                wPt[0] = hPt[0] ^ 0x36363636U;
                wPt[1] = hPt[1] ^ 0x36363636U;
                wPt[2] = hPt[2] ^ 0x36363636U;
                wPt[3] = hPt[3] ^ 0x36363636U;
                wPt[4] = hPt[4] ^ 0x36363636U;
                wPt[5] = hPt[5] ^ 0x36363636U;
                wPt[6] = hPt[6] ^ 0x36363636U;
                wPt[7] = hPt[7] ^ 0x36363636U;
                wPt[8] = 0x36363636U;
                wPt[9] = 0x36363636U;
                wPt[10] = 0x36363636U;
                wPt[11] = 0x36363636U;
                wPt[12] = 0x36363636U;
                wPt[13] = 0x36363636U;
                wPt[14] = 0x36363636U;
                wPt[15] = 0x36363636U;

                // 1. Compute SHA256(inner_pad | data)
                sha.Init(hPt);
                sha.CompressBlock(hPt, wPt);

                wPt[0] = 0x01010101;
                wPt[1] = 0x01010101;
                wPt[2] = 0x01010101;
                wPt[3] = 0x01010101;
                wPt[4] = 0x01010101;
                wPt[5] = 0x01010101;
                wPt[6] = 0x01010101;
                wPt[7] = 0x01010101;
                wPt[8] = 0x80000000;
                wPt[9] = 0x00000000;
                wPt[10] = 0x00000000;
                wPt[11] = 0x00000000;
                wPt[12] = 0x00000000;
                wPt[13] = 0x00000000;
                wPt[14] = 0x00000000;
                wPt[15] = 0x00000300; // Total len = 96 (64 + 32)
                sha.CompressBlock(hPt, wPt);

                // 2. Compute SHA256(outer_pad | hash)
                *(Block32*)wPt = *(Block32*)oPt;
                *(Block32*)oPt = *(Block32*)hPt;

                wPt[8] = 0x5c5c5c5cU;
                wPt[9] = 0x5c5c5c5cU;
                wPt[10] = 0x5c5c5c5cU;
                wPt[11] = 0x5c5c5c5cU;
                wPt[12] = 0x5c5c5c5cU;
                wPt[13] = 0x5c5c5c5cU;
                wPt[14] = 0x5c5c5c5cU;
                wPt[15] = 0x5c5c5c5cU;

                sha.Init(hPt);
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)oPt;
                wPt[8] = 0b10000000_00000000_00000000_00000000U;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                // The total data length is: oPad.Length(=64) + hashState.Lengh(=32) = 96 byte *8 = 768 bit
                wPt[14] = 0;
                wPt[15] = 768;
                sha.CompressBlock(hPt, wPt);
                // Now hPt holds new V


                // * Step f. K = HMAC_K(V || 0x01 || int2octets(x) || bits2octets(h1))
                oPt[0] = ktPt[0] ^ 0x5c5c5c5cU;
                oPt[1] = ktPt[1] ^ 0x5c5c5c5cU;
                oPt[2] = ktPt[2] ^ 0x5c5c5c5cU;
                oPt[3] = ktPt[3] ^ 0x5c5c5c5cU;
                oPt[4] = ktPt[4] ^ 0x5c5c5c5cU;
                oPt[5] = ktPt[5] ^ 0x5c5c5c5cU;
                oPt[6] = ktPt[6] ^ 0x5c5c5c5cU;
                oPt[7] = ktPt[7] ^ 0x5c5c5c5cU;

                wPt[0] = ktPt[0] ^ 0x36363636U;
                wPt[1] = ktPt[1] ^ 0x36363636U;
                wPt[2] = ktPt[2] ^ 0x36363636U;
                wPt[3] = ktPt[3] ^ 0x36363636U;
                wPt[4] = ktPt[4] ^ 0x36363636U;
                wPt[5] = ktPt[5] ^ 0x36363636U;
                wPt[6] = ktPt[6] ^ 0x36363636U;
                wPt[7] = ktPt[7] ^ 0x36363636U;
                wPt[8] = 0x36363636U;
                wPt[9] = 0x36363636U;
                wPt[10] = 0x36363636U;
                wPt[11] = 0x36363636U;
                wPt[12] = 0x36363636U;
                wPt[13] = 0x36363636U;
                wPt[14] = 0x36363636U;
                wPt[15] = 0x36363636U;

                // Now ktPt holds new V
                *(Block32*)ktPt = *(Block32*)hPt;

                // 1. Compute SHA256(inner_pad | data) where data is (V' | 1 | Key | Hash)
                sha.Init(hPt);
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)ktPt;
                *(Block32*)(wPt + 8) = *(Block32*)tPt;
                wPt[8] |= 0b00000001_00000000_00000000_00000000U;
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)(tPt + 8);
                wPt[8] = tPt[16];
                wPt[9] = 0x00000000U;
                wPt[10] = 0x00000000U;
                wPt[11] = 0x00000000U;
                wPt[12] = 0x00000000U;
                wPt[13] = 0x00000000U;
                wPt[14] = 0x00000000U;
                wPt[15] = 0x00000508U; // total len = 161 (64 + 97)
                sha.CompressBlock(hPt, wPt);

                // 2. Compute SHA256(outer_pad | hash)
                *(Block32*)wPt = *(Block32*)oPt;
                *(Block32*)oPt = *(Block32*)hPt;

                wPt[8] = 0x5c5c5c5cU;
                wPt[9] = 0x5c5c5c5cU;
                wPt[10] = 0x5c5c5c5cU;
                wPt[11] = 0x5c5c5c5cU;
                wPt[12] = 0x5c5c5c5cU;
                wPt[13] = 0x5c5c5c5cU;
                wPt[14] = 0x5c5c5c5cU;
                wPt[15] = 0x5c5c5c5cU;

                sha.Init(hPt);
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)oPt;
                wPt[8] = 0b10000000_00000000_00000000_00000000U;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                // The total data length is: oPad.Length(=64) + hashState.Lengh(=32) = 96 byte *8 = 768 bit
                wPt[14] = 0;
                wPt[15] = 768;
                sha.CompressBlock(hPt, wPt);
                // Now hPt holds the new K


                // * Step g. V = HMAC_K(V)
                *(Block32*)tPt = *(Block32*)hPt; // Store K
                oPt[0] = hPt[0] ^ 0x5c5c5c5cU;
                oPt[1] = hPt[1] ^ 0x5c5c5c5cU;
                oPt[2] = hPt[2] ^ 0x5c5c5c5cU;
                oPt[3] = hPt[3] ^ 0x5c5c5c5cU;
                oPt[4] = hPt[4] ^ 0x5c5c5c5cU;
                oPt[5] = hPt[5] ^ 0x5c5c5c5cU;
                oPt[6] = hPt[6] ^ 0x5c5c5c5cU;
                oPt[7] = hPt[7] ^ 0x5c5c5c5cU;

                wPt[0] = hPt[0] ^ 0x36363636U;
                wPt[1] = hPt[1] ^ 0x36363636U;
                wPt[2] = hPt[2] ^ 0x36363636U;
                wPt[3] = hPt[3] ^ 0x36363636U;
                wPt[4] = hPt[4] ^ 0x36363636U;
                wPt[5] = hPt[5] ^ 0x36363636U;
                wPt[6] = hPt[6] ^ 0x36363636U;
                wPt[7] = hPt[7] ^ 0x36363636U;
                wPt[8] = 0x36363636U;
                wPt[9] = 0x36363636U;
                wPt[10] = 0x36363636U;
                wPt[11] = 0x36363636U;
                wPt[12] = 0x36363636U;
                wPt[13] = 0x36363636U;
                wPt[14] = 0x36363636U;
                wPt[15] = 0x36363636U;

                // 1. Compute SHA256(inner_pad | data)
                sha.Init(hPt);
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)ktPt;
                wPt[8] = 0x80000000;
                wPt[9] = 0x00000000;
                wPt[10] = 0x00000000;
                wPt[11] = 0x00000000;
                wPt[12] = 0x00000000;
                wPt[13] = 0x00000000;
                wPt[14] = 0x00000000;
                wPt[15] = 0x00000300; // Total len = 96 (64 + 32)
                sha.CompressBlock(hPt, wPt);

                // 2. Compute SHA256(outer_pad | hash)
                *(Block32*)wPt = *(Block32*)oPt;
                *(Block32*)oPt = *(Block32*)hPt;

                wPt[8] = 0x5c5c5c5cU;
                wPt[9] = 0x5c5c5c5cU;
                wPt[10] = 0x5c5c5c5cU;
                wPt[11] = 0x5c5c5c5cU;
                wPt[12] = 0x5c5c5c5cU;
                wPt[13] = 0x5c5c5c5cU;
                wPt[14] = 0x5c5c5c5cU;
                wPt[15] = 0x5c5c5c5cU;

                sha.Init(hPt);
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)oPt;
                wPt[8] = 0b10000000_00000000_00000000_00000000U;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                // The total data length is: oPad.Length(=64) + hashState.Lengh(=32) = 96 byte *8 = 768 bit
                wPt[14] = 0;
                wPt[15] = 768;
                sha.CompressBlock(hPt, wPt);
                // Now hPt holds new V


                // Final result is v=hPt and k=tPt
                fixed (uint* x = &kv[0], y = &kv[8])
                {
                    *(Block32*)x = *(Block32*)tPt;
                    *(Block32*)y = *(Block32*)hPt;
                }
            }
        }


        /// <summary>
        /// Generate the next nonce value. Instance has to be initialized first by calling
        /// <see cref="Init(ReadOnlySpan{byte}, ReadOnlySpan{byte})"/>.
        /// </summary>
        /// <returns>Next nonce value</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public byte[] Generate()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(Rfc6979));
            if (kv.All(x => x == 0))
                throw new ArgumentException("Not initialized.");

            return GenerateChecked();
        }

        internal unsafe byte[] GenerateChecked()
        {
            // * Step h.
            //   V = HMAC_K(V)
            //   loop
            //     if (v in [1,N))
            //       return v
            //     else
            //       K = HMAC_K(V || 0x00)
            //       V = HMAC_K(V)

            using Sha256 sha = new Sha256();

            uint* tPt = stackalloc uint[8];
            fixed (uint* kPt = &kv[0], vPt = &kv[8])
            fixed (uint* hPt = &sha.hashState[0], wPt = &sha.w[0])
            {
                if (retry)
                {
                    // K = HMAC_K(V || 0x00)
                    wPt[0] = kPt[0] ^ 0x36363636U;
                    wPt[1] = kPt[1] ^ 0x36363636U;
                    wPt[2] = kPt[2] ^ 0x36363636U;
                    wPt[3] = kPt[3] ^ 0x36363636U;
                    wPt[4] = kPt[4] ^ 0x36363636U;
                    wPt[5] = kPt[5] ^ 0x36363636U;
                    wPt[6] = kPt[6] ^ 0x36363636U;
                    wPt[7] = kPt[7] ^ 0x36363636U;
                    wPt[8] = 0x36363636U;
                    wPt[9] = 0x36363636U;
                    wPt[10] = 0x36363636U;
                    wPt[11] = 0x36363636U;
                    wPt[12] = 0x36363636U;
                    wPt[13] = 0x36363636U;
                    wPt[14] = 0x36363636U;
                    wPt[15] = 0x36363636U;

                    // 1. Compute SHA256(inner_pad | data)
                    sha.Init(hPt);
                    sha.CompressBlock(hPt, wPt);

                    *(Block32*)wPt = *(Block32*)vPt;
                    wPt[8] = 0x00800000;
                    wPt[9] = 0x00000000;
                    wPt[10] = 0x00000000;
                    wPt[11] = 0x00000000;
                    wPt[12] = 0x00000000;
                    wPt[13] = 0x00000000;
                    wPt[14] = 0x00000000;
                    wPt[15] = 0x00000308; // Total len = 97 (64 + 1 + 32)
                    sha.CompressBlock(hPt, wPt);

                    *(Block32*)tPt = *(Block32*)hPt; // Store for future use

                    // 2. Compute SHA256(outer_pad | hash)
                    wPt[0] = kPt[0] ^ 0x5c5c5c5cU;
                    wPt[1] = kPt[1] ^ 0x5c5c5c5cU;
                    wPt[2] = kPt[2] ^ 0x5c5c5c5cU;
                    wPt[3] = kPt[3] ^ 0x5c5c5c5cU;
                    wPt[4] = kPt[4] ^ 0x5c5c5c5cU;
                    wPt[5] = kPt[5] ^ 0x5c5c5c5cU;
                    wPt[6] = kPt[6] ^ 0x5c5c5c5cU;
                    wPt[7] = kPt[7] ^ 0x5c5c5c5cU;
                    wPt[8] = 0x5c5c5c5cU;
                    wPt[9] = 0x5c5c5c5cU;
                    wPt[10] = 0x5c5c5c5cU;
                    wPt[11] = 0x5c5c5c5cU;
                    wPt[12] = 0x5c5c5c5cU;
                    wPt[13] = 0x5c5c5c5cU;
                    wPt[14] = 0x5c5c5c5cU;
                    wPt[15] = 0x5c5c5c5cU;

                    sha.Init(hPt);
                    sha.CompressBlock(hPt, wPt);

                    *(Block32*)wPt = *(Block32*)tPt;
                    wPt[8] = 0b10000000_00000000_00000000_00000000U;
                    wPt[9] = 0;
                    wPt[10] = 0;
                    wPt[11] = 0;
                    wPt[12] = 0;
                    wPt[13] = 0;
                    // The total data length is: oPad.Length(=64) + hashState.Lengh(=32) = 96 byte *8 = 768 bit
                    wPt[14] = 0;
                    wPt[15] = 768;
                    sha.CompressBlock(hPt, wPt);

                    // Now hPt stores new K
                    *(Block32*)kPt = *(Block32*)hPt;


                    // V = HMAC_K(V)
                    wPt[0] = kPt[0] ^ 0x36363636U;
                    wPt[1] = kPt[1] ^ 0x36363636U;
                    wPt[2] = kPt[2] ^ 0x36363636U;
                    wPt[3] = kPt[3] ^ 0x36363636U;
                    wPt[4] = kPt[4] ^ 0x36363636U;
                    wPt[5] = kPt[5] ^ 0x36363636U;
                    wPt[6] = kPt[6] ^ 0x36363636U;
                    wPt[7] = kPt[7] ^ 0x36363636U;
                    wPt[8] = 0x36363636U;
                    wPt[9] = 0x36363636U;
                    wPt[10] = 0x36363636U;
                    wPt[11] = 0x36363636U;
                    wPt[12] = 0x36363636U;
                    wPt[13] = 0x36363636U;
                    wPt[14] = 0x36363636U;
                    wPt[15] = 0x36363636U;

                    // 1. Compute SHA256(inner_pad | data)
                    sha.Init(hPt);
                    sha.CompressBlock(hPt, wPt);

                    *(Block32*)wPt = *(Block32*)vPt;
                    wPt[8] = 0x80000000;
                    wPt[9] = 0x00000000;
                    wPt[10] = 0x00000000;
                    wPt[11] = 0x00000000;
                    wPt[12] = 0x00000000;
                    wPt[13] = 0x00000000;
                    wPt[14] = 0x00000000;
                    wPt[15] = 0x00000300; // Total len = 96 (64 + 32)
                    sha.CompressBlock(hPt, wPt);

                    *(Block32*)tPt = *(Block32*)hPt; // Store for future use

                    // 2. Compute SHA256(outer_pad | hash)
                    wPt[0] = kPt[0] ^ 0x5c5c5c5cU;
                    wPt[1] = kPt[1] ^ 0x5c5c5c5cU;
                    wPt[2] = kPt[2] ^ 0x5c5c5c5cU;
                    wPt[3] = kPt[3] ^ 0x5c5c5c5cU;
                    wPt[4] = kPt[4] ^ 0x5c5c5c5cU;
                    wPt[5] = kPt[5] ^ 0x5c5c5c5cU;
                    wPt[6] = kPt[6] ^ 0x5c5c5c5cU;
                    wPt[7] = kPt[7] ^ 0x5c5c5c5cU;
                    wPt[8] = 0x5c5c5c5cU;
                    wPt[9] = 0x5c5c5c5cU;
                    wPt[10] = 0x5c5c5c5cU;
                    wPt[11] = 0x5c5c5c5cU;
                    wPt[12] = 0x5c5c5c5cU;
                    wPt[13] = 0x5c5c5c5cU;
                    wPt[14] = 0x5c5c5c5cU;
                    wPt[15] = 0x5c5c5c5cU;

                    sha.Init(hPt);
                    sha.CompressBlock(hPt, wPt);

                    *(Block32*)wPt = *(Block32*)tPt;
                    wPt[8] = 0b10000000_00000000_00000000_00000000U;
                    wPt[9] = 0;
                    wPt[10] = 0;
                    wPt[11] = 0;
                    wPt[12] = 0;
                    wPt[13] = 0;
                    // The total data length is: oPad.Length(=64) + hashState.Lengh(=32) = 96 byte *8 = 768 bit
                    wPt[14] = 0;
                    wPt[15] = 768;
                    sha.CompressBlock(hPt, wPt);

                    // Now hPt stores new V
                    *(Block32*)vPt = *(Block32*)hPt;
                }

                // V = HMAC_K(V)
                wPt[0] = kPt[0] ^ 0x36363636U;
                wPt[1] = kPt[1] ^ 0x36363636U;
                wPt[2] = kPt[2] ^ 0x36363636U;
                wPt[3] = kPt[3] ^ 0x36363636U;
                wPt[4] = kPt[4] ^ 0x36363636U;
                wPt[5] = kPt[5] ^ 0x36363636U;
                wPt[6] = kPt[6] ^ 0x36363636U;
                wPt[7] = kPt[7] ^ 0x36363636U;
                wPt[8] = 0x36363636U;
                wPt[9] = 0x36363636U;
                wPt[10] = 0x36363636U;
                wPt[11] = 0x36363636U;
                wPt[12] = 0x36363636U;
                wPt[13] = 0x36363636U;
                wPt[14] = 0x36363636U;
                wPt[15] = 0x36363636U;

                // 1. Compute SHA256(inner_pad | data)
                sha.Init(hPt);
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)vPt;
                wPt[8] = 0x80000000;
                wPt[9] = 0x00000000;
                wPt[10] = 0x00000000;
                wPt[11] = 0x00000000;
                wPt[12] = 0x00000000;
                wPt[13] = 0x00000000;
                wPt[14] = 0x00000000;
                wPt[15] = 0x00000300; // Total len = 96 (64 + 32)
                sha.CompressBlock(hPt, wPt);

                *(Block32*)tPt = *(Block32*)hPt; // Store for future use

                // 2. Compute SHA256(outer_pad | hash)
                wPt[0] = kPt[0] ^ 0x5c5c5c5cU;
                wPt[1] = kPt[1] ^ 0x5c5c5c5cU;
                wPt[2] = kPt[2] ^ 0x5c5c5c5cU;
                wPt[3] = kPt[3] ^ 0x5c5c5c5cU;
                wPt[4] = kPt[4] ^ 0x5c5c5c5cU;
                wPt[5] = kPt[5] ^ 0x5c5c5c5cU;
                wPt[6] = kPt[6] ^ 0x5c5c5c5cU;
                wPt[7] = kPt[7] ^ 0x5c5c5c5cU;
                wPt[8] = 0x5c5c5c5cU;
                wPt[9] = 0x5c5c5c5cU;
                wPt[10] = 0x5c5c5c5cU;
                wPt[11] = 0x5c5c5c5cU;
                wPt[12] = 0x5c5c5c5cU;
                wPt[13] = 0x5c5c5c5cU;
                wPt[14] = 0x5c5c5c5cU;
                wPt[15] = 0x5c5c5c5cU;

                sha.Init(hPt);
                sha.CompressBlock(hPt, wPt);

                *(Block32*)wPt = *(Block32*)tPt;
                wPt[8] = 0b10000000_00000000_00000000_00000000U;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                // The total data length is: oPad.Length(=64) + hashState.Lengh(=32) = 96 byte *8 = 768 bit
                wPt[14] = 0;
                wPt[15] = 768;
                sha.CompressBlock(hPt, wPt);

                // Now hPt stores new V
                *(Block32*)vPt = *(Block32*)hPt;

                retry = true;
                return sha.GetBytes(vPt);
            }
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
        [Obsolete("Use the more optimized version through Init()+Generate() methods")]
        public BigInteger GetK(byte[] data, byte[] keyBytes, byte[] extraEntropy)
        {
            using HmacSha256 HmacK = new HmacSha256();

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
                if (kv != null)
                {
                    Array.Clear(kv, 0, kv.Length);
                }
                kv = null;

                isDisposed = true;
            }
        }
    }
}
