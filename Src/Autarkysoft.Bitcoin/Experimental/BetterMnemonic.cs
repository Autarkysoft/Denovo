// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Cryptography.KeyDerivationFunctions;
using Autarkysoft.Bitcoin.ImprovementProposals;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.Experimental
{
    /// <summary>
    /// An experimental idea to be used as an alternative to existing mnemonic proposals.
    /// <para>
    /// 4 bit version | 4 bit depth | 32 bit index * depth | CompactInt entropy length | entropy | checksum (pad)
    /// </para>
    /// </summary>
    public class BetterMnemonic : BIP0032
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BetterMnemonic"/> using the given arguments.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="entropy">Entropy to use</param>
        /// <param name="path">Child key derivation path</param>
        /// <param name="creationTime">Creation time as a <see cref="LockTime"/></param>
        /// <param name="wl">Word list to use</param>
        /// <param name="passPhrase">Optional passphrase to extend the key</param>
        public BetterMnemonic(byte[] entropy, BIP0032Path path, LockTime creationTime,
                              BIP0039.WordLists wl = BIP0039.WordLists.English, string passPhrase = null)
        {
            if (entropy == null)
                throw new ArgumentNullException(nameof(entropy), "Entropy can not be null.");
            if (!BIP0039.allowedEntropyLengths.Contains(entropy.Length))
                throw new ArgumentOutOfRangeException(nameof(entropy), "Entropy must be 16 or 20 or 24 or 28 or 32 bytes.");

            DerivationPath = path;
            Locktime = creationTime;
            allWords = BIP0039.GetAllWords(wl);
            this.entropy = entropy;
            SetBip32(passPhrase);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BetterMnemonic"/> using the given arguments.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="rng">Random number generator to use</param>
        /// <param name="entropySize">Entropy size</param>
        /// <param name="path">Child key derivation path</param>
        /// <param name="creationTime">Creation time as a <see cref="LockTime"/></param>
        /// <param name="wl">Word list to use</param>
        /// <param name="passPhrase">Optional passphrase to extend the key</param>
        public BetterMnemonic(IRandomNumberGenerator rng, int entropySize, BIP0032Path path, LockTime creationTime,
                              BIP0039.WordLists wl = BIP0039.WordLists.English, string passPhrase = null)
        {
            if (rng is null)
                throw new ArgumentNullException(nameof(rng), "Random number generator can not be null.");
            if (!BIP0039.allowedEntropyLengths.Contains(entropySize))
                throw new ArgumentOutOfRangeException(nameof(entropySize), "Entropy must be 16 or 20 or 24 or 28 or 32 bytes.");

            DerivationPath = path;
            Locktime = creationTime;
            allWords = BIP0039.GetAllWords(wl);
            entropy = new byte[entropySize];
            rng.GetBytes(entropy);
            SetBip32(passPhrase);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BetterMnemonic"/> using the given arguments.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <param name="mnemonic">Mnemonic to use</param>
        /// <param name="wl">Word list to use</param>
        /// <param name="passPhrase">Optional passphrase to extend the key</param>
        public BetterMnemonic(string mnemonic, BIP0039.WordLists wl = BIP0039.WordLists.English, string passPhrase = null)
        {
            if (string.IsNullOrWhiteSpace(mnemonic))
                throw new ArgumentNullException(nameof(mnemonic), "Seed can not be null or empty!");
            allWords = BIP0039.GetAllWords(wl);

            string[] words = mnemonic.Normalize(NormalizationForm.FormKD)
                                     .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (!words.All(x => allWords.Contains(x)))
            {
                throw new ArgumentException("Seed has invalid words.", nameof(mnemonic));
            }

            uint[] wordIndexes = new uint[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                wordIndexes[i] = (uint)Array.IndexOf(allWords, words[i]);
            }

            int itemIndex = 0;
            int bitIndex = 0;
            byte verDepth = Read(wordIndexes, ref itemIndex, ref bitIndex, 8);
            int version = verDepth >> 4;
            int depth = verDepth & 0b00001111;
            uint[] pathIndexes = new uint[depth];
            for (int i = 0; i < pathIndexes.Length; i++)
            {
                byte a1 = Read(wordIndexes, ref itemIndex, ref bitIndex, 8);
                byte a2 = Read(wordIndexes, ref itemIndex, ref bitIndex, 8);
                byte a3 = Read(wordIndexes, ref itemIndex, ref bitIndex, 8);
                byte a4 = Read(wordIndexes, ref itemIndex, ref bitIndex, 8);
                pathIndexes[i] = (uint)(a1 | a2 << 8 | a3 << 16 | a4 << 24);
            }
            DerivationPath = new BIP0032Path(pathIndexes);

            byte entLen = Read(wordIndexes, ref itemIndex, ref bitIndex, 8);

            entropy = new byte[entLen];
            for (int i = 0; i < entropy.Length; i++)
            {
                entropy[i] = Read(wordIndexes, ref itemIndex, ref bitIndex, 8);
            }

            // Compute and compare checksum:
            var stream = new FastStream(100);
            Debug.Assert(version <= 0b00001111);
            Debug.Assert(depth <= 0b00001111);
            stream.Write((byte)(version << 4 | depth));
            foreach (var item in DerivationPath.Indexes)
            {
                stream.Write(item);
            }
            stream.WriteWithCompactIntLength(entropy);

            using Sha256 sha = new Sha256();
            byte[] hash = sha.ComputeHash(stream.ToByteArray());

            int rem = wordIndexes.Length * 11 - (itemIndex * 11) - bitIndex;
            int hashIndex = 0;
            if (rem > 8)
            {
                byte cs1 = Read(wordIndexes, ref itemIndex, ref bitIndex, 8);
                if (hash[hashIndex++] != cs1)
                {
                    throw new FormatException("Invalid checksum.");
                }

                rem -= 8;
            }

            byte cs2 = (byte)((wordIndexes[itemIndex] << (bitIndex - 3)) & 0xff);
            if ((hash[hashIndex] & (0xff << (8 - rem))) != cs2)
            {
                throw new FormatException("Invalid checksum.");
            }

            SetBip32(passPhrase);
        }

        private byte Read(uint[] wordIndexes, ref int itemIndex, ref int bitIndex, int toTake)
        {
            int maxBits = 11;
            byte result;
            if (bitIndex + toTake <= maxBits)
            {
                result = (byte)(wordIndexes[itemIndex] >> (3 - bitIndex));
            }
            else
            {
                result = (byte)(((wordIndexes[itemIndex] << (bitIndex - 3)) & 0xff) |
                                     (wordIndexes[itemIndex + 1] >> (14 - bitIndex)));
            }

            bitIndex += toTake;
            if (bitIndex >= maxBits)
            {
                bitIndex -= maxBits;
                itemIndex++;
            }

            return result;
        }


        /// <summary>
        /// The derivation path of the child extended keys
        /// </summary>
        public BIP0032Path DerivationPath { get; }
        /// <summary>
        /// The time when this key was first created
        /// </summary>
        public LockTime Locktime { get; }
        private byte[] entropy;

        private string[] allWords;



        private uint[] Convert8To11(FastStream stream)
        {
            using Sha256 sha = new Sha256();
            byte[] hash = sha.ComputeHash(stream.ToByteArray());

            int bitSize = stream.GetSize() * 8;
            int cs = 11 - (bitSize % 11);
            if (cs < 4)
            {
                cs += 11;
            }
            Debug.Assert(cs >= 4 && cs <= 14);
            stream.Write(hash, 0, cs > 8 ? 2 : 1);

            bitSize += cs;
            int wordCount = bitSize / 11;
            Debug.Assert(bitSize % 11 == 0);

            byte[] ba = stream.ToByteArray();
            uint[] bits = new uint[(int)Math.Ceiling((double)bitSize / 32)];
            for (int i = 0, j = 0; j < bits.Length; i += 4, j++)
            {
                bits[j] = (uint)(ba[i + 3] | (ba[i + 2] << 8) | (ba[i + 1] << 16) | (ba[i] << 24));
            }

            int itemIndex = 0;
            int bitIndex = 0;
            // Number of bits in a word
            int toTake = 11;
            // UInt32 is 32 bit!
            int maxBits = 32;
            uint[] wordIndexes = new uint[wordCount];
            for (int i = 0; i < wordIndexes.Length; i++)
            {
                if (bitIndex + toTake <= maxBits)
                {
                    wordIndexes[i] = (bits[itemIndex] << bitIndex) >> (maxBits - toTake);
                }
                else
                {
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

            return wordIndexes;
        }


        /// <summary>
        /// Returns mnemonic (seed words)
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <returns>mnemonic (seed words)</returns>
        public string ToMnemonic()
        {
            if (entropy == null)
                throw new ObjectDisposedException(nameof(BetterMnemonic));

            var stream = new FastStream(100);
            int version = 1;
            int depth = DerivationPath.Indexes.Length;
            Debug.Assert(version <= 0b00001111);
            Debug.Assert(depth <= 0b00001111);
            stream.Write((byte)(version << 4 | depth));
            foreach (var item in DerivationPath.Indexes)
            {
                stream.Write(item);
            }
            stream.WriteWithCompactIntLength(entropy);

            uint[] wordIndexes = Convert8To11(stream);

            StringBuilder sb = new StringBuilder(wordIndexes.Length * 8);
            for (int i = 0; i < wordIndexes.Length; i++)
            {
                sb.Append($"{allWords[wordIndexes[i]]} ");
            }

            // no space at the end.
            sb.Length--;
            return sb.ToString();
        }


        private void SetBip32(string passPhrase)
        {
            byte[] salt = Encoding.UTF8.GetBytes($"BetterMnemonic{passPhrase?.Normalize(NormalizationForm.FormKD)}");

            using PBKDF2 kdf = new PBKDF2(2048, new HmacSha512());
            byte[] bytes = kdf.GetBytes(entropy, salt, 64);

            // Initialize BIP32 here:
            SetEntropy(bytes);
        }


        /// <summary>
        /// Releases the resources used by this instance
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (entropy != null)
                    Array.Clear(entropy, 0, entropy.Length);
                entropy = null;

                // Dispose the big field (2048 items)
                if (allWords != null)
                    Array.Clear(allWords, 0, allWords.Length);
                allWords = null;
            }

            base.Dispose(disposing);
        }
    }
}
