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

        internal ushort? _portToReturn;
        internal ushort? _portToSet;
        public ushort Port
        {
            get
            {
                Assert.True(_portToReturn.HasValue, UnexpectedCall);
                return _portToReturn.Value;
            }
            set
            {
                Assert.True(_portToSet.HasValue, UnexpectedCall);
                Assert.Equal(_portToSet.Value, value);
            }
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

        internal bool? _sendHdrToReturn;
        internal bool? _sendHdrToSet;
        public bool SendHeaders
        {
            get
            {
                Assert.True(_sendHdrToReturn.HasValue, UnexpectedCall);
                return _sendHdrToReturn.Value;
            }
            set
            {
                Assert.True(_sendHdrToSet.HasValue, UnexpectedCall);
                Assert.Equal(_sendHdrToSet.Value, value);
            }
        }

        internal bool? _hasManyViolation;
        public bool HasTooManyViolations
        {
            get
            {
                Assert.True(_hasManyViolation.HasValue, UnexpectedCall);
                return _hasManyViolation.Value;
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

        internal TimeSpan? _latency;
        public TimeSpan Latency
        {
            get
            {
                Assert.True(_latency.HasValue, UnexpectedCall);
                return _latency.Value;
            }
            set
            {
                Assert.True(_latency.HasValue, UnexpectedCall);
                Assert.Equal(_latency.Value, value);
                _latency = null;
            }
        }

        internal bool? _tooManyPings;
        public bool HasTooManyUnansweredPings
        {
            get
            {
                Assert.True(_tooManyPings.HasValue, UnexpectedCall);
                return _tooManyPings.Value;
            }
        }

        internal long? expPingNonce;
        internal bool storePingReturn;
        public bool StorePing(long nonce)
        {
            Assert.True(expPingNonce.HasValue, UnexpectedCall);
            Assert.Equal(expPingNonce.Value, nonce);
            return storePingReturn;
        }

        internal long? expPongNonce;
        public void CheckPing(long nonce)
        {
            Assert.True(expPongNonce.HasValue, UnexpectedCall);
            Assert.Equal(expPongNonce.Value, nonce);
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

        internal bool expectDiscSignal = false;
        public void SignalDisconnect()
        {
            Assert.True(expectDiscSignal, UnexpectedCall);
            expectDiscSignal = false;
        }

        public void UpdateTime()
        {
            Assert.True(updateTime, UnexpectedCall);
            updateTime = false;
        }

#pragma warning restore CS0649 // Field is never assigned to
    }
}
