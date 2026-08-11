// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using System;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class PointStorageTests
    {
        /// <summary>
        /// secp256k1_memcmp_var
        /// </summary>
        internal static ulong Libsecp256k1_CmpVar(in PointStorage a, in PointStorage b)
        {
            ReadOnlySpan<ulong> p1 = new ulong[8]
            {
                a.x.b0, a.x.b1, a.x.b2, a.x.b3,
                a.y.b0, a.y.b1, a.y.b2, a.y.b3
            };
            ReadOnlySpan<ulong> p2 = new ulong[8]
            {
                b.x.b0, b.x.b1, b.x.b2, b.x.b3,
                b.y.b0, b.y.b1, b.y.b2, b.y.b3
            };
            for (int i = 0; i < p1.Length; i++)
            {
                ulong diff = p1[i] - p2[i];
                if (diff != 0)
                {
                    return diff;
                }
            }
            return 0;
        }

        [Fact]
        public void CMovTest()
        {
            // ge_storage_cmov_test
            UInt256_4x64 zeroU32 = new(0, 0, 0, 0);
            UInt256_4x64 oneU32 = new(0, 0, 0, 1);
            UInt256_4x64 maxU32 = new(0xFFFFFFFFFFFFFFFFU, 0xFFFFFFFFFFFFFFFFU, 0xFFFFFFFFFFFFFFFFU, 0xFFFFFFFFFFFFFFFFU);

            PointStorage zero = new(zeroU32, zeroU32);
            PointStorage one = new(oneU32, oneU32);
            PointStorage max = new(maxU32, maxU32);

            PointStorage r = max;
            PointStorage a = zero;

            r = PointStorage.CMov(r, a, 0);
            Assert.Equal(0U, Libsecp256k1_CmpVar(r, max));

            r = zero; a = max;
            r = PointStorage.CMov(r, a, 1);
            Assert.Equal(0U, Libsecp256k1_CmpVar(r, max));

            a = zero;
            r = PointStorage.CMov(r, a, 1);
            Assert.Equal(0U, Libsecp256k1_CmpVar(r, zero));

            a = one;
            r = PointStorage.CMov(r, a, 1);
            Assert.Equal(0U, Libsecp256k1_CmpVar(r, one));

            r = one; a = zero;
            r = PointStorage.CMov(r, a, 0);
            Assert.Equal(0U, Libsecp256k1_CmpVar(r, one));
        }
    }
}
