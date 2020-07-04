// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Cryptography.KeyDerivationFunctions;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Mnemonic code for generating deterministic keys as defined by Electrum. Inherits from <see cref="BIP0032"/>.
    /// <para/> https://github.com/spesmilo/electrum/blob/392a648de5f6edf2e06620763bf0685854d0d56d/electrum/mnemonic.py
    /// </summary>
    public class ElectrumMnemonic : BIP0032
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ElectrumMnemonic"/> with the given entropy, world list and the passphrase.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="entropy">Entropy to use (must be 17 bytes or 132 bits)</param>
        /// <param name="mnType">Type of the mnemonic to create (anything but <see cref="MnemonicType.Undefined"/>)</param>
        /// <param name="wl">[Defaultvalue = <see cref="BIP0039.WordLists.English"/> Word list to use</param>
        /// <param name="passPhrase">
        /// [Default value = null] Optional passphrase to use for computing <see cref="BIP0032"/> entropy
        /// </param>
        public ElectrumMnemonic(byte[] entropy, MnemonicType mnType,
                                BIP0039.WordLists wl = BIP0039.WordLists.English, string passPhrase = null)
        {
            if (entropy == null)
                throw new ArgumentNullException(nameof(entropy), "Entropy can not be null.");
            if (entropy.Length != EntropyByteLen)
                throw new ArgumentOutOfRangeException(nameof(entropy), $"Entropy must be {EntropyByteLen} bytes or 132 bits.");
            if (!Enum.IsDefined(typeof(MnemonicType), mnType) || mnType == MnemonicType.Undefined)
                throw new ArgumentException("Undefined mnemonic type.", nameof(mnType));

            MnType = mnType;
            allWords = BIP0039.GetAllWords(wl);
            SetWordsFromEntropy(entropy);
            SetBip32(passPhrase);
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ElectrumMnemonic"/> with a randomly generated entropy
        /// using the given <see cref="IRandomNumberGenerator"/> instance, world list and an the passphrase.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="rng">Random number generator to use</param>
        /// <param name="mnType">Type of the mnemonic to create (anything but <see cref="MnemonicType.Undefined"/>)</param>
        /// <param name="wl">[Defaultvalue = <see cref="BIP0039.WordLists.English"/> Word list to use</param>
        /// <param name="passPhrase">
        /// [Default value = null] Optional passphrase to use for computing <see cref="BIP0032"/> entropy
        /// </param>
        public ElectrumMnemonic(IRandomNumberGenerator rng, MnemonicType mnType,
                                BIP0039.WordLists wl = BIP0039.WordLists.English, string passPhrase = null)
        {
            if (rng is null)
                throw new ArgumentNullException(nameof(rng), "Random number generator can not be null.");
            if (!Enum.IsDefined(typeof(MnemonicType), mnType) || mnType == MnemonicType.Undefined)
                throw new ArgumentException("Undefined mnemonic type.", nameof(mnType));

            MnType = mnType;
            allWords = BIP0039.GetAllWords(wl);

            byte[] entropy = new byte[EntropyByteLen];
            rng.GetBytes(entropy);
            SetWordsFromEntropy(entropy);
            SetBip32(passPhrase);
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ElectrumMnemonic"/> with the given mnemonic, world list and the passphrase.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="mnemonic">Mnemonic (should be 12 words)</param>
        /// <param name="wl">[Defaultvalue = <see cref="BIP0039.WordLists.English"/> Word list to use</param>
        /// <param name="passPhrase">
        /// [Default value = null] Optional passphrase to use for computing <see cref="BIP0032"/> entropy
        /// </param>
        public ElectrumMnemonic(string mnemonic, BIP0039.WordLists wl = BIP0039.WordLists.English, string passPhrase = null)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
                throw new ArgumentNullException(nameof(mnemonic), "Seed can not be null or empty!");
            allWords = BIP0039.GetAllWords(wl);

            string[] words = mnemonic.Normalize(NormalizationForm.FormKD)
                                     .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length != WordLen)
            {
                throw new FormatException("Invalid seed length. It should be 12.");
            }
            if (!words.All(x => allWords.Contains(x)))
            {
                throw new ArgumentException(nameof(mnemonic), "Seed has invalid words.");
            }

            wordIndexes = new int[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                wordIndexes[i] = Array.IndexOf(allWords, words[i]);
            }

            MnType = GetMnomonicType(Normalize(ToMnemonic()));
            if (MnType == MnemonicType.Undefined)
            {
                throw new FormatException("Invalid mnemonic (undefined version).");
            }

            SetBip32(passPhrase);
        }



        private const int EntropyByteLen = 17; // 132 bits
        private const int WordLen = 12;
        private int[] wordIndexes;
        private string[] allWords;

        /// <summary>
        /// Indicates wallet/address type generated by this mnemonic
        /// </summary>
        public MnemonicType MnType { get; private set; }

        /// <summary>
        /// Defines different mnemonic types that generate different wallet/address types
        /// </summary>
        public enum MnemonicType
        {
            /// <summary>
            /// Undefined or invalid mnemonic (can't be passed to constructors)
            /// </summary>
            Undefined,
            /// <summary>
            /// Standard (legacy) wallets
            /// </summary>
            Standard,
            /// <summary>
            /// Segwit wallets
            /// </summary>
            SegWit,
            /// <summary>
            /// Legacy wallet with Two-factor authentication
            /// </summary>
            Legacy2Fa,
            /// <summary>
            /// SegWit wallet with Two-factor authentication
            /// </summary>
            SegWit2Fa
        }

        private void SetWordsFromEntropy(byte[] entropy)
        {
            while (true)
            {
                // Treat the given 136 bits of entropy as 132 by ignoring first 4 bits and split it to 11-bit parts
                wordIndexes = new int[WordLen]
                {                                                                          // bits used   -> leftover
                    (entropy[0]  & 0b00001111) << 7  | entropy[1]  >> 1,                   // 4 + 7       -> 1
                    (entropy[1]  & 0b00000001) << 10 | entropy[2]  << 2 | entropy[3]  >> 6,// 1 + 8 + 2   -> 6
                    (entropy[3]  & 0b00111111) << 5  | entropy[4]  >> 3,                   // 6 + 5       -> 3
                    (entropy[4]  & 0b00000111) << 8  | entropy[5],                         // 3 + 8       -> 0
                     entropy[6]                << 3  | entropy[7]  >> 5,                   // 8 + 3       -> 5
                    (entropy[7]  & 0b00011111) << 6  | entropy[8]  >> 2,                   // 5 + 6       -> 2
                    (entropy[8]  & 0b00000011) << 9  | entropy[9]  << 1 | entropy[10] >> 7,// 2 + 8 + 1   -> 7
                    (entropy[10] & 0b01111111) << 4  | entropy[11] >> 4,                   // 7 + 4       -> 4
                    (entropy[11] & 0b00001111) << 7  | entropy[12] >> 1,                   // 4 + 7       -> 1
                    (entropy[12] & 0b00000001) << 10 | entropy[13] << 2 | entropy[14] >> 6,// 1 + 8 + 2   -> 6
                    (entropy[14] & 0b00111111) << 5  | entropy[15] >> 3,                   // 6 + 5       -> 3
                    (entropy[15] & 0b00000111) << 8  | entropy[16],                        // 3 + 8       -> 0
                };

                if (GetMnomonicType(Normalize(ToMnemonic())) == MnType)
                {
                    break;
                }
                else
                {
                    bool incrementNext = true;
                    for (int i = entropy.Length - 1; i >= 0 && incrementNext; i--)
                    {
                        incrementNext = entropy[i] == 255;
                        entropy[i]++;
                    }
                }
            }
        }

        private readonly int[][] CJK_INTERVALS = new int[][]
        {
            new int[] { 0x4E00, 0x9FFF }, // CJK Unified Ideographs
            new int[] { 0x3400, 0x4DBF }, // CJK Unified Ideographs Extension A
            new int[] { 0x20000, 0x2A6DF }, // CJK Unified Ideographs Extension B
            new int[] { 0x2A700, 0x2B73F }, // CJK Unified Ideographs Extension C
            new int[] { 0x2B740, 0x2B81F }, // CJK Unified Ideographs Extension D
            new int[] { 0xF900, 0xFAFF }, // CJK Compatibility Ideographs
            new int[] { 0x2F800, 0x2FA1D }, // CJK Compatibility Ideographs Supplement
            new int[] { 0x3190, 0x319F }, // Kanbun
            new int[] { 0x2E80, 0x2EFF }, // CJK Radicals Supplement
            new int[] { 0x2F00, 0x2FDF }, // CJK Radicals
            new int[] { 0x31C0, 0x31EF }, // CJK Strokes
            new int[] { 0x2FF0, 0x2FFF }, // Ideographic Description Characters
            new int[] { 0xE0100, 0xE01EF }, // Variation Selectors Supplement
            new int[] { 0x3100, 0x312F }, // Bopomofo
            new int[] { 0x31A0, 0x31BF }, // Bopomofo Extended
            new int[] { 0xFF00, 0xFFEF }, // Halfwidth and Fullwidth Forms
            new int[] { 0x3040, 0x309F }, // Hiragana
            new int[] { 0x30A0, 0x30FF }, // Katakana
            new int[] { 0x31F0, 0x31FF }, // Katakana Phonetic Extensions
            new int[] { 0x1B000, 0x1B0FF }, // Kana Supplement
            new int[] { 0xAC00, 0xD7AF }, // Hangul Syllables
            new int[] { 0x1100, 0x11FF }, // Hangul Jamo
            new int[] { 0xA960, 0xA97F }, // Hangul Jamo Extended A
            new int[] { 0xD7B0, 0xD7FF }, // Hangul Jamo Extended B
            new int[] { 0x3130, 0x318F }, // Hangul Compatibility Jamo
            new int[] { 0xA4D0, 0xA4FF }, // Lisu
            new int[] { 0x16F00, 0x16F9F }, // Miao
            new int[] { 0xA000, 0xA48F }, // Yi Syllables
            new int[] { 0xA490, 0xA4CF }, // Yi Radicals
        };

        private bool IsCJK(char c)
        {
            int val = char.ConvertToUtf32($"{c}", 0);
            foreach (var item in CJK_INTERVALS)
            {
                if (val >= item[0] && val <= item[1])
                {
                    return true;
                }
            }
            return false;
        }

        private string RemoveDiacritics(string mn)
        {
            var temp = new StringBuilder();
            foreach (char c in mn)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    temp.Append(c);
                }
            }
            return temp.ToString().Normalize(NormalizationForm.FormC);
        }

        // Input must have removed extra spaces and checked validity of each word to be in the word-list
        private string Normalize(string mnemonic)
        {
            string mn = RemoveDiacritics(mnemonic.Normalize(NormalizationForm.FormKD));
            // Remove whitespaces between CJK
            var temp = new StringBuilder();
            for (int i = 0; i < mn.Length; i++)
            {
                if (!(char.IsWhiteSpace(mn[i]) && IsCJK(mn[i - 1]) && IsCJK(mn[i + 1])))
                {
                    temp.Append(mn[i]);
                }
            }
            return temp.ToString();
        }

        private MnemonicType GetMnomonicType(string normalizedMn)
        {
            using HmacSha512 hmac = new HmacSha512(Encoding.UTF8.GetBytes("Seed version"));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(normalizedMn));

            if (hash[0] == 1)
            {
                return MnemonicType.Standard;
            }
            else if (hash[0] == 0x10)
            {
                int second = hash[1] & 0xf0;
                if (second == 0)
                {
                    return MnemonicType.SegWit;
                }
                else if (second == 0x10)
                {
                    return MnemonicType.Legacy2Fa;
                }
                else if (second == 0x20)
                {
                    return MnemonicType.SegWit2Fa;
                }
            }

            return MnemonicType.Undefined;
        }


        private void SetBip32(string passPhrase)
        {
            byte[] password = Encoding.UTF8.GetBytes(ToMnemonic());
            byte[] salt = Encoding.UTF8.GetBytes($"mnemonic{passPhrase?.Normalize(NormalizationForm.FormKD)}");

            using PBKDF2 kdf = new PBKDF2(2048, new HmacSha512());
            byte[] bytes = kdf.GetBytes(password, salt, 64);

            // Initialize BIP32 here:
            SetEntropy(bytes);
        }


        /// <summary>
        /// Returns mnemonic (seed words)
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <returns>mnemonic (seed words)</returns>
        public string ToMnemonic()
        {
            if (wordIndexes == null)
                throw new ObjectDisposedException(nameof(BIP0039));

            StringBuilder sb = new StringBuilder(wordIndexes.Length * 8);
            for (int i = 0; i < wordIndexes.Length; i++)
            {
                sb.Append($"{allWords[wordIndexes[i]]} ");
            }

            // no space at the end.
            sb.Length--;
            return sb.ToString();
        }



        /// <summary>
        /// Releases the resources used by the <see cref="BIP0039"/> class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (wordIndexes != null)
                    Array.Clear(wordIndexes, 0, wordIndexes.Length);
                wordIndexes = null;

                // Dispose the big field (2048 items)
                if (allWords != null)
                    Array.Clear(allWords, 0, allWords.Length);
                allWords = null;
            }

            base.Dispose(disposing);
        }
    }
}
