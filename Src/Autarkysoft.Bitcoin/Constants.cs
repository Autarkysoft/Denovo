// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// Constant values used globally in different places.
    /// </summary>
    public struct Constants
    {
        /// <summary>
        /// Smallest bitcoin unit (10^-8)
        /// </summary>
        public const decimal Satoshi = 0.0000_0001m;
        /// <summary>
        /// The bitcoin symbol
        /// </summary>
        public const string Symbol = "฿";
        /// <summary>
        /// Total bitcoin supply in satoshi, 21 million with 8 decimal places.
        /// </summary>
        public const ulong TotalSupply = 21_000_000_0000_0000;
        /// <summary>
        /// Number of times to retry setting random bytes using <see cref="Cryptography.IRandomNumberGenerator"/>.
        /// </summary>
        /// <remarks>
        /// 2 should be enough, we set it higher to be safe!
        /// </remarks>
        public const int RngRetryCount = 5;
        /// <summary>
        /// The constend string that concatinated to the beginning of a message before signing it.
        /// </summary>
        public const string MsgSignConst = "Bitcoin Signed Message:\n";
    }
}
