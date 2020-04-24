// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Xunit;

namespace Tests.Bitcoin.ExtentionsAndHelpersTests
{
    public class SigHashTypeExtensionTests
    {
        [Theory]
        [InlineData(SigHashType.All, false)]
        [InlineData(SigHashType.None, false)]
        [InlineData(SigHashType.Single, false)]
        [InlineData(SigHashType.All | SigHashType.AnyoneCanPay, true)]
        [InlineData(SigHashType.None | SigHashType.AnyoneCanPay, true)]
        [InlineData(SigHashType.Single | SigHashType.AnyoneCanPay, true)]
        [InlineData((SigHashType)0b011_11111, false)]
        [InlineData((SigHashType)0b111_11111, true)]
        [InlineData((SigHashType)0b100_00000, true)]
        [InlineData((SigHashType)0b100_10000, true)]
        [InlineData((SigHashType)0b110_00000, true)]
        public void IsAnyoneCanPayTest(SigHashType sht, bool expected)
        {
            Assert.Equal(expected, sht.IsAnyoneCanPay());
        }

        [Theory]
        [InlineData(SigHashType.All, false)]
        [InlineData(SigHashType.None, true)]
        [InlineData(SigHashType.Single, false)]
        [InlineData(SigHashType.All | SigHashType.AnyoneCanPay, false)]
        [InlineData(SigHashType.None | SigHashType.AnyoneCanPay, true)]
        [InlineData(SigHashType.Single | SigHashType.AnyoneCanPay, false)]
        [InlineData((SigHashType)0b000_00110, false)]
        [InlineData((SigHashType)0b000_10010, false)]
        [InlineData((SigHashType)0b111_11110, false)]
        [InlineData((SigHashType)0b111_00000, false)]
        [InlineData((SigHashType)0b111_00010, true)]
        [InlineData((SigHashType)0b101_00010, true)]
        public void IsNoneTest(SigHashType sht, bool expected)
        {
            Assert.Equal(expected, sht.IsNone());
        }

        [Theory]
        [InlineData(SigHashType.All, false)]
        [InlineData(SigHashType.None, false)]
        [InlineData(SigHashType.Single, true)]
        [InlineData(SigHashType.All | SigHashType.AnyoneCanPay, false)]
        [InlineData(SigHashType.None | SigHashType.AnyoneCanPay, false)]
        [InlineData(SigHashType.Single | SigHashType.AnyoneCanPay, true)]
        [InlineData((SigHashType)0b000_00111, false)]
        [InlineData((SigHashType)0b000_10011, false)]
        [InlineData((SigHashType)0b111_11111, false)]
        [InlineData((SigHashType)0b111_00000, false)]
        [InlineData((SigHashType)0b111_00011, true)]
        [InlineData((SigHashType)0b101_00011, true)]
        public void IsSingleTest(SigHashType sht, bool expected)
        {
            Assert.Equal(expected, sht.IsSingle());
        }
    }
}
