// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Tests
{
    public class ValueCompareResult
    {
        public ValueCompareResult(bool bigger, bool biggerEqual, bool smaller, bool smallerEqual, bool equal, int compare = 0)
        {
            Bigger = bigger;
            BiggerEqual = biggerEqual;
            Smaller = smaller;
            SmallerEqual = smallerEqual;
            Equal = equal;
            Compare = compare;
        }

        public bool Bigger { get; }
        public bool BiggerEqual { get; }
        public bool Smaller { get; }
        public bool SmallerEqual { get; }
        public bool Equal { get; }
        public int Compare { get; }
    }
}
