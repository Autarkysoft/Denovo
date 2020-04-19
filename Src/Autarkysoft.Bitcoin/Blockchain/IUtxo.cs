// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;

namespace Autarkysoft.Bitcoin.Blockchain
{
    public interface IUtxo
    {
        public bool IsMempoolSpent { get; set; }
        public bool IsBlockSpent { get; set; }

        public int Index { get; set; }
        public ulong Amount { get; set; }
        public PubkeyScript PubScript { get; set; }
    }
}
