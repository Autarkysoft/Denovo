// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Encoders
{
    /// <summary>
    /// Address type, there are currently 4 defined types
    /// </summary>
    public enum AddressType
    {
        /// <summary>
        /// Unknown or invalid address
        /// </summary>
        Unknown,
        /// <summary>
        /// A provably invalid address (only for Bech32 addresses with witVer>0 that aren't using Bech32m mode)
        /// </summary>
        Invalid,
        /// <summary>
        /// Pay to public key hash
        /// </summary>
        P2PKH,
        /// <summary>
        /// Pay to script hash
        /// </summary>
        P2SH,
        /// <summary>
        /// Pay to witness public key hash
        /// </summary>
        P2WPKH,
        /// <summary>
        /// Pay to witness script hash
        /// </summary>
        P2WSH
    }
}
