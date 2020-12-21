// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.P2PNetwork;
using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Net;
using System.Threading;
using Xunit;

namespace Tests.Bitcoin.P2PNetwork
{
    public class MockClientSettings : IClientSettings
    {
#pragma warning disable CS0649 // Field is never assigned to

        private const string UnexpectedCall = "Unexpected call was made";

        internal IBlockchain _bchain;
        public IBlockchain Blockchain
        {
            get
            {
                Assert.NotNull(_bchain);
                return _bchain;
            }
            set => throw new NotImplementedException();
        }

        internal IMemoryPool _memPool;
        public IMemoryPool MemPool
        {
            get
            {
                Assert.NotNull(_memPool);
                return _memPool;
            }
            set => throw new NotImplementedException();
        }

        internal IStorage _storage;
        public IStorage Storage
        {
            get
            {
                Assert.NotNull(_storage);
                return _storage;
            }
            set => throw new NotImplementedException();
        }

        internal bool? _catchup;
        public bool IsCatchingUp
        {
            get
            {
                Assert.True(_catchup.HasValue, UnexpectedCall);
                return _catchup.Value;
            }
            set => throw new NotImplementedException();
        }

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

        internal ulong? _fee;
        public ulong MinTxRelayFee
        {
            get
            {
                Assert.True(_fee.HasValue, UnexpectedCall);
                return _fee.Value;
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

        internal bool? _listen;
        public bool AcceptIncomingConnections
        {
            get
            {
                Assert.True(_listen.HasValue, UnexpectedCall);
                return _listen.Value;
            }
            set => throw new NotImplementedException();
        }

        internal string[] _dns;
        public string[] DnsSeeds
        {
            get
            {
                Assert.False(_dns is null, UnexpectedCall);
                return _dns;
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


        internal ITransaction _expMemPoolTx;
        internal bool _addToMemPoolReturn;
        public bool AddToMempool(ITransaction tx)
        {
            Assert.NotNull(_expMemPoolTx);
            Assert.Equal(_expMemPoolTx.GetTransactionHash(), tx.GetTransactionHash());
            return _addToMemPoolReturn;
        }

        internal NetworkAddressWithTime[] addrsToReturn;
        public NetworkAddressWithTime[] GetNodeAddrs()
        {
            Assert.True(addrsToReturn != null, $"Unexpected call to {nameof(GetNodeAddrs)} method was made.");
            return addrsToReturn;
        }

        internal NetworkAddressWithTime[] addrsToReceive;
        public void UpdateNodeAddrs(NetworkAddressWithTime[] nodeAddresses)
        {
            Assert.True(addrsToReceive != null, $"Unexpected call to {nameof(UpdateNodeAddrs)} method was made.");

            var actualStream = new FastStream();
            var expectedStream = new FastStream();
            Assert.Equal(addrsToReceive.Length, nodeAddresses.Length);
            foreach (var item in nodeAddresses)
            {
                item.Serialize(actualStream);
            }
            foreach (var item in addrsToReceive)
            {
                item.Serialize(expectedStream);
            }

            Assert.Equal(expectedStream.ToByteArray(), actualStream.ToByteArray());
        }

        internal IPAddress myIpToReturn;
        public IPAddress GetMyIP()
        {
            Assert.NotNull(myIpToReturn);
            return myIpToReturn;
        }

        internal IPAddress expUpdateAddr;
        public void UpdateMyIP(IPAddress addr)
        {
            Assert.Equal(expUpdateAddr, addr);
        }

#pragma warning restore CS0649 // Field is never assigned to
    }
}
