// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Represents the derivation paths used in <see cref="BIP0032"/> deterministic keys.
    /// </summary>
    public class BIP0032Path
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032Path"/> using the given indexes.
        /// </summary>
        /// <param name="ui">An array of indexes (use null, to represents the master)</param>
        public BIP0032Path(params uint[] ui)
        {
            Indexes = ui ?? new uint[0];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032Path"/> from the given string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="path">
        /// The path string (must start with letter 'm', each index separated with '/' and hardened indexes must use ''' or 'h')
        /// </param>
        public BIP0032Path(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("Path can not be null or empty.");
            }
            path = path.ToLower();
            if (!path.StartsWith("m"))
            {
                throw new FormatException("Path should start with letter \"m\".");
            }

            string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > byte.MaxValue)
            {
                throw new FormatException("Depth can not be bigger than 1 byte.");
            }

            Indexes = new uint[0];

            if (parts.Length > 1)
            {
                Indexes = new uint[parts.Length - 1];
                for (int i = 1; i < parts.Length; i++)
                {
                    // remove any extra whitespace.
                    string num = parts[i].Replace(" ", "");
                    if (HardenedChars.Any(x => num.Contains(x)))
                    {
                        num = num[0..^1];
                        if (HardenedChars.Any(x => num.Contains(x)))
                        {
                            throw new FormatException("Input contains more than one indicator for hardened key.");
                        }
                        Indexes[i - 1] = HardenedIndex;
                    }

                    if (!uint.TryParse(num, out uint ui))
                    {
                        throw new FormatException($"Input ({num}) is not a valid positive number.");
                    }
                    if ((ui & HardenedIndex) != 0)
                    {
                        throw new FormatException("Index is too big.");
                    }
                    Indexes[i - 1] += ui;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032Path"/> based on the given
        /// Electrum mnemonic type.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when given an undefined mnemonic type enum</exception>
        /// <param name="mnType">Electrum mnemonic type</param>
        public BIP0032Path(ElectrumMnemonic.MnemonicType mnType)
        {
            Indexes = mnType switch
            {
                ElectrumMnemonic.MnemonicType.Undefined => new uint[0], // m/
                ElectrumMnemonic.MnemonicType.Standard => new uint[1], // m/0/
                ElectrumMnemonic.MnemonicType.SegWit => new uint[2] { 0 + HardenedIndex, 0 }, // m/0'/0/
                ElectrumMnemonic.MnemonicType.Legacy2Fa => new uint[2] { 1 + HardenedIndex, 0 }, // m/1'/0/
                ElectrumMnemonic.MnemonicType.SegWit2Fa => new uint[2] { 1 + HardenedIndex, 0 }, // m/1'/0/
                _ => throw new ArgumentException("Undefined Electrum mnemonic type."),
            };
        }



        /// <summary>
        /// Accepted hardened index indicators
        /// </summary>
        public static readonly char[] HardenedChars = { '\'', 'h' };
        /// <summary>
        /// Minimum value for an index to be considered hardened
        /// </summary>
        public const uint HardenedIndex = 0x80000000;
        /// <summary>
        /// An array of <see cref="BIP0032Path"/> indexes.
        /// </summary>
        public uint[] Indexes { get; private set; }


        /// <summary>
        /// Different purpose index values for BIP-43
        /// <para/>m / purpose' / *
        /// </summary>
        public enum Bip43Purpose : uint
        {
            /// <summary>
            /// Default index defined by BIP-32 (0')
            /// </summary>
            Default = 0 + HardenedIndex,
            /// <summary>
            /// Multi-Account Hierarchy for Deterministic Wallets (44')
            /// </summary>
            Bip44 = 44 + HardenedIndex,
            /// <summary>
            /// Structure for Deterministic P2SH Multisignature Wallets (45')
            /// </summary>
            Bip45 = 45 + HardenedIndex,
            /// <summary>
            /// Derivation scheme for P2WPKH-nested-in-P2SH based accounts (49')
            /// </summary>
            Bip49 = 49 + HardenedIndex,
        }

        /// <summary>
        /// Coin type used in BIP-44 scheme
        /// </summary>
        /// <remarks>
        /// See https://github.com/satoshilabs/slips/blob/master/slip-0044.md for list of coins
        /// </remarks>
        public enum CoinType : uint
        {
            /// <summary>
            /// Bitcoin (0')
            /// </summary>
            Bitcoin = 0 + HardenedIndex,
            /// <summary>
            /// Bitcoin testnet (1')
            /// </summary>
            BitcoinTestnet = 1 + HardenedIndex
        }

        /// <summary>
        /// Creates a new instance of <see cref="BIP0032Path"/> based on BIP-44 scheme where purpose is 44'
        /// <para/>m / purpose' / coin_type' / account' / change /
        /// </summary>
        /// <param name="ct">
        /// Coin type (any undefined enum can be used by passing a valid UInt32 value, add <see cref="HardenedIndex"/> if needed)
        /// </param>
        /// <param name="account">A zero index account number (BIP-44 suggests hardened values)</param>
        /// <param name="isChange">
        /// Indicates if the path is for external chain (main addresses used for payments) or change addresses. 
        /// If true the last index is 1' otherwise 0'.
        /// </param>
        /// <returns>New instance of <see cref="BIP0032Path"/></returns>
        public static BIP0032Path CreateBip44(CoinType ct, uint account, bool isChange)
        {
            uint[] temp = new uint[4]
            {
                44 + HardenedIndex,
                (uint)ct,
                account,
                isChange ? 1U : 0U
            };

            return new BIP0032Path(temp);
        }

        /// <summary>
        /// Creates a new instance of <see cref="BIP0032Path"/> based on BIP-49 scheme where purpose is 49'
        /// <para/>m / purpose' / coin_type' / account' / change /
        /// </summary>
        /// <param name="ct">
        /// Coin type (any undefined enum can be used by passing a valid UInt32 value, add <see cref="HardenedIndex"/> if needed)
        /// </param>
        /// <param name="account">A zero index account number (BIP-44 suggests hardened values)</param>
        /// <param name="isChange">
        /// Indicates if the path is for external chain (main addresses used for payments) or change addresses. 
        /// If true the last index is 1' otherwise 0'.
        /// </param>
        /// <returns>New instance of <see cref="BIP0032Path"/></returns>
        public static BIP0032Path CreateBip49(CoinType ct, uint account, bool isChange)
        {
            uint[] temp = new uint[4]
            {
                49 + HardenedIndex,
                (uint)ct,
                account,
                isChange ? 1U : 0U
            };

            return new BIP0032Path(temp);
        }

        /// <summary>
        /// Adds the given index to the end of the index array (expands the array by one item).
        /// </summary>
        /// <param name="index">Index to add</param>
        public void Add(uint index)
        {
            uint[] temp = new uint[Indexes.Length + 1];
            Buffer.BlockCopy(Indexes, 0, temp, 0, Indexes.Length * 4);
            temp[^1] = index;
            Indexes = temp;
        }

        /// <summary>
        /// Converts this instance to its string representation.
        /// </summary>
        /// <returns>A string starting with 'm' indicating master followed by each index</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("m");
            foreach (var item in Indexes)
            {
                if ((item & HardenedIndex) != 0)
                {
                    sb.Append($"/{item - HardenedIndex}{HardenedChars[0]}");
                }
                else
                {
                    sb.Append($"/{item}");
                }
            }

            return sb.ToString();
        }
    }
}
