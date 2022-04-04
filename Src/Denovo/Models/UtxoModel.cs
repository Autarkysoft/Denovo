// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Encoders;
using Denovo.MVVM;

namespace Denovo.Models
{
    public class UtxoModel : InpcBase
    {
        public UtxoModel() { }

        public UtxoModel(byte[] txHash, uint index)
        {
            TxId = Base16.EncodeReverse(txHash);
            Index = index;
        }


        private string _txId;
        public string TxId
        {
            get => _txId;
            set => SetField(ref _txId, value);
        }

        private uint _index;
        public uint Index
        {
            get => _index;
            set => SetField(ref _index, value);
        }

        private ulong _amaount;
        public ulong Amount
        {
            get => _amaount;
            set => SetField(ref _amaount, value);
        }

        private string _scr = string.Empty;
        public string Script
        {
            get => _scr;
            set => SetField(ref _scr, value);
        }


        public bool TryConvertToUtxo(out Utxo result, out string error)
        {
            if (!Base16.TryDecode(Script, out byte[] scrBa))
            {
                error = "Invalid pubkey script hex.";
                result = null;
                return false;
            }

            PubkeyScript scr = new(scrBa);
            result = new Utxo(Index, Amount, scr);
            error = null;
            return true;
        }
    }
}
