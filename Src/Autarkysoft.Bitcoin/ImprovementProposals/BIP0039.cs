// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Cryptography.KeyDerivationFunctions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Mnemonic code for generating deterministic keys. Inherits from <see cref="BIP0032"/>.
    /// <para/> https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki
    /// </summary>
    public class BIP0039 : BIP0032
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BIP0039"/> with the given entropy, world list and the passphrase.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="entropy">Entropy to use (must be 16, 24, 28 or 32 bytes)</param>
        /// <param name="wl">[Defaultvalue = <see cref="WordLists.English"/> Word list to use</param>
        /// <param name="passPhrase">
        /// [Default value = null] Optional passphrase to use for computing <see cref="BIP0032"/> entropy
        /// </param>
        public BIP0039(byte[] entropy, WordLists wl = WordLists.English, string passPhrase = null)
        {
            if (entropy == null)
                throw new ArgumentNullException(nameof(entropy), "Entropy can not be null.");
            if (!allowedEntropyLengths.Contains(entropy.Length))
                throw new ArgumentOutOfRangeException(nameof(entropy), "Entropy must be 16 or 20 or 24 or 28 or 32 bytes.");

            allWords = GetAllWords(wl);
            SetWordsFromEntropy(entropy);
            SetBip32(passPhrase);
        }


        /// <summary>
        /// Initializes a new instance of <see cref="BIP0039"/> with a randomly generated entropy of given
        /// <paramref name="entropySize"/> using the given <see cref="IRandomNumberGenerator"/> instance, 
        /// world list and an the passphrase.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="rng">Random number generator to use</param>
        /// <param name="entropySize">
        /// Size of the entropy which determines number of words in final mnemonic.
        /// Size must be 16, 20, 24, 28 or 32 which results in 12, 15, 18, 21, and 24 words respectively.
        /// </param>
        /// <param name="wl">[Defaultvalue = <see cref="WordLists.English"/> Word list to use</param>
        /// <param name="passPhrase">
        /// [Default value = null] Optional passphrase to use for computing <see cref="BIP0032"/> entropy
        /// </param>
        public BIP0039(IRandomNumberGenerator rng, int entropySize, WordLists wl = WordLists.English, string passPhrase = null)
        {
            if (rng is null)
                throw new ArgumentNullException(nameof(rng), "Random number generator can not be null.");
            if (!allowedEntropyLengths.Contains(entropySize))
                throw new ArgumentOutOfRangeException(nameof(entropySize), "Entropy must be 16 or 20 or 24 or 28 or 32 bytes.");

            allWords = GetAllWords(wl);

            byte[] entropy = new byte[entropySize];
            rng.GetBytes(entropy);
            SetWordsFromEntropy(entropy);

            SetBip32(passPhrase);
        }


        /// <summary>
        /// Initializes a new instance of <see cref="BIP0039"/> with the given mnemonic, world list and the passphrase.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="mnemonic">Mnemonic (should be 12, 15, 18, 21 or 24 words)</param>
        /// <param name="wl">[Defaultvalue = <see cref="WordLists.English"/> Word list to use</param>
        /// <param name="passPhrase">
        /// [Default value = null] Optional passphrase to use for computing <see cref="BIP0032"/> entropy
        /// </param>
        public BIP0039(string mnemonic, WordLists wl = WordLists.English, string passPhrase = null)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
                throw new ArgumentNullException(nameof(mnemonic), "Seed can not be null or empty!");
            allWords = GetAllWords(wl);

            string[] words = mnemonic.Normalize(NormalizationForm.FormKD)
                                     .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (!words.All(x => allWords.Contains(x)))
            {
                throw new ArgumentException(nameof(mnemonic), "Seed has invalid words.");
            }
            if (!allowedWordLengths.Contains(words.Length))
            {
                throw new FormatException("Invalid seed length. It should be ∈{12, 15, 18, 21, 24}");
            }

            wordIndexes = new uint[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                wordIndexes[i] = (uint)Array.IndexOf(allWords, words[i]);
            }

            // Compute and check checksum
            int MS = words.Length;
            int ENTCS = MS * 11;
            int CS = ENTCS % 32;
            int ENT = ENTCS - CS;

            byte[] entropy = new byte[ENT / 8];

            int itemIndex = 0;
            int bitIndex = 0;
            // Number of bits in a word
            int toTake = 8;
            // Indexes are held in a UInt32 but they are only 11 bits
            int maxBits = 11;
            for (int i = 0; i < entropy.Length; i++)
            {
                if (bitIndex + toTake <= maxBits)
                {
                    // All 8 bits are in one item

                    // To take 8 bits (*) out of 00000000 00000000 00000xx* *******x:
                    // 1. Shift right to get rid of extra bits on right, then cast to byte to get rid of the rest
                    // >> maxBits - toTake - bitIndex
                    entropy[i] = (byte)(wordIndexes[itemIndex] >> (3 - bitIndex));
                }
                else
                {
                    // Only a part of 8 bits are in this item, the rest is in the next.
                    // Since items are only 32 bits there is no other possibility (8<32)

                    // To take 8 bits(*) out of [00000000 00000000 00000xxx xxxx****] [00000000 00000000 00000*** *xxxxxxx]:
                    // Take first item at itemIndex [00000000 00000000 00000xxx xxxx****]: 
                    //    * At most 7 bits and at least 1 bit should be taken
                    // 1. Shift left [00000000 00000000 0xxxxxxx ****0000] (<< 8 - (maxBits - bitIndex)) 8-max+bi
                    // 2. Zero the rest of the bits (& (00000000 00000000 00000000 11111111))

                    // Take next item at itemIndex+1 [00000000 00000000 00000*** *xxxxxxx]
                    // 3. Shift right [00000000 00000000 00000000 0000****]
                    // number of bits already taken = maxBits - bitIndex
                    // nuber of bits to take = toTake - (maxBits - bitIndex)
                    // Number of bits on the right to get rid of= maxBits - (toTake - (maxBits - bitIndex))
                    // 4. Add two values to each other using bitwise OR [****0000] | [0000****]
                    entropy[i] = (byte)(((wordIndexes[itemIndex] << (bitIndex - 3)) & 0xff) |
                                         (wordIndexes[itemIndex + 1] >> (14 - bitIndex)));
                }

                bitIndex += toTake;
                if (bitIndex >= maxBits)
                {
                    bitIndex -= maxBits;
                    itemIndex++;
                }
            }

            // Compute and compare checksum:
            // CS is at most 8 bits and it is the remaining bits from the loop above and it is only from last item
            // [00000000 00000000 00000xxx xxxx****]
            // We already know the number of bits here: CS
            // A simple & does the work
            uint mask = (1U << CS) - 1;
            byte expectedChecksum = (byte)(wordIndexes[itemIndex] & mask);

            // Checksum is the "first" CS bits of hash: [****xxxx]
            using Sha256 hash = new Sha256();
            byte[] hashOfEntropy = hash.ComputeHash(entropy);
            byte actualChecksum = (byte)(hashOfEntropy[0] >> (8 - CS));

            if (expectedChecksum != actualChecksum)
            {
                Array.Clear(wordIndexes, 0, wordIndexes.Length);
                wordIndexes = null;

                throw new FormatException("Wrong checksum.");
            }

            SetBip32(passPhrase);
        }



        private readonly int[] allowedEntropyLengths = { 16, 20, 24, 28, 32 };
        private static readonly int[] allowedWordLengths = { 12, 15, 18, 21, 24 };
        private uint[] wordIndexes;
        private string[] allWords;

        /// <summary>
        /// Language of the list of words used in generation of mnemonic
        /// </summary>
        public enum WordLists
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            English,
            ChineseSimplified,
            ChineseTraditional,
            French,
            Italian,
            Japanese,
            Korean,
            Spanish,
            Czech,
            Portuguese
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        private void SetWordsFromEntropy(byte[] entropy)
        {
            using Sha256 hash = new Sha256();
            byte[] hashOfEntropy = hash.ComputeHash(entropy);

            int ENT = entropy.Length * 8;
            int CS = ENT / 32;
            int ENTCS = ENT + CS;
            int MS = ENTCS / 11;

            // To convert a given entropy to mnemonic (word list) it must be converted to binary and then
            // split into 11-bit chunks each representing an index inside the list of all words (2048 total).
            // Here we use a UInt32 array to hold the bits and select each 11 bits from that array

            // To make the entropy length divisible by 11 it needs to be padded with a checksum of CS bits first
            // Extra bytes are added to make conversion and selection easier and the extra bits will be ignored in final
            // selection step.
            int arrSize = (int)Math.Ceiling((double)ENTCS / 32);
            int fillingBytes = (arrSize * 4) - entropy.Length;
            byte[] ba = entropy.ConcatFast(hashOfEntropy.SubArray(0, fillingBytes));

            uint[] bits = new uint[arrSize];
            for (int i = 0, j = 0; i < ba.Length; i += 4, j++)
            {
                bits[j] = (uint)(ba[i + 3] | (ba[i + 2] << 8) | (ba[i + 1] << 16) | (ba[i] << 24));
            }

            int itemIndex = 0;
            int bitIndex = 0;
            // Number of bits in a word
            int toTake = 11;
            // UInt32 is 32 bit!
            int maxBits = 32;
            wordIndexes = new uint[MS];
            for (int i = 0; i < MS; i++)
            {
                if (bitIndex + toTake <= maxBits)
                {
                    // All 11 bits are in one item

                    // To take astrix out of xx***xxx:
                    // 1. Shift left bitIndex times to get rid of values on the left: ***xxx00 (<< bitIndex)
                    // 2. Shift right the same amount to put bits back where they were: 00***xxx (>> bitIndex)
                    // 3. Shift right to get rid of the extra values on the right: 00000*** (>> maxBits - (bitIndex + toTake))
                    // 2+3= bitIndex + maxBits - bitIndex - toTake
                    wordIndexes[i] = (bits[itemIndex] << bitIndex) >> (maxBits - toTake);
                }
                else
                {
                    // Only a part of 11 bits are in this item, the rest is in the next.
                    // Since items are only 32 bits there is no other possibility (11<32)

                    // To take astrix out of [xxxxxx**] [***xxxxx]:
                    // Take first item at itemIndex [xxxxxx**]: 
                    // 1. Shift left bitIndex times to to get rid of values on the right: **000000 (<< bitIndex)
                    // 2. Shift right the same amount to put bits back where they were: 000000** (>> bitIndex)
                    // 3. Shift left to open up room for remaining bits: 000**000 (<< toTake - (maxBits - bitIndex))
                    // 2+3= bitIndex - toTake + maxBits - bitIndex

                    // Take next item at itemIndex+1 [***xxxxx]:
                    // 4. Shift right to get rid of extra values: 00000***
                    // Number of bits already taken= maxBits - bitIndex
                    // Number of bits to take = toTake - (maxBits - bitIndex)
                    // Number of bits on the right to get rid of= maxBits - (toTake - (maxBits - bitIndex))

                    // 5. Add two values to each other using bitwise OR (000**000 | 00000*** = 000*****)
                    wordIndexes[i] = ((bits[itemIndex] << bitIndex) >> (maxBits - toTake)) |
                                     (bits[itemIndex + 1] >> (maxBits - toTake + maxBits - bitIndex));
                }

                bitIndex += toTake;
                if (bitIndex >= maxBits)
                {
                    bitIndex -= maxBits;
                    itemIndex++;
                }
            }
        }

        /// <summary>
        /// Returns all the 2048 words found in the given word-list.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="wl">Word list to use</param>
        /// <returns>An array of 2048 strings</returns>
        public static string[] GetAllWords(WordLists wl)
        {
            if (!Enum.IsDefined(typeof(WordLists), wl))
                throw new ArgumentException("Given word list is not defined.");

            string path = $"Autarkysoft.Bitcoin.ImprovementProposals.BIP0039WordLists.{wl}.txt";
            Assembly asm = Assembly.GetExecutingAssembly();
            using Stream stream = asm.GetManifestResourceStream(path);
            if (stream != null)
            {
                using StreamReader reader = new StreamReader(stream);
                int i = 0;
                string[] result = new string[2048];
                while (!reader.EndOfStream)
                {
                    result[i++] = reader.ReadLine();
                }
                if (i != 2048)
                {
                    throw new ArgumentException("There is something wrong with the embeded word list.");
                }

                return result;
            }
            else
            {
                throw new ArgumentException("Word list was not found.");
            }
        }


        /// <summary>
        /// Returns Levenshtein distance between each 2 words in a jagged array (2-dimentional) for the given word list.
        /// <para/>Note: items at index [n,n] are set to -1 which indicates distance of each word with itself. This creates
        /// a distiction between comparing a word with itself or with a duplicate. In other words if the value at 
        /// index [n,m] where n!=m was 0 the list contains a duplicate.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="words">List of 2048 words to use</param>
        /// <returns>Levenshtein distance between each word compared to others</returns>
        public static int[,] LevenshteinDistance(string[] words)
        {
            if (words == null)
                throw new ArgumentNullException(nameof(words), "Word list can not be null.");
            if (words.Length != 2048)
                throw new ArgumentOutOfRangeException(nameof(words), "Word list must contain 2048 items.");

            int[,] result = new int[2048, 2048];

            for (int i = 0; i < words.Length; i++)
            {
                for (int j = 0; j < words.Length; j++)
                {
                    if (i == j)
                    {
                        result[i, j] = -1;
                    }
                    else
                    {
                        result[i, j] = words[i].LevenshteinDistance(words[j]);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc cref="LevenshteinDistance(string[])"/>
        /// <exception cref="ArgumentException"/>
        public static int[,] LevenshteinDistance(WordLists wl) => LevenshteinDistance(GetAllWords(wl));


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
