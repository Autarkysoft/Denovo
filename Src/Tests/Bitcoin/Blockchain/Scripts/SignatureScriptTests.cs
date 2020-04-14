// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts
{
    public class SignatureScriptTests
    {
        private static readonly Signature sig1 = new Signature(10, 20) { SigHash = SigHashType.All };
        private static readonly string sig1Hex = "300602010a02011401";
        private static readonly string sig1Len = $"{sig1Hex.Length / 2:x2}";
        private static readonly byte[] sigBa1 = Helper.HexToBytes(sig1Hex);
        private static readonly Signature sig2 = new Signature(11, 22) { SigHash = SigHashType.None };
        private static readonly string sig2Hex = "300602010b02011502";
        private static readonly string sig2Len = $"{sig2Hex.Length / 2:x2}";
        private static readonly byte[] sigBa2 = Helper.HexToBytes(sig2Hex);


        [Fact]
        public void ConstructorTest()
        {
            SignatureScript scr = new SignatureScript();
            Assert.Empty(scr.Data);
        }

        [Fact]
        public void Constructor_WithNullBytesTest()
        {
            byte[] data = null;
            SignatureScript scr = new SignatureScript(data);
            Assert.Empty(scr.Data); // NotNull
        }

        [Fact]
        public void Constructor_WithBytesTest()
        {
            byte[] data = Helper.GetBytes(10);
            SignatureScript scr = new SignatureScript(data);
            Assert.Equal(data, scr.Data);
        }

        [Fact]
        public void Constructor_OpsTest()
        {
            SignatureScript scr = new SignatureScript(new IOperation[] { new DUPOp(), new PushDataOp(new byte[] { 10, 20, 30 }) });
            Assert.Equal(new byte[] { (byte)OP.DUP, 3, 10, 20, 30 }, scr.Data);
        }

        [Fact]
        public void Constructor_EmptyOpsTest()
        {
            SignatureScript scr = new SignatureScript(new IOperation[0]);
            Assert.Equal(new byte[0], scr.Data);
        }


        [Fact]
        public void SetToEmptyTest()
        {
            SignatureScript scr = new SignatureScript(new byte[2]);
            Assert.NotEmpty(scr.Data);
            scr.SetToEmpty();
            Assert.Empty(scr.Data);
        }

        [Fact]
        public void SetToP2PKTest()
        {
            SignatureScript scr = new SignatureScript();
            scr.SetToP2PK(sig1);
            byte[] expected = Helper.HexToBytes($"{sig1Len}{sig1Hex}");

            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2PKTest_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PK(null));
        }


        public static IEnumerable<object[]> GetP2PKHCases()
        {
            yield return new object[] 
            { 
                true, KeyHelper.Pub1, sig1, 
                Helper.HexToBytes($"{sig1Len}{sig1Hex}21{KeyHelper.Pub1CompHex}")
            };
            yield return new object[]
            {
                false, KeyHelper.Pub1, sig1,
                Helper.HexToBytes($"{sig1Len}{sig1Hex}41{KeyHelper.Pub1UnCompHex}")
            };
        }
        [Theory]
        [MemberData(nameof(GetP2PKHCases))]
        public void SetToP2PKHTest(bool useComp, PublicKey pub, Signature sig, byte[] expected)
        {
            SignatureScript scr = new SignatureScript();
            scr.SetToP2PKH(sig, pub, useComp);
            Assert.Equal(expected, scr.Data);
        }

        [Fact]
        public void SetToP2PKH_ExceptionTest()
        {
            SignatureScript scr = new SignatureScript();

            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(null, KeyHelper.Pub1, true));
            Assert.Throws<ArgumentNullException>(() => scr.SetToP2PKH(sig1, null, true));
        }
    }
}
