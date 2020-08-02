// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using System;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MockNodeStatus : INodeStatus
    {
#pragma warning disable CS0649 // Field is never assigned to

        private const string UnexpectedCall = "Unexpected call was made";

        internal bool? _sendCmpt;
        public bool SendCompact
        {
            get
            {
                Assert.True(_sendCmpt.HasValue, UnexpectedCall);
                return _sendCmpt.Value;
            }
            set
            {
                Assert.True(_sendCmpt.HasValue, UnexpectedCall);
                Assert.Equal(_sendCmpt.Value, value);
            }
        }

        internal bool? _disconnect;
        public bool ShouldDisconnect
        {
            get
            {
                Assert.True(_disconnect.HasValue, UnexpectedCall);
                return _disconnect.Value;
            }
        }

        internal DateTime? _lastSeen;
        public DateTime LastSeen
        {
            get
            {
                Assert.True(_lastSeen.HasValue, UnexpectedCall);
                return _lastSeen.Value;
            }
        }

        internal HandShakeState? _handShakeToReturn;
        internal HandShakeState? _handShakeToSet;
        public HandShakeState HandShake
        {
            get
            {
                Assert.True(_handShakeToReturn.HasValue, UnexpectedCall);
                return _handShakeToReturn.Value;
            }
            set
            {
                Assert.True(_handShakeToSet.HasValue, UnexpectedCall);
                Assert.Equal(_handShakeToSet.Value, value);
                _handShakeToSet = null;
            }
        }

        internal bool bigViolation = false;
        public void AddBigViolation() => Assert.True(bigViolation, UnexpectedCall);

        internal bool mediumViolation = false;
        public void AddMediumViolation() => Assert.True(mediumViolation, UnexpectedCall);

        internal bool smallViolation = false;
        public void AddSmallViolation() => Assert.True(smallViolation, UnexpectedCall);

        internal bool updateTime = false;
        public void UpdateTime()
        {
            Assert.True(updateTime, UnexpectedCall);
            updateTime = false;
        }

#pragma warning restore CS0649 // Field is never assigned to
    }
}
