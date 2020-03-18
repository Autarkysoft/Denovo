// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Blocks
{
    public class BlockTests
    {
        public static IEnumerable<object[]> GetCtorNullCases()
        {
            yield return new object[] { null, new byte[32], new ITransaction[1] };
            yield return new object[] { new byte[32], null, new ITransaction[1] };
            yield return new object[] { new byte[32], new byte[32], new ITransaction[0] };
            yield return new object[] { new byte[32], new byte[32], null };
        }
        [Theory]
        [MemberData(nameof(GetCtorNullCases))]
        public void Constructor_NullExceptionTest(byte[] header, byte[] merkle, ITransaction[] txs)
        {
            Assert.Throws<ArgumentNullException>(() => new Block(1, header, merkle, 123, 0x1d00ffffU, 0, txs));
        }

        public static IEnumerable<object[]> GetCtorOutOfRangeCases()
        {
            yield return new object[] { new byte[31], new byte[32] };
            yield return new object[] { new byte[32], new byte[33] };
        }
        [Theory]
        [MemberData(nameof(GetCtorOutOfRangeCases))]
        public void Constructor_OutOfRangeExceptionTest(byte[] header, byte[] merkle)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Block(1, header, merkle, 123, 0x1d00ffffU, 0, new ITransaction[1]));
        }


        private static Block GetSampleBlock()
        {
            return new Block()
            {
                Version = 0x3fffe000,
                PreviousBlockHeaderHash = Helper.HexToBytes("97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000"),
                MerkleRootHash = Helper.HexToBytes("afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7"),
                BlockTime = 0x5e71b1c6,
                NBits = 0x17110119,
                Nonce = 0x2a436a69
            };
        }

        private static string GetSampleBlockHex() => "0000000000000000000d558fdcdde616702d1f91d6c8567a89be99ff9869012d";
        private static byte[] GetSampleBlockHash() => Helper.HexToBytes(GetSampleBlockHex(), true);
        private static byte[] GetSampleBlockHeaderBytes() => Helper.HexToBytes("00e0ff3f97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7c6b1715e19011117696a432a");

        [Fact]
        public void GetBlockHashTest()
        {
            Block blk = GetSampleBlock();

            byte[] actual = blk.GetBlockHash();
            byte[] expected = GetSampleBlockHash();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBlockIDTest()
        {
            Block blk = GetSampleBlock();

            string actual = blk.GetBlockID();
            string expected = GetSampleBlockHex();

            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetMerkleCases()
        {
            yield return new object[]
            {
                 new ITransaction[1] // block #1
                 {
                     // MockTx reverses the tx id (hex) to return the correct hash
                     new MockTxIdTx("0e3e2357e806b6cdb1f70b54c3a3a17b6714ee1f0e68bebb44a74b1efd512098")
                 },
                 // Merkle root copied from block explorers is in reverse order and needs to be corrected
                 Helper.HexToBytes("0e3e2357e806b6cdb1f70b54c3a3a17b6714ee1f0e68bebb44a74b1efd512098", true)
            };
            yield return new object[]
            {
                new ITransaction[2] // block #170
                {
                    new MockTxIdTx("b1fea52486ce0c62bb442b530a3f0132b826c74e473d1f2c220bfa78111c5082"),
                    new MockTxIdTx("f4184fc596403b9d638783cf57adfe4c75c605f6356fbc91338530e9831e9e16")
                },
                Helper.HexToBytes("7dac2c5666815c17a3b36427de37bb9d2e2c5ccec3f8633eb91a4205cb4c10ff", true)
            };
            yield return new object[]
            {
                new ITransaction[3] // block #586  3(->4) => 2 => 1
                {
                    new MockTxIdTx("d45724bacd1480b0c94d363ebf59f844fb54e60cdfda0cd38ef67154e9d0bc43"),
                    new MockTxIdTx("4d6edbeb62735d45ff1565385a8b0045f066055c9425e21540ea7a8060f08bf2"),
                    new MockTxIdTx("6bf363548b08aa8761e278be802a2d84b8e40daefe8150f9af7dd7b65a0de49f")
                },
                Helper.HexToBytes("197b3d968ce463aa5da7d8eeba8af35eba80ded4e4fe6808e6cc0dd1c069594d", true)
            };
            yield return new object[]
            {
                new ITransaction[4] // block #546  4 => 2 => 1
                {
                    new MockTxIdTx("e980fe9f792d014e73b95203dc1335c5f9ce19ac537a419e6df5b47aecb93b70"),
                    new MockTxIdTx("28204cad1d7fc1d199e8ef4fa22f182de6258a3eaafe1bbe56ebdcacd3069a5f"),
                    new MockTxIdTx("6b0f8a73a56c04b519f1883e8aafda643ba61a30bd1439969df21bea5f4e27e2"),
                    new MockTxIdTx("3c1d7e82342158e4109df2e0b6348b6e84e403d8b4046d7007663ace63cddb23")
                },
                Helper.HexToBytes("e10a7f8442ea6cc6803a2b83713765c0b1199924110205f601f90fef125e7dfe", true)
            };
            yield return new object[]
            {
                new ITransaction[5] // block #26816  5(->6) => 3(->4) => 2 => 1
                {
                    new MockTxIdTx("0ef1e30857aaae86e317c993bb14f2e73bfbe7a86292f63b320550a2b3d10b0f"),
                    new MockTxIdTx("c6dbae4c8ca97a746030b390441cdfc750218a20b07d29b56f07b157cdc0bbd3"),
                    new MockTxIdTx("b34d15d7b7e6c2a4333fe13f354de1d715b7d8d00ec86b4cf0f8d24bfa71a2e1"),
                    new MockTxIdTx("a40d0843b9868a26792e952851a082442eace99f2c384f0ed6ca991612fd2f60"),
                    new MockTxIdTx("0b8f2d77c16afaa08435d71cd31467e62011cc39fe1d1318959bc74f1ad5b064"),
                },
                Helper.HexToBytes("4ca93df2e469f6b5eadf3cb41fda4959563a791e4a20fc65fe29272d73a01bbd", true)
            };
            yield return new object[]
            {
                new ITransaction[6] // block #2812  6 => 3(->4) => 2 => 1
                {
                    new MockTxIdTx("73c145f4a4ad7375a6fd0c2aa2de5ff2776033151f1a077bfa2ae893b0f4fa7b"),
                    new MockTxIdTx("00e45be5b605fdb2106afa4cef5992ee6d4e3724de5dc8b13e729a3fc3ad4b94"),
                    new MockTxIdTx("74c1a6dd6e88f73035143f8fc7420b5c395d28300a70bb35b943f7f2eddc656d"),
                    new MockTxIdTx("131f68261e28a80c3300b048c4c51f3ca4745653ba7ad6b20cc9188322818f25"),
                    new MockTxIdTx("a64be218809b61ac67ddc7f6c7f9fbebfe420cf75fe0318ebc727f060df48b37"),
                    new MockTxIdTx("8f5db6d157f79f2649719d5c3ff12eb5502edf098dbfb69d6ce58363e6ff293f"),
                },
                Helper.HexToBytes("289a86c44c4698fd8f181929dc2dd3c25820c959eab28980b27bb3cf8fcacb65", true)
            };
            yield return new object[]
            {
                new ITransaction[7] // block #49820  7(->8) => 4 => 2 => 1
                {
                    new MockTxIdTx("d52ef09a696544f062bf8ea68d8ffb3b3aa6aaf5c7017271334b05509f5c624b"),
                    new MockTxIdTx("380e2b238302f6e91779b5f2dd1173fa6142e67f605155a3264cf3612396a63c"),
                    new MockTxIdTx("49d7e47db44fa3fd650e411bcc0fe197fcbecb2c44e0414909d1598f491dd15e"),
                    new MockTxIdTx("995580f9f29a2c015cb2e4e3b9477011ea187c5952eb7f9d9a41b5d6ae4e0667"),
                    new MockTxIdTx("c059911a80d2b52780920e22faf11bad30ba1fff808cce6bc750a71d135109dc"),
                    new MockTxIdTx("026e5a5d362cb4ea40c73e38d7512bce9742b13ab9dd78b4654607f1723e62e1"),
                    new MockTxIdTx("447c8d3c215deedbdb4c1f95fa5c44d71fb5f7c82ed9b7550a9a021b86c463e6"),
                },
                Helper.HexToBytes("c986ddbf9bef586077a95ac9a92446e92769502c555f1e21bba536d4734ca4a8", true)
            };
            yield return new object[]
            {
                new ITransaction[8] // block #53066  8 => 4 => 2 => 1
                {
                    new MockTxIdTx("e0598db6abb41bf57ee0019c23520121565d2217eb9ae91d2114199fec5ac41d"),
                    new MockTxIdTx("1001d10ddf64509c1548125ca3120f32355e8af588fe6724aa5dc033e699a617"),
                    new MockTxIdTx("3cd17728f2e9152cc908976701a28e910838a86fe5b745af74bd5b373aff6e1d"),
                    new MockTxIdTx("7d8514357058d8b1a08d51bbca54329b7dbafc5c2e792f99c38e67297fda2c28"),
                    new MockTxIdTx("32a83b09394f17131074360c6628147bfb3eaf0f57000bc416da7bce140c74dd"),
                    new MockTxIdTx("4e3a183b09d35e5adeed6d12c880b46486db3f25869c939269770268a7bd5298"),
                    new MockTxIdTx("8fb3751403381c11979f8b0d9fac7b121ad49561c6a07645e58da7d5ab5bf8f8"),
                    new MockTxIdTx("c429d280b4f74e016c358d8bb3a909889ee23b058c26767f14384d9ff8d9b8f4"),
                },
                Helper.HexToBytes("271eafea9cfeb730c6fae8c39da387b37967646d26224a10878e04f3f6212fbe", true)
            };
            yield return new object[]
            {
                new ITransaction[11] // block #57286  11(->12) => 6 => 3(->4) => 2 => 1
                {
                    new MockTxIdTx("e17e4987fb4565e496c4751d44f52aca00eed2387379669f039bf04ae174048b"),
                    new MockTxIdTx("03a5497d96f8f39cdac8761e7bfa21049816378c6bdf331921c607093ec8474b"),
                    new MockTxIdTx("2c43eebedbd5529b4c9496f08d31d5d22aa7b061aa5efa6d45f1d8efe611150d"),
                    new MockTxIdTx("3d2a352ca353760d2d8fc2b10e9944789f1ea0ea2a8ca340fe7a426c1f5008cb"),
                    new MockTxIdTx("4a225d19c0bf7ed70433a8c9acfb239e160414e1ed0c6733c051111f624dd78b"),
                    new MockTxIdTx("6227326e6c5035ad1bc5c61c78c27223bfe5694c797bd974cf296e509162777f"),
                    new MockTxIdTx("9bf4cc2687cb85712e964f23bc48b8a90ede1b1a985e35ee7601af137631c3a2"),
                    new MockTxIdTx("b0c940c008ff0ee11c1babea4d92da387b563fdbb2b15a5f693567b44575dc36"),
                    new MockTxIdTx("b6eee3a4f98271224e205fcf56a76fc55bfffcabe1b0121348cbc038601338e2"),
                    new MockTxIdTx("b7d9686a8310881505e6853d13c4ae31dd52c925e729d3b4975120436e6ca56e"),
                    new MockTxIdTx("f651523681ff86796635cb0671bf01a5f35c41b1804d3f6cce903257bb41768e"),
                },
                Helper.HexToBytes("23d97ad1b6e828398aff13122e312883c47986e8c8a9d1f4042876fa2e9e1fe4", true)
            };
        }
        [Theory]
        [MemberData(nameof(GetMerkleCases))]
        public void ComputeMerkleRootTest(ITransaction[] txs, byte[] expected)
        {
            Block block = new Block()
            {
                TransactionList = txs
            };

            byte[] actual = block.ComputeMerkleRoot();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SerializeHeaderTest()
        {
            Block blk = GetSampleBlock();

            FastStream stream = new FastStream();
            blk.SerializeHeader(stream);

            byte[] expected = GetSampleBlockHeaderBytes();

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, blk.SerializeHeader());
        }

        [Fact]
        public void SerializeTest()
        {
            Block blk = GetSampleBlock();
            blk.TransactionList = new ITransaction[]
            {
                new MockSerializableTx(new byte[] { 1, 2, 3 }),
                new MockSerializableTx(new byte[] { 10, 20, 30 }),
                new MockSerializableTx(new byte[] { 255, 255 })
            };

            FastStream stream = new FastStream();
            blk.Serialize(stream);

            byte[] expected = new byte[80 + 1 + (3 + 3 + 2)];
            Buffer.BlockCopy(GetSampleBlockHeaderBytes(), 0, expected, 0, 80);
            expected[80] = 3; // Tx count
            expected[81] = 1;
            expected[82] = 2;
            expected[83] = 3;
            expected[84] = 10;
            expected[85] = 20;
            expected[86] = 30;
            expected[87] = 255;
            expected[88] = 255;

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, blk.Serialize());
        }

        [Fact]
        public void TryDeserializeHeaderTest()
        {
            Block blk = new Block();
            bool b = blk.TryDeserializeHeader(new FastStreamReader(GetSampleBlockHeaderBytes()), out string error);
            Block expected = GetSampleBlock();

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(expected.Version, blk.Version);
            Assert.Equal(expected.PreviousBlockHeaderHash, blk.PreviousBlockHeaderHash);
            Assert.Equal(expected.MerkleRootHash, blk.MerkleRootHash);
            Assert.Equal(expected.BlockTime, blk.BlockTime);
            Assert.Equal(expected.NBits, blk.NBits);
            Assert.Equal(expected.Nonce, blk.Nonce);
        }
    }
}
