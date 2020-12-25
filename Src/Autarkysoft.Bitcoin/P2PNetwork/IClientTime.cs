// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Defines methods and properties that a class handling client's time and its adjustment implements.
    /// </summary>
    public interface IClientTime
    {
        /// <summary>
        /// An event to be raised if the client's computer's time is too far behind or ahead.
        /// It can be used to show a warning message in UI.
        /// </summary>
        event EventHandler WrongClockEvent;

        /// <summary>
        /// Returns current UTC time after adjustment as a Unix timestamp
        /// </summary>
        long Now { get; }

        /// <summary>
        /// Updates client's time
        /// </summary>
        /// <param name="time">Current time</param>
        void UpdateTime(long time);
    }
}
