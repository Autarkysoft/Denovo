// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Diagnostics;
using System.Text;

namespace Autarkysoft.Bitcoin.Cryptography.EllipticCurve
{
    /// <summary>
    /// All implementations in this namespace are done with the help of https://github.com/bitcoin-core/secp256k1 
    /// </summary>
    public class Calc
    {
        public Calc()
        {
            // TODO: add more/different contexts here to be selected in ctor using an enum
            ECMultGenContext();
        }

        internal PointStorage[,] prec; /* prec[j][i] = 16^j * i * G + U_i */

        public void ECMultGenContext()
        {
            Span<Point> prec = stackalloc Point[1024];

            this.prec = new PointStorage[64, 16];
            PointJacobian gJ = Point.G.ToPointJacobian();
            /* Construct a group element with no known corresponding scalar (nothing up my sleeve). */
            byte[] ba = Encoding.UTF8.GetBytes("The scalar for this x is unknown");
            Debug.Assert(ba.Length == 32);
            UInt256_5x52 x = new UInt256_5x52(ba, out bool b);
            Debug.Assert(b);

            b = Point.TryCreateVar(x, false, out Point nums_ge);
            Debug.Assert(b);

            PointJacobian numsGJ = nums_ge.ToPointJacobian();
            /* Add G to make the bits in x uniformly distributed. */
            numsGJ = numsGJ.AddVar(Point.G, out _);


            /* compute prec. */
            Span<PointJacobian> preJ = stackalloc PointJacobian[1024]; /* Jacobian versions of prec. */
            PointJacobian gBase = gJ;
            PointJacobian numsBase = numsGJ;
            for (int j = 0; j < 64; j++)
            {
                /* Set precj[j*16 .. j*16+15] to (numsbase, numsbase + gbase, ..., numsbase + 15*gbase). */
                preJ[j * 16] = numsBase;
                for (int i = 1; i < 16; i++)
                {
                    preJ[j * 16 + i] = preJ[j * 16 + i - 1].AddVar(gBase, out _);
                }
                /* Multiply gbase by 16. */
                for (int i = 0; i < 4; i++)
                {
                    gBase = gBase.DoubleVar(out _);
                }
                /* Multiply numbase by 2. */
                numsBase = numsBase.DoubleVar(out _);
                if (j == 62)
                {
                    /* In the last iteration, numsbase is (1 - 2^j) * nums instead. */
                    numsBase = numsBase.Negate();
                    numsBase = numsBase.AddVar(numsGJ, out _);
                }
            }
            Point.SetAllPointsToJacobianVar(prec, preJ);

            for (int j = 0; j < 64; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    this.prec[j, i] = prec[j * 16 + i].ToStorage();
                }
            }
        }


        public unsafe PointJacobian MultiplyByG(in Scalar4x64 a)
        {
            PointStorage adds = default;
            PointJacobian result = PointJacobian.Infinity;

            ulong* pt = stackalloc ulong[4] { a.b0, a.b1, a.b2, a.b3 };
            for (int i = 0; i < 64; i++)
            {
                int n_i = (int)Scalar4x64.GetBitsLimb32(pt, i * 4, 4);
                for (int j = 0; j < 16; j++)
                {
                    adds = PointStorage.CMov(adds, prec[i, j], j == n_i ? 1U : 0);
                }
                Point add = adds.ToPoint();
                result = result.AddVar(add, out _);
            }

            //ulong[] temp = new ulong[] { a.b0, a.b1, a.b2, a.b3 };
            //for (int i = 0, k = 0; i < 64;)
            //{
            //    uint bit = temp[k] & 0x0000000f;
            //    for (uint j = 0; j < 16; j++)
            //    {
            //        adds = PointStorage.CMov(adds, prec[i, j], j == bit ? 1U : 0);
            //    }
            //    result = result.AddVar(adds.ToPoint(), out _);
            //    i++;

            //    bit = (temp[k] & 0x000000f0) >> 4;
            //    for (uint j = 0; j < 16; j++)
            //    {
            //        adds = PointStorage.CMov(adds, prec[i, j], j == bit ? 1U : 0);
            //    }
            //    result = result.AddVar(adds.ToPoint(), out _);
            //    i++;

            //    bit = (temp[k] & 0x00000f00) >> 8;
            //    for (uint j = 0; j < 16; j++)
            //    {
            //        adds = PointStorage.CMov(adds, prec[i, j], j == bit ? 1U : 0);
            //    }
            //    result = result.AddVar(adds.ToPoint(), out _);
            //    i++;

            //    bit = (temp[k] & 0x0000f000) >> 12;
            //    for (uint j = 0; j < 16; j++)
            //    {
            //        adds = PointStorage.CMov(adds, prec[i, j], j == bit ? 1U : 0);
            //    }
            //    result = result.AddVar(adds.ToPoint(), out _);
            //    i++;

            //    bit = (temp[k] & 0x000f0000) >> 16;
            //    for (uint j = 0; j < 16; j++)
            //    {
            //        adds = PointStorage.CMov(adds, prec[i, j], j == bit ? 1U : 0);
            //    }
            //    result = result.AddVar(adds.ToPoint(), out _);
            //    i++;

            //    bit = (temp[k] & 0x00f00000) >> 20;
            //    for (uint j = 0; j < 16; j++)
            //    {
            //        adds = PointStorage.CMov(adds, prec[i, j], j == bit ? 1U : 0);
            //    }
            //    result = result.AddVar(adds.ToPoint(), out _);
            //    i++;

            //    bit = (temp[k] & 0x0f000000) >> 24;
            //    for (uint j = 0; j < 16; j++)
            //    {
            //        adds = PointStorage.CMov(adds, prec[i, j], j == bit ? 1U : 0);
            //    }
            //    result = result.AddVar(adds.ToPoint(), out _);
            //    i++;

            //    bit = (temp[k] & 0xf0000000) >> 28;
            //    for (uint j = 0; j < 16; j++)
            //    {
            //        adds = PointStorage.CMov(adds, prec[i, j], j == bit ? 1U : 0);
            //    }
            //    result = result.AddVar(adds.ToPoint(), out _);
            //    i++;

            //    k++;
            //}

            return result;
        }

        public Span<byte> GetPubkey(in Scalar4x64 priv, bool compressed)
        {
            PointJacobian pubJ = MultiplyByG(priv);
            Point pub = pubJ.ToPoint();
            return pub.ToByteArray(compressed);
        }

        public void GetPubkey(in Scalar4x64 priv, out Span<byte> comp, out Span<byte> uncomp)
        {
            PointJacobian pubJ = MultiplyByG(priv);
            Point pub = pubJ.ToPoint();

            UInt256_5x52 xNorm = pub.x.NormalizeVar();
            UInt256_5x52 yNorm = pub.y.NormalizeVar();

            byte firstByte = yNorm.IsOdd ? (byte)3 : (byte)2;

            uncomp = new byte[65];
            uncomp[0] = 4;
            xNorm.WriteToSpan(uncomp[1..]);
            yNorm.WriteToSpan(uncomp[33..]);

            comp = new byte[33];
            comp[0] = firstByte;
            uncomp.Slice(1, 32).CopyTo(comp[1..]);
        }


        // This method is a simple way of checking and debugging the code not an actual test
        public void Test()
        {
            using SharpRandom rng = new SharpRandom();
            byte[] data = new byte[32];
            rng.GetBytes(data);

            //data = new Sha256().ComputeHash(Encoding.UTF8.GetBytes("foo"));

            Scalar4x64 sec = new Scalar4x64(data, out bool overflow);
            Debug.Assert(!overflow);
            PointJacobian pj = MultiplyByG(sec);
            Point p = pj.ToPoint();

            Span<byte> final = p.ToByteArray(false);

            string actual = final.ToArray().ToBase16();

            using Asymmetric.KeyPairs.PrivateKey key = new Asymmetric.KeyPairs.PrivateKey(data);
            string expected = key.ToPublicKey().ToByteArray(false).ToBase16();

            Debug.Assert(actual == expected);
        }
    }
}
