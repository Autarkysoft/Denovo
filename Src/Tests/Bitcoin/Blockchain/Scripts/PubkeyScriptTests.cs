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
        private static readonly byte[] pubBaC1 = Helper.HexToBytes("02a17d82262d4ab8d9499d664c637c075a3ddc1bf2bb0b392188ba9e1043514ffd");
        private static readonly byte[] pubBaUC1 = Helper.HexToBytes("04a17d82262d4ab8d9499d664c637c075a3ddc1bf2bb0b392188ba9e1043514ffd2ec1051619dec03da6be55608dff5a2a800907e8358b3b76ea86f90f22cd2fc6");
        private static readonly byte[] pubBaC1_hash = Helper.HexToBytes("03814c6125f6ac2ebfc42d74339af43dc7530313");
        private static readonly byte[] pubBaUC1_hash = Helper.HexToBytes("7f8b56fd6eeb910db9c0bca69aebada4d3e16d6f");
        private static readonly string addrC1 = "1KXvPCw8vWJyWVoYnGcXnozMgKjUHexJN";
        private static readonly string addrBechC1 = "bc1qqwq5ccf976kza07y946r8xh58hr4xqcnzq75um";

        private static PublicKey GetPubKey()
        {
            PublicKey.TryRead(pubBaUC1, out PublicKey pub);
            return pub;
        }


        [Fact]
        public void ConstructorTest()
        {
            PubkeyScript scr = new PubkeyScript();

            Assert.Empty(scr.OperationList);
            Assert.False(scr.IsWitness);
            Assert.Equal(ScriptType.ScriptPub, scr.ScriptType);
        }

        [Fact]
        public void Serialize_CtorWithBytes_Test()
        {
            PubkeyScript scr = new PubkeyScript(new byte[] { 10, 20, 30 });
            FastStream stream = new FastStream(4);
            scr.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[] { 3, 10, 20, 30 };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Serialize_CtorWithOps_Test()
        {
            PubkeyScript scr = new PubkeyScript()
            {
                OperationList = new IOperation[]
                {
                    new DUPOp()
                }
            };
            FastStream stream = new FastStream(2);
            scr.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[] { 1, (byte)OP.DUP };

            Assert.Equal(expected, actual);
        }

        // All cases could be found on testnet txid = c79220f88b08c8ef851fc0b2a90141e6394875188d0e09cbdc003271d93803f7
        [Theory]
        [InlineData(new byte[] { 0 })] // Empty PubkeyScript
        [InlineData(new byte[] { 1, 55 })] // OP_5
        [InlineData(new byte[] { 4, 3, 1, 2, 3 })] // push [1,2,3] with correct push OP
        [InlineData(new byte[] { 5, 1, 2, 3, 4, 5 })] // [1,2,3,4,5] without the correct Push OP (push1byte(2)+push3byte(4,5,X))
        [InlineData(new byte[] { 1, 0xfe })] // invalid OP code
        [InlineData(new byte[] { 3, 0xba, 0xbb, 0xbc })] // 3 invalid OP codes
        public void TryDeserializeTest(byte[] data)
        {
            PubkeyScript scr = new PubkeyScript();
            bool b = scr.TryDeserialize(new FastStreamReader(data), out string error);
            FastStream write = new FastStream(data.Length);
            scr.Serialize(write);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(data, write.ToByteArray());
        }


        public static IEnumerable<object[]> GetP2pkCases()
        {
            yield return new object[] { GetPubKey(), true, new IOperation[] { new PushDataOp(pubBaC1), new CheckSigOp() } };
            yield return new object[] { GetPubKey(), false, new IOperation[] { new PushDataOp(pubBaUC1), new CheckSigOp() } };
        }
        [Theory]
        [MemberData(nameof(GetP2pkCases))]
        public void SetToP2PKTest(PublicKey pub, bool comp, IOperation[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2PK(pub, comp);
            Assert.Equal(expected, scr.OperationList);
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
            IOperation[] expected = new IOperation[]
            {
                new DUPOp(), new Hash160Op(), new PushDataOp(Helper.GetBytes(20)), new EqualVerifyOp(), new CheckSigOp()
            };

            Assert.Equal(expected, scr.OperationList);
        }

        public static IEnumerable<object[]> GetP2pkhCases()
        {
            yield return new object[]
            {
                GetPubKey(), true,
                new IOperation[] { new DUPOp(), new Hash160Op(), new PushDataOp(pubBaC1_hash), new EqualVerifyOp(), new CheckSigOp() }
            };
            yield return new object[]
            {
                GetPubKey(), false,
                new IOperation[] { new DUPOp(), new Hash160Op(), new PushDataOp(pubBaUC1_hash), new EqualVerifyOp(), new CheckSigOp() }
            };
        }
        [Theory]
        [MemberData(nameof(GetP2pkhCases))]
        public void SetToP2PKH_FromPubTest(PublicKey pub, bool comp, IOperation[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2PKH(pub, comp);
            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2PKH_FromAddressTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2PKH(addrC1);
            IOperation[] expected = new IOperation[]
            {
                new DUPOp(), new Hash160Op(), new PushDataOp(pubBaC1_hash), new EqualVerifyOp(), new CheckSigOp()
            };

            Assert.Equal(expected, scr.OperationList);
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
            IOperation[] expected = new IOperation[]
            {
                new Hash160Op(), new PushDataOp(Helper.GetBytes(20)), new EqualOp()
            };

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2SH_FromAddressTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2SH("3FuPMWfen385RLwbMsZEVhx9QsHyR6zEmv");
            IOperation[] expected = new IOperation[]
            {
                new Hash160Op(), new PushDataOp(Helper.HexToBytes("9be8a44c3ef40c1eeab2d487ecd2ef7c7cd9ce55")), new EqualOp()
            };

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2SH_FromScriptTest()
        {
            PubkeyScript scr = new PubkeyScript();
            var redeem = new MockSerializableRedeemScript(new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2SH(redeem);
            IOperation[] expected = new IOperation[]
            {
                new Hash160Op(), new PushDataOp(Helper.HexToBytes("9bc4860bb936abf262d7a51f74b4304833fee3b2")), new EqualOp()
            };

            Assert.Equal(expected, scr.OperationList);
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
            Assert.Throws<FormatException>(() => scr.SetToP2SH(addrC1));
        }


        public static IEnumerable<object[]> GetNestedP2wpkhCases()
        {
            yield return new object[]
            {
                GetPubKey(), true,
                new IOperation[]
                {
                    new Hash160Op(), new PushDataOp(Helper.HexToBytes("46243ca6fa444aa034ee462a144cfc9d446a53d8")), new EqualOp()
                }
            };
            yield return new object[]
            {
                GetPubKey(), false,
                new IOperation[]
                {
                    new Hash160Op(), new PushDataOp(Helper.HexToBytes("c5410f77f29a5c65647a317ebd5334774e4b3866")), new EqualOp()
                }
            };
        }
        [Theory]
        [MemberData(nameof(GetNestedP2wpkhCases))]
        public void SetToP2SH_P2WPKH_FromPubkeyTest(PublicKey pub, bool comp, IOperation[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2SH_P2WPKH(pub, comp);
            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2SH_P2WPKH_FromScriptTest()
        {
            PubkeyScript scr = new PubkeyScript();
            var redeem = new MockSerializableRedeemScript(RedeemScriptType.P2SH_P2WPKH, new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2SH_P2WPKH(redeem);
            IOperation[] expected = new IOperation[]
            {
                new Hash160Op(), new PushDataOp(Helper.HexToBytes("9bc4860bb936abf262d7a51f74b4304833fee3b2")), new EqualOp()
            };

            Assert.Equal(expected, scr.OperationList);
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
            IOperation[] expected = new IOperation[]
            {
                new Hash160Op(), new PushDataOp(Helper.HexToBytes("9bc4860bb936abf262d7a51f74b4304833fee3b2")), new EqualOp()
            };

            Assert.Equal(expected, scr.OperationList);
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
            IOperation[] expected = new IOperation[]
            {
                new PushDataOp(OP._0), new PushDataOp(Helper.GetBytes(20))
            };

            Assert.Equal(expected, scr.OperationList);
        }

        public static IEnumerable<object[]> GetP2wpkhCases()
        {
            yield return new object[]
            {
                GetPubKey(), true,
                new IOperation[] { new PushDataOp(OP._0), new PushDataOp(pubBaC1_hash) }
            };
            yield return new object[]
            {
                GetPubKey(), false,
                new IOperation[] { new PushDataOp(OP._0), new PushDataOp(pubBaUC1_hash) }
            };
        }
        [Theory]
        [MemberData(nameof(GetP2wpkhCases))]
        public void SetToP2WPKH_FromPubTest(PublicKey pub, bool comp, IOperation[] expected)
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WPKH(pub, comp);
            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2WPKH_FromAddressTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WPKH(addrBechC1);
            IOperation[] expected = new IOperation[]
            {
                new PushDataOp(OP._0), new PushDataOp(pubBaC1_hash)
            };

            Assert.Equal(expected, scr.OperationList);
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
            Assert.Throws<FormatException>(() => scr.SetToP2WPKH(addrC1));
        }


        [Fact]
        public void SetToP2WSH_FromBytesTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WSH(Helper.GetBytes(32));
            IOperation[] expected = new IOperation[]
            {
                new PushDataOp(OP._0), new PushDataOp(Helper.GetBytes(32))
            };

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2WSH_FromScriptTest()
        {
            PubkeyScript scr = new PubkeyScript();
            var mock = new MockSerializableScript(new byte[] { 1, 2, 3 }, 255);
            scr.SetToP2WSH(mock);
            IOperation[] expected = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(Helper.HexToBytes("039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81"))
            };

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToP2WSH_FromAddressTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToP2WSH("bc1qr8rpjl3pgzuaqd8myzu6c7ah2wjpyv7278sa4ld8x94fnnh5zstq6q0csc");
            IOperation[] expected = new IOperation[]
            {
                new PushDataOp(OP._0),
                new PushDataOp(Helper.HexToBytes("19c6197e2140b9d034fb20b9ac7bb753a41233caf1e1dafda7316a99cef41416"))
            };

            Assert.Equal(expected, scr.OperationList);
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
            Assert.Throws<FormatException>(() => scr.SetToP2WSH(addrC1));
        }


        [Fact]
        public void SetToReturn_FromBytesTest()
        {
            PubkeyScript scr = new PubkeyScript();
            scr.SetToReturn(Helper.GetBytes(12));
            IOperation[] expected = new IOperation[]
            {
                new ReturnOp(Helper.GetBytes(12))
            };

            Assert.Equal(expected, scr.OperationList);
        }

        [Fact]
        public void SetToReturn_FromScriptTest()
        {
            PubkeyScript scr = new PubkeyScript();
            var mock = new MockSerializableScript(new byte[] { 1, 2, 3 }, 255);
            scr.SetToReturn(mock);
            IOperation[] expected = new IOperation[]
            {
                new ReturnOp(new byte[] { 1, 2, 3 })
            };

            Assert.Equal(expected, scr.OperationList);
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
