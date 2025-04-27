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

        [Theory]
        [InlineData(-1, "-1")]
        [InlineData(0, "0")]
        [InlineData(1, "1st")]
        [InlineData(2, "2nd")]
        [InlineData(3, "3rd")]
        [InlineData(4, "4th")]
        [InlineData(10, "10th")]
        [InlineData(11, "11th")]
        [InlineData(12, "12th")]
        [InlineData(13, "13th")]
        [InlineData(14, "14th")]
        [InlineData(20, "20th")]
        [InlineData(21, "21st")]
        [InlineData(22, "22nd")]
        [InlineData(23, "23rd")]
        [InlineData(24, "24th")]
        [InlineData(101, "101st")]
        public void ToOrdinalTest(int val, string expected)
        {
            string actual = val.ToOrdinal();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, -1)] // -1 is 0xffffffff
        [InlineData(0x12345678, 0x78563412)]
        public void SwapEndian_IntTest(int val, int expected)
        {
            Assert.Equal(expected, val.SwapEndian());
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0xffffffff, 0xffffffff)]
        [InlineData(0x12345678, 0x78563412)]
        public void SwapEndian_UIntTest(uint val, uint expected)
        {
            Assert.Equal(expected, val.SwapEndian());
        }

        [Theory]
        [InlineData(-1, "-1 B")]
        [InlineData(-1024, "-1.00 KB")]
        [InlineData(0, "0 B")]
        [InlineData(1, "1 B")]
        [InlineData(999, "999 B")]
        [InlineData(1000, "0.98 KB")] // 0.9765 rounded up with 2 decimal places
        [InlineData(1024, "1.00 KB")] // 1.0000 with 2 decimal places
        [InlineData(1030, "1.01 KB")] // 1.0058 rounded up
        [InlineData(10240, "10.0 KB")] // 1 decimal place
        [InlineData(102400, "100 KB")] // 0 decimal place
        [InlineData(153600, "150 KB")] // 0 decimal place
        [InlineData(1032192, "0.98 MB")] // 0.984375
        [InlineData(1024L * 1024, "1.00 MB")]
        [InlineData(1024L * 1024 * 1024, "1.00 GB")]
        [InlineData(1024L * 1024 * 1024 * 1024, "1.00 TB")]
        [InlineData(1024L * 1024 * 1024 * 1024 * 1024, "1.00 PB")]
        [InlineData(1024L * 1024 * 1024 * 1024 * 1024 * 1024, "1.00 EB")]
        public void ToDataSizeTest(long val, string expected)
        {
            Assert.Equal(expected, val.ToDataSize());
        }
    }
}
