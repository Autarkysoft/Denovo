// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Denovo.MVVM;

namespace Denovo.Models
{
    public class TxWithFeeModel : InpcBase
    {
        public TxWithFeeModel(ITransaction tx, ulong fee)
        {
            Tx = tx;
            Id = tx.GetTransactionId();
            Fee = fee;
        }

        public ITransaction Tx { get; set; }
        public string Id { get; set; }

        private ulong _fee;
        public ulong Fee
        {
            get => _fee;
            set => SetField(ref _fee, value);
        }
    }
}
