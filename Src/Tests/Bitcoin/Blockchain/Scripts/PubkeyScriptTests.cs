// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts
{
    public class PubkeyScriptTests
    {
        [Fact]
        public void ConstructorTest()
        {
            PubkeyScript scr = new PubkeyScript();
            Assert.Empty(scr.Data);
        }

        [Fact]
        public void Constructor_WithNullBytesTest()
        {
            byte[] data = null;
            PubkeyScript scr = new PubkeyScript(data);
            Assert.Empty(scr.Data); // NotNull
        }

        [Fact]
        public void Constructor_WithBytesTest()
        {
            byte[] data = Helper.GetBytes(10);
            PubkeyScript scr = new PubkeyScript(data);
            Assert.Equal(data, scr.Data);
        }

        [Fact]
        public void Constructor_OpsTest()
        {
            PubkeyScript scr = new PubkeyScript(new IOperation[] { new DUPOp(), new PushDataOp(new byte[] { 10, 20, 30 }) });
            Assert.Equal(new byte[] { (byte)OP.DUP, 3, 10, 20, 30 }, scr.Data);
        }

        [Fact]
        public void Constructor_EmptyOpsTest()
        {
            PubkeyScript scr = new PubkeyScript(new IOperation[0]);
            Assert.Equal(new byte[0], scr.Data);
        }

        public static IEnumerable<object[]> GetSerCases()
        {
            yield return new object[] { new byte[0], new byte[1] };
            yield return new object[] { new byte[] { 1, 2, 3 }, new byte[1] { 3 } };
            yield return new object[] { Helper.GetBytes(10100), new byte[3] { 0xfd, 0x74, 0x27 } };
        }
        [Theory]
        [MemberData(nameof(GetSerCases))]
        public void SerializeTest(byte[] data, byte[] start)
        {
            PubkeyScript scr = new PubkeyScript(data);
            FastStream stream = new FastStream(data.Length + start.Length);
            scr.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[start.Length + data.Length];
            Buffer.BlockCopy(start, 0, expected, 0, start.Length);
            Buffer.BlockCopy(data, 0, expected, start.Length, data.Length);

            Assert.Equal(expected, actual);
        }


        // This test makes sure PubkeyScript is not evaluating scripts while deserializing since some of these are broken scripts
        // All cases could be found on testnet txid = c79220f88b08c8ef851fc0b2a90141e6394875188d0e09cbdc003271d93803f7
        [Theory]
        [InlineData(new byte[1] { 0 }, new byte[0])] // Empty PubkeyScript
        [InlineData(new byte[] { 1, 55 }, new byte[] { 55 })] // OP_5
        [InlineData(new byte[] { 4, 3, 1, 2, 3 }, new byte[] { 3, 1, 2, 3 })] // push [1,2,3] with correct push OP
        // [1,2,3,4,5] without the correct Push OP (push1byte(2)+push3byte(4,5,X))
        [InlineData(new byte[] { 5, 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3, 4, 5 })]
        [InlineData(new byte[] { 1, 0xfe }, new byte[] { 0xfe })] // invalid OP code
        [InlineData(new byte[] { 3, 0xba, 0xbb, 0xbc }, new byte[] { 0xba, 0xbb, 0xbc })] // 3 invalid OP codes
        public void TryDeserializeTest(byte[] data, byte[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            bool b = scr.TryDeserialize(new FastStreamReader(data), out string error);
            FastStream write = new FastStream(data.Length);
            scr.Serialize(write);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void TryDeserialize_LargeScriptTest()
        {
            // TestNet TxId= 88bac1e84c235aa0418345bf430fb43b336875974b6e87dc958de196f9222c35
            byte[] expected = Helper.HexToBytes("4d1127").ConcatFast(Helper.GetBytes(10001)).AppendToEnd((byte)OP.DROP);
            byte[] veryLongData = Helper.HexToBytes("fd1527").ConcatFast(expected);
            PubkeyScript scr = new PubkeyScript();
            bool b = scr.TryDeserialize(new FastStreamReader(veryLongData), out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(expected, scr.Data);
        }

        public static IEnumerable<object[]> GetIsUnspendableCases()
        {
            yield return new object[] { new byte[] { (byte)OP.RETURN }, true };
            yield return new object[] { new byte[] { (byte)OP.RETURN, 1, 2, 3 }, true };
            yield return new object[] { Helper.GetBytes(Constants.MaxScriptLength + 1), true };
            yield return new object[] { new byte[] { (byte)OP.DUP }, false };
            yield return new object[] { new byte[] { 1, 2, 3 }, false };
        }
        [Theory]
        [MemberData(nameof(GetIsUnspendableCases))]
        public void IsUnspendableTest(byte[] data, bool expected)
        {
            PubkeyScript scr = new PubkeyScript(data);
            Assert.Equal(expected, scr.IsUnspendable());
        }

        public static IEnumerable<object[]> GetSpecialTypeCases()
        {
            yield return new object[] { Helper.HexToBytes("00"), PubkeyScriptSpecialType.None };
            yield return new object[] { Helper.HexToBytes($"0014{Helper.GetBytesHex(20)}"), PubkeyScriptSpecialType.P2WPKH };
            yield return new object[] { Helper.HexToBytes($"0020{Helper.GetBytesHex(32)}"), PubkeyScriptSpecialType.P2WSH };
            yield return new object[] { Helper.HexToBytes($"a914{Helper.GetBytesHex(20)}87"), PubkeyScriptSpecialType.P2SH };
        }
        [Theory]
        [MemberData(nameof(GetSpecialTypeCases))]
        public void GetSpecialTypeTest(byte[] data, PubkeyScriptSpecialType expected)
        {
            PubkeyScript scr = new PubkeyScript(data);
            Assert.Equal(expected, scr.GetSpecialType());
        }

        public static IEnumerable<object[]> GetP2pkCases()
        {
            yield return new object[]
            {
                KeyHelper.Pub1,
                false,
                Helper.ConcatBytes(67, new byte[] { 65 }, KeyHelper.Pub1UnCompBytes, new byte[] { (byte)OP.CheckSig })
            };
            yield return new object[]
            {
                KeyHelper.Pub1,
                true,
                Helper.ConcatBytes(35, new byte[] { 33 }, KeyHelper.Pub1CompBytes, new byte[] { (byte)OP.CheckSig })
            };
        }
        [Theory]
        [MemberData(nameof(GetP2pkCases))]
        public void SetToP2PKTest(PublicKey pub, bool comp, byte[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2PK(pub, comp);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2PK_ExceptionTest()
        {
            PubkeyScript scr = new PubkeyScript();
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PK(null, true));
        }


        [Fact]
        public void SetToP2PKH_FromByteTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2PKH(Helper.GetBytes(20));
            byte[] expected = Helper.HexToBytes($"76a914{Helper.GetBytesHex(20)}88ac");
            Assert.Equal(expected, scr.Data);
        }

        public static IEnumerable<object[]> GetP2pkhCases()
        {
            yield return new object[]
            {
                KeyHelper.Pub1, true,
                Helper.HexToBytes($"76a914{KeyHelper.Pub1CompHashHex}88ac")
            };
            yield return new object[]
            {
                KeyHelper.Pub1, false,
                Helper.HexToBytes($"76a914{KeyHelper.Pub1UnCompHashHex}88ac")
            };
        }
        [Theory]
        [MemberData(nameof(GetP2pkhCases))]
        public void SetToP2PKH_FromPubTest(PublicKey pub, bool comp, byte[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2PKH(pub, comp);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2PKH_FromAddressTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2PKH(KeyHelper.Pub1CompAddr);
            byte[] expected = Helper.HexToBytes($"76a914{KeyHelper.Pub1CompHashHex}88ac");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2PKH_ExceptionTest()
        {
            PubkeyScript scr = new PubkeyScript();
            byte[] nba = null;
            PublicKey npub = null;
            string naddr = null;

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(nba));
            Assert.Throws<ArgumentOutOfRangeException>(() => scr.SetToP2PKH(new byte[19]));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(npub, true));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(naddr));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(""));
            Assert.Throws<FormatException>(() => scr.SetToP2PKH("$"));
            Assert.Throws<FormatException>(() => scr.SetToP2PKH("3FuPMWfen385RLwbMsZEVhx9QsHyR6zEmv"));
        }


        [Fact]
        public void SetToP2SH_FromBytesTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2SH(Helper.GetBytes(20));
            byte[] expected = Helper.HexToBytes($"a914{Helper.GetBytesHex(20)}87");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_FromAddressTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2SH("3FuPMWfen385RLwbMsZEVhx9QsHyR6zEmv");
            byte[] expected = Helper.HexToBytes($"a9149be8a44c3ef40c1eeab2d487ecd2ef7c7cd9ce5587");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_FromScriptTest()
        {
            PubkeyScript scr = new PubkeyScript();
            var redeem = new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2SH(redeem);
            byte[] expected = Helper.HexToBytes($"a9149bc4860bb936abf262d7a51f74b4304833fee3b287");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_ExceptionTest()
        {
            PubkeyScript scr = new PubkeyScript();
            byte[] nba = null;
            RedeemScript nscr = null;
            string naddr = null;

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH(nba));
            Assert.Throws<ArgumentOutOfRangeException>(() => scr.SetToP2SH(new byte[19]));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH(nscr));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH(naddr));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH(""));
            Assert.Throws<FormatException>(() => scr.SetToP2SH("$"));
            Assert.Throws<FormatException>(() => scr.SetToP2SH(KeyHelper.Pub1CompAddr));
        }


        public static IEnumerable<object[]> GetNestedP2wpkhCases()
        {
            yield return new object[]
            {
                KeyHelper.Pub1, true, Helper.HexToBytes($"a914{KeyHelper.Pub1NestedSegwitHex}87")
            };
            yield return new object[]
            {
                KeyHelper.Pub1, false, Helper.HexToBytes($"a914{KeyHelper.Pub1NestedSegwitUncompHex}87")
            };
        }
        [Theory]
        [MemberData(nameof(GetNestedP2wpkhCases))]
        public void SetToP2SH_P2WPKH_FromPubkeyTest(PublicKey pub, bool comp, byte[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2SH_P2WPKH(pub, comp);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_P2WPKH_FromScriptTest()
        {
            PubkeyScript scr = new PubkeyScript();
            var redeem = new MockSerializableRedeemScript(RedeemScriptType.P2SH_P2WPKH, new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2SH_P2WPKH(redeem);
            byte[] expected = Helper.HexToBytes($"a9149bc4860bb936abf262d7a51f74b4304833fee3b287");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_P2WPKH_ExceptionTest()
        {
            PubkeyScript scr = new PubkeyScript();

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WPKH(null, true));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WPKH(null));
            Assert.Throws<ArgumentException>(()
                => scr.SetToP2SH_P2WPKH(new MockSerializableRedeemScript(RedeemScriptType.Empty, new byte[0], 1)));
        }


        [Fact]
        public void SetToP2SH_P2WSH_Test()
        {
            PubkeyScript scr = new PubkeyScript();
            var redeem = new MockSerializableRedeemScript(RedeemScriptType.P2SH_P2WSH, new byte[] { 1, 2, 3 }, 10);
            scr.SetToP2SH_P2WSH(redeem);
            byte[] expected = Helper.HexToBytes($"a9149bc4860bb936abf262d7a51f74b4304833fee3b287");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2SH_P2WSH_ExceptionTest()
        {
            PubkeyScript scr = new PubkeyScript();

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2SH_P2WSH(null));
            Assert.Throws<ArgumentException>(()
                => scr.SetToP2SH_P2WSH(new MockSerializableRedeemScript(RedeemScriptType.Empty, new byte[0], 1)));
        }


        [Fact]
        public void SetToP2WPKH_FromByteTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WPKH(Helper.GetBytes(20));
            byte[] expected = Helper.HexToBytes($"0014{Helper.GetBytesHex(20)}");
            Assert.Equal(expected, scr.Data);
        }

        public static IEnumerable<object[]> GetP2wpkhCases()
        {
            yield return new object[]
            {
                KeyHelper.Pub1, true, Helper.HexToBytes($"0014{KeyHelper.Pub1BechAddrHex}")
            };
            yield return new object[]
            {
                KeyHelper.Pub1, false, Helper.HexToBytes($"0014{KeyHelper.Pub1BechAddrHexUncomp}")
            };
        }
        [Theory]
        [MemberData(nameof(GetP2wpkhCases))]
        public void SetToP2WPKH_FromPubTest(PublicKey pub, bool comp, byte[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WPKH(pub, comp);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2WPKH_FromAddressTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WPKH(KeyHelper.Pub1BechAddr);
            byte[] expected = Helper.HexToBytes($"0014{KeyHelper.Pub1BechAddrHex}");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2WPKH_ExceptionTest()
        {
            PubkeyScript scr = new PubkeyScript();
            byte[] nba = null;
            PublicKey npub = null;
            string naddr = null;

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2WPKH(nba));
            Assert.Throws<ArgumentOutOfRangeException>(() => scr.SetToP2WPKH(new byte[19]));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2WPKH(npub));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2WPKH(naddr));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2WPKH(""));
            Assert.Throws<FormatException>(() => scr.SetToP2WPKH(KeyHelper.Pub1CompAddr));
        }


        [Fact]
        public void SetToP2WSH_FromBytesTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WSH(Helper.GetBytes(32));
            byte[] expected = Helper.HexToBytes($"0020{Helper.GetBytesHex(32)}");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2WSH_FromScriptTest()
        {
            PubkeyScript scr = new PubkeyScript();
            var mock = new MockSerializableScript(new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2WSH(mock);
            byte[] expected = Helper.HexToBytes("0020039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2WSH_FromAddressTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WSH("bc1qr8rpjl3pgzuaqd8myzu6c7ah2wjpyv7278sa4ld8x94fnnh5zstq6q0csc");
            byte[] expected = Helper.HexToBytes("002019c6197e2140b9d034fb20b9ac7bb753a41233caf1e1dafda7316a99cef41416");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2WSH_ExceptionTest()
        {
            PubkeyScript scr = new PubkeyScript();
            byte[] nba = null;
            IScript nscr = null;
            string naddr = null;

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2WSH(nba));
            Assert.Throws<ArgumentOutOfRangeException>(() => scr.SetToP2WSH(new byte[20]));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2WSH(nscr));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2WSH(naddr));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2WSH(""));
            Assert.Throws<FormatException>(() => scr.SetToP2WSH(KeyHelper.Pub1CompAddr));
        }


        [Fact]
        public void SetToReturn_FromBytesTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToReturn(Helper.GetBytes(12));
            byte[] expected = Helper.HexToBytes($"6a0c{Helper.GetBytesHex(12)}");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToReturn_FromScriptTest()
        {
            PubkeyScript scr = new PubkeyScript();
            var mock = new MockSerializableScript(new byte[] { 1, 2, 3 }, 255);
            scr.SetToReturn(mock);
            byte[] expected = Helper.HexToBytes("6a03010203");
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToReturn_ExceptionTest()
        {
            PubkeyScript scr = new PubkeyScript();
            byte[] nba = null;
            IScript nscr = null;

            Assert.Throws<ArgumentNullException>(() => scr.SetToReturn(nba));
            Assert.Throws<ArgumentNullException>(() => scr.SetToReturn(nscr));
        }
    }
}
