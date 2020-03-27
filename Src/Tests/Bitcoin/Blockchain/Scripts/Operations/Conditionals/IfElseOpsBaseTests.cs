// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations.Conditionals
{
    public class IfElseOpsBaseTests
    {
        public static IEnumerable<object[]> GetStreamCases()
        {
            byte start = (byte)OP.IF;
            byte middle = (byte)OP.ELSE;
            byte end = (byte)OP.EndIf;

            yield return new object[] { null, null, new byte[] { start, end } };
            yield return new object[] { null, new IOperation[0], new byte[] { start, middle, end } };
            yield return new object[] { new IOperation[0], new IOperation[0], new byte[] { start, middle, end } };
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
                new IOperation[0],
                new byte[] { start, (byte)OP.DUP, (byte)OP.DUP2, middle, end }
            };
            yield return new object[]
            {
                new IOperation[] { new PushDataOp(new byte[] { 1, 2, 3 }), new DUP2Op() },
                new IOperation[] { new Hash160Op(), new PushDataOp(new byte[] { 5, 6 }) },
                new byte[] { start, 3, 1, 2, 3, (byte)OP.DUP2, middle, (byte)OP.HASH160, 2, 5, 6, end }
            };
        }
        [Theory]
        [MemberData(nameof(GetStreamCases))]
        public void WriteToStreamTest(IOperation[] main, IOperation[] other, byte[] expected)
        {
            IFOp op = new IFOp(main, other, true);
            FastStream stream = new FastStream();
            op.WriteToStream(stream);

            Assert.Equal(expected, stream.ToByteArray());
        }


        public static IEnumerable<object[]> GetEqualsCases()
        {
            IFOp op1 = new IFOp(null, null, true);
            IFOp op2 = new IFOp(null, null, true);
            IFOp op3 = new IFOp(new IOperation[0], null, true); // mainOp is the same with null or empty array
            IFOp op4 = new IFOp(null, new IOperation[0], true); // but elseOp is not the same
            IFOp op5 = new IFOp(new IOperation[] { new DUPOp(), new ABSOp() }, null, true);
            IFOp op6 = new IFOp(new IOperation[] { new DUPOp(), new ABSOp() }, new IOperation[0], true);

            yield return new object[] { op1, "operation!", false };
            yield return new object[] { op1, op1, true };
            yield return new object[] { op1, op2, true };
            yield return new object[] { op1, op3, true };
            yield return new object[] { op1, op4, false };
            yield return new object[] { op3, op4, false };
            yield return new object[] { op1, op6, false };
            yield return new object[] { op5, op6, false };
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
            IFOp op1 = new IFOp(null, null, true);
            IFOp op2 = new IFOp(null, null, false);
            IFOp op3 = new IFOp(new IOperation[0], null, true);
            IFOp op4 = new IFOp(null, new IOperation[0], true);
            IFOp op5 = new IFOp(new IOperation[] { new DUP2Op() }, null, true);
            IFOp op6 = new IFOp(new IOperation[] { new DUP2Op() }, new IOperation[] { new DUP2Op() }, true);

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
