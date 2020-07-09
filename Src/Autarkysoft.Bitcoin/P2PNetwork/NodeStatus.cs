// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Used in each <see cref="Node"/> to show its status at all times.
    /// </summary>
    public class NodeStatus : INodeStatus
    {
        private int violation;
        private const int SmallV = 10;
        private const int MediumV = 20;
        private const int BigV = 50;
        private const int DisconnectThreshold = 100;


        /// <inheritdoc/>
        public bool SendCompact { get; set; }
        /// <inheritdoc/>
        public bool ShouldDisconnect => violation > DisconnectThreshold;
        /// <inheritdoc/>
        public DateTime LastSeen { get; private set; }
        /// <inheritdoc/>
        public HandShakeState HandShake { get; set; } = HandShakeState.None;

        /// <inheritdoc/>
        public void UpdateTime() => LastSeen = DateTime.Now;
        /// <inheritdoc/>
        public void AddBigViolation() => violation += SmallV;
        /// <inheritdoc/>
        public void AddMediumViolation() => violation += MediumV;
        /// <inheritdoc/>
        public void AddSmallViolation() => violation += BigV;
    }
}
