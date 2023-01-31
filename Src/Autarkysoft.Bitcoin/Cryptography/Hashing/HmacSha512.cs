// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) by using the <see cref="Sha512"/> hash function.
    /// Based on RFC-2104
    /// <para/> https://tools.ietf.org/html/rfc2104
    /// </summary>
    public sealed class HmacSha512 : IHmacFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HmacSha512"/>. 
        /// <para/> Useful for using the same instance for computing HMAC each time with a different key 
        /// by calling <see cref="ComputeHash(ReadOnlySpan{byte}, ReadOnlySpan{byte})"/>
        /// </summary>
        public HmacSha512()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HmacSha512"/> using the given key.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="key">Key to use, if the key is bigger than 64 bytes it will be hashed using <see cref="Sha512"/></param>
        public HmacSha512(byte[] key)
        {
            Key = key;
        }



        /// <inheritdoc/>
        public int BlockSize => 128;
        /// <inheritdoc/>
        public int OutputSize => 64;

        // these pads are supposed to be used as working vector of SHA512 hence the size=80
        private ulong[] opad = new ulong[80];
        private ulong[] ipad = new ulong[80];
        private byte[] _keyValue;



        /// <summary>
        /// Gets and sets key used for computing Hash-based Message Authentication Code (HMAC).
        /// Keys bigger than block size (128 bytes) will be hashed using <see cref="Sha512"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public byte[] Key
        {
            get => _keyValue;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Key can not be null.");


                if (value.Length > Sha512.BlockByteSize)
                {
                    _keyValue = Sha512.ComputeHash(value);
                }
                else
                {
                    _keyValue = value.CloneByteArray();
                }

                unsafe
                {
                    // In order to set pads we have to XOR key with pad values. 
                    // Since we don't know the length of the key, it is harder to loop using UInt32 so we use 2 temp pad bytes:
                    byte[] opadB = new byte[Sha512.BlockByteSize];
                    byte[] ipadB = new byte[Sha512.BlockByteSize];

                    // Note (kp = _keyValue) can't assign to first item because key might be empty array which will throw an excpetion
                    fixed (byte* kp = _keyValue, temp_opB = &opadB[0], temp_ipB = &ipadB[0])
                    fixed (ulong* op = &opad[0], ip = &ipad[0])
                    {
                        for (int i = 0; i < _keyValue.Length; i++)
                        {
                            temp_opB[i] = (byte)(kp[i] ^ 0x5c);
                            temp_ipB[i] = (byte)(kp[i] ^ 0x36);
                        }

                        for (int i = _keyValue.Length; i < opadB.Length; i++)
                        {
                            temp_opB[i] = 0 ^ 0x5c;
                            temp_ipB[i] = 0 ^ 0x36;
                        }

                        // Now copy the temp pad bytes into real pad UInt[]
                        // There are 16 items inside of a pad (Hash.BlockSize = 64 byte /4 = 16 uint)
                        for (int i = 0, j = 0; i < 16; i++, j += 8)
                        {
                            op[i] =
                                ((ulong)temp_opB[j] << 56) |
                                ((ulong)temp_opB[j + 1] << 48) |
                                ((ulong)temp_opB[j + 2] << 40) |
                                ((ulong)temp_opB[j + 3] << 32) |
                                ((ulong)temp_opB[j + 4] << 24) |
                                ((ulong)temp_opB[j + 5] << 16) |
                                ((ulong)temp_opB[j + 6] << 8) |
                                temp_opB[j + 7];

                            ip[i] =
                                ((ulong)temp_ipB[j] << 56) |
                                ((ulong)temp_ipB[j + 1] << 48) |
                                ((ulong)temp_ipB[j + 2] << 40) |
                                ((ulong)temp_ipB[j + 3] << 32) |
                                ((ulong)temp_ipB[j + 4] << 24) |
                                ((ulong)temp_ipB[j + 5] << 16) |
                                ((ulong)temp_ipB[j + 6] << 8) |
                                temp_ipB[j + 7];
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Computes the hash value for the specified byte array.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">The byte array to compute hash for</param>
        /// <param name="key">Key to use. Arrays smaller than block size (128 bytes) will be hashed first.</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHash(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(HmacSha512));
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key can not be null.");


            ulong* pt = stackalloc ulong[88];
            fixed (byte* kPt = key, dPt = data)
            fixed (ulong* oPt = &opad[0], iPt = &ipad[0])
            {
                if (key.Length > Sha512.BlockByteSize)
                {
                    Sha512.Init(pt);
                    Sha512.CompressData(kPt, key.Length, key.Length, pt, pt + 8);

                    for (int i = 0; i < 8; i++) // 8 items in HashState = 8*8 = 64 byte
                    {
                        iPt[i] = 0x3636363636363636U ^ pt[i];
                        oPt[i] = 0x5c5c5c5c5c5c5c5cU ^ pt[i];
                    }
                    for (int i = 8; i < 16; i++)
                    {
                        iPt[i] = 0x3636363636363636U;
                        oPt[i] = 0x5c5c5c5c5c5c5c5cU;
                    }
                }
                else
                {
                    byte[] temp = new byte[Sha512.BlockByteSize];
                    fixed (byte* tPt = &temp[0])
                    {
                        Buffer.MemoryCopy(kPt, tPt, temp.Length, key.Length);
                        for (int i = 0, j = 0; i < 16; i++, j += 8)
                        {
                            ulong val =
                                ((ulong)tPt[j] << 56) |
                                ((ulong)tPt[j + 1] << 48) |
                                ((ulong)tPt[j + 2] << 40) |
                                ((ulong)tPt[j + 3] << 32) |
                                ((ulong)tPt[j + 4] << 24) |
                                ((ulong)tPt[j + 5] << 16) |
                                ((ulong)tPt[j + 6] << 8) |
                                tPt[j + 7];

                            iPt[i] = 0x3636363636363636U ^ val;
                            oPt[i] = 0x5c5c5c5c5c5c5c5cU ^ val;
                        }
                    }
                }

                // Now based on key, the pads are set. 
                // We use pad fields as working vectors for SHA512 hash which also contain the data and act as blocks.

                // Final result is SHA512(outer_pad | SHA512(inner_pad | data))

                // 1. Compute SHA512(inner_pad | data)
                Sha512.Init(pt);
                Sha512.CompressBlock(pt, iPt);
                // Total data length is len + hashFunc.BlockByteSize
                Sha512.CompressData(dPt, data.Length, data.Length + Sha512.BlockByteSize, pt, pt + 8);

                // 2. Compute SHA512(outer_pad | hash)
                // Copy 64 bytes and fill unto index 7 in wPt
                *(Block64*)(pt + 8) = *(Block64*)pt;
                pt[16] = 0b10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000UL; // 1 followed by 0 bits: pad1
                pt[17] = 0;
                pt[18] = 0;
                pt[19] = 0;
                pt[20] = 0;
                pt[21] = 0;

                // The total data length is: oPad.Length(=128) + hashState.Lengh(=64) = 192 byte *8 = 1,536 bit
                pt[22] = 0;
                pt[23] = 1536;

                Sha512.Init(pt);
                Sha512.CompressBlock(pt, oPt);
                Sha512.CompressBlock(pt, pt + 8);

                return Sha512.GetBytes(pt);
            }
        }


        /// <summary>
        /// Computes the hash value for the specified byte array. Key must be set in constructor or by using the property setter.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="data">The byte array to compute hash for</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHash(ReadOnlySpan<byte> data)
        {
            if (disposedValue)
                throw new ObjectDisposedException($"{nameof(HmacSha512)} instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");
            if (_keyValue == null)
                throw new ArgumentNullException(nameof(Key), "Key must be set before calling this function");

            ulong* pt = stackalloc ulong[88];
            // Pads are already set
            fixed (byte* dPt = data)
            fixed (ulong* oPt = &opad[0], iPt = &ipad[0])
            {
                // Final result is SHA512(outer_pad | SHA512(inner_pad | data))

                // 1. Compute SHA512(inner_pad | data)
                Sha512.Init(pt);
                Sha512.CompressBlock(pt, iPt);
                // Total data length is len + hashFunc.BlockByteSize
                Sha512.CompressData(dPt, data.Length, data.Length + Sha512.BlockByteSize, pt, pt + 8);

                // 2. Compute SHA512(outer_pad | hash)
                // Copy 64 bytes and fill unto index 7 in wPt
                *(Block64*)(pt + 8) = *(Block64*)pt;
                pt[16] = 0b10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000UL; // 1 followed by 0 bits: pad1
                pt[17] = 0;
                pt[18] = 0;
                pt[19] = 0;
                pt[20] = 0;
                pt[21] = 0;

                // The total data length is: oPad.Length(=128) + hashState.Lengh(=64) = 192 byte *8 = 1,536 bit
                pt[22] = 0;
                pt[23] = 1536;

                Sha512.Init(pt);
                Sha512.CompressBlock(pt, oPt);
                Sha512.CompressBlock(pt, pt + 8);

                return Sha512.GetBytes(pt);
            }
        }


        private bool disposedValue = false;

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="HmacSha512"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!disposedValue)
            {
                if (!(_keyValue is null))
                    Array.Clear(_keyValue, 0, _keyValue.Length);
                _keyValue = null;

                if (!(ipad is null))
                    Array.Clear(ipad, 0, ipad.Length);
                ipad = null;

                if (!(opad is null))
                    Array.Clear(opad, 0, opad.Length);
                opad = null;

                disposedValue = true;
            }
        }
    }
}
