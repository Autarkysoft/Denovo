// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Blockchain
{
    /// <summary>
    /// Implementation of consensus rules for <see cref="NetworkType.MainNet"/>, <see cref="NetworkType.TestNet"/> and 
    /// <see cref="NetworkType.RegTest"/>.
    /// Implements <see cref="IConsensus"/>.
    /// </summary>
    public class Consensus : IConsensus
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Consensus"/> using block height zero for <see cref="NetworkType.MainNet"/>.
        /// </summary>
        public Consensus() : this(0, NetworkType.MainNet)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Consensus"/> using the <see cref="NetworkType"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="height">
        /// Block height to use for setting the consens rules (can be set to zero and changed later using the
        /// <see cref="BlockHeight"/> property). It has to change based on block height.
        /// </param>
        /// <param name="netType">Network type</param>
        public Consensus(int height, NetworkType netType)
        {
            BlockHeight = height;

            // https://github.com/bitcoin/bitcoin/blob/544709763e1f45148d1926831e07ff03487673ee/src/chainparams.cpp
            switch (netType)
            {
                case NetworkType.MainNet:
                    MaxSigOpCount = 80000;
                    HalvingInterval = 210000;
                    bip16 = 170060;
                    bip34 = 227931;
                    bip65 = 388381;
                    bip66 = 363725;
                    bip112 = 419328;
                    seg = 481824;
                    break;
                case NetworkType.TestNet:
                    MaxSigOpCount = 80000;
                    HalvingInterval = 210000;
                    bip16 = 1718436;
                    bip34 = 21111;
                    bip65 = 581885;
                    bip66 = 330776;
                    bip112 = 770112;
                    seg = 834624;
                    break;
                case NetworkType.RegTest:
                    MaxSigOpCount = 80000;
                    HalvingInterval = 150;
                    bip16 = 0;
                    bip34 = 500;
                    bip65 = 1351;
                    bip66 = 1251;
                    bip112 = 432;
                    seg = 0;
                    break;
                default:
                    throw new ArgumentException("Network type is not defined.");
            }
        }


        private readonly int bip16, bip34, bip65, bip66, bip112, seg;
        private int _height;

        /// <inheritdoc/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public int BlockHeight
        {
            get => _height;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(BlockHeight), "Block height can not be negative.");

                _height = value;
            }
        }

        /// <inheritdoc/>
        public int MaxSigOpCount { get; }

        /// <inheritdoc/>
        public int HalvingInterval { get; }

        /// <inheritdoc/>
        public ulong BlockReward
        {
            get
            {
                int halvings = BlockHeight / HalvingInterval;

                // Dividing max reward (50 BTC) 33 times will result in 0 but we'll use the same logic as core 
                // https://github.com/bitcoin/bitcoin/blob/master/src/validation.cpp#L1222-L1224
                // Note: in (ulong >> count) shifts the (count & 0x3F) is used instead
                if (halvings >= 64)
                {
                    return 0;
                }

                return 50_0000_0000UL >> halvings;
            }
        }

        /// <inheritdoc/>
        // Only 1 block on MainNet and 1 block on TestNet violate P2SH validation rule (before it activates)
        // the rest either don't have any or BIP-16 is already activated.
        public bool IsBip16Enabled => BlockHeight != bip16;

        /// <inheritdoc/>
        public bool IsBip34Enabled => BlockHeight >= bip34;

        /// <inheritdoc/>
        public bool IsBip65Enabled => BlockHeight >= bip65;

        /// <inheritdoc/>
        public bool IsStrictDerSig => BlockHeight >= bip66;

        /// <inheritdoc/>
        public bool IsBip112Enabled => BlockHeight >= bip112;

        /// <inheritdoc/>
        public bool IsBip147Enabled => BlockHeight >= seg; // BIP-147 was enabled alongside segwit

        /// <inheritdoc/>
        public bool IsSegWitEnabled => BlockHeight >= seg;
    }
}
