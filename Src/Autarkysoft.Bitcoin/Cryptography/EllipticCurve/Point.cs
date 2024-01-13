// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// Elliptic curve point in Affine coordinates
    /// </summary>
    public readonly struct Point
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Point"/> using the given parameters.
        /// </summary>
        /// <param name="x26">x coordinate</param>
        /// <param name="y26">y coordinate</param>
        public Point(in UInt256_10x26 x26, in UInt256_10x26 y26) : this(x26, y26, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Point"/> using the given parameters.
        /// </summary>
        /// <param name="x26">x coordinate</param>
        /// <param name="y26">y coordinate</param>
        /// <param name="infinity">Is point at infinity</param>
        public Point(in UInt256_10x26 x26, in UInt256_10x26 y26, bool infinity)
        {
#if DEBUG
            x26.Verify();
            y26.Verify();
#endif
            x = x26;
            y = y26;
            isInfinity = infinity;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Point"/> using the given parameters.
        /// </summary>
        /// <param name="x0">x0</param>
        /// <param name="x1">x1</param>
        /// <param name="x2">x2</param>
        /// <param name="x3">x3</param>
        /// <param name="x4">x4</param>
        /// <param name="x5">x5</param>
        /// <param name="x6">x6</param>
        /// <param name="x7">x7</param>
        /// <param name="y0">y0</param>
        /// <param name="y1">y1</param>
        /// <param name="y2">y2</param>
        /// <param name="y3">y3</param>
        /// <param name="y4">y4</param>
        /// <param name="y5">y5</param>
        /// <param name="y6">y6</param>
        /// <param name="y7">y7</param>
        public Point(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6, uint x7,
                     uint y0, uint y1, uint y2, uint y3, uint y4, uint y5, uint y6, uint y7)
        {
            x = new UInt256_10x26(x0, x1, x2, x3, x4, x5, x6, x7);
            y = new UInt256_10x26(y0, y1, y2, y3, y4, y5, y6, y7);
            isInfinity = false;
        }


        /// <summary>
        /// Coordinates
        /// </summary>
        public readonly UInt256_10x26 x, y;
        /// <summary>
        /// True if this is the point at infinity; otherwise false.
        /// </summary>
        public readonly bool isInfinity;

#if DEBUG
        // Maximum allowed magnitudes for group element coordinates
        // SECP256K1_GE_{X/Y}_MAGNITUDE_MAX
        // Any changes to these values should be reflected in the same hard-coded values in tests
        private const int MaxXMagnitude = 4;
        private const int MaxYMagnitude = 3;

        /// <summary>
        /// Only works in DEBUG
        /// </summary>
        internal void Verify()
        {
            x.Verify();
            y.Verify();
            UInt256_10x26.VerifyMagnitude(x.magnitude, MaxXMagnitude);
            UInt256_10x26.VerifyMagnitude(y.magnitude, MaxYMagnitude);
        }
#endif

        private const uint CurveB = 7;
        /// <summary>
        /// First byte used in even 33-byte compressed public key encoding
        /// </summary>
        public const byte EvenByte = 2;
        /// <summary>
        /// First byte used in odd 33-byte compressed public key encoding
        /// </summary>
        public const byte OddByte = 3;
        /// <summary>
        /// First byte used in uncompressed 65-byte public key encoding
        /// </summary>
        public const byte UncompressedByte = 4;
        /// <summary>
        /// First byte used in even hybrid public key encoding
        /// </summary>
        public const byte EvenHybridByte = 6;
        /// <summary>
        /// First byte used in odd hybrid public key encoding
        /// </summary>
        public const byte OddHybridByte = 7;

        private static readonly Point _infinity = new Point(UInt256_10x26.Zero, UInt256_10x26.Zero, true);
        /// <summary>
        /// Point at infinity
        /// </summary>
        public static ref readonly Point Infinity => ref _infinity;

        private static readonly Point _g = new Point(
            0x16F81798U, 0x59F2815BU, 0x2DCE28D9U, 0x029BFCDBU, 0xCE870B07U, 0x55A06295U, 0xF9DCBBACU, 0x79BE667EU,
            0xFB10D4B8U, 0x9C47D08FU, 0xA6855419U, 0xFD17B448U, 0x0E1108A8U, 0x5DA4FBFCU, 0x26A3C465U, 0x483ADA77U);
        /// <summary>
        /// Secp256k1 curve generator point
        /// </summary>
        public static ref readonly Point G => ref _g;


        /// <summary>
        /// Calculates y from y^2 = x^3 + ax + b (mod p) by having x and whether y is odd or even.
        /// Return value indicates success.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="isOdd">Whether y is odd or even</param>
        /// <param name="y">Calculated y</param>
        /// <returns>True if y was found; otherwise false.</returns>
        public static bool TryFindY(in UInt256_10x26 x, bool isOdd, out UInt256_10x26 y)
        {
            // x^3 + b (mod p)
            UInt256_10x26 right = x.Multiply(x.Sqr()) + 7;
            if (!right.Sqrt(out y))
            {
                return false;
            }
            y = y.NormalizeVar();
            if (y.IsOdd != isOdd)
            {
                y = y.Negate(1);
            }
            return true;
        }

        /// <summary>
        /// Converts the given byte array to a <see cref="Point"/>. Return value indicates success.
        /// </summary>
        /// <param name="bytes">Byte sequence to use (must be 33 or 65 bytes and start with 2/3 or 4/6/7)</param>
        /// <param name="result">Resulting point (<see cref="Infinity"/> if fails)</param>
        /// <returns>True if the conversion is successful; otherwise false.</returns>
        public static bool TryRead(ReadOnlySpan<byte> bytes, out Point result)
        {
            if (bytes == null || bytes.Length < 33)
            {
                result = Infinity;
                return false;
            }

            byte b = bytes[0];
            if (bytes.Length == 33 && (b == EvenByte || b == OddByte))
            {
                UInt256_10x26 x = new UInt256_10x26(bytes.Slice(1, 32), out bool isValid);
                if (isValid && TryFindY(x, b == OddByte, out UInt256_10x26 y))
                {
                    result = new Point(x, y);
                    return true;
                }
            }
            else if (bytes.Length == 65 && (b == UncompressedByte || b == EvenHybridByte || b == OddHybridByte))
            {
                UInt256_10x26 x = new UInt256_10x26(bytes.Slice(1, 32), out bool isValidX);
                UInt256_10x26 y = new UInt256_10x26(bytes.Slice(33, 32), out bool isValidY);
                if (isValidX && isValidY)
                {
                    if ((b == EvenHybridByte && y.IsOdd) || (b == OddHybridByte && !y.IsOdd))
                    {
                        result = Infinity;
                        return false;
                    }

                    result = new Point(x, y, false);
                    return result.IsValidVar();
                }
            }

            result = Infinity;
            return false;
        }


        public static bool TryReadXOnly(ReadOnlySpan<byte> bytes, out Point result)
        {
            if (bytes == null || bytes.Length != 32)
            {
                result = Infinity;
                return false;
            }

            UInt256_10x26 x = new UInt256_10x26(bytes, out bool isValid);
            if (!isValid)
            {
                result = Infinity;
                return false;
            }
            if (!TryCreateVar(x, false, out result))
            {
                result = Infinity;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Creates a new instance of <see cref="Point"/> from the given x coordinate.
        /// Return value indicates success.
        /// </summary>
        /// <remarks>
        /// This method is not constant-time.
        /// </remarks>
        /// <param name="x">X coordinate</param>
        /// <param name="odd">Is y coordinate odd</param>
        /// <param name="result">Result</param>
        /// <returns>True if y was found; otherwise false.</returns>
        public static bool TryCreateVar(in UInt256_10x26 x, bool odd, out Point result)
        {
            // secp256k1_ge_set_xo_var
#if DEBUG
            x.Verify();
#endif
            // y^2 = x^3 + 7
            UInt256_10x26 right = (x.Sqr() * x) + CurveB;
            if (!right.Sqrt(out UInt256_10x26 y))
            {
                result = Infinity;
                return false;
            }

            UInt256_10x26 ry = y.NormalizeVar();
            if (ry.IsOdd != odd)
            {
                ry = ry.Negate(1);
            }

            result = new Point(x, ry, false);
#if DEBUG
            result.Verify();
#endif
            return true;
        }


        /// <summary>
        /// Bring a batch of inputs to the same global z "denominator", based on ratios between
        /// (omitted) z coordinates of adjacent elements.
        /// 
        /// Although the elements a[i] are _ge rather than _gej, they actually represent elements
        /// in Jacobian coordinates with their z coordinates omitted.
        /// 
        /// Using the notation z(b) to represent the omitted z coordinate of b, the array zr of
        /// z coordinate ratios must satisfy zr[i] == z(a[i]) / z(a[i - 1]) for 0 &#60; 'i' &#60; <paramref name="len"/>.
        /// The zr[0] value is unused.
        /// 
        /// This function adjusts the coordinates of 'a' in place so that for all 'i', z(a[i]) == z(a[len - 1]).
        /// In other words, the initial value of z(a[len - 1]) becomes the global z "denominator". Only the
        /// a[i].x and a[i].y coordinates are explicitly modified; the adjustment of the omitted z coordinate is
        /// implicit.
        /// The coordinates of the final element a[len - 1] are not changed.
        /// </summary>
        /// <param name="len"></param>
        /// <param name="a"></param>
        /// <param name="zr"></param>
        public static void SetGlobalZ(int len, Span<Point> a, ReadOnlySpan<UInt256_10x26> zr)
        {
            UInt256_10x26 zs;
#if DEBUG
            for (int i = 0; i < len; i++)
            {
                a[i].Verify();
                zr[i].Verify();
            }
#endif
            if (len > 0)
            {
                int i = len - 1;
                // Ensure all y values are in weak normal form for fast negation of points
                a[i] = new Point(a[i].x, a[i].y.NormalizeWeak(), a[i].isInfinity);
                zs = zr[i];

                // Work our way backwards, using the z-ratios to scale the x/y values.
                while (i > 0)
                {
                    if (i != len - 1)
                    {
                        zs *= zr[i];
                    }
                    i--;
                    a[i] = a[i].ToPointZInv(zs);
                }
            }
#if DEBUG
            for (int i = 0; i < len; i++)
            {
                a[i].Verify();
            }
#endif
        }


        /// <summary>
        /// Converts all <paramref name="r"/> to <see cref="Point"/>s converted from given <see cref="PointJacobian"/>
        /// array.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        public static void SetAllPointsToJacobianVar(Span<Point> r, ReadOnlySpan<PointJacobian> a)
        {
            Debug.Assert(r.Length <= a.Length);

            int i;
            int lastI = int.MaxValue;
#if DEBUG
            for (i = 0; i < r.Length; i++)
            {
                a[i].Verify();
            }
#endif
            for (i = 0; i < r.Length; i++)
            {
                if (a[i].isInfinity)
                {
                    r[i] = Infinity;
                }
                else
                {
                    // Use destination's x coordinates as scratch space
                    if (lastI == int.MaxValue)
                    {
                        r[i] = new Point(a[i].z, r[i].y, r[i].isInfinity);
                    }
                    else
                    {
                        UInt256_10x26 rx = r[lastI].x * a[i].z;
                        r[i] = new Point(rx, r[i].y, r[i].isInfinity);
                    }
                    lastI = i;
                }
            }
            if (lastI == int.MaxValue)
            {
                return;
            }
            UInt256_10x26 u = r[lastI].x.InverseVar();

            i = lastI;
            while (i > 0)
            {
                i--;
                if (!a[i].isInfinity)
                {
                    UInt256_10x26 rx = r[i].x * u;
                    r[lastI] = new Point(rx, r[lastI].y, r[lastI].isInfinity);
                    u *= a[lastI].z;
                    lastI = i;
                }
            }
            Debug.Assert(!a[lastI].isInfinity);
            r[lastI] = new Point(u, r[lastI].y, r[lastI].isInfinity);

            for (i = 0; i < r.Length; i++)
            {
                if (!a[i].isInfinity)
                {
                    r[i] = a[i].ToPointZInv(r[i].x);
                }
            }
#if DEBUG
            for (i = 0; i < r.Length; i++)
            {
                r[i].Verify();
            }
#endif
        }


        /// <summary>
        /// Return whether <paramref name="x"/> is a valid X coordinate on the curve.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool IsOnCurveVar(in UInt256_10x26 x)
        {
            UInt256_10x26 c = x.Sqr();
            c = c.Multiply(x);
            c = c.Add(CurveB);
            return c.IsSquareVar();
        }

        /// <summary>
        /// Returns whether fraction xn/xd is a valid X coordinate on the curve (xd != 0).
        /// </summary>
        /// <param name="xn"></param>
        /// <param name="xd">Must not be zero</param>
        /// <returns></returns>
        public static bool IsFracOnCurve(in UInt256_10x26 xn, in UInt256_10x26 xd)
        {
            // We want to determine whether (xn/xd) is on the curve.
            // (xn/xd)^3 + 7 is square <=> xd*xn^3 + 7*xd^4 is square (multiplying by xd^4, a square).
            Debug.Assert(!xd.IsZeroNormalizedVar());

            UInt256_10x26 r = xd * xn;      // r = xd*xn
            UInt256_10x26 t = xn.Sqr();     // t = xn^2
            r = r.Multiply(t);              // r = xd*xn^3
            t = xd.Sqr();                   // t = xd^2
            t = t.Sqr();                    // t = xd^4
            // TODO: pointless check since we don't have the EXHAUSTIVE_TEST_ORDER
            Debug.Assert(CurveB <= 31);
            t = t.Multiply(CurveB);         // t = 7*xd^4
            r = r.Add(t);                   // r = xd*xn^3 + 7*xd^4
            return r.IsSquareVar();
        }

        /// <summary>
        /// Returns if this instance is valid (on curve) and is not the point at infinity
        /// </summary>
        /// <remarks>
        /// This method is not constant-time
        /// </remarks>
        /// <returns></returns>
        public bool IsValidVar()
        {
#if DEBUG
            Verify();
#endif
            if (isInfinity)
            {
                return false;
            }

            // y^2 = x^3 + 7
            UInt256_10x26 left = y.Sqr();
            UInt256_10x26 right = (x.Sqr() * x) + CurveB;
            return left.Equals(right);
        }


        /// <summary>
        /// Returns inverse of this instance by mirroring it around X-axis: -P=(x,-y)
        /// </summary>
        /// <returns>-P</returns>
        public Point Negate()
        {
#if DEBUG
            Verify();
#endif
            UInt256_10x26 yNorm = y.NormalizeWeak();
            UInt256_10x26 yNeg = yNorm.Negate(1);
            Point result = new Point(x, yNeg, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }

        /// <summary>
        /// Return lambda times this instance, where lambda is chosen in a way such that this is very fast.
        /// </summary>
        /// <returns></returns>
        public Point MulLambda()
        {
#if DEBUG
            Verify();
#endif
            UInt256_10x26 rx = x.Multiply(UInt256_10x26.Beta);
            Point r = new Point(rx, y, isInfinity);
#if DEBUG
            r.Verify();
#endif
            return r;
        }


        /// <summary>
        /// Converts this instance to its byte array representation
        /// </summary>
        /// <param name="compressed"></param>
        /// <returns></returns>
        public Span<byte> ToByteArray(bool compressed)
        {
            UInt256_10x26 xNorm = x.NormalizeVar();
            UInt256_10x26 yNorm = y.NormalizeVar();

            if (compressed)
            {
                Span<byte> result = new byte[33];
                result[0] = yNorm.IsOdd ? OddByte : EvenByte;
                xNorm.WriteToSpan(result[1..]);
                return result;
            }
            else
            {
                Span<byte> result = new byte[65];
                result[0] = UncompressedByte;
                xNorm.WriteToSpan(result[1..]);
                yNorm.WriteToSpan(result[33..]);
                return result;
            }
        }


        internal Point ToPointZInv(in UInt256_10x26 zi)
        {
#if DEBUG
            Verify();
            zi.Verify();
            Debug.Assert(!isInfinity);
#endif
            UInt256_10x26 zi2 = zi.Sqr();
            UInt256_10x26 zi3 = zi2 * zi;
            UInt256_10x26 rx = x * zi2;
            UInt256_10x26 ry = y * zi3;
            Point result = new Point(rx, ry, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }


        /// <summary>
        /// Converts this instance in affine coordinates to point in jacobian coordinates
        /// </summary>
        public PointJacobian ToPointJacobian()
        {
            PointJacobian result = new PointJacobian(x, y, UInt256_10x26.One, isInfinity);
#if DEBUG
            result.Verify();
#endif
            return result;
        }

        /// <summary>
        /// Converts this instance to a <see cref="PointStorage"/>.
        /// It is assumed that this instance is not infinity.
        /// </summary>
        /// <returns>Result</returns>
        public PointStorage ToStorage()
        {
#if DEBUG
            Verify();
            Debug.Assert(!isInfinity);
#endif
            return new PointStorage(x, y);
        }


        /// <summary>
        /// Returns if the given group element (affine) is equal to this instance
        /// in variable time.
        /// </summary>
        /// <param name="other">Other point to use</param>
        /// <returns>True if the two points are equal; otherwise false.</returns>
        public bool EqualsVar(in Point other)
        {
#if DEBUG
            Verify();
            other.Verify();
#endif
            if (isInfinity != other.isInfinity)
            {
                return false;
            }
            if (isInfinity)
            {
                return true;
            }

            UInt256_10x26 tmp = x.NormalizeWeak();
            if (!tmp.Equals(other.x))
            {
                return false;
            }

            tmp = y.NormalizeWeak();
            if (!tmp.Equals(other.y))
            {
                return false;
            }

            return true;
        }
    }
}
