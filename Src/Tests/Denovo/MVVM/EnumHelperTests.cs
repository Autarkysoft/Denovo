// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Denovo.Models;
using Denovo.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Tests.Denovo.MVVM
{
    public class EnumHelperTests
    {
        public enum Foo
        {
            [Description("Desc 1")]
            Foo1,
            Foo2,
            [Description("Desc 3")]
            Foo3,
            [Description("Desc 4")]
            Foo4,
        }

        public class EqHelper<T> : IEqualityComparer<T> where T : DescriptiveEnum<Foo>
        {
            public bool Equals([AllowNull] T x, [AllowNull] T y)
            {
                if (x is null && y is null)
                {
                    return true;
                }
                else if (x is null || y is null)
                {
                    return false;
                }
                else
                {
                    return x.Value == y.Value && x.Description == y.Description;
                }
            }

            public int GetHashCode([DisallowNull] T obj) => HashCode.Combine(obj?.Value);
        }


        [Fact]
        public void GetAllEnumValuesTest()
        {
            IEnumerable<Foo> actual = EnumHelper.GetAllEnumValues<Foo>();
            IEnumerable<Foo> expected = new Foo[] { Foo.Foo1, Foo.Foo2, Foo.Foo3, Foo.Foo4 };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetEnumValues_All_Test()
        {
            IEnumerable<Foo> actual = EnumHelper.GetEnumValues<Foo>();
            IEnumerable<Foo> expected = new Foo[] { Foo.Foo1, Foo.Foo2, Foo.Foo3, Foo.Foo4 };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetEnumValues_Exclude_Test()
        {
            IEnumerable<Foo> actual = EnumHelper.GetEnumValues(Foo.Foo2, Foo.Foo2, Foo.Foo4);
            IEnumerable<Foo> expected = new Foo[] { Foo.Foo1, Foo.Foo3 };
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDescriptiveEnumsTest()
        {
            IEnumerable<DescriptiveEnum<Foo>> actual = EnumHelper.GetDescriptiveEnums<Foo>();
            IEnumerable<DescriptiveEnum<Foo>> expected = new DescriptiveEnum<Foo>[]
            {
                new DescriptiveEnum<Foo>(Foo.Foo1),
                new DescriptiveEnum<Foo>(Foo.Foo2),
                new DescriptiveEnum<Foo>(Foo.Foo3),
                new DescriptiveEnum<Foo>(Foo.Foo4),
            };

            Assert.Equal(expected, actual, new EqHelper<DescriptiveEnum<Foo>>());
        }

        [Fact]
        public void GetDescriptiveEnums_WithExclusion_Test()
        {
            IEnumerable<DescriptiveEnum<Foo>> actual = EnumHelper.GetDescriptiveEnums(Foo.Foo2, Foo.Foo4);
            IEnumerable<DescriptiveEnum<Foo>> expected = new DescriptiveEnum<Foo>[]
            {
                new DescriptiveEnum<Foo>(Foo.Foo1),
                new DescriptiveEnum<Foo>(Foo.Foo3),
            };

            Assert.Equal(expected, actual, new EqHelper<DescriptiveEnum<Foo>>());
        }
    }
}
