// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

#if !NET10_0
using System;
using System.Collections.Generic;

using LibUInt128 = Autarkysoft.Bitcoin.Cryptography.EllipticCurve.Primitives.UInt128;
using SysUInt128 = System.UInt128;

namespace Tests.Bitcoin.Cryptography.EllipticCurve.Primitives
{
    public class UInt128Tests
    {
        private const int FuzzMax = 100_000;

        private static ulong NextULong(Random rng)
        {
            byte[] ba = new byte[sizeof(ulong)];
            rng.NextBytes(ba);
            return BitConverter.ToUInt64(ba);
        }

        private static void AssertEqual(SysUInt128 expected, LibUInt128 actual)
        {
            Assert.Equal((ulong)expected, actual.Lower);
            Assert.Equal((ulong)(expected >> 64), actual.Upper);
        }



        [Fact]
        public void EmptyCtorTest()
        {
            LibUInt128 i = new();
            Assert.Equal(0UL, i.Lower);
            Assert.Equal(0UL, i.Upper);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 2)]
        [InlineData(0xdaa7e1aca5e9cfa5, 0xa75181e64103cf3c)]
        [InlineData(ulong.MaxValue, ulong.MaxValue)]
        public void CtorTest(ulong up, ulong low)
        {
            LibUInt128 i = new(up, low);
            Assert.Equal(up, i.Upper);
            Assert.Equal(low, i.Lower);
        }

        public static IEnumerable<TheoryDataRow<LibUInt128, LibUInt128, ulong, ulong>> AddCases()
        {
            LibUInt128 zero = new(0, 0);
            LibUInt128 one = new(0, 1);
            LibUInt128 max = new(ulong.MaxValue, ulong.MaxValue);

            yield return new(zero, zero, 0, 0);
            yield return new(zero, one, 0, 1);
            yield return new(one, zero, 0, 1);
            yield return new(zero, max, ulong.MaxValue, ulong.MaxValue);
            yield return new(max, zero, ulong.MaxValue, ulong.MaxValue);
            yield return new(max, one, 0, 0);
            yield return new(max, max, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFE);
        }

        [Theory]
        [MemberData(nameof(AddCases))]
        public void AddTest(LibUInt128 a, LibUInt128 b, ulong expUp, ulong expLow)
        {
            LibUInt128 actual = a + b;
            Assert.Equal(expUp, actual.Upper);
            Assert.Equal(expLow, actual.Lower);
        }

        [Fact]
        public void Add_FuzzTest()
        {
            Random rng = new(42);
            for (int i = 0; i < FuzzMax; i++)
            {
                ulong leftUp = NextULong(rng), leftLow = NextULong(rng);
                ulong rightUp = NextULong(rng), rightLow = NextULong(rng);

                LibUInt128 actual = new LibUInt128(leftUp, leftLow) + new LibUInt128(rightUp, rightLow);
                SysUInt128 expected = new SysUInt128(leftUp, leftLow) + new SysUInt128(rightUp, rightLow);

                AssertEqual(expected, actual);
            }
        }

        public static IEnumerable<TheoryDataRow<LibUInt128, LibUInt128, ulong, ulong>> MultiplyCases()
        {
            LibUInt128 zero = new(0, 0);
            LibUInt128 one = new(0, 1);
            LibUInt128 max = new(ulong.MaxValue, ulong.MaxValue);

            yield return new(zero, zero, 0, 0);
            yield return new(zero, one, 0, 0);
            yield return new(zero, max, 0, 0);
            yield return new(max, zero, 0, 0);
            yield return new(max, zero, 0, 0);
            yield return new(max, one, ulong.MaxValue, ulong.MaxValue);
            yield return new(max, new LibUInt128(0, 2), 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFE);
            yield return new(max, max, 0, 1);
            yield return new(new LibUInt128(0xE8FAF08929B46BB5, 0x26B442D59782BA17),
                             new LibUInt128(0x26B442D59782BA17, 0xE8FAF08929B46BB5),
                             0xea183ee8d7ccd26d, 0x1eb6255f4a612f43);
        }

        [Theory]
        [MemberData(nameof(MultiplyCases))]
        public void MultiplyTest(LibUInt128 a, LibUInt128 b, ulong expUp, ulong expLow)
        {
            LibUInt128 actual = a * b;
            Assert.Equal(expUp, actual.Upper);
            Assert.Equal(expLow, actual.Lower);
        }

        [Fact]
        public void Multiply_FuzzTest()
        {
            Random rng = new(42);
            for (int i = 0; i < 100_000; i++)
            {
                ulong leftUp = NextULong(rng), leftLow = NextULong(rng);
                ulong rightUp = NextULong(rng), rightLow = NextULong(rng);

                LibUInt128 actual = new LibUInt128(leftUp, leftLow) * new LibUInt128(rightUp, rightLow);
                SysUInt128 expected = new SysUInt128(leftUp, leftLow) * new SysUInt128(rightUp, rightLow);

                AssertEqual(expected, actual);
            }
        }


        [Fact]
        public void ShiftRightTest()
        {
            Random rng = new(42);

            ulong up = NextULong(rng);
            ulong low = NextULong(rng);

            LibUInt128 i1 = new(up, low);
            SysUInt128 i2 = new(up, low);

            for (int i = 0; i < 128; i++)
            {
                LibUInt128 actual = i1 >> i;
                SysUInt128 expected = i2 >> i;

                AssertEqual(expected, actual);
            }
        }

        [Fact]
        public void ShiftLeftTest()
        {
            Random rng = new(42);

            ulong up = NextULong(rng);
            ulong low = NextULong(rng);

            LibUInt128 i1 = new(up, low);
            SysUInt128 i2 = new(up, low);

            for (int i = 0; i < 128; i++)
            {
                LibUInt128 actual = i1 << i;
                SysUInt128 expected = i2 << i;

                AssertEqual(expected, actual);
            }
        }

        [Fact]
        public void BitwiseOpsTest()
        {
            Random rng = new(42);
            for (int i = 0; i < FuzzMax; i++)
            {
                ulong leftUp = NextULong(rng), leftLow = NextULong(rng);
                ulong rightUp = NextULong(rng), rightLow = NextULong(rng);

                LibUInt128 libLeft = new(leftUp, leftLow);
                LibUInt128 libRight = new(rightUp, rightLow);
                SysUInt128 sysLeft = new(leftUp, leftLow);
                SysUInt128 sysRight = new(rightUp, rightLow);

                AssertEqual(sysLeft & sysRight, libLeft & libRight);
                AssertEqual(sysLeft | sysRight, libLeft | libRight);
                AssertEqual(~sysLeft, ~libLeft);
                AssertEqual(~sysRight, ~libRight);
            }
        }
    }
}
#endif
