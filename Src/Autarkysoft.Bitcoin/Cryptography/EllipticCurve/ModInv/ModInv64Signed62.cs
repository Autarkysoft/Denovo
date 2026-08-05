// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve.ModInv
{
    /// <summary>
    /// A signed 62-bit limb representation of integers. Its value is sum(v[i] * 2^(62*i), i=0..4).
    /// </summary>
    public readonly struct ModInv64Signed62
    {
        public ModInv64Signed62(long a0, long a1, long a2, long a3, long a4)
        {
            v0 = a0; v1 = a1; v2 = a2; v3 = a3; v4 = a4;
        }

        public ModInv64Signed62(ReadOnlySpan<long> arr)
        {
            Debug.Assert(arr.Length == 5);
            v0 = arr[0]; v1 = arr[1]; v2 = arr[2]; v3 = arr[3]; v4 = arr[4];
        }

        public ModInv64Signed62(in UInt256_5x52 a)
        {
            v0 = (long)((a.b0 | a.b1 << 52) & M62);
            v1 = (long)((a.b1 >> 10 | a.b2 << 42) & M62);
            v2 = (long)((a.b2 >> 20 | a.b3 << 32) & M62);
            v3 = (long)((a.b3 >> 30 | a.b4 << 22) & M62);
            v4 = (long)((a.b4 >> 40) & M62);
        }

        public ModInv64Signed62(in Scalar4x64 a)
        {
#if DEBUG
            a.Verify();
#endif
            v0 = (long)(a.b0 & M62);
            v1 = (long)((a.b0 >> 62 | a.b1 << 2) & M62);
            v2 = (long)((a.b1 >> 60 | a.b2 << 4) & M62);
            v3 = (long)((a.b2 >> 58 | a.b3 << 6) & M62);
            v4 = (long)(a.b3 >> 56);
        }


        public readonly long v0, v1, v2, v3, v4;


        public long[] GetArray()
        {
            return new long[5] { v0, v1, v2, v3, v4 };
        }


        private const ulong M62 = ulong.MaxValue >> 2;

        private static readonly ModInv64Signed62 _zero = new ModInv64Signed62(0, 0, 0, 0, 0);
        private static readonly ModInv64Signed62 _one = new ModInv64Signed62(1, 0, 0, 0, 0);
        /// <summary>
        /// Zero
        /// </summary>
        public static ref readonly ModInv64Signed62 Zero => ref _zero;
        /// <summary>
        /// One
        /// </summary>
        public static ref readonly ModInv64Signed62 One => ref _one;


        public Scalar4x64 ToScalar4x64()
        {
            // The output from secp256k1_modinv64{_var} should be normalized to range [0,modulus), and
            // have limbs in [0,2^62). The modulus is < 2^256, so the top limb must be below 2^(256-62*4).
            Debug.Assert(v0 >> 62 == 0);
            Debug.Assert(v1 >> 62 == 0);
            Debug.Assert(v2 >> 62 == 0);
            Debug.Assert(v3 >> 62 == 0);
            Debug.Assert(v4 >> 8 == 0);

            ulong r0 = (ulong)(v0 | v1 << 62);
            ulong r1 = (ulong)(v1 >> 2 | v2 << 60);
            ulong r2 = (ulong)(v2 >> 4 | v3 << 58);
            ulong r3 = (ulong)(v3 >> 6 | v4 << 56);

            return new Scalar4x64(r0, r1, r2, r3);
        }


        public UInt256_5x52 ToUInt256_5x52()
        {
            const ulong M52 = ulong.MaxValue >> 12;

            // The output from secp256k1_modinv64{_var} should be normalized to range [0,modulus), and
            // have limbs in [0,2^62). The modulus is < 2^256, so the top limb must be below 2^(256-62*4).
            Debug.Assert(v0 >> 62 == 0);
            Debug.Assert(v1 >> 62 == 0);
            Debug.Assert(v2 >> 62 == 0);
            Debug.Assert(v3 >> 62 == 0);
            Debug.Assert(v4 >> 8 == 0);

            ulong r0 = (ulong)v0 & M52;
            ulong r1 = (ulong)(v0 >> 52 | v1 << 10) & M52;
            ulong r2 = (ulong)(v1 >> 42 | v2 << 20) & M52;
            ulong r3 = (ulong)(v2 >> 32 | v3 << 30) & M52;
            ulong r4 = (ulong)(v3 >> 22 | v4 << 40);

#if DEBUG
            int m = (r0 | r1 | r2 | r3 | r4) == 0 ? 0 : 1;
#endif

            return new UInt256_5x52(r0, r1, r2, r3, r4
#if DEBUG
                , m, true
#endif
                );
        }
    }
}
