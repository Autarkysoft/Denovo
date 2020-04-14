// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

namespace Autarkysoft.Bitcoin.Blockchain
{
    public interface IConsensus
    {
        int MaxSigOpCount { get; }

        /// <summary>
        /// BIP-34 requires coinbase transactions to include the block height. It was enabled on 227,931 on mainnet and on 21111 on TestNet and 500 on regtest
        /// </summary>
        /// <param name="blockHeight"></param>
        /// <returns></returns>
        bool IsBip34Enabled(int blockHeight);
        bool IsStrictNumberPush(int blockHeight);
        ulong BlockReward(int blockHeight);
    }
}
