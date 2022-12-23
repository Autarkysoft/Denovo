// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs
{
    /// <summary>
    /// These are keys used in some physical bitcoins.
    /// <para/>https://en.bitcoin.it/wiki/Mini_private_key_format
    /// </summary>
    [Obsolete]
    public class MiniPrivateKey : PrivateKey
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MiniPrivateKey"/> with a randomly generated key 
        /// using the given <see cref="IRandomNumberGenerator"/> instance.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="rng">Random number generator to use</param>
        public MiniPrivateKey(IRandomNumberGenerator rng)
        {
            if (rng is null)
                throw new ArgumentNullException("Random number generator can not be null.");

            // This is the way Casascius does it.
            // https://github.com/casascius/Bitcoin-Address-Utility/blob/e493d51e4a1da7536fc8e8aea38eeaee38abf4cb/Model/MiniKeyPair.cs#L54-L80
            // Create a random 256 bit key. It will only be used for its characters
            byte[] tempBa = new byte[32 + 1];
            rng.GetBytes(tempBa);
            tempBa[0] = GetWifFirstByte(NetworkType.MainNet);
            string b58 = 'S' + Base58.EncodeWithChecksum(tempBa).Replace("1", "").Substring(4, 29);

            char[] chars = b58.ToCharArray();
            char[] charstest = (b58 + "?").ToCharArray();
            while (hash.ComputeHash(Encoding.UTF8.GetBytes(charstest))[0] != 0)
            {
                // As long as key doesn't pass typo check, increment it.
                for (int i = chars.Length - 1; i >= 0; i--)
                {
                    char c = chars[i];
                    if (c == '9')
                    {
                        charstest[i] = chars[i] = 'A';
                        break;
                    }
                    else if (c == 'H')
                    {
                        charstest[i] = chars[i] = 'J';
                        break;
                    }
                    else if (c == 'N')
                    {
                        charstest[i] = chars[i] = 'P';
                        break;
                    }
                    else if (c == 'Z')
                    {
                        charstest[i] = chars[i] = 'a';
                        break;
                    }
                    else if (c == 'k')
                    {
                        charstest[i] = chars[i] = 'm';
                        break;
                    }
                    else if (c == 'z')
                    {
                        charstest[i] = chars[i] = '2';
                        // No break - let loop increment prior character.
                    }
                    else
                    {
                        charstest[i] = chars[i] = ++c;
                        break;
                    }
                }
            }

            smallBytes = Encoding.UTF8.GetBytes(chars);
            SetBytes(hash.ComputeHash(smallBytes));
        }


        /// <summary>
        /// Initializes a new instance of <see cref="MiniPrivateKey"/> with the given key string.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="FormatException"/>
        /// <param name="miniKeyString">Key to use (should be 22, 26, or 30 character long and start with 'S')</param>
        public MiniPrivateKey(string miniKeyString)
        {
            if (string.IsNullOrWhiteSpace(miniKeyString))
                throw new ArgumentNullException(nameof(miniKeyString), "Key can not be null or empty.");
            if (miniKeyString.Length != 22 && miniKeyString.Length != 26 && miniKeyString.Length != 30)
                throw new ArgumentOutOfRangeException("Key must be 22 or 26 or 30 character long.", nameof(miniKeyString));
            if (!miniKeyString.StartsWith("S"))
                throw new FormatException("Key must start with letter 'S'.");
            if (!miniKeyString.All(x => b58Chars.Contains(x)))
                throw new FormatException("Invalid character was found in given key.");


            byte[] bytes = Encoding.UTF8.GetBytes(miniKeyString);
            if (hash.ComputeHash(bytes.AppendToEnd((byte)'?'))[0] != 0)
            {
                throw new FormatException("Invalid key (wrong hash).");
            }

            smallBytes = bytes;
            SetBytes(hash.ComputeHash(smallBytes));
        }



        private const string b58Chars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        private Sha256 hash = new Sha256();
        private byte[] smallBytes;



        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        /// <returns>The mini key string.</returns>
        public override string ToString()
        {
            CheckDisposed();
            return Encoding.UTF8.GetString(smallBytes);
        }



        /// <summary>
        /// Releases the resources used by the <see cref="MiniPrivateKey"/> class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (hash != null)
                    hash.Dispose();
                hash = null;

                if (smallBytes != null)
                    Array.Clear(smallBytes, 0, smallBytes.Length);
                smallBytes = null;
            }

            base.Dispose(disposing);
        }
    }
}
