// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Arithmetic;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve
{
    /// <summary>
    /// Implementation of elliptic curve cryptography for secp256k1 curve. 
    /// From basic functions such as EC point multiplication to signing (ECDSA and ECSDSA).
    /// </summary>
    public class EllipticCurveCalculator
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EllipticCurveCalculator"/> using the default
        /// <see cref="SecP256k1"/> curve.
        /// </summary>
        public EllipticCurveCalculator() : this(new SecP256k1())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EllipticCurveCalculator"/> using the given <see cref="IECurveFp"/>.
        /// <para/> This constructor is only used for certain tests such as signature.
        /// </summary>
        /// <param name="curve">Curve to use</param>
        public EllipticCurveCalculator(IECurveFp curve)
        {
            if (curve is null)
                throw new ArgumentNullException(nameof(curve), "Curve can not be null.");

            this.curve = curve;
        }



        private readonly IECurveFp curve;



        /// <summary>
        /// Checks to see if the given point is on the used elliptic curve.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="point">Point to check</param>
        public void CheckOnCurve(EllipticCurvePoint point)
        {
            if (!curve.IsOnCurve(point))
                throw new ArgumentException("The given point is not on the given curve.", nameof(point));
        }


        /// <summary>
        /// Calculates x from y^2 = x^3 + ax + b (mod p) by having y. Return value indicates success.
        /// <para/> Only defined for <see cref="SecP256k1"/> where <see cref="IECurveFp.A"/> is zero.
        /// </summary>
        /// <param name="y">Y coordinate</param>
        /// <param name="x">Calculated x (0 if fails to calculate)</param>
        /// <returns>True if y was found, otherwise false.</returns>
        public bool TryFindX(BigInteger y, out BigInteger x)
        {
            // y^2 = x^3 + ax + b  --(a=0)-->  x^3 = y^2 - b
            BigInteger right = BigInteger.Pow(y, 2) - curve.B;
            x = BigInteger.ModPow(right, BigInteger.Divide(curve.P + 2, 9), curve.P);

            return curve.IsOnCurve(new EllipticCurvePoint(x, y));
        }


        /// <summary>
        /// Calculates y from y^2 = x^3 + ax + b (mod p) by having x and the first byte indicating whether y is odd or even.
        /// Return value indicates success.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="firstByte">The first byte indicating whether y is odd or even (must be 2 or 3)</param>
        /// <param name="y">Calculated y (0 if fails to calculate)</param>
        /// <returns>True if y was found, otherwise false.</returns>
        public bool TryFindY(BigInteger x, byte firstByte, out BigInteger y)
        {
            if (firstByte != 2 && firstByte != 3)
                return false;
            if (x.Sign < 1)
                return false;

            // Curve.A is zero
            BigInteger right = (BigInteger.Pow(x, 3) + curve.B) % curve.P;
            if (SquareRoot.TryFind(right, curve.P, out y))
            {
                if ((firstByte == 2 && !y.IsEven) || (firstByte == 3 && y.IsEven))
                {
                    y = curve.P - y;
                    Debug.Assert(y.Sign > 0);
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Calculates y from y^2 = x^3 + ax + b (mod p) by having x for Schnorr encoding.
        /// Return value indicates success.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Calculated y (0 if fails to calculate)</param>
        /// <returns>True if y was found, otherwise false.</returns>
        public bool TryFindYSchnorr(BigInteger x, out BigInteger y)
        {
            if (x.Sign < 1 || x > curve.P)
                return false;

            // Curve.A is zero
            BigInteger right = (BigInteger.Pow(x, 3) + curve.B) % curve.P;
            return SquareRoot.TryFind(right, curve.P, out y);
        }


        /// <summary>
        /// Converts the given byte array to an <see cref="EllipticCurvePoint"/>. Return value indicates success.
        /// </summary>
        /// <param name="bytes">Byte sequence to use (must be 33 or 65 bytes and start with 2/3 or 4)</param>
        /// <param name="result">Resulting point (<see cref="EllipticCurvePoint.InfinityPoint"/> if fails)</param>
        /// <returns>True if the conversion is successful, otherwise false.</returns>
        public bool TryGetPoint(byte[] bytes, out EllipticCurvePoint result)
        {
            if (bytes == null)
            {
                return false;
            }
            else if (bytes.Length == 33 && (bytes[0] == 2 || bytes[0] == 3))
            {
                byte[] xBa = bytes.SubArray(1, 32);
                BigInteger x = xBa.ToBigInt(true, true);
                if (!TryFindY(x, bytes[0], out BigInteger y))
                {
                    return false;
                }
                result = new EllipticCurvePoint(x, y);
                return curve.IsOnCurve(result);
            }
            else if (bytes.Length == 65 && (bytes[0] == 4 || bytes[0] == 6 || bytes[0] == 7))
            {
                byte[] xBa = bytes.SubArray(1, 32);
                byte[] yBa = bytes.SubArray(33, 32);

                BigInteger x = xBa.ToBigInt(true, true);
                BigInteger y = yBa.ToBigInt(true, true);

                // Hybrid form: http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.202.2977&rep=rep1&type=pdf
                if ((bytes[0] == 6 && !y.IsEven) || (bytes[0] == 7 && y.IsEven))
                {
                    return false;
                }

                result = new EllipticCurvePoint(x, y);
                return curve.IsOnCurve(result);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts the given byte array to an <see cref="EllipticCurvePoint"/> assuming it is the X-only public key
        /// used by Schnorr signatures. Return value indicates success.
        /// </summary>
        /// <param name="bytes">Byte sequence to use (must be exactly 32 bytes)</param>
        /// <param name="result">Resulting point (<see cref="EllipticCurvePoint.InfinityPoint"/> if fails)</param>
        /// <returns>True if the conversion is successful; otherwise false.</returns>
        public bool TryGetPointXOnly(ReadOnlySpan<byte> bytes, out EllipticCurvePoint result)
        {
            if (bytes == null || bytes.Length != 32)
            {
                return false;
            }
            else
            {
                BigInteger x = new BigInteger(bytes, true, true);
                if (!TryFindY(x, 2, out BigInteger y))
                {
                    return false;
                }
                result = new EllipticCurvePoint(x, y);
                return curve.IsOnCurve(result);
            }
        }


        /// <summary>
        /// Given a point (x,y) returns -point (x,-y) if the coin was on curve.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="point">Point to negate</param>
        /// <returns>The negative point</returns>
        public EllipticCurvePoint PointNeg(EllipticCurvePoint point)
        {
            CheckOnCurve(point);
            return PointNegChecked(point);
        }

        /// <summary>
        /// Given a point (x,y) returns -point (x,-y) without checking if the point is on curve.
        /// </summary>
        /// <param name="point">Point to negate</param>
        /// <returns>The negative point</returns>
        internal EllipticCurvePoint PointNegChecked(EllipticCurvePoint point)
        {
            return (point == EllipticCurvePoint.InfinityPoint) ? point : new EllipticCurvePoint(point.X, (-point.Y).Mod(curve.P));
        }

        /// <summary>
        /// Adds two <see cref="EllipticCurvePoint"/>s after verifying if they are on curve and returns the result.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="point1">First point to add</param>
        /// <param name="point2">Second point to add</param>
        /// <returns>Result of point addition</returns>
        public EllipticCurvePoint Add(EllipticCurvePoint point1, EllipticCurvePoint point2)
        {
            CheckOnCurve(point1);
            CheckOnCurve(point2);

            return AddChecked(point1, point2);
        }

        /// <summary>
        /// Adds two <see cref="EllipticCurvePoint"/>s without verifying if they are on curve and returns the result.
        /// </summary>
        /// <param name="point1">First point to add</param>
        /// <param name="point2">Second point to add</param>
        /// <returns>Result of point addition</returns>
        internal EllipticCurvePoint AddChecked(EllipticCurvePoint point1, EllipticCurvePoint point2)
        {
            if (point1 == EllipticCurvePoint.InfinityPoint)
                return point2;
            if (point2 == EllipticCurvePoint.InfinityPoint)
                return point1;

            BigInteger m;

            if (point1.X == point2.X)
            {
                if (point1.Y != point2.Y) // (x,y) + (x,−y) = O
                {
                    return EllipticCurvePoint.InfinityPoint;
                }

                // Point double or (x,y) + (x,y)
                m = ((3 * point1.X * point1.X) + curve.A) * (2 * point1.Y).ModInverse(curve.P);

                // Note that since points are on a group with a prime (mod p) all of them do have multiplicative inverses.
            }
            else // point1 != point2. (x1,y1) + (x2,y2)
            {
                m = (point1.Y - point2.Y) * (point1.X - point2.X).ModInverse(curve.P);
            }

            BigInteger x3 = ((m * m) - point1.X - point2.X).Mod(curve.P);
            BigInteger y3 = (m * (point1.X - x3) - point1.Y).Mod(curve.P);

            return new EllipticCurvePoint(x3, y3);
        }


        /// <summary>
        /// Returtns the result of multiplying the curve's generator with the given integer.
        /// Assumes point is on curve and k>0 and &#60;<see cref="IECurveFp.N"/>.
        /// </summary>
        /// <param name="k">The integer to multiply the point with</param>
        /// <returns>Result of multiplication</returns>
        public EllipticCurvePoint MultiplyByG(BigInteger k)
        {
            return MultiplyChecked(k, curve.G);
        }

        /// <summary>
        /// After checking if the point was on curve it returtns the result of multiplying the point with the given integer.
        /// Also the integer will be changed if it is bigger than <see cref="IECurveFp.N"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="k">The integer to multiply the point with</param>
        /// <param name="point">The <see cref="EllipticCurvePoint"/></param>
        /// <returns>Result of multiplication</returns>
        public EllipticCurvePoint Multiply(BigInteger k, EllipticCurvePoint point)
        {
            CheckOnCurve(point);

            if (k % curve.N == 0 || point == EllipticCurvePoint.InfinityPoint)
                return EllipticCurvePoint.InfinityPoint;

            k %= curve.N;
            return (k < 0) ? MultiplyChecked(-k, PointNegChecked(point)) : MultiplyChecked(k, point);
        }


        /// <summary>
        /// Returtns the result of multiplying the point with the given integer.
        /// Assumes point is on curve and k>0 and &#60;<see cref="IECurveFp.N"/>.
        /// </summary>
        /// <param name="k">The integer to multiply the point with</param>
        /// <param name="point">The <see cref="EllipticCurvePoint"/></param>
        /// <returns>Result of multiplication</returns>
        internal EllipticCurvePoint MultiplyChecked(BigInteger k, EllipticCurvePoint point)
        {
            EllipticCurvePoint result = EllipticCurvePoint.InfinityPoint;
            EllipticCurvePoint addend = point;

            while (k != 0)
            {
                if ((k & 1) == 1)
                {
                    result = AddChecked(result, addend);
                }

                addend = AddChecked(addend, addend);

                k >>= 1;
            }

            return result;
        }


        // Note: in all the following methods, we assume inputs are valid hence skip checkking. 
        // It is because all these methods are used internally by other classes that perform the checks.
        // for example Sign() is used by PrivateKey so the keyBytes and hash are both valid

        /// <summary>
        /// Creates a signature using ECDSA based on Standards for Efficient Cryptography (SEC 1: Elliptic Curve Cryptography)
        /// section 4.1.3 Signing Operation (page 44).
        /// Return value indicates success.
        /// </summary>
        /// <param name="hash">Hash(m) to use for signing</param>
        /// <param name="key">Private key bytes (must be padded to 32 bytes)</param>
        /// <param name="k">
        /// The ephemeral elliptic curve key used for signing
        /// (k should be smaller than <see cref="IECurveFp.N"/>, it is always smaller if <see cref="Rfc6979"/> is used).
        /// </param>
        /// <param name="lowS">If true s values bigger than <see cref="IECurveFp.N"/>/2 will be converted.</param>
        /// <param name="lowR">If true the process fails if R.X had its highest bit set</param>
        /// <param name="sig">Signature (null if process fails)</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool TrySign(byte[] hash, byte[] key, BigInteger k, bool lowS, bool lowR, out Signature sig)
        {
            // TODO: research what happens if e value is zero. does it reveal private key?
            // https://bitcointalk.org/index.php?topic=260595.msg4928224#msg4928224
            BigInteger e = hash.ToBigInt(true, true);

            EllipticCurvePoint rp = MultiplyChecked(k, curve.G);
            byte v = (byte)(((rp.X > curve.N) ? 2 : 0) | (rp.Y.IsEven ? 0 : 1));

            BigInteger r = rp.X % curve.N;
            // Note about r: 
            // R.X is at most 32 bytes, if the highest bit is set then in DER encoding 0 is appended to indicate
            // it is a positive integer. 
            // Here if low r is requested, we only check the length and reject cases where R.X is 32 bytes and needs the 
            // additional 0 and if R.X was smaller than 32 bytes it passes even if the higest bit was set.
            // In other words 0<31 bytes> or 0<30 byte>,... are accepted

            // TODO: when BigInteger is replaced by ModularUInt256 in the future, this should be replaced by a simple
            // test of the highest bit instead of ToByteArray and length check.
            if (r == 0 || (lowR && r.ToByteArray(isBigEndian: true).Length > 32))
            {
                sig = null;
                return false;
            }

            BigInteger s = k.ModInverse(curve.N) * (e + (r * key.ToBigInt(true, true))) % curve.N;
            if (s == 0)
            {
                sig = null;
                return false;
            }
            if (lowS && s > curve.N / 2)
            {
                v ^= 1;
                s = curve.N - s;
            }

            sig = new Signature(r, s, v);
            return true;
        }


        /// <summary>
        /// Creates a signature using ECDSA based on Standards for Efficient Cryptography (SEC 1: Elliptic Curve Cryptography)
        /// section 4.1.3 Signing Operation (page 44) with a low s (&#60;<see cref="IECurveFp.N"/>) and 
        /// low r (DER encoding of it will be &#60;= 32 bytes)
        /// </summary>
        /// <param name="hash">Hash(m) to use for signing</param>
        /// <param name="key">Private key bytes (must be padded to 32 bytes)</param>
        /// <returns>Signature</returns>
        public Signature Sign(byte[] hash, byte[] key)
        {
            using Rfc6979 kGen = new Rfc6979();
            if (TrySign(hash, key, kGen.GetK(hash, key, null), true, true, out Signature sig))
            {
                return sig;
            }
            else
            {
                uint count = 1;
                byte[] extraEntropy = new byte[32];
                do
                {
                    extraEntropy[0] = (byte)count;
                    extraEntropy[1] = (byte)(count >> 8);
                    extraEntropy[2] = (byte)(count >> 16);
                    extraEntropy[3] = (byte)(count >> 24);
                    count++;
                } while (!TrySign(hash, key, kGen.GetK(hash, key, extraEntropy), true, true, out sig));
                return sig;
            }
        }


        /// <summary>
        /// Verifies if the given signature is a valid ECDSA signature based on Standards for Efficient Cryptography 
        /// (SEC 1: Elliptic Curve Cryptography) 4.1.4 Verifying Operation (page 46).
        /// </summary>
        /// <param name="hash">Hash(m) used in signing</param>
        /// <param name="sig">Signature</param>
        /// <param name="pubPoint">The public key's <see cref="EllipticCurvePoint"/></param>
        /// <param name="lowS">If true s values bigger than <see cref="IECurveFp.N"/>/2 are rejected.</param>
        /// <returns>True if the signature was valid, otherwise false.</returns>
        public bool Verify(byte[] hash, Signature sig, EllipticCurvePoint pubPoint, bool lowS = true)
        {
            if (sig.R == 0 || sig.R > curve.N || sig.S == 0 || sig.S > curve.N || (lowS && sig.S > curve.N / 2))
            {
                return false;
            }

            BigInteger e = hash.ToBigInt(true, true);

            BigInteger invMod = sig.S.ModInverse(curve.N);
            BigInteger u1 = e * invMod % curve.N;
            BigInteger u2 = sig.R * invMod % curve.N;

            EllipticCurvePoint Rxy = AddChecked(MultiplyChecked(u1, curve.G), MultiplyChecked(u2, pubPoint));
            if (Rxy.X == 0 && Rxy.Y == 0)
            {
                return false;
            }

            BigInteger v = Rxy.X % curve.N;

            return v == sig.R;
        }

        /// <summary>
        /// Verifies if the given signature is a valid ECDSA signature based on Standards for Efficient Cryptography 
        /// (SEC 1: Elliptic Curve Cryptography) 4.1.4 Verifying Operation (page 46).
        /// </summary>
        /// <param name="hash">Hash(m) used in signing</param>
        /// <param name="sig">Signature</param>
        /// <param name="pubK">Public key</param>
        /// <param name="lowS">If true s values bigger than <see cref="IECurveFp.N"/>/2 are rejected.</param>
        /// <returns>True if the signature was valid, otherwise false.</returns>
        public bool Verify(byte[] hash, Signature sig, PublicKey pubK, bool lowS = true)
        {
            return Verify(hash, sig, pubK.ToPoint(), lowS);
        }


        /// <summary>
        /// Recovers all possible public keys (up to 4) from the given ECDSA signature based on Standards for Efficient Cryptography 
        /// (SEC 1: Elliptic Curve Cryptography) 4.1.6 Public Key Recovery Operation (page 47).
        /// Return value indicates success.
        /// </summary>
        /// <param name="hash">Hash(m) used in signing</param>
        /// <param name="sig">Signature</param>
        /// <param name="results">Recovered public keys (empty if no public key could be recovered)</param>
        /// <returns>True if any public key was found, otherwise false.</returns>
        public bool TryRecoverPublicKeys(byte[] hash, Signature sig, out EllipticCurvePoint[] results)
        {
            List<EllipticCurvePoint> temp = new List<EllipticCurvePoint>(curve.H * 4);

            for (int j = 0; j <= curve.H; j++)
            {
                BigInteger x = sig.R + (j * curve.N);
                if (!TryFindY(x, 2, out BigInteger y))
                {
                    continue;
                }
                EllipticCurvePoint R = new EllipticCurvePoint(x, y);
                if (!curve.IsOnCurve(R))
                {
                    continue;
                }

                BigInteger e = hash.ToBigInt(true, true);
                for (int k = 1; k <= 2; k++)
                {
                    // Q = r^−1(sR − eG).
                    EllipticCurvePoint Q =
                        MultiplyChecked(sig.R.ModInverse(curve.N),
                                        AddChecked(MultiplyChecked(sig.S, R),
                                                   PointNegChecked(MultiplyChecked(e, curve.G)))
                                       );

                    if (curve.IsOnCurve(Q))
                    {
                        if (!temp.Contains(Q))
                        {
                            // TODO: we are missing step 1.6.2 (verify if this pubkey + signature is valid)
                            temp.Add(Q);
                        }
                    }

                    R = PointNegChecked(R);
                }
            }

            results = temp.ToArray();
            return results.Length != 0;
        }

        private BigInteger ComputeSchnorrE(byte[] rba, EllipticCurvePoint P, byte[] hash)
        {
            // Compute tagged hash:
            // tagHash = Sha256(tagstring) 
            // msg = R.X | P.X | hash
            // return sha256(tagHash | tagHash | msg)
            using Sha256 sha = new Sha256();
            byte[] pba = P.X.ToByteArray(true, true);
            // TODO: change all these BigIntegers to ModUint256 type so that it doesn't need length checks,...!
            byte[] pubBa = new byte[32];
            Buffer.BlockCopy(pba, 0, pubBa, 32 - pba.Length, pba.Length);
            BigInteger e = sha.ComputeTaggedHash_BIP340_challenge(rba, pubBa, hash).ToBigInt(true, true) % curve.N;
            return e;
        }


        // TODO: change Sign*() methods accessibility to internal or add additinal checks (eg. input.legnth == 32)

        /// <summary>
        /// Creates a signature using ECSDSA based on BIP-340.
        /// </summary>
        /// <param name="hash">Hash(m) to use for signing</param>
        /// <param name="key">Private key bytes (must be padded to 32 bytes)</param>
        /// <param name="rng">A cryptographic secure random number generator to get an auxilary random used to derive k</param>
        /// <returns>Signature</returns>
        public Signature SignSchnorr(byte[] hash, byte[] key, IRandomNumberGenerator rng)
        {
            BigInteger d = key.ToBigInt(true, true);
            EllipticCurvePoint pubkPoint = MultiplyChecked(d, curve.G);
            byte[] xBytes = pubkPoint.X.ToByteArray(true, true);
            byte[] pubBa = new byte[32];
            Buffer.BlockCopy(xBytes, 0, pubBa, 32 - xBytes.Length, xBytes.Length);

            if (!pubkPoint.Y.IsEven)
            {
                d = curve.N - d;
                // The internal ComputeTaggedHash_*() methods assume inputs are 32 byte so it needs to be padded here
                // both hash and key are already 32 byte since the caller is PrivateKey.Sign() method
                byte[] temp = new byte[32];
                byte[] secBa = d.ToByteArray(true, true);
                Buffer.BlockCopy(secBa, 0, temp, 32 - secBa.Length, secBa.Length);
                key = temp;
            }

            using Sha256 sha = new Sha256();
            byte[] auxRand = new byte[32];
            BigInteger k = 0;
            do
            {
                rng.GetBytes(auxRand);
                auxRand = sha.ComputeTaggedHash_BIP340_aux(auxRand);
                for (int i = 0; i < auxRand.Length; i++)
                {
                    key[i] ^= auxRand[i];
                }

                byte[] kBa = sha.ComputeTaggedHash_BIP340_nonce(key, pubBa, hash);
                k = kBa.ToBigInt(true, true) % curve.N;
            } while (k == 0);

            EllipticCurvePoint R = MultiplyChecked(k, curve.G);

            if (!R.Y.IsEven)
            {
                k = curve.N - k;
            }

            byte[] rBa = new byte[32];
            byte[] tempR = R.X.ToByteArray(true, true);
            Buffer.BlockCopy(tempR, 0, rBa, 32 - tempR.Length, tempR.Length);

            BigInteger e = ComputeSchnorrE(rBa, pubkPoint, hash);
            BigInteger s = (k + (e * d)) % curve.N;

            return new Signature(R.X, s);
        }


        /// <summary>
        /// Verifies if the given signature is a valid ECSDSA signature based on BIP-340.
        /// </summary>
        /// <param name="hash">Hash(m) used in signing</param>
        /// <param name="sig">Signature</param>
        /// <param name="pubPoint">The public key's <see cref="EllipticCurvePoint"/></param>
        /// <returns>True if the signature was valid, otherwise false.</returns>
        public bool VerifySchnorr(byte[] hash, Signature sig, EllipticCurvePoint pubPoint)
        {
            // Validity of hash and sig and pubPoint must have been checked by the caller already
            if (sig.R >= curve.P || sig.S >= curve.N)
            {
                return false;
            }
            byte[] rBa = new byte[32];
            byte[] tempR = sig.R.ToByteArray(true, true);
            Buffer.BlockCopy(tempR, 0, rBa, 32 - tempR.Length, tempR.Length);

            BigInteger e = ComputeSchnorrE(rBa, pubPoint, hash);
            EllipticCurvePoint R = AddChecked(MultiplyChecked(sig.S, curve.G), MultiplyChecked(curve.N - e, pubPoint));

            return !R.IsInfinity && R.Y.IsEven && R.X == sig.R;
        }

        /// <summary>
        /// Verifies if the given signature is a valid ECSDSA signature based on BIP-340.
        /// </summary>
        /// <param name="hash">Hash(m) used in signing</param>
        /// <param name="sig">Signature</param>
        /// <param name="pubK">Public key</param>
        /// <returns>True if the signature was valid, otherwise false.</returns>
        public bool VerifySchnorr(byte[] hash, Signature sig, PublicKey pubK)
        {
            return VerifySchnorr(hash, sig, pubK.ToPoint());
        }
    }
}
