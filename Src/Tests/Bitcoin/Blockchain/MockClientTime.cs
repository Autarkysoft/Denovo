// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using System;
using Xunit;

namespace Tests.Bitcoin.Blockchain
{
    class MockClientTime : IClientTime
    {
        internal long _now = 123456;
        public long Now => _now;

        public event EventHandler WrongClockEvent;

        internal long expUptdateTime = -1;
        public void UpdateTime(long time)
        {
            Assert.Equal(expUptdateTime, time);
        }
    }
}
