// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    public readonly struct ModInv32Signed30
    {
        public ModInv32Signed30(in Scalar8x32 a)
        {
            Debug.Assert(a.Verify());

            v0 = (int)(a.b0 & M30);
            v1 = (int)((a.b0 >> 30 | a.b1 << 2) & M30);
            v2 = (int)((a.b1 >> 28 | a.b2 << 4) & M30);
            v3 = (int)((a.b2 >> 26 | a.b3 << 6) & M30);
            v4 = (int)((a.b3 >> 24 | a.b4 << 8) & M30);
            v5 = (int)((a.b4 >> 22 | a.b5 << 10) & M30);
            v6 = (int)((a.b5 >> 20 | a.b6 << 12) & M30);
            v7 = (int)((a.b6 >> 18 | a.b7 << 14) & M30);
            v8 = (int)(a.b7 >> 16);
        }

        public ModInv32Signed30(int a0, int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8)
        {
            v0 = a0; v1 = a1; v2 = a2; v3 = a3;
            v4 = a4; v5 = a5; v6 = a6; v7 = a7; v8 = a8;
        }

        public ModInv32Signed30(ReadOnlySpan<int> arr)
        {
            Debug.Assert(arr.Length == 9);
            v0 = arr[0]; v1 = arr[1]; v2 = arr[2]; v3 = arr[3];
            v4 = arr[4]; v5 = arr[5]; v6 = arr[6]; v7 = arr[7]; v8 = arr[8];
        }

        private const uint M30 = uint.MaxValue >> 2;
        public readonly int v0, v1, v2, v3, v4, v5, v6, v7, v8;

        public int[] GetArray()
        {
            return new int[9] { v0, v1, v2, v3, v4, v5, v6, v7, v8 };
        }


        private static readonly ModInv32Signed30 _zero = new ModInv32Signed30(0, 0, 0, 0, 0, 0, 0, 0, 0);
        private static readonly ModInv32Signed30 _one = new ModInv32Signed30(1, 0, 0, 0, 0, 0, 0, 0, 0);
        /// <summary>
        /// Zero
        /// </summary>
        public static ref readonly ModInv32Signed30 Zero => ref _zero;
        /// <summary>
        /// One
        /// </summary>
        public static ref readonly ModInv32Signed30 One => ref _one;


        public Scalar8x32 ToScalar8x32()
        {
            // The output from secp256k1_modinv32{_var} should be normalized to range [0,modulus), and
            // have limbs in [0,2^30). The modulus is < 2^256, so the top limb must be below 2^(256-30*8).
            Debug.Assert(v0 >> 30 == 0);
            Debug.Assert(v1 >> 30 == 0);
            Debug.Assert(v2 >> 30 == 0);
            Debug.Assert(v3 >> 30 == 0);
            Debug.Assert(v4 >> 30 == 0);
            Debug.Assert(v5 >> 30 == 0);
            Debug.Assert(v6 >> 30 == 0);
            Debug.Assert(v7 >> 30 == 0);
            Debug.Assert(v8 >> 16 == 0);

            uint r0 = (uint)(v0 | v1 << 30);
            uint r1 = (uint)(v1 >> 2 | v2 << 28);
            uint r2 = (uint)(v2 >> 4 | v3 << 26);
            uint r3 = (uint)(v3 >> 6 | v4 << 24);
            uint r4 = (uint)(v4 >> 8 | v5 << 22);
            uint r5 = (uint)(v5 >> 10 | v6 << 20);
            uint r6 = (uint)(v6 >> 12 | v7 << 18);
            uint r7 = (uint)(v7 >> 14 | v8 << 16);

            Scalar8x32 result = new Scalar8x32(r0, r1, r2, r3, r4, r5, r6, r7);
            Debug.Assert(result.Verify());
            return result;
        }
    }
}
