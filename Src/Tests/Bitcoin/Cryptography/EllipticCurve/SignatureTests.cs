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
    public class SignatureTests
    {
        private static readonly Scalar8x32 _c1 = new(1, 2, 3, 4, 5, 6, 7, 8);
        private static readonly Scalar8x32 _c2 = new(11, 12, 13, 14, 15, 16, 17, 18);
        private static readonly Scalar8x32 _one = new(1, 0, 0, 0, 0, 0, 0, 0);
        public static ref readonly Scalar8x32 C1 => ref _c1;
        public static ref readonly Scalar8x32 C2 => ref _c2;
        public static ref readonly Scalar8x32 One => ref _one;


        [Fact]
        public void Constructor_RecIdTest()
        {
            Signature sig = new(C1, C2, 3);
            Assert.Equal(C1, sig.R);
            Assert.Equal(C2, sig.S);
            Assert.Equal(3, sig.RecoveryId);
        }

        [Fact]
        public void Constructor_SigHashTest()
        {
            Signature sig = new(C1, C2, SigHashType.Single);
            Assert.Equal(C1, sig.R);
            Assert.Equal(C2, sig.S);
            Assert.Equal(SigHashType.Single, sig.SigHash);
        }


        public static IEnumerable<object[]> GetReadLooseCases()
        {
            yield return new object[]
            {
                // R has starting 0x00 but it shouldn't
                Helper.HexToBytes("3007"+"02020001"+"020101"+"01"),
                One,
                One,
                SigHashType.All
            };
            yield return new object[]
            {
                // S has starting 0x00 but it shouldn't
                Helper.HexToBytes("3007"+"020101"+"02020001"+"01"),
                One,
                One,
                SigHashType.All
            };
            yield return new object[]
            {
                // R should have starting 0x00 but it doesn't
                Helper.HexToBytes("3006"+"020180"+"020101"+"01"),
                new Scalar8x32(128),
                One,
                SigHashType.All
            };
            yield return new object[]
            {
                // S should have starting 0x00 but it doesn't
                Helper.HexToBytes("3006"+"020101"+"020180"+"81"),
                One,
                new Scalar8x32(128),
                SigHashType.All | SigHashType.AnyoneCanPay
            };
            yield return new object[]
            {
                // Correct DER encoded signature but SigHash has extra bytes
                Helper.HexToBytes("3006"+"020101"+"020101"+"ff02"),
                One,
                One,
                SigHashType.None
            };
        }
        [Theory]
        [MemberData(nameof(GetReadLooseCases))]
        public void TryReadLooseTest(byte[] data, in Scalar8x32 expR, in Scalar8x32 expS, SigHashType expSH)
        {
            bool b = Signature.TryReadLoose(data, out Signature sig, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expR, sig.R);
            Assert.Equal(expS, sig.S);
            Assert.Equal(expSH, sig.SigHash);
        }
        [Theory]
        [MemberData(nameof(GetReadStrictCases))]
        public void TryReadLoose_FromStrictCases_Test(byte[] data, in Scalar8x32 expR, in Scalar8x32 expS, SigHashType expSH)
        {
            // Reading with loose rules must still pass on strict cases
            bool b = Signature.TryReadLoose(data, out Signature sig, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expR, sig.R);
            Assert.Equal(expS, sig.S);
            Assert.Equal(expSH, sig.SigHash);
        }


        public static IEnumerable<object[]> GetReadLooseFailCases()
        {
            yield return new object[]
            {
                null,
                Errors.NullOrEmptyBytes
            };
            yield return new object[]
            {
                Array.Empty<byte>(),
                Errors.NullOrEmptyBytes
            };
            yield return new object[]
            {
                Helper.HexToBytes("3000"+"0102030405060708"),
                Errors.InvalidDerSeqLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("308a"+"0102030405060708"),
                Errors.InvalidDerSeqLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("30ff"+"0102030405060708"),
                Errors.InvalidDerSeqLength
            };
            yield return new object[]
            {
                // seq_len=8 covers the entire remaining bytes which means SigHash is missing
                Helper.HexToBytes("3008"+"0102030405060708"),
                Errors.InvalidDerSeqLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"01"+"02030405060708"),
                Errors.MissingDerIntTag1
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"0200"+"02030405060708"),
                Errors.InvalidDerRLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"0200"+"0102030405060708"),
                Errors.InvalidDerRLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"028a"+"0102030405060708"),
                Errors.InvalidDerRLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"02ff"+"0102030405060708"),
                Errors.InvalidDerRLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"0209"+"0102030405060708"),
                Errors.EndOfStream
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"0102030405060708"),
                Errors.MissingDerIntTag2
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"0200"+"010203040506"),
                Errors.InvalidDerSLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"028a"+"010203040506"),
                Errors.InvalidDerSLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02ff"+"010203040506"),
                Errors.InvalidDerSLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"0207"+"010203040506"),
                Errors.EndOfStream
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02020001"),
                Errors.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetReadLooseFailCases))]
        public void TryReadLoose_FailTest(byte[] data, Errors expErr)
        {
            bool b = Signature.TryReadLoose(data, out Signature sig, out Errors error);

            Assert.False(b, error.Convert());
            Assert.Null(sig);
            Assert.Equal(expErr, error);
        }


        public static IEnumerable<object[]> GetReadStrictCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"020101"+"01"),
                One,
                One,
                SigHashType.All
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020102"+"020103"+"02"),
                new Scalar8x32(2),
                new Scalar8x32(3),
                SigHashType.None
            };
            yield return new object[]
            {
                Helper.HexToBytes("3007"+"02017f"+"02020080"+"03"),
                new Scalar8x32(127),
                new Scalar8x32(128),
                SigHashType.Single
            };
            yield return new object[]
            {
                Helper.HexToBytes("300a"+"0204008350c4"+"02020081"+"81"),
                new Scalar8x32(8605892),
                new Scalar8x32(129),
                SigHashType.All | SigHashType.AnyoneCanPay
            };
            yield return new object[]
            {
                Helper.HexToBytes("3046"+"022100ff7353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+
                    "022100817353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+"82"),
                new Scalar8x32(Helper.HexToBytes("ff7353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"), out _),
                new Scalar8x32(Helper.HexToBytes("817353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"), out _),
                SigHashType.None | SigHashType.AnyoneCanPay
            };
            yield return new object[]
            {
                Helper.HexToBytes("3044"+"0220797353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+
                    "0220117353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+"83"),
                new Scalar8x32(Helper.HexToBytes("797353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"), out _),
                new Scalar8x32(Helper.HexToBytes("117353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"), out _),
                SigHashType.Single | SigHashType.AnyoneCanPay
            };
        }
        [Theory]
        [MemberData(nameof(GetReadStrictCases))]
        public void TryReadStrictTest(byte[] data, in Scalar8x32 expR, in Scalar8x32 expS, SigHashType expSH)
        {
            bool b = Signature.TryReadStrict(data, out Signature sig, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expR, sig.R);
            Assert.Equal(expS, sig.S);
            Assert.Equal(expSH, sig.SigHash);
        }


        public static IEnumerable<object[]> GetReadStrictFailCases()
        {
            yield return new object[]
            {
                null,
                Errors.NullBytes
            };
            yield return new object[]
            {
                Helper.HexToBytes("30"+"02030405060708"),
                Errors.InvalidDerEncodingLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("30"+Helper.GetBytesHex(73)),
                Errors.InvalidDerEncodingLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("31"+"0203040506070809"),
                Errors.MissingDerSeqTag
            };
            yield return new object[]
            {
                Helper.HexToBytes("30"+"00"+"03040506070809"),
                Errors.InvalidDerSeqLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("30"+"07"+"01020304050607"), // missing 1 byte (SigHash)
                Errors.InvalidDerSeqLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"020101"+"ff01"), // SigHash is bigger than 1 byte
                Errors.InvalidDerSeqLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"03"+"020304050607"),
                Errors.MissingDerIntTag1
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"02"+"00"+"0506070809"),
                Errors.InvalidDerRLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"02"+"0100"+"06070809"),
                Errors.InvalidDerRLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"02"+"22"+"0506070809"),
                Errors.InvalidDerRLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"0202"+"0506"+"070809"),
                Errors.InvalidDerIntLength1
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"03"+"070809"),
                Errors.MissingDerIntTag2
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02"+"00"+"0809"),
                Errors.InvalidDerSLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02"+"0100"+"09"),
                Errors.InvalidDerSLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02"+"2201"+"09"),
                Errors.InvalidDerSLength
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02"+"0201"+"09"), // Doesn't have s itself if we consider last byte as SigHash
                Errors.InvalidDerIntLength2
            };
            yield return new object[]
            {
                Helper.HexToBytes("3007"+"020101"+"020101"+"00"+"01"), // s has extra byte not covered by its length
                Errors.InvalidDerIntLength2
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020180"+"020101"+"09"),
                Errors.InvalidDerRFormat
            };
            yield return new object[]
            {
                Helper.HexToBytes("3007"+"02020079"+"020101"+"09"),
                Errors.InvalidDerRFormat
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"020180"+"09"),
                Errors.InvalidDerSFormat
            };
            yield return new object[]
            {
                Helper.HexToBytes("3007"+"020101"+"02020079"+"09"),
                Errors.InvalidDerSFormat
            };
        }
        [Theory]
        [MemberData(nameof(GetReadStrictFailCases))]
        public void TryReadStrict_FailTest(byte[] data, Errors expErr)
        {
            bool b = Signature.TryReadStrict(data, out Signature sig, out Errors error);

            Assert.False(b, error.Convert());
            Assert.Null(sig);
            Assert.Equal(expErr, error);
        }


        [Theory]
        [MemberData(nameof(GetReadStrictCases))]
        public void ToByteArrayTest(byte[] expBytes, in Scalar8x32 r, in Scalar8x32 s, SigHashType sh)
        {
            Signature sig = new(r, s, sh);
            byte[] actualBytes = sig.ToByteArray();
            Assert.Equal(expBytes, actualBytes);
        }


        public static IEnumerable<object[]> GetSigRecIdCases()
        {
            yield return new object[]
            {
                One,
                new Scalar8x32(2),
                (byte)2,
                true,
                Helper.HexToBytes("21"+"0000000000000000000000000000000000000000000000000000000000000001"+
                                        "0000000000000000000000000000000000000000000000000000000000000002")
            };
            yield return new object[]
            {
                new Scalar8x32(Helper.HexToBytes("797353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"), out _),
                new Scalar8x32(Helper.HexToBytes("017353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"), out _),
                (byte)0,
                false,
                Helper.HexToBytes("1b"+"797353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+
                                       "017353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c")
            };
            yield return new object[]
            {
                new Scalar8x32(Helper.HexToBytes("817353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"), out _),
                new Scalar8x32(Helper.HexToBytes("aa7353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"), out _),
                (byte)1,
                false,
                Helper.HexToBytes("1c"+"817353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+
                                       "aa7353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c")
            };
        }
        [Theory]
        [MemberData(nameof(GetSigRecIdCases))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning disable IDE0060 // Remove unused parameter
        public void TryReadWithRecIdTest(in Scalar8x32 r, in Scalar8x32 s, byte v, bool isComp, byte[] toRead)
#pragma warning restore IDE0060
#pragma warning restore xUnit1026
        {
            bool b = Signature.TryReadWithRecId(toRead, out Signature sig, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(r, sig.R);
            Assert.Equal(s, sig.S);
            Assert.Equal(toRead[0], sig.RecoveryId);
        }

        [Theory]
        [InlineData(null, "Byte array can not be null or empty.")]
        [InlineData(new byte[0], "Byte array can not be null or empty.")]
        [InlineData(new byte[] { 1, 2, 3 }, "Signatures with recovery ID must be fixed 65 bytes.")]
        public void TryReadWithRecId_FailTest(byte[] data, string expErr)
        {
            bool b = Signature.TryReadWithRecId(data, out Signature sig, out string error);
            Assert.False(b);
            Assert.Null(sig);
            Assert.Equal(expErr, error);
        }

        [Theory]
        [MemberData(nameof(GetSigRecIdCases))]
        public void ToByteArrayWithRecIdTest(in Scalar8x32 r, in Scalar8x32 s, byte v, bool isComp, byte[] expected)
        {
            Signature sig = new(r, s, v);
            byte[] actual = sig.ToByteArrayWithRecId(isComp);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToByteArrayWithRecId_NochangeTest()
        {
            Signature sig = new(One, new Scalar8x32(2), 164);
            byte[] actual = sig.ToByteArrayWithRecId();
            byte[] expected = Helper.HexToBytes("a4" + "0000000000000000000000000000000000000000000000000000000000000001" +
                                                       "0000000000000000000000000000000000000000000000000000000000000002");
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GetSigRecIdCases))]
        public void WriteToStreamWithRecIdTest(in Scalar8x32 r, in Scalar8x32 s, byte v, bool isComp, byte[] expected)
        {
            Signature sig = new(r, s, v);
            var stream = new FastStream(65);
            sig.WriteToStreamWithRecId(stream, isComp);

            byte[] actual = stream.ToByteArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteToStreamWithRecId_NochangeTest()
        {
            Signature sig = new(One, new Scalar8x32(2), 164);
            var stream = new FastStream(65);
            sig.WriteToStreamWithRecId(stream);
            byte[] expected = Helper.HexToBytes("a4" + "0000000000000000000000000000000000000000000000000000000000000001" +
                                                       "0000000000000000000000000000000000000000000000000000000000000002");
            Assert.Equal(expected, stream.ToByteArray());
        }
    }
}
