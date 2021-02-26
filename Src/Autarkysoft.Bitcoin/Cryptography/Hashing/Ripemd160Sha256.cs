// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// Performs RIPEMD160 hash function on the result of SHA256 also known as HASH160
    /// <para/> This is more optimized and a lot faster than using .Net functions individually 
    /// specially when computing hash for small byte arrays such as 33 bytes (bitcoin public keys used in P2PKH scripts)
    /// <para/>Implements <see cref="IDisposable"/>
    /// </summary>
    public sealed class Ripemd160Sha256 : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ripemd160Sha256"/>.
        /// </summary>
        public Ripemd160Sha256()
        {
            rip = new Ripemd160();
            sha = new Sha256();
        }


        /// <summary>
        /// Size of the hash result in bytes.
        /// </summary>
        public const int HashByteSize = 20;


        private Ripemd160 rip;
        private Sha256 sha;


        /// <summary>
        /// Computes the hash value for the specified byte array 
        /// by calculating its SHA256 hash first then calculating RIPEMD160 hash of that hash.
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


            // Since HASH160 in 99% of cases is performed on a public key which is
            // 33 byte compressed (most of the times) or 65 bytes uncompressed, 2 special cases are added:
            fixed (byte* dPt = data)
            fixed (uint* rip_blkPt = &rip.block[0], rip_hPt = &rip.hashState[0], sh_wPt = &sha.w[0])
            {
                // Step 1: compute SHA256 of data then copy the result (HashState) into RIPEMD160 block.
                // To skip the copy we just pass RIPEMD160 block as HashState of SHA256
                sha.Init(rip_blkPt);

                // Depending on the data length SHA256 can be different but the rest is similar
                if (data.Length == 33)
                {
                    int dIndex = 0;
                    for (int i = 0; i < 8; i++, dIndex += 4)
                    {
                        sh_wPt[i] = (uint)((dPt[dIndex] << 24) |
                                           (dPt[dIndex + 1] << 16) |
                                           (dPt[dIndex + 2] << 8) |
                                            dPt[dIndex + 3]);
                    }
                    sh_wPt[8] = (uint)((dPt[dIndex] << 24) | 0b00000000_10000000_00000000_00000000U);
                    // Note that the following items MUST be set to keep the class reusable
                    sh_wPt[9] = 0;
                    sh_wPt[10] = 0;
                    sh_wPt[11] = 0;
                    sh_wPt[12] = 0;
                    sh_wPt[13] = 0;
                    sh_wPt[14] = 0;
                    sh_wPt[15] = 264; // 33*8 = 264

                    sh_wPt[16] = SSIG0(sh_wPt[1]) + sh_wPt[0];
                    sh_wPt[17] = 10813440 + 0 + SSIG0(sh_wPt[2]) + sh_wPt[1];
                    sh_wPt[18] = SSIG1(sh_wPt[16]) + SSIG0(sh_wPt[3]) + sh_wPt[2];
                    sh_wPt[19] = SSIG1(sh_wPt[17]) + SSIG0(sh_wPt[4]) + sh_wPt[3];
                    sh_wPt[20] = SSIG1(sh_wPt[18]) + SSIG0(sh_wPt[5]) + sh_wPt[4];
                    sh_wPt[21] = SSIG1(sh_wPt[19]) + SSIG0(sh_wPt[6]) + sh_wPt[5];
                    sh_wPt[22] = SSIG1(sh_wPt[20]) + 264 + SSIG0(sh_wPt[7]) + sh_wPt[6];
                    sh_wPt[23] = SSIG1(sh_wPt[21]) + sh_wPt[16] + SSIG0(sh_wPt[8]) + sh_wPt[7];
                    sh_wPt[24] = SSIG1(sh_wPt[22]) + sh_wPt[17] + sh_wPt[8];
                    sh_wPt[25] = SSIG1(sh_wPt[23]) + sh_wPt[18];
                    sh_wPt[26] = SSIG1(sh_wPt[24]) + sh_wPt[19];
                    sh_wPt[27] = SSIG1(sh_wPt[25]) + sh_wPt[20];
                    sh_wPt[28] = SSIG1(sh_wPt[26]) + sh_wPt[21];
                    sh_wPt[29] = SSIG1(sh_wPt[27]) + sh_wPt[22];
                    sh_wPt[30] = SSIG1(sh_wPt[28]) + sh_wPt[23] + 272760867;
                    sh_wPt[31] = SSIG1(sh_wPt[29]) + sh_wPt[24] + SSIG0(sh_wPt[16]) + 264;
                    sh_wPt[32] = SSIG1(sh_wPt[30]) + sh_wPt[25] + SSIG0(sh_wPt[17]) + sh_wPt[16];
                    sh_wPt[33] = SSIG1(sh_wPt[31]) + sh_wPt[26] + SSIG0(sh_wPt[18]) + sh_wPt[17];
                    sh_wPt[34] = SSIG1(sh_wPt[32]) + sh_wPt[27] + SSIG0(sh_wPt[19]) + sh_wPt[18];
                    sh_wPt[35] = SSIG1(sh_wPt[33]) + sh_wPt[28] + SSIG0(sh_wPt[20]) + sh_wPt[19];
                    sh_wPt[36] = SSIG1(sh_wPt[34]) + sh_wPt[29] + SSIG0(sh_wPt[21]) + sh_wPt[20];
                    sh_wPt[37] = SSIG1(sh_wPt[35]) + sh_wPt[30] + SSIG0(sh_wPt[22]) + sh_wPt[21];
                    sh_wPt[38] = SSIG1(sh_wPt[36]) + sh_wPt[31] + SSIG0(sh_wPt[23]) + sh_wPt[22];
                    sh_wPt[39] = SSIG1(sh_wPt[37]) + sh_wPt[32] + SSIG0(sh_wPt[24]) + sh_wPt[23];
                    sh_wPt[40] = SSIG1(sh_wPt[38]) + sh_wPt[33] + SSIG0(sh_wPt[25]) + sh_wPt[24];
                    sh_wPt[41] = SSIG1(sh_wPt[39]) + sh_wPt[34] + SSIG0(sh_wPt[26]) + sh_wPt[25];
                    sh_wPt[42] = SSIG1(sh_wPt[40]) + sh_wPt[35] + SSIG0(sh_wPt[27]) + sh_wPt[26];
                    sh_wPt[43] = SSIG1(sh_wPt[41]) + sh_wPt[36] + SSIG0(sh_wPt[28]) + sh_wPt[27];
                    sh_wPt[44] = SSIG1(sh_wPt[42]) + sh_wPt[37] + SSIG0(sh_wPt[29]) + sh_wPt[28];
                    sh_wPt[45] = SSIG1(sh_wPt[43]) + sh_wPt[38] + SSIG0(sh_wPt[30]) + sh_wPt[29];
                    sh_wPt[46] = SSIG1(sh_wPt[44]) + sh_wPt[39] + SSIG0(sh_wPt[31]) + sh_wPt[30];
                    sh_wPt[47] = SSIG1(sh_wPt[45]) + sh_wPt[40] + SSIG0(sh_wPt[32]) + sh_wPt[31];
                    sh_wPt[48] = SSIG1(sh_wPt[46]) + sh_wPt[41] + SSIG0(sh_wPt[33]) + sh_wPt[32];
                    sh_wPt[49] = SSIG1(sh_wPt[47]) + sh_wPt[42] + SSIG0(sh_wPt[34]) + sh_wPt[33];
                    sh_wPt[50] = SSIG1(sh_wPt[48]) + sh_wPt[43] + SSIG0(sh_wPt[35]) + sh_wPt[34];
                    sh_wPt[51] = SSIG1(sh_wPt[49]) + sh_wPt[44] + SSIG0(sh_wPt[36]) + sh_wPt[35];
                    sh_wPt[52] = SSIG1(sh_wPt[50]) + sh_wPt[45] + SSIG0(sh_wPt[37]) + sh_wPt[36];
                    sh_wPt[53] = SSIG1(sh_wPt[51]) + sh_wPt[46] + SSIG0(sh_wPt[38]) + sh_wPt[37];
                    sh_wPt[54] = SSIG1(sh_wPt[52]) + sh_wPt[47] + SSIG0(sh_wPt[39]) + sh_wPt[38];
                    sh_wPt[55] = SSIG1(sh_wPt[53]) + sh_wPt[48] + SSIG0(sh_wPt[40]) + sh_wPt[39];
                    sh_wPt[56] = SSIG1(sh_wPt[54]) + sh_wPt[49] + SSIG0(sh_wPt[41]) + sh_wPt[40];
                    sh_wPt[57] = SSIG1(sh_wPt[55]) + sh_wPt[50] + SSIG0(sh_wPt[42]) + sh_wPt[41];
                    sh_wPt[58] = SSIG1(sh_wPt[56]) + sh_wPt[51] + SSIG0(sh_wPt[43]) + sh_wPt[42];
                    sh_wPt[59] = SSIG1(sh_wPt[57]) + sh_wPt[52] + SSIG0(sh_wPt[44]) + sh_wPt[43];
                    sh_wPt[60] = SSIG1(sh_wPt[58]) + sh_wPt[53] + SSIG0(sh_wPt[45]) + sh_wPt[44];
                    sh_wPt[61] = SSIG1(sh_wPt[59]) + sh_wPt[54] + SSIG0(sh_wPt[46]) + sh_wPt[45];
                    sh_wPt[62] = SSIG1(sh_wPt[60]) + sh_wPt[55] + SSIG0(sh_wPt[47]) + sh_wPt[46];
                    sh_wPt[63] = SSIG1(sh_wPt[61]) + sh_wPt[56] + SSIG0(sh_wPt[48]) + sh_wPt[47];

                    sha.CompressBlock_WithWSet(hPt: rip_blkPt, wPt: sh_wPt);
                }
                else if (data.Length == 65)
                {
                    // There are two blocks in SHA256: first 64 bytes and the remaining 1 byte with pads
                    int dIndex = 0;
                    for (int i = 0; i < 16; i++, dIndex += 4)
                    {
                        sh_wPt[i] = (uint)((dPt[dIndex] << 24) |
                                           (dPt[dIndex + 1] << 16) |
                                           (dPt[dIndex + 2] << 8) |
                                           dPt[dIndex + 3]);
                    }
                    sha.CompressBlock(rip_blkPt, sh_wPt);

                    sh_wPt[0] = (uint)((dPt[dIndex] << 24) | 0b00000000_10000000_00000000_00000000U);
                    // Same as above, these MUST be set
                    sh_wPt[1] = 0;
                    sh_wPt[2] = 0;
                    sh_wPt[3] = 0;
                    sh_wPt[4] = 0;
                    sh_wPt[5] = 0;
                    sh_wPt[6] = 0;
                    sh_wPt[7] = 0;
                    sh_wPt[8] = 0;
                    sh_wPt[9] = 0;
                    sh_wPt[10] = 0;
                    sh_wPt[11] = 0;
                    sh_wPt[12] = 0;
                    sh_wPt[13] = 0;
                    sh_wPt[14] = 0;
                    sh_wPt[15] = 520; // 65*8 = 520

                    sh_wPt[16] = sh_wPt[0];
                    sh_wPt[17] = 21299200;
                    sh_wPt[18] = SSIG1(sh_wPt[16]);
                    sh_wPt[19] = SSIG1(sh_wPt[17]);
                    sh_wPt[20] = SSIG1(sh_wPt[18]);
                    sh_wPt[21] = SSIG1(sh_wPt[19]);
                    sh_wPt[22] = SSIG1(sh_wPt[20]) + 520;
                    sh_wPt[23] = SSIG1(sh_wPt[21]) + sh_wPt[16];
                    sh_wPt[24] = SSIG1(sh_wPt[22]) + sh_wPt[17];
                    sh_wPt[25] = SSIG1(sh_wPt[23]) + sh_wPt[18];
                    sh_wPt[26] = SSIG1(sh_wPt[24]) + sh_wPt[19];
                    sh_wPt[27] = SSIG1(sh_wPt[25]) + sh_wPt[20];
                    sh_wPt[28] = SSIG1(sh_wPt[26]) + sh_wPt[21];
                    sh_wPt[29] = SSIG1(sh_wPt[27]) + sh_wPt[22];
                    sh_wPt[30] = SSIG1(sh_wPt[28]) + sh_wPt[23] + 276955205;
                    sh_wPt[31] = SSIG1(sh_wPt[29]) + sh_wPt[24] + SSIG0(sh_wPt[16]) + 520;
                    sh_wPt[32] = SSIG1(sh_wPt[30]) + sh_wPt[25] + SSIG0(sh_wPt[17]) + sh_wPt[16];
                    sh_wPt[33] = SSIG1(sh_wPt[31]) + sh_wPt[26] + SSIG0(sh_wPt[18]) + sh_wPt[17];
                    sh_wPt[34] = SSIG1(sh_wPt[32]) + sh_wPt[27] + SSIG0(sh_wPt[19]) + sh_wPt[18];
                    sh_wPt[35] = SSIG1(sh_wPt[33]) + sh_wPt[28] + SSIG0(sh_wPt[20]) + sh_wPt[19];
                    sh_wPt[36] = SSIG1(sh_wPt[34]) + sh_wPt[29] + SSIG0(sh_wPt[21]) + sh_wPt[20];
                    sh_wPt[37] = SSIG1(sh_wPt[35]) + sh_wPt[30] + SSIG0(sh_wPt[22]) + sh_wPt[21];
                    sh_wPt[38] = SSIG1(sh_wPt[36]) + sh_wPt[31] + SSIG0(sh_wPt[23]) + sh_wPt[22];
                    sh_wPt[39] = SSIG1(sh_wPt[37]) + sh_wPt[32] + SSIG0(sh_wPt[24]) + sh_wPt[23];
                    sh_wPt[40] = SSIG1(sh_wPt[38]) + sh_wPt[33] + SSIG0(sh_wPt[25]) + sh_wPt[24];
                    sh_wPt[41] = SSIG1(sh_wPt[39]) + sh_wPt[34] + SSIG0(sh_wPt[26]) + sh_wPt[25];
                    sh_wPt[42] = SSIG1(sh_wPt[40]) + sh_wPt[35] + SSIG0(sh_wPt[27]) + sh_wPt[26];
                    sh_wPt[43] = SSIG1(sh_wPt[41]) + sh_wPt[36] + SSIG0(sh_wPt[28]) + sh_wPt[27];
                    sh_wPt[44] = SSIG1(sh_wPt[42]) + sh_wPt[37] + SSIG0(sh_wPt[29]) + sh_wPt[28];
                    sh_wPt[45] = SSIG1(sh_wPt[43]) + sh_wPt[38] + SSIG0(sh_wPt[30]) + sh_wPt[29];
                    sh_wPt[46] = SSIG1(sh_wPt[44]) + sh_wPt[39] + SSIG0(sh_wPt[31]) + sh_wPt[30];
                    sh_wPt[47] = SSIG1(sh_wPt[45]) + sh_wPt[40] + SSIG0(sh_wPt[32]) + sh_wPt[31];
                    sh_wPt[48] = SSIG1(sh_wPt[46]) + sh_wPt[41] + SSIG0(sh_wPt[33]) + sh_wPt[32];
                    sh_wPt[49] = SSIG1(sh_wPt[47]) + sh_wPt[42] + SSIG0(sh_wPt[34]) + sh_wPt[33];
                    sh_wPt[50] = SSIG1(sh_wPt[48]) + sh_wPt[43] + SSIG0(sh_wPt[35]) + sh_wPt[34];
                    sh_wPt[51] = SSIG1(sh_wPt[49]) + sh_wPt[44] + SSIG0(sh_wPt[36]) + sh_wPt[35];
                    sh_wPt[52] = SSIG1(sh_wPt[50]) + sh_wPt[45] + SSIG0(sh_wPt[37]) + sh_wPt[36];
                    sh_wPt[53] = SSIG1(sh_wPt[51]) + sh_wPt[46] + SSIG0(sh_wPt[38]) + sh_wPt[37];
                    sh_wPt[54] = SSIG1(sh_wPt[52]) + sh_wPt[47] + SSIG0(sh_wPt[39]) + sh_wPt[38];
                    sh_wPt[55] = SSIG1(sh_wPt[53]) + sh_wPt[48] + SSIG0(sh_wPt[40]) + sh_wPt[39];
                    sh_wPt[56] = SSIG1(sh_wPt[54]) + sh_wPt[49] + SSIG0(sh_wPt[41]) + sh_wPt[40];
                    sh_wPt[57] = SSIG1(sh_wPt[55]) + sh_wPt[50] + SSIG0(sh_wPt[42]) + sh_wPt[41];
                    sh_wPt[58] = SSIG1(sh_wPt[56]) + sh_wPt[51] + SSIG0(sh_wPt[43]) + sh_wPt[42];
                    sh_wPt[59] = SSIG1(sh_wPt[57]) + sh_wPt[52] + SSIG0(sh_wPt[44]) + sh_wPt[43];
                    sh_wPt[60] = SSIG1(sh_wPt[58]) + sh_wPt[53] + SSIG0(sh_wPt[45]) + sh_wPt[44];
                    sh_wPt[61] = SSIG1(sh_wPt[59]) + sh_wPt[54] + SSIG0(sh_wPt[46]) + sh_wPt[45];
                    sh_wPt[62] = SSIG1(sh_wPt[60]) + sh_wPt[55] + SSIG0(sh_wPt[47]) + sh_wPt[46];
                    sh_wPt[63] = SSIG1(sh_wPt[61]) + sh_wPt[56] + SSIG0(sh_wPt[48]) + sh_wPt[47];

                    sha.CompressBlock_WithWSet(rip_blkPt, sh_wPt);
                }
                else // Any length but 33 or 65
                {
                    // Perform SHA256:
                    // Init() is already called using rip_block
                    sha.CompressData(dPt, data.Length, data.Length, hPt: rip_blkPt, wPt: sh_wPt);
                }

                // SHA256 compression is over and the result is already inside RIPEMD160 Block
                // But SHA256 endianness is reverse of RIPEMD160, so we have to do an endian swap

                // Only 32 byte or 8 uint items coming from SHA256
                for (int i = 0; i < 8; i++)
                {
                    // RIPEMD160 uses little-endian while SHA256 uses big-endian
                    rip_blkPt[i] =
                        (rip_blkPt[i] >> 24) | (rip_blkPt[i] << 24) |                       // Swap byte 1 and 4
                        ((rip_blkPt[i] >> 8) & 0xff00) | ((rip_blkPt[i] << 8) & 0xff0000);  // Swap byte 2 and 3
                }
                rip_blkPt[8] = 0b00000000_00000000_00000000_10000000U;
                rip_blkPt[14] = 256;
                // rip_blkPt[15] = 0;
                // There is no need to set other items in block (like 13, 12,...)
                // because they are not changed and they are always zero

                rip.Init(rip_hPt);
                rip.CompressBlock(rip_blkPt, rip_hPt);

                return rip.GetBytes(rip_hPt);
            }
        }

        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array
        /// by calculating its SHA256 hash first then calculating RIPEMD160 hash of that hash.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="buffer">The byte array to compute hash for</param>
        /// <param name="offset">The offset into the byte array from which to begin using data.</param>
        /// <param name="count">The number of bytes in the array to use as data.</param>
        /// <returns>The computed hash</returns>
        public byte[] ComputeHash(byte[] buffer, int offset, int count) => ComputeHash(buffer.SubArray(offset, count));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint SSIG0(uint x) => (x >> 7 | x << 25) ^ (x >> 18 | x << 14) ^ (x >> 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint SSIG1(uint x) => (x >> 17 | x << 15) ^ (x >> 19 | x << 13) ^ (x >> 10);



        private bool isDisposed = false;

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Ripemd160Sha256"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                if (!(rip is null))
                    rip.Dispose();
                rip = null;

                if (!(sha is null))
                    sha.Dispose();
                sha = null;

                isDisposed = true;
            }
        }
    }
}
