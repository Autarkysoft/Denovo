// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Denovo.MVVM;

namespace Denovo.Models
{
    public class ConsensusModel : InpcBase, IConsensus
    {
        public ConsensusModel() : this(new Consensus(NetworkType.MainNet))
        {
        }

        public ConsensusModel(IConsensus c)
        {
            backup = c ?? new Consensus(NetworkType.MainNet);
        }


        private readonly IConsensus backup;


        private int _height;
        public int BlockHeight
        {
            get => _height;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (SetField(ref _height, value))
                {
                    backup.BlockHeight = value;
                }
            }
        }

        public int MaxSigOpCount => backup.MaxSigOpCount;

        public int HalvingInterval => backup.HalvingInterval;

        public ulong BlockReward => backup.BlockReward;

        private bool _bip94;
        public bool IsBip94
        {
            get => _bip94;
            set => SetField(ref _bip94, value);
        }

        private bool _bip16;
        public bool IsBip16Enabled
        {
            get => _bip16;
            set
            {
                if (SetField(ref _bip16, value) && !value)
                {
                    // Disable SegWit
                    IsSegWitEnabled = value;
                }
            }
        }

        private bool _bip30;
        public bool IsBip30Enabled
        {
            get => _bip30;
            set => SetField(ref _bip30, value);
        }

        private bool _bip34;
        public bool IsBip34Enabled
        {
            get => _bip34;
            set => SetField(ref _bip34, value);
        }

        private bool _bip65;
        public bool IsBip65Enabled
        {
            get => _bip65;
            set => SetField(ref _bip65, value);
        }

        private bool _der;
        public bool IsStrictDerSig
        {
            get => _der;
            set => SetField(ref _der, value);
        }

        private bool _bip112;
        public bool IsBip112Enabled
        {
            get => _bip112;
            set => SetField(ref _bip112, value);
        }

        private bool _bip147;
        public bool IsBip147Enabled
        {
            get => _bip147;
            set => SetField(ref _bip147, value);
        }

        private bool _segwit;
        public bool IsSegWitEnabled
        {
            get => _segwit;
            set
            {
                if (SetField(ref _segwit, value))
                {
                    if (value)
                    {
                        // Enable BIP-16 (P2SH) when SegWit is enabled
                        IsBip16Enabled = value;
                    }
                    else
                    {
                        // Disable Taproot
                        IsTaprootEnabled = false;
                    }
                }
            }
        }

        private bool _taproot;
        public bool IsTaprootEnabled
        {
            get => _taproot;
            set
            {
                if (SetField(ref _taproot, value) && value)
                {
                    // Enable SegWit whenever Taproot is enabled
                    IsSegWitEnabled = value;
                }
            }
        }

        public int MinBlockVersion => backup.MinBlockVersion;
        public bool AllowMinDifficultyBlocks => backup.AllowMinDifficultyBlocks;
        public Digest256 PowLimit => backup.PowLimit;

        public IBlock GetGenesisBlock() => backup.GetGenesisBlock();
    }
}
