// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;

namespace Tests.Bitcoin
{
    public class ConstantsTests
    {
        [Fact]
        public void GetMainNetDnsSeedsTest()
        {
            string[] results = Constants.GetMainNetDnsSeeds();
            Assert.NotEmpty(results);
            foreach (var item in results)
            {
                Assert.False(string.IsNullOrEmpty(item));
                Assert.Contains('.', item);
            }
        }

        [Fact]
        public void GetTestNet3DnsSeedsTest()
        {
            string[] results = Constants.GetTestNet3DnsSeeds();
            Assert.NotEmpty(results);
            foreach (var item in results)
            {
                Assert.False(string.IsNullOrEmpty(item));
                Assert.Contains('.', item);
            }
        }

        [Fact]
        public void GetTestNet4DnsTest()
        {
            string[] results = Constants.GetTestNet4DnsSeeds();
            Assert.NotEmpty(results);
            foreach (var item in results)
            {
                Assert.False(string.IsNullOrEmpty(item));
                Assert.Contains('.', item);
            }
        }
    }
}
