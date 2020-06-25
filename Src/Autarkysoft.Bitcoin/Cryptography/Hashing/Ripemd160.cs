// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// Implementation of RIPEMD160.
    /// Implements <see cref="IHashFunction"/>.
    /// </summary>
    public class Ripemd160 : IHashFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ripemd160"/>.
        /// </summary>
        /// <param name="isDouble">Determines whether the hash should be performed twice.</param>
        public Ripemd160(bool isDouble = false)
        {
            IsDouble = isDouble;
        }



        /// <summary>
        /// Indicates whether the hash function should be performed twice on message.
        /// </summary>
        public bool IsDouble { get; set; }

        /// <summary>
        /// Size of the hash result in bytes.
        /// </summary>
        public int HashByteSize => 20;

        /// <summary>
        /// Size of the blocks used in each round.
        /// </summary>
        public int BlockByteSize => 64;

        internal uint[] block = new uint[16];
        internal uint[] hashState = new uint[5];



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

            DoHash(data);

            return GetBytes();
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

        internal unsafe byte[] GetBytes()
        {
            fixed (uint* hPt = &hashState[0])
                return GetBytes(hPt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe byte[] GetBytes(uint* hPt)
        {
            return new byte[20]
            {
                (byte)hPt[0], (byte)(hPt[0] >> 8), (byte)(hPt[0] >> 16), (byte)(hPt[0] >> 24),
                (byte)hPt[1], (byte)(hPt[1] >> 8), (byte)(hPt[1] >> 16), (byte)(hPt[1] >> 24),
                (byte)hPt[2], (byte)(hPt[2] >> 8), (byte)(hPt[2] >> 16), (byte)(hPt[2] >> 24),
                (byte)hPt[3], (byte)(hPt[3] >> 8), (byte)(hPt[3] >> 16), (byte)(hPt[3] >> 24),
                (byte)hPt[4], (byte)(hPt[4] >> 8), (byte)(hPt[4] >> 16), (byte)(hPt[4] >> 24),
            };
        }


        /* 
         * Since RIPEMD160 is only performed on small byte arrays (like result of SHA256=32 bytes) unlike SHA256 implemenetation
         * here we simply pad the data first then loop through that. 
         * If data sizes increase this method will be slow due to Buffer.BlockCopy being slower
         */
        internal unsafe void DoHash(byte[] data)
        {
            // According to RIPEMD160 paper (https://homes.esat.kuleuven.be/~bosselae/ripemd160/pdf/AB-9601/AB-9601.pdf)
            // padding is identical to MD4 (https://tools.ietf.org/html/rfc1320)

            // data is always padded with 8 bytes (its length), the total length has to be divisible by 64
            // since each block is 64 bytes (16x uint).
            int padLen = 64 - ((data.Length + 8) & 63);

            byte[] dataToHash = new byte[data.Length + 8 + padLen];
            Buffer.BlockCopy(data, 0, dataToHash, 0, data.Length);

            fixed (byte* dPt = dataToHash)
            {
                /*
                 * 
                 * Maximum of `msgLen` is (int.MaxValue * 8) = 17179869176
                 * = 00000000_00000000_00000000_00000011_11111111_11111111_11111111_11111000
                 * in other words the first 3 bytes are always zero
                */
                //dPt[index--] = (byte)(msgLen >> 56); 
                //dPt[index--] = (byte)(msgLen >> 48);
                //dPt[index--] = (byte)(msgLen >> 40);

                int index = dataToHash.Length - 4; // -3 is added to cover the 3 index-- which were skipped above
                long msgLen = (long)data.Length << 3; // *8
                // Message length is added at the end in little-endian order
                dPt[index--] = (byte)(msgLen >> 32);
                dPt[index--] = (byte)(msgLen >> 24);
                dPt[index--] = (byte)(msgLen >> 16);
                dPt[index--] = (byte)(msgLen >> 8);
                dPt[index--] = (byte)msgLen;

                dPt[data.Length] = 0b1000_0000;

                fixed (uint* xPt = &block[0], hPt = &hashState[0])
                {
                    int dIndex = 0;
                    while (dIndex < dataToHash.Length)
                    {
                        for (int i = 0; i < block.Length; i++, dIndex += 4)
                        {
                            xPt[i] = (uint)(dPt[dIndex] | (dPt[dIndex + 1] << 8) | (dPt[dIndex + 2] << 16) | (dPt[dIndex + 3] << 24));
                        }
                        CompressBlock(xPt, hPt);
                    }
                }
            }

            if (IsDouble)
            {
                DoSecondHash();
            }
        }

        internal virtual unsafe void DoSecondHash()
        {
            // TODO: change this like Sha256, etc.
            block = new uint[16]
            {
                hashState[0], hashState[1], hashState[2], hashState[3], hashState[4], // 5*4=20 byte
                0b00000000_00000000_00000000_10000000U, 0, 0, 0, 0, 0, 0, 0, 0, // 9*4=36 byte pad1 starting with 1
                160, 0 // 2*4=8 byte pad2 equal to length (20byte=160bit)
            };

            Init();

            fixed (uint* xPt = &block[0], hPt = &hashState[0])
            {
                // We only have 1 block so there is no need for a loop.
                CompressBlock(xPt, hPt);
            }
        }

        internal unsafe void Init()
        {
            fixed (uint* hPt = &hashState[0])
                Init(hPt);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void Init(uint* hPt)
        {
            hPt[0] = 0x67452301U;
            hPt[1] = 0xefcdab89U;
            hPt[2] = 0x98badcfeU;
            hPt[3] = 0x10325476U;
            hPt[4] = 0xc3d2e1f0U;
        }


        internal unsafe void CompressBlock(uint* xPt, uint* hPt)
        {
            uint aa = hPt[0];
            uint bb = hPt[1];
            uint cc = hPt[2];
            uint dd = hPt[3];
            uint ee = hPt[4];

            uint aaa = aa;
            uint bbb = bb;
            uint ccc = cc;
            uint ddd = dd;
            uint eee = ee;


            /* round 1 */
            aa += (bb ^ cc ^ dd) + xPt[0];
            aa = ((aa << 11) | (aa >> 21)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += (aa ^ bb ^ cc) + xPt[1];
            ee = ((ee << 14) | (ee >> 18)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += (ee ^ aa ^ bb) + xPt[2];
            dd = ((dd << 15) | (dd >> 17)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += (dd ^ ee ^ aa) + xPt[3];
            cc = ((cc << 12) | (cc >> 20)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += (cc ^ dd ^ ee) + xPt[4];
            bb = ((bb << 5) | (bb >> 27)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += (bb ^ cc ^ dd) + xPt[5];
            aa = ((aa << 8) | (aa >> 24)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += (aa ^ bb ^ cc) + xPt[6];
            ee = ((ee << 7) | (ee >> 25)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += (ee ^ aa ^ bb) + xPt[7];
            dd = ((dd << 9) | (dd >> 23)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += (dd ^ ee ^ aa) + xPt[8];
            cc = ((cc << 11) | (cc >> 21)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += (cc ^ dd ^ ee) + xPt[9];
            bb = ((bb << 13) | (bb >> 19)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += (bb ^ cc ^ dd) + xPt[10];
            aa = ((aa << 14) | (aa >> 18)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += (aa ^ bb ^ cc) + xPt[11];
            ee = ((ee << 15) | (ee >> 17)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += (ee ^ aa ^ bb) + xPt[12];
            dd = ((dd << 6) | (dd >> 26)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += (dd ^ ee ^ aa) + xPt[13];
            cc = ((cc << 7) | (cc >> 25)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += (cc ^ dd ^ ee) + xPt[14];
            bb = ((bb << 9) | (bb >> 23)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += (bb ^ cc ^ dd) + xPt[15];
            aa = ((aa << 8) | (aa >> 24)) + ee;
            cc = (cc << 10) | (cc >> 22);

            /* round 2 */
            ee += ((aa & bb) | ((~aa) & cc)) + xPt[7] + 0x5a827999U;
            ee = ((ee << 7) | (ee >> 25)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee & aa) | ((~ee) & bb)) + xPt[4] + 0x5a827999U;
            dd = ((dd << 6) | (dd >> 26)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd & ee) | ((~dd) & aa)) + xPt[13] + 0x5a827999U;
            cc = ((cc << 8) | (cc >> 24)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc & dd) | ((~cc) & ee)) + xPt[1] + 0x5a827999U;
            bb = ((bb << 13) | (bb >> 19)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb & cc) | ((~bb) & dd)) + xPt[10] + 0x5a827999U;
            aa = ((aa << 11) | (aa >> 21)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa & bb) | ((~aa) & cc)) + xPt[6] + 0x5a827999U;
            ee = ((ee << 9) | (ee >> 23)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee & aa) | ((~ee) & bb)) + xPt[15] + 0x5a827999U;
            dd = ((dd << 7) | (dd >> 25)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd & ee) | ((~dd) & aa)) + xPt[3] + 0x5a827999U;
            cc = ((cc << 15) | (cc >> 17)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc & dd) | ((~cc) & ee)) + xPt[12] + 0x5a827999U;
            bb = ((bb << 7) | (bb >> 25)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb & cc) | ((~bb) & dd)) + xPt[0] + 0x5a827999U;
            aa = ((aa << 12) | (aa >> 20)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa & bb) | ((~aa) & cc)) + xPt[9] + 0x5a827999U;
            ee = ((ee << 15) | (ee >> 17)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee & aa) | ((~ee) & bb)) + xPt[5] + 0x5a827999U;
            dd = ((dd << 9) | (dd >> 23)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd & ee) | ((~dd) & aa)) + xPt[2] + 0x5a827999U;
            cc = ((cc << 11) | (cc >> 21)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc & dd) | ((~cc) & ee)) + xPt[14] + 0x5a827999U;
            bb = ((bb << 7) | (bb >> 25)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb & cc) | ((~bb) & dd)) + xPt[11] + 0x5a827999U;
            aa = ((aa << 13) | (aa >> 19)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa & bb) | ((~aa) & cc)) + xPt[8] + 0x5a827999U;
            ee = ((ee << 12) | (ee >> 20)) + dd;
            bb = (bb << 10) | (bb >> 22);

            /* round 3 */
            dd += ((ee | (~aa)) ^ bb) + xPt[3] + 0x6ed9eba1U;
            dd = ((dd << 11) | (dd >> 21)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd | (~ee)) ^ aa) + xPt[10] + 0x6ed9eba1U;
            cc = ((cc << 13) | (cc >> 19)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc | (~dd)) ^ ee) + xPt[14] + 0x6ed9eba1U;
            bb = ((bb << 6) | (bb >> 26)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb | (~cc)) ^ dd) + xPt[4] + 0x6ed9eba1U;
            aa = ((aa << 7) | (aa >> 25)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa | (~bb)) ^ cc) + xPt[9] + 0x6ed9eba1U;
            ee = ((ee << 14) | (ee >> 18)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee | (~aa)) ^ bb) + xPt[15] + 0x6ed9eba1U;
            dd = ((dd << 9) | (dd >> 23)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd | (~ee)) ^ aa) + xPt[8] + 0x6ed9eba1U;
            cc = ((cc << 13) | (cc >> 19)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc | (~dd)) ^ ee) + xPt[1] + 0x6ed9eba1U;
            bb = ((bb << 15) | (bb >> 17)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb | (~cc)) ^ dd) + xPt[2] + 0x6ed9eba1U;
            aa = ((aa << 14) | (aa >> 18)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa | (~bb)) ^ cc) + xPt[7] + 0x6ed9eba1U;
            ee = ((ee << 8) | (ee >> 24)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee | (~aa)) ^ bb) + xPt[0] + 0x6ed9eba1U;
            dd = ((dd << 13) | (dd >> 19)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd | (~ee)) ^ aa) + xPt[6] + 0x6ed9eba1U;
            cc = ((cc << 6) | (cc >> 26)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc | (~dd)) ^ ee) + xPt[13] + 0x6ed9eba1U;
            bb = ((bb << 5) | (bb >> 27)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb | (~cc)) ^ dd) + xPt[11] + 0x6ed9eba1U;
            aa = ((aa << 12) | (aa >> 20)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa | (~bb)) ^ cc) + xPt[5] + 0x6ed9eba1U;
            ee = ((ee << 7) | (ee >> 25)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee | (~aa)) ^ bb) + xPt[12] + 0x6ed9eba1U;
            dd = ((dd << 5) | (dd >> 27)) + cc;
            aa = (aa << 10) | (aa >> 22);

            /* round 4 */
            cc += ((dd & aa) | (ee & (~aa))) + xPt[1] + 0x8f1bbcdcU;
            cc = ((cc << 11) | (cc >> 21)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc & ee) | (dd & (~ee))) + xPt[9] + 0x8f1bbcdcU;
            bb = ((bb << 12) | (bb >> 20)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb & dd) | (cc & (~dd))) + xPt[11] + 0x8f1bbcdcU;
            aa = ((aa << 14) | (aa >> 18)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa & cc) | (bb & (~cc))) + xPt[10] + 0x8f1bbcdcU;
            ee = ((ee << 15) | (ee >> 17)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee & bb) | (aa & (~bb))) + xPt[0] + 0x8f1bbcdcU;
            dd = ((dd << 14) | (dd >> 18)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd & aa) | (ee & (~aa))) + xPt[8] + 0x8f1bbcdcU;
            cc = ((cc << 15) | (cc >> 17)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc & ee) | (dd & (~ee))) + xPt[12] + 0x8f1bbcdcU;
            bb = ((bb << 9) | (bb >> 23)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb & dd) | (cc & (~dd))) + xPt[4] + 0x8f1bbcdcU;
            aa = ((aa << 8) | (aa >> 24)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa & cc) | (bb & (~cc))) + xPt[13] + 0x8f1bbcdcU;
            ee = ((ee << 9) | (ee >> 23)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee & bb) | (aa & (~bb))) + xPt[3] + 0x8f1bbcdcU;
            dd = ((dd << 14) | (dd >> 18)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd & aa) | (ee & (~aa))) + xPt[7] + 0x8f1bbcdcU;
            cc = ((cc << 5) | (cc >> 27)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += ((cc & ee) | (dd & (~ee))) + xPt[15] + 0x8f1bbcdcU;
            bb = ((bb << 6) | (bb >> 26)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += ((bb & dd) | (cc & (~dd))) + xPt[14] + 0x8f1bbcdcU;
            aa = ((aa << 8) | (aa >> 24)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += ((aa & cc) | (bb & (~cc))) + xPt[5] + 0x8f1bbcdcU;
            ee = ((ee << 6) | (ee >> 26)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += ((ee & bb) | (aa & (~bb))) + xPt[6] + 0x8f1bbcdcU;
            dd = ((dd << 5) | (dd >> 27)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += ((dd & aa) | (ee & (~aa))) + xPt[2] + 0x8f1bbcdcU;
            cc = ((cc << 12) | (cc >> 20)) + bb;
            ee = (ee << 10) | (ee >> 22);

            /* round 5 */
            bb += (cc ^ (dd | (~ee))) + xPt[4] + 0xa953fd4eU;
            bb = ((bb << 9) | (bb >> 23)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += (bb ^ (cc | (~dd))) + xPt[0] + 0xa953fd4eU;
            aa = ((aa << 15) | (aa >> 17)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += (aa ^ (bb | (~cc))) + xPt[5] + 0xa953fd4eU;
            ee = ((ee << 5) | (ee >> 27)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += (ee ^ (aa | (~bb))) + xPt[9] + 0xa953fd4eU;
            dd = ((dd << 11) | (dd >> 21)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += (dd ^ (ee | (~aa))) + xPt[7] + 0xa953fd4eU;
            cc = ((cc << 6) | (cc >> 26)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += (cc ^ (dd | (~ee))) + xPt[12] + 0xa953fd4eU;
            bb = ((bb << 8) | (bb >> 24)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += (bb ^ (cc | (~dd))) + xPt[2] + 0xa953fd4eU;
            aa = ((aa << 13) | (aa >> 19)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += (aa ^ (bb | (~cc))) + xPt[10] + 0xa953fd4eU;
            ee = ((ee << 12) | (ee >> 20)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += (ee ^ (aa | (~bb))) + xPt[14] + 0xa953fd4eU;
            dd = ((dd << 5) | (dd >> 27)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += (dd ^ (ee | (~aa))) + xPt[1] + 0xa953fd4eU;
            cc = ((cc << 12) | (cc >> 20)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += (cc ^ (dd | (~ee))) + xPt[3] + 0xa953fd4eU;
            bb = ((bb << 13) | (bb >> 19)) + aa;
            dd = (dd << 10) | (dd >> 22);
            aa += (bb ^ (cc | (~dd))) + xPt[8] + 0xa953fd4eU;
            aa = ((aa << 14) | (aa >> 18)) + ee;
            cc = (cc << 10) | (cc >> 22);
            ee += (aa ^ (bb | (~cc))) + xPt[11] + 0xa953fd4eU;
            ee = ((ee << 11) | (ee >> 21)) + dd;
            bb = (bb << 10) | (bb >> 22);
            dd += (ee ^ (aa | (~bb))) + xPt[6] + 0xa953fd4eU;
            dd = ((dd << 8) | (dd >> 24)) + cc;
            aa = (aa << 10) | (aa >> 22);
            cc += (dd ^ (ee | (~aa))) + xPt[15] + 0xa953fd4eU;
            cc = ((cc << 5) | (cc >> 27)) + bb;
            ee = (ee << 10) | (ee >> 22);
            bb += (cc ^ (dd | (~ee))) + xPt[13] + 0xa953fd4eU;
            bb = ((bb << 6) | (bb >> 26)) + aa;
            dd = (dd << 10) | (dd >> 22);

            /* parallel round 1 */
            aaa += (bbb ^ (ccc | (~ddd))) + xPt[5] + 0x50a28be6U;
            aaa = ((aaa << 8) | (aaa >> 24)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += (aaa ^ (bbb | (~ccc))) + xPt[14] + 0x50a28be6U;
            eee = ((eee << 9) | (eee >> 23)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += (eee ^ (aaa | (~bbb))) + xPt[7] + 0x50a28be6U;
            ddd = ((ddd << 9) | (ddd >> 23)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += (ddd ^ (eee | (~aaa))) + xPt[0] + 0x50a28be6U;
            ccc = ((ccc << 11) | (ccc >> 21)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += (ccc ^ (ddd | (~eee))) + xPt[9] + 0x50a28be6U;
            bbb = ((bbb << 13) | (bbb >> 19)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += (bbb ^ (ccc | (~ddd))) + xPt[2] + 0x50a28be6U;
            aaa = ((aaa << 15) | (aaa >> 17)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += (aaa ^ (bbb | (~ccc))) + xPt[11] + 0x50a28be6U;
            eee = ((eee << 15) | (eee >> 17)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += (eee ^ (aaa | (~bbb))) + xPt[4] + 0x50a28be6U;
            ddd = ((ddd << 5) | (ddd >> 27)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += (ddd ^ (eee | (~aaa))) + xPt[13] + 0x50a28be6U;
            ccc = ((ccc << 7) | (ccc >> 25)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += (ccc ^ (ddd | (~eee))) + xPt[6] + 0x50a28be6U;
            bbb = ((bbb << 7) | (bbb >> 25)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += (bbb ^ (ccc | (~ddd))) + xPt[15] + 0x50a28be6U;
            aaa = ((aaa << 8) | (aaa >> 24)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += (aaa ^ (bbb | (~ccc))) + xPt[8] + 0x50a28be6U;
            eee = ((eee << 11) | (eee >> 21)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += (eee ^ (aaa | (~bbb))) + xPt[1] + 0x50a28be6U;
            ddd = ((ddd << 14) | (ddd >> 18)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += (ddd ^ (eee | (~aaa))) + xPt[10] + 0x50a28be6U;
            ccc = ((ccc << 14) | (ccc >> 18)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += (ccc ^ (ddd | (~eee))) + xPt[3] + 0x50a28be6U;
            bbb = ((bbb << 12) | (bbb >> 20)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += (bbb ^ (ccc | (~ddd))) + xPt[12] + 0x50a28be6U;
            aaa = ((aaa << 6) | (aaa >> 26)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);

            /* parallel round 2 */
            eee += ((aaa & ccc) | (bbb & (~ccc))) + xPt[6] + 0x5c4dd124U;
            eee = ((eee << 9) | (eee >> 23)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee & bbb) | (aaa & (~bbb))) + xPt[11] + 0x5c4dd124U;
            ddd = ((ddd << 13) | (ddd >> 19)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd & aaa) | (eee & (~aaa))) + xPt[3] + 0x5c4dd124U;
            ccc = ((ccc << 15) | (ccc >> 17)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc & eee) | (ddd & (~eee))) + xPt[7] + 0x5c4dd124U;
            bbb = ((bbb << 7) | (bbb >> 25)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb & ddd) | (ccc & (~ddd))) + xPt[0] + 0x5c4dd124U;
            aaa = ((aaa << 12) | (aaa >> 20)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa & ccc) | (bbb & (~ccc))) + xPt[13] + 0x5c4dd124U;
            eee = ((eee << 8) | (eee >> 24)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee & bbb) | (aaa & (~bbb))) + xPt[5] + 0x5c4dd124U;
            ddd = ((ddd << 9) | (ddd >> 23)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd & aaa) | (eee & (~aaa))) + xPt[10] + 0x5c4dd124U;
            ccc = ((ccc << 11) | (ccc >> 21)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc & eee) | (ddd & (~eee))) + xPt[14] + 0x5c4dd124U;
            bbb = ((bbb << 7) | (bbb >> 25)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb & ddd) | (ccc & (~ddd))) + xPt[15] + 0x5c4dd124U;
            aaa = ((aaa << 7) | (aaa >> 25)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa & ccc) | (bbb & (~ccc))) + xPt[8] + 0x5c4dd124U;
            eee = ((eee << 12) | (eee >> 20)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee & bbb) | (aaa & (~bbb))) + xPt[12] + 0x5c4dd124U;
            ddd = ((ddd << 7) | (ddd >> 25)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd & aaa) | (eee & (~aaa))) + xPt[4] + 0x5c4dd124U;
            ccc = ((ccc << 6) | (ccc >> 26)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc & eee) | (ddd & (~eee))) + xPt[9] + 0x5c4dd124U;
            bbb = ((bbb << 15) | (bbb >> 17)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb & ddd) | (ccc & (~ddd))) + xPt[1] + 0x5c4dd124U;
            aaa = ((aaa << 13) | (aaa >> 19)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa & ccc) | (bbb & (~ccc))) + xPt[2] + 0x5c4dd124U;
            eee = ((eee << 11) | (eee >> 21)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);

            /* parallel round 3 */
            ddd += ((eee | (~aaa)) ^ bbb) + xPt[15] + 0x6d703ef3U;
            ddd = ((ddd << 9) | (ddd >> 23)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd | (~eee)) ^ aaa) + xPt[5] + 0x6d703ef3U;
            ccc = ((ccc << 7) | (ccc >> 25)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc | (~ddd)) ^ eee) + xPt[1] + 0x6d703ef3U;
            bbb = ((bbb << 15) | (bbb >> 17)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb | (~ccc)) ^ ddd) + xPt[3] + 0x6d703ef3U;
            aaa = ((aaa << 11) | (aaa >> 21)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa | (~bbb)) ^ ccc) + xPt[7] + 0x6d703ef3U;
            eee = ((eee << 8) | (eee >> 24)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee | (~aaa)) ^ bbb) + xPt[14] + 0x6d703ef3U;
            ddd = ((ddd << 6) | (ddd >> 26)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd | (~eee)) ^ aaa) + xPt[6] + 0x6d703ef3U;
            ccc = ((ccc << 6) | (ccc >> 26)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc | (~ddd)) ^ eee) + xPt[9] + 0x6d703ef3U;
            bbb = ((bbb << 14) | (bbb >> 18)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb | (~ccc)) ^ ddd) + xPt[11] + 0x6d703ef3U;
            aaa = ((aaa << 12) | (aaa >> 20)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa | (~bbb)) ^ ccc) + xPt[8] + 0x6d703ef3U;
            eee = ((eee << 13) | (eee >> 19)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee | (~aaa)) ^ bbb) + xPt[12] + 0x6d703ef3U;
            ddd = ((ddd << 5) | (ddd >> 27)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd | (~eee)) ^ aaa) + xPt[2] + 0x6d703ef3U;
            ccc = ((ccc << 14) | (ccc >> 18)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc | (~ddd)) ^ eee) + xPt[10] + 0x6d703ef3U;
            bbb = ((bbb << 13) | (bbb >> 19)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb | (~ccc)) ^ ddd) + xPt[0] + 0x6d703ef3U;
            aaa = ((aaa << 13) | (aaa >> 19)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa | (~bbb)) ^ ccc) + xPt[4] + 0x6d703ef3U;
            eee = ((eee << 7) | (eee >> 25)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee | (~aaa)) ^ bbb) + xPt[13] + 0x6d703ef3U;
            ddd = ((ddd << 5) | (ddd >> 27)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);

            /* parallel round 4 */
            ccc += ((ddd & eee) | ((~ddd) & aaa)) + xPt[8] + 0x7a6d76e9U;
            ccc = ((ccc << 15) | (ccc >> 17)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc & ddd) | ((~ccc) & eee)) + xPt[6] + 0x7a6d76e9U;
            bbb = ((bbb << 5) | (bbb >> 27)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb & ccc) | ((~bbb) & ddd)) + xPt[4] + 0x7a6d76e9U;
            aaa = ((aaa << 8) | (aaa >> 24)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa & bbb) | ((~aaa) & ccc)) + xPt[1] + 0x7a6d76e9U;
            eee = ((eee << 11) | (eee >> 21)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee & aaa) | ((~eee) & bbb)) + xPt[3] + 0x7a6d76e9U;
            ddd = ((ddd << 14) | (ddd >> 18)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd & eee) | ((~ddd) & aaa)) + xPt[11] + 0x7a6d76e9U;
            ccc = ((ccc << 14) | (ccc >> 18)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc & ddd) | ((~ccc) & eee)) + xPt[15] + 0x7a6d76e9U;
            bbb = ((bbb << 6) | (bbb >> 26)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb & ccc) | ((~bbb) & ddd)) + xPt[0] + 0x7a6d76e9U;
            aaa = ((aaa << 14) | (aaa >> 18)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa & bbb) | ((~aaa) & ccc)) + xPt[5] + 0x7a6d76e9U;
            eee = ((eee << 6) | (eee >> 26)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee & aaa) | ((~eee) & bbb)) + xPt[12] + 0x7a6d76e9U;
            ddd = ((ddd << 9) | (ddd >> 23)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd & eee) | ((~ddd) & aaa)) + xPt[2] + 0x7a6d76e9U;
            ccc = ((ccc << 12) | (ccc >> 20)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += ((ccc & ddd) | ((~ccc) & eee)) + xPt[13] + 0x7a6d76e9U;
            bbb = ((bbb << 9) | (bbb >> 23)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += ((bbb & ccc) | ((~bbb) & ddd)) + xPt[9] + 0x7a6d76e9U;
            aaa = ((aaa << 12) | (aaa >> 20)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += ((aaa & bbb) | ((~aaa) & ccc)) + xPt[7] + 0x7a6d76e9U;
            eee = ((eee << 5) | (eee >> 27)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += ((eee & aaa) | ((~eee) & bbb)) + xPt[10] + 0x7a6d76e9U;
            ddd = ((ddd << 15) | (ddd >> 17)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += ((ddd & eee) | ((~ddd) & aaa)) + xPt[14] + 0x7a6d76e9U;
            ccc = ((ccc << 8) | (ccc >> 24)) + bbb;
            eee = (eee << 10) | (eee >> 22);

            /* parallel round 5 */
            bbb += (ccc ^ ddd ^ eee) + xPt[12];
            bbb = ((bbb << 8) | (bbb >> 24)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += (bbb ^ ccc ^ ddd) + xPt[15];
            aaa = ((aaa << 5) | (aaa >> 27)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += (aaa ^ bbb ^ ccc) + xPt[10];
            eee = ((eee << 12) | (eee >> 20)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += (eee ^ aaa ^ bbb) + xPt[4];
            ddd = ((ddd << 9) | (ddd >> 23)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += (ddd ^ eee ^ aaa) + xPt[1];
            ccc = ((ccc << 12) | (ccc >> 20)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += (ccc ^ ddd ^ eee) + xPt[5];
            bbb = ((bbb << 5) | (bbb >> 27)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += (bbb ^ ccc ^ ddd) + xPt[8];
            aaa = ((aaa << 14) | (aaa >> 18)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += (aaa ^ bbb ^ ccc) + xPt[7];
            eee = ((eee << 6) | (eee >> 26)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += (eee ^ aaa ^ bbb) + xPt[6];
            ddd = ((ddd << 8) | (ddd >> 24)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += (ddd ^ eee ^ aaa) + xPt[2];
            ccc = ((ccc << 13) | (ccc >> 19)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += (ccc ^ ddd ^ eee) + xPt[13];
            bbb = ((bbb << 6) | (bbb >> 26)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);
            aaa += (bbb ^ ccc ^ ddd) + xPt[14];
            aaa = ((aaa << 5) | (aaa >> 27)) + eee;
            ccc = (ccc << 10) | (ccc >> 22);
            eee += (aaa ^ bbb ^ ccc) + xPt[0];
            eee = ((eee << 15) | (eee >> 17)) + ddd;
            bbb = (bbb << 10) | (bbb >> 22);
            ddd += (eee ^ aaa ^ bbb) + xPt[3];
            ddd = ((ddd << 13) | (ddd >> 19)) + ccc;
            aaa = (aaa << 10) | (aaa >> 22);
            ccc += (ddd ^ eee ^ aaa) + xPt[9];
            ccc = ((ccc << 11) | (ccc >> 21)) + bbb;
            eee = (eee << 10) | (eee >> 22);
            bbb += (ccc ^ ddd ^ eee) + xPt[11];
            bbb = ((bbb << 11) | (bbb >> 21)) + aaa;
            ddd = (ddd << 10) | (ddd >> 22);


            /* combine results */
            ddd += cc + hPt[1];               /* final result for MDbuf[0] */
            hPt[1] = hPt[2] + dd + eee;
            hPt[2] = hPt[3] + ee + aaa;
            hPt[3] = hPt[4] + aa + bbb;
            hPt[4] = hPt[0] + bb + ccc;
            hPt[0] = ddd;
        }



        private bool disposedValue = false;

        /// <summary>
        /// Releases the resources used by the <see cref="Ripemd160"/> class.
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
                    if (!(block is null))
                        Array.Clear(block, 0, block.Length);
                    block = null;

                    if (!(hashState is null))
                        Array.Clear(hashState, 0, hashState.Length);
                    hashState = null;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Ripemd160"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }



        #region Code Generator

        /***********************************************************************************************
         * The code in this region was used to generate the long code above
         * it is included here for for historical and verification reasons.
        ************************************************************************************************/

        //System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //public void CodeGen()
        //{
        //    sb.AppendLine("/* round 1 */");
        //    FF("aa", "bb", "cc", "dd", "ee", "xPt[0]", 11);
        //    FF("ee", "aa", "bb", "cc", "dd", "X1", 14);
        //    FF("dd", "ee", "aa", "bb", "cc", "xPt[2]", 15);
        //    FF("cc", "dd", "ee", "aa", "bb", "xPt[3]", 12);
        //    FF("bb", "cc", "dd", "ee", "aa", "xPt[4]", 5);
        //    FF("aa", "bb", "cc", "dd", "ee", "xPt[5]", 8);
        //    FF("ee", "aa", "bb", "cc", "dd", "xPt[6]", 7);
        //    FF("dd", "ee", "aa", "bb", "cc", "xPt[7]", 9);
        //    FF("cc", "dd", "ee", "aa", "bb", "xPt[8]", 11);
        //    FF("bb", "cc", "dd", "ee", "aa", "xPt[9]", 13);
        //    FF("aa", "bb", "cc", "dd", "ee", "xPt[10]", 14);
        //    FF("ee", "aa", "bb", "cc", "dd", "xPt[11]", 15);
        //    FF("dd", "ee", "aa", "bb", "cc", "xPt[12]", 6);
        //    FF("cc", "dd", "ee", "aa", "bb", "xPt[13]", 7);
        //    FF("bb", "cc", "dd", "ee", "aa", "xPt[14]", 9);
        //    FF("aa", "bb", "cc", "dd", "ee", "xPt[15]", 8);

        //    sb.AppendLine();
        //    sb.AppendLine("/* round 2 */");
        //    GG("ee", "aa", "bb", "cc", "dd", "xPt[7]", 7);
        //    GG("dd", "ee", "aa", "bb", "cc", "xPt[4]", 6);
        //    GG("cc", "dd", "ee", "aa", "bb", "xPt[13]", 8);
        //    GG("bb", "cc", "dd", "ee", "aa", "X1", 13);
        //    GG("aa", "bb", "cc", "dd", "ee", "xPt[10]", 11);
        //    GG("ee", "aa", "bb", "cc", "dd", "xPt[6]", 9);
        //    GG("dd", "ee", "aa", "bb", "cc", "xPt[15]", 7);
        //    GG("cc", "dd", "ee", "aa", "bb", "xPt[3]", 15);
        //    GG("bb", "cc", "dd", "ee", "aa", "xPt[12]", 7);
        //    GG("aa", "bb", "cc", "dd", "ee", "xPt[0]", 12);
        //    GG("ee", "aa", "bb", "cc", "dd", "xPt[9]", 15);
        //    GG("dd", "ee", "aa", "bb", "cc", "xPt[5]", 9);
        //    GG("cc", "dd", "ee", "aa", "bb", "xPt[2]", 11);
        //    GG("bb", "cc", "dd", "ee", "aa", "xPt[14]", 7);
        //    GG("aa", "bb", "cc", "dd", "ee", "xPt[11]", 13);
        //    GG("ee", "aa", "bb", "cc", "dd", "xPt[8]", 12);

        //    sb.AppendLine();
        //    sb.AppendLine("/* round 3 */");
        //    HH("dd", "ee", "aa", "bb", "cc", "xPt[3]", 11);
        //    HH("cc", "dd", "ee", "aa", "bb", "xPt[10]", 13);
        //    HH("bb", "cc", "dd", "ee", "aa", "xPt[14]", 6);
        //    HH("aa", "bb", "cc", "dd", "ee", "xPt[4]", 7);
        //    HH("ee", "aa", "bb", "cc", "dd", "xPt[9]", 14);
        //    HH("dd", "ee", "aa", "bb", "cc", "xPt[15]", 9);
        //    HH("cc", "dd", "ee", "aa", "bb", "xPt[8]", 13);
        //    HH("bb", "cc", "dd", "ee", "aa", "X1", 15);
        //    HH("aa", "bb", "cc", "dd", "ee", "xPt[2]", 14);
        //    HH("ee", "aa", "bb", "cc", "dd", "xPt[7]", 8);
        //    HH("dd", "ee", "aa", "bb", "cc", "xPt[0]", 13);
        //    HH("cc", "dd", "ee", "aa", "bb", "xPt[6]", 6);
        //    HH("bb", "cc", "dd", "ee", "aa", "xPt[13]", 5);
        //    HH("aa", "bb", "cc", "dd", "ee", "xPt[11]", 12);
        //    HH("ee", "aa", "bb", "cc", "dd", "xPt[5]", 7);
        //    HH("dd", "ee", "aa", "bb", "cc", "xPt[12]", 5);

        //    sb.AppendLine();
        //    sb.AppendLine("/* round 4 */");
        //    II("cc", "dd", "ee", "aa", "bb", "X1", 11);
        //    II("bb", "cc", "dd", "ee", "aa", "xPt[9]", 12);
        //    II("aa", "bb", "cc", "dd", "ee", "xPt[11]", 14);
        //    II("ee", "aa", "bb", "cc", "dd", "xPt[10]", 15);
        //    II("dd", "ee", "aa", "bb", "cc", "xPt[0]", 14);
        //    II("cc", "dd", "ee", "aa", "bb", "xPt[8]", 15);
        //    II("bb", "cc", "dd", "ee", "aa", "xPt[12]", 9);
        //    II("aa", "bb", "cc", "dd", "ee", "xPt[4]", 8);
        //    II("ee", "aa", "bb", "cc", "dd", "xPt[13]", 9);
        //    II("dd", "ee", "aa", "bb", "cc", "xPt[3]", 14);
        //    II("cc", "dd", "ee", "aa", "bb", "xPt[7]", 5);
        //    II("bb", "cc", "dd", "ee", "aa", "xPt[15]", 6);
        //    II("aa", "bb", "cc", "dd", "ee", "xPt[14]", 8);
        //    II("ee", "aa", "bb", "cc", "dd", "xPt[5]", 6);
        //    II("dd", "ee", "aa", "bb", "cc", "xPt[6]", 5);
        //    II("cc", "dd", "ee", "aa", "bb", "xPt[2]", 12);

        //    sb.AppendLine();
        //    sb.AppendLine("/* round 5 */");
        //    JJ("bb", "cc", "dd", "ee", "aa", "xPt[4]", 9);
        //    JJ("aa", "bb", "cc", "dd", "ee", "xPt[0]", 15);
        //    JJ("ee", "aa", "bb", "cc", "dd", "xPt[5]", 5);
        //    JJ("dd", "ee", "aa", "bb", "cc", "xPt[9]", 11);
        //    JJ("cc", "dd", "ee", "aa", "bb", "xPt[7]", 6);
        //    JJ("bb", "cc", "dd", "ee", "aa", "xPt[12]", 8);
        //    JJ("aa", "bb", "cc", "dd", "ee", "xPt[2]", 13);
        //    JJ("ee", "aa", "bb", "cc", "dd", "xPt[10]", 12);
        //    JJ("dd", "ee", "aa", "bb", "cc", "xPt[14]", 5);
        //    JJ("cc", "dd", "ee", "aa", "bb", "X1", 12);
        //    JJ("bb", "cc", "dd", "ee", "aa", "xPt[3]", 13);
        //    JJ("aa", "bb", "cc", "dd", "ee", "xPt[8]", 14);
        //    JJ("ee", "aa", "bb", "cc", "dd", "xPt[11]", 11);
        //    JJ("dd", "ee", "aa", "bb", "cc", "xPt[6]", 8);
        //    JJ("cc", "dd", "ee", "aa", "bb", "xPt[15]", 5);
        //    JJ("bb", "cc", "dd", "ee", "aa", "xPt[13]", 6);

        //    sb.AppendLine();
        //    sb.AppendLine("/* parallel round 1 */");
        //    JJJ("aaa", "bbb", "ccc", "ddd", "eee", "xPt[5]", 8);
        //    JJJ("eee", "aaa", "bbb", "ccc", "ddd", "xPt[14]", 9);
        //    JJJ("ddd", "eee", "aaa", "bbb", "ccc", "xPt[7]", 9);
        //    JJJ("ccc", "ddd", "eee", "aaa", "bbb", "xPt[0]", 11);
        //    JJJ("bbb", "ccc", "ddd", "eee", "aaa", "xPt[9]", 13);
        //    JJJ("aaa", "bbb", "ccc", "ddd", "eee", "xPt[2]", 15);
        //    JJJ("eee", "aaa", "bbb", "ccc", "ddd", "xPt[11]", 15);
        //    JJJ("ddd", "eee", "aaa", "bbb", "ccc", "xPt[4]", 5);
        //    JJJ("ccc", "ddd", "eee", "aaa", "bbb", "xPt[13]", 7);
        //    JJJ("bbb", "ccc", "ddd", "eee", "aaa", "xPt[6]", 7);
        //    JJJ("aaa", "bbb", "ccc", "ddd", "eee", "xPt[15]", 8);
        //    JJJ("eee", "aaa", "bbb", "ccc", "ddd", "xPt[8]", 11);
        //    JJJ("ddd", "eee", "aaa", "bbb", "ccc", "X1", 14);
        //    JJJ("ccc", "ddd", "eee", "aaa", "bbb", "xPt[10]", 14);
        //    JJJ("bbb", "ccc", "ddd", "eee", "aaa", "xPt[3]", 12);
        //    JJJ("aaa", "bbb", "ccc", "ddd", "eee", "xPt[12]", 6);

        //    sb.AppendLine();
        //    sb.AppendLine("/* parallel round 2 */");
        //    III("eee", "aaa", "bbb", "ccc", "ddd", "xPt[6]", 9);
        //    III("ddd", "eee", "aaa", "bbb", "ccc", "xPt[11]", 13);
        //    III("ccc", "ddd", "eee", "aaa", "bbb", "xPt[3]", 15);
        //    III("bbb", "ccc", "ddd", "eee", "aaa", "xPt[7]", 7);
        //    III("aaa", "bbb", "ccc", "ddd", "eee", "xPt[0]", 12);
        //    III("eee", "aaa", "bbb", "ccc", "ddd", "xPt[13]", 8);
        //    III("ddd", "eee", "aaa", "bbb", "ccc", "xPt[5]", 9);
        //    III("ccc", "ddd", "eee", "aaa", "bbb", "xPt[10]", 11);
        //    III("bbb", "ccc", "ddd", "eee", "aaa", "xPt[14]", 7);
        //    III("aaa", "bbb", "ccc", "ddd", "eee", "xPt[15]", 7);
        //    III("eee", "aaa", "bbb", "ccc", "ddd", "xPt[8]", 12);
        //    III("ddd", "eee", "aaa", "bbb", "ccc", "xPt[12]", 7);
        //    III("ccc", "ddd", "eee", "aaa", "bbb", "xPt[4]", 6);
        //    III("bbb", "ccc", "ddd", "eee", "aaa", "xPt[9]", 15);
        //    III("aaa", "bbb", "ccc", "ddd", "eee", "X1", 13);
        //    III("eee", "aaa", "bbb", "ccc", "ddd", "xPt[2]", 11);

        //    sb.AppendLine();
        //    sb.AppendLine("/* parallel round 3 */");
        //    HHH("ddd", "eee", "aaa", "bbb", "ccc", "xPt[15]", 9);
        //    HHH("ccc", "ddd", "eee", "aaa", "bbb", "xPt[5]", 7);
        //    HHH("bbb", "ccc", "ddd", "eee", "aaa", "X1", 15);
        //    HHH("aaa", "bbb", "ccc", "ddd", "eee", "xPt[3]", 11);
        //    HHH("eee", "aaa", "bbb", "ccc", "ddd", "xPt[7]", 8);
        //    HHH("ddd", "eee", "aaa", "bbb", "ccc", "xPt[14]", 6);
        //    HHH("ccc", "ddd", "eee", "aaa", "bbb", "xPt[6]", 6);
        //    HHH("bbb", "ccc", "ddd", "eee", "aaa", "xPt[9]", 14);
        //    HHH("aaa", "bbb", "ccc", "ddd", "eee", "xPt[11]", 12);
        //    HHH("eee", "aaa", "bbb", "ccc", "ddd", "xPt[8]", 13);
        //    HHH("ddd", "eee", "aaa", "bbb", "ccc", "xPt[12]", 5);
        //    HHH("ccc", "ddd", "eee", "aaa", "bbb", "xPt[2]", 14);
        //    HHH("bbb", "ccc", "ddd", "eee", "aaa", "xPt[10]", 13);
        //    HHH("aaa", "bbb", "ccc", "ddd", "eee", "xPt[0]", 13);
        //    HHH("eee", "aaa", "bbb", "ccc", "ddd", "xPt[4]", 7);
        //    HHH("ddd", "eee", "aaa", "bbb", "ccc", "xPt[13]", 5);

        //    sb.AppendLine();
        //    sb.AppendLine("/* parallel round 4 */");
        //    GGG("ccc", "ddd", "eee", "aaa", "bbb", "xPt[8]", 15);
        //    GGG("bbb", "ccc", "ddd", "eee", "aaa", "xPt[6]", 5);
        //    GGG("aaa", "bbb", "ccc", "ddd", "eee", "xPt[4]", 8);
        //    GGG("eee", "aaa", "bbb", "ccc", "ddd", "X1", 11);
        //    GGG("ddd", "eee", "aaa", "bbb", "ccc", "xPt[3]", 14);
        //    GGG("ccc", "ddd", "eee", "aaa", "bbb", "xPt[11]", 14);
        //    GGG("bbb", "ccc", "ddd", "eee", "aaa", "xPt[15]", 6);
        //    GGG("aaa", "bbb", "ccc", "ddd", "eee", "xPt[0]", 14);
        //    GGG("eee", "aaa", "bbb", "ccc", "ddd", "xPt[5]", 6);
        //    GGG("ddd", "eee", "aaa", "bbb", "ccc", "xPt[12]", 9);
        //    GGG("ccc", "ddd", "eee", "aaa", "bbb", "xPt[2]", 12);
        //    GGG("bbb", "ccc", "ddd", "eee", "aaa", "xPt[13]", 9);
        //    GGG("aaa", "bbb", "ccc", "ddd", "eee", "xPt[9]", 12);
        //    GGG("eee", "aaa", "bbb", "ccc", "ddd", "xPt[7]", 5);
        //    GGG("ddd", "eee", "aaa", "bbb", "ccc", "xPt[10]", 15);
        //    GGG("ccc", "ddd", "eee", "aaa", "bbb", "xPt[14]", 8);

        //    sb.AppendLine();
        //    sb.AppendLine("/* parallel round 5 */");
        //    FFF("bbb", "ccc", "ddd", "eee", "aaa", "xPt[12]", 8);
        //    FFF("aaa", "bbb", "ccc", "ddd", "eee", "xPt[15]", 5);
        //    FFF("eee", "aaa", "bbb", "ccc", "ddd", "xPt[10]", 12);
        //    FFF("ddd", "eee", "aaa", "bbb", "ccc", "xPt[4]", 9);
        //    FFF("ccc", "ddd", "eee", "aaa", "bbb", "X1", 12);
        //    FFF("bbb", "ccc", "ddd", "eee", "aaa", "xPt[5]", 5);
        //    FFF("aaa", "bbb", "ccc", "ddd", "eee", "xPt[8]", 14);
        //    FFF("eee", "aaa", "bbb", "ccc", "ddd", "xPt[7]", 6);
        //    FFF("ddd", "eee", "aaa", "bbb", "ccc", "xPt[6]", 8);
        //    FFF("ccc", "ddd", "eee", "aaa", "bbb", "xPt[2]", 13);
        //    FFF("bbb", "ccc", "ddd", "eee", "aaa", "xPt[13]", 6);
        //    FFF("aaa", "bbb", "ccc", "ddd", "eee", "xPt[14]", 5);
        //    FFF("eee", "aaa", "bbb", "ccc", "ddd", "xPt[0]", 15);
        //    FFF("ddd", "eee", "aaa", "bbb", "ccc", "xPt[3]", 13);
        //    FFF("ccc", "ddd", "eee", "aaa", "bbb", "xPt[9]", 11);
        //    FFF("bbb", "ccc", "ddd", "eee", "aaa", "xPt[11]", 11);

        //    string result = sb.ToString();
        //}
        //private void FF(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += ({b} ^ {c} ^ {d}) + {x};");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << 10) | ({c} >> {32 - 10});");
        //}
        //private void GG(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += (({b} & {c}) | ((~{b}) & {d})) + {x} + 0x5a827999U;");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << 10) | ({c} >> {32 - 10});");
        //}
        //private void HH(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += (({b} | (~{c})) ^ {d}) + {x} + 0x6ed9eba1U;");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << {10}) | ({c} >> {32 - 10});");
        //}
        //private void II(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += (({b} & {d}) | ({c} & (~{d}))) + {x} + 0x8f1bbcdcU;");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << {10}) | ({c} >> {32 - 10});");
        //}
        //private void JJ(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += ({b} ^ ({c} | (~{d}))) + {x} + 0xa953fd4eU;");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << {10}) | ({c} >> {32 - 10});");
        //}
        //private void FFF(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += ({b} ^ {c} ^ {d}) + {x};");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << {10}) | ({c} >> {32 - 10});");
        //}
        //private void GGG(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += (({b} & {c}) | ((~{b}) & {d})) + {x} + 0x7a6d76e9U;");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << {10}) | ({c} >> {32 - 10});");
        //}
        //private void HHH(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += (({b} | (~{c})) ^ {d}) + {x} + 0x6d703ef3U;");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << {10}) | ({c} >> {32 - 10});");
        //}
        //private void III(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += (({b} & {d}) | ({c} & (~{d}))) + {x} + 0x5c4dd124U;");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << {10}) | ({c} >> {32 - 10});");
        //}
        //private void JJJ(string a, string b, string c, string d, string e, string x, byte s)
        //{
        //    sb.AppendLine($"{a} += ({b} ^ ({c} | (~{d}))) + {x} + 0x50a28be6U;");
        //    sb.AppendLine($"{a} = (({a} << {s}) | ({a} >> {32 - s})) + {e};");
        //    sb.AppendLine($"{c} = ({c} << {10}) | ({c} >> {32 - 10});");
        //}

        #endregion

    }
}
