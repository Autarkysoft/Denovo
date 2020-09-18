// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Used in each <see cref="Node"/> to show its status at all times.
    /// </summary>
    public class NodeStatus : INodeStatus, INotifyPropertyChanged
    {
        private int violation;
        private const int SmallV = 10;
        private const int MediumV = 20;
        private const int BigV = 50;
        private const int DisconnectThreshold = 100;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;


        /// <inheritdoc/>
        public int ProtocolVersion { get; set; }
        /// <inheritdoc/>
        public NodeServiceFlags Services { get; set; }
        /// <inheritdoc/>
        public ulong Nonce { get; set; }
        /// <inheritdoc/>
        public string UserAgent { get; set; }
        /// <inheritdoc/>
        public int StartHeight { get; set; }
        /// <inheritdoc/>
        public bool Relay { get; set; }
        /// <inheritdoc/>
        public ulong FeeFilter { get; set; }
        /// <inheritdoc/>
        public bool SendCompact { get; set; }
        /// <inheritdoc/>
        public bool ShouldDisconnect => violation > DisconnectThreshold;

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

        /// <inheritdoc/>
        public void UpdateTime() => LastSeen = DateTime.Now;
        /// <inheritdoc/>
        public void AddBigViolation() => violation += SmallV;
        /// <inheritdoc/>
        public void AddMediumViolation() => violation += MediumV;
        /// <inheritdoc/>
        public void AddSmallViolation() => violation += BigV;


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event using the given property name.
        /// The event is only invoked if data binding is used
        /// </summary>
        /// <param name="propertyName">The Name of the property that is changing.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
