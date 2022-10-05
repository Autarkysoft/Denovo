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


        /// <summary>
        /// Returns if this instance is valid (on curve) and is not the point at infinity
        /// </summary>
        /// <remarks>
        /// This method is not constant-time
        /// </remarks>
        /// <returns></returns>
        public bool IsValidVar()
        {
            if (isInfinity)
            {
                return false;
            }

            // y^2 = x^3 + 7
            UInt256_10x26 left = y.Sqr();
            // TODO: create a constant for curve.b=7
            UInt256_10x26 right = (x.Sqr() * x) + 7;
            right = right.NormalizeWeak();
            return right.EqualsVar(left);
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
        public static void SetGlobalZ(int len, Point[] a, UInt256_10x26[] zr)
        {
            int i = len - 1;
            UInt256_10x26 zs;

            if (len > 0) 
            {
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
                    PointJacobian tmpa = new PointJacobian(a[i].x, a[i].y, UInt256_10x26.Zero, false);
                    a[i] = tmpa.ToPointZInv(zs);
                }
            }
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
            // y^2 = x^3 + 7
            // TODO: create a constant for curve.b=7
            UInt256_10x26 right = (x.Sqr() * x) + 7;
            if (!right.Sqrt(out UInt256_10x26 y))
            {
                result = Infinity;
                return false;
            }

            UInt256_10x26 ry = y.NormalizeVar();
            if (ry.IsOdd != odd)
            {
                ry = ry.Negate(1).NormalizeVar();
            }

            result = new Point(x, ry, false);
            return true;
        }


        /// <summary>
        /// Returns inverse of this instance by mirroring it around X-axis: -P=(x,-y)
        /// </summary>
        /// <returns>-P</returns>
        public Point Negate()
        {
            UInt256_10x26 yNorm = y.NormalizeWeak();
            UInt256_10x26 yNeg = yNorm.Negate(1);
            return new Point(x, yNeg, isInfinity);
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


        /// <summary>
        /// Converts all <see cref="PointJacobian"/>s in <paramref name="a"/> to <see cref="Point"/>s and
        /// sets <paramref name="r"/> items.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="len"></param>
        public static void SetAllPointsToJacobianVar(Span<Point> r, ReadOnlySpan<PointJacobian> a, int len)
        {
            int i;
            int lastI = int.MaxValue;

            for (i = 0; i < len; i++)
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
            UInt256_10x26 u = r[lastI].x.InverseVariable_old();

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

            for (i = 0; i < len; i++)
            {
                if (!a[i].isInfinity)
                {
                    r[i] = a[i].ToPointZInv(r[i].x);
                }
            }
        }


        /// <summary>
        /// Converts this instance in affine coordinates to point in jacobian coordinates
        /// </summary>
        public PointJacobian ToPointJacobian() => new PointJacobian(x, y, UInt256_10x26.One, isInfinity);

        /// <summary>
        /// Converts this instance to a <see cref="PointStorage"/>.
        /// It is assumed that this instance is not infinity.
        /// </summary>
        /// <returns>Result</returns>
        public PointStorage ToStorage()
        {
            Debug.Assert(!isInfinity);
            return new PointStorage(x, y);
        }
    }
}
