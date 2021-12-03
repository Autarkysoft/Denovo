// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Xunit;

namespace Tests.Bitcoin.ExtentionsAndHelpersTests
{
    // The following transaction was made with undefined (invalid) SigHashTypes (some of the test cases below)
    // signed and mined on RegTest using bitcoin core
    // 01000000067dc9385e655464c1e7b4347bb23837de89c774646c00a904175a411d350f7186000000006a47304402206c1660572b37628d7284a2993c115430e9a19a5f0841a3efe031b7d74e2a0bec02200ed2b4a0057b183b386591737754b403812f0905b1b6fae04219791750b1e57400210261a17a95ba773caba5d2f87a60afa657e3f97ac304e4fb67babc7b8d761d5f14ffffffff7dc9385e655464c1e7b4347bb23837de89c774646c00a904175a411d350f7186010000006a47304402205d78f502e36aa2a4861fd64e23ec48e7f9da18c5f0e1754052c2a5da95b5f0e202207257e4528ed25124219a4a2a07dc26915fc943f1cf6ae5c515ea33cdb800a12f62210261a17a95ba773caba5d2f87a60afa657e3f97ac304e4fb67babc7b8d761d5f14ffffffff7dc9385e655464c1e7b4347bb23837de89c774646c00a904175a411d350f7186020000006a473044022072b215078351c5b300b86cfd3ca085552826cf7ef3a2b70e5c2531d691abeaf50220634325e0526035d0647c8b69a4bd2b4db2d92656299dc639c04df3a7d683ad5d3f210261a17a95ba773caba5d2f87a60afa657e3f97ac304e4fb67babc7b8d761d5f14ffffffff7dc9385e655464c1e7b4347bb23837de89c774646c00a904175a411d350f7186030000006a473044022022cc33e454d9793d6a3f9657842839b4a8813b4e4025fa815f0abd15ce8de2c4022000adf106c98a65ad40f51ada179f856e2ee18bddbcfa55190034306db289e0c6a9210261a17a95ba773caba5d2f87a60afa657e3f97ac304e4fb67babc7b8d761d5f14ffffffff7dc9385e655464c1e7b4347bb23837de89c774646c00a904175a411d350f7186040000006a47304402205059af3feffbb19bc6c15e7eaaa3c33437bb6e34baf6accc602a6965ebd2ed130220696d6ae5bc5403b835d075c67caa8641bec54fcabcaf773238d671c45ce69900c2210261a17a95ba773caba5d2f87a60afa657e3f97ac304e4fb67babc7b8d761d5f14ffffffff7dc9385e655464c1e7b4347bb23837de89c774646c00a904175a411d350f7186050000006a47304402203434f02055906cfe07f9d90a834eee94925b52f1ae05eaae8c9a5eec76818d59022010292c0c1e5ca1165869f9ca9a40285261808b33dc6f05dc2a3f57404ce3688da3210261a17a95ba773caba5d2f87a60afa657e3f97ac304e4fb67babc7b8d761d5f14ffffffff01c0270900000000001976a9146aba5cd539dff84dd5ec27a0b5a2bde983651cfb88ac00000000
    public class SigHashTypeExtensionTests
    {
        [Theory]
        [InlineData(SigHashType.Default, false)]
        [InlineData(SigHashType.All, false)]
        [InlineData(SigHashType.None, false)]
        [InlineData(SigHashType.Single, false)]
        [InlineData(SigHashType.Default | SigHashType.AnyoneCanPay, true)]
        [InlineData(SigHashType.All | SigHashType.AnyoneCanPay, true)]
        [InlineData(SigHashType.None | SigHashType.AnyoneCanPay, true)]
        [InlineData(SigHashType.Single | SigHashType.AnyoneCanPay, true)]
        [InlineData((SigHashType)0b011_11111, false)]
        [InlineData((SigHashType)0b111_11111, true)]
        [InlineData((SigHashType)0b100_10000, true)]
        [InlineData((SigHashType)0b110_00000, true)]
        public void IsAnyoneCanPayTest(SigHashType sht, bool expected)
        {
            Assert.Equal(expected, sht.IsAnyoneCanPay());
        }

        [Theory]
        [InlineData(SigHashType.Default, false)]
        [InlineData(SigHashType.All, false)]
        [InlineData(SigHashType.None, true)]
        [InlineData(SigHashType.Single, false)]
        [InlineData(SigHashType.Default | SigHashType.AnyoneCanPay, false)]
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
        [InlineData(SigHashType.Default, false)]
        [InlineData(SigHashType.All, false)]
        [InlineData(SigHashType.None, false)]
        [InlineData(SigHashType.Single, true)]
        [InlineData(SigHashType.Default | SigHashType.AnyoneCanPay, false)]
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


        [Theory]
        [InlineData(SigHashType.Default, SigHashType.All)]
        [InlineData(SigHashType.All, SigHashType.All)]
        [InlineData(SigHashType.None, SigHashType.None)]
        [InlineData(SigHashType.Single, SigHashType.Single)]
        [InlineData(SigHashType.Default | SigHashType.AnyoneCanPay, SigHashType.Default)]
        [InlineData(SigHashType.All | SigHashType.AnyoneCanPay, SigHashType.All)]
        [InlineData(SigHashType.None | SigHashType.AnyoneCanPay, SigHashType.None)]
        [InlineData(SigHashType.Single | SigHashType.AnyoneCanPay, SigHashType.Single)]
        [InlineData((SigHashType)0b000_00100, SigHashType.Default)]
        [InlineData((SigHashType)0b010_00100, SigHashType.Default)]
        [InlineData((SigHashType)0b110_00100, SigHashType.Default)]
        [InlineData((SigHashType)0b000_00101, SigHashType.All)]
        [InlineData((SigHashType)0b010_00101, SigHashType.All)]
        [InlineData((SigHashType)0b110_00101, SigHashType.All)]
        [InlineData((SigHashType)0b000_00110, SigHashType.None)]
        [InlineData((SigHashType)0b010_00110, SigHashType.None)]
        [InlineData((SigHashType)0b110_00110, SigHashType.None)]
        [InlineData((SigHashType)0b000_00111, SigHashType.Single)]
        [InlineData((SigHashType)0b010_00111, SigHashType.Single)]
        [InlineData((SigHashType)0b110_00111, SigHashType.Single)]
        public void ToOutputTypeTest(SigHashType sht, SigHashType expected)
        {
            Assert.Equal(expected, sht.ToOutputType());
        }
    }
}
