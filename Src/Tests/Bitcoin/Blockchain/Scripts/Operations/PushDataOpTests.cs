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

namespace Tests.Bitcoin.Blockchain.Scripts.Operations
{
    public class PushDataOpTests
    {
        [Fact]
        public void Constructor_DefaultTest()
        {
            PushDataOp op = new PushDataOp();
            FastStream stream = new FastStream();
            op.WriteToStream(stream);

            byte[] actual = stream.ToByteArray();
            byte[] expected = new byte[1] { 0 };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_FromBytesTest()
        {
            byte[] data = { 1, 2, 3 };
            PushDataOp op = new PushDataOp(data);

            // Make sure data is cloned
            data[0] = 255;

            Helper.ComparePrivateField(op, "data", new byte[] { 1, 2, 3 });
            Assert.Equal(3, (byte)op.OpValue);
        }

        [Fact]
        public void Constructor_FromBytes_NullExceptionTest()
        {
            byte[] data = null;
            Assert.Throws<ArgumentNullException>(() => new PushDataOp(data));
        }

        [Fact]
        public void Constructor_FromBytes_ArgumentExceptionTest()
        {
            byte[] data = { 1 };
            Assert.Throws<ArgumentException>(() => new PushDataOp(data));
        }

        [Fact]
        public void Constructor_FromBytes_OutOfRangeExceptionTest()
        {
            byte[] data = new byte[Constants.MaxScriptItemLength + 1];
            Assert.Throws<ArgumentOutOfRangeException>(() => new PushDataOp(data));
        }


        [Fact]
        public void Constructor_FromScriptTest()
        {
            byte[] data = { 1, 2, 3 };
            MockSerializableScript scr = new MockSerializableScript(data, 255);
            PushDataOp op = new PushDataOp(scr);

            // Make sure data is cloned
            scr.Data[0] = 255;

            Helper.ComparePrivateField(op, "data", new byte[] { 1, 2, 3 });
            Assert.Equal(3, (byte)op.OpValue);
        }

        [Fact]
        public void Constructor_FromScript_NullExceptionTest()
        {
            IScript scr = null;
            Assert.Throws<ArgumentNullException>(() => new PushDataOp(scr));
        }

        [Fact]
        public void Constructor_FromScript_OutOfRangeExceptionTest()
        {
            byte[] data = new byte[Constants.MaxScriptItemLength + 1];
            MockSerializableScript scr = new MockSerializableScript(data, 255);
            Assert.Throws<ArgumentOutOfRangeException>(() => new PushDataOp(scr));
        }


        [Theory]
        [InlineData(OP._0)]
        [InlineData(OP.Negative1)]
        [InlineData(OP._1)]
        [InlineData(OP._2)]
        [InlineData(OP._3)]
        [InlineData(OP._4)]
        [InlineData(OP._5)]
        [InlineData(OP._6)]
        [InlineData(OP._7)]
        [InlineData(OP._8)]
        [InlineData(OP._9)]
        [InlineData(OP._10)]
        [InlineData(OP._11)]
        [InlineData(OP._12)]
        [InlineData(OP._13)]
        [InlineData(OP._14)]
        [InlineData(OP._15)]
        [InlineData(OP._16)]
        public void Constructor_FromOpNumTest(OP val)
        {
            PushDataOp op = new PushDataOp(val);
            Assert.Equal(val, op.OpValue);
        }

        [Theory]
        [InlineData((OP)1)]
        [InlineData((OP)0x4b)]
        [InlineData(OP.PushData1)]
        [InlineData(OP.PushData2)]
        [InlineData(OP.PushData4)]
        [InlineData(OP.Reserved)]
        [InlineData(OP.NOP)]
        public void Constructor_FromOpNum_ExceptionTest(OP val)
        {
            Assert.Throws<ArgumentException>(() => new PushDataOp(val));
        }


        [Theory]
        [InlineData(0, OP._0)]
        [InlineData(-1, OP.Negative1)]
        [InlineData(1, OP._1)]
        [InlineData(2, OP._2)]
        [InlineData(3, OP._3)]
        [InlineData(4, OP._4)]
        [InlineData(5, OP._5)]
        [InlineData(6, OP._6)]
        [InlineData(7, OP._7)]
        [InlineData(8, OP._8)]
        [InlineData(9, OP._9)]
        [InlineData(10, OP._10)]
        [InlineData(11, OP._11)]
        [InlineData(12, OP._12)]
        [InlineData(13, OP._13)]
        [InlineData(14, OP._14)]
        [InlineData(15, OP._15)]
        [InlineData(16, OP._16)]
        public void Constructor_FromInt_HasOpNum_Test(int i, OP expected)
        {
            PushDataOp op = new PushDataOp(i);
            Assert.Equal(expected, op.OpValue);
        }

        [Theory]
        [InlineData(17, new byte[] { 17 }, (OP)1)]
        [InlineData(75, new byte[] { 75 }, (OP)1)]
        [InlineData(128, new byte[] { 128, 0 }, (OP)2)]
        [InlineData(256, new byte[] { 0, 1 }, (OP)2)]
        [InlineData(-2, new byte[] { 0b10000010 }, (OP)1)] // 0b10000010 = 0x82 = 130
        [InlineData(-8388607, new byte[] { 255, 255, 255 }, (OP)3)]
        public void Constructor_FromInt_Test(int i, byte[] expectedBa, OP expectedOP)
        {
            PushDataOp op = new PushDataOp(i);

            Helper.ComparePrivateField(op, "data", expectedBa);
            Assert.Equal(expectedOP, op.OpValue);
        }


        public static IEnumerable<object[]> GetRunCases()
        {
            yield return new object[] { new PushDataOp(0L), new byte[0] };
            yield return new object[] { new PushDataOp(-1), new byte[] { 0b10000001 } };
            yield return new object[] { new PushDataOp(1), new byte[] { 1 } };
            yield return new object[] { new PushDataOp(2), new byte[] { 2 } };
            yield return new object[] { new PushDataOp(16), new byte[] { 16 } };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2, 3 }), new byte[] { 1, 2, 3 } };
        }
        [Theory]
        [MemberData(nameof(GetRunCases))]
        public void RunTest(PushDataOp op, byte[] expectedData)
        {
            MockOpData opData = new MockOpData(FuncCallName.Push)
            {
                _itemCount = 0,
                _altItemCount = 0,
                pushData = new byte[][] { expectedData }
            };

            bool b = op.Run(opData, out string error);

            // The mock data is already checking the call type and the data that was pushed to be correct.
            Assert.True(b);
            Assert.Null(error);
        }

        [Fact]
        public void Run_ItemCountOverflowTest()
        {
            PushDataOp op = new PushDataOp(OpTestCaseHelper.b1);
            MockOpData opData = new MockOpData(FuncCallName.Push)
            {
                _itemCount = 1001,
                _altItemCount = 0,
                pushData = new byte[][] { OpTestCaseHelper.b1 }
            };

            bool b = op.Run(opData, out string error);

            Assert.False(b);
            Assert.Equal(Err.OpStackItemOverflow, error);
        }

        [Fact]
        public void Run_ItemLenghOverflowTest()
        {
            PushDataOp op = new PushDataOp();
            byte[] data = Helper.HexToBytes($"4d0902{Helper.GetBytesHex(521)}");
            bool b1 = op.TryRead(new FastStreamReader(data), out string error);
            Assert.True(b1, error);
            MockOpData opData = new MockOpData();

            bool b2 = op.Run(opData, out error);

            Assert.False(b2);
            Assert.Equal("Item to be pushed to the stack can not be bigger than 520 bytes.", error);
        }


        [Theory]
        [InlineData(new byte[] { 17 }, 17)]
        [InlineData(new byte[] { 128, 128 }, -128)]
        public void TryGetNumberTest(byte[] data, long expected)
        {
            PushDataOp op = new PushDataOp(data);

            bool b = op.TryGetNumber(out long actual, out string error);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(OP._0, 0)]
        [InlineData(OP.Negative1, -1)]
        [InlineData(OP._1, 1)]
        [InlineData(OP._2, 2)]
        [InlineData(OP._16, 16)]
        public void TryGetNumber_NullDataTest(OP val, long expected)
        {
            PushDataOp op = new PushDataOp(val);

            bool b = op.TryGetNumber(out long actual, out string error);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryGetNumber_FailTest()
        {
            PushDataOp op = new PushDataOp(Helper.GetBytes(9));

            bool b = op.TryGetNumber(out long actual, out string error);

            Assert.False(b);
            Assert.Equal("Invalid number format.", error);
            Assert.Equal(0, actual);
        }

        [Fact]
        public void TryGetNumber_SpecialCaseTest()
        {
            PushDataOp op = new PushDataOp();
            FastStreamReader stream = new FastStreamReader(new byte[2] { 1, 2 });
            bool didRead = op.TryRead(stream, out string error);

            Assert.True(didRead, error);
            Assert.Null(error);

            // The value is 2 but didn't use OP_2, instead it used byte[] { 2 }
            bool b = op.TryGetNumber(out long actual, out error, isStrict: false);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(2, actual);
        }


        public static IEnumerable<object[]> GetReadCases()
        {
            // 0x4c4d.... => script (0x4c4d = StackInt len) len-value=77
            // 0x4c4d.... => witness (0x4c = CompactInt len) len-value=76
            byte[] data77 = Helper.GetBytes(2 + 77);
            data77[0] = 76; // set StackInt(77) first byte will also be CompactInt(76)
            data77[1] = 77;
            byte[] exp77 = new byte[77];
            Buffer.BlockCopy(data77, 2, exp77, 0, 77);
            byte[] expWit77 = new byte[76];
            Buffer.BlockCopy(data77, 1, expWit77, 0, 76);

            yield return new object[] { new byte[1], true, null, OP._0 };
            yield return new object[] { new byte[1], false, null, OP._0 };
            yield return new object[] { new byte[] { 0x4f }, true, null, OP.Negative1 };
            yield return new object[] { new byte[] { 0x4f }, false, null, OP.Negative1 };
            yield return new object[] { new byte[] { 0x51 }, true, null, OP._1 };
            yield return new object[] { new byte[] { 0x51, 2 }, false, null, OP._1 };
            yield return new object[] { new byte[] { 0x60 }, true, null, OP._16 };
            yield return new object[] { new byte[] { 0x60 }, false, null, OP._16 };

            yield return new object[] { new byte[] { 1, 0 }, true, new byte[1], (OP)1 }; // not strict
            yield return new object[] { new byte[] { 1, 0 }, false, new byte[1], (OP)1 }; // not strict
            yield return new object[] { new byte[] { 1, 5 }, true, new byte[1] { 5 }, (OP)1 }; // not strict
            yield return new object[] { new byte[] { 1, 3, 4 }, false, new byte[1] { 3 }, (OP)1 }; // not strict

            yield return new object[] { data77, true, expWit77, (OP)0x4c }; //0x4c=OP.PushData1=76
            yield return new object[] { data77, false, exp77, OP.PushData1 };
        }
        [Theory]
        [MemberData(nameof(GetReadCases))]
        public void TryReadTest(byte[] data, bool isWit, byte[] expData, OP expOP)
        {
            PushDataOp op = new PushDataOp();
            FastStreamReader stream = new FastStreamReader(data);
            bool b = isWit ? op.TryReadWitness(stream, out string error) : op.TryRead(stream, out error);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(expOP, op.OpValue);
            if (expData != null)
            {
                Helper.ComparePrivateField(op, "data", expData);
            }
        }

        [Fact]
        public void TryRead_DataExceedsLimitTest()
        {
            PushDataOp op = new PushDataOp();
            // PubkeyScript of Tx ID on TestNet: 88bac1e84c235aa0418345bf430fb43b336875974b6e87dc958de196f9222c35
            // The significant of this test case is that the following PushDataOp exceeds both item length (520) 
            // and script length (10,000) bytes but is considered valid!
            string dataHex = "5b2c6c127e14007ada947a7bbacadcc212e9002c47ba4a1309665f99d019169839cc72de623427903ac52691c8e4fc815452ed879935baa1e1590817b466aac07ee242ee57fe60ddc270f18193b84a2569de457e2b8efe0411d1fd082b0fdbd41b6513e5a20e2956f10fc52843380d280ee3aed6212b95efe33ddc850057b1b2951d652ea565835bb4ee8c1bc9d6c1d9c3d5fa486105b2f0d024016431a355995d129326d2df83a95f5221329f79250d7c03c22b5d6951906a755e80631e291099f2bdb6429a06e45150e80ee37f5057fff3e82ee1293cf4c103bc5c81024540291099acbb6fcbe1fcded90c8042acea2507816f36fb2f6fb6c9f43fdea9634f0aa8e5d2e6a49b8f28063103502f747e57d1377a81d60a2de12af6f27a8b03537b8a01d85dd904816bf14630ca2b1e865476e80c72e8ce349eb7f0bbada9159d03de88c52e9c217dcdb60e8f8fda63b50b709d04a3f6e092bccd3aa6e848d3c1dba92deca35957b8546a3fbcd93ea4c4543baf3c0c11f90cfdf50861617b9484e24cb3e6ac7712b218e4af38b624b87e287652ad9896b5caaf20eeae50f656d9d5bad98d8abe17dc29fd242762c155d93d8a38eb29e7eb4b97e9368c896361218dd562fc4a2bf36a5f75849b728d67eecb1570c5bd6bb0eecf8ef5a0878a53c59c029a8c5e598e1197ce8cb36250d9098248c173f39b642225742dbc3552e2d421bffe70f2db9071865e2eefdefc8b7eb4ec1e3ec9dfd1009e7866ea2cd1dc45ecb891a3526916a6b84fed946d754329e8ae074933a30b42edfdd17a74a545b482e902266ae82b4bd5788679f23d9329343d017dca921148ad4fb554979c31120a381283721dbe0083bc74bb1d914fb069a84537a4c7eaefd7ba791a32d7b43846736eb92b10b6177e01fa0d1e562b1bff05eaee57fa6e434e22acb7a4a4cdab083b74a923968459f12db9a2cddcd1818f5e1a2cecc5e14c052b6b0863ff73da334e05e2e181f7698191fd2940cb7571512a284589a0bf425928a4f72a7bf5f2badf6f7ed7ee9f0d66b1ab5bc8b80b3ab5fab738d62d50928fc288b9dd003e059b0a6e615cf79a638b1bdfd7f5637aa11c694130c4bd1c8c8edcef1465e5306573a6c0bd49aadebe5f54adbb4d799d2b48e79d73be0d41ddeee1d37f0886fda5ea79644ea3bc57728b83d06f0f603f143ec3cb7d48bca76564e2cf67d65eb6d95607d0d3144e878b8f2be83a3dd1ce8da2be4144ba7f5d2ed2ffe9c1edef9b2dcdf61808906bffef3b40ed1cd46c28931441c5171c711fd6aaa1011564c69da08fba6d46b8c5fc37bf51087a92405a65d656adab2a84105bd7425eee69e98a75a24d8c19ffe2da4e458d58ca68a44469479c3e0e18f2750baa116e76670dcdd5c793d844b34166f47c5e669d2b45f333fdf4a7cdab7c5cafd8816fed7363e737c08303d5cc00b506354a91ba8bd8e0641e3b797266002a4a36286b32d1788019772fdda8421e67fc3fa19760eaa22400a00bcf38e974d1bf58b89ea3948334bdc978425cfa372200d47c33c468c96094046b91c6a9482f035e1d4f897fc367bc6dda97725fc70569e9ce646c400c578f58099e29f16d1380483ffedde79880101477578e31ed343d6528dddae3c2c251c5c03176253ecaffc6162a20c92781504e5458c9b5c95d6bd461c4c890399d857cac18276609ebd6ac57c71ac214733da68c8c8f6af99bfc62c6a1c379026f8d79a5a41c0f5ae94469811c5e0143a8c0ed862103ad0d1a82101bc4a068275daae73aa01d6f562d4ca24726e9dd3dfb1f1de0d5f2e06bdf6b8a210cf49549dc42237176c6a28a519758d56065e730a5d220a58287516aca7b5b34934e4a39b7596be4c56bc7347c938f3e990b0b7a84597b39d3efc654f0b9a7fa6b6df7024c1e1d07fa036b3a0beb29eff712d0bf70e0b039d21166c89316f30cc4ccc90e9c8df27aed037f80ee412de7c6d65cdee9e8898abcbd2f3bb85574e3105f753e221a536a2055d16c60304b91e37e02bcb91523dc433f52df5db6c2988f9c6f8fcbefc5ab83f1bd93161c298c719e583953b829fb3b99fbe2c8740d699338dcc4c0bcc6f4b95638c7578fc52615c56dbf5e8845d772745331f3e2e30fe45300de4064ccb4d86380fc7f61a4acd4d5cd02064d0b0af305524b699400d67329e3038fe6e6b0f8b8e7fa07485810d0a65d1e15e0089647f5ea3b12804b4363aa89567ea01dedef1c094d661103aaac417bc236e2c192204cbb99fbab9e786da3965dbf01922f896f2ff8d6ba3036f2408ed105880d81e9f73631e583f1437d91e0c37aa6ebabaf4b39c6b2c8bb651c85a7bb1ccd73e4961574c8cb604a057eeb7a44da6dbb4c55e221b94f46ce203bd1155fdc3626b01efb2c653223df4e003348c74d768aafc95a6639abb9954f47538cfa4bea2848c4ca26de40561a3103d3b91bfe07d3c446659581cfec056566dd1dde96e503150a00028250ac6f73403436d8ff7950407fc8b4e555a778d7c5c07762383b9b1dcfa311fe04507399f622419bb62c65ed8ddd5fce311dc063872abd8f340c3b059818085c68513b2e541aef3265dc21985b89d55bdbcdecb2e2ad76d992e464e9e73b029d2408853ba13079162d8dfeab092533ab37df29cb2c557abbdb619c6fb5054b29da847452049abaab53d68454afcbd06db0b9b207123671ce8eca603f940a602841d4d084b1e1a69d257d7855d0250b0f912ebffc534417a313a5c33a397a1749755c13dbfa4cbccf1314e194be1bfd69be51ae654a64d4fdbc2d710b1d6c63ab662fe0741230144572f0e93e6282e4f8a75a7be8ccb1fe1088151eccdab0b1a6b1362e8f3a89578d3c69f35a7b959a3a497963c3f17adeb530cfebfc12476ac13e428d2c5455bd1e02edab62bda395f37be383614ceeb53657785e6a9e5b7bf5fe321c38076ee6f4bb45c003ce7bf88d3af8802ec309744f8d56d4dbf65280a772b6648a865ceb0dfd8fa5072eb167db80d030a5db4c31f5becdabb0990afd69c5199018176d06dd0d723134636d21dbeff85a2621dacc26846eccb9b03a73f2923c4114214ec313ca5298681afd9250accfc1a0afd747ea4d6994a9a14d2dd3ff3ea2272b0cbaa034475bcf42630c86db7b0de178d342baf556c7b7ba1a717c25c569db5b6b249f15216c687b77a8e95d0dcd25404b5a2cfbe732d54503e61ad5dfa97dfa302b70566dcb5766da3f7daddd59ac4c4c3cd96a3de073384f5a123b51929cce2bee2de25dced38002c520a7b52767a939ce89a2d012bd50b7370712b91c1548a3af82ef2cc9ee7d69596877bfcb8328f368018308f9fe1830dd2a25c7c0c7e6afd80406a4787d6fdf0c3876aa4690ecb4df1e501a9240a9b169be4f9fc9b5a89281c7e6c77d837beb91a3fcc0876eea4c3e71e8a276cb1d985a553ab92ed0155690e4287eaa24365ba36488ee43e829fbd4026058cc061bb6aa4e6c2848c1bdc18da530a2456ca21a3b8dfe12422f54712b3fe52b6fbd8989c872d45bb70d13e2a7c7609db786b56860461d3f0809737af9fafd08dc9251d01ba7d9d39746622d791c6b7bceb6b9a48a7dca28a04a81bfff5961bc0f0a061575908210ace2d01ae4efc6ec9ef8ac4340587e5ec72830d1251ad43ee4a51d6e1656053e34a9f7a7adb044b084952106ad1e7bb78ca74d4c5fb0ea920d91353fd257d647f26d0e9761d6b117e47f4d90f8cd91c62966e26e08b8de83be65991b7a05de2cbfd9133f0fe455b5f63668e0635a9940006f156b75673f398f8bd9fc3c959cdfa120d11eb951b645102c78d250fe1f64cb591b0bef72b6579ad9b0840a5979609a893e42887b86aeb8d05a49ef6de9916b7531e37990ef2e697a84dfa1323ab7b21738c7bfdcdb50d26d521f186d682df01f18952896e609990fd80ef6a60c8695a396d73521d18e99e1100c3ed016fc1157ef8219a58e1c312e2db003cd592b60cb8974d791f7cf4e89fc4785dacac9f8dcfc9eb78a6df01479a606f3208acb87e4ac7246a224805f1f348fa84796576364b726d0bb73d65e98605debf1db2dfe30c0a12d588356b87c3e9855de47cce24fa28f6863fbcb70d514ae41c6956d4407907887f5290be1616e537cdb808050937b9c4f4cc49a35f4305844d415f1e7d0544e4dcf45ed2596ae07f5a74f9454945dad4bf81df742f54393c4d0c70c4c7d6cab873232814e8d76b99ebda58da59c03af7767a5eeb25c0c588bb22855cfa1bb783ba30c2261dc5ebd826459854fb5ed28cd1795e4d77df82b25daf59c6421eafc022d52028fe04d9a08e76b4e038708d5c40087b80cd6a0bdaae1fc87e7c259617a5c832263eb98ecc9fadbef11aa289ed11fd8649932f02b8c9557aee4c6b3468e0ad3a604e672d8689bee11fbb1abffb95db6c7eaf5c8486c5795ce93e9080347946007d6819aebadfffbfd43de30dbe958f5860feb5c99418b3aac51a4447e93ebca6650a32837c3cb1aeaf9ca058e27b9e43bf481f35c38c7c54b47ee9908ba7d6a5714c98a452fbdc344d171a07786ac8497c0088911f76e2d89deddf0d43111307c60ad795d2e9e0c36beaf8ac6274ec59a6a0248b1f3595fa69c016751cdee12bf412417d832266821fc535248e366628f18556a2ec97e88064dd3c12d36ba69d09ccb840237ec68439926b5a00617c3359fd7397783e4f6c39b108b5e643bea41e981ac17e15f582bec2794a2c8e80035108c8d5f81f24cb8d5c167276109de5e3c73b6846bab533f7f0e8a5c4f53fab310554ed8ad8bc0f735f44220f831cae348366654317bca64394edf7367f06881c6b3160bba06b0b0b3bf4f766832474e43a71419eda582948ae99e5fab4d8f448bc159441abe284185b5a1712b54fc0606912fc0db4f9321318281d167510566300aeba6d90233857ed2933804106936ed9858191dcf693b2e3813983d57da33b03a412068552d045eaa4109bc5a1d20184be3cff8eb4860a1af78a1ef9b4727f85f87c9cf48cd986a54bb738d5f7fd31bf27ff2730a3bc5f2e07fb9dd8c15c2621a105cd35351d07d4b64a42fd4d3422fa485fc1c55b3ee24c2d6eec913bbde3ef911724e2300c7a1ae7428a596951d0d3c8a380619a50c662237107c63076a90866636c260f30ba4be5a6de18a488e2cb9e18c8882d72553050c2bebef4a3ea70a981f305f5aac497e9347be372bdbdaa9e4c3817ae10dae40e2bd54c720b5731cc6d6f2295ecc5e34420aee8f671bd93051410b87bc2c298a70ef7d9d58825eded4a17154e9b14b17654d37d71e1ed9c7e32475dea9ec5f99ad68b86e4fc0dcc0ab10bbc5aa8546a0f6f3216ff5516b5115b6b9330c8034e741d7a681f8dcc7a470772b0e62141c5eddd5e7b6caf0a45bfc67599907a8b32bdbd483d2dec5a71a2b7dff7d265cee01995b5636987e1c51334b8491832e9dc8c300338e904a7f172ecc8a4df126359f785700ab527d925cb7319f93401697fddf37b9262960704edb3e08cbf4ce600c16e5192b2df175466dd6d65fabecd717358c6dba6c0e7618e6b1cf1374c668e8d1927c5bfd6bcfeec173b8528911a3ef67f0966761a69f076f01457c2262d2a66eee8d186f7b1ac9bd7ff9f956238ed40ee0ac833000872bf375ecfc2653cb5fa111e39b848a7fecd6ccaae1aca6b04b72706d41b54e766d666225f98e1ea269b39a574440f3508460c0ad6356a57e4cc4a189b6e68aa9539b76342264855db729402e124e6c0b7e20e8e0f5a01216c1e3d835ed94ec8329f104a8c9bf2e20db62f53916776bfe58960b95fda9b3b2895f733e8b506f223cbc042149c441bef2c80592abd8004977dadc57895686803d521f6a4cef958af3d2fd59734ab594544ca335a32007b05e29c7b1731899e53683e4f6a4d67202327a9fd8dba6d62d2448023cb350dc1d4b6e0b2228e417da381722ef66bfef7ed9a10228802ed5b853998c2a155d32e6a7c6aa36ca50da4e63c49fc484e6c765598462114db39c1c9853ce8f57ecc4dbe43c489f14b8b388c6b4c5bc8f11336dd127d191fa2098f32a8d9d77824bf677ffc3df479e2850a0d0d94b98eee08d871f61bd301a32e0c3a35475861a3b067bbe4dbccb31ebcea9d630c121d5000549001657218dc55ccce31ba449b3c98594d3cfdaa024c15533921359e971bfeea34f2b8604e4e4edf8f1efe8154784f60daa2c4d062664253fc0238f7a6c735b242d01fa884381b0eee1e1af35d93fec0d1d4c3e4571e77dea054643c055738c7e93c4e846896ec9297b5e2444b5f0658137815c203415b246831082fc3df0986d5430a11557a683af8214c912c3383415e0c288d4176c497d11a2c279ebcdec8ac5b6f0844e502991fdac9e1f4c9f834bdb6c1b7a67c456e8e72303b5e309c3b5283e4c0c48afc69098f7d76ace2a31711e779e7eb172220160079d2b4d74adf3e9e8f60d5276022ba68be170edeb1abcf191966c6b4403b61b4bab40129a42244ae381e533241ea87473d82590b21474ece13abbdd4087a4e440f107cac2190b48bc26096a3588cc72eef8f86cde256937375430608746095bdd00f33317ddb25accddd38c0a09b7586d48ef488f37061b87d0936ee8abc1761cad8d3595995b11469e9f0151dd2c2d9cd8f33e57c1cca5f9d8ab46494471211b5d6f5d7c82c3d3edde01f19705c9aaa465d110b7821e5be1e73bbdee14c7022c7ee5f46ecfb11a141f5178bfdb0df30f00a2e4726169993e137c944605e5d817ffc41920d43cd4b4b6fb9830a69c05d948aeed72e28fcb144bb55a531cd40e397861bf16f4d33ac4cd2fbb65d168987776bafe57f8663470f904d41fe18198e2ec5eb744dcefe5747428a4b2d36543e21de3dc61d5b741122c4f1cf0fe1be34b97d97972b0539a87d589e3cfc2829c94c0f101fda5370c351ca5188cd2f5765845581bfaecd032c949558b4cd3714faab3882f0c19e5a660cc43550a3b0db4e0128a8a6967a1a8efbf38b34fabc062aa556ed949c56a3f5fb1ba2ea87b278e3ca0ab3948e7e54311e03f4ce13257b50be0aab5cc336a1aa84b2bb8ac73ff7498a977aeff8619c36f2ac769861d5e2d5a0522c8d072c108359191fdbc2d969ee0cab437baa8b685c7e745a26c01938aedfd5da75037a426e86f786df41c0642942989bb692673b14e9d01e82d1f9c34fd060fbecfef4da9268e4f4a2f4890aae0020d4277d821c78aab58b336bcf8c8d34470ed0c4872a4dbdf95dbd72882060c56072a1159bc25e5483324969a7369958676ebd8a39d98906867974246ef622951a7dfb4ceb77a874a08c4144e0e416c2b4550b1972e69b86f9aa8f224e29e58d1fa44f393b5c7cffc638d9ad984425a252b17009fb092233e2745340e886268696b4253c94ed9767e81b533a1cedeea574eccf9cb9eb9216821b46c6f93c1740124316e41ee91d752a0821efb5f826f485adcc0bb689dcbb121eba7996c06ebbf40317c9c70a969bcf4a72a1b9b26099f442415b601b8988766bc2003e93ee599965eba2d05d3d7b17203c7c173af16fbe1085448f134be8899f536b48965d3b5c290843c6bdb99714c6344fdf1d614b6a62421e0ea9ca7629f3194109c23280d2f11ffe22ad98f066fe2fb8df0c097c2e17f0e6443f2cb25e8229c3a4688d4293b36cedfd7318ccc951b7107a01971df9ad22f1d15c3c93d45d9f9083c55c0e60dd252b9f2b7eaaff341ce2881c484ceb0bd9f7458b713776c909afd38838f405b8af9cec7b7ece3e7802ad3f5a1bb81542a577eb54b8d56beda4d611dec776fb18ecdf6cab607f8d1bc8cd88d3f82cbd418a80d2b108a0ffca30fed6a8ac5fb09ba2266d3c5b69e235fadf9bfb5c87e57818c04436943a003bef926a7198d46f690cfd21d1cf80f620ae9cc63914f47da9095e9fec1b79f77816215cb8ff609d167022ee5f6613b887da13880763d726eb14c1784a34835d9305a9cb9e343d213dffafc8e2161e42f5961801f7f1e0043f0a61943df852d1ecc55b59bc482a539cf24a07b41e44d5067e82a42c7165a115a59c93c298f345accddc1cbac486ad7bb47505a7e85010c59c9dbf65aeb57b6fba3416ab868103ff045ae642281a31e7d528780c6ff92d557bde35fb33dffe1860bd00807ff1f5236106939d69de9d825bc790864d6f0fb106c13fb85b08bb245cdde17b09b177e333449b59659febad30b39f6c4f77daf1ad4f79bec23cd8685b7f308bed2278aa5fbd42f8c3b65b76d58287a7694ed4e4f2efcdca37c911067c330de66f6fb01e3af51c9224d22df6f402d93fb5210f0c9a4b990d54ae72969c4c8c90a8650775d2696c743242c010b1c6c634efe1414b24015c23c6a2b98c727605d8559d4e8f0b636d4bb48aca478b784b9de84da6bb0b316ca718e3af2fbfdff1bc4ce7cb247847298c740586a93704985e672ba434df4add1420a77627b43d8be4991e0a93ad271591142a36f5bac370044b40c8942bbcecf20ab703d0d101d84679a21018f9f4b140161320b513d7695d2761dc6374e7b8dc311898e61334c00241f08c08b3571a469f888e189c37de335da6a96f2564c181f933e7e16bcb2cd1432e63235b50a9d05795d3fc599040093a55986c8b60dced2efa0f7f6f28dac45bbcd56dc4bad7fd0cd9179f90c52868f3eea71622b055e24175f461512a612ecb0f90acbad2348f8dab6fa2494d5c69c1a68b8081c4a1ab66f32c5ec676abe277f8c481348bae11ba27987750a39904299a9faa9ed09d065df545c7b5e5b9070f7d3a5078cb3545f879d65a3c2a5ab35a28253be6f153975bce333f35e66bd46e8177d379388b5e5559a2150a42ca33aa926d8db08d201e199304aa97f600120df1ebaab2774a119cc6d68d4f3e9d08f40f94a388de13e16159b9e633e0c66f49900708572f7123484be0e9a692ceeaf5fba698516c992fee0a4958bd53edbbf06743bf2784b1d48e715b3ced3a3ace9adee0483a3e0decf6e72a5ce61875f46e34e93f5f0fb37139d209e0015c42b3cf7acdde739571c25956c8406a64ee75fb66deb840fb7f8b2b7f0e68342ddb9aa4768d7b4a9770c8b326d5f8476316d4e393e4aa8d8a62c3fb42b30497198e3b8b2dabe3d7732b63e615f51a157733b85839749039211d1a03c97007e47c21a998aa7d29c7230d607880c4779558ae840efaac6d94edf25a0091e00e03137f69e45b817b0a19f1353c0427b86ca584bb2aebf8f9dfc404aab813192532ff6d55015a8339c73389f4f9673180c639fd6b1d34e3172a859a1046d6c2e6f0cbf23ca6d2a702124ae3f761f3f0689283e47e25ae180e0c562a8bc072f2f06093043d74f4e9e392eb58c250fba52695217dc2f244c142b3f43040a82b419f47f386deffc2f3ad9a00db4a5555bd00b103f69ef5300e1bc1b17980b4a93b7799b98a5d74c626d22dd028f16986fd6e372f8fa434a2aa17f56b1d4e4972e8f0c563a0abdb5f8bfadd147125170ca24ac41b4985199732f71f626819b8e898bae7282c05e65192d7a394c710317500e8b8c28a2e2963e73e20b98c68e77231f48bb64deee0ac64728861e174c022b5aa618f50e65e6a817340fdd52cc529f02162d2847b2712080c6e1795c772726e0df3e1f768115b961134ca9de2d737ad5e71c001fd9e3a735f87548846d4ca00de8fc738d3328810173c63465c741535266a3755b260706fb97ea7d0ccc35f4c6ac9cf53000eda2be9affab12af416db622f51dc9d744e6e1f677f549ab74343ed26eb92b4b22527801b6c0265bf34c557cc6ead7172aef67b6143a88f7581326cb6d404187426affa2a5c82084bf85022a9fcce48a5d26a87e527537e8ca80a8a39a5beeb30bce3a5cea347261cfe2f68e375d588c2e9b6d4698c235cd3316d824207de964974f1fd19188a028b446af6f7ea8b16c281b74ffeda71a49986d28d35c4d4151f992f7e73e86faa6fb45915213c64e8a656bfb0408ea76ef34d639235fcccc67045d76ecb1c92ea221cd03a815c835d21f9e6ef0513d490e1b07b3674cd7472f782a4db54f61d89b3d59e33ae6cdda6ca06604c05ddc16c71467d0aa16c6554a86d42feb7b7f82bec6d2b2f75a0e284e2c46c7e5d286d19c1486a1e8902e85770399b4119deff020e446fa8b291e6d053f827dcfff6f77ff0393d6f4de9a2d974c3bf4c05916b03d56a6018c8b4672f6189ba80eb0b5779321b40e0a18fb424a19633eee29acac29d13f79586eea74bc20e0dc42f11ee797c3609ea407a6245a64b773280884cc6cef7f3d703c1f25374051ea381fff58fa7ba72ac90d07212a1b1ff4b6774bb49b136f5271e1765aa43a6764b6bcec3a645e79ac6059025ee85e12551d2a6ebff49c4e24aface719054c868ea5a26a6e2ee49058fe9f534110116a12cf203d79cca76f196dc78ab77760a714d374b845f4a326389a16c155480c139e42d449e40e0dc2fe325001675ad8e09ef73f6e9c58251e2bfa05deb6e61ad160b3ec75a216c205c5b2ba12a3a1bfddc235235d089d77461cb51eea092e0b17187fa0b36e633e88353aa8ea9d127b893a271c81c2bcc8c33ca658edf7d7fc97a6a2f924010b6f5ef79b28cef48da8093a94afe1118a555cb6dd76c80a51905d3d1143fa1957882eb8be892793f9ceaffd4eb263281a851c4d9d6d2395a753a60bcb91e349adf0e30b802853002dc8bd0fc7cabc9e2c35238c27ad7ce96499116ae31cc868eb65505ece57a7f1311b1fd4af14cddc9707de966e56220074f9a7e617e91d11ff552461795ecfd58d315b0b8da73a4fcc34d2dc3d32fa944350e9d5b0c0da47fe0dd45af06c62393709083cfb1672cdbf571ae31740d4cd5a8f1e2974fa25cc56130d7fd2df5f5fd199a43429bd982cdd1c1558a5f730bd3907ff614e39c43ce33be6e808f3fadff8bb117e9c9fa39df6f14c58e5cc265fe25045cbdffe1198d46750edd7b023e9223560eae5b2219963626a5cec6c5717158605b8ae2f00f9a979aa344fb5c545cd6236af6c3c7a302fab677c93b4ab670f39d851ab6a3bf33566406b08ec4aae9d248e8a36a8aad80bd48902c78366f5ce422415dc749a6227427905fb0f284260b9df6e252130678eff45d6e1392bb2004b9dc5c734d1288a2b62f32fa5ddfcac46a10ea3f7cfad12bd87dd2ec381ed17c605b071627b2ce8b6e3fabe3c37e922492a9a971d0e087cf0b309adf4fac7aa74dc689f2c8a49cb7525ed14d5eed5859ebae598e756f9b7fd3481af717ff4bc1c4f4960ce49f5bee1ce22f721f4a601de830e39142fc21ee2b0a1f8089a5af53131f785a474b754c426b718330296098d80d0ed70baa7ec8d2a7d1ea7fdb646742cc6f0fc215c69ccb975ee78aaebe01a986b88c96d85778a0bf879f699acc77070a4063ca60c6c236620b644b874e33f9d030e912d7d07a74e66f129e372330cae7d0705e5a4748bff65624cce32d196153a59959e3543f29d5eed3f1b4340f4eca9963b6cdd59f88f8392c71abad81b945a13a8199e6e783a146886b63470bb256e5f259c962937e061061aa27bd8e286d1abc12f97db4e3a1d005775d3415a13d6a2ab5bb37ffe31dcc8b89753d5bf3c8500e589ae8c8d9310ea7e95d1c20d1fab07cbd82c87d251ac8f52cf23b6e647411befd25cd1644a14184ac801695f19ba98ec4c0ae610fe2e93dc1e39be705a033654b6d33ac2d9a381c3143c0950433039148aca9a3d5a0bf0a4c8f4451056a7f0c5cb14a94680d513a88822fbc2244399ee00aae68f69f8f9aae395fae45e706d6c4c44852e86f394b4b6f2b5370f2e80855b5610acc1605cd21afe63eadb2e4918b58f75e0be6a670d1631d97643c29bb41574890760c1aaebe8b2138790a9d988a89c827fb89fc7f0ee03959087e58f5863aed466a542b7f4e58af70b57df7d6c5850cea63f1bfc75ad98185d2b19640223ba24e1d472e266e58d9a2da7bb990e8629373617e81848b15eb90c295ac8cb25f5e89cbb9b97e66e68c1b280cecef1ff09d601e12deb3aa1db3076b9d63c8ac4ce07b2d2fc53eb9151c8af2766c7e290c70d1f84c7e9f809aefae06a212d972b36edb1c61b1e03b6eb7090653c123a134de5f1c91c06fde50fa82401728070243083f079161c75a80a6998575992cd9cc0cc1d3b3bce0e30efccb672fa01446bff6b34b921c0fd317049beb36ea68b7ddb8841a56235e96174c08a496502ae08242d57ee5d147affcfeccaf20f5ac24840399671bf9e08ca82d3446136b3cb6d9bb68e404f9d1fc70e42d6766af75119fd42092b6b3c4886248d6b8acf18c91b205d5659cc81f8a21acf637dc7915f8a812a63f3aa9f015f6d081cc13194f7f579ba550da0f84d7c4966119cd8cbbf932de009a5b4d571815bf7ecc365367185d76e040c3b52ea88dce7a9448d6586a2488994508b5240a349d4b1a80c891612f58bc56ac7b257daf680c8cd2c2df3f6704a7bdca220fa842d60c367a0417c52d1964e019f1cda9631b95a0a8973771ce0bb38c20e3a70d591403c67efef3ab50375fb95def156546fe4aae9c5f0fabae8a787e409ba44972679705fe073a7427fa5061377e23def65e352298f870fa145817e476855ab4831646f6480f774fb3d686920e9e6420669987f25681a079f6acf195ef55fda29f9bee21373d86e6e6f56ec271deeb75e3f78ded0d685e0175cd0fab1dd1606de8e40262b042f92912911b6112de0817b17ba1f7c81095373ddcab6a3debd9059500e99ddc78c590fe54da6c4cfcfea7b4e7e9923f6895c84ecb24d3a79efb36c805cc01f47a4b38b60109734638ea40edf3a545f236d62c6b114f0153f1f95f59a3944720ecb34a9cb0ec2e6f128215d0e3f5a3e9b534813fafa6d32b39e4b417aa299294a31ff1405aa9315aa7bd488f39306ffc4565a46f03078145d01f62cc2816c4f8ef712dd404538f0c033687e25e43f20984c7c234853381ab8ebb704b46753290f85f9fb8129bf7e1e6986bdc7f9ef78c2d594750449073bbef824558e343b58e03f6f9feb439e6f8dfe05decd7b52bffb3f41ded2dd25c57dbe3d88046af0e92e2ddd888a4f8d7f3725e80644296d43a665594ff25c85031aa9212c06166a220e1baf9f9ec934acb1c2a7adf41f815450f9e29f11a0f6d2a7d96ee02d607820bac6f82879cda5dd30ac9d2fb4d80efb28cddb0c33cb116d24ddf8c45a547f504986f22e2658db8254056f6403c71dcfcb73b760846cecefb48a049bd5e082db2d4ce62de5e0d4d0f158b72e5cad6a5dbf29f9e69c1eccdd9903799b543fad74829336b6e0f0e9ea9fe76e2cb4be930620a6dbabe79b3f0e800893e166387cb1ab43acc976b37ccf3f0e47b5b0a028db70727bad63018ce7532735d85c0783676bbc807112ca906d2a0b6cd854895ee0e0b771f3bcce4da9b6c5754e7da473378ae04af31fb7c9a5e20d835795b492cac955f7a4968faa9c6ed7ba4c5541ef8598344d743f6bc129eecb11af4bf5130232aa8528accfa1fb35ae8447fc7c50a01efdbf60661e8b5d93ceb6ca7efb5078c3b81aaabcc7d8c992523e91e6792c717b70904d36b39b131f2794dd19d77c3a5d0b844461a6e45724f6cd14e0f9761aa2444e0d6a0342101c078a9432fab0bbd50434dbf5e70fd48b39283fe5873225b07a3bd4886ecf59c973285a1c50242946123edcc59b24bb34798131da022b94d464e3b8bfdb326634da1281beb5b83b14fef9d2485660155580a00eed9e7ec6489385b76c6c6a6be0d7a7eb89a2fc7ec1461ddc09584b7e04cd623f3b2379965b4b392b3001cea7948ef271af748cb284214da680a2bdef4806b39f0a2b2fbc5e108f3d69d14ebf7775487b66dacf8f41afbb19d5d215b88e49e2294e2f9d685e35139ee44e48b2d28e2f0b5a41896d37235d5a7015e68bfbd6af20ee247147d8df1d30c730aaf366084747a30a5e4dbf75f12ad242f8484e87a30d9f28c4ae1ce8337b2458a6067869f468ce9fe8f07862607833477595e6d5e23f8d9c543e43992bbbd7e32867cfebb1684e9079af9fb2953dc74b7bd5739d944f766679db15b62435742d199539f9a948950c47565b32a0db071676f71e5547394e";
            string dataLenHex = "4d1127";
            FastStreamReader stream = new FastStreamReader(Helper.HexToBytes($"{dataLenHex}{dataHex}"));
            bool b = op.TryRead(stream, out string error);

            Assert.True(b);
            Assert.Null(error);
            Assert.Equal(OP.PushData2, op.OpValue);
            Helper.ComparePrivateField(op, "data", Helper.HexToBytes(dataHex));
        }

        public static IEnumerable<object[]> GetReadFailCases()
        {
            byte[] bigBytesWit = new byte[524];
            // CompctInt(521)
            bigBytesWit[0] = 0xfd;
            bigBytesWit[1] = 0x09;
            bigBytesWit[2] = 0x02;

            yield return new object[] { new FastStreamReader(new byte[0]), false, Err.EndOfStream };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 253 }),
                true,
                "First byte 253 needs to be followed by at least 2 byte."
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 76 }),
                false,
                "OP_PushData1 needs to be followed by at least one byte."
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 20, 1 }),
                true,
                Err.EndOfStream
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 20, 1 }),
                false,
                Err.EndOfStream
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { (byte)OP.Reserved, 1 }),
                false,
                "Unknown OP_Push value."
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0x4e, 0xff, 0xff, 0xff, 0xff, 1 }), // StackInt(uint.max)
                false,
                "Data size is too big."
            };
            yield return new object[]
            {
                new FastStreamReader(new byte[] { 0xfe, 0xff, 0xff, 0xff, 0xff, 1 }), // CompactInt(uint.max)
                true,
                "Data size is too big."
            };
        }
        [Theory]
        [MemberData(nameof(GetReadFailCases))]
        public void TryRead_FailTest(FastStreamReader stream, bool isWit, string expErr)
        {
            PushDataOp op = new PushDataOp();
            bool b = isWit ? op.TryReadWitness(stream, out string error) : op.TryRead(stream, out error);

            Assert.False(b);
            Assert.Equal(expErr, error);
        }


        public static IEnumerable<object[]> WriteToStreamCases()
        {
            byte[] data75 = Helper.GetBytes(75);
            byte[] exp75 = new byte[76];
            exp75[0] = 75;
            Buffer.BlockCopy(data75, 0, exp75, 1, data75.Length);
            byte[] expWit75 = new byte[76];
            expWit75[0] = 75;
            Buffer.BlockCopy(data75, 0, expWit75, 1, data75.Length);

            byte[] data76 = Helper.GetBytes(76);
            byte[] exp76 = new byte[78];
            exp76[0] = 76;
            exp76[1] = 76;
            Buffer.BlockCopy(data76, 0, exp76, 2, data76.Length);
            byte[] expWit76 = new byte[77];
            expWit76[0] = 76;
            Buffer.BlockCopy(data76, 0, expWit76, 1, data76.Length);

            byte[] data253 = Helper.GetBytes(253);
            byte[] exp253 = new byte[255]; // 253 = 0x4cfd <-- StackInt
            exp253[0] = 0x4c;
            exp253[1] = 0xfd;
            Buffer.BlockCopy(data253, 0, exp253, 2, data253.Length);
            byte[] expWit253 = new byte[256]; // 253 = 0xfdfd00 <-- CompactInt
            expWit253[0] = 0xfd;
            expWit253[1] = 0xfd;
            expWit253[2] = 0;
            Buffer.BlockCopy(data253, 0, expWit253, 3, data253.Length);

            yield return new object[] { new PushDataOp(-1), new byte[1] { 0x4f }, new byte[1] { 0x4f } };
            yield return new object[] { new PushDataOp(0L), new byte[1] { 0 }, new byte[1] { 0 } };
            yield return new object[] { new PushDataOp(1), new byte[1] { 0x51 }, new byte[1] { 0x51 } };
            yield return new object[] { new PushDataOp(2), new byte[1] { 0x52 }, new byte[1] { 0x52 } };
            yield return new object[] { new PushDataOp(16), new byte[1] { 0x60 }, new byte[1] { 0x60 } };
            yield return new object[] { new PushDataOp(17), new byte[2] { 1, 17 }, new byte[2] { 1, 17 } };
            yield return new object[] { new PushDataOp(data75), exp75, expWit75 };
            yield return new object[] { new PushDataOp(data76), exp76, expWit76 };
            yield return new object[] { new PushDataOp(data253), exp253, expWit253 };
        }
        [Theory]
        [MemberData(nameof(WriteToStreamCases))]
        public void WriteToStreamTest(PushDataOp op, byte[] expected, byte[] expectedWit)
        {
            FastStream normalStream = new FastStream();
            FastStream witnessStream = new FastStream();
            FastStream witnessSignStream = new FastStream();

            op.WriteToStream(normalStream);
            op.WriteToWitnessStream(witnessStream);
            op.WriteToStreamForSigningSegWit(witnessSignStream);

            byte[] actualNorm = normalStream.ToByteArray();
            byte[] actualWit = witnessStream.ToByteArray();
            byte[] actualWitSign = witnessSignStream.ToByteArray();

            Assert.Equal(expected, actualNorm);
            Assert.Equal(expectedWit, actualWit);
            Assert.Equal(expectedWit, actualWitSign); // ForSigningSegWit doesn't change anything
        }

        public static IEnumerable<object[]> WriteToStreamSigningSingleCases()
        {
            byte[] data75 = Helper.GetBytes(75);
            byte[] exp75 = new byte[76];
            exp75[0] = 75;
            Buffer.BlockCopy(data75, 0, exp75, 1, data75.Length);

            byte[] data76 = Helper.GetBytes(76);
            byte[] exp76 = new byte[78];
            exp76[0] = 76;
            exp76[1] = 76;
            Buffer.BlockCopy(data76, 0, exp76, 2, data76.Length);

            byte[] data253 = Helper.GetBytes(253);
            byte[] exp253 = new byte[255]; // 253 = 0x4cfd <-- StackInt
            exp253[0] = 0x4c;
            exp253[1] = 0xfd;
            Buffer.BlockCopy(data253, 0, exp253, 2, data253.Length);

            byte[] sig = new byte[] { 10, 20, 30 };

            yield return new object[] { new PushDataOp(OP._0), sig, new byte[1] };
            yield return new object[] { new PushDataOp(OP.Negative1), sig, new byte[1] { (byte)OP.Negative1 } };
            yield return new object[] { new PushDataOp(OP._1), sig, new byte[1] { (byte)OP._1 } };
            yield return new object[] { new PushDataOp(OP._16), sig, new byte[1] { (byte)OP._16 } };
            yield return new object[] { new PushDataOp(new byte[] { 255, 255, 255 }), sig, new byte[] { 3, 255, 255, 255 } };
            yield return new object[] { new PushDataOp(data75), sig, exp75 };
            yield return new object[] { new PushDataOp(data76), sig, exp76 };
            yield return new object[] { new PushDataOp(data253), sig, exp253 };
            yield return new object[] { new PushDataOp(new byte[] { 10, 20, 30 }), sig, new byte[0] };
        }
        [Theory]
        [MemberData(nameof(WriteToStreamSigningSingleCases))]
        public void WriteToStreamForSigning_SingleTest(PushDataOp op, byte[] sig, byte[] expected)
        {
            FastStream stream = new FastStream();
            op.WriteToStreamForSigning(stream, sig);
            byte[] actual = stream.ToByteArray();

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> WriteToStreamSigningMultiCases()
        {
            byte[] data75 = Helper.GetBytes(75);
            byte[] exp75 = new byte[76];
            exp75[0] = 75;
            Buffer.BlockCopy(data75, 0, exp75, 1, data75.Length);

            byte[] data76 = Helper.GetBytes(76);
            byte[] exp76 = new byte[78];
            exp76[0] = 76;
            exp76[1] = 76;
            Buffer.BlockCopy(data76, 0, exp76, 2, data76.Length);

            byte[] data253 = Helper.GetBytes(253);
            byte[] exp253 = new byte[255]; // 253 = 0x4cfd <-- StackInt
            exp253[0] = 0x4c;
            exp253[1] = 0xfd;
            Buffer.BlockCopy(data253, 0, exp253, 2, data253.Length);

            byte[][] sigs = new byte[][]
            {
                new byte[] { 10, 20, 30 },
                new byte[] { 40, 50, 60, 70 },
                new byte[] { 110, 111, 112, 113, 114, 115 }
            };

            yield return new object[] { new PushDataOp(OP._0), sigs, new byte[1] };
            yield return new object[] { new PushDataOp(OP.Negative1), sigs, new byte[1] { (byte)OP.Negative1 } };
            yield return new object[] { new PushDataOp(OP._1), sigs, new byte[1] { (byte)OP._1 } };
            yield return new object[] { new PushDataOp(OP._16), sigs, new byte[1] { (byte)OP._16 } };
            yield return new object[] { new PushDataOp(new byte[] { 255, 255, 255 }), sigs, new byte[] { 3, 255, 255, 255 } };
            yield return new object[] { new PushDataOp(data75), sigs, exp75 };
            yield return new object[] { new PushDataOp(data76), sigs, exp76 };
            yield return new object[] { new PushDataOp(data253), sigs, exp253 };
            // Remove signature
            yield return new object[] { new PushDataOp(new byte[] { 10, 20, 30 }), sigs, new byte[0] };
            yield return new object[] { new PushDataOp(new byte[] { 40, 50, 60, 70 }), sigs, new byte[0] };
            yield return new object[] { new PushDataOp(new byte[] { 110, 111, 112, 113, 114, 115 }), sigs, new byte[0] };
        }
        [Theory]
        [MemberData(nameof(WriteToStreamSigningMultiCases))]
        public void WriteToStreamForSigning_MultiTest(PushDataOp op, byte[][] sigs, byte[] expected)
        {
            FastStream stream = new FastStream();
            op.WriteToStreamForSigning(stream, sigs);
            byte[] actual = stream.ToByteArray();

            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetEqualCases()
        {
            yield return new object[] { new PushDataOp(1), new PushDataOp(1), true };
            yield return new object[] { new PushDataOp(1), new PushDataOp(0L), false };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2, 3 }), new PushDataOp(0L), false };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2, 3 }), new PushDataOp(new byte[] { 1, 2, 3 }), true };
            yield return new object[] { new PushDataOp(new byte[] { 1, 4, 3 }), new PushDataOp(new byte[] { 1, 2, 3 }), false };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2 }), new PushDataOp(new byte[] { 1, 2, 3 }), false };
        }
        [Theory]
        [MemberData(nameof(GetEqualCases))]
        public void EqualsTest(PushDataOp op1, PushDataOp op2, bool expected)
        {
            bool actual = op1.Equals(op2);
            Assert.Equal(expected, actual);
        }


        public static IEnumerable<object[]> GetHashCodeCases()
        {
            yield return new object[] { new PushDataOp(1), OP._1.GetHashCode() };
            yield return new object[] { new PushDataOp(new byte[] { 1, 2, 3 }), 507473 };
        }
        [Theory]
        [MemberData(nameof(GetHashCodeCases))]
        public void GetHashCodeTest(PushDataOp op, int expected)
        {
            int actual = op.GetHashCode();
            Assert.Equal(expected, actual);
        }
    }
}
