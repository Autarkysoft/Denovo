// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.Conditionals
{
    public class IfElseOpsBaseTests
    {
        public static IEnumerable<object[]> GetCountSigOpCases()
        {
            yield return new object[] { Array.Empty<IOperation>(), null, 0 };
            yield return new object[] { new IOperation[] { new DUPOp() }, null, 0 };
            yield return new object[] { new IOperation[] { new ROT2Op(), new ROLLOp(), new Hash160Op() }, null, 0 };
            yield return new object[] { new IOperation[] { new ADD1Op(), new CheckSigOp(), new Hash160Op() }, null, 1 };
            yield return new object[] { new IOperation[] { new AddOp(), new CheckSigVerifyOp(), new Sha1Op() }, null, 1 };
            yield return new object[] { new IOperation[] { new CheckSigOp(), new CheckSigVerifyOp(), new CheckSigOp() }, null, 3 };
            yield return new object[]
            {
                Array.Empty<IOperation>(), new IOperation[] { new CheckSigOp(), new CheckSigVerifyOp(), new CheckSigOp() }, 3
            };
            yield return new object[]
            {
                // Only check the previous OP not stack item (in reality this is only 1 SigOp but counted as 20!)
                new IOperation[] { new PushDataOp(OP._1), new DUPOp(), new CheckMultiSigOp() },
                null,
                20
            };
            yield return new object[]
            {
                // Same as above
                new IOperation[] { new PushDataOp(OP._2), new CheckMultiSigOp() },
                null,
                2
            };
            yield return new object[]
            {
                // Same as above but for ElseOps
                Array.Empty<IOperation>(),
                new IOperation[] { new CheckMultiSigOp() },
                20
            };
            yield return new object[]
            {
                Array.Empty<IOperation>(),
                new IOperation[] { new PushDataOp(OP._1), new DUPOp(), new CheckMultiSigOp() },
                20
            };
            yield return new object[]
            {
                Array.Empty<IOperation>(),
                new IOperation[] { new PushDataOp(OP._2), new CheckMultiSigOp() },
                2
            };
            yield return new object[]
            {
                new IOperation[] { new CheckSigVerifyOp(), new PushDataOp(OP._10), new CheckMultiSigVerifyOp() },
                new IOperation[] { new Hash256Op(), new PushDataOp(OP._5), new CheckMultiSigOp() },
                16
            };
            yield return new object[]
            {
                // Only check the previous OP not stack item (in this case previous OP code is OP_IF)
                new IOperation[] { new CheckMultiSigOp() },
                null,
                20
            };
            yield return new object[]
            {
                // Same as above but for ElseOps (previous OP is OP_ELSE)
                Array.Empty<IOperation>(),
                new IOperation[] { new CheckMultiSigOp() },
                20
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[] { new CheckSigVerifyOp(), new IFOp(new IOperation[] { new CheckSigOp() }, null) } ,
                null,
                2
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[] { new CheckSigVerifyOp(), new IFOp(new IOperation[] { new CheckMultiSigOp() }, null) } ,
                new IOperation[]
                {
                    new IFOp(
                        new IOperation[] { new IFOp(new IOperation[] { new DUPOp(), new CheckSigOp() }, null) },
                        new IOperation[] { new CheckSigVerifyOp() })
                },
                23
            };
            yield return new object[]
            {
                // CheckMultiSig operations only count previous push if it is a number OP
                new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new CheckMultiSigOp() },
                new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new CheckMultiSigOp() },
                40
            };

            PushDataOp badEncoding = new();
            badEncoding.TryRead(new FastStreamReader(new byte[] { 1, 1 }), out _);
            yield return new object[]
            {
                // Same as above, even if the push is a valid small number but uses bad encoding
                new IOperation[] { badEncoding, new CheckMultiSigOp() },
                new IOperation[] { badEncoding, new CheckMultiSigOp() },
                40
            };
        }
        [Theory]
        [MemberData(nameof(GetCountSigOpCases))]
        public void CountSigOpsTest(IOperation[] main, IOperation[] other, int expected)
        {
            IFOp op = new(main, other);
            int actual = op.CountSigOps();
            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> GetExecCodeSepCases()
        {
            yield return new object[] { Array.Empty<IOperation>(), null, false };
            yield return new object[] { Array.Empty<IOperation>(), Array.Empty<IOperation>(), false };
            yield return new object[] { new IOperation[] { new DUPOp() }, new IOperation[] { new Hash256Op() }, false };
            yield return new object[]
            {
                new IOperation[] { new CodeSeparatorOp() },
                new IOperation[] { new CodeSeparatorOp() },
                false
            };
            yield return new object[]
            {
                new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                new IOperation[] { new CodeSeparatorOp() },
                true
            };
            yield return new object[]
            {
                new IOperation[] { new CodeSeparatorOp() },
                new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                true
            };
            yield return new object[]
            {
                new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                true
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new CodeSeparatorOp() { IsExecuted = false }, new ADD1Op() },
                new IOperation[] { new CheckSigOp(), new CodeSeparatorOp(), new DROP2Op(), new NEGATEOp() },
                false
            };
            yield return new object[]
            {
                new IOperation[] { new CheckSigOp(), new CodeSeparatorOp(), new DROP2Op(), new NEGATEOp() },
                new IOperation[] { new DUPOp(), new CodeSeparatorOp() { IsExecuted = false }, new ADD1Op() },
                false
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new CodeSeparatorOp() { IsExecuted = true }, new ADD1Op() },
                new IOperation[] { new CheckSigOp(), new CodeSeparatorOp(), new DROP2Op(), new NEGATEOp() },
                true
            };
            yield return new object[]
            {
                new IOperation[] { new CheckSigOp(), new CodeSeparatorOp(), new DROP2Op(), new NEGATEOp() },
                new IOperation[] { new DUPOp(), new CodeSeparatorOp() { IsExecuted = true }, new ADD1Op() },
                true
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[]
                {
                    new CheckSigOp(), new CodeSeparatorOp(),
                    new IFOp(new IOperation[] { new CodeSeparatorOp() }, new IOperation[] { new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new DUPOp(), new CodeSeparatorOp(), new ADD1Op(),
                    new IFOp(new IOperation[] { new CheckMultiSigOp(), new CodeSeparatorOp(), new ADD1Op() },
                             new IOperation[] { new DUP3Op(), new DUP2Op(), new CodeSeparatorOp() })
                },
                false
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[]
                {
                    new CheckSigOp(), new CodeSeparatorOp() { IsExecuted = true },
                    new IFOp(new IOperation[] { new CodeSeparatorOp() }, new IOperation[] { new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new DUPOp(), new CodeSeparatorOp(), new ADD1Op(),
                    new IFOp(new IOperation[] { new CheckMultiSigOp(), new CodeSeparatorOp(), new ADD1Op() },
                             new IOperation[] { new DUP3Op(), new DUP2Op(), new CodeSeparatorOp() })
                },
                true
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[]
                {
                    new CheckSigOp(), new CodeSeparatorOp(),
                    new IFOp(new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } },
                             new IOperation[] { new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new DUPOp(), new CodeSeparatorOp(), new ADD1Op(),
                    new IFOp(new IOperation[] { new CheckMultiSigOp(), new CodeSeparatorOp(), new ADD1Op() },
                             new IOperation[] { new DUP3Op(), new DUP2Op(), new CodeSeparatorOp() })
                },
                true
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[]
                {
                    new CheckSigOp(), new CodeSeparatorOp(),
                    new IFOp(new IOperation[] { new CodeSeparatorOp() },
                             new IOperation[] { new CodeSeparatorOp() { IsExecuted = true } })
                },
                new IOperation[]
                {
                    new DUPOp(), new CodeSeparatorOp(), new ADD1Op(),
                    new IFOp(new IOperation[] { new CheckMultiSigOp(), new CodeSeparatorOp(), new ADD1Op() },
                             new IOperation[] { new DUP3Op(), new DUP2Op(), new CodeSeparatorOp() })
                },
                true
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[]
                {
                    new CheckSigOp(), new CodeSeparatorOp(),
                    new IFOp(new IOperation[] { new CodeSeparatorOp() },
                             new IOperation[] { new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new DUPOp(), new CodeSeparatorOp() { IsExecuted = true }, new ADD1Op(),
                    new IFOp(new IOperation[] { new CheckMultiSigOp(), new CodeSeparatorOp(), new ADD1Op() },
                             new IOperation[] { new DUP3Op(), new DUP2Op(), new CodeSeparatorOp() })
                },
                true
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[]
                {
                    new CheckSigOp(), new CodeSeparatorOp(),
                    new IFOp(new IOperation[] { new CodeSeparatorOp() },
                             new IOperation[] { new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new DUPOp(), new CodeSeparatorOp(), new ADD1Op(),
                    new IFOp(new IOperation[] { new CheckMultiSigOp(), new CodeSeparatorOp() { IsExecuted = true }, new ADD1Op() },
                             new IOperation[] { new DUP3Op(), new DUP2Op(), new CodeSeparatorOp() })
                },
                true
            };
            yield return new object[]
            {
                // Nested ifs
                new IOperation[]
                {
                    new CheckSigOp(), new CodeSeparatorOp(),
                    new IFOp(new IOperation[] { new CodeSeparatorOp() },
                             new IOperation[] { new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new DUPOp(), new CodeSeparatorOp(), new ADD1Op(),
                    new IFOp(new IOperation[] { new CheckMultiSigOp(), new CodeSeparatorOp(), new ADD1Op() },
                             new IOperation[] { new DUP3Op(), new DUP2Op(), new CodeSeparatorOp() { IsExecuted = true } })
                },
                true
            };
        }
        [Theory]
        [MemberData(nameof(GetExecCodeSepCases))]
        public void HasExecutedCodeSeparatorTest(IOperation[] main, IOperation[] other, bool expected)
        {
            IFOp op = new(main, other);
            bool actual = op.HasExecutedCodeSeparator();
            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetStreamCases()
        {
            byte start = (byte)OP.IF;
            byte middle = (byte)OP.ELSE;
            byte end = (byte)OP.EndIf;

            yield return new object[] { null, null, new byte[] { start, end } };
            yield return new object[] { null, Array.Empty<IOperation>(), new byte[] { start, middle, end } };
            yield return new object[] { Array.Empty<IOperation>(), Array.Empty<IOperation>(), new byte[] { start, middle, end } };
            yield return new object[]
            {
                new IOperation[] { new DUPOp() },
                null,
                new byte[] { start, (byte)OP.DUP, end }
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new DUP2Op() },
                null,
                new byte[] { start, (byte)OP.DUP, (byte)OP.DUP2, end }
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new DUP2Op() },
                Array.Empty<IOperation>(),
                new byte[] { start, (byte)OP.DUP, (byte)OP.DUP2, middle, end }
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new DUP2Op() },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 5, 6 }) },
                new byte[] { start, 3, 1, 2, 3, (byte)OP.DUP2, middle, (byte)OP.HASH160, 2, 5, 6, end }
            };
            yield return new object[]
            {
                // Nested Ifs
                new IOperation[]
                {
                    new Hash160Op(), new IFOp(new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new DUP2Op() },
                                              Array.Empty<IOperation>())
                },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 5, 6 }) },
                new byte[] { start, (byte)OP.HASH160, start, 3, 1, 2, 3, (byte)OP.DUP2, middle, end,
                             middle, (byte)OP.HASH160, 2, 5, 6, end }
            };
        }
        [Theory]
        [MemberData(nameof(GetStreamCases))]
        public void WriteToStreamTest(IOperation[] main, IOperation[] other, byte[] expected)
        {
            IFOp op = new(main, other);
            FastStream stream = new();
            op.WriteToStream(stream);

            Assert.Equal(expected, stream.ToByteArray());
        }

        public static IEnumerable<object[]> GetStreamSignSingleCases()
        {
            byte start = (byte)OP.IF;
            byte middle = (byte)OP.ELSE;
            byte end = (byte)OP.EndIf;
            byte[] sig = new byte[] { 10, 20, 30 };

            yield return new object[] { null, null, sig, new byte[] { start, end } };
            yield return new object[] { null, Array.Empty<IOperation>(), sig, new byte[] { start, middle, end } };
            yield return new object[] { Array.Empty<IOperation>(), Array.Empty<IOperation>(), sig, new byte[] { start, middle, end } };
            yield return new object[]
            {
                new IOperation[] { new DUPOp() },
                null,
                sig,
                new byte[] { start, (byte)OP.DUP, end }
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new DUP2Op() },
                null,
                sig,
                new byte[] { start, (byte)OP.DUP, (byte)OP.DUP2, end }
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new DUP2Op() },
                Array.Empty<IOperation>(),
                sig,
                new byte[] { start, (byte)OP.DUP, (byte)OP.DUP2, middle, end }
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new DUP2Op() },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 5, 6 }) },
                sig,
                new byte[] { start, 3, 1, 2, 3, (byte)OP.DUP2, middle, (byte)OP.HASH160, 2, 5, 6, end }
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(new byte[] { 10, 20, 30 }), new DUP2Op() },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 10, 20, 30 }) },
                sig,
                new byte[] { start, (byte)OP.DUP2, middle, (byte)OP.HASH160, end }
            };
            yield return new object[]
            {
                // Nested Ifs
                new IOperation[]
                {
                    new Hash160Op(), new IFOp(new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new DUP2Op() },
                                              Array.Empty<IOperation>())
                },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 5, 6 }) },
                sig,
                new byte[] { start, (byte)OP.HASH160, start, 3, 1, 2, 3, (byte)OP.DUP2, middle, end,
                             middle, (byte)OP.HASH160, 2, 5, 6, end }
            };
            yield return new object[]
            {
                // Nested Ifs (make sure correct method on PushDataOp is called => removes the "signature")
                new IOperation[]
                {
                    new Hash160Op(), new IFOp(new IOperation[] { new PushDataOp(new byte[] { 10, 20, 30 }), new DUP2Op() },
                                              Array.Empty<IOperation>())
                },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 10, 20, 30 }) },
                sig,
                new byte[] { start, (byte)OP.HASH160, start, (byte)OP.DUP2, middle, end,
                             middle, (byte)OP.HASH160, end }
            };
            yield return new object[]
            {
                // Nested Ifs (all unexecuted CS)
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] { start,
                               start, middle, end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                // Nested Ifs (some executed CS in different places)
                new IOperation[]
                {
                    new CodeSeparatorOp() { IsExecuted = true },
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] {
                               start, middle, end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] {
                                     middle, end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }), new CodeSeparatorOp() { IsExecuted = true } },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] {
                                     middle, end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] {
                                              end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }), new CodeSeparatorOp() { IsExecuted = true } })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] {
                                              end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp() { IsExecuted = true },
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] {
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] {
                                      middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }), new CodeSeparatorOp(){ IsExecuted = true } },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] {
                                      middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                sig,
                new byte[] { end, end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }), new CodeSeparatorOp() { IsExecuted = true } })
                },
                sig,
                new byte[] { end, end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() }),
                    new CodeSeparatorOp() { IsExecuted = true }
                },
                sig,
                new byte[] { end }
            };
        }
        [Theory]
        [MemberData(nameof(GetStreamSignSingleCases))]
        public void WriteToStreamForSigning_SingleTest(IOperation[] main, IOperation[] other, byte[] sig, byte[] expected)
        {
            IFOp op = new(main, other);
            FastStream stream = new();
            op.WriteToStreamForSigning(stream, sig);

            Assert.Equal(expected, stream.ToByteArray());
        }

        public static IEnumerable<object[]> GetStreamSignMultiCases()
        {
            byte start = (byte)OP.IF;
            byte middle = (byte)OP.ELSE;
            byte end = (byte)OP.EndIf;
            byte[][] sigs = new byte[][] { new byte[] { 10, 20, 30 }, new byte[] { 40, 50 } };

            yield return new object[] { null, null, sigs, new byte[] { start, end } };
            yield return new object[] { null, Array.Empty<IOperation>(), sigs, new byte[] { start, middle, end } };
            yield return new object[] { Array.Empty<IOperation>(), Array.Empty<IOperation>(), sigs, new byte[] { start, middle, end } };
            yield return new object[]
            {
                new IOperation[] { new DUPOp() },
                null,
                sigs,
                new byte[] { start, (byte)OP.DUP, end }
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new DUP2Op() },
                null,
                sigs,
                new byte[] { start, (byte)OP.DUP, (byte)OP.DUP2, end }
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new DUP2Op() },
                Array.Empty<IOperation>(),
                sigs,
                new byte[] { start, (byte)OP.DUP, (byte)OP.DUP2, middle, end }
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new DUP2Op() },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 5, 6 }) },
                sigs,
                new byte[] { start, 3, 1, 2, 3, (byte)OP.DUP2, middle, (byte)OP.HASH160, 2, 5, 6, end }
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(new byte[] { 10, 20, 30 }), new DUP2Op() },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 40, 50 }) },
                sigs,
                new byte[] { start, (byte)OP.DUP2, middle, (byte)OP.HASH160, end }
            };
            yield return new object[]
            {
                // Nested Ifs
                new IOperation[]
                {
                    new Hash160Op(), new IFOp(new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new DUP2Op() },
                                              Array.Empty<IOperation>())
                },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 5, 6 }) },
                sigs,
                new byte[] { start, (byte)OP.HASH160, start, 3, 1, 2, 3, (byte)OP.DUP2, middle, end,
                             middle, (byte)OP.HASH160, 2, 5, 6, end }
            };
            yield return new object[]
            {
                // Nested Ifs (make sure correct method on PushDataOp is called => removes the "signature")
                new IOperation[]
                {
                    new Hash160Op(), new IFOp(new IOperation[] { new PushDataOp(new byte[] { 10, 20, 30 }), new DUP2Op() },
                                              Array.Empty<IOperation>())
                },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 10, 20, 30 }) },
                sigs,
                new byte[] { start, (byte)OP.HASH160, start, (byte)OP.DUP2, middle, end,
                             middle, (byte)OP.HASH160, end }
            };
            yield return new object[]
            {
                // Nested Ifs (all unexecuted CS)
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] { start,
                               start, middle, end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                // Nested Ifs (some executed CS in different places)
                new IOperation[]
                {
                    new CodeSeparatorOp() { IsExecuted = true },
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                               start, middle, end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                                     middle, end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }), new CodeSeparatorOp() { IsExecuted = true } },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                                     middle, end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                                              end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }), new CodeSeparatorOp() { IsExecuted = true } })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                                              end,
                             middle,
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp() { IsExecuted = true },
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                               start, middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                                      middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }), new CodeSeparatorOp(){ IsExecuted = true } },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                                      middle, end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                sigs,
                new byte[] {
                                              end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }), new CodeSeparatorOp() { IsExecuted = true } })
                },
                sigs,
                new byte[] {
                                              end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() }),
                    new CodeSeparatorOp() { IsExecuted = true }
                },
                sigs,
                new byte[] { end }
            };
        }
        [Theory]
        [MemberData(nameof(GetStreamSignMultiCases))]
        public void WriteToStreamForSigning_MultiTest(IOperation[] main, IOperation[] other, byte[][] sigs, byte[] expected)
        {
            IFOp op = new(main, other);
            FastStream stream = new();
            op.WriteToStreamForSigning(stream, sigs);

            Assert.Equal(expected, stream.ToByteArray());
        }


        public static IEnumerable<object[]> GetStreamSignSegWitCases()
        {
            byte start = (byte)OP.IF;
            byte middle = (byte)OP.ELSE;
            byte end = (byte)OP.EndIf;
            byte cs = (byte)OP.CodeSeparator;

            yield return new object[] { null, null, new byte[] { start, end } };
            yield return new object[] { null, Array.Empty<IOperation>(), new byte[] { start, middle, end } };
            yield return new object[] { Array.Empty<IOperation>(), Array.Empty<IOperation>(), new byte[] { start, middle, end } };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }) },
                new IOperation[] { new Hash160Op(), new CodeSeparatorOp(), new MAXOp() },
                new byte[] { start, (byte)OP.DUP, cs, 2, 10, 20, middle, (byte)OP.HASH160, cs, (byte)OP.MAX, end }
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 10, 20 }) },
                new IOperation[] { new Hash160Op(), new CodeSeparatorOp(), new MAXOp() },
                new byte[] { 2, 10, 20, middle, (byte)OP.HASH160, cs, (byte)OP.MAX, end }
            };
            yield return new object[]
            {
                new IOperation[] { new DUPOp(), new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20 }) },
                new IOperation[] { new Hash160Op(), new CodeSeparatorOp() { IsExecuted = true }, new MAXOp() },
                new byte[] { (byte)OP.MAX, end }
            };
            yield return new object[]
            {
                // Nested Ifs (there is no signature removal in SegWit)
                new IOperation[]
                {
                    new Hash160Op(), new IFOp(new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new DUP2Op() },
                                              Array.Empty<IOperation>())
                },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 5, 6 }) },
                new byte[] { start, (byte)OP.HASH160, start, 3, 1, 2, 3, (byte)OP.DUP2, middle, end,
                             middle, (byte)OP.HASH160, 2, 5, 6, end }
            };
            yield return new object[]
            {
                // Nested Ifs (all unexecuted CS)
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] { start,
                               cs, start,
                                     cs, 3, 10, 20, 30, cs,
                                   middle,
                                     cs, 2, 40, 50, cs,
                                   end,
                             middle,
                               cs, start,
                                     cs, 3, 70, 80, 90, cs,
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                // Nested Ifs (some unexecuted CS)
                new IOperation[]
                {
                    new CodeSeparatorOp() { IsExecuted = true },
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                   start,
                                     cs, 3, 10, 20, 30, cs,
                                   middle,
                                     cs, 2, 40, 50, cs,
                                   end,
                             middle,
                               cs, start,
                                     cs, 3, 70, 80, 90, cs,
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                         3, 10, 20, 30, cs,
                                   middle,
                                     cs, 2, 40, 50, cs,
                                   end,
                             middle,
                               cs, start,
                                     cs, 3, 70, 80, 90, cs,
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() { IsExecuted = true } },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                   middle,
                                     cs, 2, 40, 50, cs,
                                   end,
                             middle,
                               cs, start,
                                     cs, 3, 70, 80, 90, cs,
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                         2, 40, 50, cs,
                                   end,
                             middle,
                               cs, start,
                                     cs, 3, 70, 80, 90, cs,
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp(){ IsExecuted = true } })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                   end,
                             middle,
                               cs, start,
                                     cs, 3, 70, 80, 90, cs,
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp() { IsExecuted = true },
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                   start,
                                     cs, 3, 70, 80, 90, cs,
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                         3, 70, 80, 90, cs,
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() { IsExecuted = true } },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                   middle,
                                     cs, 2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp() { IsExecuted = true }, new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() })
                },
                new byte[] {
                                         2, 200, 250, cs,
                                   end,
                             end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() { IsExecuted = true } })
                },
                new byte[] { end, end }
            };
            yield return new object[]
            {
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 10, 20, 30 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 40, 50 }), new CodeSeparatorOp() })
                },
                new IOperation[]
                {
                    new CodeSeparatorOp(),
                    new IFOp(
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 70, 80, 90 }), new CodeSeparatorOp() },
                        new IOperation[] { new CodeSeparatorOp(), new PushDataOp(new byte[] { 200, 250 }), new CodeSeparatorOp() }),
                    new CodeSeparatorOp() { IsExecuted = true }
                },
                new byte[] { end }
            };
        }
        [Theory]
        [MemberData(nameof(GetStreamSignSegWitCases))]
        public void WriteToStreamForSigningSegWitTest(IOperation[] main, IOperation[] other, byte[] expected)
        {
            IFOp op = new(main, other);
            FastStream stream = new();
            op.WriteToStreamForSigningSegWit(stream);

            Assert.Equal(expected, stream.ToByteArray());
        }


        public static IEnumerable<object[]> GetEqualsCases()
        {
            IFOp op1 = new(null, null);
            IFOp op2 = new(null, null);
            IFOp op3 = new(Array.Empty<IOperation>(), null); // mainOp is the same with null or empty array
            IFOp op4 = new(null, Array.Empty<IOperation>()); // but elseOp is not the same
            IFOp op5 = new(new IOperation[] { new DUPOp(), new ABSOp() }, null);
            IFOp op6 = new(new IOperation[] { new DUPOp(), new ABSOp() }, Array.Empty<IOperation>());
            IFOp op7 = new(new IOperation[] { new DUP2Op(), new ABSOp() }, null);
            IFOp op8 = new(null, new IOperation[] { new DUPOp(), new Sha256Op() });
            IFOp op9 = new(null, new IOperation[] { new DUPOp(), new Sha256Op() });
            IFOp op10 = new(null, new IOperation[] { new DUPOp(), new Sha1Op() });
            IFOp op11 = new(null, new IOperation[] { new DUPOp() });

            yield return new object[] { op1, "operation!", false };
            yield return new object[] { op1, op1, true };
            yield return new object[] { op1, op2, true };
            yield return new object[] { op1, op3, true };
            yield return new object[] { op1, op4, false };
            yield return new object[] { op4, op1, false };
            yield return new object[] { op3, op4, false };
            yield return new object[] { op1, op6, false };
            yield return new object[] { op5, op6, false };
            yield return new object[] { op5, op7, false };
            yield return new object[] { op8, op9, true };
            yield return new object[] { op8, op10, false };
            yield return new object[] { op8, op11, false };
        }
        [Theory]
        [MemberData(nameof(GetEqualsCases))]
        public void EqualsTest(IfElseOpsBase first, object second, bool expBool)
        {
            bool b = first.Equals(second);
            Assert.Equal(expBool, b);
        }

        [Fact]
        public void GetHashCodeTest()
        {
            IFOp op1 = new(null, null);
            IFOp op2 = new(null, null);
            IFOp op3 = new(Array.Empty<IOperation>(), null);
            IFOp op4 = new(null, Array.Empty<IOperation>());
            IFOp op5 = new(new IOperation[] { new DUP2Op() }, null);
            IFOp op6 = new(new IOperation[] { new DUP2Op() }, new IOperation[] { new DUP2Op() });

            int h1 = op1.GetHashCode();
            int h2 = op2.GetHashCode();
            int h3 = op3.GetHashCode();
            int h4 = op4.GetHashCode();
            int h5 = op5.GetHashCode();
            int h6 = op6.GetHashCode();


            Assert.Equal(h1, h2);
            Assert.Equal(h1, h3);
            Assert.NotEqual(h1, h4);
            Assert.NotEqual(h1, h5);
            Assert.NotEqual(h5, h6);
        }
    }
}
