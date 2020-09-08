// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Xunit;

namespace Tests.Bitcoin.ExtentionsAndHelpersTests
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData("ab", "abc", 1)]
        [InlineData("kitten", "sitting", 3)]
        [InlineData("sunday", "saturday", 3)]
        [InlineData("flaw", "lawn", 2)]
        public void LevenshteinDistanceTest(string s1, string s2, int expected)
        {
            int actual1 = s1.LevenshteinDistance(s2);
            int actual2 = s2.LevenshteinDistance(s1);
            int actual3 = s1.LevenshteinDistance(s1);
            int actual4 = s2.LevenshteinDistance(s2);

            Assert.Equal(expected, actual1);
            Assert.Equal(expected, actual2);
            Assert.Equal(0, actual3);
            Assert.Equal(0, actual4);
        }
    }
}
