// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.KeyDerivationFunctions
{
    /// <summary>
    /// Implementation of scrypt, a memory-hard password-based key derivation function Based on RFC-7914.
    /// Implements <see cref="IDisposable"/>.
    /// <para/> https://tools.ietf.org/html/rfc7914
    /// </summary>
    public class Scrypt : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Scrypt"/> with the given parameters.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="costParam">CPU/memory cost parameter. Must be a multiple of 2^n.</param>
        /// <param name="blockSizeFactor">The blocksize parameter</param>
        /// <param name="parallelization">Parallelization</param>
        public Scrypt(int costParam, int blockSizeFactor, int parallelization)
        {
            if (costParam <= 1 || (costParam & (costParam - 1)) != 0)
                throw new ArgumentException("Cost parameter must be a multiple of 2^n and bigger than 1.", nameof(costParam));
            if (blockSizeFactor <= 0)
                throw new ArgumentOutOfRangeException(nameof(blockSizeFactor), "Blocksize factor must be bigger than 0.");
            if (parallelization <= 0)
                throw new ArgumentOutOfRangeException(nameof(parallelization), "Parallelization factor must be bigger than 0.");
            //TODO: check OutOfMemory possibility (since scrypt is used internally for BIPs it is not important for now)

            n = costParam;
            n1 = n - 1;
            r = blockSizeFactor;
            p = parallelization;
            kdf = new PBKDF2(1, new HmacSha256());

            blockSize = blockSizeFactor * 128;
            blockSizeUint = blockSize / 4;

            V = new uint[blockSizeUint * n];
        }



        private readonly int blockSize;

        /// <summary>
        /// r * 128 / 4 = r * 32
        /// </summary>
        private readonly int blockSizeUint;
        private readonly int r;
        private readonly int n;

        /// <summary>
        /// (n-1) which can be used for mod operations using bitwise AND
        /// </summary>
        private readonly int n1;
        private readonly int p;
        private PBKDF2 kdf;
        private uint[] V;



        /// <summary>
        /// Returns the pseudo-random key based on given password and salt.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="password">Password</param>
        /// <param name="salt">Salt</param>
        /// <param name="dkLen">Length of the returned derived key</param>
        /// <returns>The derived key</returns>
        public unsafe byte[] GetBytes(byte[] password, byte[] salt, int dkLen)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(Scrypt));
            if (password is null)
                throw new ArgumentNullException(nameof(password), "Password can not be null.");
            if (salt is null)
                throw new ArgumentNullException(nameof(salt), "Salt can not be null.");
            if (dkLen <= 0)
                throw new ArgumentOutOfRangeException(nameof(dkLen), "Derived key length must be bigger than zero.");

            byte[] dk = kdf.GetBytes(password, salt, p * 128 * r);

            fixed (byte* dkPt = &dk[0])
            fixed (uint* vPt = &V[0])
            {
                byte* dPt = dkPt;

                for (int i = 0; i < p; i++)
                {
                    ROMIX(dPt, vPt);
                    dPt += blockSize;
                }
            }

            return kdf.GetBytes(password, dk, dkLen);
        }

        private unsafe void ROMIX(byte* dataPt, uint* vPt)
        {
            // 1. X = B
            // 2. for i = 0 to N - 1 do
            //      V[i] = X
            //      X = scryptBlockMix (X)

            // Instead of using X we just use V and work inside one array to skip multiple calls to Array.Copy().
            // The values on each step (i) look like this:
            // X = block
            // i=0:
            //   V0 = X           ->    V0 = block
            //   X = BlockMix(X)
            // i=1:
            //   V1 = X           ->    V1 = BlockMix(V0)
            //   X = BlockMix(X)
            // i=2:
            //   V2 = X           ->    V2 = BlockMix(V1)
            //   X = BlockMix(X)
            // i=n-1:
            //   V(n-1) = X       ->    V(n-1) = BlockMix(V(n-2))
            //   X = BlockMix(X)
            // Important note is to perform BlockMix one more time to get the final X that is not in V but will be used later

            // Convert data byte[] to uint[]
            // On each call to this function we work on 1 blockSize hence the length of the foor loop
            // data pointer is moved forward by the caller.
            // This loop sets V0
            for (int i = 0, index = 0; i < blockSize; i += 4, index++)
            {
                vPt[index] = unchecked((uint)(dataPt[i] | (dataPt[i + 1] << 8) | (dataPt[i + 2] << 16) | (dataPt[i + 3] << 24)));
            }

            uint* srcPt = vPt;
            uint* dstPt = vPt + blockSizeUint;

            // Set V1 to final V(n-1)
            for (int i = 0; i < n - 1; i++)
            {
                BlockMix(srcPt, dstPt);
                srcPt += blockSizeUint;
                dstPt += blockSizeUint;
            }

            uint[] x = new uint[blockSizeUint];
            // We need a clone of x becasue BlockMix function needs to use the same fixed values of x 
            // while setting the result in output
            uint[] xClone = new uint[blockSizeUint];
            fixed (uint* xPt = &x[0], xClPt = &xClone[0])
            {
                // Perform BlockMix on X to update its result
                BlockMix(srcPt, xPt);

                for (int i = 0; i < n; i++)
                {
                    // *** Integerify ***
                    // j = Integerify (X) mod N
                    //      Integerify (B[0] ... B[2 * r - 1]) is defined as 
                    //      the result of interpreting B[2 * r - 1] as a little-endian integer.

                    // Interpret (B[2 * r - 1]) in (B[0] ... B[2 * r - 1]) as a little-endian integer and compute mod N
                    // This means taking the last 64 byte chunk from the block
                    // Since the conversion from data to uint[] is done in little-endian order,
                    // the result is already in correct endian
                    // and we need the least significat bytes (first item in that chunk) 
                    //                                                  => index = B.Length - 16

                    // B (or x) always has blockSize(=r*128) items in byte[] or blockSizeUint(=r*32) items in uint[]
                    // last 64 bytes = last 16*4 byte or 16 uint items in its uint[]

                    // Since value of N (costParam) is an integer set in constructor, it will always be smaller than a uint
                    // so mod N can be calculated with only 1 item from uint[] and that is the last item

                    // Since N is a power of 2, calculating mod N is a simple bitwise AND with (N-1)
                    // The final cast to int doesn't overflow since N is an int and mod N is always smaller than N.
                    int j = (int)(xPt[x.Length - 16] & n1);
                    XOR(xPt, vPt + (j * blockSizeUint), x.Length);

                    BlockMix(xPt, xClPt);
                    Buffer.BlockCopy(xClone, 0, x, 0, blockSize);
                }

                // Now that this block is "mixed" we have the expensive salt for second call to PBKDF2
                // it just needs to be converted back to byte[]
                for (int i = 0, index = 0; i < blockSize; i += 4, index++)
                {
                    dataPt[i] = (byte)xPt[index];
                    dataPt[i + 1] = (byte)(xPt[index] >> 8);
                    dataPt[i + 2] = (byte)(xPt[index] >> 16);
                    dataPt[i + 3] = (byte)(xPt[index] >> 24);
                }
            }
        }

        private uint[] blockMixBuffer = new uint[16]; // (64/4)=16
        private unsafe void BlockMix(uint* srcPt, uint* dstPt)
        {
            // Treat block as 2r 64 byte chunks
            fixed (uint* xPt = &blockMixBuffer[0])
            {
                Copy64(srcPt + blockSizeUint - 16, xPt);

                uint* block = srcPt;

                int i1 = 0;
                int i2 = r * 16;
                for (int i = 0; i < 2 * r; i++)
                {
                    XOR(xPt, block, 16);
                    Salsa20_8(xPt);

                    // Final result B' is:
                    //      Y[0], Y[2], ..., Y[2 * r - 2], Y[1], Y[3], ..., Y[2 * r - 1]
                    if ((i & 1) == 0) // i = 0,2,4,...
                    {
                        Copy64(xPt, dstPt + i1);
                        i1 += 16;
                    }
                    else
                    {
                        Copy64(xPt, dstPt + i2);
                        i2 += 16;
                    }

                    block += 16;
                }
            }
        }

        private unsafe void XOR(uint* first, uint* second, int uLen)
        {
            for (int i = 0; i < uLen; i++)
            {
                first[i] ^= second[i];
            }
        }

        private unsafe void Salsa20_8(uint* block)
        {
            // Salsa is performed on a block with 64 byte length (16 uint)
            uint x0 = block[0];
            uint x1 = block[1];
            uint x2 = block[2];
            uint x3 = block[3];
            uint x4 = block[4];
            uint x5 = block[5];
            uint x6 = block[6];
            uint x7 = block[7];
            uint x8 = block[8];
            uint x9 = block[9];
            uint x10 = block[10];
            uint x11 = block[11];
            uint x12 = block[12];
            uint x13 = block[13];
            uint x14 = block[14];
            uint x15 = block[15];

            // Inside the loop value of `i` is not used, the loop is repetition of the process 4 times
            // there is no point in doing it as RFC documents:
            // for (int i = 8; i > 0; i -= 2) 
            // i+=2 or the reverse is only used when the `Rounds` (here=8) is unknown 
            // in which case a double round on each iteration is performed.

            for (int i = 0; i < 4; i++)
            {
                x4 ^= R(x0 + x12, 7); x8 ^= R(x4 + x0, 9);
                x12 ^= R(x8 + x4, 13); x0 ^= R(x12 + x8, 18);
                x9 ^= R(x5 + x1, 7); x13 ^= R(x9 + x5, 9);
                x1 ^= R(x13 + x9, 13); x5 ^= R(x1 + x13, 18);
                x14 ^= R(x10 + x6, 7); x2 ^= R(x14 + x10, 9);
                x6 ^= R(x2 + x14, 13); x10 ^= R(x6 + x2, 18);
                x3 ^= R(x15 + x11, 7); x7 ^= R(x3 + x15, 9);
                x11 ^= R(x7 + x3, 13); x15 ^= R(x11 + x7, 18);

                x1 ^= R(x0 + x3, 7); x2 ^= R(x1 + x0, 9);
                x3 ^= R(x2 + x1, 13); x0 ^= R(x3 + x2, 18);
                x6 ^= R(x5 + x4, 7); x7 ^= R(x6 + x5, 9);
                x4 ^= R(x7 + x6, 13); x5 ^= R(x4 + x7, 18);
                x11 ^= R(x10 + x9, 7); x8 ^= R(x11 + x10, 9);
                x9 ^= R(x8 + x11, 13); x10 ^= R(x9 + x8, 18);
                x12 ^= R(x15 + x14, 7); x13 ^= R(x12 + x15, 9);
                x14 ^= R(x13 + x12, 13); x15 ^= R(x14 + x13, 18);
            }

            block[0] += x0;
            block[1] += x1;
            block[2] += x2;
            block[3] += x3;
            block[4] += x4;
            block[5] += x5;
            block[6] += x6;
            block[7] += x7;
            block[8] += x8;
            block[9] += x9;
            block[10] += x10;
            block[11] += x11;
            block[12] += x12;
            block[13] += x13;
            block[14] += x14;
            block[15] += x15;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint R(uint a, int b) => unchecked((a << b) | (a >> (32 - b)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Copy64(uint* src, uint* dst)
        {
            for (int i = 0; i < 16; i += 2)
            {
                *(ulong*)(dst + i) = *(ulong*)(src + i);
            }
        }



        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by the <see cref="Scrypt"/> class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (!(V is null))
                        Array.Clear(V, 0, V.Length);
                    V = null;

                    if (!(blockMixBuffer is null))
                        Array.Clear(blockMixBuffer, 0, blockMixBuffer.Length);
                    blockMixBuffer = null;

                    if (!(kdf is null))
                        kdf.Dispose();
                    kdf = null;
                }

                isDisposed = true;
            }
        }


        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Scrypt"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
