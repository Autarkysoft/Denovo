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
            CodeSeparatorOp cs1 = new();
            CodeSeparatorOp cs2 = new(0);
            CodeSeparatorOp cs3 = new(1);

            Assert.Equal(0u, cs1.Position);
            Assert.Equal(0u, cs2.Position);
            Assert.Equal(1u, cs3.Position);
        }


        [Fact]
        public void RunTest()
        {
            uint mockPos = 5;
            CodeSeparatorOp op = new(mockPos);
            MockOpData mockStack = new();

            Assert.False(op.IsExecuted);
            Assert.Equal(OP.CodeSeparator, op.OpValue);
            Assert.Equal(0U, mockStack.CodeSeparatorPosition);

            bool b = op.Run(mockStack, out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.True(op.IsExecuted);
            Assert.Equal(mockPos, mockStack.CodeSeparatorPosition);
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
