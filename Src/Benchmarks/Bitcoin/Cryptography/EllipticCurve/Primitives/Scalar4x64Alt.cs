// Autarkysoft Benchmarks
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;

namespace Benchmarks.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    public readonly struct Scalar4x64Alt : IEquatable<Scalar4x64Alt>
    {
        public Scalar4x64Alt(ulong u0, ulong u1, ulong u2, ulong u3)
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3;
        }

        public Scalar4x64Alt(ReadOnlySpan<byte> data, out bool overflow)
        {
            b0 = data[31]
                | ((ulong)data[30] << 8)
                | ((ulong)data[29] << 16)
                | ((ulong)data[28] << 24)
                | ((ulong)data[27] << 32)
                | ((ulong)data[26] << 40)
                | ((ulong)data[25] << 48)
                | ((ulong)data[24] << 56);
            b1 = data[23]
                | ((ulong)data[22] << 8)
                | ((ulong)data[21] << 16)
                | ((ulong)data[20] << 24)
                | ((ulong)data[19] << 32)
                | ((ulong)data[18] << 40)
                | ((ulong)data[17] << 48)
                | ((ulong)data[16] << 56);
            b2 = data[15]
                | ((ulong)data[14] << 8)
                | ((ulong)data[13] << 16)
                | ((ulong)data[12] << 24)
                | ((ulong)data[11] << 32)
                | ((ulong)data[10] << 40)
                | ((ulong)data[9] << 48)
                | ((ulong)data[8] << 56);
            b3 = data[7]
                | ((ulong)data[6] << 8)
                | ((ulong)data[5] << 16)
                | ((ulong)data[4] << 24)
                | ((ulong)data[3] << 32)
                | ((ulong)data[2] << 40)
                | ((ulong)data[1] << 48)
                | ((ulong)data[0] << 56);

            uint of = 0U;
            uint no = 0U;
            no |= (b3 < N3 ? 1U : 0U); // No need for a > check.
            no |= (b2 < N2 ? 1U : 0U);
            of |= (b2 > N2 ? 1U : 0U) & ~no;
            no |= (b1 < N1 ? 1U : 0U);
            of |= (b1 > N1 ? 1U : 0U) & ~no;
            of |= (b0 >= N0 ? 1U : 0U) & ~no;

            ulong low = b0 + (of * NC0);
            ulong high = low < b0 ? 1UL : 0UL;
            b0 = low;
            // t >>= 64 -> low = high; high = 0;
            low = high + b1 + (of * NC1);
            high = low < b1 ? 1UL : 0UL;
            b1 = low;
            // t >>= 64 -> low = high; high = 0;
            low = high + b2 + (of * NC2);
            high = low < b2 ? 1UL : 0UL;
            b2 = low;
            b3 += high;

            overflow = of != 0;
        }



        public readonly ulong b0, b1, b2, b3;

        private const ulong N0 = 0xBFD25E8CD0364141UL;
        private const ulong N1 = 0xBAAEDCE6AF48A03BUL;
        private const ulong N2 = 0xFFFFFFFFFFFFFFFEUL;
        private const ulong N3 = 0xFFFFFFFFFFFFFFFFUL;
        private const ulong NC0 = ~N0 + 1;
        private const ulong NC1 = ~N1;
        private const ulong NC2 = 1;
        private const ulong NH0 = 0xDFE92F46681B20A0UL;
        private const ulong NH1 = 0x5D576E7357A4501DUL;
        private const ulong NH2 = 0xFFFFFFFFFFFFFFFFUL;
        private const ulong NH3 = 0x7FFFFFFFFFFFFFFFUL;





        public Scalar4x64Alt Add(in Scalar4x64Alt other, out bool overflow)
        {
            ulong r0 = b0 + other.b0;
            ulong high = r0 < b0 ? 1UL : 0UL;

            ulong r1 = b1 + high;
            high = r1 < b1 ? 1UL : 0UL;
            r1 += other.b1;
            high = (high == 0 && r1 >= other.b1) ? 0 : 1UL;

            ulong r2 = b2 + high;
            high = r2 < b2 ? 1UL : 0UL;
            r2 += other.b2;
            high = (high == 0 && r2 >= other.b2) ? 0 : 1UL;

            ulong r3 = b3 + high;
            high = r3 < b3 ? 1UL : 0UL;
            r3 += other.b3;
            high = (high == 0 && r3 >= other.b3) ? 0 : 1UL;

            // Compute overflow
            uint of = 0U;
            uint no = 0U;
            no |= (r3 < N3 ? 1U : 0U); // No need for a > check.
            no |= (r2 < N2 ? 1U : 0U);
            of |= (r2 > N2 ? 1U : 0U) & ~no;
            no |= (r1 < N1 ? 1U : 0U);
            of |= (r1 > N1 ? 1U : 0U) & ~no;
            of |= (r0 >= N0 ? 1U : 0U) & ~no;
            of += (uint)high;
            overflow = of != 0;

            // Reduce
            r0 = r0 + (of * NC0);
            high = r0 < (of * NC0) ? 1UL : 0UL;

            r1 = r1 + high;
            high = r1 < high ? 1UL : 0UL;
            r1 += of * NC1;
            high = (high == 0 && r1 >= of * NC1) ? 0 : 1UL;

            r2 = r2 + high;
            high = r2 < high ? 1UL : 0UL;
            r2 += of * NC2;
            high = (high == 0 && r2 >= of * NC2) ? 0 : 1UL;

            r3 = r3 + high;

            return new Scalar4x64Alt(r0, r1, r2, r3);
        }


        public Scalar4x64Alt Multiply(in Scalar4x64Alt b)
        {
            UInt128 t;
            ulong th, tl;
            ulong over;
            // 160 bit accumulator.
            ulong c0, c1, c2;
            ulong l0, l1, l2, l3, l4, l5, l6, l7;

            // l8[0..7] = a[0..3] * b[0..3].
            t = (UInt128)b0 * b.b0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 = tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 = th;

            Debug.Assert(c1 >= th);
            l0 = c0;
            c0 = c1;
            c1 = 0;
            // Debug.Assert(c2 == 0);
            t = (UInt128)b0 * b.b1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 = (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b1 * b.b0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            l1 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            t = (UInt128)b0 * b.b2;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b1 * b.b1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b2 * b.b0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            l2 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            t = (UInt128)b0 * b.b3;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b1 * b.b2;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b2 * b.b1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b3 * b.b0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            l3 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            t = (UInt128)b1 * b.b3;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b2 * b.b2;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b3 * b.b1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            l4 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            t = (UInt128)b2 * b.b3;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)b3 * b.b2;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            l5 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            t = (UInt128)b3 * b.b3;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;

            Debug.Assert(c1 >= th);
            l6 = c0;
            c0 = c1;
            c1 = 0;
            Debug.Assert(c2 == 0);
            Debug.Assert(c1 == 0);
            l7 = c0;
            UInt128 c128;
            ulong c;
            ulong n0 = l4, n1 = l5, n2 = l6, n3 = l7;
            ulong m0 = 0, m1 = 0, m2 = 0, m3 = 0, m4 = 0, m5 = 0;
            ulong m6;
            ulong p0 = 0, p1 = 0, p2 = 0, p3 = 0;
            ulong p4;
            c0 = l0; c1 = 0; c2 = 0;
            t = (UInt128)n0 * NC0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;

            Debug.Assert(c1 >= th);
            m0 = c0;
            c0 = c1;
            c1 = 0;
            Debug.Assert(c2 == 0);
            c0 += l1;
            c1 += (c0 < l1) ? 1UL : 0UL;
            Debug.Assert((c1 != 0) | (c0 >= l1));
            Debug.Assert(c2 == 0);
            t = (UInt128)n1 * NC0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)n0 * NC1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            m1 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            c0 += l2;
            over = (c0 < l2) ? 1UL : 0UL;
            c1 += over;
            c2 += (c1 < over) ? 1UL : 0UL;
            t = (UInt128)n2 * NC0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)n1 * NC1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            c0 += n0;
            over = (c0 < n0) ? 1UL : 0UL;
            c1 += over;
            c2 += (c1 < over) ? 1UL : 0UL;
            m2 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            c0 += l3;
            over = (c0 < l3) ? 1UL : 0UL;
            c1 += over;
            c2 += (c1 < over) ? 1UL : 0UL;
            t = (UInt128)n3 * NC0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)n2 * NC1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            c0 += n1;
            over = (c0 < n1) ? 1UL : 0UL;
            c1 += over;
            c2 += (c1 < over) ? 1UL : 0UL;
            m3 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            t = (UInt128)n3 * NC1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            c0 += n2;
            over = (c0 < n2) ? 1UL : 0UL;
            c1 += over;
            c2 += (c1 < over) ? 1UL : 0UL;
            m4 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            c0 += n3;
            c1 += (c0 < n3) ? 1UL : 0UL;
            Debug.Assert((c1 != 0) | (c0 >= n3));
            Debug.Assert(c2 == 0);
            m5 = c0;
            c0 = c1;
            c1 = 0;
            Debug.Assert(c2 == 0);
            Debug.Assert(c0 <= 1);
            m6 = c0;
            c0 = m0; c1 = 0; c2 = 0;
            t = (UInt128)m4 * NC0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;

            Debug.Assert(c1 >= th);
            p0 = c0;
            c0 = c1;
            c1 = 0;
            Debug.Assert(c2 == 0);
            c0 += m1;
            c1 += (c0 < m1) ? 1UL : 0UL;
            Debug.Assert((c1 != 0) | (c0 >= m1));
            Debug.Assert(c2 == 0);
            t = (UInt128)m5 * NC0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)m4 * NC1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            p1 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            c0 += m2;
            over = (c0 < m2) ? 1UL : 0UL;
            c1 += over;
            c2 += (c1 < over) ? 1UL : 0UL;
            t = (UInt128)m6 * NC0;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            t = (UInt128)m5 * NC1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;
            c2 += (c1 < th) ? 1U : 0U;
            Debug.Assert((c1 >= th) || (c2 != 0));
            c0 += m4;
            over = (c0 < m4) ? 1UL : 0UL;
            c1 += over;
            c2 += (c1 < over) ? 1UL : 0UL;
            p2 = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
            c0 += m3;
            c1 += (c0 < m3) ? 1UL : 0UL;
            Debug.Assert((c1 != 0) | (c0 >= m3));
            Debug.Assert(c2 == 0);
            t = (UInt128)m6 * NC1;
            th = (ulong)(t >> 64);
            tl = (ulong)t;
            c0 += tl;
            th += (c0 < tl) ? 1U : 0U;
            c1 += th;

            Debug.Assert(c1 >= th);
            c0 += m5;
            c1 += (c0 < m5) ? 1UL : 0UL;
            Debug.Assert((c1 != 0) | (c0 >= m5));
            Debug.Assert(c2 == 0);
            p3 = c0;
            c0 = c1;
            c1 = 0;
            Debug.Assert(c2 == 0);
            p4 = c0 + m6;
            Debug.Assert(p4 <= 2);
            c128 = (UInt128)p0 + ((UInt128)NC0 * p4);
            p0 = (ulong)c128; c128 >>= 64;
            c128 += (UInt128)p1 + ((UInt128)NC1 * p4);
            p1 = (ulong)c128; c128 >>= 64;
            c128 += (UInt128)p2 + (UInt128)p4;
            p2 = (ulong)c128; c128 >>= 64;
            c128 += (UInt128)p3;
            p3 = (ulong)c128;
            c = (ulong)(c128 >> 64);

            // Final reduction of r
            // GetOverflow()
            uint of = 0U;
            uint no = 0U;
            no |= (p3 < N3 ? 1U : 0U); // No need for a > check.
            no |= (p2 < N2 ? 1U : 0U);
            of |= (p2 > N2 ? 1U : 0U) & ~no;
            no |= (p1 < N1 ? 1U : 0U);
            of |= (p1 > N1 ? 1U : 0U) & ~no;
            of |= (p0 >= N0 ? 1U : 0U) & ~no;

            of += (uint)c;

            // Reduce(r, overflow);
            Debug.Assert(of <= 1);

            ulong low = p0 + (of * NC0);
            ulong high = low < p0 ? 1UL : 0UL;
            p0 = low;
            // t >>= 64 -> low = high; high = 0;
            low = high + p1 + (of * NC1);
            high = low < p1 ? 1UL : 0UL;
            p1 = low;
            // t >>= 64 -> low = high; high = 0;
            low = high + p2 + (of * NC2);
            high = low < p2 ? 1UL : 0UL;
            p2 = low;
            p3 += high;

            Scalar4x64Alt result = new(p0, p1, p2, p3);
            return result;
        }




        //public Scalar4x64Alt Half()
        //{
        //    ulong mask = (ulong)-(long)(b0 & 1U);
        //    ulong temp = (NH0 + 1UL) & mask;
        //    ulong low = ((b0 >> 1) | (b1 << 63)) + temp;
        //    ulong high = low < temp ? 1UL : 0UL;
        //    ulong r0 = low;

        //    low = high;

        //    temp = NH1 & mask;
        //    low = ((b1 >> 1) | (b2 << 63)) + temp;

        //    UInt128 t = new UInt128(0, (b0 >> 1) | (b1 << 63)) + new UInt128(0, (NH0 + 1UL) & mask);
        //    ulong r0 = (ulong)t; t >>= 64;

        //    t += new UInt128(0, (b1 >> 1) | (b2 << 63)) + new UInt128(0, NH1 & mask);
        //    ulong r1 = (ulong)t; t >>= 64;
        //    t += new UInt128(0, (b2 >> 1) | (b3 << 63)) + new UInt128(0, NH2 & mask);
        //    ulong r2 = (ulong)t; t >>= 64;
        //    ulong r3 = (ulong)t + (b3 >> 1) + (NH3 & mask);

        //    Scalar4x64Alt result = new Scalar4x64Alt(r0, r1, r2, r3);
        //    return result;
        //}


        public bool Equals(in Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives.Scalar4x64 other)
        {
            return ((b0 ^ other.b0) | (b1 ^ other.b1) | (b2 ^ other.b2) | (b3 ^ other.b3)) == 0;
        }
        public bool Equals(Scalar4x64Alt other) => this == other;
        public override bool Equals(object? obj) => obj is Scalar4x64Alt other && this == other;
        public static bool operator ==(in Scalar4x64Alt left, in Scalar4x64Alt right)
        {
            return ((left.b0 ^ right.b0) | (left.b1 ^ right.b1) | (left.b2 ^ right.b2) | (left.b3 ^ right.b3)) == 0;
        }
        public static bool operator !=(in Scalar4x64Alt left, in Scalar4x64Alt right) => !(left == right);
        public override int GetHashCode() => HashCode.Combine(b0, b1, b2, b3);
    }
}
