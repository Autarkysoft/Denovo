// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.ExtentionsAndHelpersTests
{
    public class ByteArrayComparerTests
    {
        public static IEnumerable<object[]> GetEqualsCases()
        {
            yield return new object[] { Array.Empty<byte>(), Array.Empty<byte>(), true };
            yield return new object[] { null, Array.Empty<byte>(), false };
            yield return new object[] { Array.Empty<byte>(), null, false };
            yield return new object[] { new byte[] { 1 }, null, false };
            yield return new object[] { null, new byte[] { 1 }, false };
            yield return new object[] { new byte[] { 1, 2 }, new byte[] { 1, 2 }, true };
            yield return new object[] { new byte[] { 1, 2 }, new byte[] { 1, 3 }, false };
        }
        [Theory]
        [MemberData(nameof(GetEqualsCases))]
        public void EqualsTest(byte[] a, byte[] b, bool expected)
        {
            ByteArrayComparer comp = new();
            bool actual = comp.Equals(a, b);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetHashCodeTest()
        {
            ByteArrayComparer comp = new();
            int h1 = comp.GetHashCode(new byte[] { 1, 2, 3 });
            int h2 = comp.GetHashCode(new byte[] { 1, 2, 4 });
            Assert.NotEqual(h1, h2);
        }
    }
}
