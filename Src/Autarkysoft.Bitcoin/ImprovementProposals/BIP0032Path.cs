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
                    if (hardenedChars.Any(x => num.Contains(x)))
                    {
                        num = num[0..^1];
                        if (hardenedChars.Any(x => num.Contains(x)))
                        {
                            throw new FormatException("Input contains more than one indicator for hardened key.");
                        }
                        Indexes[i - 1] = 0x80000000;
                    }

                    if (!uint.TryParse(num, out uint ui))
                    {
                        throw new FormatException($"Input ({num}) is not a valid positive number.");
                    }
                    if ((ui & 0x80000000) != 0)
                    {
                        throw new FormatException("Index is too big.");
                    }
                    Indexes[i - 1] += ui;
                }
            }
        }



        private readonly char[] hardenedChars = { '\'', 'h' };
        /// <summary>
        /// An array of <see cref="BIP0032Path"/> indexes.
        /// </summary>
        public uint[] Indexes { get; private set; }



        /// <summary>
        /// Converts this instance to its string representation.
        /// </summary>
        /// <returns>A string starting with 'm' indicating master followed by each index</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("m");
            foreach (var item in Indexes)
            {
                if ((item & 0x80000000) != 0)
                {
                    sb.Append($"/{item - 0x80000000}{hardenedChars[0]}");
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
