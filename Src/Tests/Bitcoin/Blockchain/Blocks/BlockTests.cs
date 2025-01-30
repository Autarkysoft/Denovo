﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Bitcoin.Blockchain.Blocks
{
    public class BlockTests
    {
        [Fact]
        public void ConstructorTest()
        {
            BlockHeader expHdr = BlockHeaderTests.GetSampleBlockHeader();
            ITransaction[] expTxs = new ITransaction[2];

            Block blk = new(expHdr, expTxs);

            Assert.Equal(expHdr, blk.Header);
            Assert.Same(expTxs, blk.TransactionList);
        }

        [Fact]
        public void Constructor_NullExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new Block(new BlockHeader(), null));
            Assert.Throws<ArgumentNullException>(() => new Block(new BlockHeader(), Array.Empty<Transaction>()));
        }

        // TODO: find and add more tests for edge cases
        public static IEnumerable<object[]> GetSizeCases()
        {
            JArray jar = Helper.ReadResource<JArray>("Blocks");
            foreach (JToken item in jar)
            {
                string testName = item["Info"].ToString();
                Block block = new();
                FastStreamReader stream = new(Helper.HexToBytes(item["Hex"].ToString()));
                bool b = block.TryDeserialize(stream, out Errors error);
                Assert.True(b, $"{testName}{error.Convert()}");

                yield return new object[]
                {
                    testName,
                    block,
                    (int)item["TotalSize"],
                    (int)item["StrippedSize"],
                    (int)item["Weight"],
                };
            }
        }
        [Theory]
        [MemberData(nameof(GetSizeCases))]
        public void SizeTest(string testName, Block block, int totalSize, int strippedSize, int weight)
        {
            Assert.False(string.IsNullOrEmpty(testName));
            Assert.Equal(totalSize, block.TotalSize);
            Assert.Equal(strippedSize, block.StrippedSize);
            Assert.Equal(weight, block.Weight);
        }


        [Fact]
        public void GetBlockHashTest()
        {
            Block blk = new() { Header = BlockHeaderTests.GetSampleBlockHeader() };

            byte[] actual = blk.GetBlockHash();
            byte[] expected = BlockHeaderTests.GetSampleBlockHash();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBlockIDTest()
        {
            Block blk = new() { Header = BlockHeaderTests.GetSampleBlockHeader() };

            string actual = blk.GetBlockID();
            string expected = BlockHeaderTests.GetSampleBlockHex();

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
            Block block = new()
            {
                TransactionList = txs
            };

            Digest256 actual = block.ComputeMerkleRoot();
            Assert.Equal(expected, actual.ToByteArray());
        }


        public static IEnumerable<object[]> GetWitMerkleCases()
        {
            byte[] Commit0 = new byte[32];
            byte[] Commit1 = Enumerable.Repeat((byte)1, 32).ToArray();
            byte[] Commit255 = Enumerable.Repeat((byte)255, 32).ToArray();
            // SHA256("Witness Commitment Test")
            byte[] CommitRand = Helper.HexToBytes("1d505f2e8168d4fb2c8b163a6c54f80fa52c5e1de6361bba53a2af0b6a45ad25");
            Digest256 nBa = Digest256.Zero;
            yield return new object[]
            {
                 new ITransaction[1] // block #481828
                 {
                     // If Block calls GetWitnessTransactionHash on coinbase transaction the MockWTxIdTx will throw an exception
                     // it should never call that method, instead a byte[32] (all zero) should be used
                     new MockWTxIdTx(nBa)
                 },
                 Commit0,
                 // Merkle root copied from block's highest index output starting with 0x6a24aa21a9ed data using a block explorer
                 Helper.HexToBytes("e2f61c3f71d1defd3fa999dfa36953755c690689799962b48bebd836974e8cf9")
            };
            yield return new object[]
            {
                new ITransaction[2] // block #610784 (tx has no witness)
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("d6c900b378c39a125906d5beb1a0032f2f69c67e05dac0fb0ad1a7b51ebf9283"),
                },
                Commit0,
                Helper.HexToBytes("6e07e98e02f24198969280bf1248daf49e64c3792dbd7f92776080af59a85bc3")
            };
            yield return new object[]
            {
                new ITransaction[2] // block #557991 (tx has witness)
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("bea31d447fc019818137a7ab7ed33c3a131b50c4860faae55f05e1df566c3521"),
                },
                Commit0,
                Helper.HexToBytes("02a7c90fe9795d0af5206b29acea5111d03eb8219fff0a67577a5c9482441869")
            };
            yield return new object[]
            {
                new ITransaction[3] // block #542205  3(->4) => 2 => 1
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("c84bb5f71c134f00450a90b3822625dd9d8d507a724643b5bc1441d7fec06730"),
                    new MockWTxIdTx("e92905efb73409ecf56cc7729402d00807199316f38f00a5e4c6e30d88fb96ae")
                },
                Commit0,
                Helper.HexToBytes("7d69a85bb62a695bdff903264e505360d3badc9f7dd0488cafd6a53d6ee0e977")
            };
            yield return new object[]
            {
                new ITransaction[4] // block #619699  4 => 2 => 1
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("616dd6f2a1ccd43eb73666537eec1a31c2fcdfcb27987ba3594dd6eed713f97c"),
                    new MockWTxIdTx("9b68ec760e5a32e31e887f7148fd89f82262dd9b2168196b8ab56dea8fbc4990"),
                    new MockWTxIdTx("d960cc7e9912ee1f636f490cedf0afc8f935a69b37f5923d5070600144e10fc1")
                },
                Commit0,
                Helper.HexToBytes("0c6b3ef03fd460da49822111b81d4e264d325d71495e5434fedaa4412076fcb8")
            };
            yield return new object[]
            {
                new ITransaction[5] // block #528767  5(->6) => 3(->4) => 2 => 1
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("0b6969f0532acd2c0b6edf8ceba0cda4263e34bb74e126fa38ca0f743990d3b5"),
                    new MockWTxIdTx("dd31e005ada2d1c0974538a40e82aa5b015d5dfb1f206ab588f276b447abb20d"),
                    new MockWTxIdTx("2996921b19da93e5493e0fb3a5577ec01989c9b385c3c6a70e48320a5546a3da"),
                    new MockWTxIdTx("94b0a6335715ee927631c711c2c424e8d3fd2fbf1704e1d485542917a2282132"),
                },
                Commit0,
                Helper.HexToBytes("656ee0f12871bcd1516faa8034b8d77849d5cec944421aa44d5fe109a3bc2756")
            };
            yield return new object[]
            {
                new ITransaction[6] // block #541861  6 => 3(->4) => 2 => 1
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("36382f295fd496a189b994a4cf9056fe48fa2fd541e018dd0148269904484614"),
                    new MockWTxIdTx("52788762dc471098103ee0dacdff94eea4847cd45dbb3db9dfaa0a2d97fac5fd"),
                    new MockWTxIdTx("d9c00d28537b9998914cb6fadf7ecc8fe8afbe54a67378ce7c560cf08e864520"),
                    new MockWTxIdTx("daa91bc760d6d1a4878300ed680cfb12a57eecb6bd18d036e7009e7577dcd7bc"),
                    new MockWTxIdTx("340d0e8e5bfcdb703b752bfb1cc1f2076e192cdaadd3660a02bf60c4431c252c"),
                },
                Commit0,
                Helper.HexToBytes("c9e9e34416703ef71ae09d0d1e4d5a824ac8a2d10e66e164661df2267f5f2d97")
            };
            yield return new object[]
            {
                new ITransaction[7] // block #538950  7(->8) => 4 => 2 => 1
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("b4fd245a2c45675562546c74d564b8a843b7a10f9b8ac2d5b5939fb817e8b934"),
                    new MockWTxIdTx("902294c21afa1a00f3d01c0e025f6a840ead5fc31135d83ad5bbbc2bcc0cd4b2"),
                    new MockWTxIdTx("e7dce4a3e2a958d41bda0e840fca1c4200a8710f0f51df7aa03aff107d7eef90"),
                    new MockWTxIdTx("547c5115c3d25c6666743e4d5349cd4135c9ea5d9bc563abfa7276a670d213b7"),
                    new MockWTxIdTx("10e56b8945be140a64ce53cb7f11588085cce11d4da3ee9dc90913677497433b"),
                    new MockWTxIdTx("508b7aeeb896797cb8cfca9de87e874f6e8d58a085f137a1fcbf29aacfac2eab"),
                },
                Commit0,
                Helper.HexToBytes("8b4a4ec22ff353f6cdaef2d1632690599be00c64a7a198219a63c26abaa0404d")
            };
            yield return new object[]
            {
                new ITransaction[8] // block #545098  8 => 4 => 2 => 1
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("b921f5f55e8e5b2f3fa95bb5246a6a825e6ac92e4f87dcd796cb054a2f9393e1"),
                    new MockWTxIdTx("722c5109690a894bf41ed61186c388a09d85492de4de93f3f2fcb50ed4c979f7"),
                    new MockWTxIdTx("56ee79fce2bc6fd4ff491ee371b23f66f68b67ccc2b57bd3fd4fccc21f8442f7"),
                    new MockWTxIdTx("2be891b00842e2c665c08e4d839d5543d4d0b9974948c9d234dce38a28dab531"),
                    new MockWTxIdTx("b03c796d535bbc7b363bf52fe9f005489cf1b63aad939a155a88c6386eb37790"),
                    new MockWTxIdTx("3610d21802b13eb21bca8301e735f3e056ea179ca9a81c6f8ec32b3c99b2b5b2"),
                    new MockWTxIdTx("46d9b50bbaa6a1aa384347541ba4beff96ffcc6b69b9542fadd2d9578a531bd6"),
                },
                Commit0,
                Helper.HexToBytes("03f33ee4bc04703ebe27df891f3a695c3e6fcda1c7bfc9007e56be9949b1b408")
            };
            yield return new object[]
            {
                new ITransaction[11] // block #486828  11(->12) => 6 => 3(->4) => 2 => 1
                {
                    new MockWTxIdTx(nBa),
                    new MockWTxIdTx("1527d8af6ed673615333c9440e933866327bffb656bd0c82222cdcdbddfb75e0"),
                    new MockWTxIdTx("ffe4cd908f03b209775c853c08681f6b25d618d7210bb993ff723090b8bb957a"),
                    new MockWTxIdTx("8fca64173b40f2472565f9e00982a75edfbc098257fa9e14c0bad525b86fe476"),
                    new MockWTxIdTx("ab822e2b5443d8decca13bc9d17939a5c2b959b5fea418dbc43a79296e2db2f6"),
                    new MockWTxIdTx("777046e0571e172a92c4605b68352c8fb3528d861b719cb06ccca8b8e03a1e3d"),
                    new MockWTxIdTx("c3add815275ae4306566d193148a9ff9654466c89b0e2d0c1e04c3b69dad4140"),
                    new MockWTxIdTx("28e0df2abd103d552b1a05d7ad5ab3cf1c5665b95fc3dacdc2bc57c69919c35f"),
                    new MockWTxIdTx("d82bc6f392ef9dd5d74f8b37b4d001059b15588669607a1a9d9ddce08e9f2453"),
                    new MockWTxIdTx("4773295e87e5bbff4c50c0e0770a42c046922e77319ec64d6269ac4c70875285"),
                    new MockWTxIdTx("ec61ba60120e4cad1ab1029bb83981ad7573ce8396b554382a35ff8c678d98a3"),
                },
                Commit0,
                Helper.HexToBytes("73499f5bfa0338e3683100e993f496a93352fee3bcd0f7fe5b161a4393aa241a")
            };

            // The following blocks are found on TestNet3 and are mined using Denovo each with a custom witness commitment
            yield return new object[]
            {
                 new ITransaction[1] // block #1836793 on testnet
                 {
                     new MockWTxIdTx(nBa)
                 },
                 Commit1,
                 Helper.HexToBytes("705ede9d42476fc3e5a978b042ce790a193678f46d19f47ec4ab46539c47b76d")
            };
            yield return new object[]
            {
                 new ITransaction[1] // block #1836806 on testnet
                 {
                     new MockWTxIdTx(nBa)
                 },
                 Commit255,
                 Helper.HexToBytes("4fa4a869878bd4837e6192377293a0cc14f192b88711635ca3df0447dc1bea76")
            };
            yield return new object[]
            {
                 new ITransaction[3] // block #1836819 on testnet
                 {
                     new MockWTxIdTx(nBa),
                     new MockWTxIdTx("3fa93b79ee6e7e0280e8303393b725b0211b0f7e6eb144359677173841255e09"),
                     new MockWTxIdTx("f260a173cd9d57875d9a89b70abfcbd619c794e5c63ec10feb488921f4293c17"),
                 },
                 Commit255,
                 Helper.HexToBytes("14a32ab00fa19ffc3ad13f160e3361b47b7420f12baf5c41d412e916eee8d94f")
            };
            yield return new object[]
            {
                 new ITransaction[3] // block #1836832 on testnet
                 {
                     new MockWTxIdTx(nBa),
                     new MockWTxIdTx("34199146aa700ecac71c8da751f6d211906a37313c7cd85dd94f845ecdca428b"),
                     new MockWTxIdTx("827ea1784b8b18691d103fe0c42a1c40dbd5337f9f692460bbbac44a16b3dd34"),
                 },
                 CommitRand,
                 Helper.HexToBytes("848fdbeef5571a41ae2520a9cb5a877b61e9df72d14fe7163913b2d3665884fa")
            };
        }
        [Theory]
        [MemberData(nameof(GetWitMerkleCases))]
        public void ComputeWitnessMerkleRootTest(ITransaction[] txs, byte[] commitment, byte[] expected)
        {
            Block block = new()
            {
                TransactionList = txs
            };

            Digest256 actual = block.ComputeWitnessMerkleRoot(commitment);
            Assert.Equal(expected, actual.ToByteArray());
        }


        [Fact]
        public void SerializeTest()
        {
            Block blk = new()
            {
                Header = BlockHeaderTests.GetSampleBlockHeader(),
                TransactionList = new ITransaction[]
                {
                    new MockSerializableTx(new byte[] { 1, 2, 3 }),
                    new MockSerializableTx(new byte[] { 10, 20, 30 }),
                    new MockSerializableTx(new byte[] { 255, 255 })
                }
            };

            FastStream stream = new(90);
            blk.Serialize(stream);

            byte[] expected = new byte[80 + 1 + (3 + 3 + 2)];
            Buffer.BlockCopy(BlockHeaderTests.GetSampleBlockHeaderBytes(), 0, expected, 0, BlockHeader.Size);
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
        public void SerializeWithoutWitnessTest()
        {
            Block blk = new()
            {
                Header = BlockHeaderTests.GetSampleBlockHeader(),
                TransactionList = new ITransaction[]
                {
                    new MockSerializableTx(new byte[] { 1, 2, 3 }, true),
                }
            };

            FastStream stream = new(84);
            blk.SerializeWithoutWitness(stream);

            byte[] expected = new byte[80 + 1 + 3];
            Buffer.BlockCopy(BlockHeaderTests.GetSampleBlockHeaderBytes(), 0, expected, 0, BlockHeader.Size);
            expected[80] = 1; // Tx count
            expected[81] = 1;
            expected[82] = 2;
            expected[83] = 3;

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, blk.SerializeWithoutWitness());
        }


        public static IEnumerable<object[]> GetDeserCases()
        {
            yield return new object[]
            {
                // Block #19
                Helper.HexToBytes("01000000161126f0d39ec082e51bbd29a1dfb40b416b445ac8e493f88ce993860000000030e2a3e32abf1663a854efbef1b233c67c8cdcef5656fe3b4f28e52112469e9bae306849ffff001d16d1b42d0101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff0704ffff001d0116ffffffff0100f2052a01000000434104f5efde0c2d30ab28e3dbe804c1a4aaf13066f9b198a4159c76f8f79b3b20caf99f7c979ed6c71481061277a6fc8666977c249da99960c97c8d8714fda9f0e883ac00000000"),
                new string[] { "9b9e461221e5284f3bfe5656efdc8c7cc633b2f1beef54a86316bf2ae3a3e230" },
                215
            };
            yield return new object[]
            {
                // Block #19
                Helper.HexToBytes("00e0002091921d693f92703ba351c50c0a1751da84fe90b2102b0a000000000000000000e3385bebd6d2913298ccb95771292dca37cb4981e69e2ab583629efc88ff730eb3a09a5ebc20131754b578560301000000010000000000000000000000000000000000000000000000000000000000000000ffffffff5f034b8f09047ba09a5e2f706f6f6c696e2e636f6d2ffabe6d6d2b37576a52671625b0a352b2d646dfb4051152ba3c82d6953302d43fbf08265c0100000000000000ee6d828d00b7412bbccb13ed180f4eba1091558621001300000000000000ffffffff03807c814a0000000017a91454705dd010ab50c03543a543cda327a60d9bf7af870000000000000000266a24b9e11b6d68d05f595165b3f89acda3863f4f07756994e3d7125383506b2c242afc74f63500000000000000002b6a2952534b424c4f434b3a342421e6df124adc350ceb831ea10068c8f16a2f693161ac94129a170022d4cafcb0be5c02000000011629d83d52d711558fdc05fc38875296f34fa36b9c0910bcf2de2d19a57a0e25480000006a47304402204f5d3be59b8f473e09fcc6fe941018b345e860feca178e7a5f4ec20ce7b6cfe3022001f791f95fbd1192fc778b25e478331c31a75b42823a4d8c4b56d72c288818f1012102573239c0bcf5a67afc4bfcdd114a9589ddd8f50e360ae4ee29cc5c4cb62343a1ffffffff01b80b0000000000001976a9144381d5eecfc6c66597621c8a86405b97c913404488ac0000000002000000011629d83d52d711558fdc05fc38875296f34fa36b9c0910bcf2de2d19a57a0e25490000006a47304402202137f9a7796d635717769c04c14e786e0b78b4e2a979818022a61af3545ba69b02200eeacae2827e13584e61ec55377b49720f75393fecc1b6fe240586353e772d0101210392f47b2b822369b978477ca4de231f5eb77e995532bef6d39cf568f45ed6f2f6ffffffff01b80b0000000000001976a914838dcf13cb5fa9bf9d8f2ec792b4306d81408f3b88ac00000000"),
                new string[]
                {
                    "629509d4018a810f7af1e77d5abc5051c09e5b0df9552ca8ab329e0fa5b317cd",
                    "13618bac55c04704269602e2af1701cd7e1a03617a3f8855ec8f2a4240916961",
                    "75cb5ec8437b313d7201d7e25bb00d971c554069bdea6c06805314d8b3f14470"
                },
                740
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserCases))]
        public void TryDeserializeTest(byte[] data, string[] expTxIds, int expBlockSize)
        {
            Block blk = new();
            bool b = blk.TryDeserialize(new FastStreamReader(data), out Errors error);

            Assert.True(b, error.Convert());
            Assert.Equal(Errors.None, error);
            Assert.Equal(expBlockSize, blk.TotalSize);
            // A quick check to make sure transaction is correct
            Assert.Equal(expTxIds.Length, blk.TransactionList.Length);
            for (int i = 0; i < blk.TransactionList.Length; i++)
            {
                Assert.Equal(expTxIds[i], blk.TransactionList[i].GetTransactionId());
            }
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            byte[] hdba = BlockHeaderTests.GetSampleBlockHeaderBytes();

            yield return new object[]
            {
                new byte[BlockHeader.Size -1],
                Errors.EndOfStream
            };
            yield return new object[]
            {
                Helper.ConcatBytes(BlockHeader.Size + 1, hdba, new byte[] { 253 }),
                Errors.ShortCompactInt2
            };
            yield return new object[]
            {
                Helper.ConcatBytes(BlockHeader.Size + 5, hdba, new byte[] { 0xfe, 0x00, 0x00, 0x00, 0x80 }),
                Errors.TxCountOverflow
            };
            yield return new object[]
            {
                Helper.ConcatBytes(BlockHeader.Size + 1, hdba, new byte[] { 1 }),
                Errors.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTests(byte[] data, Errors expErr)
        {
            Block blk = new();
            bool b = blk.TryDeserialize(new FastStreamReader(data), out Errors error);

            Assert.False(b);
            Assert.Equal(expErr, error);
        }


        [Fact]
        public void Block481759_Test()
        {
            // This is one of the biggest bitcoin block before SegWit fork that has the maximum possible size of 1MB
            string hex = Helper.ReadResource("Block481759", "txt");
            Block block = new();
            FastStreamReader stream = new(Helper.HexToBytes(hex));
            bool b = block.TryDeserialize(stream, out Errors error);
            Assert.True(b, error.Convert());

            Assert.Equal(2127, block.TransactionList.Length);
            Assert.Equal(1000000, block.TotalSize);
            Assert.Equal(1000000, block.StrippedSize);
            Assert.Equal(4000000, block.Weight);
            Assert.Equal("0000000000000000006bbb7fb15eaea51703fae52457d37a612a6bce220ab607", block.GetBlockID());
            Assert.Equal(block.Header.MerkleRootHash, block.ComputeMerkleRoot());
        }

        [Fact]
        public void Block894090_Test()
        {
            // This is the biggest bitcoin block to this date (2022-02-23) and is on TestNet3
            string hex = Helper.ReadResource("Block894090", "txt");
            Block block = new();
            FastStreamReader stream = new(Helper.HexToBytes(hex));
            bool b = block.TryDeserialize(stream, out Errors error);
            Assert.True(b, error.Convert());

            Assert.Equal(468, block.TransactionList.Length);
            Assert.Equal(3876524, block.TotalSize);
            Assert.Equal(38843, block.StrippedSize);
            Assert.Equal(3993053, block.Weight);
            Assert.Equal("00000000000016a805a7c5d27c3cc0ecb6d51372e15919dfb49d24bd56ae0a8b", block.GetBlockID());
            Assert.Equal(block.Header.MerkleRootHash, block.ComputeMerkleRoot());
        }
    }
}
