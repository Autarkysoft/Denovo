// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives;
using System.Collections.Generic;

namespace Tests.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    public class UInt256_4x64Tests
    {
        [Fact]
        public void EmptyCtorTest()
        {
            UInt256_4x64 value = new();
            Assert.Equal(0UL, value.b0);
            Assert.Equal(0UL, value.b1);
            Assert.Equal(0UL, value.b2);
            Assert.Equal(0UL, value.b3);
        }


        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0xd81a34264a2395c0U, 0x1c62b7426c221126U, 0xc16e1027c3612d6eU, 0x1cc853fc1220e352U)]
        [InlineData(ulong.MaxValue, ulong.MaxValue, ulong.MaxValue, ulong.MaxValue)]
        public void ConstructorTest(ulong u0, ulong u1, ulong u2, ulong u3)
        {
            UInt256_4x64 value = new(u0, u1, u2, u3);
            Assert.Equal(u0, value.b0);
            Assert.Equal(u1, value.b1);
            Assert.Equal(u2, value.b2);
            Assert.Equal(u3, value.b3);
        }


        public static IEnumerable<TheoryDataRow<ulong[], ulong[]>> GetCtorCases()
        {
            yield return new(new ulong[4], new ulong[5]);
            yield return new
            (
                new ulong[4]
                {
                    0xd67aa1d36a8dcf88, 0x174c67284d18dbe5, 0xbb9fa76bcb69f201, 0xf99b0c103c60a610
                },
                new ulong[5]
                {
                    0x000aa1d36a8dcf88, 0x000284d18dbe5d67, 0x000b69f201174c67, 0x000a610bb9fa76bc, 0x0000f99b0c103c60
                }
            );
        }
        [Theory]
        [MemberData(nameof(GetCtorCases))]
        public void ToUInt256_5x52Test(ulong[] arr, ulong[] exp)
        {
            UInt256_4x64 value = new(arr[0], arr[1], arr[2], arr[3]);
            UInt256_5x52 actual = value.ToUInt256_5x52();
            Assert.Equal(exp[0], actual.b0);
            Assert.Equal(exp[1], actual.b1);
            Assert.Equal(exp[2], actual.b2);
            Assert.Equal(exp[3], actual.b3);
            Assert.Equal(exp[4], actual.b4);
        }


        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 0, true)]
        [InlineData(0, 0, 0, 1, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 1, 0, 0, 0, 1, true)]
        [InlineData(0, 0, 1, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 1, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(1, 0, 0, 0, 0, 0, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 0, 1, false)]
        [InlineData(0, 0, 0, 0, 0, 0, 1, 0, false)]
        [InlineData(0, 0, 0, 0, 0, 1, 0, 0, false)]
        [InlineData(0, 0, 0, 0, 1, 0, 0, 0, false)]
        public void EqualsTest(ulong a0, ulong a1, ulong a2, ulong a3, ulong b0, ulong b1, ulong b2, ulong b3, bool expected)
        {
            UInt256_4x64 a = new(a0, a1, a2, a3);
            UInt256_4x64 b = new(b0, b1, b2, b3);
            Assert.Equal(expected, a.Equals(b));
            Assert.Equal(expected, b.Equals(a));
        }

        #region https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/tests.c#L7844-L7872

        // https://github.com/bitcoin-core/secp256k1/blob/0f6baf319fcae0d7f11a44fc9b4d4899b3f8082a/src/field_5x52.h#L49-L54
        internal static UInt256_4x64 SECP256K1_FE_STORAGE_CONST(uint d7, uint d6, uint d5, uint d4, uint d3, uint d2, uint d1, uint d0)
        {
            ulong b0 = d0 | (((ulong)d1) << 32);
            ulong b1 = d2 | (((ulong)d3) << 32);
            ulong b2 = d4 | (((ulong)d5) << 32);
            ulong b3 = d6 | (((ulong)d7) << 32);

            return new UInt256_4x64(b0, b1, b2, b3);
        }

        /// <summary>
        /// fe_storage_cmov_test
        /// </summary>
        [Fact]
        public void Libsecp256k1_CMovTest()
        {
            UInt256_4x64 zero = SECP256K1_FE_STORAGE_CONST(0, 0, 0, 0, 0, 0, 0, 0);
            UInt256_4x64 one = SECP256K1_FE_STORAGE_CONST(0, 0, 0, 0, 0, 0, 0, 1);
            UInt256_4x64 max = SECP256K1_FE_STORAGE_CONST(
                0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU,
                0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU, 0xFFFFFFFFU);
            UInt256_4x64 r = max;
            UInt256_4x64 a = zero;

            r = UInt256_4x64.CMov(r, a, 0);
            Assert.Equal(0UL, UInt256_5x52Tests.Libsecp256k1_CmpVar(r, max));

            r = zero; a = max;
            r = UInt256_4x64.CMov(r, a, 1);
            Assert.Equal(0UL, UInt256_5x52Tests.Libsecp256k1_CmpVar(r, max));

            a = zero;
            r = UInt256_4x64.CMov(r, a, 1);
            Assert.Equal(0UL, UInt256_5x52Tests.Libsecp256k1_CmpVar(r, zero));

            a = one;
            r = UInt256_4x64.CMov(r, a, 1);
            Assert.Equal(0UL, UInt256_5x52Tests.Libsecp256k1_CmpVar(r, one));

            r = one; a = zero;
            r = UInt256_4x64.CMov(r, a, 0);
            Assert.Equal(0UL, UInt256_5x52Tests.Libsecp256k1_CmpVar(r, one));
        }

        #endregion
    }
}
