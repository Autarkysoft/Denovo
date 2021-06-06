// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// Implementation of 256-bit Secure Hash Algorithm (SHA) base on RFC-6234.
    /// <para/>Implements <see cref="IDisposable"/>
    /// <para/>https://tools.ietf.org/html/rfc6234
    /// </summary>
    public sealed class Sha256 : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sha256"/>.
        /// </summary>
        public Sha256()
        {
        }



        /// <summary>
        /// Size of the hash result in bytes (=32 bytes).
        /// </summary>
        public const int HashByteSize = 32;

        /// <summary>
        /// Size of the blocks used in each round (=64 bytes).
        /// </summary>
        public int BlockByteSize => 64;


        internal uint[] hashState = new uint[8];
        internal uint[] w = new uint[64];

        private static readonly uint[] Ks =
        {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
            0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
            0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
            0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
            0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
            0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
            0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
            0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
            0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };



        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">The byte array to compute hash for</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHash(byte[] data)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");

            fixed (byte* dPt = data)
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                Init(hPt);
                CompressData(dPt, data.Length, data.Length, hPt, wPt);

                return GetBytes(hPt);
            }
        }

        /// <summary>
        /// Computes the hash value for the specified byte array twice (hash of hash).
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">The byte array to compute hash for</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHashTwice(byte[] data)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");

            fixed (byte* dPt = data)
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                Init(hPt);
                CompressData(dPt, data.Length, data.Length, hPt, wPt);
                ComputeSecondHash(hPt, wPt);

                return GetBytes(hPt);
            }
        }


        /// <summary>
        /// Computes double hash of the given data and returns the first 4 bytes of the result as checksum.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">Data to hash</param>
        /// <returns>First 4 bytes of the hash result</returns>
        public unsafe byte[] ComputeChecksum(byte[] data)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");

            fixed (byte* dPt = data)
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                Init(hPt);
                CompressData(dPt, data.Length, data.Length, hPt, wPt);
                ComputeSecondHash(hPt, wPt);

                return new byte[4] { (byte)(hPt[0] >> 24), (byte)(hPt[0] >> 16), (byte)(hPt[0] >> 8), (byte)hPt[0] };
            }
        }



        public unsafe byte[] ComputeShortIdKey(byte[] header, ulong nonce)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Instance was disposed.");
            if (header == null)
                throw new ArgumentNullException(nameof(header), "Header can not be null.");
            if (header.Length != 80)
                throw new ArgumentOutOfRangeException(nameof(header), "Header must be 80 bytes long.");

            // Compute SHA256 of 80+8 bytes => 2 blocks of 64+24
            fixed (byte* dPt = &header[0])
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                Init(hPt);

                // Set and compress first block (from 0 to 63 byte of header)
                int dIndex = 0;
                for (int i = 0; i < 16; i++, dIndex += 4)
                {
                    wPt[i] = (uint)((dPt[dIndex] << 24) | (dPt[dIndex + 1] << 16) | (dPt[dIndex + 2] << 8) | dPt[dIndex + 3]);
                }
                CompressBlock(hPt, wPt);

                // Set and compress second block (from 63 to 80 byte of header)
                wPt[0] = (uint)((dPt[64] << 24) | (dPt[65] << 16) | (dPt[66] << 8) | dPt[67]);
                wPt[1] = (uint)((dPt[68] << 24) | (dPt[69] << 16) | (dPt[70] << 8) | dPt[71]);
                wPt[2] = (uint)((dPt[72] << 24) | (dPt[73] << 16) | (dPt[74] << 8) | dPt[75]);
                wPt[3] = (uint)((dPt[76] << 24) | (dPt[77] << 16) | (dPt[78] << 8) | dPt[79]);
                // 8 byte of nonce
                wPt[4] = (uint)(nonce >> 32);
                wPt[5] = (uint)nonce;
                wPt[6] = 0b10000000_00000000_00000000_00000000U;
                wPt[7] = 0;
                wPt[8] = 0;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                wPt[14] = 0;
                wPt[15] = 704;

                wPt[16] = SSIG0(wPt[1]) + wPt[0];
                wPt[17] = 20447232 + SSIG0(wPt[2]) + wPt[1];
                wPt[18] = SSIG1(wPt[16]) + SSIG0(wPt[3]) + wPt[2];
                wPt[19] = SSIG1(wPt[17]) + SSIG0(wPt[4]) + wPt[3];
                wPt[20] = SSIG1(wPt[18]) + SSIG0(wPt[5]) + wPt[4];
                wPt[21] = SSIG1(wPt[19]) + 285220864 + wPt[5];
                wPt[22] = SSIG1(wPt[20]) + 2147484352;
                wPt[23] = SSIG1(wPt[21]) + wPt[16];
                wPt[24] = SSIG1(wPt[22]) + wPt[17];
                wPt[25] = SSIG1(wPt[23]) + wPt[18];
                wPt[26] = SSIG1(wPt[24]) + wPt[19];
                wPt[27] = SSIG1(wPt[25]) + wPt[20];
                wPt[28] = SSIG1(wPt[26]) + wPt[21];
                wPt[29] = SSIG1(wPt[27]) + wPt[22];
                wPt[30] = SSIG1(wPt[28]) + wPt[23] + 2159018077;
                wPt[31] = SSIG1(wPt[29]) + wPt[24] + SSIG0(wPt[16]) + 704;
                wPt[32] = SSIG1(wPt[30]) + wPt[25] + SSIG0(wPt[17]) + wPt[16];
                wPt[33] = SSIG1(wPt[31]) + wPt[26] + SSIG0(wPt[18]) + wPt[17];
                wPt[34] = SSIG1(wPt[32]) + wPt[27] + SSIG0(wPt[19]) + wPt[18];
                wPt[35] = SSIG1(wPt[33]) + wPt[28] + SSIG0(wPt[20]) + wPt[19];
                wPt[36] = SSIG1(wPt[34]) + wPt[29] + SSIG0(wPt[21]) + wPt[20];
                wPt[37] = SSIG1(wPt[35]) + wPt[30] + SSIG0(wPt[22]) + wPt[21];
                wPt[38] = SSIG1(wPt[36]) + wPt[31] + SSIG0(wPt[23]) + wPt[22];
                wPt[39] = SSIG1(wPt[37]) + wPt[32] + SSIG0(wPt[24]) + wPt[23];
                wPt[40] = SSIG1(wPt[38]) + wPt[33] + SSIG0(wPt[25]) + wPt[24];
                wPt[41] = SSIG1(wPt[39]) + wPt[34] + SSIG0(wPt[26]) + wPt[25];
                wPt[42] = SSIG1(wPt[40]) + wPt[35] + SSIG0(wPt[27]) + wPt[26];
                wPt[43] = SSIG1(wPt[41]) + wPt[36] + SSIG0(wPt[28]) + wPt[27];
                wPt[44] = SSIG1(wPt[42]) + wPt[37] + SSIG0(wPt[29]) + wPt[28];
                wPt[45] = SSIG1(wPt[43]) + wPt[38] + SSIG0(wPt[30]) + wPt[29];
                wPt[46] = SSIG1(wPt[44]) + wPt[39] + SSIG0(wPt[31]) + wPt[30];
                wPt[47] = SSIG1(wPt[45]) + wPt[40] + SSIG0(wPt[32]) + wPt[31];
                wPt[48] = SSIG1(wPt[46]) + wPt[41] + SSIG0(wPt[33]) + wPt[32];
                wPt[49] = SSIG1(wPt[47]) + wPt[42] + SSIG0(wPt[34]) + wPt[33];
                wPt[50] = SSIG1(wPt[48]) + wPt[43] + SSIG0(wPt[35]) + wPt[34];
                wPt[51] = SSIG1(wPt[49]) + wPt[44] + SSIG0(wPt[36]) + wPt[35];
                wPt[52] = SSIG1(wPt[50]) + wPt[45] + SSIG0(wPt[37]) + wPt[36];
                wPt[53] = SSIG1(wPt[51]) + wPt[46] + SSIG0(wPt[38]) + wPt[37];
                wPt[54] = SSIG1(wPt[52]) + wPt[47] + SSIG0(wPt[39]) + wPt[38];
                wPt[55] = SSIG1(wPt[53]) + wPt[48] + SSIG0(wPt[40]) + wPt[39];
                wPt[56] = SSIG1(wPt[54]) + wPt[49] + SSIG0(wPt[41]) + wPt[40];
                wPt[57] = SSIG1(wPt[55]) + wPt[50] + SSIG0(wPt[42]) + wPt[41];
                wPt[58] = SSIG1(wPt[56]) + wPt[51] + SSIG0(wPt[43]) + wPt[42];
                wPt[59] = SSIG1(wPt[57]) + wPt[52] + SSIG0(wPt[44]) + wPt[43];
                wPt[60] = SSIG1(wPt[58]) + wPt[53] + SSIG0(wPt[45]) + wPt[44];
                wPt[61] = SSIG1(wPt[59]) + wPt[54] + SSIG0(wPt[46]) + wPt[45];
                wPt[62] = SSIG1(wPt[60]) + wPt[55] + SSIG0(wPt[47]) + wPt[46];
                wPt[63] = SSIG1(wPt[61]) + wPt[56] + SSIG0(wPt[48]) + wPt[47];

                CompressBlock_WithWSet(hPt, wPt);

                // Only the first 16 bytes are used in SipHash
                return new byte[16]
                {
                    (byte)(hPt[0] >> 24), (byte)(hPt[0] >> 16), (byte)(hPt[0] >> 8), (byte)hPt[0],
                    (byte)(hPt[1] >> 24), (byte)(hPt[1] >> 16), (byte)(hPt[1] >> 8), (byte)hPt[1],
                    (byte)(hPt[2] >> 24), (byte)(hPt[2] >> 16), (byte)(hPt[2] >> 8), (byte)hPt[2],
                    (byte)(hPt[3] >> 24), (byte)(hPt[3] >> 16), (byte)(hPt[3] >> 8), (byte)hPt[3],
                };
            }
        }


        private void CheckTagInputs(int count, byte[][] data)
        {
            if (data.Length != count)
                throw new ArgumentOutOfRangeException(nameof(data), $"This tag needs {count} data input(s).");
            if (data.Any(item => item.Length != 32))
                throw new ArgumentOutOfRangeException(nameof(data), "Each array must be 32 bytes.");
        }
        /// <summary>
        /// Computes "Tagged Hash" specified by BIP-340 to be used in Taproot.
        /// </summary>
        /// <remarks>
        /// This method is mainly used for testing internal methods and should not be used by internal functions.
        /// </remarks>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="tag">The tag used in computation</param>
        /// <param name="usedOptimization">Returns if an optimized route was used</param>
        /// <param name="data">A list of 32-byte long arrays</param>
        /// <returns>The computed hash</returns>
        public byte[] ComputeTaggedHash(string tag, out bool usedOptimization, params byte[][] data)
        {
            if (tag is null)
                throw new ArgumentNullException(nameof(tag), "Tag can not be null."); // It can be empty!
            if (data == null || data.Length == 0 || data.Any(x => x == null))
                throw new ArgumentNullException(nameof(data), "The extra data can not be null or empty.");

            usedOptimization = true;
            switch (tag)
            {
                case "BIP0340/aux":
                    CheckTagInputs(1, data);
                    return ComputeTaggedHash_BIP340_aux(data[0]);
                case "BIP0340/nonce":
                    CheckTagInputs(3, data);
                    return ComputeTaggedHash_BIP340_nonce(data[0], data[1], data[2]);
                case "BIP0340/challenge":
                    CheckTagInputs(3, data);
                    return ComputeTaggedHash_BIP340_challenge(data[0], data[1], data[2]);
                case "TapLeaf":
                    // TapLeaf data length is variable
                    return ComputeTaggedHash_TapLeaf(data[0]);
                case "TapBranch":
                    CheckTagInputs(2, data);
                    return ComputeTaggedHash_TapBranch(data[0], data[1]);

            }
            usedOptimization = false;

            byte[] tagHash = ComputeHash(Encoding.UTF8.GetBytes(tag));
            byte[] toHash = new byte[tagHash.Length + tagHash.Length + data.Sum(x => x.Length)];
            Buffer.BlockCopy(tagHash, 0, toHash, 0, 32);
            Buffer.BlockCopy(tagHash, 0, toHash, 32, 32);
            int offset = 64;
            foreach (var ba in data)
            {
                Buffer.BlockCopy(ba, 0, toHash, offset, ba.Length);
                offset += ba.Length;
            }
            return ComputeHash(toHash);
        }

        internal unsafe byte[] ComputeTaggedHash_BIP340_aux(byte[] aux)
        {
            Debug.Assert(aux != null && aux.Length == 32);

            // Total data length to be hashed is 96 ([32+32] + 32)
            fixed (byte* dPt = &aux[0])
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                // The first 64 bytes (1 block) is equal to SHA256("BIP0340/aux") | SHA256("BIP0340/aux")
                // This can be pre-computed and change the HashState's initial value
                hPt[0] = 0x24dd3219U;
                hPt[1] = 0x4eba7e70U;
                hPt[2] = 0xca0fabb9U;
                hPt[3] = 0x0fa3166dU;
                hPt[4] = 0x3afbe4b1U;
                hPt[5] = 0x4c44df97U;
                hPt[6] = 0x4aac2739U;
                hPt[7] = 0x249e850aU;

                // The second block (64 to 96) is aux
                wPt[0] = (uint)((dPt[00] << 24) | (dPt[01] << 16) | (dPt[02] << 8) | dPt[03]);
                wPt[1] = (uint)((dPt[04] << 24) | (dPt[05] << 16) | (dPt[06] << 8) | dPt[07]);
                wPt[2] = (uint)((dPt[08] << 24) | (dPt[09] << 16) | (dPt[10] << 8) | dPt[11]);
                wPt[3] = (uint)((dPt[12] << 24) | (dPt[13] << 16) | (dPt[14] << 8) | dPt[15]);
                wPt[4] = (uint)((dPt[16] << 24) | (dPt[17] << 16) | (dPt[18] << 8) | dPt[19]);
                wPt[5] = (uint)((dPt[20] << 24) | (dPt[21] << 16) | (dPt[22] << 8) | dPt[23]);
                wPt[6] = (uint)((dPt[24] << 24) | (dPt[25] << 16) | (dPt[26] << 8) | dPt[27]);
                wPt[7] = (uint)((dPt[28] << 24) | (dPt[29] << 16) | (dPt[30] << 8) | dPt[31]);
                wPt[8] = 0b10000000_00000000_00000000_00000000U;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                wPt[14] = 0;
                wPt[15] = 768; // len = 96*8

                wPt[16] = SSIG0(wPt[1]) + wPt[0];
                wPt[17] = 31457280 + SSIG0(wPt[2]) + wPt[1];
                wPt[18] = SSIG1(wPt[16]) + SSIG0(wPt[3]) + wPt[2];
                wPt[19] = SSIG1(wPt[17]) + SSIG0(wPt[4]) + wPt[3];
                wPt[20] = SSIG1(wPt[18]) + SSIG0(wPt[5]) + wPt[4];
                wPt[21] = SSIG1(wPt[19]) + SSIG0(wPt[6]) + wPt[5];
                wPt[22] = SSIG1(wPt[20]) + 768 + SSIG0(wPt[7]) + wPt[6];
                wPt[23] = SSIG1(wPt[21]) + wPt[16] + 285220864 + wPt[7];
                wPt[24] = SSIG1(wPt[22]) + wPt[17] + 2147483648;
                wPt[25] = SSIG1(wPt[23]) + wPt[18];
                wPt[26] = SSIG1(wPt[24]) + wPt[19];
                wPt[27] = SSIG1(wPt[25]) + wPt[20];
                wPt[28] = SSIG1(wPt[26]) + wPt[21];
                wPt[29] = SSIG1(wPt[27]) + wPt[22];
                wPt[30] = SSIG1(wPt[28]) + wPt[23] + 12583014;
                wPt[31] = SSIG1(wPt[29]) + wPt[24] + SSIG0(wPt[16]) + 768;
                wPt[32] = SSIG1(wPt[30]) + wPt[25] + SSIG0(wPt[17]) + wPt[16];
                wPt[33] = SSIG1(wPt[31]) + wPt[26] + SSIG0(wPt[18]) + wPt[17];
                wPt[34] = SSIG1(wPt[32]) + wPt[27] + SSIG0(wPt[19]) + wPt[18];
                wPt[35] = SSIG1(wPt[33]) + wPt[28] + SSIG0(wPt[20]) + wPt[19];
                wPt[36] = SSIG1(wPt[34]) + wPt[29] + SSIG0(wPt[21]) + wPt[20];
                wPt[37] = SSIG1(wPt[35]) + wPt[30] + SSIG0(wPt[22]) + wPt[21];
                wPt[38] = SSIG1(wPt[36]) + wPt[31] + SSIG0(wPt[23]) + wPt[22];
                wPt[39] = SSIG1(wPt[37]) + wPt[32] + SSIG0(wPt[24]) + wPt[23];
                wPt[40] = SSIG1(wPt[38]) + wPt[33] + SSIG0(wPt[25]) + wPt[24];
                wPt[41] = SSIG1(wPt[39]) + wPt[34] + SSIG0(wPt[26]) + wPt[25];
                wPt[42] = SSIG1(wPt[40]) + wPt[35] + SSIG0(wPt[27]) + wPt[26];
                wPt[43] = SSIG1(wPt[41]) + wPt[36] + SSIG0(wPt[28]) + wPt[27];
                wPt[44] = SSIG1(wPt[42]) + wPt[37] + SSIG0(wPt[29]) + wPt[28];
                wPt[45] = SSIG1(wPt[43]) + wPt[38] + SSIG0(wPt[30]) + wPt[29];
                wPt[46] = SSIG1(wPt[44]) + wPt[39] + SSIG0(wPt[31]) + wPt[30];
                wPt[47] = SSIG1(wPt[45]) + wPt[40] + SSIG0(wPt[32]) + wPt[31];
                wPt[48] = SSIG1(wPt[46]) + wPt[41] + SSIG0(wPt[33]) + wPt[32];
                wPt[49] = SSIG1(wPt[47]) + wPt[42] + SSIG0(wPt[34]) + wPt[33];
                wPt[50] = SSIG1(wPt[48]) + wPt[43] + SSIG0(wPt[35]) + wPt[34];
                wPt[51] = SSIG1(wPt[49]) + wPt[44] + SSIG0(wPt[36]) + wPt[35];
                wPt[52] = SSIG1(wPt[50]) + wPt[45] + SSIG0(wPt[37]) + wPt[36];
                wPt[53] = SSIG1(wPt[51]) + wPt[46] + SSIG0(wPt[38]) + wPt[37];
                wPt[54] = SSIG1(wPt[52]) + wPt[47] + SSIG0(wPt[39]) + wPt[38];
                wPt[55] = SSIG1(wPt[53]) + wPt[48] + SSIG0(wPt[40]) + wPt[39];
                wPt[56] = SSIG1(wPt[54]) + wPt[49] + SSIG0(wPt[41]) + wPt[40];
                wPt[57] = SSIG1(wPt[55]) + wPt[50] + SSIG0(wPt[42]) + wPt[41];
                wPt[58] = SSIG1(wPt[56]) + wPt[51] + SSIG0(wPt[43]) + wPt[42];
                wPt[59] = SSIG1(wPt[57]) + wPt[52] + SSIG0(wPt[44]) + wPt[43];
                wPt[60] = SSIG1(wPt[58]) + wPt[53] + SSIG0(wPt[45]) + wPt[44];
                wPt[61] = SSIG1(wPt[59]) + wPt[54] + SSIG0(wPt[46]) + wPt[45];
                wPt[62] = SSIG1(wPt[60]) + wPt[55] + SSIG0(wPt[47]) + wPt[46];
                wPt[63] = SSIG1(wPt[61]) + wPt[56] + SSIG0(wPt[48]) + wPt[47];

                CompressBlock_WithWSet(hPt, wPt);

                return GetBytes(hPt);
            }
        }

        internal unsafe byte[] ComputeTaggedHash_BIP340_nonce(byte[] t, byte[] pba, byte[] data)
        {
            Debug.Assert(t != null && t.Length == 32);
            Debug.Assert(pba != null && pba.Length == 32);
            Debug.Assert(data != null && data.Length == 32);

            // Total data length to be hashed is 160 ([32+32] + 32 + 32 + 32)
            fixed (byte* tPt = &t[0], pPt = &pba[0], dPt = &data[0])
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                // The first 64 bytes (1 block) is equal to SHA256("BIP0340/nonce") | SHA256("BIP0340/nonce")
                // This can be pre-computed and change the HashState's initial value
                hPt[0] = 0x46615b35U;
                hPt[1] = 0xf4bfbff7U;
                hPt[2] = 0x9f8dc671U;
                hPt[3] = 0x83627ab3U;
                hPt[4] = 0x60217180U;
                hPt[5] = 0x57358661U;
                hPt[6] = 0x21a29e54U;
                hPt[7] = 0x68b07b4cU;

                // The second block (64 to 128) is kBa | data
                wPt[0] = (uint)((tPt[00] << 24) | (tPt[01] << 16) | (tPt[02] << 8) | tPt[03]);
                wPt[1] = (uint)((tPt[04] << 24) | (tPt[05] << 16) | (tPt[06] << 8) | tPt[07]);
                wPt[2] = (uint)((tPt[08] << 24) | (tPt[09] << 16) | (tPt[10] << 8) | tPt[11]);
                wPt[3] = (uint)((tPt[12] << 24) | (tPt[13] << 16) | (tPt[14] << 8) | tPt[15]);
                wPt[4] = (uint)((tPt[16] << 24) | (tPt[17] << 16) | (tPt[18] << 8) | tPt[19]);
                wPt[5] = (uint)((tPt[20] << 24) | (tPt[21] << 16) | (tPt[22] << 8) | tPt[23]);
                wPt[6] = (uint)((tPt[24] << 24) | (tPt[25] << 16) | (tPt[26] << 8) | tPt[27]);
                wPt[7] = (uint)((tPt[28] << 24) | (tPt[29] << 16) | (tPt[30] << 8) | tPt[31]);

                wPt[8] = (uint)((pPt[00] << 24) | (pPt[01] << 16) | (pPt[02] << 8) | pPt[03]);
                wPt[9] = (uint)((pPt[04] << 24) | (pPt[05] << 16) | (pPt[06] << 8) | pPt[07]);
                wPt[10] = (uint)((pPt[08] << 24) | (pPt[09] << 16) | (pPt[10] << 8) | pPt[11]);
                wPt[11] = (uint)((pPt[12] << 24) | (pPt[13] << 16) | (pPt[14] << 8) | pPt[15]);
                wPt[12] = (uint)((pPt[16] << 24) | (pPt[17] << 16) | (pPt[18] << 8) | pPt[19]);
                wPt[13] = (uint)((pPt[20] << 24) | (pPt[21] << 16) | (pPt[22] << 8) | pPt[23]);
                wPt[14] = (uint)((pPt[24] << 24) | (pPt[25] << 16) | (pPt[26] << 8) | pPt[27]);
                wPt[15] = (uint)((pPt[28] << 24) | (pPt[29] << 16) | (pPt[30] << 8) | pPt[31]);

                CompressBlock(hPt, wPt);

                // The third block (128 to 192) is pad
                wPt[0] = (uint)((dPt[00] << 24) | (dPt[01] << 16) | (dPt[02] << 8) | dPt[03]);
                wPt[1] = (uint)((dPt[04] << 24) | (dPt[05] << 16) | (dPt[06] << 8) | dPt[07]);
                wPt[2] = (uint)((dPt[08] << 24) | (dPt[09] << 16) | (dPt[10] << 8) | dPt[11]);
                wPt[3] = (uint)((dPt[12] << 24) | (dPt[13] << 16) | (dPt[14] << 8) | dPt[15]);
                wPt[4] = (uint)((dPt[16] << 24) | (dPt[17] << 16) | (dPt[18] << 8) | dPt[19]);
                wPt[5] = (uint)((dPt[20] << 24) | (dPt[21] << 16) | (dPt[22] << 8) | dPt[23]);
                wPt[6] = (uint)((dPt[24] << 24) | (dPt[25] << 16) | (dPt[26] << 8) | dPt[27]);
                wPt[7] = (uint)((dPt[28] << 24) | (dPt[29] << 16) | (dPt[30] << 8) | dPt[31]);
                wPt[8] = 0b10000000_00000000_00000000_00000000U;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                wPt[14] = 0;
                wPt[15] = 1280; // len = 160*8

                wPt[16] = SSIG0(wPt[1]) + wPt[0];
                wPt[17] = 35651585 + SSIG0(wPt[2]) + wPt[1];
                wPt[18] = SSIG1(wPt[16]) + SSIG0(wPt[3]) + wPt[2];
                wPt[19] = SSIG1(wPt[17]) + SSIG0(wPt[4]) + wPt[3];
                wPt[20] = SSIG1(wPt[18]) + SSIG0(wPt[5]) + wPt[4];
                wPt[21] = SSIG1(wPt[19]) + SSIG0(wPt[6]) + wPt[5];
                wPt[22] = SSIG1(wPt[20]) + 1280 + SSIG0(wPt[7]) + wPt[6];
                wPt[23] = SSIG1(wPt[21]) + wPt[16] + 285220864 + wPt[7];
                wPt[24] = SSIG1(wPt[22]) + wPt[17] + 2147483648;
                wPt[25] = SSIG1(wPt[23]) + wPt[18];
                wPt[26] = SSIG1(wPt[24]) + wPt[19];
                wPt[27] = SSIG1(wPt[25]) + wPt[20];
                wPt[28] = SSIG1(wPt[26]) + wPt[21];
                wPt[29] = SSIG1(wPt[27]) + wPt[22];
                wPt[30] = SSIG1(wPt[28]) + wPt[23] + 20971690;
                wPt[31] = SSIG1(wPt[29]) + wPt[24] + SSIG0(wPt[16]) + 1280;
                wPt[32] = SSIG1(wPt[30]) + wPt[25] + SSIG0(wPt[17]) + wPt[16];
                wPt[33] = SSIG1(wPt[31]) + wPt[26] + SSIG0(wPt[18]) + wPt[17];
                wPt[34] = SSIG1(wPt[32]) + wPt[27] + SSIG0(wPt[19]) + wPt[18];
                wPt[35] = SSIG1(wPt[33]) + wPt[28] + SSIG0(wPt[20]) + wPt[19];
                wPt[36] = SSIG1(wPt[34]) + wPt[29] + SSIG0(wPt[21]) + wPt[20];
                wPt[37] = SSIG1(wPt[35]) + wPt[30] + SSIG0(wPt[22]) + wPt[21];
                wPt[38] = SSIG1(wPt[36]) + wPt[31] + SSIG0(wPt[23]) + wPt[22];
                wPt[39] = SSIG1(wPt[37]) + wPt[32] + SSIG0(wPt[24]) + wPt[23];
                wPt[40] = SSIG1(wPt[38]) + wPt[33] + SSIG0(wPt[25]) + wPt[24];
                wPt[41] = SSIG1(wPt[39]) + wPt[34] + SSIG0(wPt[26]) + wPt[25];
                wPt[42] = SSIG1(wPt[40]) + wPt[35] + SSIG0(wPt[27]) + wPt[26];
                wPt[43] = SSIG1(wPt[41]) + wPt[36] + SSIG0(wPt[28]) + wPt[27];
                wPt[44] = SSIG1(wPt[42]) + wPt[37] + SSIG0(wPt[29]) + wPt[28];
                wPt[45] = SSIG1(wPt[43]) + wPt[38] + SSIG0(wPt[30]) + wPt[29];
                wPt[46] = SSIG1(wPt[44]) + wPt[39] + SSIG0(wPt[31]) + wPt[30];
                wPt[47] = SSIG1(wPt[45]) + wPt[40] + SSIG0(wPt[32]) + wPt[31];
                wPt[48] = SSIG1(wPt[46]) + wPt[41] + SSIG0(wPt[33]) + wPt[32];
                wPt[49] = SSIG1(wPt[47]) + wPt[42] + SSIG0(wPt[34]) + wPt[33];
                wPt[50] = SSIG1(wPt[48]) + wPt[43] + SSIG0(wPt[35]) + wPt[34];
                wPt[51] = SSIG1(wPt[49]) + wPt[44] + SSIG0(wPt[36]) + wPt[35];
                wPt[52] = SSIG1(wPt[50]) + wPt[45] + SSIG0(wPt[37]) + wPt[36];
                wPt[53] = SSIG1(wPt[51]) + wPt[46] + SSIG0(wPt[38]) + wPt[37];
                wPt[54] = SSIG1(wPt[52]) + wPt[47] + SSIG0(wPt[39]) + wPt[38];
                wPt[55] = SSIG1(wPt[53]) + wPt[48] + SSIG0(wPt[40]) + wPt[39];
                wPt[56] = SSIG1(wPt[54]) + wPt[49] + SSIG0(wPt[41]) + wPt[40];
                wPt[57] = SSIG1(wPt[55]) + wPt[50] + SSIG0(wPt[42]) + wPt[41];
                wPt[58] = SSIG1(wPt[56]) + wPt[51] + SSIG0(wPt[43]) + wPt[42];
                wPt[59] = SSIG1(wPt[57]) + wPt[52] + SSIG0(wPt[44]) + wPt[43];
                wPt[60] = SSIG1(wPt[58]) + wPt[53] + SSIG0(wPt[45]) + wPt[44];
                wPt[61] = SSIG1(wPt[59]) + wPt[54] + SSIG0(wPt[46]) + wPt[45];
                wPt[62] = SSIG1(wPt[60]) + wPt[55] + SSIG0(wPt[47]) + wPt[46];
                wPt[63] = SSIG1(wPt[61]) + wPt[56] + SSIG0(wPt[48]) + wPt[47];

                CompressBlock_WithWSet(hPt, wPt);

                return GetBytes(hPt);
            }
        }

        internal unsafe byte[] ComputeTaggedHash_BIP340_challenge(byte[] rba, byte[] pba, byte[] data)
        {
            Debug.Assert(rba != null && rba.Length == 32);
            Debug.Assert(pba != null && pba.Length == 32);
            Debug.Assert(data != null && data.Length == 32);

            // Total data length to be hashed is 160 ([32+32] + 32 + 32 + 32)
            fixed (byte* rPt = &rba[0], pPt = &pba[0], dPt = &data[0])
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                // The first 64 bytes (1 block) is equal to SHA256("BIP0340/challenge") | SHA256("BIP0340/challenge")
                // This can be pre-computed and change the HashState's initial value
                hPt[0] = 0x9cecba11U;
                hPt[1] = 0x23925381U;
                hPt[2] = 0x11679112U;
                hPt[3] = 0xd1627e0fU;
                hPt[4] = 0x97c87550U;
                hPt[5] = 0x003cc765U;
                hPt[6] = 0x90f61164U;
                hPt[7] = 0x33e9b66aU;

                // The second block (64 to 128) is rBa | pBa
                for (int i = 0, j = 0; i < 8; i++, j += 4)
                {
                    wPt[i] = (uint)((rPt[j] << 24) | (rPt[j + 1] << 16) | (rPt[j + 2] << 8) | rPt[j + 3]);
                }
                for (int i = 8, j = 0; i < 16; i++, j += 4)
                {
                    wPt[i] = (uint)((pba[j] << 24) | (pba[j + 1] << 16) | (pba[j + 2] << 8) | pba[j + 3]);
                }

                CompressBlock(hPt, wPt);

                // The third block (128 to 192) is data | pad
                for (int i = 0, j = 0; i < 8; i++, j += 4)
                {
                    wPt[i] = (uint)((dPt[j] << 24) | (dPt[j + 1] << 16) | (dPt[j + 2] << 8) | dPt[j + 3]);
                }
                wPt[8] = 0b10000000_00000000_00000000_00000000U;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                wPt[14] = 0;
                wPt[15] = 1280; // len = 160*8

                CompressBlock(hPt, wPt);

                return GetBytes(hPt);
            }
        }

        internal unsafe byte[] ComputeTaggedHash_TapLeaf(byte[] data)
        {
            Debug.Assert(data != null && data.Length > 0);

            fixed (byte* dPt = &data[0])
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                // The first 64 bytes (1 block) is equal to SHA256("TapLeaf") | SHA256("TapLeaf")
                // This can be pre-computed and change the HashState's initial value
                hPt[0] = 0x9ce0e4e6;
                hPt[1] = 0x7c116c39;
                hPt[2] = 0x38b3caf2;
                hPt[3] = 0xc30f5089;
                hPt[4] = 0xd3f3936c;
                hPt[5] = 0x47636e60;
                hPt[6] = 0x7db33eea;
                hPt[7] = 0xddc6f0c9;

                // The second block
                CompressData(dPt, data.Length, data.Length + 64, hPt, wPt);

                return GetBytes(hPt);
            }
        }

        internal unsafe byte[] ComputeTaggedHash_TapBranch(ReadOnlySpan<byte> first32, ReadOnlySpan<byte> second32)
        {
            Debug.Assert(first32 != null && first32.Length == 32);
            Debug.Assert(second32 != null && second32.Length == 32);

            // Total data length to be hashed is 128 ([32+32] + 32 + 32)
            fixed (byte* pt1 = &first32[0], pt2 = &second32[0])
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                // The first 64 bytes (1 block) is equal to SHA256("TapBranch") | SHA256("TapBranch")
                // This can be pre-computed and change the HashState's initial value
                hPt[0] = 0x23a865a9;
                hPt[1] = 0xb8a40da7;
                hPt[2] = 0x977c1e04;
                hPt[3] = 0xc49e246f;
                hPt[4] = 0xb5be1376;
                hPt[5] = 0x9d24c9b7;
                hPt[6] = 0xb583b5d4;
                hPt[7] = 0xa8d226d2;

                // The second block (32+32 bytes)
                wPt[0] = (uint)((pt1[00] << 24) | (pt1[01] << 16) | (pt1[02] << 8) | pt1[03]);
                wPt[1] = (uint)((pt1[04] << 24) | (pt1[05] << 16) | (pt1[06] << 8) | pt1[07]);
                wPt[2] = (uint)((pt1[08] << 24) | (pt1[09] << 16) | (pt1[10] << 8) | pt1[11]);
                wPt[3] = (uint)((pt1[12] << 24) | (pt1[13] << 16) | (pt1[14] << 8) | pt1[15]);
                wPt[4] = (uint)((pt1[16] << 24) | (pt1[17] << 16) | (pt1[18] << 8) | pt1[19]);
                wPt[5] = (uint)((pt1[20] << 24) | (pt1[21] << 16) | (pt1[22] << 8) | pt1[23]);
                wPt[6] = (uint)((pt1[24] << 24) | (pt1[25] << 16) | (pt1[26] << 8) | pt1[27]);
                wPt[7] = (uint)((pt1[28] << 24) | (pt1[29] << 16) | (pt1[30] << 8) | pt1[31]);

                wPt[8] = (uint)((pt2[00] << 24) | (pt2[01] << 16) | (pt2[02] << 8) | pt2[03]);
                wPt[9] = (uint)((pt2[04] << 24) | (pt2[05] << 16) | (pt2[06] << 8) | pt2[07]);
                wPt[10] = (uint)((pt2[08] << 24) | (pt2[09] << 16) | (pt2[10] << 8) | pt2[11]);
                wPt[11] = (uint)((pt2[12] << 24) | (pt2[13] << 16) | (pt2[14] << 8) | pt2[15]);
                wPt[12] = (uint)((pt2[16] << 24) | (pt2[17] << 16) | (pt2[18] << 8) | pt2[19]);
                wPt[13] = (uint)((pt2[20] << 24) | (pt2[21] << 16) | (pt2[22] << 8) | pt2[23]);
                wPt[14] = (uint)((pt2[24] << 24) | (pt2[25] << 16) | (pt2[26] << 8) | pt2[27]);
                wPt[15] = (uint)((pt2[28] << 24) | (pt2[29] << 16) | (pt2[30] << 8) | pt2[31]);

                CompressBlock(hPt, wPt);

                // The third block (paddings)
                wPt[0] = 0b10000000_00000000_00000000_00000000U;
                wPt[1] = 0;
                wPt[2] = 0;
                wPt[3] = 0;
                wPt[4] = 0;
                wPt[5] = 0;
                wPt[6] = 0;
                wPt[7] = 0;
                wPt[8] = 0;
                wPt[9] = 0;
                wPt[10] = 0;
                wPt[11] = 0;
                wPt[12] = 0;
                wPt[13] = 0;
                wPt[14] = 0;
                wPt[15] = 1024;

                wPt[16] = 0x80000000;
                wPt[17] = 0x02800001;
                wPt[18] = 0x00205000;
                wPt[19] = 0x00000110;
                wPt[20] = 0x22000800;
                wPt[21] = 0x00aa0000;
                wPt[22] = 0x05089942;
                wPt[23] = 0xc0002ac0;
                wPt[24] = 0x62080004;
                wPt[25] = 0x1028c80a;
                wPt[26] = 0x001a4055;
                wPt[27] = 0x9f004823;
                wPt[28] = 0x68ca269e;
                wPt[29] = 0x323b15b4;
                wPt[30] = 0x1886f73d;
                wPt[31] = 0x5b6835a3;
                wPt[32] = 0x37fd1798;
                wPt[33] = 0x3311a7d2;
                wPt[34] = 0xe8977a87;
                wPt[35] = 0x55edccc1;
                wPt[36] = 0x26785e65;
                wPt[37] = 0x1c1a75cd;
                wPt[38] = 0x1898add6;
                wPt[39] = 0x70d975ed;
                wPt[40] = 0xfc995de5;
                wPt[41] = 0xc72d9f47;
                wPt[42] = 0x225062f2;
                wPt[43] = 0xfa62c148;
                wPt[44] = 0x6d6275f8;
                wPt[45] = 0x4876537f;
                wPt[46] = 0x3e6bd0af;
                wPt[47] = 0xaf3a394c;
                wPt[48] = 0x5d69345c;
                wPt[49] = 0x7d685338;
                wPt[50] = 0x9ad3729d;
                wPt[51] = 0xc04f60b4;
                wPt[52] = 0x4af2ba27;
                wPt[53] = 0x3b5ad539;
                wPt[54] = 0x5b9a980b;
                wPt[55] = 0x818b7cdd;
                wPt[56] = 0x89cdea52;
                wPt[57] = 0x2c88481e;
                wPt[58] = 0x69cbcd7e;
                wPt[59] = 0xd265fe42;
                wPt[60] = 0xab09cb34;
                wPt[61] = 0x9288f7b9;
                wPt[62] = 0x9fb768b8;
                wPt[63] = 0x9c18607f;

                CompressBlock_WithWSet(hPt, wPt);

                return GetBytes(hPt);
            }
        }


        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="buffer">The byte array to compute hash for</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Instance was disposed.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Data can not be null.");
            if (offset < 0 || count < 0)
                throw new IndexOutOfRangeException("Offset or count can not be negative.");
            if (buffer.Length != 0 && offset > buffer.Length - 1 || buffer.Length == 0 && offset != 0)
                throw new IndexOutOfRangeException("Index can not be bigger than array length.");
            if (count > buffer.Length - offset)
                throw new IndexOutOfRangeException("Array is not long enough.");

            fixed (byte* dPt = &buffer[offset])
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                Init(hPt);
                CompressData(dPt, count, count, hPt, wPt);

                return GetBytes(hPt);
            }
        }



        internal unsafe void Init()
        {
            fixed (uint* hPt = &hashState[0])
            {
                Init(hPt);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void Init(uint* hPt)
        {
            hPt[0] = 0x6a09e667U;
            hPt[1] = 0xbb67ae85U;
            hPt[2] = 0x3c6ef372U;
            hPt[3] = 0xa54ff53aU;
            hPt[4] = 0x510e527fU;
            hPt[5] = 0x9b05688cU;
            hPt[6] = 0x1f83d9abU;
            hPt[7] = 0x5be0cd19U;
        }


        internal unsafe byte[] GetBytes()
        {
            fixed (uint* hPt = &hashState[0])
                return GetBytes(hPt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe byte[] GetBytes(uint* hPt)
        {
            return new byte[32]
            {
                (byte)(hPt[0] >> 24), (byte)(hPt[0] >> 16), (byte)(hPt[0] >> 8), (byte)hPt[0],
                (byte)(hPt[1] >> 24), (byte)(hPt[1] >> 16), (byte)(hPt[1] >> 8), (byte)hPt[1],
                (byte)(hPt[2] >> 24), (byte)(hPt[2] >> 16), (byte)(hPt[2] >> 8), (byte)hPt[2],
                (byte)(hPt[3] >> 24), (byte)(hPt[3] >> 16), (byte)(hPt[3] >> 8), (byte)hPt[3],
                (byte)(hPt[4] >> 24), (byte)(hPt[4] >> 16), (byte)(hPt[4] >> 8), (byte)hPt[4],
                (byte)(hPt[5] >> 24), (byte)(hPt[5] >> 16), (byte)(hPt[5] >> 8), (byte)hPt[5],
                (byte)(hPt[6] >> 24), (byte)(hPt[6] >> 16), (byte)(hPt[6] >> 8), (byte)hPt[6],
                (byte)(hPt[7] >> 24), (byte)(hPt[7] >> 16), (byte)(hPt[7] >> 8), (byte)hPt[7]
            };
        }


        internal unsafe void CompressData(byte* dPt, int dataLen, int totalLen, uint* hPt, uint* wPt)
        {
            Span<byte> finalBlock = new byte[64];

            fixed (byte* fPt = &finalBlock[0])
            {
                int dIndex = 0;
                while (dataLen >= BlockByteSize)
                {
                    for (int i = 0; i < 16; i++, dIndex += 4)
                    {
                        wPt[i] = (uint)((dPt[dIndex] << 24) | (dPt[dIndex + 1] << 16) | (dPt[dIndex + 2] << 8) | dPt[dIndex + 3]);
                    }

                    CompressBlock(hPt, wPt);

                    dataLen -= BlockByteSize;
                }

                // Copy the reamaining bytes into a blockSize length buffer so that we can loop through it easily:
                Buffer.MemoryCopy(dPt + dIndex, fPt, finalBlock.Length, dataLen);

                // Append 1 bit followed by zeros. Since we only work with bytes, this is 1 whole byte
                fPt[dataLen] = 0b1000_0000;

                if (dataLen >= 56) // blockSize - pad2.Len = 64 - 8
                {
                    // This means we have an additional block to compress, which we do it here:

                    for (int i = 0, j = 0; i < 16; i++, j += 4)
                    {
                        wPt[i] = (uint)((fPt[j] << 24) | (fPt[j + 1] << 16) | (fPt[j + 2] << 8) | fPt[j + 3]);
                    }

                    CompressBlock(hPt, wPt);

                    // Zero out all the items in FinalBlock so it can be reused
                    finalBlock.Clear();
                }

                // Add length in bits as the last 8 bytes of final block in big-endian order
                // See MessageLengthTest in Test project to understand what the following shifts are
                fPt[63] = (byte)(totalLen << 3);
                fPt[62] = (byte)(totalLen >> 5);
                fPt[61] = (byte)(totalLen >> 13);
                fPt[60] = (byte)(totalLen >> 21);
                fPt[59] = (byte)(totalLen >> 29);
                // The remainig 3 bytes are always zero
                // The remaining 56 bytes are already set

                for (int i = 0, j = 0; i < 16; i++, j += 4)
                {
                    wPt[i] = (uint)((fPt[j] << 24) | (fPt[j + 1] << 16) | (fPt[j + 2] << 8) | fPt[j + 3]);
                }

                CompressBlock(hPt, wPt);
            }
        }


        /// <summary>
        /// Computes double SHA-256 hash of a 64 byte[] block inside <paramref name="src"/> and writes the result 
        /// to the given <paramref name="dst"/>. Useful for computing merkle roots.
        /// </summary>
        /// <param name="src">Pointer to start of a byte array with at least 64 items as the source</param>
        /// <param name="dst">Pointer to start of a byte array with at least 32 items for result</param>
        /// <param name="hPt">Hash state pointer</param>
        /// <param name="wPt">Working vector pointer</param>
        internal unsafe void Compress64Double(byte* src, byte* dst, uint* hPt, uint* wPt)
        {
            // TODO: maybe turn byte* to uint* to directly act as the working vector to skip extra byte to uint conversions

            // There are 3 block compressions here: 1st 64 byte data, 2nd 64 byte padding and 3rd second hash
            // Round 1, block 1
            Init(hPt);
            int dIndex = 0;
            for (int i = 0; i < 16; i++, dIndex += 4)
            {
                wPt[i] = (uint)((src[dIndex] << 24) | (src[dIndex + 1] << 16) | (src[dIndex + 2] << 8) | src[dIndex + 3]);
            }
            CompressBlock(hPt, wPt);

            // Round 1, block 2
            wPt[0] = 2147483648;
            wPt[1] = 0;
            wPt[2] = 0;
            wPt[3] = 0;
            wPt[4] = 0;
            wPt[5] = 0;
            wPt[6] = 0;
            wPt[7] = 0;
            wPt[8] = 0;
            wPt[9] = 0;
            wPt[10] = 0;
            wPt[11] = 0;
            wPt[12] = 0;
            wPt[13] = 0;
            wPt[14] = 0;
            wPt[15] = 512;
            wPt[16] = 2147483648;
            wPt[17] = 20971520;
            wPt[18] = 2117632;
            wPt[19] = 20616;
            wPt[20] = 570427392;
            wPt[21] = 575995924;
            wPt[22] = 84449090;
            wPt[23] = 2684354592;
            wPt[24] = 1518862336;
            wPt[25] = 6067200;
            wPt[26] = 1496221;
            wPt[27] = 4202700544;
            wPt[28] = 3543279056;
            wPt[29] = 291985753;
            wPt[30] = 4142317530;
            wPt[31] = 3003913545;
            wPt[32] = 145928272;
            wPt[33] = 2642168871;
            wPt[34] = 216179603;
            wPt[35] = 2296832490;
            wPt[36] = 2771075893;
            wPt[37] = 1738633033;
            wPt[38] = 3610378607;
            wPt[39] = 1324035729;
            wPt[40] = 1572820453;
            wPt[41] = 2397971253;
            wPt[42] = 3803995842;
            wPt[43] = 2822718356;
            wPt[44] = 1168996599;
            wPt[45] = 921948365;
            wPt[46] = 3650881000;
            wPt[47] = 2958106055;
            wPt[48] = 1773959876;
            wPt[49] = 3172022107;
            wPt[50] = 3820646885;
            wPt[51] = 991993842;
            wPt[52] = 419360279;
            wPt[53] = 3797604839;
            wPt[54] = 322392134;
            wPt[55] = 85264541;
            wPt[56] = 1326255876;
            wPt[57] = 640108622;
            wPt[58] = 822159570;
            wPt[59] = 3328750644;
            wPt[60] = 1107837388;
            wPt[61] = 1657999800;
            wPt[62] = 3852183409;
            wPt[63] = 2242356356;
            CompressBlock_WithWSet(hPt, wPt);

            // Round 2, block 3
            ComputeSecondHash(hPt, wPt);

            // Write result
            for (int i = 0, j = 0; i < 32; i += 4, j++)
            {
                dst[i] = (byte)(hPt[j] >> 24);
                dst[i + 1] = (byte)(hPt[j] >> 16);
                dst[i + 2] = (byte)(hPt[j] >> 8);
                dst[i + 3] = (byte)hPt[j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void ComputeSecondHash(uint* hPt, uint* wPt)
        {
            // Result of previous hash (hashState[]) is now our new block. So copy it here:
            wPt[0] = hPt[0];
            wPt[1] = hPt[1];
            wPt[2] = hPt[2];
            wPt[3] = hPt[3];
            wPt[4] = hPt[4];
            wPt[5] = hPt[5];
            wPt[6] = hPt[6];
            wPt[7] = hPt[7]; // 8*4 = 32 byte hash result

            wPt[8] = 0b10000000_00000000_00000000_00000000U; // 1 followed by 0 bits to fill pad1
            wPt[9] = 0;
            wPt[10] = 0;
            wPt[11] = 0;
            wPt[12] = 0;
            wPt[13] = 0;

            // Message length for pad2, since message is the 32 byte result of previous hash, length is 256 bit
            wPt[14] = 0;
            wPt[15] = 256;

            wPt[16] = SSIG0(wPt[1]) + wPt[0];
            wPt[17] = 10485760 + SSIG0(wPt[2]) + wPt[1];
            wPt[18] = SSIG1(wPt[16]) + SSIG0(wPt[3]) + wPt[2];
            wPt[19] = SSIG1(wPt[17]) + SSIG0(wPt[4]) + wPt[3];
            wPt[20] = SSIG1(wPt[18]) + SSIG0(wPt[5]) + wPt[4];
            wPt[21] = SSIG1(wPt[19]) + SSIG0(wPt[6]) + wPt[5];
            wPt[22] = SSIG1(wPt[20]) + 256 + SSIG0(wPt[7]) + wPt[6];
            wPt[23] = SSIG1(wPt[21]) + wPt[16] + 285220864 + wPt[7];
            wPt[24] = SSIG1(wPt[22]) + wPt[17] + 2147483648;
            wPt[25] = SSIG1(wPt[23]) + wPt[18];
            wPt[26] = SSIG1(wPt[24]) + wPt[19];
            wPt[27] = SSIG1(wPt[25]) + wPt[20];
            wPt[28] = SSIG1(wPt[26]) + wPt[21];
            wPt[29] = SSIG1(wPt[27]) + wPt[22];
            wPt[30] = SSIG1(wPt[28]) + wPt[23] + 4194338;
            wPt[31] = SSIG1(wPt[29]) + wPt[24] + SSIG0(wPt[16]) + 256;
            wPt[32] = SSIG1(wPt[30]) + wPt[25] + SSIG0(wPt[17]) + wPt[16];
            wPt[33] = SSIG1(wPt[31]) + wPt[26] + SSIG0(wPt[18]) + wPt[17];
            wPt[34] = SSIG1(wPt[32]) + wPt[27] + SSIG0(wPt[19]) + wPt[18];
            wPt[35] = SSIG1(wPt[33]) + wPt[28] + SSIG0(wPt[20]) + wPt[19];
            wPt[36] = SSIG1(wPt[34]) + wPt[29] + SSIG0(wPt[21]) + wPt[20];
            wPt[37] = SSIG1(wPt[35]) + wPt[30] + SSIG0(wPt[22]) + wPt[21];
            wPt[38] = SSIG1(wPt[36]) + wPt[31] + SSIG0(wPt[23]) + wPt[22];
            wPt[39] = SSIG1(wPt[37]) + wPt[32] + SSIG0(wPt[24]) + wPt[23];
            wPt[40] = SSIG1(wPt[38]) + wPt[33] + SSIG0(wPt[25]) + wPt[24];
            wPt[41] = SSIG1(wPt[39]) + wPt[34] + SSIG0(wPt[26]) + wPt[25];
            wPt[42] = SSIG1(wPt[40]) + wPt[35] + SSIG0(wPt[27]) + wPt[26];
            wPt[43] = SSIG1(wPt[41]) + wPt[36] + SSIG0(wPt[28]) + wPt[27];
            wPt[44] = SSIG1(wPt[42]) + wPt[37] + SSIG0(wPt[29]) + wPt[28];
            wPt[45] = SSIG1(wPt[43]) + wPt[38] + SSIG0(wPt[30]) + wPt[29];
            wPt[46] = SSIG1(wPt[44]) + wPt[39] + SSIG0(wPt[31]) + wPt[30];
            wPt[47] = SSIG1(wPt[45]) + wPt[40] + SSIG0(wPt[32]) + wPt[31];
            wPt[48] = SSIG1(wPt[46]) + wPt[41] + SSIG0(wPt[33]) + wPt[32];
            wPt[49] = SSIG1(wPt[47]) + wPt[42] + SSIG0(wPt[34]) + wPt[33];
            wPt[50] = SSIG1(wPt[48]) + wPt[43] + SSIG0(wPt[35]) + wPt[34];
            wPt[51] = SSIG1(wPt[49]) + wPt[44] + SSIG0(wPt[36]) + wPt[35];
            wPt[52] = SSIG1(wPt[50]) + wPt[45] + SSIG0(wPt[37]) + wPt[36];
            wPt[53] = SSIG1(wPt[51]) + wPt[46] + SSIG0(wPt[38]) + wPt[37];
            wPt[54] = SSIG1(wPt[52]) + wPt[47] + SSIG0(wPt[39]) + wPt[38];
            wPt[55] = SSIG1(wPt[53]) + wPt[48] + SSIG0(wPt[40]) + wPt[39];
            wPt[56] = SSIG1(wPt[54]) + wPt[49] + SSIG0(wPt[41]) + wPt[40];
            wPt[57] = SSIG1(wPt[55]) + wPt[50] + SSIG0(wPt[42]) + wPt[41];
            wPt[58] = SSIG1(wPt[56]) + wPt[51] + SSIG0(wPt[43]) + wPt[42];
            wPt[59] = SSIG1(wPt[57]) + wPt[52] + SSIG0(wPt[44]) + wPt[43];
            wPt[60] = SSIG1(wPt[58]) + wPt[53] + SSIG0(wPt[45]) + wPt[44];
            wPt[61] = SSIG1(wPt[59]) + wPt[54] + SSIG0(wPt[46]) + wPt[45];
            wPt[62] = SSIG1(wPt[60]) + wPt[55] + SSIG0(wPt[47]) + wPt[46];
            wPt[63] = SSIG1(wPt[61]) + wPt[56] + SSIG0(wPt[48]) + wPt[47];

            // Now initialize hashState to compute next round, since this is a new hash
            Init(hPt);

            // We only have 1 block so there is no need for a loop.
            CompressBlock_WithWSet(hPt, wPt);
        }


        internal unsafe void CompressBlock(uint* hPt, uint* wPt)
        {
            for (int i = 16; i < w.Length; i++)
            {
                wPt[i] = SSIG1(wPt[i - 2]) + wPt[i - 7] + SSIG0(wPt[i - 15]) + wPt[i - 16];
            }

            uint a = hPt[0];
            uint b = hPt[1];
            uint c = hPt[2];
            uint d = hPt[3];
            uint e = hPt[4];
            uint f = hPt[5];
            uint g = hPt[6];
            uint h = hPt[7];

            uint temp, aa, bb, cc, dd, ee, ff, hh, gg;

            fixed (uint* kPt = &Ks[0])
            {
                for (int j = 0; j < 64;)
                {
                    temp = h + BSIG1(e) + CH(e, f, g) + kPt[j] + wPt[j];
                    ee = d + temp;
                    aa = temp + BSIG0(a) + MAJ(a, b, c);
                    j++;

                    temp = g + BSIG1(ee) + CH(ee, e, f) + kPt[j] + wPt[j];
                    ff = c + temp;
                    bb = temp + BSIG0(aa) + MAJ(aa, a, b);
                    j++;

                    temp = f + BSIG1(ff) + CH(ff, ee, e) + kPt[j] + wPt[j];
                    gg = b + temp;
                    cc = temp + BSIG0(bb) + MAJ(bb, aa, a);
                    j++;

                    temp = e + BSIG1(gg) + CH(gg, ff, ee) + kPt[j] + wPt[j];
                    hh = a + temp;
                    dd = temp + BSIG0(cc) + MAJ(cc, bb, aa);
                    j++;

                    temp = ee + BSIG1(hh) + CH(hh, gg, ff) + kPt[j] + wPt[j];
                    h = aa + temp;
                    d = temp + BSIG0(dd) + MAJ(dd, cc, bb);
                    j++;

                    temp = ff + BSIG1(h) + CH(h, hh, gg) + kPt[j] + wPt[j];
                    g = bb + temp;
                    c = temp + BSIG0(d) + MAJ(d, dd, cc);
                    j++;

                    temp = gg + BSIG1(g) + CH(g, h, hh) + kPt[j] + wPt[j];
                    f = cc + temp;
                    b = temp + BSIG0(c) + MAJ(c, d, dd);
                    j++;

                    temp = hh + BSIG1(f) + CH(f, g, h) + kPt[j] + wPt[j];
                    e = dd + temp;
                    a = temp + BSIG0(b) + MAJ(b, c, d);
                    j++;
                }
            }

            hPt[0] += a;
            hPt[1] += b;
            hPt[2] += c;
            hPt[3] += d;
            hPt[4] += e;
            hPt[5] += f;
            hPt[6] += g;
            hPt[7] += h;
        }

        // TODO: move every computation to this method instead
        internal unsafe void CompressBlock_WithWSet(uint* hPt, uint* wPt)
        {
            uint a = hPt[0];
            uint b = hPt[1];
            uint c = hPt[2];
            uint d = hPt[3];
            uint e = hPt[4];
            uint f = hPt[5];
            uint g = hPt[6];
            uint h = hPt[7];

            uint temp, aa, bb, cc, dd, ee, ff, hh, gg;

            fixed (uint* kPt = &Ks[0])
            {
                for (int j = 0; j < 64;)
                {
                    temp = h + BSIG1(e) + CH(e, f, g) + kPt[j] + wPt[j];
                    ee = d + temp;
                    aa = temp + BSIG0(a) + MAJ(a, b, c);
                    j++;

                    temp = g + BSIG1(ee) + CH(ee, e, f) + kPt[j] + wPt[j];
                    ff = c + temp;
                    bb = temp + BSIG0(aa) + MAJ(aa, a, b);
                    j++;

                    temp = f + BSIG1(ff) + CH(ff, ee, e) + kPt[j] + wPt[j];
                    gg = b + temp;
                    cc = temp + BSIG0(bb) + MAJ(bb, aa, a);
                    j++;

                    temp = e + BSIG1(gg) + CH(gg, ff, ee) + kPt[j] + wPt[j];
                    hh = a + temp;
                    dd = temp + BSIG0(cc) + MAJ(cc, bb, aa);
                    j++;

                    temp = ee + BSIG1(hh) + CH(hh, gg, ff) + kPt[j] + wPt[j];
                    h = aa + temp;
                    d = temp + BSIG0(dd) + MAJ(dd, cc, bb);
                    j++;

                    temp = ff + BSIG1(h) + CH(h, hh, gg) + kPt[j] + wPt[j];
                    g = bb + temp;
                    c = temp + BSIG0(d) + MAJ(d, dd, cc);
                    j++;

                    temp = gg + BSIG1(g) + CH(g, h, hh) + kPt[j] + wPt[j];
                    f = cc + temp;
                    b = temp + BSIG0(c) + MAJ(c, d, dd);
                    j++;

                    temp = hh + BSIG1(f) + CH(f, g, h) + kPt[j] + wPt[j];
                    e = dd + temp;
                    a = temp + BSIG0(b) + MAJ(b, c, d);
                    j++;
                }
            }

            hPt[0] += a;
            hPt[1] += b;
            hPt[2] += c;
            hPt[3] += d;
            hPt[4] += e;
            hPt[5] += f;
            hPt[6] += g;
            hPt[7] += h;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint CH(uint x, uint y, uint z)
        {
            // (x & y) ^ ((~x) & z);
            return z ^ (x & (y ^ z)); //TODO: find mathematical proof for this change
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint MAJ(uint x, uint y, uint z)
        {
            // (x & y) ^ (x & z) ^ (y & z);
            return (x & y) | (z & (x | y)); //TODO: find mathematical proof for this change
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint BSIG0(uint x)
        {
            // ROTR(x, 2) ^ ROTR(x, 13) ^ ROTR(x, 22);
            return (x >> 2 | x << 30) ^ (x >> 13 | x << 19) ^ (x >> 22 | x << 10);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint BSIG1(uint x)
        {
            // ROTR(x, 6) ^ ROTR(x, 11) ^ ROTR(x, 25);
            return (x >> 6 | x << 26) ^ (x >> 11 | x << 21) ^ (x >> 25 | x << 7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint SSIG0(uint x)
        {
            // ROTR(x, 7) ^ ROTR(x, 18) ^ (x >> 3);
            return (x >> 7 | x << 25) ^ (x >> 18 | x << 14) ^ (x >> 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint SSIG1(uint x)
        {
            // ROTR(x, 17) ^ ROTR(x, 19) ^ (x >> 10);
            return (x >> 17 | x << 15) ^ (x >> 19 | x << 13) ^ (x >> 10);
        }



        private bool isDisposed = false;

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Sha256"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                if (!(hashState is null))
                    Array.Clear(hashState, 0, hashState.Length);
                hashState = null;

                if (!(w is null))
                    Array.Clear(w, 0, w.Length);
                w = null;

                isDisposed = true;
            }
        }
    }
}
