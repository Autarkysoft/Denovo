// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using Denovo.Models;
using Denovo.MVVM;
using System;
using System.Linq;

namespace Denovo.ViewModels
{
    public class VerifyTxViewModel : VmWithSizeBase
    {
        public VerifyTxViewModel() : base(800, 750)
        {
            VerifyCommand = new BindableCommand(Verify, () => IsVerifyEnable);

            Consensus = new();
            verifier = new(Consensus);
        }



        private readonly Transaction tx = new();
        private readonly TransactionVerifier verifier;
        public ConsensusModel Consensus { get; }


        public static string BlockHeightToolTip => "Block height is used to determine which consensus rules are active." +
            $"{Environment.NewLine}It is mandatory for verification of coinbase transactions.";


        private string _txHex;
        public string TxHex
        {
            get => _txHex;
            set
            {
                if (SetField(ref _txHex, value))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        Result = "Enter a valid transaction hex.";
                    }
                    else if (!Base16.TryDecode(value, out byte[] ba))
                    {
                        UtxoList = Array.Empty<UtxoModel>();
                        IsVerifyEnable = false;
                        Result = "Invalid hexadecimal transaction.";
                    }
                    else if (!tx.TryDeserialize(new FastStreamReader(ba), out Errors error))
                    {
                        UtxoList = Array.Empty<UtxoModel>();
                        IsVerifyEnable = false;
                        Result = $"Error deserializing transaction: {error.Convert()}";
                    }
                    else
                    {
                        // Has to recompute hashes when more than one tx is verified
                        tx.ComputeTransactionHashes();

                        Result = string.Empty;
                        if (tx.TxInList.Length == 1 && tx.TxInList[0].TxHash.Equals(Digest256.Zero))
                        {
                            // Minimal check to guess coinbase transaction
                            UtxoList = Array.Empty<UtxoModel>();
                        }
                        else
                        {
                            // Non-coinbase transactions have UTXOs
                            UtxoList = tx.TxInList.Select(x => new UtxoModel(x.TxHash, x.Index)).ToArray();
                        }

                        IsVerifyEnable = true;
                    }
                }
            }
        }

        private UtxoModel[] _utxos;
        public UtxoModel[] UtxoList
        {
            get => _utxos;
            private set => SetField(ref _utxos, value);
        }

        private string _res;
        public string Result
        {
            get => _res;
            private set => SetField(ref _res, value);
        }

        private bool _isEnable;
        public bool IsVerifyEnable
        {
            get => _isEnable;
            private set
            {
                if (SetField(ref _isEnable, value))
                {
                    VerifyCommand.RaiseCanExecuteChanged();
                }
            }
        }


        public BindableCommand VerifyCommand { get; private set; }
        private void Verify()
        {
            Utxo[] temp = new Utxo[UtxoList.Length];
            for (int i = 0; i < UtxoList.Length; i++)
            {
                if (!UtxoList[i].TryConvertToUtxo(out Utxo u, out string error))
                {
                    Result = $"Failed to set UTXO at index = {i}.{Environment.NewLine}{error}";
                    return;
                }

                temp[i] = u;
            }

            verifier.Init();
            if (UtxoList.Length == 0)
            {
                // Tx is guessed to be coinbase
                if (verifier.VerifyCoinbasePrimary(tx, out string error))
                {
                    Result = $"Coinbase transaction is valid (without checking output amount ie block reward + fees)." +
                             $"{Environment.NewLine}Transaction SigOp count: {verifier.TotalSigOpCount}";
                }
                else
                {
                    Result = $"Failed to verify the given coinbase transaction. Error: {error}";
                }
            }
            else if (verifier.Verify(tx, temp, out string err))
            {
                Result = $"Transaction is valid (assuming all given UTXOs existed and were unspent at the time)." +
                         $"{Environment.NewLine}Transaction fee: {verifier.TotalFee} satoshi" +
                         $"{Environment.NewLine}Transaction SigOp count: {verifier.TotalSigOpCount}";
            }
            else
            {
                Result = $"Failed to verify the given transaction. Error: {err}";
            }
        }
    }
}
