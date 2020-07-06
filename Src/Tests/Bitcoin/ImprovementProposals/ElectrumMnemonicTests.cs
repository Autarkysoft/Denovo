// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.ImprovementProposals;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Tests.Bitcoin.ImprovementProposals
{
    public class ElectrumMnemonicTests
    {
        public static IEnumerable<object[]> GetCtorByteCases()
        {
            // TODO: test vector with "UNICODE_HORROR" does not pass
            //string UNICODE_HORROR = Encoding.UTF8.GetString(Helper.HexToBytes("e282bf20f09f988020f09f98882020202020e3818620e38191e3819fe381be20e3828fe3828b2077cda2cda2cd9d68cda16fcda2cda120ccb8cda26bccb5cd9f6eccb4cd98c7ab77ccb8cc9b73cd9820cc80cc8177cd98cda2e1b8a9ccb561d289cca1cda27420cca7cc9568cc816fccb572cd8fccb5726f7273cca120ccb6cda1cda06cc4afccb665cd9fcd9f20ccb6cd9d696ecda220cd8f74cc9568ccb7cca1cd9f6520cd9fcd9f64cc9b61cd9c72cc95cda16bcca2cca820cda168ccb465cd8f61ccb7cca2cca17274cc81cd8f20ccb4ccb7cda0c3b2ccb5ccb666ccb82075cca7cd986ec3adcc9bcd9c63cda2cd8f6fccb7cd8f64ccb8cda265cca1cd9d3fcd9e"));

            yield return new object[]
            {
                BIP0039.WordLists.English,
                "wild father tree among universe such mobile favorite target dynamic credit identify",
                null, // entropy
                ElectrumMnemonic.MnemonicType.SegWit,
                null, // pass phrase
                Helper.HexToBytes("aac2a6302e48577ab4b46f23dbae0774e2e62c796f797d0a1b5faeb528301e3064342dafb79069e7c4c6b8c38ae11d7a973bec0d4f70626f8cc5184a8d0b0756"),
            };
            yield return new object[]
            {
                BIP0039.WordLists.English,
                "wild father tree among universe such mobile favorite target dynamic credit identify",
                null,
                ElectrumMnemonic.MnemonicType.SegWit,
                "Did you ever hear the tragedy of Darth Plagueis the Wise?",
                Helper.HexToBytes("4aa29f2aeb0127efb55138ab9e7be83b36750358751906f86c662b21a1ea1370f949e6d1a12fa56d3d93cadda93038c76ac8118597364e46f5156fde6183c82f"),
            };
            yield return new object[]
            {
                BIP0039.WordLists.Japanese,
                "なのか ひろい しなん まなぶ つぶす さがす おしゃれ かわく おいかける けさき かいとう さたん",
                BigInteger.Parse("1938439226660562861250521787963972783469").ToByteArray(true, true),
                ElectrumMnemonic.MnemonicType.Standard,
                null,
                Helper.HexToBytes("d3eaf0e44ddae3a5769cb08a26918e8b308258bcb057bb704c6f69713245c0b35cb92c03df9c9ece5eff826091b4e74041e010b701d44d610976ce8bfb66a8ad")
            };
            //yield return new object[]
            //{
            //    BIP0039.WordLists.Japanese,
            //    "なのか ひろい しなん まなぶ つぶす さがす おしゃれ かわく おいかける けさき かいとう さたん",
            //    BigInteger.Parse("1938439226660562861250521787963972783469").ToByteArray(true, true),
            //    ElectrumMnemonic.MnemonicType.Standard,
            //    UNICODE_HORROR,
            //    Helper.HexToBytes("251ee6b45b38ba0849e8f40794540f7e2c6d9d604c31d68d3ac50c034f8b64e4bc037c5e1e985a2fed8aad23560e690b03b120daf2e84dceb1d7857dda042457")
            //};
            yield return new object[]
            {
                BIP0039.WordLists.ChineseSimplified,
                "眼 悲 叛 改 节 跃 衡 响 疆 股 遂 冬",
                BigInteger.Parse("3083737086352778425940060465574397809099").ToByteArray(true, true),
                ElectrumMnemonic.MnemonicType.SegWit,
                null,
                Helper.HexToBytes("0b9077db7b5a50dbb6f61821e2d35e255068a5847e221138048a20e12d80b673ce306b6fe7ac174ebc6751e11b7037be6ee9f17db8040bb44f8466d519ce2abf")
            };
            yield return new object[]
            {
                BIP0039.WordLists.ChineseSimplified,
                "眼 悲 叛 改 节 跃 衡 响 疆 股 遂 冬",
                BigInteger.Parse("3083737086352778425940060465574397809099").ToByteArray(true, true),
                ElectrumMnemonic.MnemonicType.SegWit,
                "给我一些测试向量谷歌",
                Helper.HexToBytes("6c03dd0615cf59963620c0af6840b52e867468cc64f20a1f4c8155705738e87b8edb0fc8a6cee4085776cb3a629ff88bb1a38f37085efdbf11ce9ec5a7fa5f71")
            };
            yield return new object[]
            {
                BIP0039.WordLists.Spanish,
                "almíbar tibio superar vencer hacha peatón príncipe matar consejo polen vehículo odisea",
                BigInteger.Parse("3423992296655289706780599506247192518735").ToByteArray(true, true),
                ElectrumMnemonic.MnemonicType.Standard,
                null,
                Helper.HexToBytes("18bffd573a960cc775bbd80ed60b7dc00bc8796a186edebe7fc7cf1f316da0fe937852a969c5c79ded8255cdf54409537a16339fbe33fb9161af793ea47faa7a")
            };
            yield return new object[]
            {
                BIP0039.WordLists.Spanish,
                "almíbar tibio superar vencer hacha peatón príncipe matar consejo polen vehículo odisea",
                BigInteger.Parse("3423992296655289706780599506247192518735").ToByteArray(true, true),
                ElectrumMnemonic.MnemonicType.Standard,
                "araña difícil solución término cárcel",
                Helper.HexToBytes("363dec0e575b887cfccebee4c84fca5a3a6bed9d0e099c061fa6b85020b031f8fe3636d9af187bf432d451273c625e20f24f651ada41aae2c4ea62d87e9fa44c")
            };
            yield return new object[]
            {
                BIP0039.WordLists.Spanish,
                "equipo fiar auge langosta hacha calor trance cubrir carro pulmón oro áspero",
                BigInteger.Parse("448346710104003081119421156750490206837").ToByteArray(true, true),
                ElectrumMnemonic.MnemonicType.SegWit,
                null,
                Helper.HexToBytes("001ebce6bfde5851f28a0d44aae5ae0c762b600daf3b33fc8fc630aee0d207646b6f98b18e17dfe3be0a5efe2753c7cdad95860adbbb62cecad4dedb88e02a64")
            };
            yield return new object[]
            {
                BIP0039.WordLists.Spanish,
                "vidrio jabón muestra pájaro capucha eludir feliz rotar fogata pez rezar oír",
                BigInteger.Parse("3444792611339130545499611089352232093648").ToByteArray(true, true),
                ElectrumMnemonic.MnemonicType.SegWit,
                "¡Viva España! repiten veinte pueblos y al hablar dan fe del ánimo español... ¡Marquen arado martillo y clarín",
                Helper.HexToBytes("c274665e5453c72f82b8444e293e048d700c59bf000cacfba597629d202dcf3aab1cf9c00ba8d3456b7943428541fed714d01d8a0a4028fc3a9bb33d981cb49f")
            };
        }
        [Theory]
        [MemberData(nameof(GetCtorByteCases))]
        public void ConstructorTest(BIP0039.WordLists wl, string mnemonic, byte[] entropy,
                                               ElectrumMnemonic.MnemonicType mnType, string pass, byte[] bip32Seed)
        {
            using var elmn = new ElectrumMnemonic(mnemonic, wl, pass);
            Assert.Equal(mnType, elmn.MnType);
            Assert.Equal(mnemonic, elmn.ToMnemonic());

            if (entropy != null)
            {
                using var fromEntropy = new ElectrumMnemonic(entropy, mnType, wl, pass);
                Assert.Equal(mnType, fromEntropy.MnType);
                Assert.Equal(mnemonic, fromEntropy.ToMnemonic());
            }

            if (bip32Seed != null)
            {
                string actualXprv = elmn.ToBase58(false);
                string expectedXprv = new BIP0032(bip32Seed).ToBase58(false);
                Assert.Equal(expectedXprv, actualXprv);
            }
        }

        [Fact]
        public void Constructor_FromByte_IncrementTest()
        {
            byte[] ent = Helper.HexToBytes("0a0fecede9bf8a975eb6b4ef75bb799f00");
            using var elmn = new ElectrumMnemonic(ent, ElectrumMnemonic.MnemonicType.Standard, BIP0039.WordLists.Spanish);

            string actual = elmn.ToMnemonic();
            string expected = "almíbar tibio superar vencer hacha peatón príncipe matar consejo polen vehículo odisea";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_FromRngTest()
        {
            var rng = new MockRng(BigInteger.Parse("3423992296655289706780599506247192518735").ToByteArray(true, true));
            using var elmn = new ElectrumMnemonic(rng, ElectrumMnemonic.MnemonicType.Standard, BIP0039.WordLists.Spanish);

            string actual = elmn.ToMnemonic();
            string expected = "almíbar tibio superar vencer hacha peatón príncipe matar consejo polen vehículo odisea";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Constructor_ExceptionTest()
        {
            byte[] nba = null;
            string nstr = null;
            IRandomNumberGenerator nrng = null;
            var mnt = ElectrumMnemonic.MnemonicType.Standard;
            var mntBad1 = ElectrumMnemonic.MnemonicType.Undefined;
            var mntBad2 = (ElectrumMnemonic.MnemonicType)100;

            Assert.Throws<ArgumentNullException>(() => new ElectrumMnemonic(nstr));
            Assert.Throws<ArgumentNullException>(() => new ElectrumMnemonic(" "));
            Assert.Throws<ArgumentNullException>(() => new ElectrumMnemonic(nba, mnt));
            Assert.Throws<ArgumentNullException>(() => new ElectrumMnemonic(nrng, mnt));

            Assert.Throws<ArgumentOutOfRangeException>(() => new ElectrumMnemonic(new byte[16], mnt));
            Assert.Throws<ArgumentException>(() => new ElectrumMnemonic(new byte[17], mntBad1));
            Assert.Throws<ArgumentException>(() => new ElectrumMnemonic(new byte[17], mntBad2));
            Assert.Throws<ArgumentException>(() => new ElectrumMnemonic(new MockRng(new byte[0]), mntBad1));
            Assert.Throws<ArgumentException>(() => new ElectrumMnemonic(new MockRng(new byte[0]), mntBad2));
        }

        [Theory]
        [InlineData("wild father tree among universe such mobile favorite target dynamic credit identify identify")]
        [InlineData("wild father foo among universe such mobile favorite target dynamic credit identify")]
        [InlineData("wild father tree among universe such mobile favorite target dynamic credit credit")]
        public void Constructor_FormatExceptionTest(string mn)
        {
            Assert.Throws<FormatException>(() => new ElectrumMnemonic(mn));
        }

        [Fact]
        public void ToMnemonic_DisoisedExceptionTest()
        {
            byte[] ent = Helper.HexToBytes("0a0fecede9bf8a975eb6b4ef75bb799f00");
            var elmn = new ElectrumMnemonic(ent, ElectrumMnemonic.MnemonicType.Standard, BIP0039.WordLists.Spanish);
            elmn.Dispose();

            Assert.Throws<ObjectDisposedException>(() => elmn.ToMnemonic());
        }
    }
}
