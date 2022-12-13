// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Blocks;
using Autarkysoft.Bitcoin.Blockchain.Scripts;
using Autarkysoft.Bitcoin.Blockchain.Scripts.Operations;
using Autarkysoft.Bitcoin.Blockchain.Transactions;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Text;

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
        /// Initializes a new instance of <see cref="Consensus"/> using block height zero and the given <see cref="NetworkType"/>.
        /// </summary>
        /// <param name="netType">Network type</param>
        public Consensus(NetworkType netType) : this(0, netType)
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
            // https://github.com/bitcoin/bitcoin/blob/544709763e1f45148d1926831e07ff03487673ee/src/chainparams.cpp
            switch (netType)
            {
                case NetworkType.MainNet:
                    PowLimit = Digest256.ParseHex("00000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffff");
                    MaxSigOpCount = 80000;
                    HalvingInterval = 210000;
                    bip16 = 170060;
                    bip34 = 227931;
                    bip65 = 388381;
                    bip66 = 363725;
                    bip112 = 419328;
                    seg = 481824;
                    tap = 709632;
                    break;
                case NetworkType.TestNet:
                    PowLimit = Digest256.ParseHex("00000000ffffffffffffffffffffffffffffffffffffffffffffffffffffffff");
                    MaxSigOpCount = 80000;
                    HalvingInterval = 210000;
                    bip16 = 1718436;
                    bip34 = 21111;
                    bip65 = 581885;
                    bip66 = 330776;
                    bip112 = 770112;
                    seg = 834624;
                    tap = 2064268;
                    break;
                case NetworkType.RegTest:
                    PowLimit = Digest256.ParseHex("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff");
                    MaxSigOpCount = 80000;
                    HalvingInterval = 150;
                    bip16 = 0;
                    bip34 = 500;
                    bip65 = 1351;
                    bip66 = 1251;
                    bip112 = 432;
                    seg = 0;
                    tap = 0;
                    break;
                default:
                    throw new ArgumentException("Network type is not defined.");
            }

            BlockHeight = height;
            network = netType;
        }


        private readonly int bip16, bip34, bip65, bip66, bip112, seg, tap;
        private int minBlkVer;
        private readonly NetworkType network;
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

                if (value >= bip65)
                {
                    minBlkVer = 4;
                }
                else if (value >= bip66)
                {
                    minBlkVer = 3;
                }
                else if (value >= bip34)
                {
                    minBlkVer = 2;
                }
                else
                {
                    minBlkVer = 1;
                }
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
        public bool IsBip30Enabled => (!IsBip34Enabled &&
                                      network == NetworkType.MainNet && BlockHeight != 91842 && BlockHeight != 91880) ||
                                      BlockHeight >= 1983702;

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

        /// <inheritdoc/>
        public bool IsTaprootEnabled => BlockHeight >= tap;

        /// <inheritdoc/>
        public int MinBlockVersion => minBlkVer;

        /// <inheritdoc/>
        public Digest256 PowLimit { get; }

        /// <inheritdoc/>
        public IBlock GetGenesisBlock()
        {
            return network switch
            {
                NetworkType.MainNet => CreateGenesisBlock(1231006505, 2083236893, 0x1d00ffff, 1, 50_0000_0000),
                NetworkType.TestNet => CreateGenesisBlock(1296688602, 414098458, 0x1d00ffff, 1, 50_0000_0000),
                NetworkType.RegTest => CreateGenesisBlock(1296688602, 2, 0x207fffff, 1, 50_0000_0000),
                _ => throw new ArgumentException(Errors.InvalidNetwork.Convert()),
            };
        }

        /// <inheritdoc/>
        public Block CreateGenesisBlock(uint time, uint nonce, Target nbits, int version, ulong reward)
        {
            string timestamp = "The Times 03/Jan/2009 Chancellor on brink of second bailout for banks";
            byte[] tsBytes = Encoding.UTF8.GetBytes(timestamp);
            byte[] sigData = new byte[8 + tsBytes.Length];
            Buffer.BlockCopy(Base16.Decode("04ffff001d010445"), 0, sigData, 0, 8);
            Buffer.BlockCopy(tsBytes, 0, sigData, 8, tsBytes.Length);
            var sigScr = new SignatureScript(sigData);

            var pubOps = new IOperation[]
            {
                new PushDataOp(Base16.Decode("04678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5f")),
                new CheckSigOp()
            };
            var pubScr = new PubkeyScript(pubOps);
            return CreateGenesisBlock(version, time, nbits, nonce, 1, sigScr, reward, pubScr);
        }

        /// <inheritdoc/>
        public Block CreateGenesisBlock(int blkVer, uint time, Target nbits, uint nonce, int txVer,
                                        ISignatureScript sigScr, ulong reward, IPubkeyScript pubScr)
        {
            var coinbase = new Transaction()
            {
                Version = txVer,
                TxInList = new TxIn[1]
                {
                    new TxIn()
                    {
                        TxHash = Digest256.Zero,
                        Index = uint.MaxValue,
                        Sequence = uint.MaxValue,
                        SigScript = sigScr
                    }
                },
                TxOutList = new TxOut[1]
                {
                    new TxOut(reward, pubScr)
                },
                LockTime = 0
            };

            Digest256 merkle = coinbase.GetTransactionHash();
            var header = new BlockHeader(blkVer, Digest256.Zero, merkle, time, nbits, nonce);

            return new Block(header, new ITransaction[] { coinbase });
        }
    }
}
