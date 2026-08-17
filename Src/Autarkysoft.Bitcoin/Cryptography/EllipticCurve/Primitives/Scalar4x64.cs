// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.ModInv;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    /// <summary>
    /// 256-bit scalar using 4 64-bit limbs using little-endian order
    /// </summary>
    public readonly struct Scalar4x64 : IEquatable<Scalar4x64>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Scalar4x64"/> using the given unsigned 32-bit integer.
        /// </summary>
        /// <param name="u">Value to use</param>
        public Scalar4x64(uint u)
        {
            b0 = u;
            b1 = 0; b2 = 0; b3 = 0;
#if DEBUG
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar4x64"/> using the given parameters.
        /// </summary>
        /// <remarks>
        /// Assumes caller handles overflow
        /// </remarks>
        /// <param name="u0">1st 32 bits</param>
        /// <param name="u1">2nd 32 bits</param>
        /// <param name="u2">3rd 32 bits</param>
        /// <param name="u3">4th 32 bits</param>
        public Scalar4x64(ulong u0, ulong u1, ulong u2, ulong u3)
        {
            b0 = u0; b1 = u1; b2 = u2; b3 = u3;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar4x64"/> using the given array.
        /// </summary>
        /// <remarks>
        /// Assumes caller handles overflow
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="array">Array of unsigned 64-bit integers</param>
        public Scalar4x64(ReadOnlySpan<ulong> array)
        {
            if (array.Length != 4)
                throw new ArgumentOutOfRangeException(nameof(array), "Array must contain 4 items.");

            b0 = array[0]; b1 = array[1]; b2 = array[2]; b3 = array[3];
#if DEBUG
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar4x64"/> using the given pointer
        /// to a <see cref="Hashing.Sha256.hashState"/>.
        /// </summary>
        /// <remarks>
        /// Useful for micro-optimization to skip allocating a byte array.
        /// </remarks>
        /// <param name="hPt"><see cref="Hashing.Sha256.hashState"/> pointer</param>
        /// <param name="overflow">Returns true if value is bigger than or equal to curve order; otherwise false</param>
        public unsafe Scalar4x64(uint* hPt, out bool overflow)
        {
            b3 = hPt[1] | (ulong)hPt[0] << 32;
            b2 = hPt[3] | (ulong)hPt[2] << 32;
            b1 = hPt[5] | (ulong)hPt[4] << 32;
            b0 = hPt[7] | (ulong)hPt[6] << 32;

            overflow = GetOverflow() != 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar4x64"/> using the given pointer
        /// to a SHA512 hash-state. Only the first 256 bits will be used.
        /// </summary>
        /// <remarks>
        /// Useful for micro-optimization to skip allocating a byte array.
        /// </remarks>
        /// <param name="hPt">SHA512 hashState pointer</param>
        /// <param name="overflow">Returns true if value is bigger than or equal to curve order; otherwise false</param>
        public unsafe Scalar4x64(ulong* hPt, out bool overflow)
        {
            b3 = hPt[0];
            b2 = hPt[1];
            b1 = hPt[2];
            b0 = hPt[3];

            overflow = GetOverflow() != 0;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar4x64"/> using the given pointer.
        /// </summary>
        /// <remarks>
        /// Assumes there is no overflow
        /// </remarks>
        /// <param name="pt">Pointer of the array containing 8 items (256 bits)</param>
        public unsafe Scalar4x64(ulong* pt)
        {
            b0 = pt[0]; b1 = pt[1]; b2 = pt[2]; b3 = pt[3];
#if DEBUG
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar4x64"/> using the given pointer to a big-endian array
        /// and reduces the result modulo curve order (n).
        /// </summary>
        /// <param name="pt">Pointer</param>
        /// <param name="overflow">Returns true if value was bigger than or equal to curve order; otherwise false</param>
        public unsafe Scalar4x64(byte* pt, out bool overflow)
        {
            ulong* r = stackalloc ulong[4];
            overflow = SetB32(pt, r);
            b0 = r[0]; b1 = r[1]; b2 = r[2]; b3 = r[3];
#if DEBUG
            Verify();
#endif
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Scalar4x64"/> using the given big-endian array
        /// and reduces the result modulo curve order (n).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="data">Array to use</param>
        /// <param name="overflow">Returns true if value was bigger than or equal to curve order; otherwise false</param>
        public unsafe Scalar4x64(ReadOnlySpan<byte> data, out bool overflow)
        {
            if (data.Length != 32)
                throw new ArgumentOutOfRangeException(nameof(data));

            ulong* r = stackalloc ulong[4];
            fixed (byte* pt = &data[0])
            {
                overflow = SetB32(pt, r);
                b0 = r[0]; b1 = r[1]; b2 = r[2]; b3 = r[3];
            }
#if DEBUG
            Verify();
#endif
        }

        private static unsafe bool SetB32(byte* pt, ulong* r)
        {
            r[0] = pt[31]
                | ((ulong)pt[30] << 8)
                | ((ulong)pt[29] << 16)
                | ((ulong)pt[28] << 24)
                | ((ulong)pt[27] << 32)
                | ((ulong)pt[26] << 40)
                | ((ulong)pt[25] << 48)
                | ((ulong)pt[24] << 56);
            r[1] = pt[23]
                | ((ulong)pt[22] << 8)
                | ((ulong)pt[21] << 16)
                | ((ulong)pt[20] << 24)
                | ((ulong)pt[19] << 32)
                | ((ulong)pt[18] << 40)
                | ((ulong)pt[17] << 48)
                | ((ulong)pt[16] << 56);
            r[2] = pt[15]
                | ((ulong)pt[14] << 8)
                | ((ulong)pt[13] << 16)
                | ((ulong)pt[12] << 24)
                | ((ulong)pt[11] << 32)
                | ((ulong)pt[10] << 40)
                | ((ulong)pt[9] << 48)
                | ((ulong)pt[8] << 56);
            r[3] = pt[7]
                | ((ulong)pt[6] << 8)
                | ((ulong)pt[5] << 16)
                | ((ulong)pt[4] << 24)
                | ((ulong)pt[3] << 32)
                | ((ulong)pt[2] << 40)
                | ((ulong)pt[1] << 48)
                | ((ulong)pt[0] << 56);

            uint of = GetOverflow(r);
            Debug.Assert(of == 0 || of == 1);
            Reduce(r, of);
            return of != 0;
        }



        /// <summary>
        /// Bit chunks
        /// </summary>
        public readonly ulong b0, b1, b2, b3;

        // Secp256k1 curve order (N)
        private const ulong N0 = 0xBFD25E8CD0364141UL;
        private const ulong N1 = 0xBAAEDCE6AF48A03BUL;
        private const ulong N2 = 0xFFFFFFFFFFFFFFFEUL;
        private const ulong N3 = 0xFFFFFFFFFFFFFFFFUL;

        // 2^256 - N
        // Since overflow will be less than 2N the result of X % N is X - N
        // X - N ≡ Z (mod N) => X + (2^256 - N) ≡ Z + 2^256 (mod N)
        // 250 ≡ 9 (mod 241) => 250 - 241 ≡ 250 + 256 - 241 ≡ 265 ≡ 265 - 256 ≡ 9 (mod 241)
        //                   => 265=0x0109 256=0x0100 => 265-256: get rid of highest bit => 0x0109≡0x09
        private const ulong NC0 = ~N0 + 1;
        private const ulong NC1 = ~N1;
        private const ulong NC2 = 1;

        // N/2
        private const ulong NH0 = 0xDFE92F46681B20A0UL;
        private const ulong NH1 = 0x5D576E7357A4501DUL;
        private const ulong NH2 = 0xFFFFFFFFFFFFFFFFUL;
        private const ulong NH3 = 0x7FFFFFFFFFFFFFFFUL;


        /// <summary>
        /// Byte size of <see cref="Scalar4x64"/>
        /// </summary>
        public const int ByteSize = 32;

        private static readonly Scalar4x64 _zero = new Scalar4x64(0);
        private static readonly Scalar4x64 _one = new Scalar4x64(1);
        private static readonly Scalar4x64 _lambda = new Scalar4x64(0xDF02967C1B23BD72U, 0x122E22EA20816678U,
                                                                    0xA5261C028812645AU, 0x5363AD4CC05C30E0U);
        private static readonly Scalar4x64 _mb1 = new Scalar4x64(0x6F547FA90ABFE4C3U, 0xE4437ED6010E8828U,
                                                                 0x0000000000000000U, 0x0000000000000000U);
        private static readonly Scalar4x64 _mb2 = new Scalar4x64(0xD765CDA83DB1562CU, 0x8A280AC50774346DU,
                                                                 0xFFFFFFFFFFFFFFFEU, 0xFFFFFFFFFFFFFFFFU);
        private static readonly Scalar4x64 _g1 = new Scalar4x64(0xE893209A45DBB031U, 0x3DAA8A1471E8CA7FU,
                                                                0xE86C90E49284EB15U, 0x3086D221A7D46BCDU);
        private static readonly Scalar4x64 _g2 = new Scalar4x64(0x1571B4AE8AC47F71U, 0x221208AC9DF506C6U,
                                                                0x6F547FA90ABFE4C4U, 0xE4437ED6010E8828U);
        /// <summary>
        /// Zero
        /// </summary>
        public static ref readonly Scalar4x64 Zero => ref _zero;
        /// <summary>
        /// One
        /// </summary>
        public static ref readonly Scalar4x64 One => ref _one;
        /// <summary>
        /// The Secp256k1 curve has an endomorphism, where lambda* (x, y) = (beta* x, y), where lambda is:
        /// </summary>
        public static ref readonly Scalar4x64 Lambda => ref _lambda;

        internal static ref readonly Scalar4x64 Minus_b1 => ref _mb1;
        internal static ref readonly Scalar4x64 Minus_b2 => ref _mb2;
        internal static ref readonly Scalar4x64 G1 => ref _g1;
        internal static ref readonly Scalar4x64 G2 => ref _g2;

        /// <summary>
        /// Returns if the value is equal to zero
        /// </summary>
        public bool IsZero
        {
            get
            {
#if DEBUG
                Verify();
#endif
                return (b0 | b1 | b2 | b3) == 0;
            }
        }

        /// <summary>
        /// Returns if the value is equal to one
        /// </summary>
        public bool IsOne
        {
            get
            {
#if DEBUG
                Verify();
#endif
                return ((b0 ^ 1) | b1 | b2 | b3) == 0;
            }
        }

        /// <summary>
        /// Returns if the value is even
        /// </summary>
        public bool IsEven
        {
            get
            {
#if DEBUG
                Verify();
#endif
                return (b0 & 1) == 0;
            }
        }


        /// <summary>
        /// Returns if this scalar is higher than the group order divided by 2.
        /// </summary>
        public bool IsHigh
        {
            get
            {
#if DEBUG
                Verify();
#endif
                int yes = 0;
                int no = 0;
                no |= (b3 < NH3 ? 1 : 0);
                yes |= (b3 > NH3 ? 1 : 0) & ~no;
                no |= (b2 < NH2 ? 1 : 0) & ~yes; // No need for a > check.
                no |= (b1 < NH1 ? 1 : 0) & ~yes;
                yes |= (b1 > NH1 ? 1 : 0) & ~no;
                yes |= (b0 > NH0 ? 1 : 0) & ~no;
                return yes != 0;
            }
        }

#if DEBUG
        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        internal void Verify()
        {
            Debug.Assert(GetOverflow() == 0);
        }
#endif

        public uint GetOverflow()
        {
            uint yes = 0U;
            uint no = 0U;
            no |= (b3 < N3 ? 1U : 0U); // No need for a > check.
            no |= (b2 < N2 ? 1U : 0U);
            yes |= (b2 > N2 ? 1U : 0U) & ~no;
            no |= (b1 < N1 ? 1U : 0U);
            yes |= (b1 > N1 ? 1U : 0U) & ~no;
            yes |= (b0 >= N0 ? 1U : 0U) & ~no;
            return yes;
        }

        private static unsafe uint GetOverflow(ulong* r)
        {
            uint yes = 0U;
            uint no = 0U;
            no |= (r[3] < N3 ? 1U : 0U); // No need for a > check.
            no |= (r[2] < N2 ? 1U : 0U);
            yes |= (r[2] > N2 ? 1U : 0U) & ~no;
            no |= (r[1] < N1 ? 1U : 0U);
            yes |= (r[1] > N1 ? 1U : 0U) & ~no;
            yes |= (r[0] >= N0 ? 1U : 0U) & ~no;
            return yes;
        }

        private static unsafe void Reduce(ulong* r, uint overflow)
        {
            // TODO: Should we rewrite this using ulongs+carry like what happens in UInt128 under the hood?
            //       We need to rewrite and benchmark. We can skip some oprations like in add (accum) lines, UInt128
            //       adds "left._upper + right._upper" which is pointless since both are zero. It just needs to hold carry (0 or 1).
            //       https://github.com/dotnet/dotnet/blob/6fd37e7c29da8f0afea252d7659fe19d6c9c10d4/src/runtime/src/libraries/System.Private.CoreLib/src/System/UInt128.cs#L744
            //       Same with right shifts.
            //       https://github.com/dotnet/dotnet/blob/6fd37e7c29da8f0afea252d7659fe19d6c9c10d4/src/runtime/src/libraries/System.Private.CoreLib/src/System/UInt128.cs#L2097
            //       dotnet's shifts are general purpose. Ours are shifting 64 bits so we basically swap _lower with _upper
            //       then set _upper to zero. UInt128 has branches and performs extra pointless steps and creates new UInt128 instance(s).

            Debug.Assert(overflow <= 1);

            UInt128 t = new UInt128(0, r[0]) + new UInt128(0, overflow * NC0);
            r[0] = (ulong)t; t >>= 64;
            t += new UInt128(0, r[1]) + new UInt128(0, overflow * NC1);
            r[1] = (ulong)t; t >>= 64;
            t += new UInt128(0, r[2]) + new UInt128(0, overflow * NC2);
            r[2] = (ulong)t; t >>= 64;
            t += new UInt128(0, r[3]);
            r[3] = (ulong)t;

            Debug.Assert(GetOverflow(r) == 0);
        }


        /// <summary>
        /// Creates a new instance of <see cref="Scalar4x64"/> using the given big-endian array
        /// and reduces the result modulo curve order (n). 
        /// Return value indicates validity of the result as a private key.
        /// </summary>
        /// <param name="data">Array to use</param>
        /// <param name="res">Scalar</param>
        /// <returns>True if value was non-zero and smaller than curve order; otherwise false.</returns>
        public static bool TrySetPrivateKey(ReadOnlySpan<byte> data, out Scalar4x64 res)
        {
            // secp256k1_scalar_set_b32_seckey
            res = new Scalar4x64(data, out bool overflow);
#if DEBUG
            res.Verify();
#endif
            return !overflow && !res.IsZero;
        }


        /// <summary>
        /// Adds the two scalars together modulo the group order.
        /// </summary>
        /// <param name="other">Other value</param>
        /// <param name="overflow">Returns whether it overflowed</param>
        /// <returns>Result</returns>
        public unsafe Scalar4x64 Add(in Scalar4x64 other, out bool overflow)
        {
#if DEBUG
            Verify();
            other.Verify();
#endif

            ulong* r = stackalloc ulong[4];

            UInt128 t = new UInt128(0, b0) + new UInt128(0, other.b0);
            r[0] = (ulong)t; t >>= 64;
            t += new UInt128(0, b1) + new UInt128(0, other.b1);
            r[1] = (ulong)t; t >>= 64;
            t += new UInt128(0, b2) + new UInt128(0, other.b2);
            r[2] = (ulong)t; t >>= 64;
            t += new UInt128(0, b3) + new UInt128(0, other.b3);
            r[3] = (ulong)t; t >>= 64;

            uint of = GetOverflow(r) + (uint)(ulong)t;
            overflow = of != 0;

            Debug.Assert(of == 0 || of == 1);
            Reduce(r, of);

            return new Scalar4x64(r);
        }


        /// <summary>
        /// Conditionally add a power of two to this scalar. The result is not allowed to overflow.
        /// </summary>
        /// <param name="bit"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public Scalar4x64 CAddBit(uint bit, uint flag)
        {
#if DEBUG
            Verify();
            Debug.Assert(bit < 256);
            Debug.Assert(flag == 0 || flag == 1);
#endif

            bit += (flag - 1) & 0x100;  // forcing (bit >> 6) > 3 makes this a noop

            UInt128 t = new UInt128(0, b0) + new UInt128(0, ((bit >> 6) == 0 ? 1UL : 0UL) << ((int)bit & 0x3F));
            ulong r0 = (ulong)t; t >>= 64;
            t += new UInt128(0, b1) + new UInt128(0, ((bit >> 6) == 1 ? 1UL : 0UL) << ((int)bit & 0x3F));
            ulong r1 = (ulong)t; t >>= 64;
            t += new UInt128(0, b2) + new UInt128(0, ((bit >> 6) == 2 ? 1UL : 0UL) << ((int)bit & 0x3F));
            ulong r2 = (ulong)t; t >>= 64;
            t += new UInt128(0, b3) + new UInt128(0, ((bit >> 6) == 3 ? 1UL : 0UL) << ((int)bit & 0x3F));
            ulong r3 = (ulong)t;

            Scalar4x64 result = new Scalar4x64(r0, r1, r2, r3);
#if DEBUG
            result.Verify();
            Debug.Assert((t >> 64) == 0);
#endif
            return result;
        }


        /// <summary>
        /// Access bits (1 &#60;= <paramref name="count"/> &#60;= 32) from a scalar.
        /// All requested bits must belong to the same 32-bit limb.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static unsafe uint GetBitsLimb32(ulong* pt, int offset, int count)
        {
            Debug.Assert(GetOverflow(pt) == 0);
            Debug.Assert(count > 0 && count <= 32);
            Debug.Assert(offset <= 256 - count);
            Debug.Assert((offset + count - 1) >> 5 == offset >> 5);

            return (uint)((pt[offset >> 6] >> (offset & 0x3F)) & (0xFFFFFFFF >> (32 - count)));
        }

        /// <summary>
        /// Access bits (1 &#60;= <paramref name="count"/> &#60;= 32) from a scalar.
        /// <paramref name="offset"/> + <paramref name="count"/> must be &#60;= 256.
        /// Not constant time in offset and count.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static unsafe uint GetBitsVar(ulong* pt, int offset, int count)
        {
            Debug.Assert(GetOverflow(pt) == 0);
            Debug.Assert(count > 0 && count <= 32);
            Debug.Assert(offset <= 256 - count);

            if ((offset + count - 1) >> 6 == offset >> 6)
            {
                return (uint)((pt[offset >> 6] >> (offset & 0x3F)) & (0xFFFFFFFF >> (32 - count)));
            }
            else
            {
                Debug.Assert((offset >> 6) + 1 < 4);
                Debug.Assert((offset & 0x3F) > 0);
                return (uint)(((pt[offset >> 6] >> (offset & 0x3F)) | (pt[(offset >> 6) + 1] << (64 - (offset & 0x3F)))) &
                       (0xFFFFFFFF >> (32 - count)));
            }
        }


        /// <summary>
        /// Returns the inverse of this scalar modulo the group order.
        /// </summary>
        /// <returns>Inverse</returns>
        public Scalar4x64 Inverse()
        {
#if DEBUG
            Verify();
            bool zero_in = IsZero;
#endif
            ModInv64Signed62 s = new ModInv64Signed62(this);
            ModInv64.Compute(ref s, ModInv64ModInfo.ScalarConstant);
            Scalar4x64 r = s.ToScalar4x64();
#if DEBUG
            r.Verify();
            Debug.Assert(r.IsZero == zero_in);
#endif
            return r;
        }


        /// <summary>
        /// Returns the inverse of this scalar modulo the group order, without constant-time guarantee.
        /// </summary>
        /// <returns>Inverse</returns>
        public Scalar4x64 InverseVar()
        {
#if DEBUG
            Verify();
            bool zero_in = IsZero;
#endif
            ModInv64Signed62 s = new ModInv64Signed62(this);
            ModInv64.ComputeVar(ref s, ModInv64ModInfo.ScalarConstant);
            Scalar4x64 r = s.ToScalar4x64();
#if DEBUG
            r.Verify();
            Debug.Assert(r.IsZero == zero_in);
#endif
            return r;
        }


        /// <summary>
        /// Multiply two scalars modulo the group order.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public unsafe Scalar4x64 Multiply(in Scalar4x64 b)
        {
#if DEBUG
            Verify();
            b.Verify();
#endif
            ulong* l = stackalloc ulong[8];
            Mult512(l, this, b);
            return Reduce512(l);
        }

        // Add a*b to the number defined by (c0,c1,c2). c2 must never overflow.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Muladd(ulong a, ulong b, ref ulong c0, ref ulong c1, ref ulong c2)
        {
            UInt128 t = (UInt128)a * b;
            ulong th = (ulong)(t >> 64);
            ulong tl = (ulong)t;

            c0 += tl;                    // overflow is handled on the next line
            th += (c0 < tl) ? 1U : 0U;   // at most 0xFFFFFFFFFFFFFFFF
            c1 += th;                    // overflow is handled on the next line
            c2 += (c1 < th) ? 1U : 0U;   // never overflows by contract (verified in the next line)

            Debug.Assert((c1 >= th) || (c2 != 0));
        }

        // Add a*b to the number defined by (c0,c1). c1 must never overflow.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MuladdFast(ulong a, ulong b, ref ulong c0, ref ulong c1)
        {
            UInt128 t = (UInt128)a * b;
            ulong th = (ulong)(t >> 64);   // at most 0xFFFFFFFFFFFFFFFE
            ulong tl = (ulong)t;

            c0 += tl;                    // overflow is handled on the next line
            th += (c0 < tl) ? 1U : 0U;   // at most 0xFFFFFFFFFFFFFFFF
            c1 += th;                    // never overflows by contract (verified in the next line)

            Debug.Assert(c1 >= th);
        }

        // Add a to the number defined by (c0,c1,c2). c2 must never overflow.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SumAdd(ulong a, ref ulong c0, ref ulong c1, ref ulong c2)
        {
            c0 += a;                        // overflow is handled on the next line
            uint over = (c0 < a) ? 1U : 0U;
            c1 += over;                     // overflow is handled on the next line
            c2 += (c1 < over) ? 1U : 0U;    // never overflows by contract
        }

        // Add a to the number defined by (c0,c1). c1 must never overflow, c2 must be zero.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SumaddFast(ulong a, ref ulong c0, ref ulong c1, ref ulong c2)
        {
            c0 += a;                        // overflow is handled on the next line
            c1 += (c0 < a) ? 1U : 0U;       // never overflows by contract (verified the next line)

            Debug.Assert((c1 != 0) | (c0 >= a));
            Debug.Assert(c2 == 0);
        }

        // Extract the lowest 64 bits of (c0,c1,c2) into n, and left shift the number 64 bits.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Extract(ref ulong n, ref ulong c0, ref ulong c1, ref ulong c2)
        {
            n = c0;
            c0 = c1;
            c1 = c2;
            c2 = 0;
        }

        // Extract the lowest 64 bits of (c0,c1,c2) into n, and left shift the number 64 bits. c2 is required to be zero.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExtractFast(ref ulong n, ref ulong c0, ref ulong c1, ref ulong c2)
        {
            n = c0;
            c0 = c1;
            c1 = 0;
            Debug.Assert(c2 == 0);
        }

        private static unsafe void Mult512(ulong* l8, in Scalar4x64 a, in Scalar4x64 b)
        {
            /* 160 bit accumulator. */
            ulong c0 = 0, c1 = 0;
            ulong c2 = 0;

            /* l8[0..7] = a[0..3] * b[0..3]. */
            MuladdFast(a.b0, b.b0, ref c0, ref c1);
            ExtractFast(ref l8[0], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l8[1], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l8[2], ref c0, ref c1, ref c2);
            Muladd(a.b0, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b1, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b0, ref c0, ref c1, ref c2);
            Extract(ref l8[3], ref c0, ref c1, ref c2);
            Muladd(a.b1, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b2, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b1, ref c0, ref c1, ref c2);
            Extract(ref l8[4], ref c0, ref c1, ref c2);
            Muladd(a.b2, b.b3, ref c0, ref c1, ref c2);
            Muladd(a.b3, b.b2, ref c0, ref c1, ref c2);
            Extract(ref l8[5], ref c0, ref c1, ref c2);
            MuladdFast(a.b3, b.b3, ref c0, ref c1);
            ExtractFast(ref l8[6], ref c0, ref c1, ref c2);
            Debug.Assert(c1 == 0);
            l8[7] = c0;
        }

        private static unsafe Scalar4x64 Reduce512(ulong* l)
        {
            UInt128 c128;
            ulong c, c0, c1, c2;
            ulong n0 = l[4], n1 = l[5], n2 = l[6], n3 = l[7];
            ulong m0 = 0, m1 = 0, m2 = 0, m3 = 0, m4 = 0, m5 = 0;
            ulong m6;
            ulong p0 = 0, p1 = 0, p2 = 0, p3 = 0;
            ulong p4;

            // Reduce 512 bits into 385.
            // m[0..6] = l[0..3] + n[0..3] * SECP256K1_N_C.
            c0 = l[0]; c1 = 0; c2 = 0;
            MuladdFast(n0, NC0, ref c0, ref c1);
            ExtractFast(ref m0, ref c0, ref c1, ref c2);
            SumaddFast(l[1], ref c0, ref c1, ref c2);
            Muladd(n1, NC0, ref c0, ref c1, ref c2);
            Muladd(n0, NC1, ref c0, ref c1, ref c2);
            Extract(ref m1, ref c0, ref c1, ref c2);
            SumAdd(l[2], ref c0, ref c1, ref c2);
            Muladd(n2, NC0, ref c0, ref c1, ref c2);
            Muladd(n1, NC1, ref c0, ref c1, ref c2);
            SumAdd(n0, ref c0, ref c1, ref c2);
            Extract(ref m2, ref c0, ref c1, ref c2);
            SumAdd(l[3], ref c0, ref c1, ref c2);
            Muladd(n3, NC0, ref c0, ref c1, ref c2);
            Muladd(n2, NC1, ref c0, ref c1, ref c2);
            SumAdd(n1, ref c0, ref c1, ref c2);
            Extract(ref m3, ref c0, ref c1, ref c2);
            Muladd(n3, NC1, ref c0, ref c1, ref c2);
            SumAdd(n2, ref c0, ref c1, ref c2);
            Extract(ref m4, ref c0, ref c1, ref c2);
            SumaddFast(n3, ref c0, ref c1, ref c2);
            ExtractFast(ref m5, ref c0, ref c1, ref c2);
            Debug.Assert(c0 <= 1);
            m6 = c0;

            /* Reduce 385 bits into 258. */
            /* p[0..4] = m[0..3] + m[4..6] * SECP256K1_N_C. */
            c0 = m0; c1 = 0; c2 = 0;
            MuladdFast(m4, NC0, ref c0, ref c1);
            ExtractFast(ref p0, ref c0, ref c1, ref c2);
            SumaddFast(m1, ref c0, ref c1, ref c2);
            Muladd(m5, NC0, ref c0, ref c1, ref c2);
            Muladd(m4, NC1, ref c0, ref c1, ref c2);
            Extract(ref p1, ref c0, ref c1, ref c2);
            SumAdd(m2, ref c0, ref c1, ref c2);
            Muladd(m6, NC0, ref c0, ref c1, ref c2);
            Muladd(m5, NC1, ref c0, ref c1, ref c2);
            SumAdd(m4, ref c0, ref c1, ref c2);
            Extract(ref p2, ref c0, ref c1, ref c2);
            SumaddFast(m3, ref c0, ref c1, ref c2);
            MuladdFast(m6, NC1, ref c0, ref c1);
            SumaddFast(m5, ref c0, ref c1, ref c2);
            ExtractFast(ref p3, ref c0, ref c1, ref c2);
            p4 = c0 + m6;
            Debug.Assert(p4 <= 2);

            /* Reduce 258 bits into 256. */
            /* r[0..3] = p[0..3] + p[4] * SECP256K1_N_C. */
            c128 = (UInt128)p0 + ((UInt128)NC0 * p4);
            p0 = (ulong)c128; c128 >>= 64;
            c128 += (UInt128)p1 + ((UInt128)NC1 * p4);
            p1 = (ulong)c128; c128 >>= 64;
            c128 += (UInt128)p2 + (UInt128)p4;
            p2 = (ulong)c128; c128 >>= 64;
            c128 += (UInt128)p3;
            p3 = (ulong)c128;
            c = (ulong)(c128 >> 64);

            Scalar4x64 r = new Scalar4x64(p0, p1, p2, p3);

            // Final reduction of r
            return Reduce(r, (uint)c + r.GetOverflow());
        }

        private static Scalar4x64 Reduce(in Scalar4x64 r, uint overflow)
        {
            Debug.Assert(overflow <= 1);

            UInt128 t = new UInt128(0, r.b0) + new UInt128(0, overflow * NC0);
            ulong r0 = (ulong)t; t >>= 64;
            t += new UInt128(0, r.b1) + new UInt128(0, overflow * NC1);
            ulong r1 = (ulong)t; t >>= 64;
            t += new UInt128(0, r.b2) + new UInt128(0, overflow * NC2);
            ulong r2 = (ulong)t; t >>= 64;
            t += new UInt128(0, r.b3);
            ulong r3 = (ulong)t;

            Scalar4x64 result = new Scalar4x64(r0, r1, r2, r3);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Multiply a and b (without taking the modulus!), divide by 2**shift, and round to the nearest integer.
        /// Shift must be at least 256
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <param name="shift">Shift must be at least 256</param>
        /// <returns>Result</returns>
        public static unsafe Scalar4x64 MulShiftVar(in Scalar4x64 a, in Scalar4x64 b, int shift)
        {
#if DEBUG
            a.Verify();
            b.Verify();
            Debug.Assert(shift >= 256);
#endif
            ulong* l = stackalloc ulong[8];
            Mult512(l, a, b);

            int shiftlimbs = shift >> 6;
            int shiftlow = shift & 0x3F;
            int shifthigh = 64 - shiftlow;

            bool sb = shiftlow != 0;

            ulong r0 = shift < 512 ? (l[0 + shiftlimbs] >> shiftlow | (shift < 448 && sb ? (l[1 + shiftlimbs] << shifthigh) : 0)) : 0;
            ulong r1 = shift < 448 ? (l[1 + shiftlimbs] >> shiftlow | (shift < 384 && sb ? (l[2 + shiftlimbs] << shifthigh) : 0)) : 0;
            ulong r2 = shift < 384 ? (l[2 + shiftlimbs] >> shiftlow | (shift < 320 && sb ? (l[3 + shiftlimbs] << shifthigh) : 0)) : 0;
            ulong r3 = shift < 320 ? (l[3 + shiftlimbs] >> shiftlow) : 0;


            Scalar4x64 r = new Scalar4x64(r0, r1, r2, r3);
#if DEBUG
            r.Verify();
#endif
            return r.CAddBit(0, (uint)((l[(shift - 1) >> 6] >> ((shift - 1) & 0x3f)) & 1));
        }


        /// <summary>
        /// Returns the complement of this scalar modulo the group order.
        /// </summary>
        /// <returns></returns>
        public Scalar4x64 Negate()
        {
#if DEBUG
            Verify();
#endif
            ulong nonzero = IsZero ? 0 : 0xFFFFFFFFFFFFFFFFUL;

            UInt128 t = new UInt128(0, ~b0) + new UInt128(0, N0 + 1);
            ulong r0 = (ulong)t & nonzero; t >>= 64;
            t += new UInt128(0, ~b1) + new UInt128(0, N1);
            ulong r1 = (ulong)t & nonzero; t >>= 64;
            t += new UInt128(0, ~b2) + new UInt128(0, N2);
            ulong r2 = (ulong)t & nonzero; t >>= 64;
            t += new UInt128(0, ~b3) + new UInt128(0, N3);
            ulong r3 = (ulong)t & nonzero;

            Scalar4x64 result = new Scalar4x64(r0, r1, r2, r3);
#if DEBUG
            result.Verify();
#endif
            return result;
        }

        /// <summary>
        /// Returns the conditional complement of this scalar modulo the group order.
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="result"></param>
        /// <returns>-1 if the number was negated; otherwise 1.</returns>
        public int NegateConditional(int flag, out Scalar4x64 result)
        {
#if DEBUG
            Verify();
            Debug.Assert(flag == 0 || flag == 1);
#endif
            // If flag = 0 then mask = 00...00 so this is a no-op
            // if flag = 1 then mask = 11...11 so this is identical Negate()
            ulong mask = (ulong)-flag;
            ulong nonzero = IsZero ? 0 : 0xFFFFFFFFFFFFFFFFUL;

            UInt128 t = new UInt128(0, b0 ^ mask) + new UInt128(0, (N0 + 1) & mask);
            ulong r0 = (ulong)t & nonzero; t >>= 64;
            t += new UInt128(0, b1 ^ mask) + new UInt128(0, N1 & mask);
            ulong r1 = (ulong)t & nonzero; t >>= 64;
            t += new UInt128(0, b2 ^ mask) + new UInt128(0, N2 & mask);
            ulong r2 = (ulong)t & nonzero; t >>= 64;
            t += new UInt128(0, b3 ^ mask) + new UInt128(0, N3 & mask);
            ulong r3 = (ulong)t & nonzero;

            result = new Scalar4x64(r0, r1, r2, r3);
#if DEBUG
            result.Verify();
#endif
            // return 2 * (mask == 0) - 1;
            return mask == 0 ? 1 : -1;
        }


        /// <summary>
        /// Conditional move. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is true (=1).
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="flag">Zero or one. Sets <paramref name="r"/> equal to <paramref name="a"/> if flag is one.</param>
        /// <returns></returns>
        public static Scalar4x64 CMov(in Scalar4x64 r, in Scalar4x64 a, uint flag)
        {
#if DEBUG
            r.Verify();
            a.Verify();
            Debug.Assert(flag == 0 || flag == 1);
#endif
            ulong mask0 = flag + ~0UL;
            ulong mask1 = ~mask0;
            ulong r0 = (r.b0 & mask0) | (a.b0 & mask1);
            ulong r1 = (r.b1 & mask0) | (a.b1 & mask1);
            ulong r2 = (r.b2 & mask0) | (a.b2 & mask1);
            ulong r3 = (r.b3 & mask0) | (a.b3 & mask1);

            Scalar4x64 result = new Scalar4x64(r0, r1, r2, r3);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Find r1 and r2 such that r1+r2*2^128 = k
        /// </summary>
        /// <param name="k"></param>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        public static void Split128(in Scalar4x64 k, out Scalar4x64 r1, out Scalar4x64 r2)
        {
#if DEBUG
            k.Verify();
#endif
            r1 = new Scalar4x64(k.b0, k.b1, 0, 0);
            r2 = new Scalar4x64(k.b2, k.b3, 0, 0);
#if DEBUG
            r1.Verify();
            r2.Verify();
#endif
        }


        /// <summary>
        /// Find r1 and r2 such that r1+r2*lambda = k, where r1 and r2 or their negations are
        /// maximum 128 bits long (see <see cref="Point.MulLambda"/>).
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="k"></param>
        internal static void SplitLambda(out Scalar4x64 r1, out Scalar4x64 r2, in Scalar4x64 k)
        {
#if DEBUG
            k.Verify();
#endif
            // these *Var calls are constant time since the shift amount is constant
            Scalar4x64 c1 = MulShiftVar(k, G1, 384);
            Scalar4x64 c2 = MulShiftVar(k, G2, 384);
            c1 = c1.Multiply(Minus_b1);
            c2 = c2.Multiply(Minus_b2);
            r2 = c1.Add(c2, out _);
            r1 = r2.Multiply(Lambda);
            r1 = r1.Negate();
            r1 = r1.Add(k, out _);

            Debug.Assert(r1.GetOverflow() == 0);
            Debug.Assert(r2.GetOverflow() == 0);
#if DEBUG
            SplitLambdaVerify(r1, r2, k);
#endif
        }

#if DEBUG
        private static void SplitLambdaVerify(in Scalar4x64 r1, in Scalar4x64 r2, in Scalar4x64 k)
        {
            // (a1 + a2 + 1)/2 is 0xa2a8918ca85bafe22016d0b917e4dd77
            Span<byte> k1_bound = new byte[32]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0xa2, 0xa8, 0x91, 0x8c, 0xa8, 0x5b, 0xaf, 0xe2, 0x20, 0x16, 0xd0, 0xb9, 0x17, 0xe4, 0xdd, 0x77
            };

            // (-b1 + b2)/2 + 1 is 0x8a65287bd47179fb2be08846cea267ed
            Span<byte> k2_bound = new byte[32]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x8a, 0x65, 0x28, 0x7b, 0xd4, 0x71, 0x79, 0xfb, 0x2b, 0xe0, 0x88, 0x46, 0xce, 0xa2, 0x67, 0xed
            };

            Scalar4x64 s = Lambda.Multiply(r2);
            s = s.Add(r1, out _);

            Debug.Assert(s.Equals(k));

            s = r1.Negate();
            Span<byte> buf1 = r1.ToByteArray();
            Span<byte> buf2 = s.ToByteArray();

            Debug.Assert(MemCmpVar(buf1, k1_bound, 32) < 0 || MemCmpVar(buf2, k1_bound, 32) < 0);

            s = r2.Negate();
            buf1 = r2.ToByteArray();
            buf2 = s.ToByteArray();

            Debug.Assert(MemCmpVar(buf1, k2_bound, 32) < 0 || MemCmpVar(buf2, k2_bound, 32) < 0);
        }

        // https://github.com/bitcoin-core/secp256k1/blob/b314cf28334a91db2fe144d04f86077e2bfd7a25/src/util.h#L212-L228
        private static int MemCmpVar(ReadOnlySpan<byte> s1, ReadOnlySpan<byte> s2, int n)
        {
            for (int i = 0; i < n; i++)
            {
                int diff = s1[i] - s2[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return 0;
        }
#endif // DEBUG


        /// <summary>
        /// Multiply a scalar with the multiplicative inverse of 2
        /// </summary>
        /// <returns></returns>
        public Scalar4x64 Half()
        {
#if DEBUG
            Verify();
#endif
            // Writing `/` for field division and `//` for integer division, we compute
            //
            //   a/2 = (a - (a&1))/2 + (a&1)/2
            //       = (a >> 1) + (a&1 ?    1/2 : 0)
            //       = (a >> 1) + (a&1 ? n//2+1 : 0),
            //
            // where n is the group order and in the last equality we have used 1/2 = n//2+1 (mod n).
            // For n//2, we have the constants SECP256K1_N_H_0, ...
            //
            // This sum does not overflow. The most extreme case is a = -2, the largest odd scalar.
            // Here:
            // - the left summand is:  a >> 1 = (a - a&1)/2 = (n-2-1)//2           = (n-3)//2
            // - the right summand is: a&1 ? n//2+1 : 0 = n//2+1 = (n-1)//2 + 2//2 = (n+1)//2
            // Together they sum to (n-3)//2 + (n+1)//2 = (2n-2)//2 = n - 1, which is less than n.

            ulong mask = (ulong)-(long)(b0 & 1U);
            UInt128 t = new UInt128(0, (b0 >> 1) | (b1 << 63)) + new UInt128(0, (NH0 + 1UL) & mask);
            ulong r0 = (ulong)t; t >>= 64;
            t += new UInt128(0, (b1 >> 1) | (b2 << 63)) + new UInt128(0, NH1 & mask);
            ulong r1 = (ulong)t; t >>= 64;
            t += new UInt128(0, (b2 >> 1) | (b3 << 63)) + new UInt128(0, NH2 & mask);
            ulong r2 = (ulong)t; t >>= 64;
            ulong r3 = (ulong)t + (b3 >> 1) + (NH3 & mask);

#if DEBUG
            // The line above only computed the bottom 64 bits of r->d[3]; redo the computation
            // in full 128 bits to make sure the top 64 bits are indeed zero.
            t += new UInt128(0, b3 >> 1) + new UInt128(0, NH3 & mask);
            t >>= 64;
            Debug.Assert((ulong)t == 0);
#endif

            Scalar4x64 result = new Scalar4x64(r0, r1, r2, r3);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Returns byte array representation of this instance.
        /// </summary>
        /// <returns>32 bytes</returns>
        public Span<byte> ToByteArray()
        {
#if DEBUG
            Verify();
#endif
            Span<byte> result = new byte[ByteSize];
            WriteToSpan(result);
            return result;
        }

        /// <summary>
        /// Writes this instance to the given span as a byte array.
        /// </summary>
        /// <param name="stream">Span to write to (must be at least 32 bytes)</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void WriteToSpan(Span<byte> stream)
        {
            if (stream.Length < 32)
                throw new ArgumentOutOfRangeException(nameof(stream), "Stream must be at least 32 bytes.");
#if DEBUG
            Verify();
#endif

            stream[0] = (byte)(b3 >> 56); stream[1] = (byte)(b3 >> 48); stream[2] = (byte)(b3 >> 40); stream[3] = (byte)(b3 >> 32);
            stream[4] = (byte)(b3 >> 24); stream[5] = (byte)(b3 >> 16); stream[6] = (byte)(b3 >> 8); stream[7] = (byte)b3;
            stream[8] = (byte)(b2 >> 56); stream[9] = (byte)(b2 >> 48); stream[10] = (byte)(b2 >> 40); stream[11] = (byte)(b2 >> 32);
            stream[12] = (byte)(b2 >> 24); stream[13] = (byte)(b2 >> 16); stream[14] = (byte)(b2 >> 8); stream[15] = (byte)b2;
            stream[16] = (byte)(b1 >> 56); stream[17] = (byte)(b1 >> 48); stream[18] = (byte)(b1 >> 40); stream[19] = (byte)(b1 >> 32);
            stream[20] = (byte)(b1 >> 24); stream[21] = (byte)(b1 >> 16); stream[22] = (byte)(b1 >> 8); stream[23] = (byte)b1;
            stream[24] = (byte)(b0 >> 56); stream[25] = (byte)(b0 >> 48); stream[26] = (byte)(b0 >> 40); stream[27] = (byte)(b0 >> 32);
            stream[28] = (byte)(b0 >> 24); stream[29] = (byte)(b0 >> 16); stream[30] = (byte)(b0 >> 8); stream[31] = (byte)b0;
        }


        /// <summary>
        /// Returns if the given scalar is equal to this instance
        /// </summary>
        /// <param name="other">Scalar to compare to</param>
        /// <returns>True if the two scalars are equal; otherwise false.</returns>
        public bool Equals(Scalar4x64 other) => this == other;

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Scalar4x64 other && this == other;

        /// <summary>
        /// Returns if the two scalars are equal to each other
        /// </summary>
        /// <param name="left">First scalar</param>
        /// <param name="right">Second scalar</param>
        /// <returns>True if the two scalars are equal; otherwise false.</returns>
        public static bool operator ==(in Scalar4x64 left, in Scalar4x64 right)
        {
#if DEBUG
            left.Verify();
            right.Verify();
#endif

            return ((left.b0 ^ right.b0) | (left.b1 ^ right.b1) | (left.b2 ^ right.b2) | (left.b3 ^ right.b3)) == 0;
        }

        /// <summary>
        /// Returns if the two scalars are not equal to each other
        /// </summary>
        /// <param name="left">First scalar</param>
        /// <param name="right">Second scalar</param>
        /// <returns>True if the two scalars are not equal; otherwise false.</returns>
        public static bool operator !=(in Scalar4x64 left, in Scalar4x64 right) => !(left == right);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(b0, b1, b2, b3);

        /// <inheritdoc/>
        public override string ToString() => $"0x{Base16.Encode(ToByteArray())}";
    }
}
