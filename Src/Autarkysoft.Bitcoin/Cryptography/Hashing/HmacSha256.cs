﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Cryptography.Hashing
{
    /// <summary>
    /// Computes a Hash-based Message Authentication Code (HMAC) by using the <see cref="Sha256"/> hash function.
    /// Based on RFC-2104
    /// <para/> https://tools.ietf.org/html/rfc2104
    /// </summary>
    public sealed class HmacSha256 : IHmacFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HmacSha256"/>. 
        /// <para/> Useful for using the same instance for computing HMAC each time with a different key 
        /// by calling <see cref="ComputeHash(ReadOnlySpan{byte}, ReadOnlySpan{byte})"/>
        /// </summary>
        public HmacSha256()
        {
            hashFunc = new Sha256();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HmacSha256"/> using the given key.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="key">Key to use, if the key is bigger than 64 bytes it will be hashed using <see cref="Sha256"/></param>
        public HmacSha256(byte[] key)
        {
            hashFunc = new Sha256();
            Key = key;
        }



        private Sha256 hashFunc;

        /// <inheritdoc/>
        public int BlockSize => 64;
        /// <inheritdoc/>
        public int OutputSize => 32;

        // these pads are supposed to be used as working vector of SHA256 hence the size=64
        private uint[] opad = new uint[64];
        private uint[] ipad = new uint[64];
        private byte[] _keyValue;



        /// <summary>
        /// Gets and sets key used for computing Hash-based Message Authentication Code (HMAC).
        /// Keys bigger than block size (64 bytes) will be hashed using <see cref="Sha256"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public byte[] Key
        {
            get => _keyValue;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Key can not be null.");


                if (value.Length > Sha256.BlockByteSize)
                {
                    _keyValue = hashFunc.ComputeHash(value);
                }
                else
                {
                    _keyValue = value.CloneByteArray();
                }

                unsafe
                {
                    // In order to set pads we have to XOR key with pad values. 
                    // Since we don't know the length of the key, it is harder to loop using UInt64 so we use 2 temp pad bytes:
                    byte[] opadB = new byte[Sha256.BlockByteSize];
                    byte[] ipadB = new byte[Sha256.BlockByteSize];

                    // Note (kp = _keyValue) can't assign to first item because key might be empty array which will throw an excpetion
                    fixed (byte* kp = _keyValue, tOpB = &opadB[0], tIpB = &ipadB[0])
                    fixed (uint* op = &opad[0], ip = &ipad[0])
                    {
                        for (int i = 0; i < _keyValue.Length; i++)
                        {
                            tOpB[i] = (byte)(kp[i] ^ 0x5c);
                            tIpB[i] = (byte)(kp[i] ^ 0x36);
                        }

                        for (int i = _keyValue.Length; i < opadB.Length; i++)
                        {
                            tOpB[i] = 0 ^ 0x5c;
                            tIpB[i] = 0 ^ 0x36;
                        }

                        // Now copy the temp pad bytes into real pad UInt[]
                        // There are 16 items inside of a pad (Hash.BlockSize = 64 byte /4 = 16 uint)
                        for (int i = 0, j = 0; i < 16; i++, j += 4)
                        {
                            op[i] = (uint)((tOpB[j] << 24) | (tOpB[j + 1] << 16) | (tOpB[j + 2] << 8) | tOpB[j + 3]);
                            ip[i] = (uint)((tIpB[j] << 24) | (tIpB[j + 1] << 16) | (tIpB[j + 2] << 8) | tIpB[j + 3]);
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
        /// <param name="key">Key to use. Arrays smaller than block size (64 bytes) will be hashed first.</param>
        /// <returns>The computed hash</returns>
        public unsafe byte[] ComputeHash(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(HmacSha256));
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key can not be null.");


            fixed (byte* kPt = key, dPt = data)
            fixed (uint* oPt = &opad[0], iPt = &ipad[0])
            fixed (uint* hPt = &hashFunc.hashState[0], wPt = &hashFunc.w[0])
            {
                if (key.Length > Sha256.BlockByteSize)
                {
                    hashFunc.Init(hPt);
                    hashFunc.CompressData(kPt, key.Length, key.Length, hPt, wPt);

                    for (int i = 0; i < 8; i++) // 8 items in HashState = 8*4 = 32 byte
                    {
                        iPt[i] = 0x36363636U ^ hPt[i];
                        oPt[i] = 0x5c5c5c5cU ^ hPt[i];
                    }
                    for (int i = 8; i < 16; i++)
                    {
                        iPt[i] = 0x36363636U;
                        oPt[i] = 0x5c5c5c5cU;
                    }
                }
                else
                {
                    byte[] temp = new byte[Sha256.BlockByteSize];
                    fixed (byte* tPt = &temp[0])
                    {
                        Buffer.MemoryCopy(kPt, tPt, temp.Length, key.Length);
                        for (int i = 0, j = 0; i < 16; i++, j += 4)
                        {
                            uint val = (uint)((tPt[j] << 24) | (tPt[j + 1] << 16) | (tPt[j + 2] << 8) | tPt[j + 3]);
                            iPt[i] = 0x36363636U ^ val;
                            oPt[i] = 0x5c5c5c5cU ^ val;
                        }
                    }
                }

                // Now based on key, the pads are set. 
                // We use pad fields as working vectors for SHA256 hash which also contain the data and act as blocks.

                // Final result is SHA256(outer_pad | SHA256(inner_pad | data))

                // 1. Compute SHA256(inner_pad | data)
                hashFunc.Init(hPt);
                hashFunc.CompressBlock(hPt, iPt);
                // Total data length is len + hashFunc.BlockByteSize
                hashFunc.CompressData(dPt, data.Length, data.Length + 64, hPt, wPt);

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

                hashFunc.Init(hPt);
                hashFunc.CompressBlock(hPt, oPt);
                hashFunc.CompressBlock(hPt, wPt);

                return hashFunc.GetBytes(hPt);
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
                throw new ObjectDisposedException($"{nameof(HmacSha256)} instance was disposed.");
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data can not be null.");
            if (_keyValue == null)
                throw new ArgumentNullException(nameof(Key), "Key must be set before calling this function.");


            // Pads are already set
            fixed (byte* dPt = data)
            fixed (uint* oPt = &opad[0], iPt = &ipad[0])
            fixed (uint* hPt = &hashFunc.hashState[0], wPt = &hashFunc.w[0])
            {
                // Final result is SHA256(outer_pad | SHA256(inner_pad | data))

                // 1. Compute SHA256(inner_pad | data)
                hashFunc.Init(hPt);
                hashFunc.CompressBlock(hPt, iPt);
                // Total data length is len + hashFunc.BlockByteSize
                hashFunc.CompressData(dPt, data.Length, data.Length + 64, hPt, wPt);

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

                hashFunc.Init(hPt);
                hashFunc.CompressBlock(hPt, oPt);
                hashFunc.CompressBlock(hPt, wPt);

                return hashFunc.GetBytes(hPt);
            }
        }


        private bool disposedValue = false;

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="HmacSha256"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!disposedValue)
            {
                if (!(hashFunc is null))
                    hashFunc.Dispose();
                hashFunc = null;

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
