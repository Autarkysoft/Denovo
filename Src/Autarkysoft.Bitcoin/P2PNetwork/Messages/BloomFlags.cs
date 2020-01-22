// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.P2PNetwork.Messages
{
    public enum BloomFlags : byte
    {
        BLOOM_UPDATE_NONE = 0,
        BLOOM_UPDATE_ALL = 1,
        BLOOM_UPDATE_P2PUBKEY_ONLY = 2,
    }
}
