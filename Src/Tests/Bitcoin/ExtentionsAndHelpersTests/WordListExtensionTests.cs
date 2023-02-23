// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.ImprovementProposals;
using Xunit;

namespace Tests.Bitcoin.ExtentionsAndHelpersTests
{
    public class WordListExtensionTests
    {
        [Theory]
        [InlineData(BIP0039.WordLists.ChineseSimplified, true)]
        [InlineData(BIP0039.WordLists.ChineseTraditional, true)]
        [InlineData(BIP0039.WordLists.Japanese, true)]
        [InlineData(BIP0039.WordLists.Korean, true)]
        [InlineData(BIP0039.WordLists.English, false)]
        [InlineData(BIP0039.WordLists.French, false)]
        public void IsCJKTest(BIP0039.WordLists wl, bool expected)
        {
            bool actual = wl.IsCJK();
            Assert.Equal(expected, actual);
        }
    }
}
