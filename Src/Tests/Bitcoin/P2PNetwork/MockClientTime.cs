// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using System;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    class MockClientTime : IClientTime
    {
#pragma warning disable CS0649 // Field is never assigned to
#pragma warning disable CS0067 // Field is never used
        private const string UnexpectedCall = "Unexpected call was made";

        internal long? _now;
        public long Now
        {
            get
            {
                Assert.True(_now.HasValue, UnexpectedCall);
                return _now.Value;
            }
        }

        public event EventHandler WrongClockEvent;

        internal long? _updateTime;
        public void UpdateTime(long time)
        {
            Assert.True(_updateTime.HasValue, UnexpectedCall);
            Assert.Equal(_updateTime.Value, time);
        }
#pragma warning restore CS0649 // Field is never assigned to
#pragma warning restore CS0067 // Field is never used
    }
}
