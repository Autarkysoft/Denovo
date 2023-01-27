// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography;
using Autarkysoft.Bitcoin.Cryptography.EllipticCurve;
using Autarkysoft.Bitcoin.Cryptography.Hashing;
using Autarkysoft.Bitcoin.Encoders;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Autarkysoft.Bitcoin.ImprovementProposals
{
    /// <summary>
    /// Hierarchical Deterministic Wallets. Implements <see cref="IDisposable"/>.
    /// <para/>https://github.com/bitcoin/bips/blob/master/bip-0032.mediawiki
    /// </summary>
    public class BIP0032 : IDisposable
    {
        /// <summary>
        /// An empty constructor to let this class be inherited.
        /// </summary>
        protected BIP0032() { }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032"/> with the given entropy.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="entropy">Entropy to use (must be between 128 and 512 bits; ie. 16 and 64 bytes)</param>
        public BIP0032(byte[] entropy)
        {
            if (entropy == null)
                throw new ArgumentNullException(nameof(entropy), "Entropy can not be null.");
            if (entropy.Length < MinEntropyLength || entropy.Length > MaxEntropyLength)
                throw new ArgumentOutOfRangeException(nameof(entropy), "Entropy must be between 16 and 64 bytes.");
            SetEntropy(entropy);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032"/> with an entropy derived from the given
        /// <see cref="IRandomNumberGenerator"/> and the entropy size.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="rng">RNG to use</param>
        /// <param name="entropySize">
        /// [Default value = 32]
        /// Size of the entropy (must be between 128 and 512 bits; ie. 16 and 64 bytes).
        /// </param>
        public BIP0032(IRandomNumberGenerator rng, int entropySize = 32)
        {
            if (rng is null)
                throw new ArgumentNullException(nameof(rng), "Random number generator can not be null.");
            if (entropySize < MinEntropyLength || entropySize > MaxEntropyLength)
                throw new ArgumentOutOfRangeException(nameof(entropySize), "Entropy size must be between 16 and 64 bytes.");


            byte[] entropy = new byte[entropySize];
            int count = 0;
            while (count <= Constants.RngRetryCount)
            {
                try
                {
                    rng.GetBytes(entropy);
                    SetEntropy(entropy);
                    break;
                }
                catch (Exception)
                {
                    // We should never land here for curves like secp256k1 because of how close N and P are.
                    count++;
                    // This will only throw if the RNG is broken.
                    if (count == Constants.RngRetryCount)
                        throw new ArgumentException(Err.BadRNG);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BIP0032"/> using the given base-58 encoded 
        /// extended public or private key string.
        /// <para/>This will set the <see cref="ExtendedKeyType"/> property that can be used by caller to decide
        /// what derivation path and address type to use for child keys.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException">If given private key is outside of curve range.</exception>
        /// <exception cref="FormatException"/>
        /// <param name="extendedKey">Base-58 encoded extended key to use</param>
        public BIP0032(string extendedKey)
        {
            if (string.IsNullOrWhiteSpace(extendedKey))
                throw new ArgumentNullException(nameof(extendedKey), "Extended key can not be null or empty.");


            byte[] decoded = Base58.DecodeWithChecksum(extendedKey);
            if (decoded.Length != ExtendedKeyLength)
            {
                throw new FormatException($"Extended key length should be {ExtendedKeyLength} bytes " +
                                          $"but it is {decoded.Length} bytes.");
            }

            int version = decoded[3] | (decoded[2] << 8) | (decoded[1] << 16) | (decoded[0] << 24);
            ExtendedKeyDepth = decoded[4];
            ParentFingerPrint = decoded.SubArray(5, 4);
            ChildNumber = decoded.SubArray(9, 4);
            ChainCode = decoded.SubArray(13, 32);
            byte[] key = decoded.SubArray(45, 33);

            bool isPublic;
            if (Enum.IsDefined(typeof(XType), version))
            {
                ExtendedKeyType = (XType)version;
                isPublic = IsPublic(ExtendedKeyType);
            }
            else
            {
                ExtendedKeyType = XType.Unknown;
                isPublic = key[0] != 0;
            }

            if (!isPublic && key[0] != 0)
            {
                throw new FormatException($"The key has an invalid first byte, " +
                    $"it should be 0 for private keys but it is 0x{key[0]:x2}.");
            }

            if (isPublic)
            {
                PrvKey = null;
                if (!Point.TryRead(key, out PubKey))
                {
                    throw new ArgumentOutOfRangeException("public key", "Invalid public key format.");
                }
            }
            else
            {
                // The following line will check if the key is valid and throws ArgumentOutOfRangeException if not
                PrvKey = new PrivateKey(key.SubArray(1));
                PubKey = PrvKey.ToPublicKey(calc);
            }
        }



        /// <summary>
        /// Extended key type set based on first 4 bytes of the base58 encoded string and is used to determine the type of
        /// addresses to derive from this extended key.
        /// </summary>
        /// <remarks>
        /// https://github.com/bitcoin/bips/blob/master/bip-0084.mediawiki#extended-key-version
        /// https://github.com/satoshilabs/slips/blob/master/slip-0132.md#registered-hd-version-bytes
        /// </remarks>
        public enum XType
        {
            /// <summary>
            /// Unknown type
            /// <para/>Returned from constructor for any extended key string that can not be recognized. Should not be used
            /// for (re)encoding this instance to Base-58
            /// </summary>
            Unknown,
            /// <summary>
            /// The original type defined by BIP-32 for extended MainNet private keys starting with xprv
            /// </summary>
            MainNet_xprv = 0x0488ade4,
            /// <summary>
            /// The original type defined by BIP-32 for extended MainNet public keys starting with xpub
            /// </summary>
            MainNet_xpub = 0x0488b21e,
            /// <summary>
            /// Extended MainNet private key starting with yprv defined by BIP-49 to derive P2SH-P2WPKH address types
            /// </summary>
            MainNet_yprv = 0x049d7878,
            /// <summary>
            /// Extended MainNet public key starting with ypub defined by BIP-49 to derive P2SH-P2WPKH address types
            /// </summary>
            MainNet_ypub = 0x049d7cb2,
            /// <summary>
            /// Extended MainNet private key starting with zpub defined by BIP-84 to derive P2WPKH address types
            /// </summary>
            MainNet_zprv = 0x04b2430c,
            /// <summary>
            /// Extended MainNet public key starting with zpub defined by BIP-84 to derive P2WPKH address types
            /// </summary>
            MainNet_zpub = 0x04b24746,
            /// <summary>
            /// Extended MainNet private key starting with Yprv to derive P2SH-P2WSH address types
            /// </summary>
            MainNet_Yprv = 0x0295b005,
            /// <summary>
            /// Extended MainNet public key starting with Ypub to derive P2SH-P2WSH address types
            /// </summary>
            MainNet_Ypub = 0x0295b43f,
            /// <summary>
            /// Extended MainNet private key starting with Zprv to derive P2WSH address types
            /// </summary>
            MainNet_Zprv = 0x02aa7a99,
            /// <summary>
            /// Extended MainNet public key starting with Zpub to derive P2WSH address types
            /// </summary>
            MainNet_Zpub = 0x02aa7ed3,

            /// <summary>
            /// The original type defined by BIP-32 for extended TestNet private keys starting with tprv
            /// </summary>
            TestNet_tprv = 0x04358394,
            /// <summary>
            /// The original type defined by BIP-32 for extended TestNet public keys starting with tpub
            /// </summary>
            TestNet_tpub = 0x043587cf,
            /// <summary>
            /// Extended TestNet private key starting with uprv defined by BIP-49 to derive P2SH-P2WPKH address types
            /// </summary>
            TestNet_uprv = 0x044a4e28,
            /// <summary>
            /// Extended TestNet public key starting with upub defined by BIP-49 to derive P2SH-P2WPKH address types
            /// </summary>
            TestNet_upub = 0x044a5262,
            /// <summary>
            /// Extended TestNet private key starting with vpub defined by BIP-84 to derive P2WPKH address types
            /// </summary>
            TestNet_vprv = 0x045f18bc,
            /// <summary>
            /// Extended TestNet public key starting with vpub defined by BIP-84 to derive P2WPKH address types
            /// </summary>
            TestNet_vpub = 0x045f1cf6,
            /// <summary>
            /// Extended TestNet private key starting with Uprv to derive P2SH-P2WSH address types
            /// </summary>
            TestNet_Uprv = 0x024285b5,
            /// <summary>
            /// Extended TestNet public key starting with Upub to derive P2SH-P2WSH address types
            /// </summary>
            TestNet_Upub = 0x024289ef,
            /// <summary>
            /// Extended TestNet private key starting with Vprv to derive P2WSH address types
            /// </summary>
            TestNet_Vprv = 0x02575048,
            /// <summary>
            /// Extended TestNet public key starting with Vpub to derive P2WSH address types
            /// </summary>
            TestNet_Vpub = 0x02575483,
        }


        /// <summary>
        /// Minimum value for an index to be considered hardened (equal to 2^31)
        /// </summary>
        public const uint HardenedIndex = 0x80000000;
        /// <summary>
        /// Minimum accepted length for the entropy (128 bits)
        /// </summary>
        public const int MinEntropyLength = 16;
        /// <summary>
        /// Maximum accepted length for the entropy (512 bits)
        /// </summary>
        public const int MaxEntropyLength = 64;

        // version(4) + depth(1) + fingerPrint(4) + index(4) + chain(32) + key(33)
        internal const int ExtendedKeyLength = 78;

        // TODO: hardcode this value?
        private readonly byte[] MasterKeyHashKey = Encoding.UTF8.GetBytes("Bitcoin seed");

        private HmacSha512 hmac = new HmacSha512();
        private readonly Calc calc = new Calc();

        private byte ExtendedKeyDepth;
        internal byte[] ParentFingerPrint;
        internal byte[] ChildNumber;
        private byte[] ChainCode;
        private PrivateKey PrvKey;
        private Point PubKey;

        /// <summary>
        /// Gets or sets the extended key type
        /// </summary>
        public XType ExtendedKeyType { get; set; }

        private bool IsPublic(XType xType) => xType == XType.MainNet_xpub || xType == XType.MainNet_Ypub ||
            xType == XType.MainNet_ypub || xType == XType.MainNet_Zpub || xType == XType.MainNet_zpub ||
            xType == XType.TestNet_tpub || xType == XType.TestNet_upub || xType == XType.TestNet_Upub ||
            xType == XType.TestNet_Vpub || xType == XType.TestNet_vpub;

        /// <summary>
        /// Initializes this instance using the given entropy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Is thrown if the left 32 byte chunk of the HMAC result is not a valid private key.
        /// </exception>
        /// <param name="entropy">Entropy to use (it must be between 128 and 512 bits; ie. 16 and 64 bytes) </param>
        protected void SetEntropy(byte[] entropy)
        {
            HmacAndSplitData(entropy, MasterKeyHashKey, out byte[] il, out byte[] ir);

            // Master key doesn't have the (+previous private key % N), we use 'il' itself
            // The following line will check if the key is valid and throws ArgumentOutOfRangeException if not
            PrvKey = new PrivateKey(il);
            PubKey = PrvKey.ToPublicKey(calc);
            ChainCode = ir;

            ExtendedKeyDepth = 0;
            ParentFingerPrint = new byte[4];
            ChildNumber = new byte[4];
        }


        private void HmacAndSplitData(byte[] data, byte[] key, out byte[] left32, out byte[] right32)
        {
            byte[] hash = hmac.ComputeHash(data, key);
            left32 = hash.SubArray(0, 32);
            right32 = hash.SubArray(32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsHardendedIndex(uint i) => (i & HardenedIndex) != 0;


        /// <summary>
        /// Derives and returns the requested number of child private keys using the given derivation path.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="path">Derivation path</param>
        /// <param name="count">Number of keys to return</param>
        /// <param name="startIndex">[Default value = 0] Starting index of the child key</param>
        /// <param name="step">[Default value = 1] Steps between each key (minimum will always be 1)</param>
        /// <returns>An array of derived child private keys</returns>
        public PrivateKey[] GetPrivateKeys(BIP0032Path path, uint count, uint startIndex = 0, uint step = 1)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0032));
            if (path is null)
                throw new ArgumentNullException(nameof(path), "Path can not be null!");
            if (PrvKey is null)
                throw new ArgumentNullException(nameof(PrvKey), "Can not get child private keys from extended public key.");
            if (ExtendedKeyDepth == byte.MaxValue || ExtendedKeyDepth + path.Indexes.Length + 1 > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(ExtendedKeyDepth), "Can not get children since " +
                    "depth will be bigger than 1 byte");

            // Two quick fixes:
            if (count == 0)
                return null;
            if (step < 1)
                step = 1;

            // First start deriving the extended keys for each index
            byte[] prevPrvBa = PrvKey.ToBytes();
            Scalar8x32 prevPrvInt = PrvKey.ToScalar();
            byte[] prevPubBa = PubKey.ToByteArray(true).ToArray();
            byte[] tempCC = ChainCode;
            byte[] tempLeft;
            foreach (var index in path.Indexes)
            {
                FastStream stream = new FastStream(33 + 4);
                if (IsHardendedIndex(index))
                {
                    stream.Write((byte)0);
                    stream.Write(prevPrvBa);
                }
                else
                {
                    stream.Write(prevPubBa);
                }
                stream.WriteBigEndian(index);
                HmacAndSplitData(stream.ToByteArray(), tempCC, out tempLeft, out tempCC);

                Scalar8x32 kTemp = new Scalar8x32(tempLeft, out bool overflow);
                // Note that we throw an exception here if the values were invalid (highly unlikely)
                // because it is the extended keys, we can't skip anything here.
                if (kTemp.IsZero || overflow)
                {
                    throw new ArgumentException();
                }
                PrivateKey temp = new PrivateKey(kTemp.Add(prevPrvInt, out _));
                prevPrvInt = temp.ToScalar();
                prevPrvBa = temp.ToBytes();
                prevPubBa = temp.ToPublicKey(calc).ToByteArray(true).ToArray();
            }

            // Then derive the actual keys
            PrivateKey[] result = new PrivateKey[count];
            int i = 0;
            uint childIndex = startIndex;
            while (i < count)
            {
                try
                {
                    FastStream stream = new FastStream(33 + 4);
                    if (IsHardendedIndex(childIndex))
                    {
                        stream.Write((byte)0);
                        stream.Write(prevPrvBa);
                    }
                    else
                    {
                        stream.Write(prevPubBa);
                    }
                    stream.WriteBigEndian(childIndex);
                    HmacAndSplitData(stream.ToByteArray(), tempCC, out tempLeft, out _);

                    Scalar8x32 kTemp = new Scalar8x32(tempLeft, out bool overflow);
                    // Note: we don't throw any exceptions here. We simply ignore invalid values (highly unlikely)
                    // and move on to the next index. The returned value will always be filled with expected number of items.
                    if (kTemp.IsZero || overflow)
                    {
                        continue;
                    }
                    PrivateKey temp = new PrivateKey(kTemp.Add(prevPrvInt, out _));
                    result[i++] = temp;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Only ignore this type that is thrown by the PrivateKey constructor.
                }
                finally
                {
                    childIndex += step;
                }
            }
            return result;
        }


        /// <summary>
        /// Derives and returns the requested number of child public keys using the given derivation path.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <param name="path">Derivation path</param>
        /// <param name="count">Number of keys to return</param>
        /// <param name="startIndex">[Default value = 0] Starting index of the child key</param>
        /// <param name="step">[Default value = 1] Steps between each key (minimum will always be 1)</param>
        /// <returns>An array of derived child public keys</returns>
        public Point[] GetPublicKeys(BIP0032Path path, uint count, uint startIndex = 0, uint step = 1)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0032));
            if (path is null)
                throw new ArgumentNullException(nameof(path), "Path can not be null!");
            if (ExtendedKeyDepth == byte.MaxValue || ExtendedKeyDepth + path.Indexes.Length + 1 > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(ExtendedKeyDepth), "Can not get children since " +
                    "depth will be bigger than 1 byte");

            Point[] result = new Point[count];
            if (!(PrvKey is null))
            {
                PrivateKey[] tempPK = GetPrivateKeys(path, count, startIndex, step);
                for (int j = 0; j < tempPK.Length; j++)
                {
                    result[j] = tempPK[j].ToPublicKey(calc);
                    tempPK[j].Dispose();
                }
                return result;
            }

            // If we are here it means PrivateKey was null
            bool anyHardened = path.Indexes.Any(x => IsHardendedIndex(x));
            if (anyHardened || IsHardendedIndex(startIndex))
            {
                throw new ArgumentException();
            }

            // Two quick fixes:
            if (count == 0)
                return null;
            if (step < 1)
                step = 1;

            // First start deriving the extended keys for each index
            BigInteger prevPrvInt = PrvKey.ToBigInt();
            byte[] prevPubBa = PubKey.ToByteArray(true).ToArray();
            PointJacobian prevPubPoint = PubKey.ToPointJacobian();
            byte[] tempCC = ChainCode;
            byte[] tempLeft;
            foreach (var index in path.Indexes)
            {
                // There is no hardened indexes thanks to first check
                FastStream stream = new FastStream(33 + 4);
                stream.Write(prevPubBa);
                stream.WriteBigEndian(index);
                HmacAndSplitData(stream.ToByteArray(), tempCC, out tempLeft, out tempCC);

                Scalar8x32 kTemp = new Scalar8x32(tempLeft, out bool overflow);
                // Note that we throw an exception here if the values were invalid (highly unlikely)
                // because it is the extended keys, we can't skip anything here.
                if (kTemp.IsZero || overflow)
                {
                    throw new ArgumentException();
                }
                PointJacobian pt = calc.MultiplyByG(kTemp).AddVar(prevPubPoint, out _);
                Point tempPub = pt.ToPointVar();
                prevPubPoint = tempPub.ToPointJacobian();
                prevPubBa = tempPub.ToByteArray(true).ToArray();
            }

            // Then derive the actual keys

            int i = 0;
            uint childIndex = startIndex;
            while (i < count)
            {
                // There is no hardened indexes thanks to first check
                FastStream stream = new FastStream(33 + 4);
                stream.Write(prevPubBa);
                stream.WriteBigEndian(childIndex);
                HmacAndSplitData(stream.ToByteArray(), tempCC, out tempLeft, out _);

                Scalar8x32 kTemp = new Scalar8x32(tempLeft, out bool overflow);
                // Note: we don't throw any exceptions here. We simply ignore invalid values (highly unlikely)
                // and move on to the next index. The returned value will always be filled with expected number of items.
                if (kTemp.IsZero || overflow)
                {
                    continue;
                }
                PointJacobian pt = calc.MultiplyByG(kTemp).AddVar(prevPubPoint, out _);
                Point temp = pt.ToPointVar();
                result[i++] = temp;

                childIndex += step;
            }
            return result;
        }


        /// <summary>
        /// Returns base58-encoded string representation of this instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="xType">Extended key type to return</param>
        /// <returns>Base58-encoded extended key</returns>
        public string ToBase58(XType xType)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(BIP0032), "Instance was disposed.");
            bool isPub = IsPublic(xType);
            if (!isPub && PrvKey is null)
                throw new ArgumentNullException(nameof(PrvKey), "Can not get extended private key from public key.");


            FastStream stream = new FastStream(ExtendedKeyLength);
            stream.WriteBigEndian((uint)xType);
            stream.Write(ExtendedKeyDepth);
            stream.Write(ParentFingerPrint);
            stream.Write(ChildNumber);
            stream.Write(ChainCode);
            if (isPub)
            {
                stream.Write(PubKey.ToByteArray(true).ToArray());
            }
            else
            {
                stream.Write((byte)0);
                stream.Write(PrvKey.ToBytes());
            }

            return Base58.EncodeWithChecksum(stream.ToByteArray());
        }


        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by the <see cref="BIP0032"/> class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (!(hmac is null))
                        hmac.Dispose();
                    hmac = null;

                    if (!(PrvKey is null))
                        PrvKey.Dispose();
                    PrvKey = null;

                    PubKey = Point.Infinity;

                    if (ChainCode != null)
                        Array.Clear(ChainCode, 0, ChainCode.Length);
                    ChainCode = null;

                    if (ParentFingerPrint != null)
                        Array.Clear(ParentFingerPrint, 0, ParentFingerPrint.Length);
                    ParentFingerPrint = null;

                    if (ChildNumber != null)
                        Array.Clear(ChildNumber, 0, ChildNumber.Length);
                    ChildNumber = null;
                }

                isDisposed = true;
            }
        }


        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="BIP0032"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
