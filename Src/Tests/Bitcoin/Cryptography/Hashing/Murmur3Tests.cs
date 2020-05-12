// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Hashing
{
    public class Murmur3Tests
    {
        public static IEnumerable<object[]> GetHash32Cases()
        {
            // https://stackoverflow.com/a/31929528/10401748
            yield return new object[] { new byte[0], 0, 0 };
            yield return new object[] { new byte[0], 1, 0x514e28b7 };
            yield return new object[] { new byte[0], 0xffffffff, 0x81f16f39 };
            yield return new object[] { new byte[4] { 255, 255, 255, 255 }, 0, 0x76293b50 };
            yield return new object[] { new byte[4] { 0x21, 0x43, 0x65, 0x87 }, 0, 0xf55b516b };
            yield return new object[] { new byte[4] { 0x21, 0x43, 0x65, 0x87 }, 0x5082edee, 0x2362f9de };
            yield return new object[] { new byte[3] { 0x21, 0x43, 0x65 }, 0, 0x7e4a8634 };
            yield return new object[] { new byte[2] { 0x21, 0x43 }, 0, 0xa0f7b07a };
            yield return new object[] { new byte[1] { 0x21 }, 0, 0x72661cf4 };
            yield return new object[] { new byte[4], 0, 0x2362f9de };
            yield return new object[] { new byte[3], 0, 0x85f0b427 };
            yield return new object[] { new byte[2], 0, 0x30f4c306 };
            yield return new object[] { new byte[1], 0, 0x514e28b7 };
            yield return new object[] { Encoding.UTF8.GetBytes("aaaa"), 0x9747b28c, 0x5a97808a };
            yield return new object[] { Encoding.UTF8.GetBytes("aaa"), 0x9747b28c, 0x283e0130 };
            yield return new object[] { Encoding.UTF8.GetBytes("aa"), 0x9747b28c, 0x5d211726 };
            yield return new object[] { Encoding.UTF8.GetBytes("a"), 0x9747b28c, 0x7fa09ea6 };
            yield return new object[] { Encoding.UTF8.GetBytes("abcd"), 0x9747b28c, 0xf0478627 };
            yield return new object[] { Encoding.UTF8.GetBytes("abc"), 0x9747b28c, 0xc84a62dd };
            yield return new object[] { Encoding.UTF8.GetBytes("ab"), 0x9747b28c, 0x74875592 };
            yield return new object[] { Encoding.UTF8.GetBytes("Hello, world!"), 0x9747b28c, 0x24884cba };
            yield return new object[] { Encoding.UTF8.GetBytes(new string('a', 256)), 0x9747b28c, 0x37405bdc };
            yield return new object[] { Encoding.UTF8.GetBytes("abc"), 0, 0xb3dd93fa };
            yield return new object[]
            {
                Encoding.UTF8.GetBytes("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"), 0, 0xee925b90
            };
            yield return new object[]
            {
                Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog"), 0x9747b28c, 0x2fa826cd
            };

            // https://github.com/bitcoin/bitcoin/blob/376294cde6b1588cb17055d8fde567eaf5848c3c/src/test/hash_tests.cpp#L29-L43
            yield return new object[] { new byte[0], 0xfba4c795, 0x6a396f08U };
            yield return new object[] { new byte[1], 0, 0x514e28b7U };
            yield return new object[] { new byte[1], 0xfba4c795, 0xea3f0b17U };
            yield return new object[] { new byte[] { 255 }, 0, 0xfd6cf10dU };
            yield return new object[] { new byte[] { 0, 0x11 }, 0, 0x16c6b7abU };
            yield return new object[] { new byte[] { 0, 0x11, 0x22 }, 0, 0x8eb51c3dU };
            yield return new object[] { new byte[] { 0, 0x11, 0x22, 0x33 }, 0, 0xb4471bf8U };
            yield return new object[] { new byte[] { 0, 0x11, 0x22, 0x33, 0x44 }, 0, 0xe2301fa8U };
            yield return new object[] { new byte[] { 0, 0x11, 0x22, 0x33, 0x44, 0x55 }, 0, 0xfc2e4a15U };
            yield return new object[] { new byte[] { 0, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 }, 0, 0xb074502cU };
            yield return new object[] { new byte[] { 0, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 }, 0, 0x8034d2a0U };
            yield return new object[] { new byte[] { 0, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 }, 0, 0xb4698defU };
        }

        [Theory]
        [MemberData(nameof(GetHash32Cases))]
        public void ComputeHash32Test(byte[] data, uint seed, uint expected)
        {
            Murmur3 hasher = new Murmur3();
            uint actual = hasher.ComputeHash32(data, seed);
            Assert.Equal(expected, actual);
        }
    }
}
