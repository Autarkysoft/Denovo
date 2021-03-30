// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Asymmetric.KeyPairs;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Linq;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Deterministic Entropy From BIP32 Keychains (used in Coldcard)
    /// <para/> https://github.com/bitcoin/bips/blob/master/bip-0085.mediawiki
    /// </summary>
    public sealed class BIP0085 : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="BIP0085"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="FormatException"/>
        /// <param name="masterExtendedKey">Master extended key (xprv string)</param>
        /// <param name="netType">
        /// [Default value = <see cref="NetworkType.MainNet"/>]
        /// The expected network that this extended key belongs to.
        /// </param>
        public BIP0085(string masterExtendedKey, NetworkType netType = NetworkType.MainNet)
        {
            bip32 = new BIP0032(masterExtendedKey, netType);
            ReadOnlySpan<byte> empty = new byte[4];
            if (!empty.SequenceEqual(bip32.ChildNumber) || !empty.SequenceEqual(bip32.ParentFingerPrint))
            {
                throw new ArgumentException("BIP-85 is only defined for master extended keys.", nameof(masterExtendedKey));
            }
            hmac = new HmacSha512();
        }


        private BIP0032 bip32;
        private HmacSha512 hmac;

        private const uint FirstIndex = BIP0032.HardenedIndex + 83696968;
        private const uint HardenedIndex = BIP0032.HardenedIndex;
        // TODO: hardcode this value?
        private readonly byte[] HmacKey = Encoding.UTF8.GetBytes("bip-entropy-from-k");


        /// <summary>
        /// Derives a 512-bit entropy from the BIP-32 instance based on the given path.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="path">Path to use (all indexes must be hardened)</param>
        /// <returns>512-bit entropy</returns>
        public byte[] DeriveEntropy(BIP0032Path path)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0085));
            if (path is null)
                throw new ArgumentNullException(nameof(path), "Path can not be null.");
            if (path.Indexes.Any(x => x < HardenedIndex))
                throw new ArgumentException("All indexes inside the given path must be hardened.", nameof(path));

            // BIP-32 treats paths differently, meaning path is the "path" of the child extended key not the child key.
            // Since BIP-85 treats paths as the "path" of the child key itself it must be modified here.
            var path2 = new BIP0032Path(path.Indexes.AsSpan(0, path.Indexes.Length - 1).ToArray());
            using PrivateKey key = bip32.GetPrivateKeys(path2, 1, path.Indexes[^1])[0];
            return hmac.ComputeHash(key.ToBytes(), HmacKey);
        }

        /// <summary>
        /// Derives upto 512-bit entropy from the BIP-32 instance using given parameters and 
        /// path = m/83696968'/39'/{language}'/{words}'/{index}' to be used in <see cref="BIP0039"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="language">Wordlist (language) to use</param>
        /// <param name="wordLen">Number of words in the BIP-39 mnemonic (∈{12, 15, 18, 21, 24})</param>
        /// <param name="index">Index of the derived key (last index in path)</param>
        /// <returns>Up to 512-bit entropy</returns>
        public byte[] DeriveEntropyBip39(BIP0039.WordLists language, int wordLen, uint index)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0085));

            int entLen = wordLen switch
            {
                12 => 16,
                15 => 20,
                18 => 24,
                21 => 28,
                24 => 32,
                _ => throw new ArgumentException("Invalid seed length. It should be ∈{12, 15, 18, 21, 24}"),
            };

            uint lang = language switch
            {
                BIP0039.WordLists.English => 0,
                BIP0039.WordLists.ChineseSimplified => 4,
                BIP0039.WordLists.ChineseTraditional => 5,
                BIP0039.WordLists.French => 6,
                BIP0039.WordLists.Italian => 7,
                BIP0039.WordLists.Japanese => 1,
                BIP0039.WordLists.Korean => 2,
                BIP0039.WordLists.Spanish => 3,
                BIP0039.WordLists.Czech => 8,
                _ => throw new ArgumentException("Word-list is not defined.", nameof(language)),
            } + HardenedIndex;

            var path = new BIP0032Path(FirstIndex, 39 + HardenedIndex, lang, (uint)wordLen + HardenedIndex, index + HardenedIndex);
            Span<byte> ent = DeriveEntropy(path);
            return ent.Slice(0, entLen).ToArray();
        }

        /// <summary>
        /// Derives a 256-bit entropy from the BIP-32 instance using given parameters and 
        /// path = m/83696968'/2'/{index}' to be used in `hdseed` for Bitcoin Core wallets.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="index">Index of the derived key (last index in path)</param>
        /// <returns>256-bit entropy</returns>
        public byte[] DeriveEntropyHdSeedWif(uint index)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0085));

            Span<byte> ent = DeriveEntropy(new BIP0032Path(FirstIndex, 2 + HardenedIndex, index + HardenedIndex));
            return ent.Slice(0, 32).ToArray();
        }

        /// <summary>
        /// Derives an extended private key from the BIP-32 instance using given parameters and 
        /// path = m/83696968'/32'/{index}' to be used in <see cref="BIP0032"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="index">Index of the derived key (last index in path)</param>
        /// <returns>Base-58 encoded extended private key</returns>
        public string DeriveEntropyXprv(uint index)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0085));

            Span<byte> b64 = DeriveEntropy(new BIP0032Path(FirstIndex, 32 + HardenedIndex, index + HardenedIndex));
            FastStream stream = new FastStream(BIP0032.ExtendedKeyLength);
            stream.Write(new byte[] { 0x04, 0x88, 0xad, 0xe4, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            stream.Write(b64.Slice(0, 32).ToArray());
            stream.Write((byte)0);
            stream.Write(b64.Slice(32, 32).ToArray());

            return Base58.EncodeWithChecksum(stream.ToByteArray());
        }

        /// <summary>
        /// Derives an arbitrary length entropy from the BIP-32 instance using given parameters and 
        /// path = m/83696968'/128169'/{num_bytes}'/{index}' to be used as raw bytes.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="byteLen"></param>
        /// <param name="index">Index of the derived key (last index in path)</param>
        /// <returns>Hexadecimal entropy</returns>
        public string DeriveEntropyHex(int byteLen, uint index)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0085));
            if (byteLen < 16 || byteLen > 64)
                throw new ArgumentOutOfRangeException(nameof(byteLen), "Byte length must be between 16 and 64.");

            var path = new BIP0032Path(FirstIndex, 128169 + HardenedIndex, (uint)byteLen + HardenedIndex, index + HardenedIndex);
            return DeriveEntropy(path).ToBase16();
        }


        private bool isDisposed;
        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                if (!(bip32 is null))
                    bip32.Dispose();
                bip32 = null;

                if (!(hmac is null))
                    hmac.Dispose();
                hmac = null;
            }

            isDisposed = true;
        }
    }
}
