// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Cryptography.EllipticCurve
{
    public class SchnorrSignatureTests
    {
        private static readonly Scalar8x32 _c1 = new(1, 2, 3, 4, 5, 6, 7, 8);
        private static readonly Scalar8x32 _c2 = new(11, 12, 13, 14, 15, 16, 17, 18);
        private static readonly Scalar8x32 _one = new(1, 0, 0, 0, 0, 0, 0, 0);
        public static ref readonly Scalar8x32 C1 => ref _c1;
        public static ref readonly Scalar8x32 C2 => ref _c2;
        public static ref readonly Scalar8x32 One => ref _one;


        [Fact]
        public void Constructor_SigHashTest()
        {
            SchnorrSignature sig = new(UInt256_10x26.One, C2, SigHashType.Single);
            Assert.Equal(UInt256_10x26.One, sig.R);
            Assert.Equal(C2, sig.S);
            Assert.Equal(SigHashType.Single, sig.SigHash);
        }


        public static IEnumerable<object[]> GetReadCases()
        {
            // Signature doesn't check r and s values
            yield return new object[] { new byte[64], UInt256_10x26.Zero, Scalar8x32.Zero, SigHashType.Default };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002"),
                UInt256_10x26.One, new Scalar8x32(2), SigHashType.Default
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "01"),
                UInt256_10x26.One, new Scalar8x32(2), SigHashType.All
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "02"),
                UInt256_10x26.One, new Scalar8x32(2), SigHashType.None
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "03"),
                UInt256_10x26.One, new Scalar8x32(2), SigHashType.Single
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "81"),
                UInt256_10x26.One, new Scalar8x32(2), SigHashType.All | SigHashType.AnyoneCanPay
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "82"),
                UInt256_10x26.One, new Scalar8x32(2), SigHashType.None | SigHashType.AnyoneCanPay
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "83"),
                UInt256_10x26.One, new Scalar8x32(2), SigHashType.Single | SigHashType.AnyoneCanPay
            };
            yield return new object[]
            {
                Helper.HexToBytes("ec96b19cbaa1be6f7fc252d4cbaafbd0408c9de580996411e57a0c8ea3fe8ede" +
                                  "6a7964164fdfdbf90e85efdb041340ef8bc692f57e89f5c282d3a6d8e112d5a4"),
                new UInt256_10x26(Helper.HexToBytes("ec96b19cbaa1be6f7fc252d4cbaafbd0408c9de580996411e57a0c8ea3fe8ede"), out _),
                new Scalar8x32(Helper.HexToBytes("6a7964164fdfdbf90e85efdb041340ef8bc692f57e89f5c282d3a6d8e112d5a4"), out _),
                SigHashType.Default
            };
        }
        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryReadTest(byte[] data, in UInt256_10x26 expR, in Scalar8x32 expS, SigHashType expSH)
        {
            bool b = SchnorrSignature.TryRead(data, out SchnorrSignature sig, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.True(expR.EqualsVar(sig.R));
            Assert.Equal(expS, sig.S);
            Assert.Equal(expSH, sig.SigHash);
        }

        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void ToByteArrayTest(byte[] expBytes, in UInt256_10x26 r, in Scalar8x32 s, SigHashType sh)
        {
            SchnorrSignature sig = new(r, s, sh);
            byte[] actualBytes = sig.ToByteArray();
            Assert.Equal(expBytes, actualBytes);
        }

        public static IEnumerable<object[]> GetReadFailCases()
        {
            yield return new object[] { null, Errors.NullOrEmptyBytes };
            yield return new object[] { Array.Empty<byte>(), Errors.NullOrEmptyBytes };
            yield return new object[] { new byte[1], Errors.InvalidSchnorrSigLength };
            yield return new object[] { new byte[63], Errors.InvalidSchnorrSigLength };
            yield return new object[] { new byte[66], Errors.InvalidSchnorrSigLength };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "00"),
                Errors.SigHashTypeZero
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "04"),
                Errors.InvalidSigHashType
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "79"),
                Errors.InvalidSigHashType
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "80"),
                Errors.InvalidSigHashType
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "84"),
                Errors.InvalidSigHashType
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "ff"),
                Errors.InvalidSigHashType
            };
            yield return new object[]
            {
                Helper.HexToBytes("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff" +
                                  "0000000000000000000000000000000000000000000000000000000000000002" + "01"),
                Errors.InvalidDerRFormat
            };
            yield return new object[]
            {
                Helper.HexToBytes("0000000000000000000000000000000000000000000000000000000000000001" +
                                  "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff" + "01"),
                Errors.InvalidDerSFormat
            };
        }
        [Theory]
        [MemberData(nameof(GetReadFailCases))]
        public void TryRead_FailTest(byte[] data, Errors expErr)
        {
            bool b = SchnorrSignature.TryRead(data, out SchnorrSignature sig, out Errors error);

            Assert.False(b, error.Convert());
            Assert.Null(sig);
            Assert.Equal(expErr, error);
        }
    }
}
