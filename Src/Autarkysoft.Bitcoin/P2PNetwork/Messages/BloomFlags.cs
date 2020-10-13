// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    /// <summary>
    /// Bloom flags used in bloom filters
    /// </summary>
    public enum BloomFlags
    {
        /// <summary>
        /// Adds nothing
        /// </summary>
        UpdateNone = 0,
        /// <summary>
        /// Adds everything
        /// </summary>
        UpdateAll = 1,
        /// <summary>
        /// Adds only outpoints to the filter if the output is a pay-to-pubkey/pay-to-multisig script
        /// </summary>
        UpdateP2PubkeyOnly = 2,
    }
}
