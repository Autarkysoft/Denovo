// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Collections.Generic;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Handles current time and adjusting it.
    /// <para/>Implements <see cref="IClientTime"/>
    /// </summary>
    public class ClientTime : IClientTime
    {
        /// <summary>
        /// Maximum number of time offsets to hold
        /// </summary>
        public const int Capacity = 200;
        /// <summary>
        /// Maximum time offset that is ignored (5 minutes)
        /// </summary>
        public const int MaxIgnoredTimeOffset = 5 * 60;
        /// <summary>
        /// Minimum number of items in offset list needed before time offset is calculated.
        /// </summary>
        public const int MinIgnoreCount = 5;

        /// <summary>
        /// List of time offsets
        /// </summary>
        public readonly List<long> offsetList = new List<long>(Capacity);
        private readonly object timelock = new object();
        private bool removeFirst;
        private long offset;

        /// <inheritdoc/>
        public event EventHandler WrongClockEvent;

        private void RaiseWrongClockEvent() => WrongClockEvent?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public long Now
        {
            get
            {
                lock (timelock)
                {
                    return UnixTimeStamp.GetEpochUtcNow() + offset;
                }
            }
        }

        /// <inheritdoc/>
        public void UpdateTime(long time)
        {
            lock (timelock)
            {
                if (offsetList.Count >= Capacity)
                {
                    // To prevent the list from growing too big one item is removed that has the biggest offset
                    // from start (most negative) or from end (most positive).
                    // Note that the list is always sorted.
                    offsetList.RemoveAt(removeFirst ? 0 : offsetList.Count - 1);
                    removeFirst = !removeFirst;
                }

                long diff = UnixTimeStamp.GetEpochUtcNow() - time;
                offsetList.Add(diff);
                // TODO: this is very inefficient
                offsetList.Sort();

                if (offsetList.Count < MinIgnoreCount)
                {
                    // Leave offset as 0 if we haven't checked client time with at least 5 other nodes
                    return;
                }

                // Set offset to the median of all offsets
                if (offsetList.Count % 2 == 0)
                {
                    offset = (offsetList[(offsetList.Count / 2) - 1] + offsetList[offsetList.Count / 2]) / 2;
                }
                else
                {
                    offset = offsetList[offsetList.Count / 2];
                }

                // If the offset is more than ±5 minutes raise the wrong clock event
                if (offset < -MaxIgnoredTimeOffset || offset > MaxIgnoredTimeOffset)
                {
                    RaiseWrongClockEvent();
                }
            }
        }
    }
}
