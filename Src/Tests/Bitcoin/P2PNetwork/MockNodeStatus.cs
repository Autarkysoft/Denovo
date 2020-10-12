// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using Autarkysoft.Bitcoin.P2PNetwork.Messages.MessagePayloads;
using System;
using System.Net;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MockNodeStatus : INodeStatus
    {
#pragma warning disable CS0649 // Field is never assigned to
        public MockNodeStatus()
        {
        }

        public MockNodeStatus(VersionPayload pl)
        {
            _protVer = pl.Version;
            _servs = pl.Services;
            _nonce = pl.Nonce;
            _agent = pl.UserAgent;
            _height = pl.StartHeight;
            _relayToReturn = pl.Relay;
            _relayToSet = pl.Relay;
        }


        private const string UnexpectedCall = "Unexpected call was made";

        internal IPAddress _ip;
        public IPAddress IP
        {
            get => _ip;
            set => Assert.Equal(_ip, value);
        }

        internal int? _protVer;
        public int ProtocolVersion
        {
            get
            {
                Assert.True(_protVer.HasValue, UnexpectedCall);
                return _protVer.Value;
            }
            set
            {
                Assert.True(_protVer.HasValue, UnexpectedCall);
                Assert.Equal(_protVer.Value, value);
            }
        }

        internal NodeServiceFlags? _servs;
        public NodeServiceFlags Services
        {
            get
            {
                Assert.True(_servs.HasValue, UnexpectedCall);
                return _servs.Value;
            }
            set
            {
                Assert.True(_servs.HasValue, UnexpectedCall);
                Assert.Equal(_servs.Value, value);
            }
        }

        internal ulong? _nonce;
        public ulong Nonce
        {
            get
            {
                Assert.True(_nonce.HasValue, UnexpectedCall);
                return _nonce.Value;
            }
            set
            {
                Assert.True(_nonce.HasValue, UnexpectedCall);
                Assert.Equal(_nonce.Value, value);
            }
        }

        internal string _agent;
        public string UserAgent
        {
            get
            {
                Assert.False(string.IsNullOrEmpty(_agent), UnexpectedCall);
                return _agent;
            }
            set
            {
                Assert.False(string.IsNullOrEmpty(_agent), UnexpectedCall);
                Assert.Equal(_agent, value);
            }
        }

        internal int? _height;
        public int StartHeight
        {
            get
            {
                Assert.True(_height.HasValue, UnexpectedCall);
                return _height.Value;
            }
            set
            {
                Assert.True(_height.HasValue, UnexpectedCall);
                Assert.Equal(_height.Value, value);
            }
        }

        internal bool? _relayToReturn;
        internal bool? _relayToSet;
        public bool Relay
        {
            get
            {
                Assert.True(_relayToReturn.HasValue, UnexpectedCall);
                return _relayToReturn.Value;
            }
            set
            {
                Assert.True(_relayToSet.HasValue, UnexpectedCall);
                Assert.Equal(_relayToSet.Value, value);
            }
        }

        internal ulong? _fee;
        public ulong FeeFilter
        {
            get
            {
                Assert.True(_fee.HasValue, UnexpectedCall);
                return _fee.Value;
            }
            set
            {
                Assert.True(_fee.HasValue, UnexpectedCall);
                Assert.Equal(_fee.Value, value);
            }
        }

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

        internal ulong? _CmptVer;
        public ulong SendCompactVer
        {
            get
            {
                Assert.True(_CmptVer.HasValue, UnexpectedCall);
                return _CmptVer.Value;
            }
            set
            {
                Assert.True(_CmptVer.HasValue, UnexpectedCall);
                Assert.Equal(_CmptVer.Value, value);
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

        internal bool? _isDead;
        public bool IsDisconnected
        {
            get
            {
                Assert.True(_isDead.HasValue, UnexpectedCall);
                return _isDead.Value;
            }
            set
            {
                Assert.True(_isDead.HasValue, UnexpectedCall);
                Assert.Equal(_isDead.Value, value);
                _isDead = null;
            }
        }

        internal bool? _addrSent;
        public bool IsAddrSent
        {
            get
            {
                Assert.True(_addrSent.HasValue, UnexpectedCall);
                return _addrSent.Value;
            }
            set
            {
                // Only true is set by ReplyManager
                Assert.True(value);
                _addrSent = null;
            }
        }

        internal bool bigViolation = false;
        public void AddBigViolation()
        {
            Assert.True(bigViolation, UnexpectedCall);
            bigViolation = false;
        }

        internal bool mediumViolation = false;
        public void AddMediumViolation()
        {
            Assert.True(mediumViolation, UnexpectedCall);
            mediumViolation = false;
        }

        internal bool smallViolation = false;
        public void AddSmallViolation()
        {
            Assert.True(smallViolation, UnexpectedCall);
            smallViolation = false;
        }

        internal bool updateTime = false;

        public event EventHandler DisconnectEvent;

        public void UpdateTime()
        {
            Assert.True(updateTime, UnexpectedCall);
            updateTime = false;
        }

#pragma warning restore CS0649 // Field is never assigned to
    }
}
