﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts
{
    public class ScriptSerializerTests
    {
        public static IEnumerable<object[]> GetSingleSigCases()
        {
            yield return new object[]
            {
                Array.Empty<IOperation>(),
                Helper.HexToBytes("0102"),
                Array.Empty<byte>()
            };
            yield return new object[]
            {
                new IOperation[] { new ROLLOp(), new Hash256Op() },
                Helper.HexToBytes("0102"),
                new byte[] { (byte)OP.ROLL, (byte)OP.HASH256 }
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(new byte[] { 10, 20, 30, 40 }) },
                new byte[] { 10, 20, 30, 40 },
                Array.Empty<byte>()
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(new byte[] { 10, 20, 30, 40 }) },
                new byte[] { 20, 30, 40 },
                new byte[] { 4, 10, 20, 30, 40 }
            };
            yield return new object[]
            {
                new IOperation[] { new ROLLOp(), new NOP10Op(), new PushDataOp(new byte[] { 10, 20, 30, 40 }), new CheckSigOp() },
                new byte[] { 10, 20, 30, 40 },
                new byte[] { (byte)OP.ROLL, (byte)OP.NOP10, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[] { new CheckSigOp() },
                new byte[] { 1, 2 },
                new byte[] { (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new PushDataOp(Helper.ShortSig1Bytes), new CheckSigOp(), new PushDataOp(Helper.ShortSig1Bytes)
                },
                Helper.ShortSig1Bytes,
                new byte[] { (byte)OP.CheckSig, }
            };
            yield return new object[]
            {
                new IOperation[] { new DUP2Op(), new CheckSigOp(), new ROTOp() },
                Helper.ShortSig1Bytes,
                new byte[] { (byte)OP.DUP2, (byte)OP.CheckSig, (byte)OP.ROT }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new DUPOp(),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new PushDataOp(Helper.GetBytes(10)),
                    new CheckSigOp()
                },
                Helper.ShortSig1Bytes,
                Helper.ConcatBytes(13, new byte[] { (byte)OP.DUP, 10 }, Helper.GetBytes(10), new byte[] { (byte)OP.CheckSig })
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new DUPOp(),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new PushDataOp(Helper.GetBytes(10)),
                    new CheckSigOp()
                },
                Helper.ShortSig1Bytes,
                Helper.ConcatBytes(12, new byte[] { 10 }, Helper.GetBytes(10), new byte[] { (byte)OP.CheckSig })
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                Helper.ShortSig1Bytes,
                new byte[] { (byte)OP.IF, (byte)OP.ELSE, (byte)OP.EndIf, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                Helper.ShortSig1Bytes,
                new byte[] { (byte)OP.ELSE, (byte)OP.EndIf, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                Helper.ShortSig1Bytes,
                new byte[] { (byte)OP.EndIf, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } }),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new CheckSigOp()
                },
                Helper.ShortSig1Bytes,
                new byte[] { (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new PushDataOp(Helper.ShortSig1Bytes) }, null),
                    new CheckSigOp()
                },
                Helper.ShortSig1Bytes,
                new byte[] { (byte)OP.IF, (byte)OP.EndIf, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                Helper.ShortSig1Bytes,
                new byte[]
                {
                    (byte)OP.IF, (byte)OP.ROLL, (byte)OP.ADD, (byte)OP.ELSE, (byte)OP.SUB1, (byte)OP.IfDup, (byte)OP.EndIf,
                    (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL
                }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = true }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                Helper.ShortSig1Bytes,
                new byte[]
                {
                    (byte)OP.ADD, (byte)OP.ELSE, (byte)OP.SUB1, (byte)OP.IfDup, (byte)OP.EndIf,
                    (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL
                }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = true }, new IfDupOp() }),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                Helper.ShortSig1Bytes,
                new byte[]
                {
                    (byte)OP.IfDup, (byte)OP.EndIf,
                    (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL
                }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }),
                    new Hash160Op(),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                Helper.ShortSig1Bytes,
                new byte[] { (byte)OP.CheckMultiSig, (byte)OP.EQUAL }
            };
        }
        [Theory]
        [MemberData(nameof(GetSingleSigCases))]
        public void Convert_SingleSigTest(IOperation[] ops, byte[] sig, byte[] expected)
        {
            ScriptSerializer ser = new();
            byte[] actual = ser.Convert(ops, sig);
            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetMultiSigCases()
        {
            yield return new object[]
            {
                Array.Empty<IOperation>(),
                new byte[][] { Helper.HexToBytes("0102") },
                Array.Empty<byte>()
            };
            yield return new object[]
            {
                new IOperation[] { new Sha1Op() },
                new byte[][] { Helper.HexToBytes("0102") },
                new byte[] { (byte)OP.SHA1 }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new RipeMd160Op(),
                    new PushDataOp(new byte[] { 1, 2 }),
                    new DUPOp(),
                    new PushDataOp(new byte[] { 1, 2 }),
                    new PushDataOp(new byte[] { 1, 2, 3 }),
                },
                new byte[][] { Helper.HexToBytes("0102") },
                new byte[] { (byte)OP.RIPEMD160, (byte)OP.DUP, 3, 1, 2, 3 }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new RipeMd160Op(),
                    new PushDataOp(new byte[] { 1, 2 }),
                    new DUPOp(),
                    new PushDataOp(new byte[] { 1, 2 }),
                    new PushDataOp(new byte[] { 1, 2, 3 }),
                },
                new byte[][] { Helper.HexToBytes("0102"), Helper.HexToBytes("010203") },
                new byte[] { (byte)OP.RIPEMD160, (byte)OP.DUP }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new PushDataOp(Helper.ShortSig1Bytes) }, null),
                    new CheckSigOp()
                },
                new byte[][] { Helper.ShortSig1Bytes },
                new byte[] { (byte)OP.IF, (byte)OP.EndIf, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                new byte[] { (byte)OP.IF, (byte)OP.ELSE, (byte)OP.EndIf, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                new byte[] { (byte)OP.ELSE, (byte)OP.EndIf, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                new byte[] { (byte)OP.EndIf, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } }),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new CheckSigOp()
                },
                new byte[][] { Helper.ShortSig1Bytes, Helper.ShortSig2Bytes },
                new byte[] { (byte)OP.CheckSig }
            };
        }
        [Theory]
        [MemberData(nameof(GetMultiSigCases))]
        public void Convert_MultiSigTest(IOperation[] ops, byte[][] sigs, byte[] expected)
        {
            ScriptSerializer ser = new();
            byte[] actual = ser.ConvertMulti(ops, sigs);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConvertP2wpkhTest()
        {
            IOperation[] ops = new IOperation[2] { new PushDataOp(OP._0), new PushDataOp(Helper.GetBytes(20)) };
            ScriptSerializer ser = new();
            byte[] actual = ser.ConvertP2wpkh(ops);
            byte[] expected = Helper.HexToBytes($"76a914{Helper.GetBytesHex(20)}88ac");

            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetWitCases()
        {
            byte cs = (byte)OP.CodeSeparator;
            // 76 byte length is good since it is the smallest byte with a different StackInt than CompactInt
            byte[] data76 = Helper.GetBytes(76);
            byte[] expWit76 = new byte[77];
            expWit76[0] = 76;
            Buffer.BlockCopy(data76, 0, expWit76, 1, data76.Length);

            yield return new object[] { Array.Empty<IOperation>(), Array.Empty<byte>() };
            yield return new object[]
            {
                new IOperation[] { new AddOp(), new DUP3Op() },
                new byte[] { (byte)OP.ADD, (byte)OP.DUP3 }
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(data76), new  PushDataOp(OP._5) },
                Helper.ConcatBytes(78, expWit76, new byte[] { (byte)OP._5 })
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new DUPOp(),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new PushDataOp(Helper.GetBytes(10)),
                    new CheckSigOp()
                },
                Helper.ConcatBytes(14, new byte[] { (byte)OP.DUP, cs, 10 }, Helper.GetBytes(10), new byte[] { (byte)OP.CheckSig })
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new DUPOp(),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new PushDataOp(Helper.GetBytes(10)),
                    new CheckSigOp()
                },
                Helper.ConcatBytes(12, new byte[] { 10 }, Helper.GetBytes(10), new byte[] { (byte)OP.CheckSig })
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                new byte[] { (byte)OP.IF, cs, (byte)OP.ELSE, cs, (byte)OP.EndIf, cs, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                new byte[] { (byte)OP.ELSE, cs, (byte)OP.EndIf, cs, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } }),
                    new CodeSeparatorOp() { IsExecuted = false },
                    new CheckSigOp()
                },
                new byte[] { (byte)OP.EndIf, cs, (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = false } },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } }),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new CheckSigOp()
                },
                new byte[] { (byte)OP.CheckSig }
            };
            yield return new object[]
            {
                // There is no signature removal for SegWit
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new PushDataOp(Helper.ShortSig1Bytes) }, null),
                    new CheckSigOp()
                },
                Helper.ConcatBytes(13, new byte[] { (byte)OP.IF, 9 }, Helper.ShortSig1Bytes,
                                      new byte[] { (byte)OP.EndIf, (byte)OP.CheckSig })

            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                new byte[]
                {
                    (byte)OP.IF, (byte)OP.ROLL, cs, (byte)OP.ADD, (byte)OP.ELSE, (byte)OP.SUB1, cs, (byte)OP.IfDup, (byte)OP.EndIf,
                    (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL
                }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = true }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                new byte[]
                {
                    (byte)OP.ADD, (byte)OP.ELSE, (byte)OP.SUB1, cs, (byte)OP.IfDup, (byte)OP.EndIf,
                    (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL
                }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = true }, new IfDupOp() }),
                    new Hash160Op(),
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                new byte[]
                {
                    (byte)OP.IfDup, (byte)OP.EndIf,
                    (byte)OP.HASH160, (byte)OP.CheckMultiSig, (byte)OP.EQUAL
                }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new IFOp(new IOperation[] { new ROLLOp(), new CodeSeparatorOp() { IsExecuted = false }, new AddOp() },
                             new IOperation[] { new SUB1Op(), new CodeSeparatorOp() { IsExecuted = false }, new IfDupOp() }),
                    new Hash160Op(),
                    new CodeSeparatorOp() { IsExecuted = true },
                    new CheckMultiSigOp(),
                    new EqualOp()
                },
                new byte[] { (byte)OP.CheckMultiSig, (byte)OP.EQUAL }
            };
        }
        [Theory]
        [MemberData(nameof(GetWitCases))]
        public void ConvertWitnessTest(IOperation[] ops, byte[] expected)
        {
            ScriptSerializer ser = new();
            byte[] actual = ser.ConvertWitness(ops);
            Assert.Equal(expected, actual);
        }
    }
}
