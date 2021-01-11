// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Timers;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Used in each <see cref="Node"/> to show its status at all times.
    /// </summary>
    public class NodeStatus : INodeStatus, INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NodeStatus"/>.
        /// </summary>
        public NodeStatus()
        {
            discTimer = new Timer();
            discTimer.Elapsed += DiscTimer_Elapsed;
        }


        private void DiscTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SignalDisconnect();
            discTimer.Stop();
        }

        private int _v;
        /// <summary>
        /// Returns the violation score of this node
        /// </summary>
        public int Violation
        {
            get => _v;
            set
            {
                if (SetField(ref _v, value))
                {
                    IsDisconnected = Violation >= DisconnectThreshold;
                    if (IsDisconnected)
                    {
                        RaiseDisconnectEvent();
                    }
                }
            }
        }

        private const int SmallV = 10;
        private const int MediumV = 20;
        private const int BigV = 50;
        private const int DisconnectThreshold = 100;

        private Timer discTimer;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <inheritdoc/>
        public event EventHandler DisconnectEvent;

        private IPAddress _ip;
        /// <inheritdoc/>
        public IPAddress IP
        {
            get => _ip;
            set => SetField(ref _ip, value);
        }

        private ushort _port;
        /// <inheritdoc/>
        public ushort Port
        {
            get => _port;
            set => SetField(ref _port, value);
        }

        private int _protVer;
        /// <inheritdoc/>
        public int ProtocolVersion
        {
            get => _protVer;
            set => SetField(ref _protVer, value);
        }

        private NodeServiceFlags _servs;
        /// <inheritdoc/>
        public NodeServiceFlags Services
        {
            get => _servs;
            set => SetField(ref _servs, value);
        }

        private ulong _nonce;
        /// <inheritdoc/>
        public ulong Nonce
        {
            get => _nonce;
            set => SetField(ref _nonce, value);
        }

        private string _ua;
        /// <inheritdoc/>
        public string UserAgent
        {
            get => _ua;
            set => SetField(ref _ua, value);
        }

        private int _height;
        /// <inheritdoc/>
        public int StartHeight
        {
            get => _height;
            set => SetField(ref _height, value);
        }

        private bool _relay;
        /// <inheritdoc/>
        public bool Relay
        {
            get => _relay;
            set => SetField(ref _relay, value);
        }

        private ulong _fee;
        /// <inheritdoc/>
        public ulong FeeFilter
        {
            get => _fee;
            set => SetField(ref _fee, value);
        }

        private bool _cmpt;
        /// <inheritdoc/>
        public bool SendCompact
        {
            get => _cmpt;
            set => SetField(ref _cmpt, value);
        }

        private ulong _cmptVer;
        /// <inheritdoc/>
        public ulong SendCompactVer
        {
            get => _cmptVer;
            set
            {
                if (_cmptVer < value)
                    SetField(ref _cmptVer, value);
            }
        }

        private bool _sendHdr;
        /// <inheritdoc/>
        public bool SendHeaders
        {
            get => _sendHdr;
            set => SetField(ref _sendHdr, value);
        }

        private DateTime _lastSeen;
        /// <inheritdoc/>
        public DateTime LastSeen
        {
            get => _lastSeen;
            private set => SetField(ref _lastSeen, value);
        }

        private HandShakeState _handShake = HandShakeState.None;
        /// <inheritdoc/>
        public HandShakeState HandShake
        {
            get => _handShake;
            set => SetField(ref _handShake, value);
        }

        private readonly object discLock = new object();
        private bool _isDead = false;
        /// <inheritdoc/>
        public bool IsDisconnected
        {
            get
            {
                lock (discLock)
                {
                    return _isDead;
                }
            }

            set
            {
                lock (discLock)
                {
                    SetField(ref _isDead, value);
                }
            }
        }

        /// <inheritdoc/>
        public bool IsAddrSent { get; set; } = false;

        private TimeSpan _latency;
        /// <inheritdoc/>
        public TimeSpan Latency
        {
            get => _latency;
            set => SetField(ref _latency, value);
        }


        // TODO: some sort of "watcher" mechanism is needed to check connection on intervals (based on last seen)
        //       which would also be responsible for emptying pings list to prevent it from growing too big if the other node
        //       is not responding to any of them.

        // TODO: Dictionary could be replaced by a class that handles adding nonces + times itself and also makes sure to
        //       keep the array short and also to punish misbehaving nodes that spam Ping messages by raising the disconnect event
        //       Right now the following methods are only used by the Pings the client sends not the ones it receives.

        private readonly Dictionary<long, DateTime> pings = new Dictionary<long, DateTime>(5);

        /// <inheritdoc/>
        /// <remarks>
        /// With the default 2 minute ping interval and 5 cap this makes up for 10+ minutes of no Pong response.
        /// Or this can mean a connected node that never answered any of the Ping messages.
        /// </remarks>
        public bool HasTooManyUnansweredPings => pings.Count >= 5;

        /// <inheritdoc/>
        public bool StorePing(long nonce) => pings.TryAdd(nonce, DateTime.Now);

        /// <inheritdoc/>
        public void CheckPing(long nonce)
        {
            if (pings.Remove(nonce, out DateTime sendTime))
            {
                Latency = DateTime.Now - sendTime;
            }
            else
            {
                // Node replied with a nonce that wasn't supplied by us
                AddSmallViolation();
            }
        }


        /// <inheritdoc/>
        public bool HasTooManyViolations => Violation >= DisconnectThreshold;

        /// <inheritdoc/>
        private void RaiseDisconnectEvent() => DisconnectEvent?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public void SignalDisconnect()
        {
            IsDisconnected = true;
            RaiseDisconnectEvent();
        }

        /// <inheritdoc/>
        public void DisposeDisconnectTimer()
        {
            if (!(discTimer is null))
            {
                discTimer.Stop();
                discTimer.Dispose();
                discTimer = null;
            }
        }

        /// <inheritdoc/>
        public void ReStartDisconnectTimer()
        {
            if (!(discTimer is null))
            {
                discTimer.Stop();
                discTimer.Start();
            }
        }

        /// <inheritdoc/>
        public void StartDisconnectTimer(double interval)
        {
            if (!(discTimer is null))
            {
                discTimer.Interval = interval;
                discTimer.Start();
            }
        }

        /// <inheritdoc/>
        public void StopDisconnectTimer()
        {
            if (!(discTimer is null))
            {
                discTimer.Stop();
            }
        }

        /// <inheritdoc/>
        public void UpdateTime() => LastSeen = DateTime.Now;
        /// <inheritdoc/>
        public void AddBigViolation() => Violation += BigV;
        /// <inheritdoc/>
        public void AddMediumViolation() => Violation += MediumV;
        /// <inheritdoc/>
        public void AddSmallViolation() => Violation += SmallV;


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event using the given property name.
        /// The event is only invoked if data binding is used
        /// </summary>
        /// <param name="propertyName">The Name of the property that is changing.</param>
        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            else
            {
                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }
        }
    }
}
