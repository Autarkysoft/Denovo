﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Bitcoin.Blockchain.Scripts.Operations;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts
{
    public class ScriptTests : Script
    {
        [Fact]
        public void DataPropertyTest()
        {
            // Making sure data is never null
            Data = null;
            Assert.Equal(Array.Empty<byte>(), Data);
        }

        [Fact]
        public void SetDataTest()
        {
            IOperation[] ops = new IOperation[]
            {
                new MockSerializableOp(new byte[] { 1, 2, 3 }),
                new MockSerializableOp(new byte[] { 10, 20 })
            };
            SetData(ops);
            byte[] expected = { 1, 2, 3, 10, 20 };

            Assert.Equal(expected, Data);
        }


        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(252, 253)]
        [InlineData(253, 256)]
        public void AddSerializedSizeTest(int dataLen, int expectedSize)
        {
            Data = new byte[dataLen];
            SizeCounter counter = new();
            AddSerializedSize(counter);
            Assert.Equal(expectedSize, counter.Size);
        }

        [Fact]
        public void SerializeTest()
        {
            Data = new byte[] { 1, 2, 3 };
            FastStream stream = new(4);
            Serialize(stream);
            Assert.Equal(new byte[] { 3, 1, 2, 3 }, stream.ToByteArray());
        }

        [Theory]
        [InlineData(new byte[] { 0 }, new byte[] { })]
        [InlineData(new byte[] { 1, 10 }, new byte[] { 10 })]
        [InlineData(new byte[] { 3, 10, 20, 30 }, new byte[] { 10, 20, 30 })]
        public void TryDeserializeTest(byte[] ba, byte[] expected)
        {
            FastStreamReader stream = new(ba);
            bool b = TryDeserialize(stream, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expected, Data);
        }

        [Fact]
        public void TryDeserialize_FailTest()
        {
            FastStreamReader stream = new(new byte[] { 1 });
            bool b = TryDeserialize(stream, out Errors error);

            Assert.False(b);
            Assert.Equal(Errors.EndOfStream, error);
        }


        public static IEnumerable<object[]> GetSigOpCountCases()
        {
            yield return new object[] { Array.Empty<byte>(), 0 };
            yield return new object[] { new byte[] { (byte)OP.CheckSig }, 1 };
            yield return new object[] { new byte[] { (byte)OP.CheckSigVerify }, 1 };

            yield return new object[] { new byte[] { (byte)OP.CheckMultiSig }, 20 };
            yield return new object[] { new byte[] { (byte)OP.CheckMultiSigVerify }, 20 };
            yield return new object[] { new byte[] { (byte)OP._0, (byte)OP.CheckMultiSig }, 20 };
            yield return new object[] { new byte[] { (byte)OP._0, (byte)OP.CheckMultiSigVerify }, 20 };
            yield return new object[] { new byte[] { (byte)OP.Negative1, (byte)OP.CheckMultiSig }, 20 };
            yield return new object[] { new byte[] { (byte)OP.Negative1, (byte)OP.CheckMultiSigVerify }, 20 };
            // CheckMultiSig(Verify) counts as 20 in this base method (it is overriden in RedeemScript)
            yield return new object[] { new byte[] { (byte)OP._2, (byte)OP.CheckMultiSig }, 20 };
            yield return new object[] { new byte[] { (byte)OP._2, (byte)OP.CheckMultiSigVerify }, 20 };
            // SigOpCount don't fail on disabled or invalid OP codes!
            yield return new object[] { new byte[] { (byte)OP._0, (byte)OP._1, (byte)OP.DIV, (byte)OP.CheckMultiSig }, 20 };
            yield return new object[] { new byte[] { (byte)OP.IF, (byte)OP.CheckSig }, 1 };
            yield return new object[] { new byte[] { (byte)OP.EndIf, (byte)OP.CheckSig }, 1 };
            // Result should be addition of all Sig Ops
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP.DUP, (byte)OP.CheckSig, (byte)OP.CheckSequenceVerify,
                    (byte)OP._12, (byte)OP.CheckMultiSig, (byte)OP.EQUAL, (byte)OP.CheckSigVerify
                },
                22
            };
            // Push data Ops should move the index forward correctly and are the only time counting fails/stops
            yield return new object[] { new byte[] { 0, (byte)OP.CheckSig }, 1 }; // Doesn't cover SigOp
            yield return new object[] { new byte[] { 1, (byte)OP.CheckSig }, 0 }; // SigOp is the single byte to be pushed
            yield return new object[] { new byte[] { 1, 1, (byte)OP.CheckSig }, 1 }; // Doesn't cover SigOp
            yield return new object[] { new byte[] { 2, (byte)OP.CheckSig }, 0 }; // Not enough bytes
            yield return new object[] { new byte[] { 1, (byte)OP.CheckSig, (byte)OP.CheckSig, (byte)OP.CheckSig }, 2 };
            yield return new object[] { new byte[] { 2, (byte)OP.CheckSig, (byte)OP.CheckSig, (byte)OP.CheckSig }, 1 };
            yield return new object[] { new byte[] { 3, (byte)OP.CheckSig, (byte)OP.CheckSig, (byte)OP.CheckSig }, 0 };

            // OP_PushData1
            yield return new object[] { new byte[] { 0x4c }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4c, 1 }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4c, 1, (byte)OP.CheckSig, (byte)OP.CheckSig }, 1 }; // Not strict about encoding
            yield return new object[] { new byte[] { 0x4c, 2, (byte)OP.CheckSig, (byte)OP.CheckSig }, 0 };
            // Same tests for OP_PushData1 but there are other Ops before Push
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4c }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4c, 1 }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4c, 1, (byte)OP.CheckSig, (byte)OP.CheckSig }, 1 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4c, 2, (byte)OP.CheckSig, (byte)OP.CheckSig }, 0 };

            // OP_PushData2
            yield return new object[] { new byte[] { 0x4d }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4d, 1 }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4d, 5, 0 }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4d, 1, 0, (byte)OP.CheckSig, (byte)OP.CheckSig }, 1 };
            yield return new object[] { new byte[] { 0x4d, 2, 0, (byte)OP.CheckSig, (byte)OP.CheckSig }, 0 };
            // Same tests for OP_PushData2 but there are other Ops before Push
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4d }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4d, 1 }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4d, 5, 0 }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4d, 1, 0, (byte)OP.CheckSig, (byte)OP.CheckSig }, 1 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4d, 2, 0, (byte)OP.CheckSig, (byte)OP.CheckSig }, 0 };

            // OP_PushData4
            yield return new object[] { new byte[] { 0x4e }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4e, 1 }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4e, 1, 2 }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4e, 1, 2, 3 }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4e, 1, 0, 0, 0 }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4e, 1, 0, 0, (byte)OP.CheckSig }, 0 }; // Fail
            yield return new object[] { new byte[] { 0x4e, 1, 0, 0, 0, (byte)OP.CheckSig }, 0 };
            yield return new object[] { new byte[] { 0x4e, 1, 0, 0, 0, (byte)OP.CheckSig, (byte)OP.CheckSig }, 1 };
            // Same tests for OP_PushData4 but there are other Ops before Push
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4e }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4e, 1 }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4e, 1, 2 }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4e, 1, 2, 3 }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4e, 1, 0, 0, 0 }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4e, 1, 0, 0, (byte)OP.CheckSig }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4e, 1, 0, 0, 0, (byte)OP.CheckSig }, 0 };
            yield return new object[] { new byte[] { (byte)OP.ADD, (byte)OP.ADD, 0x4e, 1, 0, 0, 0, (byte)OP.CheckSig, (byte)OP.CheckSig }, 1 };
        }
        [Theory]
        [MemberData(nameof(GetSigOpCountCases))]
        public void CountSigOpsTest(byte[] data, int expected)
        {
            Data = data;
            int actual = CountSigOps();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(0x4b, true)]
        [InlineData(0x4c, true)] // PushData1
        [InlineData(0x4d, true)] // PushData2
        [InlineData(0x4e, true)] // PushData4
        [InlineData(0x4f, true)] // -1
        [InlineData(0x50, false)] // Reserved
        [InlineData(0x51, true)] // OP_1
        [InlineData(0x52, true)]
        [InlineData(0x60, true)] // OP_16
        [InlineData(0x61, false)]
        public void IsPushOpTest(byte b, bool expected)
        {
            bool actual = IsPushOp(b);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(80)]
        [InlineData(98)]
        [InlineData(126)]
        [InlineData(127)]
        [InlineData(128)]
        [InlineData(129)]
        [InlineData(131)]
        [InlineData(132)]
        [InlineData(133)]
        [InlineData(134)]
        [InlineData(137)]
        [InlineData(138)]
        [InlineData(141)]
        [InlineData(142)]
        [InlineData(149)]
        [InlineData(150)]
        [InlineData(151)]
        [InlineData(152)]
        [InlineData(153)]
        public void IsIsOpSuccess_TrueTest(byte b)
        {
            Assert.True(IsOpSuccess(b));
        }

        [Theory]
        [InlineData(187, 255)]
        public void IsIsOpSuccess_TrueRangeTest(byte start, byte end)
        {
            for (byte b = start; b < end; b++)
            {
                Assert.True(IsOpSuccess(b));
            }
        }

        [Theory]
        [InlineData(130)]
        [InlineData(135)]
        [InlineData(136)]
        [InlineData(139)]
        [InlineData(140)]
        [InlineData(143)]
        [InlineData(144)]
        [InlineData(145)]
        [InlineData(146)]
        [InlineData(147)]
        [InlineData(148)]
        [InlineData(255)]
        public void IsIsOpSuccess_FalseTest(byte b)
        {
            Assert.False(IsOpSuccess(b));
        }

        [Theory]
        [InlineData(0, 80)]
        [InlineData(81, 98)]
        [InlineData(99, 126)]
        [InlineData(154, 187)]
        public void IsIsOpSuccess_FalseRangeTest(byte start, byte end)
        {
            for (byte b = start; b < end; b++)
            {
                Assert.False(IsOpSuccess(b));
            }
        }


        public static IEnumerable<object[]> GetEvalOpSuccessCases()
        {
            byte ops1 = 80;
            byte ops2 = 98;
            byte ops3 = 126;

            yield return new object[] { Array.Empty<byte>(), true, false };
            yield return new object[] { new byte[1] { 0 }, true, false };
            yield return new object[] { new byte[2] { 0, ops1 }, true, true };
            yield return new object[] { new byte[1] { 1 }, false, false };
            yield return new object[] { new byte[2] { 1, 1 }, true, false };
            yield return new object[] { new byte[4] { 2, 1, 2, ops1 }, true, true };
            yield return new object[] { new byte[4] { 2, 1, ops1, 0 }, true, false }; // OP_SUCCESS is inside PushData
            yield return new object[] { new byte[1] { (byte)OP.PushData1 }, false, false };
            yield return new object[] { new byte[2] { (byte)OP.PushData1, 1 }, false, false };
            yield return new object[] { new byte[2] { (byte)OP.PushData1, 0 }, true, false }; // PushData is not strict about len
            yield return new object[] { new byte[3] { (byte)OP.PushData1, 0, ops1 }, true, true };
            yield return new object[] { new byte[3] { (byte)OP.PushData1, 1, 1 }, true, false };
            yield return new object[] { new byte[4] { (byte)OP.PushData1, 1, 1, ops1 }, true, true };
            yield return new object[] { new byte[4] { (byte)OP.PushData1, 2, 1, ops1 }, true, false };
            yield return new object[] { new byte[2] { (byte)OP.PushData2, 0 }, false, false };
            yield return new object[] { new byte[2] { (byte)OP.PushData2, 1 }, false, false };
            yield return new object[] { new byte[3] { (byte)OP.PushData2, 1, 0 }, false, false };
            yield return new object[] { new byte[3] { (byte)OP.PushData2, 0, 0 }, true, false };
            yield return new object[] { new byte[4] { (byte)OP.PushData2, 0, 0, ops1 }, true, true };
            yield return new object[] { new byte[4] { (byte)OP.PushData2, 1, 0, 1 }, true, false };
            yield return new object[] { new byte[4] { (byte)OP.PushData2, 1, 0, ops1 }, true, false };
            yield return new object[] { new byte[5] { (byte)OP.PushData2, 1, 0, 1, ops1 }, true, true };
            yield return new object[] { new byte[1] { (byte)OP.PushData4 }, false, false };
            yield return new object[] { new byte[2] { (byte)OP.PushData4, 1 }, false, false };
            yield return new object[] { new byte[5] { (byte)OP.PushData4, 1, 0, 0, 0 }, false, false };
            yield return new object[] { new byte[5] { (byte)OP.PushData4, 0, 0, 0, 0 }, true, false };
            yield return new object[] { new byte[6] { (byte)OP.PushData4, 0, 0, 0, 0, ops1 }, true, true };
            yield return new object[] { new byte[6] { (byte)OP.PushData4, 1, 0, 0, 0, 1 }, true, false };
            yield return new object[] { new byte[6] { (byte)OP.PushData4, 1, 0, 0, 0, ops1 }, true, false };
            yield return new object[] { new byte[7] { (byte)OP.PushData4, 1, 0, 0, 0, ops1, ops1 }, true, true };
            yield return new object[] { new byte[7] { (byte)OP.PushData4, 255, 255, 255, 255, ops1, ops1 }, false, false };
            yield return new object[] { new byte[2] { ops1, 1 }, true, true }; // Note how this script is broken
            yield return new object[] { new byte[2] { (byte)OP.DUP, 0 }, true, false };
            yield return new object[] { new byte[2] { (byte)OP.DUP, ops2 }, true, true };
            yield return new object[] { new byte[2] { (byte)OP.IF, ops3 }, true, true }; // Another broken script
            yield return new object[] { new byte[4] { (byte)OP.IF, 0, ops3, 3 }, true, true }; // Another broken script
        }
        [Theory]
        [MemberData(nameof(GetEvalOpSuccessCases))]
        public void TryEvaluateOpSuccessTest(byte[] data, bool expectedEvaluate, bool expectedHasOpSuccess)
        {
            Data = data;
            bool actualEvaluate = TryEvaluateOpSuccess(out bool actualHasOpSuccess);
            Assert.Equal(expectedEvaluate, actualEvaluate);
            Assert.Equal(expectedHasOpSuccess, actualHasOpSuccess);
        }

        public static IEnumerable<object[]> GetEvalCases()
        {
            yield return new object[] { null, Array.Empty<IOperation>(), 0 };
            yield return new object[] { new byte[1], new IOperation[] { new PushDataOp(OP._0) }, 0 };
            yield return new object[]
            {
                new byte[] { 0, 0x51, 0x60, 2, 10, 20 },
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(OP._1), new PushDataOp(OP._16), new PushDataOp(new byte[] { 10, 20 })
                },
                0
            };
            yield return new object[] { new byte[] { (byte)OP.Reserved }, new IOperation[] { new ReservedOp() }, 0 };
            yield return new object[] { new byte[] { (byte)OP.RETURN }, new IOperation[] { new ReturnOp() }, 1 };
            yield return new object[]
            {
                new byte[] { (byte)OP.RETURN, 1, 2, 3 }, new IOperation[] { new ReturnOp(new byte[] { 1, 2, 3 }, false) }, 1
            };
            yield return new object[]
            {
                new byte[] { (byte)OP.NOP, (byte)OP.RETURN, (byte)OP.NOP },
                new IOperation[] { new NOPOp(), new ReturnOp(new byte[] { (byte)OP.NOP }, false) },
                2
            };

            yield return new object[]
            {
                new byte[30]
                {
                    (byte)OP.VER, (byte)OP.VERIFY, (byte)OP.ToAltStack, (byte)OP.FromAltStack, (byte)OP.DROP2,
                    (byte)OP.DUP2, (byte)OP.DUP3, (byte)OP.OVER2, (byte)OP.ROT2, (byte)OP.SWAP2,
                    (byte)OP.IfDup, (byte)OP.DEPTH, (byte)OP.DROP, (byte)OP.DUP, (byte)OP.NIP,
                    (byte)OP.OVER, (byte)OP.PICK, (byte)OP.ROLL, (byte)OP.ROT, (byte)OP.SWAP,
                    (byte)OP.TUCK, (byte)OP.SIZE, (byte)OP.EQUAL, (byte)OP.EqualVerify, (byte)OP.Reserved1,
                    (byte)OP.Reserved2, (byte)OP.ADD1, (byte)OP.SUB1, (byte)OP.NEGATE, (byte)OP.ABS
                },
                new IOperation[30]
                {
                    new VEROp(), new VerifyOp(), new ToAltStackOp(), new FromAltStackOp(), new DROP2Op(),
                    new DUP2Op(), new DUP3Op(), new OVER2Op(), new ROT2Op(), new SWAP2Op(),
                    new IfDupOp(), new DEPTHOp(), new DROPOp(), new DUPOp(), new NIPOp(),
                    new OVEROp(), new PICKOp(), new ROLLOp(), new ROTOp(), new SWAPOp(),
                    new TUCKOp(), new SizeOp(), new EqualOp(), new EqualVerifyOp(), new Reserved1Op(),
                    new Reserved2Op(), new ADD1Op(), new SUB1Op(), new NEGATEOp(), new ABSOp()
                },
                30
            };
            yield return new object[]
            {
                new byte[30]
                {
                    (byte)OP.NOT, (byte)OP.NotEqual0, (byte)OP.ADD, (byte)OP.SUB, (byte)OP.BoolAnd,
                    (byte)OP.BoolOr, (byte)OP.NumEqual, (byte)OP.NumEqualVerify, (byte)OP.NumNotEqual, (byte)OP.LessThan,
                    (byte)OP.GreaterThan, (byte)OP.LessThanOrEqual, (byte)OP.GreaterThanOrEqual, (byte)OP.MIN, (byte)OP.MAX,
                    (byte)OP.WITHIN, (byte)OP.RIPEMD160, (byte)OP.SHA1, (byte)OP.SHA256, (byte)OP.HASH160,
                    (byte)OP.HASH256, (byte)OP.CodeSeparator, (byte)OP.CheckSig, (byte)OP.CheckSigVerify, (byte)OP.CheckMultiSig,
                    (byte)OP.CheckMultiSigVerify, (byte)OP.NOP1, (byte)OP.CheckLocktimeVerify, (byte)OP.CheckSequenceVerify, (byte)OP.NOP4
                },
                new IOperation[30]
                {
                    new NOTOp(), new NotEqual0Op(), new AddOp(), new SUBOp(), new BoolAndOp(),
                    new BoolOrOp(), new NumEqualOp(), new NumEqualVerifyOp(), new NumNotEqualOp(), new LessThanOp(),
                    new GreaterThanOp(), new LessThanOrEqualOp(), new GreaterThanOrEqualOp(), new MINOp(), new MAXOp(),
                    new WITHINOp(), new RipeMd160Op(), new Sha1Op(), new Sha256Op(), new Hash160Op(),
                    new Hash256Op(), new CodeSeparatorOp(), new CheckSigOp(), new CheckSigVerifyOp(), new CheckMultiSigOp(),
                    new CheckMultiSigVerifyOp(), new NOP1Op(), new CheckLocktimeVerifyOp(), new CheckSequenceVerifyOp(), new NOP4Op()
                },
                30
            };
            yield return new object[]
            {
                new byte[6]
                {
                    (byte)OP.NOP5, (byte)OP.NOP6, (byte)OP.NOP7, (byte)OP.NOP8, (byte)OP.NOP9,
                    (byte)OP.NOP10
                },
                new IOperation[6]
                {
                    new NOP5Op(), new NOP6Op(), new NOP7Op(), new NOP8Op(), new NOP9Op(),
                    new NOP10Op()
                },
                6
            };

            // Conditionals (OP_IF)
            yield return new object[] { new byte[] { (byte)OP.IF, (byte)OP.EndIf }, new IOperation[] { new IFOp(null, null) }, 2 };
            yield return new object[]
            {
                new byte[] { (byte)OP.IF, (byte)OP.ELSE, (byte)OP.EndIf },
                new IOperation[] { new IFOp(null, Array.Empty<IOperation>()) },
                3
            };
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP.DUP2, (byte)OP.IF, (byte)OP.ADD, 3, 10, 20, 30, (byte)OP.SUB, (byte)OP.EndIf, (byte)OP.DUP
                },
                new IOperation[]
                {
                    new DUP2Op(),
                    new IFOp(new IOperation[] { new AddOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new SUBOp() }, null),
                    new DUPOp()
                },
                6
            };
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP._1, (byte)OP.IF, (byte)OP.ADD, (byte)OP.ELSE, 2, 10, 20, (byte)OP.DUP2, (byte)OP.EndIf, (byte)OP.DUP
                },
                new IOperation[]
                {
                    new PushDataOp(OP._1),
                    new IFOp(new IOperation[] { new AddOp() },
                             new IOperation[] { new PushDataOp(new byte[] { 10, 20 }), new DUP2Op() }),
                    new DUPOp()
                },
                6 // OP_1 is not counted
            };
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP._2,
                    (byte)OP.IF,
                      (byte)OP.IF,
                        (byte)OP.IF,
                        (byte)OP.ELSE,
                        (byte)OP.EndIf,
                      (byte)OP.ELSE,
                      (byte)OP.EndIf,
                    (byte)OP.EndIf
                },
                new IOperation[]
                {
                    new PushDataOp(OP._2),
                    new IFOp(new IOperation[] { new IFOp(new IOperation[] { new IFOp(null, Array.Empty<IOperation>()) }, Array.Empty<IOperation>()) }, null)
                },
                8
            };

            // Conditionals (OP_NotIf) same as above but If is change to NotIf
            yield return new object[]
            {
                new byte[] { (byte)OP.NotIf, (byte)OP.EndIf },
                new IOperation[] { new NotIfOp(null, null) },
                2
            };
            yield return new object[]
            {
                new byte[] { (byte)OP.NotIf, (byte)OP.ELSE, (byte)OP.EndIf },
                new IOperation[] { new NotIfOp(null, Array.Empty<IOperation>()) },
                3
            };
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP.DUP2, (byte)OP.NotIf, (byte)OP.ADD, 3, 10, 20, 30, (byte)OP.SUB, (byte)OP.EndIf, (byte)OP.DUP
                },
                new IOperation[]
                {
                    new DUP2Op(),
                    new NotIfOp(new IOperation[] { new AddOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new SUBOp() }, null),
                    new DUPOp()
                },
                6
            };
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP._1, (byte)OP.NotIf, (byte)OP.ADD, (byte)OP.ELSE, 2, 10, 20, (byte)OP.DUP2, (byte)OP.EndIf, (byte)OP.DUP
                },
                new IOperation[]
                {
                    new PushDataOp(OP._1),
                    new NotIfOp(new IOperation[] { new AddOp() },
                                new IOperation[] { new PushDataOp(new byte[] { 10, 20 }), new DUP2Op() }),
                    new DUPOp()
                },
                6 // OP_1 is not counted
            };
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP._2,
                    (byte)OP.NotIf,
                      (byte)OP.NotIf,
                        (byte)OP.NotIf,
                        (byte)OP.ELSE,
                        (byte)OP.EndIf,
                      (byte)OP.ELSE,
                      (byte)OP.EndIf,
                    (byte)OP.EndIf
                },
                new IOperation[]
                {
                    new PushDataOp(OP._2),
                    new NotIfOp(
                                new IOperation[] { new NotIfOp(new IOperation[] { new NotIfOp(null, Array.Empty<IOperation>()) },
                                                               Array.Empty<IOperation>()) },
                                null)
                },
                8
            };

            // Actual nested if case from:
            // https://github.com/lightningnetwork/lightning-rfc/blob/master/03-transactions.md#offered-htlc-outputs
            byte[] b20 = Enumerable.Range(1, 20).Select(i => (byte)i).ToArray();
            byte[] b33 = Enumerable.Range(1, 33).Select(i => (byte)i).ToArray();
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP.DUP, (byte)OP.HASH160,
                    20, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, // Random 20 bytes
                    (byte)OP.EQUAL,
                    (byte)OP.IF,
                       (byte)OP.CheckSig,
                    (byte)OP.ELSE,
                       33, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                       21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, // Random 33 bytes (pubkey)
                       (byte)OP.SWAP, (byte)OP.SIZE, 1, 32, (byte)OP.EQUAL,
                       (byte)OP.NotIf,
                          (byte)OP.DROP, (byte)OP._2, (byte)OP.SWAP,
                          33, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                          21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, // Random 33 bytes (pubkey)
                          (byte)OP._2, (byte)OP.CheckMultiSig,
                       (byte)OP.ELSE,
                          (byte)OP.HASH160,
                          20, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, // Random 20 bytes
                          (byte)OP.EqualVerify, (byte)OP.CheckSig,
                       (byte)OP.EndIf,
                    (byte)OP.EndIf
                },
                new IOperation[]
                {
                    new DUPOp(), new Hash160Op(), new PushDataOp(b20), new EqualOp(),
                    new IFOp(
                             new IOperation[]{ new CheckSigOp() },
                             new IOperation[]
                             {
                                 new PushDataOp(b33), new SWAPOp(), new SizeOp(), new PushDataOp(32), new EqualOp(),
                                 new NotIfOp(
                                             new IOperation[]
                                             {
                                                 new DROPOp(), new PushDataOp(OP._2), new SWAPOp(),
                                                 new PushDataOp(b33), new PushDataOp(OP._2), new CheckMultiSigOp()
                                             },
                                             new IOperation[]
                                             {
                                                 new Hash160Op(), new PushDataOp(b20), new EqualVerifyOp(), new CheckSigOp()
                                             }
                                            )
                             })
                },
                19
            };
        }
        [Theory]
        [MemberData(nameof(GetEvalCases))]
        public void TryEvaluateTest(byte[] scrBa, IOperation[] expectedOps, int expectedCount)
        {
            Data = scrBa;
            bool b1 = TryEvaluate(ScriptEvalMode.Legacy, out IOperation[] actOpsLegacy, out int actCountLegacy, out Errors e1);
            bool b2 = TryEvaluate(ScriptEvalMode.WitnessV0, out IOperation[] actOpsWit0, out int actCountWit0, out Errors e2);

            Assert.True(b1, e1.Convert());
            Assert.True(b2, e2.Convert());

            Assert.Equal(Errors.None, e1);
            Assert.Equal(Errors.None, e2);

            Assert.Equal(expectedOps, actOpsLegacy);
            Assert.Equal(expectedOps, actOpsWit0);

            Assert.Equal(expectedCount, actCountLegacy);
            Assert.Equal(expectedCount, actCountWit0);
        }

        public static IEnumerable<object[]> GetEvalWitVer1Cases()
        {
            yield return new object[] { null, Array.Empty<IOperation>() };
            yield return new object[] { new byte[1], new IOperation[] { new PushDataOp(OP._0) } };
            yield return new object[]
            {
                new byte[] { 0, 0x51, 0x60, 2, 10, 20 },
                new IOperation[]
                {
                    new PushDataOp(OP._0), new PushDataOp(OP._1), new PushDataOp(OP._16), new PushDataOp(new byte[] { 10, 20 })
                }
            };
            yield return new object[] { new byte[] { 80 }, new IOperation[] { new SuccessOp(80) } };
            yield return new object[]
            {
                new byte[] { 2, 10, 20, 98 },
                new IOperation[] { new PushDataOp(new byte[] { 10, 20 }), new SuccessOp(98) }
            };
            yield return new object[]
            {
                new byte[] { 2, 10, 20, 126, 0x51 }, // The last byte (0x51=OP_1) is ignored as we reach OP_SUCCESS
                new IOperation[] { new PushDataOp(new byte[] { 10, 20 }), new SuccessOp(126) }
            };
        }
        [Theory]
        [MemberData(nameof(GetEvalWitVer1Cases))]
        public void TryEvaluate_WitVer1Test(byte[] scrBa, IOperation[] expectedOps)
        {
            Data = scrBa;
            bool b = TryEvaluate(ScriptEvalMode.WitnessV1, out IOperation[] actOps, out int actualCount, out Errors err);

            Assert.True(b, err.Convert());
            Assert.Equal(Errors.None, err);
            Assert.Equal(expectedOps, actOps);
            Assert.Equal(0, actualCount);
        }

        public static IEnumerable<object[]> GetEvalFailCases()
        {
            yield return new object[] { new byte[] { 2, 10 }, Errors.EndOfStream };
            yield return new object[] { new byte[] { (byte)OP.VerIf }, Errors.InvalidOP };
            yield return new object[] { new byte[] { (byte)OP.VerNotIf }, Errors.InvalidOP };
            yield return new object[] { new byte[] { (byte)OP.CAT }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.SubStr }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.LEFT }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.RIGHT }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.INVERT }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.AND }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.OR }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.XOR }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.MUL2 }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.DIV2 }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.MUL }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.DIV }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.MOD }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.LSHIFT }, Errors.DisabledOP };
            yield return new object[] { new byte[] { (byte)OP.RSHIFT }, Errors.DisabledOP };
            yield return new object[] { new byte[] { 255 }, Errors.UndefinedOp };
            yield return new object[] { new byte[] { (byte)OP.CheckSigAdd }, Errors.OpCheckSigAddPreTaproot };
        }
        [Theory]
        [MemberData(nameof(GetEvalFailCases))]
        public void TryEvaluate_FailTest(byte[] scrBa, Errors expErr)
        {
            Data = scrBa;
            bool b1 = TryEvaluate(ScriptEvalMode.Legacy, out _, out _, out Errors e1);
            bool b2 = TryEvaluate(ScriptEvalMode.WitnessV0, out _, out _, out Errors e2);

            Assert.False(b1);
            Assert.False(b2);

            Assert.Equal(expErr, e1);
            Assert.Equal(expErr, e2);
        }

        [Fact]
        public void TryEvaluate_DataOverflowTest()
        {
            Data = new byte[Constants.MaxScriptLength + 1];

            bool b1 = TryEvaluate(ScriptEvalMode.Legacy, out _, out _, out Errors e1);
            bool b2 = TryEvaluate(ScriptEvalMode.WitnessV0, out _, out _, out Errors e2);
            bool b3 = TryEvaluate(ScriptEvalMode.WitnessV1, out _, out _, out Errors e3);

            Assert.False(b1);
            Assert.False(b2);
            Assert.True(b3);

            Assert.Equal(Errors.ScriptOverflow, e1);
            Assert.Equal(Errors.ScriptOverflow, e2);
            Assert.Equal(Errors.None, e3);
        }

        [Fact]
        public void TryRead_OpOverflowTest()
        {
            int count = Constants.MaxScriptOpCount;
            uint pos = 0;
            FastStreamReader stream = new(new byte[] { (byte)OP.DUP });
            List<IOperation> list = new();

            bool b1 = TryRead(ScriptEvalMode.Legacy, stream, list, ref count, ref pos, out Errors e1);
            bool b2 = TryRead(ScriptEvalMode.WitnessV0, stream, list, ref count, ref pos, out Errors e2);

            Assert.False(b1);
            Assert.False(b2);

            Assert.Equal(Errors.OpCountOverflow, e1);
            Assert.Equal(Errors.OpCountOverflow, e2);
        }

        public static IEnumerable<object[]> GetPositionCases()
        {
            yield return new object[] { new byte[] { 0 }, 1 };
            yield return new object[] { new byte[] { 1, 1 }, 1 };
            yield return new object[] { new byte[] { 4, 1, 2, 3, 4 }, 1 };
            yield return new object[] { new byte[] { 4, 1, 2, 3, 4, (byte)OP.ABS }, 2 };
            yield return new object[] { new byte[] { 4, 1, 2, 3, 4, (byte)OP.ABS, (byte)OP.ABS }, 3 };
            yield return new object[] { new byte[] { (byte)OP.CheckSig, (byte)OP.CodeSeparator, (byte)OP.DUP, (byte)OP._16 }, 4 };
            yield return new object[]
            {
                new byte[]
                {
                    (byte)OP.DUP, (byte)OP.IF, (byte)OP.DUP, (byte)OP.IF, (byte)OP.DUP, (byte)OP.ELSE, (byte)OP.DUP,
                    (byte)OP.EndIf, (byte)OP.EndIf, 2, 10, 20
                },
                10
            };
        }
        [Theory]
        [MemberData(nameof(GetPositionCases))]
        public void TryRead_OpPositionTest(byte[] scrBa, uint expectedPos)
        {
            int count = 0;
            uint actualPos = 0;
            FastStreamReader stream = new(scrBa);
            List<IOperation> list = new();

            while (stream.GetRemainingBytesCount() > 0)
            {
                bool b = TryRead(ScriptEvalMode.WitnessV1, stream, list, ref count, ref actualPos, out Errors err);
                Assert.True(b, err.Convert());
                Assert.Equal(Errors.None, err);
            }

            Assert.Equal(expectedPos, actualPos);
        }
    }
}
