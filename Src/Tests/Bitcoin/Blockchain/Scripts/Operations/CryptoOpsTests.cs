// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using System.Text;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class CryptoOpsTests
    {
        private readonly byte[] message = Encoding.UTF8.GetBytes("message digest");
        private readonly byte[] expRipeMd = Helper.HexToBytes("5d0689ef49d2fae572b881b123a85ffa21595f36");
        private readonly byte[] expSha1 = Helper.HexToBytes("c12252ceda8be8994d5fa0290a47231c1d16aae3");
        private readonly byte[] expSha256 = Helper.HexToBytes("f7846f55cf23e14eebeab5b4e1550cad5b509e3348fbc4efa3a1413d393cb650");
        private readonly byte[] expHash160 = Helper.HexToBytes("c0f5356420849b03a32ddfa5f9204f41392bad94");
        private readonly byte[] expHash256 = Helper.HexToBytes("0b9731e12cfdc96ebb07e4a96d6dff767c20682c120743c177715033ce747c12");


        [Fact]
        public void RipeMd160OpTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { message },
                pushData = new byte[][] { expRipeMd }
            };

            OpTestCaseHelper.RunTest<RipeMd160Op>(data, OP.RIPEMD160);
        }

        [Fact]
        public void RipeMd160Op_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<RipeMd160Op>(data, Errors.NotEnoughStackItems);
        }


        [Fact]
        public void Sha1OpTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { message },
                pushData = new byte[][] { expSha1 }
            };

            OpTestCaseHelper.RunTest<Sha1Op>(data, OP.SHA1);
        }

        [Fact]
        public void Sha1Op_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<Sha1Op>(data, Errors.NotEnoughStackItems);
        }


        [Fact]
        public void Sha256OpTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { message },
                pushData = new byte[][] { expSha256 }
            };

            OpTestCaseHelper.RunTest<Sha256Op>(data, OP.SHA256);
        }

        [Fact]
        public void Sha256Op_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<Sha256Op>(data, Errors.NotEnoughStackItems);
        }


        [Fact]
        public void Hash160OpTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { message },
                pushData = new byte[][] { expHash160 }
            };

            OpTestCaseHelper.RunTest<Hash160Op>(data, OP.HASH160);
        }

        [Fact]
        public void Hash160Op_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<Hash160Op>(data, Errors.NotEnoughStackItems);
        }


        [Fact]
        public void Hash256OpTest()
        {
            MockOpData data = new(FuncCallName.Pop, FuncCallName.Push)
            {
                _itemCount = 1,
                popData = new byte[][] { message },
                pushData = new byte[][] { expHash256 }
            };

            OpTestCaseHelper.RunTest<Hash256Op>(data, OP.HASH256);
        }

        [Fact]
        public void Hash256OpOp_FailTest()
        {
            MockOpData data = new() { _itemCount = 0, };
            OpTestCaseHelper.RunFailTest<Hash256Op>(data, Errors.NotEnoughStackItems);
        }
    }
}
