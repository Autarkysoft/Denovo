// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.ImprovementProposals;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class BIP0137Tests
    {
        [Theory]
        [InlineData("", "80e795d4a4caadd7047af389d9f7f220562feb6196032e2131e10563352c4bcc")]
        [InlineData(" ", "a18ceda30a92e61ee1ae12b1c4815de1562fbc4892ddeff931e87b083ca570e4")]
        [InlineData("Foo", "209df902da05da746dfa5389a8e76cefd76ab48db4a8d1013497832a61a07910")]
        public void GetBytesToSignTest(string message, string hex)
        {
            BIP0137 bip = new BIP0137();
            byte[] actual = bip.GetBytesToSign(message);
            byte[] expected = Helper.HexToBytes(hex);

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetSignCases()
        {
            byte[] sigBa = Helper.HexToBytes(KeyHelper.Msg1Sig);
            byte[] compBa = sigBa.AppendToBeginning(32);

            yield return new object[]
            {
                BIP0137.AddressType.P2PKH_Uncompressed, sigBa.AppendToBeginning(28), sigBa.AppendToBeginning(28)
            };
            yield return new object[]
            {
                BIP0137.AddressType.P2PKH_Compressed, compBa, compBa
            };
            yield return new object[]
            {
                BIP0137.AddressType.P2SH_P2WPKH, sigBa.AppendToBeginning(36), compBa
            };
            yield return new object[]
            {
                BIP0137.AddressType.P2WPKH, sigBa.AppendToBeginning(40), compBa
            };
        }
        [Theory]
        [MemberData(nameof(GetSignCases))]
        public void SignTest(BIP0137.AddressType addrType, byte[] expected, byte[] expectedIgnore)
        {
            BIP0137 bip = new BIP0137();

            Signature sig1 = bip.Sign(KeyHelper.Prv1, KeyHelper.Msg1ToSign, addrType, false);
            byte[] actual1 = sig1.ToByteArrayWithRecId();

            Signature sig2 = bip.Sign(KeyHelper.Prv1, KeyHelper.Msg1ToSign, addrType, true);
            byte[] actual2 = sig2.ToByteArrayWithRecId();

            Assert.Equal(expected, actual1);
            Assert.Equal(expectedIgnore, actual2);
        }

        private string GetAddress(BIP0137.AddressType addrType)
        {
            return addrType switch
            {
                BIP0137.AddressType.P2PKH_Uncompressed => KeyHelper.Pub1UnCompAddr,
                BIP0137.AddressType.P2PKH_Compressed => KeyHelper.Pub1CompAddr,
                BIP0137.AddressType.P2SH_P2WPKH => KeyHelper.Pub1NestedSegwit,
                BIP0137.AddressType.P2WPKH => KeyHelper.Pub1BechAddr,
                _ => throw new ArgumentException("Address type is not defined."),
            };
        }
        [Theory]
        [MemberData(nameof(GetSignCases))]
        public void VerifyTest(BIP0137.AddressType addrType, byte[] expected, byte[] expectedIgnore)
        {
            BIP0137 bip = new BIP0137();

            bool b1 = bip.Verify(KeyHelper.Msg1ToSign, GetAddress(addrType), expected.ToBase64(), false);
            bool b2 = bip.Verify(KeyHelper.Msg1ToSign, GetAddress(addrType), expectedIgnore.ToBase64(), true);

            Assert.True(b1, "1");
            Assert.True(b2, "2");
        }
    }
}
