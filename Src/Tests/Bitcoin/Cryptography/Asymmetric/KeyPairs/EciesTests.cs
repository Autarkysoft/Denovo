// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using System;
using System.Linq;
using Xunit;

namespace Tests.Bitcoin.Cryptography.Asymmetric.KeyPairs
{
    public class EciesTests
    {
        [Fact]
        public void EncryptDecryptTest()
        {
            using SharpRandom rng = new SharpRandom();
            using PrivateKey privateKey = new PrivateKey(rng);
            PublicKey pubkey = privateKey.ToPublicKey();

            for (int i = 0; i < 100; i++)
            {
                string message = new string(Enumerable.Repeat('a', i).ToArray());
                string enc = pubkey.Encrypt(message);
                string dec = privateKey.Decrypt(enc);

                Assert.Equal(dec, message);
            }
        }

        [Fact]
        public void EncryptDecrypt_LongTest()
        {
            using SharpRandom rng = new SharpRandom();
            using PrivateKey privateKey = new PrivateKey(rng);
            PublicKey pubkey = privateKey.ToPublicKey();

            string message = $"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer ac lectus metus. Donec ac mollis ex, sed feugiat leo. In aliquet erat et maximus vestibulum. Mauris non purus vitae nibh rhoncus scelerisque id at mauris. Maecenas id condimentum massa, quis fermentum quam. Suspendisse maximus nunc eget dapibus pulvinar. Nam at metus volutpat, molestie velit sit amet, pulvinar libero. Suspendisse eu pellentesque enim. Nullam porta molestie leo at sagittis. Pellentesque lacinia in ipsum et accumsan. Quisque cursus a nisi vel bibendum. Vivamus consectetur leo non eros convallis elementum.\r\nPhasellus diam massa, commodo pulvinar mauris sit amet, pellentesque gravida metus. Nullam dapibus sed tortor sed lobortis. Duis sodales, urna vel tincidunt sagittis, metus nisi sodales massa, at gravida eros ipsum sed tortor. Morbi rutrum augue augue, vel ornare felis faucibus at. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam ut gravida libero. Cras consectetur, est ut faucibus euismod, elit massa porta ipsum, in faucibus enim odio non felis. Fusce sed mi eu mi consectetur dignissim ut quis odio. Fusce non sapien mauris. Curabitur viverra tempor auctor. Nullam lobortis eleifend tincidunt. Ut arcu ante, porttitor vitae iaculis ut, condimentum eget purus. Phasellus pretium, dui eget dictum aliquam, lectus turpis gravida erat, in pretium libero neque vel dui.\r\nDonec semper, eros vulputate fringilla pellentesque, nibh ligula vestibulum risus, eu pretium nunc erat vitae purus. Nam maximus lacus vel erat mollis suscipit. Duis eget placerat odio, ac efficitur risus. Phasellus porta blandit bibendum. Aliquam ut odio sed nibh egestas sagittis. Nunc sodales urna velit, ac bibendum lacus pharetra ut. Sed ac sem a odio consequat pretium sed a dui. In ut dapibus lacus. Nam pulvinar nunc et nisl hendrerit mattis. Donec faucibus feugiat lacinia. Fusce congue, mauris vehicula ornare suscipit, dolor leo interdum libero, eu euismod sapien erat dignissim lorem.";
            string enc = pubkey.Encrypt(message);
            string dec = privateKey.Decrypt(enc);

            Assert.Equal(dec, message);

        }


    }
}
