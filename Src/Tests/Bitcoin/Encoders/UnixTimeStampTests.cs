// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Encoders
{
    public class UnixTimeStampTests
    {
        public static IEnumerable<object[]> GetDtEpoch()
        {
            yield return new object[] { new DateTime(1970, 1, 1, 0, 0, 0), 0 }; // epoch 0
            yield return new object[] { new DateTime(1969, 12, 31, 17, 47, 29), -22351 }; // negative epoch
            yield return new object[] { new DateTime(2018, 8, 23, 8, 51, 39), 1535014299 }; // random time
            yield return new object[] { new DateTime(2040, 1, 1, 12, 0, 52), 2209032052 }; // Year 2038 problem
        }

        [Theory]
        [MemberData(nameof(GetDtEpoch))]
        public void TimeToEpochTest(DateTime dt, long expected)
        {
            long actual = UnixTimeStamp.TimeToEpoch(dt);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GetDtEpoch))]
        public void EpochToTimeTest(DateTime expected, long epoch)
        {
            DateTime actual = UnixTimeStamp.EpochToTime(epoch);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetEpochNowTest()
        {
            DateTime expected = DateTime.Now;
            DateTime actual = UnixTimeStamp.EpochToTime(UnixTimeStamp.GetEpochNow());

            Assert.True(expected.ToLongTimeString() == actual.ToLongTimeString() ||
                                                    expected.Subtract(actual).Seconds < 5);
        }

        [Fact]
        public void GetEpochUtcNowTest()
        {
            DateTime expected = DateTime.UtcNow;
            DateTime actual = UnixTimeStamp.EpochToTime(UnixTimeStamp.GetEpochUtcNow());

            Assert.True(expected.ToLongTimeString() == actual.ToLongTimeString() ||
                                                    expected.Subtract(actual).Seconds < 5);
        }

    }
}
