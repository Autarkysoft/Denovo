// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Asymmetric.EllipticCurve
{
    public class SignatureTests
    {
        [Fact]
        public void ConstructorTest()
        {
            Signature sig = new Signature(1, 2, 3);
            Assert.Equal(1, sig.R);
            Assert.Equal(2, sig.S);
            Assert.Equal(3, sig.RecoveryId);
        }

        public static IEnumerable<object[]> GetSigCases()
        {
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"020101"+"01"),
                new BigInteger(1),
                new BigInteger(1),
                SigHashType.All
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020102"+"020103"+"02"),
                new BigInteger(2),
                new BigInteger(3),
                SigHashType.None
            };
            yield return new object[]
            {
                Helper.HexToBytes("3007"+"02017f"+"02020080"+"03"),
                new BigInteger(127),
                new BigInteger(128),
                SigHashType.Single
            };
            yield return new object[]
            {
                Helper.HexToBytes("300a"+"0204008350c4"+"02020081"+"81"),
                new BigInteger(8605892),
                new BigInteger(129),
                SigHashType.All | SigHashType.AnyoneCanPay
            };
            yield return new object[]
            {
                Helper.HexToBytes("3046"+"022100ff7353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+
                    "022100817353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+"82"),
                BigInteger.Parse("115543541542776308314691939859152054848347353025896947872614337685168215546780"),
                BigInteger.Parse("58552122621284743379653095675188475201816032448276982759443808056273472052124"),
                SigHashType.None | SigHashType.AnyoneCanPay
            };
            yield return new object[]
            {
                Helper.HexToBytes("3045"+"0220797353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+
                    "022100817353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+"83"),
                BigInteger.Parse("54933619832618612272666502393666978081401345427475715133210758556026186750876"),
                BigInteger.Parse("58552122621284743379653095675188475201816032448276982759443808056273472052124"),
                SigHashType.Single | SigHashType.AnyoneCanPay
            };
        }
        [Theory]
        [MemberData(nameof(GetSigCases))]
        public void TryReadTest(byte[] data, BigInteger expR, BigInteger expS, SigHashType expSH)
        {
            bool b = Signature.TryRead(data, out Signature sig, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(expR, sig.R);
            Assert.Equal(expS, sig.S);
            Assert.Equal(expSH, sig.SigHash);
        }


        public static IEnumerable<object[]> GetSigFailCases()
        {
            yield return new object[]
            {
                null,
                "Byte array can not be null."
            };
            yield return new object[]
            {
                Helper.HexToBytes("30"+"02030405060708"),
                "Invalid DER encoding length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("30"+Helper.GetBytesHex(73)),
                "Invalid DER encoding length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("31"+"0203040506070809"),
                "Sequence tag was not found in DER encoded signature."
            };
            yield return new object[]
            {
                Helper.HexToBytes("30"+"00"+"03040506070809"),
                "Invalid data length according to sequence length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("30"+"07"+"03040506070809"),
                "Invalid data length according to sequence length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"03"+"040506070809"),
                "First integer tag was not found in DER encoded signature."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"02"+"00"+"0506070809"),
                "Invalid r length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"02"+"0100"+"06070809"),
                "Invalid r length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"02"+"22"+"0506070809"),
                "Invalid r length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"0202"+"0506"+"070809"),
                "Invalid data length according to first integer length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"03"+"070809"),
                "Second integer tag was not found in DER encoded signature."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02"+"00"+"0809"),
                "Invalid s length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02"+"0100"+"09"),
                "Invalid s length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02"+"2201"+"09"),
                "Invalid s length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"02"+"0201"+"09"),
                "Invalid data length according to second integer length."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020180"+"020101"+"09"),
                "Invalid r format."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3007"+"02020079"+"020101"+"09"),
                "Invalid r format."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3006"+"020101"+"020180"+"09"),
                "Invalid s format."
            };
            yield return new object[]
            {
                Helper.HexToBytes("3007"+"020101"+"02020079"+"09"),
                "Invalid s format."
            };
        }
        [Theory]
        [MemberData(nameof(GetSigFailCases))]
        public void TryRead_FailTest(byte[] data, string expErr)
        {
            bool b = Signature.TryRead(data, out Signature sig, out string error);

            Assert.False(b, error);
            Assert.Null(sig);
            Assert.Equal(expErr, error);
        }

        // TODO: No Schnorr tests since the BIP is not final at this point

        [Theory]
        [MemberData(nameof(GetSigCases))]
        public void ToByteArrayTest(byte[] expBytes, BigInteger r, BigInteger s, SigHashType sh)
        {
            Signature sig = new Signature()
            {
                R = r,
                S = s,
                SigHash = sh
            };

            byte[] actualBytes = sig.ToByteArray();

            Assert.Equal(expBytes, actualBytes);
        }


        public static IEnumerable<object[]> GetSigRecIdCases()
        {
            yield return new object[]
            {
                BigInteger.One,
                new BigInteger(2),
                (byte)2,
                true,
                Helper.HexToBytes("21"+"0000000000000000000000000000000000000000000000000000000000000001"+
                                       "0000000000000000000000000000000000000000000000000000000000000002")
            };
            yield return new object[]
            {
                BigInteger.Parse("54933619832618612272666502393666978081401345427475715133210758556026186750876"),
                BigInteger.Parse("656078002626645667867603170844521275181040115456700739715016052316907232156"),
                (byte)0,
                false,
                Helper.HexToBytes("1b"+"797353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+
                                       "017353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c")
            };
            yield return new object[]
            {
                BigInteger.Parse("58552122621284743379653095675188475201816032448276982759443808056273472052124"),
                BigInteger.Parse("77096949413198665302959386242986147943941303429883479343888186745040809221020"),
                (byte)1,
                false,
                Helper.HexToBytes("1c"+"817353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c"+
                                       "aa7353b5a071afd330a0c2e2da5cb587a6ff5a1de1b4723933efb751af3fdb9c")
            };
        }
        [Theory]
        [MemberData(nameof(GetSigRecIdCases))]
        public void WriteToStreamWithRecIdTest(BigInteger r, BigInteger s, byte v, bool isComp, byte[] expected)
        {
            Signature sig = new Signature(r, s, v);
            FastStream stream = new FastStream();
            sig.WriteToStreamWithRecId(stream, isComp);

            byte[] actual = stream.ToByteArray();

            Assert.Equal(expected, actual);
        }

    }
}
