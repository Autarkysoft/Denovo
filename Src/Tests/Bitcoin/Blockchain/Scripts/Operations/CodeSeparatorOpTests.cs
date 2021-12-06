// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CodeSeparatorOpTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var cs1 = new CodeSeparatorOp();
            var cs2 = new CodeSeparatorOp(0);
            var cs3 = new CodeSeparatorOp(1);

            Assert.Equal(0u, cs1.Position);
            Assert.Equal(0u, cs2.Position);
            Assert.Equal(1u, cs3.Position);
        }


        [Fact]
        public void RunTest()
        {
            CodeSeparatorOp op = new();

            Assert.False(op.IsExecuted);
            Assert.Equal(OP.CodeSeparator, op.OpValue);

            bool b = op.Run(new MockOpData(), out string error);

            Assert.True(b, error);
            Assert.Null(error);
            Assert.True(op.IsExecuted);
        }

        [Fact]
        public void WriteToStreamTest()
        {
            // This test makes sure WriteToStream is not overriden and performs correctly
            CodeSeparatorOp op = new();
            FastStream stream = new(5);

            op.WriteToStream(stream);

            Assert.Equal(new byte[1] { (byte)OP.CodeSeparator }, stream.ToByteArray());
        }

        [Fact]
        public void WriteToStreamForSigningTest()
        {
            FastStream stream = new(5);

            CodeSeparatorOp op = new();
            op.WriteToStreamForSigning(stream, new byte[] { 1, 2 });

            // Execution should not make any difference
            op.IsExecuted = true;
            op.WriteToStreamForSigning(stream, new byte[] { 1, 2 });

            Assert.Equal(Array.Empty<byte>(), stream.ToByteArray());
        }

        [Fact]
        public void WriteToStreamForSigning_MultiTest()
        {
            FastStream stream = new(5);

            CodeSeparatorOp op = new();
            op.WriteToStreamForSigning(stream, new byte[][] { new byte[] { 1, 2 } });

            // Execution should not make any difference
            op.IsExecuted = true;
            op.WriteToStreamForSigning(stream, new byte[][] { new byte[] { 1, 2 } });

            Assert.Equal(Array.Empty<byte>(), stream.ToByteArray());
        }

        [Fact]
        public void WriteToStreamForSigningSegWit_NotExecutedTest()
        {
            CodeSeparatorOp op = new()
            {
                IsExecuted = false
            };
            FastStream stream = new(5);

            op.WriteToStreamForSigningSegWit(stream);

            Assert.Equal(new byte[1] { (byte)OP.CodeSeparator }, stream.ToByteArray());
        }
        [Fact]
        public void WriteToStreamForSigningSegWit_ExecutedTest()
        {
            CodeSeparatorOp op = new()
            {
                IsExecuted = true
            };
            FastStream stream = new(5);

            op.WriteToStreamForSigningSegWit(stream);

            Assert.Equal(Array.Empty<byte>(), stream.ToByteArray());
        }
    }
}
