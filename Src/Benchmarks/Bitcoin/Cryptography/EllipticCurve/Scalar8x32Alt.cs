// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Benchmarks.Bitcoin.Cryptography.EllipticCurve
{
    public readonly struct Scalar8x32Alt
    {
        public Scalar8x32Alt(uint u0, uint u1, uint u2, uint u3, uint u4, uint u5, uint u6, uint u7)
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3;
            b4 = u4; b5 = u5; b6 = u6; b7 = u7;
            Debug.Assert(CheckOverflow() == 0);
        }

        public Scalar8x32Alt(ReadOnlySpan<byte> data)
        {
            if (data.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(data));

            b0 = data[31] | ((uint)data[30] << 8) | ((uint)data[29] << 16) | ((uint)data[28] << 24);
            b1 = data[27] | ((uint)data[26] << 8) | ((uint)data[25] << 16) | ((uint)data[24] << 24);
            b2 = data[23] | ((uint)data[22] << 8) | ((uint)data[21] << 16) | ((uint)data[20] << 24);
            b3 = data[19] | ((uint)data[18] << 8) | ((uint)data[17] << 16) | ((uint)data[16] << 24);
            b4 = data[15] | ((uint)data[14] << 8) | ((uint)data[13] << 16) | ((uint)data[12] << 24);
            b5 = data[11] | ((uint)data[10] << 8) | ((uint)data[09] << 16) | ((uint)data[08] << 24);
            b6 = data[07] | ((uint)data[06] << 8) | ((uint)data[05] << 16) | ((uint)data[04] << 24);
            b7 = data[03] | ((uint)data[02] << 8) | ((uint)data[01] << 16) | ((uint)data[00] << 24);

            uint of = CheckOverflow();
            ulong t = (ulong)b0 + (of * NC0);
            b0 = (uint)t; t >>= 32;
            t += (ulong)b1 + (of * NC1);
            b1 = (uint)t; t >>= 32;
            t += (ulong)b2 + (of * NC2);
            b2 = (uint)t; t >>= 32;
            t += (ulong)b3 + (of * NC3);
            b3 = (uint)t; t >>= 32;
            t += (ulong)b4 + (of * NC4);
            b4 = (uint)t; t >>= 32;
            t += b5;
            b5 = (uint)t; t >>= 32;
            t += b6;
            b6 = (uint)t; t >>= 32;
            t += b7;
            b7 = (uint)t;
        }


        public readonly uint b0, b1, b2, b3, b4, b5, b6, b7;

        private const uint N0 = 0xD0364141U;
        private const uint N1 = 0xBFD25E8CU;
        private const uint N2 = 0xAF48A03BU;
        private const uint N3 = 0xBAAEDCE6U;
        private const uint N4 = 0xFFFFFFFEU;
        private const uint N5 = 0xFFFFFFFFU;
        private const uint N6 = 0xFFFFFFFFU;
        private const uint N7 = 0xFFFFFFFFU;

        private const uint NC0 = ~N0 + 1;
        private const uint NC1 = ~N1;
        private const uint NC2 = ~N2;
        private const uint NC3 = ~N3;
        private const uint NC4 = 1;

        private uint CheckOverflow()
        {
            uint yes = 0U;
            uint no = 0U;
            no |= (b7 < N7 ? 1U : 0U);
            no |= (b6 < N6 ? 1U : 0U);
            no |= (b5 < N5 ? 1U : 0U);
            no |= (b4 < N4 ? 1U : 0U);
            yes |= (b4 > N4 ? 1U : 0U) & ~no;
            no |= (b3 < N3 ? 1U : 0U) & ~yes;
            yes |= (b3 > N3 ? 1U : 0U) & ~no;
            no |= (b2 < N2 ? 1U : 0U) & ~yes;
            yes |= (b2 > N2 ? 1U : 0U) & ~no;
            no |= (b1 < N1 ? 1U : 0U) & ~yes;
            yes |= (b1 > N1 ? 1U : 0U) & ~no;
            yes |= (b0 >= N0 ? 1U : 0U) & ~no;
            return yes;
        }

        public Scalar8x32Alt Multiply(in Scalar8x32Alt b)
        {
            uint[] l = new uint[16];
            secp256k1_scalar_mul_512(l, this, b);
            return secp256k1_scalar_reduce_512(l);
        }

        private static void secp256k1_scalar_mul_512(uint[] l, in Scalar8x32Alt a,in Scalar8x32Alt b)
        {
            /* 96 bit accumulator. */
            uint c0 = 0, c1 = 0, c2 = 0;

            /* l[0..15] = a[0..7] * b[0..7]. */
            muladd_fast(a.b0, b.b0, ref c0, ref c1);
            extract_fast(ref l[0], ref c0, ref c1, ref c2);
            muladd(a.b0, b.b1, ref c0, ref c1, ref c2);
            muladd(a.b1, b.b0, ref c0, ref c1, ref c2);
            extract(ref l[1], ref c0, ref c1, ref c2);
            muladd(a.b0, b.b2, ref c0, ref c1, ref c2);
            muladd(a.b1, b.b1, ref c0, ref c1, ref c2);
            muladd(a.b2, b.b0, ref c0, ref c1, ref c2);
            extract(ref l[2], ref c0, ref c1, ref c2);
            muladd(a.b0, b.b3, ref c0, ref c1, ref c2);
            muladd(a.b1, b.b2, ref c0, ref c1, ref c2);
            muladd(a.b2, b.b1, ref c0, ref c1, ref c2);
            muladd(a.b3, b.b0, ref c0, ref c1, ref c2);
            extract(ref l[3], ref c0, ref c1, ref c2);
            muladd(a.b0, b.b4, ref c0, ref c1, ref c2);
            muladd(a.b1, b.b3, ref c0, ref c1, ref c2);
            muladd(a.b2, b.b2, ref c0, ref c1, ref c2);
            muladd(a.b3, b.b1, ref c0, ref c1, ref c2);
            muladd(a.b4, b.b0, ref c0, ref c1, ref c2);
            extract(ref l[4], ref c0, ref c1, ref c2);
            muladd(a.b0, b.b5, ref c0, ref c1, ref c2);
            muladd(a.b1, b.b4, ref c0, ref c1, ref c2);
            muladd(a.b2, b.b3, ref c0, ref c1, ref c2);
            muladd(a.b3, b.b2, ref c0, ref c1, ref c2);
            muladd(a.b4, b.b1, ref c0, ref c1, ref c2);
            muladd(a.b5, b.b0, ref c0, ref c1, ref c2);
            extract(ref l[5], ref c0, ref c1, ref c2);
            muladd(a.b0, b.b6, ref c0, ref c1, ref c2);
            muladd(a.b1, b.b5, ref c0, ref c1, ref c2);
            muladd(a.b2, b.b4, ref c0, ref c1, ref c2);
            muladd(a.b3, b.b3, ref c0, ref c1, ref c2);
            muladd(a.b4, b.b2, ref c0, ref c1, ref c2);
            muladd(a.b5, b.b1, ref c0, ref c1, ref c2);
            muladd(a.b6, b.b0, ref c0, ref c1, ref c2);
            extract(ref l[6], ref c0, ref c1, ref c2);
            muladd(a.b0, b.b7, ref c0, ref c1, ref c2);
            muladd(a.b1, b.b6, ref c0, ref c1, ref c2);
            muladd(a.b2, b.b5, ref c0, ref c1, ref c2);
            muladd(a.b3, b.b4, ref c0, ref c1, ref c2);
            muladd(a.b4, b.b3, ref c0, ref c1, ref c2);
            muladd(a.b5, b.b2, ref c0, ref c1, ref c2);
            muladd(a.b6, b.b1, ref c0, ref c1, ref c2);
            muladd(a.b7, b.b0, ref c0, ref c1, ref c2);
            extract(ref l[7], ref c0, ref c1, ref c2);
            muladd(a.b1, b.b7, ref c0, ref c1, ref c2);
            muladd(a.b2, b.b6, ref c0, ref c1, ref c2);
            muladd(a.b3, b.b5, ref c0, ref c1, ref c2);
            muladd(a.b4, b.b4, ref c0, ref c1, ref c2);
            muladd(a.b5, b.b3, ref c0, ref c1, ref c2);
            muladd(a.b6, b.b2, ref c0, ref c1, ref c2);
            muladd(a.b7, b.b1, ref c0, ref c1, ref c2);
            extract(ref l[8], ref c0, ref c1, ref c2);
            muladd(a.b2, b.b7, ref c0, ref c1, ref c2);
            muladd(a.b3, b.b6, ref c0, ref c1, ref c2);
            muladd(a.b4, b.b5, ref c0, ref c1, ref c2);
            muladd(a.b5, b.b4, ref c0, ref c1, ref c2);
            muladd(a.b6, b.b3, ref c0, ref c1, ref c2);
            muladd(a.b7, b.b2, ref c0, ref c1, ref c2);
            extract(ref l[9], ref c0, ref c1, ref c2);
            muladd(a.b3, b.b7, ref c0, ref c1, ref c2);
            muladd(a.b4, b.b6, ref c0, ref c1, ref c2);
            muladd(a.b5, b.b5, ref c0, ref c1, ref c2);
            muladd(a.b6, b.b4, ref c0, ref c1, ref c2);
            muladd(a.b7, b.b3, ref c0, ref c1, ref c2);
            extract(ref l[10], ref c0, ref c1, ref c2);
            muladd(a.b4, b.b7, ref c0, ref c1, ref c2);
            muladd(a.b5, b.b6, ref c0, ref c1, ref c2);
            muladd(a.b6, b.b5, ref c0, ref c1, ref c2);
            muladd(a.b7, b.b4, ref c0, ref c1, ref c2);
            extract(ref l[11], ref c0, ref c1, ref c2);
            muladd(a.b5, b.b7, ref c0, ref c1, ref c2);
            muladd(a.b6, b.b6, ref c0, ref c1, ref c2);
            muladd(a.b7, b.b5, ref c0, ref c1, ref c2);
            extract(ref l[12], ref c0, ref c1, ref c2);
            muladd(a.b6, b.b7, ref c0, ref c1, ref c2);
            muladd(a.b7, b.b6, ref c0, ref c1, ref c2);
            extract(ref l[13], ref c0, ref c1, ref c2);
            muladd_fast(a.b7, b.b7, ref c0, ref c1);
            extract_fast(ref l[14], ref c0, ref c1, ref c2);
            Debug.Assert(c1 == 0);
            l[15] = c0;
        }

        private static Scalar8x32Alt secp256k1_scalar_reduce_512(uint[] l)
        {
            ulong c;
            uint n0 = l[8], n1 = l[9], n2 = l[10], n3 = l[11], n4 = l[12], n5 = l[13], n6 = l[14], n7 = l[15];
            uint m0 = 0, m1 = 0, m2 = 0, m3 = 0, m4 = 0, m5 = 0, m6 = 0, m7 = 0, m8 = 0, m9 = 0, m10 = 0, m11 = 0, m12 = 0;
            uint p0 = 0, p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0, p6 = 0, p7 = 0, p8 = 0;

            /* 96 bit accumulator. */
            uint c0, c1, c2;

            /* Reduce 512 bits into 385. */
            /* m[0..12] = l[0..7] + n[0..7] * NC. */
            c0 = l[0]; c1 = 0; c2 = 0;
            muladd_fast(n0, NC0, ref c0, ref c1);
            extract_fast(ref m0, ref c0, ref c1, ref c2);
            sumadd_fast(l[1], ref c0, ref c1, ref c2);
            muladd(n1, NC0, ref c0, ref c1, ref c2);
            muladd(n0, NC1, ref c0, ref c1, ref c2);
            extract(ref m1, ref c0, ref c1, ref c2);
            sumadd(l[2], ref c0, ref c1, ref c2);
            muladd(n2, NC0, ref c0, ref c1, ref c2);
            muladd(n1, NC1, ref c0, ref c1, ref c2);
            muladd(n0, NC2, ref c0, ref c1, ref c2);
            extract(ref m2, ref c0, ref c1, ref c2);
            sumadd(l[3], ref c0, ref c1, ref c2);
            muladd(n3, NC0, ref c0, ref c1, ref c2);
            muladd(n2, NC1, ref c0, ref c1, ref c2);
            muladd(n1, NC2, ref c0, ref c1, ref c2);
            muladd(n0, NC3, ref c0, ref c1, ref c2);
            extract(ref m3, ref c0, ref c1, ref c2);
            sumadd(l[4], ref c0, ref c1, ref c2);
            muladd(n4, NC0, ref c0, ref c1, ref c2);
            muladd(n3, NC1, ref c0, ref c1, ref c2);
            muladd(n2, NC2, ref c0, ref c1, ref c2);
            muladd(n1, NC3, ref c0, ref c1, ref c2);
            sumadd(n0, ref c0, ref c1, ref c2);
            extract(ref m4, ref c0, ref c1, ref c2);
            sumadd(l[5], ref c0, ref c1, ref c2);
            muladd(n5, NC0, ref c0, ref c1, ref c2);
            muladd(n4, NC1, ref c0, ref c1, ref c2);
            muladd(n3, NC2, ref c0, ref c1, ref c2);
            muladd(n2, NC3, ref c0, ref c1, ref c2);
            sumadd(n1, ref c0, ref c1, ref c2);
            extract(ref m5, ref c0, ref c1, ref c2);
            sumadd(l[6], ref c0, ref c1, ref c2);
            muladd(n6, NC0, ref c0, ref c1, ref c2);
            muladd(n5, NC1, ref c0, ref c1, ref c2);
            muladd(n4, NC2, ref c0, ref c1, ref c2);
            muladd(n3, NC3, ref c0, ref c1, ref c2);
            sumadd(n2, ref c0, ref c1, ref c2);
            extract(ref m6, ref c0, ref c1, ref c2);
            sumadd(l[7], ref c0, ref c1, ref c2);
            muladd(n7, NC0, ref c0, ref c1, ref c2);
            muladd(n6, NC1, ref c0, ref c1, ref c2);
            muladd(n5, NC2, ref c0, ref c1, ref c2);
            muladd(n4, NC3, ref c0, ref c1, ref c2);
            sumadd(n3, ref c0, ref c1, ref c2);
            extract(ref m7, ref c0, ref c1, ref c2);
            muladd(n7, NC1, ref c0, ref c1, ref c2);
            muladd(n6, NC2, ref c0, ref c1, ref c2);
            muladd(n5, NC3, ref c0, ref c1, ref c2);
            sumadd(n4, ref c0, ref c1, ref c2);
            extract(ref m8, ref c0, ref c1, ref c2);
            muladd(n7, NC2, ref c0, ref c1, ref c2);
            muladd(n6, NC3, ref c0, ref c1, ref c2);
            sumadd(n5, ref c0, ref c1, ref c2);
            extract(ref m9, ref c0, ref c1, ref c2);
            muladd(n7, NC3, ref c0, ref c1, ref c2);
            sumadd(n6, ref c0, ref c1, ref c2);
            extract(ref m10, ref c0, ref c1, ref c2);
            sumadd_fast(n7, ref c0, ref c1, ref c2);
            extract_fast(ref m11, ref c0, ref c1, ref c2);
            Debug.Assert(c0 <= 1);
            m12 = c0;

            /* Reduce 385 bits into 258. */
            /* p[0..8] = m[0..7] + m[8..12] * NC. */
            c0 = m0; c1 = 0; c2 = 0;
            muladd_fast(m8, NC0, ref c0, ref c1);
            extract_fast(ref p0, ref c0, ref c1, ref c2);
            sumadd_fast(m1, ref c0, ref c1, ref c2);
            muladd(m9, NC0, ref c0, ref c1, ref c2);
            muladd(m8, NC1, ref c0, ref c1, ref c2);
            extract(ref p1, ref c0, ref c1, ref c2);
            sumadd(m2, ref c0, ref c1, ref c2);
            muladd(m10, NC0, ref c0, ref c1, ref c2);
            muladd(m9, NC1, ref c0, ref c1, ref c2);
            muladd(m8, NC2, ref c0, ref c1, ref c2);
            extract(ref p2, ref c0, ref c1, ref c2);
            sumadd(m3, ref c0, ref c1, ref c2);
            muladd(m11, NC0, ref c0, ref c1, ref c2);
            muladd(m10, NC1, ref c0, ref c1, ref c2);
            muladd(m9, NC2, ref c0, ref c1, ref c2);
            muladd(m8, NC3, ref c0, ref c1, ref c2);
            extract(ref p3, ref c0, ref c1, ref c2);
            sumadd(m4, ref c0, ref c1, ref c2);
            muladd(m12, NC0, ref c0, ref c1, ref c2);
            muladd(m11, NC1, ref c0, ref c1, ref c2);
            muladd(m10, NC2, ref c0, ref c1, ref c2);
            muladd(m9, NC3, ref c0, ref c1, ref c2);
            sumadd(m8, ref c0, ref c1, ref c2);
            extract(ref p4, ref c0, ref c1, ref c2);
            sumadd(m5, ref c0, ref c1, ref c2);
            muladd(m12, NC1, ref c0, ref c1, ref c2);
            muladd(m11, NC2, ref c0, ref c1, ref c2);
            muladd(m10, NC3, ref c0, ref c1, ref c2);
            sumadd(m9, ref c0, ref c1, ref c2);
            extract(ref p5, ref c0, ref c1, ref c2);
            sumadd(m6, ref c0, ref c1, ref c2);
            muladd(m12, NC2, ref c0, ref c1, ref c2);
            muladd(m11, NC3, ref c0, ref c1, ref c2);
            sumadd(m10, ref c0, ref c1, ref c2);
            extract(ref p6, ref c0, ref c1, ref c2);
            sumadd_fast(m7, ref c0, ref c1, ref c2);
            muladd_fast(m12, NC3, ref c0, ref c1);
            sumadd_fast(m11, ref c0, ref c1, ref c2);
            extract_fast(ref p7, ref c0, ref c1, ref c2);
            p8 = c0 + m12;
            Debug.Assert(p8 <= 2);

            /* Reduce 258 bits into 256. */
            /* r[0..7] = p[0..7] + p[8] * NC. */
            c = p0 + (ulong)NC0 * p8;
            p0 = (uint)c; c >>= 32;
            c += p1 + (ulong)NC1 * p8;
            p1 = (uint)c; c >>= 32;
            c += p2 + (ulong)NC2 * p8;
            p2 = (uint)c; c >>= 32;
            c += p3 + (ulong)NC3 * p8;
            p3 = (uint)c; c >>= 32;
            c += p4 + (ulong)p8;
            p4 = (uint)c; c >>= 32;
            c += p5;
            p5 = (uint)c; c >>= 32;
            c += p6;
            p6 = (uint)c; c >>= 32;
            c += p7;
            p7 = (uint)c; c >>= 32;

            Scalar8x32Alt r = new(p0, p1, p2, p3, p4, p5, p6, p7);

            /* Final reduction of r. */
            secp256k1_scalar_reduce(ref r, (uint)c + GetOverflow(r));

            return r;
        }

        private static uint GetOverflow(in Scalar8x32Alt d)
        {
            uint yes = 0;
            uint no = 0;
            no |= (d.b7 < N7 ? 1U : 0);
            no |= (d.b6 < N6 ? 1U : 0);
            no |= (d.b5 < N5 ? 1U : 0);
            no |= (d.b4 < N4 ? 1U : 0);
            yes |= (d.b4 > N4 ? 1U : 0) & ~no;
            no |= (d.b3 < N3 ? 1U : 0) & ~yes;
            yes |= (d.b3 > N3 ? 1U : 0) & ~no;
            no |= (d.b2 < N2 ? 1U : 0) & ~yes;
            yes |= (d.b2 > N2 ? 1U : 0) & ~no;
            no |= (d.b1 < N1 ? 1U : 0) & ~yes;
            yes |= (d.b1 > N1 ? 1U : 0) & ~no;
            yes |= (d.b0 >= N0 ? 1U : 0) & ~no;
            return yes;
        }

        private static void secp256k1_scalar_reduce(ref Scalar8x32Alt r, uint overflow)
        {
            ulong t;
            Debug.Assert(overflow <= 1);
            t = (ulong)r.b0 + (overflow * NC0);
            uint r0 = (uint)t; t >>= 32;
            t += (ulong)r.b1 + (overflow * NC1);
            uint r1 = (uint)t; t >>= 32;
            t += (ulong)r.b2 + (overflow * NC2);
            uint r2 = (uint)t; t >>= 32;
            t += (ulong)r.b3 + (overflow * NC3);
            uint r3 = (uint)t; t >>= 32;
            t += (ulong)r.b4 + (overflow * NC4);
            uint r4 = (uint)t; t >>= 32;
            t += r.b5;
            uint r5 = (uint)t; t >>= 32;
            t += r.b6;
            uint r6 = (uint)t; t >>= 32;
            t += r.b7;
            uint r7 = (uint)t;
            r = new Scalar8x32Alt(r0, r1, r2, r3, r4, r5, r6, r7);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void muladd(uint a, uint b, ref uint c0, ref uint c1, ref uint c2)
        {
            uint tl, th;
            {
                ulong t = (ulong)a * b;
                th = (uint)(t >> 32);         /* at most 0xFFFFFFFE */
                tl = (uint)t;
            }
            c0 += tl;                 /* overflow is handled on the next line */
            th += (c0 < tl ? 1U : 0);          /* at most 0xFFFFFFFF */
            c1 += th;                 /* overflow is handled on the next line */
            c2 += (c1 < th ? 1U : 0);          /* never overflows by contract (verified in the next line) */
            Debug.Assert((c1 >= th) || (c2 != 0));
        }

        /** Add a*b to the number defined by (c0,c1). c1 must never overflow. */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void muladd_fast(uint a, uint b, ref uint c0, ref uint c1)
        {
            uint tl, th;
            {
                ulong t = (ulong)a * b;
                th = (uint)(t >> 32);         /* at most 0xFFFFFFFE */
                tl = (uint)t;
            }
            c0 += tl;                 /* overflow is handled on the next line */
            th += (c0 < tl ? 1U : 0);          /* at most 0xFFFFFFFF */
            c1 += th;                 /* never overflows by contract (verified in the next line) */
            Debug.Assert(c1 >= th);
        }

        /** Add a to the number defined by (c0,c1,c2). c2 must never overflow. */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void sumadd(uint a, ref uint c0, ref uint c1, ref uint c2)
        {
            uint over;
            c0 += (a);                  /* overflow is handled on the next line */
            over = (c0 < (a) ? 1U : 0);
            c1 += over;                 /* overflow is handled on the next line */
            c2 += (c1 < over ? 1U : 0);          /* never overflows by contract */
        }

        /** Add a to the number defined by (c0,c1). c1 must never overflow, c2 must be zero. */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void sumadd_fast(uint a, ref uint c0, ref uint c1, ref uint c2)
        {
            c0 += (a);                 /* overflow is handled on the next line */
            c1 += (c0 < (a) ? 1U : 0);          /* never overflows by contract (verified the next line) */
            Debug.Assert((c1 != 0) | (c0 >= (a)));
            Debug.Assert(c2 == 0);
        }

        /** Extract the lowest 32 bits of (c0,c1,c2) into n, and left shift the number 32 bits. */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void extract(ref uint n, ref uint c0, ref uint c1, ref uint c2)
        {
            (n) = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
        }

        /** Extract the lowest 32 bits of (c0,c1,c2) into n, and left shift the number 32 bits. c2 is required to be zero. */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void extract_fast(ref uint n, ref uint c0, ref uint c1, ref uint c2)
        {
            (n) = c0;
            c0 = c1;
            c1 = 0;
            Debug.Assert(c2 == 0);
        }


        public bool Equals(Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Scalar8x32 right)
        {
            return ((b0 ^ right.b0) | (b1 ^ right.b1) | (b2 ^ right.b2) | (b3 ^ right.b3) |
             (b4 ^ right.b4) | (b5 ^ right.b5) | (b6 ^ right.b6) | (b7 ^ right.b7)) == 0;
        }
    }
}
