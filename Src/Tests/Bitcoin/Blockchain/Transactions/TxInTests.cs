// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.EllipticCurve;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Transactions
{
    public class TxInTests
    {
        [Fact]
        public void ConstructorTest()
        {
            TxIn tx = new TxIn(new byte[32], 1, null, 0);

            Assert.Equal(new byte[32], tx.TxHash);
            Assert.Equal(1U, tx.Index);
            Assert.Equal(0U, tx.Sequence);
            Assert.NotNull(tx.SigScript);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new TxIn(null, 1, null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TxIn(new byte[31], 1, null, 0));
        }

        [Fact]
        public void SerializeTest()
        {
            var scr = new MockSerializableSigScript(new byte[1] { 255 }, 2);
            TxIn tx = new TxIn(Helper.GetBytes(32), 1, scr, 953132143);
            FastStream stream = new FastStream();
            tx.Serialize(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[32 + 4 + 2 + 4];
            Buffer.BlockCopy(Helper.GetBytes(32), 0, expected, 0, 32);
            expected[32] = 1;
            expected[36] = 2;
            expected[37] = 255;
            expected[38] = 0x6f;
            expected[39] = 0xa4;
            expected[40] = 0xcf;
            expected[41] = 0x38;

            Assert.Equal(expected, actual);
        }


        private static readonly Signature sig = new Signature(1, 2) { SigHash = SigHashType.All };
        private static readonly byte[] sigBa = sig.ToByteArray();

        public static IEnumerable<object[]> GetSignSerCases()
        {
            byte[] txHash = Helper.GetBytes(32);
            byte[] indexBa = new byte[] { 0x78, 0x56, 0x34, 0x12 };
            byte[] seqBa = new byte[] { 0x98, 0xba, 0xdc, 0xfe };
            TxIn tx = new TxIn()
            {
                TxHash = txHash,
                Index = 0x12345678,
                Sequence = 0xfedcba98
            };
            int len1 = 32 + 4 + 4;

            yield return new object[]
            {
                tx,
                new IOperation[] { new CheckSigOp() },
                Helper.ConcatBytes(len1 + 2, txHash, indexBa, new byte[] { 1, (byte)OP.CheckSig }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[] { new PushDataOp(sigBa), new CheckSigOp(), new PushDataOp(sigBa) },
                Helper.ConcatBytes(len1 + 2, txHash, indexBa, new byte[] { 1, (byte)OP.CheckSig, }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[] { new DUP2Op(), new CheckSigOp(), new ROTOp() },
                Helper.ConcatBytes(len1 + 4, txHash, indexBa, new byte[] { 3, (byte)OP.DUP2, (byte)OP.CheckSig, (byte)OP.ROT }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new DUPOp(),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new PushDataOp(Helper.GetBytes(10)),
                    new CheckSigOp()
                },
                Helper.ConcatBytes(len1 + 14, txHash, indexBa,
                        new byte[] { 13, (byte)OP.DUP, 10 }, Helper.GetBytes(10), new byte[] { (byte)OP.CheckSig }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new DUPOp(),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new PushDataOp(Helper.GetBytes(10)),
                    new CheckSigOp()
                },
                Helper.ConcatBytes(len1 + 13, txHash, indexBa,
                        new byte[] { 12, 10 }, Helper.GetBytes(10), new byte[] { (byte)OP.CheckSig }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } }, false),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                Helper.ConcatBytes(len1 + 5, txHash, indexBa,
                        new byte[] { 4, (byte)OP.IF, (byte)OP.ELSE, (byte)OP.EndIf, (byte)OP.CheckSig }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } }, false),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                Helper.ConcatBytes(len1 + 4, txHash, indexBa,
                        new byte[] { 3, (byte)OP.ELSE, (byte)OP.EndIf, (byte)OP.CheckSig }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } }, false),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                Helper.ConcatBytes(len1 + 3, txHash, indexBa,
                        new byte[] { 2, (byte)OP.EndIf, (byte)OP.CheckSig }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } }, false),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new CheckSigOp()
                },
                Helper.ConcatBytes(len1 + 2, txHash, indexBa,
                        new byte[] { 1, (byte)OP.CheckSig }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new PushDataOp(sigBa) }, null, false),
                    new CheckSigOp()
                },
                Helper.ConcatBytes(len1 + 4, txHash, indexBa,
                        new byte[] { 3, (byte)OP.IF, (byte)OP.EndIf, (byte)OP.CheckSig }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }, false),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                Helper.ConcatBytes(len1 + 11, txHash, indexBa,
                        new byte[] { 10,
                            (byte)OP.IF, (byte)OP.ROLL, (byte)OP.ADD, (byte)OP.ELSE, (byte)OP.SUB1, (byte)OP.IfDup, (byte)OP.EndIf,
                            (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = true }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }, false),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                Helper.ConcatBytes(len1 + 9, txHash, indexBa,
                        new byte[] { 8,
                            (byte)OP.ADD, (byte)OP.ELSE, (byte)OP.SUB1, (byte)OP.IfDup, (byte)OP.EndIf,
                            (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = true }, new IfDupOp() }, false),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                Helper.ConcatBytes(len1 + 6, txHash, indexBa,
                        new byte[] { 5,
                            (byte)OP.IfDup, (byte)OP.EndIf,
                            (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL }, seqBa)
            };
            yield return new object[]
            {
                tx,
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }, false),
                    new Hash160Op(),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                Helper.ConcatBytes(len1 + 3, txHash, indexBa,
                        new byte[] { 2, (byte)OP.CheckMultiSig, (byte)OP.EQUAL }, seqBa)
            };
        }
        [Theory]
        [MemberData(nameof(GetSignSerCases))]
        public void SerializeForSigningTest(TxIn tx, IOperation[] ops, byte[] expected)
        {
            FastStream stream = new FastStream();
            tx.SerializeForSigning(stream, ops, sigBa, false);
            byte[] actual = stream.ToByteArray();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeForSigning_ChangeSequenceTest()
        {
            TxIn tx = new TxIn()
            {
                TxHash = Helper.GetBytes(32),
                Index = 1,
                Sequence = 123456789
            };
            FastStream stream = new FastStream();
            tx.SerializeForSigning(stream, new IOperation[] { new CheckSigOp() }, sigBa, true);

            byte[] actual = stream.ToByteArray();
            byte[] expected = Helper.ConcatBytes(32 + 4 + 6, Helper.GetBytes(32), Helper.HexToBytes("0100000001ac00000000"));

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetDeserCases()
        {
            yield return new object[] { new byte[41], new MockDeserializableSigScript(36, 1), new byte[32], 0, 0 };
            yield return new object[]
            {
                Helper.HexToBytes("a5c63f45d7f03633aec127b2821c181ea326044e9ab20d2abaf20bafffe79c4e"+"7b000000"+"ff"+"e73403b3"),
                new MockDeserializableSigScript(36, 1),
                Helper.HexToBytes("a5c63f45d7f03633aec127b2821c181ea326044e9ab20d2abaf20bafffe79c4e"),
                123,
                3003331815
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserCases))]
        public void TryDeserializeTest(byte[] data, MockDeserializableSigScript scr, byte[] expHash, uint expIndex, uint expSeq)
        {
            TxIn tx = new TxIn()
            {
                SigScript = scr
            };
            FastStreamReader stream = new FastStreamReader(data);
            bool b = tx.TryDeserialize(stream, out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(expHash, tx.TxHash);
            Assert.Equal(expIndex, tx.Index);
            Assert.Equal(expSeq, tx.Sequence);
            // Mock script has its own tests.
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[] { null, null, "Stream can not be null." };
            yield return new object[] { new FastStreamReader(new byte[31]), null, Err.EndOfStream };
            yield return new object[] { new FastStreamReader(new byte[35]), null, Err.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[37]),
                new MockDeserializableSigScript(36, 0, "Foo"),
                "Foo"
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[40]),
                new MockDeserializableSigScript(36, 1),
                Err.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTest(FastStreamReader stream, MockDeserializableSigScript scr, string expErr)
        {
            TxIn tx = new TxIn()
            {
                SigScript = scr
            };
            bool b = tx.TryDeserialize(stream, out string error);

            Assert.False(b);
            Assert.Equal(expErr, error);
        }
    }
}
