// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// Implementation of 256-bit Secure Hash Algorithm (SHA) base on RFC-6234.
    /// Implements <see cref="IHashFunction"/>.
    /// <para/> https://tools.ietf.org/html/rfc6234
    /// </summary>
    public class Sha256 : IHashFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sha256"/>.
        /// </summary>
        /// <param name="isDouble">Determines whether the hash should be performed twice.</param>
        public Sha256(bool isDouble = false)
        {
            IsDouble = isDouble;
        }


        // TODO: retire IsDouble from both here and interface
        /// <summary>
        /// Indicates whether the hash function should be performed twice on message.
        /// For example Double SHA256 that bitcoin uses.
        /// </summary>
        public bool IsDouble { get; set; }

        /// <summary>
        /// Size of the hash result in bytes (=32 bytes).
        /// </summary>
        public virtual int HashByteSize => 32;

        /// <summary>
        /// Size of the blocks used in each round (=64 bytes).
        /// </summary>
        public int BlockByteSize => 64;


        internal uint[] hashState = new uint[8];
        internal uint[] w = new uint[64];

        private readonly uint[] Ks =
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
        public byte[] ComputeHash(byte[] data)
        {
            if (disposedValue)
                throw new ObjectDisposedException("Instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");

            Init();
            DoHash(data, data.Length);
            return GetBytes();
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
            if (disposedValue)
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


        /// <summary>
        /// Computes "Tagged Hash" specified by BIP-340 to be used in Schnorr signatures.
        /// <para/> If tage is "BIPSchnorr" => 3x arrays are expected each 32 bytes (r + pubkey + message)
        /// <para/> If tage is "BIPSchnorrDerive" => 2x arrays are expected each 32 bytes (key + message)
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="tag">The tag used in computation</param>
        /// <param name="data">A list of 32-byte long arrays</param>
        /// <returns>The computed hash</returns>
        public byte[] ComputeTaggedHash(string tag, params byte[][] data)
        {
            if (tag is null)
                throw new ArgumentNullException(nameof(tag), "Tag can not be null."); // It can be empty!
            if (data == null || data.Length == 0)
                throw new ArgumentNullException(nameof(data), "The extra data can not be null or empty.");
            if (data.Any(item => item.Length != 32))
                throw new ArgumentOutOfRangeException(nameof(data), "Each additional data must be 32 bytes.");


            if (tag == "BIPSchnorr")
            {
                if (data.Length != 3)
                    throw new ArgumentOutOfRangeException(nameof(data), "BIPSchnorr tag needs 3 data inputs.");

                return ComputeTaggedHash_BIPSchnorr(data[0], data[1], data[2]);
            }
            else if (tag == "BIPSchnorrDerive")
            {
                if (data.Length != 2)
                    throw new ArgumentOutOfRangeException(nameof(data), "BIPSchnorrDerive tag needs 2 data inputs.");

                return ComputeTaggedHash_BIPSchnorrDerive(data[0], data[1]);
            }

            byte[] tagHash = ComputeHash(Encoding.UTF8.GetBytes(tag));
            byte[] toHash = new byte[tagHash.Length + tagHash.Length + (data.Length * 32)];
            Buffer.BlockCopy(tagHash, 0, toHash, 0, 32);
            Buffer.BlockCopy(tagHash, 0, toHash, 32, 32);
            int offset = 64;
            foreach (var ba in data)
            {
                Buffer.BlockCopy(ba, 0, toHash, offset, 32);
                offset += 32;
            }
            return ComputeHash(toHash);
        }

        internal unsafe byte[] ComputeTaggedHash_BIPSchnorrDerive(byte[] kba, byte[] data)
        {
            // Total data length to be hashed is 128 ([32+32] + 32 + 32)
            fixed (byte* kPt = &kba[0], dPt = data)
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                // The first 64 bytes (1 block) is equal to SHA256("BIPSchnorrDerive") | SHA256("BIPSchnorrDerive")
                // This can be pre-computed and change the HashState's initial value
                hPt[0] = 0x1cd78ec3U;
                hPt[1] = 0xc4425f87U;
                hPt[2] = 0xb4f1a9f1U;
                hPt[3] = 0xa16abd8dU;
                hPt[4] = 0x5a6dea72U;
                hPt[5] = 0xd28469e3U;
                hPt[6] = 0x17119b2eU;
                hPt[7] = 0x7bd19a16U;

                // The second block (64 to 128) is kBa | data
                for (int i = 0, j = 0; i < 8; i++, j += 4)
                {
                    wPt[i] = (uint)((kPt[j] << 24) | (kPt[j + 1] << 16) | (kPt[j + 2] << 8) | kPt[j + 3]);
                }
                for (int i = 8, j = 0; i < 16; i++, j += 4)
                {
                    wPt[i] = (uint)((dPt[j] << 24) | (dPt[j + 1] << 16) | (dPt[j + 2] << 8) | dPt[j + 3]);
                }

                CompressBlock(hPt, wPt);

                // The third block (128 to 192) is pad
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
                wPt[15] = 1024; // len = 128*8
                wPt[16] = 0x80000000U;
                wPt[17] = 0x02800001U;
                wPt[18] = 0x00205000U;
                wPt[19] = 0x00000110U;
                wPt[20] = 0x22000800U;
                wPt[21] = 0x00aa0000U;
                wPt[22] = 0x05089942U;
                wPt[23] = 0xc0002ac0U;
                wPt[24] = 0x62080004U;
                wPt[25] = 0x1028c80aU;
                wPt[26] = 0x001a4055U;
                wPt[27] = 0x9f004823U;
                wPt[28] = 0x68ca269eU;
                wPt[29] = 0x323b15b4U;
                wPt[30] = 0x1886f73dU;
                wPt[31] = 0x5b6835a3U;
                wPt[32] = 0x37fd1798U;
                wPt[33] = 0x3311a7d2U;
                wPt[34] = 0xe8977a87U;
                wPt[35] = 0x55edccc1U;
                wPt[36] = 0x26785e65U;
                wPt[37] = 0x1c1a75cdU;
                wPt[38] = 0x1898add6U;
                wPt[39] = 0x70d975edU;
                wPt[40] = 0xfc995de5U;
                wPt[41] = 0xc72d9f47U;
                wPt[42] = 0x225062f2U;
                wPt[43] = 0xfa62c148U;
                wPt[44] = 0x6d6275f8U;
                wPt[45] = 0x4876537fU;
                wPt[46] = 0x3e6bd0afU;
                wPt[47] = 0xaf3a394cU;
                wPt[48] = 0x5d69345cU;
                wPt[49] = 0x7d685338U;
                wPt[50] = 0x9ad3729dU;
                wPt[51] = 0xc04f60b4U;
                wPt[52] = 0x4af2ba27U;
                wPt[53] = 0x3b5ad539U;
                wPt[54] = 0x5b9a980bU;
                wPt[55] = 0x818b7cddU;
                wPt[56] = 0x89cdea52U;
                wPt[57] = 0x2c88481eU;
                wPt[58] = 0x69cbcd7eU;
                wPt[59] = 0xd265fe42U;
                wPt[60] = 0xab09cb34U;
                wPt[61] = 0x9288f7b9U;
                wPt[62] = 0x9fb768b8U;
                wPt[63] = 0x9c18607fU;

                CompressBlock_WithWSet(hPt, wPt);

                return GetBytes(hPt);
            }
        }

        internal unsafe byte[] ComputeTaggedHash_BIPSchnorr(byte[] rba, byte[] pba, byte[] data)
        {
            // Total data length to be hashed is 160 ([32+32] + 32 + 32 + 32)
            fixed (byte* rPt = &rba[0], pPt = &pba[0], dPt = data)
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                // The first 64 bytes (1 block) is equal to SHA256("BIPSchnorr") | SHA256("BIPSchnorr")
                // This can be pre-computed and change the HashState's initial value
                hPt[0] = 0x048d9a59U;
                hPt[1] = 0xfe39fb05U;
                hPt[2] = 0x28479648U;
                hPt[3] = 0xe4a660f9U;
                hPt[4] = 0x814b9e66U;
                hPt[5] = 0x0469e801U;
                hPt[6] = 0x83909280U;
                hPt[7] = 0xb329e454U;

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
        public byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            return ComputeHash(buffer.SubArray(offset, count));
        }



        internal virtual unsafe void Init()
        {
            fixed (uint* hPt = &hashState[0])
            {
                Init(hPt);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual unsafe void Init(uint* hPt)
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
            byte[] res = new byte[HashByteSize];
            fixed (byte* bPt = &res[0])
            {
                for (int i = 0, j = 0; i < res.Length; i += 4, j++)
                {
                    bPt[i] = (byte)(hPt[j] >> 24);
                    bPt[i + 1] = (byte)(hPt[j] >> 16);
                    bPt[i + 2] = (byte)(hPt[j] >> 8);
                    bPt[i + 3] = (byte)hPt[j];
                }
            }
            return res;
        }


        internal unsafe void DoHash(byte[] data, int len)
        {
            // If data.Length == 0 => &data[0] will throw an exception
            fixed (byte* dPt = data)
            fixed (uint* hPt = &hashState[0], wPt = &w[0])
            {
                CompressData(dPt, data.Length, len, hPt, wPt);

                if (IsDouble)
                {
                    ComputeSecondHash(hPt, wPt);
                }
            }
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
        internal virtual unsafe void ComputeSecondHash(uint* hPt, uint* wPt)
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



        private bool disposedValue = false;

        /// <summary>
        /// Releases the resources used by the <see cref="Sha256"/> class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (!(hashState is null))
                        Array.Clear(hashState, 0, hashState.Length);
                    hashState = null;

                    if (!(w is null))
                        Array.Clear(w, 0, w.Length);
                    w = null;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Sha256"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
