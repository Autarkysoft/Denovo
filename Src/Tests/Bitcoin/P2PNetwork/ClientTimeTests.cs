// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using Autarkysoft.Bitcoin.P2PNetwork;
using System;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class ClientTimeTests
    {
        [Fact]
        public void WrongClockEventTest()
        {
            var time = new ClientTime();
            bool raised = false;
            time.WrongClockEvent += (sender, e) =>
            {
                raised = true;
            };

            // Now + 1 min
            long toAdd = UnixTimeStamp.GetEpochUtcNow() + (ClientTime.MaxIgnoredTimeOffset + 60);
            for (int i = 0; i < ClientTime.MinIgnoreCount - 1; i++)
            {
                time.UpdateTime(toAdd);
                Assert.False(raised);
            }

            time.UpdateTime(toAdd);
            Assert.True(raised);
        }

        [Fact]
        public void NowTest()
        {
            var time = new ClientTime();
            var actual = time.Now;
            var expected = UnixTimeStamp.GetEpochUtcNow();
            Assert.True(Math.Abs(expected - actual) < 3);

            for (int i = 0; i < 6; i++)
            {
                time.UpdateTime(expected + 20);
            }

            actual = time.Now;
            expected = UnixTimeStamp.GetEpochUtcNow();
            var diff = Math.Abs(expected - actual);
            Assert.True(diff > 15 && diff < 25);
        }

        [Fact]
        public void UpdateTime_OverflowTest()
        {
            var time = new ClientTime();
            long toAdd = UnixTimeStamp.GetEpochUtcNow();
            time.offsetList.Add(-1000);
            time.offsetList.Add(1000);
            for (int i = 0; i < ClientTime.Capacity - 2; i++)
            {
                time.UpdateTime(toAdd);
            }

            Assert.Equal(ClientTime.Capacity, time.offsetList.Count);
            Assert.Equal(ClientTime.Capacity, time.offsetList.Capacity);
            Assert.Equal(-1000, time.offsetList[0]);
            Assert.Equal(1000, time.offsetList[^1]);

            // Overflow capacity but don't resize the list, instead remove items.
            // Last item is removed first.
            time.UpdateTime(toAdd);
            Assert.Equal(ClientTime.Capacity, time.offsetList.Count);
            Assert.Equal(ClientTime.Capacity, time.offsetList.Capacity);
            Assert.Equal(-1000, time.offsetList[0]);
            Assert.NotEqual(1000, time.offsetList[^1]);

            // Another overflow but this time first item is removed.
            time.UpdateTime(toAdd);
            Assert.Equal(ClientTime.Capacity, time.offsetList.Count);
            Assert.Equal(ClientTime.Capacity, time.offsetList.Capacity);
            Assert.NotEqual(-1000, time.offsetList[0]);
            Assert.NotEqual(1000, time.offsetList[^1]);
        }
    }
}
