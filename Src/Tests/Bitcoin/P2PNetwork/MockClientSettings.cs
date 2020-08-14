// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Threading;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MockClientSettings : IClientSettings
    {
#pragma warning disable CS0649 // Field is never assigned to

        private const string UnexpectedCall = "Unexpected call was made";

        internal int _protoVer;
        public int ProtocolVersion
        {
            get
            {
                Assert.True(_protoVer != 0, UnexpectedCall);
                return _protoVer;
            }
            set => throw new NotImplementedException();
        }

        internal bool? _relay;
        public bool Relay
        {
            get
            {
                Assert.True(_relay.HasValue, UnexpectedCall);
                return _relay.Value;
            }
            set => throw new NotImplementedException();
        }

        internal string _ua;
        public string UserAgent
        {
            get
            {
                Assert.True(!string.IsNullOrEmpty(_ua), UnexpectedCall);
                return _ua;
            }
            set => throw new NotImplementedException();
        }

        internal NetworkType? _netType;
        public NetworkType Network
        {
            get
            {
                Assert.True(_netType.HasValue, UnexpectedCall);
                return _netType.Value;
            }
            set => throw new NotImplementedException();
        }

        internal NodeServiceFlags? _services;
        public NodeServiceFlags Services
        {
            get
            {
                Assert.True(_services.HasValue, UnexpectedCall);
                return _services.Value;
            }
            set => throw new NotImplementedException();
        }

        internal long _time;
        public long Time
        {
            get
            {
                Assert.True(_time != 0, UnexpectedCall);
                return _time;
            }
        }

        internal ushort _port;
        public ushort Port
        {
            get
            {
                Assert.True(_port != 0, UnexpectedCall);
                return _port;
            }
            set => throw new NotImplementedException();
        }

        internal int _buffLen;
        public int BufferLength
        {
            get
            {
                Assert.True(_buffLen != 0, UnexpectedCall);
                return _buffLen;
            }
        }

        internal int _maxConnection;
        public int MaxConnectionCount
        {
            get
            {
                Assert.True(_maxConnection != 0, UnexpectedCall);
                return _maxConnection;
            }
            set => throw new NotImplementedException();
        }

        internal int _semaphore;
        public Semaphore MaxConnectionEnforcer
        {
            get
            {
                Assert.True(_semaphore != 0, UnexpectedCall);
                return new Semaphore(_semaphore, _semaphore);
            }
        }

        public SocketAsyncEventArgsPool SendReceivePool => throw new NotImplementedException();

#pragma warning restore CS0649 // Field is never assigned to
    }
}
