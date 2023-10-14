// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Cryptography.KeyDerivationFunctions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
                throw new FormatException("Mnemonic has invalid words.");
            }

            wordIndexes = new int[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                wordIndexes[i] = Array.IndexOf(allWords, words[i]);
            }

            MnType = GetMnemonicType(Normalize(ToMnemonic()));
            if (MnType == MnemonicType.Undefined)
            {
                throw new FormatException("Invalid mnemonic (undefined version).");
            }

            SetBip32(passPhrase);
        }



        private const int OldWordListLength = 1626;
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
                {                                                                           // bits used (leftover)
                    (entropy[15] & 0b00000111) << 8  | entropy[16],                         // 3(5) + 8
                    (entropy[14] & 0b00111111) << 5  | entropy[15] >> 3,                    // 6(2) + 5
                    (entropy[12] & 0b00000001) << 10 | entropy[13] << 2 | entropy[14] >> 6, // 1(7) + 8 + 2
                    (entropy[11] & 0b00001111) << 7  | entropy[12] >> 1,                    // 4(4) + 7
                    (entropy[10] & 0b01111111) << 4  | entropy[11] >> 4,                    // 7(1) + 4
                    (entropy[8]  & 0b00000011) << 9  | entropy[9]  << 1 | entropy[10] >> 7, // 2(6) + 8 + 1
                    (entropy[7]  & 0b00011111) << 6  | entropy[8]  >> 2,                    // 5(3) + 6
                     entropy[6]                << 3  | entropy[7]  >> 5,                    // 8(0) + 3
                    (entropy[4]  & 0b00000111) << 8  | entropy[5],                          // 3(5) + 8
                    (entropy[3]  & 0b00111111) << 5  | entropy[4]  >> 3,                    // 6(2) + 5
                    (entropy[1]  & 0b00000001) << 10 | entropy[2]  << 2 | entropy[3] >> 6,  // 1(7) + 8 + 2
                    (entropy[0]  & 0b00001111) << 7  | entropy[1]  >> 1,                    // 4 + 7
                };

                string normalized = Normalize(ToMnemonic());
                if (IsOld(normalized))
                {
                    continue;
                }
                if (GetMnemonicType(normalized) == MnType)
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

        private static readonly int[][] CJK_INTERVALS = new int[][]
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

        private static bool IsCJK(char c)
        {
            int val = c;
            foreach (var item in CJK_INTERVALS)
            {
                if (val >= item[0] && val <= item[1])
                {
                    return true;
                }
            }
            return false;
        }

        private static string RemoveDiacritics(string text)
        {
            return new string(
                text.Normalize(NormalizationForm.FormD)
                    .ToCharArray()
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray());
        }

        /// <summary>
        /// Normalizes the input string (passphrase or mnemonic) as defined by Electrum
        /// </summary>
        /// <param name="text">Text to normalize</param>
        /// <returns>The normalized result</returns>
        public static string Normalize(string text)
        {
            if (text is null)
            {
                return string.Empty;
            }

            string norm = RemoveDiacritics(text.Normalize(NormalizationForm.FormKD).ToLower());
            norm = string.Join(' ', norm.Split(" ", StringSplitOptions.RemoveEmptyEntries));
            // Remove whitespaces between CJK
            var temp = new StringBuilder();
            for (int i = 0; i < norm.Length; i++)
            {
                if (!(char.IsWhiteSpace(norm[i]) && IsCJK(norm[i - 1]) && IsCJK(norm[i + 1])))
                {
                    temp.Append(norm[i]);
                }
            }
            return temp.ToString();
        }

        private MnemonicType GetMnemonicType(string normalizedMn)
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

        /// <summary>
        /// Returns all the 2048 words found in the given word-list.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="wl">Word list to use</param>
        /// <returns>An array of 2048 strings</returns>
        public static string[] GetAllWords(BIP0039.WordLists wl) => BIP0039.GetAllWords(wl);


        private static string[] GetOldWordList()
        {
            string[] allWords = new string[OldWordListLength];
            string path = $"Autarkysoft.Bitcoin.ImprovementProposals.BIP0039WordLists.OldElectrumEnglish.txt";
            Assembly asm = Assembly.GetExecutingAssembly();
            using Stream stream = asm.GetManifestResourceStream(path);
            if (stream != null)
            {
                using StreamReader reader = new StreamReader(stream);
                int i = 0;

                while (!reader.EndOfStream)
                {
                    allWords[i++] = reader.ReadLine();
                }
                if (i != OldWordListLength)
                {
                    throw new ArgumentException("There is something wrong with the embeded word list.");
                }
            }
            else
            {
                throw new ArgumentException("Word list was not found.");
            }

            return allWords;
        }

        private static bool IsOld(string normalized, ReadOnlySpan<string> allWords)
        {
            string[] words = normalized.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                // We don't actually support old versions, just the verification is enough
                //BigInteger val = BigInteger.Zero;
                for (int i = 0; i < words.Length; i += 3)
                {
                    int w1 = allWords.IndexOf(words[i]);
                    int w2 = allWords.IndexOf(words[i + 1]) % OldWordListLength;
                    int w3 = allWords.IndexOf(words[i + 2]) % OldWordListLength;
                    if (w1 < 0 || w2 < 0 || w3 < 0)
                    {
                        return false;
                    }

                    //int x = w1 + n * ((w2 - w1) % n) + n * n * ((w3 - w2) % n);
                    //val += x;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Returns if the given mnemonic is of the old Electrum mnemonic type
        /// </summary>
        /// <remarks>
        /// https://github.com/spesmilo/electrum/blob/63143307f1ed588f9ce0bca4adff71f6baca4277/electrum/old_mnemonic.py
        /// https://github.com/spesmilo/electrum/blob/63143307f1ed588f9ce0bca4adff71f6baca4277/electrum/mnemonic.py#L241-L256
        /// </remarks>
        /// <param name="mnemonic">Mneomonic to check</param>
        /// <returns>True if the given words are of the old Electrum mnemonic type; false otherwise.</returns>
        public static bool IsOld(string mnemonic)
        {
            ReadOnlySpan<string> allWords = GetOldWordList();
            return IsOld(Normalize(mnemonic), allWords);
        }

        private void SetBip32(string passPhrase)
        {
            byte[] password = Encoding.UTF8.GetBytes(Normalize(ToMnemonic()));
            string pass = passPhrase == null ? string.Empty : Normalize(passPhrase);
            byte[] salt = Encoding.UTF8.GetBytes($"electrum{pass}");

            using PBKDF2 kdf = new PBKDF2(2048, new HmacSha512());
            byte[] bytes = kdf.GetBytes(password, salt, 64);

            // Initialize BIP32 here:
            SetEntropy(bytes);
        }


        /// <summary>
        /// Returns the 12 word mnemonic string
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <returns>12-word mnemonic (seed words)</returns>
        public string ToMnemonic()
        {
            if (wordIndexes == null)
                throw new ObjectDisposedException(nameof(BIP0039));

            var sb = new StringBuilder(wordIndexes.Length * 8);
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
