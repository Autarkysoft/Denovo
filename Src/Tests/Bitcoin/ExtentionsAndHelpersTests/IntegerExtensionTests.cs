// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Xunit;

namespace Tests.Bitcoin.ExtentionsAndHelpersTests
{
    public class IntegerExtensionTests
    {
        [Theory]
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, false)]// Little endian
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, true)]// Big endian
        [InlineData(new byte[] { 0xff, 0x00, 0x00, 0x00 }, 255, false)]
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0xff }, 255, true)]
        [InlineData(new byte[] { 0xa9, 0x57, 0x31, 0x28 }, 674322345, false)]
        [InlineData(new byte[] { 0x28, 0x31, 0x57, 0xa9 }, 674322345, true)]
        [InlineData(new byte[] { 0xff, 0xff, 0xff, 0x7f }, int.MaxValue, false)]
        [InlineData(new byte[] { 0x7f, 0xff, 0xff, 0xff }, int.MaxValue, true)]
        [InlineData(new byte[] { 0xff, 0xff, 0xff, 0xff }, -1, false)]
        [InlineData(new byte[] { 0xff, 0xff, 0xff, 0xff }, -1, true)]
        [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x80 }, int.MinValue, false)]
        [InlineData(new byte[] { 0x80, 0x00, 0x00, 0x00 }, int.MinValue, true)]
        public void Int_ToByteArrayTest(byte[] expectedBytes, int val, bool bigEndian)
        {
            Assert.Equal(expectedBytes, val.ToByteArray(bigEndian));
        }
    }
}
