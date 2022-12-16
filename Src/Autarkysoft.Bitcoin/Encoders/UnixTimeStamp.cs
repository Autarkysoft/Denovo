// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// Time encoder to convert between Epoch and <see cref="DateTime"/>.
    /// </summary>
    public static class UnixTimeStamp
    {
        /// <summary>
        /// Converts a given <see cref="DateTime"/> to Epoch time.
        /// </summary>
        /// <param name="dt"><see cref="DateTime"/> to convert</param>
        /// <returns>Epoch time</returns>
        public static long TimeToEpoch(DateTime dt)
        {
            return ((DateTimeOffset)DateTime.SpecifyKind(dt, DateTimeKind.Utc)).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Converts a given Epoch time into its <see cref="DateTime"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="epoch">Epoch time</param>
        /// <returns>Converted <see cref="DateTime"/></returns>
        public static DateTime EpochToTime(long epoch)
        {
            return DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
        }

        /// <summary>
        /// Returns current system Epoch time.
        /// </summary>
        /// <returns>Current Epoch time</returns>
        public static long GetEpochNow()
        {
            return TimeToEpoch(DateTime.Now);
        }

        /// <summary>
        /// Returns current UTC Epoch time.
        /// </summary>
        /// <returns>Current UTC Epoch time.</returns>
        public static long GetEpochUtcNow()
        {
            return TimeToEpoch(DateTime.UtcNow);
        }
    }
}
